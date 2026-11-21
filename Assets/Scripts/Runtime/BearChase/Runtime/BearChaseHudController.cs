using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Interface da Fase 4: barras de cinema na entrada, crossfade, coracoes rosa no topo,
    /// botoes circulares no canto superior direito, dica sutil de desvio,
    /// banner de queda, modal de tentativa/modo facil e modal de vitoria.
    /// </summary>
    public sealed class BearChaseHudController : MonoBehaviour
    {
        private Canvas canvas;
        private Font uiFont;

        private RectTransform topBar;
        private RectTransform bottomBar;
        private Image fadeOverlay;
        private RectTransform heartsRoot;
        private readonly List<Image> heartImages = new List<Image>();

        private Text hintText;
        private GameObject fallenBannerRoot;
        private GameObject modalRoot;
        private Text modalTitle;
        private Text modalBody;
        private Button primaryButton;
        private Text primaryButtonLabel;
        private Button easyButton;
        private Button titleButton;
        private Button pauseButton;
        private Button muteButton;
        private Text muteLabel;

        private bool isEasyOfferActive;
        private bool isVictoryState;

        public event Action OnPauseToggled;
        public event Action OnMuteToggled;
        public event Action<bool> OnRetryRequested;
        public event Action OnReturnToTitleRequested;

        public void ConfigureFont(Font font)
        {
            uiFont = font;
        }

        private void Awake()
        {
            BuildUi();
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

            var root = canvasGo.transform;

            // Barras letterbox da cena de entrada
            topBar = CreateRect("TopLetterbox", root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 76f));
            topBar.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.02f, 0.02f, 0.96f);

            bottomBar = CreateRect("BottomLetterbox", root, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 76f));
            bottomBar.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.02f, 0.02f, 0.96f);

            // Coracoes no topo central
            heartsRoot = CreateRect("HeartsRow", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(360f, 44f));
            var hlg = heartsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 10f;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            heartsRoot.gameObject.SetActive(false);

            // Botoes circulares no canto superior direito (Inicio, Pausa, Som)
            var controlsRoot = CreateRect("TopRightControls", root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -24f), new Vector2(210f, 48f));
            var clg = controlsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            clg.childAlignment = TextAnchor.MiddleRight;
            clg.spacing = 10f;
            clg.childControlWidth = false;
            clg.childControlHeight = false;

            CreateTopCircleButton("Inicio", "Início", controlsRoot, () => OnReturnToTitleRequested?.Invoke());
            pauseButton = CreateTopCircleButton("Pausa", "P", controlsRoot, () => OnPauseToggled?.Invoke());
            muteButton = CreateTopCircleButton("Som", "Som", controlsRoot, () => OnMuteToggled?.Invoke());
            muteLabel = muteButton.GetComponentInChildren<Text>(true);

            // Dica sutil de controle na corrida
            var hintRt = CreateRect("DodgeHint", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(540f, 44f));
            hintText = hintRt.gameObject.AddComponent<Text>();
            hintText.font = uiFont;
            hintText.fontSize = 26;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.color = new Color(0.98f, 0.94f, 0.84f, 0.88f);
            hintText.text = "← A · desvie · D →";
            hintRt.gameObject.SetActive(false);

            // Banner de queda ("A menina caiu.")
            var bannerRt = CreateRect("FallenBanner", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(760f, 92f));
            bannerRt.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.04f, 0.04f, 0.88f);
            fallenBannerRoot = bannerRt.gameObject;
            var bannerTextRt = CreateRect("BannerText", bannerRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var bannerText = bannerTextRt.gameObject.AddComponent<Text>();
            bannerText.font = uiFont;
            bannerText.fontSize = 28;
            bannerText.alignment = TextAnchor.MiddleCenter;
            bannerText.color = new Color(0.98f, 0.92f, 0.80f);
            bannerText.text = "A menina caiu.\nTente novamente pelos botões da tela.";
            fallenBannerRoot.SetActive(false);

            // Modal central (Retry / Modo Facil / Vitoria)
            BuildModal(root);

            // Overlay de crossfade escuro por cima da cena (abaixo de modais/botoes)
            var fadeRt = CreateRect("CrossfadeOverlay", root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            fadeRt.SetSiblingIndex(2);
            fadeOverlay = fadeRt.gameObject.AddComponent<Image>();
            fadeOverlay.color = new Color(0.03f, 0.02f, 0.02f, 0f);
            fadeOverlay.raycastTarget = false;
        }

        private void BuildModal(Transform root)
        {
            var bgRt = CreateRect("CenterModal", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 360f));
            bgRt.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.05f, 0.05f, 0.94f);
            var outline = bgRt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.86f, 0.72f, 0.45f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
            modalRoot = bgRt.gameObject;

            var titleRt = CreateRect("Title", bgRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(620f, 52f));
            modalTitle = titleRt.gameObject.AddComponent<Text>();
            modalTitle.font = uiFont;
            modalTitle.fontSize = 34;
            modalTitle.alignment = TextAnchor.MiddleCenter;
            modalTitle.color = new Color(1f, 0.86f, 0.56f);

            var bodyRt = CreateRect("Body", bgRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 22f), new Vector2(600f, 110f));
            modalBody = bodyRt.gameObject.AddComponent<Text>();
            modalBody.font = uiFont;
            modalBody.fontSize = 25;
            modalBody.alignment = TextAnchor.MiddleCenter;
            modalBody.color = new Color(0.96f, 0.92f, 0.84f);

            var btnRow = CreateRect("Buttons", bgRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(600f, 64f));
            var rowLayout = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.spacing = 18f;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = false;

            primaryButton = CreateModalButton("PrimaryBtn", "Tentar de novo", btnRow, () =>
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

            titleButton = CreateModalButton("TitleBtn", "Voltar ao início", btnRow, () =>
            {
                OnReturnToTitleRequested?.Invoke();
            }, out _);

            modalRoot.SetActive(false);
        }

        public void SetHearts(int current, int max)
        {
            if (heartsRoot == null)
            {
                return;
            }

            while (heartImages.Count < max)
            {
                var rt = CreateRect($"Heart_{heartImages.Count}", heartsRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(36f, 36f));
                var text = rt.gameObject.AddComponent<Text>();
                text.font = uiFont;
                text.fontSize = 32;
                text.alignment = TextAnchor.MiddleCenter;
                text.text = "♥";
                var img = rt.gameObject.AddComponent<Image>();
                img.color = Color.clear;
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

                var txt = heartImages[i].GetComponent<Text>();
                if (txt != null)
                {
                    txt.color = i < current
                        ? new Color(0.98f, 0.42f, 0.62f, 1f)
                        : new Color(0.32f, 0.24f, 0.28f, 0.65f);
                }
            }
        }

        public void UpdateVisualState(BearChaseSimulation sim)
        {
            if (sim == null)
            {
                return;
            }

            bool isEntrance = sim.Phase == BearChasePhase.EntranceWalk
                || sim.Phase == BearChasePhase.EntranceNudge
                || sim.Phase == BearChasePhase.BearWakeUp
                || sim.Phase == BearChasePhase.TransitionToChase;

            if (topBar != null) topBar.gameObject.SetActive(isEntrance);
            if (bottomBar != null) bottomBar.gameObject.SetActive(isEntrance);
            if (heartsRoot != null) heartsRoot.gameObject.SetActive(!isEntrance);
            if (hintText != null) hintText.gameObject.SetActive(sim.Phase == BearChasePhase.Chasing && sim.TotalChaseTime < 6.5f);

            if (fadeOverlay != null)
            {
                float alpha = 0f;
                if (sim.Phase == BearChasePhase.TransitionToChase)
                {
                    alpha = Mathf.Clamp01(sim.PhaseTimer / BearChaseConfig.TransitionDuration);
                }
                else if (sim.Phase == BearChasePhase.Chasing && sim.TotalChaseTime < 0.45f)
                {
                    alpha = 1f - Mathf.Clamp01(sim.TotalChaseTime / 0.45f);
                }

                fadeOverlay.color = new Color(0.03f, 0.02f, 0.02f, alpha);
            }

            if (fallenBannerRoot != null)
            {
                fallenBannerRoot.SetActive(sim.Phase == BearChasePhase.Fallen);
            }

            if (modalRoot != null)
            {
                if (sim.Phase == BearChasePhase.RetryModal)
                {
                    isVictoryState = false;
                    modalRoot.SetActive(true);
                    modalTitle.text = "✦ PERSEGUIÇÃO AO URSO ✦";
                    modalBody.text = isEasyOfferActive
                        ? "A menina não alcançou o urso. Deseja tentar no fácil?\nA fadinha oferece 6 corações para ajudar."
                        : "A menina não alcançou o urso. Tentar novamente?\nA fadinha está pronta para acompanhar uma nova tentativa.";
                    primaryButtonLabel.text = "Tentar de novo";
                    easyButton.gameObject.SetActive(isEasyOfferActive);
                }
                else if (sim.Phase == BearChasePhase.Victory)
                {
                    isVictoryState = true;
                    isEasyOfferActive = false;
                    modalRoot.SetActive(true);
                    modalTitle.text = "✦ URSO ALCANÇADO! ✦";
                    modalBody.text = "A menina alcançou o urso fujão pelo corredor do quarto.\nA fadinha brilha satisfeita ao seu lado.";
                    primaryButtonLabel.text = "Voltar ao início";
                    easyButton.gameObject.SetActive(false);
                }
                else
                {
                    modalRoot.SetActive(false);
                }
            }
        }

        public void ShowEasyModeOffer()
        {
            isEasyOfferActive = true;
        }

        public void SetMuted(bool muted)
        {
            if (muteLabel != null)
            {
                muteLabel.text = muted ? "Mudo" : "Som";
            }
        }

        private Button CreateTopCircleButton(string name, string label, Transform parent, Action onClick)
        {
            var rt = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(58f, 40f));
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.09f, 0.07f, 0.07f, 0.85f);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            var txtRt = CreateRect("Label", rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var txt = txtRt.gameObject.AddComponent<Text>();
            txt.font = uiFont;
            txt.fontSize = 18;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.96f, 0.90f, 0.78f);
            txt.text = label;
            return btn;
        }

        private Button CreateModalButton(string name, string label, Transform parent, Action onClick, out Text labelText)
        {
            var rt = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(182f, 52f));
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.22f, 0.14f, 0.11f, 0.98f);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            var txtRt = CreateRect("Label", rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
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
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return rt;
        }
    }
}
