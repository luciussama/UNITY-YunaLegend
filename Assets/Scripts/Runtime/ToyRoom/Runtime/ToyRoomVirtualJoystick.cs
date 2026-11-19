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

                    float outerAlpha = 1f - Mathf.SmoothStep(outerRadiusRatio - 0.04f, outerRadiusRatio, dist);
                    float innerAlpha = innerRadiusRatio > 0f
                        ? Mathf.SmoothStep(innerRadiusRatio - 0.04f, innerRadiusRatio, dist)
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
    }
}
