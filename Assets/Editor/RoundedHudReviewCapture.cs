using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YunaLegend.BearChase.Core;
using YunaLegend.BearChase.Runtime;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;
using YunaLegend.Arena.Core;
using YunaLegend.Arena.Runtime;
using YunaLegend.Illustrated25D.Environment;
using YunaLegend.ToyRoom.Core;
using YunaLegend.ToyRoom.Runtime;
using YunaLegend.UI;

namespace YunaLegend.EditorScripts
{
    /// <summary>Static Unity camera evidence; exercises the existing UI events without changing gameplay.</summary>
    public static class RoundedHudReviewCapture
    {
        private static readonly List<string> measurements = new List<string>();
        private static readonly MethodInfo renderer = typeof(MobileUiCapture).GetMethod("RenderSceneWithUi", BindingFlags.Static | BindingFlags.NonPublic);

        public static void CaptureAll()
        {
            MobileUiCapture.CaptureAll();
            CaptureAdditional();
        }

        public static void CaptureAdditional()
        {
            Directory.CreateDirectory("Logs/mobile_ui_audit");
            CaptureBear();
            CapturePicnic();
            CaptureResponsiveEdges();
            File.WriteAllLines("Logs/mobile_ui_audit/control-bounds.tsv", measurements);
            Debug.Log("ROUNDED_HUD_CAPTURE_OK");
        }

        private static void CaptureBear()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/BearChase_Gameplay.unity");
            var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            var hud = UnityEngine.Object.FindFirstObjectByType<BearChaseHudController>();
            var sim = new BearChaseSimulation();
            for (int i = 0; i < 480; i++)
            {
                sim.Step(1f / 60f);
            }
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<BearChaseStageBootstrap>();
            Vector3 origin = bootstrap.ChaseOrigin;
            UnityEngine.Object.FindFirstObjectByType<BearChaseSceneryController>().ApplyPhaseVisibility(BearChasePhase.Chasing);
            UnityEngine.Object.FindFirstObjectByType<BearChaseLightingController>().ApplyAtmosphere();
            UnityEngine.Object.FindFirstObjectByType<BearChasePlayerController>(FindObjectsInactive.Include).UpdatePlayer(sim, origin, 0f);
            UnityEngine.Object.FindFirstObjectByType<BearChaseCameraController>().UpdateCamera(sim, origin, 10f);
            hud.EnsureBuilt();
            hud.SetHearts(3, 3);
            hud.HandlePhaseChanged(BearChasePhase.Chasing, false);
            Render(camera, 390, 844, () => hud.SetViewportOverride(390, 844), "17_bear_hud_portrait.png");
            Render(camera, 844, 390, () => hud.SetViewportOverride(844, 390), "18_bear_hud_landscape.png");
            Render(camera, 320, 568, () => hud.SetViewportOverride(320, 568), "29_bear_hud_320.png");
            Render(camera, 1280, 720, () => hud.SetViewportOverride(1280, 720), "30_bear_hud_desktop.png");
            hud.SetPaused(true);
            Render(camera, 390, 844, () => hud.SetViewportOverride(390, 844), "19_bear_pause_portrait.png");
            hud.SetPaused(false);
            hud.HandlePhaseChanged(BearChasePhase.RetryModal, true);
            Render(camera, 844, 390, () => hud.SetViewportOverride(844, 390), "20_bear_retry_landscape.png");
            Render(camera, 320, 568, () => hud.SetViewportOverride(320, 568), "31_bear_retry_320.png");
        }

        private static void CapturePicnic()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Picnic_Gameplay.unity");
            var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            var hud = UnityEngine.Object.FindFirstObjectByType<PicnicNarrativeAndHudController>();
            var platforms = UnityEngine.Object.FindFirstObjectByType<PicnicPlatformController>();
            var scenery = UnityEngine.Object.FindFirstObjectByType<PicnicSceneryController>();
            var actors = UnityEngine.Object.FindFirstObjectByType<PicnicActorController>();
            var sim = new PicnicSimulation();
            platforms.Build(sim.Level);
            scenery.Build(camera, sim.Level);
            actors.Build(sim);
            hud.Build();
            Render(camera, 390, 844, () => { Override(hud, 390, 844); hud.Apply(sim, false); }, "21_picnic_story_portrait.png");
            Render(camera, 844, 390, () => { Override(hud, 844, 390); hud.Apply(sim, false); }, "22_picnic_story_landscape.png");
            sim.SkipStory();
            platforms.Apply(sim);
            actors.Apply(sim);
            scenery.Apply(sim);
            hud.Apply(sim, false);
            Render(camera, 390, 844, () => { Override(hud, 390, 844); hud.Apply(sim, false); }, "23_picnic_hud_portrait.png");
            Render(camera, 844, 390, () => { Override(hud, 844, 390); hud.Apply(sim, false); }, "24_picnic_hud_landscape.png");
            Render(camera, 320, 568, () => { Override(hud, 320, 568); hud.Apply(sim, false); }, "25_picnic_hud_320.png");
            Render(camera, 1280, 720, () => { Override(hud, 1280, 720); hud.Apply(sim, false); }, "32_picnic_hud_desktop.png");
            Render(camera, 390, 844, () => { Override(hud, 390, 844); hud.Apply(sim, true); }, "26_picnic_pause_portrait.png");
            Render(camera, 844, 390, () => { Override(hud, 844, 390); hud.Apply(sim, true); }, "27_picnic_pause_landscape.png");
            Render(camera, 320, 568, () => { Override(hud, 320, 568); hud.Apply(sim, true); }, "28_picnic_pause_320.png");
            CheckPicnicEvents(hud);
        }

        private static void CaptureResponsiveEdges()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/TitleScreen.unity");
            var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            var title = UnityEngine.Object.FindFirstObjectByType<TitleScreenResponsiveLayout>();
            Render(camera, 320, 568, () => title.SetViewportOverride(320, 568), "33_title_320.png");
            Render(camera, 1280, 720, () => title.SetViewportOverride(1280, 720), "34_title_desktop.png");

            EditorSceneManager.OpenScene("Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-Completa.unity");
            camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            var dark = new GameObject("DarkHudReview").AddComponent<StageHudController>();
            dark.GetPhase = () => StageFlowStateMachine.FlowPhase.OpeningNandaWake1;
            dark.IsPaused = () => false;
            dark.IsMuted = () => false;
            dark.GetOpeningCurtainAlpha = () => 0f;
            dark.GetOpeningDialogueBeat = () => 0;
            dark.SetViewportOverride(320, 568);
            Render(camera, 320, 568, () => { dark.SetViewportOverride(320, 568); dark.RefreshHud(); }, "35_darkroom_320.png");
            Render(camera, 1280, 720, () => { dark.SetViewportOverride(1280, 720); dark.RefreshHud(); }, "36_darkroom_desktop.png");

            EditorSceneManager.OpenScene("Assets/Scenes/ToyRoom_Gameplay.unity");
            camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            var toy = UnityEngine.Object.FindFirstObjectByType<ToyRoomHudController>();
            toy.UpdateToyCounter(3, 8);
            toy.ForceShowMobileControlsForCapture(true);
            toy.SetActionPrompt("GUARDAR", true);
            toy.ShowBeat(new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Nanda,
                "Olha só quanta bagunça! Vamos guardar os brinquedos no baú antes que escureça.", 5f));
            Render(camera, 320, 568, () => toy.SetViewportOverride(320, 568), "37_toyroom_320.png");
            toy.ForceShowMobileControlsForCapture(false);
            Render(camera, 1280, 720, () => toy.SetViewportOverride(1280, 720), "38_toyroom_desktop.png");

            EditorSceneManager.OpenScene("Assets/Scenes/Arena_Gameplay.unity");
            camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            var arena = UnityEngine.Object.FindFirstObjectByType<ArenaHudController>();
            typeof(ArenaHudController).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(arena, null);
            arena.SkipFadeForCapture();
            arena.ForceShowMobileControlsForCapture(true);
            var sim = new ArenaCombatSimulation(false, true);
            arena.Refresh(sim);
            Render(camera, 320, 568, () => { arena.SetViewportOverride(320, 568); arena.Refresh(sim); }, "39_arena_320.png");
            arena.ForceShowMobileControlsForCapture(false);
            Render(camera, 1280, 720, () => { arena.SetViewportOverride(1280, 720); arena.Refresh(sim); }, "40_arena_desktop.png");
            arena.ForceShowMobileControlsForCapture(true);
            arena.SetPausedState(true);
            Render(camera, 320, 568, () => arena.SetViewportOverride(320, 568), "41_arena_pause_320.png");
        }

        private static void Override(PicnicNarrativeAndHudController hud, int width, int height)
        {
            typeof(PicnicNarrativeAndHudController).GetMethod("SetViewportOverride")?.Invoke(hud, new object[] { (float)width, (float)height });
        }

        private static void Render(Camera camera, int width, int height, Action layout, string file)
        {
            renderer.Invoke(null, new object[] { camera, width, height, (Action)(() =>
            {
                layout();
                Canvas.ForceUpdateCanvases();
                AuditControls(file, width, height);
            }), file });
        }

        private static void AuditControls(string capture, int width, int height)
        {
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                foreach (var rect in canvas.GetComponentsInChildren<RectTransform>())
                {
                    if (rect.GetComponent<Button>() == null && rect.GetComponent<EventTrigger>() == null)
                    {
                        continue;
                    }

                    var corners = new Vector3[4];
                    rect.GetWorldCorners(corners);
                    Vector2 a = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[0]);
                    Vector2 b = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[2]);
                    measurements.Add($"{capture}\t{rect.name}\t{a.x:F1}\t{a.y:F1}\t{b.x - a.x:F1}\t{b.y - a.y:F1}\t{width}x{height}");
                }
            }
        }

        private static void CheckPicnicEvents(PicnicNarrativeAndHudController hud)
        {
            int[] calls = new int[5];
            hud.AdvanceRequested += () => calls[0]++;
            hud.SkipRequested += () => calls[1]++;
            hud.RetryRequested += () => calls[2]++;
            hud.MenuRequested += () => calls[3]++;
            hud.PauseRequested += () => calls[4]++;
            foreach (var button in hud.GetComponentsInChildren<Button>(true))
            {
                button.onClick.Invoke();
            }

            foreach (int count in calls)
            {
                if (count != 1) throw new Exception("Picnic UI event lost or duplicated");
            }
            var jump = hud.transform.Find("PicnicHudCanvas/SafeArea/Jump").GetComponent<PicnicTouchButton>();
            var jumpPointer = new PointerEventData(EventSystem.current) { pointerId = 2 };
            ExecuteEvents.Execute(jump.gameObject, jumpPointer, ExecuteEvents.pointerDownHandler);
            if (!hud.ConsumeTouchJump() || !hud.ConsumeTouchJump()) throw new Exception("Jump press/hold lost");
            ExecuteEvents.Execute(jump.gameObject, jumpPointer, ExecuteEvents.pointerUpHandler);
            if (hud.ConsumeTouchJump()) throw new Exception("Jump release lost");
            foreach (var button in hud.GetComponentsInChildren<PicnicTouchButton>(true))
            {
                if (button.name == "Jump") continue;
                var pointer = new PointerEventData(EventSystem.current) { pointerId = 1 };
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
                float expected = button.name == "Left" ? -1f : 1f;
                if (hud.TouchAxis != expected) throw new Exception("Hold direction changed");
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                if (hud.TouchAxis != 0f) throw new Exception("Hold release changed");
            }
            Debug.Log("PICNIC_UI_EVENTS_OK five actions, jump, hold and release");
        }
    }
}
