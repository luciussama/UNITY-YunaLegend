using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using YunaLegend.Editor.ToyRoom;

namespace YunaLegend.Editor.Arena
{
    /// <summary>
    /// Constroi os modelos 3D volumetricos da Arena (Dinossauro, Urso Guardiao e Cavalinho de Pau)
    /// a partir das malhas em relevo 3D inflado (dinosaur_3d.obj, bear_3d.obj, rocking_horse_3d.obj)
    /// com as texturas ilustradas originais em alta definicao no shader ToyRoom_Painterly3D.
    /// </summary>
    public static class Arena3DModelSculptor
    {
        private const string DinoObjPath = "Assets/Art/Arena/Models/dinosaur_hunyuan.obj";
        private const string DinoTexPath = "Assets/Art/Arena/Models/T_Dinosaur_Albedo.png";
        private const string DinoMatPath = "Assets/Art/Arena/Models/M_Arena_Dino.mat";
        private const string BearObjPath = "Assets/Art/Arena/Models/bear_hunyuan.obj";
        private const string BearTexPath = "Assets/Art/Arena/Models/T_Bear_Albedo.png";
        private const string BearMatPath = "Assets/Art/Arena/Models/M_Arena_Bear.mat";
        private const string HorseObjPath = "Assets/Art/Arena/Models/rocking_horse_hunyuan.obj";
        private const string HorseTexPath = "Assets/Art/Arena/Models/T_RockingHorse_Albedo.png";
        private const string HorseMatPath = "Assets/Art/Arena/Models/M_Arena_Horse.mat";

        public static GameObject BuildDinosaur3D(Transform parent = null)
        {
            return BuildVolumetricModel("Arena_Dinosaur_3D", DinoObjPath, DinoTexPath, DinoMatPath, parent);
        }

        public static GameObject BuildBear3D(Transform parent = null)
        {
            return BuildVolumetricModel("Arena_Bear_3D", BearObjPath, BearTexPath, BearMatPath, parent);
        }

        public static GameObject BuildRockingHorse3D(Transform parent = null)
        {
            return BuildVolumetricModel("Arena_RockingHorse_3D", HorseObjPath, HorseTexPath, HorseMatPath, parent);
        }

        private static GameObject BuildVolumetricModel(string name, string objPath, string texPath, string matPath, Transform parent)
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(objPath);
            if (mesh == null)
            {
                AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);
                mesh = AssetDatabase.LoadAssetAtPath<Mesh>(objPath);
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex == null)
            {
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            }

            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            var shader = Shader.Find("YunaLegend/ToyRoomPainterly3D") ?? Shader.Find("Universal Render Pipeline/Lit");
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (shader != null) mat.shader = shader;
            mat.SetColor("_BaseColor", Color.white);
            mat.SetTexture("_BaseMap", tex != null ? tex : Texture2D.whiteTexture);
            mat.SetColor("_EmissionColor", Color.black);
            EditorUtility.SetDirty(mat);

            var go = new GameObject(name);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;
            return go;
        }

        [MenuItem("YunaLegend/Fase 3 - Arena Imaginaria/Testar Modelos 3D Agora (Play Direto)", false, 6)]
        public static void OpenAndPlayShowcase3D()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            const string showcasePath = "Assets/Scenes/Arena_3D_Showcase.unity";
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            var keyGo = new GameObject("KeyLight");
            var key = keyGo.AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.45f;
            key.color = new Color(1f, 0.96f, 0.90f);
            keyGo.transform.rotation = Quaternion.Euler(28f, -24f, 0f);

            var fillGo = new GameObject("FillLight");
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.65f;
            fill.color = new Color(0.80f, 0.90f, 1f);
            fillGo.transform.rotation = Quaternion.Euler(18f, 155f, 0f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(1.5f, 1f, 1.0f);
            floor.GetComponent<Renderer>().sharedMaterial = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.38f, 0.24f, 0.14f), outlineWidth: 0f);

            var dino = BuildDinosaur3D();
            dino.transform.position = new Vector3(-1.45f, 0f, 0f);
            dino.transform.rotation = Quaternion.Euler(0f, 162f, 0f);

            var horse = BuildRockingHorse3D();
            horse.transform.position = new Vector3(0f, 0f, 0.15f);
            horse.transform.rotation = Quaternion.Euler(0f, 165f, 0f);

            var bear = BuildBear3D();
            bear.transform.position = new Vector3(1.45f, 0f, 0f);
            bear.transform.rotation = Quaternion.Euler(0f, 198f, 0f);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.16f, 0.13f, 0.11f);
            cam.fieldOfView = 34f;
            camGo.transform.position = new Vector3(0f, 1.15f, -4.4f);
            camGo.transform.LookAt(new Vector3(0f, 0.55f, 0f));

            Directory.CreateDirectory("Assets/Scenes");
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, showcasePath);
            UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene = null;
            EditorApplication.isPlaying = true;
        }

        [MenuItem("YunaLegend/Fase 3 - Arena Imaginaria/Gerar Amostras 3D (Dino, Urso, Cavalinho)", false, 30)]
        public static void CaptureModelSamples()
        {
            RenderSample("Dinosaur", () => BuildDinosaur3D(), new Vector3(0.55f, 0.72f, 1.65f), new Vector3(0f, 0.48f, 0f), "Logs/sample_dinosaur.png");
            RenderSample("Bear", () => BuildBear3D(), new Vector3(0.55f, 0.72f, 1.65f), new Vector3(0f, 0.48f, 0f), "Logs/sample_bear.png");
            RenderSample("RockingHorse", () => BuildRockingHorse3D(), new Vector3(0.55f, 0.72f, 1.65f), new Vector3(0f, 0.48f, 0f), "Logs/sample_horse.png");
        }

        private static void RenderSample(string label, Func<GameObject> builder, Vector3 camPos, Vector3 target, string outputPath)
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);

            var lightGo = new GameObject("KeyLight");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.45f;
            light.color = new Color(1f, 0.96f, 0.90f);
            lightGo.transform.rotation = Quaternion.Euler(28f, -24f, 0f);

            var fillGo = new GameObject("FillLight");
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.65f;
            fill.color = new Color(0.80f, 0.90f, 1f);
            fillGo.transform.rotation = Quaternion.Euler(18f, 155f, 0f);

            var model = builder();
            var camGo = new GameObject("SampleCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.14f, 0.12f);
            cam.fieldOfView = 34f;
            camGo.transform.position = camPos;
            camGo.transform.LookAt(target);

            var rt = new RenderTexture(640, 640, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(640, 640, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, 640, 640), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(rt);

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "Logs");
            File.WriteAllBytes(outputPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(model);
            Debug.Log($"SAMPLE_RENDERED {label} -> {outputPath}");
        }
    }
}
