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
using YunaLegend.EditorScripts;
using YunaLegend.Illustrated25D;
using Object = UnityEngine.Object;

namespace YunaLegend.YunaIllustration.Editor
{
    /// <summary>
    /// T06.1 candidate: the real DarkRoom_Gameplay route, bear (index0) to books (index1),
    /// with the approved illustrated Yuna mounted on the real 3D controller.
    /// The gameplay scene is never saved: the illustrated visual exists only in this run.
    /// </summary>
    [InitializeOnLoad]
    public static class YunaT061TraversalCapture
    {
        const string Yuna = "Assets/Art/Illustrated25D/Yuna";
        const string Runtime = Yuna + "/Joint/Runtime";
        const string Frames = Runtime + "/Frames";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        const string Controller = "Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs";
        const string Key = "YunaT061Traversal.Active";

        static string Arg(string key, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
        }

        static int Hz => int.Parse(Arg("-t061Hz", "60"));
        static string Output => Arg("-t061Output", "Evidence/25d/T06.1/capture");
        static string Stills => Output + "/stills";
        static string FramesOut => Output + "/frames";

        static Yuna25DPlayerController player;
        static CharacterController capsule;
        static PaintedMeshDeformer idleSkin;
        static YunaIdleIllustration idle;
        static YunaRunIllustration run;
        static YunaJumpIllustration jump;
        static YunaStumbleIllustration stumble;
        static YunaIllustrationVisibilityResolver resolver;
        static Camera game;
        static Keyboard keyboard;
        static InputSettings.BackgroundBehavior oldBackground;
        static InputSettings.EditorInputBehaviorInPlayMode oldEditor;

        static Report report;
        static int frame, lastFrame, releaseAt = -1, booksFrame = -1, stopFrame = -1, sample;
        static bool firstJump, secondJump, bear, books, died;
        static Vector3 scaleStart, controllerStartCenter;
        static float controllerStartHeight, controllerStartRadius;
        static float minBearGap = 99f, maxBearGap = -99f, minBooksGap = 99f, maxBooksGap = -99f;
        static readonly HashSet<string> shotNames = new HashSet<string>();

        [Serializable]
        public sealed class Sample
        {
            public int frame;
            public int support;
            public string source;
            public bool grounded;
            public int runFrame;
            public int jumpPose;
            public float phase;
            public Vector3 position;
            public Vector3 velocity;
            public float supportGap;
            public int visibleSources;
        }

        [Serializable]
        public sealed class Report
        {
            public int hz;
            public int captureFps;
            public int hidden3DRenderers;
            public int frames;
            public bool success;
            public bool reachedBear;
            public bool reachedBooks;
            public bool died;
            public int jumpTakeoffs;
            public int jumpLandings;
            public int maxVisibleSources;
            public int visibilityConflicts;
            public int emptyComponentFrames;
            public bool resolverActive;
            public bool singleSourceVisible;
            public bool colliderUnchanged;
            public bool rootScaleUnchanged;
            public bool gameplayUnchanged;
            public bool controllerUnchanged;
            public float minBearGap;
            public float maxBearGap;
            public float minBooksGap;
            public float maxBooksGap;
            public string gameplayBefore;
            public string gameplayAfter;
            public string controllerBefore;
            public string controllerAfter;
            public List<Sample> samples = new List<Sample>();
            public List<string> shots = new List<string>();
        }

        static YunaT061TraversalCapture()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.update += Tick;
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Key, false);
                    EditorApplication.Exit(SessionState.GetInt(Key + ".Exit", 1));
                }
            };
        }

        public static void Run()
        {
            if (Hz != 30 && Hz != 60 && Hz != 120) throw new ArgumentException("t061Hz must be 30,60,120");
            Directory.CreateDirectory(Output);
            Directory.CreateDirectory(Stills);
            Directory.CreateDirectory(FramesOut);

            EditorSceneManager.OpenScene(Gameplay, OpenSceneMode.Single);
            var probe = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            if (probe == null || probe.GetComponent<CharacterController>() == null)
                throw new InvalidOperationException("Gameplay has no Yuna25DPlayerController with CharacterController.");
            if (Camera.main == null) throw new InvalidOperationException("Main camera missing.");

            SessionState.SetInt(Key + ".Hz", Hz);
            SessionState.SetString(Key + ".Gameplay", Hash(Gameplay));
            SessionState.SetString(Key + ".Controller", Hash(Controller));
            SessionState.SetBool(Key, true);
            SessionState.SetInt(Key + ".Exit", 1);
            Debug.Log("YUNA_T061_ARMED hz=" + Hz);
            EditorApplication.EnterPlaymode();
        }

        static void Initialize()
        {
            report = new Report
            {
                hz = SessionState.GetInt(Key + ".Hz", Hz),
                captureFps = SessionState.GetInt(Key + ".Hz", Hz),
                gameplayBefore = SessionState.GetString(Key + ".Gameplay", ""),
                controllerBefore = SessionState.GetString(Key + ".Controller", "")
            };
            player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            if (player == null) throw new InvalidOperationException("Player missing in play mode.");
            capsule = player.GetComponent<CharacterController>();
            game = Camera.main;
            if (capsule == null || game == null) throw new InvalidOperationException("Controller or camera missing in play mode.");

            // Prefer the persisted installer (the same one used by the playable build);
            // fall back to installing the visual only for this run.
            resolver = Object.FindFirstObjectByType<YunaIllustrationVisibilityResolver>();
            if (resolver != null)
            {
                idle = resolver.idle;
                idleSkin = idle != null ? idle.skin : null;
                run = resolver.run;
                jump = resolver.jump;
                stumble = resolver.stumble;
                report.hidden3DRenderers = -1; // hidden by the persisted installer
                if (run == null || jump == null || stumble == null)
                    throw new InvalidOperationException("Persisted installer is incomplete.");
            }
            else
            {
                // The persisted scene still carries the discarded 3D body: hide its renderers only.
                int hidden = 0;
                foreach (var renderer in player.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer is SpriteRenderer) continue;
                    renderer.enabled = false;
                    hidden++;
                }
                report.hidden3DRenderers = hidden;

                InstallIdle();
                InstallRun();
                InstallJump();
                InstallStumble();
                InstallResolver();
            }

            scaleStart = player.transform.localScale;
            controllerStartCenter = capsule.center;
            controllerStartHeight = capsule.height;
            controllerStartRadius = capsule.radius;

            Time.captureDeltaTime = 1f / report.captureFps;
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            Debug.Log("YUNA_T061_INSTALLED hz=" + report.hz + " hidden3D=" + report.hidden3DRenderers);
        }

        static void InstallIdle()
        {
            var body = new GameObject("T06_Idle_Visual");
            body.transform.SetParent(player.transform, false);
            idleSkin = body.AddComponent<PaintedMeshDeformer>();
            var paint = AssetDatabase.LoadAssetAtPath<Sprite>(Runtime + "/Yuna_Profile_Runtime_v1.png");
            var source = AssetDatabase.LoadAssetAtPath<Material>(Yuna + "/Idle/Idle_Painted.mat");
            var blink = AssetDatabase.LoadAssetAtPath<Texture2D>(Runtime + "/Yuna_Blink_Runtime_v1.png");
            if (paint == null || source == null || blink == null)
                throw new InvalidOperationException("Idle visual dependencies are missing.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(Runtime + "/Idle_Painted_Runtime.mat");
            if (material == null) throw new InvalidOperationException("Runtime idle material is missing.");
            material.SetTexture("_MainTex", paint.texture);
            material.SetTexture("_BlinkTex", blink);
            material.SetFloat("_Blink", 0f);
            idleSkin.Build(paint, material);
            idle = body.AddComponent<YunaIdleIllustration>();
            idle.skin = idleSkin;
            idle.player = player;
            idle.previewClock = false;
            idle.Evaluate(0f);
            var renderer = idleSkin.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;
        }

        static void InstallRun()
        {
            var frames = LoadFrames("Run");
            var body = new GameObject("T06_Run_Visual");
            body.transform.SetParent(player.transform, false);
            var renderer = body.AddComponent<SpriteRenderer>();
            var material = AssetDatabase.LoadAssetAtPath<Material>(Yuna + "/Run/Run_Painted.mat");
            if (material != null) renderer.sharedMaterial = material;
            renderer.sortingOrder = 60;
            renderer.sprite = frames[0];
            run = body.AddComponent<YunaRunIllustration>();
            run.player = player;
            run.painting = renderer;
            run.frames = frames;
            run.footPlant = true;
        }

        static void InstallJump()
        {
            var frames = LoadFrames("Jump");
            var body = new GameObject("T06_Jump_Visual");
            body.transform.SetParent(player.transform, false);
            var renderer = body.AddComponent<SpriteRenderer>();
            if (run.painting.sharedMaterial != null) renderer.sharedMaterial = run.painting.sharedMaterial;
            renderer.sortingOrder = 61;
            renderer.sprite = frames[7];
            renderer.enabled = false;
            jump = body.AddComponent<YunaJumpIllustration>();
            jump.player = player;
            jump.run = run;
            jump.painting = renderer;
            jump.frames = frames;
        }

        static void InstallStumble()
        {
            var frames = LoadFrames("Stumble");
            var body = new GameObject("T06_Stumble_Visual");
            body.transform.SetParent(player.transform, false);
            var renderer = body.AddComponent<SpriteRenderer>();
            var material = AssetDatabase.LoadAssetAtPath<Material>(Yuna + "/Stumble/Stumble_Painted.mat");
            if (material != null) renderer.sharedMaterial = material;
            renderer.sortingOrder = 62;
            renderer.sprite = frames[7];
            renderer.enabled = false;
            stumble = body.AddComponent<YunaStumbleIllustration>();
            stumble.player = player;
            stumble.run = run;
            stumble.painting = renderer;
            stumble.frames = frames;
        }

        static void InstallResolver()
        {
            resolver = player.GetComponent<YunaIllustrationVisibilityResolver>();
            if (resolver == null) resolver = player.gameObject.AddComponent<YunaIllustrationVisibilityResolver>();
            resolver.idle = idle;
            resolver.idleRenderer = idleSkin.GetComponent<MeshRenderer>();
            resolver.run = run;
            resolver.jump = jump;
            resolver.stumble = stumble;
            resolver.showIdle = false;
        }

        static Sprite[] LoadFrames(string action)
        {
            var frames = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(Frames + "/" + action + i + ".asset");
                if (frames[i] == null) throw new InvalidOperationException("Missing frame asset: " + action + i);
            }
            return frames;
        }

        static void PressSpace()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(UnityEngine.InputSystem.Key.Space));
            releaseAt = frame + Mathf.Max(2, Mathf.RoundToInt(.15f * report.captureFps));
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            try
            {
                if (player == null) Initialize();
                frame++;
                if (frame == releaseAt) InputSystem.QueueStateEvent(keyboard, new KeyboardState());

                if (frame == 10) Shot("01-start");
                if (!firstJump && player.transform.position.x >= -.64f) { firstJump = true; PressSpace(); }
                if (player.CurrentSupportIndex == 0 && !bear) { bear = true; report.reachedBear = true; Shot("02-bear-contact"); }
                if (bear && !secondJump && player.transform.position.x >= 2.90f - 5.2f / report.captureFps)
                {
                    secondJump = true;
                    PressSpace();
                    Shot("03-bear-takeoff");
                }
                if (secondJump && !player.IsGrounded && player.Velocity.y > .2f) Shot("04-rise");
                if (secondJump && !player.IsGrounded && player.Velocity.y < -.2f) Shot("05-fall");
                if (secondJump && player.IsGrounded && jump != null && jump.Pose >= 0 && jump.LandingContact) Shot("06-books-land");
                if (player.CurrentSupportIndex == 1 && !books)
                {
                    books = true;
                    booksFrame = frame;
                    // Brake on contact: the approved books platform is narrow and sloped.
                    player.AutoRunActive = false;
                    stopFrame = frame + Mathf.Max(1, Mathf.RoundToInt(.05f * report.captureFps));
                    report.reachedBooks = true;
                    Shot("07-books-contact");
                }
                if (stopFrame > 0 && frame == stopFrame)
                {
                    player.AutoRunActive = false;
                    Shot("09-books-stop");
                }
                if (books && frame >= booksFrame + 30) Shot("08-books-run");

                Record();
                if (frame % 2 == 0) SaveFrame();
                if (books && frame >= booksFrame + 40) { Finish(false); return; }
                if (player.IsDead || frame > report.captureFps * 8)
                {
                    died = player.IsDead;
                    throw new InvalidOperationException("T06.1 traversal failed: frame=" + frame
                        + " x=" + player.transform.position.x + " support=" + player.CurrentSupportIndex);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                File.WriteAllText(Path.Combine(Output, "failure.txt"), ex.ToString());
                Finish(true);
            }
        }

        static void Record()
        {
            if (frame % 2 != 0) return;
            sample++;
            var gap = SupportGap();
            if (!float.IsNaN(gap))
            {
                if (player.CurrentSupportIndex == 0) { minBearGap = Mathf.Min(minBearGap, gap); maxBearGap = Mathf.Max(maxBearGap, gap); }
                if (player.CurrentSupportIndex == 1) { minBooksGap = Mathf.Min(minBooksGap, gap); maxBooksGap = Mathf.Max(maxBooksGap, gap); }
            }
            else gap = 0f;
            report.samples.Add(new Sample
            {
                frame = frame,
                support = player.CurrentSupportIndex,
                source = resolver != null ? resolver.VisibleSource : "None",
                grounded = player.IsGrounded,
                runFrame = run != null ? run.CurrentFrame : -1,
                jumpPose = jump != null ? jump.Pose : -1,
                phase = run != null ? run.Phase : 0f,
                position = player.transform.position,
                velocity = player.Velocity,
                supportGap = gap,
                visibleSources = resolver != null ? resolver.VisibleCount : 0
            });
            if (resolver != null)
            {
                report.maxVisibleSources = Mathf.Max(report.maxVisibleSources, resolver.VisibleCount);
                report.visibilityConflicts = Mathf.Max(report.visibilityConflicts, resolver.ConflictingFrames);
                report.emptyComponentFrames = Mathf.Max(report.emptyComponentFrames, resolver.EmptyComponentFrames);
            }
        }

        static float SupportGap()
        {
            if (!player.IsGrounded) return float.NaN;
            RaycastHit hit;
            var origin = player.transform.position + Vector3.up * .4f;
            if (Physics.Raycast(origin, Vector3.down, out hit, 2.5f))
            {
                float bottom = player.transform.position.y + capsule.center.y - capsule.height * .5f;
                float gap = bottom - hit.point.y;
                return gap >= -.05f && gap <= .35f ? gap : float.NaN;
            }
            return float.NaN;
        }

        static void Shot(string name)
        {
            if (!shotNames.Add(name)) return;
            report.shots.Add(name);
            Save(game, Path.Combine(Stills, name + ".png"), 1280, 720);
        }

        static void SaveFrame()
        {
            Save(game, Path.Combine(FramesOut, "frame-" + frame.ToString("D4") + ".png"), 640, 360);
        }

        static void Save(Camera camera, string path, int width, int height)
        {
            float aspect = camera.aspect;
            camera.aspect = (float)width / height;
            var image = DarkRoomStartCapture.RenderDisplayImage(camera, width, height);
            File.WriteAllBytes(path, image.EncodeToPNG());
            Object.DestroyImmediate(image);
            camera.aspect = aspect;
        }

        static void Finish(bool failed)
        {
            EditorApplication.update -= Tick;
            report.frames = sample;
            report.died = died;
            report.jumpTakeoffs = jump != null ? jump.Takeoffs : 0;
            report.jumpLandings = jump != null ? jump.Landings : 0;
            report.resolverActive = resolver != null;
            report.singleSourceVisible = resolver != null && report.maxVisibleSources == 1;
            report.minBearGap = minBearGap;
            report.maxBearGap = maxBearGap;
            report.minBooksGap = minBooksGap;
            report.maxBooksGap = maxBooksGap;
            report.gameplayAfter = Hash(Gameplay);
            report.controllerAfter = Hash(Controller);
            report.gameplayUnchanged = report.gameplayBefore == report.gameplayAfter;
            report.controllerUnchanged = report.controllerBefore == report.controllerAfter;
            report.colliderUnchanged = capsule != null
                && capsule.center == controllerStartCenter && capsule.height == controllerStartHeight
                && capsule.radius == controllerStartRadius;
            report.rootScaleUnchanged = player != null && player.transform.localScale == scaleStart;
            report.success = !failed && !died && report.reachedBear && report.reachedBooks
                && report.jumpTakeoffs >= 2 && report.jumpLandings >= 2
                && report.singleSourceVisible && report.visibilityConflicts == 0
                && report.colliderUnchanged && report.rootScaleUnchanged
                && report.gameplayUnchanged && report.controllerUnchanged;
            File.WriteAllText(Path.Combine(Output, "traversal-report.json"), JsonUtility.ToJson(report, true));
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = oldBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = oldEditor;
            Time.captureDeltaTime = 0f;
            Debug.Log("YUNA_T061_DONE success=" + report.success + " hz=" + report.hz
                + " bear=" + report.reachedBear + " books=" + report.reachedBooks
                + " jumps=" + report.jumpTakeoffs + "/" + report.jumpLandings
                + " visibleMax=" + report.maxVisibleSources + " conflicts=" + report.visibilityConflicts);
            SessionState.SetInt(Key + ".Exit", report.success ? 0 : 1);
            EditorApplication.ExitPlaymode();
        }

        static string Hash(string path)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(File.ReadAllBytes(path));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}



