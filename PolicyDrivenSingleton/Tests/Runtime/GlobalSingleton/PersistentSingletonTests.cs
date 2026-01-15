using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.GlobalSingleton
{
    [TestFixture]
    public class PersistentSingletonTests
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
        public IEnumerator Instance_AutoCreates_WhenNotExists()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: instance, message: "Instance should be auto-created");
            Assert.AreEqual(expected: 1, actual: instance.AwakeCount, message: "Awake should be called once");
        }

        [UnityTest]
        public IEnumerator Instance_ReturnsSameInstance_OnMultipleAccess()
        {
            var first = TestPersistentSingleton.Instance;
            yield return null;

            var second = TestPersistentSingleton.Instance;
            yield return null;

            Assert.AreSame(expected: first, actual: second, message: "Multiple accesses should return the same instance");
            Assert.AreEqual(expected: 1, actual: first.AwakeCount, message: "Awake should be called only once");
        }

        [UnityTest]
        public IEnumerator TryGetInstance_ReturnsFalse_WhenNotExists()
        {
            bool result = TestPersistentSingleton.TryGetInstance(instance: out var instance);
            yield return null;

            Assert.IsFalse(condition: result, message: "TryGetInstance should return false when no instance exists");
            Assert.IsNull(anObject: instance, message: "Out parameter should be null");
        }

        [UnityTest]
        public IEnumerator TryGetInstance_ReturnsTrue_WhenExists()
        {
            var created = TestPersistentSingleton.Instance;
            yield return null;

            bool result = TestPersistentSingleton.TryGetInstance(instance: out var instance);

            Assert.IsTrue(condition: result, message: "TryGetInstance should return true when instance exists");
            Assert.AreSame(expected: created, actual: instance, message: "Should return the same instance");
        }

        [UnityTest]
        public IEnumerator TryGetInstance_DoesNotAutoCreate()
        {
            TestPersistentSingleton.TryGetInstance(instance: out _);
            yield return null;

            bool exists = TestPersistentSingleton.TryGetInstance(instance: out _);

            Assert.IsFalse(condition: exists, message: "TryGetInstance should not auto-create");
        }

        [UnityTest]
        public IEnumerator Duplicate_IsDestroyed()
        {
            var first = TestPersistentSingleton.Instance;
            yield return null;

            var duplicateGo = new GameObject(name: "Duplicate");
            duplicateGo.AddComponent<TestPersistentSingleton>();
            yield return null;

            Assert.AreSame(expected: first, actual: TestPersistentSingleton.Instance, message: "Original instance should remain");
            Assert.AreEqual(
                expected: 1,
                actual: Object.FindObjectsByType<TestPersistentSingleton>(sortMode: FindObjectsSortMode.None).Length,
                message: "Only one instance should exist"
            );
        }

        [UnityTest]
        public IEnumerator Instance_HasDontDestroyOnLoad()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsTrue(
                condition: instance.gameObject.scene.name == "DontDestroyOnLoad" || instance.gameObject.scene.buildIndex == -1,
                message: "Persistent singleton should be in DontDestroyOnLoad scene"
            );
        }
    }
}
