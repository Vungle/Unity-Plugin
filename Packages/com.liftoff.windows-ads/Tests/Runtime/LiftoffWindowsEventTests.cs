using System;
using System.Threading;
using NUnit.Framework;

namespace Liftoff.Windows.Tests
{
    [TestFixture]
    public class LiftoffWindowsEventTests
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

        // --- OnInitialized ---

        [Test]
        public void InitOkTrampoline_FiresOnInitialized()
        {
            bool fired = false;
            LiftoffWindows.OnInitialized += () => fired = true;

            // Call the trampoline directly (simulates native callback).
            // LiftoffMainThread.Post runs synchronously when called from main thread.
            LiftoffWindows.InitOkTrampoline();

            Assert.IsTrue(fired);
        }

        // --- OnInitializationFailed ---

        [Test]
        public void InitFailTrampoline_FiresOnInitializationFailed()
        {
            int receivedCode = -1;
            string receivedMessage = null;
            LiftoffWindows.OnInitializationFailed += (code, msg) =>
            {
                receivedCode = code;
                receivedMessage = msg;
            };

            LiftoffWindows.InitFailTrampoline(42, "test error");

            Assert.AreEqual(42, receivedCode);
            Assert.AreEqual("test error", receivedMessage);
        }

        // --- OnAdLoaded ---

        [Test]
        public void LoadOkTrampoline_FiresOnAdLoaded()
        {
            string receivedPlacement = null;
            LiftoffWindows.OnAdLoaded += p => receivedPlacement = p;

            LiftoffWindows.LoadOkTrampoline("test_placement");

            Assert.AreEqual("test_placement", receivedPlacement);
        }

        // --- OnAdLoadFailed ---

        [Test]
        public void LoadFailTrampoline_FiresOnAdLoadFailed()
        {
            string receivedPlacement = null;
            int receivedCode = -1;
            string receivedMessage = null;
            LiftoffWindows.OnAdLoadFailed += (p, c, m) =>
            {
                receivedPlacement = p;
                receivedCode = c;
                receivedMessage = m;
            };

            LiftoffWindows.LoadFailTrampoline("pl", 99, "load error");

            Assert.AreEqual("pl", receivedPlacement);
            Assert.AreEqual(99, receivedCode);
            Assert.AreEqual("load error", receivedMessage);
        }

        // --- OnAdStart ---

        [Test]
        public void AdStartTrampoline_FiresOnAdStart()
        {
            string receivedPlacement = null;
            string receivedEventId = null;
            LiftoffWindows.OnAdStart += (p, e) =>
            {
                receivedPlacement = p;
                receivedEventId = e;
            };

            LiftoffWindows.AdStartTrampoline("pl", "evt_123");

            Assert.AreEqual("pl", receivedPlacement);
            Assert.AreEqual("evt_123", receivedEventId);
        }

        // --- OnAdEnd ---

        [Test]
        public void AdEndTrampoline_FiresOnAdEnd()
        {
            string receivedPlacement = null;
            LiftoffWindows.OnAdEnd += p => receivedPlacement = p;

            LiftoffWindows.AdEndTrampoline("pl");

            Assert.AreEqual("pl", receivedPlacement);
        }

        // --- OnAdPlayFailed ---

        [Test]
        public void AdPlayFailTrampoline_FiresOnAdPlayFailed()
        {
            string receivedPlacement = null;
            int receivedCode = -1;
            string receivedMessage = null;
            LiftoffWindows.OnAdPlayFailed += (p, c, m) =>
            {
                receivedPlacement = p;
                receivedCode = c;
                receivedMessage = m;
            };

            LiftoffWindows.AdPlayFailTrampoline("pl", 5, "play error");

            Assert.AreEqual("pl", receivedPlacement);
            Assert.AreEqual(5, receivedCode);
            Assert.AreEqual("play error", receivedMessage);
        }

        // --- OnAdRewarded ---

        [Test]
        public void AdRewardedTrampoline_FiresOnAdRewarded()
        {
            string receivedPlacement = null;
            LiftoffWindows.OnAdRewarded += p => receivedPlacement = p;

            LiftoffWindows.AdRewardedTrampoline("pl");

            Assert.AreEqual("pl", receivedPlacement);
        }

        // --- OnAdClick ---

        [Test]
        public void AdClickTrampoline_FiresOnAdClick()
        {
            string receivedPlacement = null;
            LiftoffWindows.OnAdClick += p => receivedPlacement = p;

            LiftoffWindows.AdClickTrampoline("pl");

            Assert.AreEqual("pl", receivedPlacement);
        }

        // --- OnDiagnostic ---

        [Test]
        public void DiagnosticTrampoline_FiresOnDiagnostic()
        {
            int receivedLevel = -1;
            string receivedSender = null;
            string receivedMessage = null;
            LiftoffWindows.OnDiagnostic += (l, s, m) =>
            {
                receivedLevel = l;
                receivedSender = s;
                receivedMessage = m;
            };

            LiftoffWindows.DiagnosticTrampoline(3, "TestSender", "diag message");

            Assert.AreEqual(3, receivedLevel);
            Assert.AreEqual("TestSender", receivedSender);
            Assert.AreEqual("diag message", receivedMessage);
        }

        // --- No subscribers (no crash) ---

        [Test]
        public void AllTrampolines_NoSubscribers_NoCrash()
        {
            // Events are cleared by ResetForTesting. Calling trampolines
            // with no subscribers should not throw.
            Assert.DoesNotThrow(() =>
            {
                LiftoffWindows.InitOkTrampoline();
                LiftoffWindows.InitFailTrampoline(0, "msg");
                LiftoffWindows.LoadOkTrampoline("p");
                LiftoffWindows.LoadFailTrampoline("p", 0, "m");
                LiftoffWindows.AdStartTrampoline("p", "e");
                LiftoffWindows.AdEndTrampoline("p");
                LiftoffWindows.AdPlayFailTrampoline("p", 0, "m");
                LiftoffWindows.AdRewardedTrampoline("p");
                LiftoffWindows.AdClickTrampoline("p");
                LiftoffWindows.DiagnosticTrampoline(0, "s", "m");
            });
        }
    }
}
