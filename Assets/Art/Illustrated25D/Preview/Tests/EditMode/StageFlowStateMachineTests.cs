using System.Collections.Generic;
using NUnit.Framework;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.Illustrated25D.Tests
{
    public sealed class StageFlowStateMachineTests
    {
        [Test]
        public void Opening_AutoAdvances_ThroughTimers_UntilPreJumpCue()
        {
            var sm = new StageFlowStateMachine();
            var transitions = new List<StageFlowStateMachine.FlowPhase>();
            sm.OnPhaseChanged += transitions.Add;

            Assert.AreEqual(StageFlowStateMachine.FlowPhase.OpeningNandaWake1, sm.Phase);
            Assert.IsTrue(sm.ShowIdlePose);

            sm.Tick(4.5f, confirmPressed: false, jumpOrStartPressed: false);
            Assert.AreEqual(StageFlowStateMachine.FlowPhase.OpeningPoeticIntro, sm.Phase);

            sm.Tick(4.5f, confirmPressed: false, jumpOrStartPressed: false);
            Assert.AreEqual(StageFlowStateMachine.FlowPhase.OpeningNandaWake2, sm.Phase);

            sm.Tick(3.5f, confirmPressed: false, jumpOrStartPressed: false);
            Assert.AreEqual(StageFlowStateMachine.FlowPhase.PreJumpCue, sm.Phase);

            // PreJumpCue waits indefinitely until player presses jump/start
            sm.Tick(10f, confirmPressed: false, jumpOrStartPressed: false);
            Assert.AreEqual(StageFlowStateMachine.FlowPhase.PreJumpCue, sm.Phase);

            sm.Tick(0.016f, confirmPressed: false, jumpOrStartPressed: true);
            Assert.AreEqual(StageFlowStateMachine.FlowPhase.Stage1Gameplay, sm.Phase);
            Assert.IsFalse(sm.ShowIdlePose);

            CollectionAssert.AreEqual(
                new[]
                {
                    StageFlowStateMachine.FlowPhase.OpeningPoeticIntro,
                    StageFlowStateMachine.FlowPhase.OpeningNandaWake2,
                    StageFlowStateMachine.FlowPhase.PreJumpCue,
                    StageFlowStateMachine.FlowPhase.Stage1Gameplay
                },
                transitions);
        }

        [Test]
        public void CastleCutscene_OnlyTriggersOnce_InStage1_AndSetsStage2Checkpoint()
        {
            var sm = new StageFlowStateMachine();
            sm.AdvanceStep();
            sm.AdvanceStep();
            sm.AdvanceStep();
            sm.AdvanceStep(); // -> Stage1Gameplay

            Assert.IsTrue(sm.TryTriggerCastleCutscene());
            Assert.AreEqual(StageFlowStateMachine.FlowPhase.CastleCutsceneStep0, sm.Phase);
            Assert.IsTrue(sm.ShowIdlePose);
            Assert.IsFalse(sm.TryTriggerCastleCutscene());

            sm.Tick(0.016f, confirmPressed: true, jumpOrStartPressed: false);
            Assert.AreEqual(StageFlowStateMachine.FlowPhase.CastleCutsceneStep1, sm.Phase);
            Assert.IsFalse(sm.Stage2CheckpointReached);

            sm.Tick(0.016f, confirmPressed: true, jumpOrStartPressed: false);
            Assert.AreEqual(StageFlowStateMachine.FlowPhase.Stage2Gameplay, sm.Phase);
            Assert.IsTrue(sm.Stage2CheckpointReached);
            Assert.IsFalse(sm.ShowIdlePose);
        }

        [Test]
        public void Stage2CheckpointRespawn_AndCompletion_WorkDeterministically()
        {
            var sm = new StageFlowStateMachine();
            sm.RespawnAtStage2Checkpoint();
            Assert.AreEqual(StageFlowStateMachine.FlowPhase.Stage2Gameplay, sm.Phase);
            Assert.IsTrue(sm.Stage2CheckpointReached);

            Assert.IsTrue(sm.TryCompleteStage2());
            Assert.AreEqual(StageFlowStateMachine.FlowPhase.Stage2Completed, sm.Phase);
            Assert.IsTrue(sm.ShowIdlePose);
            Assert.IsFalse(sm.TryCompleteStage2());

            sm.Reset();
            Assert.AreEqual(StageFlowStateMachine.FlowPhase.OpeningNandaWake1, sm.Phase);
            Assert.IsFalse(sm.Stage2CheckpointReached);
            Assert.IsFalse(sm.CastleCutsceneTriggered);
        }
    }
}
