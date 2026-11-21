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
        public void ThrownToyHits_TriggerStumbleThenFallenAndDefeatEvent()
        {
            var sim = new BearChaseSimulation(seed: 42019);
            sim.ResetFullStage(skipEntrance: true, useEasyMode: false);
            Assert.AreEqual(3, sim.Hearts);

            bool sawStumble = false;
            bool sawDefeat = false;
            int hitCount = 0;

            sim.OnPhaseChanged += p =>
            {
                if (p == BearChasePhase.Stumble)
                {
                    sawStumble = true;
                }
            };
            sim.OnObstacleHit += _ => hitCount++;
            sim.OnDefeat += () => sawDefeat = true;

            // Dirigir Yuna intencionalmente para a faixa do brinquedo mais proximo a frente para testar impacto real
            for (int i = 0; i < 1800; i++)
            {
                AimAtNearestUpcomingToy(sim);
                sim.Step(1f / 60f);
                if (sim.Phase == BearChasePhase.RetryModal)
                {
                    break;
                }
            }

            Assert.IsTrue(sawStumble, "Primeiro ou segundo golpe deve causar tropeço.");
            Assert.IsTrue(sawDefeat, "Terceiro golpe deve disparar evento OnDefeat.");
            Assert.AreEqual(3, hitCount);
            Assert.AreEqual(0, sim.Hearts);
            Assert.AreEqual(1, sim.FailureCount);
        }

        [Test]
        public void TwoFailures_OffersEasyModeWithSixHeartsAndClearsOfferAfterActivation()
        {
            var sim = new BearChaseSimulation(seed: 42019);
            bool easyOffered = false;
            sim.OnEasyModeOffered += () => easyOffered = true;

            for (int attempt = 0; attempt < 2; attempt++)
            {
                sim.RetryChase(activateEasyMode: false);
                for (int i = 0; i < 1800; i++)
                {
                    AimAtNearestUpcomingToy(sim);
                    sim.Step(1f / 60f);
                    if (sim.Phase == BearChasePhase.RetryModal)
                    {
                        break;
                    }
                }
            }

            Assert.AreEqual(2, sim.FailureCount);
            Assert.IsTrue(easyOffered);
            Assert.IsTrue(sim.ShouldOfferEasyMode);

            sim.RetryChase(activateEasyMode: true);
            Assert.IsTrue(sim.EasyMode);
            Assert.IsFalse(sim.ShouldOfferEasyMode);
            Assert.AreEqual(6, sim.Hearts);
            Assert.AreEqual(6, sim.MaxHearts);
        }

        [Test]
        public void DodgingThrownToys_ReachesVictoryAtTrackEnd()
        {
            var sim = new BearChaseSimulation(seed: 42019);
            sim.ResetFullStage(skipEntrance: true, useEasyMode: false);

            bool victoryFired = false;
            sim.OnVictory += () => victoryFired = true;

            for (int i = 0; i < 4000; i++)
            {
                SteerAwayFromNearestToy(sim);
                sim.Step(1f / 60f);
                if (sim.Phase == BearChasePhase.Victory)
                {
                    break;
                }
            }

            Assert.AreEqual(BearChasePhase.Victory, sim.Phase);
            Assert.IsTrue(victoryFired);
            Assert.AreEqual(3, sim.Hearts);
        }

        [Test]
        public void LateralInput_ClampsInsideTrackWidthAndIgnoresZeroDt()
        {
            var sim = new BearChaseSimulation();
            sim.ResetFullStage(skipEntrance: true, useEasyMode: false);
            float zBefore = sim.YunaZ;
            sim.Step(0f);
            sim.Step(-0.5f);
            Assert.AreEqual(zBefore, sim.YunaZ);

            sim.SetLateralInput(1f);
            for (int i = 0; i < 120; i++)
            {
                sim.Step(1f / 60f);
            }

            Assert.LessOrEqual(sim.YunaLaneOffsetX, sim.TrackSpec.LaneHalfWidth + 0.0001f);
        }

        [Test]
        public void Bear_AlwaysStaysAheadOfYuna_UntilFinalCatchupAtVictory()
        {
            var sim = new BearChaseSimulation(seed: 42019);
            sim.ResetFullStage(skipEntrance: true, useEasyMode: false);

            for (int i = 0; i < 4000; i++)
            {
                SteerAwayFromNearestToy(sim);
                sim.Step(1f / 60f);

                Assert.Greater(sim.BearZ, sim.YunaZ + 0.95f, $"Menina ultrapassou ou emparelhou com o urso em progress={sim.NormalizedProgress:F3}");
                if (sim.NormalizedProgress >= 0.10f && sim.NormalizedProgress <= 0.90f)
                {
                    Assert.GreaterOrEqual(sim.BearZ - sim.YunaZ, 8.0f, $"Urso aproximou demais antes do fim em progress={sim.NormalizedProgress:F3}");
                }

                if (sim.Phase == BearChasePhase.Victory)
                {
                    break;
                }
            }

            Assert.AreEqual(BearChasePhase.Victory, sim.Phase);
        }

        private static void AimAtNearestUpcomingToy(BearChaseSimulation sim)
        {
            float bestDist = float.MaxValue;
            float targetLane = 0f;
            var toys = sim.ActiveThrownToys;
            for (int i = 0; i < toys.Count; i++)
            {
                BearChaseThrownToyState t = toys[i];
                if (t.Phase == BearChaseThrownToyPhase.Deflected || t.Phase == BearChaseThrownToyPhase.Expired)
                {
                    continue;
                }

                float dz = t.CurrentZ - sim.YunaZ;
                if (dz >= -0.2f && dz < bestDist)
                {
                    bestDist = dz;
                    targetLane = t.Phase == BearChaseThrownToyPhase.InFlight ? t.TargetLaneX : t.CurrentLaneX;
                }
            }

            float diff = targetLane - sim.YunaLaneOffsetX;
            sim.SetLateralInput(System.Math.Abs(diff) < 0.05f ? 0f : System.Math.Sign(diff));
        }

        private static void SteerAwayFromNearestToy(BearChaseSimulation sim)
        {
            float[] lanes = { -1.85f, 0f, 1.85f };
            float bestSafeLane = 0f;
            float bestClearance = -1f;

            for (int l = 0; l < lanes.Length; l++)
            {
                float candidate = lanes[l];
                float minDz = float.MaxValue;
                var toys = sim.ActiveThrownToys;
                for (int i = 0; i < toys.Count; i++)
                {
                    BearChaseThrownToyState t = toys[i];
                    if (t.Phase == BearChaseThrownToyPhase.Deflected || t.Phase == BearChaseThrownToyPhase.Expired)
                    {
                        continue;
                    }

                    float lane = t.Phase == BearChaseThrownToyPhase.InFlight ? t.TargetLaneX : t.CurrentLaneX;
                    if (System.Math.Abs(lane - candidate) < 0.9f)
                    {
                        float dz = t.CurrentZ - sim.YunaZ;
                        if (dz >= -0.5f && dz < minDz)
                        {
                            minDz = dz;
                        }
                    }
                }

                if (minDz > bestClearance)
                {
                    bestClearance = minDz;
                    bestSafeLane = candidate;
                }
            }

            float diff = bestSafeLane - sim.YunaLaneOffsetX;
            sim.SetLateralInput(System.Math.Abs(diff) < 0.05f ? 0f : System.Math.Sign(diff));
        }
    }
}
