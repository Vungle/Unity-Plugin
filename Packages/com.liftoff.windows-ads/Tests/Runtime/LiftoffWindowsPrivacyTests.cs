using NUnit.Framework;

namespace Liftoff.Windows.Tests
{
    [TestFixture]
    public class LiftoffWindowsPrivacyTests
    {
        FakeNativeBridge _fake;

        [SetUp]
        public void SetUp()
        {
            _fake = new FakeNativeBridge();
            LiftoffWindows.ResetForTesting(_fake);
        }

        [TearDown]
        public void TearDown()
        {
            LiftoffWindows.RestoreAfterTesting();
        }

        // --- COPPA ---

        [Test]
        public void SetCoppaStatus_True_CallsNativeWithTrue()
        {
            LiftoffWindows.SetCoppaStatus(true);
            Assert.IsTrue(_fake.SetCoppaCalled);
            Assert.IsTrue(_fake.LastCoppaStatus);
        }

        [Test]
        public void SetCoppaStatus_False_CallsNativeWithFalse()
        {
            LiftoffWindows.SetCoppaStatus(false);
            Assert.IsTrue(_fake.SetCoppaCalled);
            Assert.IsFalse(_fake.LastCoppaStatus);
        }

        // --- CCPA ---

        [Test]
        public void SetCcpaStatus_OptIn_CallsNativeWith1()
        {
            LiftoffWindows.SetCcpaStatus(true);
            Assert.IsTrue(_fake.SetCcpaCalled);
            Assert.AreEqual(1, _fake.LastCcpaStatus);
        }

        [Test]
        public void SetCcpaStatus_OptOut_CallsNativeWith2()
        {
            LiftoffWindows.SetCcpaStatus(false);
            Assert.IsTrue(_fake.SetCcpaCalled);
            Assert.AreEqual(2, _fake.LastCcpaStatus);
        }

        // --- GDPR ---

        [Test]
        public void SetGdprConsentStatus_OptIn_CallsNativeWith1()
        {
            LiftoffWindows.SetGdprConsentStatus(true, "v1.0");
            Assert.IsTrue(_fake.SetGdprCalled);
            Assert.AreEqual(1, _fake.LastGdprStatus);
            Assert.AreEqual("v1.0", _fake.LastGdprVersion);
        }

        [Test]
        public void SetGdprConsentStatus_OptOut_CallsNativeWith2()
        {
            LiftoffWindows.SetGdprConsentStatus(false, "v2.0");
            Assert.IsTrue(_fake.SetGdprCalled);
            Assert.AreEqual(2, _fake.LastGdprStatus);
            Assert.AreEqual("v2.0", _fake.LastGdprVersion);
        }

        [Test]
        public void SetGdprConsentStatus_NullVersion_PassesEmpty()
        {
            LiftoffWindows.SetGdprConsentStatus(true, null);
            Assert.IsTrue(_fake.SetGdprCalled);
            Assert.AreEqual("", _fake.LastGdprVersion);
        }

        [Test]
        public void SetGdprConsentStatus_DefaultVersion_PassesEmpty()
        {
            LiftoffWindows.SetGdprConsentStatus(true);
            Assert.IsTrue(_fake.SetGdprCalled);
            Assert.AreEqual("", _fake.LastGdprVersion);
        }

        // --- ASHWID ---

        [Test]
        public void SetDisableAshwidTracking_True_CallsNative()
        {
            LiftoffWindows.SetDisableAshwidTracking(true);
            Assert.IsTrue(_fake.SetAshwidCalled);
            Assert.IsTrue(_fake.LastAshwidDisabled);
        }

        [Test]
        public void SetDisableAshwidTracking_False_CallsNative()
        {
            LiftoffWindows.SetDisableAshwidTracking(false);
            Assert.IsTrue(_fake.SetAshwidCalled);
            Assert.IsFalse(_fake.LastAshwidDisabled);
        }
    }
}
