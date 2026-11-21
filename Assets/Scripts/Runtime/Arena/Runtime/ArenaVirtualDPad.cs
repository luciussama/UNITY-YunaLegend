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

        public void ApplyMobileSizing(float dpadSize, Vector2 anchoredPos, int helperFontSize, float px)
        {
            if (padArea == null)
            {
                padArea = GetComponent<RectTransform>();
            }

            if (padArea != null)
            {
                padArea.anchoredPosition = anchoredPos;
                padArea.sizeDelta = new Vector2(dpadSize, dpadSize);
            }

            maxRadius = dpadSize * 0.34f;

            if (thumbIndicator != null)
            {
                float thumbSize = dpadSize * 0.18f;
                thumbIndicator.sizeDelta = new Vector2(thumbSize, thumbSize);
            }

            var topHint = transform.Find("DPad_HintTop")?.GetComponent<Text>();
            if (topHint != null)
            {
                topHint.fontSize = helperFontSize;
                topHint.rectTransform.anchoredPosition = new Vector2(0f, dpadSize * 0.5f + 12f * px);
                topHint.rectTransform.sizeDelta = new Vector2(Mathf.Max(dpadSize + 40f * px, 160f * px), 20f * px);
            }

            var bottomHint = transform.Find("DPad_HintBottom")?.GetComponent<Text>();
            if (bottomHint != null)
            {
                bottomHint.fontSize = helperFontSize;
                bottomHint.rectTransform.anchoredPosition = new Vector2(0f, -(dpadSize * 0.5f + 12f * px));
                bottomHint.rectTransform.sizeDelta = new Vector2(Mathf.Max(dpadSize + 40f * px, 160f * px), 20f * px);
            }
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
            // D-Pad fiel ao jogo legado: disco circular escuro translucido + cruz (+) cinza-ardosia
            // com borda branca nitida, pivo central escuro, 4 setas brancas de alto contraste
            // e legendas "↑ PULAR · ↓ ABAIXAR" (acima) e "TRÁS: DEFENDER" (abaixo).
            var root = new GameObject("Mobile_DPad", typeof(RectTransform), typeof(Image), typeof(ArenaVirtualDPad));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(138f, 134f);
            rect.sizeDelta = new Vector2(226f, 226f);

            var hitBg = root.GetComponent<Image>();
            hitBg.sprite = CreateLegacyDPadSprite(256);
            hitBg.color = Color.white;
            hitBg.raycastTarget = true;

            var thumbGo = new GameObject("Thumb", typeof(RectTransform), typeof(Image));
            thumbGo.transform.SetParent(root.transform, false);
            var thumbRect = thumbGo.GetComponent<RectTransform>();
            thumbRect.sizeDelta = new Vector2(36f, 36f);
            var thumbImg = thumbGo.GetComponent<Image>();
            thumbImg.sprite = CreateCircleSprite(64);
            thumbImg.color = new Color(1f, 0.90f, 0.56f, 0.55f);
            thumbImg.raycastTarget = false;

            CreateHelperLabel(root.transform, font, "DPad_HintTop", "↑ PULAR · ↓ ABAIXAR", new Vector2(0f, 128f));
            CreateHelperLabel(root.transform, font, "DPad_HintBottom", "TRÁS: DEFENDER", new Vector2(0f, -120f));

            var dpad = root.GetComponent<ArenaVirtualDPad>();
            dpad.Configure(rect, thumbRect, 76f);
            return dpad;
        }

        private static void CreateHelperLabel(Transform parent, Font font, string name, string content, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(260f, 28f);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = 15;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(1f, 1f, 1f, 0.96f);
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private static Sprite CreateCircleSprite(int size)
        {
            int s = Mathf.Max(16, size);
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
                    float a = 1f - Mathf.SmoothStep(0.88f, 0.98f, r);
                    pixels[y * s + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// Gera o sprite completo do D-Pad da Arena fiel ao legado:
        /// disco escuro translucido + cruz (+) cinza-ardosia com borda clara + pivo central escuro + 4 setas brancas nitidas.
        /// </summary>
        private static Sprite CreateLegacyDPadSprite(int size)
        {
            int s = Mathf.Max(128, size);
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            float half = (s - 1) * 0.5f;
            float invHalf = 1f / half;
            var pixels = new Color32[s * s];

            Color discCol = new Color(0.08f, 0.08f, 0.11f, 0.66f);
            Color crossFill = new Color(0.295f, 0.285f, 0.335f, 0.94f);
            Color crossBorder = new Color(0.88f, 0.87f, 0.92f, 0.96f);
            Color centerHole = new Color(0.15f, 0.15f, 0.19f, 0.96f);
            Color arrowWhite = new Color(1f, 1f, 1f, 1f);

            const float armHalfW = 0.27f;
            const float armLen = 0.82f;
            const float borderW = 0.028f;

            for (int y = 0; y < s; y++)
            {
                float ny = (y - half) * invHalf;
                for (int x = 0; x < s; x++)
                {
                    float nx = (x - half) * invHalf;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);
                    if (r > 0.99f)
                    {
                        pixels[y * s + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    // 1) Base circular escura translucida
                    Color col = discCol;
                    col.a *= 1f - Mathf.SmoothStep(0.955f, 0.988f, r);

                    // 2) Cruz (+) com borda branca nitida (SDF exata da uniao de 2 retangulos)
                    float dV = SdfBox(nx, ny, armHalfW, armLen);
                    float dH = SdfBox(nx, ny, armLen, armHalfW);
                    float dCross = Mathf.Min(dV, dH);

                    if (dCross < 0.012f)
                    {
                        float insideMask = 1f - Mathf.SmoothStep(-0.008f, 0.010f, dCross);
                        float borderMask = Mathf.SmoothStep(-borderW - 0.008f, -borderW + 0.008f, dCross);
                        Color crossCol = Color.Lerp(crossFill, crossBorder, borderMask);
                        col = Color.Lerp(col, crossCol, insideMask);
                        col.a = Mathf.Max(col.a, crossCol.a * insideMask);
                    }

                    // 3) Pivo central circular escuro
                    if (r < 0.165f)
                    {
                        float holeMask = 1f - Mathf.SmoothStep(0.145f, 0.165f, r);
                        col = Color.Lerp(col, centerHole, holeMask);
                    }

                    // 4) 4 setas brancas nitidas (Cima, Baixo, Esquerda, Direita)
                    if (IsInsideArrow(nx, ny - 0.54f)
                        || IsInsideArrow(nx, -(ny + 0.54f))
                        || IsInsideArrow(ny, -(nx + 0.54f))
                        || IsInsideArrow(ny, nx - 0.54f))
                    {
                        col = arrowWhite;
                    }

                    pixels[y * s + x] = (Color32)col;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f), 100f);
        }

        private static float SdfBox(float x, float y, float bx, float by)
        {
            float dx = Mathf.Abs(x) - bx;
            float dy = Mathf.Abs(y) - by;
            float outDist = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f));
            float inDist = Mathf.Min(Mathf.Max(dx, dy), 0f);
            return outDist + inDist;
        }

        /// <summary>
        /// Testa se o ponto local (lx, ly) esta dentro de uma seta apontando para +Y centrada na origem.
        /// </summary>
        private static bool IsInsideArrow(float lx, float ly)
        {
            // Haste vertical da seta
            if (Mathf.Abs(lx) <= 0.030f && ly >= -0.10f && ly <= 0.06f)
            {
                return true;
            }

            // Cabeca triangular apontando para cima (+Y)
            if (ly >= -0.01f && ly <= 0.115f)
            {
                float tipT = (ly - (-0.01f)) / 0.125f;
                float halfW = Mathf.Lerp(0.115f, 0.0f, tipT);
                if (Mathf.Abs(lx) <= halfW && ly >= Mathf.Abs(lx) * 0.55f - 0.01f)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
