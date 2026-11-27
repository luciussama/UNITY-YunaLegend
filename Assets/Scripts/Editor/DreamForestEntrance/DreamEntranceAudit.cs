using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.SceneManagement;
using YunaLegend.DreamForest.Core;
using YunaLegend.DreamForestEntrance.Runtime;

namespace YunaLegend.DreamForestEntrance.Editor
{
    /// <summary>Bounded editor audit command adapter; no second Editor or scene rebuild is required.</summary>
    [InitializeOnLoad]
    public static class DreamEntranceAudit
    {
        private static readonly string Evidence = Path.GetFullPath("Evidence/DreamForestEntrance");
        private static readonly string Request = Path.Combine(Evidence,"editor-request.txt");
        private static double nextPoll;
        private static readonly double StartedAt;
        private static TestRunnerApi runner;
        private static double batchWaitStarted;

        static DreamEntranceAudit()
        {
            StartedAt = EditorApplication.timeSinceStartup;
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            if (EditorApplication.isPlaying && SessionState.GetBool("DreamEntranceAudit.Motion",false))
            {
                EditorApplication.delayCall += BeginMotionCapture;
            }
            if (EditorApplication.isPlaying && SessionState.GetBool("DreamEntranceAudit.Interactions",false))
            {
                EditorApplication.delayCall += BeginInteractionCapture;
            }
        }

        private static void Poll()
        {
            if (EditorApplication.timeSinceStartup-StartedAt>300)
            {
                EditorApplication.update -= Poll;
                return;
            }
            if (EditorApplication.timeSinceStartup<nextPoll || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            nextPoll = EditorApplication.timeSinceStartup+.5;
            if (!File.Exists(Request)) return;
            string command = File.ReadAllText(Request).Trim();
            File.Delete(Request);
            try
            {
                if (command == "capture") Capture();
                else if (command == "tests") RunTests();
                else if (command == "motion") StartMotionCapture();
                else if (command == "interactions") StartInteractionCapture();
                else throw new InvalidOperationException("Unknown entrance audit command");
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(Evidence,"editor-error.txt"),ex.ToString());
                Debug.LogException(ex);
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("DreamEntranceAudit.Motion",false))
            {
                BeginMotionCapture();
            }
            if (state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("DreamEntranceAudit.Interactions",false))
            {
                BeginInteractionCapture();
            }
        }

        private static void BeginInteractionCapture()
        {
            DreamEntranceInteractionAudit.Begin();
        }

        [MenuItem("YunaLegend/Fase 6 - Jardim dos Sonhos/Entrada/Validar seletor e pausa",false,63)]
        public static void StartInteractionCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            SessionState.SetBool("DreamEntranceAudit.Interactions",true);
            EditorSceneManager.OpenScene("Assets/Scenes/DreamForest_Gameplay.unity",OpenSceneMode.Single);
            EditorSceneManager.playModeStartScene=null;
            EditorApplication.isPlaying=true;
        }

        private static void BeginMotionCapture()
        {
            var bootstrap=UnityEngine.Object.FindFirstObjectByType<YunaLegend.DreamForest.Runtime.DreamForestStageBootstrap>();
            if (bootstrap==null) return;
            DreamEntranceMotionCapture.Begin(bootstrap);
        }

        [MenuItem("YunaLegend/Fase 6 - Jardim dos Sonhos/Entrada/Capturar movimento Play Mode",false,62)]
        public static void StartMotionCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            SessionState.SetBool("DreamEntranceAudit.Motion",true);
            EditorSceneManager.OpenScene("Assets/Scenes/DreamForest_Gameplay.unity",OpenSceneMode.Single);
            EditorSceneManager.playModeStartScene=null;
            EditorApplication.isPlaying=true;
        }

        [MenuItem("YunaLegend/Fase 6 - Jardim dos Sonhos/Entrada/Capturar paridade",false,60)]
        public static void Capture()
        {
            Directory.CreateDirectory(Evidence);
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var go = new GameObject("DreamEntranceAudit");
                SceneManager.MoveGameObjectToScene(go,preview);
                var view = go.AddComponent<DreamEntranceView>();
                view.Build();
                view.RenderCamera.scene = preview;
                foreach(var camera in go.GetComponentsInChildren<Camera>(true)) camera.scene=preview;
                view.RenderCamera.clearFlags = CameraClearFlags.SolidColor;
                view.RenderCamera.backgroundColor = Color.black;
                int[,] sizes = { {1043,840},{1200,720},{390,844},{844,390} };
                for (int size=0;size<sizes.GetLength(0);size++)
                {
                    int width = sizes[size,0];
                    int height = sizes[size,1];
                    string folder = Path.Combine(Evidence,"Unity",$"{width}x{height}");
                    Directory.CreateDirectory(folder);
                    var sim = new DreamForestSimulation();
                    float elapsed = 0;
                    for (int beat=0;beat<7;beat++)
                    {
                        sim.Opening.Index = beat;
                        sim.Opening.Age = beat == 1 ? 3f : beat == 6 ? 1.5f : 1.2f;
                        sim.Opening.Time = elapsed+sim.Opening.Age;
                        sim.Opening.Dismissed = false;
                        Render(view,sim,width,height,Path.Combine(folder,$"{beat}-{DreamForestLevelData.OpeningBeats[beat].Shot}.png"));
                        elapsed += DreamForestLevelData.OpeningBeats[beat].Duration;
                    }
                    sim.Opening.Index = 6;
                    sim.Opening.Age = 3.4f;
                    Render(view,sim,width,height,Path.Combine(folder,"7-departure-fade.png"));
                    sim.Opening.Index=2;
                    sim.Opening.Age=1.2f;
                    sim.Opening.Time=10.2f;
                    Render(view,sim,width,height,Path.Combine(folder,"8-paused.png"),true);
                    view.GetComponentInChildren<EntrancePhaseMenuView>(true).Toggle();
                    Render(view,sim,width,height,Path.Combine(folder,"9-phase-menu.png"));
                    view.ClosePhaseMenu();
                }
                File.WriteAllText(Path.Combine(Evidence,"capture-result.txt"),"DREAM_ENTRANCE_CAPTURE_OK: 40 native Unity renders; fixture, not PlayMode.");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        internal static void Render(DreamEntranceView view, DreamForestSimulation sim, int width, int height, string file,bool paused=false)
        {
            var rt = new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            rt.Create();
            var previous = RenderTexture.active;
            try
            {
                view.RenderCamera.targetTexture = rt;
                Canvas.ForceUpdateCanvases();
                view.Apply(sim,paused,false,width,height);
                Canvas.ForceUpdateCanvases();
                view.RenderPainting();
                view.RenderCamera.Render();
                RenderTexture.active = rt;
                var texture = new Texture2D(width,height,TextureFormat.RGB24,false);
                texture.ReadPixels(new Rect(0,0,width,height),0,0);
                texture.Apply();
                File.WriteAllBytes(file,texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            finally
            {
                view.RenderCamera.targetTexture = null;
                RenderTexture.active = previous;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        [MenuItem("YunaLegend/Fase 6 - Jardim dos Sonhos/Entrada/Testes EditMode",false,61)]
        public static void RunTests()
        {
            runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            runner.RegisterCallbacks(new Results());
            runner.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "YunaLegend.DreamForestEntrance.Tests", "YunaLegend.DreamForest.Tests" }
            }));
        }

        public static void RunBatchValidation()
        {
            SessionState.SetBool("DreamEntranceAudit.Batch",true);
            RunTests();
        }

        public static void RunBatchInteractionValidation()
        {
            Capture();
            StartInteractionCapture();
        }

        private static void FinishBatchAfterTestCleanup()
        {
            if(EditorApplication.timeSinceStartup-batchWaitStarted>30)
            {
                EditorApplication.update-=FinishBatchAfterTestCleanup;
                EditorApplication.Exit(1);
                return;
            }
            var active=typeof(TestRunnerApi).GetMethod("IsRunActive",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
            if(active!=null && (bool)active.Invoke(null,null)) return;
            EditorApplication.update-=FinishBatchAfterTestCleanup;
            Capture();
            EditorApplication.Exit(0);
        }

        private sealed class Results : ICallbacks
        {
            public void RunStarted(ITestAdaptor test) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                Directory.CreateDirectory(Evidence);
                TestRunnerApi.SaveResultToFile(result,Path.Combine(Evidence,"editmode.xml"));
                File.WriteAllText(Path.Combine(Evidence,"test-result.txt"),
                    $"{result.TestStatus}: passed={result.PassCount}, failed={result.FailCount}");
                UnityEngine.Object.DestroyImmediate(runner);
                if (SessionState.GetBool("DreamEntranceAudit.Batch",false))
                {
                    if (result.FailCount>0)
                    {
                        EditorApplication.Exit(1);
                        return;
                    }
                    batchWaitStarted=EditorApplication.timeSinceStartup;
                    EditorApplication.update+=FinishBatchAfterTestCleanup;
                }
            }
        }
    }
}
