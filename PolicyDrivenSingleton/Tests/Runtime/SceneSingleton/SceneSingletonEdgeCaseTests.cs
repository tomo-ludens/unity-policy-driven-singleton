#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_ASSERTIONS
#define TEST_IS_DEV
#endif

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.SceneSingleton
{
    [TestFixture]
    public class SceneSingletonEdgeCaseTests
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
        public IEnumerator Instance_ThrowsInDev_WhenNotPlaced()
        {
#if TEST_IS_DEV
            Assert.Throws<InvalidOperationException>(
                code: () => { _ = TestSceneSingleton.Instance; },
                message: "Should throw when SceneSingleton is not placed (dev-only)"
            );
#else
            var instance = TestSceneSingleton.Instance;
            Assert.IsNull(anObject: instance, message: "Should return null when not placed (non-dev build)");
#endif
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneSingleton_DoesNotAutoCreate()
        {
            TestSceneSingleton.TryGetInstance(instance: out var before);
            yield return null;

            TestSceneSingleton.TryGetInstance(instance: out var after);

            Assert.IsNull(anObject: before, message: "Should not exist before");
            Assert.IsNull(anObject: after, message: "Should still not exist - no auto-creation");
        }
    }
}
