using UnityEngine;
using UnityEngine.UI;

namespace YunaLegend.DreamForestEntrance.Runtime
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class EntranceFlagGraphic : MaskableGraphic
    {
        protected EntranceFlagGraphic()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Line(vh,new Vector2(5,21),new Vector2(5,4));
            for (int i=0;i<28;i++)
            {
                float a=i/28f;
                float b=(i+1)/28f;
                Line(vh,Point(a,4),Point(b,4));
                Line(vh,Point(a,14),Point(b,14));
            }
            Line(vh,new Vector2(19,4),new Vector2(19,14));
        }

        private static Vector2 Point(float t, float y)
        {
            return new Vector2(5+14*t,y+Mathf.Sin((t*2-1)*Mathf.PI)*1.2f);
        }

        private void Line(VertexHelper vh, Vector2 a, Vector2 b)
        {
            Rect r=rectTransform.rect;
            a=new Vector2(r.x+a.x*r.width/24,r.yMax-a.y*r.height/24);
            b=new Vector2(r.x+b.x*r.width/24,r.yMax-b.y*r.height/24);
            Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*(1.6f*r.width/48);
            int i=vh.currentVertCount;
            Color c=new Color(185/255f,177/255f,196/255f);
            vh.AddVert(a+n,c,Vector2.zero);
            vh.AddVert(a-n,c,Vector2.zero);
            vh.AddVert(b-n,c,Vector2.zero);
            vh.AddVert(b+n,c,Vector2.zero);
            vh.AddTriangle(i,i+1,i+2);
            vh.AddTriangle(i,i+2,i+3);
        }
    }
}
