using System.Collections.Generic;
using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>
    /// 3D diorama platform renderer for Picnic_Gameplay (1B faithful coordinates):
    /// renders all ground terraces, hills, wooden decks, gravel beds, river stones,
    /// floating wooden planks, rotating water wheels, and clouds as 3D meshes with URP lighting
    /// and 100% bottom-of-screen floor coverage (FloorY = -4.2f), topped by multi-Z high-res
    /// surface layers (780x390 tile-grass/gravel/soil/wood) while keeping Yuna, Nanda, and insects in 2D sprites.
    /// </summary>
    public sealed class PicnicPlatformController : MonoBehaviour
    {
        // 3D platforms start at Z = -0.06f and extend back to Z = +2.14f (DepthZ = 2.20f)
        // so the 3D top face is clearly visible in perspective while never depth-clipping Yuna (Z = -0.18f) or insects (Z = -0.14f).
        public const float FrontZ = -0.06f;
        public const float DepthZ = 2.20f;
        public const float FloorY = -4.2f;

        private readonly List<GameObject> staticRoots = new List<GameObject>();
        private readonly List<PicnicPlatformSpec> staticSpecs = new List<PicnicPlatformSpec>();
        private readonly List<CloudVisual> cloudVisuals = new List<CloudVisual>();
        private readonly List<WheelVisual> wheels = new List<WheelVisual>();

        private Material ivyFaceMat;
        private Material topGrassMat;
        private Material topMeadowMat;
        private Material earthRimMat;
        private Material topGravelMat;
        private Material topWoodMat;
        private Material woodBodyMat;
        private Material stoneMat;
        private Material stoneCapMat;
        private Material riverStone3DMat;
        private Material woodPlatform3DMat;
        private Material spokeMat;
        private Material cloudBaseMat;
        private Material grassSurfaceMat;
        private Material cornerIvyTrimMat;
        private Texture2D cornerIvyTrimTex;
        private readonly List<Mesh> grassSurfaceMeshes = new List<Mesh>();
        private readonly List<Mesh> generatedMeshes = new List<Mesh>();

        private Texture2D woodPlankTex;
        private Texture2D riverStoneTex;
        private Texture2D gravelCapTex;
        private Texture2D cloudGradTex;

        private Sprite tileSoil;
        private Sprite tileGrass;
        private Sprite tileGravel;
        private Sprite tileWood;
        private Sprite sprWoodPlat;
        private Sprite sprStonePlat;
        private Sprite sprRockPillar;

        private sealed class CloudVisual
        {
            public string Id;
            public MeshRenderer[] Renderers;
            public Material InstanceMaterial;
            public float LastAlpha = -1f;
        }

        private sealed class WheelVisual
        {
            public PicnicWheelSpec Spec;
            public GameObject Root;
            public Transform[] Platforms;
            public Transform[] Spokes;
        }

        public void Build(PicnicLevelData level)
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default");

            var wallTex = PicnicAtlasCatalog.LoadTexture("wall-plant-v1");
            var soilTex = PicnicAtlasCatalog.LoadTexture("soil-continuous-v1");
            if (wallTex != null) wallTex.wrapMode = TextureWrapMode.Repeat;
            if (soilTex != null) soilTex.wrapMode = TextureWrapMode.Repeat;

            woodPlankTex = CreateWoodPlankTexture();
            riverStoneTex = CreateRiverStoneTexture();
            gravelCapTex = CreatePebbleCapTexture();
            cloudGradTex = CreateCloudShadedTexture();

            ivyFaceMat = new Material(lit)
            {
                color = new Color(0.86f, 0.96f, 0.80f, 1f),
            };
            if (wallTex != null)
            {
                ivyFaceMat.mainTexture = wallTex;
                ivyFaceMat.mainTextureScale = new Vector2(1f, 1f);
            }
            SetSmoothness(ivyFaceMat, 0.05f);

            topGrassMat = new Material(lit)
            {
                color = new Color(0.54f, 0.76f, 0.24f, 1f),
            };
            if (wallTex != null)
            {
                topGrassMat.mainTexture = wallTex;
                topGrassMat.mainTextureScale = new Vector2(1.4f, 1.4f);
            }
            SetSmoothness(topGrassMat, 0.08f);

            // Meadow turf cap for Soil platforms (lush green garden lawn matching tile-soil & wall-plant)
            topMeadowMat = new Material(lit)
            {
                color = new Color(0.56f, 0.78f, 0.26f, 1f),
            };
            if (wallTex != null)
            {
                topMeadowMat.mainTexture = wallTex;
                topMeadowMat.mainTextureScale = new Vector2(1.3f, 1.3f);
            }
            SetSmoothness(topMeadowMat, 0.07f);

            earthRimMat = new Material(lit)
            {
                color = new Color(0.56f, 0.40f, 0.25f, 1f),
            };
            if (soilTex != null)
            {
                earthRimMat.mainTexture = soilTex;
            }
            SetSmoothness(earthRimMat, 0.05f);

            topGravelMat = new Material(lit)
            {
                color = new Color(0.72f, 0.66f, 0.60f, 1f),
                mainTexture = gravelCapTex,
            };
            SetSmoothness(topGravelMat, 0.06f);

            topWoodMat = new Material(lit)
            {
                color = new Color(1.00f, 0.94f, 0.84f, 1f),
                mainTexture = woodPlankTex,
                mainTextureScale = new Vector2(0.85f, 0.85f),
            };
            SetSmoothness(topWoodMat, 0.14f);

            woodBodyMat = new Material(lit)
            {
                color = new Color(0.78f, 0.66f, 0.54f, 1f),
                mainTexture = woodPlankTex,
                mainTextureScale = new Vector2(0.85f, 0.85f),
            };
            SetSmoothness(woodBodyMat, 0.10f);

            stoneMat = new Material(lit)
            {
                color = new Color(0.56f, 0.60f, 0.58f, 1f),
                mainTexture = riverStoneTex,
            };
            SetSmoothness(stoneMat, 0.14f);

            stoneCapMat = new Material(lit)
            {
                color = new Color(0.72f, 0.76f, 0.73f, 1f),
                mainTexture = riverStoneTex,
            };
            SetSmoothness(stoneCapMat, 0.20f);

            var stone3DTex = PicnicAtlasCatalog.LoadTexture("river-stone-3d-tex");
            riverStone3DMat = new Material(lit)
            {
                color = new Color(0.76f, 0.76f, 0.75f, 1f),
                mainTexture = stone3DTex,
            };
            SetSmoothness(riverStone3DMat, 0.04f);

            var wood3DTex = PicnicAtlasCatalog.LoadTexture("wood-platform-3d-tex");
            woodPlatform3DMat = new Material(lit)
            {
                color = new Color(0.80f, 0.78f, 0.76f, 1f),
                mainTexture = wood3DTex,
            };
            SetSmoothness(woodPlatform3DMat, 0.05f);

            spokeMat = new Material(lit)
            {
                color = new Color(0.72f, 0.52f, 0.33f, 1f),
                mainTexture = woodPlankTex,
            };
            SetSmoothness(spokeMat, 0.15f);

            var cloud3DTex = PicnicAtlasCatalog.LoadTexture("cloud-3d-tex");
            Shader cloudShader = Shader.Find("Sprites/Default") ?? lit;
            cloudBaseMat = new Material(cloudShader)
            {
                mainTexture = cloud3DTex != null ? cloud3DTex : cloudGradTex,
                color = Color.white,
            };

            cornerIvyTrimTex = CreateCornerIvyTrimTexture();
            cornerIvyTrimMat = new Material(Shader.Find("Sprites/Default") ?? lit)
            {
                mainTexture = cornerIvyTrimTex,
                color = Color.white,
            };

            // Pivot at bottom-center (0.5, 0) for multi-Z 3D surface placement
            tileSoil = PicnicAtlasCatalog.FullSprite("tile-soil", new Vector2(0.5f, 0f));
            tileGrass = PicnicAtlasCatalog.FullSprite("tile-grass", new Vector2(0.5f, 0f));
            if (tileGrass != null)
            {
                grassSurfaceMat = new Material(Shader.Find("Sprites/Default"))
                {
                    mainTexture = tileGrass.texture,
                };
            }
            tileGravel = PicnicAtlasCatalog.FullSprite("tile-gravel", new Vector2(0.5f, 0f));
            tileWood = PicnicAtlasCatalog.FullSprite("tile-wood", new Vector2(0.5f, 0f));
            sprWoodPlat = PicnicAtlasCatalog.Slice("objects-v1", PicnicAtlasCatalog.ObjetosFrames[1], 100f, "plat:wood");
            sprStonePlat = PicnicAtlasCatalog.Slice("objects-v1", PicnicAtlasCatalog.ObjetosFrames[2], 100f, "plat:stone");
            sprRockPillar = PicnicAtlasCatalog.Slice("life-aquatic-v1", PicnicAtlasCatalog.AquaticFrames[0], 100f, "plat:pillar");

            foreach (var p in level.StaticPlatforms)
            {
                BuildStaticPlatform(p, level.Width, level.StaticPlatforms);
            }
            foreach (var w in level.Wheels)
            {
                BuildWheel(w);
            }
        }

        private static void SetSmoothness(Material mat, float value)
        {
            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", value);
            }
            if (mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", value);
            }
        }

        public void Apply(PicnicSimulation sim)
        {
            float px = sim.PlayerX;
            for (int i = 0; i < staticRoots.Count; i++)
            {
                var p = staticSpecs[i];
                bool near = p.X + p.Width >= px - 1250f && p.X <= px + 1250f;
                bool active = near;
                if (near && p.Terrain == PicnicTerrain.Cloud)
                {
                    var st = PicnicLevelData.GetCloudStatus(p.Id, sim.Clouds, sim.Difficulty, sim.Time);
                    active = st.Solid;
                }
                if (staticRoots[i].activeSelf != active)
                {
                    staticRoots[i].SetActive(active);
                }
            }

            for (int i = 0; i < cloudVisuals.Count; i++)
            {
                var cv = cloudVisuals[i];
                if (cv.Renderers.Length == 0 || !cv.Renderers[0].gameObject.activeInHierarchy) continue;
                var st = PicnicLevelData.GetCloudStatus(cv.Id, sim.Clouds, sim.Difficulty, sim.Time);
                if (!Mathf.Approximately(cv.LastAlpha, st.Alpha))
                {
                    cv.LastAlpha = st.Alpha;
                    cv.InstanceMaterial.color = new Color(1f, 1f, 1f, st.Alpha);
                }
            }

            for (int i = 0; i < wheels.Count; i++)
            {
                var wv = wheels[i];
                bool near = Mathf.Abs(wv.Spec.X - px) < 1250f;
                if (wv.Root.activeSelf != near)
                {
                    wv.Root.SetActive(near);
                }
                if (!near) continue;
                UpdateWheelTransforms(wv, sim.Time * wv.Spec.Speed + wv.Spec.Phase);
            }
        }

        public void ShowAllForEditorPreview(PicnicSimulation sim)
        {
            for (int i = 0; i < staticRoots.Count; i++)
            {
                staticRoots[i].SetActive(true);
            }
            for (int i = 0; i < cloudVisuals.Count; i++)
            {
                cloudVisuals[i].InstanceMaterial.color = Color.white;
            }
            for (int i = 0; i < wheels.Count; i++)
            {
                var wv = wheels[i];
                wv.Root.SetActive(true);
                UpdateWheelTransforms(wv, wv.Spec.Phase);
            }
        }

        private static void UpdateWheelTransforms(WheelVisual wv, float baseAngle)
        {
            Vector3 hub = PicnicAtlasCatalog.ToUnity(wv.Spec.X, wv.Spec.Y, FrontZ + 0.56f);
            for (int k = 0; k < 4; k++)
            {
                float angle = baseAngle + k * Mathf.PI * 0.5f;
                float platX = wv.Spec.X + Mathf.Cos(angle) * wv.Spec.Rx;
                float platY = wv.Spec.Y + Mathf.Sin(angle) * wv.Spec.Ry;
                Vector3 platPos = PicnicAtlasCatalog.ToUnity(platX, platY + 6f, FrontZ + 0.48f);
                wv.Platforms[k].position = platPos;

                Vector3 spokeEnd = PicnicAtlasCatalog.ToUnity(platX, platY + 18f, FrontZ + 0.56f);
                Vector3 delta = spokeEnd - hub;
                float len = delta.magnitude;
                wv.Spokes[k].position = hub + delta * 0.5f;
                wv.Spokes[k].localScale = new Vector3(0.09f, len, 0.09f);
                if (len > 0.001f)
                {
                    wv.Spokes[k].up = delta / len;
                }
            }
        }

        private void BuildStaticPlatform(PicnicPlatformSpec p, float levelWidth, IReadOnlyList<PicnicPlatformSpec> allPlatforms)
        {
            var go = new GameObject(p.Id);
            go.transform.SetParent(transform, false);
            staticRoots.Add(go);
            staticSpecs.Add(p);

            if (p.Ground)
            {
                Build3DGroundPlatform(go.transform, p, levelWidth, allPlatforms);
                return;
            }

            if (p.Aerial && p.Terrain == PicnicTerrain.Gravel)
            {
                Build3DAerialGravelPlatform(go.transform, p);
                return;
            }

            if (p.Terrain == PicnicTerrain.Stone)
            {
                Build3DRiverStonePlatform(go.transform, p);
                return;
            }

            if (p.Terrain == PicnicTerrain.Cloud)
            {
                Build3DCloudPlatform(go.transform, p);
                return;
            }

            Build3DFloatingWoodPlatform(go.transform, p);
        }

        private static bool IsGroundCornerExposed(PicnicPlatformSpec p, IReadOnlyList<PicnicPlatformSpec> allPlatforms, bool isRight, float levelWidth)
        {
            if (!isRight && p.X <= 1f) return false;
            if (isRight && p.X + p.Width >= levelWidth - 1f) return false;

            float edgeX = isRight ? (p.X + p.Width) : p.X;
            if (allPlatforms == null) return true;

            for (int i = 0; i < allPlatforms.Count; i++)
            {
                PicnicPlatformSpec q = allPlatforms[i];
                if (q == null || q == p || !q.Ground) continue;
                if (edgeX >= q.X - 6f && edgeX <= q.X + q.Width + 6f)
                {
                    // In top-down pixel Y, smaller Y means higher in world space.
                    // If an adjacent ground platform is at the same height or taller, this corner is buried.
                    if (q.Y <= p.Y + 4f)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void Build3DGroundPlatform(Transform parent, PicnicPlatformSpec p, float levelWidth, IReadOnlyList<PicnicPlatformSpec> allPlatforms)
        {
            float extraLeft = p.X <= 1f ? 320f : 0f;
            float extraRight = (p.X + p.Width >= levelWidth - 1f) ? 320f : 0f;
            float startPx = p.X - extraLeft;
            float totalWidthPx = p.Width + extraLeft + extraRight;

            Material topMat = p.Terrain == PicnicTerrain.Grass ? topGrassMat
                : p.Terrain == PicnicTerrain.Gravel ? topGravelMat
                : p.Terrain == PicnicTerrain.Wood ? topWoodMat
                : topMeadowMat;
            Material faceMat = p.Terrain == PicnicTerrain.Wood ? woodBodyMat : ivyFaceMat;

            bool leftExposed = IsGroundCornerExposed(p, allPlatforms, false, levelWidth);
            bool rightExposed = IsGroundCornerExposed(p, allPlatforms, true, levelWidth);

            if (p.Hill <= 0f)
            {
                float xMin = startPx / 100f;
                float w = totalWidthPx / 100f;
                float topY = PicnicAtlasCatalog.ToUnity(p.X, p.Y).y;

                float cornerRadius = p.Terrain == PicnicTerrain.Wood ? 0.28f : 0.42f;
                float rTopLeft = leftExposed ? cornerRadius : 0f;
                float rTopRight = rightExposed ? cornerRadius : 0f;
                float frontBevel = p.Terrain == PicnicTerrain.Wood ? 0.06f : 0.085f;

                // 3D wall block with generous rounded top shoulders on exposed corners (lower body remains full width to seal cleanly against river water)
                var wallGo = new GameObject("Wall3D", typeof(MeshFilter), typeof(MeshRenderer));
                wallGo.transform.SetParent(parent, false);
                Mesh wallMesh = CreateRoundedTopExtrudedMesh(
                    xMin - 0.02f, w + 0.04f, FloorY, topY - 0.02f, FrontZ + 0.04f, DepthZ - 0.04f,
                    rTopLeft, rTopRight, 0f, 0f, frontBevel * 0.7f, 3.2f);
                generatedMeshes.Add(wallMesh);
                wallGo.GetComponent<MeshFilter>().sharedMesh = wallMesh;
                wallGo.GetComponent<MeshRenderer>().sharedMaterial = faceMat;

                // Generous overhanging rounded 3D top turf/deck/meadow/gravel cap matching the full rounded shoulder arc
                var capGo = new GameObject("TopCap3D", typeof(MeshFilter), typeof(MeshRenderer));
                capGo.transform.SetParent(parent, false);
                float capHeight = p.Terrain == PicnicTerrain.Wood ? 0.34f : 0.46f;
                float capRadiusLeft = leftExposed ? cornerRadius : 0f;
                float capRadiusRight = rightExposed ? cornerRadius : 0f;
                float capBotLeft = leftExposed ? 0.05f : 0f;
                float capBotRight = rightExposed ? 0.05f : 0f;

                if (p.Terrain == PicnicTerrain.Gravel)
                {
                    Mesh capMesh = CreateRoundedTopExtrudedMesh(
                        xMin - 0.06f, w + 0.12f, topY - capHeight, topY - 0.01f, FrontZ, DepthZ + 0.08f,
                        capRadiusLeft, capRadiusRight, capBotLeft, capBotRight, frontBevel, 3.2f);
                    generatedMeshes.Add(capMesh);
                    capGo.GetComponent<MeshFilter>().sharedMesh = capMesh;
                    capGo.GetComponent<MeshRenderer>().sharedMaterial = topMeadowMat;

                    float pebbleInsetL = leftExposed ? 0.16f : 0f;
                    float pebbleInsetR = rightExposed ? 0.16f : 0f;
                    float pebbleX = p.X / 100f + pebbleInsetL;
                    float pebbleW = Mathf.Max(0.24f, p.Width / 100f - pebbleInsetL - pebbleInsetR);

                    var earthBed = new GameObject("EarthBed3D", typeof(MeshFilter), typeof(MeshRenderer));
                    earthBed.transform.SetParent(parent, false);
                    Mesh earthMesh = CreateRoundedTopExtrudedMesh(
                        pebbleX - 0.02f, pebbleW + 0.04f, topY - 0.03f, topY + 0.01f, FrontZ + 0.05f, DepthZ - 0.04f,
                        0.02f, 0.02f, 0f, 0f, 0.015f, 2.0f);
                    generatedMeshes.Add(earthMesh);
                    earthBed.GetComponent<MeshFilter>().sharedMesh = earthMesh;
                    earthBed.GetComponent<MeshRenderer>().sharedMaterial = earthRimMat;

                    var pebbleField = new GameObject("SculptedPebbles3D", typeof(MeshFilter), typeof(MeshRenderer));
                    pebbleField.transform.SetParent(parent, false);
                    Mesh pebbleMesh = CreateSculpted3DPebbleFieldMesh(
                        pebbleX, pebbleW, topY + 0.005f, FrontZ + 0.06f, DepthZ - 0.12f);
                    generatedMeshes.Add(pebbleMesh);
                    pebbleField.GetComponent<MeshFilter>().sharedMesh = pebbleMesh;
                    pebbleField.GetComponent<MeshRenderer>().sharedMaterial = topGravelMat;
                }
                else
                {
                    Mesh capMesh = CreateRoundedTopExtrudedMesh(
                        xMin - 0.06f, w + 0.12f, topY - capHeight, topY, FrontZ, DepthZ + 0.08f,
                        capRadiusLeft, capRadiusRight, capBotLeft, capBotRight, frontBevel,
                        p.Terrain == PicnicTerrain.Wood ? 1.6f : 3.2f);
                    generatedMeshes.Add(capMesh);
                    capGo.GetComponent<MeshFilter>().sharedMesh = capMesh;
                    capGo.GetComponent<MeshRenderer>().sharedMaterial = topMat;
                }

                if (p.Terrain == PicnicTerrain.Wood)
                {
                    BuildWoodenDeckPilings(parent, xMin, w, topY, leftExposed, rightExposed);
                }

                // Painterly hanging ivy/blossom corner sprigs hugging the rounded shoulders
                float platLeftX = p.X / 100f;
                float platRightX = (p.X + p.Width) / 100f;
                if (leftExposed)
                {
                    BuildCornerOrnament(parent, platLeftX, topY, FrontZ, false, p.Terrain, p.Id.GetHashCode() ^ 31);
                }
                if (rightExposed)
                {
                    BuildCornerOrnament(parent, platRightX, topY, FrontZ, true, p.Terrain, p.Id.GetHashCode() ^ 97);
                }
            }
            else
            {
                BuildExtrudedHill3DMesh(parent, p, startPx, totalWidthPx, faceMat, topMat);
            }

            // Multi-Z high-resolution surface layers (780x390) on top of the 3D cap
            BuildMultiZSurfaceLayers(parent, p, p.Terrain, p.X, p.Width);

            foreach (var patch in p.GrassPatches)
            {
                float px = p.X + p.Width * patch.Start;
                float pw = p.Width * (patch.End - patch.Start);
                BuildMultiZSurfaceLayers(parent, p, PicnicTerrain.Grass, px, pw);
            }
        }

        private void BuildWoodenDeckPilings(Transform parent, float xMin, float width, float topY, bool leftExposed, bool rightExposed)
        {
            float step = 1.85f;
            float startX = xMin + (leftExposed ? 0.55f : 0.45f);
            float endX = xMin + width - (rightExposed ? 0.48f : 0.20f);
            for (float x = startX; x < endX; x += step)
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.name = "DeckPost3D";
                post.transform.SetParent(parent, false);
                var col = post.GetComponent<Collider>();
                if (col != null)
                {
                    if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);
                }
                float h = topY - FloorY;
                post.transform.position = new Vector3(x, FloorY + h * 0.5f - 0.16f, FrontZ + 0.02f);
                post.transform.localScale = new Vector3(0.20f, h - 0.16f, 0.16f);
                post.GetComponent<MeshRenderer>().sharedMaterial = spokeMat;
            }
        }

        private void BuildExtrudedHill3DMesh(Transform parent, PicnicPlatformSpec p,
            float startPx, float totalWidthPx, Material faceMat, Material topMat)
        {
            int cols = Mathf.Max(12, Mathf.CeilToInt(totalWidthPx / 26f));
            float zFront = FrontZ + 0.04f;

            // 1. Front 3D ivy wall mesh extending down to FloorY = -4.2f
            var wallVerts = new Vector3[(cols + 1) * 2];
            var wallUvs = new Vector2[wallVerts.Length];
            var wallNormals = new Vector3[wallVerts.Length];
            var wallTris = new int[cols * 6];

            // 2. Top curved 3D cap ribbon with upward normals so it catches 3D directional sunlight
            var capVerts = new Vector3[(cols + 1) * 4];
            var capUvs = new Vector2[capVerts.Length];
            var capNormals = new Vector3[capVerts.Length];
            var capTris = new int[cols * 12];
            float capZFront = FrontZ;
            float capZBack = FrontZ + DepthZ + 0.08f;

            for (int i = 0; i <= cols; i++)
            {
                float wxPx = startPx + totalWidthPx * i / cols;
                float samplePx = Mathf.Clamp(wxPx, p.X, p.X + p.Width);
                float wyPx = PicnicLevelData.SurfaceY(p, samplePx);
                Vector3 topPt = PicnicAtlasCatalog.ToUnity(wxPx, wyPx);

                wallVerts[i * 2] = new Vector3(topPt.x, topPt.y - 0.04f, zFront);
                wallVerts[i * 2 + 1] = new Vector3(topPt.x, FloorY, zFront);
                wallUvs[i * 2] = new Vector2(topPt.x / 3.2f, topPt.y / 3.2f);
                wallUvs[i * 2 + 1] = new Vector2(topPt.x / 3.2f, FloorY / 3.2f);
                wallNormals[i * 2] = Vector3.back;
                wallNormals[i * 2 + 1] = Vector3.back;

                capVerts[i * 4] = new Vector3(topPt.x, topPt.y, capZFront);
                capVerts[i * 4 + 1] = new Vector3(topPt.x, topPt.y, capZBack);
                capVerts[i * 4 + 2] = new Vector3(topPt.x, topPt.y, capZFront);
                capVerts[i * 4 + 3] = new Vector3(topPt.x, topPt.y - 0.14f, capZFront);

                capUvs[i * 4] = new Vector2(topPt.x / 3.2f, capZFront / 3.2f);
                capUvs[i * 4 + 1] = new Vector2(topPt.x / 3.2f, capZBack / 3.2f);
                capUvs[i * 4 + 2] = new Vector2(topPt.x / 3.2f, topPt.y / 3.2f);
                capUvs[i * 4 + 3] = new Vector2(topPt.x / 3.2f, (topPt.y - 0.14f) / 3.2f);

                capNormals[i * 4] = Vector3.up;
                capNormals[i * 4 + 1] = Vector3.up;
                capNormals[i * 4 + 2] = Vector3.back;
                capNormals[i * 4 + 3] = Vector3.back;

                if (i == cols) continue;
                int v = i * 2;
                int t = i * 6;
                wallTris[t] = v; wallTris[t + 1] = v + 2; wallTris[t + 2] = v + 1;
                wallTris[t + 3] = v + 2; wallTris[t + 4] = v + 3; wallTris[t + 5] = v + 1;

                int cv = i * 4;
                int ct = i * 12;
                capTris[ct] = cv; capTris[ct + 1] = cv + 1; capTris[ct + 2] = cv + 4;
                capTris[ct + 3] = cv + 4; capTris[ct + 4] = cv + 1; capTris[ct + 5] = cv + 5;
                capTris[ct + 6] = cv + 2; capTris[ct + 7] = cv + 6; capTris[ct + 8] = cv + 3;
                capTris[ct + 9] = cv + 6; capTris[ct + 10] = cv + 7; capTris[ct + 11] = cv + 3;
            }

            var wallMesh = new Mesh { name = p.Id + "_hillWall3D" };
            wallMesh.vertices = wallVerts;
            wallMesh.uv = wallUvs;
            wallMesh.normals = wallNormals;
            wallMesh.triangles = wallTris;

            var wallGo = new GameObject("HillWall3D", typeof(MeshFilter), typeof(MeshRenderer));
            wallGo.transform.SetParent(parent, false);
            wallGo.GetComponent<MeshFilter>().sharedMesh = wallMesh;
            wallGo.GetComponent<MeshRenderer>().sharedMaterial = faceMat;

            var capMesh = new Mesh { name = p.Id + "_hillCap3D" };
            capMesh.vertices = capVerts;
            capMesh.uv = capUvs;
            capMesh.normals = capNormals;
            capMesh.triangles = capTris;

            var capGo = new GameObject("HillCap3D", typeof(MeshFilter), typeof(MeshRenderer));
            capGo.transform.SetParent(parent, false);
            capGo.GetComponent<MeshFilter>().sharedMesh = capMesh;
            capGo.GetComponent<MeshRenderer>().sharedMaterial = topMat;
        }

        private void BuildMultiZSurfaceLayers(Transform parent, PicnicPlatformSpec p, PicnicTerrain terrain,
            float startX, float width)
        {
            if (terrain == PicnicTerrain.Grass)
            {
                if (tileGrass == null) return;
                // Keep every row inside the cap; each blade strip follows the ground along its full width.
                float[] zRows = { 1.15f, 0.42f, FrontZ + 0.02f };
                float[] alphas = { 0.98f, 0.92f, 0.56f };
                int[] orders = { 3, 5, 15 };
                int chunks = Mathf.Max(1, Mathf.RoundToInt(width / 196f));
                float chunkW = width / chunks;
                for (int c = 0; c < chunks; c++)
                {
                    for (int r = 0; r < zRows.Length; r++)
                    {
                        var tuft = new GameObject($"Grass3DRow_{c}_{r}", typeof(MeshFilter), typeof(MeshRenderer));
                        tuft.transform.SetParent(parent, false);
                        Color tint = r == 0
                            ? new Color(0.88f, 0.96f, 0.85f, alphas[r])
                            : new Color(1f, 1f, 1f, alphas[r]);
                        Mesh mesh = CreateGroundedGrassMesh(p, startX + c * chunkW, chunkW, tint);
                        grassSurfaceMeshes.Add(mesh);
                        tuft.GetComponent<MeshFilter>().sharedMesh = mesh;
                        var renderer = tuft.GetComponent<MeshRenderer>();
                        renderer.sharedMaterial = grassSurfaceMat;
                        renderer.sortingOrder = orders[r];
                        tuft.transform.localPosition = new Vector3(0f, 0f, zRows[r]);
                    }
                }
                return;
            }

            if (terrain == PicnicTerrain.Gravel)
            {
                // 3D sculpted pebbles already cover the entire 3D top surface
                return;
            }

            // Soil (meadow turf) and Wood (house deck) are 100% 3D blocks (TopCap3D / HillCap3D);
            // do not place 2D vertical strip cutouts (tile-soil / tile-wood) on top of them.
        }

        private static Mesh CreateGroundedGrassMesh(PicnicPlatformSpec platform, float startX, float width, Color tint)
        {
            int columns = Mathf.Max(1, Mathf.CeilToInt(width / 13f));
            var vertices = new Vector3[(columns + 1) * 2];
            var uvs = new Vector2[vertices.Length];
            var colors = new Color[vertices.Length];
            var triangles = new int[columns * 6];

            for (int i = 0; i <= columns; i++)
            {
                float u = (float)i / columns;
                float x = startX + width * u;
                Vector3 surface = PicnicAtlasCatalog.ToUnity(x, PicnicLevelData.SurfaceY(platform, x));
                // The texture has 48px of transparent padding below its roots (390px canvas).
                // Embed that padding in the cap, keeping the visible roots at ground level.
                vertices[i * 2] = new Vector3(surface.x, surface.y - 0.14f, 0f);
                vertices[i * 2 + 1] = vertices[i * 2] + Vector3.up * 1.06f;
                uvs[i * 2] = new Vector2(u, 0f);
                uvs[i * 2 + 1] = new Vector2(u, 1f);
                colors[i * 2] = tint;
                colors[i * 2 + 1] = tint;

                if (i == columns)
                {
                    continue;
                }

                int v = i * 2;
                int t = i * 6;
                triangles[t] = v;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 2;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 1;
                triangles[t + 5] = v + 3;
            }

            var mesh = new Mesh { name = platform.Id + "_groundedGrass" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void Build3DAerialGravelPlatform(Transform parent, PicnicPlatformSpec p)
        {
            float xMin = p.X / 100f;
            float w = p.Width / 100f;
            float topY = PicnicAtlasCatalog.ToUnity(p.X, p.Y).y;
            float depth = 1.40f;

            var body = new GameObject("AerialIsland3D", typeof(MeshFilter), typeof(MeshRenderer));
            body.transform.SetParent(parent, false);
            Mesh bodyMesh = CreateRoundedTopExtrudedMesh(
                xMin, w, topY - 0.54f, topY - 0.05f, FrontZ + 0.04f, depth,
                0.20f, 0.20f, 0.22f, 0.22f, 0.065f, 3.2f);
            generatedMeshes.Add(bodyMesh);
            body.GetComponent<MeshFilter>().sharedMesh = bodyMesh;
            body.GetComponent<MeshRenderer>().sharedMaterial = ivyFaceMat;

            var under = new GameObject("AerialBase3D", typeof(MeshFilter), typeof(MeshRenderer));
            under.transform.SetParent(parent, false);
            Mesh underMesh = CreateRoundedTopExtrudedMesh(
                xMin + w * 0.12f, w * 0.76f, topY - 0.76f, topY - 0.50f, FrontZ + 0.18f, depth * 0.76f,
                0.10f, 0.10f, 0.16f, 0.16f, 0.05f, 3.2f);
            generatedMeshes.Add(underMesh);
            under.GetComponent<MeshFilter>().sharedMesh = underMesh;
            under.GetComponent<MeshRenderer>().sharedMaterial = earthRimMat;

            var cap = new GameObject("AerialGravelCap3D", typeof(MeshFilter), typeof(MeshRenderer));
            cap.transform.SetParent(parent, false);
            Mesh capMesh = CreateRoundedTopExtrudedMesh(
                xMin - 0.05f, w + 0.10f, topY - 0.16f, topY - 0.01f, FrontZ, depth + 0.10f,
                0.14f, 0.14f, 0.06f, 0.06f, 0.065f, 3.2f);
            generatedMeshes.Add(capMesh);
            cap.GetComponent<MeshFilter>().sharedMesh = capMesh;
            cap.GetComponent<MeshRenderer>().sharedMaterial = topMeadowMat;

            var pebbleField = new GameObject("AerialSculptedPebbles3D", typeof(MeshFilter), typeof(MeshRenderer));
            pebbleField.transform.SetParent(parent, false);
            Mesh pebbleMesh = CreateSculpted3DPebbleFieldMesh(
                xMin, w, topY + 0.005f, FrontZ + 0.05f, depth - 0.06f);
            generatedMeshes.Add(pebbleMesh);
            pebbleField.GetComponent<MeshFilter>().sharedMesh = pebbleMesh;
            pebbleField.GetComponent<MeshRenderer>().sharedMaterial = topGravelMat;

            BuildCornerOrnament(parent, xMin, topY, FrontZ, false, PicnicTerrain.Gravel, p.Id.GetHashCode() ^ 17);
            BuildCornerOrnament(parent, xMin + w, topY, FrontZ, true, PicnicTerrain.Gravel, p.Id.GetHashCode() ^ 53);

            BuildMultiZSurfaceLayers(parent, p, PicnicTerrain.Gravel, p.X, p.Width);
        }

        private void Build3DRiverStonePlatform(Transform parent, PicnicPlatformSpec p)
        {
            Mesh stoneMesh = PicnicAtlasCatalog.LoadRiverStone3DMesh();
            if (stoneMesh != null)
            {
                float w = p.Width / 100f;
                float cx = (p.X + p.Width * 0.5f) / 100f;
                float topY = PicnicAtlasCatalog.ToUnity(p.X, p.Y).y;
                float bedY = PicnicAtlasCatalog.ToUnity(p.X, PicnicLevelData.RiverBedY + 26f).y;
                float totalH = Mathf.Max(1.65f, topY - bedY);

                var stoneGo = new GameObject("RiverStone3D", typeof(MeshFilter), typeof(MeshRenderer));
                stoneGo.transform.SetParent(parent, false);
                stoneGo.GetComponent<MeshFilter>().sharedMesh = stoneMesh;
                stoneGo.GetComponent<MeshRenderer>().sharedMaterial = riverStone3DMat;

                // Compensate perspective pitch (-0.07f Y at Z = FrontZ + 0.56f) so Yuna's feet at (topY, Z = -0.15f)
                // rest right in the middle of the flat top plateau of the 3D stone.
                stoneGo.transform.position = new Vector3(cx, topY - 0.07f, FrontZ + 0.56f);
                stoneGo.transform.localScale = new Vector3(w * 1.04f, totalH, 1.24f);
                return;
            }
        }

        private static void FitSprite(Transform t, Sprite s, float worldW, float worldH)
        {
            if (s == null || s.bounds.size.x <= 0.0001f || s.bounds.size.y <= 0.0001f) return;
            t.localScale = new Vector3(worldW / s.bounds.size.x, worldH / s.bounds.size.y, 1f);
        }

        private void Build3DFloatingWoodPlatform(Transform parent, PicnicPlatformSpec p)
        {
            Mesh woodMesh = PicnicAtlasCatalog.LoadWoodPlatform3DMesh();
            if (woodMesh != null)
            {
                float w = p.Width / 100f;
                float cx = (p.X + p.Width * 0.5f) / 100f;
                float topY = PicnicAtlasCatalog.ToUnity(p.X, p.Y).y;

                var woodGo = new GameObject("WoodPlatform3D", typeof(MeshFilter), typeof(MeshRenderer));
                woodGo.transform.SetParent(parent, false);
                woodGo.GetComponent<MeshFilter>().sharedMesh = woodMesh;
                woodGo.GetComponent<MeshRenderer>().sharedMaterial = woodPlatform3DMat;

                // Compensate perspective pitch (-0.06f Y at Z = FrontZ + 0.48f) so Yuna's feet rest on the center of the top plank
                woodGo.transform.position = new Vector3(cx, topY - 0.06f, FrontZ + 0.48f);
                woodGo.transform.localScale = new Vector3(w * 1.04f, 0.56f, 0.92f);
                return;
            }
        }

        private void Build3DCloudPlatform(Transform parent, PicnicPlatformSpec p)
        {
            float cx = (p.X + p.Width * 0.5f) / 100f;
            float w = p.Width / 100f;
            float topY = PicnicAtlasCatalog.ToUnity(p.X, p.Y).y;

            var instMat = new Material(cloudBaseMat);
            Mesh cloudMesh = PicnicAtlasCatalog.LoadCloud3DMesh();
            if (cloudMesh != null)
            {
                var cloudGo = new GameObject("Cloud3D", typeof(MeshFilter), typeof(MeshRenderer));
                cloudGo.transform.SetParent(parent, false);
                cloudGo.GetComponent<MeshFilter>().sharedMesh = cloudMesh;
                var mr = cloudGo.GetComponent<MeshRenderer>();
                mr.sharedMaterial = instMat;

                // Compensate perspective pitch (-0.06f Y at Z = FrontZ + 0.48f) so Yuna's feet rest on the fluffy top cushion of the cloud
                cloudGo.transform.position = new Vector3(cx, topY - 0.06f, FrontZ + 0.48f);
                cloudGo.transform.localScale = new Vector3(w * 1.06f, 0.64f, 0.96f);

                cloudVisuals.Add(new CloudVisual
                {
                    Id = p.Id,
                    Renderers = new[] { mr },
                    InstanceMaterial = instMat,
                });
                return;
            }
        }

        private void BuildWheel(PicnicWheelSpec w)
        {
            var root = new GameObject("Wheel3D_" + w.Id);
            root.transform.SetParent(transform, false);

            var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "Hub3D";
            hub.transform.SetParent(root.transform, false);
            Destroy(hub.GetComponent<Collider>());
            hub.transform.position = PicnicAtlasCatalog.ToUnity(w.X, w.Y, FrontZ + 0.56f);
            hub.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            hub.transform.localScale = new Vector3(0.42f, 0.34f, 0.42f);
            hub.GetComponent<MeshRenderer>().sharedMaterial = woodBodyMat;

            Mesh woodMesh = PicnicAtlasCatalog.LoadWoodPlatform3DMesh();
            var plats = new Transform[4];
            var spokes = new Transform[4];
            for (int k = 0; k < 4; k++)
            {
                var spokeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spokeGo.name = "Spoke3D_" + k;
                spokeGo.transform.SetParent(root.transform, false);
                Destroy(spokeGo.GetComponent<Collider>());
                spokeGo.GetComponent<MeshRenderer>().sharedMaterial = spokeMat;
                spokes[k] = spokeGo.transform;

                var platRoot = new GameObject(w.Id + "-" + k, typeof(MeshFilter), typeof(MeshRenderer));
                platRoot.transform.SetParent(root.transform, false);
                platRoot.GetComponent<MeshFilter>().sharedMesh = woodMesh;
                platRoot.GetComponent<MeshRenderer>().sharedMaterial = woodPlatform3DMat;
                platRoot.transform.localScale = new Vector3(1.50f, 0.56f, 0.92f);
                plats[k] = platRoot.transform;
            }

            var wv = new WheelVisual { Spec = w, Root = root, Platforms = plats, Spokes = spokes };
            wheels.Add(wv);
            UpdateWheelTransforms(wv, w.Phase);
        }

        public static Texture2D CreateWoodPlankTexture()
        {
            const int W = 256;
            const int H = 256;
            const int PlankH = 64;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
            };

            Color seamDark = new Color(0.20f, 0.09f, 0.03f, 1f);
            Color deepGrain = new Color(0.42f, 0.21f, 0.08f, 1f);
            Color midCedar = new Color(0.72f, 0.42f, 0.18f, 1f);
            Color warmAmber = new Color(0.92f, 0.66f, 0.30f, 1f);
            Color bevelHighlight = new Color(0.98f, 0.82f, 0.48f, 1f);

            for (int y = 0; y < H; y++)
            {
                int plankRow = y / PlankH;
                int localY = y % PlankH;
                float rowSeed = plankRow * 17.3f;
                int jointX = (plankRow % 2 == 0) ? 128 : 24;
                float boardVariation = 0.90f + ((plankRow * 37) % 10) * 0.018f;

                for (int x = 0; x < W; x++)
                {
                    // Horizontal plank seam + warm top bevel + shaded bottom edge
                    if (localY <= 3)
                    {
                        tex.SetPixel(x, y, seamDark);
                        continue;
                    }

                    // Staggered vertical plank end-joint seam
                    int dxJoint = Mathf.Abs(x - jointX);
                    if (dxJoint <= 2)
                    {
                        tex.SetPixel(x, y, Color.Lerp(seamDark, deepGrain, 0.35f));
                        continue;
                    }

                    // Organic wood grain rings & flowing cedar fibers (periodic across W=256)
                    float fx = (x / (float)W) * Mathf.PI * 2f;
                    float fy = localY / (float)PlankH;
                    float warp = Mathf.Sin(fx * 2f + rowSeed) * 2.4f
                               + Mathf.Sin(fx * 5f - rowSeed * 0.7f) * 0.9f;
                    float knotDx = ((x - (jointX + 76 + plankRow * 41) % W + W) % W) - 128f;
                    float knotDy = (localY - PlankH * 0.52f) * 2.4f;
                    float knotDist = Mathf.Sqrt(knotDx * knotDx * 0.06f + knotDy * knotDy * 0.16f);
                    float knotRing = Mathf.Exp(-knotDist * 0.35f) * Mathf.Sin(knotDist * 2.6f) * 0.35f;

                    float fiberWave = Mathf.Sin((localY * 0.42f) + warp + knotRing * 4f) * 0.5f + 0.5f;
                    float fineStreak = Mathf.Sin(localY * 1.65f + Mathf.Sin(fx * 4f + rowSeed) * 1.8f) * 0.5f + 0.5f;
                    float tGrain = Mathf.Clamp01(fiberWave * 0.62f + fineStreak * 0.28f + knotRing * 0.25f);

                    Color col = tGrain < 0.45f
                        ? Color.Lerp(deepGrain, midCedar, tGrain / 0.45f)
                        : Color.Lerp(midCedar, warmAmber, (tGrain - 0.45f) / 0.55f);

                    // Top edge highlight bevel & bottom edge shadow per plank for crisp hand-painted depth
                    if (localY >= PlankH - 4)
                    {
                        col = Color.Lerp(col, bevelHighlight, 0.42f);
                    }
                    else if (localY <= 8)
                    {
                        col = Color.Lerp(col, deepGrain, 0.38f);
                    }

                    // Small dark iron nail heads near vertical plank joints
                    if (dxJoint >= 6 && dxJoint <= 10 && (Mathf.Abs(localY - 14) <= 2 || Mathf.Abs(localY - (PlankH - 14)) <= 2))
                    {
                        col = Color.Lerp(col, seamDark, 0.78f);
                    }

                    col.r = Mathf.Clamp01(col.r * boardVariation);
                    col.g = Mathf.Clamp01(col.g * boardVariation);
                    col.b = Mathf.Clamp01(col.b * boardVariation);
                    tex.SetPixel(x, y, col);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateRiverStoneTexture()
        {
            const int W = 128;
            const int H = 128;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
            };
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    float n1 = Mathf.Sin(x * 0.19f + y * 0.13f) * Mathf.Cos(x * 0.11f - y * 0.17f);
                    float n2 = Mathf.Sin(x * 0.47f - y * 0.39f) * 0.35f;
                    float v = Mathf.Clamp(0.84f + (n1 + n2) * 0.12f, 0.62f, 0.98f);
                    tex.SetPixel(x, y, new Color(v * 0.94f, v * 0.98f, v * 0.95f, 1f));
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePebbleCapTexture()
        {
            // 4x4 palette atlas of natural river pebble tones from ambient-v2 (warm ochre, amber, cream, sandstone, slate-grey)
            const int W = 128;
            const int H = 128;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
            };
            Color[] palette =
            {
                new Color(0.86f, 0.65f, 0.42f, 1f), // warm ochre
                new Color(0.78f, 0.56f, 0.35f, 1f), // terracotta amber
                new Color(0.89f, 0.76f, 0.58f, 1f), // golden cream sandstone
                new Color(0.52f, 0.56f, 0.59f, 1f), // river slate blue-grey
                new Color(0.73f, 0.63f, 0.50f, 1f), // taupe stone
                new Color(0.82f, 0.60f, 0.38f, 1f), // honey pebble
                new Color(0.61f, 0.64f, 0.65f, 1f), // light granite grey
                new Color(0.91f, 0.81f, 0.66f, 1f), // pale sand pebble
                new Color(0.68f, 0.50f, 0.33f, 1f), // roasted brown pebble
                new Color(0.84f, 0.70f, 0.51f, 1f), // warm buff
                new Color(0.46f, 0.50f, 0.53f, 1f), // dark slate pebble
                new Color(0.88f, 0.67f, 0.45f, 1f), // bright ochre
                new Color(0.76f, 0.67f, 0.56f, 1f), // warm limestone
                new Color(0.80f, 0.58f, 0.37f, 1f), // copper stone
                new Color(0.65f, 0.58f, 0.49f, 1f), // weathered stone
                new Color(0.87f, 0.74f, 0.55f, 1f), // sunlit sand stone
            };
            for (int y = 0; y < H; y++)
            {
                int row = (y / 32) % 4;
                for (int x = 0; x < W; x++)
                {
                    int col = (x / 32) % 4;
                    Color baseCol = palette[row * 4 + col];
                    float grain = Mathf.Sin(x * 0.7f + y * 0.5f) * 0.03f + Mathf.Cos(x * 1.3f - y * 1.1f) * 0.02f;
                    tex.SetPixel(x, y, new Color(
                        Mathf.Clamp01(baseCol.r + grain),
                        Mathf.Clamp01(baseCol.g + grain),
                        Mathf.Clamp01(baseCol.b + grain),
                        1f));
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Generates a single combined 3D mesh of hundreds of individual rounded 3D river pebbles/cobblestones
        /// packed across the 3D top face of a gravel platform. Each pebble is a 3D ellipsoid dome with real 3D normals,
        /// random size/orientation, and a natural ochre/sandstone/slate-grey color cell from the 4x4 pebble palette.
        /// </summary>
        public static Mesh CreateSculpted3DPebbleFieldMesh(float xMin, float width, float baseY, float zMin, float depth)
        {
            const int Segs = 10;
            const int Rings = 3;
            const int VertsPerPebble = 1 + Rings * Segs;
            const int TrisPerPebble = Segs + (Rings - 1) * Segs * 2;

            float stepX = 0.145f;
            float stepZ = 0.165f;
            int cols = Mathf.Max(2, Mathf.FloorToInt(width / stepX));
            int rows = Mathf.Max(2, Mathf.FloorToInt(depth / stepZ));
            int totalPebbles = cols * rows;

            var verts = new Vector3[totalPebbles * VertsPerPebble];
            var norms = new Vector3[verts.Length];
            var uvs = new Vector2[verts.Length];
            var tris = new int[totalPebbles * TrisPerPebble * 3];

            int vIdx = 0;
            int tIdx = 0;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // Deterministic hash per pebble in world coordinates with hexagonal row offset (zero grid lines)
                    int seed = Mathf.Abs(Mathf.RoundToInt((xMin + c * stepX) * 173f) * 73856093 ^ (r * 19349663));
                    float h1 = ((seed & 0xFF) / 255f) - 0.5f;
                    float h2 = (((seed >> 8) & 0xFF) / 255f) - 0.5f;
                    float h3 = ((seed >> 16) & 0xFF) / 255f;
                    float h4 = ((seed >> 24) & 0x7F) / 127f;

                    float rowShift = (r & 1) * 0.5f;
                    float cx = xMin + (c + 0.35f + rowShift + h1 * 0.78f) * (width / cols);
                    float cz = zMin + (r + 0.5f + h2 * 0.78f) * (depth / rows);
                    cx = Mathf.Clamp(cx, xMin + 0.03f, xMin + width - 0.03f);
                    cz = Mathf.Clamp(cz, zMin + 0.03f, zMin + depth - 0.03f);

                    float rx = Mathf.Lerp(0.082f, 0.142f, h3);
                    float rz = Mathf.Lerp(0.076f, 0.132f, h4);
                    float ry = Mathf.Lerp(0.048f, 0.088f, (h3 + h4) * 0.5f);
                    float yaw = (h1 - h2) * Mathf.PI;
                    float cosY = Mathf.Cos(yaw);
                    float sinY = Mathf.Sin(yaw);

                    int palCell = seed & 15;
                    Vector2 cellUvCenter = new Vector2(((palCell % 4) + 0.5f) * 0.25f, ((palCell / 4) + 0.5f) * 0.25f);

                    int baseV = vIdx;
                    // Top apex vertex
                    verts[vIdx] = new Vector3(cx, baseY + ry, cz);
                    norms[vIdx] = Vector3.up;
                    uvs[vIdx] = cellUvCenter;
                    vIdx++;

                    for (int ring = 1; ring <= Rings; ring++)
                    {
                        float phi = (ring / (float)Rings) * (Mathf.PI * 0.52f);
                        float sinPhi = Mathf.Sin(phi);
                        float cosPhi = Mathf.Cos(phi);

                        for (int s = 0; s < Segs; s++)
                        {
                            float theta = (s / (float)Segs) * Mathf.PI * 2f;
                            float lx = Mathf.Cos(theta) * sinPhi;
                            float lz = Mathf.Sin(theta) * sinPhi;
                            float ly = cosPhi;

                            float wx = (lx * rx) * cosY - (lz * rz) * sinY;
                            float wz = (lx * rx) * sinY + (lz * rz) * cosY;
                            float wy = (ly - 0.12f) * ry;

                            Vector3 n = new Vector3(lx * cosY - lz * sinY, ly * 1.15f, lx * sinY + lz * cosY).normalized;
                            verts[vIdx] = new Vector3(cx + wx, baseY + wy, cz + wz);
                            norms[vIdx] = n;
                            uvs[vIdx] = cellUvCenter + new Vector2(lx * 0.08f, lz * 0.08f);
                            vIdx++;
                        }
                    }

                    // Apex fan triangles
                    for (int s = 0; s < Segs; s++)
                    {
                        int nextS = (s + 1) % Segs;
                        tris[tIdx++] = baseV;
                        tris[tIdx++] = baseV + 1 + nextS;
                        tris[tIdx++] = baseV + 1 + s;
                    }

                    // Ring strip triangles
                    for (int ring = 0; ring < Rings - 1; ring++)
                    {
                        int r0 = baseV + 1 + ring * Segs;
                        int r1 = r0 + Segs;
                        for (int s = 0; s < Segs; s++)
                        {
                            int nextS = (s + 1) % Segs;
                            tris[tIdx++] = r0 + s;
                            tris[tIdx++] = r0 + nextS;
                            tris[tIdx++] = r1 + s;

                            tris[tIdx++] = r0 + nextS;
                            tris[tIdx++] = r1 + nextS;
                            tris[tIdx++] = r1 + s;
                        }
                    }
                }
            }

            var mesh = new Mesh
            {
                name = "Sculpted3DPebbleField",
                indexFormat = verts.Length > 65000
                    ? UnityEngine.Rendering.IndexFormat.UInt32
                    : UnityEngine.Rendering.IndexFormat.UInt16,
            };
            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Texture2D CreateCloudShadedTexture()
        {
            const int W = 64;
            const int H = 64;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
            };
            Color bottom = new Color(0.76f, 0.87f, 0.95f, 0.96f);
            Color mid = new Color(0.92f, 0.96f, 1.00f, 0.98f);
            Color top = new Color(1.00f, 1.00f, 1.00f, 1.00f);
            for (int y = 0; y < H; y++)
            {
                float t = y / (float)(H - 1);
                Color row = t < 0.45f
                    ? Color.Lerp(bottom, mid, t / 0.45f)
                    : Color.Lerp(mid, top, (t - 0.45f) / 0.55f);
                for (int x = 0; x < W; x++)
                {
                    tex.SetPixel(x, y, row);
                }
            }
            tex.Apply();
            return tex;
        }

        private void BuildCornerOrnament(Transform parent, float cornerX, float topY, float frontZ, bool isRight, PicnicTerrain terrain, int seed)
        {
            if (cornerIvyTrimMat == null) return;
            float dir = isRight ? 1f : -1f;
            int h = Mathf.Abs(seed * 73856093);

            // Painterly alpha-cutout hanging ivy & flower sprig draping naturally around the rounded corner
            float w = terrain == PicnicTerrain.Wood ? 0.62f : 0.68f;
            float hh = terrain == PicnicTerrain.Wood ? 0.72f : 0.76f;
            float cx = cornerX - dir * (terrain == PicnicTerrain.Wood ? 0.18f : 0.22f);
            float cy = topY - hh * 0.44f + 0.05f;
            float cz = frontZ - 0.015f;

            var sprig = new GameObject(isRight ? "CornerIvySprig_R" : "CornerIvySprig_L", typeof(MeshFilter), typeof(MeshRenderer));
            sprig.transform.SetParent(parent, false);
            Mesh quad = CreateFlippedQuadMesh(w, hh, !isRight);
            generatedMeshes.Add(quad);
            sprig.GetComponent<MeshFilter>().sharedMesh = quad;
            var mr = sprig.GetComponent<MeshRenderer>();
            mr.sharedMaterial = cornerIvyTrimMat;
            mr.sortingOrder = 14;
            sprig.transform.position = new Vector3(cx, cy, cz);
            sprig.transform.localRotation = Quaternion.Euler(0f, 0f, (h % 5 - 2) * 1.8f);
        }

        private static Mesh CreateFlippedQuadMesh(float width, float height, bool flipX)
        {
            float hw = width * 0.5f;
            float hh = height * 0.5f;
            float u0 = flipX ? 1f : 0f;
            float u1 = flipX ? 0f : 1f;
            var mesh = new Mesh { name = "CornerTrimQuad" };
            mesh.vertices = new[]
            {
                new Vector3(-hw, -hh, 0f),
                new Vector3(-hw, hh, 0f),
                new Vector3(hw, hh, 0f),
                new Vector3(hw, -hh, 0f),
            };
            mesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
            mesh.uv = new[]
            {
                new Vector2(u0, 0f),
                new Vector2(u0, 1f),
                new Vector2(u1, 1f),
                new Vector2(u1, 0f),
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a 128x128 hand-painted alpha-cutout hanging ivy & meadow blossom sprig
        /// designed for right-hand rounded corners (mirrored horizontally for left corners),
        /// matching the crisp dark-outlined illustrated foliage on the floating wooden logs.
        /// </summary>
        private static Texture2D CreateCornerIvyTrimTexture()
        {
            const int S = 128;
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var px = new Color[S * S];
            for (int i = 0; i < px.Length; i++) px[i] = Color.clear;

            Color outline = new Color(0.11f, 0.22f, 0.08f, 1f);
            Color deepLeaf = new Color(0.22f, 0.48f, 0.14f, 1f);
            Color midLeaf = new Color(0.42f, 0.70f, 0.18f, 1f);
            Color brightLeaf = new Color(0.68f, 0.88f, 0.26f, 1f);
            Color petalWhite = new Color(0.99f, 0.97f, 0.88f, 1f);
            Color petalGold = new Color(0.99f, 0.80f, 0.26f, 1f);

            // Leaf cluster curving smoothly along top-right rounded shoulder and draping down the right edge
            // (cx, cy in [0..128], rx, ry, angleDeg, shade 0=deep, 1=mid, 2=bright)
            var leaves = new (float cx, float cy, float rx, float ry, float ang, int shade)[]
            {
                // Back dark layer for rich silhouette
                (44f, 102f, 18f, 12f, -12f, 0),
                (68f, 100f, 20f, 13f, -24f, 0),
                (88f, 88f, 19f, 13f, -48f, 0),
                (98f, 68f, 17f, 12f, -72f, 0),
                (99f, 46f, 15f, 11f, -84f, 0),
                (95f, 26f, 12f, 9f, -92f, 0),
                // Mid sunlit ivy leaves
                (34f, 105f, 14f, 9f, -6f, 1),
                (56f, 104f, 17f, 11f, -18f, 1),
                (78f, 95f, 17f, 11f, -40f, 1),
                (92f, 77f, 16f, 11f, -64f, 1),
                (96f, 56f, 14f, 10f, -80f, 1),
                (93f, 36f, 11f, 8f, -88f, 1),
                (76f, 76f, 13f, 9f, -52f, 1),
                // Bright sunlit top-edge leaf highlights
                (46f, 108f, 12f, 7f, -10f, 2),
                (69f, 102f, 13f, 8f, -28f, 2),
                (87f, 86f, 12f, 8f, -54f, 2),
                (95f, 65f, 10f, 7f, -76f, 2),
                (94f, 44f, 8f, 6f, -86f, 2),
            };

            for (int li = 0; li < leaves.Length; li++)
            {
                var lf = leaves[li];
                float rad = lf.ang * Mathf.Deg2Rad;
                float cosA = Mathf.Cos(rad);
                float sinA = Mathf.Sin(rad);
                int minX = Mathf.Max(0, Mathf.FloorToInt(lf.cx - lf.rx - 4));
                int maxX = Mathf.Min(S - 1, Mathf.CeilToInt(lf.cx + lf.rx + 4));
                int minY = Mathf.Max(0, Mathf.FloorToInt(lf.cy - lf.ry - 4));
                int maxY = Mathf.Min(S - 1, Mathf.CeilToInt(lf.cy + lf.ry + 4));

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        float dx = x - lf.cx;
                        float dy = y - lf.cy;
                        float lx = (dx * cosA + dy * sinA) / lf.rx;
                        float ly = (-dx * sinA + dy * cosA) / lf.ry;
                        // Pointed teardrop leaf shape (narrower at +lx tip)
                        float tipTaper = 1f - Mathf.Clamp01(lx * 0.35f);
                        float d = lx * lx + (ly / Mathf.Max(0.45f, tipTaper)) * (ly / Mathf.Max(0.45f, tipTaper));
                        if (d > 1.24f) continue;

                        Color col;
                        if (d > 0.88f)
                        {
                            col = outline;
                        }
                        else
                        {
                            Color baseCol = lf.shade == 0 ? deepLeaf : (lf.shade == 1 ? midLeaf : brightLeaf);
                            float vein = Mathf.Abs(ly) < 0.10f ? 0.14f : 0f;
                            float shadeGrad = Mathf.Clamp01(0.55f + ly * 0.35f - lx * 0.15f);
                            col = Color.Lerp(deepLeaf, baseCol, shadeGrad);
                            col = Color.Lerp(col, brightLeaf, vein);
                            col.a = 1f;
                        }
                        px[y * S + x] = col;
                    }
                }
            }

            // Two delicate illustrated garden blossoms nestled along the curve
            DrawBlossom(px, S, 62f, 103f, 7.5f, petalWhite, petalGold, outline);
            DrawBlossom(px, S, 91f, 75f, 6.5f, petalWhite, petalGold, outline);

            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        private static void DrawBlossom(Color[] px, int S, float cx, float cy, float radius, Color petal, Color center, Color outline)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(cx - radius - 2));
            int maxX = Mathf.Min(S - 1, Mathf.CeilToInt(cx + radius + 2));
            int minY = Mathf.Max(0, Mathf.FloorToInt(cy - radius - 2));
            int maxY = Mathf.Min(S - 1, Mathf.CeilToInt(cy + radius + 2));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float ang = Mathf.Atan2(dy, dx);
                    float petalEdge = radius * (0.78f + 0.22f * Mathf.Cos(ang * 5f));
                    if (r > petalEdge + 1.3f) continue;

                    if (r <= radius * 0.34f)
                    {
                        px[y * S + x] = center;
                    }
                    else if (r <= petalEdge)
                    {
                        px[y * S + x] = petal;
                    }
                    else
                    {
                        px[y * S + x] = outline;
                    }
                }
            }
        }

        /// <summary>
        /// Builds a 3D extruded prism with smooth rounded X-Y corners and a sunlit front bevel,
        /// using world-space UVs so textures wrap seamlessly around every curved shoulder.
        /// </summary>
        public static Mesh CreateRoundedTopExtrudedMesh(
            float xMin, float width, float yMin, float yMax, float zMin, float depth,
            float rTopLeft, float rTopRight, float rBotLeft, float rBotRight,
            float frontBevel, float uvTileMeters = 3.2f)
        {
            const int ArcSegs = 6;
            float xMax = xMin + width;
            float zMax = zMin + depth;
            float h = Mathf.Max(0.02f, yMax - yMin);

            float maxW = width * 0.44f;
            float maxTopH = (rBotLeft > 0.001f || rBotRight > 0.001f) ? (h - Mathf.Max(rBotLeft, rBotRight) - 0.01f) : (h * 0.90f);
            float maxTopR = Mathf.Max(0f, Mathf.Min(maxW, maxTopH));
            float maxBotR = Mathf.Max(0f, Mathf.Min(maxW, h * 0.46f));
            rTopLeft = Mathf.Clamp(rTopLeft, 0f, maxTopR);
            rTopRight = Mathf.Clamp(rTopRight, 0f, maxTopR);
            rBotLeft = Mathf.Clamp(rBotLeft, 0f, maxBotR);
            rBotRight = Mathf.Clamp(rBotRight, 0f, maxBotR);
            float bevel = Mathf.Clamp(frontBevel, 0f, Mathf.Min(depth * 0.25f, width * 0.18f, h * 0.35f));

            var profile = new List<Vector2>(32);

            // Clockwise 2D profile (viewed from -Z: +X right, +Y up)
            // 1. Bottom-left corner
            if (rBotLeft > 0.001f)
            {
                Vector2 c = new Vector2(xMin + rBotLeft, yMin + rBotLeft);
                for (int i = 0; i <= ArcSegs; i++)
                {
                    float a = Mathf.Lerp(-Mathf.PI * 0.5f, -Mathf.PI, i / (float)ArcSegs);
                    profile.Add(c + new Vector2(Mathf.Cos(a) * rBotLeft, Mathf.Sin(a) * rBotLeft));
                }
            }
            else
            {
                profile.Add(new Vector2(xMin, yMin));
            }

            // 2. Top-left corner
            if (rTopLeft > 0.001f)
            {
                Vector2 c = new Vector2(xMin + rTopLeft, yMax - rTopLeft);
                for (int i = 0; i <= ArcSegs; i++)
                {
                    float a = Mathf.Lerp(Mathf.PI, Mathf.PI * 0.5f, i / (float)ArcSegs);
                    profile.Add(c + new Vector2(Mathf.Cos(a) * rTopLeft, Mathf.Sin(a) * rTopLeft));
                }
            }
            else
            {
                profile.Add(new Vector2(xMin, yMax));
            }

            // 3. Top-right corner
            if (rTopRight > 0.001f)
            {
                Vector2 c = new Vector2(xMax - rTopRight, yMax - rTopRight);
                for (int i = 0; i <= ArcSegs; i++)
                {
                    float a = Mathf.Lerp(Mathf.PI * 0.5f, 0f, i / (float)ArcSegs);
                    profile.Add(c + new Vector2(Mathf.Cos(a) * rTopRight, Mathf.Sin(a) * rTopRight));
                }
            }
            else
            {
                profile.Add(new Vector2(xMax, yMax));
            }

            // 4. Bottom-right corner
            if (rBotRight > 0.001f)
            {
                Vector2 c = new Vector2(xMax - rBotRight, yMin + rBotRight);
                for (int i = 0; i <= ArcSegs; i++)
                {
                    float a = Mathf.Lerp(0f, -Mathf.PI * 0.5f, i / (float)ArcSegs);
                    profile.Add(c + new Vector2(Mathf.Cos(a) * rBotRight, Mathf.Sin(a) * rBotRight));
                }
            }
            else
            {
                profile.Add(new Vector2(xMax, yMin));
            }

            int n = profile.Count;
            var outNormals = new Vector2[n];
            Vector2 centroid = Vector2.zero;
            for (int i = 0; i < n; i++)
            {
                centroid += profile[i];
                Vector2 prev = profile[(i - 1 + n) % n];
                Vector2 curr = profile[i];
                Vector2 next = profile[(i + 1) % n];

                Vector2 e1 = (curr - prev);
                Vector2 e2 = (next - curr);
                // For a clockwise polygon in XY, outward normal of edge (dx, dy) is (-dy, dx)
                Vector2 n1 = e1.sqrMagnitude > 1e-8f ? new Vector2(-e1.y, e1.x).normalized : Vector2.zero;
                Vector2 n2 = e2.sqrMagnitude > 1e-8f ? new Vector2(-e2.y, e2.x).normalized : Vector2.zero;
                Vector2 avg = n1 + n2;
                outNormals[i] = avg.sqrMagnitude > 1e-6f ? avg.normalized : (n1.sqrMagnitude > 1e-6f ? n1 : Vector2.up);
            }
            centroid /= n;

            // Cumulative perimeter arc length for seamless UV wrapping along curved shoulders
            var arcLen = new float[n + 1];
            for (int i = 0; i < n; i++)
            {
                arcLen[i + 1] = arcLen[i] + Vector2.Distance(profile[i], profile[(i + 1) % n]);
            }

            float s = 1f / uvTileMeters;
            var verts = new List<Vector3>(n * 7 + 1);
            var norms = new List<Vector3>(n * 7 + 1);
            var uvs = new List<Vector2>(n * 7 + 1);
            var tris = new List<int>(n * 18);

            // A. Front flat face (at z = zMin, inset by bevel along outward normal so front lip rolls smoothly)
            int centerIdx = verts.Count;
            verts.Add(new Vector3(centroid.x, centroid.y, zMin));
            norms.Add(Vector3.back);
            uvs.Add(new Vector2(centroid.x * s, centroid.y * s));

            int frontRingStart = verts.Count;
            var frontPts = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                // Do not inset vertices touching the floor (y == yMin) when bottom corners are unrounded
                float insetScale = (profile[i].y <= yMin + 0.002f && rBotLeft <= 0.001f && rBotRight <= 0.001f) ? 0f : bevel;
                Vector2 fp = profile[i] - outNormals[i] * insetScale;
                frontPts[i] = fp;
                verts.Add(new Vector3(fp.x, fp.y, zMin));
                norms.Add(Vector3.back);
                uvs.Add(new Vector2(fp.x * s, fp.y * s));
            }
            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                tris.Add(centerIdx);
                tris.Add(frontRingStart + i);
                tris.Add(frontRingStart + next);
            }

            // B. Front bevel ribbon (zMin -> zMin + bevel)
            float zShoulder = zMin + bevel;
            if (bevel > 0.001f)
            {
                int bevStart = verts.Count;
                for (int i = 0; i <= n; i++)
                {
                    int idx = i % n;
                    Vector2 fp = frontPts[idx];
                    Vector2 sp = profile[idx];
                    Vector2 on = outNormals[idx];
                    Vector3 bevNorm = new Vector3(on.x, on.y, -0.75f).normalized;

                    verts.Add(new Vector3(fp.x, fp.y, zMin));
                    norms.Add(bevNorm);
                    uvs.Add(new Vector2((xMin + arcLen[i]) * s, zMin * s));

                    verts.Add(new Vector3(sp.x, sp.y, zShoulder));
                    norms.Add(new Vector3(on.x, on.y, -0.15f).normalized);
                    uvs.Add(new Vector2((xMin + arcLen[i]) * s, zShoulder * s));
                }
                for (int i = 0; i < n; i++)
                {
                    int v0 = bevStart + i * 2;
                    int v1 = v0 + 1;
                    int v2 = v0 + 2;
                    int v3 = v0 + 3;
                    tris.Add(v0); tris.Add(v1); tris.Add(v3);
                    tris.Add(v0); tris.Add(v3); tris.Add(v2);
                }
            }

            // C. Main extruded outer ribbon (zShoulder -> zMax)
            int extStart = verts.Count;
            for (int i = 0; i <= n; i++)
            {
                int idx = i % n;
                Vector2 sp = profile[idx];
                Vector2 on = outNormals[idx];
                Vector3 extNorm = new Vector3(on.x, on.y, 0f).normalized;

                // Use world X for top/bottom faces and world Y for vertical sides via arcLen
                float uCoord = Mathf.Abs(on.y) > 0.65f ? sp.x * s : (sp.x + sp.y) * s * 0.75f;

                verts.Add(new Vector3(sp.x, sp.y, zShoulder));
                norms.Add(extNorm);
                uvs.Add(new Vector2(uCoord, zShoulder * s));

                verts.Add(new Vector3(sp.x, sp.y, zMax));
                norms.Add(extNorm);
                uvs.Add(new Vector2(uCoord, zMax * s));
            }
            for (int i = 0; i < n; i++)
            {
                int v0 = extStart + i * 2;
                int v1 = v0 + 1;
                int v2 = v0 + 2;
                int v3 = v0 + 3;
                tris.Add(v0); tris.Add(v1); tris.Add(v3);
                tris.Add(v0); tris.Add(v3); tris.Add(v2);
            }

            var mesh = new Mesh { name = "RoundedWorldUV3D" };
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Builds a 3D rectangular prism mesh with distinct normals per face (for crisp URP 3D lighting)
        /// and world-space UVs so adjacent blocks tile seamlessly without texture seams.
        /// </summary>
        public static Mesh CreateWorldUVBoxMesh(float xMin, float width, float yMin, float yMax, float zMin, float depth, float uvTileMeters = 3.2f)
        {
            float xMax = xMin + width;
            float zMax = zMin + depth;

            Vector3 p000 = new Vector3(xMin, yMin, zMin);
            Vector3 p100 = new Vector3(xMax, yMin, zMin);
            Vector3 p110 = new Vector3(xMax, yMax, zMin);
            Vector3 p010 = new Vector3(xMin, yMax, zMin);

            Vector3 p001 = new Vector3(xMin, yMin, zMax);
            Vector3 p101 = new Vector3(xMax, yMin, zMax);
            Vector3 p111 = new Vector3(xMax, yMax, zMax);
            Vector3 p011 = new Vector3(xMin, yMax, zMax);

            var verts = new Vector3[]
            {
                p000, p010, p110, p100,
                p010, p011, p111, p110,
                p001, p011, p010, p000,
                p100, p110, p111, p101,
                p001, p000, p100, p101,
            };

            var normals = new Vector3[]
            {
                Vector3.back, Vector3.back, Vector3.back, Vector3.back,
                Vector3.up, Vector3.up, Vector3.up, Vector3.up,
                Vector3.left, Vector3.left, Vector3.left, Vector3.left,
                Vector3.right, Vector3.right, Vector3.right, Vector3.right,
                Vector3.down, Vector3.down, Vector3.down, Vector3.down,
            };

            float s = 1f / uvTileMeters;
            var uvs = new Vector2[]
            {
                new Vector2(xMin * s, yMin * s), new Vector2(xMin * s, yMax * s), new Vector2(xMax * s, yMax * s), new Vector2(xMax * s, yMin * s),
                new Vector2(xMin * s, zMin * s), new Vector2(xMin * s, zMax * s), new Vector2(xMax * s, zMax * s), new Vector2(xMax * s, zMin * s),
                new Vector2(zMax * s, yMin * s), new Vector2(zMax * s, yMax * s), new Vector2(zMin * s, yMax * s), new Vector2(zMin * s, yMin * s),
                new Vector2(zMin * s, yMin * s), new Vector2(zMin * s, yMax * s), new Vector2(zMax * s, yMax * s), new Vector2(zMax * s, yMin * s),
                new Vector2(xMin * s, zMax * s), new Vector2(xMin * s, zMin * s), new Vector2(xMax * s, zMin * s), new Vector2(xMax * s, zMax * s),
            };

            var tris = new int[]
            {
                0, 1, 2, 0, 2, 3,
                4, 5, 6, 4, 6, 7,
                8, 9, 10, 8, 10, 11,
                12, 13, 14, 12, 14, 15,
                16, 17, 18, 16, 18, 19,
            };

            var mesh = new Mesh { name = "WorldUVBox3D" };
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = tris;
            return mesh;
        }

        private void OnDestroy()
        {
            if (ivyFaceMat != null) Destroy(ivyFaceMat);
            if (topGrassMat != null) Destroy(topGrassMat);
            if (topMeadowMat != null) Destroy(topMeadowMat);
            if (earthRimMat != null) Destroy(earthRimMat);
            if (topGravelMat != null) Destroy(topGravelMat);
            if (topWoodMat != null) Destroy(topWoodMat);
            if (woodBodyMat != null) Destroy(woodBodyMat);
            if (stoneMat != null) Destroy(stoneMat);
            if (stoneCapMat != null) Destroy(stoneCapMat);
            if (riverStone3DMat != null) Destroy(riverStone3DMat);
            if (woodPlatform3DMat != null) Destroy(woodPlatform3DMat);
            if (spokeMat != null) Destroy(spokeMat);
            if (cloudBaseMat != null) Destroy(cloudBaseMat);
            if (grassSurfaceMat != null) Destroy(grassSurfaceMat);
            if (cornerIvyTrimMat != null) Destroy(cornerIvyTrimMat);
            if (cornerIvyTrimTex != null) Destroy(cornerIvyTrimTex);
            foreach (Mesh mesh in grassSurfaceMeshes)
            {
                if (mesh != null) Destroy(mesh);
            }
            foreach (Mesh mesh in generatedMeshes)
            {
                if (mesh != null) Destroy(mesh);
            }
            if (woodPlankTex != null) Destroy(woodPlankTex);
            if (riverStoneTex != null) Destroy(riverStoneTex);
            if (gravelCapTex != null) Destroy(gravelCapTex);
            if (cloudGradTex != null) Destroy(cloudGradTex);
            for (int i = 0; i < cloudVisuals.Count; i++)
            {
                if (cloudVisuals[i].InstanceMaterial != null) Destroy(cloudVisuals[i].InstanceMaterial);
            }
        }
    }
}
