using NUnit.Framework;
using YunaLegend.Illustrated25D.Preview.Controllers;

namespace YunaLegend.Illustrated25D.Tests
{
    public sealed class IllustratedPlatformSpecTests
    {
        [Test]
        public void TrainTrestle10Contract_ComputesExactTopYAndSupportSpan()
        {
            var spec = new IllustratedPlatformSpec
            {
                index = 10,
                floorY = -9.1f,
                landingPixelY = 522f,
                feetPixelY = 807f,
                centrePixelX = 448f,
                lipLeftPixelX = 22f,
                lipRightPixelX = 881f,
                contentPixelWidth = 865f,
                targetX = 70.22f,
                targetHeight = 5.45f,
                depth = 2.0f
            };

            Assert.AreEqual(-3.65f, spec.TopY, 0.0001f);
            Assert.AreEqual(285f / 5.45f, spec.PixelsPerMetre, 0.001f);
            Assert.Greater(spec.SupportWidth, 16.0f);
            Assert.Less(spec.SupportWidth, spec.FullWidth);
            Assert.AreEqual(3.5f / spec.PixelsPerMetre, spec.SupportOffsetX, 0.0001f);
        }
    }
}
