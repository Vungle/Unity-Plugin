using System;
using NUnit.Framework;

namespace Liftoff.Windows.Tests
{
    [TestFixture]
    public class LiftoffWindowsSuperTokenTests
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
        public void GetSuperToken_DelegatesToNative()
        {
            _fake.SuperTokenResult = "super_token_abc";
            string result = LiftoffWindows.GetSuperToken("test_placement");

            Assert.IsTrue(_fake.GetSuperTokenCalled);
            Assert.AreEqual("test_placement", _fake.LastSuperTokenPlacement);
            Assert.AreEqual("super_token_abc", result);
        }

        [Test]
        public void GetSuperToken_NullResult_ReturnsNull()
        {
            _fake.SuperTokenResult = null;
            string result = LiftoffWindows.GetSuperToken("test_placement");

            Assert.IsTrue(_fake.GetSuperTokenCalled);
            Assert.IsNull(result);
        }

        [Test]
        public void GetSuperToken_EmptyPlacement_StillCallsNative()
        {
            _fake.SuperTokenResult = "token";
            LiftoffWindows.GetSuperToken("");

            Assert.IsTrue(_fake.GetSuperTokenCalled);
            Assert.AreEqual("", _fake.LastSuperTokenPlacement);
        }

        [Test]
        public void GetSuperToken_NullPlacement_StillCallsNative()
        {
            _fake.SuperTokenResult = "token";
            LiftoffWindows.GetSuperToken(null);

            Assert.IsTrue(_fake.GetSuperTokenCalled);
            Assert.IsNull(_fake.LastSuperTokenPlacement);
        }

        [Test]
        public void GetSuperToken_UnicodeToken_ReturnedCorrectly()
        {
            _fake.SuperTokenResult = "token_\u00e9\u00e8\u00ea";
            string result = LiftoffWindows.GetSuperToken("pl");

            Assert.AreEqual("token_\u00e9\u00e8\u00ea", result);
        }
    }
}
