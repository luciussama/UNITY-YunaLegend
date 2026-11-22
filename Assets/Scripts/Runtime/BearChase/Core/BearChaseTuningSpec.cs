namespace YunaLegend.BearChase.Core
{
    /// <summary>
    /// Descritor data-driven de ritmo, velocidade, vidas e arremessos da Fase 4.
    /// </summary>
    public sealed class BearChaseTuningSpec
    {
        public static readonly BearChaseTuningSpec Default = new BearChaseTuningSpec();

        public float BaseForwardSpeed { get; }
        public float MaxForwardSpeed { get; }
        public float LateralSpeed { get; }
        public float YunaCollisionRadius { get; }

        public float BearInitialLead { get; }
        public float BearTargetLead { get; }
        public float BearCatchupThreshold { get; }

        public int NormalMaxHearts { get; }
        public int EasyMaxHearts { get; }
        public int FailuresToOfferEasyMode { get; }
        public float StumbleSpeedFactor { get; }
        public float StumbleDuration { get; }
        public float InvulnerabilityDuration { get; }
        public float FallenBannerDuration { get; }

        public float ThrowWindupDuration { get; }
        public float ThrowIntervalStart { get; }
        public float ThrowIntervalEnd { get; }
        public float EasyModeThrowMultiplier { get; }

        public BearChaseTuningSpec(
            float baseForwardSpeed = 11.4f,
            float maxForwardSpeed = 15.2f,
            float lateralSpeed = 9.4f,
            float yunaCollisionRadius = 0.44f,
            float bearInitialLead = 10.0f,
            float bearTargetLead = 12.2f,
            float bearCatchupThreshold = 2.0f,
            int normalMaxHearts = 3,
            int easyMaxHearts = 6,
            int failuresToOfferEasyMode = 2,
            float stumbleSpeedFactor = 0.62f,
            float stumbleDuration = 0.50f,
            float invulnerabilityDuration = 1.35f,
            float fallenBannerDuration = 1.05f,
            float throwWindupDuration = 0.34f,
            float throwIntervalStart = 1.50f,
            float throwIntervalEnd = 0.92f,
            float easyModeThrowMultiplier = 1.0f)
        {
            BaseForwardSpeed = baseForwardSpeed;
            MaxForwardSpeed = maxForwardSpeed;
            LateralSpeed = lateralSpeed;
            YunaCollisionRadius = yunaCollisionRadius;
            BearInitialLead = bearInitialLead;
            BearTargetLead = bearTargetLead;
            BearCatchupThreshold = bearCatchupThreshold;
            NormalMaxHearts = normalMaxHearts;
            EasyMaxHearts = easyMaxHearts;
            FailuresToOfferEasyMode = failuresToOfferEasyMode;
            StumbleSpeedFactor = stumbleSpeedFactor;
            StumbleDuration = stumbleDuration;
            InvulnerabilityDuration = invulnerabilityDuration;
            FallenBannerDuration = fallenBannerDuration;
            ThrowWindupDuration = throwWindupDuration;
            ThrowIntervalStart = throwIntervalStart;
            ThrowIntervalEnd = throwIntervalEnd;
            EasyModeThrowMultiplier = easyModeThrowMultiplier;
        }

        public BearChaseTuningSpec WithEasyModeSpacing()
        {
            return new BearChaseTuningSpec(
                BaseForwardSpeed,
                MaxForwardSpeed,
                LateralSpeed,
                YunaCollisionRadius,
                BearInitialLead,
                BearTargetLead,
                BearCatchupThreshold,
                NormalMaxHearts,
                EasyMaxHearts,
                FailuresToOfferEasyMode,
                StumbleSpeedFactor,
                StumbleDuration,
                InvulnerabilityDuration,
                FallenBannerDuration,
                ThrowWindupDuration,
                ThrowIntervalStart,
                ThrowIntervalEnd,
                easyModeThrowMultiplier: 1.24f);
        }

        public float EvaluateForwardSpeed(float normalizedProgress)
        {
            float p = normalizedProgress < 0f ? 0f : (normalizedProgress > 1f ? 1f : normalizedProgress);
            return BaseForwardSpeed + (MaxForwardSpeed - BaseForwardSpeed) * p;
        }
    }
}
