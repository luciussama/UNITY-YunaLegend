using NUnit.Framework;
using YunaLegend.BearChase.Core;

namespace YunaLegend.Tests.EditMode.BearChase
{
    public sealed class BearChaseSimulationTests
    {
        [Test]
        public void EntranceSequence_FollowsExactLegacyCadence()
        {
            var sim = new BearChaseSimulation();
            Assert.AreEqual(BearChasePhase.EntranceWalk, sim.Phase);

            sim.Step(3.9f);
            Assert.AreEqual(BearChasePhase.EntranceWalk, sim.Phase);

            sim.Step(0.2f);
            Assert.AreEqual(BearChasePhase.EntranceNudge, sim.Phase);

            sim.Step(1.05f);
            Assert.AreEqual(BearChasePhase.BearWakeUp, sim.Phase);

            sim.Step(1.05f);
            Assert.AreEqual(BearChasePhase.TransitionToChase, sim.Phase);

            sim.Step(0.55f);
            Assert.AreEqual(BearChasePhase.Chasing, sim.Phase);
        }

        [Test]
        public void ObstacleCollision_DeductsHeartsAndTriggersFallenAfterThreeHits()
        {
            var obs = new[]
            {
                new BearChaseObstacleSpec(0, 5f, 0f, 0.5f, 0.5f, BearChaseObstacleKind.Stool),
                new BearChaseObstacleSpec(1, 14f, 0f, 0.5f, 0.5f, BearChaseObstacleKind.Bookcase),
                new BearChaseObstacleSpec(2, 23f, 0f, 0.5f, 0.5f, BearChaseObstacleKind.BlockTower)
            };

            var sim = new BearChaseSimulation(obs);
            sim.ResetFullStage(skipEntrance: true, useEasyMode: false);
            Assert.AreEqual(3, sim.Hearts);

            // Simular passos curtos ate bater nos 3 obstaculos em sequencia
            for (int i = 0; i < 400; i++)
            {
                sim.Step(0.02f);
                if (sim.Phase == BearChasePhase.Fallen)
                {
                    break;
                }
            }

            Assert.AreEqual(BearChasePhase.Fallen, sim.Phase);
            Assert.AreEqual(0, sim.Hearts);
            Assert.AreEqual(1, sim.FailureCount);
        }

        [Test]
        public void TwoFailures_OffersEasyModeWithSixHearts()
        {
            var obs = new[]
            {
                new BearChaseObstacleSpec(0, 4f, 0f, 0.8f, 0.8f, BearChaseObstacleKind.Bookcase),
                new BearChaseObstacleSpec(1, 12f, 0f, 0.8f, 0.8f, BearChaseObstacleKind.Bookcase),
                new BearChaseObstacleSpec(2, 20f, 0f, 0.8f, 0.8f, BearChaseObstacleKind.Bookcase)
            };

            var sim = new BearChaseSimulation(obs);
            for (int attempt = 0; attempt < 2; attempt++)
            {
                sim.RetryChase(activateEasyMode: false);
                for (int i = 0; i < 400; i++)
                {
                    sim.Step(0.02f);
                }
            }

            Assert.AreEqual(2, sim.FailureCount);
            Assert.IsTrue(sim.ShouldOfferEasyMode);

            sim.RetryChase(activateEasyMode: true);
            Assert.IsTrue(sim.EasyMode);
            Assert.AreEqual(6, sim.Hearts);
            Assert.AreEqual(6, sim.MaxHearts);
        }

        [Test]
        public void DodgingObstacles_ReachesVictoryAtTrackEnd()
        {
            var obs = new[]
            {
                new BearChaseObstacleSpec(0, 20f, 0f, 0.5f, 0.5f, BearChaseObstacleKind.Armchair)
            };

            var sim = new BearChaseSimulation(obs);
            sim.ResetFullStage(skipEntrance: true, useEasyMode: false);
            sim.SetLateralInput(1f);

            for (int i = 0; i < 4000; i++)
            {
                sim.Step(0.02f);
                if (sim.Phase == BearChasePhase.Victory)
                {
                    break;
                }
            }

            Assert.AreEqual(BearChasePhase.Victory, sim.Phase);
            Assert.AreEqual(3, sim.Hearts);
        }
    }
}
