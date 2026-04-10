#include "FakeLiftoffAds.h"

#include <future>
#include <stdexcept>

// ---- Global fake state ----
FakeSdkState g_fakeSdk;

// ---- Singleton fake instance ----
static LiftoffAds g_fakeInstance;

// ---- FakeSdkState::Reset ----
void FakeSdkState::Reset() {
    initShouldSucceed = true;
    initErrorMessage = "Fake init failure";
    initShouldThrow = false;
    initExceptionMessage = "Fake init exception";

    lastAppId.clear();
    lastHwnd = nullptr;
    lastInitCallback = InitializationCallback();
    initializeCalled = false;

    loadAdReturn = true;
    loadMediatedAdReturn = true;

    lastLoadPlacement.clear();
    lastLoadMarkup.clear();
    lastLoadCallback = AdLoadCallback();
    loadAdCalled = false;
    loadMediatedAdCalled = false;

    playAdResult = LiftoffAdPlayInfo();
    playMediatedAdResult = LiftoffAdPlayInfo();

    lastPlayPlacement.clear();
    lastPlayMarkup.clear();
    lastPlayCallback = AdPlayCallback();
    playAdCalled = false;
    playMediatedAdCalled = false;

    superTokenResult.clear();

    lastSuperTokenPlacement.clear();
    getSuperTokenCalled = false;

    setCoppaCalled = false;
    lastCoppaStatus = false;
    setCcpaCalled = false;
    lastCcpaStatus = CcpaConsentStatus::OptedIn;
    setGdprCalled = false;
    lastGdprStatus = GdprConsentStatus::ConsentAccepted;
    lastGdprVersion.clear();
    setAshwidCalled = false;
    lastAshwidDisabled = false;

    lastDiagListener = nullptr;
    addDiagListenerCalled = false;
    removeDiagListenerCalled = false;

    isAdPlayableReturn = false;
}

// ---- InitializeAsync (3-arg) ----
std::future<LiftoffAds*> LiftoffAds::InitializeAsync(
    const std::string appID, HWND hWnd, const InitializationCallback initCallback)
{
    g_fakeSdk.initializeCalled = true;
    g_fakeSdk.lastAppId = appID;
    g_fakeSdk.lastHwnd = hWnd;
    g_fakeSdk.lastInitCallback = initCallback;

    std::promise<LiftoffAds*> p;
    if (g_fakeSdk.initShouldThrow) {
        p.set_exception(std::make_exception_ptr(
            std::runtime_error(g_fakeSdk.initExceptionMessage)));
    } else {
        p.set_value(&g_fakeInstance);
    }

    // Fire callback synchronously for test determinism
    if (!g_fakeSdk.initShouldThrow) {
        if (g_fakeSdk.initShouldSucceed && initCallback.OnInitializationSuccess) {
            initCallback.OnInitializationSuccess(InitializationSuccessEventArgs());
        } else if (!g_fakeSdk.initShouldSucceed && initCallback.OnInitializationFailure) {
            initCallback.OnInitializationFailure(InitializationFailureEventArgs(g_fakeSdk.initErrorMessage));
        }
    }

    return p.get_future();
}

// ---- InitializeAsync (4-arg, delegates to 3-arg) ----
std::future<LiftoffAds*> LiftoffAds::InitializeAsync(
    const std::string appID, const LiftoffSdkConfig config,
    HWND hWnd, const InitializationCallback initCallback)
{
    return InitializeAsync(appID, hWnd, initCallback);
}

// ---- LoadAd ----
bool LiftoffAds::LoadAd(const std::string placement, const AdLoadCallback callback) {
    g_fakeSdk.loadAdCalled = true;
    g_fakeSdk.lastLoadPlacement = placement;
    g_fakeSdk.lastLoadCallback = callback;
    return g_fakeSdk.loadAdReturn;
}

// ---- LoadMediatedAd ----
bool LiftoffAds::LoadMediatedAd(const std::string placement,
    const AdLoadCallback callback, const std::string headerBiddingMarkup)
{
    g_fakeSdk.loadMediatedAdCalled = true;
    g_fakeSdk.lastLoadPlacement = placement;
    g_fakeSdk.lastLoadMarkup = headerBiddingMarkup;
    g_fakeSdk.lastLoadCallback = callback;
    return g_fakeSdk.loadMediatedAdReturn;
}

// ---- PlayAd (3-arg) ----
LiftoffAdPlayInfo LiftoffAds::PlayAd(const std::string placement,
    const AdPlayCallback callbacks, const AdConfig config)
{
    g_fakeSdk.playAdCalled = true;
    g_fakeSdk.lastPlayPlacement = placement;
    g_fakeSdk.lastPlayCallback = callbacks;
    return g_fakeSdk.playAdResult;
}

// ---- PlayAd (2-arg, delegates to 3-arg) ----
LiftoffAdPlayInfo LiftoffAds::PlayAd(const std::string placement,
    const AdPlayCallback callbacks)
{
    return PlayAd(placement, callbacks, AdConfig());
}

// ---- PlayMediatedAd ----
LiftoffAdPlayInfo LiftoffAds::PlayMediatedAd(
    const AdConfig config, const std::string placement,
    const AdPlayCallback callbacks, const std::string headerBiddingMarkup)
{
    g_fakeSdk.playMediatedAdCalled = true;
    g_fakeSdk.lastPlayPlacement = placement;
    g_fakeSdk.lastPlayMarkup = headerBiddingMarkup;
    g_fakeSdk.lastPlayCallback = callbacks;
    return g_fakeSdk.playMediatedAdResult;
}

// ---- IsAdPlayable ----
bool LiftoffAds::IsAdPlayable(const std::string placement) {
    return g_fakeSdk.isAdPlayableReturn;
}

// ---- GetMediationSuperToken ----
std::string LiftoffAds::GetMediationSuperToken(const std::string targetPlacement) {
    g_fakeSdk.getSuperTokenCalled = true;
    g_fakeSdk.lastSuperTokenPlacement = targetPlacement;
    return g_fakeSdk.superTokenResult;
}

// ---- Privacy setters ----
void LiftoffAds::SetCoppaStatus(bool status) {
    g_fakeSdk.setCoppaCalled = true;
    g_fakeSdk.lastCoppaStatus = status;
}

bool LiftoffAds::GetCoppaStatus() {
    return g_fakeSdk.lastCoppaStatus;
}

void LiftoffAds::SetCcpaStatus(CcpaConsentStatus status) {
    g_fakeSdk.setCcpaCalled = true;
    g_fakeSdk.lastCcpaStatus = status;
}

CcpaConsentStatus LiftoffAds::GetCcpaStatus() {
    return g_fakeSdk.lastCcpaStatus;
}

void LiftoffAds::SetGdprConsentStatus(GdprConsentStatus status, const std::string version) {
    g_fakeSdk.setGdprCalled = true;
    g_fakeSdk.lastGdprStatus = status;
    g_fakeSdk.lastGdprVersion = version;
}

GdprConsentStatus LiftoffAds::GetGdprConsentStatus() {
    return g_fakeSdk.lastGdprStatus;
}

std::string LiftoffAds::GetGdprConsentMessageVersion() {
    return g_fakeSdk.lastGdprVersion;
}

void LiftoffAds::ResetGdprConsentStatusToUnknown() {
    // no-op for tests
}

void LiftoffAds::SetDisableAshwidTracking(bool disabled) {
    g_fakeSdk.setAshwidCalled = true;
    g_fakeSdk.lastAshwidDisabled = disabled;
}

bool LiftoffAds::GetDisableASHWID() {
    return g_fakeSdk.lastAshwidDisabled;
}

// ---- Diagnostics ----
void LiftoffAds::AddDiagnosticListener(std::function<void(const DiagnosticLogEvent)> listener) {
    g_fakeSdk.addDiagListenerCalled = true;
    g_fakeSdk.lastDiagListener = listener;
}

void LiftoffAds::RemoveDiagnosticListener(std::function<void(const DiagnosticLogEvent)> listener) {
    g_fakeSdk.removeDiagListenerCalled = true;
    g_fakeSdk.lastDiagListener = nullptr;
}

// ---- PlacementConfigFromServer default constructor ----
PlacementConfigFromServer::PlacementConfigFromServer()
    : Name(), IsAutoCached(false), IsIncentivized(false),
      IsHeaderBidding(false), MaxHeaderBiddingCache(0) {
}

// ---- Test helpers ----
void FakeSdk_FireInitSuccess() {
    if (g_fakeSdk.lastInitCallback.OnInitializationSuccess) {
        g_fakeSdk.lastInitCallback.OnInitializationSuccess(InitializationSuccessEventArgs());
    }
}

void FakeSdk_FireInitFailure(const std::string& error) {
    if (g_fakeSdk.lastInitCallback.OnInitializationFailure) {
        g_fakeSdk.lastInitCallback.OnInitializationFailure(InitializationFailureEventArgs(error));
    }
}
