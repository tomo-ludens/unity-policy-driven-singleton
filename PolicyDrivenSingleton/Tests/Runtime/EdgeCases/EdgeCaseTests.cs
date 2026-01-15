using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.EdgeCases
{
    [TestFixture]
    public class EdgeCaseTests
    {
        [TearDown]
        public void TearDown()
        {
            if (TestSceneSingleton.TryGetInstance(instance: out var scene))
            {
                Object.DestroyImmediate(obj: scene.gameObject);
            }

            if (TestPersistentSingleton.TryGetInstance(instance: out var persistent))
            {
                Object.DestroyImmediate(obj: persistent.gameObject);
            }

            default(TestSceneSingleton).ResetStaticCacheForTesting();
            default(TestPersistentSingleton).ResetStaticCacheForTesting();
            TestExtensions.ResetQuittingFlagForTesting();
        }

        [UnityTest]
        public IEnumerator DestroyedInstance_IsProperlyCleanedUp_FromCache()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: instance);

            Object.DestroyImmediate(obj: instance.gameObject);

            bool exists = TestPersistentSingleton.TryGetInstance(instance: out var retrieved);
            Assert.IsFalse(condition: exists, message: "TryGetInstance should return false after destruction");
            Assert.IsNull(anObject: retrieved, message: "Retrieved instance should be null");
        }

        [UnityTest]
        public IEnumerator MultipleRapidAccesses_ReturnSameInstance()
        {
            var instances = new TestPersistentSingleton[10];

            for (int i = 0; i < 10; i++)
            {
                instances[i] = TestPersistentSingleton.Instance;
            }

            yield return null;

            for (int i = 1; i < 10; i++)
            {
                Assert.AreSame(expected: instances[0], actual: instances[i], message: $"Access {i} should return same instance");
            }

            Assert.AreEqual(
                expected: 1,
                actual: Object.FindObjectsByType<TestPersistentSingleton>(sortMode: FindObjectsSortMode.None).Length,
                message: "Only one instance should exist"
            );
        }

        [UnityTest]
        public IEnumerator SceneSingleton_AccessBeforePlacement_ThenPlacement_Works()
        {
            bool beforeResult = TestSceneSingleton.TryGetInstance(instance: out var before);
            Assert.IsFalse(condition: beforeResult, message: "Should not find instance before placement");
            Assert.IsNull(anObject: before);

            yield return null;

            var go = new GameObject(name: "TestSceneSingleton");
            var placed = go.AddComponent<TestSceneSingleton>();
            yield return null;

            bool afterResult = TestSceneSingleton.TryGetInstance(instance: out var after);
            Assert.IsTrue(condition: afterResult, message: "Should find instance after placement");
            Assert.AreSame(expected: placed, actual: after);
        }
    }
}
