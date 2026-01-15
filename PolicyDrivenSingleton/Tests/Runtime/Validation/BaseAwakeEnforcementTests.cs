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
    public class BaseAwakeEnforcementTests
    {
        [TearDown]
        public void TearDown()
        {
            var allObjects = Object.FindObjectsByType<TestSingletonWithoutBaseAwake>(
                findObjectsInactive: FindObjectsInactive.Include,
                sortMode: FindObjectsSortMode.None
            );
            foreach (var obj in allObjects)
            {
                Object.DestroyImmediate(obj: obj.gameObject);
            }

            default(TestSingletonWithoutBaseAwake).ResetStaticCacheForTesting();
        }

        [UnityTest]
        public IEnumerator Singleton_LogsError_WhenBaseAwakeNotCalled_InDev()
        {
#if !TEST_IS_DEV
            Assert.Ignore(message: "base.Awake enforcement log verification is dev-only.");
            yield break;
#else
            var start = TestLogCounter.Take();

            using (new IgnoreFailingMessagesScope(enabled: true))
            {
                var go = new GameObject(name: "TestSingletonWithoutBaseAwake");
                var singleton = go.AddComponent<TestSingletonWithoutBaseAwake>();
                yield return null;

                Assert.IsTrue(condition: singleton.AwakeWasCalled, message: "Custom Awake should have been called");
            }

            var delta = TestLogCounter.Take().Delta(baseline: start);
            Assert.AreEqual(expected: 1, actual: delta.Error, message: "Missing base.Awake should emit exactly one error log (dev-only)");
            Assert.AreEqual(expected: 0, actual: delta.Exception, message: "No exceptions expected (dev-only)");
            Assert.AreEqual(expected: 0, actual: delta.Assert, message: "No asserts expected (dev-only)");
#endif
        }
    }
}
