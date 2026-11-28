using System.Collections.Generic;
using UnityEngine;
using YunaLegend.DreamForest.Core;

namespace YunaLegend.DreamForest.Runtime
{
    /// <summary>
    /// Exact sprite slice catalog and procedural texture helpers for DreamForest_Gameplay,
    /// matching the legacy Dream Forest art coordinates (dreamArt.js, gardenArt.js,
    /// gardenBeetleArt.js, dreamNightmareArt.js) 1:1.
    /// </summary>
    public static class DreamForestAtlasCatalog
    {
        public const float Ppu = DreamForestLevelData.PixelsPerUnit;
        public const float GirlReferenceHeight = 256f;
        public const float ClimbReferenceHeight = 521f;

        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        private static Sprite whiteSprite;
        private static Sprite grassBladeSprite;
        private static Sprite radialGlowSprite;
        private static Sprite lowerMistSprite;
        private static Sprite featheredDreamBackdrop;
        private static Texture2D grassRibbonTex;

        // Exact tight opaque bounds of girl-running-v1.png (1536x768, sole at bottom edge <= 1px)
        public static readonly RectInt[] GirlRunningRects =
        {
            new RectInt(11, 11, 156, 254),
            new RectInt(395, 11, 156, 254),
            new RectInt(779, 11, 162, 254),
            new RectInt(1163, 11, 168, 254),
            new RectInt(11, 395, 163, 249),
            new RectInt(395, 395, 163, 255),
            new RectInt(779, 395, 165, 255),
            new RectInt(1163, 395, 173, 254),
        };

        // Exact tight opaque bounds of girl-actions-v1.png (1536x768, 1=jump up, 3=fall, 7=idle/complete)
        public static readonly RectInt[] GirlActionRects =
        {
            new RectInt(11, 11, 177, 241),
            new RectInt(395, 11, 171, 245),
            new RectInt(779, 11, 174, 210),
            new RectInt(1163, 11, 173, 242),
            new RectInt(11, 395, 176, 247),
            new RectInt(395, 395, 179, 249),
            new RectInt(779, 395, 200, 188),
            new RectInt(1163, 395, 158, 254),
        };

        // Exact DREAM_CLIMB_CYCLE frames (yuna-climbing-v2.png)
        public static readonly RectInt[] ClimbCycleRects =
        {
            new RectInt(94, 8, 308, 521),
            new RectInt(610, 18, 307, 514),
            new RectInt(1135, 21, 299, 514),
            new RectInt(99, 530, 308, 484),
            new RectInt(653, 535, 335, 467),
            new RectInt(1152, 568, 302, 442),
        };

        // Exact painted hand anchor [x, y] inside each ClimbCycleRects frame (from dreamArt.js)
        public static readonly Vector2[] ClimbHands =
        {
            new Vector2(274f, 156f),
            new Vector2(305f, 110f),
            new Vector2(273f, 154f),
            new Vector2(305f, 119f),
            new Vector2(281f, 238f),
            new Vector2(159f, 312f),
        };

        // Exact non-overlapping rock slices in rocks-nightmare-v1.png (2172x724)
        public static readonly RectInt[] NightmareRockRects =
        {
            new RectInt(0, 0, 665, 724),
            new RectInt(670, 0, 825, 724),
            new RectInt(1500, 0, 672, 724),
        };

        public static Vector3 ToUnity(float px, float py, float z = 0f)
        {
            return new Vector3(px / Ppu, (450f - py) / Ppu, z);
        }

        public static Texture2D LoadTexture(string name)
        {
            if (Textures.TryGetValue(name, out var cached) && cached != null) return cached;
            var tex = Resources.Load<Texture2D>("DreamForest/" + name);
            if (tex == null)
            {
                tex = Resources.Load<Texture2D>("Picnic/" + name);
            }
            if (tex != null)
            {
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
            }
            Textures[name] = tex;
            return tex;
        }

        public static AudioClip LoadAudio(string clipName)
        {
            return Resources.Load<AudioClip>("DreamForest/Audio/" + clipName);
        }

        public static Sprite FullSprite(string textureName, Vector2 pivot, float ppu = Ppu)
        {
            string key = $"full:{textureName}:{pivot.x:F2},{pivot.y:F2}:{ppu:F1}";
            if (Sprites.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = LoadTexture(textureName);
            if (tex == null) return null;
            var s = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), pivot, ppu, 0, SpriteMeshType.FullRect);
            Sprites[key] = s;
            return s;
        }

        public static Sprite SliceTopDown(string textureName, int x, int y, int w, int h, Vector2 pivot, float ppu = Ppu, string cacheKey = null)
        {
            string key = cacheKey ?? $"{textureName}:{x},{y},{w},{h}:{pivot.x:F3},{pivot.y:F3}:{ppu:F1}";
            if (Sprites.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = LoadTexture(textureName);
            if (tex == null) return null;

            float uy = Mathf.Clamp(tex.height - y - h, 0, tex.height - 1);
            float cx = Mathf.Clamp(x, 0, tex.width - 1);
            float cw = Mathf.Clamp(w, 1, tex.width - cx);
            float ch = Mathf.Clamp(h, 1, tex.height - uy);
            var s = Sprite.Create(tex, new Rect(cx, uy, cw, ch), pivot, ppu, 0, SpriteMeshType.FullRect);
            Sprites[key] = s;
            return s;
        }

        public static Sprite SliceRect(string textureName, RectInt r, Vector2 pivot, float ppu = Ppu)
        {
            return SliceTopDown(textureName, r.x, r.y, r.width, r.height, pivot, ppu);
        }

        public static Sprite SliceGrid(string textureName, int cols, int rows, int index, Vector2 pivot, float ppu = Ppu)
        {
            var tex = LoadTexture(textureName);
            if (tex == null) return null;
            int total = Mathf.Max(1, cols * rows);
            int idx = ((index % total) + total) % total;
            int col = idx % cols;
            int row = idx / cols;
            int cw = tex.width / cols;
            int ch = tex.height / rows;
            return SliceTopDown(textureName, col * cw, row * ch, cw, ch, pivot, ppu, $"grid:{textureName}:{cols}x{rows}:{idx}:{pivot.x:F2},{pivot.y:F2}");
        }

        /// <summary>
        /// Builds a horizontal-edge-feathered backdrop matching legacy prepareDreamBackdrop
        /// (alpha 0->1 over 0..17%, 1 over 17..83%, 1->0 over 83..100%) so overlapping tiles
        /// blend seamlessly at step = 0.74 * w with zero mirroring.
        /// </summary>
        public static Sprite FeatheredDreamBackdropSprite()
        {
            if (featheredDreamBackdrop != null) return featheredDreamBackdrop;
            var src = LoadTexture("forest-blue-v1");
            if (src == null) return null;

            if (!src.isReadable)
            {
                featheredDreamBackdrop = FullSprite("forest-blue-v1", new Vector2(0.5f, 0.5f));
                return featheredDreamBackdrop;
            }

            int w = src.width;
            int h = src.height;
            var pixels = src.GetPixels32();
            float leftFade = w * 0.17f;
            float rightStart = w * 0.83f;

            for (int x = 0; x < w; x++)
            {
                float alphaMul = 1f;
                if (x < leftFade) alphaMul = x / Mathf.Max(1f, leftFade);
                else if (x > rightStart) alphaMul = (w - 1 - x) / Mathf.Max(1f, w - 1 - rightStart);
                if (alphaMul >= 0.999f) continue;

                for (int y = 0; y < h; y++)
                {
                    int idx = y * w + x;
                    Color32 c = pixels[idx];
                    c.a = (byte)Mathf.RoundToInt(c.a * alphaMul);
                    pixels[idx] = c;
                }
            }

            var feathered = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
            };
            feathered.SetPixels32(pixels);
            feathered.Apply();
            featheredDreamBackdrop = Sprite.Create(feathered, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), Ppu, 0, SpriteMeshType.FullRect);
            featheredDreamBackdrop.hideFlags = HideFlags.HideAndDontSave;
            return featheredDreamBackdrop;
        }

        public static Sprite LowerMistGradientSprite()
        {
            if (lowerMistSprite != null) return lowerMistSprite;
            const int h = 64;
            var tex = new Texture2D(2, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var pixels = new Color32[2 * h];
            for (int y = 0; y < h; y++)
            {
                float t = 1f - y / (float)(h - 1); // 1 at bottom, 0 at top
                float a = t * t * 0.16f;
                Color c = new Color(0.39f, 0.56f, 0.78f, a);
                pixels[y * 2] = c;
                pixels[y * 2 + 1] = c;
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            lowerMistSprite = Sprite.Create(tex, new Rect(0, 0, 2, h), new Vector2(0.5f, 0.5f), 2f, 0, SpriteMeshType.FullRect);
            lowerMistSprite.hideFlags = HideFlags.HideAndDontSave;
            return lowerMistSprite;
        }

        public static Texture2D GrassRibbonTexture()
        {
            if (grassRibbonTex != null) return grassRibbonTex;
            const int w = 64;
            const int h = 256;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var pixels = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                float v = y / (float)(h - 1); // 0 = root/bottom, 1 = tip/top
                for (int x = 0; x < w; x++)
                {
                    float u = x / (float)(w - 1);
                    float dx = (u - 0.5f) * 2f;
                    float adx = Mathf.Abs(dx);
                    float edgeAlpha = Mathf.Clamp01((1f - adx) * 8f);
                    float vein = Mathf.Exp(-adx * adx * 16f);
                    float rim = Mathf.Clamp01((adx - 0.55f) / 0.45f) * (dx < 0f ? 1f : 0.4f);
                    Color baseCol = Color.Lerp(new Color(0.06f, 0.20f, 0.18f, 1f), new Color(0.22f, 0.56f, 0.44f, 1f), Mathf.Pow(v, 0.7f));
                    Color lit = Color.Lerp(baseCol, new Color(0.54f, 0.86f, 0.76f, 1f), vein * 0.32f + rim * 0.38f * (0.25f + 0.75f * v));
                    lit.a = edgeAlpha * 0.96f;
                    pixels[y * w + x] = lit;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            grassRibbonTex = tex;
            return grassRibbonTex;
        }

        public static Sprite SolidWhiteSprite()
        {
            if (whiteSprite != null) return whiteSprite;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
            whiteSprite.hideFlags = HideFlags.HideAndDontSave;
            return whiteSprite;
        }

        public static Sprite RadialGlowSprite()
        {
            if (radialGlowSprite != null) return radialGlowSprite;
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - r);
                    a = a * a;
                    pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            radialGlowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
            radialGlowSprite.hideFlags = HideFlags.HideAndDontSave;
            return radialGlowSprite;
        }

        public static Sprite GrassBladeSprite()
        {
            if (grassBladeSprite != null) return grassBladeSprite;
            var tex = GrassRibbonTexture();
            grassBladeSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            grassBladeSprite.hideFlags = HideFlags.HideAndDontSave;
            return grassBladeSprite;
        }

        public static void FitSprite(Transform t, Sprite s, float worldW, float worldH)
        {
            if (t == null || s == null || s.bounds.size.x <= 0.0001f || s.bounds.size.y <= 0.0001f) return;
            t.localScale = new Vector3(worldW / s.bounds.size.x, worldH / s.bounds.size.y, 1f);
        }
    }
}
