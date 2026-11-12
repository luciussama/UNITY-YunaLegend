using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using Object = UnityEngine.Object;

namespace YunaLegend.EditorScripts
{
    [InitializeOnLoad]
    public static class IllustratedStableCameraCapture
    {
        public const string ScenePath = "Assets/Scenes/Validation/IllustratedStableCamera.unity";
        const string Key = "IllustratedStableCameraCapture.Active";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        static string Output => Argument("-stableOutput", "Evidence/25d/T02.2b");
        static string Frames => Argument("-stableFrames", "Build/25d/StableCameraFrames");
        static int frame, lastFrame = -1, releaseAt = -1, booksFrame = -1;
        static bool initialized, firstJump, secondJump, bear;
        static Keyboard keyboard;
        static InputSettings.BackgroundBehavior oldBackground;
        static InputSettings.EditorInputBehaviorInPlayMode oldEditor;
        static readonly Report report = new Report();
        [Serializable] class Report
        {
            public string method = "Editor PlayMode normal KeyboardState events; unchanged physics and neutral painting. Perspective45, constant7.5m vertical span. Camera-only opt-in in isolated validation scene. Camera renders exclude IMGUI; not browser/device coverage.";
            public bool reachedBear, reachedBooks, sourceGameplayUnchanged;
            public string gameplayBefore, gameplayAfter, controllerBefore, controllerAfter;
            public float minHeightPixels = 10000, maxHeightPixels, minDepth = 10000, maxDepth;
            public List<Sample> samples = new List<Sample>();
        }
        [Serializable] class Sample
        {
            public int frame, support;
            public string label;
            public bool grounded, supportMeasured;
            public Vector3 player, camera;
            public float heightPixels, nextLandingX, nextLandingY, soleToSupport;
        }
        static IllustratedStableCameraCapture()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.update += Tick;
                if (state == PlayModeStateChange.EnteredEditMode)
                { SessionState.SetBool(Key, false); EditorApplication.Exit(SessionState.GetInt(Key + ".Exit", 0)); }
            };
        }
        static string Argument(string key, string fallback)
        { var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, key); return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback; }
        static string Hash(string path)
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant(); }
        public static void Run()
        {
            Directory.CreateDirectory(Output); Directory.CreateDirectory(Frames);
            SessionState.SetString(Key + ".Gameplay", Hash(Gameplay));
            SessionState.SetString(Key + ".Controller", Hash("Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs"));
            EditorSceneManager.OpenScene(IllustratedCameraComparison.ScenePath);
            var camera = Camera.main; camera.aspect = 16f / 9f;
            camera.GetComponent<SideScroll25DCamera>().ConfigureIllustratedPilot(true);
            EditorSceneManager.SaveScene(camera.gameObject.scene, ScenePath);
            SessionState.SetBool(Key, true); SessionState.SetInt(Key + ".Exit", 0);
            EditorApplication.EnterPlaymode();
        }
        static void PressSpace()
        { InputSystem.QueueStateEvent(keyboard, new KeyboardState(UnityEngine.InputSystem.Key.Space)); releaseAt = frame + 9; }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount; frame++;
            try
            {
                if (!initialized)
                {
                    initialized = true; Time.captureDeltaTime = 1f / 60f;
                    oldBackground = InputSystem.settings.backgroundBehavior; oldEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>();
                }
                if (frame == releaseAt) InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
                if (frame == 20) Capture(player, "01-entry");
                if (!firstJump && player.transform.position.x >= -.64f) { firstJump = true; PressSpace(); Capture(player, "02-first-takeoff"); }
                if (!bear && player.CurrentSupportIndex == 0 && player.IsGrounded)
                { bear = true; report.reachedBear = true; Capture(player, "03-bear"); }
                if (bear && !secondJump && player.transform.position.x >= 2.90f) { secondJump = true; PressSpace(); }
                if (booksFrame < 0 && player.CurrentSupportIndex == 1 && player.IsGrounded)
                { booksFrame = frame; report.reachedBooks = true; Capture(player, "04-books"); }
                if (frame % 6 == 0 && frame >= 18)
                {
                    Record(player, ""); Save(Camera.main, Path.Combine(Frames, "frame-" + frame.ToString("D5") + ".png"), 960, 540);
                }
                if (booksFrame >= 0 && frame >= booksFrame + 20) { Capture(player, "05-furniture-approach"); Finish(); return; }
                if (player.IsDead || frame > 480) throw new InvalidOperationException("Stable camera traversal failed.");
            }
            catch (Exception ex)
            { Debug.LogException(ex); File.WriteAllText(Path.Combine(Output, "failure.txt"), ex.ToString()); SessionState.SetInt(Key + ".Exit", 1); Finish(); }
        }
        static DarkRoomPlatformMarker Marker(int index)
        {
            foreach (var marker in Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsSortMode.None)) if (marker.spec.index == index) return marker;
            return null;
        }
        static void Record(Yuna25DPlayerController player, string label)
        {
            var camera = Camera.main; var root = player.transform.position;
            float height = Mathf.Abs(camera.WorldToViewportPoint(root + Vector3.up * 1.4f).y - camera.WorldToViewportPoint(root).y) * 720f;
            var sample = new Sample { frame = frame, support = player.CurrentSupportIndex, label = label, grounded = player.IsGrounded, player = root, camera = camera.transform.position, heightPixels = height };
            var next = Marker(player.CurrentSupportIndex < 0 ? 0 : player.CurrentSupportIndex + 1);
            if (next != null)
            {
                var landing = camera.WorldToViewportPoint(next.GetLandingPoint(player.RunDirection));
                sample.nextLandingX = landing.x; sample.nextLandingY = landing.y;
            }
            var current = Marker(player.CurrentSupportIndex);
            if (current != null && current.Support.TryGetPoint(root.x, out var contact))
            { sample.supportMeasured = true; sample.soleToSupport = root.y - contact.y; }
            report.minHeightPixels = Mathf.Min(report.minHeightPixels, height); report.maxHeightPixels = Mathf.Max(report.maxHeightPixels, height);
            report.minDepth = Mathf.Min(report.minDepth, Mathf.Abs(camera.transform.position.z)); report.maxDepth = Mathf.Max(report.maxDepth, Mathf.Abs(camera.transform.position.z));
            report.samples.Add(sample);
        }
        static void Save(Camera camera, string path, int width, int height)
        {
            float aspect = camera.aspect; camera.aspect = (float)width / height;
            var image = DarkRoomStartCapture.RenderDisplayImage(camera, width, height);
            File.WriteAllBytes(path, image.EncodeToPNG()); Object.DestroyImmediate(image); camera.aspect = aspect;
        }
        static void Capture(Yuna25DPlayerController player, string name)
        {
            Record(player, name);
            Save(Camera.main, Path.Combine(Output, name + ".png"), 1280, 720);
            Save(Camera.main, Path.Combine(Output, name + "-wide.png"), 1560, 720);
        }
        static void Finish()
        {
            EditorApplication.update -= Tick;
            report.gameplayBefore = SessionState.GetString(Key + ".Gameplay", ""); report.gameplayAfter = Hash(Gameplay);
            report.controllerBefore = SessionState.GetString(Key + ".Controller", ""); report.controllerAfter = Hash("Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs");
            report.sourceGameplayUnchanged = report.gameplayBefore == report.gameplayAfter && report.controllerBefore == report.controllerAfter;
            if (!report.reachedBear || !report.reachedBooks || !report.sourceGameplayUnchanged || report.maxHeightPixels - report.minHeightPixels > .1f)
                SessionState.SetInt(Key + ".Exit", 1);
            File.WriteAllText(Path.Combine(Output, "stable-camera.json"), JsonUtility.ToJson(report, true));
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldEditor; Time.captureDeltaTime = 0;
            Debug.Log("ILLUSTRATED_STABLE_CAMERA_DONE success=" + (SessionState.GetInt(Key + ".Exit", 0) == 0));
            EditorApplication.ExitPlaymode();
        }
    }
}
