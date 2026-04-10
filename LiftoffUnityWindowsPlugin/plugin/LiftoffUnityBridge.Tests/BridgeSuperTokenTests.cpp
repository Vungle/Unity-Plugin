#include <gtest/gtest.h>
#include "../LiftoffUnityBridge/LiftoffUnityBridge.h"
#include "FakeSdk/FakeLiftoffAds.h"

#include <string>
#include <thread>
#include <chrono>

// ---- Helpers ----
static void __stdcall StInitSuccess() { /* no-op */ }
static void __stdcall StInitFailure(int code, const wchar_t* msg) { /* no-op */ }

class BridgeSuperTokenTest : public ::testing::Test {
protected:
    void SetUp() override {
        g_fakeSdk.Reset();
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
    }
    void TearDown() override {
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
    }

    void InitBridge() {
        BridgeCallbacks cbs = {};
        cbs.initSuccess = StInitSuccess;
        cbs.initFailure = StInitFailure;
        Liftoff_SetCallbacks(cbs);

        g_fakeSdk.initShouldSucceed = true;
        Liftoff_Initialize(L"test-app-id", nullptr);
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
    }
};

TEST_F(BridgeSuperTokenTest, GetSuperToken_NotInitialized_ReturnsNull) {
    // Do NOT initialize
    const wchar_t* result = Liftoff_GetSuperToken(L"placement1");

    EXPECT_EQ(result, nullptr);
    // SDK should not have been called since instance is null
    EXPECT_FALSE(g_fakeSdk.getSuperTokenCalled);
}

TEST_F(BridgeSuperTokenTest, GetSuperToken_Success_ReturnsString) {
    InitBridge();

    g_fakeSdk.superTokenResult = "super-token-abc-123";

    const wchar_t* result = Liftoff_GetSuperToken(L"placement1");

    ASSERT_NE(result, nullptr);
    EXPECT_EQ(std::wstring(result), L"super-token-abc-123");
    EXPECT_TRUE(g_fakeSdk.getSuperTokenCalled);
    EXPECT_EQ(g_fakeSdk.lastSuperTokenPlacement, "placement1");

    // Free the CoTaskMemAlloc'd string
    CoTaskMemFree(const_cast<wchar_t*>(result));
}

TEST_F(BridgeSuperTokenTest, GetSuperToken_EmptyResult_ReturnsNull) {
    InitBridge();

    g_fakeSdk.superTokenResult = "";

    const wchar_t* result = Liftoff_GetSuperToken(L"placement1");

    EXPECT_EQ(result, nullptr);
    EXPECT_TRUE(g_fakeSdk.getSuperTokenCalled);
}

TEST_F(BridgeSuperTokenTest, GetSuperToken_NullPlacement_DoesNotCrash) {
    InitBridge();

    g_fakeSdk.superTokenResult = "some-token";

    // Pass nullptr for placement -- should not crash
    const wchar_t* result = Liftoff_GetSuperToken(nullptr);

    ASSERT_NE(result, nullptr);
    EXPECT_TRUE(g_fakeSdk.getSuperTokenCalled);
    EXPECT_EQ(g_fakeSdk.lastSuperTokenPlacement, "");

    // Free the CoTaskMemAlloc'd string
    CoTaskMemFree(const_cast<wchar_t*>(result));
}
