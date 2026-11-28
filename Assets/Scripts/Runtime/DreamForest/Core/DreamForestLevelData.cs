using System;
using System.Collections.Generic;

namespace YunaLegend.DreamForest.Core
{
    /// <summary>
    /// Pure C# level geometry and narrative catalog transcribed from dreamLevel.js,
    /// dreamGlider.js, dreamOpening.js, dreamMud.js, dreamStealthLevel.js, and dreamNightmare.js.
    /// </summary>
    public sealed class DreamForestLevelData
    {
        public const float PixelsPerUnit = 100f;
        public const float Height = 900f;
        public const float GrassRoot = 1800f;
        public const float Gravity = 900f;
        public const float Speed = 230f;
        public const float JumpSpeed = 470f;
        public const float ClimbSpeed = 145f;
        public const float BodyHeight = 88f;
        public const float BodyRadius = 16f;
        public const float SpatialCell = 600f;

        // Glider constants (dreamGlider.js)
        public const float GlideStart = 28228f;
        public const float GlideLength = 11200f;
        public const float GlideSpeed = 220f;
        public const float GlideMaxVertical = 400f;
        public const float GlideAcceleration = 400f;

        // Nightmare stealth constants (dreamStealthLevel.js / dreamNightmare.js)
        public const float StealthEntryX = 220f;
        public const float StealthExitX = 8750f;
        public const float StealthWidth = 9300f;
        public const float StealthGroundY = 850f;
        public const float StealthSpeed = 125f;
        public const float StealthGravity = 1500f;
        public const float StealthJumpSpeed = 680f;
        public const float NightmareLightRange = 2100f;
        public const float NightmareRevealDuration = 8f;
        public const float RoachStepSeconds = 1.3f;

        public static readonly DreamForestOpeningBeat[] OpeningBeats =
        {
            new DreamForestOpeningBeat("lost", "", "", 3f, 3f),
            new DreamForestOpeningBeat("discover", "Narrador",
                "Ao olhar para trás, Yuna encontrou Nanda.\nEstranhamente, a fadinha tinha o mesmo tamanho que ela.", 6f, 2f),
            new DreamForestOpeningBeat("shake", "Yuna",
                "Nanda, onde estamos? Está frio aqui.", 7f, 3f),
            new DreamForestOpeningBeat("wait", "", "", 3f, 3f),
            new DreamForestOpeningBeat("plead", "Yuna",
                "Nanda, estou com medo! Me fale como sair daqui.", 7f, 3f),
            new DreamForestOpeningBeat("alone", "Narrador",
                "Sem a orientação da irmã fadinha, Yuna se sentiu perdida.\nSem escolha, decidiu seguir em frente e deixar Nanda dormindo.", 8f, 3f),
            new DreamForestOpeningBeat("leave", "", "", 4f, 4f),
        };

        public static readonly string[] IntroLines =
        {
            "O jardim cresceu por dentro do sono.\nUma folha podia esconder a lua.",
            "Ao tocar as gramas, Yuna se agarra.\nW/S ou ▲/▼ escala; Espaço ou PULAR salta.",
            "Suba no lombo e espere o voo.\nSalte quando o próximo apoio estiver perto.",
        };

        public static readonly DreamForestDialogueLine[] NightmareIntroLines =
        {
            new DreamForestDialogueLine("Yuna", "Será que os bichos foram embora?", 6f),
            new DreamForestDialogueLine("Narrador",
                "Um cheiro repugnante aparece no ar. Cheiro de comida que ninguém quer, cheiro de luz apagada, cheiro de vontade de ir pra casa, cheiro de tristeza.", 14f),
        };

        public static readonly DreamForestDialogueLine[] NightmareWakeLines =
        {
            new DreamForestDialogueLine("Narrador", "O susto saiu num grito.\nNanda abriu os olhos.", 5f),
            new DreamForestDialogueLine("Nanda", "Yuna… estou aqui.", 5f),
            new DreamForestDialogueLine("Narrador", "Era a toalha. Era o sol.\nO pesadelo tinha ficado do outro lado do sono.", 7f),
        };

        private static readonly DreamForestPoint2D[] GlideKnots =
        {
            new DreamForestPoint2D(0f, 420f),
            new DreamForestPoint2D(1000f, 420f),
            new DreamForestPoint2D(2400f, 280f),
            new DreamForestPoint2D(3800f, 510f),
            new DreamForestPoint2D(4800f, 240f),
            new DreamForestPoint2D(6200f, -650f),
            new DreamForestPoint2D(7700f, 790f),
            new DreamForestPoint2D(9200f, -700f),
            new DreamForestPoint2D(10600f, 670f),
            new DreamForestPoint2D(11200f, 350f),
        };

        public static readonly DreamForestStealthPlatformSpec[] StealthPlatforms =
        {
            new DreamForestStealthPlatformSpec(2520f, 2980f, 775f),
            new DreamForestStealthPlatformSpec(3240f, 3700f, 750f),
            new DreamForestStealthPlatformSpec(3960f, 4280f, 785f),
        };

        public static readonly DreamForestLevelData Default = new DreamForestLevelData();

        public DreamForestSectionSpec[] Sections { get; }
        public DreamForestObjectSpec[] Objects { get; }
        public Dictionary<string, DreamForestObjectSpec> ById { get; }
        public float WorldWidth { get; }
        public DreamForestGliderThornSpec[] GlideThorns { get; }
        public DreamForestMudLakeSpec[] MudLakes { get; }
        public DreamForestStealthCoverSpec[] StealthCovers { get; }

        private readonly Dictionary<int, List<DreamForestObjectSpec>> spatialCells = new Dictionary<int, List<DreamForestObjectSpec>>();

        private DreamForestLevelData()
        {
            var sectionDefs = new[]
            {
                (name: "Onde a grama toca a lua", kind: "grass", count: 24, span: 0f),
                (name: "O sopro entre as folhas", kind: "wind", count: 25, span: 0f),
                (name: "Carapaças à deriva", kind: "beetle", count: 4, span: 9020f),
                (name: "Asas sobre o abismo", kind: "bee", count: 22, span: 0f),
                (name: "Fios entre as nuvens", kind: "web", count: 6, span: 0f),
                (name: "Os galhos que o vento embala", kind: "branch", count: 2, span: 0f),
            };

            Sections = new DreamForestSectionSpec[sectionDefs.Length];
            var list = new List<DreamForestObjectSpec>(128);
            float cursor = 0f;

            for (int section = 0; section < sectionDefs.Length; section++)
            {
                var def = sectionDefs[section];
                Sections[section] = new DreamForestSectionSpec(section, def.name, def.kind, cursor);

                if (def.kind == "web")
                {
                    list.Add(new DreamForestObjectSpec
                    {
                        Id = "rest-4",
                        Kind = DreamForestObjectKind.Leaf,
                        X = cursor,
                        Y = 650f,
                        W = 360f,
                        H = 120f,
                        Checkpoint = true,
                        Section = section,
                    });
                    cursor += 540f;
                    for (int i = 0; i < def.count; i++)
                    {
                        list.Add(new DreamForestObjectSpec
                        {
                            Id = "web-" + i,
                            Kind = DreamForestObjectKind.Web,
                            X = cursor,
                            Y = -500f,
                            W = 0f,
                            H = 1500f,
                            Period = 9f,
                            Phase = -i * 1.8f,
                            Section = section,
                            SpiderW = 140f,
                            SpiderH = 84f,
                        });
                        cursor += 310f;
                    }
                    cursor = list[list.Count - 1].X + 220f;
                    continue;
                }

                if (def.kind == "branch")
                {
                    list.Add(new DreamForestObjectSpec
                    {
                        Id = "rest-5",
                        Kind = DreamForestObjectKind.Branch,
                        X = cursor,
                        Y = 570f,
                        W = 560f,
                        H = 200f,
                        Checkpoint = true,
                        Section = section,
                        Amplitude = 14f,
                        Period = 9f,
                        Phase = 0.5f,
                    });
                    cursor = GlideStart + GlideLength + 120f;
                    continue;
                }

                list.Add(new DreamForestObjectSpec
                {
                    Id = "rest-" + section,
                    Kind = DreamForestObjectKind.Leaf,
                    X = cursor,
                    Y = 650f,
                    W = 360f,
                    H = 120f,
                    Checkpoint = true,
                    Section = section,
                });
                cursor += 420f;

                float[] beetleYs = { 500f, 300f, 540f, 360f };
                for (int i = 0; i < def.count; i++)
                {
                    DreamForestObjectKind kind = def.kind == "beetle"
                        ? DreamForestObjectKind.Beetle
                        : (def.kind == "bee" && i % 3 == 0 ? DreamForestObjectKind.Bee : DreamForestObjectKind.Grass);
                    bool moving = kind != DreamForestObjectKind.Grass;
                    int cycle = i % 6;
                    float width = moving ? (kind == DreamForestObjectKind.Beetle ? 300f : 250f) : 30f;
                    float height = kind == DreamForestObjectKind.Beetle
                        ? width * (626f / 996f)
                        : (kind == DreamForestObjectKind.Bee ? width * (400f / 458f) : 350f + cycle * 10f);

                    list.Add(new DreamForestObjectSpec
                    {
                        Id = "dream-" + section + "-" + i,
                        Kind = kind,
                        X = cursor,
                        Y = kind == DreamForestObjectKind.Beetle ? beetleYs[i % beetleYs.Length] : (moving ? 460f + cycle * 20f : 180f + cycle * 33f),
                        W = width,
                        H = height,
                        Amplitude = kind == DreamForestObjectKind.Grass ? (def.kind == "wind" ? 75f : 44f) : (kind == DreamForestObjectKind.Beetle ? 74f : 35f),
                        Period = kind == DreamForestObjectKind.Beetle ? 7.5f : (kind == DreamForestObjectKind.Bee ? 6.5f : 8.5f),
                        Phase = section * 0.7f + i * 1.37f,
                        Section = section,
                    });

                    cursor += kind == DreamForestObjectKind.Beetle
                        ? def.span / def.count
                        : (moving ? 300f : 196f);
                }
            }

            list.Add(new DreamForestObjectSpec
            {
                Id = "dream-finish",
                Kind = DreamForestObjectKind.Plateau,
                X = cursor,
                Y = 2400f,
                W = 560f,
                H = 200f,
                Checkpoint = true,
                Finish = true,
                Section = 5,
                Amplitude = 18f,
                Period = 9f,
                Phase = 2f,
            });

            float[] flightDurations = { 12f, 14f, 13f, 14f };
            float[] flightArcs = { -170f, 140f, -190f, 100f };
            float[] flightWaves = { 65f, -70f, 75f, -65f };
            int beetleIdx = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Kind == DreamForestObjectKind.Beetle && i + 1 < list.Count)
                {
                    var next = list[i + 1];
                    int n = beetleIdx % 4;
                    beetleIdx++;
                    list[i].Flight = new DreamForestFlightSpec
                    {
                        ToX = next.X - list[i].W - 100f,
                        ToY = next.Kind == DreamForestObjectKind.Leaf ? next.Y - list[i].H * 0.7f : next.Y + 35f,
                        Duration = flightDurations[n],
                        Delay = 0.65f,
                        Arc = flightArcs[n],
                        Wave = flightWaves[n],
                        NextId = next.Id,
                    };
                }
            }

            Objects = list.ToArray();
            WorldWidth = cursor + 600f;
            ById = new Dictionary<string, DreamForestObjectSpec>(Objects.Length);
            foreach (var o in Objects)
            {
                ById[o.Id] = o;
                float maxX = o.Flight != null ? Math.Max(o.X, o.Flight.ToX) : o.X;
                int c0 = (int)Math.Floor((o.X - 150f) / SpatialCell);
                int c1 = (int)Math.Floor((maxX + o.W + 150f) / SpatialCell);
                for (int c = c0; c <= c1; c++)
                {
                    if (!spatialCells.TryGetValue(c, out var bucket))
                    {
                        bucket = new List<DreamForestObjectSpec>();
                        spatialCells[c] = bucket;
                    }
                    bucket.Add(o);
                }
            }

            // Build 21 pairs of upper/lower glider thorns
            var thorns = new List<DreamForestGliderThornSpec>(42);
            for (int i = 0; i < 21; i++)
            {
                float tx = GlideStart + 1000f + i * 480f;
                float ty = GlideRouteY(tx);
                float gap = i < 8 ? 300f : 340f;
                thorns.Add(new DreamForestGliderThornSpec(tx - 150f, ty - gap - 900f, 300f, 900f, -1));
                thorns.Add(new DreamForestGliderThornSpec(tx - 150f, ty + 240f, 300f, 900f, 1));
            }
            GlideThorns = thorns.ToArray();

            // Build 4 mud lakes (dreamMud.js)
            MudLakes = BuildMudLakes();

            // Build 15 stealth covers (12 rocks/trunks + 3 mud-bank trunks)
            StealthCovers = BuildStealthCovers(MudLakes);
        }

        private static DreamForestMudLakeSpec[] BuildMudLakes()
        {
            var raw = new[]
            {
                (x: 4400f, w: 850f, h: 330f, path: new[] { new DreamForestPoint2D(0.14f, 0.65f), new DreamForestPoint2D(0.32f, 0.62f), new DreamForestPoint2D(0.55f, 0.58f), new DreamForestPoint2D(0.73f, 0.55f), new DreamForestPoint2D(0.86f, 0.50f) }),
                (x: 5310f, w: 1020f, h: 410f, path: new[] { new DreamForestPoint2D(0.12f, 0.49f), new DreamForestPoint2D(0.30f, 0.44f), new DreamForestPoint2D(0.47f, 0.40f), new DreamForestPoint2D(0.65f, 0.44f), new DreamForestPoint2D(0.88f, 0.62f) }),
                (x: 6430f, w: 700f, h: 300f, path: new[] { new DreamForestPoint2D(0.12f, 0.66f), new DreamForestPoint2D(0.35f, 0.68f), new DreamForestPoint2D(0.52f, 0.58f), new DreamForestPoint2D(0.70f, 0.40f), new DreamForestPoint2D(0.88f, 0.36f) }),
                (x: 7210f, w: 1410f, h: 430f, path: new[] { new DreamForestPoint2D(0.12f, 0.65f), new DreamForestPoint2D(0.30f, 0.68f), new DreamForestPoint2D(0.50f, 0.60f), new DreamForestPoint2D(0.70f, 0.49f), new DreamForestPoint2D(0.82f, 0.39f), new DreamForestPoint2D(0.91f, 0.31f) }),
            };
            var lakes = new DreamForestMudLakeSpec[raw.Length];
            for (int i = 0; i < raw.Length; i++)
            {
                var r = raw[i];
                lakes[i] = new DreamForestMudLakeSpec
                {
                    Id = "mud-lake-" + i,
                    Index = i,
                    PaintX = r.x,
                    W = r.w,
                    H = r.h,
                    Top = 870f - r.h * 0.55f,
                    From = r.x + r.path[0].X * r.w,
                    To = r.x + r.path[r.path.Length - 1].X * r.w,
                    Path = r.path,
                };
            }
            return lakes;
        }

        private DreamForestStealthCoverSpec[] BuildStealthCovers(DreamForestMudLakeSpec[] lakes)
        {
            var covers = new List<DreamForestStealthCoverSpec>(15);
            float[] rockWs = { 250f, 282f, 240f };
            float[] rockHs = { 245f, 220f, 240f };
            float[] trunkWs = { 270f, 310f };
            float[] trunkHs = { 215f, 185f };

            for (int i = 0; i < 12; i++)
            {
                bool isRock = i < 6;
                int frame = isRock ? (i % 3) : ((i - 6) % 2);
                float w = isRock ? rockWs[frame] : trunkWs[frame];
                float h = isRock ? rockHs[frame] : trunkHs[frame];
                float x = 220f + i * 360f;
                float baseY = StealthGroundYAt(x) - 25f;
                covers.Add(CreateCover("stealth-cover-" + i, i, isRock, frame, x, w, h, baseY));
            }

            for (int i = 0; i < lakes.Length - 1; i++)
            {
                float x = (lakes[i].To + lakes[i + 1].From) * 0.5f;
                float baseY = StealthGroundYAt(x) - 25f;
                covers.Add(CreateCover("mud-bank-cover-" + i, 12 + i, false, i % 2, x, 260f, 205f, baseY));
            }

            return covers.ToArray();
        }

        private static DreamForestStealthCoverSpec CreateCover(string id, int index, bool isRock, int frame, float x, float w, float h, float baseY)
        {
            // Rounded boulder/trunk silhouette polygon for raycast spotlight occlusion
            var poly = new[]
            {
                new DreamForestPoint2D(x - w * 0.46f, baseY),
                new DreamForestPoint2D(x - w * 0.42f, baseY - h * 0.62f),
                new DreamForestPoint2D(x - w * 0.22f, baseY - h * 0.94f),
                new DreamForestPoint2D(x + w * 0.22f, baseY - h * 0.94f),
                new DreamForestPoint2D(x + w * 0.42f, baseY - h * 0.62f),
                new DreamForestPoint2D(x + w * 0.46f, baseY),
            };
            return new DreamForestStealthCoverSpec
            {
                Id = id,
                Index = index,
                IsRock = isRock,
                Frame = frame,
                X = x,
                W = w,
                H = h,
                BaseY = baseY,
                Polygon = poly,
            };
        }

        public void QueryNearby(float left, float right, List<DreamForestObjectSpec> output, HashSet<string> seen)
        {
            output.Clear();
            seen.Clear();
            int c0 = (int)Math.Floor(left / SpatialCell);
            int c1 = (int)Math.Floor(right / SpatialCell);
            for (int c = c0; c <= c1; c++)
            {
                if (!spatialCells.TryGetValue(c, out var bucket)) continue;
                for (int i = 0; i < bucket.Count; i++)
                {
                    var o = bucket[i];
                    if (seen.Add(o.Id)) output.Add(o);
                }
            }
        }

        public static DreamForestObjectFrame EvaluateObjectFrame(
            DreamForestObjectSpec o,
            float time,
            Dictionary<string, float?> rides)
        {
            if (o.Kind == DreamForestObjectKind.Web)
            {
                float age = PositiveMod(time + o.Phase, o.Period);
                float spiderY;
                if (age < 1.8f)
                {
                    spiderY = -180f + 880f * CosineSmooth(age / 1.8f);
                }
                else if (age < 5.2f)
                {
                    spiderY = 700f;
                }
                else if (age < 7.0f)
                {
                    spiderY = 700f - 880f * CosineSmooth((age - 5.2f) / 1.8f);
                }
                else
                {
                    spiderY = -180f;
                }
                return new DreamForestObjectFrame(o, o.X, o.Y, 0f, spiderY, 0f, 0f);
            }

            if (o.Kind == DreamForestObjectKind.Plateau)
            {
                return new DreamForestObjectFrame(o, o.X, o.Y, 0f, 0f, 0f, 0f);
            }

            if (o.Kind == DreamForestObjectKind.Branch)
            {
                float angle = (time * (float)Math.PI * 2f) / o.Period + o.Phase;
                return new DreamForestObjectFrame(
                    o,
                    o.X + (float)Math.Sin(angle) * o.Amplitude,
                    o.Y + (float)Math.Sin(angle * 0.85f) * 12f,
                    0f, 0f, 0f, 0f);
            }

            if (o.Flight != null)
            {
                float? started = null;
                if (rides != null && rides.TryGetValue(o.Id, out var s)) started = s;
                bool active = started.HasValue;
                float age = active ? Math.Max(0f, time - started.Value) : 0f;
                float progress = active ? Clamp01((age - o.Flight.Delay) / o.Flight.Duration) : 0f;
                float q = progress * progress * progress * (progress * (progress * 6f - 15f) + 10f);
                float fx = o.X + (o.Flight.ToX - o.X) * q;
                float fy = o.Y + (o.Flight.ToY - o.Y) * q
                    + (float)Math.Sin(Math.PI * q) * o.Flight.Arc
                    + (float)Math.Sin(2.0 * Math.PI * q) * o.Flight.Wave
                    + (float)Math.Sin(time * 2f + o.Phase) * 2f;
                float energy = !active ? 0.3f : (progress >= 1f ? 0.6f : Math.Min(1f, 0.3f + age / 0.65f));
                return new DreamForestObjectFrame(o, fx, fy, 0f, 0f, progress, energy);
            }

            float ang = o.Period > 0.001f ? (time * (float)Math.PI * 2f) / o.Period + o.Phase : 0f;
            float ox = o.X + (o.Kind == DreamForestObjectKind.Grass || o.Kind == DreamForestObjectKind.Leaf ? 0f : (float)Math.Sin(ang) * o.Amplitude);
            float oy = o.Y + (o.Kind == DreamForestObjectKind.Bee ? (float)Math.Sin(ang) * 100f : 0f);
            float sway = o.Kind == DreamForestObjectKind.Grass
                ? (float)Math.Sin(time * 0.9f + o.Phase) * o.Amplitude + (float)Math.Sin(time * 0.37f) * 15f
                : 0f;
            return new DreamForestObjectFrame(o, ox, oy, sway, 0f, 0f, 0f);
        }

        public static float GrassX(DreamForestObjectFrame frame, float y)
        {
            float u = Clamp01((y - frame.Y) / Math.Max(1f, frame.H));
            return frame.X + frame.Sway * (1f - u) * (1f - u);
        }

        public static float SurfaceY(DreamForestObjectFrame frame, float x)
        {
            float u = Clamp01((x - frame.X) / Math.Max(1f, frame.W));
            if (frame.Spec.Kind == DreamForestObjectKind.Plateau) return frame.Y;
            if (frame.Spec.Kind == DreamForestObjectKind.Branch) return frame.Y + 12f * (float)Math.Sin(Math.PI * u) + 5f * (2f * u - 1f);
            if (frame.Spec.Kind == DreamForestObjectKind.Leaf) return frame.Y + 30f * (2f * u - 1f) * (2f * u - 1f);
            return frame.Y + frame.H * (0.12f + 0.35f * (2f * u - 1f) * (2f * u - 1f));
        }

        public static float GlideRouteY(float x)
        {
            float d = x - GlideStart;
            for (int i = 1; i < GlideKnots.Length; i++)
            {
                if (d <= GlideKnots[i].X)
                {
                    var a = GlideKnots[i - 1];
                    var b = GlideKnots[i];
                    float t = Clamp01((d - a.X) / (b.X - a.X));
                    return a.Y + (b.Y - a.Y) * t * t * (3f - 2f * t);
                }
            }
            return GlideKnots[GlideKnots.Length - 1].Y;
        }

        public DreamForestMudLakeSpec MudLakeAt(float x)
        {
            for (int i = 0; i < MudLakes.Length; i++)
            {
                if (x >= MudLakes[i].From && x <= MudLakes[i].To) return MudLakes[i];
            }
            return null;
        }

        public static float MudWaterline(DreamForestMudLakeSpec lake, float x)
        {
            float uMin = lake.Path[0].X;
            float uMax = lake.Path[lake.Path.Length - 1].X;
            float u = Math.Max(uMin, Math.Min(uMax, (x - lake.PaintX) / lake.W));
            int i = 1;
            while (i < lake.Path.Length - 1 && u > lake.Path[i].X) i++;
            var a = lake.Path[i - 1];
            var b = lake.Path[i];
            float t = (u - a.X) / Math.Max(0.0001f, b.X - a.X);
            return lake.Top + (a.Y + (b.Y - a.Y) * t) * lake.H;
        }

        public static float MudBankY(DreamForestMudLakeSpec lake, int side)
        {
            return MudWaterline(lake, side < 0 ? lake.From : lake.To) - 18f;
        }

        public float? MudGroundAt(float x)
        {
            var lake = MudLakeAt(x);
            if (lake != null) return MudWaterline(lake, x) + 185f;
            for (int i = 0; i < MudLakes.Length; i++)
            {
                var l = MudLakes[i];
                if (x >= l.From - 90f && x < l.From)
                {
                    float t = SmoothStep((x - l.From + 90f) / 90f);
                    return StealthGroundY + (MudBankY(l, -1) - StealthGroundY) * t;
                }
                if (x > l.To && x <= l.To + 90f)
                {
                    float t = SmoothStep((x - l.To) / 90f);
                    return MudBankY(l, 1) + (StealthGroundY - MudBankY(l, 1)) * t;
                }
            }
            return null;
        }

        public float StealthGroundYAt(float x)
        {
            float? wet = MudGroundAt(x);
            if (wet.HasValue) return wet.Value;

            for (int i = 0; i < StealthPlatforms.Length; i++)
            {
                var p = StealthPlatforms[i];
                if (x >= p.From && x <= p.To + 120f)
                {
                    if (x <= p.To) return p.Y;
                    float q = SmoothStep((x - p.To) / 120f);
                    return p.Y * (1f - q) + StealthGroundY * q;
                }
            }
            return StealthGroundY;
        }

        public static bool SpiderContact(DreamForestObjectFrame frame, float playerX, float playerY)
        {
            float cy = frame.SpiderY + frame.Spec.SpiderH * 0.5f;
            float dx = Math.Max(0f, Math.Abs(playerX - frame.X) - BodyRadius);
            float dy = cy - Math.Max(playerY - BodyHeight, Math.Min(playerY, cy));
            float rx = Math.Max(1f, frame.Spec.SpiderW * 0.5f);
            float ry = Math.Max(1f, frame.Spec.SpiderH * 0.5f);
            return (dx / rx) * (dx / rx) + (dy / ry) * (dy / ry) <= 1f;
        }

        public static bool WebCanGrab(DreamForestObjectFrame frame, float gripY)
        {
            return gripY >= 100f && gripY <= frame.SpiderY - 96f;
        }

        public static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
        public static float SmoothStep(float t)
        {
            float c = Clamp01(t);
            return c * c * (3f - 2f * c);
        }
        private static float CosineSmooth(float t) => (1f - (float)Math.Cos(Math.PI * Clamp01(t))) * 0.5f;
        private static float PositiveMod(float v, float m)
        {
            float r = v % m;
            return r < 0f ? r + m : r;
        }
    }
}
