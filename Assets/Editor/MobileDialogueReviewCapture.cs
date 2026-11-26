using System;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YunaLegend.Presentation;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;
using YunaLegend.ToyRoom.Core;
using YunaLegend.ToyRoom.Runtime;
using YunaLegend.Arena.Core;
using YunaLegend.Arena.Runtime;
using YunaLegend.BearChase.Core;
using YunaLegend.BearChase.Runtime;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;
namespace YunaLegend.EditorScripts
{
    public static class MobileDialogueReviewCapture
    {
        private const string LiveFlag = "YunaMobileDialogueLive";
        private static double nextLiveCapture;
        private static int liveScene;
        public static void CaptureLive()
        {
            SessionState.SetBool(LiveFlag, true);
            EditorSceneManager.OpenScene("Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-Completa.unity");
            EditorApplication.EnterPlaymode();
        }

        [InitializeOnLoadMethod]
        private static void ResumeLiveCapture()
        {
            if (!SessionState.GetBool(LiveFlag, false))
            {
                return;
            }
            nextLiveCapture = EditorApplication.timeSinceStartup + 6;
            EditorApplication.update += TickLiveCapture;
        }
        private static void TickLiveCapture()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < nextLiveCapture)
            {
                return;
            }
            try
            {
                var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
                var presenter = new GameObject("LiveMobileDialogue").AddComponent<MobileDialoguePresenter>();
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(844, 390) })
                {
                    render.Invoke(null, new object[] {camera,size.x,size.y,(Action)(() => {
                        presenter.SetMobileViewportOverride(true,size.x,size.y);presenter.ApplyNow();Canvas.ForceUpdateCanvases();
                    }),"live-dialogue-"+liveScene+"-"+size.x+"x"+size.y+".png"});
                }
                if (liveScene == 0)
                {
                    liveScene = 1;
                    UnityEngine.SceneManagement.SceneManager.LoadScene("ToyRoom_Gameplay");
                    nextLiveCapture = EditorApplication.timeSinceStartup + 7;
                    return;
                }
                Debug.Log("MOBILE_LIVE_DIALOGUE_OK");
                SessionState.SetBool(LiveFlag, false);
                EditorApplication.update -= TickLiveCapture;
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                SessionState.SetBool(LiveFlag, false);
                EditorApplication.Exit(1);
            }
        }

        private static readonly MethodInfo render = typeof(MobileUiCapture).GetMethod("RenderSceneWithUi", BindingFlags.Static | BindingFlags.NonPublic);
        private static bool darkBoth;
        private static readonly string[] scenes = { "dark", "toy", "arena", "bear", "picnic" };
        private static string UiSnapshot()
        {
            var output = new System.Text.StringBuilder();
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.InstanceID))
            {
                foreach (var rect in canvas.GetComponentsInChildren<RectTransform>(true))
                {
                    output.Append(rect.GetInstanceID()).Append(rect.anchoredPosition).Append(rect.sizeDelta).Append(rect.anchorMin).Append(rect.anchorMax).Append(rect.pivot);
                    var text = rect.GetComponent<Text>();
                    if (text != null)
                    {
                        output.Append(text.fontSize).Append(text.lineSpacing).Append(text.text);
                    }
                    var graphic = rect.GetComponent<Graphic>();
                    if (graphic != null)
                    {
                        output.Append(graphic.enabled).Append(graphic.color);
                    }
                }
            }
            return output.ToString();
        }

        public static void CaptureAllFinal()
        {
            ArenaMobileCameraReview.Capture();
            Capture();
        }

        public static void Capture()
        {
            Directory.CreateDirectory("Logs/mobile_ui_audit");
            foreach (string scene in scenes)
            {
                string path = scene == "dark" ? "Assets/Art/Illustrated25D/Environment/Stage2Runtime/Fase-Jogavel-Completa.unity" : "Assets/Scenes/" + (scene == "toy" ? "ToyRoom" : scene == "arena" ? "Arena" : scene == "bear" ? "BearChase" : "Picnic") + "_Gameplay.unity";
                EditorSceneManager.OpenScene(path);
                Camera camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
                Action<int, int> setup;
                if (scene == "dark")
                {
                    var hud = new GameObject("MobileDialogueHud").AddComponent<StageHudController>();
                    hud.GetPhase = () => darkBoth ? StageFlowStateMachine.FlowPhase.CastleCutsceneStep0 : StageFlowStateMachine.FlowPhase.OpeningNandaWake1;
                    hud.IsPaused = () => false;
                    hud.IsMuted = () => false;
                    hud.IsPlayerDead = () => false;
                    hud.IsFalseDoorDialogueReady = () => true;
                    hud.GetOpeningCurtainAlpha = () => 0f;
                    hud.GetOpeningDialogueBeat = () => 0;
                    hud.IsOpeningYunaInBed = () => false;
                    setup = (w, h) => { hud.SetViewportOverride(w, h); hud.RefreshHud(); };
                }
                else if (scene == "toy")
                {
                    var hud = UnityEngine.Object.FindFirstObjectByType<ToyRoomHudController>();
                    hud.UpdateToyCounter(3, 8);
                    hud.ForceShowMobileControlsForCapture(true);
                    hud.ShowBeat(new ToyRoomNarrativeBeat(ToyRoomBeatKind.Dialogue, ToyRoomSpeaker.Nanda, "Olha s? quanta bagun?a! Vamos guardar os brinquedos no ba? antes que escure?a.", 5f));
                    setup = (w, h) => hud.SetViewportOverride(w, h);
                }
                else if (scene == "arena")
                {
                    var hud = UnityEngine.Object.FindFirstObjectByType<ArenaHudController>();
                    typeof(ArenaHudController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hud, null);
                    var sim = new ArenaCombatSimulation(false, true);
                    var framing = UnityEngine.Object.FindFirstObjectByType<ArenaCameraController>();
                    framing.SetMobileViewportOverride(true);
                    hud.SkipFadeForCapture();
                    hud.ForceShowMobileControlsForCapture(true);
                    setup = (w, h) => { camera.aspect = (float)w / h; framing.SetMobileViewportOverride(w < 1000); framing.UpdateFraming(sim, 10f); hud.SetViewportOverride(w, h); hud.Refresh(sim); };
                }
                else if (scene == "bear")
                {
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
                    hud.UpdatePerFrameOverlays(sim);
                    setup = (w, h) => hud.SetViewportOverride(w, h);
                }
                else
                {
                    var sim = new PicnicSimulation();
                    var hud = UnityEngine.Object.FindFirstObjectByType<PicnicNarrativeAndHudController>();
                    UnityEngine.Object.FindFirstObjectByType<PicnicPlatformController>().Build(sim.Level);
                    UnityEngine.Object.FindFirstObjectByType<PicnicSceneryController>().Build(camera, sim.Level);
                    UnityEngine.Object.FindFirstObjectByType<PicnicActorController>().Build(sim);
                    hud.Build();
                    setup = (w, h) => { hud.SetViewportOverride(w, h); hud.Apply(sim, false); };
                }
                setup(390, 844);
                var presenter = new GameObject("MobileDialogueReview").AddComponent<MobileDialoguePresenter>();
                presenter.SetMobileViewportOverride(false, 1280, 720);
                string desktopBefore = UiSnapshot();
                presenter.ApplyNow();
                if (desktopBefore != UiSnapshot())
                {
                    throw new Exception("Desktop UI mutated");
                }
                render.Invoke(null, new object[] { camera, 1280, 720, (Action)(() => { setup(1280, 720); presenter.ApplyNow(); }), "desktop-" + scene + ".png" });
                foreach (Vector2Int size in new[] { new Vector2Int(390, 844), new Vector2Int(844, 390), new Vector2Int(320, 568) })
                {
                    render.Invoke(null, new object[] {camera,size.x,size.y,(Action)(() => {
                        setup(size.x,size.y); Canvas.ForceUpdateCanvases(); setup(size.x,size.y);
                        presenter.SetMobileViewportOverride(true,size.x,size.y); presenter.ApplyNow(); Canvas.ForceUpdateCanvases();
                        if(scene=="bear" && size.x==390)
                        {
                            var output = new System.Text.StringBuilder();
                            output.AppendLine("camera="+camera.name+" main="+(Camera.main==null?"NULL":Camera.main.name));
                            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                            {
                                if (r.name.Contains("Yuna")) { output.AppendLine(r.name+" enabled="+r.enabled+" bounds="+r.bounds+" head="+camera.WorldToScreenPoint(new Vector3(r.bounds.center.x,r.bounds.max.y,r.bounds.center.z))); }
                            }
                            foreach(var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)) output.AppendLine("canvas="+c.name+" head="+typeof(MobileDialoguePresenter).GetMethod("FindHead",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(presenter,new object[]{c}));
                            File.WriteAllText("Logs/mobile_ui_audit/bear-head-diagnostics.txt",output.ToString());
                        }
                    }), "compact-"+scene+"-"+size.x+"x"+size.y+".png" });
                }
                if (scene == "dark")
                {
                    darkBoth = true;
                    render.Invoke(null, new object[]{camera,390,844,(Action)(()=>{
                        setup(390,844);Canvas.ForceUpdateCanvases();presenter.SetMobileViewportOverride(true,390,844);presenter.ApplyNow();
                    }),"compact-dark-stacked-390x844.png"});
                    darkBoth = false;
                }
            }
            EditorSceneManager.OpenScene("Assets/Scenes/TitleScreen.unity");
            var protectedPresenter = new GameObject("ProtectedTitleTest").AddComponent<MobileDialoguePresenter>();
            string titleBefore = UiSnapshot();
            protectedPresenter.SetMobileViewportOverride(true, 390, 844);
            protectedPresenter.ApplyNow();
            if (titleBefore != UiSnapshot())
            {
                throw new Exception("Title UI mutated");
            }
            Debug.Log("TITLE_DESKTOP_NO_MUTATION_OK");
            Debug.Log("MOBILE_DIALOGUE_REVIEW_OK");
        }
    }
}
