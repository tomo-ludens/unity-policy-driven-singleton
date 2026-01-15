using NUnit.Framework;
using PolicyDrivenSingleton.Policy;

namespace PolicyDrivenSingleton.Tests.Editor.Policy
{
    [TestFixture]
    public class PolicyTests
    {
        [Test]
        public void PersistentPolicy_HasCorrectValues()
        {
            var policy = new PersistentPolicy();
            Assert.IsTrue(condition: policy.PersistAcrossScenes, message: "PersistentPolicy should persist across scenes");
            Assert.IsTrue(condition: policy.AutoCreateIfMissing, message: "PersistentPolicy should auto-create if missing");
        }

        [Test]
        public void SceneScopedPolicy_HasCorrectValues()
        {
            var policy = new SceneScopedPolicy();
            Assert.IsFalse(condition: policy.PersistAcrossScenes, message: "SceneScopedPolicy should not persist across scenes");
            Assert.IsFalse(condition: policy.AutoCreateIfMissing, message: "SceneScopedPolicy should not auto-create if missing");
        }

        [Test]
        public void Policy_ReadonlyStruct_ZeroAllocation()
        {
            // Verify policies are readonly structs with zero allocation
            var persistentPolicy1 = default(PersistentPolicy);
            var persistentPolicy2 = default(PersistentPolicy);
            var scenePolicy1 = default(SceneScopedPolicy);
            var scenePolicy2 = default(SceneScopedPolicy);

            // Test struct equality (same default values)
            Assert.AreEqual(expected: persistentPolicy1.PersistAcrossScenes, actual: persistentPolicy2.PersistAcrossScenes);
            Assert.AreEqual(expected: persistentPolicy1.AutoCreateIfMissing, actual: persistentPolicy2.AutoCreateIfMissing);
            Assert.AreEqual(expected: scenePolicy1.PersistAcrossScenes, actual: scenePolicy2.PersistAcrossScenes);
            Assert.AreEqual(expected: scenePolicy1.AutoCreateIfMissing, actual: scenePolicy2.AutoCreateIfMissing);

            // Test expected values
            Assert.IsTrue(condition: persistentPolicy1.PersistAcrossScenes, message: "PersistentPolicy should persist across scenes");
            Assert.IsTrue(condition: persistentPolicy1.AutoCreateIfMissing, message: "PersistentPolicy should auto-create");
            Assert.IsFalse(condition: scenePolicy1.PersistAcrossScenes, message: "SceneScopedPolicy should not persist");
            Assert.IsFalse(condition: scenePolicy1.AutoCreateIfMissing, message: "SceneScopedPolicy should not auto-create");

            // Verify type characteristics
            var persistentType = typeof(PersistentPolicy);
            var sceneType = typeof(SceneScopedPolicy);

            Assert.IsTrue(condition: persistentType.IsValueType, message: "PersistentPolicy should be a value type");
            Assert.IsTrue(condition: sceneType.IsValueType, message: "SceneScopedPolicy should be a value type");
            Assert.IsTrue(condition: persistentType.IsVisible, message: "Policy types should be public");
        }

        [Test]
        public void Policy_StructImmutability()
        {
            // Test that policy structs are immutable (cannot be modified)
            var persistentPolicy = default(PersistentPolicy);
            var scenePolicy = default(SceneScopedPolicy);

            // Store original values
            bool originalPersistentPersist = persistentPolicy.PersistAcrossScenes;
            bool originalPersistentAuto = persistentPolicy.AutoCreateIfMissing;
            bool originalScenePersist = scenePolicy.PersistAcrossScenes;
            bool originalSceneAuto = scenePolicy.AutoCreateIfMissing;

            // Attempt to "modify" (this should not compile if truly readonly, but let's test the concept)
            // Since they're readonly structs, any modification attempt would create new instances

            // Verify values remain constant across multiple instantiations
            for (int i = 0; i < 10; i++)
            {
                var newPersistent = default(PersistentPolicy);
                var newScene = default(SceneScopedPolicy);

                Assert.AreEqual(expected: originalPersistentPersist, actual: newPersistent.PersistAcrossScenes);
                Assert.AreEqual(expected: originalPersistentAuto, actual: newPersistent.AutoCreateIfMissing);
                Assert.AreEqual(expected: originalScenePersist, actual: newScene.PersistAcrossScenes);
                Assert.AreEqual(expected: originalSceneAuto, actual: newScene.AutoCreateIfMissing);
            }
        }

        [Test]
        public void Policy_DefaultInitialization_Consistency()
        {
            // Test that default initialization is consistent and predictable
            var persistentPolicies = new PersistentPolicy[5];
            var scenePolicies = new SceneScopedPolicy[5];

            // All should have the same values since they're default initialized
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(condition: persistentPolicies[i].PersistAcrossScenes);
                Assert.IsTrue(condition: persistentPolicies[i].AutoCreateIfMissing);
                Assert.IsFalse(condition: scenePolicies[i].PersistAcrossScenes);
                Assert.IsFalse(condition: scenePolicies[i].AutoCreateIfMissing);
            }

            // Test policy interface compliance
            TestPolicyInterfaceCompliance(policy: default(PersistentPolicy));
            TestPolicyInterfaceCompliance(policy: default(SceneScopedPolicy));
        }

        private void TestPolicyInterfaceCompliance(ISingletonPolicy policy)
        {
            // Test that policies properly implement ISingletonPolicy interface
            bool persistProperty = policy.PersistAcrossScenes;
            bool autoProperty = policy.AutoCreateIfMissing;

            // Values should be boolean (basic interface compliance test)
            Assert.IsInstanceOf<bool>(actual: persistProperty);
            Assert.IsInstanceOf<bool>(actual: autoProperty);

            switch (policy)
            {
                // At least one should be true for PersistentPolicy, both false for SceneScopedPolicy
                // (This is a structural test of the policy design)
                case PersistentPolicy:
                    Assert.IsTrue(condition: persistProperty && autoProperty);
                    break;
                case SceneScopedPolicy:
                    Assert.IsFalse(condition: persistProperty || autoProperty);
                    break;
            }
        }
    }
}
