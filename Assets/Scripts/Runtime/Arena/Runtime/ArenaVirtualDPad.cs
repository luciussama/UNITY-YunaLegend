using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YunaLegend.Arena.Runtime
{
    /// <summary>
    /// D-Pad virtual em cruz para mobile: exibe o desenho classico da cruz direcional,
    /// mas aceita arraste analogico continuo em todas as direcoes (8 direcoes com zona morta).
    /// </summary>
    public sealed class ArenaVirtualDPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const float Deadzone = 0.22f;
        private const float VerticalThreshold = 0.42f;

        [SerializeField] private RectTransform padArea;
        [SerializeField] private RectTransform thumbIndicator;
        [SerializeField] private float maxRadius = 68f;

        private bool previousJumpHeld;

        public Vector2 Direction { get; private set; }
        public bool CrouchHeld => Direction.y < -VerticalThreshold;

        public bool ConsumeJumpPressed()
        {
            bool jumpHeld = Direction.y > VerticalThreshold;
            bool triggered = jumpHeld && !previousJumpHeld;
            previousJumpHeld = jumpHeld;
            return triggered;
        }

        public void Configure(RectTransform area, RectTransform thumb, float radius = 68f)
        {
            padArea = area;
            thumbIndicator = thumb;
            maxRadius = radius;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateInput(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateInput(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Direction = Vector2.zero;
            previousJumpHeld = false;
            if (thumbIndicator != null)
            {
                thumbIndicator.anchoredPosition = Vector2.zero;
            }
        }

        private void OnDisable()
        {
            Direction = Vector2.zero;
            previousJumpHeld = false;
            if (thumbIndicator != null)
            {
                thumbIndicator.anchoredPosition = Vector2.zero;
            }
        }

        private void UpdateInput(PointerEventData eventData)
        {
            if (padArea == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(padArea, eventData.position, eventData.pressEventCamera, out Vector2 local))
            {
                return;
            }

            Vector2 clamped = Vector2.ClampMagnitude(local, maxRadius);
            if (thumbIndicator != null)
            {
                thumbIndicator.anchoredPosition = clamped;
            }

            Vector2 normalized = clamped / Mathf.Max(1f, maxRadius);
            if (normalized.magnitude < Deadzone)
            {
                Direction = Vector2.zero;
                return;
            }

            Direction = normalized;
        }

        public static ArenaVirtualDPad CreateVisualDPad(Transform parent, Font font)
        {
            var root = new GameObject("Mobile_DPad", typeof(RectTransform), typeof(Image), typeof(ArenaVirtualDPad));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(126f, 120f);
            rect.sizeDelta = new Vector2(184f, 184f);

            // Area circular sutil de toque (recebe clique em qualquer ponto do D-Pad)
            var hitBg = root.GetComponent<Image>();
            hitBg.color = new Color(0.06f, 0.05f, 0.09f, 0.35f);
            hitBg.raycastTarget = true;

            Color crossColor = new Color(0.16f, 0.13f, 0.22f, 0.88f);
            CreateBar("Cross_V", root.transform, new Vector2(56f, 158f), crossColor);
            CreateBar("Cross_H", root.transform, new Vector2(158f, 56f), crossColor);

            Color arrowColor = new Color(0.96f, 0.90f, 0.78f, 0.92f);
            CreateArrowLabel(root.transform, font, "▲", new Vector2(0f, 54f), arrowColor);
            CreateArrowLabel(root.transform, font, "▼", new Vector2(0f, -54f), arrowColor);
            CreateArrowLabel(root.transform, font, "◄", new Vector2(-54f, 0f), arrowColor);
            CreateArrowLabel(root.transform, font, "►", new Vector2(54f, 0f), arrowColor);

            var thumbGo = new GameObject("Thumb", typeof(RectTransform), typeof(Image));
            thumbGo.transform.SetParent(root.transform, false);
            var thumbRect = thumbGo.GetComponent<RectTransform>();
            thumbRect.sizeDelta = new Vector2(36f, 36f);
            var thumbImg = thumbGo.GetComponent<Image>();
            thumbImg.color = new Color(1f, 0.86f, 0.48f, 0.65f);
            thumbImg.raycastTarget = false;

            var dpad = root.GetComponent<ArenaVirtualDPad>();
            dpad.Configure(rect, thumbRect, 68f);
            return dpad;
        }

        private static void CreateBar(string name, Transform parent, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
        }

        private static void CreateArrowLabel(Transform parent, Font font, string symbol, Vector2 pos, Color color)
        {
            var go = new GameObject("Arrow_" + symbol, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(40f, 40f);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.text = symbol;
            text.fontSize = 22;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
        }
    }
}
