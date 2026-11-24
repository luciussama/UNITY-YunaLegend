using System.Collections.Generic;
using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>
    /// 1:1 faithful Unity renderer of legacy GardenRenderer scenery:
    /// parallax panoramic garden, crystal-clear turquoise rivers with swimming fish and plants,
    /// painted house, checkpoints, great tree, lateral picnic, and camera tracking.
    /// </summary>
    public sealed class PicnicSceneryController : MonoBehaviour
    {
        private Camera stageCamera;
        private SpriteRenderer backdrop;
        private readonly List<FishEntry> fishes = new List<FishEntry>();
        private readonly List<CheckpointFlag> flags = new List<CheckpointFlag>();
        private Material waterMat;
        private Material bedMat;
        private Material flagMatActive;
        private Material flagMatIdle;

        private sealed class FishEntry
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float AnchorX;
            public float BaseY;
            public float PhaseOffset;
        }

        private sealed class CheckpointFlag
        {
            public float X;
            public MeshRenderer FlagRenderer;
        }

        public void Build(Camera cam, PicnicLevelData level)
        {
            stageCamera = cam;
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");

            // Panoramic garden backdrop (scenery-v1.png)
            var bgGo = new GameObject("GardenPanorama", typeof(SpriteRenderer));
            bgGo.transform.SetParent(cam.transform, false);
            backdrop = bgGo.GetComponent<SpriteRenderer>();
            backdrop.sprite = PicnicAtlasCatalog.FullSprite("scenery-v1", new Vector2(0.5f, 0.5f));
            backdrop.sortingOrder = -20;

            // Crystal clear translucent turquoise river material (#7bd9db -> #347caa)
            Shader spriteShader = Shader.Find("Sprites/Default") ?? unlit;
            waterMat = new Material(spriteShader);
            waterMat.mainTexture = CreateVerticalGradientTexture(
                new Color(0.482f, 0.851f, 0.859f, 0.62f),
                new Color(0.204f, 0.486f, 0.667f, 0.80f));
            waterMat.color = Color.white;

            bedMat = new Material(unlit);
            bedMat.color = new Color(0.32f, 0.55f, 0.52f, 1f);

            flagMatActive = new Material(unlit) { color = new Color(0.957f, 0.722f, 0.373f, 1f) }; // #f4b85f
            flagMatIdle = new Material(unlit) { color = new Color(0.714f, 0.824f, 0.518f, 1f) };   // #b6d284

            BuildRivers(level);
            BuildHouse();
            BuildGoalArea(level);
            BuildCheckpoints(level);
        }

        public void Apply(PicnicSimulation sim)
        {
            if (stageCamera == null) return;
            // Exact legacy camera framing (GardenRenderer.frame: 690 px visible height)
            float orthoHeight = 6.9f;
            float orthoWidth = orthoHeight * stageCamera.aspect;
            stageCamera.orthographic = true;
            stageCamera.orthographicSize = orthoHeight * 0.5f;

            float worldW = sim.Level.Width / 100f;
            float ox = Mathf.Clamp(sim.PlayerX / 100f - orthoWidth * 0.36f, 0f, Mathf.Max(0f, worldW - orthoWidth));
            float camX = ox + orthoWidth * 0.5f;
            float camY = orthoHeight * 0.31f;
            stageCamera.transform.position = new Vector3(camX, camY, -10f);

            if (backdrop != null && backdrop.sprite != null)
            {
                float bgAspect = backdrop.sprite.bounds.size.x / backdrop.sprite.bounds.size.y;
                float bgH = Mathf.Max(orthoHeight, orthoWidth / bgAspect) * 1.35f;
                float bgW = bgH * bgAspect;
                backdrop.transform.localScale = new Vector3(bgW / backdrop.sprite.bounds.size.x, bgH / backdrop.sprite.bounds.size.y, 1f);
                float progress = Mathf.Clamp01(ox / Mathf.Max(1f, worldW - orthoWidth));
                float pxOffset = (bgW - orthoWidth) * (0.5f - progress);
                backdrop.transform.localPosition = new Vector3(pxOffset, (bgH - orthoHeight) * 0.5f, 20f);
            }

            // Animate swimming fish
            float t = sim.Time;
            for (int i = 0; i < fishes.Count; i++)
            {
                var f = fishes[i];
                float phase = t * 0.35f + f.PhaseOffset;
                float fx = f.AnchorX + Mathf.Sin(phase) * 35f;
                float fy = f.BaseY + Mathf.Sin(phase * 1.7f) * 3f;
                f.Transform.position = PicnicAtlasCatalog.ToUnity(fx, fy, 0.09f);
                f.Renderer.flipX = Mathf.Cos(phase) < 0f;
            }

            // Update checkpoint flag colors
            for (int i = 0; i < flags.Count; i++)
            {
                flags[i].FlagRenderer.sharedMaterial = sim.Checkpoint >= flags[i].X ? flagMatActive : flagMatIdle;
            }
        }

        private void BuildRivers(PicnicLevelData level)
        {
            Sprite[] fishSprites =
            {
                PicnicAtlasCatalog.Slice("life-aquatic-v1", PicnicAtlasCatalog.AquaticFrames[1], 100f, "aq:fish1"),
                PicnicAtlasCatalog.Slice("life-aquatic-v1", PicnicAtlasCatalog.AquaticFrames[2], 100f, "aq:fish2"),
                PicnicAtlasCatalog.Slice("life-aquatic-v1", PicnicAtlasCatalog.AquaticFrames[3], 100f, "aq:fish3"),
            };
            Sprite[] plantSprites =
            {
                PicnicAtlasCatalog.Slice("life-aquatic-v1", PicnicAtlasCatalog.AquaticFrames[4], 100f, "aq:plant1"),
                PicnicAtlasCatalog.Slice("life-aquatic-v1", PicnicAtlasCatalog.AquaticFrames[5], 100f, "aq:plant2"),
            };

            foreach (var w in level.Waters)
            {
                var riverGo = new GameObject("River_" + w.Id);
                riverGo.transform.SetParent(transform, false);

                // Clear turquoise water quad
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.name = "WaterSurface";
                quad.transform.SetParent(riverGo.transform, false);
                Destroy(quad.GetComponent<Collider>());
                float topY = PicnicLevelData.WaterY - 35f;
                float h = 260f;
                quad.transform.position = PicnicAtlasCatalog.ToUnity(w.X + w.Width * 0.5f, topY + h * 0.5f, 0.10f);
                quad.transform.localScale = new Vector3((w.Width + 40f) / 100f, h / 100f, 1f);
                var mr = quad.GetComponent<MeshRenderer>();
                mr.sharedMaterial = waterMat;
                mr.sortingOrder = 0;

                // Riverbed
                var bed = GameObject.CreatePrimitive(PrimitiveType.Quad);
                bed.name = "RiverBed";
                bed.transform.SetParent(riverGo.transform, false);
                Destroy(bed.GetComponent<Collider>());
                bed.transform.position = PicnicAtlasCatalog.ToUnity(w.X + w.Width * 0.5f, PicnicLevelData.RiverBedY + 90f, 0.11f);
                bed.transform.localScale = new Vector3((w.Width + 40f) / 100f, 1.8f, 1f);
                var bmr = bed.GetComponent<MeshRenderer>();
                bmr.sharedMaterial = bedMat;
                bmr.sortingOrder = 0;

                // Swimming fish
                int fishCount = Mathf.Clamp(Mathf.FloorToInt(w.Width / 220f), 2, 5);
                for (int i = 0; i < fishCount; i++)
                {
                    float anchor = w.X + (w.Width * (i + 1)) / (fishCount + 1);
                    float baseY = PicnicLevelData.WaterY + 6f + (i % 3) * 17f;
                    var fGo = new GameObject("Fish_" + i, typeof(SpriteRenderer));
                    fGo.transform.SetParent(riverGo.transform, false);
                    var sr = fGo.GetComponent<SpriteRenderer>();
                    sr.sprite = fishSprites[i % fishSprites.Length];
                    sr.color = new Color(1f, 1f, 1f, 0.85f);
                    sr.sortingOrder = 3;
                    FitSprite(fGo.transform, sr.sprite, 0.28f, 0.13f);
                    fishes.Add(new FishEntry
                    {
                        Transform = fGo.transform,
                        Renderer = sr,
                        AnchorX = anchor,
                        BaseY = baseY,
                        PhaseOffset = i * 2.1f + w.X * 0.01f,
                    });
                }

                // Underwater plants
                int plantIdx = 0;
                for (float x = w.X + 65f; x < w.X + w.Width - 30f; x += 210f, plantIdx++)
                {
                    var pGo = new GameObject("Plant_" + plantIdx, typeof(SpriteRenderer));
                    pGo.transform.SetParent(riverGo.transform, false);
                    var sr = pGo.GetComponent<SpriteRenderer>();
                    sr.sprite = plantSprites[plantIdx % plantSprites.Length];
                    sr.color = new Color(1f, 1f, 1f, 0.70f);
                    sr.sortingOrder = 2;
                    pGo.transform.position = PicnicAtlasCatalog.ToUnity(x, PicnicLevelData.RiverBedY, 0.105f);
                    FitSprite(pGo.transform, sr.sprite, plantIdx % 2 == 1 ? 0.25f : 0.21f, plantIdx % 2 == 1 ? 0.30f : 0.44f);
                }
            }
        }

        private void BuildHouse()
        {
            // Exact legacy GARDEN_HOME placement: x=30, y=201, w=500, h=401
            var house = new GameObject("GardenHome", typeof(SpriteRenderer));
            house.transform.SetParent(transform, false);
            var sr = house.GetComponent<SpriteRenderer>();
            sr.sprite = PicnicAtlasCatalog.Slice("ambient-v2", PicnicAtlasCatalog.AmbienteFrames[0], 100f, "env:house");
            sr.sortingOrder = 6;
            house.transform.position = PicnicAtlasCatalog.ToUnity(PicnicLevelData.HomeX, PicnicLevelData.HomeY, 0.04f);
            FitSprite(house.transform, sr.sprite, PicnicLevelData.HomeW / 100f, PicnicLevelData.HomeH / 100f);
        }

        private void BuildGoalArea(PicnicLevelData level)
        {
            // Great Tree at Goal - 380
            var tree = new GameObject("GreatTree", typeof(SpriteRenderer));
            tree.transform.SetParent(transform, false);
            var tr = tree.GetComponent<SpriteRenderer>();
            tr.sprite = PicnicAtlasCatalog.Slice("objects-v1", PicnicAtlasCatalog.ObjetosFrames[8], 100f, "env:tree");
            tr.sortingOrder = 5;
            float treeW = 360f * 417f / 446f;
            tree.transform.position = PicnicAtlasCatalog.ToUnity(level.Goal - 380f + treeW * 0.5f, PicnicLevelData.GroundY + 4f, 0.05f);
            FitSprite(tree.transform, tr.sprite, treeW / 100f, 3.60f);

            // Lateral Picnic at Goal + 5 anchored by groundAnchorY = 454
            var picnic = new GameObject("LateralPicnic", typeof(SpriteRenderer));
            picnic.transform.SetParent(transform, false);
            var pr = picnic.GetComponent<SpriteRenderer>();
            pr.sprite = PicnicAtlasCatalog.Slice("picnic-lateral-v3", PicnicAtlasCatalog.PicnicLateralFrame, 100f, "env:picnic");
            pr.sortingOrder = 7;
            float pw = 400f;
            float ph = pw * PicnicAtlasCatalog.PicnicLateralFrame.H / PicnicAtlasCatalog.PicnicLateralFrame.W;
            picnic.transform.position = PicnicAtlasCatalog.ToUnity(level.Goal + 5f, PicnicLevelData.GroundY, 0.03f);
            FitSprite(picnic.transform, pr.sprite, pw / 100f, ph / 100f);
        }

        private void BuildCheckpoints(PicnicLevelData level)
        {
            var poleMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"))
            {
                color = new Color(0.502f, 0.353f, 0.247f, 1f),
            };
            for (int i = 1; i < level.Checkpoints.Count; i++)
            {
                float cx = level.Checkpoints[i];
                float cy = PicnicLevelData.GroundY;
                foreach (var p in level.StaticPlatforms)
                {
                    if (p.Ground && cx >= p.X && cx < p.X + p.Width)
                    {
                        cy = PicnicLevelData.SurfaceY(p, cx);
                        break;
                    }
                }
                var cp = new GameObject("Checkpoint_" + i);
                cp.transform.SetParent(transform, false);

                var pole = GameObject.CreatePrimitive(PrimitiveType.Quad);
                pole.transform.SetParent(cp.transform, false);
                Destroy(pole.GetComponent<Collider>());
                pole.transform.position = PicnicAtlasCatalog.ToUnity(cx, cy - 24.5f, 0.04f);
                pole.transform.localScale = new Vector3(0.07f, 0.49f, 1f);
                var pmr = pole.GetComponent<MeshRenderer>();
                pmr.sharedMaterial = poleMat;
                pmr.sortingOrder = 5;

                var flag = GameObject.CreatePrimitive(PrimitiveType.Quad);
                flag.transform.SetParent(cp.transform, false);
                Destroy(flag.GetComponent<Collider>());
                flag.transform.position = PicnicAtlasCatalog.ToUnity(cx + 16f, cy - 36f, 0.04f);
                flag.transform.localScale = new Vector3(0.30f, 0.22f, 1f);
                var fmr = flag.GetComponent<MeshRenderer>();
                fmr.sharedMaterial = flagMatIdle;
                fmr.sortingOrder = 5;
                flags.Add(new CheckpointFlag { X = cx, FlagRenderer = fmr });
            }
        }

        private static Texture2D CreateVerticalGradientTexture(Color top, Color bottom)
        {
            var tex = new Texture2D(2, 32, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 32; y++)
            {
                Color c = Color.Lerp(bottom, top, y / 31f);
                tex.SetPixel(0, y, c);
                tex.SetPixel(1, y, c);
            }
            tex.Apply();
            return tex;
        }

        private static void FitSprite(Transform t, Sprite s, float worldW, float worldH)
        {
            if (s == null || s.bounds.size.x <= 0.0001f || s.bounds.size.y <= 0.0001f) return;
            t.localScale = new Vector3(worldW / s.bounds.size.x, worldH / s.bounds.size.y, 1f);
        }

        private void OnDestroy()
        {
            if (waterMat != null) Destroy(waterMat);
            if (bedMat != null) Destroy(bedMat);
            if (flagMatActive != null) Destroy(flagMatActive);
            if (flagMatIdle != null) Destroy(flagMatIdle);
        }
    }
}
