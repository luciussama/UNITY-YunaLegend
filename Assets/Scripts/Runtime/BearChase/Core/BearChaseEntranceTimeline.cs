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
        public readonly float YunaWalkBobY;
        public readonly float YunaScaleX;
        public readonly float YunaScaleY;
        public readonly float YunaTiltDeg;
        public readonly bool BearFlipX;
        public readonly float BearScaleX;
        public readonly float BearScaleY;
        public readonly float BearTiltDeg;

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
            bool wakeCueActive,
            float yunaWalkBobY = 0f,
            float yunaScaleX = 1f,
            float yunaScaleY = 1f,
            float yunaTiltDeg = 0f,
            bool bearFlipX = false,
            float bearScaleX = 1f,
            float bearScaleY = 1f,
            float bearTiltDeg = 0f)
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
            YunaWalkBobY = yunaWalkBobY;
            YunaScaleX = yunaScaleX;
            YunaScaleY = yunaScaleY;
            YunaTiltDeg = yunaTiltDeg;
            BearFlipX = bearFlipX;
            BearScaleX = bearScaleX;
            BearScaleY = bearScaleY;
            BearTiltDeg = bearTiltDeg;
        }
    }

    /// <summary>
    /// Avaliador puro da cinematica de entrada (~6.5s):
    /// Estabelecimento -> Aproximacao (com parada natural) -> Alcance/Toque (pes plantados) -> Despertar Subito (salto de recuo) -> Corte preto pleno.
    /// </summary>
    public static class BearChaseEntranceTimeline
    {
        public const float EstablishEnd = 0.60f;
        public const float WalkMoveEnd = 3.70f;
        public const float ApproachEnd = 4.00f;
        public const float ReachEnd = 5.00f;
        public const float WakeUpEnd = 6.00f;
        public const float TotalDuration = 6.50f;

        public const float YunaStartX = 0.195f;
        public const float YunaReachX = 0.495f;
        public const float BearRestX = 0.695f;
        public const float FairyStartX = 0.130f;
        public const float FairyStartY = 0.625f;
        public const int ApproachWalkFrameCount = 8;
        public const int ApproachStandFrameIndex = 7;
        private const int ApproachSteps = 8;

        public static float ApproachSupportX(int stepCueNumber)
        {
            float stepProgress = (stepCueNumber - 0.2f) / ApproachSteps;
            return Lerp(YunaStartX, YunaReachX, stepProgress);
        }

        public static BearChaseEntranceSample Evaluate(float elapsedSeconds)
        {
            float t = Math.Max(0f, Math.Min(TotalDuration, elapsedSeconds));
            if (t < TotalDuration)
            {
                const int fps = BearChaseHandDrawnTiming.FramesPerSecond;
                t = (float)Math.Floor(t * fps) / fps;
            }

            float wing = 0.58f + 0.42f * (float)Math.Abs(Math.Cos(t * 15.0));
            float bob = (float)Math.Sin(t * 4.2) * 0.012f;

            // Respiracao suave do urso dormindo (orientado com cabeca voltada a esquerda para continuidade com despertar)
            float sleepBreath = (float)Math.Sin(t * 2.35);
            float sleepBearScaleY = 1f + sleepBreath * 0.016f;
            float sleepBearScaleX = 1f - sleepBreath * 0.008f;

            if (t < EstablishEnd)
            {
                float idleBreathY = 1f + (float)Math.Sin(t * 3.0) * 0.006f;
                return new BearChaseEntranceSample(
                    BearChaseEntranceBeat.Establish,
                    YunaStartX,
                    BearRestX,
                    FairyStartX,
                    FairyStartY + bob,
                    wing,
                    bob,
                    useReachAtlas: false,
                    girlFrameIndex: ApproachStandFrameIndex,
                    bearFrameIndex: 0,
                    bearStartleOffsetY: 0f,
                    fadeAlpha: 0f,
                    stepCueNumber: 0,
                    wakeCueActive: false,
                    yunaWalkBobY: 0f,
                    yunaScaleX: 1f,
                    yunaScaleY: idleBreathY,
                    yunaTiltDeg: 0f,
                    bearFlipX: true,
                    bearScaleX: sleepBearScaleX,
                    bearScaleY: sleepBearScaleY,
                    bearTiltDeg: 0f);
            }

            if (t < ApproachEnd)
            {
                // Caminhada ativa (0.60s -> 3.70s) seguida de parada estavel em f1 (3.70s -> 4.00s) sem pop de 1 quadro
                if (t >= WalkMoveEnd)
                {
                    float settleBreathY = 1f + (float)Math.Sin(t * 3.0) * 0.005f;
                    return new BearChaseEntranceSample(
                        BearChaseEntranceBeat.Approach,
                        YunaReachX,
                        BearRestX,
                        0.425f,
                        0.635f + bob,
                        wing,
                        bob,
                        useReachAtlas: false,
                        girlFrameIndex: ApproachStandFrameIndex,
                        bearFrameIndex: 0,
                        bearStartleOffsetY: 0f,
                        fadeAlpha: 0f,
                        stepCueNumber: 8,
                        wakeCueActive: false,
                        yunaWalkBobY: 0f,
                        yunaScaleX: 1f,
                        yunaScaleY: settleBreathY,
                        yunaTiltDeg: 0f,
                        bearFlipX: true,
                        bearScaleX: sleepBearScaleX,
                        bearScaleY: sleepBearScaleY,
                        bearTiltDeg: 0f);
                }

                float uWalk = (t - EstablishEnd) / (WalkMoveEnd - EstablishEnd);
                float baseProgress = EvaluateWalkProgress(uWalk);

                // 8 passos completos (4 ciclos de 4 quadros), iniciando o primeiro passo imediatamente ao mover
                float stepFloat = baseProgress * ApproachSteps;
                int stepIndex = Math.Min(ApproachSteps - 1, (int)Math.Floor(stepFloat));
                float stepLocal = stepFloat - stepIndex;
                float yunaX = Lerp(YunaStartX, YunaReachX, baseProgress);
                float fairyX = Lerp(FairyStartX, 0.425f, baseProgress);
                float fairyY = Lerp(FairyStartY, 0.635f, baseProgress) + (float)Math.Sin(uWalk * Math.PI) * 0.032f + bob;

                int walkFrame;
                int stepCue = stepIndex + 1;

                if (baseProgress < 0.001f)
                {
                    // Primeiros milissegundos (dx < 1 px) mantem pose inicial antes da passada imediata
                    walkFrame = ApproachStandFrameIndex;
                    stepCue = 0;
                }
                else
                {
                    int subPhase = Math.Min(3, (int)Math.Floor(stepLocal * 4f));
                    bool isEvenStep = (stepIndex % 2) == 0;
                    walkFrame = (isEvenStep ? 0 : 4) + subPhase;
                }

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
                    wakeCueActive: false,
                    yunaWalkBobY: 0f,
                    yunaScaleX: 1f,
                    yunaScaleY: 1f,
                    yunaTiltDeg: 0f,
                    bearFlipX: true,
                    bearScaleX: sleepBearScaleX,
                    bearScaleY: sleepBearScaleY,
                    bearTiltDeg: 0f);
            }

            if (t < ReachEnd)
            {
                float u = (t - ApproachEnd) / (ReachEnd - ApproachEnd);
                float smoothU = SmoothStep(u);
                int reachFrame = u < 0.30f ? 0 : (u < 0.58f ? 1 : 2);
                // Pes 100% plantados no mesmo ponto do tapete durante todo o alcance (zero foot-sliding)
                float fairyX = Lerp(0.425f, 0.445f, smoothU);
                float fairyY = 0.640f + bob;
                float reachTilt = -2.0f * smoothU;

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
                    wakeCueActive: false,
                    yunaWalkBobY: 0f,
                    yunaScaleX: 1f,
                    yunaScaleY: 1f,
                    yunaTiltDeg: reachTilt,
                    bearFlipX: true,
                    bearScaleX: sleepBearScaleX,
                    bearScaleY: sleepBearScaleY,
                    bearTiltDeg: 0f);
            }

            if (t < WakeUpEnd)
            {
                float u = (t - ReachEnd) / (WakeUpEnd - ReachEnd);

                // Yuna: 0..0.08 recolhe a mao com pes plantados -> 0.08..0.36 saltinho de susto para tras no ar -> 0.36..1.00 pousada firme
                int recoilGirlFrame;
                float yunaX;
                float yunaHopY = 0f;
                float yunaScaleX = 1f;
                float yunaScaleY = 1f;
                float yunaTiltDeg = 0f;

                if (u < 0.08f)
                {
                    recoilGirlFrame = 1;
                    yunaX = YunaReachX;
                    yunaTiltDeg = Lerp(-2.0f, 0f, u / 0.08f);
                }
                else if (u < 0.36f)
                {
                    recoilGirlFrame = 3;
                    float hopU = (u - 0.08f) / 0.28f;
                    float hopArc = (float)Math.Sin(hopU * Math.PI);
                    yunaX = Lerp(YunaReachX, YunaReachX - 0.048f, SmoothStep(hopU));
                    yunaHopY = hopArc * 0.036f;
                    yunaScaleX = 1f - hopArc * 0.028f;
                    yunaScaleY = 1f + hopArc * 0.036f;
                    yunaTiltDeg = hopArc * 3.5f;
                }
                else
                {
                    recoilGirlFrame = 3;
                    yunaX = YunaReachX - 0.048f;
                    float settleU = Math.Min(1f, (u - 0.36f) / 0.14f);
                    float landSquash = (float)Math.Sin(settleU * Math.PI);
                    yunaScaleX = 1f + landSquash * 0.035f;
                    yunaScaleY = 1f - landSquash * 0.042f;
                }

                // Urso: despertar organico com antecipacao, sobressalto, squash-and-stretch e deslocamento apenas durante os saltos
                int bearFrame;
                float startleHop = 0f;
                float bearX;
                float bearScaleX = 1f;
                float bearScaleY = 1f;
                float bearTiltDeg = 0f;

                if (u < 0.28f)
                {
                    bearFrame = 1;
                    float p1 = u / 0.28f;
                    float arc1 = (float)Math.Sin(p1 * Math.PI);
                    startleHop = arc1 * 0.024f;
                    bearX = Lerp(BearRestX, BearRestX + 0.014f, SmoothStep(p1));
                    if (p1 < 0.20f)
                    {
                        float sq = p1 / 0.20f;
                        bearScaleX = 1f + 0.065f * sq;
                        bearScaleY = 1f - 0.075f * sq;
                    }
                    else
                    {
                        float st = (float)Math.Sin(((p1 - 0.20f) / 0.80f) * Math.PI);
                        bearScaleX = 1f - 0.055f * st;
                        bearScaleY = 1f + 0.075f * st;
                    }

                    bearTiltDeg = -4.0f * arc1;
                }
                else if (u < 0.60f)
                {
                    bearFrame = 2;
                    float p2 = (u - 0.28f) / 0.32f;
                    float lift2 = p2 < 0.65f ? (float)Math.Sin((p2 / 0.65f) * Math.PI) : 0f;
                    startleHop = lift2 * 0.014f;
                    bearX = Lerp(BearRestX + 0.014f, BearRestX + 0.026f, SmoothStep(Math.Min(1f, p2 / 0.65f)));
                    if (p2 < 0.45f)
                    {
                        float st = (float)Math.Sin((p2 / 0.45f) * Math.PI);
                        bearScaleX = 1f - 0.045f * st;
                        bearScaleY = 1f + 0.060f * st;
                    }
                    else
                    {
                        float sq = (float)Math.Sin(((p2 - 0.45f) / 0.55f) * Math.PI);
                        bearScaleX = 1f + 0.055f * sq;
                        bearScaleY = 1f - 0.052f * sq;
                    }

                    bearTiltDeg = 3.0f * (float)Math.Sin(p2 * Math.PI);
                }
                else
                {
                    bearFrame = 3;
                    float p3 = (u - 0.60f) / 0.40f;
                    if (p3 < 0.48f)
                    {
                        float hop3 = p3 / 0.48f;
                        float arc3 = (float)Math.Sin(hop3 * Math.PI);
                        startleHop = arc3 * 0.022f;
                        bearX = Lerp(BearRestX + 0.026f, BearRestX + 0.042f, SmoothStep(hop3));
                        bearScaleX = 1f - 0.040f * arc3;
                        bearScaleY = 1f + 0.055f * arc3;
                        bearTiltDeg = -3.0f * arc3;
                    }
                    else
                    {
                        bearX = BearRestX + 0.042f;
                        float landU = Math.Min(1f, (p3 - 0.48f) / 0.30f);
                        float sq = (float)Math.Sin(landU * Math.PI);
                        bearScaleX = 1f + 0.048f * sq;
                        bearScaleY = 1f - 0.046f * sq;
                    }
                }

                float fairyRecoil = SmoothStep(Math.Min(1f, u * 2.4f));
                float fairyX = Lerp(0.445f, 0.360f, fairyRecoil);
                float fairyY = Lerp(0.640f, 0.715f, fairyRecoil) + bob;

                return new BearChaseEntranceSample(
                    BearChaseEntranceBeat.BearWakeUp,
                    yunaX,
                    bearX,
                    fairyX,
                    fairyY,
                    wing,
                    bob,
                    useReachAtlas: true,
                    girlFrameIndex: recoilGirlFrame,
                    bearFrameIndex: bearFrame,
                    bearStartleOffsetY: startleHop,
                    fadeAlpha: 0f,
                    stepCueNumber: 0,
                    wakeCueActive: true,
                    yunaWalkBobY: yunaHopY,
                    yunaScaleX: yunaScaleX,
                    yunaScaleY: yunaScaleY,
                    yunaTiltDeg: yunaTiltDeg,
                    bearFlipX: false,
                    bearScaleX: bearScaleX,
                    bearScaleY: bearScaleY,
                    bearTiltDeg: bearTiltDeg);
            }

            // Fade-out atinge preto pleno (100%) em t=6.38s e sustenta preto solido antes do corte
            float fadeU = Math.Max(0f, Math.Min(1f, (t - WakeUpEnd) / 0.38f));
            float smoothFade = SmoothStep(fadeU);
            return new BearChaseEntranceSample(
                BearChaseEntranceBeat.TransitionToChase,
                YunaReachX - 0.048f,
                BearRestX + 0.042f,
                0.360f,
                0.715f + bob,
                wing,
                bob,
                useReachAtlas: true,
                girlFrameIndex: 3,
                bearFrameIndex: 3,
                bearStartleOffsetY: 0f,
                fadeAlpha: smoothFade,
                stepCueNumber: 0,
                wakeCueActive: false,
                yunaWalkBobY: 0f,
                yunaScaleX: 1f,
                yunaScaleY: 1f,
                yunaTiltDeg: 0f,
                bearFlipX: false,
                bearScaleX: 1f,
                bearScaleY: 1f,
                bearTiltDeg: 0f);
        }

        private static float EvaluateWalkProgress(float u)
        {
            const float r = 0.05f;
            float clamped = Math.Max(0f, Math.Min(1f, u));
            float vMax = 1f / (1f - r);

            if (clamped < r)
            {
                return 0.5f * vMax * (clamped * clamped / r);
            }

            if (clamped <= 1f - r)
            {
                return 0.5f * vMax * r + vMax * (clamped - r);
            }

            float tail = 1f - clamped;
            return 1f - 0.5f * vMax * (tail * tail / r);
        }

        private static float SmoothStep(float x)
        {
            float c = Math.Max(0f, Math.Min(1f, x));
            return c * c * (3f - 2f * c);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Max(0f, Math.Min(1f, t));
        }
    }
}
