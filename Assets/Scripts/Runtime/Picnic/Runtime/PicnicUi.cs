using UnityEngine;
using UnityEngine.UI;

namespace YunaLegend.Picnic.Runtime
{
    internal static class PicnicUi
    {
        public static Canvas Canvas(string name, int order)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }
        public static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }
        public static Image Image(string name, Transform parent, Vector2 min, Vector2 max, Color color, string asset = null)
        {
            var image = Rect(name, parent, min, max).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            if (asset != null) image.sprite = PicnicArt.Load(asset);
            return image;
        }
        public static Text Text(string name, Transform parent, Vector2 min, Vector2 max, string value, int size, Color color)
        {
            var text = Rect(name, parent, min, max).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = value;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = size;
            return text;
        }
        public static Button Button(string name, Transform parent, Vector2 min, Vector2 max, string label)
        {
            var image = Image(name, parent, min, max, new Color(0.11f, 0.23f, 0.2f, 0.9f));
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text("Label", image.transform, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), label, 22, new Color(1f, 0.96f, 0.85f));
            return button;
        }
    }
}
