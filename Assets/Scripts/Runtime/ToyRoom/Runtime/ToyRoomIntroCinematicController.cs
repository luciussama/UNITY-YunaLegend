using System;
using UnityEngine;
using UnityEngine.UI;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Adaptador MonoBehaviour (Humble Object) que apresenta a cinematica oficial de abertura
    /// da Sala de Brinquedos em sprite stop motion, consumindo ToyRoomIntroTimeline (Core).
    /// </summary>
    public sealed class ToyRoomIntroCinematicController : MonoBehaviour
    {
        private const string RoomResourcePath = "ToyRoomIntro/room-v1";
        private const string GirlResourcePath = "ToyRoomIntro/girl-acting-v1";
        private const string FairyResourcePath = "ToyRoomIntro/illustrated-fairy-v1";

        [SerializeField] private Font poeticFont;

        private ToyRoomAudioController audioController;
        private Action onSequenceComplete;

        private Canvas introCanvas;
        private RectTransform canvasRect;
        private RectTransform viewportMaskRect;
        private RectTransform stageWorldRect;
        private Image roomBackdropImage;
        private RectTransform girlShadowRect;
        private Image girlShadowImage;
        private RectTransform girlRect;
        private Image girlImage;
        private RectTransform fairyRootRect;
        private RectTransform fairyBodyGlowRect;
        private Image fairyBodyGlowImage;
        private RectTransform fairyBodyRect;
        private Image fairyBodyImage;
        private RectTransform fairyLeftWingRect;
        private Image fairyLeftWingImage;
        private RectTransform fairyRightWingRect;
        private Image fairyRightWingImage;
        private Image warmCurtainImage;

        private GameObject narrationCardRoot;
        private RectTransform narrationCardRect;
        private RectTransform narrationInnerRect;
        private RectTransform flowerLeftRect;
        private RectTransform flowerRightRect;
        private Text narrationHeadingText;
        private Text narrationBodyText;
        private Text narrationAutoHintText;
        private Text narrationClickHintText;

        private Sprite roomSprite;
        private readonly Sprite[] girlPoseSprites = new Sprite[4];
        private Sprite fairyBodySprite;
        private Sprite fairyLeftWingSprite;
        private Sprite fairyRightWingSprite;
        private Sprite ellipseShadowSprite;
        private Sprite roundedOuterCardSprite;
        private Sprite roundedInnerBorderSprite;

        private float forcedLogicalWidth = -1f;
        private float forcedLogicalHeight = -1f;
        private float elapsedSeconds;
        private bool musicStarted;
        private bool isActive;
        private bool built;

        public bool IsActive => isActive;
        public float ElapsedSeconds => elapsedSeconds;
        public ToyRoomIntroSample CurrentSample { get; private set; }

        public event Action OnCompleted;

        public void Configure(
            ToyRoomAudioController audioCtrl,
            Font caveatFontAsset = null)
        {
            audioController = audioCtrl;
            if (caveatFontAsset != null)
            {
                poeticFont = caveatFontAsset;
            }

            EnsureBuilt();
        }

        public void SetViewportOverride(float logicalWidth, float logicalHeight)
        {
            forcedLogicalWidth = logicalWidth;
            forcedLogicalHeight = logicalHeight;
            if (isActive)
            {
                EvaluateAtTime(elapsedSeconds);
            }
        }

        public void ClearViewportOverride()
        {
            forcedLogicalWidth = -1f;
            forcedLogicalHeight = -1f;
            if (isActive)
            {
                EvaluateAtTime(elapsedSeconds);
            }
        }

        public void BeginIntroCinematic(Action onComplete = null)
        {
            EnsureBuilt();
            onSequenceComplete = onComplete;
            elapsedSeconds = 0f;
            musicStarted = false;
            isActive = true;

            if (introCanvas != null)
            {
                introCanvas.gameObject.SetActive(true);
            }

            if (audioController != null)
            {
                audioController.PlayIntroPortalExitEffects();
            }

            EvaluateAtTime(0f);
        }

        public void EndIntroCinematic()
        {
            if (!isActive && (introCanvas == null || !introCanvas.gameObject.activeSelf))
            {
                return;
            }

            isActive = false;

            if (audioController != null)
            {
                audioController.SetExplorationMusicFade(1f);
            }

            if (introCanvas != null)
            {
                introCanvas.gameObject.SetActive(false);
            }

            Action callback = onSequenceComplete;
            onSequenceComplete = null;
            callback?.Invoke();
            OnCompleted?.Invoke();
        }

        public bool TryAdvanceDialogue()
        {
            if (!isActive)
            {
                return false;
            }

            if (!ToyRoomIntroTimeline.TryAdvanceDialogue(elapsedSeconds, out float nextTime))
            {
                return false;
            }

            EvaluateAtTime(nextTime);
            return true;
        }

        public void SkipCinematic()
        {
            if (!isActive)
            {
                return;
            }

            EndIntroCinematic();
        }

        public void EvaluateAtTime(float timeSeconds)
        {
            EnsureBuilt();
            elapsedSeconds = Mathf.Max(0f, timeSeconds);

            Vector2 canvasSize = ResolveCanvasDimensions(out float cssScale);
            if (canvasRect != null)
            {
                canvasRect.sizeDelta = canvasSize;
            }

            ToyRoomIntroSample sample = ToyRoomIntroTimeline.Evaluate(
                elapsedSeconds,
                canvasSize.x,
                canvasSize.y,
                cssScale);

            CurrentSample = sample;

            if (!sample.IsActive || sample.StageId == ToyRoomIntroStageId.Tr009)
            {
                EndIntroCinematic();
                return;
            }

            if (sample.StartMusic && !musicStarted)
            {
                musicStarted = true;
            }

            if (musicStarted && audioController != null)
            {
                audioController.SetExplorationMusicFade(sample.MusicFade);
            }

            ApplySampleToView(sample, canvasSize, cssScale);
        }

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            EvaluateAtTime(elapsedSeconds + Time.deltaTime);
        }

        private Vector2 ResolveCanvasDimensions(out float cssScale)
        {
            float w = forcedLogicalWidth > 0f
                ? forcedLogicalWidth
                : (canvasRect != null && canvasRect.rect.width > 1f ? canvasRect.rect.width : Screen.width);
            float h = forcedLogicalHeight > 0f
                ? forcedLogicalHeight
                : (canvasRect != null && canvasRect.rect.height > 1f ? canvasRect.rect.height : Screen.height);

            w = Mathf.Max(320f, w);
            h = Mathf.Max(240f, h);
            cssScale = 1f;
            return new Vector2(w, h);
        }

        private void ApplySampleToView(ToyRoomIntroSample sample, Vector2 canvasSize, float cssScale)
        {
            bool showStageWorld = sample.StageId != ToyRoomIntroStageId.Tr002;
            if (viewportMaskRect != null)
            {
                viewportMaskRect.gameObject.SetActive(showStageWorld);
                if (showStageWorld)
                {
                    ToyRoomIntroViewportSpec vp = sample.Viewport;
                    viewportMaskRect.anchoredPosition = new Vector2(vp.X, -vp.Y);
                    viewportMaskRect.sizeDelta = new Vector2(vp.Width, vp.Height);

                    float scale = sample.CameraScale;
                    float localX = (vp.Width * 0.5f) - (sample.CameraX * scale);
                    float localY = -((vp.Height * 0.5f) - (sample.CameraY * scale));
                    stageWorldRect.anchoredPosition = new Vector2(localX, localY);
                    stageWorldRect.localScale = new Vector3(scale, scale, 1f);
                }
            }

            ApplyGirlPose(sample);
            ApplyFairyPose(sample);
            ApplyWarmCurtain(sample);
            ApplyPoeticNarrationCard(sample, canvasSize, cssScale);
        }

        private void ApplyGirlPose(ToyRoomIntroSample sample)
        {
            if (girlRect == null || girlImage == null || girlShadowRect == null)
            {
                return;
            }

            int poseIdx = Mathf.Clamp(sample.GirlPoseIndex, 0, ToyRoomIntroTimeline.GirlFrames.Count - 1);
            ToyRoomIntroGirlFrameSpec frame = ToyRoomIntroTimeline.GirlFrames[poseIdx];

            if (girlPoseSprites[poseIdx] != null)
            {
                girlImage.sprite = girlPoseSprites[poseIdx];
            }

            float renderScale = ToyRoomIntroTimeline.GirlRenderScale;
            float drawW = frame.Width * renderScale;
            float drawH = frame.Height * renderScale * sample.GirlBreathScaleY;
            float drawLeft = sample.GirlX - (frame.AnchorX * renderScale);
            float drawTop = sample.GirlBaseY - drawH;

            girlRect.anchoredPosition = new Vector2(drawLeft, -drawTop);
            girlRect.sizeDelta = new Vector2(drawW, drawH);

            // Sombra eliptica sob os pes: ctx.ellipse(x, base - 4, 78, 15)
            girlShadowRect.anchoredPosition = new Vector2(sample.GirlX - 78f, -(sample.GirlBaseY - 4f - 15f));
            girlShadowRect.sizeDelta = new Vector2(156f, 30f);
        }

        private void ApplyFairyPose(ToyRoomIntroSample sample)
        {
            if (fairyRootRect == null)
            {
                return;
            }

            const float texW = 110f;
            const float texH = 120f;
            float scale = ToyRoomIntroTimeline.FairyTargetHeight / texH;
            float leftSplit = texW * ToyRoomIntroTimeline.FairyLeftWingSplitRatio;
            float rightSplit = texW * ToyRoomIntroTimeline.FairyRightWingSplitRatio;
            float bodyW = (rightSplit - leftSplit) * scale;
            float leftWingBaseW = leftSplit * scale;
            float rightWingBaseW = (texW - rightSplit) * scale;
            float beat = sample.FairyWingBeatScaleX;

            fairyRootRect.anchoredPosition = new Vector2(sample.FairyX, -sample.FairyY);
            fairyRootRect.localRotation = Quaternion.Euler(0f, 0f, -sample.FairyRotationRadians * Mathf.Rad2Deg);

            float xOrigin = (-texW * scale) * 0.5f;
            const float yOrigin = -60f;
            float bodyLeft = xOrigin + (leftSplit * scale);
            float rightWingLeft = xOrigin + (rightSplit * scale);
            float leftWingDrawW = leftWingBaseW * beat;
            float leftWingLeft = bodyLeft - leftWingDrawW;
            float rightWingDrawW = rightWingBaseW * beat;

            fairyBodyRect.anchoredPosition = new Vector2(bodyLeft, -yOrigin);
            fairyBodyRect.sizeDelta = new Vector2(bodyW, ToyRoomIntroTimeline.FairyTargetHeight);

            fairyBodyGlowRect.anchoredPosition = new Vector2(bodyLeft - 18f, -(yOrigin - 18f));
            fairyBodyGlowRect.sizeDelta = new Vector2(bodyW + 36f, ToyRoomIntroTimeline.FairyTargetHeight + 36f);

            fairyLeftWingRect.anchoredPosition = new Vector2(leftWingLeft, -yOrigin);
            fairyLeftWingRect.sizeDelta = new Vector2(leftWingDrawW, ToyRoomIntroTimeline.FairyTargetHeight);

            fairyRightWingRect.anchoredPosition = new Vector2(rightWingLeft, -yOrigin);
            fairyRightWingRect.sizeDelta = new Vector2(rightWingDrawW, ToyRoomIntroTimeline.FairyTargetHeight);
        }

        private void ApplyWarmCurtain(ToyRoomIntroSample sample)
        {
            if (warmCurtainImage == null)
            {
                return;
            }

            float alpha = Mathf.Clamp01(sample.WarmCurtainAlpha);
            warmCurtainImage.gameObject.SetActive(alpha > 0.002f);
            warmCurtainImage.color = new Color(1.0f, 0.9804f, 0.9412f, alpha);
        }

        private bool IsMobileViewport()
        {
            float w = forcedLogicalWidth > 0f ? forcedLogicalWidth : Screen.width;
            float h = forcedLogicalHeight > 0f ? forcedLogicalHeight : Screen.height;
            return Application.isMobilePlatform
                || UnityEngine.InputSystem.Touchscreen.current != null
                || w <= 960f
                || h <= 540f;
        }

        private float ComputeMobileLogicalPxScale(Vector2 canvasSize)
        {
            float logicalW = forcedLogicalWidth > 0f ? forcedLogicalWidth : Screen.width;
            float logicalH = forcedLogicalHeight > 0f ? forcedLogicalHeight : Screen.height;
            if (logicalW <= 1f || logicalH <= 1f)
            {
                return canvasSize.x / 390f;
            }

            float minDim = Mathf.Min(logicalW, logicalH);
            if (forcedLogicalWidth <= 0f && Application.isMobilePlatform && minDim >= 700f)
            {
                float dpr = Mathf.Clamp(minDim / 390f, 1.5f, 3.5f);
                logicalW /= dpr;
            }

            return Mathf.Clamp(canvasSize.x / Mathf.Max(280f, logicalW), 0.5f, 4.5f);
        }

        private void ApplyPoeticNarrationCard(ToyRoomIntroSample sample, Vector2 canvasSize, float cssScale)
        {
            if (narrationCardRoot == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(sample.DialogueText))
            {
                narrationCardRoot.SetActive(false);
                return;
            }

            narrationCardRoot.SetActive(true);

            bool isMobile = IsMobileViewport();
            float pxScale = isMobile ? ComputeMobileLogicalPxScale(canvasSize) : cssScale;

            // Regressao 1: No modo mobile, todas as fontes da cena da Sala de Brinquedos respeitam 14 a 16 px logicos
            int headingFontPx = isMobile
                ? Mathf.RoundToInt(15f * pxScale)
                : Mathf.Max(12, Mathf.RoundToInt(Mathf.Min(21f * cssScale, Mathf.Min(25f * cssScale, canvasSize.x * 0.061f))));
            int bodyFontPx = isMobile
                ? Mathf.RoundToInt(16f * pxScale)
                : Mathf.Max(13, Mathf.RoundToInt(Mathf.Min(25f * cssScale, canvasSize.x * 0.061f)));
            int hintFontPx = isMobile
                ? Mathf.RoundToInt(14f * pxScale)
                : Mathf.Max(10, Mathf.RoundToInt(11f * cssScale));

            float width = isMobile
                ? Mathf.Min(560f * pxScale, canvasSize.x - (24f * pxScale))
                : Mathf.Min(680f * cssScale, canvasSize.x - (32f * cssScale));
            float lineHeight = bodyFontPx * (isMobile ? 1.32f : 1.22f);

            string[] rawLines = sample.DialogueText.Split('\n');
            int lineCount = Mathf.Max(1, rawLines.Length);
            bool hasHeading = !string.IsNullOrEmpty(sample.DialogueHeading);
            float headingHeight = hasHeading ? (isMobile ? 22f * pxScale : 30f * cssScale) : 0f;
            float controlsHeight = isMobile ? 36f * pxScale : 30f * cssScale;
            float verticalPad = isMobile ? 26f * pxScale : 38f * cssScale;
            float height = (lineCount * lineHeight) + verticalPad + headingHeight + controlsHeight;

            float xCenter = canvasSize.x * 0.5f;
            float yTop = (isMobile ? 10f : 16f) * pxScale;
            float left = xCenter - (width * 0.5f);

            narrationCardRect.anchoredPosition = new Vector2(left, -yTop);
            narrationCardRect.sizeDelta = new Vector2(width, height);

            float borderInset = 7f * pxScale;
            narrationInnerRect.anchoredPosition = new Vector2(borderInset, -borderInset);
            narrationInnerRect.sizeDelta = new Vector2(width - (borderInset * 2f), height - (borderInset * 2f));

            float flowerOffset = 15f * pxScale;
            flowerLeftRect.anchoredPosition = new Vector2(flowerOffset, -flowerOffset);
            flowerLeftRect.localScale = Vector3.one * pxScale;
            flowerRightRect.anchoredPosition = new Vector2(width - flowerOffset, -flowerOffset);
            flowerRightRect.localScale = Vector3.one * pxScale;

            narrationHeadingText.gameObject.SetActive(hasHeading);
            if (hasHeading)
            {
                narrationHeadingText.text = sample.DialogueHeading;
                narrationHeadingText.fontSize = headingFontPx;
                narrationHeadingText.rectTransform.anchoredPosition = new Vector2(0f, -(8f * pxScale));
                narrationHeadingText.rectTransform.sizeDelta = new Vector2(width, 24f * pxScale);
            }

            narrationBodyText.text = sample.DialogueText;
            narrationBodyText.fontSize = bodyFontPx;
            float bodyTop = ((isMobile ? 12f : 19f) * pxScale) + headingHeight;
            float bodyHeight = lineCount * lineHeight;
            narrationBodyText.rectTransform.anchoredPosition = new Vector2(16f * pxScale, -bodyTop);
            narrationBodyText.rectTransform.sizeDelta = new Vector2(width - (32f * pxScale), bodyHeight + (6f * pxScale));

            float controlsTop = height - controlsHeight - (5f * pxScale);
            float hintRowH = isMobile ? 18f * pxScale : 16f * pxScale;
            float hintRowGap = isMobile ? 16f * pxScale : 13f * pxScale;

            narrationAutoHintText.fontSize = hintFontPx;
            narrationAutoHintText.rectTransform.anchoredPosition = new Vector2(0f, -controlsTop);
            narrationAutoHintText.rectTransform.sizeDelta = new Vector2(width, hintRowH);

            narrationClickHintText.fontSize = hintFontPx;
            narrationClickHintText.rectTransform.anchoredPosition = new Vector2(0f, -(controlsTop + hintRowGap));
            narrationClickHintText.rectTransform.sizeDelta = new Vector2(width, hintRowH);
        }

        private void EnsureBuilt()
        {
            if (built && introCanvas != null)
            {
                return;
            }

            built = true;
            LoadOfficialSprites();

            Font uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (poeticFont == null)
            {
                poeticFont = Resources.Load<Font>("dark-room/caveat");
            }

            Font caveat = poeticFont != null ? poeticFont : uiFont;

            var canvasGo = new GameObject(
                "ToyRoom_IntroStopMotion_Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);

            introCanvas = canvasGo.GetComponent<Canvas>();
            introCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            introCanvas.sortingOrder = 120;
            canvasRect = canvasGo.GetComponent<RectTransform>();

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            // Fundo escuro #100C10
            var bgGo = CreateTopLeftPanel(
                "IntroDarkBackdrop",
                canvasGo.transform,
                Vector2.zero,
                new Vector2(1920f, 1080f),
                new Color(0.0627f, 0.0471f, 0.0627f, 1f));
            StretchFull(bgGo.GetComponent<RectTransform>());
            var advanceBgBtn = bgGo.AddComponent<Button>();
            advanceBgBtn.transition = Selectable.Transition.None;
            advanceBgBtn.onClick.AddListener(() => TryAdvanceDialogue());

            // Viewport com RectMask2D que recorta o plano cinematografico
            var vpGo = new GameObject("CameraViewport", typeof(RectTransform), typeof(RectMask2D));
            vpGo.transform.SetParent(canvasGo.transform, false);
            viewportMaskRect = vpGo.GetComponent<RectTransform>();
            SetTopLeftPivot(viewportMaskRect);

            // Stage World (1672 x 941)
            var worldGo = new GameObject("StageWorld_1672x941", typeof(RectTransform));
            worldGo.transform.SetParent(viewportMaskRect, false);
            stageWorldRect = worldGo.GetComponent<RectTransform>();
            SetTopLeftPivot(stageWorldRect);
            stageWorldRect.sizeDelta = new Vector2(
                ToyRoomIntroTimeline.RoomPixelWidth,
                ToyRoomIntroTimeline.RoomPixelHeight);

            // 1. Pintura da Sala (room-v1.png)
            var roomGo = CreateTopLeftPanel(
                "RoomPainting",
                stageWorldRect,
                Vector2.zero,
                new Vector2(ToyRoomIntroTimeline.RoomPixelWidth, ToyRoomIntroTimeline.RoomPixelHeight),
                Color.white);
            roomBackdropImage = roomGo.GetComponent<Image>();
            roomBackdropImage.sprite = roomSprite;
            roomBackdropImage.raycastTarget = false;

            // 2. Sombra eliptica sob os pes da Yuna: rgba(49, 27, 15, 0.22)
            var shadowGo = CreateTopLeftPanel(
                "GirlFloorShadow",
                stageWorldRect,
                Vector2.zero,
                new Vector2(156f, 30f),
                new Color(49f / 255f, 27f / 255f, 15f / 255f, 0.22f));
            girlShadowRect = shadowGo.GetComponent<RectTransform>();
            girlShadowImage = shadowGo.GetComponent<Image>();
            girlShadowImage.sprite = ellipseShadowSprite;
            girlShadowImage.raycastTarget = false;

            // 3. Yuna em 4 poses de stop motion (girl-acting-v1.png)
            var girlGo = CreateTopLeftPanel(
                "GirlStopMotionActor",
                stageWorldRect,
                Vector2.zero,
                new Vector2(310f, 485f),
                Color.white);
            girlRect = girlGo.GetComponent<RectTransform>();
            girlImage = girlGo.GetComponent<Image>();
            girlImage.sprite = girlPoseSprites[0];
            girlImage.raycastTarget = false;

            // 4. Nanda com asas articuladas e halo dourado (#FFDB84)
            var fairyRootGo = new GameObject("FairyStopMotionActor", typeof(RectTransform));
            fairyRootGo.transform.SetParent(stageWorldRect, false);
            fairyRootRect = fairyRootGo.GetComponent<RectTransform>();
            SetTopLeftPivot(fairyRootRect);
            fairyRootRect.sizeDelta = Vector2.zero;

            var glowGo = CreateTopLeftPanel(
                "FairyWarmGlow",
                fairyRootRect,
                Vector2.zero,
                new Vector2(88f, 150f),
                new Color(0.40f, 0.82f, 1.00f, 0.45f));
            fairyBodyGlowRect = glowGo.GetComponent<RectTransform>();
            fairyBodyGlowImage = glowGo.GetComponent<Image>();
            fairyBodyGlowImage.sprite = CreateSoftRadialGlowSprite(96);
            fairyBodyGlowImage.raycastTarget = false;

            var leftWingGo = CreateTopLeftPanel(
                "FairyLeftWing",
                fairyRootRect,
                Vector2.zero,
                new Vector2(26.4f, 120f),
                Color.white);
            fairyLeftWingRect = leftWingGo.GetComponent<RectTransform>();
            fairyLeftWingImage = leftWingGo.GetComponent<Image>();
            fairyLeftWingImage.sprite = fairyLeftWingSprite;
            fairyLeftWingImage.raycastTarget = false;

            var rightWingGo = CreateTopLeftPanel(
                "FairyRightWing",
                fairyRootRect,
                Vector2.zero,
                new Vector2(25.3f, 120f),
                Color.white);
            fairyRightWingRect = rightWingGo.GetComponent<RectTransform>();
            fairyRightWingImage = rightWingGo.GetComponent<Image>();
            fairyRightWingImage.sprite = fairyRightWingSprite;
            fairyRightWingImage.raycastTarget = false;

            var bodyGo = CreateTopLeftPanel(
                "FairyBodyCenter",
                fairyRootRect,
                Vector2.zero,
                new Vector2(58.3f, 120f),
                Color.white);
            fairyBodyRect = bodyGo.GetComponent<RectTransform>();
            fairyBodyImage = bodyGo.GetComponent<Image>();
            fairyBodyImage.sprite = fairyBodySprite;
            fairyBodyImage.raycastTarget = false;

            // 5. Cortina de fade creme rgba(255, 250, 240, alpha)
            var curtainGo = CreateTopLeftPanel(
                "WarmFadeCurtain",
                canvasGo.transform,
                Vector2.zero,
                new Vector2(1920f, 1080f),
                new Color(1.0f, 0.9804f, 0.9412f, 0f));
            StretchFull(curtainGo.GetComponent<RectTransform>());
            warmCurtainImage = curtainGo.GetComponent<Image>();
            warmCurtainImage.raycastTarget = false;
            curtainGo.SetActive(false);

            // 6. Quadro de narracao poetica e falas da introducao (renderPoeticNarration)
            BuildPoeticNarrationCard(canvasGo.transform, caveat, uiFont);

            canvasGo.SetActive(false);
        }

        private void BuildPoeticNarrationCard(Transform parent, Font caveatFont, Font sansFont)
        {
            narrationCardRoot = CreateTopLeftPanel(
                "IntroPoeticNarrationCard",
                parent,
                new Vector2(140f, -16f),
                new Vector2(680f, 130f),
                Color.white);
            narrationCardRect = narrationCardRoot.GetComponent<RectTransform>();

            var cardImg = narrationCardRoot.GetComponent<Image>();
            cardImg.sprite = roundedOuterCardSprite;
            cardImg.type = Image.Type.Sliced;

            var dropShadow = narrationCardRoot.AddComponent<Shadow>();
            dropShadow.effectColor = new Color(57f / 255f, 36f / 255f, 34f / 255f, 0.20f);
            dropShadow.effectDistance = new Vector2(0f, -3f);

            var innerGo = CreateTopLeftPanel(
                "InnerSageBorder",
                narrationCardRoot.transform,
                new Vector2(7f, -7f),
                new Vector2(666f, 116f),
                Color.white);
            narrationInnerRect = innerGo.GetComponent<RectTransform>();
            var innerImg = innerGo.GetComponent<Image>();
            innerImg.sprite = roundedInnerBorderSprite;
            innerImg.type = Image.Type.Sliced;
            innerImg.raycastTarget = false;

            flowerLeftRect = BuildCornerFlower(narrationCardRoot.transform, new Vector2(15f, -15f));
            flowerRightRect = BuildCornerFlower(narrationCardRoot.transform, new Vector2(665f, -15f));

            narrationHeadingText = CreateTopLeftText(
                "HeadingText",
                narrationCardRoot.transform,
                caveatFont,
                21,
                FontStyle.Normal,
                new Color(132f / 255f, 100f / 255f, 103f / 255f, 1f));

            narrationBodyText = CreateTopLeftText(
                "BodyText",
                narrationCardRoot.transform,
                caveatFont,
                25,
                FontStyle.Normal,
                new Color(81f / 255f, 62f / 255f, 56f / 255f, 1f));

            Color hintCol = new Color(122f / 255f, 92f / 255f, 95f / 255f, 1f);
            narrationAutoHintText = CreateTopLeftText(
                "AutoHintText",
                narrationCardRoot.transform,
                sansFont,
                11,
                FontStyle.Bold,
                hintCol);
            narrationAutoHintText.text = "Avança sozinho";

            narrationClickHintText = CreateTopLeftText(
                "ClickHintText",
                narrationCardRoot.transform,
                sansFont,
                11,
                FontStyle.Normal,
                hintCol);
            narrationClickHintText.text = "Clique/toque no quadro adianta";

            var clickBtn = narrationCardRoot.AddComponent<Button>();
            clickBtn.transition = Selectable.Transition.None;
            clickBtn.onClick.AddListener(() => TryAdvanceDialogue());
        }

        private void LoadOfficialSprites()
        {
            Texture2D roomTex = Resources.Load<Texture2D>(RoomResourcePath);
            if (roomTex != null)
            {
                roomSprite = Sprite.Create(
                    roomTex,
                    new Rect(0f, 0f, roomTex.width, roomTex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }

            Texture2D girlTex = Resources.Load<Texture2D>(GirlResourcePath);
            if (girlTex != null)
            {
                for (int i = 0; i < ToyRoomIntroTimeline.GirlFrames.Count; i++)
                {
                    ToyRoomIntroGirlFrameSpec f = ToyRoomIntroTimeline.GirlFrames[i];
                    float unityY = girlTex.height - f.Y - f.Height;
                    girlPoseSprites[i] = Sprite.Create(
                        girlTex,
                        new Rect(f.X, unityY, f.Width, f.Height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                }
            }

            Texture2D fairyTex = Resources.Load<Texture2D>(FairyResourcePath);
            if (fairyTex != null)
            {
                float w = fairyTex.width;
                float h = fairyTex.height;
                float left = Mathf.Round(w * ToyRoomIntroTimeline.FairyLeftWingSplitRatio);
                float right = Mathf.Round(w * ToyRoomIntroTimeline.FairyRightWingSplitRatio);

                fairyLeftWingSprite = Sprite.Create(
                    fairyTex,
                    new Rect(0f, 0f, left, h),
                    new Vector2(1f, 0.5f),
                    100f);
                fairyBodySprite = Sprite.Create(
                    fairyTex,
                    new Rect(left, 0f, right - left, h),
                    new Vector2(0.5f, 0.5f),
                    100f);
                fairyRightWingSprite = Sprite.Create(
                    fairyTex,
                    new Rect(right, 0f, w - right, h),
                    new Vector2(0f, 0.5f),
                    100f);
            }

            ellipseShadowSprite = ToyRoomVirtualJoystick.CreateCircleSprite(128, 0f, 0.96f);
            roundedOuterCardSprite = CreateRoundedCardSprite(
                96,
                16f,
                new Color(0.985f, 0.968f, 0.925f, 1f),
                new Color(214f / 255f, 174f / 255f, 177f / 255f, 1f),
                3f);
            roundedInnerBorderSprite = CreateRoundedCardSprite(
                96,
                10f,
                new Color(0f, 0f, 0f, 0f),
                new Color(184f / 255f, 201f / 255f, 187f / 255f, 1f),
                1.8f);
        }

        private static RectTransform BuildCornerFlower(Transform parent, Vector2 topLeftOffset)
        {
            var flowerGo = new GameObject("CornerFlower", typeof(RectTransform), typeof(Image));
            flowerGo.transform.SetParent(parent, false);
            var rt = flowerGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = topLeftOffset;
            rt.sizeDelta = new Vector2(16f, 16f);

            var img = flowerGo.GetComponent<Image>();
            img.sprite = CreateCornerFlowerSprite(64);
            img.color = Color.white;
            img.raycastTarget = false;

            return rt;
        }

        private static Sprite CreateCornerFlowerSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            float center = (size - 1) * 0.5f;
            float scale = size / 16f;
            float petalDist = 4.0f * scale;
            float petalRadius = 3.05f * scale;
            float centerRadius = 2.05f * scale;

            Color petalCol = new Color(229f / 255f, 190f / 255f, 195f / 255f, 1f);
            Color centerCol = new Color(198f / 255f, 174f / 255f, 121f / 255f, 1f);

            Vector2[] petalCenters =
            {
                new Vector2(center + petalDist, center),
                new Vector2(center, center + petalDist),
                new Vector2(center - petalDist, center),
                new Vector2(center, center - petalDist)
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    float petalAlpha = 0f;
                    for (int i = 0; i < 4; i++)
                    {
                        float d = Vector2.Distance(p, petalCenters[i]);
                        float a = Mathf.Clamp01((petalRadius + 0.6f) - d);
                        if (a > petalAlpha)
                        {
                            petalAlpha = a;
                        }
                    }

                    float dc = Vector2.Distance(p, new Vector2(center, center));
                    float centerAlpha = Mathf.Clamp01((centerRadius + 0.6f) - dc);

                    if (petalAlpha <= 0f && centerAlpha <= 0f)
                    {
                        pixels[(y * size) + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    Color final = Color.Lerp(petalCol, centerCol, centerAlpha);
                    final.a = Mathf.Max(petalAlpha, centerAlpha);
                    pixels[(y * size) + x] = (Color32)final;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateSoftRadialGlowSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            float invHalf = 1f / half;

            for (int y = 0; y < size; y++)
            {
                float dy = (y - half) * invHalf;
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) * invHalf;
                    float r = Mathf.Sqrt((dx * dx) + (dy * dy));
                    float falloff = Mathf.Clamp01(1f - r);
                    float smooth = falloff * falloff * (3f - (2f * falloff));
                    byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(smooth * 255f), 0, 255);
                    pixels[(y * size) + x] = new Color32(255, 255, 255, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateRoundedCardSprite(
            int size,
            float cornerRadius,
            Color fillColor,
            Color strokeColor,
            float strokeWidth)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            float half = size * 0.5f;
            float boxExtent = half - cornerRadius - 1f;

            for (int y = 0; y < size; y++)
            {
                float py = Mathf.Abs((y + 0.5f) - half);
                for (int x = 0; x < size; x++)
                {
                    float px = Mathf.Abs((x + 0.5f) - half);
                    float qx = Mathf.Max(px - boxExtent, 0f);
                    float qy = Mathf.Max(py - boxExtent, 0f);
                    float dist = Mathf.Sqrt((qx * qx) + (qy * qy)) + Mathf.Min(Mathf.Max(px - boxExtent, py - boxExtent), 0f) - cornerRadius;

                    float outerAlpha = Mathf.Clamp01(0.5f - dist);
                    if (outerAlpha <= 0f)
                    {
                        pixels[(y * size) + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    float innerBlend = Mathf.Clamp01((-dist - strokeWidth) + 0.5f);
                    Color c = Color.Lerp(strokeColor, fillColor, innerBlend);
                    c.a *= outerAlpha;
                    pixels[(y * size) + x] = (Color32)c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);

            float border = Mathf.Ceil(cornerRadius + strokeWidth + 2f);
            return Sprite.Create(
                tex,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
        }

        private static GameObject CreateTopLeftPanel(
            string name,
            Transform parent,
            Vector2 anchoredPos,
            Vector2 size,
            Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            SetTopLeftPivot(rt);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = color;
            return go;
        }

        private static Text CreateTopLeftText(
            string name,
            Transform parent,
            Font font,
            int fontSize,
            FontStyle style,
            Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            SetTopLeftPivot(rt);
            var txt = go.GetComponent<Text>();
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        private static void SetTopLeftPivot(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
