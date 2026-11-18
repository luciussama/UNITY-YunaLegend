using NUnit.Framework;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Tests
{
    public class ToyRoomBeatPlayerTests
    {
        [Test]
        public void BeatPlayerAdvancesThroughInvestigationAndSignalsSwordReveal()
        {
            var player = new ToyRoomBeatPlayer();
            bool swordRevealed = false;
            bool completed = false;

            player.OnBeatEntered += beat =>
            {
                if (beat.RevealsSword)
                {
                    swordRevealed = true;
                }
            };
            player.OnSequenceCompleted += () => completed = true;

            player.Play(ToyRoomNarrativeScript.InvestigationBeats);
            Assert.IsTrue(player.IsPlaying);

            for (int i = 0; i < 40; i++)
            {
                player.Tick(1.0f);
            }

            Assert.IsTrue(swordRevealed);
            Assert.IsTrue(completed);
            Assert.IsFalse(player.IsPlaying);
        }

        [Test]
        public void SkipAllCompletesActiveSequenceImmediately()
        {
            var player = new ToyRoomBeatPlayer();
            bool completed = false;
            player.OnSequenceCompleted += () => completed = true;

            player.Play(ToyRoomNarrativeScript.IntroBeats);
            Assert.IsTrue(player.IsPlaying);

            player.SkipAll();
            Assert.IsFalse(player.IsPlaying);
            Assert.IsTrue(completed);
        }

        [Test]
        public void AdvanceCurrentBeatMovesExactlyOneBeatAndCompletesAtEnd()
        {
            var player = new ToyRoomBeatPlayer();
            int enteredCount = 0;
            bool completed = false;
            player.OnBeatEntered += _ => enteredCount++;
            player.OnSequenceCompleted += () => completed = true;

            player.Play(ToyRoomNarrativeScript.StrangeDialogueBeats);
            Assert.AreEqual(1, enteredCount);
            Assert.IsTrue(player.IsPlaying);

            player.AdvanceCurrentBeat();
            Assert.AreEqual(2, enteredCount);
            Assert.IsTrue(player.IsPlaying);
            Assert.IsFalse(completed);

            player.AdvanceCurrentBeat();
            Assert.IsFalse(player.IsPlaying);
            Assert.IsTrue(completed);
        }
    }
}
