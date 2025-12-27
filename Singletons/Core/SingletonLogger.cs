using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Singletons.Core
{
    /// <summary>
    /// Conditional logger for singleton infrastructure.
    /// </summary>
    /// <remarks>
    /// <para><b>IMPORTANT:</b> <see cref="ConditionalAttribute"/> removes CALL SITES at compile time,
    /// not method bodies. Methods remain in assembly but all callers are stripped in release builds.
    /// This means argument evaluation (including string interpolation) is also eliminated.</para>
    /// <para><b>WARNING:</b> <see cref="ThrowInvalidOperation"/> is stripped in release builds.
    /// Code after the call site continues silently—callers must handle null/false returns.</para>
    /// </remarks>
    internal static class SingletonLogger
    {
        private const string EditorSymbol = "UNITY_EDITOR";
        private const string DevBuildSymbol = "DEVELOPMENT_BUILD";

        [Conditional(conditionString: EditorSymbol), Conditional(conditionString: DevBuildSymbol)]
        public static void LogWarning(string message)
        {
            Debug.LogWarning(message: message);
        }

        [Conditional(conditionString: EditorSymbol), Conditional(conditionString: DevBuildSymbol)]
        public static void LogWarning(string message, UnityEngine.Object context)
        {
            Debug.LogWarning(message: message, context: context);
        }

        [Conditional(conditionString: EditorSymbol), Conditional(conditionString: DevBuildSymbol)]
        public static void LogError(string message)
        {
            Debug.LogError(message: message);
        }

        [Conditional(conditionString: EditorSymbol), Conditional(conditionString: DevBuildSymbol)]
        public static void LogError(string message, UnityEngine.Object context)
        {
            Debug.LogError(message: message, context: context);
        }

        /// <summary>
        /// Throws in DEV/EDITOR only. In release builds, call site is removed—execution continues past it.
        /// </summary>
        [Conditional(conditionString: EditorSymbol), Conditional(conditionString: DevBuildSymbol)]
        public static void ThrowInvalidOperation(string message)
        {
            throw new InvalidOperationException(message: message);
        }
    }
}
