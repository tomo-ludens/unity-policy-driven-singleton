using System;
using PolicyDrivenSingleton.Core;
using UnityEditor;

namespace PolicyDrivenSingleton.Editor
{
    /// <summary>
    /// Editor hooks that clear quitting flag at Play Mode boundaries.
    /// Prevents stale <c>IsQuitting</c> from poisoning subsequent Play sessions when Domain Reload is disabled.
    /// </summary>
    [InitializeOnLoad]
    internal static class SingletonEditorHooks
    {
        static SingletonEditorHooks()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode(EnterPlayModeOptions options) => SingletonRuntime.ClearQuittingFlag();

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.ExitingPlayMode:
                    SingletonRuntime.ClearQuittingFlag();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.ExitingEditMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(paramName: nameof(state), actualValue: state, message: null);
            }
        }
    }
}
