using NUnit.Framework;
using PolicyDrivenSingleton.Core;

namespace PolicyDrivenSingleton.Tests.Editor.Core
{
    [TestFixture]
    public class SingletonRuntimeEditModeTests
    {
        [Test]
        public void PlaySessionId_IsAccessible()
        {
            int sessionId = SingletonRuntime.PlaySessionId;
            Assert.GreaterOrEqual(arg1: sessionId, arg2: 0, message: "PlaySessionId should be non-negative");
        }

        [Test]
        public void IsQuitting_ReturnsFalse_InEditMode()
        {
            TestExtensions.ResetQuittingFlagForTesting();
            Assert.IsFalse(condition: SingletonRuntime.IsQuitting, message: "IsQuitting should be false in Edit Mode");
        }
    }
}
