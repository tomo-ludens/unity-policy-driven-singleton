using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.GlobalSingleton
{
    [TestFixture]
    public class SoftResetTests
    {
        [TearDown]
        public void TearDown()
        {
            if (TestSoftResetSingleton.TryGetInstance(instance: out var instance))
            {
                Object.DestroyImmediate(obj: instance.gameObject);
            }

            default(TestSoftResetSingleton).ResetStaticCacheForTesting();
        }

        [UnityTest]
        public IEnumerator Reinitializes_PerPlaySession_WhenPlaySessionIdChanges()
        {
            var instance = TestSoftResetSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: instance);
            Assert.AreEqual(expected: 1, actual: instance.AwakeCalls, message: "Awake should run once per GameObject lifetime");
            Assert.AreEqual(expected: 1, actual: instance.PlaySessionStartCalls, message: "OnPlaySessionStart should run on first establishment");

            TestExtensions.AdvancePlaySessionIdForTesting();

            var sameInstance = TestSoftResetSingleton.Instance;
            yield return null;

            Assert.AreSame(expected: instance, actual: sameInstance, message: "Instance should be re-used (not recreated) across play session boundary");
            Assert.AreEqual(expected: 1, actual: sameInstance.AwakeCalls, message: "Awake should not be re-run on play session boundary");
            Assert.AreEqual(expected: 2, actual: sameInstance.PlaySessionStartCalls, message: "OnPlaySessionStart should run once per Play session");
            Assert.AreEqual(
                expected: 1,
                actual: Object.FindObjectsByType<TestSoftResetSingleton>(sortMode: FindObjectsSortMode.None).Length,
                message: "Only one instance should exist"
            );
        }
    }
}
