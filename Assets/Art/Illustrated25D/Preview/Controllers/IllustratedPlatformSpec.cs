using System;

namespace YunaLegend.Illustrated25D.Preview.Controllers
{
    /// <summary>
    /// Especificação pura (orientada a dados) de uma plataforma ilustrada 2.5D.
    /// Elimina a necessidade de criar classes C# separadas para cada nova plataforma.
    /// </summary>
    [Serializable]
    public struct IllustratedPlatformSpec
    {
        public int index;
        public string surfaceObjectName;
        public string paintingObjectName;
        public float floorY;
        public float landingPixelY;
        public float feetPixelY;
        public float centrePixelX;
        public float lipLeftPixelX;
        public float lipRightPixelX;
        public float contentPixelWidth;
        public float targetX;
        public float targetHeight;
        public float depth;

        public float PixelsPerMetre => (feetPixelY - landingPixelY) / targetHeight;
        public float TopY => floorY + targetHeight;
        public float FullWidth => contentPixelWidth / PixelsPerMetre;
        public float SupportWidth => (lipRightPixelX - lipLeftPixelX) / PixelsPerMetre;
        public float SupportOffsetX => ((lipLeftPixelX + lipRightPixelX) * 0.5f - centrePixelX) / PixelsPerMetre;
    }
}
