using UnityEngine;
using UnityEngine.UI;

namespace YunaLegend.Picnic.Runtime
{
    public enum PicnicSymbol
    {
        Heart,
        Left,
        Right,
        Infinity
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PicnicHudSymbol : MaskableGraphic
    {
        public PicnicSymbol Symbol { get; set; }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = GetPixelAdjustedRect();
            float size = Mathf.Min(bounds.width, bounds.height);
            Vector2 center = bounds.center;
            if (Symbol == PicnicSymbol.Left || Symbol == PicnicSymbol.Right)
            {
                float direction = Symbol == PicnicSymbol.Left ? -1f : 1f;
                AddTriangle(mesh, center + new Vector2(direction * size * 0.4f, 0f),
                    center + new Vector2(-direction * size * 0.3f, size * 0.35f),
                    center + new Vector2(-direction * size * 0.3f, -size * 0.35f));
                return;
            }

            const int segments = 64;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                float b = (i + 1) * Mathf.PI * 2f / segments;
                if (Symbol == PicnicSymbol.Heart)
                {
                    AddTriangle(mesh, center, center + Heart(a) * size, center + Heart(b) * size);
                }
                else
                {
                    Vector2 start = center + Infinity(a) * size;
                    Vector2 end = center + Infinity(b) * size;
                    Vector2 normal = new Vector2(-(end - start).y, (end - start).x).normalized * size * 0.035f;
                    AddTriangle(mesh, start - normal, start + normal, end + normal);
                    AddTriangle(mesh, start - normal, end + normal, end - normal);
                }
            }
        }

        private static Vector2 Heart(float t)
        {
            float sine = Mathf.Sin(t);
            return new Vector2(16f * sine * sine * sine,
                13f * Mathf.Cos(t) - 5f * Mathf.Cos(2f * t) - 2f * Mathf.Cos(3f * t) - Mathf.Cos(4f * t)) / 36f;
        }

        private static Vector2 Infinity(float t)
        {
            return new Vector2(Mathf.Cos(t) * 0.45f, Mathf.Sin(2f * t) * 0.22f);
        }

        private void AddTriangle(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(a, color, Vector2.zero);
            mesh.AddVert(b, color, Vector2.zero);
            mesh.AddVert(c, color, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
        }

        public static PicnicHudSymbol Create(string name, Transform parent, Vector2 min, Vector2 max, PicnicSymbol symbol, Color tint)
        {
            var graphic = PicnicUi.Rect(name, parent, min, max).gameObject.AddComponent<PicnicHudSymbol>();
            graphic.Symbol = symbol;
            graphic.color = tint;
            graphic.raycastTarget = false;
            return graphic;
        }
    }
}
