using UnityEngine;
using YunaLegend.Illustrated25D.Preview.Controllers;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Plataforma 10 (Fase 2 - A Fuga): Pista Elevada do Trem.
    /// Delega a montagem de sprite, colisor e apoio real para o instalador genérico IllustratedPlatformView.
    /// </summary>
    public sealed class TrainTrestle10Platform : MonoBehaviour
    {
        public const float FloorY = -9.1f;
        public const float LandingPixelY = 522f;
        public const float FeetPixelY = 807f;
        public const float CentrePixelX = 448f;
        public const float LipLeftPixelX = 22f;
        public const float LipRightPixelX = 881f;
        public const float ContentPixelWidth = 865f;
        public const float TargetHeight = 5.45f; // TopY = -3.65 m (+0.30 m acima do Castelo 09)
        public const float PixelsPerMetre = (FeetPixelY - LandingPixelY) / TargetHeight;
        public const float Height = TargetHeight;
        public const float TopY = FloorY + Height;
        public const float Depth = 2.0f;
        public const float FullWidth = ContentPixelWidth / PixelsPerMetre;
        public const float SupportWidth = (LipRightPixelX - LipLeftPixelX) / PixelsPerMetre;
        public const float SupportOffsetX = ((LipLeftPixelX + LipRightPixelX) * 0.5f - CentrePixelX) / PixelsPerMetre;
        // Castle09 MaxX (~59.955f) + Stage2OpeningGap (2.115f) = 62.07f minX
        public const float TargetX = 62.07f + SupportWidth * 0.5f - SupportOffsetX;

        private IllustratedPlatformView view;

        public BoxCollider Top => view != null ? view.Top : null;
        public SpriteRenderer Painting => view != null ? view.Painting : null;

        public static IllustratedPlatformSpec CreateSpec()
        {
            return new IllustratedPlatformSpec
            {
                index = 10,
                surfaceObjectName = "Topo_Real_Trem_10",
                paintingObjectName = "Trem_10_Pintura",
                floorY = FloorY,
                landingPixelY = LandingPixelY,
                feetPixelY = FeetPixelY,
                centrePixelX = CentrePixelX,
                lipLeftPixelX = LipLeftPixelX,
                lipRightPixelX = LipRightPixelX,
                contentPixelWidth = ContentPixelWidth,
                targetX = TargetX,
                targetHeight = TargetHeight,
                depth = Depth
            };
        }

        public void Install(Texture2D texture)
        {
            view = gameObject.AddComponent<IllustratedPlatformView>();
            view.Install(CreateSpec(), texture);
        }
    }
}
