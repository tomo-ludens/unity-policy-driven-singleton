using Singletons.Core;
using Singletons.Policy;

namespace Singletons.Tests
{
    /// <summary>
    /// Test-only extension methods for singleton testing.
    /// These methods are only available in test assemblies and do not pollute production code.
    /// </summary>
    internal static class TestExtensions
    {
        /// <summary>
        /// Test-only: Resets static cache for this singleton type.
        /// </summary>
        public static void ResetStaticCacheForTesting<TSingleton, TPolicy>(this SingletonBehaviour<TSingleton, TPolicy> singleton)
            where TSingleton : SingletonBehaviour<TSingleton, TPolicy>
            where TPolicy : struct, ISingletonPolicy
        {
            // Use reflection to access private static fields
            var type = typeof(SingletonBehaviour<TSingleton, TPolicy>);

            // Reset _instance field
            var instanceField = type.GetField(name: "_instance", bindingAttr: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            instanceField?.SetValue(obj: null, value: null);

            // Reset _cachedPlaySessionId field
            var cachedIdField = type.GetField(name: "_cachedPlaySessionId", bindingAttr: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            cachedIdField?.SetValue(obj: null, value: -1);
        }
    }
}
