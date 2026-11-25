using UnityEngine;
using UnityEngine.UI;

namespace YunaLegend.Presentation
{
    /// <summary>Shared presentation only: sliced paper cards and accessible button states.</summary>
    public static class HudTheme
    {
        public static readonly Color Paper = new Color(0.99f, 0.96f, 0.88f, 0.98f);
        public static readonly Color Ink = new Color(0.20f, 0.17f, 0.27f, 1f);
        public static readonly Color Plum = new Color(0.30f, 0.24f, 0.39f, 0.98f);
        public static readonly Color Gold = new Color(0.83f, 0.66f, 0.38f, 1f);
        public static readonly Color Rose = new Color(0.68f, 0.23f, 0.34f, 1f);
        public static readonly Color Sage = new Color(0.27f, 0.39f, 0.33f, 1f);

        private static Sprite roundedSprite;

        public static void Round(Image image)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = RoundedSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
        }

        public static void Panel(Image image)
        {
            Round(image);
            image.color = Paper;
        }

        public static void Button(Button button)
        {
            if (button == null)
            {
                return;
            }

            var image = button.GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            Round(image);
            image.color = Plum;
            image.raycastTarget = true;
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.18f, 1.13f, 1.10f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.78f, 0.74f, 0.86f, 1f);
            colors.disabledColor = new Color(0.68f, 0.68f, 0.68f, 0.75f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            foreach (Text text in button.GetComponentsInChildren<Text>(true))
            {
                text.color = Paper;
                text.raycastTarget = false;
            }
        }

        private static Sprite RoundedSprite
        {
            get
            {
                if (roundedSprite != null)
                {
                    return roundedSprite;
                }

                const int size = 96;
                const float radius = 24f;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    name = "HudRoundedPaper",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    hideFlags = HideFlags.HideAndDontSave,
                };
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = Mathf.Max(Mathf.Abs(x + 0.5f - size * 0.5f) - (size * 0.5f - radius - 1f), 0f);
                        float dy = Mathf.Max(Mathf.Abs(y + 0.5f - size * 0.5f) - (size * 0.5f - radius - 1f), 0f);
                        float distance = Mathf.Sqrt(dx * dx + dy * dy) - radius;
                        byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(0.5f - distance) * 255f);
                        pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply(false, true);
                roundedSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                    100f, 0, SpriteMeshType.FullRect, new Vector4(25f, 25f, 25f, 25f));
                roundedSprite.name = "HudRoundedPaper";
                roundedSprite.hideFlags = HideFlags.HideAndDontSave;
                return roundedSprite;
            }
        }
    }
}
