using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DreamForest.Core;
using YunaLegend.DreamForest.Runtime;

namespace YunaLegend.DreamForest.Editor
{
    public static class DreamForestSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/DreamForest_Gameplay.unity";

        [MenuItem("YunaLegend/Fase 6 - Jardim dos Sonhos/Jogar (Play Direto)", false, 10)]
        public static void PlayDreamForestDirectly()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (!File.Exists(ScenePath))
            {
                BuildDreamForestScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorSceneManager.playModeStartScene = null;
            EditorApplication.isPlaying = true;
        }

        [MenuItem("YunaLegend/Fase 6 - Jardim dos Sonhos/Abrir Cena", false, 20)]
        public static void OpenDreamForestScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[DreamForestSceneBuilder] Saia do Play Mode antes de abrir a cena.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (!File.Exists(ScenePath))
            {
                BuildDreamForestScene();
            }

            EditorSceneManager.playModeStartScene = null;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("YunaLegend/Fase 6 - Jardim dos Sonhos/Reconstruir Cena", false, 30)]
        public static void BuildDreamForestScene()
        {
            EditorSceneManager.playModeStartScene = null;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.6f;
            cam.backgroundColor = new Color(0.05f, 0.08f, 0.16f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            var bootstrapGo = new GameObject("DreamForestStageBootstrap");
            bootstrapGo.AddComponent<DreamForestStageBootstrap>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterInBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
        }

        public static void BuildSceneAndCapture()
        {
            DreamForestAssetPostprocessor.ReimportAllDreamForestTextures();
            VerifyFootSoleVisualContract();
            BuildDreamForestScene();
            CaptureVisualAudit();
            Debug.Log("[DreamForest] DREAM_FOREST_BUILD_AND_CAPTURE_OK");
        }

        private static void VerifyFootSoleVisualContract()
        {
            VerifySheetBottomPadding("girl-running-v1", DreamForestAtlasCatalog.GirlRunningRects);
            VerifySheetBottomPadding("girl-actions-v1", DreamForestAtlasCatalog.GirlActionRects);
            Debug.Log("[DreamForest] VISUAL_CONTRACT_FOOT_SOLE_OK (all 16 rects <= 2px bottom padding)");
        }

        private static void VerifySheetBottomPadding(string textureName, RectInt[] rects)
        {
            var tex = DreamForestAtlasCatalog.LoadTexture(textureName);
            if (tex == null || !tex.isReadable)
            {
                throw new System.InvalidOperationException($"[DreamForest] Texture {textureName} must be readable for foot-sole contract check.");
            }
            var pixels = tex.GetPixels32();
            int w = tex.width;
            int h = tex.height;
            for (int i = 0; i < rects.Length; i++)
            {
                var r = rects[i];
                int bottomRow = h - r.y - r.height;
                int pad = 0;
                for (int dy = 0; dy < r.height; dy++)
                {
                    int py = bottomRow + dy;
                    bool hasOpaque = false;
                    for (int dx = 0; dx < r.width; dx++)
                    {
                        if (pixels[py * w + (r.x + dx)].a > 20)
                        {
                            hasOpaque = true;
                            break;
                        }
                    }
                    if (hasOpaque) break;
                    pad++;
                }
                if (pad > 2)
                {
                    throw new System.InvalidOperationException($"[DreamForest] {textureName}[{i}] has {pad}px transparent padding below feet (max 2px).");
                }
            }
        }

        private static void RegisterInBuildSettings(string scenePath)
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in list)
            {
                if (s.path == scenePath)
                {
                    s.enabled = true;
                    EditorBuildSettings.scenes = list.ToArray();
                    return;
                }
            }
            list.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        [MenuItem("YunaLegend/Fase 6 - Jardim dos Sonhos/Capturar Evidências PNG", false, 40)]
        public static void CaptureVisualAudit()
        {
            string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Evidence", "DreamForest"));
            Directory.CreateDirectory(outDir);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("AuditCam", typeof(Camera));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            var sim = new DreamForestSimulation(false);

            var scenery = new GameObject("Scenery").AddComponent<DreamForestSceneryController>();
            scenery.Build(cam, sim.Level);
            var actor = new GameObject("Actor").AddComponent<DreamForestActorController>();
            actor.Build();
            var hud = new GameObject("Hud").AddComponent<DreamForestHudController>();
            hud.Build();

            foreach (Canvas c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                c.renderMode = RenderMode.ScreenSpaceCamera;
                c.worldCamera = cam;
                c.planeDistance = 5f;
            }

            // 1. Act 1 Dream Forest exploration (moonlit parallax forest, leaf + painted stem, fixed-root grass ribbon, fireflies, grounded Yuna)
            for (int i = 0; i < 200; i++) sim.Step(new DreamForestInput(0f, 0f, false, true), 0.2f);
            sim.Step(new DreamForestInput(1f, 0f, false), 0.1f);
            RenderFrame(cam, scenery, actor, hud, sim, Path.Combine(outDir, "02-act1-dream-forest.png"));

            // 1b. Close-up 2x zoom on Yuna's feet standing on start leaf (rest-0) for contact audit
            RenderCloseupFrame(cam, scenery, actor, hud, sim, Path.Combine(outDir, "02b-act1-closeup-feet.png"));

            // 2. Glider flight through extended thorn gates
            sim.BeginGliderSectionForTesting();
            for (int i = 0; i < 40; i++) sim.Step(new DreamForestInput(1f, 0f, false), 0.15f);
            RenderFrame(cam, scenery, actor, hud, sim, Path.Combine(outDir, "03-glider-flight.png"));

            // 3. Act 2 Nightmare Stealth (dark forest, swamp trunks, rock covers + contact shadows, giant roach + eye flares + surface-clamped spotlights)
            sim.BeginNightmareSectionForTesting(skipIntro: true);
            for (int i = 0; i < 55; i++) sim.Step(new DreamForestInput(i < 16 ? 1f : 0f, 0f, false), 0.05f);
            RenderFrame(cam, scenery, actor, hud, sim, Path.Combine(outDir, "04-act2-nightmare-stealth.png"));

            // 4. Sunlit Wake on picnic blanket with Yuna waking & blue-tinted Nanda floating
            sim.BeginSunlitWakeForTesting();
            RenderFrame(cam, scenery, actor, hud, sim, Path.Combine(outDir, "05-sunlit-wake.png"));
        }

        private static void RenderFrame(
            Camera cam,
            DreamForestSceneryController scenery,
            DreamForestActorController actor,
            DreamForestHudController hud,
            DreamForestSimulation sim,
            string filePath)
        {
            const int W = 1280;
            const int H = 720;
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;
            cam.targetTexture = rt;

            scenery.Apply(sim);
            actor.Apply(sim);
            hud.Apply(sim, false, false);
            Canvas.ForceUpdateCanvases();
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
            tex.Apply();
            File.WriteAllBytes(filePath, tex.EncodeToPNG());

            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(rt);
        }

        private static void RenderCloseupFrame(
            Camera cam,
            DreamForestSceneryController scenery,
            DreamForestActorController actor,
            DreamForestHudController hud,
            DreamForestSimulation sim,
            string filePath)
        {
            const int W = 1280;
            const int H = 720;
            var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;
            cam.targetTexture = rt;

            scenery.Apply(sim);
            actor.Apply(sim);
            hud.Apply(sim, false, false);

            // Zoom 2.4x centered directly on Yuna's feet & start leaf surface
            float prevOrtho = cam.orthographicSize;
            Vector3 prevPos = cam.transform.position;
            Vector3 yunaWorld = DreamForestAtlasCatalog.ToUnity(sim.Player.X, sim.Player.Y, 0f);
            cam.orthographicSize = 1.9f;
            cam.transform.position = new Vector3(yunaWorld.x + 0.6f, yunaWorld.y + 0.25f, -10f);

            Canvas.ForceUpdateCanvases();
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
            tex.Apply();
            File.WriteAllBytes(filePath, tex.EncodeToPNG());

            cam.orthographicSize = prevOrtho;
            cam.transform.position = prevPos;
            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(rt);
        }
    }
}
