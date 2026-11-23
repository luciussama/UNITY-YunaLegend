using NUnit.Framework;
using YunaLegend.BearChase.Core;

namespace YunaLegend.Tests.EditMode.BearChase
{
    public sealed class BearChaseEntranceTimelineTests
    {
        [Test]
        public void EntrancePose_HoldsEachExposureForFortyMilliseconds()
        {
            BearChaseEntranceSample early = BearChaseEntranceTimeline.Evaluate(1.201f);
            BearChaseEntranceSample late = BearChaseEntranceTimeline.Evaluate(1.239f);
            BearChaseEntranceSample next = BearChaseEntranceTimeline.Evaluate(1.241f);

            Assert.AreEqual(early.NormalizedYunaX, late.NormalizedYunaX);
            Assert.AreEqual(early.YunaWalkBobY, late.YunaWalkBobY);
            Assert.AreEqual(early.GirlFrameIndex, late.GirlFrameIndex);
            Assert.Greater(next.NormalizedYunaX, late.NormalizedYunaX);
        }

        [Test]
        public void EntranceTimeline_ProgressesThroughAllFiveCanonicalBeats()
        {
            BearChaseEntranceSample establish = BearChaseEntranceTimeline.Evaluate(0.25f);
            Assert.AreEqual(BearChaseEntranceBeat.Establish, establish.Beat);
            Assert.IsFalse(establish.UseReachAtlas);
            Assert.AreEqual(0, establish.BearFrameIndex);
            Assert.AreEqual(0, establish.StepCueNumber);

            BearChaseEntranceSample approach = BearChaseEntranceTimeline.Evaluate(2.20f);
            Assert.AreEqual(BearChaseEntranceBeat.Approach, approach.Beat);
            Assert.Greater(approach.NormalizedYunaX, establish.NormalizedYunaX);
            Assert.Greater(approach.NormalizedFairyX, establish.NormalizedFairyX);
            Assert.Greater(approach.StepCueNumber, 0);

            BearChaseEntranceSample reach = BearChaseEntranceTimeline.Evaluate(4.65f);
            Assert.AreEqual(BearChaseEntranceBeat.Reach, reach.Beat);
            Assert.IsTrue(reach.UseReachAtlas);
            Assert.GreaterOrEqual(reach.GirlFrameIndex, 1);

            BearChaseEntranceSample wake = BearChaseEntranceTimeline.Evaluate(5.35f);
            Assert.AreEqual(BearChaseEntranceBeat.BearWakeUp, wake.Beat);
            Assert.IsTrue(wake.WakeCueActive);
            Assert.Greater(wake.BearFrameIndex, 0);
            Assert.AreEqual(3, wake.GirlFrameIndex);

            BearChaseEntranceSample fade = BearChaseEntranceTimeline.Evaluate(6.35f);
            Assert.AreEqual(BearChaseEntranceBeat.TransitionToChase, fade.Beat);
            Assert.Greater(fade.FadeAlpha, 0.5f);
        }

        [Test]
        public void EntranceSimulation_TransitionsSmoothlyToChasingAtTotalDuration()
        {
            var sim = new BearChaseSimulation();
            Assert.IsTrue(sim.IsEntrancePhase);
            Assert.AreEqual(BearChasePhase.EntranceWalk, sim.Phase);

            for (int i = 0; i < 395; i++)
            {
                sim.Step(1f / 60f);
            }

            Assert.AreEqual(BearChasePhase.Chasing, sim.Phase);
            Assert.IsFalse(sim.IsEntrancePhase);
            Assert.AreEqual(BearChaseBearAction.Running, sim.BearAction);
        }

        [Test]
        public void EntranceApproach_DrivesStepsMonotonicallyByDistanceWithoutSlides()
        {
            BearChaseEntranceSample establishEnd = BearChaseEntranceTimeline.Evaluate(BearChaseEntranceTimeline.EstablishEnd - 0.01f);
            BearChaseEntranceSample approachStart = BearChaseEntranceTimeline.Evaluate(BearChaseEntranceTimeline.EstablishEnd + 0.001f);
            Assert.AreEqual(establishEnd.GirlFrameIndex, approachStart.GirlFrameIndex, "Inicio da caminhada deve continuar na mesma pose de Establish.");

            float prevX = approachStart.NormalizedYunaX;
            for (float t = BearChaseEntranceTimeline.EstablishEnd; t <= BearChaseEntranceTimeline.ReachEnd; t += 0.05f)
            {
                BearChaseEntranceSample s = BearChaseEntranceTimeline.Evaluate(t);
                Assert.GreaterOrEqual(s.NormalizedYunaX, prevX - 0.0001f, $"Yuna nao deve recuar em t={t:F2}s.");
                Assert.Less(s.NormalizedFairyX, s.NormalizedYunaX - 0.03f, $"Nanda deve manter separacao visual limpa atras da Yuna em t={t:F2}s.");
                prevX = s.NormalizedYunaX;
            }

            // Pes plantados durante todo o alcance (zero deslizamento horizontal)
            BearChaseEntranceSample reachStart = BearChaseEntranceTimeline.Evaluate(BearChaseEntranceTimeline.ApproachEnd + 0.02f);
            BearChaseEntranceSample reachEnd = BearChaseEntranceTimeline.Evaluate(BearChaseEntranceTimeline.ReachEnd - 0.02f);
            Assert.AreEqual(reachStart.NormalizedYunaX, reachEnd.NormalizedYunaX, 0.0001f, "Pes de Yuna devem permanecer plantados durante o alcance.");

            // Urso dormindo espelhado com cabeca voltada a esquerda para continuidade anatomica no despertar
            Assert.IsTrue(reachEnd.BearFlipX, "Urso dormindo deve ter cabeca orientada para a esquerda (direcao de Yuna).");
            BearChaseEntranceSample wakeStart = BearChaseEntranceTimeline.Evaluate(BearChaseEntranceTimeline.ReachEnd + 0.02f);
            Assert.IsFalse(wakeStart.BearFlipX, "Urso desperto ja tem cabeca a esquerda sem inversao de 180 graus.");

            // Fade final atinge preto pleno (1.0) antes do corte
            BearChaseEntranceSample finalFrame = BearChaseEntranceTimeline.Evaluate(BearChaseEntranceTimeline.TotalDuration - 0.03f);
            Assert.AreEqual(1f, finalFrame.FadeAlpha, 0.001f, "Fade final deve atingir 100% preto antes da transicao.");
        }

        [Test]
        public void EntranceApproach_AdvancesPelvisSmoothlyWithoutDiscreteJumps()
        {
            const float exposureStep = 1f / BearChaseHandDrawnTiming.FramesPerSecond;
            float prevX = BearChaseEntranceTimeline.Evaluate(BearChaseEntranceTimeline.EstablishEnd).NormalizedYunaX;

            for (float t = BearChaseEntranceTimeline.EstablishEnd + exposureStep;
                 t <= BearChaseEntranceTimeline.ApproachEnd;
                 t += exposureStep)
            {
                BearChaseEntranceSample sample = BearChaseEntranceTimeline.Evaluate(t);
                float dx = sample.NormalizedYunaX - prevX;
                Assert.GreaterOrEqual(dx, -0.0001f, $"Sem recuo de quadril em t={t:F3}s.");
                Assert.LessOrEqual(dx, 0.006f, $"Sem salto brusco de quadril em t={t:F3}s (dx={dx:F4}).");
                prevX = sample.NormalizedYunaX;
            }

            BearChaseEntranceSample settle = BearChaseEntranceTimeline.Evaluate(BearChaseEntranceTimeline.WalkMoveEnd);
            Assert.AreEqual(BearChaseEntranceTimeline.YunaReachX, settle.NormalizedYunaX, 0.0005f);
            Assert.AreEqual(BearChaseEntranceTimeline.ApproachStandFrameIndex, settle.GirlFrameIndex, "Parada ao fim da aproximacao deve fechar na pose neutra sem pop.");

            var visitedFrames = new System.Collections.Generic.HashSet<int>();
            for (float t = BearChaseEntranceTimeline.EstablishEnd + exposureStep;
                 t < BearChaseEntranceTimeline.WalkMoveEnd;
                 t += exposureStep)
            {
                visitedFrames.Add(BearChaseEntranceTimeline.Evaluate(t).GirlFrameIndex);
            }

            Assert.AreEqual(BearChaseEntranceTimeline.ApproachWalkFrameCount, visitedFrames.Count, "Aproximacao deve percorrer todos os 8 quadros do novo ciclo de caminhada.");
        }
    }
}
