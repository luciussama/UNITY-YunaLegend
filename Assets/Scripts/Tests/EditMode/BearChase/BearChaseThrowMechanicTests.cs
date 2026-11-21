using System.Collections.Generic;
using NUnit.Framework;
using YunaLegend.BearChase.Core;

namespace YunaLegend.Tests.EditMode.BearChase
{
    public sealed class BearChaseThrowMechanicTests
    {
        [Test]
        public void ChaseStartsWithZeroPrePlacedObstacles_AndBearActivelyThrowsToys()
        {
            var sim = new BearChaseSimulation(seed: 1337);
            sim.ResetFullStage(skipEntrance: true, useEasyMode: false);

            Assert.AreEqual(0, sim.ActiveThrownToys.Count, "Pista nao deve comecar com obstaculos estaticos pre-espalhados.");

            bool sawTelegraph = false;
            bool sawThrow = false;
            bool sawPositiveArc = false;
            bool sawLanding = false;

            sim.OnBearThrowTelegraphed += _ => sawTelegraph = true;
            sim.OnBearToyThrown += _ => sawThrow = true;
            sim.OnThrownToyLanded += _ => sawLanding = true;

            for (int i = 0; i < 420; i++)
            {
                sim.Step(1f / 60f);
                for (int t = 0; t < sim.ActiveThrownToys.Count; t++)
                {
                    if (sim.ActiveThrownToys[t].Phase == BearChaseThrownToyPhase.InFlight
                        && sim.ActiveThrownToys[t].HeightAboveGround > 1.2f)
                    {
                        sawPositiveArc = true;
                    }
                }

                if (sawTelegraph && sawThrow && sawPositiveArc && sawLanding)
                {
                    break;
                }
            }

            Assert.IsTrue(sawTelegraph, "Urso deve telegrafar o arremesso antes de soltar o brinquedo.");
            Assert.IsTrue(sawThrow, "Urso deve arremessar o brinquedo na direcao da menina.");
            Assert.IsTrue(sawPositiveArc, "Brinquedo arremessado deve descrever arco aereo parabolico.");
            Assert.IsTrue(sawLanding, "Brinquedo arremessado deve pousar no chao.");
        }

        [Test]
        public void ThrownToys_AlwaysTravelBackwardFromBearTowardYuna()
        {
            var sim = new BearChaseSimulation(seed: 2026);
            sim.ResetFullStage(skipEntrance: true, useEasyMode: false);

            var thrownCount = 0;
            sim.OnBearToyThrown += toy =>
            {
                thrownCount++;
                Assert.Less(toy.StartZ, sim.BearZ, "Origem do arremesso deve partir do urso.");
                Assert.Less(toy.TargetZ, toy.StartZ, "Brinquedo arremessado deve ir para tras em direcao a menina.");
            };

            for (int i = 0; i < 600; i++)
            {
                sim.SetLateralInput(-1f);
                sim.Step(1f / 60f);
                for (int t = 0; t < sim.ActiveThrownToys.Count; t++)
                {
                    Assert.Less(sim.ActiveThrownToys[t].CurrentZ, sim.BearZ, "Brinquedo nao deve aparecer a frente do urso.");
                }
            }

            Assert.Greater(thrownCount, 2);
        }

        [Test]
        public void ThrowScheduler_NeverBlocksAllThreeLanesSimultaneously()
        {
            var scheduler = new BearChaseThrowScheduler(seed: 98765);
            var track = BearChaseTrackSpec.Default;
            var tuning = BearChaseTuningSpec.Default;

            float yunaZ = 20f;
            float yunaLane = 0f;
            var spawned = new List<BearChaseThrownToyState>();

            for (int i = 0; i < 1800; i++)
            {
                float dt = 1f / 60f;
                yunaZ += 12f * dt;
                yunaLane = ((i / 35) % 3) switch
                {
                    0 => -1.85f,
                    1 => 0f,
                    _ => 1.85f
                };

                scheduler.Step(
                    dt,
                    yunaZ,
                    yunaLane,
                    12f,
                    yunaZ + 12f,
                    0f,
                    yunaZ / track.TrackLength,
                    tuning,
                    track,
                    null,
                    toy => spawned.Add(toy),
                    null);
            }

            Assert.Greater(spawned.Count, 12);

            for (int i = 0; i < spawned.Count; i++)
            {
                var lanesInWindow = new HashSet<int>();
                for (int j = 0; j < spawned.Count; j++)
                {
                    if (System.Math.Abs(spawned[j].TargetZ - spawned[i].TargetZ) <= 5.5f)
                    {
                        lanesInWindow.Add(System.Math.Sign(spawned[j].TargetLaneX));
                    }
                }

                Assert.Less(lanesInWindow.Count, 3, $"Todas as 3 faixas bloqueadas perto de z={spawned[i].TargetZ:F1}");
            }
        }
    }
}
