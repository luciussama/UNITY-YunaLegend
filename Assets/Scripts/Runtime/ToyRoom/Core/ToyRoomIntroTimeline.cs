using System;
using System.Collections.Generic;

namespace YunaLegend.ToyRoom.Core
{
    /// <summary>
    /// Exposure timing for the director's 625-drawing remake. The existing intro
    /// remains available until the complete hand-drawn sequence is validated.
    /// </summary>
    public static class ToyRoomHandDrawnTiming
    {
        public const int FramesPerSecond = 25;
        public const int FrameCount = 625;
        public const double DurationSeconds = FrameCount / (double)FramesPerSecond;

        private static readonly int[] ShotStarts = { 0, 75, 175, 275, 400, 525 };

        public static int FrameAt(double elapsedSeconds)
        {
            if (double.IsNaN(elapsedSeconds) || elapsedSeconds <= 0d)
            {
                return 0;
            }

            if (elapsedSeconds >= DurationSeconds)
            {
                return FrameCount;
            }

            // Counter floating-point error at an exact exposure boundary.
            return (int)Math.Floor((elapsedSeconds * FramesPerSecond) + 1e-9d);
        }

        public static int ShotAt(int frame)
        {
            if (frame >= FrameCount)
            {
                return -1;
            }

            for (int shot = ShotStarts.Length - 1; shot >= 0; shot--)
            {
                if (frame >= ShotStarts[shot])
                {
                    return shot;
                }
            }

            return 0;
        }

        public static void CaptionAt(int frame, out string speaker, out string text)
        {
            speaker = string.Empty;
            text = string.Empty;

            if (frame >= 75 && frame <= 124)
            {
                speaker = "Yuna";
                text = "Aqui também...";
                return;
            }

            if (frame >= 125 && frame <= 199)
            {
                speaker = "Yuna";
                text = "Eu queria espaço pra brincar.";
                return;
            }

            if (frame >= 300 && frame <= 399)
            {
                speaker = "Nanda";
                text = "Vamos. Um de cada vez!";
            }
        }
    }

    public enum ToyRoomIntroStageId
    {
        Tr002,
        Tr003,
        Tr004,
        Tr005,
        Tr006,
        Tr007,
        Tr008,
        Tr009
    }

    public readonly struct ToyRoomIntroStageSpec
    {
        public ToyRoomIntroStageId Id { get; }
        public float DurationSeconds { get; }
        public float ShotX { get; }
        public float ShotY { get; }
        public float ZoomFrom { get; }
        public float ZoomTo { get; }
        public int GirlPoseIndex { get; }

        public ToyRoomIntroStageSpec(
            ToyRoomIntroStageId id,
            float durationSeconds,
            float shotX,
            float shotY,
            float zoomFrom,
            float zoomTo,
            int girlPoseIndex)
        {
            Id = id;
            DurationSeconds = durationSeconds;
            ShotX = shotX;
            ShotY = shotY;
            ZoomFrom = zoomFrom;
            ZoomTo = zoomTo;
            GirlPoseIndex = girlPoseIndex;
        }
    }

    public readonly struct ToyRoomIntroGirlFrameSpec
    {
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public float AnchorX { get; }

        public ToyRoomIntroGirlFrameSpec(float x, float y, float width, float height, float anchorX)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            AnchorX = anchorX;
        }
    }

    public readonly struct ToyRoomIntroViewportSpec
    {
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }

        public ToyRoomIntroViewportSpec(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public readonly struct ToyRoomIntroSample
    {
        public bool IsActive { get; }
        public ToyRoomIntroStageId StageId { get; }
        public float TotalTimeSeconds { get; }
        public float StageTimeSeconds { get; }
        public float StageDurationSeconds { get; }
        public ToyRoomIntroViewportSpec Viewport { get; }
        public float CameraX { get; }
        public float CameraY { get; }
        public float CameraScale { get; }
        public int GirlPoseIndex { get; }
        public float GirlX { get; }
        public float GirlBaseY { get; }
        public float GirlBreathScaleY { get; }
        public float FairyX { get; }
        public float FairyY { get; }
        public float FairyRotationRadians { get; }
        public float FairyWingBeatScaleX { get; }
        public float WarmCurtainAlpha { get; }
        public bool StartMusic { get; }
        public float MusicFade { get; }
        public string DialogueText { get; }
        public string DialogueHeading { get; }

        public ToyRoomIntroSample(
            bool isActive,
            ToyRoomIntroStageId stageId,
            float totalTimeSeconds,
            float stageTimeSeconds,
            float stageDurationSeconds,
            ToyRoomIntroViewportSpec viewport,
            float cameraX,
            float cameraY,
            float cameraScale,
            int girlPoseIndex,
            float girlX,
            float girlBaseY,
            float girlBreathScaleY,
            float fairyX,
            float fairyY,
            float fairyRotationRadians,
            float fairyWingBeatScaleX,
            float warmCurtainAlpha,
            bool startMusic,
            float musicFade,
            string dialogueText,
            string dialogueHeading)
        {
            IsActive = isActive;
            StageId = stageId;
            TotalTimeSeconds = totalTimeSeconds;
            StageTimeSeconds = stageTimeSeconds;
            StageDurationSeconds = stageDurationSeconds;
            Viewport = viewport;
            CameraX = cameraX;
            CameraY = cameraY;
            CameraScale = cameraScale;
            GirlPoseIndex = girlPoseIndex;
            GirlX = girlX;
            GirlBaseY = girlBaseY;
            GirlBreathScaleY = girlBreathScaleY;
            FairyX = fairyX;
            FairyY = fairyY;
            FairyRotationRadians = fairyRotationRadians;
            FairyWingBeatScaleX = fairyWingBeatScaleX;
            WarmCurtainAlpha = warmCurtainAlpha;
            StartMusic = startMusic;
            MusicFade = musicFade;
            DialogueText = dialogueText ?? string.Empty;
            DialogueHeading = dialogueHeading ?? string.Empty;
        }
    }

    /// <summary>
    /// Avaliador matematico puro (sem dependencia de UnityEngine) da cinematica de introducao
    /// em sprite stop motion da Sala de Brinquedos (TR_002 a TR_009).
    /// </summary>
    public static class ToyRoomIntroTimeline
    {
        public const float RoomPixelWidth = 1672f;
        public const float RoomPixelHeight = 941f;
        public const float GirlTargetHeight = 485f;
        public const float GirlReferenceHeight = 561f;
        public const float GirlRenderScale = GirlTargetHeight / GirlReferenceHeight;
        public const float GirlBaseY = 805f;
        public const float FairyTargetHeight = 120f;
        public const float FairyLeftWingSplitRatio = 0.24f;
        public const float FairyRightWingSplitRatio = 0.77f;

        public const string PoeticPlayLine = "A sala estava cheia.\nNenhum lugar esperava por ela.";
        public const string PoeticPlaceLine = "A caixa tinha um fundo.\nO resto está vazio.";

        public static readonly IReadOnlyList<ToyRoomIntroGirlFrameSpec> GirlFrames = new List<ToyRoomIntroGirlFrameSpec>
        {
            new ToyRoomIntroGirlFrameSpec(116f, 113f, 329f, 552f, 169.5f),
            new ToyRoomIntroGirlFrameSpec(666f, 99f, 310f, 561f, 187.0f),
            new ToyRoomIntroGirlFrameSpec(1215f, 140f, 274f, 522f, 140.5f),
            new ToyRoomIntroGirlFrameSpec(1723f, 112f, 372f, 555f, 165.0f)
        };

        public static readonly IReadOnlyList<ToyRoomIntroStageSpec> Stages = new List<ToyRoomIntroStageSpec>
        {
            new ToyRoomIntroStageSpec(ToyRoomIntroStageId.Tr002, 0.8f, 800f, 490f, 1.00f, 1.08f, 0),
            new ToyRoomIntroStageSpec(ToyRoomIntroStageId.Tr003, 2.5f, 800f, 490f, 1.00f, 1.08f, 0),
            new ToyRoomIntroStageSpec(ToyRoomIntroStageId.Tr004, 4.0f, 805f, 510f, 1.42f, 1.48f, 1),
            new ToyRoomIntroStageSpec(ToyRoomIntroStageId.Tr005, 4.0f, 950f, 485f, 1.14f, 1.22f, 1),
            new ToyRoomIntroStageSpec(ToyRoomIntroStageId.Tr006, 7.0f, 795f, 510f, 1.48f, 1.57f, 2),
            new ToyRoomIntroStageSpec(ToyRoomIntroStageId.Tr007, 2.0f, 820f, 510f, 1.38f, 1.42f, 3),
            new ToyRoomIntroStageSpec(ToyRoomIntroStageId.Tr008, 3.0f, 820f, 490f, 1.15f, 1.00f, 3)
        };

        public static float TotalDurationSeconds { get; } = ComputeTotalDuration();

        public static ToyRoomIntroSample Evaluate(
            float elapsedSeconds,
            float canvasWidth,
            float canvasHeight,
            float cssScale = 1f)
        {
            float safeTime = Math.Max(0f, elapsedSeconds);
            float safeW = Math.Max(1f, canvasWidth);
            float safeH = Math.Max(1f, canvasHeight);
            float safeCss = Math.Max(0.1f, cssScale);

            ResolveStage(safeTime, out ToyRoomIntroStageSpec stage, out float stageTime);

            if (stage.Id == ToyRoomIntroStageId.Tr009)
            {
                var fullViewport = new ToyRoomIntroViewportSpec(0f, 0f, safeW, safeH);
                return new ToyRoomIntroSample(
                    isActive: false,
                    stageId: ToyRoomIntroStageId.Tr009,
                    totalTimeSeconds: safeTime,
                    stageTimeSeconds: 0f,
                    stageDurationSeconds: 0f,
                    viewport: fullViewport,
                    cameraX: 820f,
                    cameraY: 490f,
                    cameraScale: 1f,
                    girlPoseIndex: 3,
                    girlX: 710f,
                    girlBaseY: GirlBaseY,
                    girlBreathScaleY: 1f,
                    fairyX: 990f,
                    fairyY: 385f,
                    fairyRotationRadians: 0f,
                    fairyWingBeatScaleX: 1f,
                    warmCurtainAlpha: 0f,
                    startMusic: true,
                    musicFade: 1f,
                    dialogueText: string.Empty,
                    dialogueHeading: string.Empty);
            }

            ComputeCameraFrame(
                stage,
                stageTime,
                safeW,
                safeH,
                safeCss,
                out ToyRoomIntroViewportSpec viewport,
                out float camX,
                out float camY,
                out float camScale);

            float breath = 1f + (float)Math.Sin(safeTime * 2.1f) * 0.003f;
            float entry = stage.Id == ToyRoomIntroStageId.Tr003 ? 1f - SmoothStep01(stageTime / 1.8f) : 0f;
            float girlX = 710f - (entry * 110f);

            float stageProgress = stage.DurationSeconds > 0.0001f
                ? SmoothStep01(stageTime / stage.DurationSeconds)
                : 0f;
            float fairyArc = stage.Id == ToyRoomIntroStageId.Tr005
                ? (float)Math.Sin(stageProgress * Math.PI)
                : 0f;
            float fairyX = 990f + (fairyArc * 170f);
            float fairyY = 385f + ((float)Math.Sin(safeTime * 3.0f) * 9f) - (fairyArc * 80f);
            float fairyRot = (float)Math.Sin(safeTime * 3.0f) * 0.045f;
            float wingFreq = stage.Id == ToyRoomIntroStageId.Tr007 ? 32f : 18f;
            float wingBeat = 0.72f + (0.28f * Math.Abs((float)Math.Sin(safeTime * wingFreq)));

            float curtainAlpha = ComputeWarmCurtainAlpha(stage.Id, stageTime, stage.DurationSeconds);
            bool startMusic = stage.Id == ToyRoomIntroStageId.Tr008;
            float musicFade = stage.Id == ToyRoomIntroStageId.Tr008
                ? Clamp01(stageTime / 3.0f)
                : 0f;

            ResolveDialogue(stage.Id, stageTime, out string dialogueText, out string dialogueHeading);

            return new ToyRoomIntroSample(
                isActive: true,
                stageId: stage.Id,
                totalTimeSeconds: safeTime,
                stageTimeSeconds: stageTime,
                stageDurationSeconds: stage.DurationSeconds,
                viewport: viewport,
                cameraX: camX,
                cameraY: camY,
                cameraScale: camScale,
                girlPoseIndex: stage.GirlPoseIndex,
                girlX: girlX,
                girlBaseY: GirlBaseY,
                girlBreathScaleY: breath,
                fairyX: fairyX,
                fairyY: fairyY,
                fairyRotationRadians: fairyRot,
                fairyWingBeatScaleX: wingBeat,
                warmCurtainAlpha: curtainAlpha,
                startMusic: startMusic,
                musicFade: musicFade,
                dialogueText: dialogueText,
                dialogueHeading: dialogueHeading);
        }

        public static bool TryAdvanceDialogue(float currentElapsedSeconds, out float nextElapsedSeconds)
        {
            float safeTime = Math.Max(0f, currentElapsedSeconds);
            ResolveStage(safeTime, out ToyRoomIntroStageSpec stage, out float stageTime);
            ResolveDialogue(stage.Id, stageTime, out string text, out _);

            if (string.IsNullOrEmpty(text))
            {
                nextElapsedSeconds = safeTime;
                return false;
            }

            if (stage.Id == ToyRoomIntroStageId.Tr006 && stageTime < 3.1f)
            {
                nextElapsedSeconds = safeTime + (3.9f - stageTime);
                return true;
            }

            nextElapsedSeconds = safeTime + (stage.DurationSeconds - stageTime);
            return true;
        }

        public static float SmoothStep01(float value)
        {
            float t = Clamp01(value);
            return t * t * (3f - (2f * t));
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }

        private static float ComputeTotalDuration()
        {
            float sum = 0f;
            for (int i = 0; i < Stages.Count; i++)
            {
                sum += Stages[i].DurationSeconds;
            }

            return sum;
        }

        private static void ResolveStage(
            float elapsedSeconds,
            out ToyRoomIntroStageSpec resolvedStage,
            out float localTimeSeconds)
        {
            float start = 0f;
            for (int i = 0; i < Stages.Count; i++)
            {
                ToyRoomIntroStageSpec candidate = Stages[i];
                if (elapsedSeconds < start + candidate.DurationSeconds)
                {
                    resolvedStage = candidate;
                    localTimeSeconds = elapsedSeconds - start;
                    return;
                }

                start += candidate.DurationSeconds;
            }

            resolvedStage = new ToyRoomIntroStageSpec(ToyRoomIntroStageId.Tr009, 0f, 820f, 490f, 1f, 1f, 3);
            localTimeSeconds = 0f;
        }

        private static void ComputeCameraFrame(
            ToyRoomIntroStageSpec stage,
            float stageTime,
            float canvasWidth,
            float canvasHeight,
            float cssScale,
            out ToyRoomIntroViewportSpec viewport,
            out float camX,
            out float camY,
            out float camScale)
        {
            bool portrait = canvasHeight > canvasWidth;
            float header = Math.Min(136f * cssScale, canvasHeight * 0.34f);
            float height = Math.Min(
                canvasHeight - header - (16f * cssScale),
                canvasWidth * (portrait ? 0.82f : (9f / 16f)));
            float vpY = header + ((canvasHeight - header - height) * (portrait ? 0.25f : 0.5f));
            viewport = new ToyRoomIntroViewportSpec(0f, vpY, canvasWidth, height);

            float p = stage.DurationSeconds > 0.0001f
                ? SmoothStep01(stageTime / stage.DurationSeconds)
                : 0f;
            float rawZoom = stage.ZoomFrom + ((stage.ZoomTo - stage.ZoomFrom) * p);
            float zoom = portrait ? Math.Min(1.12f, rawZoom) : rawZoom;

            float desiredScale = Math.Max(
                canvasWidth / (portrait ? 1050f : RoomPixelWidth),
                height / RoomPixelHeight) * zoom;

            camScale = portrait
                ? desiredScale
                : Math.Min(desiredScale, Math.Max(canvasWidth / RoomPixelWidth, height / 470f));

            float halfW = (viewport.Width / camScale) * 0.5f;
            float halfH = (height / camScale) * 0.5f;
            float targetX = portrait ? 840f : stage.ShotX;

            camX = Math.Max(halfW, Math.Min(RoomPixelWidth - halfW, targetX));
            camY = Math.Max(halfH, Math.Min(RoomPixelHeight - halfH, stage.ShotY));
        }

        private static float ComputeWarmCurtainAlpha(
            ToyRoomIntroStageId id,
            float stageTime,
            float stageDuration)
        {
            if (id == ToyRoomIntroStageId.Tr002)
            {
                return SmoothStep01(stageTime / Math.Max(0.0001f, stageDuration));
            }

            if (id == ToyRoomIntroStageId.Tr003)
            {
                return 1f - SmoothStep01(stageTime / 1.1f);
            }

            if (id == ToyRoomIntroStageId.Tr008)
            {
                return SmoothStep01((stageTime - stageDuration + 0.65f) / 0.65f);
            }

            return 0f;
        }

        private static void ResolveDialogue(
            ToyRoomIntroStageId id,
            float stageTime,
            out string text,
            out string heading)
        {
            if (id == ToyRoomIntroStageId.Tr004)
            {
                text = "Aqui também...";
                heading = "Menina";
                return;
            }

            if (id == ToyRoomIntroStageId.Tr005)
            {
                text = "Eu queria espaço pra brincar.";
                heading = "Menina";
                return;
            }

            if (id == ToyRoomIntroStageId.Tr006)
            {
                heading = string.Empty;
                if (stageTime < 3.1f)
                {
                    text = PoeticPlayLine;
                    return;
                }

                if (stageTime < 3.9f)
                {
                    text = string.Empty;
                    return;
                }

                text = PoeticPlaceLine;
                return;
            }

            if (id == ToyRoomIntroStageId.Tr007)
            {
                text = "Vamos. Um de cada vez!";
                heading = "Nanda";
                return;
            }

            text = string.Empty;
            heading = string.Empty;
        }
    }
}
