using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.Editor.ToyRoom
{
    /// <summary>
    /// Constroi a cena base 3D isometrica da Fase 2 (ToyRoom_Gameplay.unity) com camera fixa,
    /// iluminacao matinal URP e blockout espacial dos 7 moveis e 8 brinquedos.
    /// </summary>
    public static class ToyRoomSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/ToyRoom_Gameplay.unity";

        [MenuItem("YunaLegend/Fase 2 - Toy Room/Build Base Scene (P1)")]
        public static void BuildBaseScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConfigureLighting();
            CreateIsometricCamera();
            CreatePostProcessVolume();
            CreateRoomArchitecture();
            CreateFurnitureBlockout();
            CreateToyPlaceholders();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[ToyRoomSceneBuilder] Cena base salva em {ScenePath}");
        }

        private static void ConfigureLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.95f, 0.88f, 0.76f);
            RenderSettings.ambientEquatorColor = new Color(0.66f, 0.56f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.27f, 0.24f, 0.42f); // Sombra tintada em indigo (Legend of Mana)

            var sunGo = new GameObject("MorningSun_DirectionalLight");
            var light = sunGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.90f, 0.72f);
            light.intensity = 1.45f;
            light.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        }

        private static void CreateIsometricCamera()
        {
            var camGo = new GameObject("Isometric_MainCamera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.11f, 0.10f, 0.18f);
            cam.fieldOfView = 26f;
            cam.nearClipPlane = 0.5f;
            cam.farClipPlane = 60f;

            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>();

            // Enquadramento isometrico fixo estilo diorama (Diablo IV x Legend of Mana)
            camGo.transform.position = new Vector3(0.0f, 15.5f, -15.5f);
            camGo.transform.LookAt(new Vector3(0.0f, 0.6f, 0.2f));
        }

        private static void CreatePostProcessVolume()
        {
            var volGo = new GameObject("ToyRoom_PostProcessVolume");
            var volume = volGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.28f;
            vignette.smoothness.value = 0.45f;

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.value = 0.95f;
            bloom.intensity.value = 0.35f;

            volume.sharedProfile = profile;
        }

        private static void CreateRoomArchitecture()
        {
            var root = new GameObject("Architecture");

            var floor = CreateBox("Floor_Wood", root.transform, Vector3.zero, new Vector3(ToyRoomCatalog.RoomWidth, 0.2f, ToyRoomCatalog.RoomDepth), new Color(0.68f, 0.48f, 0.28f));
            floor.transform.position = new Vector3(0f, -0.1f, 0f);

            var rug = CreateBox("Rug_Central", root.transform, new Vector3(0f, 0.015f, -0.4f), new Vector3(8.5f, 0.03f, 5.8f), new Color(0.84f, 0.68f, 0.68f));
            Object.DestroyImmediate(rug.GetComponent<Collider>());

            CreateBox("Wall_North", root.transform, new Vector3(0f, 2.5f, ToyRoomCatalog.HalfDepth + 0.15f), new Vector3(ToyRoomCatalog.RoomWidth + 0.6f, 5.0f, 0.3f), new Color(0.95f, 0.89f, 0.78f));
            CreateBox("Wall_West", root.transform, new Vector3(-ToyRoomCatalog.HalfWidth - 0.15f, 2.5f, 0f), new Vector3(0.3f, 5.0f, ToyRoomCatalog.RoomDepth), new Color(0.92f, 0.85f, 0.74f));
            CreateBox("Wall_East", root.transform, new Vector3(ToyRoomCatalog.HalfWidth + 0.15f, 2.5f, 0f), new Vector3(0.3f, 5.0f, ToyRoomCatalog.RoomDepth), new Color(0.92f, 0.85f, 0.74f));
        }

        private static void CreateFurnitureBlockout()
        {
            var root = new GameObject("Furniture_Blockout");

            foreach (var item in ToyRoomCatalog.Furniture)
            {
                float height = item.IsToyChest ? 1.15f : 1.50f;
                Color tint = item.IsToyChest ? new Color(0.75f, 0.20f, 0.20f) : new Color(0.55f, 0.38f, 0.24f);
                CreateBox(
                    $"Furniture_{item.Id}",
                    root.transform,
                    new Vector3(item.Center.X, height * 0.5f, item.Center.Z),
                    new Vector3(item.Width, height, item.Depth),
                    tint);
            }
        }

        private static void CreateToyPlaceholders()
        {
            var root = new GameObject("Toys_Placeholders");

            foreach (var toy in ToyRoomCatalog.Toys)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = $"Toy_{toy.Id}";
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(toy.InitialPosition.X, 0.25f, toy.InitialPosition.Z);
                go.transform.localScale = Vector3.one * 0.45f;

                var renderer = go.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = new Color(0.95f, 0.76f, 0.28f);
                    renderer.sharedMaterial = mat;
                }
            }
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = color;
                renderer.sharedMaterial = mat;
            }

            return go;
        }
    }
}
