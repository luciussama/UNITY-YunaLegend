using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using YunaLegend.DreamForest.Core;
using YunaLegend.Presentation;

namespace YunaLegend.DreamForest.Runtime
{
    public sealed class DreamForestHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action<bool> OnStateChanged;
        public void OnPointerDown(PointerEventData eventData) => OnStateChanged?.Invoke(true);
        public void OnPointerUp(PointerEventData eventData) => OnStateChanged?.Invoke(false);
        public void OnPointerExit(PointerEventData eventData) => OnStateChanged?.Invoke(false);
    }

    /// <summary>
    /// SRP HUD &amp; Dialogue controller for DreamForest_Gameplay.
    /// Follows UI/UX standards: desktop dialogue balloon anchored in the top-center band off Yuna/art,
    /// message filling the entire balloon, zero verbose click/tap-to-continue text, only a subtle corner tap icon.
    /// </summary>
    public sealed class DreamForestHudController : MonoBehaviour
    {
        public event Action AdvanceRequested;
        public event Action PauseRequested;
        public event Action MuteRequested;
        public event Action RetryRequested;
        public event Action MenuRequested;

        public float TouchAxisX => (touchRight ? 1f : 0f) - (touchLeft ? 1f : 0f);
        public float TouchAxisY => (touchDown ? 1f : 0f) - (touchUp ? 1f : 0f);
        public bool TouchJumpHeld => touchJump;

        private bool touchLeft;
        private bool touchRight;
        private bool touchUp;
        private bool touchDown;
        private bool touchJump;

        private Text sectionText;
        private Text progressText;
        private Text muteLabel;

        private GameObject dialogueRoot;
        private Text dialogueSpeaker;
        private Text dialogueBody;

        private GameObject modalRoot;
        private Text modalTitle;
        private Text modalBody;

        private GameObject touchControlsRoot;

        public void Build()
        {
            EnsureEventSystem();
            var canvasGo = new GameObject("DreamForestHudCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            // Top-left status bar
            var topBar = CreateCard("TopBar", canvasGo.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(310f, 64f));
            sectionText = CreateText("Section", topBar.transform, "Jardim dos Sonhos", 17, HudTheme.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            SetInset(sectionText.rectTransform, 12f, 30f, 12f, 8f);
            progressText = CreateText("Progress", topBar.transform, "0%", 15, HudTheme.Plum, TextAnchor.LowerLeft, FontStyle.Normal);
            SetInset(progressText.rectTransform, 12f, 8f, 12f, 34f);

            // Top-right Mute & Pause buttons
            var muteBtn = CreateButton("MuteBtn", canvasGo.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-118f, -14f), new Vector2(88f, 44f), "Som", () => MuteRequested?.Invoke());
            muteLabel = muteBtn.GetComponentInChildren<Text>();
            CreateButton("Pause", canvasGo.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -14f), new Vector2(92f, 44f), "Pausa", () => PauseRequested?.Invoke());

            // Top-center DialogueBox (off Yuna/art on desktop, message fills entire balloon, only discreet corner tap icon)
            var dlgImg = CreateCard("DialogueBox", canvasGo.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(560f, 114f));
            dlgImg.raycastTarget = true;
            dialogueRoot = dlgImg.gameObject;
            var dlgBtn = dialogueRoot.AddComponent<Button>();
            dlgBtn.transition = Selectable.Transition.None;
            dlgBtn.onClick.AddListener(() => AdvanceRequested?.Invoke());

            dialogueSpeaker = CreateText("Speaker", dialogueRoot.transform, "", 16, HudTheme.Sage, TextAnchor.UpperLeft, FontStyle.Bold);
            SetInset(dialogueSpeaker.rectTransform, 14f, 84f, 20f, 8f);
            dialogueBody = CreateText("Body", dialogueRoot.transform, "", 18, HudTheme.Ink, TextAnchor.MiddleLeft, FontStyle.Italic);
            SetInset(dialogueBody.rectTransform, 14f, 10f, 34f, 30f);
            HudTheme.EnsureCornerTapCue(dialogueRoot.transform, 20f, 10f, 8f, new Color(HudTheme.Plum.r, HudTheme.Plum.g, HudTheme.Plum.b, 0.78f));
            dialogueRoot.SetActive(false);

            // Multitouch mobile controls
            touchControlsRoot = new GameObject("TouchControls", typeof(RectTransform));
            touchControlsRoot.transform.SetParent(canvasGo.transform, false);
            var tcRt = touchControlsRoot.GetComponent<RectTransform>();
            tcRt.anchorMin = Vector2.zero;
            tcRt.anchorMax = Vector2.one;
            tcRt.offsetMin = Vector2.zero;
            tcRt.offsetMax = Vector2.zero;

            CreateHold("BtnLeft", touchControlsRoot.transform, new Vector2(0f, 0f), new Vector2(18f, 20f), new Vector2(76f, 72f), "◀", v => touchLeft = v);
            CreateHold("BtnRight", touchControlsRoot.transform, new Vector2(0f, 0f), new Vector2(106f, 20f), new Vector2(76f, 72f), "▶", v => touchRight = v);
            CreateHold("BtnUp", touchControlsRoot.transform, new Vector2(0f, 0f), new Vector2(62f, 100f), new Vector2(76f, 62f), "▲", v => touchUp = v);
            CreateHold("BtnDown", touchControlsRoot.transform, new Vector2(1f, 0f), new Vector2(-194f, 20f), new Vector2(82f, 72f), "▼", v => touchDown = v);
            CreateHold("BtnJump", touchControlsRoot.transform, new Vector2(1f, 0f), new Vector2(-102f, 20f), new Vector2(84f, 72f), "PULAR", v => touchJump = v);
            touchControlsRoot.SetActive(Application.isMobilePlatform);

            // Modal (Pause / GameOver / Complete)
            var modalImg = CreateCard("Modal", canvasGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460f, 230f));
            modalRoot = modalImg.gameObject;
            modalTitle = CreateText("ModalTitle", modalRoot.transform, "", 24, HudTheme.Ink, TextAnchor.UpperCenter, FontStyle.Bold);
            SetInset(modalTitle.rectTransform, 16f, 168f, 16f, 18f);
            modalBody = CreateText("ModalBody", modalRoot.transform, "", 18, HudTheme.Plum, TextAnchor.MiddleCenter, FontStyle.Normal);
            SetInset(modalBody.rectTransform, 16f, 82f, 16f, 64f);
            CreateButton("RetryBtn", modalRoot.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 18f), new Vector2(196f, 48f), "Continuar / Tentar", () => RetryRequested?.Invoke());
            CreateButton("MenuBtn", modalRoot.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-20f, 18f), new Vector2(196f, 48f), "Menu", () => MenuRequested?.Invoke());
            modalRoot.SetActive(false);
        }

        public void Apply(DreamForestSimulation sim, bool paused, bool muted)
        {
            if (sectionText != null) sectionText.text = sim.ActiveSectionName();
            if (progressText != null) progressText.text = ResolveContextSubtitle(sim);
            if (muteLabel != null) muteLabel.text = muted ? "Mudo" : "Som";

            string speaker = string.Empty;
            string body = string.Empty;
            bool hasDialogue = !paused && sim.Stage != DreamForestStage.GameOver && sim.TryGetActiveDialogue(out speaker, out body);
            if (dialogueRoot != null)
            {
                dialogueRoot.SetActive(hasDialogue);
                if (hasDialogue)
                {
                    dialogueSpeaker.text = speaker;
                    dialogueBody.text = body;
                }
            }

            if (modalRoot != null)
            {
                bool showModal = paused || sim.Stage == DreamForestStage.GameOver || sim.Stage == DreamForestStage.Complete;
                modalRoot.SetActive(showModal);
                if (showModal)
                {
                    if (paused)
                    {
                        modalTitle.text = "Jardim dos Sonhos — Pausa";
                        modalBody.text = "Escolha continuar ou voltar ao menu principal.";
                    }
                    else if (sim.Stage == DreamForestStage.Complete)
                    {
                        modalTitle.text = "O Pesadelo Passou!";
                        modalBody.text = "Yuna acordou segura ao lado de Nanda sob o sol.";
                    }
                    else
                    {
                        modalTitle.text = "Yuna caiu no sonho...";
                        modalBody.text = "Volte ao último apoio seguro e tente novamente.";
                    }
                }
            }
        }

        private static string ResolveContextSubtitle(DreamForestSimulation sim)
        {
            int pct = Mathf.RoundToInt(sim.Progress * 100f);
            if (sim.Nightmare != null)
            {
                var n = sim.Nightmare;
                if (sim.Stage == DreamForestStage.NightmareFinal && sim.Age < 4f) return "Não havia mais onde se esconder.";
                if (sim.Stage == DreamForestStage.NightmareStrike && sim.Age > 0.72f) return "A luz encontrou Yuna.";
                if (sim.Stage == DreamForestStage.Stealth)
                {
                    if (n.MudMode == DreamForestMudMode.Climb) return "Apoie as mãos e suba a margem.";
                    if (n.MudMode == DreamForestMudMode.Enter) return "Pule para dentro da lama.";
                    if (n.MudMode == DreamForestMudMode.Swim)
                        return n.MudDepth > 0.94f ? "Sob a lama. Nade com ◀/▶; na margem, ↑ ou Espaço." : "Nade com ◀/▶. Pare ou ↓ para mergulhar.";
                    return n.Exposure <= 0.001f ? "Junto ao abrigo. Espere a luz passar." : "Avance de abrigo em abrigo. Espere a luz passar.";
                }
                return $"{pct}%";
            }

            if (sim.Stage == DreamForestStage.Gliding) return "▲/▼ ou W/S: antecipe a curva na folha.";
            if (sim.Stage == DreamForestStage.GlideFalling) return "Os espinhos rasgaram o vento…";
            if (sim.Stage == DreamForestStage.Falling)
                return sim.FallCause == "spider" ? "A aranha alcançou Yuna!" : "O sonho perdeu o chão…";
            return $"{pct}%";
        }

        private static Image CreateCard(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            HudTheme.Panel(img);
            return img;
        }

        private static Text CreateText(string name, Transform parent, string content, int size, Color color, TextAnchor align, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var txt = go.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = content;
            txt.fontSize = size;
            txt.color = color;
            txt.alignment = align;
            txt.fontStyle = style;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, string label, Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());
            var txt = CreateText("Label", go.transform, label, 16, HudTheme.Paper, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetInset(txt.rectTransform, 4f, 4f, 4f, 4f);
            HudTheme.Button(btn);
            return btn;
        }

        private static void CreateHold(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, string label, Action<bool> onChanged)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(DreamForestHoldButton));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            HudTheme.Round(img);
            img.color = HudTheme.Plum;
            var hold = go.GetComponent<DreamForestHoldButton>();
            hold.OnStateChanged = onChanged;
            var txt = CreateText("Label", go.transform, label, 18, HudTheme.Paper, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetInset(txt.rectTransform, 2f, 2f, 2f, 2f);
        }

        private static void SetInset(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
    }
}
