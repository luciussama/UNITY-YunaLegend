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
using YunaLegend.Illustrated25D;
using Object = UnityEngine.Object;

namespace YunaLegend.YunaIllustration.Editor
{
    /// <summary>T03 aggregate review: one deterministic state sequence and one resident-texture budget.</summary>
    [InitializeOnLoad]
    public static class YunaT03JointReviewCapture
    {
        const string Yuna = "Assets/Art/Illustrated25D/Yuna";
        const string Folder = Yuna + "/Joint";
        const string Runtime = Folder + "/Runtime";
        const string ScenePath = Folder + "/YunaT03JointReview.unity";
        const string Key = "YunaT03JointReview.Active";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        const string Controller = "Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs";
        const float IdleEnd = 2.5f, JumpRight = 4.0f, StumbleRight = 6.0f;
        const float Turn = 7.4f, JumpLeft = 9.0f, StumbleLeft = 11.0f;
        const float Stop = 12.6f, End = 14.0f;
        static string Arg(string key, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
        }
        static int Hz => int.Parse(Arg("-t03Hz", "60"));
        static string CompressionMode => Arg("-t03Compression", "hq").ToLowerInvariant();
        static string Output => Arg("-t03Output", "Evidence/25d/T03/capture");
        static string Frames => Arg("-t03Frames", "Build/25d/T03Joint");
        static string Hash(string path)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }

        [Serializable] class Sample
        {
            public int frame;
            public float t, x, y, vy, gait, runShift, jumpShift, stumbleShift, idleBlink;
            public int runPose, jumpPose, stumblePose;
            public bool grounded, idleVisible, runVisible, jumpVisible, stumbleVisible, stumbleFlipX;
            public int visibleSources;
            public string visibleSource;
            public string state;
            public float direction;
        }
        [Serializable] class TextureUse
        {
            public string name, path, sourceHash, graphicsFormat;
            public int width, height;
            public float runtimeMiB;
        }
        [Serializable] class Report
        {
            public string method = "Unity PlayMode, isolated flat technical ground, deterministic 14 s state sequence. Captures at 30 fps; simulationHz reported separately. Textures are compressed at import time without changing source PNG bytes. Stumble orientation is verified by a rendered mirror probe in both flip states and by requiring flipX to match RunDirection in both stumble events. No production platform, device, audio, mobile UI or gameplay integration claim.";
            public string compressionMode, gameplayBefore, gameplayAfter, controllerBefore, controllerAfter;
            public int simulationHz, captureFps = 30, frames;
            public float totalResidentMiB, budgetMiB = 32f;
            public bool budgetPassed, sourcePngsUnchanged = true, colliderUnchanged = true, rootScaleUnchanged = true;
            public bool sawIdle, sawIdleFacingCamera, sawRunRight, sawRunLeft, sawJump, sawStumble, directionChanged, stoppedToIdle;
            public bool sawStumbleFacingRight, sawStumbleFacingLeft, stumbleFlipHonored;
            public int idleToRunTransitions, runToIdleTransitions, jumpTakeoffs, jumpLandings, stumbleEvents;
            public bool singleSourceVisible = true, resolverActive;
            public int visibilityConflicts, componentOverlapFrames, emptyComponentFrames, maxVisibleSources;
            public string idleDiagnostic, stumbleFlipDiagnostic;
            public bool success;
            public List<TextureUse> textures = new List<TextureUse>();
            public List<Sample> samples = new List<Sample>();
        }

        static Report report;
        static Yuna25DPlayerController player;
        static YunaRunIllustration run;
        static YunaJumpIllustration jump;
        static YunaStumbleIllustration stumble;
        static YunaIdleIllustration idle;
        static YunaIllustrationVisibilityResolver resolver;
        static PaintedMeshDeformer idleSkin;
        static CharacterController capsule;
        static Camera game, detail;
        static Keyboard keyboard;
        static InputSettings.BackgroundBehavior oldBackground;
        static InputSettings.EditorInputBehaviorInPlayMode oldEditor;
        static int lastFrame = -1, tick, sample, jumpPressTicks, jumpReleaseTick = -1;
        static bool startedRun, turned, stopped, jumpRightDone, jumpLeftDone, stumbleRightDone, stumbleLeftDone;
        static Vector3 colliderCenter, rootScale;
        static float colliderHeight, colliderRadius, colliderSkin;

        static YunaT03JointReviewCapture()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    report = new Report
                    {
                        compressionMode = SessionState.GetString(Key + ".Compression", ""),
                        gameplayBefore = SessionState.GetString(Key + ".Gameplay", ""),
                        controllerBefore = Hash(Controller),
                        simulationHz = Hz
                    };
                    Time.captureDeltaTime = 1f / Hz;
                    lastFrame = -1; tick = sample = 0;
                    EditorApplication.update -= Tick;
                    EditorApplication.update += Tick;
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
            if (Hz != 30 && Hz != 60 && Hz != 120) throw new ArgumentException("t03Hz must be 30,60,120");
            if (CompressionMode != "uncompressed" && CompressionMode != "hq")
                throw new ArgumentException("t03Compression must be uncompressed or hq");
            Directory.CreateDirectory(Output);
            Directory.CreateDirectory(Frames + "/game");
            Directory.CreateDirectory(Frames + "/detail");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Yuna, "Joint");
            if (!AssetDatabase.IsValidFolder(Folder + "/Editor")) AssetDatabase.CreateFolder(Folder, "Editor");

            var before = Hash(Gameplay);
            var controllerBefore = Hash(Controller);
            var pngs = new Dictionary<string, string>
            {
                { Yuna + "/Yuna_Profile_v1.png", Hash(Yuna + "/Yuna_Profile_v1.png") },
                { Yuna + "/Idle/Yuna_Blink_v1.png", Hash(Yuna + "/Idle/Yuna_Blink_v1.png") },
                { Yuna + "/Run/Yuna_Run_Atlas_v1.png", Hash(Yuna + "/Run/Yuna_Run_Atlas_v1.png") },
                { Yuna + "/Jump/Yuna_Jump_Atlas_v1.png", Hash(Yuna + "/Jump/Yuna_Jump_Atlas_v1.png") },
                { Yuna + "/Stumble/Yuna_Stumble_Atlas_v1.png", Hash(Yuna + "/Stumble/Yuna_Stumble_Atlas_v1.png") },
                { Runtime + "/Yuna_Profile_Runtime_v1.png", Hash(Runtime + "/Yuna_Profile_Runtime_v1.png") },
                { Runtime + "/Yuna_Blink_Runtime_v1.png", Hash(Runtime + "/Yuna_Blink_Runtime_v1.png") }
            };

            if (Hash(Runtime + "/Yuna_Profile_Runtime_v1.png") != Hash(Yuna + "/Yuna_Profile_v1.png")
                || Hash(Runtime + "/Yuna_Blink_Runtime_v1.png") != Hash(Yuna + "/Idle/Yuna_Blink_v1.png"))
                throw new InvalidOperationException("Runtime profile/blink copies differ from approved sources.");

            ApplyImports();
            EditorSceneManager.OpenScene(Yuna + "/Run/YunaRunReview.unity");
            var root = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            var runAnimation = Object.FindFirstObjectByType<YunaRunIllustration>();
            if (root == null || runAnimation == null) throw new InvalidOperationException("Run fixture is missing its controller or illustration.");
            runAnimation.frames = LoadRuntimeFrames("Run", 362, 543, 1049.2857666f * 362f / 1024f);
            runAnimation.painting.sprite = runAnimation.frames[0];
            root.enabled = false; root.AutoRunActive = false; root.RunDirection = 1f;
            root.SetFloorDeathActive(false);
            var cc = root.GetComponent<CharacterController>();
            cc.enabled = false; root.transform.position = new Vector3(0f, .08f, 0f); cc.enabled = true;

            AddIdleVisual(root);
            AddJumpVisual(root, runAnimation);
            AddStumbleVisual(root, runAnimation);
            AddResolver(root, runAnimation);
            var gameCamera = GameObject.Find("Run_GameCamera").GetComponent<Camera>();
            gameCamera.transform.position = new Vector3(0f, 2.05f, -9.0533f);
            var detailCamera = GameObject.Find("Run_DetailCamera").GetComponent<Camera>();
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);

            if (Hash(Gameplay) != before || Hash(Controller) != controllerBefore)
                throw new InvalidOperationException("Gameplay or controller changed while authoring the joint fixture.");
            foreach (var pair in pngs)
                if (Hash(pair.Key) != pair.Value) throw new InvalidOperationException("Approved source PNG changed: " + pair.Key);
            SessionState.SetString(Key + ".Gameplay", before);
            SessionState.SetString(Key + ".Compression", CompressionMode);
            SessionState.SetBool(Key, true);
            SessionState.SetInt(Key + ".Exit", 1);
            EditorApplication.EnterPlaymode();
        }

        static void ApplyImports()
        {
            var mode = CompressionMode == "hq" ? TextureImporterCompression.CompressedHQ : TextureImporterCompression.Uncompressed;
            var paths = new[]
            {
                Runtime + "/Yuna_Profile_Runtime_v1.png",
                Runtime + "/Yuna_Blink_Runtime_v1.png",
                Runtime + "/Yuna_Run_Atlas_Runtime_v1.png",
                Runtime + "/Yuna_Jump_Atlas_Runtime_v1.png",
                Runtime + "/Yuna_Stumble_Atlas_Runtime_v1.png"
            };
            foreach (var path in paths)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer == null) throw new InvalidOperationException("Missing texture importer: " + path);
                bool profile = path.EndsWith("/Yuna_Profile_Runtime_v1.png", StringComparison.Ordinal);
                importer.textureType = profile ? TextureImporterType.Sprite : TextureImporterType.Default;
                importer.textureShape = TextureImporterShape.Texture2D;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.sRGBTexture = true;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.isReadable = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 2048;
                importer.crunchedCompression = false;
                importer.textureCompression = mode;
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        static void AddIdleVisual(Yuna25DPlayerController root)
        {
            var body = new GameObject("T03_Joint_Idle_Visual");
            body.transform.SetParent(root.transform, false);
            idleSkin = body.AddComponent<PaintedMeshDeformer>();
            var paint = AssetDatabase.LoadAssetAtPath<Sprite>(Runtime + "/Yuna_Profile_Runtime_v1.png");
            var source = AssetDatabase.LoadAssetAtPath<Material>(Yuna + "/Idle/Idle_Painted.mat");
            var blink = AssetDatabase.LoadAssetAtPath<Texture2D>(Runtime + "/Yuna_Blink_Runtime_v1.png");
            if (paint == null || source == null || blink == null) throw new InvalidOperationException("Idle visual dependencies are missing.");
            var materialPath = Runtime + "/Idle_Painted_Runtime.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(source) { name = "T03_Joint_Idle_Painted" };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else
            {
                material.CopyPropertiesFromMaterial(source);
            }
            material.SetTexture("_MainTex", paint.texture);
            material.SetTexture("_BlinkTex", blink);
            material.SetFloat("_Blink", 0f);
            EditorUtility.SetDirty(material);
            idleSkin.Build(paint, material);
            idle = body.AddComponent<YunaIdleIllustration>();
            idle.skin = idleSkin; idle.player = root; idle.previewClock = false;
            idle.Evaluate(0f);
        }

        static void AddJumpVisual(Yuna25DPlayerController root, YunaRunIllustration runAnimation)
        {
            var frames = LoadRuntimeFrames("Jump", 362, 543, 1049.2857666f * 362f / 1024f);
            var body = new GameObject("T03_Joint_Jump_Visual");
            body.transform.SetParent(root.transform, false);
            var renderer = body.AddComponent<SpriteRenderer>();
            renderer.sharedMaterial = runAnimation.painting.sharedMaterial;
            renderer.sprite = frames[7]; renderer.enabled = false;
            jump = body.AddComponent<YunaJumpIllustration>();
            jump.player = root; jump.run = runAnimation; jump.painting = renderer;
            jump.frames = frames; jump.reviewSupport = runAnimation.reviewSupport;
        }

        static void AddStumbleVisual(Yuna25DPlayerController root, YunaRunIllustration runAnimation)
        {
            var frames = LoadRuntimeFrames("Stumble", 0, 0, 330f);
            var body = new GameObject("T03_Joint_Stumble_Visual");
            body.transform.SetParent(root.transform, false);
            var renderer = body.AddComponent<SpriteRenderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Yuna + "/Stumble/Stumble_Painted.mat");
            renderer.sprite = frames[7]; renderer.enabled = false;
            stumble = body.AddComponent<YunaStumbleIllustration>();
            stumble.player = root; stumble.run = runAnimation; stumble.painting = renderer;
            stumble.frames = frames; stumble.reviewSupport = runAnimation.reviewSupport;
        }

        static void AddResolver(Yuna25DPlayerController root, YunaRunIllustration runAnimation)
        {
            resolver = root.GetComponent<YunaIllustrationVisibilityResolver>();
            if (resolver == null) resolver = root.gameObject.AddComponent<YunaIllustrationVisibilityResolver>();
            resolver.idle = idle;
            resolver.idleRenderer = idleSkin.GetComponent<MeshRenderer>();
            resolver.run = runAnimation;
            resolver.jump = jump;
            resolver.stumble = stumble;
            resolver.showIdle = true;
        }

        static Sprite[] LoadRuntimeFrames(string action, int cellWidth, int cellHeight, float ppu)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Runtime + "/Yuna_" + action + "_Atlas_Runtime_v1.png");
            if (texture == null) throw new InvalidOperationException("Missing runtime atlas: " + action);
            if (!AssetDatabase.IsValidFolder(Runtime + "/Frames")) AssetDatabase.CreateFolder(Runtime, "Frames");
            var frames = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                Rect rect; Vector2 pivot; float spritePpu;
                if (action == "Stumble")
                {
                    int left = Mathf.RoundToInt(i % 4 * 1774f / 4f);
                    int right = Mathf.RoundToInt((i % 4 + 1) * 1774f / 4f);
                    int top = Mathf.RoundToInt(i / 4 * 887f / 2f);
                    int bottom = Mathf.RoundToInt((i / 4 + 1) * 887f / 2f);
                    rect = new Rect(left, texture.height - bottom, right - left, bottom - top);
                    pivot = new Vector2(.5f, (rect.height - YunaStumbleIllustration.Baseline[i]) / rect.height);
                    spritePpu = ppu;
                }
                else
                {
                    int width = action == "Run" ? cellWidth : texture.width / 4;
                    int height = action == "Run" ? cellHeight : (texture.height - 2) / 2;
                    rect = new Rect(i % 4 * width, texture.height - (i / 4 + 1) * height, width, height);
                    if (action == "Run")
                        pivot = new Vector2(536f / 1024f, (height - YunaRunIllustration.FloorY[i]) / height);
                    else
                        pivot = new Vector2(536f / 1024f, (height - YunaJumpIllustration.Baseline[i]) / height);
                    spritePpu = action == "Run" ? ppu : 1049.2857666f * width / 1024f;
                }
                var path = Runtime + "/Frames/" + action + i + ".asset";
                var authored = Sprite.Create(texture, rect, pivot, spritePpu, 0, SpriteMeshType.FullRect);
                authored.name = "Yuna_T03_" + action + "_" + i;
                var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (existing == null) AssetDatabase.CreateAsset(authored, path);
                else
                {
                    EditorUtility.CopySerialized(authored, existing);
                    Object.DestroyImmediate(authored);
                    EditorUtility.SetDirty(existing);
                    authored = existing;
                }
                frames[i] = authored;
            }
            AssetDatabase.SaveAssets();
            return frames;
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            try
            {
                if (player == null) Initialize();
                tick++;
                float t = tick / (float)Hz;

                if (!startedRun && t >= IdleEnd) StartRun();
                if (startedRun && !jumpRightDone && t >= JumpRight) { PressJump(); jumpRightDone = true; }
                if (startedRun && !stumbleRightDone && t >= StumbleRight) { BeginStumble(); stumbleRightDone = true; }
                if (startedRun && !turned && t >= Turn) { player.RunDirection = -1f; turned = true; report.directionChanged = true; }
                if (startedRun && !jumpLeftDone && t >= JumpLeft) { PressJump(); jumpLeftDone = true; }
                if (startedRun && !stumbleLeftDone && t >= StumbleLeft) { BeginStumble(); stumbleLeftDone = true; }
                if (startedRun && !stopped && t >= Stop) StopToIdle();

                if (jumpPressTicks > 0 && --jumpPressTicks == 0) ReleaseJump();

                resolver.showIdle = !startedRun || stopped;
                run.Refresh();
                jump.Refresh();
                stumble.Refresh();
                if (stumble.painting.enabled && stumble.Pose >= 0)
                {
                    if (player.RunDirection > 0f && !stumble.painting.flipX) report.sawStumbleFacingRight = true;
                    if (player.RunDirection < 0f && stumble.painting.flipX) report.sawStumbleFacingLeft = true;
                }
                resolver.Resolve();
                if (!startedRun || stopped) idle.Evaluate(t - (stopped ? Stop : 0f));
                report.sawIdleFacingCamera |= stopped && IdleFacesCamera();

                if (stopped && string.IsNullOrEmpty(report.idleDiagnostic))
                    report.idleDiagnostic = IdleDiagnostic();

                int visibleSources = CountVisibleSources();
                report.maxVisibleSources = Mathf.Max(report.maxVisibleSources, visibleSources);
                if (visibleSources != 1)
                {
                    report.visibilityConflicts++;
                    report.singleSourceVisible = false;
                }

                report.colliderUnchanged &= colliderCenter == capsule.center && colliderHeight == capsule.height
                    && colliderRadius == capsule.radius && colliderSkin == capsule.skinWidth;
                report.rootScaleUnchanged &= rootScale == player.transform.localScale;
                report.sawIdle |= idleSkin.GetComponent<MeshRenderer>().enabled;
                report.sawRunRight |= run.painting.enabled && player.RunDirection > 0f;
                report.sawRunLeft |= run.painting.enabled && player.RunDirection < 0f;
                report.sawJump |= jump.Pose >= 0;
                report.sawStumble |= stumble.Pose >= 0;

                if (tick % (Hz / report.captureFps) != 0) return;
                game.transform.position = new Vector3(player.transform.position.x, 2.05f, -9.0533f);
                detail.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + .74f, -2.172792f);
                Save(game, 1280, 720, "game");
                Save(detail, 640, 640, "detail");
                report.samples.Add(new Sample
                {
                    frame = sample, t = t, x = player.transform.position.x, y = player.transform.position.y,
                    vy = player.Velocity.y, gait = player.GaitPhase, runShift = run.BodyShift,
                    jumpShift = jump.VisualShift, stumbleShift = stumble.VisualShift, idleBlink = idle.Blink,
                    runPose = run.CurrentFrame, jumpPose = jump.Pose, stumblePose = stumble.Pose,
                    grounded = player.IsGrounded, idleVisible = idleSkin.GetComponent<MeshRenderer>().enabled,
                    runVisible = run.painting.enabled, jumpVisible = jump.painting.enabled,
                    stumbleVisible = stumble.painting.enabled, stumbleFlipX = stumble.painting.flipX,
                    direction = player.RunDirection,
                    visibleSources = visibleSources, visibleSource = resolver.VisibleSource,
                    state = CurrentState()
                });
                if (++sample >= Mathf.RoundToInt(End * report.captureFps)) Finish();
            }
            catch (Exception e) { Debug.LogException(e); Finish(true); }
        }

        static void Initialize()
        {
            player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            run = Object.FindFirstObjectByType<YunaRunIllustration>();
            jump = Object.FindFirstObjectByType<YunaJumpIllustration>();
            stumble = Object.FindFirstObjectByType<YunaStumbleIllustration>();
            idle = Object.FindFirstObjectByType<YunaIdleIllustration>();
            idleSkin = idle.skin;
            resolver = Object.FindFirstObjectByType<YunaIllustrationVisibilityResolver>();
            if (resolver == null) throw new InvalidOperationException("Joint visibility resolver is missing.");
            capsule = player.GetComponent<CharacterController>();
            colliderCenter = capsule.center; colliderHeight = capsule.height;
            colliderRadius = capsule.radius; colliderSkin = capsule.skinWidth;
            rootScale = player.transform.localScale;
            game = GameObject.Find("Run_GameCamera").GetComponent<Camera>();
            detail = GameObject.Find("Run_DetailCamera").GetComponent<Camera>();
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            player.SetFloorDeathActive(false);
            player.AutoRunActive = false;
            run.painting.enabled = false;
            idleSkin.GetComponent<MeshRenderer>().enabled = true;
            resolver.showIdle = true;
            resolver.Resolve();
            MeasureTextures();
            report.stumbleFlipHonored = VerifyStumbleFlip(out report.stumbleFlipDiagnostic);
        }

        static bool VerifyStumbleFlip(out string diagnostic)
        {
            diagnostic = "";
            var material = stumble != null && stumble.painting != null ? stumble.painting.sharedMaterial : null;
            var frames = stumble != null ? stumble.frames : null;
            if (material == null || frames == null || frames.Length == 0)
            {
                diagnostic = "missing stumble material or frames";
                return false;
            }

            const int Size = 320;
            var probe = new GameObject("T03_StumbleFlipProbe");
            var cameraObject = new GameObject("T03_StumbleFlipProbeCamera");
            var renderer = probe.AddComponent<SpriteRenderer>();
            var probeCamera = cameraObject.AddComponent<Camera>();
            var temporary = new List<Texture2D>();
            try
            {
                probe.transform.position = new Vector3(10000f, 0f, 0f);
                renderer.sharedMaterial = material;
                renderer.enabled = true;

                probeCamera.enabled = false;
                probeCamera.orthographic = true;
                probeCamera.orthographicSize = .8f;
                probeCamera.nearClipPlane = .1f;
                probeCamera.farClipPlane = 20f;
                probeCamera.clearFlags = CameraClearFlags.SolidColor;
                probeCamera.backgroundColor = Color.black;
                probeCamera.cullingMask = ~0;
                probeCamera.allowHDR = false;
                probeCamera.allowMSAA = false;
                cameraObject.transform.position = probe.transform.position + Vector3.up * .65f + Vector3.back * 5f;
                cameraObject.transform.rotation = Quaternion.identity;

                Texture2D reference = null;
                int bestAsymmetry = 0, bestPose = -1;
                for (int pose = 0; pose < frames.Length && pose < 8; pose++)
                {
                    renderer.sprite = frames[pose];
                    renderer.flipX = false;
                    var unflipped = DarkRoomStartCapture.RenderDisplayImage(probeCamera, Size, Size);
                    temporary.Add(unflipped);
                    int asymmetry = MirrorDifference(unflipped, unflipped);
                    if (asymmetry > bestAsymmetry)
                    {
                        bestAsymmetry = asymmetry;
                        bestPose = pose;
                        reference = unflipped;
                    }
                }
                if (reference == null || bestAsymmetry < 10000)
                {
                    diagnostic = "no asymmetric stumble frame (best=" + bestAsymmetry + ")";
                    return false;
                }

                renderer.sprite = frames[bestPose];
                renderer.flipX = true;
                var flipped = DarkRoomStartCapture.RenderDisplayImage(probeCamera, Size, Size);
                temporary.Add(flipped);
                int error = MirrorDifference(reference, flipped);
                int tolerance = Mathf.Max(2000, bestAsymmetry / 8);
                bool honored = error <= tolerance;
                diagnostic = "pose=" + bestPose + " asymmetry=" + bestAsymmetry
                    + " mirrorError=" + error + " tolerance=" + tolerance;
                return honored;
            }
            finally
            {
                foreach (var texture in temporary) Object.DestroyImmediate(texture);
                Object.DestroyImmediate(probe);
                Object.DestroyImmediate(cameraObject);
            }
        }

        static int MirrorDifference(Texture2D reference, Texture2D mirrored)
        {
            if (reference.width != mirrored.width || reference.height != mirrored.height)
                throw new InvalidOperationException("Stumble mirror probe texture size changed.");
            int width = reference.width, height = reference.height;
            var left = reference.GetPixels32();
            var right = mirrored.GetPixels32();
            long sum = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    var a = left[y * width + x];
                    var b = right[y * width + (width - 1 - x)];
                    sum += Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) + Math.Abs(a.b - b.b);
                }
            return (int)Math.Min(sum, int.MaxValue);
        }

        static void MeasureTextures()
        {
            AddTexture("idle profile", Runtime + "/Yuna_Profile_Runtime_v1.png", idleSkin.sprite.texture);
            AddTexture("idle blink", Runtime + "/Yuna_Blink_Runtime_v1.png", idleSkin.material.GetTexture("_BlinkTex"));
            AddTexture("run atlas", Runtime + "/Yuna_Run_Atlas_Runtime_v1.png", run.frames[0].texture);
            AddTexture("jump atlas", Runtime + "/Yuna_Jump_Atlas_Runtime_v1.png", jump.frames[0].texture);
            AddTexture("stumble atlas", Runtime + "/Yuna_Stumble_Atlas_Runtime_v1.png", stumble.frames[0].texture);
            float total = 0f;
            foreach (var texture in report.textures) total += texture.runtimeMiB;
            report.totalResidentMiB = total;
            report.budgetPassed = total <= report.budgetMiB;
        }

        static void AddTexture(string name, string path, Texture texture)
        {
            if (texture == null) throw new InvalidOperationException("Missing runtime texture: " + name);
            report.textures.Add(new TextureUse
            {
                name = name, path = path, sourceHash = Hash(path),
                width = texture.width, height = texture.height,
                graphicsFormat = texture.graphicsFormat.ToString(),
                runtimeMiB = Profiler.GetRuntimeMemorySizeLong(texture) / (1024f * 1024f)
            });
        }

        static void StartRun()
        {
            startedRun = true;
            report.idleToRunTransitions++;
            idleSkin.GetComponent<MeshRenderer>().enabled = false;
            run.painting.enabled = true;
            player.enabled = true;
            player.SetFloorDeathActive(false);
            player.AutoRunActive = true;
        }

        static void StopToIdle()
        {
            stopped = true;
            report.runToIdleTransitions++;
            report.stoppedToIdle = true;
            player.AutoRunActive = false;
            run.painting.enabled = false;
            jump.painting.enabled = false;
            stumble.painting.enabled = false;
            idleSkin.GetComponent<MeshRenderer>().enabled = true;
        }

        static void PressJump()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(UnityEngine.InputSystem.Key.Space));
            jumpPressTicks = 3;
            jumpReleaseTick = tick + 3;
        }

        static void ReleaseJump()
        {
            if (keyboard != null) InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        }

        static void BeginStumble()
        {
            player.BeginStumble(.08f, .28f);
            stumble.SynchronizeEvent();
        }

        static string CurrentState()
        {
            if (!startedRun) return "Idle";
            if (stopped) return "IdleAfterRun";
            if (stumble.Pose >= 0) return "Stumble";
            if (jump.Pose >= 0) return "Jump";
            return player.RunDirection > 0f ? "RunRight" : "RunLeft";
        }

        static int CountVisibleSources()
        {
            return (idleSkin.GetComponent<MeshRenderer>().enabled ? 1 : 0)
                + (run.painting.enabled ? 1 : 0)
                + (jump.painting.enabled ? 1 : 0)
                + (stumble.painting.enabled ? 1 : 0);
        }

        static string IdleDiagnostic()
        {
            var renderer = idleSkin.GetComponent<MeshRenderer>();
            var filter = idleSkin.GetComponent<MeshFilter>();
            var mesh = filter.sharedMesh;
            var bounds = renderer.bounds;
            var planes = GeometryUtility.CalculateFrustumPlanes(game);
            var viewport = game.WorldToViewportPoint(bounds.center);
            var material = renderer.sharedMaterial;
            return "idleActiveInHierarchy=" + idleSkin.gameObject.activeInHierarchy
                + " rendererEnabled=" + renderer.enabled
                + " rendererVisible=" + renderer.isVisible
                + " forceRenderingOff=" + renderer.forceRenderingOff
                + " idleWorld=" + idleSkin.transform.position
                + " idleLocal=" + idleSkin.transform.localPosition
                + " parent=" + (idleSkin.transform.parent == null ? "null" : idleSkin.transform.parent.name)
                + " root=" + player.transform.position
                + " boundsCenter=" + bounds.center + " boundsSize=" + bounds.size
                + " frustum=" + GeometryUtility.TestPlanesAABB(planes, bounds)
                + " viewport=" + viewport
                + " mesh=" + (mesh == null ? "null" : mesh.vertexCount + " vertices bounds=" + mesh.bounds)
                + " material=" + (material == null ? "null" : material.name)
                + " mainTexture=" + (material == null || material.mainTexture == null ? "null" : material.mainTexture.name)
                + " camera=" + game.transform.position;
        }

        static bool IdleFacesCamera()
        {
            if (idleSkin == null || !idleSkin.gameObject.activeInHierarchy) return false;
            var renderer = idleSkin.GetComponent<MeshRenderer>();
            if (!renderer.enabled) return false;
            var bounds = renderer.bounds;
            var viewport = game.WorldToViewportPoint(bounds.center);
            return bounds.size.x > .25f && bounds.size.y > 1f
                && viewport.z > 0f && viewport.x > .05f && viewport.x < .95f
                && viewport.y > .05f && viewport.y < .95f;
        }

        static void Save(Camera camera, int width, int height, string group)
        {
            var image = DarkRoomStartCapture.RenderDisplayImage(camera, width, height);
            File.WriteAllBytes(Path.Combine(Frames, group, sample.ToString("D3") + ".png"), image.EncodeToPNG());
            Object.DestroyImmediate(image);
        }

        static void Finish(bool failed = false)
        {
            EditorApplication.update -= Tick;
            report.frames = sample;
            report.jumpTakeoffs = jump != null ? jump.Takeoffs : 0;
            report.jumpLandings = jump != null ? jump.Landings : 0;
            report.stumbleEvents = stumble != null ? stumble.Events : 0;
            if (resolver != null)
            {
                report.resolverActive = true;
                report.componentOverlapFrames = resolver.ComponentOverlapFrames;
                report.emptyComponentFrames = resolver.EmptyComponentFrames;
                report.maxVisibleSources = Mathf.Max(report.maxVisibleSources, resolver.VisibleCount);
            }
            report.gameplayAfter = Hash(Gameplay);
            report.controllerAfter = Hash(Controller);
            foreach (var texture in report.textures)
                report.sourcePngsUnchanged &= texture.sourceHash == Hash(texture.path);
            report.success = !failed && sample == Mathf.RoundToInt(End * report.captureFps)
                && report.budgetPassed && report.sourcePngsUnchanged
                && report.colliderUnchanged && report.rootScaleUnchanged
                && report.sawIdle && report.sawRunRight && report.sawRunLeft
                && report.sawIdleFacingCamera
                && report.sawJump && report.sawStumble && report.directionChanged && report.stoppedToIdle
                && report.sawStumbleFacingRight && report.sawStumbleFacingLeft && report.stumbleFlipHonored
                && report.idleToRunTransitions == 1 && report.runToIdleTransitions == 1
                && report.jumpTakeoffs == 2 && report.jumpLandings == 2 && report.stumbleEvents == 2
                && report.resolverActive && report.singleSourceVisible && report.visibilityConflicts == 0
                && report.maxVisibleSources == 1
                && !player.IsDead && report.gameplayBefore == report.gameplayAfter
                && report.controllerBefore == report.controllerAfter;
            File.WriteAllText(Path.Combine(Output, "joint-proof.json"), JsonUtility.ToJson(report, true));
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = oldBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = oldEditor;
            Time.captureDeltaTime = 0f;
            Debug.Log("YUNA_T03_JOINT_DONE success=" + report.success + " totalMiB=" + report.totalResidentMiB.ToString("F4"));
            SessionState.SetInt(Key + ".Exit", report.success ? 0 : 1);
            EditorApplication.ExitPlaymode();
        }
    }
}
