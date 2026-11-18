using UnityEngine;

namespace YunaLegend.Arena.Runtime
{
    /// <summary>
    /// Converte coordenadas canonicas da simulacao 60 Hz (x=160..1440, chao y=690)
    /// para metros 2.5D na cena Unity (centro em x=0, chao em y=0).
    /// </summary>
    public static class ArenaCoordinateMapper
    {
        public const float CenterSimX = 800f;
        public const float GroundSimY = 690f;
        public const float PixelsToMeters = 0.01f;

        public static Vector3 ToWorldPosition(float simX, float simY, float z = 0f)
        {
            float worldX = (simX - CenterSimX) * PixelsToMeters;
            float worldY = (GroundSimY - simY) * PixelsToMeters;
            return new Vector3(worldX, worldY, z);
        }
    }
}
