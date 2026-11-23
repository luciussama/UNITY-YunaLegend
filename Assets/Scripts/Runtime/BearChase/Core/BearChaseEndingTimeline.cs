using System;

namespace YunaLegend.BearChase.Core
{
    public enum BearChaseEndingBeat
    {
        NandaRelief = 0,
        GirlSmiles = 1,
        QuestionGrows = 2,
        GirlAsks = 3,
        NandaFacepalmZoom = 4,
        FadeOut = 5,
        Completed = 6
    }

    public enum BearChaseEndingYunaPose
    {
        CatchingBreath = 0,
        Smiling = 1,
        Confused = 2
    }

    public enum BearChaseEndingNandaPose
    {
        Happy = 0,
        Facepalm = 1
    }

    public readonly struct BearChaseEndingSample
    {
        public readonly BearChaseEndingBeat Beat;
        public readonly bool ShowDialogue;
        public readonly string SpeakerHeader;
        public readonly string DialogueText;
        public readonly BearChaseEndingYunaPose YunaPose;
        public readonly BearChaseEndingNandaPose NandaPose;
        public readonly float CameraZoom;
        public readonly float NandaBobY;
        public readonly float FadeAlpha;
        public readonly bool IsCompleted;

        public BearChaseEndingSample(
            BearChaseEndingBeat beat,
            bool showDialogue,
            string speakerHeader,
            string dialogueText,
            BearChaseEndingYunaPose yunaPose,
            BearChaseEndingNandaPose nandaPose,
            float cameraZoom,
            float nandaBobY,
            float fadeAlpha,
            bool isCompleted)
        {
            Beat = beat;
            ShowDialogue = showDialogue;
            SpeakerHeader = speakerHeader ?? string.Empty;
            DialogueText = dialogueText ?? string.Empty;
            YunaPose = yunaPose;
            NandaPose = nandaPose;
            CameraZoom = cameraZoom;
            NandaBobY = nandaBobY;
            FadeAlpha = fadeAlpha;
            IsCompleted = isCompleted;
        }
    }

    /// <summary>
    /// Linha do tempo pura (sem dependencias de engine) da cena 2D ilustrada de finalizacao
    /// do Capitulo 1 ("Um Quarto Inteiro"), fiel as 5 batidas do legado com Nanda azul-ciano.
    /// </summary>
    public static class BearChaseEndingTimeline
    {
        public const float Beat0End = 4.60f;
        public const float Beat1End = 9.00f;
        public const float Beat2End = 14.20f;
        public const float Beat3End = 19.60f;
        public const float Beat4End = 24.00f;
        public const float TotalDuration = 25.00f;

        public const float MaxZoomScale = 1.88f;

        public static BearChaseEndingSample Evaluate(float elapsedSeconds)
        {
            float t = Math.Max(0f, Math.Min(TotalDuration, elapsedSeconds));
            float bob = (float)Math.Sin(t * 3.4) * 0.010f;
            float fadeIn = t < 0.45f ? (1f - SmoothStep(t / 0.45f)) : 0f;

            if (t < Beat0End)
            {
                return new BearChaseEndingSample(
                    BearChaseEndingBeat.NandaRelief,
                    showDialogue: true,
                    speakerHeader: "NANDA",
                    dialogueText: "Ufa, conseguimos. Guardamos os brinquedos a tempo. Agora podemos brincar.",
                    yunaPose: BearChaseEndingYunaPose.CatchingBreath,
                    nandaPose: BearChaseEndingNandaPose.Happy,
                    cameraZoom: 1f,
                    nandaBobY: bob,
                    fadeAlpha: fadeIn,
                    isCompleted: false);
            }

            if (t < Beat1End)
            {
                return new BearChaseEndingSample(
                    BearChaseEndingBeat.GirlSmiles,
                    showDialogue: true,
                    speakerHeader: "UM QUARTO INTEIRO",
                    dialogueText: "A menina sorriu.\nDe canto a canto, passeou os olhos pelo quarto.",
                    yunaPose: BearChaseEndingYunaPose.Smiling,
                    nandaPose: BearChaseEndingNandaPose.Happy,
                    cameraZoom: 1f,
                    nandaBobY: bob,
                    fadeAlpha: 0f,
                    isCompleted: false);
            }

            if (t < Beat2End)
            {
                float localU = (t - Beat1End) / (Beat2End - Beat1End);
                BearChaseEndingYunaPose pose = localU < 0.45f
                    ? BearChaseEndingYunaPose.Smiling
                    : BearChaseEndingYunaPose.Confused;

                return new BearChaseEndingSample(
                    BearChaseEndingBeat.QuestionGrows,
                    showDialogue: true,
                    speakerHeader: "UM QUARTO INTEIRO",
                    dialogueText: "Onde antes havia brinquedos, agora havia espaço.\nO sorriso ficou pequeno. Uma pergunta cresceu no lugar.",
                    yunaPose: pose,
                    nandaPose: BearChaseEndingNandaPose.Happy,
                    cameraZoom: 1f,
                    nandaBobY: bob,
                    fadeAlpha: 0f,
                    isCompleted: false);
            }

            if (t < Beat3End)
            {
                return new BearChaseEndingSample(
                    BearChaseEndingBeat.GirlAsks,
                    showDialogue: true,
                    speakerHeader: "MENINA",
                    dialogueText: "Mas... agora todos os brinquedos estão guardados.\nComo vamos brincar? Vamos pegar os brinquedos de novo?",
                    yunaPose: BearChaseEndingYunaPose.Confused,
                    nandaPose: BearChaseEndingNandaPose.Happy,
                    cameraZoom: 1f,
                    nandaBobY: bob,
                    fadeAlpha: 0f,
                    isCompleted: false);
            }

            if (t < Beat4End)
            {
                float zoomU = (t - Beat3End) / (Beat4End - Beat3End);
                float smoothZoom = SmoothStep(zoomU);
                float zoom = 1f + (MaxZoomScale - 1f) * smoothZoom;

                return new BearChaseEndingSample(
                    BearChaseEndingBeat.NandaFacepalmZoom,
                    showDialogue: false,
                    speakerHeader: string.Empty,
                    dialogueText: string.Empty,
                    yunaPose: BearChaseEndingYunaPose.Confused,
                    nandaPose: BearChaseEndingNandaPose.Facepalm,
                    cameraZoom: zoom,
                    nandaBobY: bob * 0.65f,
                    fadeAlpha: 0f,
                    isCompleted: false);
            }

            if (t < TotalDuration)
            {
                float fadeOutU = (t - Beat4End) / (TotalDuration - Beat4End);
                return new BearChaseEndingSample(
                    BearChaseEndingBeat.FadeOut,
                    showDialogue: false,
                    speakerHeader: string.Empty,
                    dialogueText: string.Empty,
                    yunaPose: BearChaseEndingYunaPose.Confused,
                    nandaPose: BearChaseEndingNandaPose.Facepalm,
                    cameraZoom: MaxZoomScale,
                    nandaBobY: 0f,
                    fadeAlpha: SmoothStep(fadeOutU),
                    isCompleted: false);
            }

            return new BearChaseEndingSample(
                BearChaseEndingBeat.Completed,
                showDialogue: false,
                speakerHeader: string.Empty,
                dialogueText: string.Empty,
                yunaPose: BearChaseEndingYunaPose.Confused,
                nandaPose: BearChaseEndingNandaPose.Facepalm,
                cameraZoom: MaxZoomScale,
                nandaBobY: 0f,
                fadeAlpha: 1f,
                isCompleted: true);
        }

        public static float GetNextAdvanceTime(float elapsedSeconds)
        {
            if (elapsedSeconds < Beat0End)
            {
                return Beat0End;
            }

            if (elapsedSeconds < Beat1End)
            {
                return Beat1End;
            }

            if (elapsedSeconds < Beat2End)
            {
                return Beat2End;
            }

            if (elapsedSeconds < Beat3End)
            {
                return Beat3End;
            }

            if (elapsedSeconds < Beat4End)
            {
                return Beat4End;
            }

            return TotalDuration;
        }

        private static float SmoothStep(float x)
        {
            float c = Math.Max(0f, Math.Min(1f, x));
            return c * c * (3f - 2f * c);
        }
    }
}
