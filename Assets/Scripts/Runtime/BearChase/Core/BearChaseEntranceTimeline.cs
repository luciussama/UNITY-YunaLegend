using System;

namespace YunaLegend.BearChase.Core
{
    public enum BearChaseEntranceBeat
    {
        Establish = 0,
        Approach = 1,
        Reach = 2,
        BearWakeUp = 3,
        TransitionToChase = 4
    }

    /// <summary>
    /// Amostra pura e deterministica de um instante da cinematica 2D ilustrada de entrada.
    /// Todas as posicoes usam coordenadas normalizadas nativas de viewport [0..1].
    /// </summary>
    public readonly struct BearChaseEntranceSample
    {
        public readonly BearChaseEntranceBeat Beat;
        public readonly float NormalizedYunaX;
        public readonly float NormalizedBearX;
        public readonly float NormalizedFairyX;
        public readonly float NormalizedFairyY;
        public readonly float FairyWingScaleX;
        public readonly float FairyBobOffset;
        public readonly bool UseReachAtlas;
        public readonly int GirlFrameIndex;
        public readonly int BearFrameIndex;
        public readonly float BearStartleOffsetY;
        public readonly float FadeAlpha;
        public readonly int StepCueNumber;
        public readonly bool WakeCueActive;

        public BearChaseEntranceSample(
            BearChaseEntranceBeat beat,
            float normalizedYunaX,
            float normalizedBearX,
            float normalizedFairyX,
            float normalizedFairyY,
            float fairyWingScaleX,
            float fairyBobOffset,
            bool useReachAtlas,
            int girlFrameIndex,
            int bearFrameIndex,
            float bearStartleOffsetY,
            float fadeAlpha,
            int stepCueNumber,
            bool wakeCueActive)
        {
            Beat = beat;
            NormalizedYunaX = normalizedYunaX;
            NormalizedBearX = normalizedBearX;
            NormalizedFairyX = normalizedFairyX;
            NormalizedFairyY = normalizedFairyY;
            FairyWingScaleX = fairyWingScaleX;
            FairyBobOffset = fairyBobOffset;
            UseReachAtlas = useReachAtlas;
            GirlFrameIndex = girlFrameIndex;
            BearFrameIndex = bearFrameIndex;
            BearStartleOffsetY = bearStartleOffsetY;
            FadeAlpha = fadeAlpha;
            StepCueNumber = stepCueNumber;
            WakeCueActive = wakeCueActive;
        }
    }

    /// <summary>
    /// Avaliador puro da cinematica de entrada (~6.5s):
    /// Estabelecimento -> Aproximacao -> Alcance/Toque -> Despertar Subito -> Corte.
    /// </summary>
    public static class BearChaseEntranceTimeline
    {
        public const float EstablishEnd = 0.60f;
        public const float ApproachEnd = 4.00f;
        public const float ReachEnd = 5.00f;
        public const float WakeUpEnd = 6.00f;
        public const float TotalDuration = 6.50f;

        public const float YunaStartX = 0.215f;
        public const float YunaReachX = 0.525f;
        public const float BearRestX = 0.695f;
        public const float FairyStartX = 0.195f;
        public const float FairyStartY = 0.560f;

        public static BearChaseEntranceSample Evaluate(float elapsedSeconds)
        {
            float t = Math.Max(0f, Math.Min(TotalDuration, elapsedSeconds));
            float wing = 0.62f + 0.38f * (float)Math.Abs(Math.Cos(t * 14.0));
            float bob = (float)Math.Sin(t * 4.2) * 0.012f;

            if (t < EstablishEnd)
            {
                return new BearChaseEntranceSample(
                    BearChaseEntranceBeat.Establish,
                    YunaStartX,
                    BearRestX,
                    FairyStartX,
                    FairyStartY + bob,
                    wing,
                    bob,
                    useReachAtlas: false,
                    girlFrameIndex: 1,
                    bearFrameIndex: 0,
                    bearStartleOffsetY: 0f,
                    fadeAlpha: 0f,
                    stepCueNumber: 0,
                    wakeCueActive: false);
            }

            if (t < ApproachEnd)
            {
                float u = (t - EstablishEnd) / (ApproachEnd - EstablishEnd);
                float smoothU = u * u * (3f - 2f * u);
                float yunaX = Lerp(YunaStartX, YunaReachX, smoothU);
                float fairyX = Lerp(FairyStartX, 0.455f, smoothU);
                float fairyY = Lerp(FairyStartY, 0.605f, smoothU) + (float)Math.Sin(u * Math.PI) * 0.045f + bob;
                int walkFrame = ((int)Math.Floor((t - EstablishEnd) * 5.6f)) % 4;
                int stepCue = 1 + (int)Math.Floor((t - EstablishEnd) / 0.42f);

                return new BearChaseEntranceSample(
                    BearChaseEntranceBeat.Approach,
                    yunaX,
                    BearRestX,
                    fairyX,
                    fairyY,
                    wing,
                    bob,
                    useReachAtlas: false,
                    girlFrameIndex: walkFrame,
                    bearFrameIndex: 0,
                    bearStartleOffsetY: 0f,
                    fadeAlpha: 0f,
                    stepCueNumber: stepCue,
                    wakeCueActive: false);
            }

            if (t < ReachEnd)
            {
                float u = (t - ApproachEnd) / (ReachEnd - ApproachEnd);
                int reachFrame = u < 0.35f ? 0 : (u < 0.70f ? 1 : 2);
                float fairyX = Lerp(0.455f, 0.485f, u);
                float fairyY = 0.615f + bob;

                return new BearChaseEntranceSample(
                    BearChaseEntranceBeat.Reach,
                    YunaReachX,
                    BearRestX,
                    fairyX,
                    fairyY,
                    wing,
                    bob,
                    useReachAtlas: true,
                    girlFrameIndex: reachFrame,
                    bearFrameIndex: 0,
                    bearStartleOffsetY: 0f,
                    fadeAlpha: 0f,
                    stepCueNumber: 0,
                    wakeCueActive: false);
            }

            if (t < WakeUpEnd)
            {
                float u = (t - ReachEnd) / (WakeUpEnd - ReachEnd);
                int bearFrame = u < 0.25f ? 1 : (u < 0.58f ? 2 : 3);
                float startleHop = u < 0.55f ? (float)Math.Sin((u / 0.55f) * Math.PI) * 0.038f : 0f;
                float bearX = Lerp(BearRestX, BearRestX + 0.04f, u);
                float yunaX = Lerp(YunaReachX, YunaReachX - 0.025f, Math.Min(1f, u * 1.6f));
                float fairyX = Lerp(0.485f, 0.420f, Math.Min(1f, u * 1.8f));
                float fairyY = 0.640f + bob;

                return new BearChaseEntranceSample(
                    BearChaseEntranceBeat.BearWakeUp,
                    yunaX,
                    bearX,
                    fairyX,
                    fairyY,
                    wing,
                    bob,
                    useReachAtlas: true,
                    girlFrameIndex: 3,
                    bearFrameIndex: bearFrame,
                    bearStartleOffsetY: startleHop,
                    fadeAlpha: 0f,
                    stepCueNumber: 0,
                    wakeCueActive: true);
            }

            float fadeU = (t - WakeUpEnd) / (TotalDuration - WakeUpEnd);
            return new BearChaseEntranceSample(
                BearChaseEntranceBeat.TransitionToChase,
                YunaReachX - 0.025f,
                BearRestX + 0.04f,
                0.420f,
                0.640f + bob,
                wing,
                bob,
                useReachAtlas: true,
                girlFrameIndex: 3,
                bearFrameIndex: 3,
                bearStartleOffsetY: 0f,
                fadeAlpha: Math.Max(0f, Math.Min(1f, fadeU)),
                stepCueNumber: 0,
                wakeCueActive: false);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Max(0f, Math.Min(1f, t));
        }
    }
}
