using System;

namespace YunaLegend.Picnic.Core
{
    public readonly struct PicnicPoseKey
    {
        public string SetName { get; }
        public int FrameIndex { get; }

        public PicnicPoseKey(string setName, int frameIndex)
        {
            SetName = setName;
            FrameIndex = frameIndex;
        }
    }

    public readonly struct PicnicFeetBounds
    {
        public float Left { get; }
        public float Right { get; }

        public PicnicFeetBounds(float left, float right)
        {
            Left = left;
            Right = right;
        }
    }

    /// <summary>
    /// 1:1 faithful C# port of gardenContact.js (feet envelope, surface contact height, and sensory pose selection).
    /// </summary>
    public static class PicnicContact
    {
        // Foot envelope derived from legacy gardenContactData.js bounds
        public const float DefaultFootLeft = -24f;
        public const float DefaultFootRight = 24f;
        public const float GravelFootLeft = -32f;
        public const float GravelFootRight = 32f;

        public static PicnicFeetBounds GetFeet(float x, int facing, PicnicTerrain terrain, float discomfort)
        {
            bool gravel = terrain == PicnicTerrain.Gravel && discomfort > 0.4f;
            float left = gravel ? GravelFootLeft : DefaultFootLeft;
            float right = gravel ? GravelFootRight : DefaultFootRight;
            if (facing < 0)
            {
                return new PicnicFeetBounds(x - right, x - left);
            }
            return new PicnicFeetBounds(x + left, x + right);
        }

        public static float ContactHeight(PicnicPlatformSpec platform, float x, int facing, PicnicTerrain terrain, float discomfort)
        {
            var feet = GetFeet(x, facing, terrain, discomfort);
            float left = Math.Max(platform.X, feet.Left);
            float right = Math.Min(platform.X + platform.Width, feet.Right);
            if (right - left < 0.05f)
            {
                return float.PositiveInfinity;
            }
            if (platform.Hill > 0f)
            {
                float hLeft = PicnicLevelData.SurfaceY(platform, left);
                float hMid = PicnicLevelData.SurfaceY(platform, Math.Max(platform.X, Math.Min(platform.X + platform.Width, x)));
                float hRight = PicnicLevelData.SurfaceY(platform, right);
                return Math.Min(hLeft, Math.Min(hMid, hRight));
            }
            return Math.Min(PicnicLevelData.SurfaceY(platform, left), PicnicLevelData.SurfaceY(platform, right));
        }

        public static PicnicPoseKey SelectGirlPose(bool onGround, float vx, float vy, PicnicTerrain terrain,
            float discomfort, float animationTime, PicnicPoseKey? balancePose)
        {
            if (onGround && balancePose.HasValue)
            {
                return balancePose.Value;
            }

            string set = onGround ? "girl-running-v1" : "girl-actions-v1";
            int index = onGround
                ? (Math.Abs(vx) > 15f
                    ? 2 + ((int)Math.Floor(animationTime * 11f) % 6)
                    : (int)Math.Floor(animationTime * 2f) % 2)
                : (vy < -180f ? 1 : vy < 80f ? 2 : 3);

            if (discomfort > 0.4f)
            {
                set = "girl-discomfort-v1";
                index = !onGround
                    ? 6
                    : Math.Abs(vx) < 10f
                        ? 7
                        : terrain == PicnicTerrain.Grass
                            ? (int)Math.Floor(animationTime * 5f) % 4
                            : 4 + ((int)Math.Floor(animationTime * 16f) % 2);
            }

            if (terrain == PicnicTerrain.Gravel && discomfort > 0.4f && onGround && Math.Abs(vx) > 15f)
            {
                set = "girl-pebbles-v2";
                index = (int)Math.Floor(animationTime * 9f) % 4;
            }

            if (index < 0) index = 0;
            return new PicnicPoseKey(set, index);
        }
    }
}
