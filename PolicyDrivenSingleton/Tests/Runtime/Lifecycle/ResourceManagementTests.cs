using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.Lifecycle
{
    [TestFixture]
    public class ResourceManagementTests
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
        public IEnumerator Singleton_CanBeDestroyed_Safely()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: instance, message: "Should create instance");

            Object.DestroyImmediate(obj: instance.gameObject);
            yield return null;

            var newInstance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: newInstance, message: "Should create new instance after destruction");
            Assert.AreNotSame(expected: instance, actual: newInstance, message: "Should be different instances");
        }

        [UnityTest]
        public IEnumerator DestroyedInstance_DoesNotAffectNewInstance()
        {
            var firstInstance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: firstInstance, message: "Should create first instance");

            Object.DestroyImmediate(obj: firstInstance.gameObject);
            yield return null;

            var secondInstance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: secondInstance, message: "Should create second instance");
            Assert.AreNotSame(expected: firstInstance, actual: secondInstance, message: "Should be different instances");
            Assert.IsFalse(condition: ReferenceEquals(objA: null, objB: secondInstance.gameObject), message: "Second instance GameObject should exist");
        }

        [UnityTest]
        public IEnumerator Instance_CanBeAccessed_DuringDestruction()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: instance, message: "Should create instance");

            var sameInstance = TestPersistentSingleton.Instance;
            Assert.AreSame(expected: instance, actual: sameInstance, message: "Should return same instance while alive");

            Object.DestroyImmediate(obj: instance.gameObject);
            yield return null;

            bool result = TestPersistentSingleton.TryGetInstance(instance: out var retrieved);
            Assert.IsFalse(condition: result, message: "TryGetInstance should return false after destruction");
            Assert.IsNull(anObject: retrieved, message: "Retrieved instance should be null after destruction");
        }
    }
}
