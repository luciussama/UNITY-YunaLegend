using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using YunaLegend.ToyRoom.Core;
using YunaLegend.ToyRoom.Runtime;
using YunaLegend.UI;

namespace YunaLegend.Editor.ToyRoom
{
    /// <summary>
    /// Constroi a cena 3D isometrica da Fase 2 (ToyRoom_Gameplay.unity) com direcao artistica
    /// Diablo IV x Legend of Mana, shader pictorico, VFX de luz matinal, audio e registro de build.
    /// </summary>
    [InitializeOnLoad]
    public static class ToyRoomSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/ToyRoom_Gameplay.unity";
        private const string PreviewOutputPath = "/Users/sergio/.gemini/antigravity/brain/1d17e175-ea96-41be-9e53-6cf1d38c269a/scratch/toyroom_preview.png";
        private const string CloseupPreviewOutputPath = "/Users/sergio/.gemini/antigravity/brain/1d17e175-ea96-41be-9e53-6cf1d38c269a/scratch/toyroom_closeup_yuna_nanda.png";
        private const string AutoBuildVersionKey = "YunaLegend.ToyRoomSceneBuildVersion";
        private const int CurrentSceneBuildVersion = 30;
        private static double lastTriggerCheckTime;

        static ToyRoomSceneBuilder()
        {
            if (!Application.isBatchMode)
            {
                EditorApplication.delayCall += TryAutoRebuildIfOutdated;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                EditorApplication.update += PollExternalRebuildTrigger;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += TryAutoRebuildIfOutdated;
            }
        }

        private static void PollExternalRebuildTrigger()
        {
            if (Application.isBatchMode || EditorApplication.timeSinceStartup - lastTriggerCheckTime < 1.0)
            {
                return;
            }

            lastTriggerCheckTime = EditorApplication.timeSinceStartup;
            const string auditTriggerPath = "Temp/trigger_toyroom_audit.txt";
            if (File.Exists(auditTriggerPath) && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                File.Delete(auditTriggerPath);
                CapturePropsAudit();
                return;
            }

            const string triggerPath = "Temp/trigger_toyroom_rebuild.txt";
            if (!File.Exists(triggerPath))
            {
                return;
            }

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            File.Delete(triggerPath);
            SessionState.SetInt(AutoBuildVersionKey, CurrentSceneBuildVersion);
            BuildBaseScene();
        }

        private static void TryAutoRebuildIfOutdated()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (SessionState.GetInt(AutoBuildVersionKey, 0) >= CurrentSceneBuildVersion
                && File.Exists("Assets/Art/ToyRoom/Materials/T_Yuna_Face_Painted.png"))
            {
                return;
            }

            SessionState.SetInt(AutoBuildVersionKey, CurrentSceneBuildVersion);
            string activePath = EditorSceneManager.GetActiveScene().path;
            BuildBaseScene();

            if (!string.IsNullOrEmpty(activePath) && activePath != ScenePath && File.Exists(activePath))
            {
                EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);
            }
        }

        [MenuItem("YunaLegend/Fase 2 - Toy Room/Abrir Cena (ToyRoom_Gameplay)", false, 10)]
        public static void OpenToyRoomScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[ToyRoomSceneBuilder] Saia do Play Mode antes de abrir a cena ToyRoom_Gameplay.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.playModeStartScene = null;
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        [MenuItem("YunaLegend/Fase 2 - Toy Room/Build Final Scene (P1-P8)", false, 20)]
        public static void BuildBaseScene()
        {
            AssetDatabase.Refresh();
            ToyRoomVisualFactory.ResetMaterialCounter();
            EnsurePainterlyRendererFeature();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConfigureLighting();
            var cam = CreateIsometricCamera();
            CreatePostProcessVolume();
            CreateRoomArchitecture();
            CreateWindowsAndLightShafts();
            CreateFloatingDustMotes();
            var chest = CreateFurnitureModels();
            var toys = CreateToyModels();
            var player = CreatePlayer();
            var isoCam = cam.GetComponent<IsometricToyRoomCamera>();
            if (isoCam != null)
            {
                isoCam.Configure(player.transform);
            }
            var swordView = CreateToySword(chest.transform.position);
            var nanda = CreateNandaGuide(player.transform, chest.transform, swordView.transform);

            var hudGo = new GameObject("ToyRoom_HUD");
            var hud = hudGo.AddComponent<ToyRoomHudController>();
            var caveatFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Caveat-Variable.ttf");
            if (caveatFont != null)
            {
                hud.ConfigureFont(caveatFont);
            }

            var narrativeGo = new GameObject("ToyRoom_Narrative");
            var narrative = narrativeGo.AddComponent<ToyRoomNarrativeController>();
            narrative.Configure(hud, swordView);

            var audioGo = new GameObject("ToyRoom_Audio");
            var audioCtrl = audioGo.AddComponent<ToyRoomAudioController>();
            var explorationClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ToyRoom/The Circle Game.mp3");
            var swordClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ToyRoom/Circular Dissonance (1).mp3");
            audioCtrl.ConfigureMusic(explorationClip, swordClip);

            var bootstrapGo = new GameObject("ToyRoom_Bootstrap");
            var bootstrap = bootstrapGo.AddComponent<ToyRoomStageBootstrap>();
            bootstrap.Configure(cam, player, chest, nanda, swordView, hud, narrative, audioCtrl, toys, autoExplore: false);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterSceneInBuildSettings();
            AssetDatabase.Refresh();
            CapturePreview();
            Debug.Log($"[ToyRoomSceneBuilder] Cena final salva em {ScenePath}");
        }

        [MenuItem("YunaLegend/Fase 2 - Toy Room/Capture Preview")]
        public static void CapturePreview()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[ToyRoomSceneBuilder] MainCamera nao encontrada.");
                return;
            }

            RenderCameraToPng(cam, PreviewOutputPath, 1280, 720);

            Vector3 origPos = cam.transform.position;
            Quaternion origRot = cam.transform.rotation;
            float origFov = cam.fieldOfView;

            // Afastar temporariamente a Nanda no close-up para nao cobrir o rabo de cavalo da Yuna
            var nandaObj = GameObject.Find("Nanda_BlueGuide");
            Vector3 nandaOrigPos = Vector3.zero;
            if (nandaObj != null)
            {
                nandaOrigPos = nandaObj.transform.position;
                nandaObj.transform.position = nandaOrigPos + new Vector3(-1.80f, 0.10f, 0.25f);
            }

            // Close-up 3/4 ligeiramente elevado (enquadrando 100% da Yuna sem cortar o rabo de cavalo nem os sapatos)
            cam.transform.position = new Vector3(-1.82f, 1.50f, -4.00f);
            cam.transform.LookAt(new Vector3(-0.08f, 0.72f, -1.10f));
            cam.fieldOfView = 28f;
            RenderCameraToPng(cam, CloseupPreviewOutputPath, 1280, 720);

            if (nandaObj != null)
            {
                nandaObj.transform.position = nandaOrigPos;
            }

            cam.transform.position = origPos;
            cam.transform.rotation = origRot;
            cam.fieldOfView = origFov;
        }

        [MenuItem("YunaLegend/Fase 2 - Toy Room/Capture Props Audit")]
        public static void CapturePropsAudit()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[ToyRoomSceneBuilder] MainCamera nao encontrada.");
                return;
            }

            const string auditDir = "/Users/sergio/.gemini/antigravity/brain/d3320b50-1c20-45d4-95f7-1ce8df59d425/scratch/audit";
            Directory.CreateDirectory(auditDir);

            Vector3 origPos = cam.transform.position;
            Quaternion origRot = cam.transform.rotation;
            float origFov = cam.fieldOfView;

            cam.fieldOfView = 35f;
            cam.transform.position = new Vector3(0f, 13.5f, -15.5f);
            cam.transform.LookAt(new Vector3(0f, 0.5f, 0.5f));
            RenderCameraToPng(cam, Path.Combine(auditDir, "00_room_overview.png"), 1280, 720);

            foreach (var item in ToyRoomCatalog.Furniture)
            {
                var go = GameObject.Find($"Furniture_{item.Id}");
                if (go == null) continue;
                Vector3 p = go.transform.position;
                cam.transform.position = p + new Vector3(0f, 1.85f, -3.2f);
                cam.transform.LookAt(p + new Vector3(0f, 0.70f, 0f));
                RenderCameraToPng(cam, Path.Combine(auditDir, $"furn_{item.Id}.png"), 640, 480);
            }

            foreach (var toy in ToyRoomCatalog.Toys)
            {
                var go = GameObject.Find($"Toy_{toy.Id}");
                if (go == null) continue;
                Vector3 p = go.transform.position;
                cam.transform.position = p + new Vector3(0f, 0.85f, -1.65f);
                cam.transform.LookAt(p + new Vector3(0f, 0.35f, 0f));
                RenderCameraToPng(cam, Path.Combine(auditDir, $"toy_{toy.Id}.png"), 640, 480);
            }

            cam.transform.position = origPos;
            cam.transform.rotation = origRot;
            cam.fieldOfView = origFov;
            Debug.Log($"[ToyRoomSceneBuilder] Props audit salvo em {auditDir}");
        }

        private static void RenderCameraToPng(Camera cam, string outputPath, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "/tmp");
            File.WriteAllBytes(outputPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Debug.Log($"[ToyRoomSceneBuilder] Preview salvo em {outputPath}");
        }

        [MenuItem("YunaLegend/Fase 2 - Toy Room/Capture TitleScreen Preview")]
        public static void CaptureTitleScreenPreview()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/TitleScreen.unity", OpenSceneMode.Single);
            var selector = Object.FindFirstObjectByType<TitlePhaseSelector>();
            if (selector != null)
            {
                var so = new SerializedObject(selector);
                var dropdownObj = so.FindProperty("dropdownRoot").objectReferenceValue as GameObject;
                if (dropdownObj != null)
                {
                    dropdownObj.SetActive(true);
                }
            }

            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            const int width = 1280;
            const int height = 720;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            string outPath = "/Users/sergio/.gemini/antigravity/brain/779e5ec9-ec3b-4b7a-9392-60ee6785d855/scratch/titlescreen_preview.png";
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Debug.Log($"[ToyRoomSceneBuilder] TitleScreen preview salvo em {outPath}");
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

        [MenuItem("YunaLegend/Fase 2 - Toy Room/Register In TitleScreen")]
        public static void RegisterToyRoomInTitleScreen()
        {
            const string titleScenePath = "Assets/Scenes/TitleScreen.unity";
            var scene = EditorSceneManager.OpenScene(titleScenePath, OpenSceneMode.Single);
            var selector = Object.FindFirstObjectByType<TitlePhaseSelector>();
            if (selector == null)
            {
                Debug.LogError("[ToyRoomSceneBuilder] TitlePhaseSelector nao encontrado em TitleScreen.");
                return;
            }

            var so = new SerializedObject(selector);
            var scenesProp = so.FindProperty("itemScenes");
            var buttonsProp = so.FindProperty("itemButtons");
            var labelsProp = so.FindProperty("itemLabels");

            for (int i = 0; i < scenesProp.arraySize; i++)
            {
                if (scenesProp.GetArrayElementAtIndex(i).stringValue == "ToyRoom_Gameplay")
                {
                    Debug.Log("[ToyRoomSceneBuilder] ToyRoom_Gameplay ja esta registrado no TitlePhaseSelector.");
                    return;
                }
            }

            if (buttonsProp.arraySize == 0 || labelsProp.arraySize == 0)
            {
                Debug.LogError("[ToyRoomSceneBuilder] Dropdown sem itens base para clonar.");
                return;
            }

            var templateButton = buttonsProp.GetArrayElementAtIndex(0).objectReferenceValue as Button;
            if (templateButton == null)
            {
                return;
            }

            var clonedGo = Object.Instantiate(templateButton.gameObject, templateButton.transform.parent);
            clonedGo.name = "PhaseItem_ToyRoom";
            var clonedRt = clonedGo.GetComponent<RectTransform>();
            var templateRt = templateButton.GetComponent<RectTransform>();
            clonedRt.anchoredPosition = templateRt.anchoredPosition + new Vector2(0f, -38f * scenesProp.arraySize);

            var dropdownRt = templateButton.transform.parent.GetComponent<RectTransform>();
            if (dropdownRt != null)
            {
                dropdownRt.sizeDelta = new Vector2(dropdownRt.sizeDelta.x, dropdownRt.sizeDelta.y + 38f);
            }

            var clonedButton = clonedGo.GetComponent<Button>();
            var clonedLabel = clonedGo.GetComponentInChildren<Text>(true);
            if (clonedLabel != null)
            {
                clonedLabel.text = "Sala de Brinquedos";
            }

            int nextIdx = scenesProp.arraySize;
            scenesProp.InsertArrayElementAtIndex(nextIdx);
            scenesProp.GetArrayElementAtIndex(nextIdx).stringValue = "ToyRoom_Gameplay";

            buttonsProp.InsertArrayElementAtIndex(nextIdx);
            buttonsProp.GetArrayElementAtIndex(nextIdx).objectReferenceValue = clonedButton;

            labelsProp.InsertArrayElementAtIndex(nextIdx);
            labelsProp.GetArrayElementAtIndex(nextIdx).objectReferenceValue = clonedLabel;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, titleScenePath);
            Debug.Log("[ToyRoomSceneBuilder] ToyRoom_Gameplay registrado no TitlePhaseSelector.");
        }

        private static void ConfigureLighting()
        {
            // Toda a iluminacao direta parte exclusivamente das 2 janelas e da fadinha Nanda;
            // o ambiente fornece apenas bounce indireto suave para manter a sala bem iluminada sem lavar as sombras.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.44f, 0.40f, 0.46f);
            RenderSettings.ambientEquatorColor = new Color(0.34f, 0.30f, 0.35f);
            RenderSettings.ambientGroundColor = new Color(0.18f, 0.15f, 0.20f);
        }

        private static Camera CreateIsometricCamera()
        {
            var camGo = new GameObject("Isometric_MainCamera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.09f, 0.08f, 0.14f);
            cam.fieldOfView = 27f;
            cam.nearClipPlane = 0.5f;
            cam.farClipPlane = 60f;

            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>();
            camGo.AddComponent<IsometricToyRoomCamera>();

            camGo.transform.position = new Vector3(0.0f, 4.35f, -9.20f);
            camGo.transform.LookAt(new Vector3(0.0f, 0.95f, -1.0f));
            return cam;
        }

        private static void CreatePostProcessVolume()
        {
            var volGo = new GameObject("ToyRoom_PostProcessVolume");
            var volume = volGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.value = TonemappingMode.ACES;

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.24f;
            vignette.smoothness.value = 0.46f;

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.value = 1.05f;
            bloom.intensity.value = 0.28f;

            volume.sharedProfile = profile;
        }

        private static void CreateRoomArchitecture()
        {
            var root = new GameObject("Architecture");

            // Piso e tapetes em tons pasteis leves/envelhecidos para destacar Yuna na cena
            var floorMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.78f, 0.72f, 0.64f),
                "Assets/Materials/T_DarkOak_Planks.png",
                new Vector2(10f, 7f),
                emission: Color.black,
                outlineWidth: 0f);
            floorMat.SetColor("_ShadowColor", new Color(0.42f, 0.36f, 0.44f, 1f));
            EditorUtility.SetDirty(floorMat);

            var rugMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                Color.white,
                "Assets/Materials/T_Floor_Rug_Mandala.png",
                emission: Color.black,
                outlineWidth: 0f);
            rugMat.SetColor("_ShadowColor", new Color(0.42f, 0.36f, 0.44f, 1f));
            EditorUtility.SetDirty(rugMat);

            var ovalRugMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                Color.white,
                "Assets/Art/ToyRoom/Textures/Legacy/oval-rug-v1.png",
                emission: Color.black,
                outlineWidth: 0f);
            ovalRugMat.SetColor("_ShadowColor", new Color(0.42f, 0.36f, 0.44f, 1f));
            EditorUtility.SetDirty(ovalRugMat);

            var rectRugMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                Color.white,
                "Assets/Art/ToyRoom/Textures/Legacy/rectangular-rug-v1.png",
                emission: Color.black,
                outlineWidth: 0f);
            rectRugMat.SetColor("_ShadowColor", new Color(0.42f, 0.36f, 0.44f, 1f));
            EditorUtility.SetDirty(rectRugMat);

            var wallMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.98f, 0.94f, 0.88f),
                "Assets/Art/ToyRoom/Textures/Legacy/wallpaper-v1.png",
                new Vector2(6f, 1.5f),
                outlineWidth: 0f);
            var baseMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                Color.white,
                "Assets/Art/ToyRoom/Textures/Legacy/oak-trim-v1.png",
                new Vector2(10f, 1f),
                outlineWidth: 0f);
            var trackMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                Color.white,
                "Assets/Art/ToyRoom/Textures/Legacy/wooden-track-v1.png",
                outlineWidth: 0.004f);

            CreateBox("Floor_Wood", root.transform, new Vector3(0f, -0.1f, 0f), new Vector3(ToyRoomCatalog.RoomWidth, 0.2f, ToyRoomCatalog.RoomDepth), floorMat);

            var centralRugPt = ToyRoomCatalog.FromLegacyCoordinates(800f, 700f);
            var rug = ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cylinder, "Rug_Central", root.transform, new Vector3(centralRugPt.X, 0.01f, centralRugPt.Z), new Vector3(6.8f, 0.015f, 5.6f), rugMat);

            var ovalRugPt = ToyRoomCatalog.FromLegacyCoordinates(1180f, 520f);
            var rugOval = ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cylinder, "Rug_OvalPlayMat", root.transform, new Vector3(ovalRugPt.X, 0.012f, ovalRugPt.Z), new Vector3(4.6f, 0.015f, 3.2f), ovalRugMat);
            _ = rugOval;

            var rectRugPt = ToyRoomCatalog.FromLegacyCoordinates(280f, 920f);
            var rugBedside = ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "Rug_BedsideFringe", root.transform, new Vector3(rectRugPt.X, 0.012f, rectRugPt.Z), new Vector3(4.4f, 0.015f, 2.8f), rectRugMat);
            _ = rugBedside;

            // Trilho de trem decorativo no piso com textura de madeira pintada
            for (int i = 0; i < 24; i++)
            {
                float angle = i * Mathf.PI * 2f / 24f;
                Vector3 pos = new Vector3(-4.5f + Mathf.Cos(angle) * 3.4f, 0.02f, -2.5f + Mathf.Sin(angle) * 2.2f);
                var seg = ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, $"TrainTrack_{i}", root.transform, pos, new Vector3(0.55f, 0.025f, 0.18f), trackMat, new Vector3(0f, -angle * Mathf.Rad2Deg, 0f));
                _ = seg;
            }
            _ = rug;

            var wallNorth = CreateBox("Wall_North", root.transform, new Vector3(0f, 2.5f, ToyRoomCatalog.HalfDepth + 0.15f), new Vector3(ToyRoomCatalog.RoomWidth + 0.6f, 5.0f, 0.3f), wallMat);
            var wallNorthRenderer = wallNorth.GetComponent<Renderer>();
            if (wallNorthRenderer != null)
            {
                wallNorthRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            CreateBox("Baseboard_North", root.transform, new Vector3(0f, 0.28f, ToyRoomCatalog.HalfDepth - 0.02f), new Vector3(ToyRoomCatalog.RoomWidth, 0.55f, 0.08f), baseMat);

            CreateBox("Wall_West", root.transform, new Vector3(-ToyRoomCatalog.HalfWidth - 0.15f, 2.5f, 0f), new Vector3(0.3f, 5.0f, ToyRoomCatalog.RoomDepth), wallMat);
            CreateBox("Wall_East", root.transform, new Vector3(ToyRoomCatalog.HalfWidth + 0.15f, 2.5f, 0f), new Vector3(0.3f, 5.0f, ToyRoomCatalog.RoomDepth), wallMat);
            CreateBox("Wall_South_Bounds", root.transform, new Vector3(0f, 1.0f, -ToyRoomCatalog.HalfDepth - 0.15f), new Vector3(ToyRoomCatalog.RoomWidth + 0.6f, 2.0f, 0.3f), wallMat).GetComponent<Renderer>().enabled = false;

            // Quadros infantis ilustrados a mao nas paredes (Environment Artist Agent)
            string[] wallDrawings =
            {
                "Assets/Materials/T_WallDrawing_Castle.png",
                "Assets/Materials/T_WallDrawing_Cat.png",
                "Assets/Materials/T_WallDrawing_Rainbow.png",
                "Assets/Materials/T_WallDrawing_Sun.png"
            };
            float[] drawingXs = { -9.8f, -0.6f, 5.4f, 11.0f };
            for (int i = 0; i < wallDrawings.Length; i++)
            {
                var artMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, wallDrawings[i], outlineWidth: 0.004f);
                ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Cube,
                    $"WallDrawing_{i}",
                    root.transform,
                    new Vector3(drawingXs[i], 2.65f, ToyRoomCatalog.HalfDepth - 0.04f),
                    new Vector3(1.45f, 1.15f, 0.04f),
                    artMat,
                    new Vector3(0f, 0f, 180f));
            }
        }

        private static void CreateWindowsAndLightShafts()
        {
            var root = new GameObject("Windows_And_Shafts");
            var frameMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.60f, 0.40f, 0.24f));
            var skyMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.75f, 0.92f, 1.0f), emission: new Color(0.55f, 0.78f, 0.96f), outlineWidth: 0f);
            var shaftMat = CreateLightShaftMaterial();

            float[] windowXs =
            {
                ToyRoomCatalog.FromLegacyCoordinates(520f, 30f).X,
                ToyRoomCatalog.FromLegacyCoordinates(1280f, 30f).X
            };
            var curtainShader = Shader.Find("Sprites/Default");
            var curtainMat = new Material(curtainShader);
            var curtainTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/ToyRoom/Textures/Legacy/window-curtains-v1.png");
            if (curtainTex != null)
            {
                curtainMat.mainTexture = curtainTex;
            }
            curtainMat.color = Color.white;
            foreach (float wx in windowXs)
            {
                var win = new GameObject($"Window_{wx:0.0}");
                win.transform.SetParent(root.transform, false);
                win.transform.position = new Vector3(wx, 2.8f, ToyRoomCatalog.HalfDepth - 0.03f);

                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "Frame", win.transform, Vector3.zero, new Vector3(2.2f, 2.4f, 0.12f), frameMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "Glass", win.transform, new Vector3(0f, 0f, -0.05f), new Vector3(1.85f, 2.05f, 0.06f), skyMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "Cross_V", win.transform, new Vector3(0f, 0f, -0.07f), new Vector3(0.08f, 2.05f, 0.06f), frameMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "Cross_H", win.transform, new Vector3(0f, 0f, -0.07f), new Vector3(1.85f, 0.08f, 0.06f), frameMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Quad, "Curtains", win.transform, new Vector3(0f, 0f, -0.11f), new Vector3(2.8f, 2.9f, 1f), curtainMat, Vector3.zero);

                var shaft = ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Quad,
                    "LightShaft",
                    win.transform,
                    new Vector3(0.85f, -1.55f, -2.1f),
                    new Vector3(2.4f, 5.2f, 1f),
                    shaftMat,
                    new Vector3(38f, -18f, -16f));
                _ = shaft;

                // Fonte de luz principal da janela posicionada fora do vidro para entrar difusa e uniforme (sem hotspot)
                var spotGo = new GameObject("WindowSun_SpotLight");
                spotGo.transform.SetParent(win.transform, false);
                spotGo.transform.localPosition = new Vector3(0f, 2.2f, 6.5f);
                spotGo.transform.rotation = Quaternion.Euler(26f, wx < 0f ? 170f : 190f, 0f);
                var spot = spotGo.AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.color = new Color(1.0f, 0.96f, 0.88f);
                spot.intensity = 195f;
                spot.range = 48f;
                spot.spotAngle = 148f;
                spot.innerSpotAngle = 110f;
                spot.shadows = LightShadows.Soft;
                spot.shadowStrength = 0.72f;
                spot.shadowResolution = LightShadowResolution.VeryHigh;
                spot.shadowBias = 0.02f;
                spot.shadowNormalBias = 0.25f;

                // Rebote indireto difuso do ceu passando pela cortina/vidro (sem sombra dupla)
                var bounceGo = new GameObject("WindowBounce_PointLight");
                bounceGo.transform.SetParent(win.transform, false);
                bounceGo.transform.localPosition = new Vector3(0f, 0.3f, -4.5f);
                var bounce = bounceGo.AddComponent<Light>();
                bounce.type = LightType.Point;
                bounce.color = new Color(0.92f, 0.95f, 1.0f);
                bounce.intensity = 14f;
                bounce.range = 28f;
                bounce.shadows = LightShadows.None;
            }
        }

        private static Material CreateLightShaftMaterial()
        {
            const string matPath = "Assets/Art/ToyRoom/Materials/M_LightShaft.mat";
            var shader = Shader.Find("Sprites/Default");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            mat.color = new Color(1.0f, 0.92f, 0.64f, 0.14f);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/T_LightShaft_Gradient.png");
            if (tex != null)
            {
                mat.mainTexture = tex;
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void EnsurePainterlyRendererFeature()
        {
            const string rendererPath = "Assets/Settings/DarkRoomRenderer.asset";
            const string postMatPath = "Assets/Art/ToyRoom/Materials/M_ToyRoomPainterlyPostProcess.mat";

            Directory.CreateDirectory("Assets/Art/ToyRoom/Materials");
            var postShader = Shader.Find("YunaLegend/ToyRoom_PainterlyPostProcess");
            if (postShader == null)
            {
                return;
            }

            var postMat = AssetDatabase.LoadAssetAtPath<Material>(postMatPath);
            if (postMat == null)
            {
                postMat = new Material(postShader);
                AssetDatabase.CreateAsset(postMat, postMatPath);
            }
            else
            {
                postMat.shader = postShader;
            }

            postMat.SetFloat("_KuwaharaBlend", 0.18f);
            postMat.SetFloat("_KuwaharaRadius", 0.85f);
            postMat.SetFloat("_InkStrength", 0.28f);
            postMat.SetFloat("_DepthThreshold", 0.42f);
            postMat.SetFloat("_NormalThreshold", 0.58f);
            postMat.SetFloat("_PaperGrainStrength", 0.024f);
            EditorUtility.SetDirty(postMat);

            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (rendererData == null)
            {
                return;
            }

            EnsureSsaoRendererFeature(rendererData);

            foreach (var existing in rendererData.rendererFeatures)
            {
                if (existing is ToyRoomPainterlyRendererFeature painterlyFeature)
                {
                    painterlyFeature.PassMaterial = postMat;
                    EditorUtility.SetDirty(painterlyFeature);
                    EditorUtility.SetDirty(rendererData);
                    return;
                }
            }

            var feature = ScriptableObject.CreateInstance<ToyRoomPainterlyRendererFeature>();
            feature.name = "ToyRoomPainterlyRendererFeature";
            feature.PassMaterial = postMat;

            AssetDatabase.AddObjectToAsset(feature, rendererData);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId);

            var so = new SerializedObject(rendererData);
            var featuresProp = so.FindProperty("m_RendererFeatures");
            var mapProp = so.FindProperty("m_RendererFeatureMap");

            int idx = featuresProp.arraySize;
            featuresProp.InsertArrayElementAtIndex(idx);
            featuresProp.GetArrayElementAtIndex(idx).objectReferenceValue = feature;

            mapProp.InsertArrayElementAtIndex(mapProp.arraySize);
            mapProp.GetArrayElementAtIndex(mapProp.arraySize - 1).longValue = localId;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(feature);
            EditorUtility.SetDirty(rendererData);
        }

        private static void EnsureSsaoRendererFeature(UniversalRendererData rendererData)
        {
            var ssaoType = typeof(UniversalRendererData).Assembly.GetType("UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusion");
            if (ssaoType == null)
            {
                return;
            }

            foreach (var existing in rendererData.rendererFeatures)
            {
                if (existing != null && existing.GetType() == ssaoType)
                {
                    return;
                }
            }

            var ssao = ScriptableObject.CreateInstance(ssaoType) as ScriptableRendererFeature;
            if (ssao == null)
            {
                return;
            }

            ssao.name = "ScreenSpaceAmbientOcclusion";
            AssetDatabase.AddObjectToAsset(ssao, rendererData);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ssao, out _, out long localId);

            var so = new SerializedObject(rendererData);
            var featuresProp = so.FindProperty("m_RendererFeatures");
            var mapProp = so.FindProperty("m_RendererFeatureMap");
            int idx = featuresProp.arraySize;
            featuresProp.InsertArrayElementAtIndex(idx);
            featuresProp.GetArrayElementAtIndex(idx).objectReferenceValue = ssao;
            mapProp.InsertArrayElementAtIndex(mapProp.arraySize);
            mapProp.GetArrayElementAtIndex(mapProp.arraySize - 1).longValue = localId;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(ssao);
            EditorUtility.SetDirty(rendererData);
        }

        private static Material CreateSparkleParticleMaterial()
        {
            const string matPath = "Assets/Art/ToyRoom/Materials/M_SparkleParticle.mat";
            var shader = Shader.Find("Sprites/Default");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            mat.color = Color.white;
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/T_Sparkle_Glow.png");
            if (tex != null)
            {
                mat.mainTexture = tex;
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void CreateFloatingDustMotes()
        {
            var dustGo = new GameObject("MorningDust_Particles");
            dustGo.transform.position = new Vector3(0f, 1.8f, 0f);
            var ps = dustGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 6f;
            main.startSpeed = 0.12f;
            main.startSize = 0.09f;
            main.startColor = new Color(1.0f, 0.94f, 0.72f, 0.55f);
            main.maxParticles = 60;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(22f, 3.5f, 16f);

            var psr = dustGo.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.sharedMaterial = CreateSparkleParticleMaterial();
            }
        }

        private static ToyChestController CreateFurnitureModels()
        {
            var root = new GameObject("Furniture_Models");
            ToyChestController chestController = null;

            foreach (var item in ToyRoomCatalog.Furniture)
            {
                if (item.IsToyChest)
                {
                    chestController = CreateToyChest(root.transform, item);
                    continue;
                }

                var host = new GameObject($"Furniture_{item.Id}");
                host.transform.SetParent(root.transform, false);
                host.transform.position = new Vector3(item.Center.X, 0f, item.Center.Z);

                var col = host.AddComponent<BoxCollider>();
                col.center = new Vector3(0f, 0.75f, 0f);
                col.size = new Vector3(item.Width, 1.50f, item.Depth);

                if (!ToyRoomPropSculptor.TryAttachGeneratedPropModel(host.transform, item.Id, 1.50f, item.Width, item.Depth))
                {
                    ToyRoomVisualFactory.BuildFurnitureDetail(host.transform, item);
                }
            }

            return chestController;
        }

        private static ToyChestController CreateToyChest(Transform parent, FurnitureSpec item)
        {
            var chestGo = new GameObject($"Furniture_{item.Id}");
            chestGo.transform.SetParent(parent, false);
            chestGo.transform.position = new Vector3(item.Center.X, 0f, item.Center.Z);

            var col = chestGo.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.45f, 0f);
            col.size = new Vector3(item.Width, 0.90f, item.Depth);

            var glowGo = new GameObject("InteriorGlow");
            glowGo.transform.SetParent(chestGo.transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            var glow = glowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(1.0f, 0.85f, 0.35f);
            glow.range = 4.5f;
            glow.intensity = 0f;

            var controller = chestGo.AddComponent<ToyChestController>();
            bool hasClosed = ToyRoomPropSculptor.TryAttachGeneratedPropModel(chestGo.transform, "toy-chest", 1.35f, item.Width, item.Depth);
            bool hasOpen = ToyRoomPropSculptor.TryAttachGeneratedPropModel(chestGo.transform, "toy-chest-open", 1.45f, item.Width, item.Depth);
            if (hasClosed && hasOpen)
            {
                var closedGo = chestGo.transform.Find("Model3D_toy-chest")?.gameObject;
                var openGo = chestGo.transform.Find("Model3D_toy-chest-open")?.gameObject;
                controller.ConfigureStates(closedGo, openGo, glow);
            }
            else
            {
                ToyRoomPropSculptor.BuildSculptedToyChest(chestGo.transform, item, out var lidPivot);
                controller.Configure(lidPivot, glow);
            }

            return controller;
        }

        private static List<ToyItemView> CreateToyModels()
        {
            var root = new GameObject("Toys_Models");
            var list = new List<ToyItemView>();

            foreach (var toy in ToyRoomCatalog.Toys)
            {
                var go = new GameObject($"Toy_{toy.Id}");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(toy.InitialPosition.X, 0.02f, toy.InitialPosition.Z);

                var col = go.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.center = new Vector3(0f, 0.25f, 0f);
                col.radius = 0.42f;

                if (!ToyRoomPropSculptor.TryAttachGeneratedPropModel(go.transform, toy.Id, 0.72f, 0.85f, 0.85f))
                {
                    ToyRoomVisualFactory.BuildToyModel3D(go.transform, toy);
                }

                var view = go.AddComponent<ToyItemView>();
                view.Configure(toy.Id, toy.WeightMultiplier);
                list.Add(view);
            }

            return list;
        }

        private static ToyRoomPlayerController CreatePlayer()
        {
            var playerGo = new GameObject("Yuna_ToyRoomPlayer");
            var startPt = ToyRoomCatalog.FromLegacyCoordinates(280f, 640f);
            playerGo.transform.position = new Vector3(startPt.X, 0f, startPt.Z);
            playerGo.transform.rotation = Quaternion.Euler(0f, 165f, 0f);

            var cc = playerGo.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 0.65f, 0f);
            cc.height = 1.3f;
            cc.radius = 0.32f;

            ToyRoomVisualFactory.BuildYunaCharacter3D(
                playerGo.transform,
                out var bodyRoot,
                out var legL,
                out var legR,
                out var armL,
                out var armR,
                out var ponytail);

            var controller = playerGo.AddComponent<ToyRoomPlayerController>();
            var animator = playerGo.AddComponent<YunaToyRoomAnimator>();
            animator.Configure(controller, bodyRoot, legL, legR, armL, armR, ponytail);

            var rigRoot = bodyRoot != null ? bodyRoot.Find("Yuna_RigRoot") : null;
            if (rigRoot != null)
            {
                var hips = rigRoot.Find("Hips");
                var spine = hips != null ? hips.Find("Spine") : null;
                var chest = spine != null ? spine.Find("Chest") : null;
                var neck = chest != null ? chest.Find("Neck") : null;
                var head = neck != null ? neck.Find("Head") : null;
                var upperArmL = chest != null ? chest.Find("UpperArm_L") : null;
                var forearmL = upperArmL != null ? upperArmL.Find("Forearm_L") : null;
                var upperArmR = chest != null ? chest.Find("UpperArm_R") : null;
                var forearmR = upperArmR != null ? upperArmR.Find("Forearm_R") : null;
                var thighL = hips != null ? hips.Find("Thigh_L") : null;
                var shinL = thighL != null ? thighL.Find("Shin_L") : null;
                var thighR = hips != null ? hips.Find("Thigh_R") : null;
                var shinR = thighR != null ? thighR.Find("Shin_R") : null;

                animator.ConfigureRig(new YunaToyRoomAnimator.RigBones
                {
                    Hips = hips,
                    Spine = spine,
                    Chest = chest,
                    Neck = neck,
                    Head = head,
                    UpperArmL = upperArmL,
                    ForearmL = forearmL,
                    HandL = forearmL != null ? forearmL.Find("Hand_L") : null,
                    UpperArmR = upperArmR,
                    ForearmR = forearmR,
                    HandR = forearmR != null ? forearmR.Find("Hand_R") : null,
                    ThighL = thighL,
                    ShinL = shinL,
                    FootL = shinL != null ? shinL.Find("Foot_L") : null,
                    ThighR = thighR,
                    ShinR = shinR,
                    FootR = shinR != null ? shinR.Find("Foot_R") : null
                });
            }

            return controller;
        }

        private static ToySwordView CreateToySword(Vector3 chestPosition)
        {
            var swordRoot = new GameObject("ToySword_Anchor");
            swordRoot.transform.position = chestPosition + new Vector3(2.3f, 0.35f, -0.4f);

            ToyRoomPropSculptor.BuildSculptedToySword(swordRoot.transform);

            var auraGo = new GameObject("SwordAura");
            auraGo.transform.SetParent(swordRoot.transform, false);
            var aura = auraGo.AddComponent<Light>();
            aura.type = LightType.Point;
            aura.color = new Color(1.0f, 0.90f, 0.55f);
            aura.range = 3.5f;
            aura.intensity = 2.4f;

            var swordView = swordRoot.AddComponent<ToySwordView>();
            swordView.Configure(aura);
            return swordView;
        }

        private static NandaToyRoomGuideController CreateNandaGuide(Transform player, Transform chest, Transform sword)
        {
            var nandaGo = new GameObject("Nanda_BlueGuide");
            nandaGo.transform.position = player.position + new Vector3(-1.25f, 1.75f, 0.35f);
            ToyRoomVisualFactory.BuildNandaFairy3D(nandaGo.transform);

            var fairyBody = nandaGo.transform.Find("Nanda_FairyBody");
            if (fairyBody != null)
            {
                fairyBody.localScale = Vector3.one * 0.45f;
            }

            var lightGo = new GameObject("Nanda_AuraLight");
            lightGo.transform.SetParent(nandaGo.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.36f, 0.86f, 1.0f);
            light.range = 5.5f;
            light.intensity = 2.4f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.65f;
            light.shadowResolution = LightShadowResolution.High;
            light.shadowBias = 0.02f;
            light.shadowNormalBias = 0.20f;

            var guide = nandaGo.AddComponent<NandaToyRoomGuideController>();
            guide.Configure(player, chest, sword, light);

            var trailPs = nandaGo.AddComponent<ParticleSystem>();
            var main = trailPs.main;
            main.startLifetime = 0.75f;
            main.startSpeed = 0.06f;
            main.startSize = 0.035f;
            main.startColor = new Color(0.45f, 0.92f, 1.0f, 0.65f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 16;

            var emission = trailPs.emission;
            emission.rateOverTime = 14f;

            var psr = nandaGo.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.sharedMaterial = CreateSparkleParticleMaterial();
            }

            return guide;
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && mat != null)
            {
                renderer.sharedMaterial = mat;
            }

            return go;
        }
    }
}
