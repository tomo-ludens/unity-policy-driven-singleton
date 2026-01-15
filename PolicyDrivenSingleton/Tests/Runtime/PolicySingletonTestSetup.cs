using NUnit.Framework;
using PolicyDrivenSingleton.Tests.Runtime.Infrastructure;

namespace PolicyDrivenSingleton.Tests.Runtime
{
    [SetUpFixture]
    public sealed class PolicySingletonTestSetup
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestLogCounter.Install();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestLogCounter.Uninstall();
        }
    }
}
