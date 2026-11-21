using System;
using System.Collections.Generic;
using UnityEngine;
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
        private RectTransform topBar;
        private RectTransform bottomBar;
        private Image fadeOverlay;
        private RectTransform heartsRoot;
        private readonly List<Image> heartImages = new List<Image>();
        private Sprite heartSprite;
        private Texture2D heartTexture;

        private GameObject hintRoot;
        private GameObject pauseBadgeRoot;
        private GameObject fallenBannerRoot;
        private GameObject modalRoot;
        private Text modalTitle;
        private Text modalBody;
        private Text primaryButtonLabel;
        private Button easyButton;
        private Button pauseButton;
        private Text pauseLabel;
        private Button muteButton;
        private Text muteLabel;

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
        }

        private void Awake()
        {
            EnsureBuilt();
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
            RectTransform controlsRoot = CreateRect("TopRightControls", root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -22f), new Vector2(220f, 48f));
            var clg = controlsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            clg.childAlignment = TextAnchor.MiddleRight;
            clg.spacing = 10f;
            clg.childControlWidth = false;
            clg.childControlHeight = false;

            CreateTopCircleButton("Inicio", "Início", controlsRoot, () => OnReturnToTitleRequested?.Invoke());
            pauseButton = CreateTopCircleButton("Pausa", "P", controlsRoot, () => OnPauseToggled?.Invoke());
            pauseLabel = pauseButton.GetComponentInChildren<Text>(true);
            muteButton = CreateTopCircleButton("Som", "Som", controlsRoot, () => OnMuteToggled?.Invoke());
            muteLabel = muteButton.GetComponentInChildren<Text>(true);
        }

        private void BuildBottomHintAndBanners(Transform root)
        {
            RectTransform hintPanel = CreateRect("DodgeHintBar", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(760f, 68f));
            var hintBg = hintPanel.gameObject.AddComponent<Image>();
            hintBg.color = new Color(0.06f, 0.04f, 0.04f, 0.58f);
            hintBg.raycastTarget = false;
            hintRoot = hintPanel.gameObject;

            RectTransform hintTextRt = CreateRect("DodgeHintText", hintPanel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Text hintText = hintTextRt.gameObject.AddComponent<Text>();
            hintText.font = uiFont;
            hintText.fontSize = 24;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.color = new Color(0.98f, 0.94f, 0.84f, 0.94f);
            hintText.text = "← A · desvie · D →\nToque e segure nas laterais · P pausa · M som";
            hintText.raycastTarget = false;
            hintRoot.SetActive(false);

            RectTransform pauseRt = CreateRect("PauseBadge", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360f, 88f));
            var pauseBg = pauseRt.gameObject.AddComponent<Image>();
            pauseBg.color = new Color(0.06f, 0.04f, 0.04f, 0.86f);
            pauseBg.raycastTarget = false;
            pauseBadgeRoot = pauseRt.gameObject;

            RectTransform pauseTxtRt = CreateRect("PauseText", pauseRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Text pauseTxt = pauseTxtRt.gameObject.AddComponent<Text>();
            pauseTxt.font = uiFont;
            pauseTxt.fontSize = 36;
            pauseTxt.alignment = TextAnchor.MiddleCenter;
            pauseTxt.color = new Color(1f, 0.88f, 0.58f);
            pauseTxt.text = "PAUSADO";
            pauseBadgeRoot.SetActive(false);

            RectTransform bannerRt = CreateRect("FallenBanner", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(760f, 92f));
            bannerRt.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.04f, 0.04f, 0.88f);
            fallenBannerRoot = bannerRt.gameObject;

            RectTransform bannerTextRt = CreateRect("BannerText", bannerRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Text bannerText = bannerTextRt.gameObject.AddComponent<Text>();
            bannerText.font = uiFont;
            bannerText.fontSize = 28;
            bannerText.alignment = TextAnchor.MiddleCenter;
            bannerText.color = new Color(0.98f, 0.92f, 0.80f);
            bannerText.text = "A menina caiu.\nTente novamente pelos botões da tela.";
            fallenBannerRoot.SetActive(false);
        }

        private void BuildModal(Transform root)
        {
            RectTransform bgRt = CreateRect("CenterModal", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 360f));
            bgRt.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.05f, 0.05f, 0.94f);
            Outline outline = bgRt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.86f, 0.72f, 0.45f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
            modalRoot = bgRt.gameObject;

            RectTransform titleRt = CreateRect("Title", bgRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(620f, 52f));
            modalTitle = titleRt.gameObject.AddComponent<Text>();
            modalTitle.font = uiFont;
            modalTitle.fontSize = 34;
            modalTitle.alignment = TextAnchor.MiddleCenter;
            modalTitle.color = new Color(1f, 0.86f, 0.56f);

            RectTransform bodyRt = CreateRect("Body", bgRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 22f), new Vector2(600f, 110f));
            modalBody = bodyRt.gameObject.AddComponent<Text>();
            modalBody.font = uiFont;
            modalBody.fontSize = 25;
            modalBody.alignment = TextAnchor.MiddleCenter;
            modalBody.color = new Color(0.96f, 0.92f, 0.84f);

            RectTransform btnRow = CreateRect("Buttons", bgRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(600f, 64f));
            var rowLayout = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.spacing = 18f;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = false;

            CreateModalButton("PrimaryBtn", "Tentar de novo", btnRow, () =>
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

            easyButton = CreateModalButton("EasyBtn", "Modo Fácil (6 ♥)", btnRow, () =>
            {
                OnRetryRequested?.Invoke(true);
            }, out _);
            easyButton.gameObject.SetActive(false);

            CreateModalButton("TitleBtn", "Voltar ao início", btnRow, () =>
            {
                OnReturnToTitleRequested?.Invoke();
            }, out _);

            modalRoot.SetActive(false);
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
