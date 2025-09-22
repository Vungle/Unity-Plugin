#define LIFTOFFBRIDGE_EXPORTS
#include "LiftoffUnityBridge.h"

#include <Windows.h>   // CP_UTF8, HWND, etc.
#include <atomic>
#include <functional>
#include <memory>
#include <mutex>
#include <string>
#include <thread>

// SDK headers
#include "EventArguments/DiagnosticLogEvent.h"
#include "LiftoffAdPlayInfo.h"  // <-- PlayAd return type

using namespace std;

// --------- SDK globals / state ----------
static std::atomic<LiftoffAds*> g_sdkInstance{ nullptr };      // set when init completes
static std::atomic<bool>        g_initialized{ false };        // true after SDK init success
static std::atomic<bool>        g_initSuccessSignaled{ false };// true once we notify C#
static BridgeCallbacks          g_cbs{};                       // managed callbacks
static std::mutex               g_cbsMutex;                    // protects g_cbs

// --------- Diagnostics state ------------
static DiagnosticCB g_diagCb = nullptr;
static std::function<void(const DiagnosticLogEvent)> g_diagForwarder;
static std::mutex g_diagMutex;

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

// --------- Helpers ----------------------
static void SignalInitSuccessIfReady() {
    // Only signal once, and only after we truly have an instance
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (inst && !g_initSuccessSignaled.exchange(true, std::memory_order_acq_rel)) {
        if (g_cbs.initSuccess) g_cbs.initSuccess();
    }
}

// --------- Bridge API -------------------
LIFTOFF_API void __stdcall Liftoff_SetCallbacks(BridgeCallbacks cbs) {
    std::lock_guard<std::mutex> lock(g_cbsMutex);
    g_cbs = cbs;
}

LIFTOFF_API bool __stdcall Liftoff_Initialize(const wchar_t* appIdW, void* hwnd) {
    // If we already signaled success and have an instance, we’re good
    if (g_initSuccessSignaled.load(std::memory_order_acquire) &&
        g_sdkInstance.load(std::memory_order_acquire))
        return true;

    try {
        const std::string appId = WToUtf8(std::wstring(appIdW ? appIdW : L""));
        HWND hWnd = static_cast<HWND>(hwnd);

        LiftoffSdkConfig config;
        InitializationCallback initCb;

        // Success/failure callbacks from the SDK
        initCb.OnInitializationSuccess = [](const InitializationSuccessEventArgs& /*args*/) {
            // Mark SDK init success; we’ll signal C# after the instance pointer is set
            g_initialized.store(true, std::memory_order_release);
            SignalInitSuccessIfReady();
        };

        initCb.OnInitializationFailure = [](const InitializationFailureEventArgs& /*args*/) {
            if (g_cbs.initFailure) {
                int code = 0; // map from args if available
                std::wstring msg = L"Initialization Failed";
                g_cbs.initFailure(code, msg.c_str());
            }
        };

        // Kick off async init (non-blocking)
        auto fut = LiftoffAds::InitializeAsync(appId, config, hWnd, initCb);

        // Resolve the future on a worker thread; some SDK builds only provide the instance here
        std::thread([f = std::move(fut)]() mutable {
            try {
                if (auto* inst = f.get()) {
                    g_sdkInstance.store(inst, std::memory_order_release);
                    if (g_initialized.load(std::memory_order_acquire)) {
                        SignalInitSuccessIfReady();
                    }
                }
            }
            catch (...) {
                // Init failure should already be reported via the failure callback
            }
        }).detach();

        return true;
    }
    catch (...) {
        if (g_cbs.initFailure) g_cbs.initFailure(-1, L"Exception during Initialize");
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
        return false;
    }

    try {
        std::wstring placementWStr = std::wstring(placementW ? placementW : L"");
        std::string placement = WToUtf8(placementWStr);

        // Keep the callback object alive in case the SDK stores a reference
        auto cb = std::make_shared<AdLoadCallback>();

        cb->OnAdLoadSuccess = [cb](AdLoadEventArgs args) {
            if (g_cbs.loadSuccess) g_cbs.loadSuccess(Utf8ToW(args.Placement).c_str());
        };
        cb->OnAdLoadFailure = [cb](AdLoadEventArgs args) {
            int code = 0; // map from args if available
            const wchar_t* msg = L"Load failed";
            if (g_cbs.loadFailure) g_cbs.loadFailure(Utf8ToW(args.Placement).c_str(), code, msg);
        };

        bool kicked = inst->LoadAd(placement, *cb); // pass by ref; adjust if your SDK copies by value
        if (!kicked && g_cbs.loadFailure) {
            g_cbs.loadFailure(placementWStr.c_str(), -3, L"LoadAd returned false");
        }
        return kicked;
    }
    catch (const std::exception& ex) {
        if (g_cbs.loadFailure) g_cbs.loadFailure(L"", -1, Utf8ToW(ex.what()).c_str());
        return false;
    }
    catch (...) {
        if (g_cbs.loadFailure) g_cbs.loadFailure(L"", -1, L"Exception during LoadAd");
        return false;
    }
}

LIFTOFF_API bool __stdcall Liftoff_PlayAd(const wchar_t* placementW) {
    LiftoffAds* inst = g_sdkInstance.load(std::memory_order_acquire);
    if (!inst) {
        if (g_cbs.adPlayFailure) g_cbs.adPlayFailure(L"", -2, L"SDK instance not ready. Wait for OnInitialized before PlayAd.");
        return false;
    }

    try {
        std::wstring placementWStr = std::wstring(placementW ? placementW : L"");
        std::string placement = WToUtf8(placementWStr);

        AdPlayCallback pcb;
        pcb.OnAdStart = [](AdPlayEventArgs args) {
            if (g_cbs.adStart) g_cbs.adStart(Utf8ToW(args.Placement).c_str(), Utf8ToW(args.EventID).c_str());
        };
        pcb.OnAdEnd = [](AdPlayEventArgs args) {
            if (g_cbs.adEnd) g_cbs.adEnd(Utf8ToW(args.Placement).c_str());
        };
        pcb.OnAdPlayFailure = [](AdPlayEventArgs args) {
            if (g_cbs.adPlayFailure) g_cbs.adPlayFailure(Utf8ToW(args.Placement).c_str(), 0, L"Play failed");
        };
        pcb.OnAdPlayRewarded = [](AdPlayEventArgs args) {
            if (g_cbs.adRewarded) g_cbs.adRewarded(Utf8ToW(args.Placement).c_str());
        };
        pcb.OnAdPlayClick = [](AdPlayEventArgs args) {
            if (g_cbs.adClick) g_cbs.adClick(Utf8ToW(args.Placement).c_str());
        };

        // NEW: PlayAd returns LiftoffAdPlayInfo, not bool
        LiftoffAdPlayInfo info = inst->PlayAd(placement, pcb, AdConfig());

        // Synchronous refusal? surface as failure
        if (!info.Success) {
            if (g_cbs.adPlayFailure) {
                std::wstring wmsg = Utf8ToW(info.ErrorMessage);
                std::wstring wplacement = Utf8ToW(info.Placement);
                g_cbs.adPlayFailure(wplacement.c_str(), -3, wmsg.empty() ? L"PlayAd returned false" : wmsg.c_str());
            }
            return false;
        }

        return true; // playback kicked off; callbacks (start/end/failure) will follow
    }
    catch (const std::exception& ex) {
        if (g_cbs.adPlayFailure) g_cbs.adPlayFailure(L"", -1, Utf8ToW(ex.what()).c_str());
        return false;
    }
    catch (...) {
        if (g_cbs.adPlayFailure) g_cbs.adPlayFailure(L"", -1, L"Exception during PlayAd");
        return false;
    }
}

LIFTOFF_API void __stdcall Liftoff_Shutdown() {
    g_sdkInstance.store(nullptr, std::memory_order_release);
    g_initialized.store(false, std::memory_order_release);
    g_initSuccessSignaled.store(false, std::memory_order_release);
}

LIFTOFF_API bool __stdcall Liftoff_IsWebView2Available() {
    typedef HRESULT(WINAPI* GetVerFn)(PCWSTR, LPWSTR*);
    HMODULE h = LoadLibraryW(L"WebView2Loader.dll");
    if (!h) return false;
    auto fn = reinterpret_cast<GetVerFn>(GetProcAddress(h, "GetAvailableCoreWebView2BrowserVersionString"));
    if (!fn) { FreeLibrary(h); return false; }
    LPWSTR ver = nullptr;
    bool ok = SUCCEEDED(fn(nullptr, &ver)) && ver != nullptr;
    FreeLibrary(h);
    return ok;
}

// ---------------- Diagnostics bridge ----------------
LIFTOFF_API void __stdcall Liftoff_SetDiagnosticCallback(DiagnosticCB cb)
{
    std::lock_guard<std::mutex> lock(g_diagMutex);
    g_diagCb = cb;

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
        LiftoffAds::AddDiagnosticListener(g_diagForwarder);
    }
}

LIFTOFF_API void __stdcall Liftoff_ClearDiagnosticCallback()
{
    std::lock_guard<std::mutex> lock(g_diagMutex);
    if (g_diagForwarder)
    {
        LiftoffAds::RemoveDiagnosticListener(g_diagForwarder);
        g_diagForwarder = nullptr;
    }
    g_diagCb = nullptr;
}
