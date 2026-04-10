#include <gtest/gtest.h>
#include "../LiftoffUnityBridge/LiftoffUnityBridge.h"
#include "FakeSdk/FakeLiftoffAds.h"

#include <string>
#include <thread>
#include <chrono>

// ---- Test callback state ----
static bool s_shut_initSuccessFired = false;
static bool s_shut_loadSuccessFired = false;

static void __stdcall ShutTestInitSuccess() { s_shut_initSuccessFired = true; }
static void __stdcall ShutTestInitFailure(int code, const wchar_t* msg) { /* no-op */ }
static void __stdcall ShutTestLoadSuccess(const wchar_t* placement) {
    s_shut_loadSuccessFired = true;
}

static void ResetShutTestState() {
    s_shut_initSuccessFired = false;
    s_shut_loadSuccessFired = false;
}

class BridgeShutdownTest : public ::testing::Test {
protected:
    void SetUp() override {
        g_fakeSdk.Reset();
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
        ResetShutTestState();
    }
    void TearDown() override {
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
    }
};

TEST_F(BridgeShutdownTest, Shutdown_ClearsCallbacks) {
    BridgeCallbacks cbs = {};
    cbs.initSuccess = ShutTestInitSuccess;
    cbs.loadSuccess = ShutTestLoadSuccess;
    Liftoff_SetCallbacks(cbs);

    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    EXPECT_TRUE(s_shut_initSuccessFired);

    // Shutdown clears callbacks
    Liftoff_Shutdown();

    // Reset state to verify callbacks don't fire again
    ResetShutTestState();
    g_fakeSdk.Reset();

    // Re-initialize without setting callbacks
    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    // Callbacks should not fire because Shutdown cleared them
    EXPECT_FALSE(s_shut_initSuccessFired);
}

TEST_F(BridgeShutdownTest, Shutdown_ClearsInstance) {
    BridgeCallbacks cbs = {};
    cbs.initSuccess = ShutTestInitSuccess;
    Liftoff_SetCallbacks(cbs);

    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    EXPECT_TRUE(Liftoff_IsInitialized());

    Liftoff_Shutdown();

    EXPECT_FALSE(Liftoff_IsInitialized());
}

TEST_F(BridgeShutdownTest, Shutdown_ThenReinit_Works) {
    BridgeCallbacks cbs = {};
    cbs.initSuccess = ShutTestInitSuccess;
    Liftoff_SetCallbacks(cbs);

    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    EXPECT_TRUE(Liftoff_IsInitialized());
    EXPECT_TRUE(s_shut_initSuccessFired);

    // Shutdown
    Liftoff_Shutdown();
    EXPECT_FALSE(Liftoff_IsInitialized());

    // Re-initialize
    ResetShutTestState();
    g_fakeSdk.Reset();
    g_fakeSdk.initShouldSucceed = true;

    // Set callbacks again since Shutdown cleared them
    Liftoff_SetCallbacks(cbs);

    Liftoff_Initialize(L"test-app-id-2", nullptr);
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    EXPECT_TRUE(Liftoff_IsInitialized());
    EXPECT_TRUE(s_shut_initSuccessFired);
    EXPECT_EQ(g_fakeSdk.lastAppId, "test-app-id-2");
}
