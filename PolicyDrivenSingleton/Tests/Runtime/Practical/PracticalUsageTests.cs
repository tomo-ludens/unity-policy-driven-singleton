using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PolicyDrivenSingleton.Tests.Runtime.Doubles;
using Object = UnityEngine.Object;

namespace PolicyDrivenSingleton.Tests.Runtime.Practical
{
    [TestFixture]
    public class PracticalUsageTests
    {
        [TearDown]
        public void TearDown()
        {
            if (GameManager.TryGetInstance(instance: out var gm))
            {
                Object.DestroyImmediate(obj: gm.gameObject);
            }
            default(GameManager).ResetStaticCacheForTesting();

            if (LevelController.TryGetInstance(instance: out var lc))
            {
                Object.DestroyImmediate(obj: lc.gameObject);
            }
            default(LevelController).ResetStaticCacheForTesting();
        }

        [UnityTest]
        public IEnumerator GameManager_PersistsAcrossAccess()
        {
            var gm1 = GameManager.Instance;
            yield return null;

            Assert.IsNotNull(anObject: gm1, message: "GameManager should be created");
            Assert.AreEqual(expected: 0, actual: gm1.PlayerScore, message: "Initial score should be 0");
            Assert.AreEqual(expected: "MainMenu", actual: gm1.CurrentLevel, message: "Initial level should be MainMenu");

            gm1.AddScore(points: 100);
            gm1.SetLevel(levelName: "Level1");
            gm1.PauseGame();

            var gm2 = GameManager.Instance;
            yield return null;

            Assert.AreSame(expected: gm1, actual: gm2, message: "Should return same instance");
            Assert.AreEqual(expected: 100, actual: gm2.PlayerScore, message: "Score should persist");
            Assert.AreEqual(expected: "Level1", actual: gm2.CurrentLevel, message: "Level should persist");
            Assert.IsTrue(condition: gm2.IsGamePaused, message: "Pause state should persist");
        }

        [UnityTest]
        public IEnumerator LevelController_RequiresPlacement()
        {
            bool result = LevelController.TryGetInstance(instance: out var controller);
            yield return null;

            Assert.IsFalse(condition: result, message: "Should return false when not placed");
            Assert.IsNull(anObject: controller, message: "Controller should be null");

            var go = new GameObject(name: "LevelController");
            var placedController = go.AddComponent<LevelController>();
            yield return null;

            bool result2 = LevelController.TryGetInstance(instance: out var controller2);
            yield return null;

            Assert.IsTrue(condition: result2, message: "Should return true when placed");
            Assert.AreSame(expected: placedController, actual: controller2, message: "Should return placed instance");
            Assert.AreEqual(expected: "DefaultLevel", actual: controller2.LevelName, message: "Should be initialized");
        }

        [UnityTest]
        public IEnumerator Singleton_StateManagement_WorksCorrectly()
        {
            var gm = GameManager.Instance;
            yield return null;

            Assert.AreEqual(expected: 0, actual: gm.PlayerScore);
            Assert.AreEqual(expected: "MainMenu", actual: gm.CurrentLevel);
            Assert.IsFalse(condition: gm.IsGamePaused);

            gm.AddScore(points: 500);
            gm.SetLevel(levelName: "BossLevel");
            gm.PauseGame();

            Assert.AreEqual(expected: 500, actual: gm.PlayerScore);
            Assert.AreEqual(expected: "BossLevel", actual: gm.CurrentLevel);
            Assert.IsTrue(condition: gm.IsGamePaused);

            gm.AddScore(points: 1000);
            gm.SetLevel(levelName: "Victory");
            gm.ResumeGame();

            Assert.AreEqual(expected: 1500, actual: gm.PlayerScore);
            Assert.AreEqual(expected: "Victory", actual: gm.CurrentLevel);
            Assert.IsFalse(condition: gm.IsGamePaused);
        }

        [UnityTest]
        public IEnumerator SceneSingleton_LevelManagement_WorksCorrectly()
        {
            var go = new GameObject(name: "LevelController");
            var controller = go.AddComponent<LevelController>();
            yield return null;

            Assert.AreEqual(expected: "DefaultLevel", actual: controller.LevelName);
            Assert.AreEqual(expected: 5, actual: controller.EnemyCount);
            Assert.IsFalse(condition: controller.IsLevelComplete);

            controller.SetLevelInfo(levelName: "CastleLevel", enemies: 10);
            controller.SetLevelInfo(levelName: "CastleLevel", enemies: 0);
            controller.CompleteLevel();

            Assert.AreEqual(expected: "CastleLevel", actual: controller.LevelName);
            Assert.AreEqual(expected: 0, actual: controller.EnemyCount);
            Assert.IsTrue(condition: controller.IsLevelComplete);
        }

        [UnityTest]
        public IEnumerator Singleton_InitializationOrder_WorksCorrectly()
        {
            var gm = GameManager.Instance;
            yield return null;

            var go = new GameObject(name: "LevelController");
            var lc = go.AddComponent<LevelController>();
            yield return null;

            Assert.IsNotNull(anObject: gm);
            Assert.IsNotNull(anObject: lc);
            Assert.AreEqual(expected: 0, actual: gm.PlayerScore);
            Assert.AreEqual(expected: "DefaultLevel", actual: lc.LevelName);

            gm.AddScore(points: 100);
            lc.SetLevelInfo(levelName: "OrderedLevel", enemies: 3);

            Assert.AreEqual(expected: 100, actual: gm.PlayerScore);
            Assert.AreEqual(expected: "OrderedLevel", actual: lc.LevelName);
            Assert.AreEqual(expected: 3, actual: lc.EnemyCount);
        }

        [UnityTest]
        public IEnumerator Singleton_ResourceManagement_WorksCorrectly()
        {
            var gm = GameManager.Instance;
            yield return null;

            gm.AddScore(points: 999);
            gm.SetLevel(levelName: "FinalLevel");

            Object.DestroyImmediate(obj: gm.gameObject);
            yield return null;

            var gm2 = GameManager.Instance;
            yield return null;

            Assert.IsNotNull(anObject: gm2);
            Assert.AreNotSame(expected: gm, actual: gm2, message: "Should be different instance");
            Assert.AreEqual(expected: 0, actual: gm2.PlayerScore, message: "Should have fresh score");
            Assert.AreEqual(expected: "MainMenu", actual: gm2.CurrentLevel, message: "Should have fresh level");
        }
    }
}
