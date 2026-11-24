using System;

namespace YunaLegend.Picnic.Core
{
    public readonly struct PicnicIllustrationPoint
    {
        public float X { get; }
        public float Y { get; }

        public PicnicIllustrationPoint(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public readonly struct PicnicIllustrationRect
    {
        public float Left { get; }
        public float Top { get; }
        public float Width { get; }
        public float Height { get; }

        public PicnicIllustrationRect(float left, float top, float width, float height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
    }

    public readonly struct PicnicOpeningFrame
    {
        public PicnicIllustrationRect Painting { get; }
        public PicnicIllustrationRect Yuna { get; }
        public PicnicIllustrationRect Nanda { get; }
        public PicnicIllustrationPoint Sole { get; }

        public PicnicOpeningFrame(PicnicIllustrationRect painting, PicnicIllustrationRect yuna,
            PicnicIllustrationRect nanda, PicnicIllustrationPoint sole)
        {
            Painting = painting;
            Yuna = yuna;
            Nanda = nanda;
            Sole = sole;
        }
    }

    /// <summary>
    /// Opening composition in source-painting pixels, with a single uniform camera transform.
    /// Coordinates use the top-left origin of the 1652 x 952 illustration.
    /// </summary>
    public static class PicnicOpeningComposition
    {
        public const float PaintingWidth = 1652f;
        public const float PaintingHeight = 952f;
        public const float YunaCanvasSize = 512f;
        public const float YunaCanvasHeightInPainting = 360f;
        public const float SolePixelX = 248f;
        public const float SolePixelY = 510f;
        public const float StepPixelX = 450f;
        public const float StepPixelY = 541f;

        public static PicnicOpeningFrame Resolve(float width, float height, float narrativeTime)
        {
            ValidateDimension(width, nameof(width));
            ValidateDimension(height, nameof(height));
            float scale = Math.Max(width / PaintingWidth, height / PaintingHeight);
            float left = Math.Max(width - PaintingWidth * scale,
                Math.Min(0f, width * 0.5f - 540f * scale));
            float top = (height - PaintingHeight * scale) * 0.5f;
            var painting = new PicnicIllustrationRect(left, top,
                PaintingWidth * scale, PaintingHeight * scale);
            var sole = new PicnicIllustrationPoint(left + StepPixelX * scale, top + StepPixelY * scale);
            float yunaScale = YunaCanvasHeightInPainting / YunaCanvasSize * scale;
            var yuna = new PicnicIllustrationRect(sole.X - SolePixelX * yunaScale,
                sole.Y - SolePixelY * yunaScale, YunaCanvasSize * yunaScale, YunaCanvasSize * yunaScale);
            return new PicnicOpeningFrame(painting, yuna, PlaceNanda(left, top, scale, narrativeTime), sole);
        }

        private static PicnicIllustrationRect PlaceNanda(float left, float top, float scale, float time)
        {
            const float height = 210f;
            const float width = height * 512f / 592f;
            float bob = (float)Math.Sin(time * 3f) * 7f;
            return new PicnicIllustrationRect(left + (645f - width * 0.5f) * scale,
                top + (300f + bob - height * 0.5f) * scale, width * scale, height * scale);
        }

        private static void ValidateDimension(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }
}
