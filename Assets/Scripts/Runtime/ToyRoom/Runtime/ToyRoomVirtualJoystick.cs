using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Analogico virtual para controles mobile/touch na Toy Room.
    /// Captura arraste relativo ao centro da base circular e emite vetor normalizado em [-1, 1].
    /// </summary>
    public sealed class ToyRoomVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float maxRadius = 56f;
        [SerializeField] private float deadZone = 0.1f;

        public Vector2 Value { get; private set; }

        public event Action<Vector2> OnValueChanged;

        public void Configure(RectTransform backgroundRect, RectTransform handleRect, float radius = 56f, float deadZoneThreshold = 0.1f)
        {
            background = backgroundRect;
            handle = handleRect;
            maxRadius = Mathf.Max(8f, radius);
            deadZone = Mathf.Clamp01(deadZoneThreshold);
            ResetStick();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateStick(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateStick(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetStick();
        }

        private void OnDisable()
        {
            ResetStick();
        }

        private void UpdateStick(PointerEventData eventData)
        {
            if (background == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            {
                return;
            }

            Vector2 clamped = Vector2.ClampMagnitude(localPoint, maxRadius);
            if (handle != null)
            {
                handle.anchoredPosition = clamped;
            }

            Vector2 normalized = clamped / maxRadius;
            if (normalized.magnitude < deadZone)
            {
                normalized = Vector2.zero;
            }

            SetValue(normalized);
        }

        private void ResetStick()
        {
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }

            SetValue(Vector2.zero);
        }

        private void SetValue(Vector2 next)
        {
            if ((Value - next).sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Value = next;
            OnValueChanged?.Invoke(Value);
        }

        private static float SmoothRange(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        public static Sprite CreateCircleSprite(int size, float innerRadiusRatio, float outerRadiusRatio)
        {
            int clampedSize = Mathf.Max(16, size);
            var tex = new Texture2D(clampedSize, clampedSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            float half = (clampedSize - 1) * 0.5f;
            float invHalf = 1f / half;
            var pixels = new Color32[clampedSize * clampedSize];

            for (int y = 0; y < clampedSize; y++)
            {
                float dy = (y - half) * invHalf;
                for (int x = 0; x < clampedSize; x++)
                {
                    float dx = (x - half) * invHalf;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float outerAlpha = 1f - SmoothRange(outerRadiusRatio - 0.04f, outerRadiusRatio, dist);
                    float innerAlpha = innerRadiusRatio > 0f
                        ? SmoothRange(innerRadiusRatio - 0.04f, innerRadiusRatio, dist)
                        : 1f;

                    byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt(outerAlpha * innerAlpha * 255f), 0, 255);
                    pixels[y * clampedSize + x] = new Color32(255, 255, 255, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);

            return Sprite.Create(
                tex,
                new Rect(0f, 0f, clampedSize, clampedSize),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        /// <summary>
        /// Gera a base circular do analogico fiel ao legado: disco escuro translucido,
        /// borda fina dourada (#F3D87C) e 4 marcas cardinais apontando para dentro (12h, 3h, 6h, 9h).
        /// </summary>
        public static Sprite CreateLegacyJoystickBaseSprite(int size = 256)
        {
            int s = Mathf.Max(64, size);
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            float half = (s - 1) * 0.5f;
            float invHalf = 1f / half;
            var pixels = new Color32[s * s];

            Color fillCol = new Color(0.10f, 0.08f, 0.14f, 0.48f);
            Color goldCol = new Color(0.953f, 0.847f, 0.486f, 0.96f);

            for (int y = 0; y < s; y++)
            {
                float dy = (y - half) * invHalf;
                for (int x = 0; x < s; x++)
                {
                    float dx = (x - half) * invHalf;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    if (r > 0.99f)
                    {
                        pixels[y * s + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    Color c = fillCol;
                    float outerFade = 1f - SmoothRange(0.955f, 0.985f, r);
                    c.a *= outerFade;

                    float ring = SmoothRange(0.915f, 0.935f, r) * (1f - SmoothRange(0.965f, 0.985f, r));

                    bool inTickRadius = r >= 0.78f && r <= 0.94f;
                    float tickV = inTickRadius ? (1f - SmoothRange(0.014f, 0.024f, Mathf.Abs(dx))) : 0f;
                    float tickH = inTickRadius ? (1f - SmoothRange(0.014f, 0.024f, Mathf.Abs(dy))) : 0f;
                    float goldMask = Mathf.Clamp01(Mathf.Max(ring, Mathf.Max(tickV, tickH)));

                    Color final = Color.Lerp(c, goldCol, goldMask);
                    final.a = Mathf.Max(c.a, goldCol.a * goldMask);
                    pixels[y * s + x] = (Color32)final;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// Gera o disco dourado com gradiente radial e borda branca nitida (usado no knob do analogico
        /// e no botao redondo PEGAR/GUARDAR fiel ao jogo legado).
        /// </summary>
        public static Sprite CreateGoldenOrbSprite(int size = 256, float borderThickness = 0.055f)
        {
            int s = Mathf.Max(64, size);
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            float half = (s - 1) * 0.5f;
            float invHalf = 1f / half;
            var pixels = new Color32[s * s];

            Color brightCenter = new Color(1.00f, 0.937f, 0.545f, 1f);
            Color warmEdge = new Color(0.847f, 0.580f, 0.133f, 1f);
            Color borderWhite = new Color(1f, 1f, 1f, 0.98f);

            float borderInner = 0.95f - borderThickness;
            for (int y = 0; y < s; y++)
            {
                float dy = (y - half) * invHalf;
                for (int x = 0; x < s; x++)
                {
                    float dx = (x - half) * invHalf;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    if (r > 0.985f)
                    {
                        pixels[y * s + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    float gx = dx;
                    float gy = dy - 0.16f;
                    float gr = Mathf.Clamp01(Mathf.Sqrt(gx * gx + gy * gy) * 0.92f);
                    Color grad = Color.Lerp(brightCenter, warmEdge, gr * gr);

                    float borderMask = SmoothRange(borderInner - 0.015f, borderInner + 0.01f, r);
                    Color c = Color.Lerp(grad, borderWhite, borderMask);
                    c.a = 1f - SmoothRange(0.95f, 0.98f, r);
                    pixels[y * s + x] = (Color32)c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// Gera halo circular tracejado sutil ao redor do botao PEGAR/GUARDAR.
        /// </summary>
        public static Sprite CreateDashedRingSprite(int size = 256, int dashCount = 24)
        {
            int s = Mathf.Max(64, size);
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            float half = (s - 1) * 0.5f;
            float invHalf = 1f / half;
            var pixels = new Color32[s * s];

            for (int y = 0; y < s; y++)
            {
                float dy = (y - half) * invHalf;
                for (int x = 0; x < s; x++)
                {
                    float dx = (x - half) * invHalf;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float ring = SmoothRange(0.91f, 0.93f, r) * (1f - SmoothRange(0.96f, 0.98f, r));
                    float angle = Mathf.Atan2(dy, dx);
                    float dash = Mathf.Sin(angle * dashCount) > 0.15f ? 1f : 0f;
                    byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(ring * dash * 255f), 0, 255);
                    pixels[y * s + x] = new Color32(255, 248, 224, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
