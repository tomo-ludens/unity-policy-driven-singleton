#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_ASSERTIONS
#define TEST_IS_DEV
#endif

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.Policy
{
    [TestFixture]
    public class PolicyBehaviorTests
    {
        [TearDown]
        public void TearDown()
        {
            if (TestPersistentSingleton.TryGetInstance(instance: out var persistent))
            {
                Object.DestroyImmediate(obj: persistent.gameObject);
            }

            if (TestSceneSingleton.TryGetInstance(instance: out var scene))
            {
                Object.DestroyImmediate(obj: scene.gameObject);
            }

            default(TestPersistentSingleton).ResetStaticCacheForTesting();
            default(TestSceneSingleton).ResetStaticCacheForTesting();
        }

        [UnityTest]
        public IEnumerator PersistentPolicy_EnablesAutoCreation()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: instance, message: "PersistentPolicy should enable auto-creation");
            Assert.AreEqual(expected: "TestPersistentSingleton", actual: instance.gameObject.name, message: "Auto-created object should have correct name");
        }

        [UnityTest]
        public IEnumerator ScenePolicy_DisablesAutoCreation()
        {
#if TEST_IS_DEV
            Assert.Throws<InvalidOperationException>(
                code: () => { _ = TestSceneSingleton.Instance; },
                message: "ScenePolicy should throw exception when not placed (dev-only)"
            );
#else
            var instance = TestSceneSingleton.Instance;
            Assert.IsNull(anObject: instance, message: "ScenePolicy should return null when not placed (non-dev build)");
#endif
            yield return null;
        }

        [UnityTest]
        public IEnumerator PersistentSingleton_SurvivesDontDestroyOnLoad()
        {
            var instance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsTrue(
                condition: instance.gameObject.scene.name == "DontDestroyOnLoad" || instance.transform.parent == null,
                message: "Persistent singleton should be in DontDestroyOnLoad scene or root"
            );

            var sameInstance = TestPersistentSingleton.Instance;
            Assert.AreSame(expected: instance, actual: sameInstance, message: "Should return the same instance after DontDestroyOnLoad");
        }
    }
}
