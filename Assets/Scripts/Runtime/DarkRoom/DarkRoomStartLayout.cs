using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public static class DarkRoomStartLayout
    {
        public const float FirstPlatformX = 2f;
        public const float StartX = -10f;
        public const float StartY = -9f;
        public const float RunSpeed = 3.6f;
        public const float JumpHeight = 2.4f;
        public const float CameraOffset = 3.5f;
        public const float CameraY = -4.5f;
        public static Vector3 Position => new Vector3(StartX, StartY, 0f);

        // Short presses can reach the introductory steps; platform 4 onward is unchanged.
        public static float PlatformCenterY(int index, float originalY) =>
            index < 4 ? -8.65f + index * 0.75f : originalY;
    }
}
