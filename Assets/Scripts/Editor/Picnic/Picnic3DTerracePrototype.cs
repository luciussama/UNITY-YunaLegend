using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;

namespace YunaLegend.EditorScripts.Picnic
{
    /// <summary>
    /// Isolated visual proof-of-concept: renders the exact 5-step grass terrace (primeira-grama)
    /// as a real 3D diorama with URP directional lighting, overhanging grass caps, ivy-draped earth faces,
    /// multi-Z tall grass layers, and 2D illustrated Yuna + blue Nanda sprites.
    /// Does not modify Picnic_Gameplay.unity or any runtime gameplay code.
    /// </summary>
    public static class Picnic3DTerracePrototype
    {
        public static void RenderPrototype()
        {
            string outDir = Path.GetFullPath("Evidence/Picnic1B/Prototype3D");
            Directory.CreateDirectory(outDir);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Perspective 2.5D camera framed so backdrop and ground fill 100% of the 1280x720 viewport
            var camGo = new GameObject("ProtoCamera", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = false;
            cam.fieldOfView = 26f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.20f, 0.34f, 0.14f, 1f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 120f;
            cam.transform.position = new Vector3(18.2f, 2.15f, -12.2f);
            cam.transform.LookAt(new Vector3(18.2f, 0.55f, 0f));

            // Warm morning directional sunlight with soft shadows
            var lightGo = new GameObject("SunLight", typeof(Light));
            var sun = lightGo.GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.86f, 1f);
            sun.intensity = 1.45f;
            sun.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.52f, 0.44f, 1f);

            // 2D panoramic garden backdrop covering 100% of the background down past the floor
            var bgGo = new GameObject("Backdrop", typeof(SpriteRenderer));
            var bgSr = bgGo.GetComponent<SpriteRenderer>();
            bgSr.sprite = PicnicAtlasCatalog.FullSprite("scenery-v1", new Vector2(0.5f, 0.5f));
            bgGo.transform.position = new Vector3(18.2f, 3.1f, 18f);
            if (bgSr.sprite != null)
            {
                float h = 23.0f;
                float w = h * (bgSr.sprite.bounds.size.x / bgSr.sprite.bounds.size.y);
                bgGo.transform.localScale = new Vector3(w / bgSr.sprite.bounds.size.x, h / bgSr.sprite.bounds.size.y, 1f);
            }

            // Materials for 3D diorama blocks
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var wallTex = PicnicAtlasCatalog.LoadTexture("wall-plant-v1");
            var soilTex = PicnicAtlasCatalog.LoadTexture("soil-continuous-v1");
            if (wallTex != null) wallTex.wrapMode = TextureWrapMode.Repeat;
            if (soilTex != null) soilTex.wrapMode = TextureWrapMode.Repeat;

            var topGrassMat = new Material(litShader);
            topGrassMat.color = new Color(0.54f, 0.76f, 0.24f, 1f);
            if (wallTex != null) { topGrassMat.mainTexture = wallTex; topGrassMat.mainTextureScale = new Vector2(1.4f, 1.4f); }
            topGrassMat.SetFloat("_Smoothness", 0.08f);

            var ivyFaceMat = new Material(litShader);
            ivyFaceMat.color = new Color(0.86f, 0.96f, 0.80f, 1f);
            if (wallTex != null) { ivyFaceMat.mainTexture = wallTex; ivyFaceMat.mainTextureScale = new Vector2(1.2f, 1.2f); }
            ivyFaceMat.SetFloat("_Smoothness", 0.05f);

            var earthBaseMat = new Material(litShader);
            earthBaseMat.color = new Color(0.56f, 0.40f, 0.26f, 1f);
            if (soilTex != null) { earthBaseMat.mainTexture = soilTex; earthBaseMat.mainTextureScale = new Vector2(2f, 0.8f); }
            earthBaseMat.SetFloat("_Smoothness", 0.04f);

            var shadowMat = new Material(Shader.Find("Sprites/Default"));
            shadowMat.color = new Color(0.08f, 0.14f, 0.06f, 0.36f);

            var tileGrass = PicnicAtlasCatalog.FullSprite("tile-grass", new Vector2(0.5f, 0f));

            // Extend ground blocks left/right and deep down (-3.8m) so 100% of the bottom and sides of the frame are filled!
            var steps = new (float x, float w, float topY)[]
            {
                (7.42f,  5.88f, 0.00f),
                (13.30f, 1.96f, 0.00f),
                (15.26f, 1.96f, 0.45f),
                (17.22f, 1.96f, 0.90f),
                (19.18f, 1.96f, 0.45f),
                (21.14f, 1.96f, 0.00f),
                (23.10f, 5.88f, 0.00f),
            };

            const float depthZ = 2.2f;
            const float floorY = -3.8f;

            foreach (var st in steps)
            {
                float cx = st.x + st.w * 0.5f;
                float totalH = st.topY - floorY;

                // Lush ivy-covered 3D soil wall extending all the way below the bottom of the screen
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.position = new Vector3(cx, floorY + totalH * 0.5f - 0.05f, 0f);
                wall.transform.localScale = new Vector3(st.w + 0.04f, totalH, depthZ);
                wall.GetComponent<MeshRenderer>().sharedMaterial = ivyFaceMat;

                // Slightly overhanging bright-green 3D grass turf cap
                var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cap.transform.position = new Vector3(cx, st.topY - 0.07f, 0f);
                cap.transform.localScale = new Vector3(st.w + 0.14f, 0.15f, depthZ + 0.16f);
                cap.GetComponent<MeshRenderer>().sharedMaterial = topGrassMat;

                // Multi-Z 2.5D high-res tall grass blades tiled every ~1.96m (back row, mid row, front overhanging row)
                float[] zRows = { 0.50f, 0.14f, -0.45f };
                float[] alphas = { 0.98f, 0.92f, 0.58f };
                int chunks = Mathf.Max(1, Mathf.RoundToInt(st.w / 1.96f));
                float chunkW = st.w / chunks;
                for (int c = 0; c < chunks; c++)
                {
                    float chunkCx = st.x + (c + 0.5f) * chunkW;
                    for (int r = 0; r < zRows.Length; r++)
                    {
                        var tuft = new GameObject($"GrassRow_{c}_{r}", typeof(SpriteRenderer));
                        var sr = tuft.GetComponent<SpriteRenderer>();
                        sr.sprite = tileGrass;
                        sr.color = r == 0 ? new Color(0.88f, 0.96f, 0.85f, alphas[r]) : new Color(1f, 1f, 1f, alphas[r]);
                        tuft.transform.position = new Vector3(chunkCx, st.topY - 0.16f, zRows[r]);
                        if (tileGrass != null)
                        {
                            tuft.transform.localScale = new Vector3((chunkW + 0.10f) / tileGrass.bounds.size.x, 1.08f / tileGrass.bounds.size.y, 1f);
                        }
                    }
                }
            }

            // 2D Illustrated Yuna standing on step 2 (relevo-1, X=16.24, Y=0.45) in discomfort pose
            var yunaGo = new GameObject("Yuna2D", typeof(SpriteRenderer));
            var yunaSr = yunaGo.GetComponent<SpriteRenderer>();
            yunaSr.sprite = PicnicAtlasCatalog.GetGirlSprite("girl-discomfort-v1", 1, false);
            yunaGo.transform.position = new Vector3(16.24f, 0.45f, -0.05f);

            // Soft 3D contact shadow disc on top of the 3D grass cap directly beneath Yuna's shoes
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadow.transform.position = new Vector3(16.24f, 0.46f, -0.05f);
            shadow.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            shadow.transform.localScale = new Vector3(0.75f, 0.34f, 1f);
            shadow.GetComponent<MeshRenderer>().sharedMaterial = shadowMat;

            // 2D Illustrated Blue Nanda hovering ahead of Yuna
            var nandaGo = new GameObject("NandaBlue2D", typeof(SpriteRenderer));
            var nandaSr = nandaGo.GetComponent<SpriteRenderer>();
            nandaSr.sprite = PicnicAtlasCatalog.GetBlueNandaFrame(2);
            nandaGo.transform.position = new Vector3(17.10f, 1.85f, -0.15f);

            // Render 1280x720 PNG
            const int W = 1280;
            const int H = 720;
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32);
            var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
            cam.targetTexture = rt;
            cam.aspect = (float)W / H;
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
            tex.Apply();

            string outFile = Path.Combine(outDir, "grass-terrace-3d-2d-hybrid.png");
            File.WriteAllBytes(outFile, tex.EncodeToPNG());

            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(topGrassMat);
            Object.DestroyImmediate(ivyFaceMat);
            Object.DestroyImmediate(earthBaseMat);
            Object.DestroyImmediate(shadowMat);

            Debug.Log("PICNIC_3D_PROTO_OK: " + outFile);
        }
    }
}
