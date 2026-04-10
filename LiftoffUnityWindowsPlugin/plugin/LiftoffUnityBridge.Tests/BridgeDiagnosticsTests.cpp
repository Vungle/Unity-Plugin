#include <gtest/gtest.h>
#include "../LiftoffUnityBridge/LiftoffUnityBridge.h"
#include "FakeSdk/FakeLiftoffAds.h"

#include <string>
#include <thread>
#include <chrono>

// ---- Test callback state ----
static bool s_diag_callbackFired = false;
static int s_diag_lastLevel = -1;
static std::wstring s_diag_lastSender;
static std::wstring s_diag_lastMessage;

static void __stdcall DiagTestCallback(int level, const wchar_t* senderType, const wchar_t* message) {
    s_diag_callbackFired = true;
    s_diag_lastLevel = level;
    s_diag_lastSender = senderType ? senderType : L"";
    s_diag_lastMessage = message ? message : L"";
}

static void __stdcall DiagTestInitSuccess() { /* no-op */ }
static void __stdcall DiagTestInitFailure(int code, const wchar_t* msg) { /* no-op */ }

static void ResetDiagTestState() {
    s_diag_callbackFired = false;
    s_diag_lastLevel = -1;
    s_diag_lastSender.clear();
    s_diag_lastMessage.clear();
}

class BridgeDiagnosticsTest : public ::testing::Test {
protected:
    void SetUp() override {
        g_fakeSdk.Reset();
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
        ResetDiagTestState();
    }
    void TearDown() override {
        Liftoff_Shutdown();
        g_fakeSdk.Reset();
    }
};

TEST_F(BridgeDiagnosticsTest, SetDiagnosticCallback_StoresCallback) {
    // Set up init so diagnostics get registered with the SDK
    BridgeCallbacks cbs = {};
    cbs.initSuccess = DiagTestInitSuccess;
    cbs.initFailure = DiagTestInitFailure;
    Liftoff_SetCallbacks(cbs);

    // Set diagnostic callback before init
    Liftoff_SetDiagnosticCallback(DiagTestCallback);

    // Initialize to trigger diagnostic registration
    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    EXPECT_TRUE(g_fakeSdk.addDiagListenerCalled);

    // Now fire a diagnostic event through the captured listener
    if (g_fakeSdk.lastDiagListener) {
        DiagnosticLogEvent evt;
        evt.Level = DiagnosticLogLevel::Warn;
        evt.SenderType = "TestSender";
        evt.Message = "Test diagnostic message";
        g_fakeSdk.lastDiagListener(evt);
    }

    EXPECT_TRUE(s_diag_callbackFired);
    EXPECT_EQ(s_diag_lastLevel, static_cast<int>(DiagnosticLogLevel::Warn));
    EXPECT_EQ(s_diag_lastSender, L"TestSender");
    // The message is the full ToString() output which includes timestamp, level, sender, and message
    EXPECT_NE(s_diag_lastMessage.find(L"Test diagnostic message"), std::wstring::npos);
}

TEST_F(BridgeDiagnosticsTest, ClearDiagnosticCallback_RemovesListener) {
    BridgeCallbacks cbs = {};
    cbs.initSuccess = DiagTestInitSuccess;
    Liftoff_SetCallbacks(cbs);

    Liftoff_SetDiagnosticCallback(DiagTestCallback);

    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    EXPECT_TRUE(g_fakeSdk.addDiagListenerCalled);

    // Clear the diagnostic callback
    Liftoff_ClearDiagnosticCallback();

    EXPECT_TRUE(g_fakeSdk.removeDiagListenerCalled);
}

TEST_F(BridgeDiagnosticsTest, DiagnosticCallback_NullAfterClear_NoCrash) {
    BridgeCallbacks cbs = {};
    cbs.initSuccess = DiagTestInitSuccess;
    Liftoff_SetCallbacks(cbs);

    Liftoff_SetDiagnosticCallback(DiagTestCallback);

    g_fakeSdk.initShouldSucceed = true;
    Liftoff_Initialize(L"test-app-id", nullptr);
    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    // Save the listener before clearing
    auto savedListener = g_fakeSdk.lastDiagListener;

    // Clear the callback
    Liftoff_ClearDiagnosticCallback();

    // Try to fire the old listener -- the bridge forwarder should check for null
    // callback and silently skip. This should not crash.
    if (savedListener) {
        DiagnosticLogEvent evt;
        evt.Level = DiagnosticLogLevel::Info;
        evt.SenderType = "TestSender";
        evt.Message = "Should be ignored";
        savedListener(evt);
    }

    // The callback should NOT have fired because we cleared it
    EXPECT_FALSE(s_diag_callbackFired);
}
