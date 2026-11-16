using System;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Avaliador puro (C# sem dependências de engine) da cinemática animada de abertura
    /// em cadência fixa de 30 fps (Yuna dormindo na cama -> Nanda azulada -> despertar/sair da cama -> gameplay).
    /// </summary>
    public static class OpeningCinematicTimeline
    {
        public const float TargetFrameRate = 30f;

        public readonly struct Sample
        {
            public readonly bool Active;
            public readonly float QuantizedTime;
            public readonly float CameraOffsetX;
            public readonly float CameraOffsetY;
            public readonly float CameraSpanScale;
            public readonly float NandaOffsetX;
            public readonly float NandaOffsetY;
            public readonly float NandaOffsetZ;
            public readonly float NandaLightStrength;
            public readonly bool OverrideYunaPose;
            public readonly float YunaBlink;
            public readonly float YunaLean;
            public readonly float YunaHeadTilt;
            public readonly float YunaBreathe;
            public readonly float YunaSquashX;
            public readonly float YunaSquashY;
            public readonly float ChapterBannerAlpha;
            public readonly bool YunaInBed;
            public readonly float BedSitProgress;
            public readonly float BedExitProgress;
            public readonly float CurtainFadeAlpha;
            public readonly int DialogueBeat;

            public Sample(
                bool active,
                float quantizedTime,
                float cameraOffsetX,
                float cameraOffsetY,
                float cameraSpanScale,
                float nandaOffsetX,
                float nandaOffsetY,
                float nandaOffsetZ,
                float nandaLightStrength,
                bool overrideYunaPose,
                float yunaBlink,
                float yunaLean,
                float yunaHeadTilt,
                float yunaBreathe,
                float yunaSquashX,
                float yunaSquashY,
                float chapterBannerAlpha,
                bool yunaInBed = false,
                float bedSitProgress = 0f,
                float bedExitProgress = 0f,
                float curtainFadeAlpha = 0f,
                int dialogueBeat = 0)
            {
                Active = active;
                QuantizedTime = quantizedTime;
                CameraOffsetX = cameraOffsetX;
                CameraOffsetY = cameraOffsetY;
                CameraSpanScale = cameraSpanScale;
                NandaOffsetX = nandaOffsetX;
                NandaOffsetY = nandaOffsetY;
                NandaOffsetZ = nandaOffsetZ;
                NandaLightStrength = nandaLightStrength;
                OverrideYunaPose = overrideYunaPose;
                YunaBlink = yunaBlink;
                YunaLean = yunaLean;
                YunaHeadTilt = yunaHeadTilt;
                YunaBreathe = yunaBreathe;
                YunaSquashX = yunaSquashX;
                YunaSquashY = yunaSquashY;
                ChapterBannerAlpha = chapterBannerAlpha;
                YunaInBed = yunaInBed;
                BedSitProgress = bedSitProgress;
                BedExitProgress = bedExitProgress;
                CurtainFadeAlpha = curtainFadeAlpha;
                DialogueBeat = dialogueBeat;
            }
        }

        public static float Quantize30Fps(float seconds)
        {
            if (seconds <= 0f)
            {
                return 0f;
            }

            return (float)(Math.Floor(seconds * TargetFrameRate) / TargetFrameRate);
        }

        public static Sample Evaluate(StageFlowStateMachine.FlowPhase phase, float phaseSeconds)
        {
            float t = Quantize30Fps(phaseSeconds);

            switch (phase)
            {
                case StageFlowStateMachine.FlowPhase.OpeningNandaWake1:
                    return EvaluateWake1(t);

                case StageFlowStateMachine.FlowPhase.OpeningPoeticIntro:
                    return EvaluatePoetic(t);

                case StageFlowStateMachine.FlowPhase.OpeningNandaWake2:
                    return EvaluateWake2(t);

                case StageFlowStateMachine.FlowPhase.PreJumpCue:
                    return EvaluatePreJumpHandoff(t);

                default:
                    return new Sample(
                        active: false,
                        quantizedTime: t,
                        cameraOffsetX: 0f,
                        cameraOffsetY: 0f,
                        cameraSpanScale: 1f,
                        nandaOffsetX: 1.65f,
                        nandaOffsetY: 1.40f,
                        nandaOffsetZ: -0.15f,
                        nandaLightStrength: 1f,
                        overrideYunaPose: false,
                        yunaBlink: 0f,
                        yunaLean: 0f,
                        yunaHeadTilt: 0f,
                        yunaBreathe: 0f,
                        yunaSquashX: 1f,
                        yunaSquashY: 1f,
                        chapterBannerAlpha: 0f);
            }
        }

        private static Sample EvaluateWake1(float t)
        {
            // Fade-in acolhedor do quarto escuro + dolly suave até a cama onde Yuna dorme.
            float curtain = 1f - SmoothStep(0f, 1.0f, t);
            float dolly = SmoothStep(0f, 3.4f, t);
            float camX = Lerp(2.10f, -0.35f, dolly);
            float camY = Lerp(0.22f, 0.08f, dolly);
            float camScale = Lerp(1.10f, 0.78f, dolly);

            // Nanda entra devagarinho e pousa/paira com carinho perto do rosto de Yuna no travesseiro.
            float enter = SmoothStep(0.2f, 2.2f, t);
            float gentleSway = (float)Math.Sin(t * 1.6f) * 0.05f;
            float hover = (float)Math.Sin(t * 2.2f) * 0.06f;
            float nandaX = Lerp(2.80f, -0.18f, enter) + gentleSway;
            float nandaY = Lerp(2.05f, 1.38f, enter) + hover;

            float breathCycle = (float)Math.Sin(t * 2.0f);
            return new Sample(
                active: true,
                quantizedTime: t,
                cameraOffsetX: camX,
                cameraOffsetY: camY,
                cameraSpanScale: camScale,
                nandaOffsetX: nandaX,
                nandaOffsetY: nandaY,
                nandaOffsetZ: -0.18f,
                nandaLightStrength: 0.85f + 0.12f * enter + 0.04f * (float)Math.Sin(t * 2.4f),
                overrideYunaPose: true,
                yunaBlink: 1f,
                yunaLean: 0.65f + 0.25f * breathCycle,
                yunaHeadTilt: 2.2f + 0.45f * breathCycle,
                yunaBreathe: 0.014f * (0.5f + 0.5f * breathCycle),
                yunaSquashX: 1f,
                yunaSquashY: 1f,
                chapterBannerAlpha: 0f,
                yunaInBed: true,
                bedSitProgress: 0f,
                bedExitProgress: 0f,
                curtainFadeAlpha: curtain,
                dialogueBeat: 0);
        }

        private static Sample EvaluatePoetic(float t)
        {
            // Plano intimista sobre a cama enquanto Nanda faz um arco afetuoso acima da irmã adormecida.
            float drift = SmoothStep(0f, 4.4f, t);
            float camX = Lerp(-0.35f, -0.28f, drift);
            float camY = Lerp(0.08f, 0.06f, drift);
            float camScale = Lerp(0.78f, 0.82f, drift);

            float arc = t * 1.35f;
            float nandaX = -0.22f + (float)Math.Sin(arc) * 0.28f;
            float nandaY = 1.42f + (float)Math.Sin(arc * 2.0f) * 0.07f;

            float stir = (float)Math.Sin(t * 1.6f);
            int beat = t < 2.15f ? 0 : 1;
            return new Sample(
                active: true,
                quantizedTime: t,
                cameraOffsetX: camX,
                cameraOffsetY: camY,
                cameraSpanScale: camScale,
                nandaOffsetX: nandaX,
                nandaOffsetY: nandaY,
                nandaOffsetZ: -0.18f,
                nandaLightStrength: 0.94f + 0.05f * (float)Math.Sin(t * 3.0f),
                overrideYunaPose: true,
                yunaBlink: 1f,
                yunaLean: 0.55f + 0.35f * stir,
                yunaHeadTilt: 1.8f - 0.8f * stir,
                yunaBreathe: 0.015f * (0.5f + 0.5f * stir),
                yunaSquashX: 1f,
                yunaSquashY: 1f,
                chapterBannerAlpha: 0f,
                yunaInBed: true,
                bedSitProgress: 0f,
                bedExitProgress: 0f,
                curtainFadeAlpha: 0f,
                dialogueBeat: beat);
        }

        private static Sample EvaluateWake2(float t)
        {
            // Nanda desce carinhosamente perto do rosto de Yuna, pulsa luz azulada suave e Yuna desperta.
            float dive = SmoothStep(0f, 0.55f, t);
            float wakeStartle = SmoothStep(1.90f, 2.75f, t);

            float nudgeEnv = t >= 0.25f && t < 0.85f ? (float)Math.Sin((t - 0.25f) / 0.60f * Math.PI) : 0f;
            float bounceY = (float)Math.Sin(t * 18f) * 0.025f * nudgeEnv;

            float camX = Lerp(-0.28f, -0.45f, dive);
            float camY = Lerp(0.06f, 0.02f, dive) + bounceY;
            float camScale = Lerp(0.82f, 0.70f, dive);

            float nandaX = Lerp(-0.22f, -0.32f, dive) + (float)Math.Sin(t * 5.5f) * 0.04f;
            float nandaY = Lerp(1.42f, 1.22f, dive) + (float)Math.Sin(t * 4.5f) * 0.04f;
            float lightPulse = 1.0f + 0.16f * dive * (1f - wakeStartle * 0.4f);

            int beat = t < 1.65f ? 0 : 1;
            return new Sample(
                active: true,
                quantizedTime: t,
                cameraOffsetX: camX,
                cameraOffsetY: camY,
                cameraSpanScale: camScale,
                nandaOffsetX: nandaX,
                nandaOffsetY: nandaY,
                nandaOffsetZ: -0.20f,
                nandaLightStrength: lightPulse,
                overrideYunaPose: true,
                yunaBlink: 1f - wakeStartle,
                yunaLean: 0f,
                yunaHeadTilt: 0f,
                yunaBreathe: 0.012f,
                yunaSquashX: 1f,
                yunaSquashY: 1f,
                chapterBannerAlpha: 0f,
                yunaInBed: true,
                bedSitProgress: wakeStartle * 0.16f,
                bedExitProgress: 0f,
                curtainFadeAlpha: 0f,
                dialogueBeat: beat);
        }

        private static Sample EvaluatePreJumpHandoff(float t)
        {
            // 0.00..0.85s: Yuna senta na beira da cama e cobertor desliza.
            // 0.80..1.45s: Yuna desce da cama para o chão.
            // 1.25..2.25s: Transição curta + cartão CAPÍTULO 1 revela cena jogável (sem cama).
            float sitProgress = Lerp(0.16f, 1f, SmoothStep(0f, 0.85f, t));
            float exitProgress = SmoothStep(0.80f, 1.45f, t);
            bool yunaInBed = t < 1.50f;

            float curtainIn = SmoothStep(1.25f, 1.52f, t);
            float curtainOut = 1f - SmoothStep(1.85f, 2.25f, t);
            float curtainAlpha = Clamp01(curtainIn * curtainOut) * 0.92f;

            float handoff = SmoothStep(0.60f, 2.10f, t);
            float camX = Lerp(-0.45f, 0f, handoff);
            float camY = Lerp(0.02f, 0f, handoff);
            float camScale = Lerp(0.70f, 1f, handoff);

            float nandaX = Lerp(-0.32f, 1.65f, handoff);
            float nandaY = Lerp(1.22f, 1.40f, handoff);

            float bannerIn = SmoothStep(1.25f, 1.55f, t);
            float bannerOut = 1f - SmoothStep(2.20f, 2.75f, t);
            float bannerAlpha = Clamp01(bannerIn * bannerOut);

            bool stillHandingOff = t < 2.25f;
            return new Sample(
                active: stillHandingOff,
                quantizedTime: t,
                cameraOffsetX: camX,
                cameraOffsetY: camY,
                cameraSpanScale: camScale,
                nandaOffsetX: nandaX,
                nandaOffsetY: nandaY,
                nandaOffsetZ: -0.15f,
                nandaLightStrength: 1f,
                overrideYunaPose: false,
                yunaBlink: 0f,
                yunaLean: 0f,
                yunaHeadTilt: 0f,
                yunaBreathe: 0f,
                yunaSquashX: 1f,
                yunaSquashY: 1f,
                chapterBannerAlpha: bannerAlpha,
                yunaInBed: yunaInBed,
                bedSitProgress: sitProgress,
                bedExitProgress: exitProgress,
                curtainFadeAlpha: curtainAlpha,
                dialogueBeat: 0);
        }

        private static float SmoothStep(float edge0, float edge1, float x)
        {
            if (edge1 <= edge0)
            {
                return x >= edge1 ? 1f : 0f;
            }

            float u = Clamp01((x - edge0) / (edge1 - edge0));
            return u * u * (3f - 2f * u);
        }

        private static float Clamp01(float v)
        {
            if (v < 0f) return 0f;
            if (v > 1f) return 1f;
            return v;
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }
    }
}
