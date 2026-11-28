using System.Collections.Generic;
using NUnit.Framework;
using YunaLegend.DreamForest.Core;

namespace YunaLegend.DreamForest.Tests
{
    public sealed class DreamForestCoreTests
    {
        [Test]
        public void Opening_AdvancesThroughAllSevenBeatsIntoRunning()
        {
            var sim = new DreamForestSimulation(false);
            Assert.AreEqual(DreamForestStage.Opening, sim.Stage);
            Assert.AreEqual(DreamForestMusicScene.Silence, sim.ActiveMusicScene());

            for (int i = 0; i < 300 && sim.Stage == DreamForestStage.Opening; i++)
            {
                sim.Step(new DreamForestInput(0f, 0f, false, advance: true), 0.15f);
            }

            Assert.AreEqual(DreamForestStage.Running, sim.Stage);
            Assert.AreEqual(DreamForestMusicScene.Dream, sim.ActiveMusicScene());
            Assert.IsTrue(sim.TutorialActive);
        }

        [Test]
        public void LevelData_BuildsAllSixSectionsThornsMudLakesAndCovers()
        {
            var level = DreamForestLevelData.Default;
            Assert.AreEqual(6, level.Sections.Length);
            Assert.AreEqual(42, level.GlideThorns.Length);
            Assert.AreEqual(4, level.MudLakes.Length);
            Assert.AreEqual(15, level.StealthCovers.Length);
            Assert.IsTrue(level.ById.ContainsKey("rest-0"));
            Assert.IsTrue(level.ById.ContainsKey("rest-5"));
            Assert.IsTrue(level.ById.ContainsKey("dream-finish"));
        }

        [Test]
        public void GrassX_KeepsRootFixedAtBottomWhileTipSways()
        {
            var level = DreamForestLevelData.Default;
            DreamForestObjectSpec grass = null;
            foreach (var o in level.Objects)
            {
                if (o.Kind == DreamForestObjectKind.Grass)
                {
                    grass = o;
                    break;
                }
            }
            Assert.IsNotNull(grass);

            var frame = DreamForestLevelData.EvaluateObjectFrame(grass, 1.35f, new Dictionary<string, float?>());
            Assert.AreEqual(frame.X, DreamForestLevelData.GrassX(frame, frame.Y + frame.H), 0.001f);
            Assert.AreEqual(frame.X + frame.Sway, DreamForestLevelData.GrassX(frame, frame.Y), 0.001f);
        }

        [Test]
        public void Act1_JumpFromStartLeafAndClimbSwayingGrassBlade()
        {
            var sim = new DreamForestSimulation(false);
            for (int i = 0; i < 300 && sim.Stage == DreamForestStage.Opening; i++)
            {
                sim.Step(new DreamForestInput(0f, 0f, false, advance: true), 0.15f);
            }

            bool gripped = false;
            for (int i = 0; i < 240; i++)
            {
                var input = DreamForestQaPilot.ComputeInput(sim);
                var events = sim.Step(input, 1f / 60f);
                for (int e = 0; e < events.Count; e++)
                {
                    if (events[e].Kind == DreamForestEventKind.Grip) gripped = true;
                }
                if (gripped) break;
            }

            Assert.IsTrue(gripped, "Yuna must grab the swaying giant grass blade when jumping off rest-0.");
        }

        [Test]
        public void Glider_LaunchesFromBranchAndQaPilotSteersCleanly()
        {
            var sim = new DreamForestSimulation(false);
            sim.BeginGliderSectionForTesting();
            Assert.AreEqual(DreamForestStage.GlideApproach, sim.Stage);

            bool reachedGliding = false;
            for (int i = 0; i < 600; i++)
            {
                var input = DreamForestQaPilot.ComputeInput(sim);
                sim.Step(input, 1f / 60f);
                if (sim.Stage == DreamForestStage.Gliding) reachedGliding = true;
            }

            Assert.IsTrue(reachedGliding, "Glider must transition from GlideApproach -> GlideGust -> Gliding.");
            Assert.AreEqual(0, sim.Hits, "QA pilot steering along GlideRouteY must avoid thorns cleanly.");
        }

        [Test]
        public void Nightmare_RaycastSpotlightsOccludedByRockCoversAndMudLakes()
        {
            var sim = new DreamForestSimulation(false);
            sim.BeginNightmareSectionForTesting(skipIntro: true);
            Assert.AreEqual(DreamForestStage.Stealth, sim.Stage);
            Assert.AreEqual(DreamForestMusicScene.Nightmare, sim.ActiveMusicScene());

            var lights = new List<DreamForestSpotlight>(2);
            sim.EvaluateSpotlights(lights);
            Assert.AreEqual(2, lights.Count, "Giant cockroach must cast two spotlight eye beams.");

            // Check that raycast distance against rock cover #0 (at x=220) is strictly shorter than full range
            var light = lights[0];
            var rock0 = sim.Level.StealthCovers[0];
            float angToRock = (float)System.Math.Atan2((rock0.BaseY - rock0.H * 0.5f) - light.Y, rock0.X - light.X);
            float hitDist = sim.RaycastCoverDistance(light, angToRock);
            Assert.Less(hitDist, DreamForestLevelData.NightmareLightRange, "Rock cover polygon must occlude spotlight ray.");
        }

        [Test]
        public void Nightmare_MudSubmersionHidesPlayerFromSpotlights()
        {
            var sim = new DreamForestSimulation(false);
            sim.BeginNightmareSectionForTesting(skipIntro: true);
            var lake = sim.Level.MudLakes[0];
            sim.Nightmare.X = lake.From + 120f;
            sim.Nightmare.Y = DreamForestLevelData.MudWaterline(lake, sim.Nightmare.X) + 156f;
            sim.Nightmare.MudMode = DreamForestMudMode.Swim;
            sim.Nightmare.MudLakeId = lake.Id;
            sim.Nightmare.MudDepth = 1f;

            Assert.IsFalse(sim.IsNightmarePlayerIlluminated(detection: true),
                "Fully submerged Yuna under the mud waterline must be occluded from spotlights.");
        }

        [Test]
        public void MobileCameraFraming_AdaptsForPortraitAndLandscape()
        {
            var portrait = DreamForestMobileCameraFraming.Evaluate(10f, 0f, 300f, 390f / 844f, 1, false, false);
            var landscape = DreamForestMobileCameraFraming.Evaluate(10f, 0f, 300f, 844f / 390f, 1, false, false);
            Assert.Greater(portrait.OrthoSize, landscape.OrthoSize);
            Assert.Greater(portrait.CenterX, 0f);
        }
    }
}
