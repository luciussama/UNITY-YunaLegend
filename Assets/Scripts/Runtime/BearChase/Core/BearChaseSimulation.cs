using System;
using System.Collections.Generic;

namespace YunaLegend.BearChase.Core
{
    /// <summary>
    /// Simulacao pura (fixed-step 60 Hz deterministico, sem dependencias UnityEngine) da Fase 4:
    /// controla a cinematica 2D ilustrada de entrada, corrida em alta velocidade em pista curva/ondulada,
    /// arremesso ativo de brinquedos pelo urso com passagem livre garantida, vidas, modo facil e vitoria.
    /// </summary>
    public sealed class BearChaseSimulation
    {
        public const float FixedStepSeconds = 1f / 60f;
        private const double FixedStepSecondsDouble = 1.0 / 60.0;

        private readonly BearChaseTuningSpec baseTuning;
        private readonly BearChaseTrackSpec trackSpec;
        private readonly BearChaseThrowScheduler throwScheduler;
        private readonly int throwSeed;

        private BearChaseTuningSpec activeTuning;
        private double fixedAccumulator;
        private float entranceElapsed;
        private float lookBackClock;
        private float currentBearLead;

        public BearChasePhase Phase { get; private set; }
        public BearChaseBearAction BearAction { get; private set; }
        public float PhaseTimer { get; private set; }
        public float TotalElapsedTime { get; private set; }
        public float TotalChaseTime { get; private set; }

        public float YunaZ { get; private set; }
        public float YunaLaneOffsetX { get; private set; }
        public float YunaLateralInput { get; private set; }
        public float CurrentForwardSpeed { get; private set; }

        public float BearZ { get; private set; }
        public float BearLaneOffsetX { get; private set; }
        public bool BearLookingBack => BearAction == BearChaseBearAction.LookingBack || BearAction == BearChaseBearAction.ThrowingToy;
        public float BearThrowWindupProgress => throwScheduler.IsWindingUp ? throwScheduler.WindupProgress : 0f;
        public BearChaseObstacleKind BearPendingThrowKind => throwScheduler.PendingKind;

        public int Hearts { get; private set; }
        public int MaxHearts { get; private set; }
        public bool EasyMode { get; private set; }
        public int FailureCount { get; private set; }
        public bool ShouldOfferEasyMode => !EasyMode && FailureCount >= activeTuning.FailuresToOfferEasyMode;
        public bool IsEntrancePhase => Phase.IsEntrance();

        public float InvulnerabilityRemaining { get; private set; }
        public float NormalizedProgress => Math.Max(0f, Math.Min(1f, YunaZ / trackSpec.TrackLength));

        public BearChaseEntranceSample EntranceSample { get; private set; }
        public IReadOnlyList<BearChaseThrownToyState> ActiveThrownToys => throwScheduler.ActiveToys;
        public BearChaseTrackSpec TrackSpec => trackSpec;
        public BearChaseTuningSpec TuningSpec => activeTuning;

        public event Action<BearChasePhase> OnPhaseChanged;
        public event Action<int, int> OnHeartsChanged;
        public event Action<BearChaseObstacleKind> OnBearThrowTelegraphed;
        public event Action<BearChaseThrownToyState> OnBearToyThrown;
        public event Action<BearChaseThrownToyState> OnThrownToyLanded;
        public event Action<BearChaseThrownToyState> OnObstacleHit;
        public event Action OnEasyModeOffered;
        public event Action OnVictory;
        public event Action OnDefeat;

        public BearChaseSimulation(
            BearChaseTuningSpec tuning = null,
            BearChaseTrackSpec track = null,
            int seed = 42019)
        {
            baseTuning = tuning ?? BearChaseTuningSpec.Default;
            trackSpec = track ?? BearChaseTrackSpec.Default;
            throwSeed = seed;
            throwScheduler = new BearChaseThrowScheduler(throwSeed);
            ResetFullStage(skipEntrance: false, useEasyMode: false);
        }

        public void ResetFullStage(bool skipEntrance, bool useEasyMode)
        {
            EasyMode = useEasyMode;
            activeTuning = useEasyMode ? baseTuning.WithEasyModeSpacing() : baseTuning;
            MaxHearts = useEasyMode ? activeTuning.EasyMaxHearts : activeTuning.NormalMaxHearts;
            Hearts = MaxHearts;
            fixedAccumulator = 0.0;
            TotalElapsedTime = 0f;
            TotalChaseTime = 0f;
            lookBackClock = 0f;
            entranceElapsed = skipEntrance ? BearChaseEntranceTimeline.TotalDuration : 0f;

            YunaZ = 0f;
            YunaLaneOffsetX = 0f;
            YunaLateralInput = 0f;
            CurrentForwardSpeed = 0f;

            currentBearLead = activeTuning.BearInitialLead;
            BearZ = YunaZ + currentBearLead;
            BearLaneOffsetX = 0f;
            InvulnerabilityRemaining = 0f;
            throwScheduler.Reset(initialDelay: 1.05f);
            EntranceSample = BearChaseEntranceTimeline.Evaluate(entranceElapsed);

            BearAction = skipEntrance ? BearChaseBearAction.Running : BearChaseBearAction.Sleeping;
            Phase = skipEntrance ? BearChasePhase.Chasing : BearChasePhase.EntranceWalk;
            PhaseTimer = 0f;
            OnPhaseChanged?.Invoke(Phase);
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

            fixedAccumulator += dt;
            while (fixedAccumulator + 1e-9 >= FixedStepSecondsDouble)
            {
                fixedAccumulator -= FixedStepSecondsDouble;
                StepFixed(FixedStepSeconds);
            }
        }

        private void StepFixed(float dt)
        {
            TotalElapsedTime += dt;
            PhaseTimer += dt;

            if (Phase.IsEntrance())
            {
                StepEntrance(dt);
                return;
            }

            switch (Phase)
            {
                case BearChasePhase.Chasing:
                    StepChase(dt, forwardFactor: 1f, allowCollisions: true);
                    break;

                case BearChasePhase.Stumble:
                    StepChase(dt, forwardFactor: activeTuning.StumbleSpeedFactor, allowCollisions: false);
                    if (PhaseTimer >= activeTuning.StumbleDuration)
                    {
                        SetPhase(BearChasePhase.Chasing);
                    }
                    break;

                case BearChasePhase.Fallen:
                    CurrentForwardSpeed = 0f;
                    if (PhaseTimer >= activeTuning.FallenBannerDuration)
                    {
                        SetPhase(BearChasePhase.RetryModal);
                        if (ShouldOfferEasyMode)
                        {
                            OnEasyModeOffered?.Invoke();
                        }
                    }
                    break;

                default:
                    CurrentForwardSpeed = 0f;
                    break;
            }
        }

        private void StepEntrance(float dt)
        {
            entranceElapsed = Math.Min(BearChaseEntranceTimeline.TotalDuration, entranceElapsed + dt);
            EntranceSample = BearChaseEntranceTimeline.Evaluate(entranceElapsed);
            CurrentForwardSpeed = 0f;

            if (entranceElapsed >= BearChaseEntranceTimeline.TotalDuration)
            {
                BearAction = BearChaseBearAction.Running;
                SetPhase(BearChasePhase.Chasing);
                return;
            }

            if (entranceElapsed >= BearChaseEntranceTimeline.WakeUpEnd)
            {
                BearAction = BearChaseBearAction.WakingUp;
                SetPhase(BearChasePhase.TransitionToChase);
                return;
            }

            if (entranceElapsed >= BearChaseEntranceTimeline.ReachEnd)
            {
                BearAction = BearChaseBearAction.WakingUp;
                SetPhase(BearChasePhase.BearWakeUp);
                return;
            }

            if (entranceElapsed >= BearChaseEntranceTimeline.ApproachEnd)
            {
                BearAction = BearChaseBearAction.Sleeping;
                SetPhase(BearChasePhase.EntranceNudge);
                return;
            }

            BearAction = BearChaseBearAction.Sleeping;
            SetPhase(BearChasePhase.EntranceWalk);
        }

        private void StepChase(float dt, float forwardFactor, bool allowCollisions)
        {
            TotalChaseTime += dt;
            if (InvulnerabilityRemaining > 0f)
            {
                InvulnerabilityRemaining = Math.Max(0f, InvulnerabilityRemaining - dt);
            }

            float baseSpeed = activeTuning.EvaluateForwardSpeed(NormalizedProgress);
            CurrentForwardSpeed = baseSpeed * forwardFactor;

            float prevYunaZ = YunaZ;
            YunaZ += CurrentForwardSpeed * dt;

            float nextOffset = YunaLaneOffsetX + YunaLateralInput * activeTuning.LateralSpeed * dt;
            YunaLaneOffsetX = Math.Max(-trackSpec.LaneHalfWidth, Math.Min(trackSpec.LaneHalfWidth, nextOffset));

            UpdateBearProgress(dt);

            if (YunaZ >= trackSpec.TrackLength)
            {
                YunaZ = trackSpec.TrackLength;
                BearZ = trackSpec.TrackLength + 1.0f;
                CurrentForwardSpeed = 0f;
                BearAction = BearChaseBearAction.Caught;
                SetPhase(BearChasePhase.Victory);
                OnVictory?.Invoke();
                return;
            }

            throwScheduler.Step(
                dt,
                YunaZ,
                YunaLaneOffsetX,
                CurrentForwardSpeed,
                BearZ,
                BearLaneOffsetX,
                NormalizedProgress,
                activeTuning,
                trackSpec,
                OnBearThrowTelegraphed,
                OnBearToyThrown,
                OnThrownToyLanded);

            if (throwScheduler.IsWindingUp)
            {
                BearAction = BearChaseBearAction.ThrowingToy;
            }
            else
            {
                lookBackClock = TotalChaseTime % 4.4f;
                BearAction = lookBackClock > 3.4f
                    ? BearChaseBearAction.LookingBack
                    : BearChaseBearAction.Running;
            }

            if (allowCollisions && InvulnerabilityRemaining <= 0f)
            {
                CheckThrownToyCollisions(prevYunaZ, YunaZ);
            }
        }

        private void UpdateBearProgress(float dt)
        {
            float progress = NormalizedProgress;
            float desiredLead;
            if (progress < 0.92f)
            {
                desiredLead = activeTuning.BearTargetLead;
            }
            else
            {
                float u = Math.Max(0f, Math.Min(1f, (progress - 0.92f) / 0.08f));
                float smoothU = u * u * (3f - 2f * u);
                desiredLead = Lerp(activeTuning.BearTargetLead, activeTuning.BearCatchupThreshold, smoothU);
            }

            float blendRate = progress < 0.12f ? 3.2f : 4.2f;
            currentBearLead = Lerp(currentBearLead, desiredLead, Math.Min(1f, dt * blendRate));
            float minGuaranteedLead = progress < 0.92f
                ? Math.Max(8.5f, activeTuning.BearInitialLead * 0.88f)
                : Math.Max(1.6f, activeTuning.BearCatchupThreshold * 0.85f);
            currentBearLead = Math.Max(minGuaranteedLead, currentBearLead);

            BearZ = YunaZ + currentBearLead;

            float maxLane = trackSpec.LaneHalfWidth * 0.62f;
            float targetLane = (float)(Math.Sin(TotalChaseTime * 0.78) * 0.95 + Math.Sin(TotalChaseTime * 0.34 + 0.8) * 0.25);
            targetLane = Math.Max(-maxLane, Math.Min(maxLane, targetLane));
            BearLaneOffsetX = Lerp(BearLaneOffsetX, targetLane, Math.Min(1f, dt * 3.2f));
        }

        private void CheckThrownToyCollisions(float prevYunaZ, float currentYunaZ)
        {
            float r = activeTuning.YunaCollisionRadius;
            var toys = throwScheduler.ActiveToys;
            for (int i = 0; i < toys.Count; i++)
            {
                BearChaseThrownToyState toy = toys[i];
                if (toy.WasHit || toy.Phase == BearChaseThrownToyPhase.Deflected || toy.Phase == BearChaseThrownToyPhase.Expired)
                {
                    continue;
                }

                if (toy.Phase == BearChaseThrownToyPhase.InFlight && (toy.NormalizedFlight < 0.85f || toy.HeightAboveGround > 0.90f))
                {
                    continue;
                }

                BearChaseObstacleDef def = BearChaseObstacleCatalog.Get(toy.Kind);
                float minZ = toy.CurrentZ - def.HalfDepth - r;
                float maxZ = toy.CurrentZ + def.HalfDepth + r;
                float sweepMinZ = Math.Min(prevYunaZ, currentYunaZ);
                float sweepMaxZ = Math.Max(prevYunaZ, currentYunaZ);

                if (sweepMaxZ < minZ || sweepMinZ > maxZ)
                {
                    continue;
                }

                float dx = Math.Abs(YunaLaneOffsetX - toy.CurrentLaneX);
                if (dx <= def.HalfWidth + r)
                {
                    float deflectDir = toy.CurrentLaneX >= YunaLaneOffsetX ? 1f : -1f;
                    throwScheduler.MarkToyDeflected(toy, deflectDir);
                    ApplyObstacleHit(toy);
                    return;
                }
            }
        }

        private void ApplyObstacleHit(BearChaseThrownToyState toy)
        {
            Hearts = Math.Max(0, Hearts - 1);
            InvulnerabilityRemaining = activeTuning.InvulnerabilityDuration;
            OnHeartsChanged?.Invoke(Hearts, MaxHearts);
            OnObstacleHit?.Invoke(toy);

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
