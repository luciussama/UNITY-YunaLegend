using System;
using NUnit.Framework;
using YunaLegend.DreamForest.Core;
using YunaLegend.DreamForestEntrance.Core;

namespace YunaLegend.DreamForestEntrance.Tests
{
    public sealed class DreamEntranceTests
    {
        [TestCase(1043,840,671,540)]
        [TestCase(390,844,540,1169)]
        [TestCase(844,390,1169,540)]
        [TestCase(1200,720,900,540)]
        public void RasterProfile_PreservesLegacyCanvasResolution(int width,int height,int expectedWidth,int expectedHeight)
        {
            var raster=new DreamEntranceRasterProfile(width,height);
            Assert.That(raster.Width,Is.EqualTo(expectedWidth));
            Assert.That(raster.Height,Is.EqualTo(expectedHeight));
        }

        [Test]
        public void SilenceBed_IsQuietDeterministicAndHasNoDcOffset()
        {
            var a=DreamEntranceSound.CreateSamples(48000);
            var b=DreamEntranceSound.CreateSamples(48000);
            Assert.That(a,Is.EqualTo(b));
            double sum=0;
            foreach(float sample in a)
            {
                Assert.That(sample,Is.InRange(-.05f,.05f));
                sum+=sample;
            }
            Assert.That(Math.Abs(sum/a.Length),Is.LessThan(.0001));
        }

        [Test]
        public void EntranceAudio_CrossfadesAtAuthoredDepartureTime()
        {
            Assert.That(DreamEntranceSound.MusicMix(5,7),Is.Zero);
            Assert.That(DreamEntranceSound.MusicMix(6,2.8f),Is.Zero);
            Assert.That(DreamEntranceSound.MusicMix(6,3.4f),Is.EqualTo(.5f).Within(.00001));
            Assert.That(DreamEntranceSound.MusicMix(6,4),Is.EqualTo(1));
        }

        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void AutomaticOpening_PreservesAllSevenBeatsAnd38Seconds(int hz)
        {
            var sim = new DreamForestSimulation();
            double time = 0;
            int seen = 1;
            int previous = 0;
            while (sim.Stage == DreamForestStage.Opening && time<40)
            {
                sim.Step(DreamEntranceQaPilot.ComputeInput(),1f/hz);
                time += 1.0/hz;
                if (sim.Opening != null && sim.Opening.Index != previous)
                {
                    previous = sim.Opening.Index;
                    seen++;
                }
            }
            Assert.That(seen,Is.EqualTo(7));
            Assert.That(time,Is.EqualTo(38).Within(8.0/hz));
            Assert.That(sim.Stage,Is.EqualTo(DreamForestStage.Running));
            Assert.That(sim.Player.JumpHeld,Is.True);
            Assert.That(sim.TutorialActive,Is.True);
        }

        [TestCase(2,455,217)]
        [TestCase(4,490,168)]
        public void KneelingHand_IsAnchoredToSleepingNandasShoulder(int beat, int handX, int handY)
        {
            var pose = DreamEntranceComposition.Evaluate(beat,1.2,10.2,1043,840,261);
            Assert.That(pose.Yuna.Flip,Is.True);
            Assert.That(pose.Yuna.PivotX,Is.EqualTo(handX));
            Assert.That(pose.Yuna.PivotY,Is.EqualTo(handY));
            double shoulderX = 720+96.5*pose.Nanda.Scale+Math.Sin(1.2*8)*3*pose.ActorFactor;
            double shoulderY = 920-160*pose.Nanda.Scale;
            Assert.That(pose.Yuna.X,Is.EqualTo(shoulderX).Within(.00001));
            Assert.That(pose.Yuna.Y,Is.EqualTo(shoulderY).Within(.00001));
        }

        [Test]
        public void Discover_UsesAuthoredLookInsteadOfMirroredOrTwistedStandingPose()
        {
            var pose = DreamEntranceComposition.Evaluate(1,3,6,1043,840,261);
            Assert.That(pose.Yuna.Asset,Is.EqualTo("Look0"));
            Assert.That(pose.Yuna.Flip,Is.False);
            Assert.That(pose.Yuna.Scale*980,Is.EqualTo(pose.ActorFactor*440));
            Assert.That(pose.Yuna.Y,Is.EqualTo(pose.Nanda.Y));
        }

        [TestCase(390,844)]
        [TestCase(844,390)]
        [TestCase(1200,720)]
        [TestCase(1043,840)]
        public void CameraCoverAndActorScale_AreFiniteAndUseOriginalAspectRatio(int width, int height)
        {
            for (int beat=0; beat<7; beat++)
            {
                var p = DreamEntranceComposition.Evaluate(beat,1,10,width,height,261);
                Assert.That(p.Scale*1536,Is.GreaterThanOrEqualTo(width));
                Assert.That(p.Scale*1024,Is.GreaterThanOrEqualTo(height));
                Assert.That(p.ActorFactor,Is.InRange(.25,height>width?.68:1));
                Assert.That(double.IsNaN(p.Yuna.X),Is.False);
            }
        }

        [Test]
        public void Departure_FadesOnlyInTheLast1Point2Seconds()
        {
            Assert.That(DreamEntranceComposition.Evaluate(6,2.8,36.8,1200,720,261).Fade,Is.Zero);
            Assert.That(DreamEntranceComposition.Evaluate(6,3.4,37.4,1200,720,261).Fade,Is.EqualTo(.5).Within(.00001));
            Assert.That(DreamEntranceComposition.Evaluate(6,4,38,1200,720,261).Fade,Is.EqualTo(1));
        }

        [Test]
        public void AdvanceDismissesOneDialogueAndPreservesActingMinimum()
        {
            var sim = new DreamForestSimulation();
            sim.Opening.Index = 2;
            sim.Opening.Age = .5f;
            Assert.That(sim.AdvanceDialogue(),Is.True);
            Assert.That(sim.AdvanceDialogue(),Is.False);
            sim.Step(default,.1f);
            Assert.That(sim.Opening.Index,Is.EqualTo(2));
            for (int i=0;i<30;i++) sim.Step(default,.1f);
            Assert.That(sim.Opening.Index,Is.EqualTo(3));
            Assert.That(sim.AdvanceDialogue(),Is.False);
        }

        [Test]
        public void Music_RemainsSilentUntilFinalDepartureFade()
        {
            var sim = new DreamForestSimulation();
            for (int beat=0;beat<7;beat++)
            {
                sim.Opening.Index = beat;
                sim.Opening.Age = 0;
                Assert.That(sim.ActiveMusicScene(),Is.EqualTo(DreamForestMusicScene.Silence));
            }
            sim.Opening.Age = 2.8f;
            Assert.That(sim.ActiveMusicScene(),Is.EqualTo(DreamForestMusicScene.Dream));
        }
    }
}
