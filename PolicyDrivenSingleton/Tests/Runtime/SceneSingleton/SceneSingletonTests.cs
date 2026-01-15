using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.SceneSingleton
{
    [TestFixture]
    public class SceneSingletonTests
    {
        [TearDown]
        public void TearDown()
        {
            if (TestSceneSingleton.TryGetInstance(instance: out var instance))
            {
                Object.DestroyImmediate(obj: instance.gameObject);
            }

            default(TestSceneSingleton).ResetStaticCacheForTesting();
        }

        [UnityTest]
        public IEnumerator Instance_ReturnsPlacedInstance()
        {
            var go = new GameObject(name: "TestSceneSingleton");
            var placed = go.AddComponent<TestSceneSingleton>();
            yield return null;

            var instance = TestSceneSingleton.Instance;

            Assert.AreSame(expected: placed, actual: instance, message: "Should return the placed instance");
            Assert.IsTrue(condition: instance.WasInitialized, message: "Awake should be called");
        }

        [UnityTest]
        public IEnumerator TryGetInstance_ReturnsFalse_WhenNotPlaced()
        {
            bool result = TestSceneSingleton.TryGetInstance(instance: out var instance);
            yield return null;

            Assert.IsFalse(condition: result, message: "TryGetInstance should return false when not placed");
            Assert.IsNull(anObject: instance, message: "Instance should be null");
        }

        [UnityTest]
        public IEnumerator TryGetInstance_ReturnsTrue_WhenPlaced()
        {
            var go = new GameObject(name: "TestSceneSingleton");
            go.AddComponent<TestSceneSingleton>();
            yield return null;

            bool result = TestSceneSingleton.TryGetInstance(instance: out var instance);

            Assert.IsTrue(condition: result, message: "TryGetInstance should return true when placed");
            Assert.IsNotNull(anObject: instance, message: "Instance should not be null");
        }

        [UnityTest]
        public IEnumerator SceneSingleton_DoesNotHaveDontDestroyOnLoad()
        {
            var go = new GameObject(name: "TestSceneSingleton");
            go.AddComponent<TestSceneSingleton>();
            yield return null;

            var instance = TestSceneSingleton.Instance;

            Assert.AreNotEqual(expected: "DontDestroyOnLoad", actual: instance.gameObject.scene.name, message: "Scene singleton should NOT be in DontDestroyOnLoad");
        }

        [UnityTest]
        public IEnumerator Duplicate_IsDestroyed()
        {
            var go1 = new GameObject(name: "First");
            var first = go1.AddComponent<TestSceneSingleton>();
            yield return null;

            var go2 = new GameObject(name: "Second");
            go2.AddComponent<TestSceneSingleton>();
            yield return null;

            Assert.AreSame(expected: first, actual: TestSceneSingleton.Instance, message: "First instance should remain");
            Assert.AreEqual(
                expected: 1,
                actual: Object.FindObjectsByType<TestSceneSingleton>(sortMode: FindObjectsSortMode.None).Length,
                message: "Only one instance should exist"
            );
        }
    }
}
