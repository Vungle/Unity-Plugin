#include <gtest/gtest.h>
#include "../LiftoffUnityBridge/LiftoffUnityBridge.h"
#include "FakeSdk/FakeLiftoffAds.h"

#include <string>
#include <thread>
#include <chrono>

// ---- Test callback state ----
static bool s_cb_initSuccessFired = false;
static bool s_cb_initFailureFired = false;
static bool s_cb_loadSuccessFired = false;
static bool s_cb_loadFailureFired = false;
static std::wstring s_cb_lastLoadPlacement;
static std::wstring s_cb_lastLoadFailMsg;
static int s_cb_lastLoadFailCode = 0;

static void __stdcall CbTestInitSuccess() { s_cb_initSuccessFired = true; }
static void __stdcall CbTestInitFailure(int code, const wchar_t* msg) {
    s_cb_initFailureFired = true;
}
static void __stdcall CbTestLoadSuccess(const wchar_t* placement) {
    s_cb_loadSuccessFired = true;
    s_cb_lastLoadPlacement = placement ? placement : L"";
}
static void __stdcall CbTestLoadFailure(const wchar_t* placement, int code, const wchar_t* msg) {
    s_cb_loadFailureFired = true;
    s_cb_lastLoadFailCode = code;
    s_cb_lastLoadFailMsg = msg ? msg : L"";
}

// Second set of callbacks to test overwriting
static bool s_cb2_loadSuccessFired = false;
static void __stdcall CbTest2LoadSuccess(const wchar_t* placement) {
    s_cb2_loadSuccessFired = true;
}

static void ResetCbTestState() {
    s_cb_initSuccessFired = false;
    s_cb_initFailureFired = false;
    s_cb_loadSuccessFired = false;
    s_cb_loadFailureFired = false;
    s_cb_lastLoadPlacement.clear();
    s_cb_lastLoadFailMsg.clear();
    s_cb_lastLoadFailCode = 0;
    s_cb2_loadSuccessFired = false;
}

class BridgeCallbackTest : public ::testing::Test {
protected:
    void SetUp() override {
        g_fakeSdk.Reset();
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
        ResetCbTestState();
    }
    void TearDown() override {
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
    }
};

TEST_F(BridgeCallbackTest, SetCallbacks_StoresCallbacks) {
    // Register callbacks
    BridgeCallbacks cbs = {};
    cbs.initSuccess = CbTestInitSuccess;
    cbs.initFailure = CbTestInitFailure;
    cbs.loadSuccess = CbTestLoadSuccess;
    cbs.loadFailure = CbTestLoadFailure;
    Liftoff_SetCallbacks(cbs);

    // Initialize to get the SDK instance in place
    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);

    // Give the init thread a moment to complete
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    EXPECT_TRUE(s_cb_initSuccessFired);

    // Now trigger a load, then fire the SDK load success callback
    g_fakeSdk.loadAdReturn = true;
    Liftoff_LoadAd(L"placement1");

    // Fire the captured load success callback from the SDK fake
    if (g_fakeSdk.lastLoadCallback.OnAdLoadSuccess) {
        AdLoadEventArgs args(true, "placement1", 1);
        g_fakeSdk.lastLoadCallback.OnAdLoadSuccess(args);
    }

    EXPECT_TRUE(s_cb_loadSuccessFired);
    EXPECT_EQ(s_cb_lastLoadPlacement, L"placement1");
}

TEST_F(BridgeCallbackTest, SetCallbacks_NullCallbacks_NoCrash) {
    // Set all-null callbacks
    BridgeCallbacks cbs = {};
    Liftoff_SetCallbacks(cbs);

    // Initialize -- should not crash even with null callbacks
    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);

    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    // Call load without crash
    Liftoff_LoadAd(L"placement1");

    // Fire SDK callbacks -- bridge should silently skip null function pointers
    if (g_fakeSdk.lastLoadCallback.OnAdLoadSuccess) {
        AdLoadEventArgs args(true, "placement1", 1);
        g_fakeSdk.lastLoadCallback.OnAdLoadSuccess(args);
    }

    // No assertion needed -- test passes if no crash occurs
    SUCCEED();
}

TEST_F(BridgeCallbackTest, SetCallbacks_Overwrites) {
    // Set first set of callbacks
    BridgeCallbacks cbs1 = {};
    cbs1.loadSuccess = CbTestLoadSuccess;
    Liftoff_SetCallbacks(cbs1);

    // Overwrite with second set
    BridgeCallbacks cbs2 = {};
    cbs2.loadSuccess = CbTest2LoadSuccess;
    cbs2.initSuccess = CbTestInitSuccess;
    Liftoff_SetCallbacks(cbs2);

    // Initialize
    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);

    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    // Load and trigger callback
    Liftoff_LoadAd(L"placement1");

    if (g_fakeSdk.lastLoadCallback.OnAdLoadSuccess) {
        AdLoadEventArgs args(true, "placement1", 1);
        g_fakeSdk.lastLoadCallback.OnAdLoadSuccess(args);
    }

    // Only the second set should have fired
    EXPECT_FALSE(s_cb_loadSuccessFired);
    EXPECT_TRUE(s_cb2_loadSuccessFired);
}
