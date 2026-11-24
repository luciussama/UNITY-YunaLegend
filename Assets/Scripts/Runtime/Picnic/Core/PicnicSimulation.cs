using System;
using System.Collections.Generic;

namespace YunaLegend.Picnic.Core
{
    public readonly struct PicnicInput
    {
        public float Axis { get; }
        public bool Jump { get; }
        public bool Advance { get; }
        public bool Skip { get; }

        public PicnicInput(float axis, bool jump, bool advance = false, bool skip = false)
        {
            Axis = axis;
            Jump = jump;
            Advance = advance;
            Skip = skip;
        }
    }

    public sealed class PicnicEnemyRuntime
    {
        public PicnicEnemySpec Spec { get; }
        public float X { get; set; }
        public float Y { get; set; }
        public float BaseY { get; set; }
        public int Hp { get; set; }
        public int Direction { get; set; } = -1;
        public bool Alive { get; set; } = true;
        public float DefeatAge { get; set; }
        public float StompLock { get; set; }
        public float HopAge { get; set; }
        public int RouteIndex { get; set; }
        public string AttackPhase { get; set; } = "patrol";
        public float AttackAge { get; set; }
        public int RetreatDirection { get; set; } = 1;

        public PicnicEnemyRuntime(PicnicEnemySpec spec)
        {
            Spec = spec;
            X = spec.X;
            Y = spec.Y;
            BaseY = spec.BaseY;
            Hp = spec.Kind == PicnicEnemyKind.Caterpillar ? 2 : 1;
        }
    }

    public sealed class PicnicRecoveryState
    {
        public float FromX;
        public float FromY;
        public float ToX;
        public float ToY;
        public string ToSupportId;
        public string Kind;
    }

    /// <summary>
    /// 1:1 faithful C# port of legacy gardenSimulation.js (engine-free pure domain simulation).
    /// </summary>
    public sealed class PicnicSimulation
    {
        private static readonly float[] GravelJumpVariations = { 0.88f, 1.10f, 0.96f, 1.16f };
        private static readonly float[] GravelAirVariations = { 1.04f, 0.90f, 1.10f, 0.95f };

        public PicnicLevelData Level { get; }
        public PicnicDifficulty Difficulty { get; set; }
        public PicnicStage Stage { get; private set; } = PicnicStage.Intro;

        public int OpeningIndex { get; private set; }
        public int EndingIndex { get; private set; }
        public bool OpeningDone { get; private set; }
        public bool EndingDone { get; private set; }
        public bool SeenOpening { get; private set; }
        public bool SeenEnding { get; private set; }
        public float CardAge { get; private set; }
        private bool storyHeld;

        public float Age { get; private set; }
        public float Time { get; private set; }
        public int Hp { get; private set; }
        public float Checkpoint { get; private set; } = 120f;
        public int Defeated { get; private set; }
        public float Immunity { get; private set; }
        public int Jumps { get; private set; }
        public int Falls { get; private set; }

        public float HurtRemaining { get; private set; }
        public int HurtDirection { get; private set; } = 1;
        public bool BeetleHintSeen { get; private set; }
        public float HintRemaining { get; private set; }

        public float PlayerX { get; private set; }
        public float PlayerY { get; private set; }
        public float PlayerVx { get; private set; }
        public float PlayerVy { get; private set; }
        public int PlayerFacing { get; private set; } = 1;
        public bool OnGround { get; private set; } = true;
        public string SupportId { get; private set; } = "entrada";
        public PicnicTerrain Terrain { get; private set; } = PicnicTerrain.Wood;
        public float AirSpeed { get; private set; } = PicnicLevelData.DefaultSpeed;
        public float Discomfort { get; private set; }
        public PicnicPoseKey? BalancePose { get; private set; }

        public float FairyX { get; private set; } = 120f;
        public float FairyY { get; private set; } = PicnicLevelData.GroundY - 135f;
        public string FairyMode { get; private set; } = "follow";
        private float fairyIdle;
        private float fairyProgressX = 120f;
        private float fairyGuideAge;
        private string fairyTargetId;

        public PicnicEncountersState Encounters { get; private set; }
        public PicnicEnemyRuntime[] Enemies { get; private set; }
        public Dictionary<string, float> Clouds { get; } = new Dictionary<string, float>();
        public PicnicRecoveryState Recovery { get; private set; }

        private readonly List<string> trail = new List<string> { "entrada" };
        private string safeGround = "entrada";
        private bool jumpHeld;
        private float jumpBuffer;
        private float coyote;

        public bool IsReadingStory => (Stage == PicnicStage.Intro && !OpeningDone) || (Stage == PicnicStage.Complete && !EndingDone);
        public bool IsPanic => Encounters.Panic != null;
        public event Action<string> Event;

        public PicnicSimulation(bool easyMode = false, PicnicLevelData level = null)
        {
            Level = level ?? PicnicLevelData.Default;
            Difficulty = easyMode ? PicnicDifficulty.Easy : PicnicDifficulty.Normal;
            ResetFreshState();
        }

        private void ResetFreshState()
        {
            Stage = PicnicStage.Intro;
            OpeningIndex = 0;
            EndingIndex = 0;
            OpeningDone = false;
            EndingDone = false;
            CardAge = 0f;
            Age = 0f;
            Time = 0f;
            Hp = Difficulty == PicnicDifficulty.Easy ? 6 : 3;
            Checkpoint = 120f;
            Defeated = 0;
            Immunity = 0f;
            Jumps = 0;
            HurtRemaining = 0f;
            HurtDirection = 1;
            PlayerX = PicnicLevelData.HomeSteps[0].X;
            PlayerY = PicnicLevelData.HomeSteps[0].Y;
            PlayerVx = 0f;
            PlayerVy = 0f;
            PlayerFacing = 1;
            OnGround = true;
            SupportId = "entrada";
            Terrain = PicnicTerrain.Wood;
            AirSpeed = PicnicLevelData.DefaultSpeed;
            Discomfort = 0f;
            BalancePose = null;
            FairyX = 120f;
            FairyY = PicnicLevelData.GroundY - 135f;
            FairyMode = "follow";
            fairyIdle = 0f;
            fairyProgressX = 120f;
            fairyGuideAge = 0f;
            fairyTargetId = null;
            Clouds.Clear();
            trail.Clear();
            trail.Add("entrada");
            safeGround = "entrada";
            Recovery = null;
            jumpHeld = false;
            jumpBuffer = 0f;
            coyote = 0f;
            Encounters = new PicnicEncountersState(Level);
            Enemies = new PicnicEnemyRuntime[Level.Enemies.Count];
            for (int i = 0; i < Level.Enemies.Count; i++)
            {
                Enemies[i] = new PicnicEnemyRuntime(Level.Enemies[i]);
            }
        }

        public void AdvanceStory()
        {
            if (!IsReadingStory) return;
            bool intro = Stage == PicnicStage.Intro;
            var cards = intro ? PicnicNarrativeData.Opening : PicnicNarrativeData.Ending;
            int idx = intro ? OpeningIndex : EndingIndex;
            if (idx >= cards.Length - 1)
            {
                FinishStory(intro);
            }
            else
            {
                if (intro) OpeningIndex++; else EndingIndex++;
                CardAge = 0f;
            }
        }

        public void SkipStory()
        {
            if (Stage == PicnicStage.Intro && !OpeningDone)
            {
                FinishStory(true);
                // Allow skipping directly to running on stairs
                Stage = PicnicStage.Running;
                Age = 0f;
                PlayerX = PicnicLevelData.HomeSteps[3].X;
                PlayerY = PicnicLevelData.HomeSteps[3].Y;
                PlayerVx = 0f;
                Event?.Invoke("started");
            }
            else if (Stage == PicnicStage.Complete && !EndingDone)
            {
                FinishStory(false);
            }
        }

        private void FinishStory(bool intro)
        {
            if (intro)
            {
                OpeningIndex = PicnicNarrativeData.Opening.Length - 1;
                OpeningDone = true;
                SeenOpening = true;
                Age = 0f;
                Event?.Invoke("scene-seen:opening");
            }
            else
            {
                EndingIndex = PicnicNarrativeData.Ending.Length - 1;
                EndingDone = true;
                SeenEnding = true;
                Event?.Invoke("scene-seen:ending");
            }
        }

        public bool Retry()
        {
            if (Stage != PicnicStage.GameOver) return false;
            Stage = PicnicStage.Running;
            Age = 0f;
            Hp = Difficulty == PicnicDifficulty.Easy ? 6 : 3;
            HurtRemaining = 0f;
            Clouds.Clear();
            Recovery = null;
            jumpHeld = false;
            jumpBuffer = 0f;
            coyote = 0f;
            BalancePose = null;

            PicnicPlatformSpec ground = null;
            foreach (var p in Level.StaticPlatforms)
            {
                if (p.Ground && Checkpoint >= p.X && Checkpoint <= p.X + p.Width) { ground = p; break; }
            }
            PlayerX = Checkpoint;
            PlayerY = ground != null
                ? PicnicContact.ContactHeight(ground, Checkpoint, PlayerFacing, ground.Terrain, 0f)
                : PicnicLevelData.GroundY;
            if (!float.IsFinite(PlayerY)) PlayerY = PicnicLevelData.GroundY;
            PlayerVx = 0f;
            PlayerVy = 0f;
            OnGround = true;
            SupportId = ground?.Id ?? "entrada";
            Terrain = ground?.Terrain ?? PicnicTerrain.Wood;
            safeGround = SupportId;
            trail.Clear();
            trail.Add(SupportId);
            Immunity = 3f;
            FairyX = Checkpoint;
            fairyProgressX = Checkpoint;
            Encounters.ResetPanic(Checkpoint);
            Event?.Invoke("retry");
            return true;
        }

        public void Step(float seconds, PicnicInput input)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds <= 0f) return;
            float remaining = Math.Min(0.2f, seconds);
            while (remaining > 1e-6f)
            {
                float dt = Math.Min(PicnicLevelData.FixedStep, remaining);
                Tick(dt, input);
                remaining -= dt;
            }
        }

        private void Tick(float dt, PicnicInput input)
        {
            Time += dt;
            Age += dt;
            HurtRemaining = Math.Max(0f, HurtRemaining - dt);

            foreach (var e in Enemies)
            {
                if (!e.Alive) e.DefeatAge += dt;
            }
            foreach (var b in Encounters.Bees)
            {
                if (!b.Alive) b.DefeatAge += dt;
            }

            if (Stage == PicnicStage.Running)
            {
                HintRemaining = Math.Max(0f, HintRemaining - dt);
                if (!BeetleHintSeen)
                {
                    foreach (var b in Encounters.Beetles)
                    {
                        if (!Encounters.ConsumedBeetles.Contains(b.Id) && b.X >= PlayerX && b.X - PlayerX < 780f)
                        {
                            BeetleHintSeen = true;
                            HintRemaining = 6f;
                            break;
                        }
                    }
                }
            }

            bool advance = input.Advance && !storyHeld;
            storyHeld = input.Advance;

            if (IsReadingStory)
            {
                bool intro = Stage == PicnicStage.Intro;
                var cards = intro ? PicnicNarrativeData.Opening : PicnicNarrativeData.Ending;
                int idx = intro ? OpeningIndex : EndingIndex;
                CardAge += dt;
                if (advance || CardAge >= cards[idx].Duration || input.Skip)
                {
                    if (input.Skip)
                    {
                        SkipStory();
                    }
                    else
                    {
                        AdvanceStory();
                    }
                }
                TickFairy(dt, Array.Empty<PicnicPlatformSpec>());
                return;
            }

            // Legacy 3-second walk down the painted wooden stairs of GARDEN_HOME into RUNNING
            if (Stage == PicnicStage.Intro)
            {
                float t = Clamp(Age / 3f, 0f, 1f) * 3f;
                int index = Math.Min(2, (int)Math.Floor(t));
                float mix = t - index;
                var a = PicnicLevelData.HomeSteps[index];
                var b = PicnicLevelData.HomeSteps[index + 1];
                PlayerX = a.X + (b.X - a.X) * mix;
                PlayerY = a.Y + (b.Y - a.Y) * mix;
                PlayerVx = b.X - a.X;
                PlayerFacing = 1;
                if (Age >= 3f)
                {
                    Stage = PicnicStage.Running;
                    Age = 0f;
                    PlayerVx = 0f;
                    jumpHeld = input.Jump;
                    Event?.Invoke("started");
                }
                TickFairy(dt, Array.Empty<PicnicPlatformSpec>());
                return;
            }

            if (Stage == PicnicStage.Falling || Stage == PicnicStage.GameOver)
            {
                foreach (var e in Enemies)
                {
                    if (Math.Abs(e.X - PlayerX) < 1100f) TickEnemy(e, dt, out _, out _);
                }
                if (Stage == PicnicStage.Falling)
                {
                    PlayerVy = Math.Min(1500f, PlayerVy + PicnicLevelData.Gravity * dt);
                    PlayerY += PlayerVy * dt;
                    PlayerX = Clamp(PlayerX + PlayerVx * dt, 22f, Level.Width - 22f);
                    if (Age >= 2.4f)
                    {
                        Stage = PicnicStage.GameOver;
                        Age = 0f;
                        Event?.Invoke("gameover");
                    }
                }
                return;
            }

            if (Stage == PicnicStage.Rescuing || Stage == PicnicStage.Disoriented)
            {
                var r = Recovery;
                float t = Clamp(Age / 1.6f, 0f, 1f);
                float mix = t * t * (3f - 2f * t);
                PlayerX = r.FromX + (r.ToX - r.FromX) * mix;
                PlayerY = r.FromY + (r.ToY - r.FromY) * mix - (float)Math.Sin(t * Math.PI) * 130f;
                FairyX = PlayerX;
                FairyY = PlayerY - 125f;
                if (t >= 1f)
                {
                    PlayerX = r.ToX;
                    PlayerY = r.ToY;
                    PlayerVx = 0f;
                    PlayerVy = 0f;
                    OnGround = true;
                    SupportId = r.ToSupportId;
                    Terrain = Level.PlatformById[r.ToSupportId].Terrain;
                    Stage = PicnicStage.Running;
                    Age = 0f;
                    Recovery = null;
                    Immunity = 4f;
                    Hp = 6;
                    FairyMode = "follow";
                    fairyIdle = 0f;
                    fairyProgressX = PlayerX;
                    jumpHeld = input.Jump;
                    Event?.Invoke("rescue_done");
                }
                return;
            }

            if (Stage != PicnicStage.Running)
            {
                TickFairy(dt, Array.Empty<PicnicPlatformSpec>());
                return;
            }

            // Expire respawned clouds
            float cloudDuration = Difficulty == PicnicDifficulty.Easy ? 3.8f : 2.8f;
            var expiredClouds = new List<string>();
            foreach (var kv in Clouds)
            {
                if (Time - kv.Value >= cloudDuration + 4f) expiredClouds.Add(kv.Key);
            }
            for (int i = 0; i < expiredClouds.Count; i++) Clouds.Remove(expiredClouds[i]);

            var oldPlatforms = Level.QueryPlatforms(Time - dt, Clouds, Difficulty, PlayerX, 650f);
            var platforms = Level.QueryPlatforms(Time, Clouds, Difficulty, PlayerX, 650f);

            bool pressed = input.Jump && !jumpHeld;
            bool released = !input.Jump && jumpHeld;
            jumpHeld = input.Jump;
            jumpBuffer = pressed ? 0.14f : Math.Max(0f, jumpBuffer - dt);
            coyote = OnGround ? 0.12f : Math.Max(0f, coyote - dt);
            Immunity = Math.Max(0f, Immunity - dt);

            var support = FindPlatform(platforms, SupportId);
            var fromSupport = FindPlatform(oldPlatforms, SupportId);
            if (OnGround && support == null)
            {
                OnGround = false;
                coyote = 0f;
            }
            if (OnGround && support != null)
            {
                Terrain = PicnicLevelData.TerrainAt(support, PlayerX);
                if (fromSupport != null)
                {
                    PlayerX += support.X - fromSupport.X;
                    PlayerY += support.Y - fromSupport.Y;
                }
                float top = PicnicContact.ContactHeight(support, PlayerX, PlayerFacing, Terrain, Discomfort);
                if (float.IsFinite(top)) PlayerY = top;
            }

            var feet = PicnicContact.GetFeet(PlayerX, PlayerFacing, Terrain, Discomfort);
            Encounters.TickBeetles(Level, PlayerX, PlayerY, OnGround, SupportId, feet.Left, Time, dt, e => Event?.Invoke(e));

            float prevX = PlayerX;
            float prevY = PlayerY;

            if (jumpBuffer > 0f && coyote > 0f)
            {
                bool gravel = Terrain == PicnicTerrain.Gravel;
                float variation = GravelJumpVariations[Jumps % 4];
                PlayerVy = -PicnicLevelData.JumpSpeed * (gravel ? variation : 1f);
                AirSpeed = gravel
                    ? 410f * GravelAirVariations[Jumps % 4]
                    : (Terrain == PicnicTerrain.Grass ? PicnicLevelData.DefaultSpeed : PicnicLevelData.TerrainSpeed(Terrain));
                Jumps++;
                BalancePose = null;
                OnGround = false;
                SupportId = null;
                jumpBuffer = 0f;
                coyote = 0f;
                Event?.Invoke("jump");
            }

            if (released && PlayerVy < 0f) PlayerVy *= 0.55f;

            float axis = IsPanic ? 1f : Clamp(input.Axis, -1f, 1f);
            float speed = OnGround ? PicnicLevelData.TerrainSpeed(Terrain) : AirSpeed;
            float desired = axis * speed;
            if (Math.Abs(axis) < 0.001f || Terrain == PicnicTerrain.Gravel) PlayerVx = desired;
            else PlayerVx += Clamp(desired - PlayerVx, -2400f * dt, 2400f * dt);

            if (Math.Abs(axis) > 0.1f) PlayerFacing = axis < 0f ? -1 : 1;
            float targetDiscomfort = (Terrain == PicnicTerrain.Grass || Terrain == PicnicTerrain.Gravel) ? 1f : 0f;
            Discomfort += (targetDiscomfort - Discomfort) * Math.Min(1f, dt * 8f);

            PlayerX = Clamp(PlayerX + PlayerVx * dt, 22f, Level.Width - 22f);
            PlayerVy = Math.Min(1800f, PlayerVy + PicnicLevelData.Gravity * dt);

            bool walking = OnGround && support != null && PlayerVy >= 0f;
            float walkingHeight = walking ? PicnicContact.ContactHeight(support, PlayerX, PlayerFacing, Terrain, Discomfort) : float.PositiveInfinity;
            float nextY = ((walking && float.IsFinite(walkingHeight) && Math.Abs(walkingHeight - PlayerY) < 24f) ? walkingHeight : PlayerY) + PlayerVy * dt;

            // Side wall collision against solid ground step-ups
            for (int i = 0; i < platforms.Count; i++)
            {
                var wall = platforms[i];
                if (!wall.Ground) continue;
                float dx = PlayerX - prevX;
                if (Math.Abs(dx) < 1e-6f) continue;
                float edge = dx > 0f ? wall.X - PicnicLevelData.HalfFoot : wall.X + wall.Width + PicnicLevelData.HalfFoot;
                float fraction = (edge - prevX) / dx;
                if (fraction < 0f || fraction > 1f) continue;
                if (prevY + (nextY - prevY) * fraction <= wall.Y) continue;
                PlayerX = edge;
                PlayerVx = 0f;
            }

            PlayerY = nextY;
            OnGround = false;
            SupportId = null;

            if (PlayerVy >= 0f)
            {
                PicnicPlatformSpec bestLand = null;
                float bestFraction = float.PositiveInfinity;
                float bestHeight = float.PositiveInfinity;

                for (int i = 0; i < platforms.Count; i++)
                {
                    var v = platforms[i];
                    var old = FindPlatform(oldPlatforms, v.Id) ?? v;
                    float height = PicnicContact.ContactHeight(v, PlayerX, PlayerFacing, Terrain, Discomfort);
                    if (!float.IsFinite(height)) continue;

                    if (walking && (support.Id == v.Id || (support.Ground && v.Ground)) && Math.Abs(height - prevY) < 24f)
                    {
                        if (0f < bestFraction || (bestFraction == 0f && height < bestHeight))
                        {
                            bestLand = v; bestFraction = 0f; bestHeight = height;
                        }
                        continue;
                    }

                    float oldHeight = PicnicContact.ContactHeight(old, prevX, PlayerFacing, Terrain, Discomfort);
                    if (!float.IsFinite(oldHeight)) oldHeight = PicnicContact.ContactHeight(old, PlayerX, PlayerFacing, Terrain, Discomfort);
                    if (!float.IsFinite(oldHeight)) oldHeight = old.Y + height - v.Y;

                    float oldRel = prevY - oldHeight;
                    float newRel = PlayerY - height;
                    if (oldRel > 2f || newRel < 0f) continue;

                    float frac = Clamp(-oldRel / Math.Max(0.0001f, newRel - oldRel), 0f, 1f);
                    if (frac < bestFraction || (Math.Abs(frac - bestFraction) < 1e-5f && height < bestHeight))
                    {
                        bestLand = v; bestFraction = frac; bestHeight = height;
                    }
                }

                if (bestLand != null)
                {
                    BalancePose = null;
                    PlayerY = bestHeight;
                    PlayerVy = 0f;
                    OnGround = true;
                    SupportId = bestLand.Id;
                    Terrain = PicnicLevelData.TerrainAt(bestLand, PlayerX);
                    if (bestLand.Terrain == PicnicTerrain.Cloud && !Clouds.ContainsKey(bestLand.Id))
                    {
                        Clouds[bestLand.Id] = Time;
                        Event?.Invoke("cloud_armed:" + bestLand.Id);
                    }
                    if (!bestLand.Rotating && bestLand.Terrain != PicnicTerrain.Cloud &&
                        (trail.Count == 0 || trail[trail.Count - 1] != bestLand.Id))
                    {
                        trail.Add(bestLand.Id);
                        if (trail.Count > 8) trail.RemoveAt(0);
                    }
                    if (bestLand.Ground) safeGround = bestLand.Id;
                }
            }

            // Ground & river enemies
            foreach (var e in Enemies)
            {
                if (Math.Abs(e.X - PlayerX) > 1100f) continue;
                TickEnemy(e, dt, out float oldEx, out float oldEy);
                if (!e.Alive) continue;
                float top = e.Y - e.Spec.Height;
                float oldTop = oldEy - e.Spec.Height;
                float oldRel = prevY - oldTop;
                float newRel = PlayerY - top;
                if (e.StompLock <= 0f && oldRel <= 5f && newRel >= 0f && newRel > oldRel && PlayerVy >= 0f)
                {
                    float frac = Clamp(-oldRel / Math.Max(0.001f, newRel - oldRel), 0f, 1f);
                    float cx = prevX + (PlayerX - prevX) * frac;
                    float ex = oldEx + (e.X - oldEx) * frac;
                    if (Math.Abs(cx - ex) < e.Spec.Width * 0.5f + PicnicLevelData.HalfFoot)
                    {
                        e.Hp--;
                        e.StompLock = 0.24f;
                        e.Alive = e.Hp > 0;
                        e.DefeatAge = 0f;
                        if (!e.Alive) Defeated++;
                        else
                        {
                            GetEnemyRange(e, out float rLeft, out float rRight);
                            int preferred = Math.Abs(e.X - PlayerX) > 8f ? Math.Sign(e.X - PlayerX) : PlayerFacing;
                            e.AttackPhase = "retreat";
                            e.AttackAge = 0f;
                            e.RetreatDirection = (preferred > 0 ? rRight - e.X : e.X - rLeft) > 190f ? preferred : -preferred;
                            e.Direction = e.RetreatDirection;
                        }
                        PlayerY = top;
                        PlayerVy = -480f;
                        OnGround = false;
                        SupportId = null;
                        coyote = 0f;
                        Event?.Invoke(e.Alive ? "enrage:" + e.Spec.Id : "stomp:" + e.Spec.Id);
                        continue;
                    }
                }
                if (Immunity <= 0f && Math.Abs(PlayerX - e.X) < e.Spec.Width * 0.5f + 16f &&
                    PlayerY > top + 8f && PlayerY - PicnicLevelData.PlayerHeight < e.Y - 6f)
                {
                    if (HurtPlayer(e.X, e.Spec.Id)) return;
                }
            }

            // Bees
            foreach (var bee in Encounters.Bees)
            {
                if (Math.Abs(bee.HomeX - PlayerX) > 1000f) continue;
                PicnicEncountersState.TickBee(bee, Level, PlayerX, PlayerY, dt, out float oldBx, out float oldBy);
                if (!bee.Alive) continue;
                float top = bee.Y - bee.Height + 3f;
                float oldRel = prevY - (oldBy - bee.Height + 3f);
                float newRel = PlayerY - top;
                if (PlayerVy >= 0f && oldRel <= 5f && newRel >= 0f && newRel > oldRel)
                {
                    float frac = Clamp(-oldRel / Math.Max(0.001f, newRel - oldRel), 0f, 1f);
                    float cx = prevX + (PlayerX - prevX) * frac;
                    float bx = oldBx + (bee.X - oldBx) * frac;
                    if (Math.Abs(cx - bx) < bee.Width * 0.5f + PicnicLevelData.HalfFoot)
                    {
                        bee.Alive = false;
                        bee.Hp = 0;
                        bee.DefeatAge = 0f;
                        Defeated++;
                        PlayerY = top;
                        PlayerVy = -480f;
                        OnGround = false;
                        SupportId = null;
                        coyote = 0f;
                        Event?.Invoke("stomp:" + bee.Id);
                        continue;
                    }
                }
                if (Immunity <= 0f && PicnicEncountersState.SweptContact(prevX, prevY, PlayerX, PlayerY, oldBx, oldBy, bee.X, bee.Y, bee.Width, bee.Height))
                {
                    if (HurtPlayer(bee.X, bee.Id)) return;
                }
            }

            // Worms (indestructible)
            foreach (var worm in Encounters.Worms)
            {
                PicnicEncountersState.TickWorm(worm, PlayerX, dt);
                if (worm.Height > 4f && Immunity <= 0f &&
                    PicnicEncountersState.SweptContact(prevX, prevY, PlayerX, PlayerY, worm.X, worm.Y, worm.X, worm.Y, worm.Width, worm.Height))
                {
                    if (HurtPlayer(worm.X, worm.Id)) return;
                }
            }

            // Water contact or fall into void
            bool inWater = false;
            if (PlayerY > PicnicLevelData.WaterY)
            {
                for (int i = 0; i < Level.Waters.Count; i++)
                {
                    var w = Level.Waters[i];
                    if (PlayerX > w.X && PlayerX < w.X + w.Width) { inWater = true; break; }
                }
            }
            if (inWater || PlayerY > PicnicLevelData.Height + 100f)
            {
                Falls++;
                Defeat("fall");
                return;
            }

            // Checkpoints
            if (OnGround)
            {
                float reached = Checkpoint;
                for (int i = 0; i < Level.Checkpoints.Count; i++)
                {
                    if (PlayerX >= Level.Checkpoints[i]) reached = Level.Checkpoints[i];
                }
                if (reached > Checkpoint)
                {
                    Checkpoint = reached;
                    Hp = Difficulty == PicnicDifficulty.Easy ? 6 : 3;
                    Event?.Invoke("checkpoint:" + reached);
                }
            }

            TickFairy(dt, platforms);

            // Goal reached -> Ending cinematic
            if (OnGround && PlayerX >= Level.Goal)
            {
                Encounters.Panic = null;
                Encounters.Exit = null;
                Stage = PicnicStage.Complete;
                Age = 0f;
                EndingIndex = 0;
                EndingDone = false;
                CardAge = 0f;
                PlayerVx = 0f;
                Event?.Invoke("complete");
            }
        }

        private bool HurtPlayer(float sourceX, string sourceId)
        {
            Hp--;
            Immunity = 2f;
            PlayerVy = -280f;
            OnGround = false;
            SupportId = null;
            HurtRemaining = 0.48f;
            HurtDirection = Math.Abs(PlayerX - sourceX) > 0.01f ? Math.Sign(PlayerX - sourceX) : -PlayerFacing;
            Event?.Invoke("hurt:" + sourceId);
            if (Hp <= 0)
            {
                Defeat("hurt");
                return true;
            }
            return false;
        }

        private void Defeat(string kind)
        {
            if (Difficulty == PicnicDifficulty.Easy)
            {
                BeginRecovery(kind);
                return;
            }
            Encounters.ResetPanic(Checkpoint);
            Hp = 0;
            Stage = PicnicStage.Falling;
            Age = 0f;
            OnGround = false;
            SupportId = null;
            PlayerVy = kind == "hurt" ? -260f : Math.Max(120f, PlayerVy);
            PlayerVx = kind == "hurt" ? -PlayerFacing * 80f : 0f;
            if (kind != "hurt") Event?.Invoke(kind);
        }

        private void BeginRecovery(string kind)
        {
            var ground = Level.PlatformById.TryGetValue(safeGround, out var sg) ? sg : Level.PlatformById["entrada"];
            float targetX = Clamp(PlayerX, ground.X + 60f, ground.X + ground.Width - 60f);
            float targetY = PicnicLevelData.SurfaceY(ground, targetX);
            if (Encounters.Panic != null)
            {
                foreach (var b in Encounters.Beetles)
                {
                    if (b.Id == Encounters.Panic.Id)
                    {
                        float beforeBeetle = Clamp(b.X - 80f, ground.X + 60f, ground.X + ground.Width - 60f);
                        if (beforeBeetle < targetX)
                        {
                            targetX = beforeBeetle;
                            targetY = PicnicLevelData.SurfaceY(ground, targetX);
                        }
                        break;
                    }
                }
            }
            Recovery = new PicnicRecoveryState
            {
                FromX = PlayerX,
                FromY = Math.Min(PlayerY, 730f),
                ToX = targetX,
                ToY = targetY,
                ToSupportId = ground.Id,
                Kind = kind,
            };
            Encounters.ResetPanic(targetX);
            Stage = kind == "hurt" ? PicnicStage.Disoriented : PicnicStage.Rescuing;
            Age = 0f;
            FairyMode = "rescue";
            fairyTargetId = null;
            Hp = 6;
            jumpBuffer = 0f;
            coyote = 0f;
            PlayerVx = 0f;
            PlayerVy = 0f;
            Event?.Invoke(kind == "hurt" ? "dazed" : "rescue");
        }

        private void TickEnemy(PicnicEnemyRuntime e, float dt, out float oldX, out float oldY)
        {
            oldX = e.X;
            oldY = e.Y;
            e.StompLock = Math.Max(0f, e.StompLock - dt);
            if (!e.Alive) return;
            e.HopAge += dt;

            if (e.Spec.Route != null && e.Spec.Route.Length >= 2)
            {
                float period = e.Spec.Period * 0.85f * (Difficulty == PicnicDifficulty.Easy ? 1.2f : 1f);
                float a = ((e.HopAge + e.Spec.Phase) % period) / period;
                int count = e.Spec.Route.Length;
                int cycle = (int)Math.Floor((e.HopAge + e.Spec.Phase) / period) % (2 * (count - 1));
                int index = cycle < count - 1 ? cycle : 2 * (count - 1) - cycle;
                int next = cycle < count - 1 ? index + 1 : index - 1;
                var from = e.Spec.Route[index];
                var to = e.Spec.Route[next];
                e.RouteIndex = index;
                e.Direction = to.X > from.X ? 1 : -1;
                float t = Clamp((a - 0.25f) / 0.65f, 0f, 1f);
                e.X = from.X + (to.X - from.X) * t;
                e.Y = from.Y + (to.Y - from.Y) * t - (float)Math.Sin(t * Math.PI) * 128f;
                return;
            }

            bool angry = e.Hp == 1 && e.Spec.Kind == PicnicEnemyKind.Caterpillar;
            GetEnemyRange(e, out float rLeft, out float rRight);
            float distance = PlayerX - e.X;
            bool engaged = Math.Abs(distance) < (angry ? 680f : 360f) && Math.Abs(PlayerY - e.Y) < 230f;
            float speed = angry ? Math.Max(155f, e.Spec.Speed * 2.7f) : e.Spec.Speed * (e.Spec.Kind == PicnicEnemyKind.Ant ? 1.5f : 1.3f);
            float bLeft = (angry || e.Spec.Kind == PicnicEnemyKind.Ant) ? rLeft : e.Spec.Left;
            float bRight = (angry || e.Spec.Kind == PicnicEnemyKind.Ant) ? rRight : e.Spec.Right;

            if (angry)
            {
                e.AttackAge += dt;
                if (e.AttackPhase == "retreat")
                {
                    e.Direction = e.RetreatDirection;
                    speed = 300f;
                    float edge = e.Direction > 0 ? bRight - e.X : e.X - bLeft;
                    if ((e.AttackAge > 0.35f && (Math.Abs(distance) > 175f || edge < 6f)) || e.AttackAge > 1.15f)
                    {
                        e.AttackPhase = "windup"; e.AttackAge = 0f;
                    }
                }
                else if (e.AttackPhase == "windup")
                {
                    e.Direction = distance < 0f ? -1 : 1;
                    speed = 0f;
                    if (e.AttackAge > 0.26f) { e.AttackPhase = "charge"; e.AttackAge = 0f; }
                }
                else if (e.AttackPhase == "charge")
                {
                    speed = 250f;
                    if (e.AttackAge > 1.3f || e.Direction * distance < -65f)
                    {
                        e.AttackPhase = "retreat"; e.AttackAge = 0f; e.RetreatDirection = e.Direction;
                    }
                }
                else if (engaged && Math.Abs(distance) > 15f)
                {
                    e.Direction = distance < 0f ? -1 : 1;
                }
            }
            else if (engaged && Math.Abs(distance) > 15f)
            {
                e.Direction = distance < 0f ? -1 : 1;
            }

            e.X += e.Direction * speed * dt * (Difficulty == PicnicDifficulty.Easy ? 0.8f : 1f);
            if (e.X < bLeft || e.X > bRight)
            {
                e.X = Clamp(e.X, bLeft, bRight);
                e.Direction *= -1;
                if (angry && e.AttackPhase == "retreat") { e.AttackPhase = "windup"; e.AttackAge = 0f; }
            }

            if (e.Spec.SupportId != null && Level.PlatformById.TryGetValue(e.Spec.SupportId, out var home))
            {
                e.BaseY = PicnicLevelData.SurfaceY(home, e.X);
            }
            e.Y = e.BaseY;
            if (e.Spec.Kind == PicnicEnemyKind.Ant)
            {
                float period = e.Spec.Period * 0.8f;
                float phase = (e.HopAge + e.Spec.Phase) % period;
                if (phase < 0.8f) e.Y -= (float)Math.Sin((phase / 0.8f) * Math.PI) * 110f;
            }
        }

        private void GetEnemyRange(PicnicEnemyRuntime e, out float left, out float right)
        {
            if (e.Spec.SupportId != null && Level.PlatformById.TryGetValue(e.Spec.SupportId, out var sup))
            {
                left = sup.X + 45f;
                right = sup.X + sup.Width - 45f;
                return;
            }
            left = e.Spec.Left;
            right = e.Spec.Right;
        }

        private void TickFairy(float dt, IReadOnlyList<PicnicPlatformSpec> platforms)
        {
            if (PlayerX > fairyProgressX + 35f)
            {
                fairyProgressX = PlayerX;
                fairyIdle = 0f;
            }
            else fairyIdle += dt;

            if (FairyMode == "guide")
            {
                fairyGuideAge += dt;
                if (fairyGuideAge > 3f || fairyIdle < 0.2f || FindPlatform(platforms, fairyTargetId) == null)
                {
                    FairyMode = "follow";
                    fairyGuideAge = 0f;
                    fairyIdle = 0f;
                    fairyTargetId = null;
                }
            }
            else if (fairyIdle > 7f && Stage == PicnicStage.Running)
            {
                PicnicPlatformSpec next = null;
                for (int i = 0; i < platforms.Count; i++)
                {
                    var v = platforms[i];
                    if (v.X + v.Width > PlayerX + 80f && v.X > PlayerX - 20f)
                    {
                        if (next == null || v.X < next.X) next = v;
                    }
                }
                if (next != null)
                {
                    FairyMode = "guide";
                    fairyTargetId = next.Id;
                    fairyGuideAge = 0f;
                }
            }

            var target = FairyMode == "guide" ? FindPlatform(platforms, fairyTargetId) : null;
            float tx = target != null ? target.X + target.Width * 0.5f : PlayerX + (float)Math.Sin(Time * 2f) * 13f;
            float ty = target != null
                ? PicnicLevelData.SurfaceY(target, tx) - 55f
                : PlayerY - PicnicLevelData.PlayerHeight - 42f + (float)Math.Sin(Time * 3f) * 6f;
            float mix = 1f - (float)Math.Exp(-dt * 4f);
            FairyX += (tx - FairyX) * mix;
            FairyY += (ty - FairyY) * mix;
        }

        private static PicnicPlatformSpec FindPlatform(IReadOnlyList<PicnicPlatformSpec> list, string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == id) return list[i];
            }
            return null;
        }

        private static float Clamp(float v, float a, float b) => Math.Max(a, Math.Min(b, v));
    }
}
