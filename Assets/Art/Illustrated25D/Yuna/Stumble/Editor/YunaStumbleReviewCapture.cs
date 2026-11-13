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
    public static class YunaStumbleReviewCapture
    {
        const string Folder = "Assets/Art/Illustrated25D/Yuna/Stumble";
        const string Key = "YunaStumbleReview.Active";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        const string Controller = "Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs";
        static string Arg(string key, string fallback) { var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, key); return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback; }
        static int Hz => int.Parse(Arg("-stumbleHz", "60"));
        static string Output => Arg("-stumbleOutput", "Evidence/25d/T03.3/capture-60");
        static string Frames => Arg("-stumbleFrames", "Build/25d/Stumble60");
        static string Hash(string path) { using (var s = SHA256.Create()) return BitConverter.ToString(s.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant(); }
        [Serializable] class Sample
        {
            public int frame, pose;
            public float t, x, y, vy, footX, footY, shift, delay, gait, elapsed;
            public bool grounded, contact, stumbling; public float recoveryRemaining, visualX, clock, timeScale;
            public string state;
        }
        [Serializable] class Report
        {
            public string method = "Unity PlayMode, existing 3D controller, BeginStumble .08/.28, full event, interrupted event reset, repeat on isolated flat technical ground. Capture 30fps; simulationHz separate. No production platform/contact, device or audio certification.";
            public string gameplayBefore, gameplayAfter, controllerBefore, controllerAfter, atlasHash;
            public int simulationHz, frames, events; public bool resetClean=true; public int resetStepCount; public float resetStepTimer; public bool timeScaleUnchanged=true, visualResetClean=true; public List<float> pauseDeltas = new List<float>();
            public float runSpeed, jumpHeight, atlasMiB;
            public bool success, colliderUnchanged = true, rootScaleUnchanged = true;
            public List<Sample> samples = new List<Sample>();
            public List<Sample> ticks = new List<Sample>();
        }
        static Report report;
        static Yuna25DPlayerController player;
        static YunaStumbleIllustration animation;
        static CharacterController capsule;
        static Camera game, detail;
        static Keyboard keyboard;
        static InputSettings.BackgroundBehavior oldBackground;
        static InputSettings.EditorInputBehaviorInPlayMode oldEditor;
        static int lastFrame = -1, tick, sample;
        static Vector3 center, scale;
        static float height, radius, skin;
        static YunaStumbleReviewCapture()
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
            if (Hz != 30 && Hz != 60 && Hz != 120) throw new ArgumentException("stumbleHz must be 30,60,120");
            Directory.CreateDirectory(Output); Directory.CreateDirectory(Frames + "/game"); Directory.CreateDirectory(Frames + "/detail");
            string before = Hash(Gameplay);
            EditorSceneManager.OpenScene("Assets/Art/Illustrated25D/Yuna/Run/YunaRunReview.unity");
            var root = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            var run = Object.FindFirstObjectByType<YunaRunIllustration>();
            var importer = (TextureImporter)AssetImporter.GetAtPath(Folder + "/Yuna_Stumble_Atlas_v1.png");
            importer.textureType = TextureImporterType.Default; importer.textureShape = TextureImporterShape.Texture2D;
            importer.npotScale = TextureImporterNPOTScale.None; importer.sRGBTexture = true; importer.alphaIsTransparency = true;
            importer.isReadable = false; importer.mipmapEnabled = false; importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(Folder + "/Yuna_Stumble_Atlas_v1.png", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "/Yuna_Stumble_Atlas_v1.png");
            int w = texture.width / 4, h = texture.height / 2;
            if (texture.width != 1774 || texture.height != 887) throw new InvalidOperationException("Unexpected stumble canvas");
            if (!AssetDatabase.IsValidFolder(Folder + "/Frames")) AssetDatabase.CreateFolder(Folder, "Frames");
            var sprites = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                string path = Folder + "/Frames/Stumble" + i + ".asset";
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (true)
                {
                    int left=Mathf.RoundToInt(i%4*texture.width/4f), right=Mathf.RoundToInt((i%4+1)*texture.width/4f);
                    int top=Mathf.RoundToInt(i/4*texture.height/2f), bottom=Mathf.RoundToInt((i/4+1)*texture.height/2f);
                    var rect = new Rect(left, texture.height-bottom, right-left, bottom-top);
                    var authored = Sprite.Create(texture, rect, new Vector2(.5f, (rect.height - YunaStumbleIllustration.Baseline[i]) / rect.height), 330f, 0, SpriteMeshType.FullRect);
                    authored.name = "Yuna_Stumble_" + i; if(sprites[i]==null) { sprites[i]=authored; AssetDatabase.CreateAsset(sprites[i],path); } else { EditorUtility.CopySerialized(authored,sprites[i]); Object.DestroyImmediate(authored); EditorUtility.SetDirty(sprites[i]); }
                }
            }
            var body = new GameObject("Stumble_Visual_Candidate"); body.transform.SetParent(root.transform, false);
            var renderer = body.AddComponent<SpriteRenderer>(); var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Stumble_Painted.mat");
            if(material==null) { material=new Material(Shader.Find("YunaLegend/IllustratedStumbleCandidate"));AssetDatabase.CreateAsset(material,Folder+"/Stumble_Painted.mat"); }
            renderer.sharedMaterial = material; renderer.sprite = sprites[7]; renderer.enabled = false;
            var stumble = body.AddComponent<YunaStumbleIllustration>();
            stumble.player = root; stumble.run = run; stumble.painting = renderer; stumble.frames = sprites; stumble.reviewSupport = run.reviewSupport;
            var camera = GameObject.Find("Run_GameCamera").GetComponent<Camera>(); camera.name = "Stumble_GameCamera";
            camera.transform.position = new Vector3(0, 2.05f, -9.0533f);
            var close = GameObject.Find("Run_DetailCamera").GetComponent<Camera>(); close.name = "Stumble_DetailCamera";
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Folder + "/YunaStumbleReview.unity");
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
                    player = Object.FindFirstObjectByType<Yuna25DPlayerController>(); animation = Object.FindFirstObjectByType<YunaStumbleIllustration>();
                    capsule = player.GetComponent<CharacterController>(); center = capsule.center; height = capsule.height; radius = capsule.radius; skin = capsule.skinWidth; scale = player.transform.localScale;
                    player.SetFloorDeathActive(false); player.AutoRunActive = true;
                    game = GameObject.Find("Stumble_GameCamera").GetComponent<Camera>(); detail = GameObject.Find("Stumble_DetailCamera").GetComponent<Camera>();
                    oldBackground = InputSystem.settings.backgroundBehavior; oldEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>();
                    report.runSpeed = player.RunSpeed; report.jumpHeight = player.JumpHeight;
                    report.atlasHash = Hash(Folder + "/Yuna_Stumble_Atlas_v1.png");
                    report.atlasMiB = Profiler.GetRuntimeMemorySizeLong(animation.frames[0].texture) / (1024f * 1024f);
                }
                tick++;
                if(tick==Hz/2 || tick==Hz*3/2 || tick==Hz*2) { player.BeginStumble(.08f,.28f); animation.SynchronizeEvent(); }
                if(tick==Hz*3/2+Hz/10)
                {
                    var position=player.transform.position; player.ResetPlayer(position); player.SetFloorDeathActive(false);player.AutoRunActive=true;
                    animation.ResetPresentation();
                    report.resetStepCount=(int)typeof(Yuna25DPlayerController).GetField("footstepCount",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(player);
                    report.resetStepTimer=(float)typeof(Yuna25DPlayerController).GetField("footstepTimer",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(player);
                    report.visualResetClean &= animation.Pose == -1 && !animation.painting.enabled && animation.Elapsed==0 && animation.VisualShift==0;
                    report.resetClean &= report.resetStepCount==0 && report.resetStepTimer==0 && !player.IsStumbling && player.StumbleDelayRemaining==0 && Mathf.Abs(player.GaitPhase-.5f)<.00001f;
                }
                if(player.StumbleDelayRemaining>0) report.pauseDeltas.Add(player.StumbleDelayRemaining);
                report.timeScaleUnchanged &= Time.timeScale==1f;
                report.colliderUnchanged &= center == capsule.center && height == capsule.height && radius == capsule.radius && skin == capsule.skinWidth;
                report.rootScaleUnchanged &= scale == player.transform.localScale;
                animation.Refresh();
                report.ticks.Add(new Sample { frame = tick, t = tick / (float)Hz, pose = animation.Pose, state = animation.State,
                    x = player.transform.position.x, y = player.transform.position.y, vy = player.Velocity.y, delay=player.StumbleDelayRemaining, gait=player.GaitPhase, elapsed=animation.Elapsed, grounded = player.IsGrounded,
                    stumbling=player.IsStumbling, recoveryRemaining=(float)typeof(Yuna25DPlayerController).GetField("stumblePose",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(player),
                    visualX=animation.Pose>=0 ? animation.transform.position.x : animation.run.transform.position.x, clock=Time.time, timeScale=Time.timeScale,
                    contact = animation.Pose>=0, footX = animation.ContactX, footY = animation.ContactY, shift = animation.VisualShift });
                if (tick % (Hz / 30) != 0) return;
                game.transform.position = new Vector3(player.transform.position.x, 2.05f, -9.0533f);
                detail.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + .74f, -2.172792f);
                report.samples.Add(new Sample { frame = sample, t = tick / (float)Hz, pose = animation.Pose, state = animation.State,
                    x = player.transform.position.x, y = player.transform.position.y, vy = player.Velocity.y, delay=player.StumbleDelayRemaining, gait=player.GaitPhase, elapsed=animation.Elapsed, grounded = player.IsGrounded,
                    stumbling=player.IsStumbling, recoveryRemaining=(float)typeof(Yuna25DPlayerController).GetField("stumblePose",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(player),
                    visualX=animation.Pose>=0 ? animation.transform.position.x : animation.run.transform.position.x, clock=Time.time, timeScale=Time.timeScale,
                    contact = animation.Pose>=0, footX = animation.ContactX, footY = animation.ContactY, shift = animation.VisualShift });
                Save(game, 1280, 720, "game"); Save(detail, 640, 640, "detail");
                if (animation.Pose >= 0)
                {
                    string path = Output + "/pose-" + animation.Pose + ".png";
                    if (!File.Exists(path)) File.Copy(Frames + "/detail/" + sample.ToString("D3") + ".png", path);
                }
                if (++sample >= 90) Finish();
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
            report.frames = sample; report.events = animation != null ? animation.Events : 0;
            report.gameplayAfter = Hash(Gameplay); report.controllerAfter = Hash(Controller);
            report.success = !failed && sample == 90 && report.events == 3 && report.resetClean && report.visualResetClean && report.timeScaleUnchanged && !player.IsDead
                && report.colliderUnchanged && report.rootScaleUnchanged && report.gameplayBefore == report.gameplayAfter && report.controllerBefore == report.controllerAfter;
            File.WriteAllText(Output + "/stumble-proof.json", JsonUtility.ToJson(report, true));
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldEditor;
            Time.captureDeltaTime = 0;
            Debug.Log("YUNA_STUMBLE_REVIEW_DONE success=" + report.success);
            SessionState.SetInt(Key + ".Exit", report.success ? 0 : 1); EditorApplication.ExitPlaymode();
        }
    }
}
