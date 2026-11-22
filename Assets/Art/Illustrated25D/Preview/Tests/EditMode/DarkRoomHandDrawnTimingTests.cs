using NUnit.Framework;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.Illustrated25D.Tests
{
    public class DarkRoomHandDrawnTimingTests
    {
        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void ExposureCoversEveryDrawingRegardlessOfRenderRate(int renderRate)
        {
            var seen = new bool[DarkRoomHandDrawnTiming.FrameCount];
            int previousFrame = 0;

            for (int tick = 0; tick < renderRate * 12; tick++)
            {
                int frame = DarkRoomHandDrawnTiming.FrameAt(tick / (double)renderRate);
                Assert.That(frame - previousFrame, Is.InRange(0, 1));
                seen[frame] = true;
                previousFrame = frame;
            }

            Assert.That(seen, Is.All.True);
            Assert.That(DarkRoomHandDrawnTiming.FrameAt(12d), Is.EqualTo(300));
            Assert.That(DarkRoomHandDrawnTiming.ShotAt(300), Is.EqualTo(-1));
        }

        [Test]
        public void ExposureHoldsEachDrawingForFortyMilliseconds()
        {
            for (int frame = 0; frame < DarkRoomHandDrawnTiming.FrameCount; frame++)
            {
                double start = frame / 25d;
                Assert.That(DarkRoomHandDrawnTiming.FrameAt(start), Is.EqualTo(frame));
                Assert.That(DarkRoomHandDrawnTiming.FrameAt(start + 0.039999d), Is.EqualTo(frame));
            }
        }

        [TestCase(0, 0)]
        [TestCase(49, 0)]
        [TestCase(50, 1)]
        [TestCase(149, 1)]
        [TestCase(150, 2)]
        [TestCase(234, 2)]
        [TestCase(235, 3)]
        [TestCase(299, 3)]
        [TestCase(300, -1)]
        public void ShotBoundariesMatchDirectorScript(int frame, int expectedShot)
        {
            Assert.That(DarkRoomHandDrawnTiming.ShotAt(frame), Is.EqualTo(expectedShot));
        }

        [TestCase(49, "", "")]
        [TestCase(50, "Nanda", "Levanta, dorminhoca.")]
        [TestCase(84, "Nanda", "Levanta, dorminhoca.")]
        [TestCase(85, "Nanda", "Temos que brincar.")]
        [TestCase(116, "Nanda", "Temos que brincar.")]
        [TestCase(117, "Nanda", "Você dormiu demais.")]
        [TestCase(149, "Nanda", "Você dormiu demais.")]
        [TestCase(150, "", "")]
        public void CaptionsMatchDirectorExposureRanges(int frame, string speaker, string text)
        {
            DarkRoomHandDrawnTiming.CaptionAt(frame, out string actualSpeaker, out string actualText);
            Assert.That(actualSpeaker, Is.EqualTo(speaker));
            Assert.That(actualText, Is.EqualTo(text));
        }
    }
}
