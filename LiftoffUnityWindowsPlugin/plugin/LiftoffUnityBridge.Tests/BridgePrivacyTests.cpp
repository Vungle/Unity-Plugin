#include <gtest/gtest.h>
#include "../LiftoffUnityBridge/LiftoffUnityBridge.h"
#include "FakeSdk/FakeLiftoffAds.h"

class BridgePrivacyTest : public ::testing::Test {
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
};

// ---- COPPA ----

TEST_F(BridgePrivacyTest, SetCoppaStatus_True_ForwardsToSdk) {
    Liftoff_SetCoppaStatus(true);

    EXPECT_TRUE(g_fakeSdk.setCoppaCalled);
    EXPECT_TRUE(g_fakeSdk.lastCoppaStatus);
}

TEST_F(BridgePrivacyTest, SetCoppaStatus_False_ForwardsToSdk) {
    Liftoff_SetCoppaStatus(false);

    EXPECT_TRUE(g_fakeSdk.setCoppaCalled);
    EXPECT_FALSE(g_fakeSdk.lastCoppaStatus);
}

// ---- CCPA ----

TEST_F(BridgePrivacyTest, SetCcpaStatus_OptedIn_Forwards) {
    Liftoff_SetCcpaStatus(1); // OptedIn

    EXPECT_TRUE(g_fakeSdk.setCcpaCalled);
    EXPECT_EQ(g_fakeSdk.lastCcpaStatus, CcpaConsentStatus::OptedIn);
}

TEST_F(BridgePrivacyTest, SetCcpaStatus_OptedOut_Forwards) {
    Liftoff_SetCcpaStatus(2); // OptedOut

    EXPECT_TRUE(g_fakeSdk.setCcpaCalled);
    EXPECT_EQ(g_fakeSdk.lastCcpaStatus, CcpaConsentStatus::OptedOut);
}

TEST_F(BridgePrivacyTest, SetCcpaStatus_InvalidValue_NoOp) {
    // Pass invalid values -- should not call SDK
    Liftoff_SetCcpaStatus(0);
    EXPECT_FALSE(g_fakeSdk.setCcpaCalled);

    Liftoff_SetCcpaStatus(3);
    EXPECT_FALSE(g_fakeSdk.setCcpaCalled);
}

// ---- GDPR ----

TEST_F(BridgePrivacyTest, SetGdprConsentStatus_Accepted_Forwards) {
    Liftoff_SetGdprConsentStatus(1, L"v1.0"); // ConsentAccepted

    EXPECT_TRUE(g_fakeSdk.setGdprCalled);
    EXPECT_EQ(g_fakeSdk.lastGdprStatus, GdprConsentStatus::ConsentAccepted);
    EXPECT_EQ(g_fakeSdk.lastGdprVersion, "v1.0");
}

TEST_F(BridgePrivacyTest, SetGdprConsentStatus_Denied_ForwardsWithVersion) {
    Liftoff_SetGdprConsentStatus(2, L"v2.0"); // ConsentDenied

    EXPECT_TRUE(g_fakeSdk.setGdprCalled);
    EXPECT_EQ(g_fakeSdk.lastGdprStatus, GdprConsentStatus::ConsentDenied);
    EXPECT_EQ(g_fakeSdk.lastGdprVersion, "v2.0");
}

TEST_F(BridgePrivacyTest, SetGdprConsentStatus_InvalidValue_NoOp) {
    Liftoff_SetGdprConsentStatus(0, L"v1.0");
    EXPECT_FALSE(g_fakeSdk.setGdprCalled);

    Liftoff_SetGdprConsentStatus(3, L"v1.0");
    EXPECT_FALSE(g_fakeSdk.setGdprCalled);
}

// ---- ASHWID ----

TEST_F(BridgePrivacyTest, SetDisableAshwidTracking_True_Forwards) {
    Liftoff_SetDisableAshwidTracking(true);

    EXPECT_TRUE(g_fakeSdk.setAshwidCalled);
    EXPECT_TRUE(g_fakeSdk.lastAshwidDisabled);
}
