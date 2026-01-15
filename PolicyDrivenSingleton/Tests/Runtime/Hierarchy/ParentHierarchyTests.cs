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

namespace PolicyDrivenSingleton.Tests.Runtime.Hierarchy
{
    [TestFixture]
    public class ParentHierarchyTests
    {
        [TearDown]
        public void TearDown()
        {
            var allObjects = Object.FindObjectsByType<TestPersistentSingleton>(
                findObjectsInactive: FindObjectsInactive.Include,
                sortMode: FindObjectsSortMode.None
            );
            foreach (var obj in allObjects)
            {
                Object.DestroyImmediate(obj: obj.gameObject);
            }

            var parents = Object.FindObjectsByType<GameObject>(sortMode: FindObjectsSortMode.None);
            foreach (var parent in parents)
            {
                if (parent.name.Contains(value: "Parent"))
                {
                    Object.DestroyImmediate(obj: parent);
                }
            }

            default(TestPersistentSingleton).ResetStaticCacheForTesting();
        }

        [UnityTest]
        public IEnumerator PersistentSingleton_WithParent_IsReparentedToRoot()
        {
            var start = TestLogCounter.Take();

            var parent = new GameObject(name: "ParentObject");
            var child = new GameObject(name: "TestPersistentSingleton");
            child.transform.SetParent(p: parent.transform);

            child.AddComponent<TestPersistentSingleton>();
            yield return null;

            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNull(anObject: instance.transform.parent, message: "Singleton should be reparented to root");

            var delta = TestLogCounter.Take().Delta(baseline: start);

#if TEST_IS_DEV
            Assert.AreEqual(expected: 1, actual: delta.Warning, message: "Reparenting should emit exactly one warning log (dev-only)");
            Assert.AreEqual(expected: 0, actual: delta.Exception, message: "No exceptions expected (dev-only)");
            Assert.AreEqual(expected: 0, actual: delta.Assert, message: "No asserts expected (dev-only)");
#endif
        }

        [UnityTest]
        public IEnumerator AutoCreatedSingleton_HasNoParent()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNull(anObject: instance.transform.parent, message: "Auto-created singleton should have no parent");
            Assert.AreEqual(expected: "TestPersistentSingleton", actual: instance.gameObject.name, message: "Auto-created singleton should have type name");
        }
    }
}
