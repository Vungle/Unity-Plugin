using NUnit.Framework;

namespace Liftoff.Windows.Tests
{
    [TestFixture]
    public class LiftoffWindowsAdTests
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

        // --- LoadAd ---

        [Test]
        public void LoadAd_NoMarkup_CallsNativeLoadAd()
        {
            LiftoffWindows.LoadAd("placement_1");

            Assert.IsTrue(_fake.LoadAdCalled);
            Assert.IsFalse(_fake.LoadAdWithMarkupCalled);
            Assert.AreEqual("placement_1", _fake.LastLoadPlacement);
        }

        [Test]
        public void LoadAd_NullMarkup_CallsNativeLoadAd()
        {
            LiftoffWindows.LoadAd("placement_1", null);

            Assert.IsTrue(_fake.LoadAdCalled);
            Assert.IsFalse(_fake.LoadAdWithMarkupCalled);
        }

        [Test]
        public void LoadAd_EmptyMarkup_CallsNativeLoadAd()
        {
            LiftoffWindows.LoadAd("placement_1", "");

            Assert.IsTrue(_fake.LoadAdCalled);
            Assert.IsFalse(_fake.LoadAdWithMarkupCalled);
        }

        [Test]
        public void LoadAd_WhitespaceMarkup_CallsNativeLoadAd()
        {
            LiftoffWindows.LoadAd("placement_1", "   ");

            Assert.IsTrue(_fake.LoadAdCalled);
            Assert.IsFalse(_fake.LoadAdWithMarkupCalled);
        }

        [Test]
        public void LoadAd_WithMarkup_CallsNativeLoadAdWithMarkup()
        {
            LiftoffWindows.LoadAd("placement_1", "<markup>data</markup>");

            Assert.IsFalse(_fake.LoadAdCalled);
            Assert.IsTrue(_fake.LoadAdWithMarkupCalled);
            Assert.AreEqual("placement_1", _fake.LastLoadPlacement);
            Assert.AreEqual("<markup>data</markup>", _fake.LastLoadMarkup);
        }

        // --- PlayAd ---

        [Test]
        public void PlayAd_NoMarkup_CallsNativePlayAd()
        {
            LiftoffWindows.PlayAd("placement_2");

            Assert.IsTrue(_fake.PlayAdCalled);
            Assert.IsFalse(_fake.PlayAdWithMarkupCalled);
            Assert.AreEqual("placement_2", _fake.LastPlayPlacement);
        }

        [Test]
        public void PlayAd_NullMarkup_CallsNativePlayAd()
        {
            LiftoffWindows.PlayAd("placement_2", null);

            Assert.IsTrue(_fake.PlayAdCalled);
            Assert.IsFalse(_fake.PlayAdWithMarkupCalled);
        }

        [Test]
        public void PlayAd_EmptyMarkup_CallsNativePlayAd()
        {
            LiftoffWindows.PlayAd("placement_2", "");

            Assert.IsTrue(_fake.PlayAdCalled);
            Assert.IsFalse(_fake.PlayAdWithMarkupCalled);
        }

        [Test]
        public void PlayAd_WithMarkup_CallsNativePlayAdWithMarkup()
        {
            LiftoffWindows.PlayAd("placement_2", "<bid>token</bid>");

            Assert.IsFalse(_fake.PlayAdCalled);
            Assert.IsTrue(_fake.PlayAdWithMarkupCalled);
            Assert.AreEqual("placement_2", _fake.LastPlayPlacement);
            Assert.AreEqual("<bid>token</bid>", _fake.LastPlayMarkup);
        }
    }
}
