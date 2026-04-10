#include <gtest/gtest.h>
#include "../LiftoffUnityBridge/LiftoffUnityBridge.h"
#include "FakeSdk/FakeLiftoffAds.h"

#include <string>
#include <thread>
#include <chrono>

// ---- Test callback state ----
static bool s_init_successFired = false;
static bool s_init_failureFired = false;
static int s_init_failureCode = 0;
static std::wstring s_init_failureMessage;

static void __stdcall InitTestSuccess() { s_init_successFired = true; }
static void __stdcall InitTestFailure(int code, const wchar_t* msg) {
    s_init_failureFired = true;
    s_init_failureCode = code;
    s_init_failureMessage = msg ? msg : L"";
}

static void ResetInitTestState() {
    s_init_successFired = false;
    s_init_failureFired = false;
    s_init_failureCode = 0;
    s_init_failureMessage.clear();
}

class BridgeInitTest : public ::testing::Test {
protected:
    void SetUp() override {
        g_fakeSdk.Reset();
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
        ResetInitTestState();
    }
    void TearDown() override {
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
    }
};

TEST_F(BridgeInitTest, Initialize_Success_InvokesInitSuccessCallback) {
    BridgeCallbacks cbs = {};
    cbs.initSuccess = InitTestSuccess;
    cbs.initFailure = InitTestFailure;
    Liftoff_SetCallbacks(cbs);

    g_fakeSdk.initShouldSucceed = true;

    bool result = Liftoff_Initialize(L"test-app-id", nullptr);
    EXPECT_TRUE(result);

    // Allow the init thread to complete
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    EXPECT_TRUE(s_init_successFired);
    EXPECT_FALSE(s_init_failureFired);
    EXPECT_TRUE(g_fakeSdk.initializeCalled);
    EXPECT_EQ(g_fakeSdk.lastAppId, "test-app-id");
}

TEST_F(BridgeInitTest, Initialize_Failure_InvokesInitFailureCallback) {
    BridgeCallbacks cbs = {};
    cbs.initSuccess = InitTestSuccess;
    cbs.initFailure = InitTestFailure;
    Liftoff_SetCallbacks(cbs);

    g_fakeSdk.initShouldSucceed = false;
    g_fakeSdk.initErrorMessage = "Something went wrong";

    bool result = Liftoff_Initialize(L"test-app-id", nullptr);
    EXPECT_TRUE(result); // Initialize returns true (it kicked off), failure comes via callback

    // Allow the init thread to complete
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    EXPECT_FALSE(s_init_successFired);
    EXPECT_TRUE(s_init_failureFired);
    EXPECT_NE(s_init_failureMessage.find(L"Something went wrong"), std::wstring::npos);
}

TEST_F(BridgeInitTest, Initialize_AlreadyInitialized_ReturnsTrueImmediately) {
    BridgeCallbacks cbs = {};
    cbs.initSuccess = InitTestSuccess;
    Liftoff_SetCallbacks(cbs);

    g_fakeSdk.initShouldSucceed = true;

    Liftoff_Initialize(L"test-app-id", nullptr);
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    EXPECT_TRUE(s_init_successFired);
    EXPECT_TRUE(Liftoff_IsInitialized());

    // Reset to detect if SDK is called again
    g_fakeSdk.initializeCalled = false;

    bool result = Liftoff_Initialize(L"test-app-id", nullptr);
    EXPECT_TRUE(result);

    // Should not have called SDK again since already initialized
    EXPECT_FALSE(g_fakeSdk.initializeCalled);
}

TEST_F(BridgeInitTest, IsInitialized_BeforeInit_ReturnsFalse) {
    EXPECT_FALSE(Liftoff_IsInitialized());
}

TEST_F(BridgeInitTest, IsInitialized_AfterInit_ReturnsTrue) {
    BridgeCallbacks cbs = {};
    cbs.initSuccess = InitTestSuccess;
    Liftoff_SetCallbacks(cbs);

    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);

    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    EXPECT_TRUE(Liftoff_IsInitialized());
}

TEST_F(BridgeInitTest, Initialize_NullAppId_DoesNotCrash) {
    BridgeCallbacks cbs = {};
    cbs.initSuccess = InitTestSuccess;
    cbs.initFailure = InitTestFailure;
    Liftoff_SetCallbacks(cbs);

    g_fakeSdk.initShouldSucceed = true;

    // Pass nullptr for appId -- should not crash
    bool result = Liftoff_Initialize(nullptr, nullptr);

    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    // Should still function (bridge converts nullptr to empty string)
    EXPECT_TRUE(result);
    EXPECT_TRUE(g_fakeSdk.initializeCalled);
    EXPECT_EQ(g_fakeSdk.lastAppId, "");
}
