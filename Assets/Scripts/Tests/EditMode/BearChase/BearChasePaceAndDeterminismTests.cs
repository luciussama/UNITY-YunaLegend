using NUnit.Framework;
using YunaLegend.BearChase.Core;

namespace YunaLegend.Tests.EditMode.BearChase
{
    public sealed class BearChasePaceAndDeterminismTests
    {
        [Test]
        public void Simulation_MaintainsHighSpeedAndAcceleratesAcrossTrack()
        {
            var sim = new BearChaseSimulation(seed: 42019);
            sim.ResetFullStage(skipEntrance: true, useEasyMode: false);

            sim.Step(0.1f);
            float earlySpeed = sim.CurrentForwardSpeed;
            Assert.GreaterOrEqual(earlySpeed, 10.5f, "Corrida inicial ja deve transmitir alta velocidade.");

            sim.SetLateralInput(0f);
            for (int i = 0; i < 1200; i++)
            {
                sim.Step(1f / 60f);
                if (sim.Phase == BearChasePhase.Fallen)
                {
                    break;
                }
            }

            if (sim.Phase == BearChasePhase.Chasing)
            {
                Assert.Greater(sim.CurrentForwardSpeed, earlySpeed, "Ritmo deve acelerar com o progresso.");
            }
        }

        [Test]
        public void FixedStepSimulation_ProducesIdenticalResultsAt30_60_120Hz()
        {
            (float yunaZ30, float bearZ30, int hearts30, BearChasePhase phase30) = RunForDuration(1f / 30f, 30 * 10);
            (float yunaZ60, float bearZ60, int hearts60, BearChasePhase phase60) = RunForDuration(1f / 60f, 60 * 10);
            (float yunaZ120, float bearZ120, int hearts120, BearChasePhase phase120) = RunForDuration(1f / 120f, 120 * 10);

            Assert.AreEqual(yunaZ60, yunaZ30, 0.001f);
            Assert.AreEqual(yunaZ60, yunaZ120, 0.001f);
            Assert.AreEqual(bearZ60, bearZ30, 0.001f);
            Assert.AreEqual(bearZ60, bearZ120, 0.001f);
            Assert.AreEqual(hearts60, hearts30);
            Assert.AreEqual(hearts60, hearts120);
            Assert.AreEqual(phase60, phase30);
            Assert.AreEqual(phase60, phase120);
        }

        [Test]
        public void ResetFullStage_ResetsThrowSchedulerDeterministically()
        {
            var fresh = new BearChaseSimulation(seed: 7741);
            fresh.ResetFullStage(skipEntrance: true, useEasyMode: false);

            var reused = new BearChaseSimulation(seed: 7741);
            reused.ResetFullStage(skipEntrance: true, useEasyMode: false);
            for (int i = 0; i < 300; i++)
            {
                reused.SetLateralInput(0.5f);
                reused.Step(1f / 60f);
            }

            reused.ResetFullStage(skipEntrance: true, useEasyMode: false);

            for (int i = 0; i < 360; i++)
            {
                fresh.Step(1f / 60f);
                reused.Step(1f / 60f);
            }

            Assert.AreEqual(fresh.ActiveThrownToys.Count, reused.ActiveThrownToys.Count);
            for (int t = 0; t < fresh.ActiveThrownToys.Count; t++)
            {
                Assert.AreEqual(fresh.ActiveThrownToys[t].Kind, reused.ActiveThrownToys[t].Kind);
                Assert.AreEqual(fresh.ActiveThrownToys[t].TargetZ, reused.ActiveThrownToys[t].TargetZ, 0.0001f);
                Assert.AreEqual(fresh.ActiveThrownToys[t].TargetLaneX, reused.ActiveThrownToys[t].TargetLaneX, 0.0001f);
            }
        }

        private static (float, float, int, BearChasePhase) RunForDuration(float dt, int steps)
        {
            var sim = new BearChaseSimulation(seed: 5511);
            sim.ResetFullStage(skipEntrance: true, useEasyMode: false);
            for (int i = 0; i < steps; i++)
            {
                float elapsed = i * dt;
                float input = ((int)(elapsed / 0.75f)) % 2 == 0 ? 0.6f : -0.6f;
                sim.SetLateralInput(input);
                sim.Step(dt);
            }

            return (sim.YunaZ, sim.BearZ, sim.Hearts, sim.Phase);
        }
    }
}
