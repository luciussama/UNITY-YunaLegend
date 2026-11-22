using System;

namespace YunaLegend.BearChase.Core
{
    /// <summary>
    /// Pure 25 FPS exposure timing for CIN04 (Bear Chase opening pantomime: approach, touch, awaken, chase).
    /// Follows Evidence/Cinematics25/CIN04-direcao.md without modifying the legacy entrance flow.
    /// </summary>
    public static class BearChaseHandDrawnTiming
    {
        public const int FramesPerSecond = 25;
        public const int FrameCount = 350;
        public const double DurationSeconds = FrameCount / (double)FramesPerSecond;

        private static readonly int[] ShotStarts = { 0, 100, 175, 260 };

        public static int FrameAt(double elapsedSeconds)
        {
            if (double.IsNaN(elapsedSeconds) || elapsedSeconds <= 0d)
            {
                return 0;
            }

            if (elapsedSeconds >= DurationSeconds)
            {
                return FrameCount;
            }

            // Counter floating-point error at an exact 40 ms exposure boundary.
            return (int)Math.Floor((elapsedSeconds * FramesPerSecond) + 1e-9d);
        }

        public static int ShotAt(int frame)
        {
            if (frame >= FrameCount)
            {
                return -1;
            }

            for (int shot = ShotStarts.Length - 1; shot >= 0; shot--)
            {
                if (frame >= ShotStarts[shot])
                {
                    return shot;
                }
            }

            return 0;
        }
    }
}
