using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;

namespace YunaLegend.EditorScripts.Picnic
{
    /// <summary>
    /// Builds Picnic_Gameplay.unity for the 1B faithful port and captures real Unity camera PNGs
    /// for key 2.5D segments in -batchmode (no WebGL build or browser required).
    /// </summary>
    public static class PicnicSceneBuilderAndSnapshot
    {
        private readonly struct Sample
        {
            public readonly string File;
            public readonly float X;
            public readonly float Y;
            public readonly float Time;
            public readonly PicnicTerrain Terrain;
            public readonly float Discomfort;

            public Sample(string file, float x, float y, float time = 0f, PicnicTerrain terrain = PicnicTerrain.Soil, float discomfort = 0f)
            {
                File = file; X = x; Y = y; Time = time; Terrain = terrain; Discomfort = discomfort;
            }
        }

        private static readonly Sample[] Samples =
        {
            new Sample("01-house-stairs.png", 338f, 540f, 0f, PicnicTerrain.Wood, 0f),
            new Sample("02-first-grass.png", 1624f, 545f, 1.5f, PicnicTerrain.Grass, 1f),
            new Sample("03-first-river.png", 2795f, 524f, 2f, PicnicTerrain.Stone, 0f),
            new Sample("04-gravel-worm.png", 4130f, 590f, 1f, PicnicTerrain.Gravel, 1f),
            new Sample("05-wheel-and-clouds.png", 5050f, 465f, 1f, PicnicTerrain.Wood, 0f),
            new Sample("06-clouds.png", 5615f, 480f, 1f, PicnicTerrain.Cloud, 0f),
            new Sample("07-aerial-wheels.png", 24350f, 465f, 2f, PicnicTerrain.Wood, 0f),
            new Sample("08-great-tree-picnic.png", 47300f, 590f, 0f, PicnicTerrain.Soil, 0f),
        };

        private const string PicnicScenePath = "Assets/Scenes/Picnic_Gameplay.unity";

        [MenuItem("YunaLegend/Fase 5 - Um Dia no Jardim/Jogar (Play Direto)", false, 10)]
        public static void PlayPicnicDirectly()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                return;
            }
            if (!File.Exists(PicnicScenePath))
            {
                ConfigureTexturesAndBuildScene();
            }
            EditorSceneManager.playModeStartScene = null;
            EditorSceneManager.OpenScene(PicnicScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("YunaLegend/Fase 5 - Um Dia no Jardim/Abrir Cena", false, 20)]
        public static void OpenPicnicScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(PicnicScenePath))
            {
                ConfigureTexturesAndBuildScene();
            }
            EditorSceneManager.playModeStartScene = null;
            EditorSceneManager.OpenScene(PicnicScenePath, OpenSceneMode.Single);
        }

        [MenuItem("YunaLegend/Fase 5 - Um Dia no Jardim/Reconstruir Cena", false, 30)]
        public static void ConfigureTexturesAndBuildScene()
        {
            string folder = "Assets/Art/Picnic/Resources/Picnic";
            foreach (string path in Directory.GetFiles(folder, "*.png"))
            {
                string assetPath = path.Replace('\\', '/');
                var imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (imp == null) continue;
                bool changed = false;
                if (imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; changed = true; }
                if (imp.npotScale != TextureImporterNPOTScale.None) { imp.npotScale = TextureImporterNPOTScale.None; changed = true; }
                if (imp.maxTextureSize < 4096) { imp.maxTextureSize = 4096; changed = true; }
                if (imp.textureCompression != TextureImporterCompression.Uncompressed) { imp.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
                if (changed) imp.SaveAndReimport();
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.749f, 0.906f, 0.929f, 1f); // #bfe7ed

            var root = new GameObject("PicnicStage1B");
            var bootstrap = root.AddComponent<PicnicStageBootstrap>();
            var platforms = new GameObject("Platforms").AddComponent<PicnicPlatformController>();
            platforms.transform.SetParent(root.transform, false);
            var scenery = new GameObject("Scenery").AddComponent<PicnicSceneryController>();
            scenery.transform.SetParent(root.transform, false);
            var actors = new GameObject("Actors").AddComponent<PicnicActorController>();
            actors.transform.SetParent(root.transform, false);
            var ui = new GameObject("UI").AddComponent<PicnicNarrativeAndHudController>();
            ui.transform.SetParent(root.transform, false);
            var audioCtrl = new GameObject("Audio").AddComponent<PicnicAudioController>();
            audioCtrl.transform.SetParent(root.transform, false);

            bootstrap.Configure(cam, platforms, scenery, actors, ui, audioCtrl);

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Picnic_Gameplay.unity");
            Debug.Log("PICNIC_1B_SCENE_SAVED");
        }

        [MenuItem("YunaLegend/Fase 5 - Um Dia no Jardim/Capturar Segmentos PNG", false, 40)]
        public static void CaptureAllSegments()
        {
            ConfigureTexturesAndBuildScene();

            string outDir = Path.GetFullPath("Evidence/Picnic1B/Segments");
            Directory.CreateDirectory(outDir);

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Picnic_Gameplay.unity", OpenSceneMode.Single);
            var cam = Object.FindFirstObjectByType<Camera>();
            var platforms = Object.FindFirstObjectByType<PicnicPlatformController>();
            var scenery = Object.FindFirstObjectByType<PicnicSceneryController>();
            var actors = Object.FindFirstObjectByType<PicnicActorController>();

            var sim = new PicnicSimulation();
            sim.SkipStory();
            platforms.Build(sim.Level);
            scenery.Build(cam, sim.Level);
            actors.Build(sim);

            const int W = 1280;
            const int H = 720;
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32);
            var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
            cam.targetTexture = rt;
            cam.aspect = (float)W / H;

            var tType = typeof(PicnicSimulation);
            var pxProp = tType.GetProperty("PlayerX");
            var pyProp = tType.GetProperty("PlayerY");
            var timeProp = tType.GetProperty("Time");
            var terProp = tType.GetProperty("Terrain");
            var discProp = tType.GetProperty("Discomfort");
            var vxProp = tType.GetProperty("PlayerVx");
            var fxProp = tType.GetProperty("FairyX");
            var fyProp = tType.GetProperty("FairyY");

            try
            {
                foreach (var s in Samples)
                {
                    pxProp.SetValue(sim, s.X);
                    pyProp.SetValue(sim, s.Y);
                    timeProp.SetValue(sim, s.Time);
                    terProp.SetValue(sim, s.Terrain);
                    discProp.SetValue(sim, s.Discomfort);
                    vxProp.SetValue(sim, s.Discomfort > 0.35f ? PicnicLevelData.TerrainSpeed(s.Terrain) : 0f);
                    fxProp.SetValue(sim, s.X + 80f);
                    fyProp.SetValue(sim, s.Y - 125f);

                    platforms.Apply(sim);
                    actors.Apply(sim);
                    scenery.Apply(sim);

                    cam.Render();
                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
                    tex.Apply();
                    File.WriteAllBytes(Path.Combine(outDir, s.File), tex.EncodeToPNG());
                    Debug.Log("[Picnic1B] Captured " + s.File);
                }
            }
            finally
            {
                RenderTexture.active = null;
                cam.targetTexture = null;
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(tex);
            }

            Debug.Log("PICNIC_1B_SEGMENTS_OK");
        }
    }
}
