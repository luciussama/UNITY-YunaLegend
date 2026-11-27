using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using YunaLegend.Presentation;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Controla a interface da Toy Room seguindo a assinatura visual do acervo legado
    /// com layout responsivo mobile:
    /// - Prioridade 1: textos (dialogo e narracao poetica) na parte superior no celular, sem encobrir Yuna ou Nanda.
    /// - Prioridade 2: fontes entre 14 e 16px logicos no celular.
    /// - Prioridade 3: preserva estetica vitoriana (ameixa/dourado, pergaminho, flores nos cantos, orbe dourado).
    /// - Prioridade 4: todos os enfeites, molduras e botoes cabem integralmente dentro da tela do celular.
    /// </summary>
    public sealed class ToyRoomHudController : MonoBehaviour
    {
        [SerializeField] private Font poeticFont;

        private RectTransform canvasRect;
        private RectTransform counterPanelRect;
        private Text counterText;
        private RectTransform progressBgRect;
        private Image progressFill;

        private RectTransform topRightContainer;
        private RectTransform muteBtnRect;
        private RectTransform pauseBtnRect;
        private Text muteButtonLabel;
        private Text pauseButtonLabel;

        private GameObject promptRoot;
        private RectTransform promptRect;
        private RectTransform promptHaloRect;
        private Image promptBackground;
        private Button promptButton;
        private Text promptText;
        private Text promptSubLabel;

        private GameObject joystickRoot;
        private RectTransform joystickRect;
        private RectTransform joystickHandleRect;
        private ToyRoomVirtualJoystick joystickComponent;

        private GameObject dialogueRoot;
        private RectTransform dialogueRect;
        private RectTransform dialogueInnerRect;
        private RectTransform portraitHaloRect;
        private RectTransform portraitRingRect;
        private Image portraitRing;
        private Image portraitHalo;
        private Text portraitInitialText;
        private Text speakerText;
        private Text dialogueBodyText;
        private Text dialogueHintText;

        private GameObject poeticRoot;
        private RectTransform poeticRect;
        private RectTransform poeticInnerRect;
        private RectTransform flowerLeftRect;
        private RectTransform flowerRightRect;
        private Text poeticText;
        private Text poeticHintText;

        private GameObject pauseMenuRoot;
        private RectTransform pauseCardRect;
        private Text pauseTitleText;
        private readonly RectTransform[] pauseButtonRects = new RectTransform[4];
        private readonly Text[] pauseButtonLabels = new Text[4];
        private Text pauseMuteButtonLabel;

        private Vector2 lastCanvasSize = new Vector2(-1f, -1f);
        private float forcedLogicalWidth = -1f;
        private float forcedLogicalHeight = -1f;
        private bool forceMobileControls;
        private bool initialized;
        private bool isPaused;
        private bool isMuted;

        public event Action OnAdvanceRequested;
        public event Action OnActionRequested;
        public event Action<Vector2> OnMoveInputChanged;
        public event Action<bool> OnPauseToggled;
        public event Action OnMuteToggled;
        public event Action OnRestartRequested;
        public event Action OnReturnToTitleRequested;

        public bool IsPaused => isPaused;

        public void ConfigureFont(Font poeticFontAsset)
        {
            poeticFont = poeticFontAsset;
        }

        public void SetViewportOverride(float logicalWidth, float logicalHeight)
        {
            forcedLogicalWidth = logicalWidth;
            forcedLogicalHeight = logicalHeight;
            lastCanvasSize = new Vector2(-1f, -1f);
            EnsureBuilt();
            ApplyResponsiveLayout(canvasRect.rect.size);
        }

        public void ClearViewportOverride()
        {
            forcedLogicalWidth = -1f;
            forcedLogicalHeight = -1f;
            lastCanvasSize = new Vector2(-1f, -1f);
        }

        public void ForceShowMobileControlsForCapture(bool force)
        {
            forceMobileControls = force;
            SetMobileControlsVisible(force);
        }

        private void Awake()
        {
            EnsureBuilt();
        }

        private void Update()
        {
            if (canvasRect == null)
            {
                return;
            }

            Vector2 size = canvasRect.rect.size;
            if (size.x > 1f && size.y > 1f && (!Mathf.Approximately(size.x, lastCanvasSize.x) || !Mathf.Approximately(size.y, lastCanvasSize.y)))
            {
                lastCanvasSize = size;
                ApplyResponsiveLayout(size);
            }
        }

        public void UpdateToyCounter(int organizedCount, int totalCount = ToyRoomCatalog.TotalToyCount)
        {
            EnsureBuilt();
            int clamped = Mathf.Clamp(organizedCount, 0, totalCount);
            counterText.text = $"Brinquedos Arrumados: {clamped}/{totalCount}";
            progressFill.fillAmount = totalCount > 0 ? (float)clamped / totalCount : 0f;
        }

        public void SetActionPrompt(string label)
        {
            SetActionPrompt(label, true);
        }

        public void SetActionPrompt(string label, bool isActionReady)
        {
            EnsureBuilt();
            bool visible = !string.IsNullOrEmpty(label);
            promptRoot.SetActive(visible);
            if (!visible)
            {
                return;
            }

            promptText.text = label;
            if (promptBackground != null)
            {
                promptBackground.color = isActionReady
                    ? Color.white
                    : new Color(1f, 1f, 1f, 0.72f);
            }

            if (promptText != null)
            {
                promptText.color = isActionReady
                    ? new Color(0.12f, 0.07f, 0.02f, 1f)
                    : new Color(0.12f, 0.07f, 0.02f, 0.78f);
            }
        }

        public void SetMobileControlsVisible(bool visible)
        {
            EnsureBuilt();
            bool showJoystick = visible && (forceMobileControls || IsMobileOrTouchDevice());
            if (joystickRoot != null && joystickRoot.activeSelf != showJoystick)
            {
                joystickRoot.SetActive(showJoystick);
            }
        }

        private static bool IsMobileOrTouchDevice()
        {
            return Application.isMobilePlatform || Touchscreen.current != null;
        }

        public void ShowBeat(ToyRoomNarrativeBeat beat)
        {
            EnsureBuilt();

            if (beat.Kind == ToyRoomBeatKind.Dialogue)
            {
                dialogueRoot.SetActive(true);
                poeticRoot.SetActive(false);

                bool isNanda = beat.Speaker == ToyRoomSpeaker.Nanda;
                speakerText.text = isNanda ? "NANDA" : "YUNA";
                speakerText.color = isNanda
                    ? new Color(0.08f, 0.34f, 0.48f, 1f)
                    : new Color(0.55f, 0.19f, 0.28f, 1f);
                portraitRing.color = isNanda
                    ? new Color(0.99f, 0.88f, 0.28f, 1f)
                    : new Color(0.96f, 0.45f, 0.71f, 1f);
                portraitHalo.color = isNanda
                    ? new Color(0.02f, 0.71f, 0.83f, 0.35f)
                    : new Color(0.96f, 0.45f, 0.71f, 0.35f);
                portraitInitialText.text = isNanda ? "N" : "Y";
                portraitInitialText.color = isNanda ? new Color(0.45f, 0.88f, 1f, 1f) : new Color(1f, 0.72f, 0.78f, 1f);
                dialogueBodyText.text = beat.Text;
                return;
            }

            if (beat.Kind == ToyRoomBeatKind.Poetic)
            {
                dialogueRoot.SetActive(false);
                poeticRoot.SetActive(true);
                poeticText.text = beat.Text;
                return;
            }

            ClearNarrativeOverlays();
        }

        public void ClearNarrativeOverlays()
        {
            EnsureBuilt();
            dialogueRoot.SetActive(false);
            poeticRoot.SetActive(false);
        }

        public void SetPausedState(bool paused)
        {
            EnsureBuilt();
            isPaused = paused;
            if (pauseMenuRoot != null)
            {
                pauseMenuRoot.SetActive(isPaused);
            }

            if (isPaused)
            {
                if (promptRoot != null) promptRoot.SetActive(false);
                if (joystickRoot != null) joystickRoot.SetActive(false);
            }
        }

        public void SetMutedState(bool muted)
        {
            EnsureBuilt();
            isMuted = muted;
            string label = isMuted ? "Som: Mudo" : "Som: Ligado";
            if (muteButtonLabel != null)
            {
                muteButtonLabel.text = isMuted ? "Mudo" : "Som";
            }

            if (pauseMuteButtonLabel != null)
            {
                pauseMuteButtonLabel.text = label;
            }
        }

        private void EnsureBuilt()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            EnsureEventSystem();
            var uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var narrativeFont = poeticFont != null ? poeticFont : uiFont;

            var canvasGo = new GameObject("ToyRoom_HUD_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            canvasRect = canvasGo.GetComponent<RectTransform>();

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            BuildCounterPanel(canvasGo.transform, uiFont);
            BuildTopRightControls(canvasGo.transform, uiFont);
            BuildActionPrompt(canvasGo.transform, uiFont);
            BuildVirtualJoystick(canvasGo.transform);
            BuildDialogueBox(canvasGo.transform, uiFont);
            BuildPoeticOverlay(canvasGo.transform, narrativeFont, uiFont);
            BuildPauseMenu(canvasGo.transform, uiFont);

            UpdateToyCounter(0);
            SetActionPrompt(null);
            SetMobileControlsVisible(false);
            ClearNarrativeOverlays();
            SetPausedState(false);
            SetMutedState(false);

            ApplyResponsiveLayout(canvasRect.rect.size.x > 1f ? canvasRect.rect.size : new Vector2(1920f, 1080f));
        }

        private void ApplyResponsiveLayout(Vector2 canvasSize)
        {
            if (canvasSize.x <= 1f || canvasSize.y <= 1f)
            {
                return;
            }

            bool isMobile = IsMobileViewport();
            if (!isMobile)
            {
                ApplyDesktopLayout();
                return;
            }

            float px = ComputePxToCanvas(canvasSize);
            bool isLandscape = canvasSize.x > canvasSize.y;
            float marginX = 10f * px;
            float topY = -8f * px;
            float usableW = Mathf.Max(220f * px, canvasSize.x - marginX * 2f);

            int f14 = Mathf.RoundToInt(16f * px);
            int f15 = Mathf.RoundToInt(17f * px);
            int f16 = Mathf.RoundToInt(18f * px);

            // Top bar: Counter (left) + Som/Pausa (right), sem sobreposicao
            float topBarH = 48f * px;
            float btnW = 58f * px;
            float btnH = 48f * px;
            float btnGap = 6f * px;
            float rightBlockW = btnW * 2f + btnGap;
            float counterW = Mathf.Min(230f * px, usableW - rightBlockW - 8f * px);

            counterPanelRect.anchoredPosition = new Vector2(marginX, topY);
            counterPanelRect.sizeDelta = new Vector2(counterW, topBarH);
            counterText.fontSize = f14;
            counterText.rectTransform.offsetMin = new Vector2(8f * px, 14f * px);
            counterText.rectTransform.offsetMax = new Vector2(-8f * px, -3f * px);
            progressBgRect.anchoredPosition = new Vector2(8f * px, 5f * px);
            progressBgRect.sizeDelta = new Vector2(-16f * px, 7f * px);

            topRightContainer.anchoredPosition = new Vector2(-marginX, topY - 2f * px);
            topRightContainer.sizeDelta = new Vector2(rightBlockW, btnH);
            muteBtnRect.anchoredPosition = new Vector2(-(btnW + btnGap), 0f);
            muteBtnRect.sizeDelta = new Vector2(btnW, btnH);
            pauseBtnRect.anchoredPosition = Vector2.zero;
            pauseBtnRect.sizeDelta = new Vector2(btnW, btnH);
            muteButtonLabel.fontSize = f14;
            pauseButtonLabel.fontSize = f14;

            float belowTopBarY = isLandscape ? topY : -(topBarH + 16f * px);
            float landscapeCenterW = usableW - counterW - rightBlockW - 16f * px;
            float cardW = isLandscape
                ? Mathf.Clamp(landscapeCenterW, 280f * px, 460f * px)
                : Mathf.Min(460f * px, usableW);

            // Em paisagem mobile, centraliza entre o contador (esq) e Som/Pausa (dir) no topo extremo
            // para nao encobrir nem Yuna nem Nanda.
            float cardCenterX = isLandscape ? (counterW - rightBlockW) * 0.5f : 0f;

            // Prioridade 1: Caixa de Dialogo Vitoriana NO TOPO (acima de Yuna e Nanda)
            float dlgH = isLandscape ? 100f * px : 124f * px;
            dialogueRect.anchorMin = new Vector2(0.5f, 1f);
            dialogueRect.anchorMax = new Vector2(0.5f, 1f);
            dialogueRect.pivot = new Vector2(0.5f, 1f);
            dialogueRect.anchoredPosition = new Vector2(cardCenterX, belowTopBarY);
            dialogueRect.sizeDelta = new Vector2(cardW, dlgH);
            dialogueInnerRect.sizeDelta = new Vector2(-8f * px, -8f * px);

            float portraitSize = isLandscape ? 40f * px : 48f * px;
            float portraitX = isLandscape ? 28f * px : 34f * px;
            portraitHaloRect.anchoredPosition = new Vector2(portraitX, 0f);
            portraitHaloRect.sizeDelta = new Vector2(portraitSize + 10f * px, portraitSize + 10f * px);
            portraitRingRect.anchoredPosition = new Vector2(portraitX, 0f);
            portraitRingRect.sizeDelta = new Vector2(portraitSize, portraitSize);
            portraitInitialText.fontSize = f16;

            float textLeft = portraitX + portraitSize * 0.5f + 12f * px;
            speakerText.fontSize = f14;
            speakerText.rectTransform.anchoredPosition = new Vector2(textLeft, isLandscape ? -6f * px : -8f * px);
            speakerText.rectTransform.sizeDelta = new Vector2(-(textLeft + 10f * px), 20f * px);

            dialogueBodyText.fontSize = f15;
            dialogueBodyText.rectTransform.offsetMin = new Vector2(textLeft, 8f * px);
            dialogueBodyText.rectTransform.offsetMax = new Vector2(-26f * px, isLandscape ? -22f * px : -26f * px);

            dialogueHintText.enabled = false;
            dialogueHintText.text = string.Empty;
            HudTheme.EnsureCornerTapCue(dialogueRoot.transform, 18f * px, 8f * px, 6f * px, new Color(HudTheme.Plum.r, HudTheme.Plum.g, HudTheme.Plum.b, 0.68f));

            // Quadro Poetico em Pergaminho NO TOPO abaixo da barra superior (zero colisao com Counter/Botoes)
            float poeticH = 98f * px;
            poeticRect.anchorMin = new Vector2(0.5f, 1f);
            poeticRect.anchorMax = new Vector2(0.5f, 1f);
            poeticRect.pivot = new Vector2(0.5f, 1f);
            poeticRect.anchoredPosition = new Vector2(0f, belowTopBarY);
            poeticRect.sizeDelta = new Vector2(cardW, poeticH);
            poeticInnerRect.sizeDelta = new Vector2(-10f * px, -10f * px);

            float flowerOffset = 12f * px;
            float flowerScale = Mathf.Clamp(px * 0.65f, 0.55f, 1.35f);
            flowerLeftRect.anchoredPosition = new Vector2(flowerOffset, -flowerOffset);
            flowerLeftRect.localScale = Vector3.one * flowerScale;
            flowerRightRect.anchoredPosition = new Vector2(-flowerOffset, -flowerOffset);
            flowerRightRect.localScale = Vector3.one * flowerScale;

            poeticText.fontSize = f16;
            poeticText.rectTransform.offsetMin = new Vector2(18f * px, 10f * px);
            poeticText.rectTransform.offsetMax = new Vector2(-28f * px, -10f * px);

            poeticHintText.enabled = false;
            poeticHintText.text = string.Empty;
            HudTheme.EnsureCornerTapCue(poeticRoot.transform, 18f * px, 8f * px, 6f * px, new Color(HudTheme.Plum.r, HudTheme.Plum.g, HudTheme.Plum.b, 0.68f));

            // Controles inferiores (Joystick + Botao Dourado PEGAR/GUARDAR) cabendo integralmente no enquadramento
            float joySize = 104f * px;
            joystickRect.anchoredPosition = new Vector2(marginX + joySize * 0.5f + 6f * px, 68f * px);
            joystickRect.sizeDelta = new Vector2(joySize, joySize);
            joystickHandleRect.sizeDelta = new Vector2(44f * px, 44f * px);
            joystickComponent.Configure(joystickRect, joystickHandleRect, 34f * px, 0.1f);

            float orbSize = 104f * px;
            float haloSize = 120f * px;
            promptRect.anchoredPosition = new Vector2(-(marginX + 12f * px), 22f * px);
            promptRect.sizeDelta = new Vector2(orbSize, orbSize);
            promptHaloRect.sizeDelta = new Vector2(haloSize, haloSize);
            promptText.fontSize = Mathf.RoundToInt(16f * px);
            promptText.rectTransform.offsetMin = new Vector2(6f * px, 32f * px);
            promptText.rectTransform.offsetMax = new Vector2(-6f * px, -28f * px);
            promptSubLabel.fontSize = f14;
            promptSubLabel.rectTransform.offsetMin = new Vector2(6f * px, 14f * px);
            promptSubLabel.rectTransform.offsetMax = new Vector2(-6f * px, -60f * px);

            // Menu de Pausa cabendo integralmente no celular
            float pauseW = Mathf.Min(320f * px, usableW);
            float pauseH = 302f * px;
            pauseCardRect.sizeDelta = new Vector2(pauseW, pauseH);
            pauseTitleText.fontSize = f16;
            pauseTitleText.rectTransform.anchoredPosition = new Vector2(0f, -14f * px);
            pauseTitleText.rectTransform.sizeDelta = new Vector2(-20f * px, 28f * px);

            float pBtnW = pauseW - 36f * px;
            float pBtnH = 48f * px;
            float[] yPositions = { 68f, 12f, -44f, -100f };
            for (int i = 0; i < pauseButtonRects.Length; i++)
            {
                if (pauseButtonRects[i] != null)
                {
                    pauseButtonRects[i].anchoredPosition = new Vector2(0f, yPositions[i] * px);
                    pauseButtonRects[i].sizeDelta = new Vector2(pBtnW, pBtnH);
                }

                if (pauseButtonLabels[i] != null)
                {
                    pauseButtonLabels[i].fontSize = f15;
                }
            }
        }

        private void ApplyDesktopLayout()
        {
            counterPanelRect.anchoredPosition = new Vector2(28f, -24f);
            counterPanelRect.sizeDelta = new Vector2(390f, 76f);
            counterText.fontSize = 22;
            counterText.rectTransform.offsetMin = new Vector2(18f, 28f);
            counterText.rectTransform.offsetMax = new Vector2(-18f, -10f);
            progressBgRect.anchoredPosition = new Vector2(18f, 14f);
            progressBgRect.sizeDelta = new Vector2(-36f, 11f);

            topRightContainer.anchoredPosition = new Vector2(-28f, -24f);
            topRightContainer.sizeDelta = new Vector2(248f, 72f);
            muteBtnRect.anchoredPosition = new Vector2(-132f, 0f);
            muteBtnRect.sizeDelta = new Vector2(108f, 72f);
            pauseBtnRect.anchoredPosition = Vector2.zero;
            pauseBtnRect.sizeDelta = new Vector2(120f, 72f);
            muteButtonLabel.fontSize = 24;
            pauseButtonLabel.fontSize = 24;

            // Restore the desktop action geometry and typography after a viewport rotation.
            promptRect.anchoredPosition = new Vector2(-60f, 52f);
            promptRect.sizeDelta = new Vector2(196f, 196f);
            promptHaloRect.sizeDelta = new Vector2(236f, 236f);
            promptText.fontSize = 26;
            promptText.rectTransform.offsetMin = new Vector2(14f, 66f);
            promptText.rectTransform.offsetMax = new Vector2(-14f, -36f);
            promptSubLabel.fontSize = 20;
            promptSubLabel.rectTransform.offsetMin = new Vector2(14f, 36f);
            promptSubLabel.rectTransform.offsetMax = new Vector2(-14f, -128f);
            joystickRect.anchoredPosition = new Vector2(168f, 158f);
            joystickRect.sizeDelta = new Vector2(224f, 224f);
            joystickHandleRect.sizeDelta = new Vector2(92f, 92f);
            joystickComponent.Configure(joystickRect, joystickHandleRect, 72f, 0.1f);

            // Desktop: place DialogueBox & PoeticNarration in the top-center safe band between Counter and Pause/Mute
            // so they stay crisp and legible without ever covering Yuna or the room art.
            dialogueRect.anchorMin = new Vector2(0.5f, 1f);
            dialogueRect.anchorMax = new Vector2(0.5f, 1f);
            dialogueRect.pivot = new Vector2(0.5f, 1f);
            dialogueRect.anchoredPosition = new Vector2(0f, -18f);
            dialogueRect.sizeDelta = new Vector2(560f, 118f);
            dialogueInnerRect.sizeDelta = new Vector2(-12f, -12f);
            portraitHaloRect.anchoredPosition = new Vector2(54f, 0f);
            portraitHaloRect.sizeDelta = new Vector2(80f, 80f);
            portraitRingRect.anchoredPosition = new Vector2(54f, 0f);
            portraitRingRect.sizeDelta = new Vector2(68f, 68f);
            portraitInitialText.fontSize = 28;
            speakerText.fontSize = 18;
            speakerText.rectTransform.anchoredPosition = new Vector2(104f, -12f);
            speakerText.rectTransform.sizeDelta = new Vector2(-124f, 26f);
            dialogueBodyText.fontSize = 21;
            dialogueBodyText.rectTransform.offsetMin = new Vector2(104f, 12f);
            dialogueBodyText.rectTransform.offsetMax = new Vector2(-36f, -38f);
            dialogueHintText.enabled = false;
            dialogueHintText.text = string.Empty;
            HudTheme.EnsureCornerTapCue(dialogueRoot.transform, 22f, 10f, 8f, new Color(HudTheme.Plum.r, HudTheme.Plum.g, HudTheme.Plum.b, 0.78f));

            poeticRect.anchorMin = new Vector2(0.5f, 1f);
            poeticRect.anchorMax = new Vector2(0.5f, 1f);
            poeticRect.pivot = new Vector2(0.5f, 1f);
            poeticRect.anchoredPosition = new Vector2(0f, -18f);
            poeticRect.sizeDelta = new Vector2(560f, 112f);
            poeticInnerRect.sizeDelta = new Vector2(-12f, -12f);
            flowerLeftRect.anchoredPosition = new Vector2(16f, -16f);
            flowerLeftRect.localScale = Vector3.one * 0.85f;
            flowerRightRect.anchoredPosition = new Vector2(-16f, -16f);
            flowerRightRect.localScale = Vector3.one * 0.85f;
            poeticText.fontSize = 26;
            poeticText.rectTransform.offsetMin = new Vector2(28f, 12f);
            poeticText.rectTransform.offsetMax = new Vector2(-36f, -12f);
            poeticHintText.enabled = false;
            poeticHintText.text = string.Empty;
            HudTheme.EnsureCornerTapCue(poeticRoot.transform, 20f, 10f, 8f, new Color(HudTheme.Plum.r, HudTheme.Plum.g, HudTheme.Plum.b, 0.68f));
        }

        private float ComputePxToCanvas(Vector2 canvasSize)
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

        private bool IsMobileViewport()
        {
            float w = forcedLogicalWidth > 0f ? forcedLogicalWidth : Screen.width;
            float h = forcedLogicalHeight > 0f ? forcedLogicalHeight : Screen.height;
            return Application.isMobilePlatform || Touchscreen.current != null || w <= 960f || h <= 540f;
        }

        private void BuildCounterPanel(Transform parent, Font font)
        {
            var panel = CreateBorderedPanel(
                "CounterPanel",
                parent,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -24f),
                new Vector2(390f, 76f),
                new Color(0.102f, 0.063f, 0.149f, 0.86f),
                new Color(0.980f, 0.800f, 0.082f, 1f),
                2f);
            counterPanelRect = panel.GetComponent<RectTransform>();
            counterPanelRect.pivot = new Vector2(0f, 1f);

            counterText = CreateLabel("CounterText", panel.transform, font, 22, TextAnchor.UpperLeft, HudTheme.Ink);
            counterText.fontStyle = FontStyle.Bold;
            var rt = counterText.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(18f, 28f);
            rt.offsetMax = new Vector2(-18f, -10f);

            var barBg = CreatePanel("ProgressBg", panel.transform, Vector2.zero, new Vector2(1f, 0f), new Vector2(18f, 14f), new Vector2(-36f, 12f), new Color(1f, 1f, 1f, 0.28f));
            progressBgRect = barBg.GetComponent<RectTransform>();
            progressBgRect.pivot = new Vector2(0f, 0f);
            progressBgRect.anchorMin = new Vector2(0f, 0f);
            progressBgRect.anchorMax = new Vector2(1f, 0f);
            progressBgRect.anchoredPosition = new Vector2(18f, 14f);
            progressBgRect.sizeDelta = new Vector2(-36f, 11f);

            var fillGo = new GameObject("ProgressFill");
            fillGo.transform.SetParent(barBg.transform, false);
            progressFill = fillGo.AddComponent<Image>();
            progressFill.sprite = CreateSolidWhiteSprite();
            progressFill.color = new Color(0.063f, 0.725f, 0.506f, 1f);
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            StretchFull(progressFill.rectTransform);
        }

        private void BuildTopRightControls(Transform parent, Font font)
        {
            var container = new GameObject("TopRightControls");
            container.transform.SetParent(parent, false);
            topRightContainer = container.AddComponent<RectTransform>();
            topRightContainer.anchorMin = new Vector2(1f, 1f);
            topRightContainer.anchorMax = new Vector2(1f, 1f);
            topRightContainer.pivot = new Vector2(1f, 1f);
            topRightContainer.anchoredPosition = new Vector2(-28f, -24f);
            topRightContainer.sizeDelta = new Vector2(220f, 46f);

            var muteBtn = CreateHudButton("MuteButton", container.transform, font, "Som", new Vector2(-116f, 0f), new Vector2(96f, 44f), () => OnMuteToggled?.Invoke());
            muteBtnRect = muteBtn.GetComponent<RectTransform>();
            muteButtonLabel = muteBtn.GetComponentInChildren<Text>();

            var pauseBtn = CreateHudButton("PauseButton", container.transform, font, "Pausa", new Vector2(0f, 0f), new Vector2(104f, 44f), () =>
            {
                OnPauseToggled?.Invoke(!isPaused);
            });
            pauseBtnRect = pauseBtn.GetComponent<RectTransform>();
            pauseButtonLabel = pauseBtn.GetComponentInChildren<Text>();
        }

        private void BuildActionPrompt(Transform parent, Font font)
        {
            promptRoot = new GameObject("ActionPrompt");
            promptRoot.transform.SetParent(parent, false);
            promptRect = promptRoot.AddComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(1f, 0f);
            promptRect.anchorMax = new Vector2(1f, 0f);
            promptRect.pivot = new Vector2(1f, 0f);
            promptRect.anchoredPosition = new Vector2(-60f, 52f);
            promptRect.sizeDelta = new Vector2(196f, 196f);

            var haloGo = new GameObject("ActionDashedHalo");
            haloGo.transform.SetParent(promptRoot.transform, false);
            promptHaloRect = haloGo.AddComponent<RectTransform>();
            promptHaloRect.anchorMin = new Vector2(0.5f, 0.5f);
            promptHaloRect.anchorMax = new Vector2(0.5f, 0.5f);
            promptHaloRect.pivot = new Vector2(0.5f, 0.5f);
            promptHaloRect.sizeDelta = new Vector2(236f, 236f);
            var haloImg = haloGo.AddComponent<Image>();
            haloImg.sprite = ToyRoomVirtualJoystick.CreateDashedRingSprite(256, 26);
            haloImg.color = new Color(1f, 0.96f, 0.82f, 0.42f);
            haloImg.raycastTarget = false;

            promptBackground = promptRoot.AddComponent<Image>();
            promptBackground.sprite = ToyRoomVirtualJoystick.CreateGoldenOrbSprite(256, 0.048f);
            promptBackground.color = Color.white;
            promptBackground.raycastTarget = true;
            var dropShadow = promptRoot.AddComponent<Shadow>();
            dropShadow.effectColor = new Color(0.05f, 0.02f, 0.01f, 0.55f);
            dropShadow.effectDistance = new Vector2(0f, -6f);

            promptButton = promptRoot.AddComponent<Button>();
            promptButton.targetGraphic = promptBackground;
            promptButton.onClick.AddListener(() => OnActionRequested?.Invoke());

            promptText = CreateLabel("PromptText", promptRoot.transform, font, 30, TextAnchor.MiddleCenter, new Color(0.12f, 0.07f, 0.02f, 1f));
            promptText.fontStyle = FontStyle.Bold;
            promptText.horizontalOverflow = HorizontalWrapMode.Overflow;
            promptText.raycastTarget = false;
            var textRt = promptText.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(16f, 44f);
            textRt.offsetMax = new Vector2(-16f, -18f);

            promptSubLabel = CreateLabel("PromptSubLabel", promptRoot.transform, font, 18, TextAnchor.LowerCenter, new Color(0.20f, 0.12f, 0.04f, 0.92f));
            promptSubLabel.fontStyle = FontStyle.Bold;
            promptSubLabel.text = "TOQUE";
            promptSubLabel.raycastTarget = false;
            var subRt = promptSubLabel.rectTransform;
            subRt.anchorMin = Vector2.zero;
            subRt.anchorMax = Vector2.one;
            subRt.offsetMin = new Vector2(16f, 34f);
            subRt.offsetMax = new Vector2(-16f, -96f);
        }

        private void BuildVirtualJoystick(Transform parent)
        {
            joystickRoot = new GameObject("VirtualJoystick");
            joystickRoot.transform.SetParent(parent, false);
            joystickRect = joystickRoot.AddComponent<RectTransform>();
            joystickRect.anchorMin = new Vector2(0f, 0f);
            joystickRect.anchorMax = new Vector2(0f, 0f);
            joystickRect.pivot = new Vector2(0.5f, 0.5f);
            joystickRect.anchoredPosition = new Vector2(168f, 158f);
            joystickRect.sizeDelta = new Vector2(224f, 224f);

            var baseImg = joystickRoot.AddComponent<Image>();
            baseImg.sprite = ToyRoomVirtualJoystick.CreateLegacyJoystickBaseSprite(256);
            baseImg.color = Color.white;
            baseImg.raycastTarget = true;

            var handleGo = new GameObject("JoystickHandle");
            handleGo.transform.SetParent(joystickRoot.transform, false);
            joystickHandleRect = handleGo.AddComponent<RectTransform>();
            joystickHandleRect.anchorMin = new Vector2(0.5f, 0.5f);
            joystickHandleRect.anchorMax = new Vector2(0.5f, 0.5f);
            joystickHandleRect.pivot = new Vector2(0.5f, 0.5f);
            joystickHandleRect.anchoredPosition = Vector2.zero;
            joystickHandleRect.sizeDelta = new Vector2(92f, 92f);
            var handleImg = handleGo.AddComponent<Image>();
            handleImg.sprite = ToyRoomVirtualJoystick.CreateGoldenOrbSprite(128, 0.075f);
            handleImg.color = Color.white;
            handleImg.raycastTarget = false;
            var handleShadow = handleGo.AddComponent<Shadow>();
            handleShadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            handleShadow.effectDistance = new Vector2(0f, -4f);

            joystickComponent = joystickRoot.AddComponent<ToyRoomVirtualJoystick>();
            joystickComponent.Configure(joystickRect, joystickHandleRect, 70f, 0.1f);
            joystickComponent.OnValueChanged += value => OnMoveInputChanged?.Invoke(value);
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private void BuildDialogueBox(Transform parent, Font font)
        {
            dialogueRoot = CreateBorderedPanel(
                "DialogueBox",
                parent,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 38f),
                new Vector2(980f, 174f),
                new Color(0.086f, 0.063f, 0.141f, 0.94f),
                new Color(0.992f, 0.878f, 0.278f, 0.95f),
                2.5f);
            dialogueRect = dialogueRoot.GetComponent<RectTransform>();
            dialogueRect.pivot = new Vector2(0.5f, 0f);

            var innerFrame = CreateBorderedPanel(
                "InnerFrame",
                dialogueRoot.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-14f, -14f),
                new Color(0f, 0f, 0f, 0f),
                new Color(1f, 1f, 1f, 0.28f),
                1f);
            dialogueInnerRect = innerFrame.GetComponent<RectTransform>();
            dialogueInnerRect.pivot = new Vector2(0.5f, 0.5f);
            innerFrame.GetComponent<Image>().raycastTarget = false;

            var circleFill = ToyRoomVirtualJoystick.CreateCircleSprite(128, 0f, 0.96f);
            var circleRing = ToyRoomVirtualJoystick.CreateCircleSprite(128, 0.80f, 0.98f);

            var haloGo = CreatePanel("PortraitHalo", dialogueRoot.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(72f, 6f), new Vector2(106f, 106f), new Color(0.02f, 0.71f, 0.83f, 0.35f));
            portraitHaloRect = haloGo.GetComponent<RectTransform>();
            portraitHaloRect.pivot = new Vector2(0.5f, 0.5f);
            portraitHalo = haloGo.GetComponent<Image>();
            portraitHalo.sprite = circleFill;
            portraitHalo.raycastTarget = false;

            var discGo = CreatePanel("PortraitDisc", dialogueRoot.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(72f, 6f), new Vector2(92f, 92f), new Color(0.086f, 0.063f, 0.141f, 1f));
            portraitRingRect = discGo.GetComponent<RectTransform>();
            portraitRingRect.pivot = new Vector2(0.5f, 0.5f);
            var discImg = discGo.GetComponent<Image>();
            discImg.sprite = circleFill;
            discImg.raycastTarget = false;

            var ringOverlay = CreatePanel("PortraitRingOverlay", discGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.992f, 0.878f, 0.278f, 1f));
            StretchFull(ringOverlay.GetComponent<RectTransform>());
            portraitRing = ringOverlay.GetComponent<Image>();
            portraitRing.sprite = circleRing;
            portraitRing.raycastTarget = false;

            portraitInitialText = CreateLabel("PortraitInitial", discGo.transform, font, 36, TextAnchor.MiddleCenter, new Color(0.99f, 0.88f, 0.28f));
            portraitInitialText.fontStyle = FontStyle.Bold;
            StretchFull(portraitInitialText.rectTransform);

            speakerText = CreateLabel("SpeakerText", dialogueRoot.transform, font, 22, TextAnchor.UpperLeft, new Color(0.22f, 0.74f, 0.97f));
            speakerText.fontStyle = FontStyle.Bold;
            var sRt = speakerText.rectTransform;
            sRt.anchorMin = new Vector2(0f, 1f);
            sRt.anchorMax = new Vector2(1f, 1f);
            sRt.pivot = new Vector2(0f, 1f);
            sRt.anchoredPosition = new Vector2(138f, -18f);
            sRt.sizeDelta = new Vector2(-170f, 34f);

            dialogueBodyText = CreateLabel("DialogueBody", dialogueRoot.transform, font, 26, TextAnchor.MiddleLeft, HudTheme.Ink);
            dialogueBodyText.fontStyle = FontStyle.Italic;
            var bRt = dialogueBodyText.rectTransform;
            bRt.anchorMin = Vector2.zero;
            bRt.anchorMax = Vector2.one;
            bRt.offsetMin = new Vector2(138f, 34f);
            bRt.offsetMax = new Vector2(-32f, -50f);

            dialogueHintText = CreateLabel("DialogueHint", dialogueRoot.transform, font, 16, TextAnchor.LowerRight, HudTheme.Plum);
            dialogueHintText.text = string.Empty;
            dialogueHintText.enabled = false;
            HudTheme.EnsureCornerTapCue(dialogueRoot.transform, 22f, 10f, 8f, new Color(HudTheme.Plum.r, HudTheme.Plum.g, HudTheme.Plum.b, 0.78f));
            var hRt = dialogueHintText.rectTransform;
            hRt.anchorMin = Vector2.zero;
            hRt.anchorMax = Vector2.one;
            hRt.offsetMin = new Vector2(138f, 12f);
            hRt.offsetMax = new Vector2(-24f, -12f);

            var clickBtn = dialogueRoot.AddComponent<Button>();
            clickBtn.transition = Selectable.Transition.None;
            clickBtn.onClick.AddListener(() => OnAdvanceRequested?.Invoke());
        }

        private void BuildPoeticOverlay(Transform parent, Font narrativeFont, Font uiFont)
        {
            poeticRoot = CreateBorderedPanel(
                "PoeticNarration",
                parent,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -34f),
                new Vector2(780f, 168f),
                new Color(1.0f, 0.961f, 0.898f, 0.97f),
                new Color(0.839f, 0.682f, 0.694f, 1f),
                3f);
            poeticRect = poeticRoot.GetComponent<RectTransform>();
            poeticRect.pivot = new Vector2(0.5f, 1f);

            var inner = CreateBorderedPanel(
                "PoeticInnerBorder",
                poeticRoot.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-16f, -16f),
                new Color(0f, 0f, 0f, 0f),
                new Color(0.722f, 0.788f, 0.733f, 1f),
                1.5f);
            poeticInnerRect = inner.GetComponent<RectTransform>();
            poeticInnerRect.pivot = new Vector2(0.5f, 0.5f);
            inner.GetComponent<Image>().raycastTarget = false;

            flowerLeftRect = BuildCornerFlower(poeticRoot.transform, new Vector2(0f, 1f), new Vector2(18f, -18f));
            flowerRightRect = BuildCornerFlower(poeticRoot.transform, new Vector2(1f, 1f), new Vector2(-18f, -18f));

            poeticText = CreateLabel("PoeticText", poeticRoot.transform, narrativeFont, 32, TextAnchor.MiddleCenter, new Color(0.224f, 0.141f, 0.133f, 1f));
            var tRt = poeticText.rectTransform;
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(36f, 28f);
            tRt.offsetMax = new Vector2(-36f, -20f);

            poeticHintText = CreateLabel("PoeticHint", poeticRoot.transform, uiFont, 14, TextAnchor.LowerCenter, new Color(0.518f, 0.392f, 0.404f, 0.78f));
            poeticHintText.text = string.Empty;
            poeticHintText.enabled = false;
            HudTheme.EnsureCornerTapCue(poeticRoot.transform, 20f, 10f, 8f, new Color(HudTheme.Plum.r, HudTheme.Plum.g, HudTheme.Plum.b, 0.68f));
            var hRt = poeticHintText.rectTransform;
            hRt.anchorMin = Vector2.zero;
            hRt.anchorMax = Vector2.one;
            hRt.offsetMin = new Vector2(20f, 10f);
            hRt.offsetMax = new Vector2(-20f, -10f);

            var clickBtn = poeticRoot.AddComponent<Button>();
            clickBtn.transition = Selectable.Transition.None;
            clickBtn.onClick.AddListener(() => OnAdvanceRequested?.Invoke());
        }

        private void BuildPauseMenu(Transform parent, Font font)
        {
            pauseMenuRoot = CreatePanel("PauseOverlay", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.05f, 0.03f, 0.09f, 0.72f));
            StretchFull(pauseMenuRoot.GetComponent<RectTransform>());

            var card = CreateBorderedPanel(
                "PauseCard",
                pauseMenuRoot.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(420f, 360f),
                new Color(0.102f, 0.063f, 0.149f, 0.96f),
                new Color(0.980f, 0.800f, 0.082f, 1f),
                2.5f);
            pauseCardRect = card.GetComponent<RectTransform>();
            pauseCardRect.pivot = new Vector2(0.5f, 0.5f);

            pauseTitleText = CreateLabel("PauseTitle", card.transform, font, 30, TextAnchor.UpperCenter, HudTheme.Ink);
            pauseTitleText.fontStyle = FontStyle.Bold;
            pauseTitleText.text = "Quarto de Brinquedos";
            var titleRt = pauseTitleText.rectTransform;
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -26f);
            titleRt.sizeDelta = new Vector2(-32f, 44f);

            var b0 = CreateMenuButton(card.transform, font, "Continuar", new Vector2(0f, 48f), () => OnPauseToggled?.Invoke(false));
            var b1 = CreateMenuButton(card.transform, font, "Som: Ligado", new Vector2(0f, -14f), () => OnMuteToggled?.Invoke());
            var b2 = CreateMenuButton(card.transform, font, "Reiniciar Fase", new Vector2(0f, -76f), () => OnRestartRequested?.Invoke());
            var b3 = CreateMenuButton(card.transform, font, "Voltar ao Título", new Vector2(0f, -138f), () => OnReturnToTitleRequested?.Invoke());

            Button[] buttons = { b0, b1, b2, b3 };
            for (int i = 0; i < buttons.Length; i++)
            {
                pauseButtonRects[i] = buttons[i].GetComponent<RectTransform>();
                pauseButtonLabels[i] = buttons[i].GetComponentInChildren<Text>();
            }

            pauseMuteButtonLabel = pauseButtonLabels[1];
        }

        private static RectTransform BuildCornerFlower(Transform parent, Vector2 anchor, Vector2 offset)
        {
            var flower = new GameObject("CornerFlower");
            flower.transform.SetParent(parent, false);
            var rt = flower.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(16f, 16f);

            var petalColor = new Color(0.898f, 0.745f, 0.765f, 1f);
            Vector2[] offsets =
            {
                new(0f, 4.5f),
                new(0f, -4.5f),
                new(-4.5f, 0f),
                new(4.5f, 0f)
            };
            for (int i = 0; i < offsets.Length; i++)
            {
                var p = CreatePanel($"Petal_{i}", flower.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), offsets[i], new Vector2(6f, 6f), petalColor);
                p.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            }

            var center = CreatePanel("Center", flower.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(5f, 5f), new Color(0.776f, 0.682f, 0.475f, 1f));
            center.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            return rt;
        }

        private static Button CreateHudButton(string name, Transform parent, Font font, string label, Vector2 anchoredPos, Vector2 size, Action onClick)
        {
            var panel = CreateBorderedPanel(
                name,
                parent,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                anchoredPos,
                size,
                new Color(0.102f, 0.063f, 0.149f, 0.86f),
                new Color(0.980f, 0.800f, 0.082f, 0.9f),
                1.5f);
            panel.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);

            var txt = CreateLabel("Label", panel.transform, font, 18, TextAnchor.MiddleCenter, new Color(0.992f, 0.878f, 0.278f, 1f));
            txt.fontStyle = FontStyle.Bold;
            txt.text = label;
            StretchFull(txt.rectTransform);

            var btn = panel.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());
            HudTheme.Button(btn);
            txt.color = Color.white;
            return btn;
        }

        private static Button CreateMenuButton(Transform parent, Font font, string label, Vector2 anchoredPos, Action onClick)
        {
            var panel = CreateBorderedPanel(
                $"Btn_{label}",
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                anchoredPos,
                new Vector2(280f, 48f),
                new Color(0.18f, 0.12f, 0.26f, 0.95f),
                new Color(0.980f, 0.800f, 0.082f, 0.9f),
                1.5f);
            panel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            var txt = CreateLabel("Label", panel.transform, font, 20, TextAnchor.MiddleCenter, new Color(0.996f, 0.980f, 0.880f, 1f));
            txt.fontStyle = FontStyle.Bold;
            txt.text = label;
            StretchFull(txt.rectTransform);

            var btn = panel.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());
            HudTheme.Button(btn);
            txt.color = Color.white;
            return btn;
        }

        private static GameObject CreateBorderedPanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pos,
            Vector2 size,
            Color bgColor,
            Color borderColor,
            float borderWidth)
        {
            var go = CreatePanel(name, parent, anchorMin, anchorMax, pos, size, bgColor);
            var image = go.GetComponent<Image>();
            HudTheme.Round(image);
            if (name == "CounterPanel" || name == "DialogueBox" || name == "PauseCard")
            {
                image.color = HudTheme.Paper;
            }
            else if (bgColor.a > 0f && bgColor.maxColorComponent < 0.3f)
            {
                image.color = HudTheme.Plum;
            }
            var outline = go.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(borderWidth, -borderWidth);
            return go;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = color;
            if (name != "PauseOverlay" && !name.StartsWith("Portrait"))
            {
                HudTheme.Round(img);
            }
            return go;
        }

        private static Text CreateLabel(string name, Transform parent, Font font, int size, TextAnchor align, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var txt = go.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = size;
            txt.alignment = align;
            txt.color = color;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Sprite CreateSolidWhiteSprite()
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var cols = new Color32[] { Color.white, Color.white, Color.white, Color.white };
            tex.SetPixels32(cols);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
