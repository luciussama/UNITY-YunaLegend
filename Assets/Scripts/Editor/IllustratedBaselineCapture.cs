using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using YunaLegend.DarkRoom;

namespace YunaLegend.EditorScripts
{
    /// <summary>Source-baseline evidence. Drives normal InputSystem events, never menu callbacks.</summary>
    [InitializeOnLoad]
    public static class IllustratedBaselineCapture
    {
        const string SessionKey = "IllustratedBaselineCapture.Active";
        static string Output => Argument("-baselineOutput", "Evidence/25d/T00B");
        static string Frames => Argument("-baselineFrames", "Build/25d/BaselineFrames");
        static Keyboard keyboard;
        static int frame, lastFrame = -1, gameplayFrame = -1, releaseAt = -1, deadFrame = -1, retryFrame = -1;
        static bool initialized, firstJump, secondJump, reachedBear, reachedBooks, retryPressed;
        static InputSettings.BackgroundBehavior previousBackground;
        static InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
        static readonly Report report = new Report();
        [Serializable] class Report
        {
            public string method = "Unity Editor Play Mode: queued KeyboardState events through normal InputSystem/UI. No StartNewGame, SceneManager.LoadScene or retry callback invoked by harness. Saved opening quick-test setting preserved. Camera frames exclude IMGUI; screen captures are requested separately. Not manual browser/device coverage.";
            public bool menuEnteredGameplay, reachedBear, reachedBooks, gameOverObserved, retryObserved;
            public string titleSelection, uiModule;
            public int screenshotWidth, screenshotHeight;
            public List<Sample> samples = new List<Sample>();
        }
        [Serializable] class Sample
        {
            public int frame, support;
            public float time, timeScale;
            public string scene, input;
            public bool dead, gameOver;
            public Vector3 player;
        }
        static IllustratedBaselineCapture()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(SessionKey, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.update += Tick;
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(SessionKey, false);
                    EditorApplication.Exit(0);
                }
            };
        }
        static string Argument(string key, string fallback)
        {
            var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
        }
        public static void Run()
        {
            Directory.CreateDirectory(Output); Directory.CreateDirectory(Frames);
            EditorSceneManager.OpenScene("Assets/Scenes/TitleScreen.unity");
            SessionState.SetBool(SessionKey, true);
            EditorApplication.EnterPlaymode();
        }
        static void PressKey(Key key, int holdFrames = 3)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            releaseAt = frame + holdFrames;
            Record("press " + key);
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || Time.frameCount == lastFrame) return;
            lastFrame = Time.frameCount;
            try
            {
                if (!initialized)
                {
                    previousBackground = InputSystem.settings.backgroundBehavior;
                    previousEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>();
                    Time.captureDeltaTime = 1f / 60f;
                    initialized = true;
                }
                frame++;
                if (releaseAt == frame)
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    releaseAt = -1; Record("release");
                }
                string scene = SceneManager.GetActiveScene().name;
                if (frame == 18)
                {
                    report.titleSelection = EventSystem.current?.currentSelectedGameObject?.name;
                    report.uiModule = EventSystem.current?.currentInputModule?.GetType().Name;
                    Save("00-title");
                }
                if (frame == 24) PressKey(Key.Enter);
                if (scene == "TitleScreen" && frame > 100)
                    throw new InvalidOperationException("Menu did not respond to queued Enter. Selected=" + report.titleSelection + ", module=" + report.uiModule);
                if (scene == "DarkRoom_Gameplay")
                {
                    if (gameplayFrame < 0)
                    {
                        gameplayFrame = frame; report.menuEnteredGameplay = true; Save("01-gameplay-entry");
                    }
                    var player = UnityEngine.Object.FindFirstObjectByType<Yuna25DPlayerController>();
                    var manager = UnityEngine.Object.FindFirstObjectByType<DarkRoomProgressionManager>();
                    if (player == null || manager == null) return;
                    if (frame - gameplayFrame == 12) Save("02-start");
                    if (!firstJump && player.AutoRunActive && player.transform.position.x >= -.64f)
                    {
                        firstJump = true; PressKey(Key.Space, 9); Save("03-first-jump");
                    }
                    if (!reachedBear && player.CurrentSupportIndex == 0 && player.IsGrounded)
                    {
                        reachedBear = true; report.reachedBear = true; Save("04-on-bear");
                    }
                    if (reachedBear && !secondJump && player.transform.position.x >= 2.90f)
                    {
                        secondJump = true; PressKey(Key.Space, 9);
                    }
                    if (!reachedBooks && player.CurrentSupportIndex == 1 && player.IsGrounded)
                    {
                        reachedBooks = true; report.reachedBooks = true; Save("05-on-books");
                    }
                    // Deliberately send no third jump: capture the ordinary failure and retry path.
                    if (manager.IsGameOver && deadFrame < 0)
                    {
                        deadFrame = frame; report.gameOverObserved = true; Save("06-game-over");
                    }
                    if (deadFrame >= 0 && frame - deadFrame == 24) { retryPressed = true; PressKey(Key.Enter); }
                    if (retryPressed && !manager.IsGameOver && !player.IsDead && retryFrame < 0)
                    {
                        retryFrame = frame; report.retryObserved = true; Save("07-retry");
                    }
                    if (frame % 6 == 0)
                    {
                        SaveCamera(Path.Combine(Frames, "frame-" + frame.ToString("D5") + ".png"), 640, 360);
                        Record("");
                    }
                    if (retryFrame >= 0 && frame - retryFrame > 30)
                    {
                        if (!reachedBear || !reachedBooks) throw new InvalidOperationException("Baseline failed to reach bear/books before retry.");
                        Finish(); return;
                    }
                }
                if (frame > 1100) throw new TimeoutException("Baseline capture did not complete within simulated 18 seconds.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                File.WriteAllText(Path.Combine(Output, "failure.txt"), e.ToString());
                WriteReport(); Cleanup(); SessionState.SetBool(SessionKey, false); EditorApplication.Exit(1);
            }
        }
        static void Record(string input)
        {
            var player = UnityEngine.Object.FindFirstObjectByType<Yuna25DPlayerController>();
            var manager = UnityEngine.Object.FindFirstObjectByType<DarkRoomProgressionManager>();
            report.samples.Add(new Sample { frame = frame, time = frame / 60f, timeScale = Time.timeScale,
                scene = SceneManager.GetActiveScene().name, input = input,
                player = player != null ? player.transform.position : Vector3.zero,
                support = player != null ? player.CurrentSupportIndex : -1,
                dead = player != null && player.IsDead, gameOver = manager != null && manager.IsGameOver });
        }
        static void Save(string name)
        {
            Record("capture " + name);
            report.screenshotWidth = Screen.width; report.screenshotHeight = Screen.height;
            ScreenCapture.CaptureScreenshot(Path.Combine(Output, name + "-screen.png"));
            SaveCamera(Path.Combine(Output, name + "-camera.png"), 1280, 720);
        }
        static void SaveCamera(string path, int width, int height)
        {
            var camera = Camera.main;
            if (camera == null) return;
            float aspect = camera.aspect;
            camera.aspect = (float)width / height;
            var overlays = new List<Canvas>();
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    overlays.Add(canvas); canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera;
                }
            try
            {
                Canvas.ForceUpdateCanvases();
                var tex = DarkRoomStartCapture.RenderDisplayImage(camera, width, height);
                File.WriteAllBytes(path, tex.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(tex);
            }
            finally
            {
                foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
                camera.aspect = aspect;
            }
        }
        static void WriteReport() => File.WriteAllText(Path.Combine(Output, "input-and-traversal.json"), JsonUtility.ToJson(report, true));
        static void Cleanup()
        {
            EditorApplication.update -= Tick; Time.captureDeltaTime = 0;
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = previousBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInput;
        }
        static void Finish()
        {
            WriteReport(); Cleanup();
            Debug.Log("ILLUSTRATED_BASELINE_OK menu=True bear=True books=True death=True retry=True");
            EditorApplication.ExitPlaymode();
        }
    }
}
