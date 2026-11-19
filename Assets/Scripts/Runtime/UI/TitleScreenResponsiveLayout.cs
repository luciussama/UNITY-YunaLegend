using UnityEngine;
using UnityEngine.UI;

namespace YunaLegend.UI
{
    /// <summary>
    /// Adapta a tela de entrada entre o layout widescreen (desktop/notebook)
    /// e o layout retrato (mobile), centralizando a menina na poltrona e
    /// reposicionando titulo, menu e rodape sobre um gradiente inferior.
    /// </summary>
    [ExecuteAlways]
    public sealed class TitleScreenResponsiveLayout : MonoBehaviour
    {
        private const float TargetArtAspect = 16f / 9f;
        private const float PortraitFocusNormalizedX = 0.68f;
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
            if (!force
                && Mathf.Approximately(size.x, lastCanvasSize.x)
                && Mathf.Approximately(size.y, lastCanvasSize.y)
                && lastPortraitState == isPortrait)
            {
                return;
            }

            lastCanvasSize = size;
            lastPortraitState = isPortrait;

            ApplyArtFraming(size, isPortrait);
            ApplyOverlayScrims(isPortrait);

            if (isPortrait)
            {
                ApplyPortraitMenu(size);
            }
            else
            {
                ApplyLandscapeMenu();
            }

            ApplyPanels(size, isPortrait);
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

            float artHeight = Mathf.Max(canvasSize.y, canvasSize.x / TargetArtAspect);
            float artWidth = artHeight * TargetArtAspect;
            artContainer.sizeDelta = new Vector2(artWidth, artHeight);

            if (!isPortrait)
            {
                artContainer.anchoredPosition = Vector2.zero;
                return;
            }

            float maxLeftShift = Mathf.Max(0f, 0.5f * (artWidth - canvasSize.x));
            float desiredShift = (0.5f - PortraitFocusNormalizedX) * artWidth;
            float clampedX = Mathf.Clamp(desiredShift, -maxLeftShift, 0f);
            artContainer.anchoredPosition = new Vector2(clampedX, 0f);
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
            }
        }

        private void ApplyLandscapeMenu()
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

        private void ApplyPortraitMenu(Vector2 canvasSize)
        {
            float marginX = Mathf.Max(68f, canvasSize.x * 0.085f);
            float contentWidth = Mathf.Max(320f, canvasSize.x - marginX * 2f);
            float bottomMargin = Mathf.Max(64f, canvasSize.y * 0.045f);

            float footerHeight = 52f;
            float buttonHeight = 76f;
            float buttonSpacing = 12f;
            float easyY = bottomMargin + footerHeight + 22f;
            float loadY = easyY + buttonHeight + buttonSpacing;
            float newY = loadY + buttonHeight + buttonSpacing;
            float titleY = newY + buttonHeight + 28f;

            if (footerRect != null)
            {
                SetBottomLeft(footerRect, marginX, bottomMargin, contentWidth, footerHeight);
                SetTextSize(footerRect, 26);
            }

            SetMenuButtonLayout(easyModeRect, marginX, easyY, Mathf.Min(360f, contentWidth * 0.56f), buttonHeight, 48, topLeft: false);

            if (easyTagRect != null)
            {
                float tagOffsetX = marginX + Mathf.Min(280f, contentWidth * 0.38f);
                SetBottomLeft(easyTagRect, tagOffsetX, easyY, Mathf.Max(180f, contentWidth - (tagOffsetX - marginX)), buttonHeight);
                SetTextSize(easyTagRect, 28);
            }

            SetMenuButtonLayout(loadGameRect, marginX, loadY, contentWidth, buttonHeight, 48, topLeft: false);
            SetMenuButtonLayout(newGameRect, marginX, newY, contentWidth, buttonHeight, 48, topLeft: false);

            if (titleText != null)
            {
                float titleHeight = 220f;
                SetBottomLeft(titleText.rectTransform, marginX, titleY, contentWidth, titleHeight);
                titleText.fontSize = Mathf.Clamp(Mathf.RoundToInt(canvasSize.x * 0.086f), 64, 92);
                titleText.lineSpacing = 0.96f;
            }

            if (focusMarker != null)
            {
                focusMarker.fontSize = 46;
                if (newGameRect != null)
                {
                    focusMarker.rectTransform.position = newGameRect.TransformPoint(
                        new Vector3(newGameRect.rect.xMin - 34f, newGameRect.rect.center.y, 0f));
                }
            }

            if (settingsButtonRect != null)
            {
                settingsButtonRect.anchoredPosition = new Vector2(-42f, -44f);
                settingsButtonRect.sizeDelta = new Vector2(84f, 84f);
            }

            if (flagButtonRect != null)
            {
                flagButtonRect.anchoredPosition = new Vector2(0f, -28f);
                flagButtonRect.sizeDelta = new Vector2(80f, 100f);
            }
        }

        private void ApplyPanels(Vector2 canvasSize, bool isPortrait)
        {
            if (settingsPanelRect != null)
            {
                float panelWidth = isPortrait ? Mathf.Min(820f, canvasSize.x - 72f) : 820f;
                settingsPanelRect.sizeDelta = new Vector2(panelWidth, 400f);

                if (volumeSliderRect != null)
                {
                    volumeSliderRect.sizeDelta = new Vector2(Mathf.Min(560f, panelWidth - 80f), 28f);
                }
            }

            if (phasePanelRect != null)
            {
                float phaseWidth = isPortrait ? Mathf.Min(420f, canvasSize.x - 80f) : 420f;
                phasePanelRect.sizeDelta = new Vector2(phaseWidth, phasePanelRect.sizeDelta.y);
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
