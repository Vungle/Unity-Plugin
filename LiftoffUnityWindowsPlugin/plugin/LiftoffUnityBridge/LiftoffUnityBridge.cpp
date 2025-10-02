#define LIFTOFFBRIDGE_EXPORTS
#include "LiftoffUnityBridge.h"

#include <Windows.h>      // CP_UTF8, HWND, OutputDebugStringW, etc.
#include <combaseapi.h>   // CoTaskMemFree
#include <atomic>
#include <functional>
#include <memory>
#include <mutex>
#include <string>
#include <thread>

// SDK headers
#include "LiftoffAds.h"
#include "EventArguments/DiagnosticLogEvent.h"
#include "LiftoffAdPlayInfo.h"  // PlayAd return type (Success, ErrorMessage, Placement, etc.)

using namespace std;

// --------- SDK globals / state ----------
static std::atomic<LiftoffAds*> g_sdkInstance{ nullptr };      // set when init completes
static std::atomic<bool>        g_initialized{ false };        // SDK init reported success
static std::atomic<bool>        g_initSuccessSignaled{ false };// we notified C#
static BridgeCallbacks          g_cbs{};                       // managed callbacks
static std::mutex               g_cbsMutex;                    // protects g_cbs

// Keep init callback alive for lifetime of SDK
static std::shared_ptr<InitializationCallback> g_initCb;

// --------- Diagnostics state ------------
static DiagnosticCB g_diagCb = nullptr;
static std::function<void(const DiagnosticLogEvent)> g_diagForwarder;
static std::mutex g_diagMutex;
static bool g_diagRegistered = false;  // <-- track whether we added the SDK listener

// --------- Host window (Editor HWND fallback) --------
static HWND g_hostWnd = nullptr;
static ATOM g_hostClass = 0;

static HWND EnsureHostWindow() {
    if (g_hostWnd && IsWindow(g_hostWnd)) return g_hostWnd;
    if (!g_hostClass) {
        WNDCLASSW wc = {};
        wc.lpszClassName = L"LiftoffHostWindow";
        wc.lpfnWndProc = DefWindowProcW;
        g_hostClass = RegisterClassW(&wc);
    }
    g_hostWnd = CreateWindowExW(0, L"LiftoffHostWindow", L"Liftoff Host",
        WS_POPUP, CW_USEDEFAULT, CW_USEDEFAULT, 1, 1,
        NULL, NULL, GetModuleHandleW(NULL), NULL);
    return g_hostWnd;
}

static void DestroyHostWindow() {
    if (g_hostWnd) {
        DestroyWindow(g_hostWnd);
        g_hostWnd = nullptr;
    }
    // (Optionally keep the class registered; safe for multi-load in Editor)
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

// --------- Small native logger ----------
static void NativeLogW(int level/*0..5*/, const wchar_t* sender, const wchar_t* msg) {
    OutputDebugStringW(L"[Liftoff] ");
    if (sender && *sender) { OutputDebugStringW(sender); OutputDebugStringW(L": "); }
    OutputDebugStringW(msg ? msg : L"(null)");
    OutputDebugStringW(L"\n");
    if (g_diagCb) g_diagCb(level, sender ? sender : L"Liftoff", msg ? msg : L"");
}

static void RegisterDiagnosticsIfNeeded();

// --------- Init success signaling ----------
static void SignalInitSuccessIfReady() {
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (inst && !g_initSuccessSignaled.exchange(true, std::memory_order_acq_rel)) {
        RegisterDiagnosticsIfNeeded();  // <-- register diag after instance exists
        if (g_cbs.initSuccess) g_cbs.initSuccess();
        NativeLogW(2, L"Init", L"Initialization complete (instance ready).");
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
            NativeLogW(1, L"Init", L"No valid HWND passed; using hidden host window.");
        }

        LiftoffSdkConfig config;

        // Persist initialization callback object
        g_initCb = std::make_shared<InitializationCallback>();
        g_initCb->OnInitializationSuccess = [](const InitializationSuccessEventArgs& /*args*/) {
            g_initialized.store(true, std::memory_order_release);
            NativeLogW(2, L"Init", L"SDK reports initialization success.");
            SignalInitSuccessIfReady();
            };
        g_initCb->OnInitializationFailure = [](const InitializationFailureEventArgs& /*args*/) {
            if (g_cbs.initFailure) {
                int code = 0;
                std::wstring msg = L"Initialization Failed";
                g_cbs.initFailure(code, msg.c_str());
            }
            NativeLogW(4, L"Init", L"Initialization failed.");
            };

        // Kick off async init (non-blocking)
        auto fut = LiftoffAds::InitializeAsync(appId, config, hWnd, *g_initCb);

        // Resolve the future on a worker thread; some SDK builds only provide the instance here
        std::thread([f = std::move(fut)]() mutable {
            try {
                if (auto* inst = f.get()) {
                    g_sdkInstance.store(inst, std::memory_order_release);
                    NativeLogW(2, L"Init", L"Instance pointer captured from future.");
                    if (g_initialized.load(std::memory_order_acquire)) {
                        SignalInitSuccessIfReady();
                    }
                }
            }
            catch (...) {
                NativeLogW(4, L"Init", L"Initialize future threw; failure should already be signaled.");
            }
            }).detach();

        // Log WebView2 version (optional)
        typedef HRESULT(WINAPI* GetVerFn)(PCWSTR, LPWSTR*);
        if (HMODULE h = LoadLibraryW(L"WebView2Loader.dll")) {
            auto fn = reinterpret_cast<GetVerFn>(GetProcAddress(h, "GetAvailableCoreWebView2BrowserVersionString"));
            if (fn) {
                LPWSTR ver = nullptr;
                if (SUCCEEDED(fn(nullptr, &ver)) && ver) {
                    std::wstring msg = L"WebView2 version: ";
                    msg += ver;
                    NativeLogW(2, L"WebView2", msg.c_str());
                    CoTaskMemFree(ver);
                }
            }
            FreeLibrary(h);
        }

        return true;
    }
    catch (...) {
        if (g_cbs.initFailure) g_cbs.initFailure(-1, L"Exception during Initialize");
        NativeLogW(4, L"Init", L"Exception during Initialize.");
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
        if (g_cbs.loadFailure)
            g_cbs.loadFailure(L"", -2, L"SDK instance not ready. Wait for OnInitialized before LoadAd.");
        NativeLogW(3, L"LoadAd", L"Called before instance was ready.");
        return false;
    }

    try {
        std::wstring placementWStr = std::wstring(placementW ? placementW : L"");
        std::string placement = WToUtf8(placementWStr);

        auto cb = std::make_shared<AdLoadCallback>();

        cb->OnAdLoadSuccess = [cb](AdLoadEventArgs args) {
            if (g_cbs.loadSuccess) g_cbs.loadSuccess(Utf8ToW(args.Placement).c_str());
            NativeLogW(2, L"LoadAd", L"Loaded successfully.");
            };
        cb->OnAdLoadFailure = [cb](AdLoadEventArgs args) {
            int code = 0;
            const wchar_t* msg = L"Load failed";
            if (g_cbs.loadFailure) g_cbs.loadFailure(Utf8ToW(args.Placement).c_str(), code, msg);
            NativeLogW(3, L"LoadAd", L"Load failed (async).");
            };

        bool kicked = inst->LoadAd(placement, *cb);
        if (!kicked && g_cbs.loadFailure) {
            g_cbs.loadFailure(placementWStr.c_str(), -3, L"LoadAd returned false");
            NativeLogW(3, L"LoadAd", L"Synchronous refusal: LoadAd returned false.");
        }
        else {
            NativeLogW(2, L"LoadAd", L"LoadAd request issued.");
        }
        return kicked;
    }
    catch (const std::exception& ex) {
        if (g_cbs.loadFailure) g_cbs.loadFailure(L"", -1, Utf8ToW(ex.what()).c_str());
        NativeLogW(4, L"LoadAd", L"Exception during LoadAd.");
        return false;
    }
    catch (...) {
        if (g_cbs.loadFailure) g_cbs.loadFailure(L"", -1, L"Exception during LoadAd");
        NativeLogW(4, L"LoadAd", L"Unknown exception during LoadAd.");
        return false;
    }
}

LIFTOFF_API bool __stdcall Liftoff_PlayAd(const wchar_t* placementW) {
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (!inst) {
        if (g_cbs.adPlayFailure) g_cbs.adPlayFailure(L"", -2, L"SDK instance not ready. Wait for OnInitialized before PlayAd.");
        NativeLogW(3, L"PlayAd", L"Called before instance was ready.");
        return false;
    }

    try {
        std::wstring placementWStr = std::wstring(placementW ? placementW : L"");
        std::string placement = WToUtf8(placementWStr);

        auto pcb = std::make_shared<AdPlayCallback>();

        pcb->OnAdStart = [pcb](AdPlayEventArgs args) {
            if (g_cbs.adStart) g_cbs.adStart(Utf8ToW(args.Placement).c_str(), Utf8ToW(args.EventID).c_str());
            NativeLogW(2, L"PlayAd", L"Ad started.");
            };
        pcb->OnAdEnd = [pcb](AdPlayEventArgs args) {
            if (g_cbs.adEnd) g_cbs.adEnd(Utf8ToW(args.Placement).c_str());
            NativeLogW(2, L"PlayAd", L"Ad ended.");
            };
        pcb->OnAdPlayFailure = [pcb](AdPlayEventArgs args) {
            if (g_cbs.adPlayFailure) g_cbs.adPlayFailure(Utf8ToW(args.Placement).c_str(), 0, L"Play failed");
            NativeLogW(3, L"PlayAd", L"Ad play failed (async).");
            };
        pcb->OnAdPlayRewarded = [pcb](AdPlayEventArgs args) {
            if (g_cbs.adRewarded) g_cbs.adRewarded(Utf8ToW(args.Placement).c_str());
            NativeLogW(2, L"PlayAd", L"Rewarded.");
            };
        pcb->OnAdPlayClick = [pcb](AdPlayEventArgs args) {
            if (g_cbs.adClick) g_cbs.adClick(Utf8ToW(args.Placement).c_str());
            NativeLogW(2, L"PlayAd", L"Clicked.");
            };

        // PlayAd returns LiftoffAdPlayInfo (NOT a bool)
        LiftoffAdPlayInfo info = inst->PlayAd(placement, *pcb, AdConfig());

        if (!info.Success) {
            std::wstring wmsg = Utf8ToW(info.ErrorMessage);
            std::wstring wplacement = Utf8ToW(info.Placement);
            if (g_cbs.adPlayFailure)
                g_cbs.adPlayFailure(wplacement.c_str(), -3, wmsg.empty() ? L"PlayAd refused" : wmsg.c_str());
            NativeLogW(3, L"PlayAd", wmsg.empty() ? L"Synchronous refusal: PlayAd failed." : wmsg.c_str());
            return false;
        }

        NativeLogW(2, L"PlayAd", L"PlayAd request accepted.");
        return true; // callbacks will follow
    }
    catch (const std::exception& ex) {
        if (g_cbs.adPlayFailure) g_cbs.adPlayFailure(L"", -1, Utf8ToW(ex.what()).c_str());
        NativeLogW(4, L"PlayAd", L"Exception during PlayAd.");
        return false;
    }
    catch (...) {
        if (g_cbs.adPlayFailure) g_cbs.adPlayFailure(L"", -1, L"Exception during PlayAd");
        NativeLogW(4, L"PlayAd", L"Unknown exception during PlayAd.");
        return false;
    }
}

LIFTOFF_API void __stdcall Liftoff_Shutdown() {
    // Remove diagnostic listener if registered + clear function pointer
    {
        std::lock_guard<std::mutex> lock(g_diagMutex);
        if (g_diagRegistered && g_diagForwarder) {
            LiftoffAds::RemoveDiagnosticListener(g_diagForwarder);
            g_diagRegistered = false;
        }
        g_diagForwarder = nullptr;
        g_diagCb = nullptr;
    }

    // Clear callback table so no stale pointers remain
    {
        std::lock_guard<std::mutex> lock(g_cbsMutex);
        g_cbs = BridgeCallbacks{};
    }

    g_sdkInstance.store(nullptr, std::memory_order_release);
    g_initialized.store(false, std::memory_order_release);
    g_initSuccessSignaled.store(false, std::memory_order_release);
    g_initCb.reset();

    DestroyHostWindow();

    NativeLogW(2, L"Shutdown", L"SDK state and callbacks cleared.");
}

// ---- WebView2: availability + version logging (no Unity API surface) ----
LIFTOFF_API bool __stdcall Liftoff_IsWebView2Available() {
    typedef HRESULT(WINAPI* GetVerFn)(PCWSTR, LPWSTR*);
    HMODULE h = LoadLibraryW(L"WebView2Loader.dll");
    if (!h) {
        NativeLogW(3, L"WebView2", L"WebView2Loader.dll not found.");
        return false;
    }

    auto fn = reinterpret_cast<GetVerFn>(GetProcAddress(h, "GetAvailableCoreWebView2BrowserVersionString"));
    if (!fn) {
        NativeLogW(3, L"WebView2", L"GetAvailableCoreWebView2BrowserVersionString not found.");
        FreeLibrary(h);
        return false;
    }

    LPWSTR ver = nullptr;
    HRESULT hr = fn(nullptr, &ver);

    if (SUCCEEDED(hr) && ver) {
        std::wstring msg = L"WebView2 version: ";
        msg += ver;
        NativeLogW(2, L"WebView2", msg.c_str());  // VS Output/DebugView, and Unity via diag callback (if set)
        CoTaskMemFree(ver);
        FreeLibrary(h);
        return true;
    }
    else {
        NativeLogW(3, L"WebView2", L"WebView2 runtime not installed.");
        FreeLibrary(h);
        return false;
    }
}

// ---------------- Diagnostics bridge ----------------
LIFTOFF_API void __stdcall Liftoff_SetDiagnosticCallback(DiagnosticCB cb)
{
    std::lock_guard<std::mutex> lock(g_diagMutex);
    g_diagCb = cb;

    // Prepare a forwarder (no SDK registration yet; we defer until init is ready)
    if (!g_diagForwarder)
    {
        g_diagForwarder = [](const DiagnosticLogEvent e)
            {
                if (g_diagCb)
                {
                    std::wstring sender = Utf8ToW(e.SenderType);
                    std::wstring msg = Utf8ToW(e.ToString()); // rich line (timestamp + level) if available
                    g_diagCb(static_cast<int>(e.Level), sender.c_str(), msg.c_str());
                }
            };
    }
}

LIFTOFF_API void __stdcall Liftoff_ClearDiagnosticCallback()
{
    std::lock_guard<std::mutex> lock(g_diagMutex);
    if (g_diagRegistered && g_diagForwarder)
    {
        LiftoffAds::RemoveDiagnosticListener(g_diagForwarder);
        g_diagRegistered = false;
    }
    g_diagForwarder = nullptr;
    g_diagCb = nullptr;
}
