using System;
using System.Collections.Generic;

namespace YunaLegend.Picnic.Core
{
    public enum PicnicTerrain { Soil, Wood, Grass, Gravel, Stone, Cloud }
    public enum PicnicPhase { Opening, Playing, Rescuing, Defeat, Ending, Complete }
    public enum PicnicCreatureKind { Ant, Caterpillar, Bee, Worm }

    public sealed class PicnicSupportSpec
    {
        public string Id { get; }
        public float X { get; }
        public float Width { get; }
        public float Y { get; }
        public PicnicTerrain Terrain { get; }
        public int Wheel { get; }
        public float Angle { get; }
        public float Slope { get; }
        public float Hill { get; }
        public float Waves { get; }
        public bool Ground { get; }
        public float[] GrassPatches { get; }

        public PicnicSupportSpec(string id, float x, float width, float y, PicnicTerrain terrain,
            int wheel = -1, float angle = 0f, float slope = 0f, float hill = 0f, float waves = 1f, bool ground = false, float[] grassPatches = null)
        {
            Id = id;
            X = x;
            Width = width;
            Y = y;
            Terrain = terrain;
            Wheel = wheel;
            Angle = angle;
            Slope = slope;
            Hill = hill;
            Waves = waves;
            Ground = ground;
            GrassPatches = grassPatches ?? Array.Empty<float>();
        }

        public float Left(float time) => Center(time) - Width * 0.5f;
        public float Right(float time) => Center(time) + Width * 0.5f;
        public float Center(float time) => X + (Wheel < 0 ? 0f : 1.5f * (float)Math.Cos(Angle + time * WheelSpeed));
        public float Height(float time, float x) => Y + (Wheel < 0 ? 0f : -1.15f * (float)Math.Sin(Angle + time * WheelSpeed)) + (x - Center(time)) * Slope + HillOffset(x);
        private float WheelSpeed => Wheel == 2 ? -0.36f : Wheel == 3 ? 0.34f : 0.38f;
        private float HillOffset(float x)
        {
            float t = Math.Max(0f, Math.Min(1f, (x - (X - Width * 0.5f)) / Width));
            float sine = (float)Math.Sin(Math.PI * Waves * t);
            return Hill * sine * sine;
        }
        public PicnicTerrain TerrainAt(float x)
        {
            float t = (x - X + Width * 0.5f) / Width;
            for (int i = 0; i < GrassPatches.Length; i += 2)
            {
                if (t >= GrassPatches[i] && t <= GrassPatches[i + 1]) return PicnicTerrain.Grass;
            }
            return Terrain;
        }
    }

    public sealed class PicnicSectionSpec
    {
        public string Name { get; }
        public float Start { get; }
        public float SafeX { get; }
        public PicnicSectionSpec(string name, float start)
        {
            Name = name;
            Start = start;
            SafeX = start + 1f;
        }
    }

    public sealed class PicnicCreatureSpec
    {
        public PicnicCreatureKind Kind { get; }
        public float X { get; }
        public float Y { get; }
        public float Range { get; }
        public PicnicCreatureSpec(PicnicCreatureKind kind, float x, float y, float range = 1.2f)
        {
            Kind = kind;
            X = x;
            Y = y;
            Range = range;
        }
    }

    /// <summary>Original world data, in metres. Reference document 05 uses 100 pixels per metre.</summary>
    public sealed class PicnicStageSpec
    {
        public const float Goal = 474.88f;
        public const float Gravity = 16f;
        public const float JumpSpeed = 7.2f;
        public IReadOnlyList<PicnicSupportSpec> Supports { get; }
        public IReadOnlyList<PicnicSectionSpec> Sections { get; }
        public IReadOnlyList<PicnicCreatureSpec> Creatures { get; }
        public float[] PanicStarts { get; } = { 107f, 227f, 484f };
        public float[] PanicEnds { get; } = { 115.5f, 240.5f, 503.2f };

        public PicnicStageSpec()
        {
            Supports = PicnicReferenceLayout.Supports;
            Sections = PicnicReferenceLayout.Sections;
            Creatures = PicnicReferenceLayout.Creatures;
            PanicStarts = new[] { 106.7f, 196.9f, 454.18f };
            PanicEnds = new[] { 115.7f, 210.5f, 473.38f };
        }

        public int SectionAt(float x)
        {
            for (int i = Sections.Count - 1; i >= 0; i--)
            {
                if (x >= Sections[i].Start) return i;
            }
            return 0;
        }

        public static float Speed(PicnicTerrain terrain)
        {
            if (terrain == PicnicTerrain.Grass) return 1f;
            return terrain == PicnicTerrain.Gravel ? 4.1f : 2.2f;
        }

    }
}
