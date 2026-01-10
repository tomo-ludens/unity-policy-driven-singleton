using System;
using System.Threading;
using UnityEngine;

namespace PolicyDrivenSingleton.Core
{
    /// <summary>
    /// Runtime state shared by singleton infrastructure.
    /// </summary>
    /// <remarks>
    /// <para><b>PlaySessionId:</b> Invalidates stale static caches when Domain Reload is disabled.</para>
    /// <para><b>Thread safety:</b> <c>volatile</c> fields ensure cross-thread visibility without locks.
    /// Main thread ID is captured at <c>SubsystemRegistration</c>; background threads must not touch Unity APIs.</para>
    /// </remarks>
    internal static class SingletonRuntime
    {
        private const int UninitializedMainThreadId = -1;

        // volatile: cross-thread visibility for best-effort SynchronizationContext capture.
        private static volatile SynchronizationContext _mainThreadSyncContext;
        private static volatile int _mainThreadId = UninitializedMainThreadId;

        private static int _lastBeginFrame = -1;

        public static int PlaySessionId { get; private set; }
        public static bool IsQuitting { get; private set; }

        private static bool IsMainThread
        {
            get
            {
                int main = _mainThreadId;
                return main != UninitializedMainThreadId && main == Thread.CurrentThread.ManagedThreadId;
            }
        }

        internal static void EnsureInitializedForCurrentPlaySession()
        {
            if (!Application.isPlaying) return;

            Application.quitting -= OnQuitting;
            Application.quitting += OnQuitting;

            TryCaptureMainThreadSyncContextIfSafe();
        }

        /// <summary>
        /// Validates caller is on main thread. Emits Error log on violation (contract for tests).
        /// </summary>
        /// <remarks>
        /// Fast path avoids Unity API calls when main thread ID is known (safe for background threads).
        /// Slow path may touch <c>Application.isPlaying</c> and catches <c>UnityException</c> if called from background.
        /// </remarks>
        internal static bool ValidateMainThread(string callerContext)
        {
            // Fast path: known main thread ID, no Unity API access.
            if (_mainThreadId != UninitializedMainThreadId)
            {
                if (IsMainThread) return true;
                LogMainThreadViolation(callerContext: callerContext, reason: null);
                return false;
            }

            // Slow path: attempt initialization, may touch Unity APIs.
            try
            {
                if (!Application.isPlaying) return true;

                EnsureInitializedForCurrentPlaySession();
                TryCaptureMainThreadIdIfSafe();

                if (_mainThreadId != UninitializedMainThreadId)
                {
                    if (IsMainThread) return true;
                    LogMainThreadViolation(callerContext: callerContext, reason: null);
                    return false;
                }

                LogMainThreadViolation(callerContext: callerContext, reason: "main thread id is not initialized yet");
                return false;
            }
            catch (UnityException)
            {
                LogMainThreadViolation(callerContext: callerContext, reason: "Unity API access from a background thread");
                return false;
            }
        }

        /// <summary>
        /// Posts action to main thread. Runs inline if already on main thread; posts via SyncContext otherwise.
        /// </summary>
        /// <returns><c>true</c> if posted/executed; <c>false</c> if SyncContext unavailable (fail-soft).</returns>
        internal static bool TryPostToMainThread(Action action, string callerContext = null)
        {
            if (action == null) return false;

            if (_mainThreadId != UninitializedMainThreadId && IsMainThread)
            {
                action();
                return true;
            }

            var ctx = _mainThreadSyncContext;
            if (ctx == null)
            {
                LogMainThreadViolation(
                    callerContext: callerContext ?? "TryPostToMainThread",
                    reason: "main thread SynchronizationContext is not available");
                return false;
            }

            ctx.Post(d: static state => ((Action)state)?.Invoke(), state: action);
            return true;
        }

        internal static void NotifyQuitting() => IsQuitting = true;

        /// <summary>
        /// Editor/Tests helper: clears the quitting flag to avoid poisoning subsequent operations when statics persist.
        /// </summary>
        internal static void ClearQuittingFlag() => IsQuitting = false;

        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            if (!Application.isPlaying) return;

            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            TryCaptureMainThreadSyncContextIfSafe();
            BeginNewPlaySession();
        }

        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void AfterAssembliesLoaded()
        {
            if (!Application.isPlaying) return;
            TryCaptureMainThreadSyncContextIfSafe();
        }

        private static void BeginNewPlaySession()
        {
            if (Time.frameCount == _lastBeginFrame) return;
            _lastBeginFrame = Time.frameCount;

            EnsureInitializedForCurrentPlaySession();
            unchecked { PlaySessionId++; }
            IsQuitting = false;
        }

        private static void OnQuitting() => NotifyQuitting();

        /// <summary>
        /// Heuristic: Unity main thread has non-null SyncContext; avoids capturing on background threads.
        /// </summary>
        private static void TryCaptureMainThreadIdIfSafe()
        {
            if (_mainThreadId != UninitializedMainThreadId) return;
            if (SynchronizationContext.Current == null) return;
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private static void TryCaptureMainThreadSyncContextIfSafe()
        {
            if (_mainThreadSyncContext != null) return;

            var ctx = SynchronizationContext.Current;
            if (ctx == null) return;
            if (_mainThreadId != UninitializedMainThreadId && !IsMainThread) return;

            Interlocked.CompareExchange(location1: ref _mainThreadSyncContext, value: ctx, comparand: null);
        }

        private static void LogMainThreadViolation(string callerContext, string reason)
        {
            int current = Thread.CurrentThread.ManagedThreadId;
            int main = _mainThreadId;

            if (string.IsNullOrEmpty(value: reason))
            {
                SingletonLogger.LogError(message: $"{callerContext} must be called from the main thread.\nCurrent thread: {current}, Main thread: {main}.");
                return;
            }

            SingletonLogger.LogError(message: $"{callerContext} must be called from the main thread ({reason}).\nCurrent thread: {current}, Main thread: {main}.");
        }
    }
}
