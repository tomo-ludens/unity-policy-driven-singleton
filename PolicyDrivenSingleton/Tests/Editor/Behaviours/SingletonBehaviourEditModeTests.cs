using NUnit.Framework;
using UnityEngine;
using PolicyDrivenSingleton.Tests.Editor.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Editor.Behaviours
{
    [TestFixture]
    public class SingletonBehaviourEditModeTests
    {
        [TearDown]
        public void TearDown()
        {
            // Clean up any created GameObjects
            var testObjects = Object.FindObjectsByType<GameObject>(sortMode: FindObjectsSortMode.None);
            foreach (var obj in testObjects)
            {
                if (obj.name.Contains(value: "Test") || obj.name.Contains(value: "Singleton"))
                {
                    Object.DestroyImmediate(obj: obj);
                }
            }

            // Reset static caches using TestExtensions (reflection-based)
            default(TestPersistentSingletonForEditMode).ResetStaticCacheForTesting();
        }

        [Test]
        public void PersistentSingleton_Instance_ReturnsNull_InEditMode()
        {
            // In Edit Mode, Instance should perform lookup only, not auto-creation
            var instance = TestPersistentSingletonForEditMode.Instance;
            Assert.IsNull(anObject: instance, message: "Instance should return null in Edit Mode when no instance exists");
        }

        [Test]
        public void PersistentSingleton_TryGetInstance_ReturnsFalse_InEditMode()
        {
            bool result = TestPersistentSingletonForEditMode.TryGetInstance(instance: out var instance);
            Assert.IsFalse(condition: result, message: "TryGetInstance should return false in Edit Mode when no instance exists");
            Assert.IsNull(anObject: instance, message: "Instance should be null");
        }

        [Test]
        public void SceneSingleton_Instance_ReturnsNull_InEditMode()
        {
            var instance = TestSceneSingletonForEditMode.Instance;
            Assert.IsNull(anObject: instance, message: "SceneSingleton.Instance should return null in Edit Mode when not placed");
        }

        [Test]
        public void SceneSingleton_TryGetInstance_ReturnsFalse_InEditMode()
        {
            bool result = TestSceneSingletonForEditMode.TryGetInstance(instance: out var instance);
            Assert.IsFalse(condition: result, message: "SceneSingleton.TryGetInstance should return false in Edit Mode when not placed");
            Assert.IsNull(anObject: instance, message: "Instance should be null");
        }

        [Test]
        public void Singleton_DoesNotCache_InEditMode()
        {
            // Create a temporary instance
            var go = new GameObject(name: "TestSingleton");

            // Access through Instance
            var instance1 = TestPersistentSingletonForEditMode.Instance;
            var instance2 = TestPersistentSingletonForEditMode.Instance;

            // Should return the same instance
            Assert.AreSame(expected: instance1, actual: instance2, message: "Should return the same instance");

            // Destroy the GameObject
            Object.DestroyImmediate(obj: go);

            // Access again - should not be cached from previous access
            var instance3 = TestPersistentSingletonForEditMode.Instance;
            Assert.IsNull(anObject: instance3, message: "Instance should not be cached from Edit Mode access");
        }
    }
}
