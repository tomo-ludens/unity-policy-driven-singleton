#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_ASSERTIONS
#define TEST_IS_DEV
#endif

using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using PolicyDrivenSingleton.Tests.Runtime.Infrastructure;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.Threading
{
    [TestFixture]
    public class ThreadSafetyTests
    {
        private const float ThreadTimeoutSeconds = 60f;

        [TearDown]
        public void TearDown()
        {
            if (TestPersistentSingleton.TryGetInstance(instance: out var instance))
            {
                Object.DestroyImmediate(obj: instance.gameObject);
            }

            default(TestPersistentSingleton).ResetStaticCacheForTesting();
        }

        private static IEnumerator WaitForThread(Thread thread, string context)
        {
            float start = Time.realtimeSinceStartup;

            while (thread.IsAlive)
            {
                if (Time.realtimeSinceStartup - start > ThreadTimeoutSeconds)
                {
                    Assert.Fail(message: $"Timed out waiting for background thread: {context}");
                }

                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator BackgroundThread_Instance_ReturnsNull_And_LogsOnceInDev()
        {
#if !TEST_IS_DEV
            Assert.Ignore(message: "Thread-safety log verification is dev-only.");
            yield break;
#else
            _ = TestPersistentSingleton.Instance;
            yield return null;

            var start = TestLogCounter.Take();
            TestPersistentSingleton backgroundResult = null;

            using (new IgnoreFailingMessagesScope(enabled: true))
            {
                var thread = new Thread(start: () =>
                {
                    backgroundResult = TestPersistentSingleton.Instance;
                });

                thread.Start();
                yield return WaitForThread(thread: thread, context: "BackgroundThread_Instance_ReturnsNull_And_LogsOnceInDev");
            }

            Assert.IsNull(anObject: backgroundResult, message: "Instance should return null from background thread");

            var delta = TestLogCounter.Take().Delta(baseline: start);
            Assert.AreEqual(expected: 1, actual: delta.Error, message: "Background thread Instance access should log exactly one error (dev-only)");
            Assert.AreEqual(expected: 0, actual: delta.Exception, message: "No exceptions expected (dev-only)");
            Assert.AreEqual(expected: 0, actual: delta.Assert, message: "No asserts expected (dev-only)");
#endif
        }

        [UnityTest]
        public IEnumerator BackgroundThread_TryGetInstance_ReturnsFalse_And_LogsOnceInDev()
        {
#if !TEST_IS_DEV
            Assert.Ignore(message: "Thread-safety log verification is dev-only.");
            yield break;
#else
            _ = TestPersistentSingleton.Instance;
            yield return null;

            var start = TestLogCounter.Take();

            bool tryGetResult = true;
            TestPersistentSingleton backgroundInstance = null;

            using (new IgnoreFailingMessagesScope(enabled: true))
            {
                var thread = new Thread(start: () =>
                {
                    tryGetResult = TestPersistentSingleton.TryGetInstance(instance: out backgroundInstance);
                });

                thread.Start();
                yield return WaitForThread(thread: thread, context: "BackgroundThread_TryGetInstance_ReturnsFalse_And_LogsOnceInDev");
            }

            Assert.IsFalse(condition: tryGetResult, message: "TryGetInstance should return false from background thread");
            Assert.IsNull(anObject: backgroundInstance, message: "Instance should be null from background thread");

            var delta = TestLogCounter.Take().Delta(baseline: start);
            Assert.AreEqual(expected: 1, actual: delta.Error, message: "Background thread TryGetInstance should log exactly one error (dev-only)");
            Assert.AreEqual(expected: 0, actual: delta.Exception, message: "No exceptions expected (dev-only)");
            Assert.AreEqual(expected: 0, actual: delta.Assert, message: "No asserts expected (dev-only)");
#endif
        }

        [UnityTest]
        public IEnumerator MainThread_Instance_AccessIsSafe()
        {
            Exception mainThreadException = null;
            TestPersistentSingleton instance = null;

            try
            {
                instance = TestPersistentSingleton.Instance;
            }
            catch (Exception ex)
            {
                mainThreadException = ex;
            }

            Assert.IsNull(anObject: mainThreadException, message: "No exception should be thrown on main thread");
            Assert.IsNotNull(anObject: instance, message: "Instance should be created successfully on main thread");
            Assert.IsInstanceOf<TestPersistentSingleton>(actual: instance, message: "Should return correct type");

            yield return null;
        }

        [UnityTest]
        public IEnumerator MainThread_TryGetInstance_AccessIsSafe()
        {
            var createdInstance = TestPersistentSingleton.Instance;
            yield return null;

            bool result = TestPersistentSingleton.TryGetInstance(instance: out var instance);

            Assert.IsTrue(condition: result, message: "TryGetInstance should return true after creation");
            Assert.IsNotNull(anObject: instance, message: "Instance should be retrieved successfully");
            Assert.AreSame(expected: createdInstance, actual: instance, message: "Should return the same instance");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ThreadSafety_Isolation_BetweenOperations()
        {
            var firstInstance = TestPersistentSingleton.Instance;
            yield return null;

            Assert.IsNotNull(anObject: firstInstance, message: "Should create instance on main thread");

            for (int i = 0; i < 5; i++)
            {
                var instance = TestPersistentSingleton.Instance;
                Assert.AreSame(expected: firstInstance, actual: instance, message: $"Instance call {i} should return same instance on main thread");
                yield return null;
            }

            bool tryResult = TestPersistentSingleton.TryGetInstance(instance: out var tryInstance);
            Assert.IsTrue(condition: tryResult, message: "TryGetInstance should succeed on main thread");
            Assert.AreSame(expected: firstInstance, actual: tryInstance, message: "TryGetInstance should return same instance");
        }

        [UnityTest]
        public IEnumerator ThreadSafety_ValidationLayer_PreventsBackgroundAccess_And_LogsOnceInDev()
        {
#if !TEST_IS_DEV
            Assert.Ignore(message: "Thread-safety log verification is dev-only.");
            yield break;
#else
            _ = TestPersistentSingleton.Instance;
            yield return null;

            var start = TestLogCounter.Take();
            TestPersistentSingleton backgroundResult = null;

            using (new IgnoreFailingMessagesScope(enabled: true))
            {
                var thread = new Thread(start: () =>
                {
                    backgroundResult = TestPersistentSingleton.Instance;
                });

                thread.Start();
                yield return WaitForThread(thread: thread, context: "ThreadSafety_ValidationLayer_PreventsBackgroundAccess_And_LogsOnceInDev");
            }

            Assert.IsNull(anObject: backgroundResult, message: "Background thread access should return null");

            var delta = TestLogCounter.Take().Delta(baseline: start);
            Assert.AreEqual(expected: 1, actual: delta.Error, message: "Validation layer should log exactly one error (dev-only)");
            Assert.AreEqual(expected: 0, actual: delta.Exception, message: "No exceptions expected (dev-only)");
            Assert.AreEqual(expected: 0, actual: delta.Assert, message: "No asserts expected (dev-only)");
#endif
        }

        [Test]
        public void ThreadSafety_MainThreadValidation_DoesNotInterfere()
        {
            Assert.DoesNotThrow(code: () =>
            {
                var instance = TestPersistentSingleton.Instance;
                Assert.IsNotNull(anObject: instance, message: "Instance creation should work on main thread");
            }, message: "Instance access should not throw on main thread");

            Assert.DoesNotThrow(code: () =>
            {
                bool result = TestPersistentSingleton.TryGetInstance(instance: out var instance);
                Assert.IsTrue(condition: result, message: "TryGetInstance should succeed");
                Assert.IsNotNull(anObject: instance, message: "Instance should be retrieved");
            }, message: "TryGetInstance should not throw on main thread");
        }
    }
}
