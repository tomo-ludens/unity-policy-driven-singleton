namespace PolicyDrivenSingleton.Tests.Editor.Doubles
{
    // Test singleton classes for EditMode testing
    public sealed class TestPersistentSingletonForEditMode : GlobalSingleton<TestPersistentSingletonForEditMode>
    {
        protected override void Awake()
        {
            base.Awake();
        }
    }

    public sealed class TestSceneSingletonForEditMode : SceneSingleton<TestSceneSingletonForEditMode>
    {
        protected override void Awake()
        {
            base.Awake();
        }
    }

    /// <summary>
    /// Test singleton that deliberately does NOT call base.Awake() for testing error detection.
    /// </summary>
    public sealed class TestSingletonWithoutBaseAwake : GlobalSingleton<TestSingletonWithoutBaseAwake>
    {
        protected override void Awake()
        {
            // Deliberately NOT calling base.Awake() to test error detection
        }
    }

    /// <summary>
    /// Test singleton for parent reparenting tests.
    /// </summary>
    public sealed class TestSingletonWithParent : GlobalSingleton<TestSingletonWithParent>
    {
        protected override void Awake()
        {
            base.Awake();
        }
    }
}
