using System.Collections.Generic;
using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>
    /// 3D diorama scenery renderer for Picnic_Gameplay (1B faithful coordinates):
    /// configures the 2.5D perspective camera (framed identically at Z=0 while showing 3D top faces),
    /// warm morning directional sunlight, panoramic garden backdrop, 3D river trenches with full bottom fill
    /// (FloorY = -4.2f) and translucent turquoise water volumes, 3D house porch steps, 3D checkpoint posts,
    /// great tree, and lateral picnic area.
    /// </summary>
    public sealed class PicnicSceneryController : MonoBehaviour
    {
        private const float DepthZ = 2.2f;
        private const float FloorY = -4.2f;
        private const float CameraDistanceZ = 13.5f;
        private const float CameraPitchDeg = 6.5f;
        private const float CameraFovDeg = 27f;

        private Camera stageCamera;
        private bool? mobileViewportOverride;
        private bool mobileCameraInitialized;
        private float mobileGroundAnchorY;
        private Vector3 mobileCameraPosition;
        private readonly List<PicnicPlatformSpec> cameraPlatforms = new List<PicnicPlatformSpec>(32);
        private readonly HashSet<string> cameraSeenIds = new HashSet<string>();
        private SpriteRenderer backdrop;
        private Light directionalSun;
        private readonly List<FishEntry> fishes = new List<FishEntry>();
        private readonly List<WaterVolumeEntry> waterVolumes = new List<WaterVolumeEntry>();
        private readonly List<CheckpointFlag> flags = new List<CheckpointFlag>();
        private readonly List<Material> fishMaterials = new List<Material>();
        private readonly List<Mesh> dynamicMeshes = new List<Mesh>();
        private Mesh sharedFishBodyMesh;
        private Mesh sharedFishTailMesh;

        private Material waterFrontMat;
        private Material waterTopMat;
        private Material riverBedWallMat;
        private Material riverBackWallMat;
        private Material porchWoodTopMat;
        private Material porchWoodBodyMat;
        private Material poleMat;
        private Material flagMatActive;
        private Material flagMatIdle;

        private sealed class FishEntry
        {
            public Transform Transform;
            public Transform TailTransform;
            public float AnchorX;
            public float SwimRadiusX;
            public float BaseY;
            public float BaseZ;
            public float PhaseOffset;
            public float Speed;
        }

        private sealed class WaterVolumeEntry
        {
            public Mesh TopMesh;
            public Mesh FrontMesh;
            public Vector3[] TopBaseVerts;
            public Vector3[] TopWorkVerts;
            public Vector3[] FrontBaseVerts;
            public Vector3[] FrontWorkVerts;
            public int Cols;
            public int TopRows;
            public float XMin;
            public float Width;
            public float FrontZ;
            public float Depth;
            public float TopY;
            public float BedY;
        }

        private sealed class CheckpointFlag
        {
            public float X;
            public MeshRenderer FlagRenderer;
        }

        public void Build(Camera cam, PicnicLevelData level)
        {
            stageCamera = cam;
            Shader lit = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default");
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            Shader spriteShader = Shader.Find("Sprites/Default") ?? unlit;

            // Warm morning directional sunlight for crisp 3D platform/scenery shading
            var sunGo = new GameObject("PicnicSunLight3D", typeof(Light));
            sunGo.transform.SetParent(transform, false);
            directionalSun = sunGo.GetComponent<Light>();
            directionalSun.type = LightType.Directional;
            directionalSun.color = new Color(1f, 0.96f, 0.86f, 1f);
            directionalSun.intensity = 1.42f;
            directionalSun.shadows = LightShadows.Soft;
            directionalSun.shadowStrength = 0.68f;
            directionalSun.shadowBias = 0.02f;
            directionalSun.shadowNormalBias = 0.02f;
            sunGo.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.46f, 0.53f, 0.45f, 1f);

            // Panoramic garden backdrop (scenery-v1.png) attached to camera far behind 3D blocks
            var existingBg = cam.transform.Find("GardenPanorama");
            if (existingBg != null)
            {
                DestroyImmediate(existingBg.gameObject);
            }
            var bgGo = new GameObject("GardenPanorama", typeof(SpriteRenderer));
            bgGo.transform.SetParent(cam.transform, false);
            backdrop = bgGo.GetComponent<SpriteRenderer>();
            backdrop.sprite = PicnicAtlasCatalog.FullSprite("scenery-v1", new Vector2(0.5f, 0.5f));
            backdrop.sortingOrder = -20;

            // Translucent turquoise 3D river materials (URP Lit Transparent for 3D specular wave glints)
            waterFrontMat = new Material(spriteShader)
            {
                mainTexture = CreateVerticalGradientTexture(
                    new Color(0.45f, 0.86f, 0.88f, 0.58f),
                    new Color(0.14f, 0.42f, 0.62f, 0.84f)),
                color = Color.white,
            };

            waterTopMat = CreateTransparentWaterTopMaterial(lit, spriteShader);

            // 3D glossy koi/pond fish palettes (orange-gold, coral-red, turquoise-gold)
            Color[] fishPalette =
            {
                new Color(0.98f, 0.56f, 0.16f, 1f),
                new Color(0.92f, 0.34f, 0.24f, 1f),
                new Color(0.28f, 0.74f, 0.86f, 1f),
            };
            for (int i = 0; i < fishPalette.Length; i++)
            {
                var fm = new Material(lit) { color = fishPalette[i] };
                if (fm.HasProperty("_Smoothness")) fm.SetFloat("_Smoothness", 0.68f);
                if (fm.HasProperty("_Metallic")) fm.SetFloat("_Metallic", 0.08f);
                fishMaterials.Add(fm);
            }
            sharedFishBodyMesh = Create3DFishBodyMesh();
            sharedFishTailMesh = Create3DFishTailMesh();

            var soilTex = PicnicAtlasCatalog.LoadTexture("soil-continuous-v1");
            if (soilTex != null) soilTex.wrapMode = TextureWrapMode.Repeat;

            riverBedWallMat = new Material(lit)
            {
                color = new Color(0.32f, 0.48f, 0.44f, 1f),
            };
            if (soilTex != null) riverBedWallMat.mainTexture = soilTex;

            riverBackWallMat = new Material(lit)
            {
                color = new Color(0.24f, 0.44f, 0.46f, 1f),
            };
            if (soilTex != null) riverBackWallMat.mainTexture = soilTex;

            var porchPlankTex = PicnicPlatformController.CreateWoodPlankTexture();

            porchWoodTopMat = new Material(lit)
            {
                color = new Color(0.98f, 0.92f, 0.80f, 1f),
                mainTexture = porchPlankTex,
                mainTextureScale = new Vector2(0.95f, 0.95f),
            };
            if (porchWoodTopMat.HasProperty("_Smoothness")) porchWoodTopMat.SetFloat("_Smoothness", 0.14f);

            porchWoodBodyMat = new Material(lit)
            {
                color = new Color(0.76f, 0.64f, 0.50f, 1f),
                mainTexture = porchPlankTex,
                mainTextureScale = new Vector2(0.95f, 0.95f),
            };
            if (porchWoodBodyMat.HasProperty("_Smoothness")) porchWoodBodyMat.SetFloat("_Smoothness", 0.10f);

            poleMat = new Material(lit)
            {
                color = new Color(0.502f, 0.353f, 0.247f, 1f),
            };
            flagMatActive = new Material(lit) { color = new Color(0.957f, 0.722f, 0.373f, 1f) }; // #f4b85f
            flagMatIdle = new Material(lit) { color = new Color(0.714f, 0.824f, 0.518f, 1f) };   // #b6d284

            BuildRivers(level);
            BuildHouseAnd3DPorchSteps();
            BuildGoalArea(level);
            BuildCheckpoints(level);
        }

        public void SetMobileViewportOverride(bool? isMobile)
        {
            mobileViewportOverride = isMobile;
            mobileCameraInitialized = false;
        }

        public void Apply(PicnicSimulation sim)
        {
            Apply(sim, Time.deltaTime);
        }

        public void Apply(PicnicSimulation sim, float cameraDeltaTime)
        {
            if (stageCamera == null) return;

            // Perspective 2.5D camera pitched 6.5 deg down so 3D platform tops are visible
            // while the Z=0 gameplay plane has the exact same framing as the 6.9m orthographic view.
            float orthoHeight = 6.9f;
            float orthoWidth = orthoHeight * stageCamera.aspect;
            stageCamera.orthographic = false;
            stageCamera.fieldOfView = CameraFovDeg;
            stageCamera.nearClipPlane = 0.2f;
            stageCamera.farClipPlane = 120f;

            float worldW = sim.Level.Width / 100f;
            float ox = Mathf.Clamp(sim.PlayerX / 100f - orthoWidth * 0.36f, 0f, Mathf.Max(0f, worldW - orthoWidth));
            float camX = ox + orthoWidth * 0.5f;
            float targetY = orthoHeight * 0.31f;
            float camYOffset = CameraDistanceZ * Mathf.Tan(CameraPitchDeg * Mathf.Deg2Rad);

            stageCamera.transform.position = new Vector3(camX, targetY + camYOffset, -CameraDistanceZ);
            stageCamera.transform.rotation = Quaternion.Euler(CameraPitchDeg, 0f, 0f);

            if (mobileViewportOverride ?? Application.isMobilePlatform)
            {
                sim.Level.QueryPlatformsInto(cameraPlatforms, cameraSeenIds, sim.Time, sim.Clouds,
                    sim.Difficulty, sim.PlayerX, 500f);
                PicnicCameraLanding landing = PicnicMobileCameraFraming.FindNextLanding(cameraPlatforms,
                    sim.SupportId, sim.PlayerX, sim.PlayerY, sim.PlayerFacing);
                if (!mobileCameraInitialized || sim.OnGround)
                {
                    mobileGroundAnchorY = PicnicAtlasCatalog.ToUnity(sim.PlayerX, sim.PlayerY).y;
                }
                PicnicMobileCameraFrame frame = PicnicMobileCameraFraming.Evaluate(sim.PlayerX / 100f,
                    mobileGroundAnchorY, worldW, stageCamera.aspect, sim.PlayerFacing, landing);
                float offsetY = frame.Distance * Mathf.Tan(CameraPitchDeg * Mathf.Deg2Rad);
                Vector3 desiredPosition = new Vector3(frame.CenterX, frame.TargetY + offsetY, -frame.Distance);
                if (!mobileCameraInitialized)
                {
                    mobileCameraPosition = desiredPosition;
                    mobileCameraInitialized = true;
                }
                else
                {
                    mobileCameraPosition = Vector3.Lerp(mobileCameraPosition, desiredPosition,
                        PicnicMobileCameraFraming.SmoothingFactor(cameraDeltaTime));
                }
                stageCamera.transform.position = mobileCameraPosition;
                orthoWidth = frame.WorldWidth;
                ox = Mathf.Clamp(frame.CenterX - orthoWidth * 0.5f, 0f, Mathf.Max(0f, worldW - orthoWidth));
            }


            if (backdrop != null && backdrop.sprite != null)
            {
                // Place panoramic backdrop at local Z = 34m and scale to fill 100% of perspective frustum
                const float bgLocalZ = 34f;
                float frustumH = 2f * bgLocalZ * Mathf.Tan(CameraFovDeg * 0.5f * Mathf.Deg2Rad);
                float frustumW = frustumH * stageCamera.aspect;
                float bgAspect = backdrop.sprite.bounds.size.x / backdrop.sprite.bounds.size.y;
                float bgH = Mathf.Max(frustumH, frustumW / bgAspect) * 1.38f;
                float bgW = bgH * bgAspect;
                backdrop.transform.localScale = new Vector3(
                    bgW / backdrop.sprite.bounds.size.x,
                    bgH / backdrop.sprite.bounds.size.y,
                    1f);
                backdrop.transform.localRotation = Quaternion.identity;
                float progress = Mathf.Clamp01(ox / Mathf.Max(1f, worldW - orthoWidth));
                float pxOffset = (bgW - frustumW) * (0.5f - progress);
                backdrop.transform.localPosition = new Vector3(pxOffset, (bgH - frustumH) * 0.36f, bgLocalZ);
            }

            // Animate 3D water ripples, subtle UV shimmer scroll, and 3D swimming fish
            float t = sim.Time;
            if (waterTopMat != null)
            {
                waterTopMat.mainTextureOffset = new Vector2(t * 0.06f, Mathf.Sin(t * 0.5f) * 0.03f);
            }
            for (int i = 0; i < waterVolumes.Count; i++)
            {
                UpdateWaterWaves(waterVolumes[i], t);
            }

            for (int i = 0; i < fishes.Count; i++)
            {
                var f = fishes[i];
                float phase = t * f.Speed + f.PhaseOffset;
                float fx = f.AnchorX + Mathf.Sin(phase) * f.SwimRadiusX;
                float vx = Mathf.Cos(phase) * f.SwimRadiusX * f.Speed;
                float fy = f.BaseY + Mathf.Sin(phase * 1.7f) * 3.5f;
                float fz = f.BaseZ + Mathf.Sin(phase * 0.85f) * 0.16f;
                f.Transform.position = PicnicAtlasCatalog.ToUnity(fx, fy, fz);

                // Smooth 3D yaw turn toward swim direction + gentle pitch bob
                float targetYaw = vx >= 0f ? 0f : 180f;
                float turnBlend = Mathf.SmoothStep(180f, 0f, Mathf.Clamp01((vx / (f.SwimRadiusX * f.Speed + 0.01f)) * 2.5f + 0.5f));
                float pitch = -Mathf.Cos(phase * 1.7f) * 8f;
                f.Transform.localRotation = Quaternion.Euler(0f, turnBlend, pitch);

                if (f.TailTransform != null)
                {
                    float wag = Mathf.Sin(t * 11.5f + f.PhaseOffset * 2f) * 26f;
                    f.TailTransform.localRotation = Quaternion.Euler(0f, wag, 0f);
                }
            }

            // Update checkpoint flag colors
            for (int i = 0; i < flags.Count; i++)
            {
                flags[i].FlagRenderer.sharedMaterial = sim.Checkpoint >= flags[i].X ? flagMatActive : flagMatIdle;
            }
        }

        private void BuildRivers(PicnicLevelData level)
        {
            Sprite[] plantSprites =
            {
                PicnicAtlasCatalog.Slice("life-aquatic-v1", PicnicAtlasCatalog.AquaticFrames[4], 100f, "aq:plant1"),
                PicnicAtlasCatalog.Slice("life-aquatic-v1", PicnicAtlasCatalog.AquaticFrames[5], 100f, "aq:plant2"),
            };

            float waterTopY = PicnicAtlasCatalog.ToUnity(0f, PicnicLevelData.WaterY - 35f).y;
            float bedTopY = PicnicAtlasCatalog.ToUnity(0f, PicnicLevelData.RiverBedY).y;

            foreach (var w in level.Waters)
            {
                var riverGo = new GameObject("River3D_" + w.Id);
                riverGo.transform.SetParent(transform, false);

                // Strictly confined to the river trench [w.X, w.X + w.Width] and extending the FULL
                // DepthZ back to the rear terrace edge so there are ZERO unfilled gaps AND ZERO bank invasion.
                float xMin = w.X / 100f;
                float width = Mathf.Max(0.20f, w.Width / 100f);
                // Tuck 0.02u behind the opaque adjacent grass/soil Wall3D (at FrontZ + 0.04f) so depth-testing
                // seals the seam seamlessly with zero gap and zero bleed onto the banks.
                float waterXMin = xMin - 0.02f;
                float waterWidth = width + 0.04f;
                float waterFrontZ = PicnicPlatformController.FrontZ + 0.055f;
                float waterDepth = DepthZ - 0.06f;

                // 1. 3D Riverbed solid earth/rock block extending from bedTopY down to FloorY = -4.2f
                var bedBlock = new GameObject("RiverBed3D", typeof(MeshFilter), typeof(MeshRenderer));
                bedBlock.transform.SetParent(riverGo.transform, false);
                bedBlock.GetComponent<MeshFilter>().sharedMesh = PicnicPlatformController.CreateWorldUVBoxMesh(
                    xMin, width, FloorY, bedTopY, PicnicPlatformController.FrontZ + 0.04f, DepthZ - 0.04f);
                bedBlock.GetComponent<MeshRenderer>().sharedMaterial = riverBedWallMat;

                // 2. 3D Rear river trench wall closing the back of the water volume cleanly
                var backWall = new GameObject("RiverBackWall3D", typeof(MeshFilter), typeof(MeshRenderer));
                backWall.transform.SetParent(riverGo.transform, false);
                backWall.GetComponent<MeshFilter>().sharedMesh = PicnicPlatformController.CreateWorldUVBoxMesh(
                    xMin, width, bedTopY - 0.1f, waterTopY + 0.04f, PicnicPlatformController.FrontZ + DepthZ - 0.18f, 0.22f);
                backWall.GetComponent<MeshRenderer>().sharedMaterial = riverBackWallMat;

                // 3 & 4. Animated 3D volumetric water (full-depth wavy top surface + matching translucent front cutaway)
                var vol = CreateAnimated3DWaterVolume(
                    riverGo.transform, waterXMin, waterWidth, bedTopY, waterTopY, waterFrontZ, waterDepth);
                waterVolumes.Add(vol);

                // 5. 3D Sculpted swimming fish with articulated wagging tails inside the 3D water volume
                int fishCount = Mathf.Clamp(Mathf.FloorToInt(w.Width / 210f), 2, 5);
                for (int i = 0; i < fishCount; i++)
                {
                    float anchor = w.X + (w.Width * (i + 1)) / (fishCount + 1);
                    float maxRadius = Mathf.Max(14f, Mathf.Min(36f, (w.Width / (fishCount + 1)) * 0.45f));
                    float baseY = PicnicLevelData.WaterY + 8f + (i % 3) * 16f;
                    float baseZ = waterFrontZ + 0.18f + (i % 3) * 0.12f;

                    var fRoot = new GameObject("Fish3D_" + i);
                    fRoot.transform.SetParent(riverGo.transform, false);
                    float scale = 1.18f + (i % 3) * 0.16f;
                    fRoot.transform.localScale = Vector3.one * scale;

                    var bodyGo = new GameObject("Body", typeof(MeshFilter), typeof(MeshRenderer));
                    bodyGo.transform.SetParent(fRoot.transform, false);
                    bodyGo.GetComponent<MeshFilter>().sharedMesh = sharedFishBodyMesh;
                    var bmr = bodyGo.GetComponent<MeshRenderer>();
                    bmr.sharedMaterial = fishMaterials[i % fishMaterials.Count];
                    bmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    var tailGo = new GameObject("Tail", typeof(MeshFilter), typeof(MeshRenderer));
                    tailGo.transform.SetParent(fRoot.transform, false);
                    tailGo.transform.localPosition = new Vector3(-0.11f, 0f, 0f);
                    tailGo.GetComponent<MeshFilter>().sharedMesh = sharedFishTailMesh;
                    var tmr = tailGo.GetComponent<MeshRenderer>();
                    tmr.sharedMaterial = fishMaterials[i % fishMaterials.Count];
                    tmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    fishes.Add(new FishEntry
                    {
                        Transform = fRoot.transform,
                        TailTransform = tailGo.transform,
                        AnchorX = anchor,
                        SwimRadiusX = maxRadius,
                        BaseY = baseY,
                        BaseZ = baseZ,
                        PhaseOffset = i * 2.1f + w.X * 0.01f,
                        Speed = 0.42f + (i % 3) * 0.08f,
                    });
                }

                // Underwater plants rooted on top of the 3D riverbed (strictly inside river bounds)
                int plantIdx = 0;
                for (float x = w.X + 55f; x < w.X + w.Width - 45f; x += 210f, plantIdx++)
                {
                    var pGo = new GameObject("Plant_" + plantIdx, typeof(SpriteRenderer));
                    pGo.transform.SetParent(riverGo.transform, false);
                    var sr = pGo.GetComponent<SpriteRenderer>();
                    sr.sprite = plantSprites[plantIdx % plantSprites.Length];
                    sr.color = new Color(1f, 1f, 1f, 0.78f);
                    sr.sortingOrder = 2;
                    pGo.transform.position = PicnicAtlasCatalog.ToUnity(x, PicnicLevelData.RiverBedY, waterFrontZ + 0.14f);
                    FitSprite(pGo.transform, sr.sprite, plantIdx % 2 == 1 ? 0.25f : 0.21f, plantIdx % 2 == 1 ? 0.30f : 0.44f);
                }
            }
        }

        private void BuildHouseAnd3DPorchSteps()
        {
            // Garden cottage recessed cleanly onto the rear of the wooden deck (Z = 0.88f)
            // aligned to Yuna's walkable deck baseline. The cottage art already includes its own
            // painted porch & doorstep, so no extra protruding 3D stair blocks are needed on the deck.
            var house = new GameObject("GardenHome", typeof(SpriteRenderer));
            house.transform.SetParent(transform, false);
            var sr = house.GetComponent<SpriteRenderer>();
            sr.sprite = PicnicAtlasCatalog.Slice("ambient-v2", PicnicAtlasCatalog.AmbienteFrames[0], 100f, "env:house");
            sr.sortingOrder = 4;
            house.transform.position = PicnicAtlasCatalog.ToUnity(PicnicLevelData.HomeX - 18f, PicnicLevelData.HomeY - 12f, 0.88f);
            FitSprite(house.transform, sr.sprite, PicnicLevelData.HomeW / 100f, PicnicLevelData.HomeH / 100f);
            AttachSpriteShadowCaster(house.transform, sr.sprite);
        }

        private void AttachSpriteShadowCaster(Transform spriteTransform, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return;
            var mat = PicnicYunaRigView.CreateAlphaClipShadowMaterial(sprite.texture);
            if (mat == null) return;
            fishMaterials.Add(mat);

            var b = sprite.bounds;
            var tr = sprite.textureRect;
            float tw = sprite.texture.width;
            float th = sprite.texture.height;
            float u0 = tr.xMin / tw, u1 = tr.xMax / tw;
            float v0 = tr.yMin / th, v1 = tr.yMax / th;

            var mesh = new Mesh { name = spriteTransform.name + "_ShadowMesh" };
            mesh.vertices = new[]
            {
                new Vector3(b.min.x, b.min.y, 0f),
                new Vector3(b.max.x, b.min.y, 0f),
                new Vector3(b.min.x, b.max.y, 0f),
                new Vector3(b.max.x, b.max.y, 0f),
            };
            mesh.uv = new[]
            {
                new Vector2(u0, v0), new Vector2(u1, v0),
                new Vector2(u0, v1), new Vector2(u1, v1),
            };
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateNormals();
            dynamicMeshes.Add(mesh);

            var shGo = new GameObject(spriteTransform.name + "_ShadowCaster", typeof(MeshFilter), typeof(MeshRenderer));
            shGo.transform.SetParent(spriteTransform, false);
            shGo.GetComponent<MeshFilter>().sharedMesh = mesh;
            var smr = shGo.GetComponent<MeshRenderer>();
            smr.sharedMaterial = mat;
            smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            smr.receiveShadows = false;
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
            tree.transform.position = PicnicAtlasCatalog.ToUnity(level.Goal - 380f + treeW * 0.5f, PicnicLevelData.GroundY + 4f, 0.25f);
            FitSprite(tree.transform, tr.sprite, treeW / 100f, 3.60f);

            // Lateral Picnic at Goal + 5 anchored by groundAnchorY = 454
            var picnic = new GameObject("LateralPicnic", typeof(SpriteRenderer));
            picnic.transform.SetParent(transform, false);
            var pr = picnic.GetComponent<SpriteRenderer>();
            pr.sprite = PicnicAtlasCatalog.Slice("picnic-lateral-v3", PicnicAtlasCatalog.PicnicLateralFrame, 100f, "env:picnic");
            pr.sortingOrder = 7;
            float pw = 400f;
            float ph = pw * PicnicAtlasCatalog.PicnicLateralFrame.H / PicnicAtlasCatalog.PicnicLateralFrame.W;
            picnic.transform.position = PicnicAtlasCatalog.ToUnity(level.Goal + 5f, PicnicLevelData.GroundY, -0.02f);
            FitSprite(picnic.transform, pr.sprite, pw / 100f, ph / 100f);
        }

        private void BuildCheckpoints(PicnicLevelData level)
        {
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
                var cp = new GameObject("Checkpoint3D_" + i);
                cp.transform.SetParent(transform, false);

                var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "Pole3D";
                pole.transform.SetParent(cp.transform, false);
                Destroy(pole.GetComponent<Collider>());
                pole.transform.position = PicnicAtlasCatalog.ToUnity(cx, cy - 26f, 0.10f);
                pole.transform.localScale = new Vector3(0.08f, 0.28f, 0.08f);
                pole.GetComponent<MeshRenderer>().sharedMaterial = poleMat;

                var flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flag.name = "Flag3D";
                flag.transform.SetParent(cp.transform, false);
                Destroy(flag.GetComponent<Collider>());
                flag.transform.position = PicnicAtlasCatalog.ToUnity(cx + 16f, cy - 38f, 0.10f);
                flag.transform.localScale = new Vector3(0.32f, 0.22f, 0.04f);
                var fmr = flag.GetComponent<MeshRenderer>();
                fmr.sharedMaterial = flagMatIdle;
                flags.Add(new CheckpointFlag { X = cx, FlagRenderer = fmr });
            }
        }

        private WaterVolumeEntry CreateAnimated3DWaterVolume(
            Transform parent, float xMin, float width, float bedY, float topY, float frontZ, float depth)
        {
            int cols = Mathf.Clamp(Mathf.RoundToInt(width * 3.5f), 10, 28);
            const int topRows = 4;

            // Subdivided 3D top wave surface
            int topVertCount = (cols + 1) * (topRows + 1);
            var topBase = new Vector3[topVertCount];
            var topWork = new Vector3[topVertCount];
            var topUvs = new Vector2[topVertCount];
            var topTris = new int[cols * topRows * 6];

            int idx = 0;
            for (int r = 0; r <= topRows; r++)
            {
                float vz = r / (float)topRows;
                float z = frontZ + vz * depth;
                for (int c = 0; c <= cols; c++)
                {
                    float ux = c / (float)cols;
                    float x = xMin + ux * width;
                    topBase[idx] = new Vector3(x, topY, z);
                    topWork[idx] = topBase[idx];
                    topUvs[idx] = new Vector2(ux, vz);
                    idx++;
                }
            }
            int ti = 0;
            for (int r = 0; r < topRows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int a = r * (cols + 1) + c;
                    int b = a + 1;
                    int d = a + (cols + 1);
                    int e = d + 1;
                    topTris[ti++] = a; topTris[ti++] = d; topTris[ti++] = b;
                    topTris[ti++] = b; topTris[ti++] = d; topTris[ti++] = e;
                }
            }

            var topMesh = new Mesh { name = "WaterTopWave3D_Mesh" };
            topMesh.MarkDynamic();
            topMesh.vertices = topWork;
            topMesh.uv = topUvs;
            topMesh.triangles = topTris;
            topMesh.RecalculateNormals();
            dynamicMeshes.Add(topMesh);

            var topGo = new GameObject("WaterTopSurface3D", typeof(MeshFilter), typeof(MeshRenderer));
            topGo.transform.SetParent(parent, false);
            topGo.GetComponent<MeshFilter>().sharedMesh = topMesh;
            var topMr = topGo.GetComponent<MeshRenderer>();
            topMr.sharedMaterial = waterTopMat;
            topMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Subdivided 3D front glass cutaway matching the top wave edge
            int frontVertCount = (cols + 1) * 2;
            var frontBase = new Vector3[frontVertCount];
            var frontWork = new Vector3[frontVertCount];
            var frontUvs = new Vector2[frontVertCount];
            var frontTris = new int[cols * 6];
            for (int c = 0; c <= cols; c++)
            {
                float ux = c / (float)cols;
                float x = xMin + ux * width;
                frontBase[c] = new Vector3(x, bedY, frontZ);
                frontBase[cols + 1 + c] = new Vector3(x, topY, frontZ);
                frontWork[c] = frontBase[c];
                frontWork[cols + 1 + c] = frontBase[cols + 1 + c];
                frontUvs[c] = new Vector2(ux, 0f);
                frontUvs[cols + 1 + c] = new Vector2(ux, 1f);
            }
            ti = 0;
            for (int c = 0; c < cols; c++)
            {
                int a = c;
                int b = c + 1;
                int d = cols + 1 + c;
                int e = d + 1;
                frontTris[ti++] = a; frontTris[ti++] = d; frontTris[ti++] = b;
                frontTris[ti++] = b; frontTris[ti++] = d; frontTris[ti++] = e;
            }

            var frontMesh = new Mesh { name = "WaterFrontWave3D_Mesh" };
            frontMesh.MarkDynamic();
            frontMesh.vertices = frontWork;
            frontMesh.uv = frontUvs;
            frontMesh.triangles = frontTris;
            frontMesh.RecalculateNormals();
            dynamicMeshes.Add(frontMesh);

            var frontGo = new GameObject("WaterFront3D", typeof(MeshFilter), typeof(MeshRenderer));
            frontGo.transform.SetParent(parent, false);
            frontGo.GetComponent<MeshFilter>().sharedMesh = frontMesh;
            var frontMr = frontGo.GetComponent<MeshRenderer>();
            frontMr.sharedMaterial = waterFrontMat;
            frontMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return new WaterVolumeEntry
            {
                TopMesh = topMesh,
                FrontMesh = frontMesh,
                TopBaseVerts = topBase,
                TopWorkVerts = topWork,
                FrontBaseVerts = frontBase,
                FrontWorkVerts = frontWork,
                Cols = cols,
                TopRows = topRows,
                XMin = xMin,
                Width = width,
                FrontZ = frontZ,
                Depth = depth,
                TopY = topY,
                BedY = bedY,
            };
        }

        private static void UpdateWaterWaves(WaterVolumeEntry vol, float t)
        {
            for (int i = 0; i < vol.TopBaseVerts.Length; i++)
            {
                Vector3 p = vol.TopBaseVerts[i];
                p.y += SampleWaveY(p.x, p.z, t);
                vol.TopWorkVerts[i] = p;
            }
            vol.TopMesh.vertices = vol.TopWorkVerts;
            vol.TopMesh.RecalculateNormals();

            int topRowStart = vol.Cols + 1;
            for (int c = 0; c <= vol.Cols; c++)
            {
                Vector3 p = vol.FrontBaseVerts[topRowStart + c];
                p.y += SampleWaveY(p.x, p.z, t);
                vol.FrontWorkVerts[topRowStart + c] = p;
            }
            vol.FrontMesh.vertices = vol.FrontWorkVerts;
        }

        private static float SampleWaveY(float x, float z, float t)
        {
            return Mathf.Sin(x * 2.6f + t * 2.4f) * 0.018f
                 + Mathf.Cos(x * 1.4f - z * 2.8f - t * 1.8f) * 0.012f;
        }

        private static Material CreateTransparentWaterTopMaterial(Shader lit, Shader spriteFallback)
        {
            // Sprites/Default guarantees 100% reliable translucent rendering in URP 17 & WebGL
            // without backface culling or opaque-keyword regressions, paired with a rich sunlit ripple texture.
            var tex = new Texture2D(128, 64, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
            };
            Color frontSun = new Color(0.54f, 0.92f, 0.94f, 0.80f);
            Color deepRear = new Color(0.24f, 0.66f, 0.78f, 0.88f);
            Color foamGlint = new Color(0.92f, 0.99f, 1.00f, 0.92f);

            for (int y = 0; y < 64; y++)
            {
                float vz = y / 63f;
                Color baseCol = Color.Lerp(frontSun, deepRear, vz);
                for (int x = 0; x < 128; x++)
                {
                    float fx = (x / 128f) * Mathf.PI * 2f;
                    float fy = vz * Mathf.PI * 2f;
                    float ripple = Mathf.Sin(fx * 3f + Mathf.Sin(fy * 2f) * 1.4f) * 0.5f + 0.5f;
                    float shimmer = Mathf.Pow(ripple, 5f) * 0.42f;
                    float frontFoam = vz < 0.08f ? (1f - vz / 0.08f) * 0.35f : 0f;
                    Color c = Color.Lerp(baseCol, foamGlint, Mathf.Clamp01(shimmer + frontFoam));
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();

            return new Material(spriteFallback)
            {
                mainTexture = tex,
                mainTextureScale = new Vector2(2.2f, 1.0f),
                color = Color.white,
            };
        }

        private static Mesh Create3DFishBodyMesh()
        {
            // Sculpted 3D streamlined ellipsoid body + dorsal fin facing +X
            const int rings = 8;
            const int segs = 10;
            var verts = new List<Vector3>();
            var tris = new List<int>();

            for (int r = 0; r <= rings; r++)
            {
                float u = r / (float)rings; // 0 = tail base (-0.11), 1 = nose (+0.13)
                float x = Mathf.Lerp(-0.11f, 0.13f, u);
                float profile = Mathf.Sin(u * Mathf.PI);
                float ry = 0.062f * Mathf.Pow(profile, 0.72f);
                float rz = 0.036f * Mathf.Pow(profile, 0.78f);

                for (int s = 0; s <= segs; s++)
                {
                    float a = (s / (float)segs) * Mathf.PI * 2f;
                    verts.Add(new Vector3(x, Mathf.Sin(a) * ry, Mathf.Cos(a) * rz));
                }
            }

            int stride = segs + 1;
            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < segs; s++)
                {
                    int a = r * stride + s;
                    int b = a + 1;
                    int c = a + stride;
                    int d = c + 1;
                    tris.Add(a); tris.Add(c); tris.Add(b);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
            }

            // Dorsal fin (double-sided thin 3D crest on top of fish)
            int fBase = verts.Count;
            verts.Add(new Vector3(0.03f, 0.048f, 0f));
            verts.Add(new Vector3(-0.04f, 0.092f, 0f));
            verts.Add(new Vector3(-0.07f, 0.036f, 0f));
            tris.Add(fBase); tris.Add(fBase + 1); tris.Add(fBase + 2);
            tris.Add(fBase); tris.Add(fBase + 2); tris.Add(fBase + 1);

            var mesh = new Mesh { name = "Fish3D_Body" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Mesh Create3DFishTailMesh()
        {
            // Double-sided forked 3D tail fin pivoting at local origin (0,0,0)
            var verts = new[]
            {
                new Vector3(0f, 0.015f, 0f),
                new Vector3(0f, -0.015f, 0f),
                new Vector3(-0.085f, 0.062f, 0f),
                new Vector3(-0.055f, 0f, 0f),
                new Vector3(-0.085f, -0.062f, 0f),
            };
            var tris = new[]
            {
                0, 2, 3,  0, 3, 2,
                0, 3, 1,  0, 1, 3,
                1, 3, 4,  1, 4, 3,
            };
            var mesh = new Mesh { name = "Fish3D_Tail" };
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            return mesh;
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
            if (waterFrontMat != null) Destroy(waterFrontMat);
            if (waterTopMat != null) Destroy(waterTopMat);
            if (riverBedWallMat != null) Destroy(riverBedWallMat);
            if (riverBackWallMat != null) Destroy(riverBackWallMat);
            if (porchWoodTopMat != null) Destroy(porchWoodTopMat);
            if (porchWoodBodyMat != null) Destroy(porchWoodBodyMat);
            if (poleMat != null) Destroy(poleMat);
            if (flagMatActive != null) Destroy(flagMatActive);
            if (flagMatIdle != null) Destroy(flagMatIdle);
            for (int i = 0; i < fishMaterials.Count; i++) if (fishMaterials[i] != null) Destroy(fishMaterials[i]);
            for (int i = 0; i < dynamicMeshes.Count; i++) if (dynamicMeshes[i] != null) Destroy(dynamicMeshes[i]);
            if (sharedFishBodyMesh != null) Destroy(sharedFishBodyMesh);
            if (sharedFishTailMesh != null) Destroy(sharedFishTailMesh);
        }
    }
}
