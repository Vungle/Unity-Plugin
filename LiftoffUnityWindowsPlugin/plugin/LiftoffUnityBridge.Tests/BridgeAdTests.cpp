#include <gtest/gtest.h>
#include "../LiftoffUnityBridge/LiftoffUnityBridge.h"
#include "FakeSdk/FakeLiftoffAds.h"

#include <string>
#include <thread>
#include <chrono>

// ---- Test callback state ----
static bool s_ad_loadSuccessFired = false;
static bool s_ad_loadFailureFired = false;
static std::wstring s_ad_lastLoadPlacement;
static std::wstring s_ad_lastLoadFailMsg;

static bool s_ad_adStartFired = false;
static std::wstring s_ad_lastStartPlacement;
static std::wstring s_ad_lastStartEventId;

static bool s_ad_adEndFired = false;
static bool s_ad_adPlayFailureFired = false;
static std::wstring s_ad_lastPlayFailPlacement;
static std::wstring s_ad_lastPlayFailMsg;

static void __stdcall AdTestInitSuccess() { /* no-op, just needed for init */ }
static void __stdcall AdTestInitFailure(int code, const wchar_t* msg) { /* no-op */ }

static void __stdcall AdTestLoadSuccess(const wchar_t* placement) {
    s_ad_loadSuccessFired = true;
    s_ad_lastLoadPlacement = placement ? placement : L"";
}
static void __stdcall AdTestLoadFailure(const wchar_t* placement, int code, const wchar_t* msg) {
    s_ad_loadFailureFired = true;
    s_ad_lastLoadFailMsg = msg ? msg : L"";
}
static void __stdcall AdTestAdStart(const wchar_t* placement, const wchar_t* eventId) {
    s_ad_adStartFired = true;
    s_ad_lastStartPlacement = placement ? placement : L"";
    s_ad_lastStartEventId = eventId ? eventId : L"";
}
static void __stdcall AdTestAdEnd(const wchar_t* placement) {
    s_ad_adEndFired = true;
}
static void __stdcall AdTestAdPlayFailure(const wchar_t* placement, int code, const wchar_t* msg) {
    s_ad_adPlayFailureFired = true;
    s_ad_lastPlayFailPlacement = placement ? placement : L"";
    s_ad_lastPlayFailMsg = msg ? msg : L"";
}

static void ResetAdTestState() {
    s_ad_loadSuccessFired = false;
    s_ad_loadFailureFired = false;
    s_ad_lastLoadPlacement.clear();
    s_ad_lastLoadFailMsg.clear();
    s_ad_adStartFired = false;
    s_ad_lastStartPlacement.clear();
    s_ad_lastStartEventId.clear();
    s_ad_adEndFired = false;
    s_ad_adPlayFailureFired = false;
    s_ad_lastPlayFailPlacement.clear();
    s_ad_lastPlayFailMsg.clear();
}

static void InitBridgeForAdTests() {
    BridgeCallbacks cbs = {};
    cbs.initSuccess = AdTestInitSuccess;
    cbs.initFailure = AdTestInitFailure;
    cbs.loadSuccess = AdTestLoadSuccess;
    cbs.loadFailure = AdTestLoadFailure;
    cbs.adStart = AdTestAdStart;
    cbs.adEnd = AdTestAdEnd;
    cbs.adPlayFailure = AdTestAdPlayFailure;
    Liftoff_SetCallbacks(cbs);

    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);
    std::this_thread::sleep_for(std::chrono::milliseconds(100));
}

class BridgeAdTest : public ::testing::Test {
protected:
    void SetUp() override {
        g_fakeSdk.Reset();
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
        ResetAdTestState();
    }
    void TearDown() override {
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
    }
};

TEST_F(BridgeAdTest, LoadAd_NotInitialized_InvokesLoadFailure) {
    BridgeCallbacks cbs = {};
    cbs.loadFailure = AdTestLoadFailure;
    Liftoff_SetCallbacks(cbs);

    // Do NOT initialize -- call LoadAd directly
    bool result = Liftoff_LoadAd(L"placement1");

    EXPECT_FALSE(result);
    EXPECT_TRUE(s_ad_loadFailureFired);
}

TEST_F(BridgeAdTest, LoadAd_Success_InvokesLoadSuccess) {
    InitBridgeForAdTests();

    g_fakeSdk.loadAdReturn = true;
    bool result = Liftoff_LoadAd(L"placement1");
    EXPECT_TRUE(result);
    EXPECT_TRUE(g_fakeSdk.loadAdCalled);
    EXPECT_EQ(g_fakeSdk.lastLoadPlacement, "placement1");

    // Simulate SDK firing load success callback
    if (g_fakeSdk.lastLoadCallback.OnAdLoadSuccess) {
        AdLoadEventArgs args(true, "placement1", 1);
        g_fakeSdk.lastLoadCallback.OnAdLoadSuccess(args);
    }

    EXPECT_TRUE(s_ad_loadSuccessFired);
    EXPECT_EQ(s_ad_lastLoadPlacement, L"placement1");
}

TEST_F(BridgeAdTest, LoadAd_NullPlacement_DoesNotCrash) {
    InitBridgeForAdTests();

    g_fakeSdk.loadAdReturn = true;

    // Pass nullptr for placement -- should not crash
    bool result = Liftoff_LoadAd(nullptr);

    EXPECT_TRUE(result);
    EXPECT_TRUE(g_fakeSdk.loadAdCalled);
    EXPECT_EQ(g_fakeSdk.lastLoadPlacement, "");
}

TEST_F(BridgeAdTest, LoadAd_WithMarkup_CallsLoadMediatedAd) {
    InitBridgeForAdTests();

    g_fakeSdk.loadMediatedAdReturn = true;
    bool result = Liftoff_LoadAd_WithMarkup(L"placement1", L"<markup>bid-data</markup>");

    EXPECT_TRUE(result);
    EXPECT_TRUE(g_fakeSdk.loadMediatedAdCalled);
    EXPECT_FALSE(g_fakeSdk.loadAdCalled);
    EXPECT_EQ(g_fakeSdk.lastLoadPlacement, "placement1");
    EXPECT_EQ(g_fakeSdk.lastLoadMarkup, "<markup>bid-data</markup>");
}

TEST_F(BridgeAdTest, PlayAd_NotInitialized_InvokesPlayFailure) {
    BridgeCallbacks cbs = {};
    cbs.adPlayFailure = AdTestAdPlayFailure;
    Liftoff_SetCallbacks(cbs);

    // Do NOT initialize -- call PlayAd directly
    bool result = Liftoff_PlayAd(L"placement1");

    EXPECT_FALSE(result);
    EXPECT_TRUE(s_ad_adPlayFailureFired);
}

TEST_F(BridgeAdTest, PlayAd_Success_InvokesAdStart) {
    InitBridgeForAdTests();

    // Configure PlayAd to succeed
    g_fakeSdk.playAdResult.Success = true;
    g_fakeSdk.playAdResult.Placement = "placement1";

    bool result = Liftoff_PlayAd(L"placement1");
    EXPECT_TRUE(result);
    EXPECT_TRUE(g_fakeSdk.playAdCalled);
    EXPECT_EQ(g_fakeSdk.lastPlayPlacement, "placement1");

    // Simulate SDK firing AdStart callback
    if (g_fakeSdk.lastPlayCallback.OnAdStart) {
        AdPlayEventArgs args("placement1", "", "event-123", nullptr);
        g_fakeSdk.lastPlayCallback.OnAdStart(args);
    }

    EXPECT_TRUE(s_ad_adStartFired);
    EXPECT_EQ(s_ad_lastStartPlacement, L"placement1");
    EXPECT_EQ(s_ad_lastStartEventId, L"event-123");
}

TEST_F(BridgeAdTest, PlayAd_Failure_InvokesAdPlayFailure) {
    InitBridgeForAdTests();

    // Configure PlayAd to fail synchronously
    g_fakeSdk.playAdResult.Success = false;
    g_fakeSdk.playAdResult.Placement = "placement1";
    g_fakeSdk.playAdResult.ErrorMessage = "No ad available";

    bool result = Liftoff_PlayAd(L"placement1");
    EXPECT_FALSE(result);
    EXPECT_TRUE(s_ad_adPlayFailureFired);
    EXPECT_NE(s_ad_lastPlayFailMsg.find(L"No ad available"), std::wstring::npos);
}

TEST_F(BridgeAdTest, PlayAd_WithMarkup_CallsPlayMediatedAd) {
    InitBridgeForAdTests();

    g_fakeSdk.playMediatedAdResult.Success = true;
    g_fakeSdk.playMediatedAdResult.Placement = "placement1";

    bool result = Liftoff_PlayAd_WithMarkup(L"placement1", L"<markup>bid-data</markup>");

    EXPECT_TRUE(result);
    EXPECT_TRUE(g_fakeSdk.playMediatedAdCalled);
    EXPECT_FALSE(g_fakeSdk.playAdCalled);
    EXPECT_EQ(g_fakeSdk.lastPlayPlacement, "placement1");
    EXPECT_EQ(g_fakeSdk.lastPlayMarkup, "<markup>bid-data</markup>");
}
