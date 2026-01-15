#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_ASSERTIONS
#define TEST_IS_DEV
#endif

using System;
using UnityEngine.TestTools;

namespace PolicyDrivenSingleton.Tests.Runtime.Infrastructure
{
#if TEST_IS_DEV
    /// <summary>
    /// Scope that locally suppresses expected Error/Exception logs from causing
    /// test failures due to "Unexpected log" detection.
    /// </summary>
    internal readonly struct IgnoreFailingMessagesScope : IDisposable
    {
        private readonly bool _previous;

        public IgnoreFailingMessagesScope(bool enabled)
        {
            _previous = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = enabled;
        }

        public void Dispose()
        {
            LogAssert.ignoreFailingMessages = _previous;
        }
    }
#endif
}
