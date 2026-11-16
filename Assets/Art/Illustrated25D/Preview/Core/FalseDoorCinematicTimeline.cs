using System;

namespace YunaLegend.Illustrated25D.Environment
{
    /// <summary>
    /// Avaliador puro (C# sem dependências de engine) da cinemática a 30 fps da Porta Falsa:
    /// queda sentada -> impacto amortecido no chão -> levantar suave -> arrumar o vestido ->
    /// olhar suave de um lado para o outro durante o diálogo.
    /// </summary>
    public static class FalseDoorCinematicTimeline
    {
        public const float TargetFrameRate = 30f;

        public readonly struct Sample
        {
            public readonly bool Active;
            public readonly float QuantizedTime;
            public readonly bool UseFallSeatedActor;
            public readonly float SeatedPoseAlpha;
            public readonly float GettingUpPoseAlpha;
            public readonly float AdjustDressAlpha;
            public readonly float ActorOffsetY;
            public readonly float ActorTiltDeg;
            public readonly float FacingDirection;
            public readonly float SmoothTurnScaleX;
            public readonly float CameraOffsetX;
            public readonly float CameraOffsetY;
            public readonly float CameraSpanScale;
            public readonly float NandaLightStrength;
            public readonly bool OverrideYunaPose;
            public readonly float YunaBlink;
            public readonly float YunaLean;
            public readonly float YunaHeadTilt;
            public readonly float YunaBreathe;
            public readonly float YunaSquashX;
            public readonly float YunaSquashY;

            public Sample(
                bool active,
                float quantizedTime,
                bool useFallSeatedActor,
                float seatedPoseAlpha,
                float gettingUpPoseAlpha,
                float adjustDressAlpha,
                float actorOffsetY,
                float actorTiltDeg,
                float facingDirection,
                float smoothTurnScaleX,
                float cameraOffsetX,
                float cameraOffsetY,
                float cameraSpanScale,
                float nandaLightStrength,
                bool overrideYunaPose,
                float yunaBlink,
                float yunaLean,
                float yunaHeadTilt,
                float yunaBreathe,
                float yunaSquashX,
                float yunaSquashY)
            {
                Active = active;
                QuantizedTime = quantizedTime;
                UseFallSeatedActor = useFallSeatedActor;
                SeatedPoseAlpha = seatedPoseAlpha;
                GettingUpPoseAlpha = gettingUpPoseAlpha;
                AdjustDressAlpha = adjustDressAlpha;
                ActorOffsetY = actorOffsetY;
                ActorTiltDeg = actorTiltDeg;
                FacingDirection = facingDirection;
                SmoothTurnScaleX = smoothTurnScaleX;
                CameraOffsetX = cameraOffsetX;
                CameraOffsetY = cameraOffsetY;
                CameraSpanScale = cameraSpanScale;
                NandaLightStrength = nandaLightStrength;
                OverrideYunaPose = overrideYunaPose;
                YunaBlink = yunaBlink;
                YunaLean = yunaLean;
                YunaHeadTilt = yunaHeadTilt;
                YunaBreathe = yunaBreathe;
                YunaSquashX = yunaSquashX;
                YunaSquashY = yunaSquashY;
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
                case StageFlowStateMachine.FlowPhase.FalseDoorFalling:
                    return EvaluateFalling(t);

                case StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep0:
                    return EvaluateRecoveryAndDialogue(t);

                case StageFlowStateMachine.FlowPhase.FalseDoorCutsceneStep1:
                    return EvaluateNandaScolding(t);

                default:
                    return new Sample(
                        active: false,
                        quantizedTime: t,
                        useFallSeatedActor: false,
                        seatedPoseAlpha: 0f,
                        gettingUpPoseAlpha: 0f,
                        adjustDressAlpha: 0f,
                        actorOffsetY: 0f,
                        actorTiltDeg: 0f,
                        facingDirection: -1f,
                        smoothTurnScaleX: 1f,
                        cameraOffsetX: 0f,
                        cameraOffsetY: 0f,
                        cameraSpanScale: 1f,
                        nandaLightStrength: 1f,
                        overrideYunaPose: false,
                        yunaBlink: 0f,
                        yunaLean: 0f,
                        yunaHeadTilt: 0f,
                        yunaBreathe: 0f,
                        yunaSquashX: 1f,
                        yunaSquashY: 1f);
            }
        }

        private static Sample EvaluateFalling(float t)
        {
            // Cai sentada/desequilibrada no ar com leve inclinação suave (nunca em pé).
            float fallZoom = SmoothStep(0f, 0.9f, t);
            float airSway = (float)Math.Sin(t * 5.2f) * 4.5f;
            return new Sample(
                active: true,
                quantizedTime: t,
                useFallSeatedActor: true,
                seatedPoseAlpha: 1f,
                gettingUpPoseAlpha: 0f,
                adjustDressAlpha: 0f,
                actorOffsetY: 0f,
                actorTiltDeg: 10f + airSway,
                facingDirection: 1f,
                smoothTurnScaleX: 1f,
                cameraOffsetX: 0f,
                cameraOffsetY: Lerp(0.15f, -0.14f, fallZoom),
                cameraSpanScale: Lerp(1.05f, 0.90f, fallZoom),
                nandaLightStrength: 0.92f,
                overrideYunaPose: false,
                yunaBlink: 0f,
                yunaLean: 0f,
                yunaHeadTilt: 0f,
                yunaBreathe: 0f,
                yunaSquashX: 0.98f,
                yunaSquashY: 1.03f);
        }

        private static Sample EvaluateRecoveryAndDialogue(float t)
        {
            // 0.00..0.80s: Pouso sentado amortecido no chão + leve tontura.
            if (t < 0.80f)
            {
                float impact = SmoothStep(0f, 0.14f, t) * (1f - SmoothStep(0.14f, 0.68f, t));
                float sway = (float)Math.Sin(t * 6.0f) * 2.2f * (1f - SmoothStep(0.28f, 0.80f, t));
                return new Sample(
                    active: true,
                    quantizedTime: t,
                    useFallSeatedActor: true,
                    seatedPoseAlpha: 1f,
                    gettingUpPoseAlpha: 0f,
                    adjustDressAlpha: 0f,
                    actorOffsetY: -0.02f * impact,
                    actorTiltDeg: 4f * impact + sway,
                    facingDirection: 1f,
                    smoothTurnScaleX: 1f,
                    cameraOffsetX: 0f,
                    cameraOffsetY: -0.14f - 0.04f * impact,
                    cameraSpanScale: Lerp(0.90f, 0.84f, SmoothStep(0f, 0.80f, t)),
                    nandaLightStrength: 0.96f,
                    overrideYunaPose: false,
                    yunaBlink: 0f,
                    yunaLean: 0f,
                    yunaHeadTilt: 0f,
                    yunaBreathe: 0f,
                    yunaSquashX: 1f + 0.08f * impact,
                    yunaSquashY: 1f - 0.09f * impact);
            }

            // 0.80..1.45s: Sentada -> ajoelhada apoiando no joelho (quadro intermediário GettingUp).
            if (t < 1.45f)
            {
                float rise = SmoothStep(0.80f, 1.45f, t);
                float seatedFade = 1f - SmoothStep(0.84f, 1.32f, t);
                float getUpFade = SmoothStep(0.88f, 1.38f, t);
                float gentleDizzy = (float)Math.Sin((t - 0.80f) * 4.2f) * 2.4f * (1f - rise);

                return new Sample(
                    active: true,
                    quantizedTime: t,
                    useFallSeatedActor: true,
                    seatedPoseAlpha: seatedFade,
                    gettingUpPoseAlpha: getUpFade,
                    adjustDressAlpha: 0f,
                    actorOffsetY: Lerp(-0.02f, -0.01f, rise),
                    actorTiltDeg: gentleDizzy,
                    facingDirection: 1f,
                    smoothTurnScaleX: 1f,
                    cameraOffsetX: 0f,
                    cameraOffsetY: Lerp(-0.14f, -0.09f, rise),
                    cameraSpanScale: 0.84f,
                    nandaLightStrength: 0.97f,
                    overrideYunaPose: false,
                    yunaBlink: 0f,
                    yunaLean: 0f,
                    yunaHeadTilt: 0f,
                    yunaBreathe: 0f,
                    yunaSquashX: Lerp(1.03f, 1f, rise),
                    yunaSquashY: Lerp(0.95f, 1f, rise));
            }

            // 1.45..2.05s: Ajoelhada -> levanta por completo e segura a saia (AdjustDress).
            if (t < 2.05f)
            {
                float stand = SmoothStep(1.45f, 2.05f, t);
                float getUpFade = 1f - SmoothStep(1.50f, 1.98f, t);
                float dressFade = SmoothStep(1.55f, 2.02f, t);

                return new Sample(
                    active: true,
                    quantizedTime: t,
                    useFallSeatedActor: true,
                    seatedPoseAlpha: 0f,
                    gettingUpPoseAlpha: getUpFade,
                    adjustDressAlpha: dressFade,
                    actorOffsetY: Lerp(-0.01f, 0f, stand),
                    actorTiltDeg: Lerp(1.5f, -1.0f, stand),
                    facingDirection: 1f,
                    smoothTurnScaleX: 1f,
                    cameraOffsetX: 0f,
                    cameraOffsetY: Lerp(-0.09f, -0.05f, stand),
                    cameraSpanScale: 0.84f,
                    nandaLightStrength: 0.99f,
                    overrideYunaPose: false,
                    yunaBlink: 0f,
                    yunaLean: 0f,
                    yunaHeadTilt: 0f,
                    yunaBreathe: 0f,
                    yunaSquashX: 1f,
                    yunaSquashY: 1f);
            }

            // 2.05..2.95s: Arruma/sacode o vestido com movimentos suaves e entrega suavemente ao rig em pé.
            if (t < 2.95f)
            {
                float u = t - 2.05f;
                float pat = (float)(0.5 - 0.5 * Math.Cos(u * 5.2f));
                float handoff = SmoothStep(2.58f, 2.95f, t);
                bool useActor = t < 2.92f;

                return new Sample(
                    active: true,
                    quantizedTime: t,
                    useFallSeatedActor: useActor,
                    seatedPoseAlpha: 0f,
                    gettingUpPoseAlpha: 0f,
                    adjustDressAlpha: 1f - handoff,
                    actorOffsetY: 0f,
                    actorTiltDeg: -1.0f - 1.8f * pat * (1f - handoff),
                    facingDirection: 1f,
                    smoothTurnScaleX: 1f,
                    cameraOffsetX: 0f,
                    cameraOffsetY: -0.05f,
                    cameraSpanScale: 0.84f,
                    nandaLightStrength: 1.0f,
                    overrideYunaPose: !useActor,
                    yunaBlink: 0f,
                    yunaLean: 0.9f * (1f - handoff),
                    yunaHeadTilt: 2.6f * (1f - handoff),
                    yunaBreathe: 0.010f,
                    yunaSquashX: 1f + 0.022f * pat * (1f - handoff),
                    yunaSquashY: 1f - 0.025f * pat * (1f - handoff));
            }

            // 2.95s+: Olhar suave e contínuo de um lado para o outro ENQUANTO o diálogo acontece.
            float lookT = t - 2.95f;
            EvaluateSmoothLookCycle(lookT, 1.35f, 0.42f, out float facing, out float turnScaleX, out float turnEase);
            float blink = (lookT % 2.5f) > 2.36f ? 1f : 0f;

            return new Sample(
                active: true,
                quantizedTime: t,
                useFallSeatedActor: false,
                seatedPoseAlpha: 0f,
                gettingUpPoseAlpha: 0f,
                adjustDressAlpha: 0f,
                actorOffsetY: 0f,
                actorTiltDeg: 0f,
                facingDirection: facing,
                smoothTurnScaleX: turnScaleX,
                cameraOffsetX: facing * 0.09f * turnEase,
                cameraOffsetY: -0.04f,
                cameraSpanScale: 0.86f,
                nandaLightStrength: 1.0f + 0.05f * (float)Math.Sin(t * 2.8f),
                overrideYunaPose: true,
                yunaBlink: blink,
                yunaLean: 0.65f * turnEase,
                yunaHeadTilt: -1.6f * turnEase,
                yunaBreathe: 0.011f * (0.5f + 0.5f * (float)Math.Sin(t * 2.2f)),
                yunaSquashX: 1f,
                yunaSquashY: 1f);
        }

        private static Sample EvaluateNandaScolding(float t)
        {
            // Durante a fala da Nanda, Yuna continua olhando suavemente e a câmera abre para 1.15f (volta).
            float widen = SmoothStep(0f, 1.4f, t);
            EvaluateSmoothLookCycle(t, 1.50f, 0.42f, out float facing, out float turnScaleX, out float turnEase);
            float idleSway = (float)Math.Sin(t * 2.0f);
            float blink = (t % 2.4f) > 2.26f ? 1f : 0f;

            return new Sample(
                active: true,
                quantizedTime: t,
                useFallSeatedActor: false,
                seatedPoseAlpha: 0f,
                gettingUpPoseAlpha: 0f,
                adjustDressAlpha: 0f,
                actorOffsetY: 0f,
                actorTiltDeg: 0f,
                facingDirection: facing,
                smoothTurnScaleX: turnScaleX,
                cameraOffsetX: 0f,
                cameraOffsetY: Lerp(-0.04f, 0f, widen),
                cameraSpanScale: Lerp(0.86f, 1.15f, widen),
                nandaLightStrength: 1.05f + 0.06f * (float)Math.Sin(t * 4.2f),
                overrideYunaPose: true,
                yunaBlink: blink,
                yunaLean: 0.35f + 0.25f * idleSway,
                yunaHeadTilt: 1.4f * idleSway * turnEase,
                yunaBreathe: 0.010f * (0.5f + 0.5f * idleSway),
                yunaSquashX: 1f,
                yunaSquashY: 1f);
        }

        private static void EvaluateSmoothLookCycle(
            float elapsed,
            float holdDuration,
            float turnDuration,
            out float facing,
            out float scaleX,
            out float settleEase)
        {
            float segment = holdDuration + turnDuration;
            int index = (int)Math.Floor(elapsed / segment);
            float local = elapsed % segment;
            float fromDir = (index % 2 == 0) ? -1f : 1f;
            float toDir = -fromDir;

            if (local < holdDuration)
            {
                facing = fromDir;
                scaleX = 1f;
                settleEase = (float)Math.Sin(Clamp01(local / holdDuration) * Math.PI);
            }
            else
            {
                float u = SmoothStep(holdDuration, segment, local);
                float signed = Lerp(fromDir, toDir, u);
                facing = signed >= 0f ? 1f : -1f;
                // Suaviza o estreitamento horizontal durante o giro sem zerar completamente.
                float absScale = Math.Abs(signed);
                scaleX = 0.28f + 0.72f * absScale;
                settleEase = 0f;
            }
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
