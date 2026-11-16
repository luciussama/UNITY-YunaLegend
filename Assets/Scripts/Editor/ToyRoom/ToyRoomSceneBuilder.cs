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
        private const string PreviewOutputPath = "/Users/sergio/.gemini/antigravity/brain/779e5ec9-ec3b-4b7a-9392-60ee6785d855/scratch/toyroom_preview.png";
        private const string CloseupPreviewOutputPath = "/Users/sergio/.gemini/antigravity/brain/779e5ec9-ec3b-4b7a-9392-60ee6785d855/scratch/toyroom_closeup_yuna_nanda.png";
        private const string AutoBuildVersionKey = "YunaLegend.ToyRoomSceneBuildVersion";
        private const int CurrentSceneBuildVersion = 10;

        static ToyRoomSceneBuilder()
        {
            if (!Application.isBatchMode)
            {
                EditorApplication.delayCall += TryAutoRebuildIfOutdated;
            }
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

        [MenuItem("YunaLegend/Fase 2 - Toy Room/Build Final Scene (P1-P8)")]
        public static void BuildBaseScene()
        {
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

            var narrativeGo = new GameObject("ToyRoom_Narrative");
            var narrative = narrativeGo.AddComponent<ToyRoomNarrativeController>();
            narrative.Configure(hud, swordView);

            var audioGo = new GameObject("ToyRoom_Audio");
            var audioCtrl = audioGo.AddComponent<ToyRoomAudioController>();

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

            // Close-up de auditoria visual (Art Director Agent) focado em Yuna, Nanda e brinquedos
            cam.transform.position = new Vector3(-0.15f, 1.45f, -3.15f);
            cam.transform.LookAt(new Vector3(-0.10f, 0.88f, -1.10f));
            cam.fieldOfView = 24f;
            RenderCameraToPng(cam, CloseupPreviewOutputPath, 1280, 720);

            cam.transform.position = origPos;
            cam.transform.rotation = origRot;
            cam.fieldOfView = origFov;
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
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.76f, 0.68f, 0.60f);
            RenderSettings.ambientEquatorColor = new Color(0.52f, 0.44f, 0.50f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.20f, 0.36f);

            var sunGo = new GameObject("MorningSun_DirectionalLight");
            var light = sunGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.88f, 0.68f);
            light.intensity = 1.05f;
            light.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(46f, -30f, 0f);
        }

        private static Camera CreateIsometricCamera()
        {
            var camGo = new GameObject("Isometric_MainCamera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.09f, 0.08f, 0.14f);
            cam.fieldOfView = 26f;
            cam.nearClipPlane = 0.5f;
            cam.farClipPlane = 60f;

            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>();
            camGo.AddComponent<IsometricToyRoomCamera>();

            camGo.transform.position = new Vector3(0.0f, 7.2f, -9.0f);
            camGo.transform.LookAt(new Vector3(0.0f, 0.7f, -1.0f));
            return cam;
        }

        private static void CreatePostProcessVolume()
        {
            var volGo = new GameObject("ToyRoom_PostProcessVolume");
            var volume = volGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.30f;
            vignette.smoothness.value = 0.48f;

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.value = 0.95f;
            bloom.intensity.value = 0.32f;

            volume.sharedProfile = profile;
        }

        private static void CreateRoomArchitecture()
        {
            var root = new GameObject("Architecture");

            var floorMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.86f, 0.68f, 0.48f), "Assets/Materials/T_DarkOak_Planks.png", new Vector2(10f, 7f), outlineWidth: 0f);
            var rugMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.95f, 0.88f, 0.84f), "Assets/Materials/T_Floor_Rug_Mandala.png", outlineWidth: 0f);
            var stripedRugMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.96f, 0.90f, 0.84f), "Assets/Materials/T_Floor_Rug_Striped.png", outlineWidth: 0f);
            var wallMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.94f, 0.88f, 0.78f), "Assets/Materials/T_Victorian_Wallpaper.png", new Vector2(8f, 2f), outlineWidth: 0f);
            var baseMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.66f, 0.46f, 0.30f), "Assets/Materials/T_Wall_Baseboard.png", new Vector2(10f, 1f), outlineWidth: 0f);
            var trackMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.78f, 0.56f, 0.38f), "Assets/Materials/T_TrainTrack_Wood.png", outlineWidth: 0.004f);

            CreateBox("Floor_Wood", root.transform, new Vector3(0f, -0.1f, 0f), new Vector3(ToyRoomCatalog.RoomWidth, 0.2f, ToyRoomCatalog.RoomDepth), floorMat);

            var rug = ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cylinder, "Rug_Central", root.transform, new Vector3(0f, 0.01f, -0.6f), new Vector3(11.5f, 0.015f, 8.2f), rugMat);
            var rugSide = ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "Rug_StripedPlayArea", root.transform, new Vector3(6.8f, 0.012f, -2.2f), new Vector3(4.8f, 0.015f, 3.4f), stripedRugMat, new Vector3(0f, -14f, 0f));
            _ = rugSide;

            // Trilho de trem decorativo no piso com textura de madeira pintada
            for (int i = 0; i < 24; i++)
            {
                float angle = i * Mathf.PI * 2f / 24f;
                Vector3 pos = new Vector3(-4.5f + Mathf.Cos(angle) * 3.4f, 0.02f, -2.5f + Mathf.Sin(angle) * 2.2f);
                var seg = ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, $"TrainTrack_{i}", root.transform, pos, new Vector3(0.55f, 0.025f, 0.18f), trackMat, new Vector3(0f, -angle * Mathf.Rad2Deg, 0f));
                _ = seg;
            }
            _ = rug;

            CreateBox("Wall_North", root.transform, new Vector3(0f, 2.5f, ToyRoomCatalog.HalfDepth + 0.15f), new Vector3(ToyRoomCatalog.RoomWidth + 0.6f, 5.0f, 0.3f), wallMat);
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
            float[] drawingXs = { -10.5f, -2.8f, 2.6f, 10.2f };
            for (int i = 0; i < wallDrawings.Length; i++)
            {
                var artMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, wallDrawings[i], outlineWidth: 0.004f);
                ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Cube,
                    $"WallDrawing_{i}",
                    root.transform,
                    new Vector3(drawingXs[i], 2.55f, ToyRoomCatalog.HalfDepth - 0.04f),
                    new Vector3(1.45f, 1.15f, 0.04f),
                    artMat);
            }
        }

        private static void CreateWindowsAndLightShafts()
        {
            var root = new GameObject("Windows_And_Shafts");
            var frameMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.60f, 0.40f, 0.24f));
            var skyMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.54f, 0.80f, 0.96f), emission: new Color(0.28f, 0.44f, 0.56f));
            var shaftMat = CreateLightShaftMaterial();

            float[] windowXs = { -7.2f, 6.4f };
            foreach (float wx in windowXs)
            {
                var win = new GameObject($"Window_{wx:0.0}");
                win.transform.SetParent(root.transform, false);
                win.transform.position = new Vector3(wx, 2.8f, ToyRoomCatalog.HalfDepth - 0.03f);

                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "Frame", win.transform, Vector3.zero, new Vector3(2.2f, 2.4f, 0.12f), frameMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "Glass", win.transform, new Vector3(0f, 0f, -0.04f), new Vector3(1.85f, 2.05f, 0.06f), skyMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "Cross_V", win.transform, new Vector3(0f, 0f, -0.06f), new Vector3(0.08f, 2.05f, 0.06f), frameMat);
                ToyRoomVisualFactory.CreateVisualPrimitive(PrimitiveType.Cube, "Cross_H", win.transform, new Vector3(0f, 0f, -0.06f), new Vector3(1.85f, 0.08f, 0.06f), frameMat);

                var shaft = ToyRoomVisualFactory.CreateVisualPrimitive(
                    PrimitiveType.Quad,
                    "LightShaft",
                    win.transform,
                    new Vector3(0.85f, -1.55f, -2.1f),
                    new Vector3(2.4f, 5.2f, 1f),
                    shaftMat,
                    new Vector3(38f, -18f, -16f));
                _ = shaft;
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

                ToyRoomVisualFactory.BuildFurnitureDetail(host.transform, item);
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

            ToyRoomPropSculptor.BuildSculptedToyChest(chestGo.transform, item, out var lidPivot);

            var glowGo = new GameObject("InteriorGlow");
            glowGo.transform.SetParent(chestGo.transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            var glow = glowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(1.0f, 0.85f, 0.35f);
            glow.range = 4.5f;
            glow.intensity = 0f;

            var controller = chestGo.AddComponent<ToyChestController>();
            controller.Configure(lidPivot, glow);
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

                ToyRoomVisualFactory.BuildToyModel3D(go.transform, toy);

                var view = go.AddComponent<ToyItemView>();
                view.Configure(toy.Id, toy.WeightMultiplier);
                list.Add(view);
            }

            return list;
        }

        private static ToyRoomPlayerController CreatePlayer()
        {
            var playerGo = new GameObject("Yuna_ToyRoomPlayer");
            playerGo.transform.position = new Vector3(0f, 0f, -1.2f);
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
            nandaGo.transform.position = player.position + new Vector3(-0.46f, 1.06f, 0.12f);
            ToyRoomVisualFactory.BuildNandaFairy3D(nandaGo.transform);

            var lightGo = new GameObject("Nanda_AuraLight");
            lightGo.transform.SetParent(nandaGo.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.55f, 0.45f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.36f, 0.88f, 1.0f);
            light.range = 4.2f;
            light.intensity = 0.85f;

            var guide = nandaGo.AddComponent<NandaToyRoomGuideController>();
            guide.Configure(player, chest, sword, light);

            var trailPs = nandaGo.AddComponent<ParticleSystem>();
            var main = trailPs.main;
            main.startLifetime = 0.85f;
            main.startSpeed = 0.08f;
            main.startSize = 0.07f;
            main.startColor = new Color(0.45f, 0.92f, 1.0f, 0.75f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 24;

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
