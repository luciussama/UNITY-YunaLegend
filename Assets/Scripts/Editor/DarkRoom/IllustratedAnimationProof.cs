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
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D;
using Object = UnityEngine.Object;

namespace YunaLegend.EditorScripts
{
    /// <summary>
    /// T02.3: a short animation-method proof and its measured cost. Only the isolated validation scene is
    /// changed; the Gameplay scene, the controller and the painted texture are preserved and hashed.
    /// The proof drives the real controller with normal InputSystem keyboard events.
    /// </summary>
    [InitializeOnLoad]
    public static class IllustratedAnimationProof
    {
        public const string ScenePath = "Assets/Scenes/Validation/IllustratedAnimationProof.unity";
        public const string MaterialPath = IllustratedYunaProof.Folder + "/Painting_Mesh_Unlit.mat";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        const string Controller = "Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs";
        const string Key = "IllustratedAnimationProof.Active";
        static string Output => Argument("-animationOutput", "Evidence/25d/T02.3");
        static string Frames => Argument("-animationFrames", "Build/25d/AnimationFrames");
        static int frame, lastFrame = -1, releaseAt = -1, landFrame = -1, booksFrame = -1;
        static bool initialized, firstJump, secondJump, bear, contactShot, passingShot, takeoffShot, riseShot, fallShot, landShot, recoveredShot, booksShot;
        static Keyboard keyboard;
        static InputSettings.BackgroundBehavior oldBackground;
        static InputSettings.EditorInputBehaviorInPlayMode oldEditor;
        static Vector3 rootStart;
        static Quaternion rotationStart;
        static Vector3 scaleStart;
        static Vector3 skinLocalStart;
        static Vector3 bodyLocalStart;
        static Vector3 controllerStartCenter;
        static float controllerStartHeight, controllerStartRadius;
        static readonly Report report = new Report();

        [Serializable] class Report
        {
            public string method = "Editor PlayMode normal KeyboardState events, unchanged physics and controller. One approved painting deformed as a proportional skin; camera is the approved stable A framing. Deformation only rewrites mesh vertices: root, collider and pivot are never moved. Legs do not articulate; drawn key poses remain a T03 decision input.";
            public string paintingHash, gameplayBefore, gameplayAfter, controllerBefore, controllerAfter;
            public bool sourceGameplayUnchanged, gameplayHadNoSkin, reachedBear, reachedBooks, visualAndColliderUnchanged, soleNeverCrossedFloor;
            public float travelDistance;
            public float pixelsPerUnit, textureMegabytes, minSoleLocalY, maxAbsLocalX, maxLocalY;
            public int textureWidth, textureHeight, meshColumns, meshRows, meshVertices, meshTriangles, skinUpdates, frameCount;
            public float skinAverageMilliseconds, skinMaxMilliseconds;
            public string estimateNote = "Frame-swap budget below is an estimate, not a measured render: one imported painting frame is 1024x1536 RGBA32.";
            public int runFramesEstimate = 12;
            public float runAtlasRawMegabytes, singleFrameRawMegabytes;
            public List<Sample> samples = new List<Sample>();
            public List<string> shots = new List<string>();
        }

        [Serializable] class Sample
        {
            public int frame, support;
            public string pose;
            public bool grounded;
            public float phase, lean, squashY, bodyCompression, minLocalY, minSoleLocalY;
            public Vector3 player;
        }

        static IllustratedAnimationProof()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode) EditorApplication.update += Tick;
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Key, false);
                    EditorApplication.Exit(SessionState.GetInt(Key + ".Exit", 0));
                }
            };
        }

        static string Argument(string key, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
        }

        static string Hash(string path)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }

        static Material PaintingMaterial(Texture2D texture)
        {
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) throw new InvalidOperationException("URP sprite shader missing.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            material.shader = shader;
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        /// <summary>Creates the isolated validation scene from the Gameplay scene, never saving Gameplay itself.</summary>
        public static void BuildScene()
        {
            var gameplayHash = Hash(Gameplay);
            EditorSceneManager.OpenScene(Gameplay);
            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            if (player == null) throw new InvalidOperationException("Player missing in Gameplay.");
            report.gameplayHadNoSkin = Object.FindObjectsByType<PaintedMeshDeformer>(FindObjectsSortMode.None).Length == 0;

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(IllustratedYunaProof.TexturePath);
            if (sprite == null) throw new InvalidOperationException("Run T02.1 first: approved painting is not imported.");
            var material = PaintingMaterial(sprite.texture);

            var oldBody = player.transform.Find("Body_Visual");
            if (oldBody != null) Object.DestroyImmediate(oldBody.gameObject);
            var body = new GameObject("Body_Visual");
            body.transform.SetParent(player.transform, false);
            var painted = new GameObject("Painted_Skin_PROOF");
            painted.transform.SetParent(body.transform, false);
            var skin = painted.AddComponent<PaintedMeshDeformer>();
            skin.sprite = sprite;
            skin.material = material;
            skin.columns = 12;
            skin.rows = 36;
            var animation = body.AddComponent<IllustratedPaintedAnimation>();
            animation.skin = skin;
            skin.Build(sprite, material);

            var camera = Camera.main;
            if (camera == null) throw new InvalidOperationException("Main camera missing.");
            camera.aspect = 16f / 9f;
            var follow = camera.GetComponent<SideScroll25DCamera>();
            if (follow == null) throw new InvalidOperationException("Stable camera missing.");
            follow.ConfigureIllustratedPilot(true);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes/Validation");
            EditorSceneManager.SaveScene(player.gameObject.scene, ScenePath);
            AssetDatabase.SaveAssets();
            if (Hash(Gameplay) != gameplayHash) throw new InvalidOperationException("Gameplay scene changed while building the proof.");
            report.gameplayBefore = gameplayHash;
            report.paintingHash = Hash(IllustratedYunaProof.TexturePath);
            report.pixelsPerUnit = sprite.pixelsPerUnit;
            report.textureWidth = sprite.texture.width;
            report.textureHeight = sprite.texture.height;
            report.textureMegabytes = Profiler.GetRuntimeMemorySizeLong(sprite.texture) / (1024f * 1024f);
            report.singleFrameRawMegabytes = report.textureWidth * (float)report.textureHeight * 4f / (1024f * 1024f);
            report.runAtlasRawMegabytes = report.singleFrameRawMegabytes * report.runFramesEstimate;
            Debug.Log("ILLUSTRATED_ANIMATION_SCENE_SAVED " + ScenePath);
        }

        public static void Run()
        {
            Directory.CreateDirectory(Output);
            Directory.CreateDirectory(Frames);
            BuildScene();
            SessionState.SetString(Key + ".Gameplay", report.gameplayBefore);
            SessionState.SetInt(Key + ".NoSkin", report.gameplayHadNoSkin ? 1 : 0);
            SessionState.SetString(Key + ".Controller", Hash(Controller));
            SessionState.SetBool(Key, true);
            SessionState.SetInt(Key + ".Exit", 0);
            EditorApplication.EnterPlaymode();
        }

        static void PressSpace()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(UnityEngine.InputSystem.Key.Space));
            releaseAt = frame + 9;
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            frame++;
            try
            {
                if (!initialized)
                {
                    initialized = true;
                    Time.captureDeltaTime = 1f / 60f;
                    oldBackground = InputSystem.settings.backgroundBehavior;
                    oldEditor = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>();
                }
                if (frame == releaseAt) InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
                if (player == null) throw new InvalidOperationException("Player missing.");
                var animation = Object.FindFirstObjectByType<IllustratedPaintedAnimation>();
                var skin = Object.FindFirstObjectByType<PaintedMeshDeformer>();
                if (animation == null || skin == null) throw new InvalidOperationException("Painted skin missing.");
                if (frame == 12)
                {
                    rootStart = player.transform.position;
                    rotationStart = player.transform.rotation;
                    scaleStart = player.transform.localScale;
                    skinLocalStart = skin.transform.localPosition;
                    bodyLocalStart = skin.transform.parent != null ? skin.transform.parent.localPosition : Vector3.zero;
                    var controller = player.GetComponent<CharacterController>();
                    if (controller != null)
                    {
                        controllerStartCenter = controller.center;
                        controllerStartHeight = controller.height;
                        controllerStartRadius = controller.radius;
                    }
                }
                if (frame == 20) Shot(player, "01-entry");
                if (!firstJump && player.transform.position.x >= -.64f) { firstJump = true; PressSpace(); }

                var phase = Mathf.Repeat(player.GaitPhase, 1f);
                bool running = player.AutoRunActive && !player.IsDead && Mathf.Abs(player.Velocity.x) > .01f;
                if (!contactShot && running && player.IsGrounded && frame > 24 && phase < .06f)
                { contactShot = true; Shot(player, "02-run-contact"); }
                if (contactShot && !passingShot && running && player.IsGrounded && phase > .24f && phase < .32f)
                { passingShot = true; Shot(player, "03-run-passing"); }
                if (!takeoffShot && !player.IsGrounded && player.Velocity.y > .2f)
                { takeoffShot = true; Shot(player, "04-takeoff"); }
                if (takeoffShot && !riseShot && !player.IsGrounded && player.Velocity.y > .2f && player.Velocity.y < 1.4f)
                { riseShot = true; Shot(player, "05-rise"); }
                if (takeoffShot && !fallShot && !player.IsGrounded && player.Velocity.y < -.2f)
                { fallShot = true; Shot(player, "06-fall"); }
                if (takeoffShot && !landShot && player.IsGrounded)
                { landShot = true; landFrame = frame; Shot(player, "07-land"); }
                if (landShot && !recoveredShot && frame >= landFrame + 10)
                { recoveredShot = true; Shot(player, "08-recovered"); }

                if (player.CurrentSupportIndex == 0 && !bear) { bear = true; report.reachedBear = true; }
                if (bear && !secondJump && player.transform.position.x >= 2.90f) { secondJump = true; PressSpace(); }
                if (player.CurrentSupportIndex == 1 && !booksShot)
                { booksShot = true; booksFrame = frame; report.reachedBooks = true; Shot(player, "09-books"); }
                if (frame % 2 == 0 && frame >= 18) SaveFrames(player, animation, skin);
                if (booksFrame >= 0 && frame >= booksFrame + 30) { Finish(player, skin); return; }
                if (player.IsDead || frame > 480) throw new InvalidOperationException("Animation proof traversal failed.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                File.WriteAllText(Path.Combine(Output, "failure.txt"), ex.ToString());
                SessionState.SetInt(Key + ".Exit", 1);
                Finish(Object.FindFirstObjectByType<Yuna25DPlayerController>(), Object.FindFirstObjectByType<PaintedMeshDeformer>());
            }
        }

        static void SaveFrames(Yuna25DPlayerController player, IllustratedPaintedAnimation animation, PaintedMeshDeformer skin)
        {
            Save(Camera.main, Path.Combine(Frames, "frame-" + frame.ToString("D5") + ".png"), 960, 540);
            if (frame % 4 != 0) return;
            report.frameCount++;
            report.samples.Add(new Sample
            {
                frame = frame,
                support = player.CurrentSupportIndex,
                pose = animation.CurrentPose.ToString(),
                grounded = player.IsGrounded,
                phase = animation.CurrentPhase,
                lean = skin.lean,
                squashY = skin.squashY,
                bodyCompression = skin.bodyCompression,
                minLocalY = skin.MinLocalY,
                minSoleLocalY = skin.MinSoleLocalY,
                player = player.transform.position
            });
        }

        static void Shot(Yuna25DPlayerController player, string name)
        {
            report.shots.Add(name);
            Save(Camera.main, Path.Combine(Output, name + ".png"), 1280, 720);
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

        static void Finish(Yuna25DPlayerController player, PaintedMeshDeformer skin)
        {
            EditorApplication.update -= Tick;
            // Static state is lost on the play-mode domain reload: build-time values come from SessionState.
            report.gameplayBefore = SessionState.GetString(Key + ".Gameplay", "");
            report.gameplayHadNoSkin = SessionState.GetInt(Key + ".NoSkin", 0) == 1;
            report.gameplayAfter = Hash(Gameplay);
            report.controllerBefore = SessionState.GetString(Key + ".Controller", "");
            report.controllerAfter = Hash(Controller);
            report.paintingHash = Hash(IllustratedYunaProof.TexturePath);
            report.sourceGameplayUnchanged = report.gameplayBefore.Length > 0
                && report.gameplayBefore == report.gameplayAfter && report.controllerBefore == report.controllerAfter;
            if (player != null)
            {
                report.travelDistance = Vector3.Distance(player.transform.position, rootStart);
                var controller = player.GetComponent<CharacterController>();
                bool rootStable = player.transform.rotation == rotationStart && player.transform.localScale == scaleStart;
                bool controllerStable = controller != null && controller.center == controllerStartCenter
                    && Mathf.Approximately(controller.height, controllerStartHeight)
                    && Mathf.Approximately(controller.radius, controllerStartRadius);
                bool pivotStable = skin != null && skin.transform.localPosition == skinLocalStart
                    && (skin.transform.parent == null || skin.transform.parent.localPosition == bodyLocalStart);
                report.visualAndColliderUnchanged = rootStable && controllerStable && pivotStable;
            }
            if (skin != null)
            {
                var sprite = skin.sprite;
                report.meshColumns = skin.columns;
                report.meshRows = skin.rows;
                report.meshVertices = skin.VertexCount;
                report.meshTriangles = skin.TriangleCount;
                report.skinUpdates = skin.UpdateCount;
                report.skinAverageMilliseconds = (float)skin.AverageUpdateMilliseconds;
                report.skinMaxMilliseconds = (float)skin.MaxUpdateMilliseconds;
                report.minSoleLocalY = skin.MinSoleLocalY;
                report.maxAbsLocalX = skin.MaxAbsLocalX;
                report.maxLocalY = skin.MaxLocalY;
                if (sprite != null)
                {
                    report.pixelsPerUnit = sprite.pixelsPerUnit;
                    report.textureWidth = sprite.texture.width;
                    report.textureHeight = sprite.texture.height;
                    report.textureMegabytes = Profiler.GetRuntimeMemorySizeLong(sprite.texture) / (1024f * 1024f);
                    report.singleFrameRawMegabytes = report.textureWidth * (float)report.textureHeight * 4f / (1024f * 1024f);
                    report.runAtlasRawMegabytes = report.singleFrameRawMegabytes * report.runFramesEstimate;
                }
                report.soleNeverCrossedFloor = true;
                foreach (var sample in report.samples) if (sample.minSoleLocalY < -.001f) report.soleNeverCrossedFloor = false;
            }
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = oldBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = oldEditor;
            Time.captureDeltaTime = 0f;
            File.WriteAllText(Path.Combine(Output, "animation-proof.json"), JsonUtility.ToJson(report, true));
            bool success = report.sourceGameplayUnchanged && report.gameplayHadNoSkin && report.reachedBear
                && report.reachedBooks && report.visualAndColliderUnchanged && report.soleNeverCrossedFloor
                && report.shots.Count >= 8 && report.frameCount > 40;
            if (!success) SessionState.SetInt(Key + ".Exit", 1);
            Debug.Log("ILLUSTRATED_ANIMATION_PROOF_DONE success=" + success
                + " skinMs=" + report.skinAverageMilliseconds.ToString("F3")
                + " verts=" + report.meshVertices + " shots=" + report.shots.Count);
            EditorApplication.ExitPlaymode();
        }
    }
}
