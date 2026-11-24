using System;
using System.Collections.Generic;

namespace YunaLegend.Picnic.Core
{
    public enum PicnicTerrain { Wood, Soil, Stone, Cloud, Grass, Gravel }
    public enum PicnicStage { Intro, Running, Rescuing, Disoriented, Complete, Falling, GameOver }
    public enum PicnicEnemyKind { Ant, Caterpillar }
    public enum PicnicDifficulty { Normal, Easy }

    public readonly struct PicnicGrassPatch
    {
        public float Start { get; }
        public float End { get; }

        public PicnicGrassPatch(float start, float end)
        {
            Start = start;
            End = end;
        }
    }

    public readonly struct PicnicStepPoint
    {
        public float X { get; }
        public float Y { get; }

        public PicnicStepPoint(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public sealed class PicnicPlatformSpec
    {
        public string Id { get; }
        public float X { get; }
        public float Width { get; }
        public float Y { get; }
        public PicnicTerrain Terrain { get; }
        public bool Ground { get; }
        public bool Aerial { get; }
        public bool Rotating { get; }
        public string WheelId { get; }
        public float Hill { get; }
        public int Waves { get; }
        public PicnicGrassPatch[] GrassPatches { get; }

        public PicnicPlatformSpec(string id, float x, float width, float y, PicnicTerrain terrain,
            bool ground = false, bool aerial = false, bool rotating = false, string wheelId = null,
            float hill = 0f, int waves = 1, PicnicGrassPatch[] grassPatches = null)
        {
            Id = id;
            X = x;
            Width = width;
            Y = y;
            Terrain = terrain;
            Ground = ground;
            Aerial = aerial;
            Rotating = rotating;
            WheelId = wheelId;
            Hill = hill;
            Waves = waves;
            GrassPatches = grassPatches ?? Array.Empty<PicnicGrassPatch>();
        }
    }

    public readonly struct PicnicWheelSpec
    {
        public string Id { get; }
        public float X { get; }
        public float Y { get; }
        public float Rx { get; }
        public float Ry { get; }
        public float Speed { get; }
        public float Phase { get; }

        public PicnicWheelSpec(string id, float x, float y, float rx, float ry, float speed, float phase = 0f)
        {
            Id = id;
            X = x;
            Y = y;
            Rx = rx;
            Ry = ry;
            Speed = speed;
            Phase = phase;
        }
    }

    public readonly struct PicnicWaterSpec
    {
        public string Id { get; }
        public float X { get; }
        public float Width { get; }

        public PicnicWaterSpec(string id, float x, float width)
        {
            Id = id;
            X = x;
            Width = width;
        }
    }

    public readonly struct PicnicSectionSpec
    {
        public string Id { get; }
        public float X { get; }
        public float End { get; }
        public string Name { get; }
        public int Chapter { get; }

        public PicnicSectionSpec(string id, float x, float end, string name, int chapter)
        {
            Id = id;
            X = x;
            End = end;
            Name = name;
            Chapter = chapter;
        }
    }

    public sealed class PicnicEnemySpec
    {
        public string Id { get; }
        public PicnicEnemyKind Kind { get; }
        public string SupportId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float BaseY { get; set; }
        public float Left { get; set; }
        public float Right { get; set; }
        public float Width { get; }
        public float Height { get; }
        public float Speed { get; }
        public float Period { get; }
        public float Phase { get; }
        public PicnicStepPoint[] Route { get; }

        public PicnicEnemySpec(string id, PicnicEnemyKind kind, string supportId, float x, float y,
            float left, float right, float width, float height, float speed, float period, float phase,
            PicnicStepPoint[] route = null)
        {
            Id = id;
            Kind = kind;
            SupportId = supportId;
            X = x;
            Y = y;
            BaseY = y;
            Left = left;
            Right = right;
            Width = width;
            Height = height;
            Speed = speed;
            Period = period;
            Phase = phase;
            Route = route;
        }
    }

    public readonly struct PicnicCloudStatus
    {
        public bool Solid { get; }
        public float Alpha { get; }
        public float Elapsed { get; }

        public PicnicCloudStatus(bool solid, float alpha, float elapsed)
        {
            Solid = solid;
            Alpha = alpha;
            Elapsed = elapsed;
        }
    }

    /// <summary>
    /// 1:1 faithful C# port of legacy gardenLevel.js in pixel world coordinates (100 px = 1 Unity unit).
    /// All Y coordinates use legacy top-down pixel space (ground = 590, waterY = 650, height = 720).
    /// </summary>
    public sealed class PicnicLevelData
    {
        public const float PixelsPerUnit = 100f;
        public const float Height = 720f;
        public const float GroundY = 590f;
        public const float PlayerHeight = 92f;
        public const float HalfFoot = 35f;
        public const float DefaultSpeed = 220f;
        public const float JumpSpeed = 720f;
        public const float Gravity = 1600f;
        public const float FixedStep = 1f / 120f;
        public const float WaterY = 650f;
        public const float RiverBedY = WaterY + 75f;

        public const float HomeX = 30f;
        public const float HomeY = 201f;
        public const float HomeW = 500f;
        public const float HomeH = 401f;

        public static readonly PicnicStepPoint[] HomeSteps =
        {
            new PicnicStepPoint(338f, 540f),
            new PicnicStepPoint(432f, 546f),
            new PicnicStepPoint(476f, 574f),
            new PicnicStepPoint(530f, 590f),
        };

        public static readonly PicnicLevelData Default = new PicnicLevelData();

        public IReadOnlyList<PicnicPlatformSpec> StaticPlatforms { get; }
        public IReadOnlyDictionary<string, PicnicPlatformSpec> PlatformById { get; }
        public IReadOnlyList<PicnicWheelSpec> Wheels { get; }
        public IReadOnlyList<PicnicWaterSpec> Waters { get; }
        public IReadOnlyList<PicnicSectionSpec> Sections { get; }
        public IReadOnlyList<float> Checkpoints { get; }
        public IReadOnlyList<PicnicEnemySpec> Enemies { get; }
        public float Goal { get; }
        public float Width { get; }
        public float FinalStart { get; }

        private readonly Dictionary<int, List<PicnicPlatformSpec>> buckets = new Dictionary<int, List<PicnicPlatformSpec>>();

        private sealed class MutablePlatform
        {
            public string Id;
            public float X;
            public float W;
            public float Y;
            public PicnicTerrain Terrain;
            public bool Ground;
            public bool Aerial;
            public float Hill;
            public int Waves = 1;
            public float[] Profile;
            public PicnicGrassPatch[] GrassPatches;
        }

        private PicnicLevelData()
        {
            var platforms = new List<MutablePlatform>();
            var enemies = new List<PicnicEnemySpec>();
            var waters = new List<PicnicWaterSpec>();
            var sections = new List<PicnicSectionSpec>();
            var checkpoints = new List<float> { 120f };

            MutablePlatform Add(string id, float x, float w, PicnicTerrain terrain = PicnicTerrain.Soil,
                float y = GroundY, bool ground = false, bool aerial = false)
            {
                var p = new MutablePlatform
                {
                    Id = id,
                    X = x,
                    W = w,
                    Y = y,
                    Terrain = terrain,
                    Ground = ground,
                    Aerial = aerial,
                };
                platforms.Add(p);
                return p;
            }

            MutablePlatform Ground(string id, float x, float w, PicnicTerrain terrain = PicnicTerrain.Soil)
            {
                return Add(id, x, w, terrain, GroundY, ground: true);
            }

            void AddEnemy(string id, PicnicEnemyKind kind, string supportId, float px, float pw, float py, float progress = 0f)
            {
                enemies.Add(new PicnicEnemySpec(
                    id,
                    kind,
                    supportId,
                    px + pw * 0.6f,
                    py,
                    px + 70f,
                    px + pw - 70f,
                    kind == PicnicEnemyKind.Ant ? 56f : 82f,
                    kind == PicnicEnemyKind.Ant ? 35f : 42f,
                    (kind == PicnicEnemyKind.Ant ? 43f : 30f) + progress * 19f,
                    3.2f - progress * 0.7f,
                    enemies.Count * 0.71f));
            }

            float AddRiver(string id, float x, int count, float gap = 122f, float rise = 32f, bool ant = false)
            {
                var stones = new List<MutablePlatform>(count);
                for (int i = 0; i < count; i++)
                {
                    stones.Add(Add($"{id}-pedra-{i}", x + i * (160f + gap), 160f, PicnicTerrain.Stone, 550f - (i % 2) * rise));
                }
                float end = stones[stones.Count - 1].X + 160f;
                waters.Add(new PicnicWaterSpec(id, x - gap, end - x + gap * 2f));
                if (ant)
                {
                    int routeLen = Math.Min(5, stones.Count) - 1;
                    var route = new PicnicStepPoint[routeLen];
                    for (int i = 0; i < routeLen; i++)
                    {
                        var st = stones[i + 1];
                        route[i] = new PicnicStepPoint(st.X + st.W * 0.5f, st.Y);
                    }
                    enemies.Add(new PicnicEnemySpec(
                        id + "-formiga-do-rio",
                        PicnicEnemyKind.Ant,
                        null,
                        route[0].X,
                        route[0].Y,
                        route[0].X,
                        route[route.Length - 1].X,
                        56f,
                        35f,
                        60f,
                        2.5f,
                        0f,
                        route));
                }
                return end;
            }

            sections.Add(new PicnicSectionSpec("saida", 0f, 7350f, "O portão de casa", 0));
            Ground("entrada", 0f, 680f, PicnicTerrain.Wood);
            Add("suspensa-1", 790f, 175f, PicnicTerrain.Wood, 535f);
            Add("suspensa-2", 1070f, 175f, PicnicTerrain.Wood, 520f);
            Ground("primeira-grama", 1330f, 980f, PicnicTerrain.Grass);
            AddRiver("primeiro-riacho", 2440f, 3, 115f, 26f, false);
            Ground("grama-da-margem", 3200f, 750f, PicnicTerrain.Grass);
            Ground("pedrinhas-da-roda", 3950f, 850f, PicnicTerrain.Gravel);

            var wheelList = new List<PicnicWheelSpec>
            {
                new PicnicWheelSpec("roda", 5050f, 465f, 150f, 115f, 0.38f, 0f),
            };

            for (int i = 0; i < 3; i++)
            {
                Add("nuvem-inicial-" + i, 5350f + i * 265f, 170f, PicnicTerrain.Cloud, 450f + (i % 2) * 30f);
            }

            var firstFight = Ground("primeiros-bichinhos", 6080f, 1270f);
            AddEnemy("primeira-lagarta", PicnicEnemyKind.Caterpillar, firstFight.Id, 6200f, 440f, firstFight.Y);
            AddEnemy("primeira-formiga", PicnicEnemyKind.Ant, firstFight.Id, 6740f, 460f, firstFight.Y);

            string[] chapters =
            {
                "Canteiros de margaridas",
                "O caminho das hortênsias",
                "Riachos de água clara",
                "Entre salgueiros",
                "O jardim das borboletas",
                "A alameda da grande árvore",
            };

            var layouts = new (string part, float width, PicnicTerrain terrain, float hill)[][]
            {
                new[] { ("caminho", 1000f, PicnicTerrain.Soil, 170f), ("gramado", 2200f, PicnicTerrain.Grass, 0f), ("margem", 300f, PicnicTerrain.Soil, 0f), ("rio", 3f, PicnicTerrain.Stone, 0f), ("clareira", 650f, PicnicTerrain.Soil, 0f) },
                new[] { ("caminho", 800f, PicnicTerrain.Soil, 0f), ("gramado", 2400f, PicnicTerrain.Grass, 180f), ("pedrinhas", 650f, PicnicTerrain.Gravel, 0f), ("clareira", 900f, PicnicTerrain.Soil, 0f) },
                new[] { ("caminho", 700f, PicnicTerrain.Soil, 0f), ("gramado", 1700f, PicnicTerrain.Grass, 0f), ("margem", 300f, PicnicTerrain.Soil, 0f), ("rio", 5f, PicnicTerrain.Stone, 0f), ("clareira", 850f, PicnicTerrain.Soil, 0f), ("colina", 1400f, PicnicTerrain.Soil, 210f) },
                new[] { ("caminho", 650f, PicnicTerrain.Soil, 0f), ("gramado", 2600f, PicnicTerrain.Grass, 200f), ("pedrinhas", 500f, PicnicTerrain.Gravel, 0f), ("clareira", 1300f, PicnicTerrain.Soil, 0f) },
                new[] { ("caminho", 1000f, PicnicTerrain.Soil, 0f), ("pedrinhas", 600f, PicnicTerrain.Gravel, 0f), ("gramado", 2250f, PicnicTerrain.Grass, 0f), ("margem", 300f, PicnicTerrain.Soil, 0f), ("rio", 4f, PicnicTerrain.Stone, 0f), ("clareira", 650f, PicnicTerrain.Soil, 0f) },
                new[] { ("caminho", 850f, PicnicTerrain.Soil, 0f), ("gramado", 2400f, PicnicTerrain.Grass, 160f), ("margem", 300f, PicnicTerrain.Soil, 0f), ("rio", 5f, PicnicTerrain.Stone, 0f), ("clareira", 850f, PicnicTerrain.Soil, 0f) },
            };

            float cursor = 7350f;
            float[] aerialWheelSpeeds = { 0.38f, -0.36f, 0.34f };

            for (int region = 0; region < layouts.Length; region++)
            {
                int index = region * 2;
                if (index == 6)
                {
                    float aerialStart = cursor;
                    Ground("aereo-entrada", aerialStart, 360f);
                    checkpoints.Add(aerialStart + 120f);
                    for (int i = 0; i < 3; i++)
                    {
                        wheelList.Add(new PicnicWheelSpec("aerea-" + (i + 1), aerialStart + 610f + i * 480f, 465f, 150f, 115f, aerialWheelSpeeds[i], i * 0.6f));
                    }
                    var aerialPieces = new (string id, float x, float w, PicnicTerrain t, float y)[]
                    {
                        ("nuvem-0", 1840f, 170f, PicnicTerrain.Cloud, 470f),
                        ("nuvem-1", 2100f, 170f, PicnicTerrain.Cloud, 435f),
                        ("ilha-0", 2360f, 180f, PicnicTerrain.Gravel, 470f),
                        ("nuvem-2", 2630f, 170f, PicnicTerrain.Cloud, 430f),
                        ("nuvem-3", 2890f, 170f, PicnicTerrain.Cloud, 465f),
                        ("ilha-1", 3150f, 190f, PicnicTerrain.Gravel, 440f),
                        ("nuvem-4", 3440f, 170f, PicnicTerrain.Cloud, 450f),
                        ("nuvem-5", 3710f, 170f, PicnicTerrain.Cloud, 490f),
                    };
                    foreach (var piece in aerialPieces)
                    {
                        Add("aereo-" + piece.id, aerialStart + piece.x, piece.w, piece.t, piece.y, ground: false, aerial: true);
                    }
                    Ground("aereo-saida", aerialStart + 4000f, 360f);
                    checkpoints.Add(aerialStart + 4120f);
                    cursor = aerialStart + 4360f;
                    sections.Add(new PicnicSectionSpec("aereo", aerialStart, cursor, "Por cima do jardim", 3));
                }

                string secId = "trecho-" + index;
                float secStart = cursor;
                float progress = region / 5f;
                checkpoints.Add(secStart + 120f);

                foreach (var item in layouts[region])
                {
                    if (item.part == "rio")
                    {
                        float gap = 110f + region * 4f;
                        cursor = AddRiver(secId + "-rio", cursor + gap, (int)item.width, gap, 26f + region * 4f, region >= 2) + gap;
                        continue;
                    }
                    var p = Ground(secId + "-" + item.part, cursor, item.width, item.terrain);
                    cursor += item.width;
                    if (item.hill > 0f)
                    {
                        p.Hill = item.hill;
                        p.Waves = (region == 3 || region == 5) ? 2 : 1;
                        if (item.terrain == PicnicTerrain.Grass)
                        {
                            p.Terrain = PicnicTerrain.Soil;
                            p.GrassPatches = new[]
                            {
                                new PicnicGrassPatch(0.17f, 0.23f),
                                new PicnicGrassPatch(0.47f, 0.54f),
                                new PicnicGrassPatch(0.77f, 0.83f),
                            };
                        }
                    }
                    else if (item.part == "caminho")
                    {
                        p.Profile = (region % 2 == 1) ? new[] { 0f, -55f, -110f, -55f, 0f } : new[] { 0f, -60f, -60f, 0f };
                    }
                    else if (item.part == "clareira")
                    {
                        if (region == 1) p.Hill = 85f;
                        else p.Profile = (region == 3) ? new[] { 0f, -60f, -60f, 0f } : new[] { 0f };
                    }

                    if (item.part == "gramado" || item.part == "clareira" || item.part == "colina")
                    {
                        AddEnemy(secId + "-" + item.part + "-lagarta", PicnicEnemyKind.Caterpillar, p.Id, p.X, p.W, p.Y, progress);
                    }
                    if (item.part == "caminho" || item.part == "pedrinhas" || item.part == "gramado")
                    {
                        AddEnemy(secId + "-" + item.part + "-formiga", PicnicEnemyKind.Ant, p.Id, p.X + 80f, p.W * 0.5f, p.Y, progress);
                        if (p.W >= 1400f)
                        {
                            AddEnemy(secId + "-" + item.part + "-formiga-2", PicnicEnemyKind.Ant, p.Id, p.X + p.W * 0.45f, p.W * 0.5f, p.Y, progress);
                        }
                    }
                }

                sections.Add(new PicnicSectionSpec(secId, secStart, cursor, chapters[region], region + 1));
            }

            FinalStart = cursor;
            Ground("descanso-final", cursor, 450f);
            checkpoints.Add(cursor + 120f);
            cursor += 570f;
            for (int i = 0; i < 4; i++)
            {
                Add("nuvem-final-" + i, cursor + i * 265f, 170f, PicnicTerrain.Cloud, 530f - (i % 2) * 36f);
            }
            cursor += 4f * 265f;
            Ground("arvore", cursor, 1450f);
            cursor += 1450f;
            Goal = cursor - 680f;
            Width = cursor;
            sections.Add(new PicnicSectionSpec("arvore", FinalStart, Width, "A grande árvore", 7));

            // Expand solid ground reliefs (exact match to gardenLevel.js lines 375-429).
            for (int i = platforms.Count - 1; i >= 0; i--)
            {
                var p = platforms[i];
                if (!p.Ground || p.Terrain == PicnicTerrain.Wood ||
                    p.Id == "primeiros-bichinhos" || p.Id == "descanso-final" || p.Id == "arvore" ||
                    p.W < 600f || p.Hill > 0f)
                {
                    continue;
                }

                bool hasTrecho = TryParseTrechoIndex(p.Id, out int n);
                float amplitude = 45f + (n / 2) * 5f;
                float[] profile = { 0f, -amplitude, -amplitude * 2f, -amplitude, 0f };
                if (n % 3 == 1) profile = new[] { 0f, -amplitude, 0f, -amplitude, 0f };
                if (n % 3 == 2) profile = new[] { 0f, -amplitude, -amplitude * 2f, -amplitude, -amplitude, 0f };
                if (hasTrecho && p.W < 1100f) profile = new[] { 0f, -amplitude, 0f };
                if (p.Terrain == PicnicTerrain.Gravel) profile = new[] { 0f, -70f, 0f };
                if (p.Profile != null) profile = p.Profile;

                float[] weights = p.Terrain == PicnicTerrain.Gravel
                    ? new[] { 0.45f, 0.25f, 0.30f }
                    : UniformWeights(profile.Length);

                float x = p.X;
                var merged = new List<MutablePlatform>();
                for (int j = 0; j < profile.Length; j++)
                {
                    float end = j == profile.Length - 1 ? p.X + p.W : x + p.W * weights[j];
                    var piece = new MutablePlatform
                    {
                        Id = j > 0 ? p.Id + "-relevo-" + j : p.Id,
                        X = x,
                        W = end - x,
                        Y = p.Y + profile[j],
                        Terrain = p.Terrain,
                        Ground = p.Ground,
                        Aerial = p.Aerial,
                        Hill = p.Hill,
                        Waves = p.Waves,
                        GrassPatches = p.GrassPatches,
                    };
                    x = end;
                    if (merged.Count > 0 && Math.Abs(merged[merged.Count - 1].Y - piece.Y) < 0.001f)
                    {
                        merged[merged.Count - 1].W += piece.W;
                    }
                    else
                    {
                        merged.Add(piece);
                    }
                }

                platforms.RemoveAt(i);
                platforms.InsertRange(i, merged);

                foreach (var e in enemies)
                {
                    if (e.SupportId != p.Id) continue;
                    var home = merged[0];
                    foreach (var v in merged)
                    {
                        if (e.X >= v.X && e.X < v.X + v.W) { home = v; break; }
                    }
                    e.X = Math.Max(home.X + 70f, Math.Min(home.X + home.W - 70f, e.X));
                    e.SupportId = home.Id;
                    e.Y = home.Y;
                    e.BaseY = home.Y;
                    e.Left = home.X + 70f;
                    e.Right = home.X + home.W - 70f;
                }
            }

            var finalPlatforms = new List<PicnicPlatformSpec>(platforms.Count);
            var map = new Dictionary<string, PicnicPlatformSpec>(platforms.Count);
            foreach (var mp in platforms)
            {
                var spec = new PicnicPlatformSpec(mp.Id, mp.X, mp.W, mp.Y, mp.Terrain, mp.Ground, mp.Aerial, false, null, mp.Hill, mp.Waves, mp.GrassPatches);
                finalPlatforms.Add(spec);
                map[spec.Id] = spec;
                int b0 = (int)Math.Floor(spec.X / 1024f);
                int b1 = (int)Math.Floor((spec.X + spec.Width) / 1024f);
                for (int k = b0; k <= b1; k++)
                {
                    if (!buckets.TryGetValue(k, out var list))
                    {
                        list = new List<PicnicPlatformSpec>();
                        buckets[k] = list;
                    }
                    list.Add(spec);
                }
            }

            foreach (var e in enemies)
            {
                if (e.Route == null && e.SupportId != null && map.TryGetValue(e.SupportId, out var sup))
                {
                    e.Y = e.BaseY = SurfaceY(sup, e.X);
                }
            }

            StaticPlatforms = finalPlatforms;
            PlatformById = map;
            Wheels = wheelList;
            Waters = waters;
            Sections = sections;
            Checkpoints = checkpoints;
            Enemies = enemies;
        }

        public static float TerrainSpeed(PicnicTerrain terrain)
        {
            if (terrain == PicnicTerrain.Grass) return 100f;
            if (terrain == PicnicTerrain.Gravel) return 410f;
            return DefaultSpeed;
        }

        public static float SurfaceY(PicnicPlatformSpec platform, float x)
        {
            if (platform == null) return GroundY;
            if (platform.Hill <= 0f) return platform.Y;
            float t = Math.Max(0f, Math.Min(1f, (x - platform.X) / platform.Width));
            double s = Math.Sin(Math.PI * platform.Waves * t);
            return platform.Y - platform.Hill * (float)(s * s);
        }

        public static PicnicTerrain TerrainAt(PicnicPlatformSpec platform, float x)
        {
            if (platform == null) return PicnicTerrain.Soil;
            for (int i = 0; i < platform.GrassPatches.Length; i++)
            {
                var patch = platform.GrassPatches[i];
                if (x >= platform.X + platform.Width * patch.Start && x <= platform.X + platform.Width * patch.End)
                {
                    return PicnicTerrain.Grass;
                }
            }
            return platform.Terrain;
        }

        public static PicnicCloudStatus GetCloudStatus(string platformId, IReadOnlyDictionary<string, float> clouds,
            PicnicDifficulty difficulty, float time)
        {
            if (clouds == null || !clouds.TryGetValue(platformId, out float armed))
            {
                return new PicnicCloudStatus(true, 1f, 0f);
            }
            float elapsed = Math.Max(0f, time - armed);
            float duration = difficulty == PicnicDifficulty.Easy ? 3.8f : 2.8f;
            if (elapsed >= duration && elapsed < duration + 4f)
            {
                return new PicnicCloudStatus(false, 0f, elapsed);
            }
            if (elapsed >= duration + 4f)
            {
                return new PicnicCloudStatus(true, 1f, elapsed);
            }
            float t = elapsed / duration;
            float phase = elapsed * (1.5f + 3f * t);
            return new PicnicCloudStatus(true, Math.Sin(phase * Math.PI * 2.0) > 0.0 ? 1f : 0.28f, elapsed);
        }

        public PicnicSectionSpec SectionAt(float x)
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                if (x >= Sections[i].X && x < Sections[i].End) return Sections[i];
            }
            return Sections[Sections.Count - 1];
        }

        public List<PicnicPlatformSpec> QueryPlatforms(float time, IReadOnlyDictionary<string, float> clouds,
            PicnicDifficulty difficulty, float? center = null, float radius = 1000f)
        {
            var result = new List<PicnicPlatformSpec>(32);
            if (!center.HasValue)
            {
                for (int i = 0; i < StaticPlatforms.Count; i++)
                {
                    var p = StaticPlatforms[i];
                    if (p.Terrain != PicnicTerrain.Cloud || GetCloudStatus(p.Id, clouds, difficulty, time).Solid)
                    {
                        result.Add(p);
                    }
                }
            }
            else
            {
                float c = center.Value;
                int b0 = (int)Math.Floor((c - radius) / 1024f);
                int b1 = (int)Math.Floor((c + radius) / 1024f);
                var seen = new HashSet<string>();
                for (int b = b0; b <= b1; b++)
                {
                    if (!buckets.TryGetValue(b, out var list)) continue;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var p = list[i];
                        if (p.X + p.Width < c - radius || p.X > c + radius) continue;
                        if (!seen.Add(p.Id)) continue;
                        if (p.Terrain != PicnicTerrain.Cloud || GetCloudStatus(p.Id, clouds, difficulty, time).Solid)
                        {
                            result.Add(p);
                        }
                    }
                }
            }

            for (int wi = 0; wi < Wheels.Count; wi++)
            {
                var w = Wheels[wi];
                if (center.HasValue && Math.Abs(center.Value - w.X) >= radius + 250f) continue;
                for (int i = 0; i < 4; i++)
                {
                    double angle = time * w.Speed + w.Phase + i * Math.PI * 0.5;
                    result.Add(new PicnicPlatformSpec(
                        w.Id + "-" + i,
                        w.X + (float)Math.Cos(angle) * w.Rx - 72f,
                        144f,
                        w.Y + (float)Math.Sin(angle) * w.Ry,
                        PicnicTerrain.Wood,
                        ground: false,
                        aerial: false,
                        rotating: true,
                        wheelId: w.Id));
                }
            }
            return result;
        }

        private static bool TryParseTrechoIndex(string id, out int index)
        {
            index = 0;
            if (!id.StartsWith("trecho-", StringComparison.Ordinal)) return false;
            int start = 7;
            int end = start;
            while (end < id.Length && char.IsDigit(id[end])) end++;
            return end > start && int.TryParse(id.Substring(start, end - start), out index);
        }

        private static float[] UniformWeights(int count)
        {
            var w = new float[count];
            float v = 1f / count;
            for (int i = 0; i < count; i++) w[i] = v;
            return w;
        }
    }
}
