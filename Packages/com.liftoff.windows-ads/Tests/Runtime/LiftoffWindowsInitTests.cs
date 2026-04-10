using System;
using NUnit.Framework;

namespace Liftoff.Windows.Tests
{
    [TestFixture]
    public class LiftoffWindowsInitTests
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

        [Test]
        public void Initialize_CallsNativeWithAppIdAndHwnd()
        {
            var hwnd = new IntPtr(0x1234);
            LiftoffWindows.Initialize("test_app_id", hwnd);

            Assert.IsTrue(_fake.InitializeCalled);
            Assert.AreEqual("test_app_id", _fake.LastAppId);
            Assert.AreEqual(hwnd, _fake.LastHwnd);
        }

        [Test]
        public void Initialize_NullAppId_StillCallsNative()
        {
            LiftoffWindows.Initialize(null, IntPtr.Zero);
            Assert.IsTrue(_fake.InitializeCalled);
            Assert.IsNull(_fake.LastAppId);
        }

        [Test]
        public void Initialize_EmptyAppId_StillCallsNative()
        {
            LiftoffWindows.Initialize("", IntPtr.Zero);
            Assert.IsTrue(_fake.InitializeCalled);
            Assert.AreEqual("", _fake.LastAppId);
        }

        [Test]
        public void IsInitialized_WhenFalse_ReturnsFalse()
        {
            _fake.IsInitializedReturn = false;
            Assert.IsFalse(LiftoffWindows.IsInitialized);
        }

        [Test]
        public void IsInitialized_WhenTrue_ReturnsTrue()
        {
            _fake.IsInitializedReturn = true;
            Assert.IsTrue(LiftoffWindows.IsInitialized);
        }

        [Test]
        public void IsWebView2Available_DelegatesToNative()
        {
            _fake.IsWebView2Return = true;
            Assert.IsTrue(LiftoffWindows.IsWebView2Available());

            _fake.IsWebView2Return = false;
            Assert.IsFalse(LiftoffWindows.IsWebView2Available());
        }
    }
}
