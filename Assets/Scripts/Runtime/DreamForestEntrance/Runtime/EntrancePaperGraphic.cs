using UnityEngine;
using UnityEngine.UI;

namespace YunaLegend.DreamForestEntrance.Runtime
{
    /// <summary>The legacy's paper, two pastel outlines and corner petals as native UI geometry.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class EntrancePaperGraphic : MaskableGraphic
    {
        protected EntrancePaperGraphic()
        {
            useLegacyMeshGeneration = false;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            for (int blur = 12; blur >= 1; blur--)
            {
                Rect shadow = Inset(r,-blur*.6f);
                shadow.y -= 3;
                Rounded(vh,shadow,16+blur*.6f,new Color(57/255f,36/255f,34/255f,.016f));
            }
            Rounded(vh, Inset(r,-1.5f), 17.5f, Hex(0xd6aeb1));
            Rounded(vh, Inset(r, 1.5f), 14.5f, Hex(0xfff5e5));
            Rounded(vh, Inset(r, 6.25f), 10.75f, Hex(0xb8c9bb));
            Rounded(vh, Inset(r, 7.75f), 9.25f, Hex(0xfff5e5));
            foreach (float x in new[] { r.xMin + 15, r.xMax - 15 })
            {
                Vector2 p = new Vector2(x, r.yMax - 15);
                for (int i = 0; i < 4; i++)
                {
                    float a = i * Mathf.PI / 2;
                    Circle(vh, p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 4, 3, Hex(0xe5bec3));
                }
                Circle(vh, p, 2, Hex(0xc6ae79));
            }
        }

        private static Rect Inset(Rect r, float n)
        {
            return new Rect(r.x+n, r.y+n, r.width-2*n, r.height-2*n);
        }

        private static Color Hex(uint rgb)
        {
            return new Color((rgb >> 16 & 255)/255f, (rgb >> 8 & 255)/255f, (rgb & 255)/255f, 1);
        }

        private static void Circle(VertexHelper vh, Vector2 p, float radius, Color color)
        {
            Rounded(vh, new Rect(p.x-radius,p.y-radius,radius*2,radius*2),radius,color);
        }

        private static void Rounded(VertexHelper vh, Rect r, float radius, Color color)
        {
            int first = vh.currentVertCount;
            vh.AddVert(r.center, color, Vector2.zero);
            for (int ring = 0; ring < 2; ring++)
            {
                Color edge = color;
                if (ring == 1) edge.a = 0;
                float ringRadius = radius + (ring == 0 ? -.4f : .4f);
                for (int corner = 0; corner < 4; corner++)
                {
                    Vector2 center = new Vector2(corner < 2 ? r.xMax-radius : r.xMin+radius,
                        corner == 0 || corner == 3 ? r.yMax-radius : r.yMin+radius);
                    for (int i = 0; i <= 12; i++)
                    {
                        float a = (90-corner*90-i*7.5f)*Mathf.Deg2Rad;
                        vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*ringRadius,edge,Vector2.zero);
                    }
                }
            }
            const int count = 52;
            for (int i = 0; i < count; i++)
            {
                vh.AddTriangle(first,first+1+i,first+1+(i+1)%count);
                int inner = first+1+i;
                int innerNext = first+1+(i+1)%count;
                vh.AddTriangle(inner,inner+count,innerNext+count);
                vh.AddTriangle(inner,innerNext+count,innerNext);
            }
        }
    }
}
