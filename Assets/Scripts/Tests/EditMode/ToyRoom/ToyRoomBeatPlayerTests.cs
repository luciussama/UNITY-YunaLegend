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
    }
}
