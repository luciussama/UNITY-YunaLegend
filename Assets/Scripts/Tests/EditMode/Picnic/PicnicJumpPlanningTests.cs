using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Tests
{
    public sealed class PicnicJumpPlanningTests
    {
        [Test]
        public void Measurements_UseEveryPhysicalSupportAndActualEdges()
        {
            var stage = new PicnicStageSpec();
            var measurements = PicnicSupportMeasurement.Measure(stage, 0f);
            Assert.AreEqual(stage.Supports.Count, measurements.Count);
            for (int i = 0; i < measurements.Count; i++)
            {
                var measured = measurements[i];
                var support = stage.Supports[i];
                Assert.AreEqual(support.Id, measured.Id);
                Assert.AreEqual(support.Width, measured.Right - measured.Left, 0.0001f);
                Assert.AreEqual(support.Height(0f, measured.Left), measured.LeftHeight);
                Assert.AreEqual(support.Height(0f, measured.Right), measured.RightHeight);
            }
            Assert.AreEqual(1.3f, measurements[7].GapTo(measurements[8]), 0.0001f);
            // Home stairs overlap the entrance. A sorted-neighbour gap is not the route gap.
            Assert.AreEqual(1.1f, measurements[0].GapTo(measurements[1]), 0.0001f);
            Assert.Less(measurements[0].GapTo(measurements[132]), 0f);
        }

        [Test]
        public void Measurements_TrackMovingWheelPhaseInsteadOfFreezingTheGap()
        {
            var stage = new PicnicStageSpec();
            int wheel = stage.Supports.ToList().FindIndex(s => s.Id == "roda-0");
            float quarterTurn = (float)Math.PI / (2f * 0.38f);
            var first = PicnicSupportMeasurement.Measure(stage, 0f);
            var later = PicnicSupportMeasurement.Measure(stage, quarterTurn);
            Assert.AreEqual(1.5f, first[wheel].Center - later[wheel].Center, 0.0001f);
            Assert.AreEqual(1.15f, first[wheel].CenterHeight - later[wheel].CenterHeight, 0.0001f);
            Assert.AreEqual(1.5f, later[wheel].GapTo(later[19]) - first[wheel].GapTo(first[19]), 0.0001f);
        }

        [Test]
        public void Planning_DoesNotAdvanceOrMutateTheLiveSimulationOrEmitItsEvents()
        {
            var sim = new PicnicSimulation();
            sim.SkipNarrative();
            float x = sim.X;
            float y = sim.Y;
            int support = sim.SupportIndex;
            var creatureX = sim.Creatures.Select(c => c.X).ToArray();
            var creatureClocks = sim.Creatures.Select(c => c.Clock).ToArray();
            var clouds = (float[])sim.CloudTimers.Clone();
            int eventCount = 0;
            sim.Event += _ => eventCount++;
            PicnicQaPilot.Read(sim);
            Assert.AreEqual(x, sim.X);
            Assert.AreEqual(y, sim.Y);
            Assert.AreEqual(0f, sim.Time);
            Assert.AreEqual(3, sim.Hearts);
            Assert.AreEqual(support, sim.SupportIndex);
            Assert.AreEqual(PicnicPhase.Playing, sim.Phase);
            CollectionAssert.AreEqual(creatureX, sim.Creatures.Select(c => c.X).ToArray());
            CollectionAssert.AreEqual(creatureClocks, sim.Creatures.Select(c => c.Clock).ToArray());
            CollectionAssert.AreEqual(clouds, sim.CloudTimers);
            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void ReusedPrediction_CopiesMutableStateWithoutAliasingOrLeakingEvents()
        {
            var source = new PicnicSimulation();
            source.SkipNarrative();
            source.CloudTimers[19] = 0.5f;
            source.Step(0.2f, 1f);
            var copyMethod = typeof(PicnicSimulation).GetMethod("CopyForPrediction",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var resetMethod = typeof(PicnicSimulation).GetMethod("CopyPredictionFrom",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var reused = (PicnicSimulation)copyMethod.Invoke(source, null);
            int leakedEvents = 0;
            source.Event += _ => leakedEvents++;
            reused.Event += _ => leakedEvents++;
            for (int cycle = 0; cycle < 6; cycle++)
            {
                source.Step(0.05f, 1f);
                resetMethod.Invoke(reused, new object[] { source });
                var fresh = (PicnicSimulation)copyMethod.Invoke(source, null);
                Assert.AreNotSame(source.CloudTimers, reused.CloudTimers);
                Assert.AreNotSame(source.Creatures, reused.Creatures);
                for (int i = 0; i < source.Creatures.Length; i++)
                {
                    Assert.AreNotSame(source.Creatures[i], reused.Creatures[i]);
                }
                float sourceTime = source.Time;
                float sourceX = source.X;
                var cloudSnapshot = (float[])source.CloudTimers.Clone();
                var creatureClocks = source.Creatures.Select(c => c.Clock).ToArray();
                reused.Step(0.3f, 1f, true);
                fresh.Step(0.3f, 1f, true);
                Assert.AreEqual(fresh.X, reused.X);
                Assert.AreEqual(fresh.Y, reused.Y);
                Assert.AreEqual(fresh.VelocityY, reused.VelocityY);
                Assert.AreEqual(fresh.SupportIndex, reused.SupportIndex);
                Assert.AreEqual(fresh.Hearts, reused.Hearts);
                CollectionAssert.AreEqual(fresh.CloudTimers, reused.CloudTimers);
                for (int i = 0; i < fresh.Creatures.Length; i++)
                {
                    Assert.AreEqual(fresh.Creatures[i].X, reused.Creatures[i].X);
                    Assert.AreEqual(fresh.Creatures[i].Y, reused.Creatures[i].Y);
                    Assert.AreEqual(fresh.Creatures[i].Clock, reused.Creatures[i].Clock);
                    Assert.AreEqual(fresh.Creatures[i].Health, reused.Creatures[i].Health);
                    Assert.AreEqual(fresh.Creatures[i].Warning, reused.Creatures[i].Warning);
                }
                Assert.AreEqual(sourceTime, source.Time);
                Assert.AreEqual(sourceX, source.X);
                CollectionAssert.AreEqual(cloudSnapshot, source.CloudTimers);
                CollectionAssert.AreEqual(creatureClocks, source.Creatures.Select(c => c.Clock).ToArray());
            }
            Assert.AreEqual(0, leakedEvents);
        }

        [Test]
        public void Pilot_HasNoCommandsDuringTheOpening()
        {
            var sim = new PicnicSimulation();
            var command = PicnicQaPilot.Read(sim);
            Assert.AreEqual(0f, command.Axis);
            Assert.IsFalse(command.Jump);
        }
    }
}
