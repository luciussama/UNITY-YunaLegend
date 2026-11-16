using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using YunaLegend.ToyRoom.Core;
using YunaLegend.ToyRoom.Runtime;

namespace YunaLegend.Editor.ToyRoom
{
    /// <summary>
    /// Constroi a cena base 3D isometrica da Fase 2 (ToyRoom_Gameplay.unity) com camera fixa,
    /// iluminacao matinal URP, HUD, narrativa, espada e blockout dos 7 moveis e 8 brinquedos.
    /// </summary>
    public static class ToyRoomSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/ToyRoom_Gameplay.unity";

        [MenuItem("YunaLegend/Fase 2 - Toy Room/Build Base Scene (P1-P6)")]
        public static void BuildBaseScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConfigureLighting();
            var cam = CreateIsometricCamera();
            CreatePostProcessVolume();
            CreateRoomArchitecture();
            var chest = CreateFurnitureBlockout();
            var toys = CreateToyPlaceholders();
            var player = CreatePlayer();
            var swordView = CreateToySword(chest.transform.position);
            var nanda = CreateNandaGuide(player.transform, chest.transform, swordView.transform);

            var hudGo = new GameObject("ToyRoom_HUD");
            var hud = hudGo.AddComponent<ToyRoomHudController>();

            var narrativeGo = new GameObject("ToyRoom_Narrative");
            var narrative = narrativeGo.AddComponent<ToyRoomNarrativeController>();
            narrative.Configure(hud, swordView);

            var bootstrapGo = new GameObject("ToyRoom_Bootstrap");
            var bootstrap = bootstrapGo.AddComponent<ToyRoomStageBootstrap>();
            bootstrap.Configure(cam, player, chest, nanda, swordView, hud, narrative, toys, autoExplore: false);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[ToyRoomSceneBuilder] Cena base salva em {ScenePath}");
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

            string outPath = "/Users/sergio/.gemini/antigravity/brain/779e5ec9-ec3b-4b7a-9392-60ee6785d855/scratch/toyroom_preview.png";
            Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? "/tmp");
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Debug.Log($"[ToyRoomSceneBuilder] Preview salvo em {outPath}");
        }

        private static void ConfigureLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.95f, 0.88f, 0.76f);
            RenderSettings.ambientEquatorColor = new Color(0.66f, 0.56f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.27f, 0.24f, 0.42f);

            var sunGo = new GameObject("MorningSun_DirectionalLight");
            var light = sunGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.90f, 0.72f);
            light.intensity = 1.45f;
            light.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        }

        private static Camera CreateIsometricCamera()
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

            camGo.transform.position = new Vector3(0.0f, 15.5f, -15.5f);
            camGo.transform.LookAt(new Vector3(0.0f, 0.6f, 0.2f));
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

            CreateBox("Floor_Wood", root.transform, new Vector3(0f, -0.1f, 0f), new Vector3(ToyRoomCatalog.RoomWidth, 0.2f, ToyRoomCatalog.RoomDepth), new Color(0.68f, 0.48f, 0.28f));
            var rug = CreateBox("Rug_Central", root.transform, new Vector3(0f, 0.015f, -0.4f), new Vector3(8.5f, 0.03f, 5.8f), new Color(0.84f, 0.68f, 0.68f));
            Object.DestroyImmediate(rug.GetComponent<Collider>());

            CreateBox("Wall_North", root.transform, new Vector3(0f, 2.5f, ToyRoomCatalog.HalfDepth + 0.15f), new Vector3(ToyRoomCatalog.RoomWidth + 0.6f, 5.0f, 0.3f), new Color(0.95f, 0.89f, 0.78f));
            CreateBox("Wall_West", root.transform, new Vector3(-ToyRoomCatalog.HalfWidth - 0.15f, 2.5f, 0f), new Vector3(0.3f, 5.0f, ToyRoomCatalog.RoomDepth), new Color(0.92f, 0.85f, 0.74f));
            CreateBox("Wall_East", root.transform, new Vector3(ToyRoomCatalog.HalfWidth + 0.15f, 2.5f, 0f), new Vector3(0.3f, 5.0f, ToyRoomCatalog.RoomDepth), new Color(0.92f, 0.85f, 0.74f));
            CreateBox("Wall_South_Bounds", root.transform, new Vector3(0f, 1.0f, -ToyRoomCatalog.HalfDepth - 0.15f), new Vector3(ToyRoomCatalog.RoomWidth + 0.6f, 2.0f, 0.3f), new Color(0.92f, 0.85f, 0.74f)).GetComponent<Renderer>().enabled = false;
        }

        private static ToyChestController CreateFurnitureBlockout()
        {
            var root = new GameObject("Furniture_Blockout");
            ToyChestController chestController = null;

            foreach (var item in ToyRoomCatalog.Furniture)
            {
                if (item.IsToyChest)
                {
                    chestController = CreateToyChest(root.transform, item);
                    continue;
                }

                CreateBox(
                    $"Furniture_{item.Id}",
                    root.transform,
                    new Vector3(item.Center.X, 0.75f, item.Center.Z),
                    new Vector3(item.Width, 1.50f, item.Depth),
                    new Color(0.55f, 0.38f, 0.24f));
            }

            return chestController;
        }

        private static ToyChestController CreateToyChest(Transform parent, FurnitureSpec item)
        {
            var chestGo = CreateBox(
                $"Furniture_{item.Id}",
                parent,
                new Vector3(item.Center.X, 0.45f, item.Center.Z),
                new Vector3(item.Width, 0.90f, item.Depth),
                new Color(0.75f, 0.20f, 0.20f));

            var lidPivot = new GameObject("LidPivot");
            lidPivot.transform.SetParent(chestGo.transform, false);
            lidPivot.transform.localPosition = new Vector3(0f, 0.50f, 0.45f);

            var lid = CreateBox("ChestLid", lidPivot.transform, Vector3.zero, new Vector3(1.02f, 0.25f, 1.02f), new Color(0.85f, 0.25f, 0.22f));
            lid.transform.localPosition = new Vector3(0f, 0.10f, -0.45f);
            Object.DestroyImmediate(lid.GetComponent<Collider>());

            var glowGo = new GameObject("InteriorGlow");
            glowGo.transform.SetParent(chestGo.transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            var glow = glowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(1.0f, 0.85f, 0.35f);
            glow.range = 4.5f;
            glow.intensity = 0f;

            var controller = chestGo.AddComponent<ToyChestController>();
            controller.Configure(lidPivot.transform, glow);
            return controller;
        }

        private static List<ToyItemView> CreateToyPlaceholders()
        {
            var root = new GameObject("Toys_Placeholders");
            var list = new List<ToyItemView>();

            foreach (var toy in ToyRoomCatalog.Toys)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = $"Toy_{toy.Id}";
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(toy.InitialPosition.X, 0.25f, toy.InitialPosition.Z);
                go.transform.localScale = Vector3.one * 0.45f;

                var col = go.GetComponent<SphereCollider>();
                if (col != null)
                {
                    col.isTrigger = true;
                }

                var renderer = go.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = new Color(0.95f, 0.76f, 0.28f);
                    renderer.sharedMaterial = mat;
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
            playerGo.transform.position = new Vector3(0f, 0f, -1.2f);

            var cc = playerGo.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 0.65f, 0f);
            cc.height = 1.3f;
            cc.radius = 0.32f;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Yuna_StandIn";
            visual.transform.SetParent(playerGo.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            visual.transform.localScale = new Vector3(0.55f, 0.65f, 0.55f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            var renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.96f, 0.88f, 0.86f);
                renderer.sharedMaterial = mat;
            }

            return playerGo.AddComponent<ToyRoomPlayerController>();
        }

        private static ToySwordView CreateToySword(Vector3 chestPosition)
        {
            var swordRoot = new GameObject("ToySword_Anchor");
            swordRoot.transform.position = chestPosition + new Vector3(2.3f, -chestPosition.y + 0.35f, -0.4f);

            var blade = CreateBox("Blade", swordRoot.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.10f, 0.65f, 0.04f), new Color(0.91f, 0.79f, 0.56f));
            Object.DestroyImmediate(blade.GetComponent<Collider>());

            var guard = CreateBox("Guard", swordRoot.transform, new Vector3(0f, 0.02f, 0f), new Vector3(0.32f, 0.07f, 0.08f), new Color(0.51f, 0.77f, 0.78f));
            Object.DestroyImmediate(guard.GetComponent<Collider>());

            var hilt = CreateBox("Hilt", swordRoot.transform, new Vector3(0f, -0.13f, 0f), new Vector3(0.08f, 0.22f, 0.06f), new Color(0.78f, 0.47f, 0.41f));
            Object.DestroyImmediate(hilt.GetComponent<Collider>());

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
            var nandaGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nandaGo.name = "Nanda_BlueGuide";
            nandaGo.transform.position = player.position + new Vector3(-0.65f, 1.55f, 0.45f);
            nandaGo.transform.localScale = Vector3.one * 0.26f;
            Object.DestroyImmediate(nandaGo.GetComponent<Collider>());

            var renderer = nandaGo.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.36f, 0.88f, 1.0f);
                renderer.sharedMaterial = mat;
            }

            var light = nandaGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.36f, 0.88f, 1.0f);
            light.range = 4.0f;
            light.intensity = 2.0f;

            var guide = nandaGo.AddComponent<NandaToyRoomGuideController>();
            guide.Configure(player, chest, sword, light);
            return guide;
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
