using System;
using System.Collections.Generic;

namespace YunaLegend.BearChase.Core
{
    /// <summary>
    /// Gerencia o agendamento deterministico, telegrafia e trajetoria parabolica
    /// dos brinquedos arremessados pelo urso na direcao da menina,
    /// garantindo sempre ao menos uma faixa livre para desvio.
    /// </summary>
    public sealed class BearChaseThrowScheduler
    {
        private static readonly float[] StandardLanes = { -1.85f, 0f, 1.85f };
        private readonly List<BearChaseThrownToyState> activeToys = new List<BearChaseThrownToyState>(12);
        private readonly int initialSeed;
        private Random rng;

        private int nextToyId;
        private float cooldownTimer;
        private float windupRemaining;
        private bool isWindingUp;
        private BearChaseObstacleKind pendingKind;

        public bool IsWindingUp => isWindingUp;
        public float WindupProgress { get; private set; }
        public BearChaseObstacleKind PendingKind => pendingKind;
        public IReadOnlyList<BearChaseThrownToyState> ActiveToys => activeToys;

        public BearChaseThrowScheduler(int seed = 42019)
        {
            initialSeed = seed;
            Reset(initialDelay: 1.05f, resetRng: true);
        }

        public void Reset(float initialDelay = 1.05f, bool resetRng = true)
        {
            if (resetRng)
            {
                rng = new Random(initialSeed);
            }

            activeToys.Clear();
            nextToyId = 0;
            cooldownTimer = initialDelay;
            windupRemaining = 0f;
            isWindingUp = false;
            WindupProgress = 0f;
            pendingKind = BearChaseObstacleKind.ToyTrain;
        }

        public void Step(
            float dt,
            float yunaZ,
            float yunaLaneX,
            float yunaSpeed,
            float bearZ,
            float bearLaneX,
            float normalizedProgress,
            BearChaseTuningSpec tuning,
            BearChaseTrackSpec track,
            Action<BearChaseObstacleKind> onTelegraphed,
            Action<BearChaseThrownToyState> onThrown,
            Action<BearChaseThrownToyState> onLanded)
        {
            UpdateActiveToys(dt, yunaZ, onLanded);

            if (yunaZ >= track.TrackLength - 24f || (bearZ - yunaZ) < 5.4f)
            {
                isWindingUp = false;
                WindupProgress = 0f;
                return;
            }

            if (isWindingUp)
            {
                windupRemaining -= dt;
                WindupProgress = 1f - Math.Max(0f, Math.Min(1f, windupRemaining / tuning.ThrowWindupDuration));
                if (windupRemaining <= 0f)
                {
                    isWindingUp = false;
                    WindupProgress = 0f;
                    BearChaseThrownToyState spawned = SpawnThrownToy(
                        yunaZ,
                        yunaLaneX,
                        yunaSpeed,
                        bearZ,
                        bearLaneX,
                        track);
                    onThrown?.Invoke(spawned);
                    float interval = Lerp(tuning.ThrowIntervalStart, tuning.ThrowIntervalEnd, normalizedProgress);
                    if (tuning.EasyModeThrowMultiplier > 1f)
                    {
                        interval *= tuning.EasyModeThrowMultiplier;
                    }

                    cooldownTimer = interval;
                }

                return;
            }

            float pressure = BearChaseTrackSampler.ThrowPressure(yunaZ, track);
            cooldownTimer -= dt * (1f + pressure * 0.85f);
            if (cooldownTimer <= 0f)
            {
                isWindingUp = true;
                windupRemaining = tuning.ThrowWindupDuration;
                WindupProgress = 0f;
                pendingKind = PickNextKind();
                onTelegraphed?.Invoke(pendingKind);
            }
        }

        public void MarkToyDeflected(BearChaseThrownToyState toy, float deflectDirX)
        {
            if (toy == null)
            {
                return;
            }

            toy.WasHit = true;
            toy.Phase = BearChaseThrownToyPhase.Deflected;
            toy.DeflectDirX = deflectDirX >= 0f ? 1f : -1f;
            toy.BounceTime = 0f;
        }

        private BearChaseObstacleKind PickNextKind()
        {
            var all = BearChaseObstacleCatalog.All;
            int idx = rng.Next(0, all.Count);
            return all[idx].Kind;
        }

        private BearChaseThrownToyState SpawnThrownToy(
            float yunaZ,
            float yunaLaneX,
            float yunaSpeed,
            float bearZ,
            float bearLaneX,
            BearChaseTrackSpec track)
        {
            BearChaseObstacleDef def = BearChaseObstacleCatalog.Get(pendingKind);
            float startZ = bearZ - 0.55f;
            float desiredLandZ = yunaZ + yunaSpeed * def.FlightDuration + 2.35f;
            float minTargetZ = yunaZ + Math.Max(3.6f, yunaSpeed * 0.34f);
            float maxTargetZ = Math.Min(track.TrackLength - 10f, startZ - 1.35f);
            if (maxTargetZ < minTargetZ)
            {
                minTargetZ = Math.Min(maxTargetZ, yunaZ + 2.4f);
            }

            float targetZ = Math.Min(maxTargetZ, Math.Max(minTargetZ, desiredLandZ));

            float targetLaneX = PickNavigableTargetLane(targetZ, yunaLaneX);
            var state = new BearChaseThrownToyState
            {
                Id = nextToyId++,
                Kind = pendingKind,
                Phase = BearChaseThrownToyPhase.InFlight,
                StartZ = startZ,
                StartLaneX = bearLaneX,
                TargetZ = targetZ,
                TargetLaneX = targetLaneX,
                CurrentZ = startZ,
                CurrentLaneX = bearLaneX,
                HeightAboveGround = 0.95f,
                NormalizedFlight = 0f,
                BounceTime = 0f,
                SpinAngleDeg = 0f,
                WasHit = false,
                DeflectDirX = 0f
            };
            activeToys.Add(state);
            return state;
        }

        private float PickNavigableTargetLane(float targetZ, float yunaLaneX)
        {
            bool[] blocked = new bool[StandardLanes.Length];
            for (int i = 0; i < activeToys.Count; i++)
            {
                BearChaseThrownToyState other = activeToys[i];
                if (other.Phase == BearChaseThrownToyPhase.Expired || other.Phase == BearChaseThrownToyPhase.Deflected)
                {
                    continue;
                }

                float otherZ = other.Phase == BearChaseThrownToyPhase.InFlight ? other.TargetZ : other.CurrentZ;
                if (Math.Abs(otherZ - targetZ) > 9.5f)
                {
                    continue;
                }

                float otherLane = other.Phase == BearChaseThrownToyPhase.InFlight ? other.TargetLaneX : other.CurrentLaneX;
                int laneIdx = ClosestLaneIndex(otherLane);
                blocked[laneIdx] = true;
            }

            int blockedCount = 0;
            for (int i = 0; i < blocked.Length; i++)
            {
                if (blocked[i])
                {
                    blockedCount++;
                }
            }

            // Garante estritamente pelo menos 1 faixa aberta dentro de uma janela de 7.5m
            int desiredIdx = ClosestLaneIndex(yunaLaneX);
            if (rng.NextDouble() < 0.38)
            {
                desiredIdx = rng.Next(0, StandardLanes.Length);
            }

            if (blockedCount >= StandardLanes.Length - 1)
            {
                for (int i = 0; i < blocked.Length; i++)
                {
                    if (blocked[i])
                    {
                        return StandardLanes[i];
                    }
                }
            }

            if (!blocked[desiredIdx])
            {
                return StandardLanes[desiredIdx];
            }

            for (int offset = 1; offset < StandardLanes.Length; offset++)
            {
                int candidate = (desiredIdx + offset) % StandardLanes.Length;
                if (!blocked[candidate])
                {
                    return StandardLanes[candidate];
                }
            }

            return StandardLanes[desiredIdx];
        }

        private static int ClosestLaneIndex(float laneX)
        {
            int best = 0;
            float bestDist = Math.Abs(laneX - StandardLanes[0]);
            for (int i = 1; i < StandardLanes.Length; i++)
            {
                float d = Math.Abs(laneX - StandardLanes[i]);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            return best;
        }

        private void UpdateActiveToys(float dt, float yunaZ, Action<BearChaseThrownToyState> onLanded)
        {
            for (int i = activeToys.Count - 1; i >= 0; i--)
            {
                BearChaseThrownToyState toy = activeToys[i];
                BearChaseObstacleDef def = BearChaseObstacleCatalog.Get(toy.Kind);

                switch (toy.Phase)
                {
                    case BearChaseThrownToyPhase.InFlight:
                    {
                        toy.NormalizedFlight = Math.Min(1f, toy.NormalizedFlight + dt / def.FlightDuration);
                        float u = toy.NormalizedFlight;
                        toy.CurrentZ = Lerp(toy.StartZ, toy.TargetZ, u);
                        toy.CurrentLaneX = Lerp(toy.StartLaneX, toy.TargetLaneX, u);
                        toy.HeightAboveGround = 4f * def.ArcHeight * u * (1f - u) + Lerp(0.92f, 0f, u);
                        toy.SpinAngleDeg += def.SpinSpeedDeg * dt;

                        if (u >= 1f)
                        {
                            toy.Phase = BearChaseThrownToyPhase.Sliding;
                            toy.HeightAboveGround = 0f;
                            toy.BounceTime = 0f;
                            onLanded?.Invoke(toy);
                        }

                        break;
                    }

                    case BearChaseThrownToyPhase.Sliding:
                    {
                        toy.BounceTime += dt;
                        toy.CurrentZ -= def.SlideSpeed * dt;
                        float damp = (float)Math.Exp(-toy.BounceTime * 3.4);
                        toy.HeightAboveGround = (float)Math.Abs(Math.Sin(toy.BounceTime * 9.5)) * def.BounceHeight * damp;
                        toy.SpinAngleDeg += def.SpinSpeedDeg * 0.25f * damp * dt;

                        if (toy.CurrentZ < yunaZ - 6.5f)
                        {
                            toy.Phase = BearChaseThrownToyPhase.Expired;
                            activeToys.RemoveAt(i);
                        }

                        break;
                    }

                    case BearChaseThrownToyPhase.Deflected:
                    {
                        toy.BounceTime += dt;
                        toy.CurrentLaneX += toy.DeflectDirX * 7.5f * dt;
                        toy.CurrentZ -= 2.0f * dt;
                        toy.HeightAboveGround = Math.Max(0f, (float)Math.Sin(Math.Min(1f, toy.BounceTime * 2.4f) * Math.PI) * 1.15f);
                        toy.SpinAngleDeg += 540f * dt;

                        if (toy.BounceTime >= 0.65f || toy.CurrentZ < yunaZ - 6.5f)
                        {
                            toy.Phase = BearChaseThrownToyPhase.Expired;
                            activeToys.RemoveAt(i);
                        }

                        break;
                    }
                }
            }
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Max(0f, Math.Min(1f, t));
        }
    }
}
