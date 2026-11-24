using System.Linq;
using NUnit.Framework;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Tests
{
    public sealed class PicnicCoreTests
    {
        [Test]
        public void LevelCatalog_MatchesLegacyStructure()
        {
            var level = PicnicLevelData.Default;
            Assert.That(level.Sections.Count, Is.EqualTo(9));
            Assert.That(level.Wheels.Count, Is.EqualTo(4));
            Assert.That(level.StaticPlatforms.Count(p => p.Terrain == PicnicTerrain.Cloud), Is.EqualTo(13));
            Assert.That(level.Waters.Count, Is.EqualTo(5));
            Assert.That(level.Sections[level.Sections.Count - 1].Name, Is.EqualTo("A grande árvore"));
        }

        [Test]
        public void Narrative_Preserves9OpeningAnd14EndingCards()
        {
            Assert.That(PicnicNarrativeData.Opening.Length, Is.EqualTo(9));
            Assert.That(PicnicNarrativeData.Ending.Length, Is.EqualTo(14));
            Assert.That(PicnicNarrativeData.Opening.Sum(c => c.Duration), Is.EqualTo(50f));
            Assert.That(PicnicNarrativeData.Ending.Sum(c => c.Duration), Is.EqualTo(84f));
        }

        [Test]
        public void SensorySpeeds_AndPoseSelection_MatchLegacy()
        {
            Assert.That(PicnicLevelData.TerrainSpeed(PicnicTerrain.Grass), Is.EqualTo(100f));
            Assert.That(PicnicLevelData.TerrainSpeed(PicnicTerrain.Soil), Is.EqualTo(220f));
            Assert.That(PicnicLevelData.TerrainSpeed(PicnicTerrain.Gravel), Is.EqualTo(410f));

            var grassPose = PicnicContact.SelectGirlPose(true, 80f, 0f, PicnicTerrain.Grass, 0.9f, 1.0f, null);
            Assert.That(grassPose.SetName, Is.EqualTo("girl-discomfort-v1"));

            var gravelPose = PicnicContact.SelectGirlPose(true, 300f, 0f, PicnicTerrain.Gravel, 0.9f, 1.0f, null);
            Assert.That(gravelPose.SetName, Is.EqualTo("girl-pebbles-v2"));
        }

        [Test]
        public void IntroWalksDownStairsAndEntersRunning()
        {
            var sim = new PicnicSimulation();
            Assert.That(sim.Stage, Is.EqualTo(PicnicStage.Intro));
            for (int i = 0; i < 9; i++) sim.AdvanceStory();
            Assert.That(sim.OpeningDone, Is.True);
            for (int i = 0; i < 32; i++) sim.Step(0.1f, default);
            Assert.That(sim.Stage, Is.EqualTo(PicnicStage.Running));
            Assert.That(sim.PlayerX, Is.EqualTo(PicnicLevelData.HomeSteps[3].X).Within(1f));
        }

        [Test]
        public void QaPilot_SkipsStoryAndAdvancesInRunning()
        {
            var sim = new PicnicSimulation();
            var cmd = PicnicQaPilot.Read(sim);
            Assert.That(cmd.Skip, Is.True);
            sim.Step(0.05f, cmd);
            Assert.That(sim.Stage, Is.EqualTo(PicnicStage.Running));
            float startX = sim.PlayerX;
            for (int i = 0; i < 120; i++)
            {
                sim.Step(1f / 60f, PicnicQaPilot.Read(sim));
            }
            Assert.That(sim.PlayerX, Is.GreaterThan(startX + 150f));
        }
    }
}
