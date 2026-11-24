using System;
using NUnit.Framework;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Tests
{
    public sealed class PicnicOpeningCompositionTests
    {
        [TestCase(1280f, 720f)]
        [TestCase(1920f, 1080f)]
        [TestCase(696f, 730f)]
        [TestCase(320f, 730f)]
        public void SoleAndPaintedStepShareTheSameTransform(float width, float height)
        {
            var frame = PicnicOpeningComposition.Resolve(width, height, 0f);
            float scale = frame.Painting.Width / PicnicOpeningComposition.PaintingWidth;
            float soleX = frame.Yuna.Left + frame.Yuna.Width * PicnicOpeningComposition.SolePixelX / 512f;
            float soleY = frame.Yuna.Top + frame.Yuna.Height * PicnicOpeningComposition.SolePixelY / 512f;
            Assert.That(soleX, Is.EqualTo(frame.Painting.Left + 450f * scale).Within(0.001f));
            Assert.That(soleY, Is.EqualTo(frame.Painting.Top + 541f * scale).Within(0.001f));
            Assert.That(frame.Painting.Height / 952f, Is.EqualTo(scale).Within(0.00001f));
            Assert.That(frame.Yuna.Width, Is.EqualTo(frame.Yuna.Height));
            Assert.That(frame.Painting.Left, Is.LessThanOrEqualTo(0.001f));
            Assert.That(frame.Painting.Left + frame.Painting.Width, Is.GreaterThanOrEqualTo(width - 0.001f));
            Assert.That(frame.Painting.Top, Is.LessThanOrEqualTo(0.001f));
            Assert.That(frame.Painting.Top + frame.Painting.Height, Is.GreaterThanOrEqualTo(height - 0.001f));
        }

        [TestCase(1280f, 720f)]
        [TestCase(696f, 730f)]
        [TestCase(320f, 730f)]
        public void CameraKeepsOpaqueCharacterAndFairyInsideTheViewport(float width, float height)
        {
            var frame = PicnicOpeningComposition.Resolve(width, height, 0f);
            Assert.That(frame.Yuna.Left + frame.Yuna.Width * 108f / 512f, Is.GreaterThanOrEqualTo(0f));
            Assert.That(frame.Yuna.Left + frame.Yuna.Width * 350f / 512f, Is.LessThanOrEqualTo(width));
            Assert.That(frame.Yuna.Top + frame.Yuna.Height * 51f / 512f, Is.GreaterThanOrEqualTo(0f));
            Assert.That(frame.Sole.Y, Is.LessThan(height * 0.71f));
            Assert.That(frame.Nanda.Left, Is.GreaterThanOrEqualTo(0f));
            Assert.That(frame.Nanda.Left + frame.Nanda.Width, Is.LessThanOrEqualTo(width));
        }

        [Test]
        public void TimeMovesOnlyTheFlyingFairyAndNeverLiftsYunasSole()
        {
            var first = PicnicOpeningComposition.Resolve(696f, 730f, 0f);
            var later = PicnicOpeningComposition.Resolve(696f, 730f, 0.5f);
            Assert.That(later.Yuna.Left, Is.EqualTo(first.Yuna.Left));
            Assert.That(later.Yuna.Top, Is.EqualTo(first.Yuna.Top));
            Assert.That(later.Sole.Y, Is.EqualTo(first.Sole.Y));
            Assert.That(later.Nanda.Top, Is.Not.EqualTo(first.Nanda.Top));
        }

        [TestCase(0f, 720f)]
        [TestCase(1280f, -1f)]
        [TestCase(float.NaN, 720f)]
        public void InvalidViewportIsRejected(float width, float height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PicnicOpeningComposition.Resolve(width, height, 0f));
        }
    }
}
