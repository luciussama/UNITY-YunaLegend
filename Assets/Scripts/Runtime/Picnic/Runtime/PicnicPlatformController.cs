using System.Collections.Generic;
using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>
    /// 1:1 faithful Unity renderer of legacy GardenRenderer terrain, surface strips, foreground grass,
    /// floating platforms (wood/stone/cloud/aerial gravel), and rotating wooden water wheels with spokes.
    /// </summary>
    public sealed class PicnicPlatformController : MonoBehaviour
    {
        private readonly List<GameObject> staticRoots = new List<GameObject>();
        private readonly List<PicnicPlatformSpec> staticSpecs = new List<PicnicPlatformSpec>();
        private readonly List<SpriteRenderer> cloudRenderers = new List<SpriteRenderer>();
        private readonly List<string> cloudIds = new List<string>();
        private readonly List<WheelVisual> wheels = new List<WheelVisual>();

        private Material wallPlantMat;
        private Material woodBodyMat;
        private Material spokeMat;
        private Sprite tileSoil;
        private Sprite tileGrass;
        private Sprite tileGravel;
        private Sprite tileWood;
        private Sprite sprWoodPlat;
        private Sprite sprStonePlat;
        private Sprite sprCloudPlat;
        private Sprite sprRockPillar;

        private sealed class WheelVisual
        {
            public PicnicWheelSpec Spec;
            public GameObject Root;
            public Transform[] Platforms;
            public LineRenderer[] Spokes;
        }

        public void Build(PicnicLevelData level)
        {
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            wallPlantMat = new Material(unlit);
            var wallTex = PicnicAtlasCatalog.LoadTexture("wall-plant-v1");
            if (wallTex != null) { wallTex.wrapMode = TextureWrapMode.Repeat; wallPlantMat.mainTexture = wallTex; }
            wallPlantMat.color = Color.white;

            woodBodyMat = new Material(unlit);
            woodBodyMat.color = new Color(0.54f, 0.33f, 0.18f, 1f);

            spokeMat = new Material(unlit);
            spokeMat.color = new Color(0.655f, 0.475f, 0.302f, 1f); // #a7794d

            tileSoil = PicnicAtlasCatalog.FullSprite("tile-soil", new Vector2(0f, 1f));
            tileGrass = PicnicAtlasCatalog.FullSprite("tile-grass", new Vector2(0f, 1f));
            tileGravel = PicnicAtlasCatalog.FullSprite("tile-gravel", new Vector2(0f, 1f));
            tileWood = PicnicAtlasCatalog.FullSprite("tile-wood", new Vector2(0f, 1f));

            sprWoodPlat = PicnicAtlasCatalog.Slice("objects-v1", PicnicAtlasCatalog.ObjetosFrames[1], 100f, "plat:wood");
            sprStonePlat = PicnicAtlasCatalog.Slice("objects-v1", PicnicAtlasCatalog.ObjetosFrames[2], 100f, "plat:stone");
            sprCloudPlat = PicnicAtlasCatalog.Slice("objects-v1", PicnicAtlasCatalog.ObjetosFrames[3], 100f, "plat:cloud");
            sprRockPillar = PicnicAtlasCatalog.Slice("life-aquatic-v1", PicnicAtlasCatalog.AquaticFrames[0], 100f, "plat:pillar");

            foreach (var p in level.StaticPlatforms)
            {
                BuildStaticPlatform(p);
            }
            foreach (var w in level.Wheels)
            {
                BuildWheel(w);
            }
        }

        public void Apply(PicnicSimulation sim)
        {
            float px = sim.PlayerX;
            for (int i = 0; i < staticRoots.Count; i++)
            {
                var p = staticSpecs[i];
                bool near = p.X + p.Width >= px - 1400f && p.X <= px + 1400f;
                if (p.Terrain == PicnicTerrain.Cloud)
                {
                    var st = PicnicLevelData.GetCloudStatus(p.Id, sim.Clouds, sim.Difficulty, sim.Time);
                    staticRoots[i].SetActive(near && st.Solid);
                }
                else
                {
                    staticRoots[i].SetActive(near);
                }
            }

            for (int i = 0; i < cloudIds.Count; i++)
            {
                var st = PicnicLevelData.GetCloudStatus(cloudIds[i], sim.Clouds, sim.Difficulty, sim.Time);
                cloudRenderers[i].color = new Color(1f, 1f, 1f, st.Alpha);
            }

            for (int i = 0; i < wheels.Count; i++)
            {
                var wv = wheels[i];
                bool near = Mathf.Abs(wv.Spec.X - px) < 1400f;
                wv.Root.SetActive(near);
                if (!near) continue;
                Vector3 hub = PicnicAtlasCatalog.ToUnity(wv.Spec.X, wv.Spec.Y, 0.14f);
                for (int k = 0; k < 4; k++)
                {
                    float angle = sim.Time * wv.Spec.Speed + wv.Spec.Phase + k * Mathf.PI * 0.5f;
                    float platX = wv.Spec.X + Mathf.Cos(angle) * wv.Spec.Rx;
                    float platY = wv.Spec.Y + Mathf.Sin(angle) * wv.Spec.Ry;
                    Vector3 pos = PicnicAtlasCatalog.ToUnity(platX, platY - 8f, 0.05f);
                    wv.Platforms[k].position = pos;
                    wv.Spokes[k].SetPosition(0, hub);
                    wv.Spokes[k].SetPosition(1, PicnicAtlasCatalog.ToUnity(platX, platY + 10f, 0.14f));
                }
            }
        }

        private void BuildStaticPlatform(PicnicPlatformSpec p)
        {
            var go = new GameObject(p.Id);
            go.transform.SetParent(transform, false);
            staticRoots.Add(go);
            staticSpecs.Add(p);

            if (p.Ground)
            {
                BuildGroundBodyMesh(go.transform, p);
                if (p.Terrain == PicnicTerrain.Grass)
                {
                    // Legacy draws soil strip underneath grass strip for clean blend
                    BuildSurfaceStrip(go.transform, p, PicnicTerrain.Soil, p.X, p.Width, 3, 1f);
                }
                BuildSurfaceStrip(go.transform, p, p.Terrain, p.X, p.Width, 4, 1f);
                if (p.Terrain == PicnicTerrain.Grass)
                {
                    // Translucent foreground grass in front of Yuna's legs (GardenRenderer.grassFront alpha = 0.45)
                    BuildSurfaceStrip(go.transform, p, PicnicTerrain.Grass, p.X, p.Width, 15, 0.45f, -0.35f);
                }
                foreach (var patch in p.GrassPatches)
                {
                    float px = p.X + p.Width * patch.Start;
                    float pw = p.Width * (patch.End - patch.Start);
                    BuildSurfaceStrip(go.transform, p, PicnicTerrain.Grass, px, pw, 5, 1f);
                    BuildSurfaceStrip(go.transform, p, PicnicTerrain.Grass, px, pw, 15, 0.45f, -0.35f);
                }
                return;
            }

            if (p.Aerial && p.Terrain == PicnicTerrain.Gravel)
            {
                // Aerial gravel island: earth underside + gravel top strip
                var under = new GameObject("AerialEarth", typeof(SpriteRenderer));
                under.transform.SetParent(go.transform, false);
                var ur = under.GetComponent<SpriteRenderer>();
                ur.sprite = PicnicAtlasCatalog.Slice("objects-v1", PicnicAtlasCatalog.ObjetosFrames[0], 100f, "plat:soil");
                ur.sortingOrder = 3;
                under.transform.position = PicnicAtlasCatalog.ToUnity(p.X + p.Width * 0.5f, p.Y + 3f, 0.05f);
                FitSprite(under.transform, ur.sprite, p.Width / 100f, 0.65f);
                BuildSurfaceStrip(go.transform, p, PicnicTerrain.Gravel, p.X, p.Width, 4, 1f);
                return;
            }

            // Floating wood, stone, or cloud
            if (p.Terrain == PicnicTerrain.Stone && sprRockPillar != null)
            {
                var pillar = new GameObject("RockFoot", typeof(SpriteRenderer));
                pillar.transform.SetParent(go.transform, false);
                var pr = pillar.GetComponent<SpriteRenderer>();
                pr.sprite = sprRockPillar;
                pr.sortingOrder = 1;
                float topY = p.Y + 26f;
                float h = PicnicLevelData.RiverBedY - topY + 12f;
                pillar.transform.position = PicnicAtlasCatalog.ToUnity(p.X + p.Width * 0.5f, topY, 0.12f);
                FitSprite(pillar.transform, sprRockPillar, (p.Width * 0.94f) / 100f, h / 100f);
            }

            Sprite spr = p.Terrain == PicnicTerrain.Stone ? sprStonePlat : p.Terrain == PicnicTerrain.Cloud ? sprCloudPlat : sprWoodPlat;
            float offsetY = p.Terrain == PicnicTerrain.Wood ? 8f : p.Terrain == PicnicTerrain.Stone ? 3f : 6f;
            float drawH = p.Terrain == PicnicTerrain.Wood ? 53f : p.Terrain == PicnicTerrain.Stone ? 70f : 60f;
            var plat = new GameObject("Top", typeof(SpriteRenderer));
            plat.transform.SetParent(go.transform, false);
            var sr = plat.GetComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingOrder = 4;
            plat.transform.position = PicnicAtlasCatalog.ToUnity(p.X + p.Width * 0.5f, p.Y - offsetY, 0.05f);
            FitSprite(plat.transform, spr, p.Width / 100f, drawH / 100f);

            if (p.Terrain == PicnicTerrain.Cloud)
            {
                cloudIds.Add(p.Id);
                cloudRenderers.Add(sr);
            }
        }

        private void BuildGroundBodyMesh(Transform parent, PicnicPlatformSpec p)
        {
            int cols = p.Hill > 0f ? Mathf.Max(8, Mathf.CeilToInt(p.Width / 24f)) : 1;
            var verts = new Vector3[(cols + 1) * 2];
            var uvs = new Vector2[verts.Length];
            var tris = new int[cols * 6];
            float bottomPy = PicnicLevelData.Height + 220f;

            for (int i = 0; i <= cols; i++)
            {
                float wx = p.X + p.Width * i / cols;
                float wy = PicnicLevelData.SurfaceY(p, wx) + 2f;
                verts[i * 2] = PicnicAtlasCatalog.ToUnity(wx, wy, 0.08f);
                verts[i * 2 + 1] = PicnicAtlasCatalog.ToUnity(wx, bottomPy, 0.08f);
                // World-space UVs (320 px tile) so adjacent platforms have ZERO seams!
                uvs[i * 2] = new Vector2(wx / 320f, -wy / 320f);
                uvs[i * 2 + 1] = new Vector2(wx / 320f, -bottomPy / 320f);
                if (i == cols) continue;
                int v = i * 2;
                int t = i * 6;
                tris[t] = v; tris[t + 1] = v + 2; tris[t + 2] = v + 1;
                tris[t + 3] = v + 2; tris[t + 4] = v + 3; tris[t + 5] = v + 1;
            }

            var mesh = new Mesh { name = p.Id + "_body" };
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();

            var body = new GameObject("Body", typeof(MeshFilter), typeof(MeshRenderer));
            body.transform.SetParent(parent, false);
            body.GetComponent<MeshFilter>().mesh = mesh;
            var mr = body.GetComponent<MeshRenderer>();
            mr.sharedMaterial = p.Terrain == PicnicTerrain.Wood ? woodBodyMat : wallPlantMat;
            mr.sortingOrder = 2;
        }

        private void BuildSurfaceStrip(Transform parent, PicnicPlatformSpec p, PicnicTerrain terrain,
            float startX, float width, int order, float alpha, float z = 0.02f)
        {
            Sprite tile = terrain == PicnicTerrain.Grass ? tileGrass
                : terrain == PicnicTerrain.Gravel ? tileGravel
                : terrain == PicnicTerrain.Wood ? tileWood
                : tileSoil;
            if (tile == null) return;

            float offset = terrain == PicnicTerrain.Grass ? 90f
                : terrain == PicnicTerrain.Soil ? 18f
                : 10f;
            float tileH = terrain == PicnicTerrain.Wood ? 80f
                : terrain == PicnicTerrain.Soil ? 60f
                : 110f;

            float step = p.Hill > 0f ? 44f : 220f;
            for (float x = startX; x < startX + width - 0.5f; x += step)
            {
                float segW = Mathf.Min(step, startX + width - x);
                float sampleY = PicnicLevelData.SurfaceY(p, x + segW * 0.5f) - offset;
                var go = new GameObject("Surface_" + terrain, typeof(SpriteRenderer));
                go.transform.SetParent(parent, false);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = tile;
                sr.sortingOrder = order;
                sr.color = new Color(1f, 1f, 1f, alpha);
                go.transform.position = PicnicAtlasCatalog.ToUnity(x, sampleY, z);
                FitSprite(go.transform, tile, (segW + 2f) / 100f, tileH / 100f);
            }
        }

        private void BuildWheel(PicnicWheelSpec w)
        {
            var root = new GameObject("Wheel_" + w.Id);
            root.transform.SetParent(transform, false);

            // Center wooden hub
            var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "Hub";
            hub.transform.SetParent(root.transform, false);
            Destroy(hub.GetComponent<Collider>());
            hub.transform.position = PicnicAtlasCatalog.ToUnity(w.X, w.Y, 0.13f);
            hub.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            hub.transform.localScale = new Vector3(0.32f, 0.02f, 0.32f);
            var hubMr = hub.GetComponent<MeshRenderer>();
            hubMr.sharedMaterial = spokeMat;
            hubMr.sortingOrder = 3;

            var plats = new Transform[4];
            var spokes = new LineRenderer[4];
            for (int k = 0; k < 4; k++)
            {
                var spokeGo = new GameObject("Spoke_" + k);
                spokeGo.transform.SetParent(root.transform, false);
                var lr = spokeGo.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.startWidth = 0.08f;
                lr.endWidth = 0.08f;
                lr.sharedMaterial = spokeMat;
                lr.sortingOrder = 2;
                spokes[k] = lr;

                var platGo = new GameObject(w.Id + "-" + k, typeof(SpriteRenderer));
                platGo.transform.SetParent(root.transform, false);
                var sr = platGo.GetComponent<SpriteRenderer>();
                sr.sprite = sprWoodPlat;
                sr.sortingOrder = 4;
                FitSprite(platGo.transform, sprWoodPlat, 1.44f, 0.53f);
                plats[k] = platGo.transform;
            }

            wheels.Add(new WheelVisual { Spec = w, Root = root, Platforms = plats, Spokes = spokes });
        }

        private static void FitSprite(Transform t, Sprite s, float worldW, float worldH)
        {
            if (s == null || s.bounds.size.x <= 0.0001f || s.bounds.size.y <= 0.0001f) return;
            t.localScale = new Vector3(worldW / s.bounds.size.x, worldH / s.bounds.size.y, 1f);
        }

        private void OnDestroy()
        {
            if (wallPlantMat != null) Destroy(wallPlantMat);
            if (woodBodyMat != null) Destroy(woodBodyMat);
            if (spokeMat != null) Destroy(spokeMat);
        }
    }
}
