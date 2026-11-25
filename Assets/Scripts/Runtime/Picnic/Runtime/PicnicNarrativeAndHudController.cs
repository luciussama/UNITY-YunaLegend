using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YunaLegend.Picnic.Core;
using YunaLegend.Presentation;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>
    /// Renders the legacy-faithful Opening and Ending cinematics (using house-without-actors-v1,
    /// picnic-without-yuna-v1, cinematic-acting-v1, yuna-sleeping-v1, and blue Nanda) plus clean gameplay HUD.
    /// </summary>
    public sealed class PicnicNarrativeAndHudController : MonoBehaviour
    {
        public event Action AdvanceRequested;
        public event Action SkipRequested;
        public event Action RetryRequested;
        public event Action MenuRequested;
        public event Action PauseRequested;

        public float TouchAxis { get; private set; }
        private bool touchJumpQueued;

        private GameObject storyCanvas;
        private Image storyBg;
        private Image storyYuna;
        private Image storyYunaPair;
        private Image storyNanda;
        private Text storySpeaker;
        private Text storyBody;

        private GameObject hudCanvas;
        private Text hudStatus;
        private Text hudHearts;
        private GameObject modalPanel;
        private Text modalTitle;
        private Text modalBody;
        private Font uiFont;
        private RectTransform hudSafeArea;
        private RectTransform storySafeArea;
        private Vector2 lastLayoutSize;
        private Rect lastSafeArea;
        private float forcedWidth = -1f;
        private float forcedHeight = -1f;

        private int lastHudPct = -1;
        private string lastHudSection;
        private int lastHudHp = -1;
        private PicnicDifficulty lastHudDifficulty;
        private int lastModalMode = -1;

        public bool ConsumeTouchJump()
        {
            bool v = touchJumpQueued;
            touchJumpQueued = false;
            return v;
        }

        public void Build()
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();
            BuildStoryUi();
            BuildHudUi();
            RefreshLayout(true);
        }

        public void SetViewportOverride(float width, float height)
        {
            forcedWidth = width;
            forcedHeight = height;
            RefreshLayout(true);
        }

        private void LateUpdate()
        {
            RefreshLayout(false);
        }

        public void Apply(PicnicSimulation sim, bool paused)
        {
            bool showStory = sim.IsReadingStory || (sim.Stage == PicnicStage.Complete && sim.EndingDone);
            bool visibilityChanged = storyCanvas.activeSelf != showStory || hudCanvas.activeSelf == showStory;
            if (storyCanvas.activeSelf != showStory) storyCanvas.SetActive(showStory);
            if (hudCanvas.activeSelf == showStory) hudCanvas.SetActive(!showStory);
            if (visibilityChanged)
            {
                Canvas.ForceUpdateCanvases();
            }
            RefreshLayout(visibilityChanged);

            if (showStory)
            {
                bool ending = sim.Stage == PicnicStage.Complete;
                int idx = ending ? sim.EndingIndex : sim.OpeningIndex;
                var state = PicnicNarrativeData.Evaluate(ending, idx, sim.CardAge, sim.Age, sim.Time, Screen.height > Screen.width);
                if (storySpeaker.text != state.Card.Speaker) storySpeaker.text = state.Card.Speaker;
                if (storyBody.text != state.Card.Text) storyBody.text = state.Card.Text;
                var bgSpr = PicnicAtlasCatalog.FullSprite(ending ? "picnic-without-yuna-v1" : "house-without-actors-v1", new Vector2(0.5f, 0.5f));
                if (storyBg.sprite != bgSpr) storyBg.sprite = bgSpr;

                ApplyCinematicActors(state, sim.Time);
                return;
            }

            var sec = sim.Level.SectionAt(sim.PlayerX);
            int pct = Mathf.Clamp(Mathf.RoundToInt(sim.PlayerX / sim.Level.Goal * 100f), 0, 100);
            if (pct != lastHudPct || sec.Name != lastHudSection)
            {
                lastHudPct = pct;
                lastHudSection = sec.Name;
                hudStatus.text = $"{pct}% - {sec.Name}";
            }
            if (sim.Hp != lastHudHp || sim.Difficulty != lastHudDifficulty)
            {
                lastHudHp = sim.Hp;
                lastHudDifficulty = sim.Difficulty;
                hudHearts.text = sim.Difficulty == PicnicDifficulty.Easy
                    ? "MODO FACIL (VIDA INFINITA)"
                    : $"VIDAS: {sim.Hp}/3";
            }

            int modalMode = paused ? 1
                : sim.Stage == PicnicStage.GameOver ? 2
                : sim.Stage == PicnicStage.Disoriented ? 3
                : sim.Stage == PicnicStage.Rescuing ? 4
                : 0;
            if (modalMode != lastModalMode)
            {
                lastModalMode = modalMode;
                modalPanel.SetActive(modalMode != 0);
                if (modalMode == 1)
                {
                    TouchAxis = 0f;
                    modalTitle.text = "PAUSA";
                    modalBody.text = "O jardim espera por voce.";
                }
                else if (modalMode == 2)
                {
                    TouchAxis = 0f;
                    modalTitle.text = "TENTAR NOVAMENTE";
                    modalBody.text = "Vamos tentar esse trechinho de novo.";
                }
                else if (modalMode == 3)
                {
                    modalTitle.text = "COM CALMA";
                    modalBody.text = "Respira fundo, eu te levo de volta.";
                }
                else if (modalMode == 4)
                {
                    modalTitle.text = "ESTOU AQUI";
                    modalBody.text = "Nanda segura voce com carinho.";
                }
            }
        }

        private void ApplyCinematicActors(PicnicCinematicState state, float time)
        {
            Rect viewport = ((RectTransform)storyCanvas.transform).rect;
            if (viewport.width <= 1f || viewport.height <= 1f) return;

            float scale = Mathf.Max(viewport.width / 1652f, viewport.height / 952f) * state.Zoom;
            float halfW = viewport.width / scale * 0.5f;
            float halfH = viewport.height / scale * 0.5f;
            float fx = Mathf.Clamp(state.FocusX, halfW, 1652f - halfW);
            float fy = Mathf.Clamp(halfH, halfH, 952f - halfH);

            float bgLeft = viewport.width * 0.5f - fx * scale;
            float bgTop = viewport.height * 0.5f - fy * scale;
            PlaceTopLeft(storyBg.rectTransform, bgLeft, bgTop, 1652f * scale, 952f * scale);

            // Yuna acting sprite from cinematic-acting-v1.png (or yuna-sleeping-v1.png in ending)
            if (state.YunaSleeping)
            {
                storyYuna.sprite = PicnicAtlasCatalog.FullSprite("yuna-sleeping-v1", new Vector2(0.5f, 0f));
                storyYunaPair.gameObject.SetActive(false);
                float h = 340f * scale;
                float w = h * 1.15f;
                PlaceTopLeft(storyYuna.rectTransform, bgLeft + state.YunaX * scale - w * 0.5f, bgTop + state.YunaBaseY * scale - h, w, h);
                storyYuna.color = Color.white;
            }
            else
            {
                var frameA = PicnicAtlasCatalog.CinematicActingFrames[state.YunaActingFrame];
                storyYuna.sprite = PicnicAtlasCatalog.Slice("cinematic-acting-v1", frameA, 100f, "cin:" + state.YunaActingFrame);
                float actorScale = (440f / 548f) * scale;
                float breathe = 1f + Mathf.Sin(time * 2.1f) * 0.003f;
                PlaceTopLeft(storyYuna.rectTransform,
                    bgLeft + state.YunaX * scale - frameA.AnchorX * actorScale,
                    bgTop + state.YunaBaseY * scale - frameA.H * actorScale * breathe,
                    frameA.W * actorScale,
                    frameA.H * actorScale * breathe);

                if (state.YunaPairFrameB >= 0 && state.YunaPairBlend > 0.01f)
                {
                    var frameB = PicnicAtlasCatalog.CinematicActingFrames[state.YunaPairFrameB];
                    storyYunaPair.gameObject.SetActive(true);
                    storyYunaPair.sprite = PicnicAtlasCatalog.Slice("cinematic-acting-v1", frameB, 100f, "cin:" + state.YunaPairFrameB);
                    PlaceTopLeft(storyYunaPair.rectTransform,
                        bgLeft + state.YunaX * scale - frameB.AnchorX * actorScale,
                        bgTop + state.YunaBaseY * scale - frameB.H * actorScale,
                        frameB.W * actorScale,
                        frameB.H * actorScale);
                    storyYuna.color = new Color(1f, 1f, 1f, 1f - state.YunaPairBlend);
                    storyYunaPair.color = new Color(1f, 1f, 1f, state.YunaPairBlend);
                }
                else
                {
                    storyYunaPair.gameObject.SetActive(false);
                    storyYuna.color = Color.white;
                }
            }

            // Blue Nanda
            storyNanda.sprite = PicnicAtlasCatalog.GetBlueNandaFrame(Mathf.FloorToInt(time * 12f));
            float fh = (state.FairySitting ? 165f : 145f) * scale;
            float fw = fh * 512f / 592f;
            PlaceTopLeft(storyNanda.rectTransform,
                bgLeft + state.FairyX * scale - fw * 0.5f,
                bgTop + state.FairyY * scale - fh * 0.5f,
                fw, fh);
        }

        private void BuildStoryUi()
        {
            storyCanvas = CreateCanvas("PicnicStoryCanvas", 30);
            storyBg = CreateImage("Background", storyCanvas.transform, Vector2.zero, Vector2.one, Color.white);
            storyYuna = CreateImage("Yuna", storyCanvas.transform, Vector2.zero, Vector2.zero, Color.white);
            storyYuna.preserveAspect = true;
            storyYunaPair = CreateImage("YunaPair", storyCanvas.transform, Vector2.zero, Vector2.zero, Color.white);
            storyYunaPair.preserveAspect = true;
            storyYunaPair.gameObject.SetActive(false);
            storyNanda = CreateImage("Nanda", storyCanvas.transform, Vector2.zero, Vector2.zero, Color.white);
            storyNanda.preserveAspect = true;

            storySafeArea = CreateSafeArea(storyCanvas.transform);
            var box = CreateImage("DialogueBox", storySafeArea, new Vector2(0.12f, 0.04f), new Vector2(0.88f, 0.28f), HudTheme.Paper);
            storySpeaker = CreateText("Speaker", box.transform, new Vector2(0.03f, 0.72f), new Vector2(0.97f, 0.96f), "", 20, new Color(0.22f, 0.32f, 0.2f));
            storyBody = CreateText("Body", box.transform, new Vector2(0.03f, 0.24f), new Vector2(0.97f, 0.72f), "", 22, new Color(0.18f, 0.16f, 0.12f));
            CreateButton("Continue", box.transform, new Vector2(0.70f, 0.04f), new Vector2(0.97f, 0.24f), "Continuar", () => AdvanceRequested?.Invoke());
            CreateButton("Skip", storySafeArea, new Vector2(0.84f, 0.91f), new Vector2(0.98f, 0.98f), "Pular cena", () => SkipRequested?.Invoke());
        }

        private void BuildHudUi()
        {
            hudCanvas = CreateCanvas("PicnicHudCanvas", 20);
            hudSafeArea = CreateSafeArea(hudCanvas.transform);
            var top = CreateImage("TopBar", hudSafeArea, new Vector2(0.02f, 0.88f), new Vector2(0.46f, 0.98f), HudTheme.Paper);
            hudHearts = CreateText("Hearts", top.transform, new Vector2(0.04f, 0.52f), new Vector2(0.96f, 0.95f), "VIDAS: 3/3", 18, new Color(0.72f, 0.18f, 0.22f));
            hudStatus = CreateText("Status", top.transform, new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.52f), "0% - O portao de casa", 18, new Color(0.2f, 0.25f, 0.18f));

            CreateButton("Pause", hudSafeArea, new Vector2(0.88f, 0.90f), new Vector2(0.98f, 0.98f), "Pausa", () => PauseRequested?.Invoke());
            CreateHoldButton("Left", hudSafeArea, new Vector2(0.02f, 0.04f), new Vector2(0.12f, 0.16f), "ESQ", -1f);
            CreateHoldButton("Right", hudSafeArea, new Vector2(0.14f, 0.04f), new Vector2(0.24f, 0.16f), "DIR", 1f);
            CreateButton("Jump", hudSafeArea, new Vector2(0.84f, 0.04f), new Vector2(0.98f, 0.16f), "PULAR", () => touchJumpQueued = true);

            modalPanel = CreateImage("Modal", hudSafeArea, new Vector2(0.26f, 0.34f), new Vector2(0.74f, 0.68f), HudTheme.Paper).gameObject;
            modalTitle = CreateText("ModalTitle", modalPanel.transform, new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.92f), "", 24, new Color(0.22f, 0.32f, 0.2f));
            modalBody = CreateText("ModalBody", modalPanel.transform, new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.66f), "", 20, new Color(0.2f, 0.18f, 0.13f));
            CreateButton("RetryBtn", modalPanel.transform, new Vector2(0.08f, 0.08f), new Vector2(0.46f, 0.28f), "Continuar / Tentar", () => RetryRequested?.Invoke());
            CreateButton("MenuBtn", modalPanel.transform, new Vector2(0.54f, 0.08f), new Vector2(0.92f, 0.28f), "Menu", () => MenuRequested?.Invoke());
            modalPanel.SetActive(false);
        }

        private void CreateHoldButton(string name, Transform parent, Vector2 min, Vector2 max, string label, float dir)
        {
            var img = CreateImage(name, parent, min, max, HudTheme.Plum);
            CreateText("Label", img.transform, Vector2.zero, Vector2.one, label, 20, Color.white);
            var trigger = img.gameObject.AddComponent<EventTrigger>();
            AddTrigger(trigger, EventTriggerType.PointerDown, () =>
            {
                TouchAxis = dir;
                img.color = HudTheme.Rose;
            });
            AddTrigger(trigger, EventTriggerType.PointerUp, () => ReleaseHold(img, dir));
            AddTrigger(trigger, EventTriggerType.PointerExit, () => ReleaseHold(img, dir));
        }

        private void ReleaseHold(Image image, float direction)
        {
            if (Mathf.Approximately(TouchAxis, direction))
            {
                TouchAxis = 0f;
            }
            image.color = HudTheme.Plum;
        }

        private static void AddTrigger(EventTrigger trigger, EventTriggerType type, Action cb)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(_ => cb());
            trigger.triggers.Add(entry);
        }

        private GameObject CreateCanvas(string name, int order)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = order;
            var cs = go.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1280f, 720f);
            cs.matchWidthOrHeight = 0.5f;
            return go;
        }

        private Image CreateImage(string name, Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            if (name != "Background" && name != "Yuna" && name != "YunaPair" && name != "Nanda")
            {
                HudTheme.Round(img);
            }
            else
            {
                img.raycastTarget = false;
            }
            return img;
        }

        private Text CreateText(string name, Transform parent, Vector2 min, Vector2 max, string content, int size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var t = go.GetComponent<Text>();
            t.font = uiFont;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = content;
            t.raycastTarget = false;
            return t;
        }

        private void CreateButton(string name, Transform parent, Vector2 min, Vector2 max, string label, Action onClick)
        {
            var img = CreateImage(name, parent, min, max, new Color(0.14f, 0.24f, 0.20f, 0.92f));
            var btn = img.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());
            CreateText("Label", img.transform, Vector2.zero, Vector2.one, label, 18, Color.white);
            HudTheme.Button(btn);
        }

        private static RectTransform CreateSafeArea(Transform parent)
        {
            var go = new GameObject("SafeArea", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private void RefreshLayout(bool force)
        {
            if (hudSafeArea == null || storySafeArea == null)
            {
                return;
            }

            var canvasRect = (RectTransform)(storyCanvas.activeInHierarchy ? storyCanvas.transform : hudCanvas.transform);
            Vector2 size = canvasRect.rect.size;
            Rect safe = Screen.safeArea;
            if (!force && size == lastLayoutSize && safe == lastSafeArea)
            {
                return;
            }

            lastLayoutSize = size;
            lastSafeArea = safe;
            bool overridden = forcedWidth > 0f && forcedHeight > 0f;
            Vector2 min = overridden ? Vector2.zero : new Vector2(safe.xMin / Mathf.Max(1, Screen.width), safe.yMin / Mathf.Max(1, Screen.height));
            Vector2 max = overridden ? Vector2.one : new Vector2(safe.xMax / Mathf.Max(1, Screen.width), safe.yMax / Mathf.Max(1, Screen.height));
            hudSafeArea.anchorMin = storySafeArea.anchorMin = min;
            hudSafeArea.anchorMax = storySafeArea.anchorMax = max;
            float logicalWidth = overridden ? forcedWidth : Mathf.Max(1, Screen.width);
            float logicalHeight = overridden ? forcedHeight : Mathf.Max(1, Screen.height);
            if (!overridden && Application.isMobilePlatform)
            {
                float density = Mathf.Max(1f, Mathf.Min(logicalWidth, logicalHeight) / 390f);
                logicalWidth /= density;
                logicalHeight /= density;
            }

            float scale = size.x / Mathf.Max(1f, logicalWidth);
            float width = logicalWidth * (max.x - min.x);
            bool portrait = logicalHeight > logicalWidth;
            float topWidth = Mathf.Min(width - 96f, 380f);
            Place(hudSafeArea.Find("TopBar"), new Vector2(0f, 1f), new Vector2(0f, 1f), 12, -12, topWidth, 88, scale);
            Place(hudHearts.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), 12, -8, topWidth - 24, 28, scale);
            Place(hudStatus.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), 12, -36, topWidth - 24, 44, scale);
            hudHearts.fontSize = Mathf.RoundToInt(18 * scale);
            hudStatus.fontSize = Mathf.RoundToInt(16 * scale);
            Place(hudSafeArea.Find("Pause"), Vector2.one, Vector2.one, -12, -12, 60, 52, scale);
            Place(hudSafeArea.Find("Left"), Vector2.zero, Vector2.zero, 12, 16, 64, 64, scale);
            Place(hudSafeArea.Find("Right"), Vector2.zero, Vector2.zero, 88, 16, 64, 64, scale);
            Place(hudSafeArea.Find("Jump"), new Vector2(1f, 0f), new Vector2(1f, 0f), -12, 16, 108, 64, scale);

            float cardWidth = Mathf.Min(width - 32, 540);
            float cardHeight = portrait ? 320 : 232;
            Place(modalPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0, 0, cardWidth, cardHeight, scale);
            Place(modalTitle.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), 0, -16, cardWidth - 40, 44, scale);
            Place(modalBody.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), 0, -66, cardWidth - 40, 54, scale);
            modalTitle.fontSize = Mathf.RoundToInt(24 * scale);
            modalBody.fontSize = Mathf.RoundToInt(18 * scale);
            if (portrait)
            {
                Place(modalPanel.transform.Find("RetryBtn"), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), 0, 84, cardWidth - 40, 52, scale);
                Place(modalPanel.transform.Find("MenuBtn"), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), 0, 20, cardWidth - 40, 52, scale);
            }
            else
            {
                Place(modalPanel.transform.Find("RetryBtn"), Vector2.zero, Vector2.zero, 20, 24, (cardWidth - 52) * 0.5f, 52, scale);
                Place(modalPanel.transform.Find("MenuBtn"), new Vector2(1f, 0f), new Vector2(1f, 0f), -20, 24, (cardWidth - 52) * 0.5f, 52, scale);
            }

            float storyWidth = Mathf.Min(width - 24, 760);
            float storyHeight = portrait ? 300 : 200;
            Transform box = storySafeArea.Find("DialogueBox");
            Place(box, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), 0, 12, storyWidth, storyHeight, scale);
            Place(storySpeaker.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), 0, -12, storyWidth - 32, 28, scale);
            Place(storyBody.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), 0, -44, storyWidth - 32, storyHeight - 120, scale);
            Place(box.Find("Continue"), new Vector2(1f, 0f), new Vector2(1f, 0f), -16, 12, 152, 52, scale);
            Place(storySafeArea.Find("Skip"), Vector2.one, Vector2.one, -12, -12, 132, 52, scale);
            storySpeaker.fontSize = Mathf.RoundToInt(18 * scale);
            storyBody.fontSize = Mathf.RoundToInt(18 * scale);
            foreach (Text label in hudSafeArea.GetComponentsInChildren<Text>(true))
            {
                if (label.name == "Label")
                {
                    label.fontSize = Mathf.RoundToInt(18 * scale);
                }
            }
            foreach (Text label in storySafeArea.GetComponentsInChildren<Text>(true))
            {
                if (label.name == "Label")
                {
                    label.fontSize = Mathf.RoundToInt(18 * scale);
                }
            }
        }

        private static void Place(Transform target, Vector2 anchor, Vector2 pivot, float x, float y, float width, float height, float scale)
        {
            var rect = (RectTransform)target;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = new Vector2(x, y) * scale;
            rect.sizeDelta = new Vector2(width, height) * scale;
        }

        private static void PlaceTopLeft(RectTransform rt, float left, float top, float width, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(left, -top);
            rt.sizeDelta = new Vector2(width, height);
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }
    }
}
