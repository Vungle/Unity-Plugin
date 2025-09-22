#pragma once
#include <string>
#include <vector>
#include <mutex>
#include <atomic>
#include <functional>
#include <windows.h>

// Liftoff SDK includes (adjust include path to your SDK)
#include "LiftoffAds.h"
#include "EventArguments/AdLoadCallback.h"
#include "EventArguments/InitializationCallback.h"
#include "EventArguments/AdLoadEventArgs.h"
#include "EventArguments/AdPlayCallback.h"
#include "EventArguments/DiagnosticLogEvent.h"

#ifdef LIFTOFFBRIDGE_EXPORTS
  #define LIFTOFF_API extern "C" __declspec(dllexport)
#else
  #define LIFTOFF_API extern "C" __declspec(dllimport)
#endif

// ---- C# callback signatures (stdcall) ----
typedef void(__stdcall* InitSuccessCB)();
typedef void(__stdcall* InitFailureCB)(int code, const wchar_t* message);

typedef void(__stdcall* AdLoadSuccessCB)(const wchar_t* placement);
typedef void(__stdcall* AdLoadFailureCB)(const wchar_t* placement, int code, const wchar_t* message);

typedef void(__stdcall* AdStartCB)(const wchar_t* placement, const wchar_t* eventId);
typedef void(__stdcall* AdEndCB)(const wchar_t* placement);
typedef void(__stdcall* AdPlayFailureCB)(const wchar_t* placement, int code, const wchar_t* message);
typedef void(__stdcall* AdRewardedCB)(const wchar_t* placement);
typedef void(__stdcall* AdClickCB)(const wchar_t* placement);

// Diagnostics
typedef void(__stdcall* DiagnosticCB)(int level, const wchar_t* senderType, const wchar_t* message);

struct BridgeCallbacks {
    InitSuccessCB      initSuccess     = nullptr;
    InitFailureCB      initFailure     = nullptr;
    AdLoadSuccessCB    loadSuccess     = nullptr;
    AdLoadFailureCB    loadFailure     = nullptr;
    AdStartCB          adStart         = nullptr;
    AdEndCB            adEnd           = nullptr;
    AdPlayFailureCB    adPlayFailure   = nullptr;
    AdRewardedCB       adRewarded      = nullptr;
    AdClickCB          adClick         = nullptr;
};

// ---- API exported to C# ----
LIFTOFF_API void  __stdcall Liftoff_SetCallbacks(BridgeCallbacks cbs);
LIFTOFF_API bool  __stdcall Liftoff_Initialize(const wchar_t* appId, void* hwnd);
LIFTOFF_API bool  __stdcall Liftoff_IsInitialized();
LIFTOFF_API bool  __stdcall Liftoff_LoadAd(const wchar_t* placement);
LIFTOFF_API bool  __stdcall Liftoff_PlayAd(const wchar_t* placement);
LIFTOFF_API void  __stdcall Liftoff_Shutdown();
LIFTOFF_API bool  __stdcall Liftoff_IsWebView2Available(); // optional helper

// Diagnostics control
LIFTOFF_API void __stdcall Liftoff_SetDiagnosticCallback(DiagnosticCB cb);
LIFTOFF_API void __stdcall Liftoff_ClearDiagnosticCallback();
