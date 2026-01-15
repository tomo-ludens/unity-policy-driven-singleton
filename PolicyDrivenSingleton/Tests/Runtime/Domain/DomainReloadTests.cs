using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Core;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.Domain
{
    [TestFixture]
    public class DomainReloadTests
    {
        [TearDown]
        public void TearDown()
        {
            TestExtensions.ResetQuittingFlagForTesting();

            var softResetInstances = Object.FindObjectsByType<TestSoftResetSingleton>(
                findObjectsInactive: FindObjectsInactive.Include,
                sortMode: FindObjectsSortMode.None
            );
            foreach (var inst in softResetInstances)
            {
                Object.DestroyImmediate(obj: inst.gameObject);
            }

            var persistentInstances = Object.FindObjectsByType<TestPersistentSingleton>(
                findObjectsInactive: FindObjectsInactive.Include,
                sortMode: FindObjectsSortMode.None
            );
            foreach (var inst in persistentInstances)
            {
                Object.DestroyImmediate(obj: inst.gameObject);
            }

            default(TestSoftResetSingleton).ResetStaticCacheForTesting();
            default(TestPersistentSingleton).ResetStaticCacheForTesting();
        }

        [UnityTest]
        public IEnumerator StaticCache_IsInvalidated_WhenPlaySessionIdChanges()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: instance, message: "Instance should be created");

            TestExtensions.AdvancePlaySessionIdForTesting();

            var sameInstance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.AreSame(expected: instance, actual: sameInstance, message: "Should return same GameObject instance");
        }

        [UnityTest]
        public IEnumerator OnPlaySessionStart_IsCalledAgain_AfterPlaySessionBoundary()
        {
            var instance = TestSoftResetSingleton.Instance;
            yield return null;

            Assert.AreEqual(expected: 1, actual: instance.PlaySessionStartCalls, message: "OnPlaySessionStart should be called once initially");

            TestExtensions.AdvancePlaySessionIdForTesting();
            _ = TestSoftResetSingleton.Instance;
            yield return null;

            Assert.AreEqual(expected: 2, actual: instance.PlaySessionStartCalls, message: "OnPlaySessionStart should be called again after boundary");

            TestExtensions.AdvancePlaySessionIdForTesting();
            _ = TestSoftResetSingleton.Instance;
            yield return null;

            Assert.AreEqual(expected: 3, actual: instance.PlaySessionStartCalls, message: "OnPlaySessionStart should be called for each Play session");
        }

        [UnityTest]
        public IEnumerator AwakeCount_DoesNotIncrease_OnPlaySessionBoundary()
        {
            var instance = TestSoftResetSingleton.Instance;
            yield return null;

            Assert.AreEqual(expected: 1, actual: instance.AwakeCalls, message: "Awake should be called once on creation");

            for (int i = 0; i < 3; i++)
            {
                TestExtensions.AdvancePlaySessionIdForTesting();
                _ = TestSoftResetSingleton.Instance;
                yield return null;
            }

            Assert.AreEqual(expected: 1, actual: instance.AwakeCalls, message: "Awake should NOT be called again on Play session boundary");
        }

        [UnityTest]
        public IEnumerator TryGetInstance_WorksCorrectly_AcrossPlaySessionBoundary()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            bool resultBefore = TestPersistentSingleton.TryGetInstance(instance: out var retrieved1);
            Assert.IsTrue(condition: resultBefore, message: "Should find instance before boundary");
            Assert.AreSame(expected: instance, actual: retrieved1);

            TestExtensions.AdvancePlaySessionIdForTesting();

            bool resultAfter = TestPersistentSingleton.TryGetInstance(instance: out var retrieved2);
            Assert.IsTrue(condition: resultAfter, message: "Should still find instance after boundary");
            Assert.AreSame(expected: instance, actual: retrieved2, message: "Should return same instance");
        }

        [UnityTest]
        public IEnumerator Instance_ReturnsNull_WhenQuitting()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: instance, message: "Instance should exist before quitting");

            SingletonRuntime.NotifyQuitting();

            bool result = TestPersistentSingleton.TryGetInstance(instance: out var retrieved);
            Assert.IsFalse(condition: result, message: "TryGetInstance should return false when quitting");
            Assert.IsNull(anObject: retrieved, message: "Retrieved instance should be null when quitting");
        }

        [UnityTest]
        public IEnumerator NewInstance_IsNotCreated_WhenQuitting()
        {
            SingletonRuntime.NotifyQuitting();

            var instance = TestPersistentSingleton.Instance;

            Assert.IsNull(anObject: instance, message: "Should not create instance when quitting");

            var found = Object.FindAnyObjectByType<TestPersistentSingleton>();
            Assert.IsNull(anObject: found, message: "No instance should exist in scene");

            yield return null;
        }
    }
}
