using System.IO;
using UnityEditor;
using UnityEngine;

namespace YunaLegend.EditorScripts
{
    public static partial class DarkRoomArtAssetBuilder
    {
        private static readonly string[] WallArtNames = {
            "WallDrawing_Sun", "WallDrawing_Castle", "WallDrawing_Rainbow", "WallDrawing_Cat",
            "WallCostume_Hat", "WallCostume_Cape", "WallCostume_Scarf", "WallDecor_Sail"
        };

        private static void GenerateWallDecorationAssets()
        {
            for (int i = 0; i < WallArtNames.Length; i++)
                SaveTextureAsPng(PaintWallIllustration(i), Path.Combine(MaterialsFolder, "T_" + WallArtNames[i] + ".png"));
            AssetDatabase.Refresh();
            for (int i = 0; i < WallArtNames.Length; i++)
            {
                string path = MaterialsFolder + "/T_" + WallArtNames[i] + ".png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.alphaIsTransparency = i >= 4;
                importer.SaveAndReimport();
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                CreateUrpMaterial("M_" + WallArtNames[i], Color.white, 0.02f, texture,
                    new Color(0.085f, 0.080f, 0.095f), alphaClip: i >= 4);
            }
            var wood = LoadTexture("T_Pastel_ToyWood.png");
            CreateUrpMaterial("M_WallDecor_Wood", new Color(0.36f, 0.25f, 0.22f), 0.08f, wood, new Color(0.042f, 0.032f, 0.035f));
            CreateUrpMaterial("M_WallDecor_Lilac", new Color(0.48f, 0.39f, 0.57f), 0.02f, wood, new Color(0.045f, 0.036f, 0.055f));
            CreateUrpMaterial("M_WallDecor_Glass", new Color(0.31f, 0.48f, 0.62f), 0.85f, wood, new Color(0.035f, 0.050f, 0.065f));
        }

        private static Texture2D PaintWallIllustration(int subject)
        {
            var p = new WallPainter(subject < 4 ? new Color(0.88f, 0.81f, 0.67f) : Color.clear);
            Color ochre = new Color(0.88f, 0.64f, 0.28f), red = new Color(0.72f, 0.35f, 0.34f);
            Color lilac = new Color(0.56f, 0.43f, 0.68f), blue = new Color(0.34f, 0.55f, 0.66f);
            Color ink = new Color(0.30f, 0.24f, 0.33f), cream = new Color(0.96f, 0.86f, 0.65f);
            if (subject == 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = i * Mathf.PI / 6f;
                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    p.Line(new Vector2(0.50f, 0.55f) + dir * 0.26f, new Vector2(0.50f, 0.55f) + dir * 0.37f, 0.014f, ochre);
                }
                p.Ellipse(0.5f, 0.55f, 0.22f, 0.25f, ochre);
                p.Ellipse(0.43f, 0.60f, 0.014f, 0.025f, ink);
                p.Ellipse(0.57f, 0.60f, 0.014f, 0.025f, ink);
                p.Arc(0.5f, 0.55f, 0.10f, 0.10f, 210f, 330f, 0.009f, red);
                p.Ellipse(0.38f, 0.51f, 0.028f, 0.017f, red);
                p.Ellipse(0.62f, 0.51f, 0.028f, 0.017f, red);
                p.Line(new Vector2(0.12f, 0.13f), new Vector2(0.88f, 0.15f), 0.012f, blue);
            }
            else if (subject == 1)
            {
                p.Polygon(lilac, new Vector2(.24f,.19f),new Vector2(.24f,.61f),new Vector2(.76f,.61f),new Vector2(.76f,.19f));
                foreach (float x in new[] { .23f, .73f })
                {
                    p.Polygon(red, new Vector2(x-.08f,.20f),new Vector2(x-.08f,.67f),new Vector2(x+.08f,.67f),new Vector2(x+.08f,.20f));
                    p.Polygon(lilac, new Vector2(x-.13f,.67f),new Vector2(x,.88f),new Vector2(x+.13f,.67f));
                    p.Ellipse(x, .53f, .023f, .05f, blue);
                    p.Line(new Vector2(x,.88f),new Vector2(x,.97f), .006f, ink);
                    p.Polygon(ochre,new Vector2(x,.97f),new Vector2(x+.09f,.94f),new Vector2(x,.91f));
                }
                p.Ellipse(.5f,.27f,.09f,.13f,ink);
                p.Polygon(ink,new Vector2(.41f,.14f),new Vector2(.41f,.28f),new Vector2(.59f,.28f),new Vector2(.59f,.14f));
                p.Ellipse(.5f,.51f,.04f,.055f,cream);
                for(int i=0;i<4;i++) p.Line(new Vector2(.34f+i*.10f,.31f),new Vector2(.38f+i*.10f,.31f),.006f,cream);
                p.Line(new Vector2(.08f,.15f),new Vector2(.92f,.15f),.013f,new Color(.45f,.58f,.40f));
            }
            else if (subject == 2)
            {
                Color[] bands = {red,ochre,new Color(.78f,.73f,.38f),new Color(.46f,.63f,.47f),blue,lilac};
                for(int i=0;i<bands.Length;i++) p.Arc(.5f,.30f,.37f-i*.039f,.50f-i*.052f,0f,180f,.027f,bands[i]);
                foreach(float x in new[]{.17f,.81f})
                {
                    p.Ellipse(x,.27f,.105f,.07f,cream);
                    p.Ellipse(x-.04f,.32f,.06f,.07f,cream);
                    p.Ellipse(x+.035f,.33f,.06f,.08f,cream);
                }
            }
            else if (subject == 3)
            {
                p.Ellipse(.50f,.34f,.19f,.23f,ochre);
                p.Arc(.68f,.29f,.16f,.15f,230f,460f,.033f,ochre);
                p.Polygon(ochre,new Vector2(.29f,.61f),new Vector2(.28f,.91f),new Vector2(.46f,.75f));
                p.Polygon(ochre,new Vector2(.53f,.75f),new Vector2(.72f,.91f),new Vector2(.72f,.61f));
                p.Ellipse(.50f,.65f,.23f,.21f,ochre);
                p.Polygon(red,new Vector2(.32f,.76f),new Vector2(.32f,.86f),new Vector2(.41f,.76f));
                p.Polygon(red,new Vector2(.59f,.76f),new Vector2(.68f,.86f),new Vector2(.68f,.76f));
                foreach(float x in new[]{.42f,.58f}) p.Ellipse(x,.68f,.018f,.029f,ink);
                p.Polygon(red,new Vector2(.47f,.61f),new Vector2(.53f,.61f),new Vector2(.50f,.57f));
                p.Line(new Vector2(.50f,.57f),new Vector2(.46f,.54f),.006f,ink);
                p.Line(new Vector2(.50f,.57f),new Vector2(.54f,.54f),.006f,ink);
                for(int i=0;i<3;i++)
                {
                    p.Line(new Vector2(.36f,.59f-i*.025f),new Vector2(.20f,.62f-i*.05f),.004f,ink);
                    p.Line(new Vector2(.64f,.59f-i*.025f),new Vector2(.80f,.62f-i*.05f),.004f,ink);
                }
            }
            else if(subject == 4)
            {
                p.Ellipse(.50f,.21f,.46f,.12f,ink);
                p.Polygon(lilac,new Vector2(.21f,.24f),new Vector2(.50f,.94f),new Vector2(.62f,.78f),new Vector2(.60f,.59f),new Vector2(.78f,.24f));
                p.Line(new Vector2(.29f,.32f),new Vector2(.71f,.32f),.030f,red);
                p.Star(.50f,.49f,.09f,ochre);
                p.Line(new Vector2(.37f,.40f),new Vector2(.51f,.80f),.007f,new Color(.67f,.55f,.76f));
            }
            else if(subject == 5)
            {
                p.Polygon(red,new Vector2(.43f,.91f),new Vector2(.57f,.91f),new Vector2(.89f,.12f),new Vector2(.69f,.08f),new Vector2(.50f,.14f),new Vector2(.28f,.08f),new Vector2(.10f,.14f));
                p.Line(new Vector2(.43f,.83f),new Vector2(.30f,.16f),.012f,new Color(.48f,.23f,.29f));
                p.Line(new Vector2(.57f,.83f),new Vector2(.72f,.16f),.012f,new Color(.48f,.23f,.29f));
                p.Line(new Vector2(.50f,.80f),new Vector2(.50f,.22f),.013f,new Color(.84f,.44f,.40f));
                p.Arc(.5f,.90f,.095f,.035f,180f,360f,.018f,ochre);
                p.Star(.50f,.54f,.085f,ochre);
                p.Line(new Vector2(.43f,.91f),new Vector2(.34f,.78f),.009f,ochre);
                p.Line(new Vector2(.57f,.91f),new Vector2(.66f,.78f),.009f,ochre);
            }
            else if(subject == 6)
            {
                p.Polygon(blue,new Vector2(.27f,.94f),new Vector2(.53f,.94f),new Vector2(.50f,.13f),new Vector2(.24f,.13f));
                p.Polygon(blue,new Vector2(.52f,.86f),new Vector2(.73f,.86f),new Vector2(.81f,.27f),new Vector2(.58f,.27f));
                for(int i=0;i<7;i++)
                {
                    float y=.18f+i*.10f;
                    p.Line(new Vector2(.26f,y),new Vector2(.50f,y+.01f),.022f,ochre);
                    if(i<5) p.Line(new Vector2(.59f,y+.14f),new Vector2(.77f,y+.14f),.021f,ochre);
                }
                p.Ellipse(.50f,.89f,.17f,.085f,blue);
                for(int i=0;i<6;i++)
                {
                    p.Line(new Vector2(.25f+i*.045f,.14f),new Vector2(.25f+i*.045f,.075f),.005f,cream);
                    p.Line(new Vector2(.59f+i*.040f,.28f),new Vector2(.60f+i*.040f,.215f),.005f,cream);
                }
            }
            else
            {
                p.Polygon(cream,new Vector2(.07f,.06f),new Vector2(.07f,.96f),new Vector2(.94f,.06f));
                p.Line(new Vector2(.10f,.12f),new Vector2(.10f,.86f),.009f,ochre);
                p.Line(new Vector2(.10f,.12f),new Vector2(.82f,.12f),.009f,red);
            }
            return p.Finish();
        }

        // Small deterministic raster painter: grain and imperfect strokes suggest wax on paper.
        // Never changes UnityEngine.Random's global state, so other asset generators remain stable.
        private sealed class WallPainter
        {
            private const int Size = 384;
            private readonly Color[] pixels = new Color[Size * Size];
            public WallPainter(Color background)
            {
                for(int i=0;i<pixels.Length;i++) pixels[i]=background;
            }
            private void Put(int x,int y,Color color)
            {
                if(x<0||y<0||x>=Size||y>=Size) return;
                float grain = Mathf.PerlinNoise(x*.83f+12.4f,y*.83f+4.7f);
                float wash = .84f + .20f*grain;
                color.r*=wash; color.g*=wash; color.b*=wash;
                pixels[y*Size+x]=Color.Lerp(pixels[y*Size+x],color,.78f+.22f*grain);
            }
            public void Ellipse(float cx,float cy,float rx,float ry,Color color)
            {
                for(int y=Mathf.FloorToInt((cy-ry)*Size);y<=Mathf.CeilToInt((cy+ry)*Size);y++)
                for(int x=Mathf.FloorToInt((cx-rx)*Size);x<=Mathf.CeilToInt((cx+rx)*Size);x++)
                {
                    float dx=(x/(float)Size-cx)/rx,dy=(y/(float)Size-cy)/ry;
                    if(dx*dx+dy*dy<=1f) Put(x,y,color);
                }
            }
            public void Line(Vector2 a,Vector2 b,float radius,Color color)
            {
                int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)*Size*1.5f));
                for(int i=0;i<=steps;i++)
                {
                    var at=Vector2.Lerp(a,b,i/(float)steps);
                    at.y+=Mathf.Sin(i*.29f+a.x*15f)*.0015f;
                    Ellipse(at.x,at.y,radius,radius,color);
                }
            }
            public void Arc(float cx,float cy,float rx,float ry,float start,float end,float width,Color color)
            {
                for(float a=start;a<=end;a+=1f)
                    Ellipse(cx+Mathf.Cos(a*Mathf.Deg2Rad)*rx,cy+Mathf.Sin(a*Mathf.Deg2Rad)*ry,width,width,color);
            }
            public void Polygon(Color color,params Vector2[] vertices)
            {
                Vector2 min=vertices[0],max=vertices[0];
                foreach(var v in vertices) {min=Vector2.Min(min,v);max=Vector2.Max(max,v);}
                for(int y=(int)(min.y*Size);y<=Mathf.CeilToInt(max.y*Size);y++)
                for(int x=(int)(min.x*Size);x<=Mathf.CeilToInt(max.x*Size);x++)
                {
                    float px=x/(float)Size,py=y/(float)Size; bool inside=false;
                    for(int i=0,j=vertices.Length-1;i<vertices.Length;j=i++)
                    {
                        var a=vertices[i];var b=vertices[j];
                        if((a.y>py)!=(b.y>py)&&px<(b.x-a.x)*(py-a.y)/(b.y-a.y)+a.x) inside=!inside;
                    }
                    if(inside) Put(x,y,color);
                }
            }
            public void Star(float x,float y,float radius,Color color)
            {
                var v=new Vector2[10];
                for(int i=0;i<10;i++) {float a=(90+i*36)*Mathf.Deg2Rad;float r=i%2==0?radius:radius*.43f;v[i]=new Vector2(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r);}
                Polygon(color,v);
            }
            public Texture2D Finish()
            {
                for(int y=0;y<Size;y++) for(int x=0;x<Size;x++)
                {
                    int index=y*Size+x;var c=pixels[index];
                    float grain=.94f+.08f*Mathf.PerlinNoise(x*.75f,y*.75f);
                    c.r*=grain;c.g*=grain;c.b*=grain;pixels[index]=c;
                }
                var texture=new Texture2D(Size,Size,TextureFormat.RGBA32,true);
                texture.SetPixels(pixels);texture.Apply();return texture;
            }
        }
    }
}
