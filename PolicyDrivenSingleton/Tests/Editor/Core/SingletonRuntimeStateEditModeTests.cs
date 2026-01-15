using NUnit.Framework;
using PolicyDrivenSingleton.Core;

namespace PolicyDrivenSingleton.Tests.Editor.Core
{
    [TestFixture]
    public class SingletonRuntimeStateEditModeTests
    {
        [SetUp]
        public void SetUp()
        {
            TestExtensions.ResetQuittingFlagForTesting();
        }

        [Test]
        public void IsQuitting_CanBeSet_ViaNotifyQuitting()
        {
            Assert.IsFalse(condition: SingletonRuntime.IsQuitting, message: "Should start as false");

            SingletonRuntime.NotifyQuitting();

            Assert.IsTrue(condition: SingletonRuntime.IsQuitting, message: "Should be true after NotifyQuitting");
        }

        [Test]
        public void PlaySessionId_IsConsistent_WithinSameSession()
        {
            int id1 = SingletonRuntime.PlaySessionId;
            int id2 = SingletonRuntime.PlaySessionId;
            int id3 = SingletonRuntime.PlaySessionId;

            Assert.AreEqual(expected: id1, actual: id2, message: "PlaySessionId should be consistent");
            Assert.AreEqual(expected: id2, actual: id3, message: "PlaySessionId should be consistent");
        }
    }
}
