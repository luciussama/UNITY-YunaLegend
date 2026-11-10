using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public static class DarkRoomReturnLayout
    {
        public const float RunSpeed = 4.2f;
        public const float JumpHeight = 2.6f;
        public const float ApproachSeconds = 3f;
        public const float FirstPlatformWidth = 2.2f;
        public const float TakeoffLead = 3.2f;
        public const float FirstPlatformOffset = RunSpeed * ApproachSeconds + TakeoffLead;
        public const float FloorY = -9.1f;
        public const float StartY = -9f;
        public const float CameraY = -3.5f;
        public const float CameraOffset = -3.5f;
        public const string JumpCue = "Pule aqui";

        public static float PlatformCenterY(int index, float originalY) =>
            index < 2 ? FloorY + 0.4f + index * 0.75f : originalY;
    }
}
