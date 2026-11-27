using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace YunaLegend.Presentation
{
    /// <summary>Mobile-only presentation of existing narrative content and actions.</summary>
    [DefaultExecutionOrder(30000)]
    public sealed class MobileDialoguePresenter : MonoBehaviour
    {
        private static readonly string[] PanelNames =
        {
            "DialogueBox", "PoeticCard", "PoeticOverlay", "PoeticNarration", "IntroPoeticNarrationCard", "EndingDialogueCard", "HintPanel", "DodgeHintBar", "Status_Hint"
        };
        private static readonly string[] BodyNames =
        {
            "Body", "DialogueBody", "PoeticBody", "PoeticText", "BodyText", "HintText", "DodgeHintText", "Status_Hint"
        };
        private static readonly string[] SuppressedHintNames =
        {
            "DialogueHint", "PoeticHint", "AutoHintText", "ClickHintText", "AdvanceHint"
        };
        private static readonly string[] TapAdvancePanelNames =
        {
            "DialogueBox", "PoeticCard", "PoeticOverlay", "PoeticNarration", "IntroPoeticNarrationCard", "EndingDialogueCard"
        };
        private readonly List<RectTransform> panels = new List<RectTransform>();
        private Renderer[] actors = Array.Empty<Renderer>();
        private float nextScan;
        private float overrideWidth;
        private float overrideHeight;
        private bool? mobileOverride;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "TitleScreen")
            {
                return;
            }
            if (FindFirstObjectByType<MobileDialoguePresenter>() == null)
            {
                new GameObject("MobileDialoguePresentation").AddComponent<MobileDialoguePresenter>();
            }
        }

        public void SetMobileViewportOverride(bool mobile, float width, float height)
        {
            mobileOverride = mobile;
            overrideWidth = width;
            overrideHeight = height;
        }

        private void LateUpdate()
        {
            ApplyNow();
        }

        public void ApplyNow()
        {
            if (SceneManager.GetActiveScene().name == "TitleScreen")
            {
                return;
            }
            if (Time.unscaledTime >= nextScan || panels.Count == 0)
            {
                Scan();
                nextScan = Time.unscaledTime + 0.5f;
            }
            bool isMobile = mobileOverride ?? Application.isMobilePlatform;
            if (!isMobile)
            {
                ApplyDesktopCardPolish();
                return;
            }
            foreach (RectTransform panel in panels)
            {
                if (panel != null && panel.gameObject.activeInHierarchy)
                {
                    Layout(panel);
                }
            }
            StackSimultaneousPanels();
        }

        private void ApplyDesktopCardPolish()
        {
            float pulse = 0.58f + 0.18f * Mathf.Sin(Time.unscaledTime * 3.6f);
            Color cueTint = new Color(HudTheme.Plum.r, HudTheme.Plum.g, HudTheme.Plum.b, pulse);

            foreach (RectTransform panel in panels)
            {
                if (panel == null || !panel.gameObject.activeInHierarchy)
                {
                    continue;
                }
                if (panel.name != "IntroPoeticNarrationCard" && panel.name != "EndingDialogueCard")
                {
                    continue;
                }

                // Suppress verbose "Avança sozinho / Clique/toque no quadro adianta" labels on desktop too
                foreach (Text text in panel.GetComponentsInChildren<Text>(true))
                {
                    if (Array.IndexOf(SuppressedHintNames, text.name) >= 0)
                    {
                        text.enabled = false;
                    }
                }

                if (panel.name == "EndingDialogueCard")
                {
                    // Reposition BearChase ending card to top-center ceiling band so it never covers Yuna/Nanda
                    panel.anchorMin = new Vector2(0.5f, 0.96f);
                    panel.anchorMax = new Vector2(0.5f, 0.96f);
                    panel.pivot = new Vector2(0.5f, 1f);
                    panel.anchoredPosition = Vector2.zero;
                    panel.sizeDelta = new Vector2(860f, 156f);

                    Transform body = panel.Find("InnerCreamCard/DialogueBody");
                    if (body != null)
                    {
                        var bodyRt = (RectTransform)body;
                        bodyRt.anchorMin = new Vector2(0.05f, 0.08f);
                        bodyRt.anchorMax = new Vector2(0.92f, 0.74f);
                        bodyRt.offsetMin = Vector2.zero;
                        bodyRt.offsetMax = Vector2.zero;
                    }
                }
                else if (panel.name == "IntroPoeticNarrationCard")
                {
                    // Expand BodyText to fill the entire narration card cleanly
                    Transform body = panel.Find("BodyText");
                    if (body != null)
                    {
                        var bodyRt = (RectTransform)body;
                        float topOffset = bodyRt.anchoredPosition.y;
                        float cardW = panel.sizeDelta.x;
                        float cardH = panel.sizeDelta.y;
                        bodyRt.sizeDelta = new Vector2(Mathf.Max(120f, cardW - 56f), Mathf.Max(36f, cardH + topOffset - 12f));
                    }
                }

                HudTheme.EnsureCornerTapCue(panel, 20f, 10f, 8f, cueTint);
            }
        }

        private void StackSimultaneousPanels()
        {
            var groups = new Dictionary<Canvas, List<RectTransform>>();
            foreach (RectTransform panel in panels)
            {
                if (panel == null || !panel.gameObject.activeInHierarchy)
                {
                    continue;
                }
                Canvas canvas = panel.GetComponentInParent<Canvas>().rootCanvas;
                if (!groups.TryGetValue(canvas, out var group))
                {
                    group = new List<RectTransform>();
                    groups.Add(canvas, group);
                }
                group.Add(panel);
            }
            foreach (var entry in groups)
            {
                if (entry.Value.Count < 2)
                {
                    continue;
                }
                Canvas canvas = entry.Key;
                float pixelsPerCanvasUnit = overrideWidth > 0f ? overrideWidth / ((RectTransform)canvas.transform).rect.width : canvas.scaleFactor;
                float total = 0f;
                foreach (var panel in entry.Value)
                {
                    total += panel.rect.height * pixelsPerCanvasUnit + 4f;
                }
                float screenHeight = overrideHeight > 0f ? overrideHeight : Screen.height;
                float bottom = Mathf.Clamp(FindHead(canvas).y + 8f, 4f, Mathf.Max(4f, screenHeight - total - 4f));
                foreach (RectTransform panel in entry.Value)
                {
                    Vector2 screen = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, panel.position);
                    screen.y = bottom;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)panel.parent, screen, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 local);
                    panel.localPosition = new Vector3(local.x, local.y, 0f);
                    bottom += panel.rect.height * pixelsPerCanvasUnit + 4f;
                }
            }
        }

        private void Scan()
        {
            panels.Clear();
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                foreach (RectTransform rect in canvas.GetComponentsInChildren<RectTransform>(true))
                {
                    if (Array.IndexOf(PanelNames, rect.name) >= 0)
                    {
                        panels.Add(rect);
                    }
                }
            }
            var found = new List<Renderer>();
            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (renderer is ParticleSystemRenderer || renderer.name.IndexOf("shadow", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }
                for (Transform ancestor = renderer.transform; ancestor != null; ancestor = ancestor.parent)
                {
                    if (ancestor.name.IndexOf("Yuna", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        found.Add(renderer);
                        break;
                    }
                }
            }
            actors = found.ToArray();
        }

        private void Layout(RectTransform panel)
        {
            Canvas canvas = panel.GetComponentInParent<Canvas>().rootCanvas;
            RectTransform canvasRect = (RectTransform)canvas.transform;
            float logicalWidth = overrideWidth > 0f ? overrideWidth : Screen.width / Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height) / 390f);
            float logicalHeight = overrideHeight > 0f ? overrideHeight : Screen.height / Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height) / 390f);
            float unit = canvasRect.rect.width / Mathf.Max(1f, logicalWidth);
            float width = Mathf.Min(276f, logicalWidth - 20f) * unit;
            float padding = 8f * unit;
            bool isTapAdvancePanel = Array.IndexOf(TapAdvancePanelNames, panel.name) >= 0;
            float tapIconSize = 18f * unit;
            float tapCornerClearance = isTapAdvancePanel ? (tapIconSize + 6f * unit) : 0f;

            // Suppress verbose "Clique para pular / Toque para avançar" labels so the message fills the entire balloon
            var texts = new List<Text>();
            foreach (Text text in panel.GetComponentsInChildren<Text>(true))
            {
                if (Array.IndexOf(SuppressedHintNames, text.name) >= 0)
                {
                    text.enabled = false;
                    continue;
                }
                if (text.name.IndexOf("Portrait", StringComparison.OrdinalIgnoreCase) >= 0 || text.GetComponentInParent<Button>() != panel.GetComponent<Button>() && text.transform.parent.GetComponent<Button>() != null)
                {
                    continue;
                }
                if (text.gameObject.activeInHierarchy && !string.IsNullOrEmpty(text.text))
                {
                    texts.Add(text);
                }
            }
            texts.Sort((a, b) => Rank(a.name).CompareTo(Rank(b.name)));
            float height = padding;
            for (int i = 0; i < texts.Count; i++)
            {
                Text text = texts[i];
                bool isSpeaker = Rank(text.name) == 0;
                bool isLast = i == texts.Count - 1;
                text.fontSize = Mathf.Max(1, Mathf.RoundToInt((isSpeaker ? 13f : 14f) * unit));
                text.resizeTextForBestFit = false;
                text.lineSpacing = 1.04f;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.alignment = TextAnchor.UpperLeft;
                float textWidth = width - padding * 2f - (isLast ? tapCornerClearance : 0f);
                float h = Mathf.Max(16f * unit, text.cachedTextGeneratorForLayout.GetPreferredHeight(text.text, text.GetGenerationSettings(new Vector2(Mathf.Max(40f * unit, textWidth), 0f))) / text.pixelsPerUnit);
                SetRect(text.rectTransform, padding, -height, Mathf.Max(40f * unit, textWidth), h);
                height += h + 3f * unit;
            }
            foreach (Button button in panel.GetComponentsInChildren<Button>(true))
            {
                if (button.transform == panel)
                {
                    continue;
                }
                // Redundant "Continue" child button on tap-advance dialogue balloons is replaced by full-balloon click + discreet corner icon
                if (isTapAdvancePanel && button.name == "Continue")
                {
                    button.gameObject.SetActive(false);
                    continue;
                }
                if (!button.gameObject.activeInHierarchy)
                {
                    continue;
                }
                SetRect((RectTransform)button.transform, width - 106f * unit, -height, 100f * unit, 36f * unit);
                foreach (Text label in button.GetComponentsInChildren<Text>())
                {
                    label.fontSize = Mathf.Max(1, Mathf.RoundToInt(14f * unit));
                }
                height += 38f * unit;
            }
            height = Mathf.Max(height + padding - 3f * unit, isTapAdvancePanel ? (tapIconSize + padding * 2f) : 0f);
            if (isTapAdvancePanel)
            {
                float pulse = 0.56f + 0.18f * Mathf.Sin(Time.unscaledTime * 3.6f);
                Color cueTint = new Color(HudTheme.Plum.r, HudTheme.Plum.g, HudTheme.Plum.b, pulse);
                HudTheme.EnsureCornerTapCue(panel, tapIconSize, 6f * unit, 5f * unit, cueTint);
            }
            foreach (RectTransform child in panel.GetComponentsInChildren<RectTransform>())
            {
                if (child.name.StartsWith("Portrait", StringComparison.Ordinal) || child.name == "CornerFlower")
                {
                    foreach (Graphic graphic in child.GetComponentsInChildren<Graphic>())
                    {
                        graphic.enabled = false;
                    }
                }
                if (child.name == "InnerBorder" || child.name == "InnerFrame" || child.name == "InnerSageBorder" || child.name == "PoeticInnerBorder")
                {
                    child.anchorMin = Vector2.zero;
                    child.anchorMax = Vector2.one;
                    child.offsetMin = Vector2.one * (2f * unit);
                    child.offsetMax = -Vector2.one * (2f * unit);
                }
            }
            Image background = panel.GetComponent<Image>();
            if (background != null)
            {
                HudTheme.Panel(background);
            }
            Vector2 head = FindHead(canvas);
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            if (overrideWidth > 0f)
            {
                screenSize = new Vector2(overrideWidth, overrideHeight);
            }
            Rect safe = overrideWidth > 0f ? new Rect(Vector2.zero, screenSize) : Screen.safeArea;
            float screenUnit = screenSize.x / Mathf.Max(1f, logicalWidth);
            float screenW = width / unit * screenUnit;
            float screenH = height / unit * screenUnit;
            head.x = Mathf.Clamp(head.x, safe.xMin + screenW * 0.5f + 4f * screenUnit, safe.xMax - screenW * 0.5f - 4f * screenUnit);
            float topHudMargin = ComputeTopOverlayClearance(canvas, safe.yMax, screenUnit);
            head.y = Mathf.Clamp(head.y + 8f * screenUnit, safe.yMin + 4f * screenUnit, Mathf.Max(safe.yMin + 4f * screenUnit, safe.yMax - screenH - topHudMargin));
            RectTransform parent = (RectTransform)panel.parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, head, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 local);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0f);
            panel.sizeDelta = new Vector2(width, height);
            panel.localPosition = new Vector3(local.x, local.y, 0f);
        }

        private Vector2 FindHead(Canvas canvas)
        {
            foreach (Image image in canvas.GetComponentsInChildren<Image>())
            {
                if (image.name != "Yuna" && image.name != "GirlStopMotionActor")
                {
                    continue;
                }
                Vector3[] corners = new Vector3[4];
                image.rectTransform.GetWorldCorners(corners);
                return RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, (corners[1] + corners[2]) * 0.5f);
            }
            Camera camera = Camera.main;
            if (camera == null)
            {
                foreach (Camera candidate in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                {
                    if (!candidate.name.StartsWith("__Temp", StringComparison.Ordinal))
                    {
                        camera = candidate;
                        break;
                    }
                }
            }
            bool any = false;
            Bounds bounds = new Bounds();
            foreach (Renderer actor in actors)
            {
                if (actor == null || !actor.enabled || !actor.gameObject.activeInHierarchy || actor.bounds.size.sqrMagnitude < 0.0001f)
                {
                    continue;
                }
                if (!any)
                {
                    bounds = actor.bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(actor.bounds);
                }
            }
            if (camera != null && any)
            {
                Vector3 point = camera.WorldToScreenPoint(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));
                if (point.z > 0f)
                {
                    return point;
                }
            }
            return new Vector2((overrideWidth > 0f ? overrideWidth : Screen.width) * 0.5f, (overrideHeight > 0f ? overrideHeight : Screen.height) * 0.5f);
        }

        private static float ComputeTopOverlayClearance(Canvas canvas, float safeYMax, float screenUnit)
        {
            float clearance = 4f * screenUnit;
            if (canvas == null)
            {
                return clearance;
            }
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var corners = new Vector3[4];
            foreach (Button btn in canvas.GetComponentsInChildren<Button>(false))
            {
                if (btn.name != "Skip" && btn.name != "Pause" && btn.name != "PauseButton")
                {
                    continue;
                }
                ((RectTransform)btn.transform).GetWorldCorners(corners);
                float btnBottomY = RectTransformUtility.WorldToScreenPoint(cam, corners[0]).y;
                float needed = (safeYMax - btnBottomY) + 6f * screenUnit;
                if (needed > clearance && needed < safeYMax * 0.30f)
                {
                    clearance = needed;
                }
            }
            return clearance;
        }

        private static int Rank(string name)
        {
            if (name.IndexOf("Speaker", StringComparison.OrdinalIgnoreCase) >= 0 || name == "HeadingText")
            {
                return 0;
            }
            return Array.IndexOf(BodyNames, name) >= 0 ? 1 : 2;
        }

        private static void SetRect(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
