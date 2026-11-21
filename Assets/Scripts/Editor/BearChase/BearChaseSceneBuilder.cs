using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using YunaLegend.Arena.Core;
using YunaLegend.BearChase.Core;
using YunaLegend.BearChase.Runtime;
using YunaLegend.Editor.Arena;
using YunaLegend.Editor.ToyRoom;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.Editor.BearChase
{
    /// <summary>
    /// Constroi a cena 3D Painterly da Fase 4 (Perseguicao do Urso Fujao) em Assets/Scenes/BearChase_Gameplay.unity,
    /// reutilizando integralmente a paleta, shaders, Yuna, Nanda e props da ToyRoom + Urso articulado de 16 ossos.
    /// </summary>
    public static class BearChaseSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/BearChase_Gameplay.unity";
        private const string BearObjPath = "Assets/Art/Arena/Models/bear_hunyuan.obj";
        private const string BearTexPath = "Assets/Art/Arena/Models/T_Bear_Albedo.png";

        [MenuItem("YunaLegend/Fase 4 - Bear Chase/Abrir Cena (BearChase_Gameplay)", false, 10)]
        public static void OpenBearChaseScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[BearChaseSceneBuilder] Saia do Play Mode antes de abrir a cena.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.playModeStartScene = null;
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        [MenuItem("YunaLegend/Fase 4 - Bear Chase/Build Final Scene", false, 20)]
        public static void BuildBearChaseScene()
        {
            EditorSceneManager.playModeStartScene = null;
            AssetDatabase.Refresh();
            ToyRoomVisualFactory.ResetMaterialCounter();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConfigureLighting();
            CreatePostProcessVolume();

            var camGo = new GameObject("BearChase_MainCamera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.14f, 0.11f, 0.12f);
            cam.fieldOfView = 32f;
            cam.nearClipPlane = 0.25f;
            cam.farClipPlane = 90f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>();
            var camCtrl = camGo.AddComponent<BearChaseCameraController>();
            camCtrl.Configure(cam);
            camCtrl.ApplyEntrancePose();

            var entranceRoot = BuildEntranceBedroom();
            var chaseRoot = BuildChaseCorridor();
            chaseRoot.SetActive(false);

            var player = CreatePlayer();
            var bear = CreateRunawayBear();
            var nanda = CreateNanda(player.transform);

            var hudGo = new GameObject("BearChase_HUD");
            var hud = hudGo.AddComponent<BearChaseHudController>();
            var caveatFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Caveat-Variable.ttf");
            if (caveatFont != null)
            {
                hud.ConfigureFont(caveatFont);
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var audioGo = new GameObject("BearChase_Audio");
            var audioCtrl = audioGo.AddComponent<BearChaseAudioController>();
            var chaseClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ToyRoom/The Circle Game.mp3");
            audioCtrl.Configure(chaseClip);

            var bootstrapGo = new GameObject("BearChase_Bootstrap");
            var bootstrap = bootstrapGo.AddComponent<BearChaseStageBootstrap>();
            bootstrap.Configure(camCtrl, player, bear, nanda, hud, audioCtrl, entranceRoot, chaseRoot);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterSceneInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log($"[BearChaseSceneBuilder] Cena da Fase 4 salva em {ScenePath}");
        }

        private static void ConfigureLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.56f, 0.50f, 0.52f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.36f, 0.38f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.19f, 0.20f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.95f, 0.88f, 0.76f);
            RenderSettings.fogStartDistance = 26f;
            RenderSettings.fogEndDistance = 78f;

            var sunGo = new GameObject("MorningSun_DirectionalLight");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1.0f, 0.94f, 0.82f);
            sun.intensity = 1.45f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.55f;
            sunGo.transform.rotation = Quaternion.Euler(42f, -26f, 0f);
        }

        private static void CreatePostProcessVolume()
        {
            var volGo = new GameObject("BearChase_PostProcessVolume");
            var volume = volGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.value = TonemappingMode.ACES;

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.20f;
            vignette.smoothness.value = 0.48f;

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.value = 1.05f;
            bloom.intensity.value = 0.24f;

            volume.sharedProfile = profile;
        }

        private static GameObject BuildEntranceBedroom()
        {
            var root = new GameObject("Entrance_Bedroom_Root");
            string floralWallpaperPath = EnsureFloralWallpaperTexture();

            Material woodFloorMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.86f, 0.70f, 0.50f),
                "Assets/Materials/T_Furniture_Wood_Painted.png",
                new Vector2(6f, 4f),
                outlineWidth: 0f);

            Material wallpaperMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                Color.white,
                floralWallpaperPath,
                new Vector2(6f, 2.5f),
                outlineWidth: 0f);

            Material wainscotMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.56f, 0.36f, 0.22f),
                "Assets/Materials/T_Furniture_Wood_Painted.png",
                new Vector2(4f, 1f),
                outlineWidth: 0.002f);

            Material rugBaseMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.16f, 0.54f, 0.52f),
                null,
                outlineWidth: 0.002f);

            Material rugGoldMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.86f, 0.72f, 0.34f),
                "Assets/Materials/T_Floor_Rug_Mandala.png",
                Vector2.one,
                outlineWidth: 0f);

            Material fringeMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.94f, 0.88f, 0.72f),
                null,
                outlineWidth: 0f);

            // Piso de tabuas quentes
            ToyRoomVisualFactory.CreateVisualPrimitive(
                PrimitiveType.Cube, "Entrance_Floor", root.transform,
                new Vector3(0f, -0.05f, 0f), new Vector3(13f, 0.1f, 7f), woodFloorMat);

            // Tapete retangular verde-azulado e dourado com franjas visiveis nas pontas
            ToyRoomVisualFactory.CreateVisualPrimitive(
                PrimitiveType.Cube, "Entrance_Rug_Base", root.transform,
                new Vector3(0.95f, 0.010f, 0.0f), new Vector3(4.8f, 0.020f, 2.8f), rugBaseMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(
                PrimitiveType.Cube, "Entrance_Rug_GoldBorder", root.transform,
                new Vector3(0.95f, 0.015f, 0.0f), new Vector3(4.4f, 0.022f, 2.4f), rugGoldMat);
            for (int f = 0; f < 14; f++)
            {
                float fz = -1.25f + f * (2.50f / 13f);
                ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Cube, $"Entrance_Rug_FringeL_{f}", root.transform,
                    new Vector3(-1.54f, 0.008f, fz), new Vector3(0.22f, 0.014f, 0.07f), fringeMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Cube, $"Entrance_Rug_FringeR_{f}", root.transform,
                    new Vector3(3.44f, 0.008f, fz), new Vector3(0.22f, 0.014f, 0.07f), fringeMat);
            }

            // Parede traseira com papel de parede floral e lambris de madeira escura
            ToyRoomVisualFactory.CreateVisualPrimitive(
                PrimitiveType.Cube, "Entrance_BackWall", root.transform,
                new Vector3(0f, 2.4f, 1.85f), new Vector3(13f, 5.2f, 0.2f), wallpaperMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(
                PrimitiveType.Cube, "Entrance_Wainscoting", root.transform,
                new Vector3(0f, 0.48f, 1.73f), new Vector3(13f, 0.96f, 0.06f), wainscotMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(
                PrimitiveType.Cube, "Entrance_ChairRail", root.transform,
                new Vector3(0f, 0.98f, 1.71f), new Vector3(13f, 0.09f, 0.10f), wainscotMat);

            // Janela arqueada com ceu azul e cortinas creme centralizada sobre o tapete
            BuildArchedWindow(root.transform, new Vector3(0.95f, 2.25f, 1.72f), Quaternion.identity, includeLight: true);

            // Poltrona vermelha estofada na extrema esquerda
            var armchairGo = new GameObject("Entrance_RedArmchair");
            armchairGo.transform.SetParent(root.transform, false);
            armchairGo.transform.position = new Vector3(-3.45f, 0f, 0.18f);
            armchairGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            var armchairSpec = new FurnitureSpec(
                "armchair", "Poltrona Vermelha",
                new ToyRoomPoint(-3.45f, 0.18f), 1.35f, 1.25f);
            ToyRoomPropSculptor.BuildSculptedFurniture(armchairGo.transform, armchairSpec);
            Material redVelvetMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.76f, 0.20f, 0.20f),
                null,
                outlineWidth: 0.003f);
            var armchairRenderers = armchairGo.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < armchairRenderers.Length; i++)
            {
                if (!armchairRenderers[i].name.Contains("Leg"))
                {
                    armchairRenderers[i].sharedMaterial = redVelvetMat;
                }
            }

            // Luz suave de preenchimento na entrada
            var fillGo = new GameObject("Entrance_WarmFill");
            fillGo.transform.SetParent(root.transform, false);
            fillGo.transform.position = new Vector3(0f, 3.2f, -2.5f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.color = new Color(1.0f, 0.94f, 0.84f);
            fill.range = 14f;
            fill.intensity = 1.6f;

            return root;
        }

        private static GameObject BuildChaseCorridor()
        {
            var root = new GameObject("Chase_Corridor_Root");
            Vector3 origin = new Vector3(0f, 0f, 40f);
            root.transform.position = origin;
            string floralWallpaperPath = EnsureFloralWallpaperTexture();

            Material floorMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.88f, 0.72f, 0.50f),
                "Assets/Materials/T_Furniture_Wood_Painted.png",
                new Vector2(2.5f, 2f),
                outlineWidth: 0f);

            Material wallMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                Color.white,
                floralWallpaperPath,
                new Vector2(2.0f, 2.0f),
                outlineWidth: 0f);

            Material baseboardMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.54f, 0.34f, 0.20f),
                "Assets/Materials/T_Furniture_Wood_Painted.png",
                new Vector2(2f, 1f),
                outlineWidth: 0f);

            Material ceilingMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.68f, 0.50f, 0.36f),
                "Assets/Materials/T_Furniture_Wood_Painted.png",
                new Vector2(2f, 2f),
                outlineWidth: 0f);

            Material rugMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.18f, 0.56f, 0.54f),
                "Assets/Materials/T_Floor_Rug_Mandala.png",
                Vector2.one,
                outlineWidth: 0f);

            const float segStep = 4.0f;
            const float corridorHalfWidth = 4.35f;
            const float wallHeight = 5.2f;

            // Parede traseira fechando o inicio do corredor (sem vazio escuro atras da Yuna)
            ToyRoomVisualFactory.CreateVisualPrimitive(
                PrimitiveType.Cube, "Corridor_StartBackWall", root.transform,
                new Vector3(0f, wallHeight * 0.5f, -9.5f), new Vector3(corridorHalfWidth * 2.3f, wallHeight, 0.3f), wallMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(
                PrimitiveType.Cube, "Corridor_StartBackBaseboard", root.transform,
                new Vector3(0f, 0.35f, -9.32f), new Vector3(corridorHalfWidth * 2.3f, 0.7f, 0.1f), baseboardMat);

            for (float z = -8f; z <= BearChaseConfig.TrackLength + 12f; z += segStep)
            {
                float zMid = z + segStep * 0.5f;
                float cx = BearChaseConfig.TrackCenterX(zMid);
                float slope = BearChaseConfig.TrackTangentSlopeX(zMid);
                float yaw = Mathf.Atan2(slope, 1f) * Mathf.Rad2Deg;
                float segLen = segStep + 1.45f;

                var segGo = new GameObject($"CorridorSeg_{Mathf.RoundToInt(z)}");
                segGo.transform.SetParent(root.transform, false);
                segGo.transform.localPosition = new Vector3(cx, 0f, zMid);
                segGo.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

                // Piso e teto
                ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Cube, "Floor", segGo.transform,
                    new Vector3(0f, -0.05f, 0f), new Vector3(corridorHalfWidth * 2.2f, 0.1f, segLen), floorMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Cube, "Ceiling", segGo.transform,
                    new Vector3(0f, wallHeight, 0f), new Vector3(corridorHalfWidth * 2.2f, 0.2f, segLen), ceilingMat);

                // Paredes laterais e lambris de madeira
                ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Cube, "Wall_L", segGo.transform,
                    new Vector3(-corridorHalfWidth, wallHeight * 0.5f, 0f), new Vector3(0.22f, wallHeight, segLen), wallMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Cube, "Baseboard_L", segGo.transform,
                    new Vector3(-corridorHalfWidth + 0.12f, 0.48f, 0f), new Vector3(0.08f, 0.96f, segLen), baseboardMat);

                ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Cube, "Wall_R", segGo.transform,
                    new Vector3(corridorHalfWidth, wallHeight * 0.5f, 0f), new Vector3(0.22f, wallHeight, segLen), wallMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Cube, "Baseboard_R", segGo.transform,
                    new Vector3(corridorHalfWidth - 0.12f, 0.48f, 0f), new Vector3(0.08f, 0.96f, segLen), baseboardMat);

                int idx = Mathf.RoundToInt(z / segStep);
                if (idx >= 0 && idx % 4 == 0)
                {
                    bool leftWindow = (idx / 4) % 2 == 0;
                    float wx = leftWindow ? (-corridorHalfWidth + 0.16f) : (corridorHalfWidth - 0.16f);
                    Quaternion wRot = Quaternion.Euler(0f, leftWindow ? -90f : 90f, 0f);
                    BuildArchedWindow(segGo.transform, new Vector3(wx, 2.55f, 0f), wRot, includeLight: idx % 4 == 0);
                }

                if (idx >= 1 && idx % 6 == 0 && z < BearChaseConfig.TrackLength - 10f)
                {
                    ToyRoomVisualFactory.CreateVisualPrimitive(
                        PrimitiveType.Cube, "DecorRug", segGo.transform,
                        new Vector3((idx % 12 == 0 ? -0.5f : 0.5f), 0.012f, 0f), new Vector3(3.1f, 0.02f, 2.2f), rugMat);
                }
            }

            // Luz branca/dourada acolhedora ao final do corredor
            float endX = BearChaseConfig.TrackCenterX(BearChaseConfig.TrackLength);
            Material exitGlowMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(1f, 0.98f, 0.90f),
                null,
                emission: new Color(1.6f, 1.45f, 1.15f),
                outlineWidth: 0f);
            ToyRoomVisualFactory.CreateVisualPrimitive(
                PrimitiveType.Cube, "Corridor_EndBrightDoor", root.transform,
                new Vector3(endX, 2.2f, BearChaseConfig.TrackLength + 7f), new Vector3(4.2f, 4.4f, 0.3f), exitGlowMat);

            var endLightGo = new GameObject("Corridor_EndLight");
            endLightGo.transform.SetParent(root.transform, false);
            endLightGo.transform.localPosition = new Vector3(endX, 2.4f, BearChaseConfig.TrackLength + 4f);
            var endLight = endLightGo.AddComponent<Light>();
            endLight.type = LightType.Point;
            endLight.color = new Color(1.0f, 0.96f, 0.85f);
            endLight.range = 24f;
            endLight.intensity = 3.2f;

            BuildCorridorObstacles(root.transform);
            return root;
        }

        private static void BuildCorridorObstacles(Transform corridorRoot)
        {
            var obsRoot = new GameObject("Chase_Obstacles");
            obsRoot.transform.SetParent(corridorRoot, false);

            var specs = BearChaseConfig.BuildDefaultObstacles();
            for (int i = 0; i < specs.Length; i++)
            {
                BearChaseObstacleSpec spec = specs[i];
                float worldX = BearChaseConfig.TrackCenterX(spec.Z) + spec.LaneOffsetX;
                var go = new GameObject($"Obstacle_{i:00}_{spec.Kind}");
                go.transform.SetParent(obsRoot.transform, false);
                go.transform.localPosition = new Vector3(worldX, 0f, spec.Z);

                switch (spec.Kind)
                {
                    case BearChaseObstacleKind.Bookcase:
                        ToyRoomPropSculptor.BuildSculptedFurniture(go.transform, new FurnitureSpec("bookshelf", "Estante", new ToyRoomPoint(0f, 0f), 1.55f, 0.72f));
                        go.transform.localScale = Vector3.one * 0.68f;
                        break;
                    case BearChaseObstacleKind.Armchair:
                        ToyRoomPropSculptor.BuildSculptedFurniture(go.transform, new FurnitureSpec("armchair", "Poltrona", new ToyRoomPoint(0f, 0f), 1.25f, 1.1f));
                        go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                        go.transform.localScale = Vector3.one * 0.75f;
                        break;
                    case BearChaseObstacleKind.Stool:
                        ToyRoomPropSculptor.BuildSculptedFurniture(go.transform, new FurnitureSpec("table", "Banquinho", new ToyRoomPoint(0f, 0f), 0.95f, 0.95f));
                        go.transform.localScale = new Vector3(0.75f, 0.68f, 0.75f);
                        break;
                    case BearChaseObstacleKind.BlockTower:
                        ToyRoomPropSculptor.BuildSculptedToy(go.transform, new ToySpec("blocks", "Blocos", 0.85f, new ToyRoomPoint(0f, 0f)));
                        go.transform.localScale = Vector3.one * 1.15f;
                        break;
                    case BearChaseObstacleKind.ToyTrain:
                        if (!ToyRoomPropSculptor.TryAttachGeneratedPropModel(go.transform, "train", 0.75f, 1.1f, 1.3f))
                        {
                            ToyRoomPropSculptor.BuildSculptedToy(go.transform, new ToySpec("train", "Trem", 0.9f, new ToyRoomPoint(0f, 0f)));
                        }
                        break;
                    case BearChaseObstacleKind.PlushBunny:
                        if (!ToyRoomPropSculptor.TryAttachGeneratedPropModel(go.transform, "bunny", 0.78f, 0.8f, 0.8f))
                        {
                            ToyRoomPropSculptor.BuildSculptedToy(go.transform, new ToySpec("bunny", "Coelho", 0.96f, new ToyRoomPoint(0f, 0f)));
                        }
                        break;
                    case BearChaseObstacleKind.RubberDuck:
                        if (!ToyRoomPropSculptor.TryAttachGeneratedPropModel(go.transform, "duck", 0.68f, 0.8f, 0.8f))
                        {
                            ToyRoomPropSculptor.BuildSculptedToy(go.transform, new ToySpec("duck", "Patinho", 0.95f, new ToyRoomPoint(0f, 0f)));
                        }
                        break;
                    default:
                        ToyRoomPropSculptor.BuildSculptedToy(go.transform, new ToySpec("teddy", "Ursinho", 0.94f, new ToyRoomPoint(0f, 0f)));
                        go.transform.localScale = Vector3.one * 1.35f;
                        break;
                }
            }
        }

        private static void BuildArchedWindow(Transform parent, Vector3 localPos, Quaternion localRot, bool includeLight)
        {
            var winGo = new GameObject("ArchedWindow");
            winGo.transform.SetParent(parent, false);
            winGo.transform.localPosition = localPos;
            winGo.transform.localRotation = localRot;

            Material frameMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.48f, 0.30f, 0.18f),
                "Assets/Materials/T_Furniture_Wood_Painted.png",
                outlineWidth: 0.002f);
            Material skyMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.42f, 0.76f, 0.96f),
                null,
                emission: new Color(0.08f, 0.18f, 0.28f),
                outlineWidth: 0f);
            Material curtainMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.88f, 0.79f, 0.62f),
                null,
                outlineWidth: 0.002f);

            // Vidro ceu azul (4 paineis)
            ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "SkyPane", winGo.transform, new Vector3(0f, 0f, 0.02f), new Vector3(1.5f, 1.8f, 0.04f), skyMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cylinder, "ArchSky", winGo.transform, new Vector3(0f, 0.90f, 0.02f), new Vector3(1.5f, 0.03f, 1.5f), skyMat, new Vector3(90f, 0f, 0f));

            // Moldura e divisoria em cruz
            ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "FrameL", winGo.transform, new Vector3(-0.78f, 0.18f, -0.02f), new Vector3(0.10f, 2.25f, 0.10f), frameMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "FrameR", winGo.transform, new Vector3(0.78f, 0.18f, -0.02f), new Vector3(0.10f, 2.25f, 0.10f), frameMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "Sill", winGo.transform, new Vector3(0f, -0.94f, -0.04f), new Vector3(1.75f, 0.12f, 0.18f), frameMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "MullionV", winGo.transform, new Vector3(0f, 0.20f, -0.01f), new Vector3(0.06f, 2.2f, 0.06f), frameMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "MullionH", winGo.transform, new Vector3(0f, 0.12f, -0.01f), new Vector3(1.55f, 0.06f, 0.06f), frameMat);

            // Cortinas creme abertas nas laterais com dobras suaves
            ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cylinder, "CurtainL_1", winGo.transform, new Vector3(-0.82f, 0.10f, -0.07f), new Vector3(0.22f, 1.12f, 0.14f), curtainMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cylinder, "CurtainL_2", winGo.transform, new Vector3(-0.96f, 0.10f, -0.06f), new Vector3(0.22f, 1.12f, 0.14f), curtainMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cylinder, "CurtainR_1", winGo.transform, new Vector3(0.82f, 0.10f, -0.07f), new Vector3(0.22f, 1.12f, 0.14f), curtainMat);
            ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cylinder, "CurtainR_2", winGo.transform, new Vector3(0.96f, 0.10f, -0.06f), new Vector3(0.22f, 1.12f, 0.14f), curtainMat);

            if (includeLight)
            {
                var lightGo = new GameObject("WindowLight");
                lightGo.transform.SetParent(winGo.transform, false);
                lightGo.transform.localPosition = new Vector3(0f, 0f, -1.2f);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1.0f, 0.94f, 0.82f);
                light.range = 13f;
                light.intensity = 1.9f;
            }
        }

        private static BearChasePlayerController CreatePlayer()
        {
            var playerGo = new GameObject("Yuna_BearChase");
            playerGo.transform.position = new Vector3(-2.65f, 0f, 0f);
            playerGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            ToyRoomVisualFactory.BuildYunaCharacter3D(
                playerGo.transform,
                out var bodyRoot,
                out var legL,
                out var legR,
                out var armL,
                out var armR,
                out var ponytail);

            // Espada de brinquedo na mao direita da Yuna
            Transform handR = FindDeepChild(playerGo.transform, "Hand_R");
            if (handR != null)
            {
                var swordAnchor = new GameObject("Yuna_ToySword");
                swordAnchor.transform.SetParent(handR, false);
                swordAnchor.transform.localPosition = new Vector3(0.02f, -0.04f, 0.12f);
                swordAnchor.transform.localRotation = Quaternion.Euler(78f, 0f, 0f);
                swordAnchor.transform.localScale = Vector3.one * 0.72f;
                ToyRoomPropSculptor.BuildSculptedToySword(swordAnchor.transform);
            }

            var ctrl = playerGo.AddComponent<BearChasePlayerController>();
            ctrl.Configure(bodyRoot, legL, legR, armL, armR, ponytail);
            return ctrl;
        }

        private static BearChaseBearAnimator CreateRunawayBear()
        {
            Material bearMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                Color.white,
                BearTexPath,
                outlineWidth: 0.0038f);

            var bearGo = ArenaEnemyRigBuilder.BuildRiggedEnemy(
                "Bear_Runaway",
                ArenaEnemyId.Bear,
                null,
                BearObjPath,
                bearMat,
                1.08f);

            bearGo.transform.position = new Vector3(2.15f, 0f, 0.05f);
            bearGo.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

            Transform head = FindDeepChild(bearGo.transform, "Head");

            // Palpebras fechadas (entrada dormindo) e olhos bem abertos (acordado/correndo)
            Transform eyelidsRoot = null;
            Transform wideEyesRoot = null;
            if (head != null)
            {
                Material darkMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.14f, 0.09f, 0.07f), null, outlineWidth: 0f);
                Material whiteMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.98f, 0.96f, 0.92f), null, outlineWidth: 0f);

                eyelidsRoot = new GameObject("SleepingEyelids").transform;
                eyelidsRoot.SetParent(head, false);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "LidL", eyelidsRoot, new Vector3(-0.07f, 0.12f, 0.18f), new Vector3(0.055f, 0.014f, 0.02f), darkMat, new Vector3(0f, 0f, 8f));
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "LidR", eyelidsRoot, new Vector3(0.07f, 0.12f, 0.18f), new Vector3(0.055f, 0.014f, 0.02f), darkMat, new Vector3(0f, 0f, -8f));

                wideEyesRoot = new GameObject("WideEyes").transform;
                wideEyesRoot.SetParent(head, false);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Sphere, "WhiteL", wideEyesRoot, new Vector3(-0.07f, 0.12f, 0.175f), new Vector3(0.052f, 0.058f, 0.025f), whiteMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Sphere, "WhiteR", wideEyesRoot, new Vector3(0.07f, 0.12f, 0.175f), new Vector3(0.052f, 0.058f, 0.025f), whiteMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Sphere, "PupilL", wideEyesRoot, new Vector3(-0.07f, 0.12f, 0.188f), new Vector3(0.026f, 0.030f, 0.016f), darkMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Sphere, "PupilR", wideEyesRoot, new Vector3(0.07f, 0.12f, 0.188f), new Vector3(0.026f, 0.030f, 0.016f), darkMat);
                wideEyesRoot.gameObject.SetActive(false);
            }

            var animator = bearGo.AddComponent<BearChaseBearAnimator>();
            var animRig = bearGo.GetComponent<YunaLegend.Arena.Runtime.ArenaEnemyRigAnimator>();
            if (animRig != null)
            {
                Object.DestroyImmediate(animRig);
            }

            animator.Configure(default, eyelidsRoot, wideEyesRoot);
            return animator;
        }

        private static BearChaseNandaController CreateNanda(Transform yunaTransform)
        {
            var nandaGo = new GameObject("Nanda_BlueGuide");
            nandaGo.transform.position = yunaTransform.position + new Vector3(-0.78f, 1.52f, 0.15f);
            ToyRoomVisualFactory.BuildNandaFairy3D(nandaGo.transform);

            var fairyBody = nandaGo.transform.Find("Nanda_FairyBody");
            if (fairyBody != null)
            {
                fairyBody.localScale = Vector3.one * 0.44f;
            }

            var lightGo = new GameObject("Nanda_AuraLight");
            lightGo.transform.SetParent(nandaGo.transform, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.36f, 0.86f, 1.0f);
            light.range = 5.0f;
            light.intensity = 2.0f;

            var ctrl = nandaGo.AddComponent<BearChaseNandaController>();
            ctrl.Configure(yunaTransform);
            return ctrl;
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                Transform found = FindDeepChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void RegisterSceneInBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == ScenePath)
                {
                    if (!scenes[i].enabled)
                    {
                        scenes[i].enabled = true;
                        EditorBuildSettings.scenes = scenes.ToArray();
                    }

                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static string EnsureFloralWallpaperTexture()
        {
            const string texPath = "Assets/Materials/T_BearChase_FloralWallpaper.png";
            if (File.Exists(texPath))
            {
                return texPath;
            }

            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var bg = new Color32(238, 224, 196, 255);
            var stripe = new Color32(230, 214, 182, 255);
            var petal = new Color32(216, 178, 146, 255);
            var center = new Color32(226, 198, 128, 255);
            var leaf = new Color32(188, 198, 160, 255);

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool onStripe = (x % 32) < 6;
                    pixels[y * size + x] = onStripe ? stripe : bg;
                }
            }

            // Motivos florais delicados em grade alternada
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int cx = col * 64 + (row % 2 == 0 ? 16 : 48);
                    int cy = row * 64 + 32;
                    DrawDot(pixels, size, cx - 7, cy - 5, 4, leaf);
                    DrawDot(pixels, size, cx + 7, cy + 5, 4, leaf);
                    DrawDot(pixels, size, cx - 6, cy, 4, petal);
                    DrawDot(pixels, size, cx + 6, cy, 4, petal);
                    DrawDot(pixels, size, cx, cy - 6, 4, petal);
                    DrawDot(pixels, size, cx, cy + 6, 4, petal);
                    DrawDot(pixels, size, cx, cy, 3, center);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(texPath) ?? "Assets/Materials");
            File.WriteAllBytes(texPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
            return texPath;
        }

        private static void DrawDot(Color32[] pixels, int size, int cx, int cy, int r, Color32 col)
        {
            int r2 = r * r;
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    if (dx * dx + dy * dy > r2)
                    {
                        continue;
                    }

                    int px = (cx + dx + size) % size;
                    int py = (cy + dy + size) % size;
                    pixels[py * size + px] = col;
                }
            }
        }
    }
}
