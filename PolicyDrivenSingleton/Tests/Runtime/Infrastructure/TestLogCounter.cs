using System.Threading;
using UnityEngine;

namespace PolicyDrivenSingleton.Tests.Runtime.Infrastructure
{
    /// <summary>
    /// Cumulative counter for observing logs by type and count during test execution.
    /// Holds the cumulative value at scope start to verify increments via delta comparison.
    /// </summary>
    internal static class TestLogCounter
    {
        private static int _sLog;
        private static int _sWarning;
        private static int _sError;
        private static int _sAssert;
        private static int _sException;

        private static bool _sInstalled;

        public readonly struct Snapshot
        {
            public readonly int Log;
            public readonly int Warning;
            public readonly int Error;
            public readonly int Assert;
            public readonly int Exception;

            public Snapshot(int log, int warning, int error, int assert, int exception)
            {
                Log = log;
                Warning = warning;
                Error = error;
                Assert = assert;
                Exception = exception;
            }

            public Snapshot Delta(Snapshot baseline)
            {
                return new Snapshot(
                    log: Log - baseline.Log,
                    warning: Warning - baseline.Warning,
                    error: Error - baseline.Error,
                    assert: Assert - baseline.Assert,
                    exception: Exception - baseline.Exception
                );
            }
        }

        public static void Install()
        {
            if (_sInstalled)
            {
                return;
            }

            _sInstalled = true;
            Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
        }

        public static void Uninstall()
        {
            if (!_sInstalled)
            {
                return;
            }

            _sInstalled = false;
            Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
        }

        public static Snapshot Take()
        {
            return new Snapshot(
                log: Volatile.Read(location: ref _sLog),
                warning: Volatile.Read(location: ref _sWarning),
                error: Volatile.Read(location: ref _sError),
                assert: Volatile.Read(location: ref _sAssert),
                exception: Volatile.Read(location: ref _sException)
            );
        }

        private static void OnLogMessageReceivedThreaded(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    Interlocked.Increment(location: ref _sError);
                    return;
                case LogType.Assert:
                    Interlocked.Increment(location: ref _sAssert);
                    return;
                case LogType.Exception:
                    Interlocked.Increment(location: ref _sException);
                    return;
                case LogType.Warning:
                    Interlocked.Increment(location: ref _sWarning);
                    return;
                case LogType.Log:
                default:
                    Interlocked.Increment(location: ref _sLog);
                    return;
            }
        }
    }
}
