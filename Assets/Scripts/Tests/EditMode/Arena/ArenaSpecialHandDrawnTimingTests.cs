using NUnit.Framework;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Tests
{
    public class ArenaSpecialHandDrawnTimingTests
    {
        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void ExposureCoversEveryDrawingRegardlessOfRenderRate(int renderRate)
        {
            var seen = new bool[ArenaSpecialHandDrawnTiming.FrameCount];
            int previousFrame = 0;

            for (int tick = 0; tick < renderRate * 3; tick++)
            {
                int frame = ArenaSpecialHandDrawnTiming.FrameAt(tick / (double)renderRate);
                Assert.That(frame - previousFrame, Is.InRange(0, 1));
                seen[frame] = true;
                previousFrame = frame;
            }

            Assert.That(seen, Is.All.True);
            Assert.That(ArenaSpecialHandDrawnTiming.FrameAt(3d), Is.EqualTo(75));
            Assert.That(ArenaSpecialHandDrawnTiming.ShotAt(75), Is.EqualTo(-1));
        }

        [Test]
        public void ExposureHoldsEachDrawingForFortyMilliseconds()
        {
            for (int frame = 0; frame < ArenaSpecialHandDrawnTiming.FrameCount; frame++)
            {
                double start = frame / 25d;
                Assert.That(ArenaSpecialHandDrawnTiming.FrameAt(start), Is.EqualTo(frame));
                Assert.That(ArenaSpecialHandDrawnTiming.FrameAt(start + 0.039999d), Is.EqualTo(frame));
            }
        }

        [TestCase(0, 0)]
        [TestCase(19, 0)]
        [TestCase(20, 1)]
        [TestCase(49, 1)]
        [TestCase(50, 2)]
        [TestCase(74, 2)]
        [TestCase(75, -1)]
        public void ShotBoundariesMatchDirectorScript(int frame, int expectedShot)
        {
            Assert.That(ArenaSpecialHandDrawnTiming.ShotAt(frame), Is.EqualTo(expectedShot));
        }
    }
}
