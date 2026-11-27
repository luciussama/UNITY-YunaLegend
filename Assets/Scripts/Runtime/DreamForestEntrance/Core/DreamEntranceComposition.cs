using System;
using YunaLegend.DreamForest.Core;

namespace YunaLegend.DreamForestEntrance.Core
{
    public readonly struct EntranceActorPose
    {
        public readonly string Asset;
        public readonly double X;
        public readonly double Y;
        public readonly double Scale;
        public readonly double PivotX;
        public readonly double PivotY;
        public readonly bool Flip;
        public readonly double Rotation;
        public readonly double Stretch;

        public EntranceActorPose(string asset, double x, double y, double scale,
            double pivotX, double pivotY, bool flip = false, double rotation = 0, double stretch = 1)
        {
            Asset = asset;
            X = x;
            Y = y;
            Scale = scale;
            PivotX = pivotX;
            PivotY = pivotY;
            Flip = flip;
            Rotation = rotation;
            Stretch = stretch;
        }
    }

    /// <summary>Authored camera and hand contact in source-painting coordinates, independent of Unity.</summary>
    public readonly struct DreamEntranceComposition
    {
        public readonly double Scale;
        public readonly double FocusX;
        public readonly double FocusY;
        public readonly double ActorFactor;
        public readonly double ForestShade;
        public readonly double Fade;
        public readonly EntranceActorPose Yuna;
        public readonly EntranceActorPose Nanda;

        private DreamEntranceComposition(double scale, double focusX, double focusY, double factor,
            double shade, double fade, EntranceActorPose yuna, EntranceActorPose nanda)
        {
            Scale = scale;
            FocusX = focusX;
            FocusY = focusY;
            ActorFactor = factor;
            ForestShade = shade;
            Fade = fade;
            Yuna = yuna;
            Nanda = nanda;
        }

        public static DreamEntranceComposition Evaluate(int index, double age, double time,
            double width, double height, double dialogueBottom, double cssScale = 1)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            index = Math.Max(0, Math.Min(6, index));
            string shot = DreamForestLevelData.OpeningBeats[index].Shot;
            bool portrait = height > width;
            bool contact = shot == "shake" || shot == "plead";
            double scale = Math.Max(width / 1536, height / 1024);
            double halfW = width / scale / 2;
            double halfH = height / scale / 2;
            double fy = Math.Max(halfH, Math.Min(1024 - halfH, portrait ? 512 : contact ? 660 : 620));
            double fx = Math.Max(halfW, Math.Min(1536 - halfW,
                shot == "lost" ? 930 : shot == "discover" ? 930 - 130 * Smooth(age / 2) : 800));
            double bottom = height / 2 + (920 - fy) * scale;
            double factor = Math.Min(portrait ? 0.68 : 1,
                Math.Max(0.25, (bottom - dialogueBottom - 18 * cssScale) / (500 * scale)));
            double girlScale = 440.0 / 540 * factor;
            double fairyScale = girlScale * 0.9;
            double shake = contact ? Math.Sin(age * 8) * 3 * factor : 0;
            var fairy = new EntranceActorPose("NandaSleeping", 720, 920, fairyScale,
                288.5, 563, rotation: shake * 0.002, stretch: 1 + Math.Sin(time * 1.4) * 0.002);
            EntranceActorPose girl;
            if (contact)
            {
                double shoulderX = 720 + (1370 - 985 - 288.5) * fairyScale + shake;
                double shoulderY = 920 + (615 - 212 - 563) * fairyScale;
                girl = new EntranceActorPose(shot == "shake" ? "Yuna1" : "Yuna2",
                    shoulderX, shoulderY, girlScale, shot == "shake" ? 455 : 490,
                    shot == "shake" ? 217 : 168, flip: true);
            }
            else if (shot == "lost" || shot == "leave")
            {
                bool leaving = shot == "leave";
                double x = leaving ? 970 + 310 * factor * Smooth(age / 4) : 930 + 40 * Smooth(age / 3);
                girl = new EntranceActorPose("Yuna3", x,
                    920 + (leaving ? Math.Sin(age * 7) * 2 * factor : 0), girlScale, 192.5, 501);
            }
            else
            {
                girl = new EntranceActorPose("Look0", 970, 920, 440.0 / 980 * factor, 345, 980);
            }

            return new DreamEntranceComposition(scale, fx, fy, factor,
                1 - (0.96 + Math.Sin(time * 0.6) * 0.025),
                shot == "leave" && age > 2.8 ? Smooth((age - 2.8) / 1.2) : 0, girl, fairy);
        }

        public static double Smooth(double value)
        {
            value = Math.Max(0, Math.Min(1, value));
            return value * value * (3 - 2 * value);
        }
    }

    /// <summary>QA observes the complete authored acting instead of continuously dismissing its dialogue.</summary>
    public static class DreamEntranceQaPilot
    {
        public static DreamForestInput ComputeInput()
        {
            return default;
        }
    }

    public readonly struct DreamEntranceRasterProfile
    {
        public readonly int Width;
        public readonly int Height;

        public DreamEntranceRasterProfile(int width,int height)
        {
            if (width<=0 || height<=0) throw new ArgumentOutOfRangeException(nameof(width));
            double aspect=width/(double)height;
            bool portrait=aspect<1.15;
            Width=portrait ? 540 : (int)Math.Floor(540*aspect+.5);
            Height=portrait ? (int)Math.Floor(540/aspect+.5) : 540;
        }
    }
}
