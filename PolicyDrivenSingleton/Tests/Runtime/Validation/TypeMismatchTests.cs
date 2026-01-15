#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_ASSERTIONS
#define TEST_IS_DEV
#endif

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using PolicyDrivenSingleton.Tests.Runtime.Infrastructure;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.Validation
{
    [TestFixture]
    public class TypeMismatchTests
    {
        [TearDown]
        public void TearDown()
        {
            var baseInstances = Object.FindObjectsByType<TestBaseSingleton>(
                findObjectsInactive: FindObjectsInactive.Include,
                sortMode: FindObjectsSortMode.None
            );

            foreach (var instance in baseInstances)
            {
                Object.DestroyImmediate(obj: instance.gameObject);
            }

            default(TestBaseSingleton).ResetStaticCacheForTesting();
        }

        [UnityTest]
        public IEnumerator DerivedClass_IsRejected_AndDestroyed()
        {
            var start = TestLogCounter.Take();

#if TEST_IS_DEV
            using (new IgnoreFailingMessagesScope(enabled: true))
            {
                var go = new GameObject(name: "DerivedSingleton");
                go.AddComponent<TestDerivedSingleton>();
                yield return null;
            }

            var deltaDev = TestLogCounter.Take().Delta(baseline: start);
            Assert.AreEqual(expected: 1, actual: deltaDev.Error, message: "Type mismatch should emit exactly one error log (dev-only)");
            Assert.AreEqual(expected: 0, actual: deltaDev.Exception, message: "No exceptions expected (dev-only)");
            Assert.AreEqual(expected: 0, actual: deltaDev.Assert, message: "No asserts expected (dev-only)");
#else
            var go = new GameObject(name: "DerivedSingleton");
            go.AddComponent<TestDerivedSingleton>();
            yield return null;
#endif

            var instance = TestBaseSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: instance, message: "Should auto-create correct type");
            Assert.AreEqual(expected: typeof(TestBaseSingleton), actual: instance.GetType(), message: "Instance should be exact type, not derived");

            var derivedInstances = Object.FindObjectsByType<TestDerivedSingleton>(
                findObjectsInactive: FindObjectsInactive.Include,
                sortMode: FindObjectsSortMode.None
            );
            Assert.AreEqual(expected: 0, actual: derivedInstances.Length, message: "Derived singleton instance should be destroyed");
        }

        [UnityTest]
        public IEnumerator BaseClass_IsAccepted()
        {
            var go = new GameObject(name: "BaseSingleton");
            var placed = go.AddComponent<TestBaseSingleton>();
            yield return null;

            var instance = TestBaseSingleton.Instance;

            Assert.AreSame(expected: placed, actual: instance, message: "Base class instance should be accepted");
            Assert.AreEqual(expected: typeof(TestBaseSingleton), actual: instance.GetType());
        }
    }
}
