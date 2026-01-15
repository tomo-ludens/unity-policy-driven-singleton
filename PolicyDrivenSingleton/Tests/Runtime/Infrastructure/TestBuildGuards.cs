#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_ASSERTIONS
#define TEST_IS_DEV
#endif

namespace PolicyDrivenSingleton.Tests.Runtime.Infrastructure
{
    internal static class TestBuildGuards
    {
#if TEST_IS_DEV
        public const bool IsDev = true;
#else
        public const bool IsDev = false;
#endif
    }
}
