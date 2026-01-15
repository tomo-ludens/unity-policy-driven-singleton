using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.Lifecycle
{
    [TestFixture]
    public class LifecycleTests
    {
        [TearDown]
        public void TearDown()
        {
            if (TestPersistentSingleton.TryGetInstance(instance: out var instance))
            {
                Object.DestroyImmediate(obj: instance.gameObject);
            }

            default(TestPersistentSingleton).ResetStaticCacheForTesting();
        }

        [UnityTest]
        public IEnumerator OnDestroy_CleansUpInstance()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Object.DestroyImmediate(obj: instance.gameObject);
            yield return null;

            bool exists = TestPersistentSingleton.TryGetInstance(instance: out _);
            Assert.IsFalse(condition: exists, message: "Instance should not exist after destruction");
        }

        [UnityTest]
        public IEnumerator Instance_CanBeRecreated_AfterDestruction()
        {
            var first = TestPersistentSingleton.Instance;
            yield return null;

            Object.DestroyImmediate(obj: first.gameObject);
            default(TestPersistentSingleton).ResetStaticCacheForTesting();
            yield return null;

            var second = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: second, message: "New instance should be created");
            Assert.AreNotSame(expected: first, actual: second, message: "Should be a different instance");
            Assert.AreEqual(expected: 1, actual: second.AwakeCount, message: "New instance should have fresh AwakeCount");
        }
    }
}
