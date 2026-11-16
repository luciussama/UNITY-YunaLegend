using UnityEngine;
using UnityEngine.UI;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Controla a interface da Toy Room (contador X/8, prompt contextual, caixa vitoriana e narracao poetica).
    /// </summary>
    public sealed class ToyRoomHudController : MonoBehaviour
    {
        private Text counterText;
        private Image progressFill;
        private GameObject promptRoot;
        private Text promptText;
        private GameObject dialogueRoot;
        private Text speakerText;
        private Text dialogueBodyText;
        private GameObject poeticRoot;
        private Text poeticText;

        private bool initialized;

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
            EnsureBuilt();
            bool visible = !string.IsNullOrEmpty(label);
            promptRoot.SetActive(visible);
            if (visible)
            {
                promptText.text = label;
            }
        }

        public void ShowBeat(ToyRoomNarrativeBeat beat)
        {
            EnsureBuilt();

            if (beat.Kind == ToyRoomBeatKind.Dialogue)
            {
                dialogueRoot.SetActive(true);
                poeticRoot.SetActive(false);
                speakerText.text = beat.Speaker == ToyRoomSpeaker.Nanda ? "NANDA" : "MENINA";
                speakerText.color = beat.Speaker == ToyRoomSpeaker.Nanda
                    ? new Color(0.42f, 0.88f, 1.0f)
                    : new Color(1.0f, 0.85f, 0.65f);
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

        private void EnsureBuilt()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGo = new GameObject("ToyRoom_HUD_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            BuildCounterPanel(canvasGo.transform, font);
            BuildActionPrompt(canvasGo.transform, font);
            BuildDialogueBox(canvasGo.transform, font);
            BuildPoeticOverlay(canvasGo.transform, font);

            UpdateToyCounter(0);
            SetActionPrompt(null);
            ClearNarrativeOverlays();
        }

        private void BuildCounterPanel(Transform parent, Font font)
        {
            var panel = CreatePanel("CounterPanel", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -32f), new Vector2(360f, 74f), new Color(0.10f, 0.08f, 0.16f, 0.88f));
            counterText = CreateLabel("CounterText", panel.transform, font, 22, TextAnchor.UpperLeft, new Color(0.99f, 0.96f, 0.82f));
            var rt = counterText.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(16f, 26f);
            rt.offsetMax = new Vector2(-16f, -10f);

            var barBg = CreatePanel("ProgressBg", panel.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 12f), new Vector2(-32f, 12f), new Color(0.22f, 0.20f, 0.30f, 1f));
            var barBgRt = barBg.GetComponent<RectTransform>();
            barBgRt.pivot = new Vector2(0f, 0f);
            barBgRt.anchorMin = new Vector2(0f, 0f);
            barBgRt.anchorMax = new Vector2(1f, 0f);
            barBgRt.anchoredPosition = new Vector2(16f, 12f);
            barBgRt.sizeDelta = new Vector2(-32f, 10f);

            var fillGo = new GameObject("ProgressFill");
            fillGo.transform.SetParent(barBg.transform, false);
            progressFill = fillGo.AddComponent<Image>();
            progressFill.color = new Color(0.45f, 0.85f, 0.58f, 1f);
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            var fillRt = progressFill.rectTransform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
        }

        private void BuildActionPrompt(Transform parent, Font font)
        {
            promptRoot = CreatePanel("ActionPrompt", parent, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-42f, 42f), new Vector2(170f, 70f), new Color(0.92f, 0.72f, 0.24f, 0.95f));
            var rt = promptRoot.GetComponent<RectTransform>();
            rt.pivot = new Vector2(1f, 0f);

            promptText = CreateLabel("PromptText", promptRoot.transform, font, 24, TextAnchor.MiddleCenter, new Color(0.14f, 0.10f, 0.06f));
            promptText.fontStyle = FontStyle.Bold;
            var textRt = promptText.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
        }

        private void BuildDialogueBox(Transform parent, Font font)
        {
            dialogueRoot = CreatePanel("DialogueBox", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 44f), new Vector2(960f, 150f), new Color(0.10f, 0.08f, 0.16f, 0.94f));
            var rt = dialogueRoot.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0f);

            speakerText = CreateLabel("SpeakerText", dialogueRoot.transform, font, 22, TextAnchor.UpperLeft, new Color(0.42f, 0.88f, 1.0f));
            speakerText.fontStyle = FontStyle.Bold;
            var sRt = speakerText.rectTransform;
            sRt.anchorMin = new Vector2(0f, 1f);
            sRt.anchorMax = new Vector2(1f, 1f);
            sRt.pivot = new Vector2(0.5f, 1f);
            sRt.anchoredPosition = new Vector2(0f, -14f);
            sRt.sizeDelta = new Vector2(-48f, 34f);

            dialogueBodyText = CreateLabel("DialogueBody", dialogueRoot.transform, font, 26, TextAnchor.MiddleLeft, new Color(0.99f, 0.98f, 0.80f));
            dialogueBodyText.fontStyle = FontStyle.Italic;
            var bRt = dialogueBodyText.rectTransform;
            bRt.anchorMin = Vector2.zero;
            bRt.anchorMax = Vector2.one;
            bRt.offsetMin = new Vector2(28f, 18f);
            bRt.offsetMax = new Vector2(-28f, -48f);
        }

        private void BuildPoeticOverlay(Transform parent, Font font)
        {
            poeticRoot = new GameObject("PoeticNarration");
            poeticRoot.transform.SetParent(parent, false);
            var rt = poeticRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 80f);
            rt.sizeDelta = new Vector2(1080f, 140f);

            poeticText = CreateLabel("PoeticText", poeticRoot.transform, font, 30, TextAnchor.MiddleCenter, new Color(0.99f, 0.97f, 0.88f));
            poeticText.fontStyle = FontStyle.Italic;
            var tRt = poeticText.rectTransform;
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(anchorMin.x, anchorMin.y);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = bgColor;
            return go;
        }

        private static Text CreateLabel(string name, Transform parent, Font font, int size, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }
    }
}
