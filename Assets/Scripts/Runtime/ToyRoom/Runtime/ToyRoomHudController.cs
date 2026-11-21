using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Controla a interface da Toy Room seguindo a assinatura visual do acervo legado:
    /// - Card de progresso ameixa/dourado com barra esmeralda (Brinquedos Arrumados: X/8)
    /// - Botao dourado contextual de acao (PEGAR / GUARDAR / PEGAR ESPADA)
    /// - Caixa vitoriana com retrato circular, aro dourado/rosa e dica discreta de leitura
    /// - Quadro poetico em pergaminho (#fff5e5) com moldura dupla rosa/salvia e flores nos cantos
    /// - Menu discreto de pausa e audio (ESC / botoes no topo direito)
    /// </summary>
    public sealed class ToyRoomHudController : MonoBehaviour
    {
        [SerializeField] private Font poeticFont;

        private Text counterText;
        private Image progressFill;
        private GameObject promptRoot;
        private Image promptBackground;
        private Button promptButton;
        private Text promptText;
        private GameObject joystickRoot;
        private GameObject dialogueRoot;
        private Image portraitRing;
        private Image portraitHalo;
        private Text portraitInitialText;
        private Text speakerText;
        private Text dialogueBodyText;
        private GameObject poeticRoot;
        private Text poeticText;
        private GameObject pauseMenuRoot;
        private Text muteButtonLabel;
        private Text pauseMuteButtonLabel;

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

        private void Awake()
        {
            EnsureBuilt();
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
            bool showJoystick = visible && IsMobileOrTouchDevice();
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
                    ? new Color(0.22f, 0.74f, 0.97f, 1f)
                    : new Color(0.99f, 0.84f, 0.67f, 1f);
                portraitRing.color = isNanda
                    ? new Color(0.99f, 0.88f, 0.28f, 1f)
                    : new Color(0.96f, 0.45f, 0.71f, 1f);
                portraitHalo.color = isNanda
                    ? new Color(0.02f, 0.71f, 0.83f, 0.35f)
                    : new Color(0.96f, 0.45f, 0.71f, 0.35f);
                portraitInitialText.text = isNanda ? "N" : "Y";
                portraitInitialText.color = speakerText.color;
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
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.pivot = new Vector2(0f, 1f);

            counterText = CreateLabel("CounterText", panel.transform, font, 22, TextAnchor.UpperLeft, new Color(0.992f, 0.878f, 0.278f, 1f));
            counterText.fontStyle = FontStyle.Bold;
            var rt = counterText.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(18f, 28f);
            rt.offsetMax = new Vector2(-18f, -10f);

            var barBg = CreatePanel("ProgressBg", panel.transform, Vector2.zero, new Vector2(1f, 0f), new Vector2(18f, 14f), new Vector2(-36f, 12f), new Color(1f, 1f, 1f, 0.15f));
            var barBgRt = barBg.GetComponent<RectTransform>();
            barBgRt.pivot = new Vector2(0f, 0f);
            barBgRt.anchorMin = new Vector2(0f, 0f);
            barBgRt.anchorMax = new Vector2(1f, 0f);
            barBgRt.anchoredPosition = new Vector2(18f, 14f);
            barBgRt.sizeDelta = new Vector2(-36f, 11f);

            var fillGo = new GameObject("ProgressFill");
            fillGo.transform.SetParent(barBg.transform, false);
            progressFill = fillGo.AddComponent<Image>();
            progressFill.color = new Color(0.063f, 0.725f, 0.506f, 1f);
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            var fillRt = progressFill.rectTransform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
        }

        private void BuildTopRightControls(Transform parent, Font font)
        {
            var container = new GameObject("TopRightControls");
            container.transform.SetParent(parent, false);
            var rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-28f, -24f);
            rt.sizeDelta = new Vector2(220f, 46f);

            var muteBtn = CreateHudButton("MuteButton", container.transform, font, "Som", new Vector2(-116f, 0f), new Vector2(96f, 44f), () => OnMuteToggled?.Invoke());
            muteButtonLabel = muteBtn.GetComponentInChildren<Text>();

            CreateHudButton("PauseButton", container.transform, font, "Pausa", new Vector2(0f, 0f), new Vector2(104f, 44f), () =>
            {
                OnPauseToggled?.Invoke(!isPaused);
            });
        }

        private void BuildActionPrompt(Transform parent, Font font)
        {
            // Botao redondo dourado de altissima visibilidade (fiel ao legado):
            // gradiente dourado-ambar, borda branca solida, sombra suave, halo tracejado e rotulo PEGAR/GUARDAR + TOQUE.
            promptRoot = new GameObject("ActionPrompt");
            promptRoot.transform.SetParent(parent, false);
            var rt = promptRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-60f, 52f);
            rt.sizeDelta = new Vector2(196f, 196f);

            var haloGo = new GameObject("ActionDashedHalo");
            haloGo.transform.SetParent(promptRoot.transform, false);
            var haloRt = haloGo.AddComponent<RectTransform>();
            haloRt.anchorMin = new Vector2(0.5f, 0.5f);
            haloRt.anchorMax = new Vector2(0.5f, 0.5f);
            haloRt.pivot = new Vector2(0.5f, 0.5f);
            haloRt.sizeDelta = new Vector2(236f, 236f);
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
            promptText.raycastTarget = false;
            var textRt = promptText.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(16f, 44f);
            textRt.offsetMax = new Vector2(-16f, -18f);

            var subLabel = CreateLabel("PromptSubLabel", promptRoot.transform, font, 18, TextAnchor.LowerCenter, new Color(0.20f, 0.12f, 0.04f, 0.92f));
            subLabel.fontStyle = FontStyle.Bold;
            subLabel.text = "TOQUE";
            subLabel.raycastTarget = false;
            var subRt = subLabel.rectTransform;
            subRt.anchorMin = Vector2.zero;
            subRt.anchorMax = Vector2.one;
            subRt.offsetMin = new Vector2(16f, 34f);
            subRt.offsetMax = new Vector2(-16f, -96f);
        }

        private void BuildVirtualJoystick(Transform parent)
        {
            // Analogico virtual fiel ao legado: anel dourado fino com 4 marcas cardinais + orbe dourado com borda branca
            joystickRoot = new GameObject("VirtualJoystick");
            joystickRoot.transform.SetParent(parent, false);
            var rootRt = joystickRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0f, 0f);
            rootRt.anchorMax = new Vector2(0f, 0f);
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.anchoredPosition = new Vector2(168f, 158f);
            rootRt.sizeDelta = new Vector2(224f, 224f);

            var baseImg = joystickRoot.AddComponent<Image>();
            baseImg.sprite = ToyRoomVirtualJoystick.CreateLegacyJoystickBaseSprite(256);
            baseImg.color = Color.white;
            baseImg.raycastTarget = true;

            var handleGo = new GameObject("JoystickHandle");
            handleGo.transform.SetParent(joystickRoot.transform, false);
            var handleRt = handleGo.AddComponent<RectTransform>();
            handleRt.anchorMin = new Vector2(0.5f, 0.5f);
            handleRt.anchorMax = new Vector2(0.5f, 0.5f);
            handleRt.pivot = new Vector2(0.5f, 0.5f);
            handleRt.anchoredPosition = Vector2.zero;
            handleRt.sizeDelta = new Vector2(92f, 92f);
            var handleImg = handleGo.AddComponent<Image>();
            handleImg.sprite = ToyRoomVirtualJoystick.CreateGoldenOrbSprite(128, 0.075f);
            handleImg.color = Color.white;
            handleImg.raycastTarget = false;
            var handleShadow = handleGo.AddComponent<Shadow>();
            handleShadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            handleShadow.effectDistance = new Vector2(0f, -4f);

            var joystick = joystickRoot.AddComponent<ToyRoomVirtualJoystick>();
            joystick.Configure(rootRt, handleRt, 70f, 0.1f);
            joystick.OnValueChanged += value => OnMoveInputChanged?.Invoke(value);
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
            var rt = dialogueRoot.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0f);

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
            var innerRt = innerFrame.GetComponent<RectTransform>();
            innerRt.pivot = new Vector2(0.5f, 0.5f);

            // Retrato circular vitoriano a esquerda (halo + disco escuro + anel)
            var haloGo = CreatePanel("PortraitHalo", dialogueRoot.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(72f, 6f), new Vector2(106f, 106f), new Color(0.02f, 0.71f, 0.83f, 0.35f));
            haloGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            portraitHalo = haloGo.GetComponent<Image>();

            var ringGo = CreateBorderedPanel(
                "PortraitDisc",
                dialogueRoot.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(72f, 6f),
                new Vector2(92f, 92f),
                new Color(0.086f, 0.063f, 0.141f, 1f),
                new Color(0.992f, 0.878f, 0.278f, 1f),
                2.5f);
            ringGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            portraitRing = ringGo.GetComponent<Image>();

            portraitInitialText = CreateLabel("PortraitInitial", ringGo.transform, font, 36, TextAnchor.MiddleCenter, new Color(0.99f, 0.88f, 0.28f));
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

            dialogueBodyText = CreateLabel("DialogueBody", dialogueRoot.transform, font, 26, TextAnchor.MiddleLeft, new Color(0.996f, 0.988f, 0.910f));
            dialogueBodyText.fontStyle = FontStyle.Italic;
            var bRt = dialogueBodyText.rectTransform;
            bRt.anchorMin = Vector2.zero;
            bRt.anchorMax = Vector2.one;
            bRt.offsetMin = new Vector2(138f, 34f);
            bRt.offsetMax = new Vector2(-32f, -50f);

            var hintText = CreateLabel("DialogueHint", dialogueRoot.transform, font, 15, TextAnchor.LowerRight, new Color(0.90f, 0.86f, 0.72f, 0.75f));
            hintText.text = IsMobileOrTouchDevice() ? "Toque na tela para avançar" : "Clique ou Espaço para avançar";
            var hRt = hintText.rectTransform;
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
            var rt = poeticRoot.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 1f);

            // Moldura interna salvia (#b8c9bb)
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
            inner.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            // Pequenas flores rosa/dourado nos cantos superiores
            BuildCornerFlower(poeticRoot.transform, new Vector2(0f, 1f), new Vector2(18f, -18f));
            BuildCornerFlower(poeticRoot.transform, new Vector2(1f, 1f), new Vector2(-18f, -18f));

            poeticText = CreateLabel("PoeticText", poeticRoot.transform, narrativeFont, 32, TextAnchor.MiddleCenter, new Color(0.224f, 0.141f, 0.133f, 1f));
            var tRt = poeticText.rectTransform;
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(36f, 28f);
            tRt.offsetMax = new Vector2(-36f, -20f);

            var hintText = CreateLabel("PoeticHint", poeticRoot.transform, uiFont, 14, TextAnchor.LowerCenter, new Color(0.518f, 0.392f, 0.404f, 0.78f));
            hintText.text = IsMobileOrTouchDevice() ? "Toque na tela para avançar" : "Avança automaticamente · clique para adiantar";
            var hRt = hintText.rectTransform;
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
            card.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

            var title = CreateLabel("PauseTitle", card.transform, font, 30, TextAnchor.UpperCenter, new Color(0.992f, 0.878f, 0.278f, 1f));
            title.fontStyle = FontStyle.Bold;
            title.text = "Quarto de Brinquedos";
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -26f);
            titleRt.sizeDelta = new Vector2(-32f, 44f);

            CreateMenuButton(card.transform, font, "Continuar", new Vector2(0f, 48f), () => OnPauseToggled?.Invoke(false));
            var muteBtn = CreateMenuButton(card.transform, font, "Som: Ligado", new Vector2(0f, -14f), () => OnMuteToggled?.Invoke());
            pauseMuteButtonLabel = muteBtn.GetComponentInChildren<Text>();
            CreateMenuButton(card.transform, font, "Reiniciar Fase", new Vector2(0f, -76f), () => OnRestartRequested?.Invoke());
            CreateMenuButton(card.transform, font, "Voltar ao Titulo", new Vector2(0f, -138f), () => OnReturnToTitleRequested?.Invoke());
        }

        private static void BuildCornerFlower(Transform parent, Vector2 anchor, Vector2 offset)
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
    }
}
