using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Profiling;
using YunaLegend.DarkRoom;
using YunaLegend.EditorScripts;
using Object = UnityEngine.Object;

namespace YunaLegend.YunaIllustration.Editor
{
    [InitializeOnLoad]
    public static class YunaJumpReviewCapture
    {
        const string Folder = "Assets/Art/Illustrated25D/Yuna/Jump";
        const string Key = "YunaJumpReview.Active";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        const string Controller = "Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs";
        static string Arg(string key, string fallback) { var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, key); return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback; }
        static int Hz => int.Parse(Arg("-jumpHz", "60"));
        static string Output => Arg("-jumpOutput", "Evidence/25d/T03.3/capture-60");
        static string Frames => Arg("-jumpFrames", "Build/25d/Jump60");
        static string Hash(string path) { using (var s = SHA256.Create()) return BitConverter.ToString(s.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant(); }
        [Serializable] class Sample
        {
            public int frame, pose;
            public float t, x, y, vy, footX, footY, shift;
            public bool grounded, contact;
            public string state;
        }
        [Serializable] class Report
        {
            public string method = "Unity PlayMode, existing 3D controller and one InputSystem Space press on isolated flat technical ground. Capture 30fps; simulationHz separate. No production platform/contact, device or audio certification.";
            public string gameplayBefore, gameplayAfter, controllerBefore, controllerAfter, atlasHash;
            public int simulationHz, frames, takeoffs, landings;
            public float runSpeed, jumpHeight, atlasMiB;
            public bool success, colliderUnchanged = true, rootScaleUnchanged = true;
            public List<Sample> samples = new List<Sample>();
            public List<Sample> ticks = new List<Sample>();
        }
        static Report report;
        static Yuna25DPlayerController player;
        static YunaJumpIllustration animation;
        static CharacterController capsule;
        static Camera game, detail;
        static Keyboard keyboard;
        static InputSettings.BackgroundBehavior oldBackground;
        static InputSettings.EditorInputBehaviorInPlayMode oldEditor;
        static int lastFrame = -1, tick, sample;
        static Vector3 center, scale;
        static float height, radius, skin;
        static YunaJumpReviewCapture()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    report = new Report { gameplayBefore = SessionState.GetString(Key + ".Gameplay", ""), controllerBefore = Hash(Controller), simulationHz = Hz };
                    Time.captureDeltaTime = 1f / Hz; lastFrame = -1; tick = sample = 0;
                    EditorApplication.update -= Tick; EditorApplication.update += Tick;
                }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Key, false);
                    EditorApplication.Exit(SessionState.GetInt(Key + ".Exit", 1));
                }
            };
        }

        public static void Run()
        {
            if (Hz != 30 && Hz != 60 && Hz != 120) throw new ArgumentException("jumpHz must be 30,60,120");
            Directory.CreateDirectory(Output); Directory.CreateDirectory(Frames + "/game"); Directory.CreateDirectory(Frames + "/detail");
            string before = Hash(Gameplay);
            EditorSceneManager.OpenScene("Assets/Art/Illustrated25D/Yuna/Run/YunaRunReview.unity");
            var root = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            var run = Object.FindFirstObjectByType<YunaRunIllustration>();
            var importer = (TextureImporter)AssetImporter.GetAtPath(Folder + "/Yuna_Jump_Atlas_v1.png");
            importer.textureType = TextureImporterType.Default; importer.textureShape = TextureImporterShape.Texture2D;
            importer.npotScale = TextureImporterNPOTScale.None; importer.sRGBTexture = true; importer.alphaIsTransparency = true;
            importer.isReadable = false; importer.mipmapEnabled = false; importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(Folder + "/Yuna_Jump_Atlas_v1.png", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "/Yuna_Jump_Atlas_v1.png");
            int w = texture.width / 4, h = texture.height / 2;
            if (texture.width != 1448 || texture.height != 1086) throw new InvalidOperationException("Unexpected jump canvas");
            if (!AssetDatabase.IsValidFolder(Folder + "/Frames")) AssetDatabase.CreateFolder(Folder, "Frames");
            var sprites = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                string path = Folder + "/Frames/Jump" + i + ".asset";
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprites[i] == null)
                {
                    var rect = new Rect(i % 4 * w, texture.height - (i / 4 + 1) * h, w, h);
                    sprites[i] = Sprite.Create(texture, rect, new Vector2(536f / 1024f, (h - YunaJumpIllustration.Baseline[i]) / h), 1049.2857666f * w / 1024f, 0, SpriteMeshType.FullRect);
                    sprites[i].name = "Yuna_Jump_" + i; AssetDatabase.CreateAsset(sprites[i], path);
                }
            }
            var body = new GameObject("Jump_Visual_Candidate"); body.transform.SetParent(root.transform, false);
            var renderer = body.AddComponent<SpriteRenderer>(); renderer.sharedMaterial = run.painting.sharedMaterial; renderer.sprite = sprites[7]; renderer.enabled = false;
            var jump = body.AddComponent<YunaJumpIllustration>();
            jump.player = root; jump.run = run; jump.painting = renderer; jump.frames = sprites; jump.reviewSupport = run.reviewSupport;
            var camera = GameObject.Find("Run_GameCamera").GetComponent<Camera>(); camera.name = "Jump_GameCamera";
            camera.transform.position = new Vector3(0, 2.05f, -9.0533f);
            var close = GameObject.Find("Run_DetailCamera").GetComponent<Camera>(); close.name = "Jump_DetailCamera";
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Folder + "/YunaJumpReview.unity");
            if (Hash(Gameplay) != before) throw new InvalidOperationException("Gameplay changed");
            SessionState.SetString(Key + ".Gameplay", before); SessionState.SetBool(Key, true); SessionState.SetInt(Key + ".Exit", 1);
            EditorApplication.EnterPlaymode();
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            try
            {
                if (player == null)
                {
                    player = Object.FindFirstObjectByType<Yuna25DPlayerController>(); animation = Object.FindFirstObjectByType<YunaJumpIllustration>();
                    capsule = player.GetComponent<CharacterController>(); center = capsule.center; height = capsule.height; radius = capsule.radius; skin = capsule.skinWidth; scale = player.transform.localScale;
                    player.SetFloorDeathActive(false); player.AutoRunActive = true;
                    game = GameObject.Find("Jump_GameCamera").GetComponent<Camera>(); detail = GameObject.Find("Jump_DetailCamera").GetComponent<Camera>();
                    oldBackground = InputSystem.settings.backgroundBehavior; oldEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>();
                    report.runSpeed = player.RunSpeed; report.jumpHeight = player.JumpHeight;
                    report.atlasHash = Hash(Folder + "/Yuna_Jump_Atlas_v1.png");
                    report.atlasMiB = Profiler.GetRuntimeMemorySizeLong(animation.frames[0].texture) / (1024f * 1024f);
                }
                tick++;
                if (tick == Hz / 2) InputSystem.QueueStateEvent(keyboard, new KeyboardState(UnityEngine.InputSystem.Key.Space));
                if (tick == Hz / 2 + 3) InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                report.colliderUnchanged &= center == capsule.center && height == capsule.height && radius == capsule.radius && skin == capsule.skinWidth;
                report.rootScaleUnchanged &= scale == player.transform.localScale;
                animation.Refresh();
                report.ticks.Add(new Sample { frame = tick, t = tick / (float)Hz, pose = animation.Pose, state = animation.State,
                    x = player.transform.position.x, y = player.transform.position.y, vy = player.Velocity.y, grounded = player.IsGrounded,
                    contact = animation.LandingContact, footX = animation.ContactX, footY = animation.ContactY, shift = animation.VisualShift });
                if (tick % (Hz / 30) != 0) return;
                game.transform.position = new Vector3(player.transform.position.x, 2.05f, -9.0533f);
                detail.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + .74f, -2.172792f);
                report.samples.Add(new Sample { frame = sample, t = tick / (float)Hz, pose = animation.Pose, state = animation.State,
                    x = player.transform.position.x, y = player.transform.position.y, vy = player.Velocity.y, grounded = player.IsGrounded,
                    contact = animation.LandingContact, footX = animation.ContactX, footY = animation.ContactY, shift = animation.VisualShift });
                Save(game, 1280, 720, "game"); Save(detail, 640, 640, "detail");
                if (animation.Pose >= 0)
                {
                    string path = Output + "/pose-" + animation.Pose + ".png";
                    if (!File.Exists(path)) File.Copy(Frames + "/detail/" + sample.ToString("D3") + ".png", path);
                }
                if (++sample >= 66) Finish();
            }
            catch (Exception e) { Debug.LogException(e); Finish(true); }
        }
        static void Save(Camera camera, int w, int h, string group)
        {
            var image = DarkRoomStartCapture.RenderDisplayImage(camera, w, h);
            File.WriteAllBytes(Frames + "/" + group + "/" + sample.ToString("D3") + ".png", image.EncodeToPNG()); Object.DestroyImmediate(image);
        }
        static void Finish(bool failed = false)
        {
            EditorApplication.update -= Tick;
            report.frames = sample; report.takeoffs = animation != null ? animation.Takeoffs : 0; report.landings = animation != null ? animation.Landings : 0;
            report.gameplayAfter = Hash(Gameplay); report.controllerAfter = Hash(Controller);
            report.success = !failed && sample == 66 && report.takeoffs == 1 && report.landings == 1 && !player.IsDead
                && report.colliderUnchanged && report.rootScaleUnchanged && report.gameplayBefore == report.gameplayAfter && report.controllerBefore == report.controllerAfter;
            File.WriteAllText(Output + "/jump-proof.json", JsonUtility.ToJson(report, true));
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldEditor;
            Time.captureDeltaTime = 0;
            Debug.Log("YUNA_JUMP_REVIEW_DONE success=" + report.success);
            SessionState.SetInt(Key + ".Exit", report.success ? 0 : 1); EditorApplication.ExitPlaymode();
        }
    }
}
