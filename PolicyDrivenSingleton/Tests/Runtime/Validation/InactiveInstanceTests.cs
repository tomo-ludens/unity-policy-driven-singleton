#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_ASSERTIONS
#define TEST_IS_DEV
#endif

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.Validation
{
    [TestFixture]
    public class InactiveInstanceTests
    {
        [TearDown]
        public void TearDown()
        {
            var allInstances = Object.FindObjectsByType<TestInactiveSingleton>(
                findObjectsInactive: FindObjectsInactive.Include,
                sortMode: FindObjectsSortMode.None
            );

            foreach (var instance in allInstances)
            {
                Object.DestroyImmediate(obj: instance.gameObject);
            }

            default(TestInactiveSingleton).ResetStaticCacheForTesting();
        }

        [UnityTest]
        public IEnumerator Instance_ThrowsInDev_WhenInactiveInstanceExists()
        {
            var go = new GameObject(name: "InactiveSingleton");
            go.AddComponent<TestInactiveSingleton>();
            go.SetActive(value: false);

            default(TestInactiveSingleton).ResetStaticCacheForTesting();
            yield return null;

#if TEST_IS_DEV
            Assert.Throws<InvalidOperationException>(
                code: () => { _ = TestInactiveSingleton.Instance; },
                message: "Should throw when inactive instance exists - auto-create blocked (dev-only)"
            );
#else
            var instance = TestInactiveSingleton.Instance;
            Assert.IsNotNull(anObject: instance, message: "Should auto-create in non-dev build");
#endif
        }

        [UnityTest]
        public IEnumerator TryGetInstance_ReturnsFalse_WhenOnlyInactiveInstanceExists()
        {
            var go = new GameObject(name: "InactiveSingleton");
            go.AddComponent<TestInactiveSingleton>();
            go.SetActive(value: false);

            default(TestInactiveSingleton).ResetStaticCacheForTesting();
            yield return null;

            bool result = TestInactiveSingleton.TryGetInstance(instance: out var instance);

            Assert.IsFalse(condition: result, message: "TryGetInstance should return false when only inactive exists");
            Assert.IsNull(anObject: instance, message: "Instance should be null");
        }

        [UnityTest]
        public IEnumerator DisabledComponent_ThrowsInDev()
        {
            var go = new GameObject(name: "DisabledComponent");
            var comp = go.AddComponent<TestInactiveSingleton>();
            yield return null;

            comp.enabled = false;
            default(TestInactiveSingleton).ResetStaticCacheForTesting();
            yield return null;

#if TEST_IS_DEV
            Assert.Throws<InvalidOperationException>(
                code: () => { _ = TestInactiveSingleton.Instance; },
                message: "Should throw when component is disabled (dev-only)"
            );
#else
            var instance = TestInactiveSingleton.Instance;
            Assert.IsNull(anObject: instance, message: "Should return null in non-dev build");
#endif
        }
    }
}
