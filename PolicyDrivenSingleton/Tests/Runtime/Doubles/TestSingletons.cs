using System;

namespace PolicyDrivenSingleton.Tests.Runtime.Doubles
{
    public sealed class TestPersistentSingleton : GlobalSingleton<TestPersistentSingleton>
    {
        public int AwakeCount { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            AwakeCount++;
        }
    }

    public sealed class TestSceneSingleton : SceneSingleton<TestSceneSingleton>
    {
        public bool WasInitialized { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            WasInitialized = true;
        }
    }

    // For type mismatch tests - a base class that is NOT sealed
    public class TestBaseSingleton : GlobalSingleton<TestBaseSingleton>
    {
    }

    // Derived class that should be rejected
    public sealed class TestDerivedSingleton : TestBaseSingleton
    {
    }

    // For inactive instance tests
    public sealed class TestInactiveSingleton : GlobalSingleton<TestInactiveSingleton>
    {
    }

    public sealed class TestSoftResetSingleton : GlobalSingleton<TestSoftResetSingleton>
    {
        public int AwakeCalls { get; private set; }

        public int PlaySessionStartCalls { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            AwakeCalls++;
        }

        protected override void OnPlaySessionStart()
        {
            PlaySessionStartCalls++;
        }
    }

    public sealed class TestSingletonWithoutBaseAwake : GlobalSingleton<TestSingletonWithoutBaseAwake>
    {
        public bool AwakeWasCalled { get; private set; }

        protected override void Awake()
        {
            AwakeWasCalled = true;
        }
    }

    public sealed class GameManager : GlobalSingleton<GameManager>
    {
        public int PlayerScore { get; private set; }
        public string CurrentLevel { get; private set; }
        public bool IsGamePaused { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            PlayerScore = 0;
            CurrentLevel = "MainMenu";
            IsGamePaused = false;
        }

        public void AddScore(int points)
        {
            PlayerScore += points;
        }

        public void SetLevel(string levelName)
        {
            CurrentLevel = levelName;
        }

        public void PauseGame()
        {
            IsGamePaused = true;
        }

        public void ResumeGame()
        {
            IsGamePaused = false;
        }
    }

    public sealed class LevelController : SceneSingleton<LevelController>
    {
        public string LevelName { get; private set; }
        public int EnemyCount { get; private set; }
        public bool IsLevelComplete { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            LevelName = "DefaultLevel";
            EnemyCount = 5;
            IsLevelComplete = false;
        }

        public void SetLevelInfo(string levelName, int enemies)
        {
            LevelName = levelName;
            EnemyCount = enemies;
        }

        public void CompleteLevel()
        {
            IsLevelComplete = true;
        }
    }

    /// <summary>
    /// Test singleton that exposes TryPostToMainThread for testing the protected API.
    /// </summary>
    public sealed class TestSingletonWithMainThreadPost : GlobalSingleton<TestSingletonWithMainThreadPost>
    {
        public static bool PostToMainThread(Action action, string callerContext = null)
            => TryPostToMainThread(action: action, callerContext: callerContext);
    }
}
