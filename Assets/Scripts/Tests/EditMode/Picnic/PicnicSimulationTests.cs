using System;
using System.Linq;
using NUnit.Framework;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Tests
{
    public sealed class PicnicSimulationTests
    {
        [Test]
        public void Catalog_HasNineSectionsFourWheelsAndThirteenClouds()
        {
            var stage = new PicnicStageSpec();
            Assert.AreEqual(9, stage.Sections.Count);
            Assert.AreEqual(16, stage.Supports.Count(s => s.Wheel >= 0));
            Assert.AreEqual(13, stage.Supports.Count(s => s.Terrain == PicnicTerrain.Cloud));
            Assert.AreEqual(stage.Supports.Count, stage.Supports.Select(s => s.Id).Distinct().Count());
            Assert.AreEqual(4, stage.Creatures.Count(c => c.Kind == PicnicCreatureKind.Worm));
            Assert.AreEqual(5, stage.Creatures.Count(c => c.Kind == PicnicCreatureKind.Bee));
        }

        [Test]
        public void Narrative_ConservesAllBeatsAndIndividualDurations()
        {
            Assert.AreEqual(9, PicnicNarrative.Opening.Beats.Length);
            Assert.AreEqual(50f, PicnicNarrative.Opening.Duration);
            Assert.AreEqual(14, PicnicNarrative.Ending.Beats.Length);
            Assert.AreEqual(84f, PicnicNarrative.Ending.Duration);
        }

        [Test]
        public void SensoryTerrain_ChangesSpeedAndPreservesJump()
        {
            Assert.AreEqual(1f, PicnicStageSpec.Speed(PicnicTerrain.Grass));
            Assert.AreEqual(4.1f, PicnicStageSpec.Speed(PicnicTerrain.Gravel));
            Assert.AreEqual(2.2f, PicnicStageSpec.Speed(PicnicTerrain.Soil));
            var sim = new PicnicSimulation();
            sim.SkipNarrative();
            sim.Step(1f / 120f, 0f, true);
            Assert.Greater(sim.VelocityY, 7f);
        }

        [Test]
        public void Hits_RespectImmunityAndNormalDefeat()
        {
            var sim = new PicnicSimulation();
            sim.SkipNarrative();
            sim.TakeHit();
            sim.TakeHit();
            Assert.AreEqual(2, sim.Hearts);
            sim.Step(2.1f);
            sim.TakeHit();
            sim.Step(2.1f);
            sim.TakeHit();
            Assert.AreEqual(PicnicPhase.Defeat, sim.Phase);
            sim.Retry();
            Assert.AreEqual(PicnicPhase.Playing, sim.Phase);
            Assert.AreEqual(3, sim.Hearts);
        }

        [Test]
        public void EasyMode_RescuesInsteadOfDefeat()
        {
            var sim = new PicnicSimulation(true);
            sim.SkipNarrative();
            sim.TakeHit();
            Assert.AreEqual(PicnicPhase.Rescuing, sim.Phase);
            sim.Step(1.6f);
            Assert.AreEqual(PicnicPhase.Playing, sim.Phase);
            Assert.AreEqual(3, sim.Hearts);
            Assert.IsTrue(sim.Grounded);
        }

        [Test]
        public void Caterpillar_NeedsTwoStompsAndWormCannotBeStomped()
        {
            var bug = new PicnicCreatureState(new PicnicCreatureSpec(PicnicCreatureKind.Caterpillar, 0f, 0f));
            Assert.IsTrue(bug.Stomp());
            Assert.AreEqual(1, bug.Health);
            Assert.IsTrue(bug.Active);
            bug.Stomp();
            Assert.IsFalse(bug.Active);
            var worm = new PicnicCreatureState(new PicnicCreatureSpec(PicnicCreatureKind.Worm, 0f, 0f));
            Assert.IsFalse(worm.Stomp());
        }

        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void QaPilot_CompletesContinuousCourseWithRealCommands(int hz)
        {
            var sim = new PicnicSimulation();
            sim.SkipNarrative();
            int panics = 0;
            sim.Event += e => { if (e.StartsWith("panic_started_")) panics++; };
            int frames = hz * 900;
            for (int i = 0; i < frames && sim.Phase == PicnicPhase.Playing; i++)
            {
                var input = PicnicQaPilot.Read(sim);
                sim.Step(1f / hz, input.Axis, input.Jump);
            }
            Assert.AreEqual(PicnicPhase.Ending, sim.Phase,
                $"x={sim.X:F3} y={sim.Y:F3} t={sim.Time:F2} support={sim.SupportIndex} hearts={sim.Hearts} falls={sim.Falls}");
            Assert.AreEqual(3, panics);
            Assert.Greater(sim.Jumps, 20);
            Assert.AreEqual(0, sim.Falls);
            sim.Step(84.1f);
            Assert.AreEqual(PicnicPhase.Complete, sim.Phase);
        }
    }
}
