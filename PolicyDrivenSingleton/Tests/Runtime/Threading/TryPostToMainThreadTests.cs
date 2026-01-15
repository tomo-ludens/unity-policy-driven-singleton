using System.Collections;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.Threading
{
    [TestFixture]
    public class TryPostToMainThreadTests
    {
        private const float ThreadTimeoutSeconds = 60f;

        [TearDown]
        public void TearDown()
        {
            if (TestSingletonWithMainThreadPost.TryGetInstance(instance: out var instance))
            {
                Object.DestroyImmediate(obj: instance.gameObject);
            }

            default(TestSingletonWithMainThreadPost).ResetStaticCacheForTesting();
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
        public IEnumerator MainThread_ExecutesActionImmediately()
        {
            _ = TestSingletonWithMainThreadPost.Instance;
            yield return null;

            int executionCount = 0;
            bool result = TestSingletonWithMainThreadPost.PostToMainThread(action: () => executionCount++);

            Assert.IsTrue(condition: result, message: "Should return true on main thread");
            Assert.AreEqual(expected: 1, actual: executionCount, message: "Action should execute immediately on main thread");
        }

        [UnityTest]
        public IEnumerator MainThread_MultipleActionsExecuteInOrder()
        {
            _ = TestSingletonWithMainThreadPost.Instance;
            yield return null;

            var executionOrder = new System.Collections.Generic.List<int>();

            TestSingletonWithMainThreadPost.PostToMainThread(action: () => executionOrder.Add(item: 1));
            TestSingletonWithMainThreadPost.PostToMainThread(action: () => executionOrder.Add(item: 2));
            TestSingletonWithMainThreadPost.PostToMainThread(action: () => executionOrder.Add(item: 3));

            Assert.AreEqual(expected: 3, actual: executionOrder.Count, message: "All actions should execute");
            Assert.AreEqual(expected: 1, actual: executionOrder[index: 0]);
            Assert.AreEqual(expected: 2, actual: executionOrder[index: 1]);
            Assert.AreEqual(expected: 3, actual: executionOrder[index: 2]);
        }

        [UnityTest]
        public IEnumerator NullAction_ReturnsFalse()
        {
            _ = TestSingletonWithMainThreadPost.Instance;
            yield return null;

            bool result = TestSingletonWithMainThreadPost.PostToMainThread(action: null);

            Assert.IsFalse(condition: result, message: "Should return false for null action");
        }

        [UnityTest]
        public IEnumerator BackgroundThread_PostsToMainThread_WhenSyncContextAvailable()
        {
            _ = TestSingletonWithMainThreadPost.Instance;
            yield return null;

            int executedOnMainThread = 0;
            int mainThreadId = Thread.CurrentThread.ManagedThreadId;
            int actionThreadId = -1;
            bool postResult = false;

            var thread = new Thread(start: () =>
            {
                postResult = TestSingletonWithMainThreadPost.PostToMainThread(
                    action: () =>
                    {
                        actionThreadId = Thread.CurrentThread.ManagedThreadId;
                        executedOnMainThread++;
                    },
                    callerContext: "BackgroundThread_PostsToMainThread_Test");
            });

            thread.Start();
            yield return WaitForThread(thread: thread, context: "BackgroundThread_PostsToMainThread");

            // Wait for posted action to execute on main thread
            yield return null;
            yield return null;

            Assert.IsTrue(condition: postResult, message: "Post should succeed when SyncContext is available");
            Assert.AreEqual(expected: 1, actual: executedOnMainThread, message: "Action should execute once");
            Assert.AreEqual(expected: mainThreadId, actual: actionThreadId, message: "Action should execute on main thread");
        }

        [Test]
        public void MainThread_DoesNotThrow_WithValidAction()
        {
            _ = TestSingletonWithMainThreadPost.Instance;

            Assert.DoesNotThrow(code: () =>
            {
                bool result = TestSingletonWithMainThreadPost.PostToMainThread(action: () => { });
                Assert.IsTrue(condition: result);
            });
        }
    }
}
