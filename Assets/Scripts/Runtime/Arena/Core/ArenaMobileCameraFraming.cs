using System;

namespace YunaLegend.Arena.Core
{
    public readonly struct ArenaMobileCameraFrame
    {
        public float WorldWidth { get; }
        public float CenterX { get; }
        public float LookY { get; }
        public float Distance { get; }

        public ArenaMobileCameraFrame(float width, float centerX, float lookY, float distance)
        {
            WorldWidth = width;
            CenterX = centerX;
            LookY = lookY;
            Distance = distance;
        }
    }

    /// <summary>Fits both combatants while keeping the mobile close shot readable.</summary>
    public static class ArenaMobileCameraFraming
    {
        public static ArenaMobileCameraFrame Evaluate(float playerX, float playerY, float enemyX, float enemyY, float aspect, bool longAttack)
        {
            aspect = Math.Max(0.45f, aspect);
            bool portrait = aspect < 1f;
            float separation = Math.Abs(playerX - enemyX);
            float t = Clamp01((separation - 2.4f) / 9.4f);
            t = t * t * (3f - 2f * t);
            // Reserve at least 4.5m vertically in landscape: actors stay clear of the controls.
            float closeWidth = Math.Max(4.8f, aspect * 4.5f);
            float wideWidth = Math.Max(closeWidth, 17.6f);
            float centerLimit = Lerp(4.9f, 0.8f, t);
            float midpoint = (playerX + enemyX) * 0.5f;
            if (portrait)
            {
                // The train is wider than Yuna: favor its side slightly without losing player focus.
                midpoint += (enemyX - playerX) * 0.08f;
            }
            float centerX = Math.Max(-centerLimit, Math.Min(centerLimit, midpoint));
            float halfSpan = Math.Max(Math.Abs(playerX - centerX), Math.Abs(enemyX - centerX));
            // Portrait needs room for the train's measured ~3.97m width and front depth.
            float padding = portrait ? 4.2f : 3.4f;
            if (longAttack)
            {
                padding += 1f;
            }
            float width = Math.Max(Lerp(closeWidth, wideWidth, t), halfSpan * 2f + padding);
            float fov = aspect < 1f ? 44f : 34f;
            float distance = width * 0.5f / ((float)Math.Tan(fov * 0.5f * Math.PI / 180f) * aspect);
            float lookY = Math.Max(1.1f, (playerY + enemyY) * 0.35f + Lerp(1f, 1.36f, t));
            return new ArenaMobileCameraFrame(width, centerX, lookY, distance);
        }

        public static float SmoothingFactor(float deltaTime, float speed)
        {
            return 1f - (float)Math.Exp(-Math.Max(0f, deltaTime) * Math.Max(0f, speed));
        }

        private static float Clamp01(float value)
        {
            return Math.Max(0f, Math.Min(1f, value));
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
