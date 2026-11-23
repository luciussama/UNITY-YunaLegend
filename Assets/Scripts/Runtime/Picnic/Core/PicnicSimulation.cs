using System;

namespace YunaLegend.Picnic.Core
{
    public sealed class PicnicCreatureState
    {
        public PicnicCreatureSpec Spec { get; }
        public int Health { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Clock { get; private set; }
        public bool Active => Health > 0;
        public bool Warning { get; private set; }
        private float beeClock;
        private float beeFrom;
        private bool beeAtOther;
        public PicnicCreatureState(PicnicCreatureSpec spec)
        {
            Spec = spec;
            Health = spec.Kind == PicnicCreatureKind.Caterpillar ? 2 : 1;
            X = spec.X;
            Y = spec.Y;
        }
        public void Step(float dt, float playerX)
        {
            if (!Active) return;
            Clock += dt;
            if (Spec.Kind == PicnicCreatureKind.Bee)
            {
                beeClock -= dt;
                if (beeClock <= -1.8f && Math.Abs(X - playerX) < 3.2f)
                {
                    beeClock = 1.75f;
                    beeFrom = X;
                }
                Warning = beeClock > 1.25f;
                if (beeClock <= 1.25f && beeClock > 0f)
                {
                    float t = 1f - beeClock / 1.25f;
                    X = beeFrom + (beeAtOther ? -1f : 1f) * Spec.Range * t;
                    Y = Spec.Y - 1.55f * (float)Math.Sin(t * Math.PI);
                    if (beeClock <= dt) beeAtOther = !beeAtOther;
                }
                else if (!Warning) Y = Spec.Y;
                return;
            }
            if (Spec.Kind == PicnicCreatureKind.Worm)
            {
                float distance = Math.Abs(playerX - X);
                Warning = distance < 5f;
                float target = distance < 2.8f ? 0.72f : distance > 3.6f ? -0.7f : Y;
                Y += Math.Max(-dt * 6.5f, Math.Min(dt * 6.5f, target - Y));
                return;
            }
            X = Spec.X + (float)Math.Sin(Clock * (Health == 1 && Spec.Kind == PicnicCreatureKind.Caterpillar ? 2f : 0.8f)) * Spec.Range;
            Y = Spec.Y;
        }
        public bool Stomp()
        {
            if (Spec.Kind == PicnicCreatureKind.Worm) return false;
            Health = Math.Max(0, Health - 1);
            return true;
        }
    }

    /// <summary>Engine-free domain simulation. All platforms expose the same contact geometry used by the 3D adapter.</summary>
    public sealed class PicnicSimulation
    {
        public PicnicStageSpec Stage { get; }
        public PicnicPhase Phase { get; private set; } = PicnicPhase.Opening;
        public float X { get; private set; } = 3.38f;
        public float Y { get; private set; } = 0.5f;
        public float VelocityY { get; private set; }
        public float VelocityX { get; private set; }
        public float Time { get; private set; }
        public float Immunity { get; private set; }
        public bool EasyMode { get; }
        public int Hearts { get; private set; } = 3;
        public int Section { get; private set; }
        public int SupportIndex { get; private set; } = -1;
        public bool Grounded => SupportIndex >= 0;
        public bool Panic { get; private set; }
        public int PanicIndex { get; private set; } = -1;
        public float PanicElapsed { get; private set; }
        public float NarrativeTime { get; private set; }
        public float RescueTime { get; private set; }
        public int Falls { get; private set; }
        public int Jumps { get; private set; }
        public PicnicCreatureState[] Creatures { get; }
        public float[] CloudTimers { get; }
        public PicnicTerrain Terrain => Grounded ? Stage.Supports[SupportIndex].TerrainAt(X) : PicnicTerrain.Soil;
        public event Action<string> Event;
        private readonly bool[] panicSeen = new bool[3];
        private float safeX = 3.38f;
        private float safeY = 0.5f;
        private float rescueX;
        private float rescueY;
        private float jumpBuffer;
        private float coyote;

        public PicnicSimulation(bool easyMode = false, PicnicStageSpec stage = null)
        {
            EasyMode = easyMode;
            Stage = stage ?? new PicnicStageSpec();
            CloudTimers = new float[Stage.Supports.Count];
            for (int i = 0; i < CloudTimers.Length; i++) CloudTimers[i] = -1f;
            Creatures = new PicnicCreatureState[Stage.Creatures.Count];
            for (int i = 0; i < Creatures.Length; i++) Creatures[i] = new PicnicCreatureState(Stage.Creatures[i]);
            TryLand(Y + 0.1f);
        }

        public void AdvanceNarrative()
        {
            if (Phase == PicnicPhase.Opening)
            {
                NarrativeTime = Math.Min(PicnicNarrative.Opening.Duration, PicnicNarrative.Opening.NextBeat(NarrativeTime));
                if (NarrativeTime >= PicnicNarrative.Opening.Duration) BeginPlaying();
            }
            else if (Phase == PicnicPhase.Ending)
            {
                NarrativeTime = Math.Min(PicnicNarrative.Ending.Duration, PicnicNarrative.Ending.NextBeat(NarrativeTime));
                if (NarrativeTime >= PicnicNarrative.Ending.Duration) Complete();
            }
        }

        public void SkipNarrative()
        {
            if (Phase == PicnicPhase.Opening) BeginPlaying();
            else if (Phase == PicnicPhase.Ending) Complete();
        }

        public void Retry()
        {
            if (Phase != PicnicPhase.Defeat) return;
            X = Stage.Sections[Section].SafeX;
            Y = 10f;
            foreach (var support in Stage.Supports)
            {
                if (support.Ground && X >= support.Left(Time) && X <= support.Right(Time)) Y = support.Height(Time, X);
            }
            Hearts = 3;
            ResetTransient();
            Phase = PicnicPhase.Playing;
            Event?.Invoke("retry");
        }

        public bool IsSupportActive(int index)
        {
            float timer = CloudTimers[index];
            return timer < (EasyMode ? 3.8f : 2.8f);
        }

        public void Step(float dt, float axis = 0f, bool jump = false)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt <= 0f) return;
            // Substeps keep swept landings invariant under 30/60/120 Hz and frame stalls.
            int count = Math.Max(1, (int)Math.Ceiling(dt / (1f / 120f)));
            float step = dt / count;
            if (jump) jumpBuffer = 0.13f;
            for (int i = 0; i < count; i++) Tick(step, Math.Max(-1f, Math.Min(1f, axis)));
        }

        private void Tick(float dt, float axis)
        {
            if (Phase == PicnicPhase.Opening || Phase == PicnicPhase.Ending)
            {
                NarrativeTime += dt;
                float duration = Phase == PicnicPhase.Opening ? PicnicNarrative.Opening.Duration : PicnicNarrative.Ending.Duration;
                if (NarrativeTime >= duration) SkipNarrative();
                return;
            }
            if (Phase == PicnicPhase.Rescuing)
            {
                RescueTime += dt;
                float t = Math.Min(1f, RescueTime / 1.5f);
                float smooth = t * t * (3f - 2f * t);
                X = rescueX + (safeX - rescueX) * smooth;
                Y = rescueY + (safeY - rescueY) * smooth + (float)Math.Sin(t * Math.PI) * 2f;
                if (t >= 1f)
                {
                    ResetTransient();
                    Immunity = 2f;
                    Phase = PicnicPhase.Playing;
                    Event?.Invoke("rescued");
                }
                return;
            }
            if (Phase != PicnicPhase.Playing) return;
            float oldTime = Time;
            Time += dt;
            Immunity = Math.Max(0f, Immunity - dt);
            UpdateClouds(dt);
            if (Grounded)
            {
                var support = Stage.Supports[SupportIndex];
                X += support.Center(Time) - support.Center(oldTime);
                Y = support.Height(Time, X);
                if (!IsSupportActive(SupportIndex) || X < support.Left(Time) - 0.08f || X > support.Right(Time) + 0.08f)
                {
                    SupportIndex = -1;
                }
            }
            coyote = Grounded ? 0.1f : Math.Max(0f, coyote - dt);
            VelocityX = Panic ? 3.2f : axis * PicnicStageSpec.Speed(Terrain);
            jumpBuffer = Math.Max(0f, jumpBuffer - dt);
            if (jumpBuffer > 0f && coyote > 0f)
            {
                VelocityY = PicnicStageSpec.JumpSpeed;
                SupportIndex = -1;
                coyote = 0f;
                jumpBuffer = 0f;
                Jumps++;
                Event?.Invoke("jump");
            }
            float previousY = Y;
            X = Math.Max(0.3f, X + VelocityX * dt);
            if (Grounded)
            {
                var support = Stage.Supports[SupportIndex];
                if (X < support.Left(Time) - 0.08f || X > support.Right(Time) + 0.08f) SupportIndex = -1;
                else Y = support.Height(Time, X);
            }
            if (!Grounded)
            {
                Y += VelocityY * dt - 0.5f * PicnicStageSpec.Gravity * dt * dt;
                VelocityY -= PicnicStageSpec.Gravity * dt;
                if (VelocityY <= 0f) TryLand(previousY);
            }
            UpdateSectionAndPanic(dt);
            foreach (var creature in Creatures)
            {
                creature.Step(dt, X);
                ResolveCreature(creature, previousY);
            }
            if (Y < -2.8f) Fall();
            if (Phase == PicnicPhase.Playing && X >= PicnicStageSpec.Goal && Grounded)
            {
                Phase = PicnicPhase.Ending;
                NarrativeTime = 0f;
                Event?.Invoke("ending_started");
            }
        }

        private void TryLand(float previousY)
        {
            float highest = float.NegativeInfinity;
            int candidate = -1;
            for (int i = 0; i < Stage.Supports.Count; i++)
            {
                var support = Stage.Supports[i];
                if (!IsSupportActive(i) || X < support.Left(Time) - 0.08f || X > support.Right(Time) + 0.08f) continue;
                float top = support.Height(Time, X);
                if (previousY + 0.04f >= top && Y <= top && top > highest)
                {
                    candidate = i;
                    highest = top;
                }
            }
            if (candidate < 0) return;
            SupportIndex = candidate;
            Y = highest;
            VelocityY = 0f;
            if (Stage.Supports[candidate].Terrain == PicnicTerrain.Cloud && CloudTimers[candidate] < 0f)
            {
                CloudTimers[candidate] = 0f;
                Event?.Invoke("cloud_armed");
            }
            if (Stage.Supports[candidate].Wheel < 0 && Stage.Supports[candidate].Terrain != PicnicTerrain.Cloud)
            {
                safeX = X;
                safeY = Y;
            }
        }

        private void UpdateClouds(float dt)
        {
            for (int i = 0; i < CloudTimers.Length; i++)
            {
                if (CloudTimers[i] < 0f) continue;
                CloudTimers[i] += dt;
                if (CloudTimers[i] >= (EasyMode ? 3.8f : 2.8f) + 4f) CloudTimers[i] = -1f;
            }
        }

        private void UpdateSectionAndPanic(float dt)
        {
            int next = Stage.SectionAt(X);
            if (next > Section)
            {
                Section = next;
                Event?.Invoke("checkpoint_" + Section);
            }
            if (Grounded && Terrain != PicnicTerrain.Cloud && Stage.Supports[SupportIndex].Wheel < 0)
            {
                safeX = X;
                safeY = Y;
            }
            for (int i = 0; i < 3; i++)
            {
                if (!panicSeen[i] && X >= Stage.PanicStarts[i] && X < Stage.PanicEnds[i])
                {
                    panicSeen[i] = true;
                    Panic = true;
                    PanicIndex = i;
                    PanicElapsed = 0f;
                    Event?.Invoke("panic_started_" + i);
                }
            }
            if (!Panic) return;
            PanicElapsed += dt;
            if ((PanicIndex == 2 && PanicElapsed >= 6f) || (PanicIndex != 2 && X >= Stage.PanicEnds[PanicIndex] && Grounded))
            {
                Panic = false;
                Event?.Invoke("panic_ended_" + PanicIndex);
            }
        }

        private void ResolveCreature(PicnicCreatureState creature, float previousY)
        {
            if (!creature.Active || Math.Abs(X - creature.X) > 0.45f) return;
            if (creature.Spec.Kind == PicnicCreatureKind.Worm && creature.Y < 0.1f) return;
            float top = creature.Y + (creature.Spec.Kind == PicnicCreatureKind.Worm ? 0f : 0.55f);
            if (Y > top + 0.15f || Y + 1.45f < creature.Y) return;
            if (VelocityY < 0f && previousY >= top - 0.2f && creature.Stomp())
            {
                Y = top;
                VelocityY = 5.5f;
                SupportIndex = -1;
                Event?.Invoke("stomp");
                return;
            }
            TakeHit();
        }

        public void TakeHit()
        {
            if (Phase != PicnicPhase.Playing || Immunity > 0f) return;
            Immunity = 2f;
            if (EasyMode)
            {
                StartRescue();
                Event?.Invoke("disoriented");
                return;
            }
            Hearts = Math.Max(0, Hearts - 1);
            Event?.Invoke("hit");
            if (Hearts == 0)
            {
                Phase = PicnicPhase.Defeat;
                Event?.Invoke("defeat");
            }
        }

        private void Fall()
        {
            Falls++;
            if (EasyMode) StartRescue();
            else
            {
                Phase = PicnicPhase.Defeat;
                Event?.Invoke("defeat");
            }
        }

        private void StartRescue()
        {
            rescueX = X;
            rescueY = Y;
            RescueTime = 0f;
            Phase = PicnicPhase.Rescuing;
            Event?.Invoke("rescue_started");
        }

        private void ResetTransient()
        {
            VelocityX = 0f;
            VelocityY = 0f;
            SupportIndex = -1;
            jumpBuffer = 0f;
            coyote = 0f;
            Panic = false;
            PanicIndex = -1;
            for (int i = 0; i < panicSeen.Length; i++) panicSeen[i] = X >= Stage.PanicEnds[i];
            for (int i = 0; i < CloudTimers.Length; i++) CloudTimers[i] = -1f;
            TryLand(Y + 0.2f);
        }

        private void BeginPlaying()
        {
            Phase = PicnicPhase.Playing;
            NarrativeTime = 0f;
            Event?.Invoke("started");
        }

        private void Complete()
        {
            Phase = PicnicPhase.Complete;
            Event?.Invoke("completed");
        }
    }
}
