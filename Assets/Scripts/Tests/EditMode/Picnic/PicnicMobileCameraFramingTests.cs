using System.Collections.Generic;
using NUnit.Framework;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Tests
{
    public sealed class PicnicMobileCameraFramingTests
    {
        [Test]
        public void LandscapePullbackIsModerateAndHasExplicitFloor()
        {
            var frame = PicnicMobileCameraFraming.Evaluate(20f, 0f, 100f, 844f / 390f, 1, default);
            Assert.That(frame.Distance, Is.EqualTo(16.2f).Within(0.0001f));
            Assert.That(frame.Distance / 13.5f, Is.InRange(1.15f, 1.25f));
        }

        [TestCase(390f / 844f)]
        [TestCase(0.45f)]
        public void PortraitShowsPlanningWidthWithoutUnlimitedZoom(float aspect)
        {
            var frame = PicnicMobileCameraFraming.Evaluate(20f, 0f, 100f, aspect, 1, default);
            Assert.That(frame.WorldWidth, Is.GreaterThanOrEqualTo(4.799f));
            Assert.That(frame.Distance, Is.InRange(16.2f, 22.5f));
        }

        [TestCase(1)]
        [TestCase(-1)]
        public void RelevantLandingAndPlayerStayInView(int facing)
        {
            float target = 20f + facing * 3.92f;
            var landing = new PicnicCameraLanding(target - 0.3f, target, 1f);
            if (facing < 0) landing = new PicnicCameraLanding(target, target + 0.3f, 1f);
            var frame = PicnicMobileCameraFraming.Evaluate(20f, 0f, 100f, 390f / 844f, facing, landing);
            float left = frame.CenterX - frame.WorldWidth * 0.5f;
            float right = frame.CenterX + frame.WorldWidth * 0.5f;
            Assert.That(left + 0.64f, Is.LessThanOrEqualTo(20f));
            Assert.That(right - 0.64f, Is.GreaterThanOrEqualTo(20f));
            Assert.That(landing.MinX, Is.GreaterThanOrEqualTo(left));
            Assert.That(landing.MaxX, Is.LessThanOrEqualTo(right));
            Assert.That(frame.Distance, Is.LessThanOrEqualTo(22.5f));
        }

        [Test]
        public void LandingSelectionIgnoresBehindFarAndUnreachableHighPlatforms()
        {
            var supports = new List<PicnicPlatformSpec>
            {
                new PicnicPlatformSpec("current", 900f, 180f, y: 590f, terrain: PicnicTerrain.Wood),
                new PicnicPlatformSpec("behind", 700f, 100f, y: 590f, terrain: PicnicTerrain.Wood),
                new PicnicPlatformSpec("too-high", 1100f, 100f, y: 200f, terrain: PicnicTerrain.Cloud),
                new PicnicPlatformSpec("far", 1600f, 150f, y: 590f, terrain: PicnicTerrain.Wood),
                new PicnicPlatformSpec("next", 1200f, 150f, y: 530f, terrain: PicnicTerrain.Wood),
            };
            var landing = PicnicMobileCameraFraming.FindNextLanding(supports, "current", 1000f, 590f, 1);
            Assert.That(landing.HasValue, Is.True);
            Assert.That(landing.MinX, Is.InRange(12f, 12.3f));
            Assert.That(landing.Y, Is.EqualTo(0.6f).Within(0.001f));
        }

        [Test]
        public void CameraStaysInsideLevelAtBothEnds()
        {
            foreach (float x in new[] { 0.3f, 99.7f })
            {
                var frame = PicnicMobileCameraFraming.Evaluate(x, 0f, 100f, 390f / 844f, 1, default);
                Assert.That(frame.CenterX - frame.WorldWidth * 0.5f, Is.GreaterThanOrEqualTo(-0.0001f));
                Assert.That(frame.CenterX + frame.WorldWidth * 0.5f, Is.LessThanOrEqualTo(100.0001f));
            }
        }

        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void FramingSmoothingHasSameOneSecondResponse(int hz)
        {
            float position = 0f;
            for (int i = 0; i < hz; i++) position += (1f - position) * PicnicMobileCameraFraming.SmoothingFactor(1f / hz);
            Assert.That(position, Is.EqualTo(1f - (float)System.Math.Exp(-5f)).Within(0.00001f));
        }
    }
}
