using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Interface uGUI da Fase 4: barras de cinema na entrada, coracoes rosa no topo,
    /// botoes no canto superior direito, dica de desvio, badge de pausa, banner de queda e modais.
    /// </summary>
    public sealed class BearChaseHudController : MonoBehaviour
    {
        [SerializeField] private Font uiFont;

        private Canvas canvas;
        private RectTransform canvasRect;
        private RectTransform topBar;
        private RectTransform bottomBar;
        private Image fadeOverlay;
        private RectTransform heartsRoot;
        private readonly List<Image> heartImages = new List<Image>();
        private Sprite heartSprite;
        private Texture2D heartTexture;

        private RectTransform controlsRoot;
        private RectTransform homeBtnRect;
        private Text homeLabel;
        private RectTransform pauseBtnRect;
        private RectTransform muteBtnRect;

        private GameObject hintRoot;
        private RectTransform hintPanelRect;
        private Text hintText;
        private GameObject pauseBadgeRoot;
        private RectTransform pauseBadgeRect;
        private Text pauseBadgeText;
        private GameObject fallenBannerRoot;
        private RectTransform fallenBannerRect;
        private Text fallenBannerText;
        private GameObject modalRoot;
        private RectTransform modalBoxRect;
        private RectTransform modalTitleRect;
        private RectTransform modalBodyRect;
        private RectTransform modalButtonsRowRect;
        private Text modalTitle;
        private Text modalBody;
        private Button primaryButton;
        private Text primaryButtonLabel;
        private Button easyButton;
        private Text easyButtonLabel;
        private Button titleButton;
        private Text titleButtonLabel;
        private Button pauseButton;
        private Text pauseLabel;
        private Button muteButton;
        private Text muteLabel;

        private float forcedLogicalWidth = -1f;
        private float forcedLogicalHeight = -1f;
        private Vector2 lastCanvasSize = Vector2.zero;
        private bool lastIsMobileLayout;
        private bool hasAppliedLayout;
        private bool isVictoryState;
        private bool built;

        public event Action OnPauseToggled;
        public event Action OnMuteToggled;
        public event Action<bool> OnRetryRequested;
        public event Action OnReturnToTitleRequested;

        public void ConfigureFont(Font font)
        {
            uiFont = font;
        }

        public void EnsureBuilt()
        {
            if (built)
            {
                return;
            }

            built = true;
            BuildUi();
        }

        public void SetCaptureCamera(Camera cam)
        {
            EnsureBuilt();
            if (canvas == null)
            {
                return;
            }

            if (cam != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 0.8f;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
            }

            Canvas.ForceUpdateCanvases();
            RefreshResponsiveLayout(force: true);
        }

        public void SetViewportOverride(float logicalWidth, float logicalHeight)
        {
            forcedLogicalWidth = logicalWidth;
            forcedLogicalHeight = logicalHeight;
            EnsureBuilt();
            RefreshResponsiveLayout(force: true);
        }

        private void Awake()
        {
            EnsureBuilt();
        }

        private void Update()
        {
            RefreshResponsiveLayout(force: false);
        }

        private void OnDestroy()
        {
            if (heartSprite != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(heartSprite);
                }
                else
                {
                    DestroyImmediate(heartSprite);
                }

                heartSprite = null;
            }

            if (heartTexture != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(heartTexture);
                }
                else
                {
                    DestroyImmediate(heartTexture);
                }

                heartTexture = null;
            }
        }

        public void SetHearts(int current, int max)
        {
            EnsureBuilt();
            if (heartsRoot == null)
            {
                return;
            }

            if (heartSprite == null)
            {
                heartSprite = CreateHeartSprite(64, out heartTexture);
            }

            while (heartImages.Count < max)
            {
                RectTransform rt = CreateRect(
                    $"Heart_{heartImages.Count}",
                    heartsRoot,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    new Vector2(36f, 34f));
                Image img = rt.gameObject.AddComponent<Image>();
                img.sprite = heartSprite;
                img.preserveAspect = true;
                img.raycastTarget = false;
                heartImages.Add(img);
            }

            for (int i = 0; i < heartImages.Count; i++)
            {
                bool visible = i < max;
                heartImages[i].gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                heartImages[i].color = i < current
                    ? new Color(0.98f, 0.44f, 0.64f, 1f)
                    : new Color(0.35f, 0.25f, 0.30f, 0.68f);
            }

            if (hasAppliedLayout && lastIsMobileLayout)
            {
                RefreshResponsiveLayout(force: true);
            }
        }

        private static Sprite CreateHeartSprite(int size, out Texture2D tex)
        {
            tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                float ny = (y / (float)(size - 1)) * 2.4f - 1.1f;
                for (int x = 0; x < size; x++)
                {
                    float nx = (x / (float)(size - 1)) * 2.5f - 1.25f;
                    float x2 = nx * nx;
                    float curve = x2 + Mathf.Pow(ny - Mathf.Sqrt(Mathf.Abs(nx)) * 0.72f, 2f);
                    float dist = curve - 0.95f;
                    float alpha = Mathf.Clamp01(1f - dist * 4.5f);
                    if (dist > 0.22f)
                    {
                        alpha = 0f;
                    }

                    byte a = (byte)Mathf.RoundToInt(alpha * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        public void HandlePhaseChanged(BearChasePhase phase, bool shouldOfferEasyMode)
        {
            EnsureBuilt();
            bool isEntrance = phase.IsEntrance();

            if (topBar != null)
            {
                topBar.gameObject.SetActive(isEntrance);
            }

            if (bottomBar != null)
            {
                bottomBar.gameObject.SetActive(isEntrance);
            }

            if (heartsRoot != null)
            {
                heartsRoot.gameObject.SetActive(!isEntrance);
            }

            if (controlsRoot != null)
            {
                controlsRoot.gameObject.SetActive(!isEntrance);
            }

            if (fallenBannerRoot != null)
            {
                fallenBannerRoot.SetActive(phase == BearChasePhase.Fallen);
            }

            UpdateModalForPhase(phase, shouldOfferEasyMode);
        }

        public void UpdatePerFrameOverlays(BearChaseSimulation sim)
        {
            if (sim == null)
            {
                return;
            }

            if (hintRoot != null)
            {
                bool showHint = sim.Phase == BearChasePhase.Chasing && sim.TotalChaseTime < 7.0f;
                if (hintRoot.activeSelf != showHint)
                {
                    hintRoot.SetActive(showHint);
                }
            }

            if (fadeOverlay != null)
            {
                float alpha = 0f;
                if (sim.Phase == BearChasePhase.TransitionToChase)
                {
                    alpha = sim.EntranceSample.FadeAlpha;
                }
                else if (sim.Phase == BearChasePhase.Chasing && sim.TotalChaseTime < 0.45f)
                {
                    alpha = 1f - Mathf.Clamp01(sim.TotalChaseTime / 0.45f);
                }

                fadeOverlay.color = new Color(0.03f, 0.02f, 0.02f, alpha);
            }
        }

        public void SetPaused(bool paused)
        {
            if (pauseBadgeRoot != null)
            {
                pauseBadgeRoot.SetActive(paused);
            }

            if (pauseLabel != null)
            {
                pauseLabel.text = paused ? "▶" : "P";
            }
        }

        public void SetMuted(bool muted)
        {
            if (muteLabel != null)
            {
                muteLabel.text = muted ? "Mudo" : "Som";
            }
        }

        private void UpdateModalForPhase(BearChasePhase phase, bool shouldOfferEasyMode)
        {
            if (modalRoot == null)
            {
                return;
            }

            if (phase == BearChasePhase.RetryModal)
            {
                isVictoryState = false;
                modalRoot.SetActive(true);
                modalTitle.text = "✦ PERSEGUIÇÃO AO URSO ✦";
                modalBody.text = shouldOfferEasyMode
                    ? "A menina não alcançou o urso. Deseja tentar no fácil?\nA fadinha oferece 6 corações para ajudar."
                    : "A menina não alcançou o urso. Tentar novamente?\nA fadinha está pronta para acompanhar uma nova tentativa.";
                primaryButtonLabel.text = "Tentar de novo";
                easyButton.gameObject.SetActive(shouldOfferEasyMode);
                return;
            }

            if (phase == BearChasePhase.Victory)
            {
                isVictoryState = true;
                modalRoot.SetActive(true);
                modalTitle.text = "✦ URSO ALCANÇADO! ✦";
                modalBody.text = "A menina alcançou o urso fujão pelo corredor do quarto.\nA fadinha brilha satisfeita ao seu lado.";
                primaryButtonLabel.text = "Voltar ao início";
                easyButton.gameObject.SetActive(false);
                return;
            }

            modalRoot.SetActive(false);
        }

        private void BuildUi()
        {
            if (uiFont == null)
            {
                uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            var canvasGo = new GameObject("BearChase_HudCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.GetComponent<Canvas>();
            canvasRect = canvasGo.GetComponent<RectTransform>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Transform root = canvasGo.transform;

            BuildLetterboxAndHearts(root);
            BuildTopRightControls(root);
            BuildBottomHintAndBanners(root);
            BuildModal(root);
            BuildCrossfadeOverlay(root);
            RefreshResponsiveLayout(force: true);
        }

        private void BuildLetterboxAndHearts(Transform root)
        {
            topBar = CreateRect("TopLetterbox", root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 54f));
            var topImg = topBar.gameObject.AddComponent<Image>();
            topImg.color = new Color(0.03f, 0.02f, 0.02f, 0.96f);
            topImg.raycastTarget = false;

            bottomBar = CreateRect("BottomLetterbox", root, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 54f));
            var bottomImg = bottomBar.gameObject.AddComponent<Image>();
            bottomImg.color = new Color(0.03f, 0.02f, 0.02f, 0.96f);
            bottomImg.raycastTarget = false;

            heartsRoot = CreateRect("HeartsRow", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(360f, 48f));
            var bg = heartsRoot.gameObject.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.07f, 0.07f, 0.58f);
            bg.raycastTarget = false;

            var hlg = heartsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 10f;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            heartsRoot.gameObject.SetActive(false);
        }

        private void BuildTopRightControls(Transform root)
        {
            controlsRoot = CreateRect("TopRightControls", root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -22f), new Vector2(220f, 48f));
            var clg = controlsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            clg.childAlignment = TextAnchor.MiddleRight;
            clg.spacing = 10f;
            clg.childControlWidth = false;
            clg.childControlHeight = false;

            Button homeBtn = CreateTopCircleButton("Inicio", "Início", controlsRoot, () => OnReturnToTitleRequested?.Invoke());
            homeBtnRect = homeBtn.GetComponent<RectTransform>();
            homeLabel = homeBtn.GetComponentInChildren<Text>(true);

            pauseButton = CreateTopCircleButton("Pausa", "P", controlsRoot, () => OnPauseToggled?.Invoke());
            pauseBtnRect = pauseButton.GetComponent<RectTransform>();
            pauseLabel = pauseButton.GetComponentInChildren<Text>(true);

            muteButton = CreateTopCircleButton("Som", "Som", controlsRoot, () => OnMuteToggled?.Invoke());
            muteBtnRect = muteButton.GetComponent<RectTransform>();
            muteLabel = muteButton.GetComponentInChildren<Text>(true);
        }

        private void BuildBottomHintAndBanners(Transform root)
        {
            hintPanelRect = CreateRect("DodgeHintBar", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(760f, 68f));
            var hintBg = hintPanelRect.gameObject.AddComponent<Image>();
            hintBg.color = new Color(0.06f, 0.04f, 0.04f, 0.58f);
            hintBg.raycastTarget = false;
            hintRoot = hintPanelRect.gameObject;

            RectTransform hintTextRt = CreateRect("DodgeHintText", hintPanelRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            hintText = hintTextRt.gameObject.AddComponent<Text>();
            hintText.font = uiFont;
            hintText.fontSize = 24;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.horizontalOverflow = HorizontalWrapMode.Wrap;
            hintText.verticalOverflow = VerticalWrapMode.Overflow;
            hintText.color = new Color(0.98f, 0.94f, 0.84f, 0.94f);
            hintText.text = "← A · desvie · D →\nToque e segure nas laterais · P pausa · M som";
            hintText.raycastTarget = false;
            hintRoot.SetActive(false);

            pauseBadgeRect = CreateRect("PauseBadge", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360f, 88f));
            var pauseBg = pauseBadgeRect.gameObject.AddComponent<Image>();
            pauseBg.color = new Color(0.06f, 0.04f, 0.04f, 0.86f);
            pauseBg.raycastTarget = false;
            pauseBadgeRoot = pauseBadgeRect.gameObject;

            RectTransform pauseTxtRt = CreateRect("PauseText", pauseBadgeRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            pauseBadgeText = pauseTxtRt.gameObject.AddComponent<Text>();
            pauseBadgeText.font = uiFont;
            pauseBadgeText.fontSize = 36;
            pauseBadgeText.alignment = TextAnchor.MiddleCenter;
            pauseBadgeText.color = new Color(1f, 0.88f, 0.58f);
            pauseBadgeText.text = "PAUSADO";
            pauseBadgeRoot.SetActive(false);

            fallenBannerRect = CreateRect("FallenBanner", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(760f, 92f));
            fallenBannerRect.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.04f, 0.04f, 0.88f);
            fallenBannerRoot = fallenBannerRect.gameObject;

            RectTransform bannerTextRt = CreateRect("BannerText", fallenBannerRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            fallenBannerText = bannerTextRt.gameObject.AddComponent<Text>();
            fallenBannerText.font = uiFont;
            fallenBannerText.fontSize = 28;
            fallenBannerText.alignment = TextAnchor.MiddleCenter;
            fallenBannerText.horizontalOverflow = HorizontalWrapMode.Wrap;
            fallenBannerText.verticalOverflow = VerticalWrapMode.Overflow;
            fallenBannerText.color = new Color(0.98f, 0.92f, 0.80f);
            fallenBannerText.text = "A menina caiu.\nTente novamente pelos botões da tela.";
            fallenBannerRoot.SetActive(false);
        }

        private void BuildModal(Transform root)
        {
            modalBoxRect = CreateRect("CenterModal", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 360f));
            modalBoxRect.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.05f, 0.05f, 0.94f);
            Outline outline = modalBoxRect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.86f, 0.72f, 0.45f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
            modalRoot = modalBoxRect.gameObject;

            modalTitleRect = CreateRect("Title", modalBoxRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(620f, 52f));
            modalTitle = modalTitleRect.gameObject.AddComponent<Text>();
            modalTitle.font = uiFont;
            modalTitle.fontSize = 34;
            modalTitle.alignment = TextAnchor.MiddleCenter;
            modalTitle.color = new Color(1f, 0.86f, 0.56f);

            modalBodyRect = CreateRect("Body", modalBoxRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 22f), new Vector2(600f, 110f));
            modalBody = modalBodyRect.gameObject.AddComponent<Text>();
            modalBody.font = uiFont;
            modalBody.fontSize = 25;
            modalBody.alignment = TextAnchor.MiddleCenter;
            modalBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            modalBody.verticalOverflow = VerticalWrapMode.Overflow;
            modalBody.color = new Color(0.96f, 0.92f, 0.84f);

            modalButtonsRowRect = CreateRect("Buttons", modalBoxRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(600f, 64f));
            var rowLayout = modalButtonsRowRect.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.spacing = 18f;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = false;

            primaryButton = CreateModalButton("PrimaryBtn", "Tentar de novo", modalButtonsRowRect, () =>
            {
                if (isVictoryState)
                {
                    OnReturnToTitleRequested?.Invoke();
                }
                else
                {
                    OnRetryRequested?.Invoke(false);
                }
            }, out primaryButtonLabel);

            easyButton = CreateModalButton("EasyBtn", "Modo Fácil (6 ♥)", modalButtonsRowRect, () =>
            {
                OnRetryRequested?.Invoke(true);
            }, out easyButtonLabel);
            easyButton.gameObject.SetActive(false);

            titleButton = CreateModalButton("TitleBtn", "Voltar ao início", modalButtonsRowRect, () =>
            {
                OnReturnToTitleRequested?.Invoke();
            }, out titleButtonLabel);

            modalRoot.SetActive(false);
        }

        private void RefreshResponsiveLayout(bool force)
        {
            if (canvasRect == null)
            {
                return;
            }

            Vector2 size = canvasRect.rect.size;
            if (size.x <= 1f || size.y <= 1f)
            {
                size = new Vector2(1920f, 1080f);
            }

            bool isMobile = IsMobileViewport();
            if (!force && hasAppliedLayout && isMobile == lastIsMobileLayout && (size - lastCanvasSize).sqrMagnitude <= 1f)
            {
                return;
            }

            lastCanvasSize = size;
            lastIsMobileLayout = isMobile;
            hasAppliedLayout = true;

            if (!isMobile)
            {
                ApplyDesktopLayout();
                return;
            }

            ApplyMobileLayout(size);
        }

        private void ApplyMobileLayout(Vector2 canvasSize)
        {
            float px = ComputePxToCanvas(canvasSize);
            bool isPortrait = (canvasSize.x / canvasSize.y) < 1.1f;
            float marginX = 10f * px;
            float topY = -10f * px;
            float usableW = Mathf.Max(240f * px, canvasSize.x - marginX * 2f);

            int f14 = Mathf.RoundToInt(14f * px);
            int f15 = Mathf.RoundToInt(15f * px);
            int f16 = Mathf.RoundToInt(16f * px);

            // Botoes superiores direitos (14 px)
            float homeW = 54f * px;
            float pauseW = 34f * px;
            float muteW = 48f * px;
            float btnH = 30f * px;
            float btnGap = 6f * px;
            float controlsW = homeW + pauseW + muteW + btnGap * 2f;

            if (controlsRoot != null)
            {
                controlsRoot.anchoredPosition = new Vector2(-marginX, topY);
                controlsRoot.sizeDelta = new Vector2(controlsW, btnH);
                var clg = controlsRoot.GetComponent<HorizontalLayoutGroup>();
                if (clg != null)
                {
                    clg.spacing = btnGap;
                }
            }

            if (homeBtnRect != null)
            {
                homeBtnRect.sizeDelta = new Vector2(homeW, btnH);
            }

            if (pauseBtnRect != null)
            {
                pauseBtnRect.sizeDelta = new Vector2(pauseW, btnH);
            }

            if (muteBtnRect != null)
            {
                muteBtnRect.sizeDelta = new Vector2(muteW, btnH);
            }

            SetFontSize(homeLabel, f14);
            SetFontSize(pauseLabel, f14);
            SetFontSize(muteLabel, f14);

            // Barra de coracoes adaptada sem colidir com os botoes no mobile
            if (heartsRoot != null)
            {
                float heartSize = 22f * px;
                float heartsW = Mathf.Min(isPortrait ? usableW - controlsW - 8f * px : 210f * px, 220f * px);
                if (isPortrait)
                {
                    heartsRoot.anchorMin = new Vector2(0f, 1f);
                    heartsRoot.anchorMax = new Vector2(0f, 1f);
                    heartsRoot.pivot = new Vector2(0f, 1f);
                    heartsRoot.anchoredPosition = new Vector2(marginX, topY);
                }
                else
                {
                    heartsRoot.anchorMin = new Vector2(0.5f, 1f);
                    heartsRoot.anchorMax = new Vector2(0.5f, 1f);
                    heartsRoot.pivot = new Vector2(0.5f, 1f);
                    heartsRoot.anchoredPosition = new Vector2(0f, topY);
                }

                heartsRoot.sizeDelta = new Vector2(Mathf.Max(110f * px, heartsW), btnH);
                var hlg = heartsRoot.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.spacing = 6f * px;
                }

                for (int i = 0; i < heartImages.Count; i++)
                {
                    if (heartImages[i] != null)
                    {
                        heartImages[i].rectTransform.sizeDelta = new Vector2(heartSize, heartSize * 0.94f);
                    }
                }
            }

            // Dica inferior de desvio (14 px)
            if (hintPanelRect != null)
            {
                hintPanelRect.anchoredPosition = new Vector2(0f, 12f * px);
                hintPanelRect.sizeDelta = new Vector2(Mathf.Min(420f * px, usableW), 44f * px);
            }

            SetFontSize(hintText, f14);

            // Badge de pausa (16 px)
            if (pauseBadgeRect != null)
            {
                pauseBadgeRect.sizeDelta = new Vector2(Mathf.Min(210f * px, usableW), 48f * px);
            }

            SetFontSize(pauseBadgeText, f16);

            // Banner de queda (15 px)
            if (fallenBannerRect != null)
            {
                fallenBannerRect.anchoredPosition = new Vector2(0f, 24f * px);
                fallenBannerRect.sizeDelta = new Vector2(Mathf.Min(360f * px, usableW), 56f * px);
            }

            SetFontSize(fallenBannerText, f15);

            // Modal central (titulo 16 px, corpo 14 px, botoes 15 px)
            if (modalBoxRect != null)
            {
                float modalW = Mathf.Min(356f * px, usableW);
                float modalH = isPortrait ? 210f * px : 190f * px;
                modalBoxRect.sizeDelta = new Vector2(modalW, modalH);

                if (modalTitleRect != null)
                {
                    modalTitleRect.anchoredPosition = new Vector2(0f, -16f * px);
                    modalTitleRect.sizeDelta = new Vector2(modalW - 24f * px, 28f * px);
                }

                if (modalBodyRect != null)
                {
                    modalBodyRect.anchoredPosition = new Vector2(0f, 16f * px);
                    modalBodyRect.sizeDelta = new Vector2(modalW - 28f * px, 64f * px);
                }

                if (modalButtonsRowRect != null)
                {
                    modalButtonsRowRect.anchoredPosition = new Vector2(0f, 16f * px);
                    modalButtonsRowRect.sizeDelta = new Vector2(modalW - 20f * px, 42f * px);
                    var rowLayout = modalButtonsRowRect.GetComponent<HorizontalLayoutGroup>();
                    if (rowLayout != null)
                    {
                        rowLayout.spacing = 6f * px;
                    }
                }

                float modalBtnW = Mathf.Clamp((modalW - 36f * px) / 3f, 92f * px, 136f * px);
                float modalBtnH = 36f * px;
                SetButtonSize(primaryButton, modalBtnW, modalBtnH);
                SetButtonSize(easyButton, modalBtnW, modalBtnH);
                SetButtonSize(titleButton, modalBtnW, modalBtnH);
            }

            SetFontSize(modalTitle, f16);
            SetFontSize(modalBody, f14);
            SetFontSize(primaryButtonLabel, f15);
            SetFontSize(easyButtonLabel, f15);
            SetFontSize(titleButtonLabel, f15);
        }

        private void ApplyDesktopLayout()
        {
            if (controlsRoot != null)
            {
                controlsRoot.anchoredPosition = new Vector2(-28f, -22f);
                controlsRoot.sizeDelta = new Vector2(220f, 48f);
                var clg = controlsRoot.GetComponent<HorizontalLayoutGroup>();
                if (clg != null)
                {
                    clg.spacing = 10f;
                }
            }

            if (homeBtnRect != null)
            {
                homeBtnRect.sizeDelta = new Vector2(62f, 42f);
            }

            if (pauseBtnRect != null)
            {
                pauseBtnRect.sizeDelta = new Vector2(62f, 42f);
            }

            if (muteBtnRect != null)
            {
                muteBtnRect.sizeDelta = new Vector2(62f, 42f);
            }

            SetFontSize(homeLabel, 18);
            SetFontSize(pauseLabel, 18);
            SetFontSize(muteLabel, 18);

            if (heartsRoot != null)
            {
                heartsRoot.anchorMin = new Vector2(0.5f, 1f);
                heartsRoot.anchorMax = new Vector2(0.5f, 1f);
                heartsRoot.pivot = new Vector2(0.5f, 1f);
                heartsRoot.anchoredPosition = new Vector2(0f, -24f);
                heartsRoot.sizeDelta = new Vector2(360f, 48f);
                var hlg = heartsRoot.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.spacing = 10f;
                }

                for (int i = 0; i < heartImages.Count; i++)
                {
                    if (heartImages[i] != null)
                    {
                        heartImages[i].rectTransform.sizeDelta = new Vector2(36f, 34f);
                    }
                }
            }

            if (hintPanelRect != null)
            {
                hintPanelRect.anchoredPosition = new Vector2(0f, 18f);
                hintPanelRect.sizeDelta = new Vector2(760f, 68f);
            }

            SetFontSize(hintText, 24);

            if (pauseBadgeRect != null)
            {
                pauseBadgeRect.sizeDelta = new Vector2(360f, 88f);
            }

            SetFontSize(pauseBadgeText, 36);

            if (fallenBannerRect != null)
            {
                fallenBannerRect.anchoredPosition = new Vector2(0f, 52f);
                fallenBannerRect.sizeDelta = new Vector2(760f, 92f);
            }

            SetFontSize(fallenBannerText, 28);

            if (modalBoxRect != null)
            {
                modalBoxRect.sizeDelta = new Vector2(680f, 360f);
                if (modalTitleRect != null)
                {
                    modalTitleRect.anchoredPosition = new Vector2(0f, -34f);
                    modalTitleRect.sizeDelta = new Vector2(620f, 52f);
                }

                if (modalBodyRect != null)
                {
                    modalBodyRect.anchoredPosition = new Vector2(0f, 22f);
                    modalBodyRect.sizeDelta = new Vector2(600f, 110f);
                }

                if (modalButtonsRowRect != null)
                {
                    modalButtonsRowRect.anchoredPosition = new Vector2(0f, 34f);
                    modalButtonsRowRect.sizeDelta = new Vector2(600f, 64f);
                    var rowLayout = modalButtonsRowRect.GetComponent<HorizontalLayoutGroup>();
                    if (rowLayout != null)
                    {
                        rowLayout.spacing = 18f;
                    }
                }

                SetButtonSize(primaryButton, 182f, 52f);
                SetButtonSize(easyButton, 182f, 52f);
                SetButtonSize(titleButton, 182f, 52f);
            }

            SetFontSize(modalTitle, 34);
            SetFontSize(modalBody, 25);
            SetFontSize(primaryButtonLabel, 21);
            SetFontSize(easyButtonLabel, 21);
            SetFontSize(titleButtonLabel, 21);
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

        private static void SetFontSize(Text txt, int size)
        {
            if (txt != null)
            {
                txt.fontSize = size;
            }
        }

        private static void SetButtonSize(Button btn, float width, float height)
        {
            if (btn != null)
            {
                var rt = btn.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = new Vector2(width, height);
                }
            }
        }

        private void BuildCrossfadeOverlay(Transform root)
        {
            RectTransform fadeRt = CreateRect("CrossfadeOverlay", root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            fadeRt.SetSiblingIndex(2);
            fadeOverlay = fadeRt.gameObject.AddComponent<Image>();
            fadeOverlay.color = new Color(0.03f, 0.02f, 0.02f, 0f);
            fadeOverlay.raycastTarget = false;
        }

        private Button CreateTopCircleButton(string name, string label, Transform parent, Action onClick)
        {
            RectTransform rt = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(62f, 42f));
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.09f, 0.07f, 0.07f, 0.85f);
            Button btn = rt.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            RectTransform txtRt = CreateRect("Label", rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Text txt = txtRt.gameObject.AddComponent<Text>();
            txt.font = uiFont;
            txt.fontSize = 18;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.96f, 0.90f, 0.78f);
            txt.text = label;
            return btn;
        }

        private Button CreateModalButton(string name, string label, Transform parent, Action onClick, out Text labelText)
        {
            RectTransform rt = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(182f, 52f));
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.22f, 0.14f, 0.11f, 0.98f);
            Button btn = rt.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            RectTransform txtRt = CreateRect("Label", rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            labelText = txtRt.gameObject.AddComponent<Text>();
            labelText.font = uiFont;
            labelText.fontSize = 21;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = new Color(1f, 0.90f, 0.66f);
            labelText.text = label;
            return btn;
        }

        private static RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPos,
            Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return rt;
        }
    }
}
