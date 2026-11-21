using UnityEngine;
using UnityEngine.UI;

namespace YunaLegend.UI
{
    /// <summary>
    /// Adapta a tela de entrada entre o layout widescreen (desktop/notebook),
    /// mobile paisagem e mobile retrato:
    /// - Prioridade 1: textos na parte superior no celular, sem encobrir a menina na poltrona.
    /// - Prioridade 2: fontes entre 14 e 16px logicos no celular.
    /// - Prioridade 3: preserva a estetica vitoriana/livro ilustrado original.
    /// - Prioridade 4: enfeites, paineis e enquadramentos cabem integralmente na tela do celular.
    /// </summary>
    [ExecuteAlways]
    public sealed class TitleScreenResponsiveLayout : MonoBehaviour
    {
        private const float TargetArtAspect = 16f / 9f;
        private const float PortraitFocusNormalizedX = 0.67f;
        private const float PortraitThresholdAspect = 1.1f;

        [Header("Containers")]
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform artContainer;
        [SerializeField] private GameObject leftShade;
        [SerializeField] private GameObject bottomScrim;

        [Header("Menu")]
        [SerializeField] private Text titleText;
        [SerializeField] private RectTransform newGameRect;
        [SerializeField] private RectTransform loadGameRect;
        [SerializeField] private RectTransform easyModeRect;
        [SerializeField] private RectTransform easyTagRect;
        [SerializeField] private RectTransform footerRect;
        [SerializeField] private Text focusMarker;

        [Header("Controles e paineis")]
        [SerializeField] private RectTransform settingsButtonRect;
        [SerializeField] private RectTransform flagButtonRect;
        [SerializeField] private RectTransform settingsPanelRect;
        [SerializeField] private RectTransform volumeSliderRect;
        [SerializeField] private RectTransform phasePanelRect;

        private Vector2 lastCanvasSize = new Vector2(-1f, -1f);
        private bool? lastPortraitState;
        private bool? lastCompactLandscapeState;
        private float forcedLogicalWidth = -1f;
        private float forcedLogicalHeight = -1f;

        private void OnEnable()
        {
            ApplyLayout(force: true);
        }

        private void Start()
        {
            ApplyLayout(force: true);
        }

        private void Update()
        {
            if (canvasRect == null)
            {
                return;
            }

            Vector2 size = canvasRect.rect.size;
            if (!Mathf.Approximately(size.x, lastCanvasSize.x) || !Mathf.Approximately(size.y, lastCanvasSize.y))
            {
                ApplyLayout(force: false);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyLayout(force: false);
        }

        /// <summary>
        /// Permite simular uma resolucao logica de viewport (usado em capturas automatizadas).
        /// </summary>
        public void SetViewportOverride(float logicalWidth, float logicalHeight)
        {
            forcedLogicalWidth = logicalWidth;
            forcedLogicalHeight = logicalHeight;
            ApplyLayout(force: true);
        }

        public void ClearViewportOverride()
        {
            forcedLogicalWidth = -1f;
            forcedLogicalHeight = -1f;
            ApplyLayout(force: true);
        }

        public void Configure(
            RectTransform canvas,
            RectTransform art,
            GameObject leftShadeGo,
            GameObject bottomScrimGo,
            Text title,
            RectTransform newGame,
            RectTransform loadGame,
            RectTransform easyMode,
            RectTransform easyTag,
            RectTransform footer,
            Text marker,
            RectTransform settingsBtn,
            RectTransform flagBtn,
            RectTransform settingsPanel,
            RectTransform volumeSlider,
            RectTransform phasePanel)
        {
            canvasRect = canvas;
            artContainer = art;
            leftShade = leftShadeGo;
            bottomScrim = bottomScrimGo;
            titleText = title;
            newGameRect = newGame;
            loadGameRect = loadGame;
            easyModeRect = easyMode;
            easyTagRect = easyTag;
            footerRect = footer;
            focusMarker = marker;
            settingsButtonRect = settingsBtn;
            flagButtonRect = flagBtn;
            settingsPanelRect = settingsPanel;
            volumeSliderRect = volumeSlider;
            phasePanelRect = phasePanel;
            ApplyLayout(force: true);
        }

        public void ApplyLayout(bool force)
        {
            if (canvasRect == null)
            {
                canvasRect = GetComponent<RectTransform>();
            }

            if (canvasRect == null)
            {
                return;
            }

            Vector2 size = canvasRect.rect.size;
            if (size.x <= 1f || size.y <= 1f)
            {
                return;
            }

            bool isPortrait = (size.x / size.y) < PortraitThresholdAspect;
            bool isCompactLandscape = !isPortrait && IsMobileLandscapeViewport();

            if (!force
                && Mathf.Approximately(size.x, lastCanvasSize.x)
                && Mathf.Approximately(size.y, lastCanvasSize.y)
                && lastPortraitState == isPortrait
                && lastCompactLandscapeState == isCompactLandscape)
            {
                return;
            }

            lastCanvasSize = size;
            lastPortraitState = isPortrait;
            lastCompactLandscapeState = isCompactLandscape;

            float pxToCanvas = ComputePxToCanvas(size);

            ApplyArtFraming(size, isPortrait);
            ApplyOverlayScrims(isPortrait);

            if (isPortrait)
            {
                ApplyPortraitMenu(size, pxToCanvas);
            }
            else if (isCompactLandscape)
            {
                ApplyCompactLandscapeMenu(size, pxToCanvas);
            }
            else
            {
                ApplyDesktopLandscapeMenu();
            }

            ApplyPanels(size, isPortrait, isCompactLandscape, pxToCanvas);
        }

        private float ComputePxToCanvas(Vector2 canvasSize)
        {
            float logicalW = forcedLogicalWidth > 0f ? forcedLogicalWidth : Screen.width;
            float logicalH = forcedLogicalHeight > 0f ? forcedLogicalHeight : Screen.height;

            if (logicalW <= 1f || logicalH <= 1f)
            {
                return canvasSize.x / 390f;
            }

            // Em telas retina mobile reais (DPR >= 2), normaliza para pixels logicos CSS (~390px largura em retrato)
            float minDim = Mathf.Min(logicalW, logicalH);
            if (forcedLogicalWidth <= 0f && Application.isMobilePlatform && minDim >= 700f)
            {
                float dpr = Mathf.Clamp(minDim / 390f, 1.5f, 3.5f);
                logicalW /= dpr;
            }

            return Mathf.Clamp(canvasSize.x / Mathf.Max(280f, logicalW), 0.5f, 4.0f);
        }

        private bool IsMobileLandscapeViewport()
        {
            float w = forcedLogicalWidth > 0f ? forcedLogicalWidth : Screen.width;
            float h = forcedLogicalHeight > 0f ? forcedLogicalHeight : Screen.height;
            if (w <= 1f || h <= 1f)
            {
                return false;
            }

            return (Application.isMobilePlatform || w <= 960f || h <= 520f) && (w / h >= PortraitThresholdAspect);
        }

        private void ApplyArtFraming(Vector2 canvasSize, bool isPortrait)
        {
            if (artContainer == null)
            {
                return;
            }

            artContainer.anchorMin = new Vector2(0.5f, 0.5f);
            artContainer.anchorMax = new Vector2(0.5f, 0.5f);
            artContainer.pivot = new Vector2(0.5f, 0.5f);

            if (!isPortrait)
            {
                float artHeight = Mathf.Max(canvasSize.y, canvasSize.x / TargetArtAspect);
                float artWidth = artHeight * TargetArtAspect;
                artContainer.sizeDelta = new Vector2(artWidth, artHeight);
                artContainer.anchoredPosition = Vector2.zero;
                return;
            }

            // Em retrato mobile: posiciona a menina na poltrona nos ~60% inferiores da tela,
            // deixando o topo livre para o titulo e menu sem encobrir a menina.
            float lowerBandHeight = canvasSize.y * 0.64f;
            float portraitArtHeight = Mathf.Max(lowerBandHeight, canvasSize.x / TargetArtAspect * 1.12f);
            float portraitArtWidth = portraitArtHeight * TargetArtAspect;
            artContainer.sizeDelta = new Vector2(portraitArtWidth, portraitArtHeight);

            float maxLeftShift = Mathf.Max(0f, 0.5f * (portraitArtWidth - canvasSize.x));
            float desiredShiftX = (0.5f - PortraitFocusNormalizedX) * portraitArtWidth;
            float clampedX = Mathf.Clamp(desiredShiftX, -maxLeftShift, 0f);

            // Alinha a base da ilustracao com a base da tela para manter a menina inteira abaixo do texto superior
            float bottomAlignY = -0.5f * (canvasSize.y - portraitArtHeight);
            artContainer.anchoredPosition = new Vector2(clampedX, bottomAlignY);
        }

        private void ApplyOverlayScrims(bool isPortrait)
        {
            if (leftShade != null)
            {
                leftShade.SetActive(!isPortrait);
            }

            if (bottomScrim != null)
            {
                bottomScrim.SetActive(isPortrait);
                if (isPortrait)
                {
                    var scrimRt = bottomScrim.GetComponent<RectTransform>();
                    if (scrimRt != null)
                    {
                        // Gradiente escuro suave no TOPO (acima da menina), mantendo legibilidade sem cobrir Yuna
                        scrimRt.anchorMin = new Vector2(0f, 0.52f);
                        scrimRt.anchorMax = new Vector2(1f, 1f);
                        scrimRt.offsetMin = Vector2.zero;
                        scrimRt.offsetMax = Vector2.zero;
                        scrimRt.localEulerAngles = new Vector3(0f, 0f, 180f);
                    }
                }
            }
        }

        private void ApplyDesktopLandscapeMenu()
        {
            if (titleText != null)
            {
                SetTopLeft(titleText.rectTransform, 130f, -180f, 1000f, 320f);
                titleText.fontSize = 92;
                titleText.lineSpacing = 0.95f;
            }

            SetMenuButtonLayout(newGameRect, 140f, -486f, 560f, 58f, 38, topLeft: true);
            SetMenuButtonLayout(loadGameRect, 140f, -556f, 560f, 58f, 38, topLeft: true);
            SetMenuButtonLayout(easyModeRect, 140f, -626f, 360f, 58f, 38, topLeft: true);

            if (easyTagRect != null)
            {
                SetTopLeft(easyTagRect, 408f, -626f, 220f, 56f);
                SetTextSize(easyTagRect, 22);
            }

            if (footerRect != null)
            {
                SetTopLeft(footerRect, 140f, -712f, 900f, 40f);
                SetTextSize(footerRect, 22);
            }

            if (focusMarker != null)
            {
                focusMarker.fontSize = 40;
            }

            if (settingsButtonRect != null)
            {
                settingsButtonRect.anchoredPosition = new Vector2(-58f, -50f);
                settingsButtonRect.sizeDelta = new Vector2(64f, 64f);
            }

            if (flagButtonRect != null)
            {
                flagButtonRect.anchoredPosition = new Vector2(0f, -26f);
                flagButtonRect.sizeDelta = new Vector2(72f, 90f);
            }
        }

        private void ApplyCompactLandscapeMenu(Vector2 canvasSize, float px)
        {
            float marginX = 20f * px;
            float colWidth = Mathf.Min(canvasSize.x * 0.44f, 320f * px);
            float topY = -18f * px;

            int titleFont = Mathf.RoundToInt(16f * px);
            int btnFont = Mathf.RoundToInt(15f * px);
            int smallFont = Mathf.RoundToInt(14f * px);

            float titleH = 48f * px;
            float btnH = 30f * px;
            float btnGap = 6f * px;

            if (titleText != null)
            {
                SetTopLeft(titleText.rectTransform, marginX, topY, colWidth, titleH);
                titleText.fontSize = titleFont;
                titleText.lineSpacing = 1.05f;
                titleText.alignment = TextAnchor.UpperLeft;
            }

            float newY = topY - titleH - 12f * px;
            float loadY = newY - btnH - btnGap;
            float easyY = loadY - btnH - btnGap;
            float footerY = easyY - btnH - 10f * px;

            SetMenuButtonLayout(newGameRect, marginX + 14f * px, newY, colWidth - 14f * px, btnH, btnFont, topLeft: true);
            SetMenuButtonLayout(loadGameRect, marginX + 14f * px, loadY, colWidth - 14f * px, btnH, btnFont, topLeft: true);
            SetMenuButtonLayout(easyModeRect, marginX + 14f * px, easyY, colWidth * 0.52f, btnH, btnFont, topLeft: true);

            if (easyTagRect != null)
            {
                float tagX = marginX + colWidth * 0.55f;
                SetTopLeft(easyTagRect, tagX, easyY, colWidth * 0.45f, btnH);
                SetTextSize(easyTagRect, smallFont);
            }

            if (footerRect != null)
            {
                SetTopLeft(footerRect, marginX + 14f * px, footerY, colWidth - 14f * px, 38f * px);
                SetTextSize(footerRect, smallFont);
            }

            if (focusMarker != null)
            {
                focusMarker.fontSize = btnFont;
                if (newGameRect != null)
                {
                    SetTopLeft(focusMarker.rectTransform, marginX, newY, 12f * px, btnH);
                    focusMarker.alignment = TextAnchor.MiddleRight;
                }
            }

            if (settingsButtonRect != null)
            {
                settingsButtonRect.anchoredPosition = new Vector2(-14f * px, -12f * px);
                settingsButtonRect.sizeDelta = new Vector2(36f * px, 36f * px);
            }

            if (flagButtonRect != null)
            {
                flagButtonRect.anchoredPosition = new Vector2(0f, -8f * px);
                flagButtonRect.sizeDelta = new Vector2(38f * px, 48f * px);
            }
        }

        private void ApplyPortraitMenu(Vector2 canvasSize, float px)
        {
            // Prioridade 1: TODOS os textos na parte SUPERIOR da tela do celular,
            // abaixo da bandeirinha/engrenagem e ACIMA da menina na poltrona.
            // Prioridade 2: fontes estritamente entre 14px e 16px logicos.
            float marginX = 18f * px;
            float contentWidth = Mathf.Max(200f * px, canvasSize.x - marginX * 2f);

            int titleFont = Mathf.RoundToInt(16f * px);
            int btnFont = Mathf.RoundToInt(15f * px);
            int smallFont = Mathf.RoundToInt(14f * px);

            float topIconsBottom = 56f * px;
            float titleH = 48f * px;
            float btnH = 34f * px;
            float btnGap = 6f * px;
            float footerH = 38f * px;

            float titleY = -topIconsBottom;
            float newY = titleY - titleH - 10f * px;
            float loadY = newY - btnH - btnGap;
            float easyY = loadY - btnH - btnGap;
            float footerY = easyY - btnH - 10f * px;

            if (titleText != null)
            {
                SetTopLeft(titleText.rectTransform, marginX, titleY, contentWidth, titleH);
                titleText.fontSize = titleFont;
                titleText.lineSpacing = 1.05f;
                titleText.alignment = TextAnchor.UpperLeft;
            }

            float menuIndent = 14f * px;
            float menuW = contentWidth - menuIndent;

            SetMenuButtonLayout(newGameRect, marginX + menuIndent, newY, menuW, btnH, btnFont, topLeft: true);
            SetMenuButtonLayout(loadGameRect, marginX + menuIndent, loadY, menuW, btnH, btnFont, topLeft: true);
            SetMenuButtonLayout(easyModeRect, marginX + menuIndent, easyY, menuW * 0.54f, btnH, btnFont, topLeft: true);

            if (easyTagRect != null)
            {
                float tagOffsetX = marginX + menuIndent + menuW * 0.56f;
                SetTopLeft(easyTagRect, tagOffsetX, easyY, menuW * 0.44f, btnH);
                SetTextSize(easyTagRect, smallFont);
            }

            if (footerRect != null)
            {
                SetTopLeft(footerRect, marginX + menuIndent, footerY, menuW, footerH);
                SetTextSize(footerRect, smallFont);
            }

            if (focusMarker != null)
            {
                focusMarker.fontSize = btnFont;
                if (newGameRect != null)
                {
                    SetTopLeft(focusMarker.rectTransform, marginX, newY, 12f * px, btnH);
                    focusMarker.alignment = TextAnchor.MiddleRight;
                }
            }

            if (settingsButtonRect != null)
            {
                settingsButtonRect.anchoredPosition = new Vector2(-14f * px, -10f * px);
                settingsButtonRect.sizeDelta = new Vector2(36f * px, 36f * px);
            }

            if (flagButtonRect != null)
            {
                flagButtonRect.anchoredPosition = new Vector2(0f, -6f * px);
                flagButtonRect.sizeDelta = new Vector2(38f * px, 46f * px);
            }
        }

        private void ApplyPanels(Vector2 canvasSize, bool isPortrait, bool isCompactLandscape, float px)
        {
            bool isMobile = isPortrait || isCompactLandscape;
            float margin = 16f * px;
            float maxPanelW = Mathf.Max(220f * px, canvasSize.x - margin * 2f);

            if (settingsPanelRect != null)
            {
                if (isMobile)
                {
                    float panelWidth = Mathf.Min(346f * px, maxPanelW);
                    float panelHeight = 192f * px;

                    // Prioridade 1: em retrato, posicionar o painel de configuracoes na faixa SUPERIOR
                    // escura (acima da menina na poltrona) com backdrop opaco cobrindo o menu atras.
                    if (isPortrait)
                    {
                        settingsPanelRect.anchorMin = new Vector2(0.5f, 1f);
                        settingsPanelRect.anchorMax = new Vector2(0.5f, 1f);
                        settingsPanelRect.pivot = new Vector2(0.5f, 1f);
                        settingsPanelRect.anchoredPosition = new Vector2(0f, -56f * px);
                        panelHeight = 238f * px;
                        panelWidth = canvasSize.x - 16f * px;
                    }
                    else
                    {
                        settingsPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
                        settingsPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
                        settingsPanelRect.pivot = new Vector2(0.5f, 0.5f);
                        settingsPanelRect.anchoredPosition = Vector2.zero;
                    }

                    settingsPanelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

                    var bgImg = settingsPanelRect.GetComponent<Image>();
                    if (bgImg != null)
                    {
                        bgImg.color = new Color(0.055f, 0.038f, 0.028f, 1f);
                    }

                    var outline = settingsPanelRect.GetComponent<Outline>();
                    if (outline == null)
                    {
                        outline = settingsPanelRect.gameObject.AddComponent<Outline>();
                    }

                    outline.effectColor = new Color(0.90f, 0.74f, 0.42f, 0.88f);
                    outline.effectDistance = new Vector2(2f * px, -2f * px);

                    var heading = settingsPanelRect.Find("Titulo_Painel")?.GetComponent<Text>();
                    if (heading != null)
                    {
                        heading.fontSize = Mathf.RoundToInt(16f * px);
                        heading.fontStyle = FontStyle.Bold;
                        SetTopLeft(heading.rectTransform, 18f * px, -14f * px, panelWidth - 36f * px, 28f * px);
                    }

                    var volLabel = settingsPanelRect.Find("Rotulo_Volume")?.GetComponent<Text>();
                    if (volLabel != null)
                    {
                        volLabel.fontSize = Mathf.RoundToInt(14f * px);
                        SetTopLeft(volLabel.rectTransform, 18f * px, -52f * px, panelWidth - 36f * px, 24f * px);
                    }

                    if (volumeSliderRect != null)
                    {
                        SetTopLeft(volumeSliderRect, 18f * px, -84f * px, panelWidth - 36f * px, 24f * px);
                    }

                    var closeBtn = settingsPanelRect.Find("Botao_Fechar") as RectTransform;
                    if (closeBtn != null)
                    {
                        SetTopLeft(closeBtn, 18f * px, -132f * px, 140f * px, 36f * px);
                        SetTextSize(closeBtn, Mathf.RoundToInt(15f * px));
                    }
                }
                else
                {
                    settingsPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
                    settingsPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
                    settingsPanelRect.pivot = new Vector2(0.5f, 0.5f);
                    settingsPanelRect.anchoredPosition = Vector2.zero;
                    settingsPanelRect.sizeDelta = new Vector2(820f, 400f);
                    if (volumeSliderRect != null)
                    {
                        volumeSliderRect.sizeDelta = new Vector2(560f, 28f);
                    }
                }
            }

            if (phasePanelRect != null)
            {
                if (isMobile)
                {
                    // Escurece o fundo do dropdown para impedir que fragmentos do menu vazem por tras
                    var backdrop = phasePanelRect.parent != null ? phasePanelRect.parent.Find("Fundo_Clique")?.GetComponent<Image>() : null;
                    if (backdrop != null)
                    {
                        backdrop.color = new Color(0.02f, 0.015f, 0.01f, 0.86f);
                    }

                    float phaseWidth = Mathf.Min(346f * px, maxPanelW);
                    int itemCount = 0;
                    for (int i = 0; i < phasePanelRect.childCount; i++)
                    {
                        if (phasePanelRect.GetChild(i).name.StartsWith("Fase_"))
                        {
                            itemCount++;
                        }
                    }

                    float headerH = 26f * px;
                    float rowH = 36f * px;
                    float totalH = headerH + 16f * px + Mathf.Max(1, itemCount) * rowH + 12f * px;
                    phasePanelRect.anchoredPosition = new Vector2(0f, -56f * px);
                    phasePanelRect.sizeDelta = new Vector2(phaseWidth, totalH);

                    var phaseImg = phasePanelRect.GetComponent<Image>();
                    if (phaseImg != null)
                    {
                        phaseImg.color = new Color(0.06f, 0.042f, 0.032f, 0.99f);
                    }

                    var phaseOutline = phasePanelRect.GetComponent<Outline>();
                    if (phaseOutline != null)
                    {
                        phaseOutline.effectColor = new Color(0.90f, 0.74f, 0.42f, 0.88f);
                        phaseOutline.effectDistance = new Vector2(2f * px, -2f * px);
                    }

                    var header = phasePanelRect.Find("Titulo_Fases")?.GetComponent<Text>();
                    if (header != null)
                    {
                        header.fontSize = Mathf.RoundToInt(14f * px);
                        header.rectTransform.anchoredPosition = new Vector2(0f, -8f * px);
                        header.rectTransform.sizeDelta = new Vector2(-16f * px, headerH);
                    }

                    int rowIdx = 0;
                    for (int i = 0; i < phasePanelRect.childCount; i++)
                    {
                        var child = phasePanelRect.GetChild(i) as RectTransform;
                        if (child == null || !child.name.StartsWith("Fase_"))
                        {
                            continue;
                        }

                        child.anchoredPosition = new Vector2(0f, -(headerH + 12f * px + rowIdx * rowH));
                        child.sizeDelta = new Vector2(-18f * px, rowH - 4f * px);
                        SetTextSize(child, Mathf.RoundToInt(15f * px));
                        rowIdx++;
                    }
                }
                else
                {
                    phasePanelRect.sizeDelta = new Vector2(420f, phasePanelRect.sizeDelta.y);
                }
            }
        }

        private static void SetMenuButtonLayout(
            RectTransform rect,
            float x,
            float y,
            float width,
            float height,
            int fontSize,
            bool topLeft)
        {
            if (rect == null)
            {
                return;
            }

            if (topLeft)
            {
                SetTopLeft(rect, x, y, width, height);
            }
            else
            {
                SetBottomLeft(rect, x, y, width, height);
            }

            SetTextSize(rect, fontSize);
        }

        private static void SetTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetBottomLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetTextSize(RectTransform rect, int fontSize)
        {
            var text = rect.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.fontSize = fontSize;
            }
        }
    }
}
