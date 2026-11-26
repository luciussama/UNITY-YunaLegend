using NUnit.Framework;
using YunaLegend.Arena.Core;

namespace YunaLegend.Arena.Tests
{
    public sealed class ArenaMobileCameraFramingTests
    {
        [TestCase(390f / 844f)]
        [TestCase(844f / 390f)]
        public void CloseShotIsCloserThanPreviousMobileFloor(float aspect)
        {
            var frame = ArenaMobileCameraFraming.Evaluate(-0.8f, 0f, 0.8f, 0f, aspect, false);
            Assert.That(frame.WorldWidth, Is.LessThan(12.8f));
            Assert.That(frame.Distance, Is.LessThan(aspect < 1f ? 18f : 11.5f));
            Assert.That(frame.WorldWidth / aspect, Is.GreaterThanOrEqualTo(4.4999f));
        }

        [TestCase(390f / 844f)]
        [TestCase(844f / 390f)]
        public void PullsBackContinuouslyAsOpponentsSeparate(float aspect)
        {
            float previousWidth = 0f;
            for (int i = 0; i <= 1280; i++)
            {
                float separation = i * 0.01f;
                var frame = ArenaMobileCameraFraming.Evaluate(-separation * 0.5f, 0f, separation * 0.5f, 0f, aspect, false);
                Assert.That(frame.WorldWidth, Is.GreaterThanOrEqualTo(previousWidth));
                if (i > 0) Assert.That(frame.WorldWidth - previousWidth, Is.LessThan(0.025f));
                previousWidth = frame.WorldWidth;
            }
        }

        [TestCase(390f / 844f)]
        [TestCase(844f / 390f)]
        public void KeepsBothCombatantsInsideFrameEvenNearArenaEdge(float aspect)
        {
            for (float player = -6.4f; player <= 6.4f; player += 0.4f)
            {
                for (float enemy = -6.4f; enemy <= 6.4f; enemy += 0.4f)
                {
                    var frame = ArenaMobileCameraFraming.Evaluate(player, 0f, enemy, 0f, aspect, false);
                    Assert.That(System.Math.Abs(player - frame.CenterX) + 1.6f, Is.LessThanOrEqualTo(frame.WorldWidth * 0.5f));
                    Assert.That(System.Math.Abs(enemy - frame.CenterX) + 1.6f, Is.LessThanOrEqualTo(frame.WorldWidth * 0.5f));
                }
            }
        }

        [Test]
        public void PortraitReservesRoomForTheMeasuredLocomotiveWidth()
        {
            var frame = ArenaMobileCameraFraming.Evaluate(-1f, 0f, 1f, 0f, 390f / 844f, false);
            Assert.That(frame.CenterX, Is.GreaterThan(0f));
            Assert.That(frame.WorldWidth * 0.5f - (1f - frame.CenterX), Is.GreaterThan(2.3f));
            Assert.That(frame.WorldWidth, Is.LessThan(7f), "Keep the gain over the former12.8m floor");
        }

        [Test]
        public void LandscapeCloseShotKeepsApprovedScaleAndCenter()
        {
            var frame = ArenaMobileCameraFraming.Evaluate(-1f, 0f, 1f, 0f, 844f / 390f, false);
            Assert.That(frame.WorldWidth, Is.EqualTo((844f / 390f) * 4.5f).Within(0.0001f));
            Assert.That(frame.CenterX, Is.Zero);
        }

        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void ExponentialSmoothingHasSameOneSecondResponse(int hz)
        {
            float smoothed = 0f;
            for (int i = 0; i < hz; i++)
            {
                smoothed += (1f - smoothed) * ArenaMobileCameraFraming.SmoothingFactor(1f / hz, 5.8f);
            }
            Assert.That(smoothed, Is.EqualTo(1f - (float)System.Math.Exp(-5.8f)).Within(0.00001f));
        }
    }
}
