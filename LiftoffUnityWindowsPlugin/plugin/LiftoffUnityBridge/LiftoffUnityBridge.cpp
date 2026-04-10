#define LIFTOFFBRIDGE_EXPORTS
#include "LiftoffUnityBridge.h"

#include <Windows.h>
#include <combaseapi.h>
#include <atomic>
#include <chrono>
#include <condition_variable>
#include <functional>
#include <memory>
#include <mutex>
#include <shared_mutex>
#include <string>
#include <thread>

// SDK headers
#include "LiftoffAds.h"
#include "EventArguments/DiagnosticLogEvent.h"
#include "LiftoffAdPlayInfo.h"


// --------- SDK globals / state ----------
static std::atomic<LiftoffAds*> g_sdkInstance{ nullptr };
static std::atomic<bool>        g_initialized{ false };
static std::atomic<bool>        g_initSuccessSignaled{ false };
static std::atomic<bool>        g_shuttingDown{ false };

static BridgeCallbacks          g_cbs{};
static std::mutex               g_cbsMutex;
static std::shared_mutex        g_instanceMutex;  // guards g_sdkInstance usage lifetime

// Keep init callback alive for lifetime of SDK
static std::shared_ptr<InitializationCallback> g_initCb;

// Init thread tracking for safe shutdown
static std::thread              g_initThread;
static std::mutex               g_initThreadMutex;
static std::atomic<uint64_t>    g_initGeneration{ 0 };
static std::atomic<bool>        g_initThreadDone{ true };
static std::condition_variable  g_initThreadCV;

// --------- Diagnostics state ------------
static DiagnosticCB g_diagCb = nullptr;
static std::function<void(const DiagnosticLogEvent)> g_diagForwarder;
static std::mutex g_diagMutex;
static bool g_diagRegistered = false;

// --------- Host window (Editor HWND fallback) --------
static HWND g_hostWnd = nullptr;
static ATOM g_hostClass = 0;
static DWORD g_hostWndThreadId = 0;

static BridgeCallbacks GetCallbacksSnapshot() {
    std::lock_guard<std::mutex> lock(g_cbsMutex);
    return g_cbs;
}

static DiagnosticCB GetDiagSnapshot() {
    std::lock_guard<std::mutex> lock(g_diagMutex);
    return g_diagCb;
}

static HWND EnsureHostWindow() {
    if (g_hostWnd && IsWindow(g_hostWnd)) return g_hostWnd;

    const wchar_t* kClassName = L"LiftoffHostWindow";
    HINSTANCE hInst = GetModuleHandleW(nullptr);

    if (!g_hostClass) {
        WNDCLASSW wc = {};
        wc.lpszClassName = kClassName;
        wc.lpfnWndProc = DefWindowProcW;
        wc.hInstance = hInst;

        ATOM a = RegisterClassW(&wc);
        if (!a) {
            DWORD err = GetLastError();
            if (err == ERROR_CLASS_ALREADY_EXISTS) {
                // Class is already registered (Unity domain reload, etc.) - treat as success.
                a = 1; // non-zero sentinel
            }
        }
        g_hostClass = a;
    }

    g_hostWnd = CreateWindowExW(
        0,
        kClassName,
        L"Liftoff Host",
        WS_POPUP,
        CW_USEDEFAULT, CW_USEDEFAULT,
        1, 1,
        nullptr, nullptr,
        hInst,
        nullptr
    );
    if (g_hostWnd) g_hostWndThreadId = GetCurrentThreadId();

    return g_hostWnd;
}

static void DestroyHostWindow() {
    if (g_hostWnd) {
        if (GetCurrentThreadId() == g_hostWndThreadId) {
            DestroyWindow(g_hostWnd);
        } else {
            // DestroyWindow must be called from the thread that created the window.
            // Post WM_CLOSE so the owning thread's message pump handles it.
            PostMessageW(g_hostWnd, WM_CLOSE, 0, 0);
        }
        g_hostWnd = nullptr;
        g_hostWndThreadId = 0;
    }
    // Leaving the class registered is fine and avoids reload issues.
}

// --------- String helpers --------------
static std::string WToUtf8(const std::wstring& ws) {
    if (ws.empty()) return std::string();
    int size_needed = WideCharToMultiByte(CP_UTF8, 0, ws.c_str(), (int)ws.size(), NULL, 0, NULL, NULL);
    std::string out(size_needed, 0);
    WideCharToMultiByte(CP_UTF8, 0, ws.c_str(), (int)ws.size(), &out[0], size_needed, NULL, NULL);
    return out;
}

static std::wstring Utf8ToW(const std::string& s) {
    if (s.empty()) return std::wstring();
    int size_needed = MultiByteToWideChar(CP_UTF8, 0, s.c_str(), (int)s.size(), NULL, 0);
    std::wstring out(size_needed, 0);
    MultiByteToWideChar(CP_UTF8, 0, s.c_str(), (int)s.size(), &out[0], size_needed);
    return out;
}

static void RegisterDiagnosticsIfNeeded();

// --------- Init success signaling ----------
static void SignalInitSuccessIfReady() {
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (!inst) return;

    if (!g_initSuccessSignaled.exchange(true, std::memory_order_acq_rel)) {
        RegisterDiagnosticsIfNeeded();

        auto cbs = GetCallbacksSnapshot();
        if (cbs.initSuccess) cbs.initSuccess();
    }
}

// Register diag listener with SDK (only once, only after init).
// We copy the forwarder under the lock, then register outside the lock
// to avoid deadlock if AddDiagnosticListener fires a callback synchronously
// (the callback would call GetDiagSnapshot() which also locks g_diagMutex).
static void RegisterDiagnosticsIfNeeded() {
    std::function<void(const DiagnosticLogEvent)> forwarderCopy;
    {
        std::lock_guard<std::mutex> lock(g_diagMutex);
        if (g_diagRegistered || !g_diagForwarder) return;
        forwarderCopy = g_diagForwarder;
        g_diagRegistered = true;
    }
    LiftoffAds::AddDiagnosticListener(forwarderCopy);
}

// --------- Bridge API -------------------
LIFTOFF_API void __stdcall Liftoff_SetCallbacks(BridgeCallbacks cbs) {
    std::lock_guard<std::mutex> lock(g_cbsMutex);
    g_cbs = cbs;
}

LIFTOFF_API bool __stdcall Liftoff_Initialize(const wchar_t* appIdW, void* hwnd) {
    if (g_initSuccessSignaled.load(std::memory_order_acquire) &&
        g_sdkInstance.load(std::memory_order_acquire)) {
        return true;
    }

    try {
        const std::string appId = WToUtf8(std::wstring(appIdW ? appIdW : L""));

        HWND hWnd = static_cast<HWND>(hwnd);
        if (!hWnd || !IsWindow(hWnd)) {
            hWnd = EnsureHostWindow();
        }

        // Keep initialization callback alive
        g_initCb = std::make_shared<InitializationCallback>();

        // Capture generation before setting up callbacks so they can detect
        // whether Shutdown (or a newer Initialize) has invalidated this cycle.
        g_shuttingDown.store(false, std::memory_order_release);
        uint64_t gen = g_initGeneration.load(std::memory_order_acquire);

        g_initCb->OnInitializationSuccess = [gen](InitializationSuccessEventArgs /*args*/) {
            if (g_shuttingDown.load(std::memory_order_acquire)) return;
            if (g_initGeneration.load(std::memory_order_acquire) != gen) return;
            g_initialized.store(true, std::memory_order_release);
            SignalInitSuccessIfReady();
            };

        g_initCb->OnInitializationFailure = [gen](InitializationFailureEventArgs args) {
            if (g_shuttingDown.load(std::memory_order_acquire)) return;
            if (g_initGeneration.load(std::memory_order_acquire) != gen) return;
            std::wstring wmsg = Utf8ToW(args.ErrorMessage);
            auto cbs = GetCallbacksSnapshot();
            if (cbs.initFailure) {
                cbs.initFailure(0, wmsg.empty() ? L"Initialization Failed" : wmsg.c_str());
            }
            };

        // Kick off async init
        auto fut = LiftoffAds::InitializeAsync(appId, hWnd, *g_initCb);

        {
            std::unique_lock<std::mutex> tlock(g_initThreadMutex);
            if (g_initThread.joinable()) {
                bool done = g_initThreadCV.wait_for(tlock, std::chrono::seconds(2), [] {
                    return g_initThreadDone.load(std::memory_order_acquire);
                });
                if (done) g_initThread.join();
                else {
                    OutputDebugStringA("[LiftoffBridge] Previous init thread did not finish in time; detaching\n");
                    g_initThread.detach();
                }
            }
            g_initThreadDone.store(false, std::memory_order_release);
            g_initThread = std::thread([f = std::move(fut), gen]() mutable {
                struct OnExit {
                    ~OnExit() {
                        {
                            std::lock_guard<std::mutex> lk(g_initThreadMutex);
                            g_initThreadDone.store(true, std::memory_order_release);
                        }
                        g_initThreadCV.notify_one();
                    }
                } onExit;
                try {
                    if (auto* inst = f.get()) {
                        // Discard result if Shutdown was called or a newer
                        // Initialize has started since this thread launched.
                        if (g_shuttingDown.load(std::memory_order_acquire)) return;
                        {
                            // Exclusive lock makes the generation check and store
                            // atomic w.r.t. Shutdown and other init threads,
                            // preventing TOCTOU races on g_sdkInstance.
                            std::unique_lock<std::shared_mutex> wlock(g_instanceMutex);
                            if (g_initGeneration.load(std::memory_order_acquire) != gen) return;
                            g_sdkInstance.store(inst, std::memory_order_release);
                        }
                        if (g_initialized.load(std::memory_order_acquire)) {
                            SignalInitSuccessIfReady();
                        }
                    }
                }
                catch (const std::exception& ex) {
                    OutputDebugStringA("[LiftoffBridge] Init thread exception: ");
                    OutputDebugStringA(ex.what());
                    OutputDebugStringA("\n");
                }
                catch (...) {
                    OutputDebugStringA("[LiftoffBridge] Init thread unknown exception\n");
                }
            });
        }

        return true;
    }
    catch (const std::exception& ex) {
        auto cbs = GetCallbacksSnapshot();
        std::wstring wmsg = Utf8ToW(ex.what());
        if (cbs.initFailure) cbs.initFailure(0, wmsg.empty() ? L"Exception during Initialize" : wmsg.c_str());
        return false;
    }
    catch (...) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.initFailure) cbs.initFailure(0, L"Exception during Initialize");
        return false;
    }
}

LIFTOFF_API bool __stdcall Liftoff_IsInitialized() {
    return g_initSuccessSignaled.load(std::memory_order_acquire) &&
        (g_sdkInstance.load(std::memory_order_acquire) != nullptr);
}

LIFTOFF_API bool __stdcall Liftoff_LoadAd(const wchar_t* placementW) {
    std::shared_lock<std::shared_mutex> rlock(g_instanceMutex);
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (!inst) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.loadFailure) cbs.loadFailure(L"", 1, L"SDK instance not ready. Wait for OnInitialized before LoadAd.");
        return false;
    }

    try {
        std::wstring placementWStr = std::wstring(placementW ? placementW : L"");
        std::string placement = WToUtf8(placementWStr);

        AdLoadCallback cb;

        cb.OnAdLoadSuccess = [](AdLoadEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.loadSuccess) cbs.loadSuccess(Utf8ToW(args.Placement).c_str());
            };

        cb.OnAdLoadFailure = [](AdLoadEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            std::wstring wplacement = Utf8ToW(args.Placement);
            std::wstring wmsg = Utf8ToW(args.ErrorMessage);
            if (cbs.loadFailure) {
                cbs.loadFailure(wplacement.c_str(), 1, wmsg.empty() ? L"Load failed" : wmsg.c_str());
            }
            };

        bool kicked = inst->LoadAd(placement, cb);
        if (!kicked) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.loadFailure) cbs.loadFailure(placementWStr.c_str(), 1, L"LoadAd returned false");
        }
        return kicked;
    }
    catch (const std::exception& ex) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.loadFailure) cbs.loadFailure(L"", 1, Utf8ToW(ex.what()).c_str());
        return false;
    }
    catch (...) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.loadFailure) cbs.loadFailure(L"", 1, L"Exception during LoadAd");
        return false;
    }
}

LIFTOFF_API bool __stdcall Liftoff_PlayAd(const wchar_t* placementW) {
    std::shared_lock<std::shared_mutex> rlock(g_instanceMutex);
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (!inst) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.adPlayFailure) cbs.adPlayFailure(L"", 2, L"SDK instance not ready. Wait for OnInitialized before PlayAd.");
        return false;
    }

    try {
        std::wstring placementWStr = std::wstring(placementW ? placementW : L"");
        std::string placement = WToUtf8(placementWStr);

        AdPlayCallback pcb;

        pcb.OnAdStart = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adStart) cbs.adStart(Utf8ToW(args.Placement).c_str(), Utf8ToW(args.EventID).c_str());
            };

        pcb.OnAdEnd = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adEnd) cbs.adEnd(Utf8ToW(args.Placement).c_str());
            };

        pcb.OnAdPlayFailure = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            std::wstring wplacement = Utf8ToW(args.Placement);
            std::wstring wmsg = Utf8ToW(args.ErrorMessage);
            if (cbs.adPlayFailure) {
                cbs.adPlayFailure(wplacement.c_str(), 2, wmsg.empty() ? L"Play failed" : wmsg.c_str());
            }
            };

        pcb.OnAdPlayRewarded = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adRewarded) cbs.adRewarded(Utf8ToW(args.Placement).c_str());
            };

        pcb.OnAdPlayClick = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adClick) cbs.adClick(Utf8ToW(args.Placement).c_str());
            };

        LiftoffAdPlayInfo info = inst->PlayAd(placement, pcb, AdConfig());

        if (!info.Success) {
            auto cbs = GetCallbacksSnapshot();
            std::wstring wmsg = Utf8ToW(info.ErrorMessage);
            std::wstring wplacement = Utf8ToW(info.Placement);
            if (cbs.adPlayFailure) {
                cbs.adPlayFailure(wplacement.c_str(), 2, wmsg.empty() ? L"PlayAd refused" : wmsg.c_str());
            }
            return false;
        }
        return true;
    }
    catch (const std::exception& ex) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.adPlayFailure) cbs.adPlayFailure(L"", 2, Utf8ToW(ex.what()).c_str());
        return false;
    }
    catch (...) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.adPlayFailure) cbs.adPlayFailure(L"", 2, L"Exception during PlayAd");
        return false;
    }
}

LIFTOFF_API void __stdcall Liftoff_Shutdown() {
    // Signal the init thread to abort if still running
    g_shuttingDown.store(true, std::memory_order_release);
    // Bump generation so any in-flight init thread discards its result,
    // even if a new Initialize resets g_shuttingDown before the thread checks.
    g_initGeneration.fetch_add(1, std::memory_order_acq_rel);

    // Wait up to 2 seconds for the init thread to finish.  If it hasn't
    // completed by then (e.g. the SDK's InitializeAsync is hung), detach
    // to avoid blocking the Unity main thread indefinitely.  The
    // g_shuttingDown flag ensures the thread discards its result even if
    // it finishes after we've moved on.
    {
        std::unique_lock<std::mutex> tlock(g_initThreadMutex);
        if (g_initThread.joinable()) {
            bool done = g_initThreadCV.wait_for(tlock, std::chrono::seconds(2), [] {
                return g_initThreadDone.load(std::memory_order_acquire);
            });
            if (done) g_initThread.join();
            else {
                OutputDebugStringA("[LiftoffBridge] Init thread did not finish in time; detaching\n");
                g_initThread.detach();
            }
        }
    }

    // Copy forwarder under lock, then remove outside lock to avoid
    // deadlock if RemoveDiagnosticListener fires a synchronous callback
    // (the forwarder calls GetDiagSnapshot() which also locks g_diagMutex).
    std::function<void(const DiagnosticLogEvent)> diagToRemove;
    {
        std::lock_guard<std::mutex> lock(g_diagMutex);
        if (g_diagRegistered && g_diagForwarder) {
            diagToRemove = g_diagForwarder;
            g_diagRegistered = false;
        }
        g_diagForwarder = nullptr;
        g_diagCb = nullptr;
    }
    if (diagToRemove) {
        LiftoffAds::RemoveDiagnosticListener(diagToRemove);
    }

    {
        std::lock_guard<std::mutex> lock(g_cbsMutex);
        g_cbs = BridgeCallbacks{};
    }

    // Exclusive lock prevents any in-flight LoadAd/PlayAd/GetSuperToken
    // from using a dangling g_sdkInstance pointer after we null it.
    {
        std::unique_lock<std::shared_mutex> wlock(g_instanceMutex);
        g_sdkInstance.store(nullptr, std::memory_order_release);
    }
    g_initialized.store(false, std::memory_order_release);
    g_initSuccessSignaled.store(false, std::memory_order_release);

    // Init thread has been joined or detached.  If detached, g_shuttingDown
    // ensures it won't store into g_sdkInstance.  Protect g_initCb under
    // g_initThreadMutex for consistency with Initialize.
    {
        std::lock_guard<std::mutex> tlock(g_initThreadMutex);
        g_initCb.reset();
    }

    DestroyHostWindow();
}

LIFTOFF_API bool __stdcall Liftoff_LoadAd_WithMarkup(const wchar_t* placementW,
    const wchar_t* headerBiddingMarkupW)
{
    std::shared_lock<std::shared_mutex> rlock(g_instanceMutex);
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (!inst) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.loadFailure) cbs.loadFailure(L"", 1, L"SDK instance not ready. Wait for OnInitialized before LoadAd.");
        return false;
    }

    try {
        std::wstring placementWStr = std::wstring(placementW ? placementW : L"");
        std::wstring markupWStr = std::wstring(headerBiddingMarkupW ? headerBiddingMarkupW : L"");

        const std::string placement = WToUtf8(placementWStr);
        const std::string markup = WToUtf8(markupWStr);

        AdLoadCallback cb;

        cb.OnAdLoadSuccess = [](AdLoadEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.loadSuccess) cbs.loadSuccess(Utf8ToW(args.Placement).c_str());
            };

        cb.OnAdLoadFailure = [](AdLoadEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            std::wstring wplacement = Utf8ToW(args.Placement);
            std::wstring wmsg = Utf8ToW(args.ErrorMessage);
            if (cbs.loadFailure) cbs.loadFailure(wplacement.c_str(), 1, wmsg.empty() ? L"Load failed" : wmsg.c_str());
            };

        bool kicked = inst->LoadMediatedAd(placement, cb, markup);
        if (!kicked) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.loadFailure) cbs.loadFailure(placementWStr.c_str(), 1, L"LoadMediatedAd returned false");
        }
        return kicked;
    }
    catch (const std::exception& ex) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.loadFailure) cbs.loadFailure(L"", 1, Utf8ToW(ex.what()).c_str());
        return false;
    }
    catch (...) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.loadFailure) cbs.loadFailure(L"", 1, L"Exception during LoadMediatedAd");
        return false;
    }
}

LIFTOFF_API bool __stdcall Liftoff_PlayAd_WithMarkup(const wchar_t* placementW,
    const wchar_t* headerBiddingMarkupW)
{
    std::shared_lock<std::shared_mutex> rlock(g_instanceMutex);
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (!inst) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.adPlayFailure) cbs.adPlayFailure(L"", 2, L"SDK instance not ready. Wait for OnInitialized before PlayAd.");
        return false;
    }

    try {
        std::wstring placementWStr = std::wstring(placementW ? placementW : L"");
        std::wstring markupWStr = std::wstring(headerBiddingMarkupW ? headerBiddingMarkupW : L"");

        const std::string placement = WToUtf8(placementWStr);
        const std::string markup = WToUtf8(markupWStr);

        AdPlayCallback pcb;

        pcb.OnAdStart = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adStart) cbs.adStart(Utf8ToW(args.Placement).c_str(), Utf8ToW(args.EventID).c_str());
            };

        pcb.OnAdEnd = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adEnd) cbs.adEnd(Utf8ToW(args.Placement).c_str());
            };

        pcb.OnAdPlayFailure = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            std::wstring wplacement = Utf8ToW(args.Placement);
            std::wstring wmsg = Utf8ToW(args.ErrorMessage);
            if (cbs.adPlayFailure) cbs.adPlayFailure(wplacement.c_str(), 2, wmsg.empty() ? L"Play failed" : wmsg.c_str());
            };

        pcb.OnAdPlayRewarded = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adRewarded) cbs.adRewarded(Utf8ToW(args.Placement).c_str());
            };

        pcb.OnAdPlayClick = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adClick) cbs.adClick(Utf8ToW(args.Placement).c_str());
            };

        LiftoffAdPlayInfo info = inst->PlayMediatedAd(AdConfig(), placement, pcb, markup);

        if (!info.Success) {
            auto cbs = GetCallbacksSnapshot();
            std::wstring wmsg = Utf8ToW(info.ErrorMessage);
            std::wstring wplacement = Utf8ToW(info.Placement);
            if (cbs.adPlayFailure) cbs.adPlayFailure(wplacement.c_str(), 2, wmsg.empty() ? L"PlayMediatedAd refused" : wmsg.c_str());
            return false;
        }
        return true;
    }
    catch (const std::exception& ex) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.adPlayFailure) cbs.adPlayFailure(L"", 2, Utf8ToW(ex.what()).c_str());
        return false;
    }
    catch (...) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.adPlayFailure) cbs.adPlayFailure(L"", 2, L"Exception during PlayMediatedAd");
        return false;
    }
}

// ---- WebView2: availability check (cached) ----
static std::atomic<int> g_webView2Cached{ -1 }; // -1 = unchecked, 0 = no, 1 = yes

LIFTOFF_API bool __stdcall Liftoff_IsWebView2Available() {
    int cached = g_webView2Cached.load(std::memory_order_acquire);
    if (cached >= 0) return cached != 0;

    typedef HRESULT(WINAPI* GetVerFn)(PCWSTR, LPWSTR*);
    HMODULE h = LoadLibraryExW(L"WebView2Loader.dll", nullptr,
        LOAD_LIBRARY_SEARCH_APPLICATION_DIR | LOAD_LIBRARY_SEARCH_SYSTEM32);
    if (!h) { g_webView2Cached.store(0, std::memory_order_release); return false; }

    auto fn = reinterpret_cast<GetVerFn>(GetProcAddress(h, "GetAvailableCoreWebView2BrowserVersionString"));
    if (!fn) { FreeLibrary(h); g_webView2Cached.store(0, std::memory_order_release); return false; }

    LPWSTR ver = nullptr;
    HRESULT hr = fn(nullptr, &ver);

    bool available = SUCCEEDED(hr) && ver;
    if (ver) CoTaskMemFree(ver);
    FreeLibrary(h);

    g_webView2Cached.store(available ? 1 : 0, std::memory_order_release);
    return available;
}

// ---------------- Diagnostics bridge ----------------
LIFTOFF_API void __stdcall Liftoff_SetDiagnosticCallback(DiagnosticCB cb)
{
    std::lock_guard<std::mutex> lock(g_diagMutex);
    g_diagCb = cb;

    if (!g_diagForwarder) {
        g_diagForwarder = [](const DiagnosticLogEvent e) {
            auto cbSnap = GetDiagSnapshot();
            if (!cbSnap) return;

            std::wstring sender = Utf8ToW(e.SenderType);
            std::wstring msg = Utf8ToW(e.ToString());
            cbSnap(static_cast<int>(e.Level), sender.c_str(), msg.c_str());
            };
    }
}

LIFTOFF_API void __stdcall Liftoff_ClearDiagnosticCallback()
{
    // Same pattern as Shutdown: copy under lock, remove outside lock
    // to avoid deadlock if the SDK fires a synchronous callback.
    std::function<void(const DiagnosticLogEvent)> forwarderCopy;
    {
        std::lock_guard<std::mutex> lock(g_diagMutex);
        if (g_diagRegistered && g_diagForwarder) {
            forwarderCopy = g_diagForwarder;
            g_diagRegistered = false;
        }
        g_diagForwarder = nullptr;
        g_diagCb = nullptr;
    }
    if (forwarderCopy) {
        LiftoffAds::RemoveDiagnosticListener(forwarderCopy);
    }
}

// COPPA
LIFTOFF_API void __stdcall Liftoff_SetCoppaStatus(bool status) {
    try { LiftoffAds::SetCoppaStatus(status); }
    catch (const std::exception& ex) { OutputDebugStringA("[LiftoffBridge] SetCoppaStatus: "); OutputDebugStringA(ex.what()); OutputDebugStringA("\n"); }
    catch (...) { OutputDebugStringA("[LiftoffBridge] SetCoppaStatus: unknown exception\n"); }
}

// CCPA  (1 = OptedIn, 2 = OptedOut)
LIFTOFF_API void __stdcall Liftoff_SetCcpaStatus(int status) {
    try {
        if (status < 1 || status > 2) {
            OutputDebugStringA("[LiftoffBridge] SetCcpaStatus: invalid status (expected 1 or 2)\n");
            return;
        }
        LiftoffAds::SetCcpaStatus(static_cast<CcpaConsentStatus>(status));
    }
    catch (const std::exception& ex) { OutputDebugStringA("[LiftoffBridge] SetCcpaStatus: "); OutputDebugStringA(ex.what()); OutputDebugStringA("\n"); }
    catch (...) { OutputDebugStringA("[LiftoffBridge] SetCcpaStatus: unknown exception\n"); }
}

// GDPR  (1 = ConsentAccepted, 2 = ConsentDenied)
LIFTOFF_API void __stdcall Liftoff_SetGdprConsentStatus(int status, const wchar_t* versionW) {
    try {
        if (status < 1 || status > 2) {
            OutputDebugStringA("[LiftoffBridge] SetGdprConsentStatus: invalid status (expected 1 or 2)\n");
            return;
        }
        std::string version = WToUtf8(std::wstring(versionW ? versionW : L""));
        LiftoffAds::SetGdprConsentStatus(static_cast<GdprConsentStatus>(status), version);
    }
    catch (const std::exception& ex) { OutputDebugStringA("[LiftoffBridge] SetGdprConsentStatus: "); OutputDebugStringA(ex.what()); OutputDebugStringA("\n"); }
    catch (...) { OutputDebugStringA("[LiftoffBridge] SetGdprConsentStatus: unknown exception\n"); }
}

LIFTOFF_API void __stdcall Liftoff_SetDisableAshwidTracking(bool disabled)
{
    try { LiftoffAds::SetDisableAshwidTracking(disabled); }
    catch (const std::exception& ex) { OutputDebugStringA("[LiftoffBridge] SetDisableAshwidTracking: "); OutputDebugStringA(ex.what()); OutputDebugStringA("\n"); }
    catch (...) { OutputDebugStringA("[LiftoffBridge] SetDisableAshwidTracking: unknown exception\n"); }
}

// ---- Super Token ----
LIFTOFF_API const wchar_t* __stdcall Liftoff_GetSuperToken(const wchar_t* placementW)
{
    std::shared_lock<std::shared_mutex> rlock(g_instanceMutex);
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (!inst) return nullptr;

    try {
        std::string placement = WToUtf8(std::wstring(placementW ? placementW : L""));
        std::string token = inst->GetMediationSuperToken(placement);
        if (token.empty()) return nullptr;

        std::wstring wtoken = Utf8ToW(token);
        size_t byteLen = (wtoken.size() + 1) * sizeof(wchar_t);
        wchar_t* result = static_cast<wchar_t*>(CoTaskMemAlloc(byteLen));
        if (result) {
            wcscpy_s(result, wtoken.size() + 1, wtoken.c_str());
        }
        return result;
    }
    catch (const std::exception& ex) {
        OutputDebugStringA("[LiftoffBridge] GetSuperToken: ");
        OutputDebugStringA(ex.what());
        OutputDebugStringA("\n");
        return nullptr;
    }
    catch (...) {
        OutputDebugStringA("[LiftoffBridge] GetSuperToken: unknown exception\n");
        return nullptr;
    }
}