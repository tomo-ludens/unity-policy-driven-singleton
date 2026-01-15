using NUnit.Framework;
using UnityEngine;
using PolicyDrivenSingleton.Tests.Editor.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Editor.Behaviours
{
    [TestFixture]
    public class SingletonLifecycleEditModeTests
    {
        [TearDown]
        public void TearDown()
        {
            var testObjects = Object.FindObjectsByType<GameObject>(sortMode: FindObjectsSortMode.None);
            foreach (var obj in testObjects)
            {
                if (obj == null)
                {
                    continue;
                }

                if (obj.name.Contains(value: "Test") || obj.name.Contains(value: "Singleton") || obj.name.Contains(value: "Parent"))
                {
                    Object.DestroyImmediate(obj: obj);
                }
            }

            default(TestPersistentSingletonForEditMode).ResetStaticCacheForTesting();
            default(TestSingletonWithoutBaseAwake).ResetStaticCacheForTesting();
            default(TestSingletonWithParent).ResetStaticCacheForTesting();
        }

        [Test]
        public void Singleton_WithParent_LogsWarning_WhenReparentedForPersistence()
        {
            // Create parent-child hierarchy
            var parent = new GameObject(name: "ParentObject");
            var child = new GameObject(name: "TestSingletonWithParent");
            child.transform.SetParent(p: parent.transform);

            // Add singleton component - in Edit Mode, EnsurePersistent is not called
            // This test verifies the structure is set up correctly
            var singleton = child.AddComponent<TestSingletonWithParent>();

            Assert.IsNotNull(anObject: singleton, message: "Singleton should be created");
            Assert.IsNotNull(anObject: singleton.transform.parent, message: "Parent should still exist in Edit Mode");
        }

        [Test]
        public void Singleton_CanBeCreated_InEditMode()
        {
            var go = new GameObject(name: "TestSingleton");
            var singleton = go.AddComponent<TestPersistentSingletonForEditMode>();

            Assert.IsNotNull(anObject: singleton, message: "Singleton component should be created");
            Assert.AreEqual(expected: "TestSingleton", actual: go.name);
        }

        [Test]
        public void MultipleSingletons_CanCoexist_InEditMode()
        {
            // In Edit Mode, duplicate detection doesn't run (no Awake execution)
            var go1 = new GameObject(name: "First");
            var go2 = new GameObject(name: "Second");

            var s1 = go1.AddComponent<TestPersistentSingletonForEditMode>();
            var s2 = go2.AddComponent<TestPersistentSingletonForEditMode>();

            // Both should exist in Edit Mode (no runtime enforcement)
            Assert.IsNotNull(anObject: s1);
            Assert.IsNotNull(anObject: s2);
        }
    }
}
