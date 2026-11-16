using NUnit.Framework;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.Illustrated25D.Preview.Tests.EditMode
{
    public sealed class OpeningCinematicTimelineTests
    {
        [Test]
        public void Quantize30Fps_LocksTo30HzSteps()
        {
            float a = OpeningCinematicTimeline.Quantize30Fps(0.101f);
            float b = OpeningCinematicTimeline.Quantize30Fps(0.132f);
            float c = OpeningCinematicTimeline.Quantize30Fps(0.135f);

            Assert.AreEqual(a, b, 0.0001f);
            Assert.Greater(c, b);
        }

        [Test]
        public void WakeAndPoetic_KeepYunaInBed_AndWake2OpensOnSecondShout()
        {
            var wake1 = OpeningCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.OpeningNandaWake1, 1.0f);
            var poetic0 = OpeningCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.OpeningPoeticIntro, 1.0f);
            var poetic1 = OpeningCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.OpeningPoeticIntro, 3.0f);
            var beforeShout = OpeningCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.OpeningNandaWake2, 0.1f);
            var afterShout = OpeningCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.OpeningNandaWake2, 2.8f);

            Assert.IsTrue(wake1.YunaInBed);
            Assert.IsTrue(poetic0.YunaInBed);
            Assert.AreEqual(0, poetic0.DialogueBeat);
            Assert.AreEqual(1, poetic1.DialogueBeat);
            Assert.AreEqual(1f, wake1.YunaBlink, 0.001f);
            Assert.AreEqual(1f, poetic0.YunaBlink, 0.001f);
            Assert.AreEqual(1f, beforeShout.YunaBlink, 0.001f);
            Assert.AreEqual(0f, afterShout.YunaBlink, 0.001f);
            Assert.Greater(afterShout.NandaLightStrength, 1f);
        }

        [Test]
        public void PreJumpCue_GetsYunaOutOfBed_AndHandsOffSmoothlyToGameplay()
        {
            var start = OpeningCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.PreJumpCue, 0.05f);
            var sitting = OpeningCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.PreJumpCue, 0.85f);
            var banner = OpeningCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.PreJumpCue, 1.70f);
            var settled = OpeningCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.PreJumpCue, 2.85f);

            Assert.Less(start.CameraSpanScale, 0.85f);
            Assert.IsTrue(start.YunaInBed);
            Assert.Greater(sitting.BedSitProgress, 0.9f);
            Assert.IsFalse(banner.YunaInBed);
            Assert.Greater(banner.ChapterBannerAlpha, 0.5f);
            Assert.AreEqual(1f, settled.CameraSpanScale, 0.001f);
            Assert.AreEqual(0f, settled.CameraOffsetX, 0.001f);
            Assert.AreEqual(0f, settled.ChapterBannerAlpha, 0.001f);
            Assert.IsFalse(settled.Active);
        }
    }
}
