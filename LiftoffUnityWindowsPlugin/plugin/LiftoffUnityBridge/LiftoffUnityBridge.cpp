#define LIFTOFFBRIDGE_EXPORTS
#include "LiftoffUnityBridge.h"

#include <Windows.h>
#include <combaseapi.h>
#include <atomic>
#include <functional>
#include <memory>
#include <mutex>
#include <string>
#include <thread>

// SDK headers
#include "LiftoffAds.h"
#include "EventArguments/DiagnosticLogEvent.h"
#include "LiftoffAdPlayInfo.h"

using namespace std;

// --------- SDK globals / state ----------
static std::atomic<LiftoffAds*> g_sdkInstance{ nullptr };
static std::atomic<bool>        g_initialized{ false };
static std::atomic<bool>        g_initSuccessSignaled{ false };
static std::atomic<bool>        g_shuttingDown{ false };

static BridgeCallbacks          g_cbs{};
static std::mutex               g_cbsMutex;

// Keep init callback alive for lifetime of SDK
static std::shared_ptr<InitializationCallback> g_initCb;

// Init thread tracking for safe shutdown
static std::thread              g_initThread;
static std::mutex               g_initThreadMutex;

// --------- Diagnostics state ------------
static DiagnosticCB g_diagCb = nullptr;
static std::function<void(const DiagnosticLogEvent)> g_diagForwarder;
static std::mutex g_diagMutex;
static bool g_diagRegistered = false;

// --------- Host window (Editor HWND fallback) --------
static HWND g_hostWnd = nullptr;
static ATOM g_hostClass = 0;

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

    return g_hostWnd;
}

static void DestroyHostWindow() {
    if (g_hostWnd) {
        DestroyWindow(g_hostWnd);
        g_hostWnd = nullptr;
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

// Register diag listener with SDK (only once, only after init)
static void RegisterDiagnosticsIfNeeded() {
    std::lock_guard<std::mutex> lock(g_diagMutex);
    if (!g_diagRegistered && g_diagForwarder) {
        LiftoffAds::AddDiagnosticListener(g_diagForwarder);
        g_diagRegistered = true;
    }
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

        g_initCb->OnInitializationSuccess = [](InitializationSuccessEventArgs /*args*/) {
            g_initialized.store(true, std::memory_order_release);
            SignalInitSuccessIfReady();
            };

        g_initCb->OnInitializationFailure = [](InitializationFailureEventArgs args) {
            std::wstring wmsg = Utf8ToW(args.ErrorMessage);
            auto cbs = GetCallbacksSnapshot();
            if (cbs.initFailure) {
                cbs.initFailure(0, wmsg.empty() ? L"Initialization Failed" : wmsg.c_str());
            }
            };

        // Kick off async init
        g_shuttingDown.store(false, std::memory_order_release);
        auto fut = LiftoffAds::InitializeAsync(appId, hWnd, *g_initCb);

        {
            std::lock_guard<std::mutex> tlock(g_initThreadMutex);
            if (g_initThread.joinable()) g_initThread.join();
            g_initThread = std::thread([f = std::move(fut)]() mutable {
                try {
                    if (auto* inst = f.get()) {
                        if (g_shuttingDown.load(std::memory_order_acquire)) return;
                        g_sdkInstance.store(inst, std::memory_order_release);
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
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (!inst) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.loadFailure) cbs.loadFailure(L"", 1, L"SDK instance not ready. Wait for OnInitialized before LoadAd.");
        return false;
    }

    try {
        std::wstring placementWStr = std::wstring(placementW ? placementW : L"");
        std::string placement = WToUtf8(placementWStr);

        auto cb = std::make_shared<AdLoadCallback>();

        cb->OnAdLoadSuccess = [](AdLoadEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.loadSuccess) cbs.loadSuccess(Utf8ToW(args.Placement).c_str());
            };

        cb->OnAdLoadFailure = [](AdLoadEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            std::wstring wplacement = Utf8ToW(args.Placement);
            std::wstring wmsg = Utf8ToW(args.ErrorMessage);
            if (cbs.loadFailure) {
                cbs.loadFailure(wplacement.c_str(), 1, wmsg.empty() ? L"Load failed" : wmsg.c_str());
            }
            };

        bool kicked = inst->LoadAd(placement, *cb);
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
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (!inst) {
        auto cbs = GetCallbacksSnapshot();
        if (cbs.adPlayFailure) cbs.adPlayFailure(L"", 2, L"SDK instance not ready. Wait for OnInitialized before PlayAd.");
        return false;
    }

    try {
        std::wstring placementWStr = std::wstring(placementW ? placementW : L"");
        std::string placement = WToUtf8(placementWStr);

        auto pcb = std::make_shared<AdPlayCallback>();

        pcb->OnAdStart = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adStart) cbs.adStart(Utf8ToW(args.Placement).c_str(), Utf8ToW(args.EventID).c_str());
            };

        pcb->OnAdEnd = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adEnd) cbs.adEnd(Utf8ToW(args.Placement).c_str());
            };

        pcb->OnAdPlayFailure = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            std::wstring wplacement = Utf8ToW(args.Placement);
            std::wstring wmsg = Utf8ToW(args.ErrorMessage);
            if (cbs.adPlayFailure) {
                cbs.adPlayFailure(wplacement.c_str(), 2, wmsg.empty() ? L"Play failed" : wmsg.c_str());
            }
            };

        pcb->OnAdPlayRewarded = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adRewarded) cbs.adRewarded(Utf8ToW(args.Placement).c_str());
            };

        pcb->OnAdPlayClick = [](const AdPlayEventArgs args) {
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adClick) cbs.adClick(Utf8ToW(args.Placement).c_str());
            };

        LiftoffAdPlayInfo info = inst->PlayAd(placement, *pcb, AdConfig());

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
        if (cbs.adPlayFailure) cbs.adPlayFailure(L"", -1, L"Exception during PlayAd");
        return false;
    }
}

LIFTOFF_API void __stdcall Liftoff_Shutdown() {
    // Signal the init thread to abort if still running
    g_shuttingDown.store(true, std::memory_order_release);

    // Wait for the init thread to finish before tearing down state
    {
        std::lock_guard<std::mutex> tlock(g_initThreadMutex);
        if (g_initThread.joinable()) g_initThread.join();
    }

    {
        std::lock_guard<std::mutex> lock(g_diagMutex);
        if (g_diagRegistered && g_diagForwarder) {
            LiftoffAds::RemoveDiagnosticListener(g_diagForwarder);
            g_diagRegistered = false;
        }
        g_diagForwarder = nullptr;
        g_diagCb = nullptr;
    }

    {
        std::lock_guard<std::mutex> lock(g_cbsMutex);
        g_cbs = BridgeCallbacks{};
    }

    g_sdkInstance.store(nullptr, std::memory_order_release);
    g_initialized.store(false, std::memory_order_release);
    g_initSuccessSignaled.store(false, std::memory_order_release);
    g_initCb.reset();

    DestroyHostWindow();
}

LIFTOFF_API bool __stdcall Liftoff_LoadAd_WithMarkup(const wchar_t* placementW,
    const wchar_t* headerBiddingMarkupW)
{
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

        auto cb = std::make_shared<AdLoadCallback>();
        std::weak_ptr<AdLoadCallback> weak = cb;

        cb->OnAdLoadSuccess = [weak](AdLoadEventArgs args) {
            if (!weak.lock()) return;
            auto cbs = GetCallbacksSnapshot();
            if (cbs.loadSuccess) cbs.loadSuccess(Utf8ToW(args.Placement).c_str());
            };

        cb->OnAdLoadFailure = [weak](AdLoadEventArgs args) {
            if (!weak.lock()) return;
            auto cbs = GetCallbacksSnapshot();
            std::wstring wplacement = Utf8ToW(args.Placement);
            std::wstring wmsg = Utf8ToW(args.ErrorMessage);
            if (cbs.loadFailure) cbs.loadFailure(wplacement.c_str(), 1, wmsg.empty() ? L"Load failed" : wmsg.c_str());
            };

        bool kicked = inst->LoadMediatedAd(placement, *cb, markup);
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

        auto pcb = std::make_shared<AdPlayCallback>();
        std::weak_ptr<AdPlayCallback> weak = pcb;

        pcb->OnAdStart = [weak](const AdPlayEventArgs args) {
            if (!weak.lock()) return;
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adStart) cbs.adStart(Utf8ToW(args.Placement).c_str(), Utf8ToW(args.EventID).c_str());
            };

        pcb->OnAdEnd = [weak](const AdPlayEventArgs args) {
            if (!weak.lock()) return;
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adEnd) cbs.adEnd(Utf8ToW(args.Placement).c_str());
            };

        pcb->OnAdPlayFailure = [weak](const AdPlayEventArgs args) {
            if (!weak.lock()) return;
            auto cbs = GetCallbacksSnapshot();
            std::wstring wplacement = Utf8ToW(args.Placement);
            std::wstring wmsg = Utf8ToW(args.ErrorMessage);
            if (cbs.adPlayFailure) cbs.adPlayFailure(wplacement.c_str(), 2, wmsg.empty() ? L"Play failed" : wmsg.c_str());
            };

        pcb->OnAdPlayRewarded = [weak](const AdPlayEventArgs args) {
            if (!weak.lock()) return;
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adRewarded) cbs.adRewarded(Utf8ToW(args.Placement).c_str());
            };

        pcb->OnAdPlayClick = [weak](const AdPlayEventArgs args) {
            if (!weak.lock()) return;
            auto cbs = GetCallbacksSnapshot();
            if (cbs.adClick) cbs.adClick(Utf8ToW(args.Placement).c_str());
            };

        LiftoffAdPlayInfo info = inst->PlayMediatedAd(AdConfig(), placement, *pcb, markup);

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

// ---- WebView2: availability check ----
LIFTOFF_API bool __stdcall Liftoff_IsWebView2Available() {
    typedef HRESULT(WINAPI* GetVerFn)(PCWSTR, LPWSTR*);
    HMODULE h = LoadLibraryW(L"WebView2Loader.dll");
    if (!h) return false;

    auto fn = reinterpret_cast<GetVerFn>(GetProcAddress(h, "GetAvailableCoreWebView2BrowserVersionString"));
    if (!fn) { FreeLibrary(h); return false; }

    LPWSTR ver = nullptr;
    HRESULT hr = fn(nullptr, &ver);

    if (SUCCEEDED(hr) && ver) {
        CoTaskMemFree(ver);
        FreeLibrary(h);
        return true;
    }

    FreeLibrary(h);
    return false;
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
    std::lock_guard<std::mutex> lock(g_diagMutex);
    if (g_diagRegistered && g_diagForwarder) {
        LiftoffAds::RemoveDiagnosticListener(g_diagForwarder);
        g_diagRegistered = false;
    }
    g_diagForwarder = nullptr;
    g_diagCb = nullptr;
}

// COPPA
LIFTOFF_API void __stdcall Liftoff_SetCoppaStatus(bool status) {
    try { LiftoffAds::SetCoppaStatus(status); }
    catch (const std::exception& ex) { OutputDebugStringA("[LiftoffBridge] SetCoppaStatus: "); OutputDebugStringA(ex.what()); OutputDebugStringA("\n"); }
    catch (...) { OutputDebugStringA("[LiftoffBridge] SetCoppaStatus: unknown exception\n"); }
}

// CCPA
LIFTOFF_API void __stdcall Liftoff_SetCcpaStatus(int status) {
    try { LiftoffAds::SetCcpaStatus(static_cast<CcpaConsentStatus>(status)); }
    catch (const std::exception& ex) { OutputDebugStringA("[LiftoffBridge] SetCcpaStatus: "); OutputDebugStringA(ex.what()); OutputDebugStringA("\n"); }
    catch (...) { OutputDebugStringA("[LiftoffBridge] SetCcpaStatus: unknown exception\n"); }
}

// GDPR
LIFTOFF_API void __stdcall Liftoff_SetGdprConsentStatus(int status, const wchar_t* versionW) {
    try {
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

// ---- Ad State ----
LIFTOFF_API bool __stdcall Liftoff_IsAdPlayable(const wchar_t* placementW)
{
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (!inst) return false;

    try {
        std::string placement = WToUtf8(std::wstring(placementW ? placementW : L""));
        return inst->IsAdPlayable(placement);
    }
    catch (const std::exception& ex) {
        OutputDebugStringA("[LiftoffBridge] IsAdPlayable: ");
        OutputDebugStringA(ex.what());
        OutputDebugStringA("\n");
        return false;
    }
    catch (...) {
        OutputDebugStringA("[LiftoffBridge] IsAdPlayable: unknown exception\n");
        return false;
    }
}
