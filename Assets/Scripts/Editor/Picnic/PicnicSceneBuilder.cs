using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YunaLegend.Picnic.Runtime;
using YunaLegend.UI;

namespace YunaLegend.Editor.Picnic
{
    public static class PicnicSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Picnic_Gameplay.unity";
        private const string ArtRoot = "Assets/Art/Picnic/Resources/Picnic";

        [MenuItem("YunaLegend/Fase 5 - Piquenique/Criar ou atualizar cena", false, 10)]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Saia do Play Mode para reconstruir a cena.");
            ConfigureArt();
            Scene previous = SceneManager.GetActiveScene();
            Scene existing = SceneManager.GetSceneByPath(ScenePath);
            if (existing.IsValid() && existing.isLoaded)
            {
                if (existing.isDirty) throw new InvalidOperationException("Picnic_Gameplay tem alterações não salvas. Salve a cena antes de reconstruir.");
                EditorSceneManager.CloseScene(existing, true);
            }
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 3.45f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.57f, 0.8f, 0.95f);
            camera.transform.position = new Vector3(6.1f, 2.139f, -10f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            var root = new GameObject("Picnic Stage");
            var platforms = root.AddComponent<PicnicPlatformController>();
            var scenery = root.AddComponent<PicnicSceneryController>();
            var creatures = root.AddComponent<PicnicCreatureController>();
            var sound = root.AddComponent<PicnicAudioController>();
            var input = root.AddComponent<PicnicInputAdapter>();
            var hud = root.AddComponent<PicnicHudController>();
            var narrative = root.AddComponent<PicnicNarrativeController>();
            var player = new GameObject("Yuna").AddComponent<PicnicCharacterController>();
            player.transform.SetParent(root.transform, false);
            var nanda = new GameObject("Nanda Azul").AddComponent<PicnicNandaController>();
            nanda.transform.SetParent(root.transform, false);
            var bootstrap = root.AddComponent<PicnicStageBootstrap>();
            bootstrap.Configure(camera, platforms, scenery, player, nanda, creatures, sound, input, hud, narrative);
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            EditorSceneManager.SaveScene(scene, ScenePath);
            PatchTitleScene();
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(s => s.path == ScenePath);
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            PersistGlobalBuildScene();
            SceneManager.SetActiveScene(scene);
            // Starting this scene alone preserves any other unsaved scene in the Editor.
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Selection.activeGameObject = root;
            Debug.Log("PICNIC_SCENE_CREATED " + ScenePath + " previous=" + previous.path);
        }

        private static void PersistGlobalBuildScene()
        {
            // Unity 6 can target a local build profile. Keep the tracked global list registered too.
            const string settings = "ProjectSettings/EditorBuildSettings.asset";
            string text = File.ReadAllText(settings);
            if (text.Contains(ScenePath)) return;
            string entry = "  - enabled: 1\n    path: " + ScenePath + "\n    guid: " + AssetDatabase.AssetPathToGUID(ScenePath) + "\n";
            if (!text.Contains("  m_configObjects:")) throw new InvalidOperationException("Unrecognised global scene settings.");
            File.WriteAllText(settings, text.Replace("  m_configObjects:", entry + "  m_configObjects:"));
        }

        [MenuItem("YunaLegend/Fase 5 - Piquenique/Jogar (Play Direto)", false, 20)]
        public static void Play()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            EditorApplication.isPlaying = true;
        }

        private static void ConfigureArt()
        {
            foreach (string path in Directory.GetFiles(ArtRoot, "*.png"))
            {
                var importer = AssetImporter.GetAtPath(path.Replace('\\', '/')) as TextureImporter;
                if (importer == null) continue;
                string name = Path.GetFileNameWithoutExtension(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 4096;
                importer.wrapMode = name == "wall" || name == "soil" ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePixelsPerUnit = 100f;
                bool surface = new[] { "grass", "wood", "stone", "cloud", "gravel" }.Contains(name);
                bool scenery = name.Contains("bg") || name.StartsWith("scenery") || name == "wall" || name == "soil";
                settings.spritePivot = new Vector2(0.5f, scenery ? 0.5f : surface ? 0.85f : name.StartsWith("nanda") ? 0.42f : name.StartsWith("yuna-run") ? 0.088f : 0f);
                if (name == "picnic") settings.spritePivot = new Vector2(0.5f, 1f - 454f / 534f);
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }

        private static void PatchTitleScene()
        {
            const string titlePath = "Assets/Scenes/TitleScreen.unity";
            var titleScene = SceneManager.GetSceneByPath(titlePath);
            bool alreadyLoaded = titleScene.IsValid() && titleScene.isLoaded;
            if (alreadyLoaded && titleScene.isDirty) throw new InvalidOperationException("TitleScreen contém alterações não salvas.");
            if (!alreadyLoaded) titleScene = EditorSceneManager.OpenScene(titlePath, OpenSceneMode.Additive);
            var all = titleScene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Component>(true)).ToArray();
            var controller = all.OfType<TitleScreenController>().Single();
            var owner = new SerializedObject(controller);
            owner.FindProperty("gameplaySceneName").stringValue = "Picnic_Gameplay";
            owner.FindProperty("selectedPhaseScene").stringValue = "Picnic_Gameplay";
            owner.ApplyModifiedPropertiesWithoutUndo();
            var selector = all.OfType<TitlePhaseSelector>().Single();
            var data = new SerializedObject(selector);
            var scenes = data.FindProperty("itemScenes");
            bool found = false;
            for (int i = 0; i < scenes.arraySize; i++) found |= scenes.GetArrayElementAtIndex(i).stringValue == "Picnic_Gameplay";
            if (!found)
            {
                var buttons = data.FindProperty("itemButtons");
                var labels = data.FindProperty("itemLabels");
                int count = scenes.arraySize;
                var last = (Button)buttons.GetArrayElementAtIndex(count - 1).objectReferenceValue;
                var copy = UnityEngine.Object.Instantiate(last.gameObject, last.transform.parent);
                copy.name = "Fase_Piquenique";
                var rect = copy.GetComponent<RectTransform>();
                float step = count > 1 ? last.GetComponent<RectTransform>().anchoredPosition.y - ((Button)buttons.GetArrayElementAtIndex(count - 2).objectReferenceValue).GetComponent<RectTransform>().anchoredPosition.y : -54f;
                rect.anchoredPosition = last.GetComponent<RectTransform>().anchoredPosition + new Vector2(0f, step);
                var label = copy.GetComponentInChildren<Text>(true);
                label.text = "O Piquenique";
                scenes.arraySize++;
                buttons.arraySize++;
                labels.arraySize++;
                scenes.GetArrayElementAtIndex(count).stringValue = "Picnic_Gameplay";
                buttons.GetArrayElementAtIndex(count).objectReferenceValue = copy.GetComponent<Button>();
                labels.GetArrayElementAtIndex(count).objectReferenceValue = label;
                var panel = last.transform.parent as RectTransform;
                if (panel != null) panel.sizeDelta += new Vector2(0f, Mathf.Abs(step));
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(titleScene);
            EditorSceneManager.SaveScene(titleScene);
            if (!alreadyLoaded) EditorSceneManager.CloseScene(titleScene, true);
        }
    }
}
