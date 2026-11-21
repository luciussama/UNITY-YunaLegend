using System;
using System.Collections.Generic;

namespace YunaLegend.BearChase.Core
{
    /// <summary>
    /// Simulacao pura (deterministica e testavel em EditMode) da Fase 4:
    /// controla a cena de entrada (~6.5s), corrida automatica com desvio lateral,
    /// vidas (3 normal / 6 facil), tropeço, oferta de modo facil apos 2 falhas e vitoria.
    /// </summary>
    public sealed class BearChaseSimulation
    {
        private readonly BearChaseObstacleSpec[] obstacles;
        private readonly HashSet<int> hitObstacleIds = new HashSet<int>();

        public BearChasePhase Phase { get; private set; }
        public float PhaseTimer { get; private set; }
        public float TotalChaseTime { get; private set; }

        public float YunaZ { get; private set; }
        public float YunaLaneOffsetX { get; private set; }
        public float YunaLateralInput { get; private set; }

        public float BearZ { get; private set; }
        public float BearLaneOffsetX { get; private set; }
        public bool BearLookingBack { get; private set; }

        public int Hearts { get; private set; }
        public int MaxHearts { get; private set; }
        public bool EasyMode { get; private set; }
        public int FailureCount { get; private set; }
        public bool ShouldOfferEasyMode => !EasyMode && FailureCount >= BearChaseConfig.FailuresToOfferEasyMode;

        public float InvulnerabilityRemaining { get; private set; }
        public float NormalizedProgress => Math.Max(0f, Math.Min(1f, YunaZ / BearChaseConfig.TrackLength));

        public IReadOnlyList<BearChaseObstacleSpec> Obstacles => obstacles;

        public event Action<BearChasePhase> OnPhaseChanged;
        public event Action<int, int> OnHeartsChanged;
        public event Action<BearChaseObstacleSpec> OnObstacleHit;
        public event Action OnEasyModeOffered;
        public event Action OnVictory;
        public event Action OnDefeat;

        public BearChaseSimulation(BearChaseObstacleSpec[] customObstacles = null)
        {
            obstacles = customObstacles ?? BearChaseConfig.BuildDefaultObstacles();
            ResetFullStage(skipEntrance: false, useEasyMode: false);
        }

        public void ResetFullStage(bool skipEntrance, bool useEasyMode)
        {
            EasyMode = useEasyMode;
            MaxHearts = useEasyMode ? BearChaseConfig.EasyMaxHearts : BearChaseConfig.NormalMaxHearts;
            Hearts = MaxHearts;
            TotalChaseTime = 0f;
            YunaZ = 0f;
            YunaLaneOffsetX = 0f;
            YunaLateralInput = 0f;
            BearZ = BearChaseConfig.BearInitialLead;
            BearLaneOffsetX = 0f;
            BearLookingBack = false;
            InvulnerabilityRemaining = 0f;
            hitObstacleIds.Clear();

            SetPhase(skipEntrance ? BearChasePhase.Chasing : BearChasePhase.EntranceWalk);
            OnHeartsChanged?.Invoke(Hearts, MaxHearts);
        }

        public void RetryChase(bool activateEasyMode)
        {
            if (activateEasyMode)
            {
                EasyMode = true;
            }

            ResetFullStage(skipEntrance: true, useEasyMode: EasyMode);
        }

        public void SetLateralInput(float input)
        {
            YunaLateralInput = Math.Max(-1f, Math.Min(1f, input));
        }

        public void Step(float dt)
        {
            if (dt <= 0f)
            {
                return;
            }

            PhaseTimer += dt;

            switch (Phase)
            {
                case BearChasePhase.EntranceWalk:
                    if (PhaseTimer >= BearChaseConfig.EntranceWalkDuration)
                    {
                        SetPhase(BearChasePhase.EntranceNudge);
                    }
                    break;

                case BearChasePhase.EntranceNudge:
                    if (PhaseTimer >= BearChaseConfig.EntranceNudgeDuration)
                    {
                        SetPhase(BearChasePhase.BearWakeUp);
                    }
                    break;

                case BearChasePhase.BearWakeUp:
                    if (PhaseTimer >= BearChaseConfig.BearWakeUpDuration)
                    {
                        SetPhase(BearChasePhase.TransitionToChase);
                    }
                    break;

                case BearChasePhase.TransitionToChase:
                    if (PhaseTimer >= BearChaseConfig.TransitionDuration)
                    {
                        SetPhase(BearChasePhase.Chasing);
                    }
                    break;

                case BearChasePhase.Chasing:
                    StepChase(dt, forwardFactor: 1f, allowCollisions: true);
                    break;

                case BearChasePhase.Stumble:
                    StepChase(dt, forwardFactor: 0.45f, allowCollisions: false);
                    if (PhaseTimer >= BearChaseConfig.StumbleDuration)
                    {
                        SetPhase(BearChasePhase.Chasing);
                    }
                    break;

                case BearChasePhase.Fallen:
                    if (PhaseTimer >= BearChaseConfig.FallenBannerDuration)
                    {
                        SetPhase(BearChasePhase.RetryModal);
                        if (ShouldOfferEasyMode)
                        {
                            OnEasyModeOffered?.Invoke();
                        }
                    }
                    break;
            }
        }

        private void StepChase(float dt, float forwardFactor, bool allowCollisions)
        {
            TotalChaseTime += dt;
            if (InvulnerabilityRemaining > 0f)
            {
                InvulnerabilityRemaining = Math.Max(0f, InvulnerabilityRemaining - dt);
            }

            float prevZ = YunaZ;
            YunaZ += BearChaseConfig.YunaForwardSpeed * forwardFactor * dt;

            float nextOffset = YunaLaneOffsetX + YunaLateralInput * BearChaseConfig.YunaLateralSpeed * dt;
            YunaLaneOffsetX = Math.Max(-BearChaseConfig.LaneHalfWidth, Math.Min(BearChaseConfig.LaneHalfWidth, nextOffset));

            UpdateBearProgress(dt);

            if (YunaZ >= BearChaseConfig.TrackLength)
            {
                YunaZ = BearChaseConfig.TrackLength;
                BearZ = BearChaseConfig.TrackLength + 0.9f;
                SetPhase(BearChasePhase.Victory);
                OnVictory?.Invoke();
                return;
            }

            if (allowCollisions && InvulnerabilityRemaining <= 0f)
            {
                CheckCollisions(prevZ, YunaZ);
            }
        }

        private void UpdateBearProgress(float dt)
        {
            // No inicio, o urso dispara rapido para abrir vantagem; nos ultimos 25%, Yuna se aproxima gradualmente
            float progress = NormalizedProgress;
            float desiredLead = progress < 0.72f
                ? BearChaseConfig.BearTargetLead
                : Lerp(BearChaseConfig.BearTargetLead, BearChaseConfig.BearCatchupThreshold, (progress - 0.72f) / 0.28f);

            float targetBearZ = Math.Min(BearChaseConfig.TrackLength + 1.2f, YunaZ + desiredLead);
            float blendRate = progress < 0.1f ? 3.2f : 2.1f;
            BearZ = Lerp(BearZ, targetBearZ, Math.Min(1f, dt * blendRate));

            // Ziguezague natural do urso evitando obstaculos e dando dinamismo
            BearLaneOffsetX = (float)Math.Sin(TotalChaseTime * 1.45) * 1.35f;

            // Urso olha rapidamente para tras a cada ~5.5s por ~0.85s
            float lookCycle = TotalChaseTime % 5.5f;
            BearLookingBack = lookCycle > 4.2f && lookCycle < 5.05f;
        }

        private void CheckCollisions(float prevZ, float currentZ)
        {
            float r = BearChaseConfig.YunaCollisionRadius;
            for (int i = 0; i < obstacles.Length; i++)
            {
                BearChaseObstacleSpec obs = obstacles[i];
                if (hitObstacleIds.Contains(obs.Id))
                {
                    continue;
                }

                float minZ = obs.Z - obs.HalfDepth - r;
                float maxZ = obs.Z + obs.HalfDepth + r;
                if (currentZ < minZ || prevZ > maxZ)
                {
                    continue;
                }

                float dx = Math.Abs(YunaLaneOffsetX - obs.LaneOffsetX);
                if (dx <= obs.HalfWidth + r)
                {
                    hitObstacleIds.Add(obs.Id);
                    ApplyObstacleHit(obs);
                    return;
                }
            }
        }

        private void ApplyObstacleHit(BearChaseObstacleSpec obs)
        {
            Hearts = Math.Max(0, Hearts - 1);
            InvulnerabilityRemaining = BearChaseConfig.InvulnerabilityDuration;
            OnHeartsChanged?.Invoke(Hearts, MaxHearts);
            OnObstacleHit?.Invoke(obs);

            if (Hearts <= 0)
            {
                FailureCount++;
                SetPhase(BearChasePhase.Fallen);
                OnDefeat?.Invoke();
            }
            else
            {
                SetPhase(BearChasePhase.Stumble);
            }
        }

        private void SetPhase(BearChasePhase next)
        {
            if (Phase == next)
            {
                PhaseTimer = 0f;
                return;
            }

            Phase = next;
            PhaseTimer = 0f;
            OnPhaseChanged?.Invoke(Phase);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Max(0f, Math.Min(1f, t));
        }
    }
}
