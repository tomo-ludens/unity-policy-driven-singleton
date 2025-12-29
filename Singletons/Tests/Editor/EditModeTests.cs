using NUnit.Framework;
using Singletons.Core;

namespace Singletons.Tests.Editor
{
    [TestFixture]
    public class SingletonRuntimeEditModeTests
    {
        [Test]
        public void PlaySessionId_IsAccessible()
        {
            var sessionId = SingletonRuntime.PlaySessionId;
            Assert.GreaterOrEqual(arg1: sessionId, arg2: 0, message: "PlaySessionId should be non-negative");
        }
    }
}
