using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using YunaLegend.DarkRoom;
using Object = UnityEngine.Object;

namespace YunaLegend.EditorScripts
{
    /// <summary>Paired renders from one simulation. Projection is the sole A/B variable.</summary>
    [InitializeOnLoad]
    public static class IllustratedCameraComparison
    {
        public const string ScenePath = "Assets/Scenes/Validation/IllustratedCameraComparison.unity";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        const string Active = "IllustratedCameraComparison.Active";
        static string Output => Argument("-comparisonOutput", "Evidence/25d/T02.2");
        static string Frames => Argument("-comparisonFrames", "Build/25d/CameraFrames");
        static int frame, lastFrame = -1, releaseAt = -1, booksFrame = -1;
        static bool initialized, firstJump, secondJump, bear;
        static Keyboard keyboard;
        static InputSettings.BackgroundBehavior previousBackground;
        static InputSettings.EditorInputBehaviorInPlayMode previousEditor;
        static readonly Report report = new Report();
        [Serializable] class Report
        {
            public string method = "One PlayMode simulation, normal InputSystem keyboard. A/B rendered sequentially before next simulation update. Only projection differs: FOV45 versus orthographicSize=abs(camera.z)*tan(22.5deg), equivalent at running plane z0. Static approved Yuna painting; existing environment and fairy remain technical baseline, not final illustrated art.";
            public string gameplayBefore, gameplayAfter, textureHash;
            public bool reachedBear, reachedBooks, gameplayUnchanged, pairedTransformsIdentical = true;
            public int pairedFrames;
            public List<Sample> samples = new List<Sample>();
        }
        [Serializable] class Sample
        {
            public int frame, support;
            public string name;
            public Vector3 player, camera;
            public float fov, orthographicSize, playerPixelsA, playerPixelsB, spritePixelsA, spritePixelsB;
            public float landingLeftA, landingRightA, landingLeftB, landingRightB;
        }
        static IllustratedCameraComparison()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Active, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.update += Tick;
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Active, false);
                    EditorApplication.Exit(SessionState.GetInt(Active + ".Exit", 0));
                }
            };
        }
        static string Argument(string key, string fallback)
        {
            var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
        }
        static string Hash(string path)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }
        public static void Run()
        {
            Directory.CreateDirectory(Output);
            Directory.CreateDirectory(Path.Combine(Frames, "A"));
            Directory.CreateDirectory(Path.Combine(Frames, "B"));
            SessionState.SetString(Active + ".Hash", Hash(Gameplay));
            EditorSceneManager.OpenScene(Gameplay);
            var root = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            var oldBody = root.transform.Find("Body_Visual");
            Object.DestroyImmediate(oldBody.gameObject);
            var body = new GameObject("Body_Visual");
            body.transform.SetParent(root.transform, false);
            var painting = new GameObject("Sprite_Painting", typeof(SpriteRenderer), typeof(IllustratedCharacterFacing));
            painting.transform.SetParent(body.transform, false);
            var renderer = painting.GetComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(IllustratedYunaProof.TexturePath);
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(IllustratedYunaProof.Folder + "/Painting_Unlit.mat");
            if (renderer.sprite == null || renderer.sharedMaterial == null)
                throw new InvalidOperationException("Run T02.1 before comparing cameras.");
            // Save a separate validation scene; never overwrite Gameplay.
            EditorSceneManager.SaveScene(root.gameObject.scene, ScenePath);
            SessionState.SetBool(Active, true);
            SessionState.SetInt(Active + ".Exit", 0);
            EditorApplication.EnterPlaymode();
        }
        static void PressSpace()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            releaseAt = frame + 9;
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount; frame++;
            try
            {
                if (!initialized)
                {
                    initialized = true; Time.captureDeltaTime = 1f / 60f;
                    previousBackground = InputSystem.settings.backgroundBehavior;
                    previousEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>();
                }
                if (frame == releaseAt) InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
                if (player == null) throw new InvalidOperationException("Player missing.");
                if (frame == 20) CapturePair(player, "01-entry");
                if (!firstJump && player.transform.position.x >= -.64f)
                { firstJump = true; PressSpace(); }
                if (player.CurrentSupportIndex == 0 && !bear)
                { bear = true; report.reachedBear = true; CapturePair(player, "02-bear"); }
                if (bear && !secondJump && player.transform.position.x >= 2.90f)
                { secondJump = true; PressSpace(); }
                if (player.CurrentSupportIndex == 1 && booksFrame < 0)
                { booksFrame = frame; report.reachedBooks = true; CapturePair(player, "03-books"); }
                if (frame % 6 == 0 && frame >= 18) CaptureFrames();
                if (booksFrame >= 0 && frame >= booksFrame + 20)
                { Finish(); return; }
                if (player.IsDead || frame > 480)
                    throw new InvalidOperationException("Camera traversal did not reach books without death.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex); File.WriteAllText(Path.Combine(Output, "failure.txt"), ex.ToString());
                SessionState.SetInt(Active + ".Exit", 1); Finish();
            }
        }
        static void Configure(Camera camera, bool orthographic)
        {
            camera.orthographic = orthographic;
            camera.fieldOfView = 45f;
            camera.orthographicSize = Mathf.Abs(camera.transform.position.z) * Mathf.Tan(22.5f * Mathf.Deg2Rad);
        }
        static void Save(Camera camera, string path, int width, int height)
        {
            var image = DarkRoomStartCapture.RenderDisplayImage(camera, width, height);
            File.WriteAllBytes(path, image.EncodeToPNG()); Object.DestroyImmediate(image);
        }
        static float CharacterPixels(Camera camera, Vector3 root)
        {
            return Mathf.Abs(camera.WorldToViewportPoint(root + Vector3.up * 1.4f).y - camera.WorldToViewportPoint(root).y) * 720f;
        }
        static void CapturePair(Yuna25DPlayerController player, string name)
        {
            var camera = Camera.main;
            var position = camera.transform.position; var rotation = camera.transform.rotation;
            camera.aspect = 16f / 9f;
            var sample = new Sample { frame = frame, support = player.CurrentSupportIndex, name = name, player = player.transform.position, camera = position, fov = 45f };
            DarkRoomPlatformMarker next = null;
            foreach (var marker in Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsSortMode.None))
                if (marker.spec.index == Mathf.Max(0, player.CurrentSupportIndex + 1)) next = marker;
            for (int variant = 0; variant < 2; variant++)
            {
                Configure(camera, variant == 1);
                string label = variant == 0 ? "A-perspective" : "B-orthographic";
                Save(camera, Path.Combine(Output, name + "-" + label + ".png"), 1280, 720);
                float pixels = CharacterPixels(camera, player.transform.position);
                var sr = player.GetComponentInChildren<SpriteRenderer>();
                var center = sr.sprite.bounds.center;
                var leftPixel = camera.WorldToViewportPoint(sr.transform.TransformPoint(center - Vector3.right * sr.sprite.bounds.extents.x));
                var rightPixel = camera.WorldToViewportPoint(sr.transform.TransformPoint(center + Vector3.right * sr.sprite.bounds.extents.x));
                float spritePixels = Mathf.Abs(rightPixel.x - leftPixel.x) * 1280f;
                if (spritePixels < pixels * .4f) throw new InvalidOperationException("Sprite is edge-on; visual yaw compensation failed.");
                float left = next == null ? 0 : camera.WorldToViewportPoint(new Vector3(next.Support.MinX, next.GetTopCenterWorld().y, 0)).x;
                float right = next == null ? 0 : camera.WorldToViewportPoint(new Vector3(next.Support.MaxX, next.GetTopCenterWorld().y, 0)).x;
                if (variant == 0) { sample.playerPixelsA = pixels; sample.spritePixelsA = spritePixels; sample.landingLeftA = left; sample.landingRightA = right; }
                else { sample.playerPixelsB = pixels; sample.spritePixelsB = spritePixels; sample.landingLeftB = left; sample.landingRightB = right; sample.orthographicSize = camera.orthographicSize; }
                camera.aspect = 19.5f / 9f;
                Save(camera, Path.Combine(Output, name + "-" + label + "-wide.png"), 1560, 720);
                camera.aspect = 16f / 9f;
                report.pairedTransformsIdentical &= camera.transform.position == position && camera.transform.rotation == rotation;
            }
            Configure(camera, false); report.samples.Add(sample);
        }
        static void CaptureFrames()
        {
            var camera = Camera.main; camera.aspect = 16f / 9f;
            for (int variant = 0; variant < 2; variant++)
            {
                Configure(camera, variant == 1);
                Save(camera, Path.Combine(Frames, variant == 0 ? "A" : "B", "frame-" + frame.ToString("D5") + ".png"), 640, 360);
            }
            Configure(camera, false); report.pairedFrames++;
        }
        static void Finish()
        {
            EditorApplication.update -= Tick;
            report.gameplayBefore = SessionState.GetString(Active + ".Hash", "");
            report.gameplayAfter = Hash(Gameplay);
            report.gameplayUnchanged = report.gameplayBefore == report.gameplayAfter;
            report.textureHash = Hash(IllustratedYunaProof.TexturePath);
            if (!report.gameplayUnchanged || !report.pairedTransformsIdentical || !report.reachedBear || !report.reachedBooks)
                SessionState.SetInt(Active + ".Exit", 1);
            File.WriteAllText(Path.Combine(Output, "camera-comparison.json"), JsonUtility.ToJson(report, true));
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = previousBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditor;
            Time.captureDeltaTime = 0;
            Debug.Log("ILLUSTRATED_CAMERA_COMPARISON_DONE success=" + (SessionState.GetInt(Active + ".Exit", 0) == 0));
            EditorApplication.ExitPlaymode();
        }
    }
}
