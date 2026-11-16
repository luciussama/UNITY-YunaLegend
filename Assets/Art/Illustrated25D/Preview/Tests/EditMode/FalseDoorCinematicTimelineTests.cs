using NUnit.Framework;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.Illustrated25D.Preview.Tests.EditMode
{
    public sealed class FalseDoorCinematicTimelineTests
    {
        [Test]
        public void Falling_UsesSeatedActorInAir_NotStandingRun()
        {
            var falling = FalseDoorCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.FalseDoorFalling, 0.35f);

            Assert.IsTrue(falling.UseFallSeatedActor);
            Assert.AreEqual(1f, falling.SeatedPoseAlpha, 0.001f);
            Assert.IsFalse(falling.OverrideYunaPose);
        }

        [Test]
        public void CutsceneStep0_LandsSeatedRisesSmoothlyAdjustsDressAndLooksAroundDuringDialogue()
        {
            var onFloor = FalseDoorCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0, 0.25f);
            var gettingUp = FalseDoorCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0, 1.25f);
            var standingUp = FalseDoorCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0, 1.85f);
            var adjustDress = FalseDoorCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0, 2.30f);
            var lookLeft = FalseDoorCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0, 3.20f);
            var lookRight = FalseDoorCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0, 5.00f);

            Assert.IsTrue(onFloor.UseFallSeatedActor);
            Assert.AreEqual(1f, onFloor.SeatedPoseAlpha, 0.001f);

            Assert.IsTrue(gettingUp.UseFallSeatedActor);
            Assert.Greater(gettingUp.GettingUpPoseAlpha, 0.5f);

            Assert.IsTrue(standingUp.UseFallSeatedActor);
            Assert.Greater(standingUp.AdjustDressAlpha, 0.4f);

            Assert.IsTrue(adjustDress.UseFallSeatedActor);
            Assert.Greater(adjustDress.AdjustDressAlpha, 0.9f);

            Assert.IsFalse(lookLeft.UseFallSeatedActor);
            Assert.IsTrue(lookLeft.OverrideYunaPose);
            Assert.Less(lookLeft.FacingDirection, 0f);
            Assert.Greater(lookRight.FacingDirection, 0f);
        }

        [Test]
        public void CutsceneStep1_WidensCameraToReturnSpanScale()
        {
            var settled = FalseDoorCinematicTimeline.Evaluate(StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep1, 1.60f);

            Assert.AreEqual(1.15f, settled.CameraSpanScale, 0.001f);
            Assert.IsTrue(settled.OverrideYunaPose);
        }
    }
}
