#pragma once
#include <string>
#include <functional>
#include <vector>
#include <mutex>

// Forward declarations matching SDK types
#include "LiftoffAds.h"
#include "EventArguments/AdLoadCallback.h"
#include "EventArguments/AdPlayCallback.h"
#include "EventArguments/InitializationCallback.h"
#include "EventArguments/DiagnosticLogEvent.h"
#include "Configuration/LiftoffAdPlayInfo.h"
#include "Configuration/AdConfig.h"

struct FakeSdkState {
    // Init control
    bool initShouldSucceed = true;
    std::string initErrorMessage = "Fake init failure";
    bool initShouldThrow = false;
    std::string initExceptionMessage = "Fake init exception";

    // Init capture
    std::string lastAppId;
    HWND lastHwnd = nullptr;
    InitializationCallback lastInitCallback;
    bool initializeCalled = false;

    // LoadAd control
    bool loadAdReturn = true;
    bool loadMediatedAdReturn = true;

    // LoadAd capture
    std::string lastLoadPlacement;
    std::string lastLoadMarkup;
    AdLoadCallback lastLoadCallback;
    bool loadAdCalled = false;
    bool loadMediatedAdCalled = false;

    // PlayAd control
    LiftoffAdPlayInfo playAdResult;
    LiftoffAdPlayInfo playMediatedAdResult;

    // PlayAd capture
    std::string lastPlayPlacement;
    std::string lastPlayMarkup;
    AdPlayCallback lastPlayCallback;
    bool playAdCalled = false;
    bool playMediatedAdCalled = false;

    // SuperToken control
    std::string superTokenResult;

    // SuperToken capture
    std::string lastSuperTokenPlacement;
    bool getSuperTokenCalled = false;

    // Privacy capture
    bool setCoppaCalled = false;
    bool lastCoppaStatus = false;
    bool setCcpaCalled = false;
    CcpaConsentStatus lastCcpaStatus = CcpaConsentStatus::OptedIn;
    bool setGdprCalled = false;
    GdprConsentStatus lastGdprStatus = GdprConsentStatus::ConsentAccepted;
    std::string lastGdprVersion;
    bool setAshwidCalled = false;
    bool lastAshwidDisabled = false;

    // Diagnostics capture
    std::function<void(const DiagnosticLogEvent)> lastDiagListener;
    bool addDiagListenerCalled = false;
    bool removeDiagListenerCalled = false;

    // IsAdPlayable
    bool isAdPlayableReturn = false;

    void Reset();
};

extern FakeSdkState g_fakeSdk;

// Helper to fire init callbacks synchronously (useful in tests)
void FakeSdk_FireInitSuccess();
void FakeSdk_FireInitFailure(const std::string& error);
