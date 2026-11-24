using System;
using System.Collections.Generic;

namespace YunaLegend.Picnic.Core
{
    /// <summary>World-space measurements of the actual support contract, independent of artwork bounds.</summary>
    public readonly struct PicnicSupportMeasurement
    {
        public int Index { get; }
        public string Id { get; }
        public float Time { get; }
        public float Left { get; }
        public float Right { get; }
        public float Center { get; }
        public float Width { get; }
        public float LeftHeight { get; }
        public float CenterHeight { get; }
        public float RightHeight { get; }
        public float OrbitLeft { get; }
        public float OrbitRight { get; }
        public float MinimumCenterHeight { get; }
        public float MaximumCenterHeight { get; }

        public PicnicSupportMeasurement(int index, PicnicSupportSpec support, float time)
        {
            Index = index;
            Id = support.Id;
            Time = time;
            Left = support.Left(time);
            Right = support.Right(time);
            Center = support.Center(time);
            Width = support.Width;
            LeftHeight = support.Height(time, Left);
            CenterHeight = support.Height(time, Center);
            RightHeight = support.Height(time, Right);
            float orbitX = support.Wheel >= 0 ? 1.5f : 0f;
            float orbitY = support.Wheel >= 0 ? 1.15f : 0f;
            OrbitLeft = support.X - orbitX - support.Width * 0.5f;
            OrbitRight = support.X + orbitX + support.Width * 0.5f;
            MinimumCenterHeight = support.Wheel >= 0 ? support.Y - orbitY + Math.Min(0f, support.Hill) : CenterHeight;
            MaximumCenterHeight = support.Wheel >= 0 ? support.Y + orbitY + Math.Max(0f, support.Hill) : CenterHeight;
        }

        public float GapTo(PicnicSupportMeasurement destination)
        {
            return destination.Left - Right;
        }

        public static IReadOnlyList<PicnicSupportMeasurement> Measure(PicnicStageSpec stage, float time)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }
            var result = new PicnicSupportMeasurement[stage.Supports.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new PicnicSupportMeasurement(i, stage.Supports[i], time);
            }
            return result;
        }
    }
}
