using NUnit.Framework;
using YunaLegend.BearChase.Core;

namespace YunaLegend.Tests.EditMode.BearChase
{
    public sealed class BearChaseEntranceTimelineTests
    {
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
    }
}
