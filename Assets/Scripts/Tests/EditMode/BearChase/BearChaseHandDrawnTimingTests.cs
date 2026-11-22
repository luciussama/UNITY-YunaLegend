using NUnit.Framework;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Tests
{
    public class BearChaseHandDrawnTimingTests
    {
        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void ExposureCoversEveryDrawingRegardlessOfRenderRate(int renderRate)
        {
            var seen = new bool[BearChaseHandDrawnTiming.FrameCount];
            int previousFrame = 0;

            for (int tick = 0; tick < renderRate * 14; tick++)
            {
                int frame = BearChaseHandDrawnTiming.FrameAt(tick / (double)renderRate);
                Assert.That(frame - previousFrame, Is.InRange(0, 1));
                seen[frame] = true;
                previousFrame = frame;
            }

            Assert.That(seen, Is.All.True);
            Assert.That(BearChaseHandDrawnTiming.FrameAt(14d), Is.EqualTo(350));
            Assert.That(BearChaseHandDrawnTiming.ShotAt(350), Is.EqualTo(-1));
        }

        [Test]
        public void ExposureHoldsEachDrawingForFortyMilliseconds()
        {
            for (int frame = 0; frame < BearChaseHandDrawnTiming.FrameCount; frame++)
            {
                double start = frame / 25d;
                Assert.That(BearChaseHandDrawnTiming.FrameAt(start), Is.EqualTo(frame));
                Assert.That(BearChaseHandDrawnTiming.FrameAt(start + 0.039999d), Is.EqualTo(frame));
            }
        }

        [TestCase(0, 0)]
        [TestCase(99, 0)]
        [TestCase(100, 1)]
        [TestCase(174, 1)]
        [TestCase(175, 2)]
        [TestCase(259, 2)]
        [TestCase(260, 3)]
        [TestCase(349, 3)]
        [TestCase(350, -1)]
        public void ShotBoundariesMatchDirectorScript(int frame, int expectedShot)
        {
            Assert.That(BearChaseHandDrawnTiming.ShotAt(frame), Is.EqualTo(expectedShot));
        }
    }
}
