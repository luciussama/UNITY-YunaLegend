using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using YunaLegend.Arena.Core;
using YunaLegend.Arena.Runtime;
using YunaLegend.Illustrated25D.Environment;
using YunaLegend.ToyRoom.Core;
using YunaLegend.ToyRoom.Runtime;
using YunaLegend.UI;

namespace YunaLegend.EditorScripts
{
    /// <summary>
    /// Captura screenshots reais renderizados pela Unity de todas as telas de UI do jogo
    /// em resolucoes mobile retrato (390x844) e paisagem (844x390) para auditoria visual.
    /// </summary>
    public static class MobileUiCapture
    {
        private const string OutputDir = "Logs/mobile_ui_audit";
        private const int PortraitW = 390;
        private const int PortraitH = 844;
        private const int LandscapeW = 844;
        private const int LandscapeH = 390;

        public static void CaptureAll()
        {
            Directory.CreateDirectory(OutputDir);

            CaptureTitleScreen();
            CaptureDarkRoom();
            CaptureToyRoom();
            CaptureArena();

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Debug.Log("MOBILE_UI_CAPTURE_OK " + Path.GetFullPath(OutputDir));
        }

        private static void CaptureTitleScreen()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/TitleScreen.unity");
            var cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            var layout = UnityEngine.Object.FindFirstObjectByType<TitleScreenResponsiveLayout>();
            var selector = UnityEngine.Object.FindFirstObjectByType<TitlePhaseSelector>();
            var controller = UnityEngine.Object.FindFirstObjectByType<TitleScreenController>();

            // 1) Portrait normal
            RenderSceneWithUi(cam, PortraitW, PortraitH, () => layout?.SetViewportOverride(PortraitW, PortraitH), "01_title_portrait.png");

            // 2) Landscape mobile
            RenderSceneWithUi(cam, LandscapeW, LandscapeH, () => layout?.SetViewportOverride(LandscapeW, LandscapeH), "02_title_landscape.png");

            // 3) Portrait with phase dropdown open
            if (selector != null)
            {
                selector.Open();
                RenderSceneWithUi(cam, PortraitW, PortraitH, () => layout?.SetViewportOverride(PortraitW, PortraitH), "03_title_dropdown_portrait.png");
                selector.Close();
            }

            // 4) Portrait with settings panel open
            if (controller != null)
            {
                controller.OpenSettings();
                RenderSceneWithUi(cam, PortraitW, PortraitH, () => layout?.SetViewportOverride(PortraitW, PortraitH), "04_title_settings_portrait.png");
                controller.CloseSettings();
            }
        }

        private static void CaptureDarkRoom()
        {
            EditorSceneManager.OpenScene("Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-Completa.unity");
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();

            // Cria um StageHudController temporario em EditMode conectado a estados reais de cena
            var hudHost = new GameObject("__TempDarkRoomHudAudit");
            try
            {
                var hud = hudHost.AddComponent<StageHudController>();
                var currentPhase = StageFlowStateMachine.FlowPhase.OpeningNandaWake1;
                bool isPaused = false;
                hud.GetPhase = () => currentPhase;
                hud.IsPaused = () => isPaused;
                hud.IsMuted = () => false;
                hud.IsPlayerDead = () => false;
                hud.IsFalseDoorDialogueReady = () => true;
                hud.GetOpeningCurtainAlpha = () => 0f;
                hud.GetOpeningDialogueBeat = () => 0;
                hud.IsOpeningYunaInBed = () => false;

                // Garante que o Canvas do StageHudController seja criado antes da busca de Canvases
                hud.SetViewportOverride(PortraitW, PortraitH);
                hud.RefreshHud();

                // 5) Opening Nanda dialogue (portrait)
                RenderSceneWithUi(cam, PortraitW, PortraitH, () =>
                {
                    hud.SetViewportOverride(PortraitW, PortraitH);
                    hud.RefreshHud();
                }, "05_darkroom_dialogue_portrait.png");

                // 6) Poetic intro card (portrait)
                currentPhase = StageFlowStateMachine.FlowPhase.OpeningPoeticIntro;
                RenderSceneWithUi(cam, PortraitW, PortraitH, () =>
                {
                    hud.SetViewportOverride(PortraitW, PortraitH);
                    hud.RefreshHud();
                }, "06_darkroom_poetic_portrait.png");

                // 7) Both Poetic + Dialogue stacked (CastleCutsceneStep0, portrait)
                currentPhase = StageFlowStateMachine.FlowPhase.CastleCutsceneStep0;
                RenderSceneWithUi(cam, PortraitW, PortraitH, () =>
                {
                    hud.SetViewportOverride(PortraitW, PortraitH);
                    hud.RefreshHud();
                }, "07_darkroom_cutscene_both_portrait.png");

                // 8) Pause modal (portrait)
                isPaused = true;
                RenderSceneWithUi(cam, PortraitW, PortraitH, () =>
                {
                    hud.SetViewportOverride(PortraitW, PortraitH);
                    hud.RefreshHud();
                }, "08_darkroom_pause_portrait.png");

                // 9) Dialogue (landscape)
                isPaused = false;
                currentPhase = StageFlowStateMachine.FlowPhase.OpeningNandaWake1;
                RenderSceneWithUi(cam, LandscapeW, LandscapeH, () =>
                {
                    hud.SetViewportOverride(LandscapeW, LandscapeH);
                    hud.RefreshHud();
                }, "09_darkroom_dialogue_landscape.png");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(hudHost);
            }
        }

        private static void CaptureToyRoom()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/ToyRoom_Gameplay.unity");
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var hud = UnityEngine.Object.FindFirstObjectByType<ToyRoomHudController>();
            if (hud == null)
            {
                return;
            }

            hud.UpdateToyCounter(3, 8);
            hud.ForceShowMobileControlsForCapture(true);
            hud.SetActionPrompt("PEGAR", true);

            // 10) Dialogue box (portrait)
            hud.ShowBeat(new ToyRoomNarrativeBeat(
                ToyRoomBeatKind.Dialogue,
                ToyRoomSpeaker.Nanda,
                "Olha só quanta bagunça! Vamos guardar os brinquedos no baú antes que escureça.",
                5f));
            RenderSceneWithUi(cam, PortraitW, PortraitH, () => hud.SetViewportOverride(PortraitW, PortraitH), "10_toyroom_dialogue_portrait.png");

            // 11) Poetic parchment card (portrait)
            hud.ShowBeat(new ToyRoomNarrativeBeat(
                ToyRoomBeatKind.Poetic,
                ToyRoomSpeaker.None,
                "A sala estava cheia.\nNenhum lugar esperava por ela.",
                5f));
            RenderSceneWithUi(cam, PortraitW, PortraitH, () => hud.SetViewportOverride(PortraitW, PortraitH), "11_toyroom_poetic_portrait.png");

            // 12) Pause menu (portrait)
            hud.ClearNarrativeOverlays();
            hud.SetPausedState(true);
            RenderSceneWithUi(cam, PortraitW, PortraitH, () => hud.SetViewportOverride(PortraitW, PortraitH), "12_toyroom_pause_portrait.png");
            hud.SetPausedState(false);

            // 13) Dialogue box (landscape)
            hud.ShowBeat(new ToyRoomNarrativeBeat(
                ToyRoomBeatKind.Dialogue,
                ToyRoomSpeaker.Menina,
                "Eu já guardei esse ursinho... Por que ele voltou pra cá?",
                5f));
            RenderSceneWithUi(cam, LandscapeW, LandscapeH, () => hud.SetViewportOverride(LandscapeW, LandscapeH), "13_toyroom_dialogue_landscape.png");
        }

        private static void CaptureArena()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Arena_Gameplay.unity");
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var hud = UnityEngine.Object.FindFirstObjectByType<ArenaHudController>();
            if (hud == null)
            {
                return;
            }

            // Garante BuildHud() em EditMode via Awake simulado se necessario
            if (hud.GetComponentInChildren<Canvas>(true) == null)
            {
                var awakeMethod = typeof(ArenaHudController).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                awakeMethod?.Invoke(hud, null);
            }

            var sim = new ArenaCombatSimulation(false, true);
            var arenaCam = UnityEngine.Object.FindFirstObjectByType<ArenaCameraController>();
            hud.SkipFadeForCapture();
            hud.ForceShowMobileControlsForCapture(true);
            hud.Refresh(sim);

            // 14) Arena HUD + Touch Controls (portrait)
            RenderSceneWithUi(cam, PortraitW, PortraitH, () =>
            {
                arenaCam?.UpdateFraming(sim, 10f);
                hud.SetViewportOverride(PortraitW, PortraitH);
                hud.Refresh(sim);
            }, "14_arena_hud_portrait.png");

            // 15) Arena Pause modal (portrait)
            hud.SetPausedState(true);
            RenderSceneWithUi(cam, PortraitW, PortraitH, () =>
            {
                arenaCam?.UpdateFraming(sim, 10f);
                hud.SetViewportOverride(PortraitW, PortraitH);
            }, "15_arena_pause_portrait.png");
            hud.SetPausedState(false);

            // 16) Arena HUD + Touch Controls (landscape)
            RenderSceneWithUi(cam, LandscapeW, LandscapeH, () =>
            {
                arenaCam?.UpdateFraming(sim, 10f);
                hud.SetViewportOverride(LandscapeW, LandscapeH);
                hud.Refresh(sim);
            }, "16_arena_hud_landscape.png");
        }

        private static void RenderSceneWithUi(Camera cam, int width, int height, Action beforeRender, string fileName)
        {
            if (cam == null)
            {
                Debug.LogWarning($"[MobileUiCapture] Camera nula ao capturar {fileName}");
                return;
            }

            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var prevModes = new RenderMode[canvases.Length];
            var prevCameras = new Camera[canvases.Length];
            var prevDistances = new float[canvases.Length];
            var prevLayers = new int[canvases.Length];

            var rtScene = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var rtUi = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prevTarget = cam.targetTexture;
            float prevAspect = cam.aspect;
            int prevMask = cam.cullingMask;

            var uiCamGo = new GameObject("__TempUiOverlayCam");
            var uiCam = uiCamGo.AddComponent<Camera>();
            uiCam.cameraType = CameraType.Preview;
            uiCam.orthographic = true;
            uiCam.orthographicSize = 5f;
            uiCam.nearClipPlane = 0.1f;
            uiCam.farClipPlane = 10f;
            uiCam.clearFlags = CameraClearFlags.SolidColor;
            uiCam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            uiCam.cullingMask = 1 << 5; // UI layer only
            uiCam.aspect = (float)width / height;
            uiCam.targetTexture = rtUi;

            try
            {
                // 1) Renderiza a cena 3D/2.5D sem a layer UI em rtScene
                cam.aspect = (float)width / height;
                cam.cullingMask = prevMask & ~(1 << 5);
                cam.targetTexture = rtScene;

                // 2) Renderiza todos os Canvases de UI com fundo transparente em rtUi
                for (int i = 0; i < canvases.Length; i++)
                {
                    prevModes[i] = canvases[i].renderMode;
                    prevCameras[i] = canvases[i].worldCamera;
                    prevDistances[i] = canvases[i].planeDistance;
                    prevLayers[i] = canvases[i].gameObject.layer;

                    SetLayerRecursively(canvases[i].gameObject, 5);
                    canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                    canvases[i].worldCamera = uiCam;
                    canvases[i].planeDistance = 1f;
                }

                Canvas.ForceUpdateCanvases();
                beforeRender?.Invoke();
                Canvas.ForceUpdateCanvases();

                cam.Render();
                uiCam.Render();

                // 3) Compoe UI sobre a cena 3D com alpha blending padrao (identico a ScreenSpaceOverlay)
                var prevActive = RenderTexture.active;
                RenderTexture.active = rtScene;
                var sceneTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                sceneTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                sceneTex.Apply();

                RenderTexture.active = rtUi;
                var uiTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                uiTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                uiTex.Apply();
                RenderTexture.active = prevActive;

                var scenePixels = sceneTex.GetPixels32();
                var uiPixels = uiTex.GetPixels32();
                for (int p = 0; p < scenePixels.Length; p++)
                {
                    byte a = uiPixels[p].a;
                    if (a == 0)
                    {
                        continue;
                    }

                    if (a == 255)
                    {
                        scenePixels[p] = uiPixels[p];
                        scenePixels[p].a = 255;
                        continue;
                    }

                    float alpha = a / 255f;
                    float inv = 1f - alpha;
                    scenePixels[p].r = (byte)Mathf.Clamp(Mathf.RoundToInt(uiPixels[p].r * alpha + scenePixels[p].r * inv), 0, 255);
                    scenePixels[p].g = (byte)Mathf.Clamp(Mathf.RoundToInt(uiPixels[p].g * alpha + scenePixels[p].g * inv), 0, 255);
                    scenePixels[p].b = (byte)Mathf.Clamp(Mathf.RoundToInt(uiPixels[p].b * alpha + scenePixels[p].b * inv), 0, 255);
                    scenePixels[p].a = 255;
                }

                sceneTex.SetPixels32(scenePixels);
                sceneTex.Apply();
                File.WriteAllBytes(Path.Combine(OutputDir, fileName), sceneTex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(sceneTex);
                UnityEngine.Object.DestroyImmediate(uiTex);
            }
            finally
            {
                for (int i = 0; i < canvases.Length; i++)
                {
                    if (canvases[i] != null)
                    {
                        canvases[i].renderMode = prevModes[i];
                        canvases[i].worldCamera = prevCameras[i];
                        canvases[i].planeDistance = prevDistances[i];
                        SetLayerRecursively(canvases[i].gameObject, prevLayers[i]);
                    }
                }

                cam.cullingMask = prevMask;
                cam.targetTexture = prevTarget;
                cam.aspect = prevAspect;
                uiCam.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(uiCamGo);
                UnityEngine.Object.DestroyImmediate(rtScene);
                UnityEngine.Object.DestroyImmediate(rtUi);
            }
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
