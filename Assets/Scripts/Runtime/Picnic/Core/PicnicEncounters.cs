using System;
using System.Collections.Generic;

namespace YunaLegend.Picnic.Core
{
    public enum PicnicBeePhase { Patrol, Prepare, Dive }

    public sealed class PicnicBeeState
    {
        public string Id { get; }
        public string SupportId { get; }
        public float HomeX { get; }
        public float HomeY { get; }
        public float OtherX { get; }
        public float OtherY { get; }
        public int Perch { get; set; }
        public float ArcDepth { get; set; }
        public float Duration { get; set; } = 1.25f;
        public float X { get; set; }
        public float Y { get; set; }
        public float Width => 62f;
        public float Height => 32f;
        public bool Alive { get; set; } = true;
        public int Hp { get; set; } = 1;
        public PicnicBeePhase Phase { get; set; } = PicnicBeePhase.Patrol;
        public float Age { get; set; }
        public float Cooldown { get; set; } = 0.8f;
        public float DefeatAge { get; set; }
        public float FromX { get; set; }
        public float FromY { get; set; }
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public int Direction { get; set; } = -1;

        public PicnicBeeState(string id, string supportId, float homeX, float homeY, float otherX, float otherY)
        {
            Id = id;
            SupportId = supportId;
            HomeX = homeX;
            HomeY = homeY;
            OtherX = otherX;
            OtherY = otherY;
            X = homeX;
            Y = homeY;
            FromX = homeX;
            FromY = homeY;
            TargetX = homeX;
            TargetY = homeY;
        }
    }

    public sealed class PicnicWormState
    {
        public string Id { get; }
        public string SupportId { get; }
        public float X { get; }
        public float Y { get; }
        public float Width => 32f;
        public float Height { get; set; }
        public float Exposure { get; set; }
        public bool Emerging { get; set; }

        public PicnicWormState(string id, string supportId, float x, float y)
        {
            Id = id;
            SupportId = supportId;
            X = x;
            Y = y;
        }
    }

    public readonly struct PicnicBeetleSpec
    {
        public string Id { get; }
        public float X { get; }
        public string SafeBankId { get; }

        public PicnicBeetleSpec(string id, float x, string safeBankId)
        {
            Id = id;
            X = x;
            SafeBankId = safeBankId;
        }
    }

    public sealed class PicnicPanicState
    {
        public string Id;
        public float Remaining;
        public float Elapsed;
        public float X;
        public float Y;
    }

    public sealed class PicnicBeetleExitState
    {
        public float X;
        public float Y;
        public float Age;
    }

    /// <summary>
    /// 1:1 faithful C# port of gardenEncounters.js (bees, worms, beetle panics with safeBank, swept contact).
    /// </summary>
    public sealed class PicnicEncountersState
    {
        private static readonly string[] BeeHomes =
        {
            "primeiros-bichinhos",
            "trecho-0-clareira",
            "trecho-2-clareira",
            "trecho-6-clareira-relevo-1",
            "trecho-8-clareira",
        };

        private static readonly string[] WormHomes =
        {
            "pedrinhas-da-roda",
            "trecho-2-pedrinhas",
            "trecho-6-pedrinhas",
            "trecho-8-pedrinhas",
        };

        public List<string> ConsumedBeetles { get; } = new List<string>();
        public PicnicPanicState Panic { get; set; }
        public PicnicBeetleExitState Exit { get; set; }
        public PicnicBeeState[] Bees { get; }
        public PicnicWormState[] Worms { get; }
        public PicnicBeetleSpec[] Beetles { get; }

        public PicnicEncountersState(PicnicLevelData level)
        {
            Beetles = new[]
            {
                new PicnicBeetleSpec("besouro-riacho-1", level.PlatformById["trecho-0-margem"].X + 120f, "trecho-0-clareira"),
                new PicnicBeetleSpec("besouro-riacho-2", level.PlatformById["trecho-4-margem"].X + 120f, "trecho-4-clareira"),
                new PicnicBeetleSpec("besouro-nuvens-finais", level.PlatformById["nuvem-final-0"].X - 240f, null),
            };

            Bees = new PicnicBeeState[BeeHomes.Length];
            for (int i = 0; i < BeeHomes.Length; i++)
            {
                var p = level.PlatformById[BeeHomes[i]];
                float x = p.X + p.Width * 0.5f;
                float y = PicnicLevelData.SurfaceY(p, x) - 180f;
                Bees[i] = new PicnicBeeState("abelha-" + i, BeeHomes[i], x, y, Math.Max(p.X + 60f, x - 360f), y);
            }

            Worms = new PicnicWormState[WormHomes.Length];
            for (int i = 0; i < WormHomes.Length; i++)
            {
                var p = level.PlatformById[WormHomes[i]];
                float x = p.X + Math.Min(180f, p.Width * 0.65f);
                Worms[i] = new PicnicWormState("minhoca-" + i, WormHomes[i], x, PicnicLevelData.SurfaceY(p, x));
            }
        }

        public static void TickWorm(PicnicWormState worm, float playerX, float dt)
        {
            float distance = Math.Abs(playerX - worm.X);
            if (distance < 280f) worm.Emerging = true;
            else if (distance > 360f) worm.Emerging = false;
            worm.Exposure = Clamp(worm.Exposure + (worm.Emerging ? 1f : -1f) * dt / 0.22f, 0f, 1f);
            worm.Height = Math.Max(0f, worm.Exposure * 72f - 9f);
        }

        public static void TickBee(PicnicBeeState bee, PicnicLevelData level, float playerX, float playerY, float dt,
            out float oldX, out float oldY)
        {
            oldX = bee.X;
            oldY = bee.Y;
            if (!bee.Alive) return;
            bee.Age += dt;
            bee.Cooldown = Math.Max(0f, bee.Cooldown - dt);
            if (bee.Phase == PicnicBeePhase.Patrol)
            {
                if (bee.Cooldown <= 0f && Math.Abs(playerX - bee.X) < 320f && Math.Abs(playerY - bee.Y) < 330f)
                {
                    bee.Phase = PicnicBeePhase.Prepare;
                    bee.Age = 0f;
                    bee.Direction = (bee.Perch == 0 ? bee.OtherX : bee.HomeX) < bee.X ? -1 : 1;
                }
            }
            else if (bee.Phase == PicnicBeePhase.Prepare)
            {
                if (bee.Age >= 0.5f)
                {
                    bee.Phase = PicnicBeePhase.Dive;
                    bee.Age = 0f;
                    bee.FromX = bee.X;
                    bee.FromY = bee.Y;
                    bee.TargetX = bee.Perch == 0 ? bee.OtherX : bee.HomeX;
                    bee.TargetY = bee.Perch == 0 ? bee.OtherY : bee.HomeY;
                    bee.Duration = 1.25f;
                    var ground = level.PlatformById[bee.SupportId];
                    float middle = (bee.FromX + bee.TargetX) * 0.5f;
                    bee.ArcDepth = Math.Max(0f, PicnicLevelData.SurfaceY(ground, middle) - 12f - (bee.FromY + bee.TargetY) * 0.5f);
                }
            }
            else if (bee.Phase == PicnicBeePhase.Dive)
            {
                float t = Clamp(bee.Age / bee.Duration, 0f, 1f);
                bee.X = bee.FromX + (bee.TargetX - bee.FromX) * t;
                bee.Y = bee.FromY + (bee.TargetY - bee.FromY) * t + 4f * t * (1f - t) * bee.ArcDepth;
                if (t >= 1f)
                {
                    bee.X = bee.TargetX;
                    bee.Y = bee.TargetY;
                    bee.Perch = 1 - bee.Perch;
                    bee.Phase = PicnicBeePhase.Patrol;
                    bee.Age = 0f;
                    bee.Cooldown = 1.8f;
                }
            }
        }

        public void TickBeetles(PicnicLevelData level, float playerX, float playerY, bool onGround,
            string supportId, float feetLeft, float time, float dt, Action<string> emit)
        {
            if (Exit != null)
            {
                Exit.Age += dt;
                Exit.X -= 240f * dt;
                Exit.Y -= 170f * dt;
                if (Exit.Age >= 1f) Exit = null;
            }
            if (Panic != null)
            {
                Panic.Elapsed += dt;
                Panic.Remaining = Math.Max(0f, 6f - Panic.Elapsed);
                float mix = 1f - (float)Math.Exp(-dt * 8f);
                Panic.X += (playerX - 135f - Panic.X) * mix;
                Panic.Y += (playerY - 70f + (float)Math.Sin(time * 16f) * 6f - Panic.Y) * mix;
                PicnicBeetleSpec def = default;
                foreach (var b in Beetles) { if (b.Id == Panic.Id) { def = b; break; } }
                bool finished;
                if (!string.IsNullOrEmpty(def.SafeBankId) && level.PlatformById.TryGetValue(def.SafeBankId, out var bank))
                {
                    bool groundedOnGround = onGround && supportId != null &&
                        level.PlatformById.TryGetValue(supportId, out var sup) && sup.Ground;
                    finished = groundedOnGround && feetLeft >= bank.X;
                }
                else
                {
                    finished = Panic.Remaining <= 1e-6f;
                }
                if (finished)
                {
                    Exit = new PicnicBeetleExitState { X = Panic.X, Y = Panic.Y, Age = 0f };
                    emit?.Invoke("calm:" + Panic.Id);
                    Panic = null;
                }
                return;
            }

            if (!onGround) return;
            foreach (var b in Beetles)
            {
                if (!ConsumedBeetles.Contains(b.Id) && playerX >= b.X && playerX < b.X + 220f)
                {
                    ConsumedBeetles.Add(b.Id);
                    Exit = null;
                    Panic = new PicnicPanicState
                    {
                        Id = b.Id,
                        Remaining = 6f,
                        Elapsed = 0f,
                        X = playerX - 160f,
                        Y = playerY - 70f,
                    };
                    emit?.Invoke("panic:" + b.Id);
                    return;
                }
            }
        }

        public void ResetPanic(float targetX)
        {
            Panic = null;
            Exit = null;
            ConsumedBeetles.RemoveAll(id =>
            {
                foreach (var b in Beetles)
                {
                    if (b.Id == id) return b.X >= targetX - 220f;
                }
                return false;
            });
            foreach (var bee in Bees)
            {
                if (!bee.Alive) continue;
                bee.X = bee.HomeX;
                bee.Y = bee.HomeY;
                bee.Perch = 0;
                bee.Phase = PicnicBeePhase.Patrol;
                bee.Age = 0f;
                bee.Cooldown = 1f;
                bee.ArcDepth = 0f;
                bee.Duration = 1.25f;
            }
        }

        public static bool SweptContact(float prevPx, float prevPy, float px, float py,
            float oldEx, float oldEy, float ex, float ey, float ew, float eh)
        {
            float ax = prevPx - oldEx;
            float ay = prevPy - oldEy;
            float bx = px - ex;
            float by = py - ey;
            float minX = -ew * 0.5f - 16f;
            float maxX = ew * 0.5f + 16f;
            float minY = -eh + 8f;
            float maxY = 86f;

            float enter = 0f;
            float leave = 1f;
            if (!ClipAxis(ax, bx, minX, maxX, ref enter, ref leave)) return false;
            if (!ClipAxis(ay, by, minY, maxY, ref enter, ref leave)) return false;
            return enter <= leave;
        }

        private static bool ClipAxis(float a, float b, float min, float max, ref float enter, ref float leave)
        {
            float d = b - a;
            if (Math.Abs(d) < 1e-7f)
            {
                return a >= min && a <= max;
            }
            float t0 = (min - a) / d;
            float t1 = (max - a) / d;
            enter = Math.Max(enter, Math.Min(t0, t1));
            leave = Math.Min(leave, Math.Max(t0, t1));
            return enter <= leave;
        }

        private static float Clamp(float v, float a, float b) => Math.Max(a, Math.Min(b, v));
    }
}
