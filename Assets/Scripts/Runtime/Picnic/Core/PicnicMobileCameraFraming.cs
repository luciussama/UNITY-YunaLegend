using System;
using System.Collections.Generic;

namespace YunaLegend.Picnic.Core
{
    public readonly struct PicnicCameraLanding
    {
        public bool HasValue { get; }
        public float MinX { get; }
        public float MaxX { get; }
        public float Y { get; }

        public PicnicCameraLanding(float minX, float maxX, float y)
        {
            HasValue = true;
            MinX = minX;
            MaxX = maxX;
            Y = y;
        }
    }

    public readonly struct PicnicMobileCameraFrame
    {
        public float CenterX { get; }
        public float TargetY { get; }
        public float Distance { get; }
        public float WorldWidth { get; }
        public float WorldHeight { get; }

        public PicnicMobileCameraFrame(float centerX, float targetY, float distance, float width, float height)
        {
            CenterX = centerX;
            TargetY = targetY;
            Distance = distance;
            WorldWidth = width;
            WorldHeight = height;
        }
    }

    /// <summary>Presentation only: previews a nearby reachable landing without changing jumps or supports.</summary>
    public static class PicnicMobileCameraFraming
    {
        public const float MinimumDistance = 16.2f;
        public const float MaximumPortraitDistance = 22.5f;
        private const float HalfFovRadians = 27f * 0.5f * (float)Math.PI / 180f;

        public static PicnicCameraLanding FindNextLanding(IReadOnlyList<PicnicPlatformSpec> platforms,
            string currentSupportId, float playerX, float playerY, int facing)
        {
            int direction = facing < 0 ? -1 : 1;
            float bestDistance = float.PositiveInfinity;
            PicnicCameraLanding best = default;
            for (int i = 0; i < platforms.Count; i++)
            {
                PicnicPlatformSpec platform = platforms[i];
                if (platform.Id == currentSupportId || (playerX >= platform.X && playerX <= platform.X + platform.Width))
                {
                    continue;
                }
                float edge = direction > 0 ? platform.X : platform.X + platform.Width;
                float distance = (edge - playerX) * direction;
                if (distance < 0f || distance > 310f)
                {
                    continue;
                }
                float surfaceY = PicnicLevelData.SurfaceY(platform, edge);
                float rise = playerY - surfaceY;
                if (!float.IsFinite(surfaceY) || rise > 182f || rise < -240f || distance >= bestDistance)
                {
                    continue;
                }
                float span = Math.Min(60f, platform.Width * 0.5f);
                float inset = Math.Min(22f, platform.Width * 0.15f);
                float near = edge + direction * inset;
                float far = edge + direction * (inset + span);
                best = new PicnicCameraLanding(Math.Min(near, far) / 100f, Math.Max(near, far) / 100f,
                    (PicnicLevelData.GroundY - surfaceY) / 100f);
                bestDistance = distance;

            }
            return best;
        }

        public static PicnicMobileCameraFrame Evaluate(float playerX, float groundAnchorY, float levelWidth,
            float aspect, int facing, PicnicCameraLanding landing)
        {
            aspect = Math.Max(0.45f, aspect);
            int direction = facing < 0 ? -1 : 1;
            float tanHalfFov = (float)Math.Tan(HalfFovRadians);
            float distance = aspect < 1f
                ? Math.Min(MaximumPortraitDistance, Math.Max(MinimumDistance, 4.8f / (2f * tanHalfFov * aspect)))
                : MinimumDistance;
            float height = distance * 2f * tanHalfFov;
            float width = height * aspect;
            if (landing.HasValue && aspect < 1f)
            {
                float farX = direction > 0 ? landing.MaxX : landing.MinX;
                float requiredWidth = Math.Abs(farX - playerX) + 0.9f;
                distance = Math.Min(MaximumPortraitDistance, Math.Max(distance, requiredWidth / (2f * tanHalfFov * aspect)));
                height = distance * 2f * tanHalfFov;
                width = height * aspect;
            }
            float center = playerX + direction * width * 0.14f;
            if (landing.HasValue)
            {
                if (direction > 0)
                {
                    center = Math.Max(center, landing.MaxX + 0.25f - width * 0.5f);
                    center = Math.Min(center, playerX + width * 0.5f - 0.65f);
                }
                else
                {
                    center = Math.Min(center, landing.MinX - 0.25f + width * 0.5f);
                    center = Math.Max(center, playerX - width * 0.5f + 0.65f);
                }
            }
            center = levelWidth <= width ? levelWidth * 0.5f : Math.Max(width * 0.5f, Math.Min(levelWidth - width * 0.5f, center));
            float targetY = groundAnchorY + height * 0.22f;
            if (landing.HasValue)
            {
                targetY = Math.Max(targetY, landing.Y + height * 0.1f);
            }
            return new PicnicMobileCameraFrame(center, targetY, distance, width, height);
        }

        public static float SmoothingFactor(float dt)
        {
            return 1f - (float)Math.Exp(-Math.Max(0f, dt) * 5f);
        }
    }
}
