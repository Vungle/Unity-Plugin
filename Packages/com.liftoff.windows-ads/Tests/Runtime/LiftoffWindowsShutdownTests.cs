using NUnit.Framework;

namespace Liftoff.Windows.Tests
{
    [TestFixture]
    public class LiftoffWindowsShutdownTests
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
        public void Shutdown_CallsNativeShutdown()
        {
            LiftoffWindows.Shutdown();
            Assert.IsTrue(_fake.ShutdownCalled);
        }

        [Test]
        public void Shutdown_ClearsOnInitialized()
        {
            bool fired = false;
            LiftoffWindows.OnInitialized += () => fired = true;

            LiftoffWindows.Shutdown();

            // Re-inject bridge since Shutdown clears events
            LiftoffWindows.ResetForTesting(_fake);
            LiftoffWindows.InitOkTrampoline();

            Assert.IsFalse(fired, "Event should have been cleared by Shutdown");
        }

        [Test]
        public void Shutdown_ClearsOnAdLoaded()
        {
            bool fired = false;
            LiftoffWindows.OnAdLoaded += _ => fired = true;

            LiftoffWindows.Shutdown();

            LiftoffWindows.ResetForTesting(_fake);
            LiftoffWindows.LoadOkTrampoline("p");

            Assert.IsFalse(fired, "Event should have been cleared by Shutdown");
        }

        [Test]
        public void Shutdown_ClearsOnAdLoadFailed()
        {
            bool fired = false;
            LiftoffWindows.OnAdLoadFailed += (_, __, ___) => fired = true;

            LiftoffWindows.Shutdown();

            LiftoffWindows.ResetForTesting(_fake);
            LiftoffWindows.LoadFailTrampoline("p", 0, "m");

            Assert.IsFalse(fired, "Event should have been cleared by Shutdown");
        }

        [Test]
        public void Shutdown_ClearsAllAdPlayEvents()
        {
            bool startFired = false, endFired = false, failFired = false;
            bool rewardFired = false, clickFired = false;
            LiftoffWindows.OnAdStart += (_, __) => startFired = true;
            LiftoffWindows.OnAdEnd += _ => endFired = true;
            LiftoffWindows.OnAdPlayFailed += (_, __, ___) => failFired = true;
            LiftoffWindows.OnAdRewarded += _ => rewardFired = true;
            LiftoffWindows.OnAdClick += _ => clickFired = true;

            LiftoffWindows.Shutdown();

            LiftoffWindows.ResetForTesting(_fake);
            LiftoffWindows.AdStartTrampoline("p", "e");
            LiftoffWindows.AdEndTrampoline("p");
            LiftoffWindows.AdPlayFailTrampoline("p", 0, "m");
            LiftoffWindows.AdRewardedTrampoline("p");
            LiftoffWindows.AdClickTrampoline("p");

            Assert.IsFalse(startFired);
            Assert.IsFalse(endFired);
            Assert.IsFalse(failFired);
            Assert.IsFalse(rewardFired);
            Assert.IsFalse(clickFired);
        }

        [Test]
        public void Shutdown_ClearsDiagnosticEvent()
        {
            bool fired = false;
            LiftoffWindows.OnDiagnostic += (_, __, ___) => fired = true;

            LiftoffWindows.Shutdown();

            LiftoffWindows.ResetForTesting(_fake);
            LiftoffWindows.DiagnosticTrampoline(0, "s", "m");

            Assert.IsFalse(fired, "Diagnostic event should have been cleared by Shutdown");
        }
    }
}
