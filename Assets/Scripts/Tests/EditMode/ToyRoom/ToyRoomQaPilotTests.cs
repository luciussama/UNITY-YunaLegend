using NUnit.Framework;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Tests
{
    public sealed class ToyRoomQaPilotTests
    {
        [Test]
        public void ComputeAction_WhenDialogueActive_AdvancesDialogueWithoutMoving()
        {
            ToyRoomQaDecision decision = ToyRoomQaPilot.ComputeAction(
                ToyRoomPhase.Intro,
                new ToyRoomPoint(0f, 0f),
                hasTarget: true,
                new ToyRoomPoint(3f, 4f),
                withinInteractRange: false,
                dialogueOrIntroActive: true);

            Assert.IsTrue(decision.ShouldAdvanceDialogue);
            Assert.IsFalse(decision.ShouldInteract);
            Assert.AreEqual(0f, decision.MoveX, 1e-4f);
            Assert.AreEqual(0f, decision.MoveZ, 1e-4f);
        }

        [Test]
        public void ComputeAction_WhenFarFromTarget_MovesNormalizedTowardTarget()
        {
            ToyRoomQaDecision decision = ToyRoomQaPilot.ComputeAction(
                ToyRoomPhase.Exploring,
                new ToyRoomPoint(0f, 0f),
                hasTarget: true,
                new ToyRoomPoint(3f, 4f),
                withinInteractRange: false,
                dialogueOrIntroActive: false);

            Assert.IsFalse(decision.ShouldInteract);
            Assert.AreEqual(0.6f, decision.MoveX, 1e-3f);
            Assert.AreEqual(0.8f, decision.MoveZ, 1e-3f);
        }

        [Test]
        public void ComputeAction_WhenWithinInteractRange_RequestsInteraction()
        {
            ToyRoomQaDecision decision = ToyRoomQaPilot.ComputeAction(
                ToyRoomPhase.SeekingSword,
                new ToyRoomPoint(2f, 2f),
                hasTarget: true,
                new ToyRoomPoint(2.2f, 2.1f),
                withinInteractRange: true,
                dialogueOrIntroActive: false);

            Assert.IsTrue(decision.ShouldInteract);
            Assert.AreEqual(0f, decision.MoveX, 1e-4f);
            Assert.AreEqual(0f, decision.MoveZ, 1e-4f);
        }
    }
}
