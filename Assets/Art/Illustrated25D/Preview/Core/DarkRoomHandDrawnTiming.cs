using System;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Pure 25 FPS exposure timing for CIN02 (Dark Room awakening with Nanda).
    /// Follows Evidence/Cinematics25/CIN02-direcao.md without modifying the legacy opening flow.
    /// </summary>
    public static class DarkRoomHandDrawnTiming
    {
        public const int FramesPerSecond = 25;
        public const int FrameCount = 300;
        public const double DurationSeconds = FrameCount / (double)FramesPerSecond;

        private static readonly int[] ShotStarts = { 0, 50, 150, 235 };

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

        public static void CaptionAt(int frame, out string speaker, out string text)
        {
            speaker = string.Empty;
            text = string.Empty;

            if (frame >= 50 && frame <= 84)
            {
                speaker = "Nanda";
                text = "Levanta, dorminhoca.";
                return;
            }

            if (frame >= 85 && frame <= 116)
            {
                speaker = "Nanda";
                text = "Temos que brincar.";
                return;
            }

            if (frame >= 117 && frame <= 149)
            {
                speaker = "Nanda";
                text = "Você dormiu demais.";
            }
        }
    }
}
