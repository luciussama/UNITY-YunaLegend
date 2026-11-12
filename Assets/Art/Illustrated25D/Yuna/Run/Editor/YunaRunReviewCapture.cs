using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using YunaLegend.DarkRoom;
using YunaLegend.EditorScripts;
using YunaLegend.YunaIllustration;
using Object = UnityEngine.Object;

namespace YunaLegend.YunaIllustration.Editor
{
    [InitializeOnLoad]
    public static class YunaRunReviewCapture
    {
        const string Folder = "Assets/Art/Illustrated25D/Yuna/Run";
        const string Atlas = Folder + "/Yuna_Run_Atlas_v1.png";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        const string Controller = "Assets/Scripts/Runtime/DarkRoom/Yuna25DPlayerController.cs";
        const string Key = "YunaRunReview.Active";
        static string Output => Arg("-runOutput", "Evidence/25d/T03.2");
        static string Frames => Arg("-runFrames", "Build/25d/RunFrames");
        static int Hz => int.Parse(Arg("-runHz", "60"));
        // Frames per clip at the fixed 30 fps sampling. 128 frames cover 11.005 gait cycles, so the
        // GIF restarts on the same pose phase; the metre marks are respaced to divide the clip travel.
        static int Samples => int.Parse(Arg("-runSamples", "120"));
        static int lastFrame = -1, warm, sample, ticks;
        static bool sampling;
        static Yuna25DPlayerController player;
        static YunaRunIllustration animation;
        static CharacterController collider;
        static Camera game, detail;
        static Vector3 start, colliderCenter, rootScale;
        static float colliderHeight, colliderRadius, colliderSkin;
        static readonly bool[] seen = new bool[8];
        static Report report = new Report();

        [Serializable] class Sample
        {
            public int sample, pose;
            public float x, y, phase, drop, footX, footY;
            public float bodyShift, legShift, anchorCellWeight;
            public bool grounded, contact;
        }
        [Serializable] class Report
        {
            public string method = "Unity PlayMode on isolated flat technical ground; original controller active, autorun, no jump, normal gravity. GaitPhase also drives existing controller footsteps. Samples captures at 30 fps, simulationHz reported separately; clip length and ruler spacing chosen so the looping GIF restarts on the same pose phase and ground pattern. Sprite painting kept intact; contact uses visual-only root motion. No production scene change or platform-art validation.";
            public string gameplayBefore, gameplayAfter, controllerBefore, controllerAfter, atlasHash;
            public int frames, atlasWidth, atlasHeight, poseCount;
            public int simulationHz;
            public float runSpeed, footstepInterval, strideFromController, travel, atlasMiB, maxDrop, minContactY = float.MaxValue, maxContactY = float.MinValue;
            public float maxBodyShift, maxLegShift;
            public int clipSamples;
            public float markSpacing, clipTravel, cyclesPerClip, loopPhaseResidual;
            public bool colliderUnchanged = true, rootScaleUnchanged = true, stayedAlive, stayedGrounded = true, success;
            public string limitation = "A drawn contact-pose foot anchor is recorded in every sample: inspect world-X drift during the same stance, not just vertical coincidence. GIF has no audio. Device CPU/GPU and final platform contacts are not validated.";
            public List<Sample> samples = new List<Sample>();
        }

        static YunaRunReviewCapture()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
                {
                    sample = warm = ticks = 0; sampling = false; lastFrame = -1;
                    Array.Clear(seen, 0, seen.Length);
                    report = new Report { gameplayBefore = SessionState.GetString(Key + ".Gameplay", ""), controllerBefore = Hash(Controller), simulationHz = Hz };
                    Time.captureDeltaTime = 1f / Hz;
                    EditorApplication.update -= Tick;
                    EditorApplication.update += Tick;
                }
                if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key, false))
                {
                    SessionState.SetBool(Key, false);
                    EditorApplication.Exit(SessionState.GetInt(Key + ".Exit", 1));
                }
            };
        }

        static string Arg(string key, string fallback)
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

        public static void Run()
        {
            if (Hz != 30 && Hz != 60 && Hz != 120) throw new ArgumentException("runHz must be 30,60,120");
            Directory.CreateDirectory(Output);
            Directory.CreateDirectory(Frames + "/game"); Directory.CreateDirectory(Frames + "/detail");
            AssetDatabase.Refresh();
            var before = Hash(Gameplay);
            EditorSceneManager.OpenScene(IllustratedYunaProof.ScenePath);
            var root = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            root.enabled = true; root.AutoRunActive = true;
            var cc = root.GetComponent<CharacterController>();
            // Fixture placement only: authoring ground is flat, no mandatory route/death manager.
            cc.enabled = false; root.transform.position = new Vector3(0, .08f, 0); cc.enabled = true;
            var old = root.transform.Find("Body_Visual");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var ground = GameObject.Find("TechnicalSupport_NotPlatformArt");
            ground.transform.position = new Vector3(15, -.16f, .3f);
            ground.transform.localScale = new Vector3(70f, .32f, 1.2f);
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var markerMaterial = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/TechnicalMetreMarks.mat");
            if (markerMaterial == null)
            {
                markerMaterial = new Material(shader);
                AssetDatabase.CreateAsset(markerMaterial, Folder + "/TechnicalMetreMarks.mat");
            }
            markerMaterial.SetColor("_BaseColor", new Color(.28f, .37f, .48f));
            EditorUtility.SetDirty(markerMaterial);
            // Clip closure: the ruler is respaced so an integer number of marks fits exactly in the clip
            // travel. Otherwise the repeating GIF restarts with the ground pattern out of phase.
            float clipTravel = Samples * root.RunSpeed / 30f;
            int markCount = Mathf.Max(1, Mathf.RoundToInt(clipTravel));
            float markSpacing = clipTravel / markCount;
            report.clipSamples = Samples;
            report.clipTravel = clipTravel;
            report.markSpacing = markSpacing;
            report.cyclesPerClip = clipTravel / (2f * root.RunSpeed * root.FootstepInterval);
            for (int i = Mathf.FloorToInt(-15f / markSpacing); i <= Mathf.CeilToInt(50f / markSpacing); i++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = "TechnicalMetreMark_" + i;
                marker.transform.position = new Vector3(i * markSpacing, .002f, .3f);
                marker.transform.localScale = new Vector3(.02f, .004f, 1.15f);
                Object.DestroyImmediate(marker.GetComponent<Collider>());
                marker.GetComponent<Renderer>().sharedMaterial = markerMaterial;
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(Atlas);
            importer.textureType = TextureImporterType.Default; importer.textureShape = TextureImporterShape.Texture2D;
            importer.npotScale = TextureImporterNPOTScale.None; importer.sRGBTexture = true;
            importer.alphaIsTransparency = true; importer.isReadable = false; importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp; importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(Atlas, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Atlas);
            if (texture == null || texture.width % 4 != 0 || texture.height % 2 != 0) throw new InvalidOperationException("Invalid run atlas grid.");
            int width = texture.width / 4, height = texture.height / 2;
            float ppu = 1049.2857666f * width / 1024f;
            var sprites = new Sprite[8];
            if (!AssetDatabase.IsValidFolder(Folder + "/Frames")) AssetDatabase.CreateFolder(Folder, "Frames");
            for (int i = 0; i < 8; i++)
            {
                var path = Folder + "/Frames/Run" + i + ".asset";
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprites[i] == null)
                {
                    var rect = new Rect(i % 4 * width, texture.height - (i / 4 + 1) * height, width, height);
                    var pivot = new Vector2(536f / 1024f, (height - YunaRunIllustration.FloorY[i]) / height);
                    sprites[i] = Sprite.Create(texture, rect, pivot, ppu, 0, SpriteMeshType.FullRect);
                    sprites[i].name = "Yuna_Run_" + i;
                    AssetDatabase.CreateAsset(sprites[i], path);
                }
            }
            var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Run_Painted.mat");
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
                AssetDatabase.CreateAsset(material, Folder + "/Run_Painted.mat");
            }
            var body = new GameObject("Run_Visual_Candidate"); body.transform.SetParent(root.transform, false);
            var spriteRenderer = body.AddComponent<SpriteRenderer>();
            spriteRenderer.sharedMaterial = material; spriteRenderer.sprite = sprites[0];
            var run = body.AddComponent<YunaRunIllustration>();
            run.painting = spriteRenderer; run.frames = sprites; run.reviewSupport = ground.GetComponent<Collider>(); run.player = root;
            run.reviewFixtureAutoRun = true;
            run.footPlant = Arg("-runFootPlant", "1") == "1";
            var audio = root.GetComponent<AudioSource>(); if (audio != null) audio.mute = false;
            var camera = Camera.main; camera.name = "Run_GameCamera";
            camera.orthographic = false; camera.fieldOfView = 45;
            camera.transform.SetPositionAndRotation(new Vector3(0, .9f, -9.0533f), Quaternion.identity);
            camera.backgroundColor = new Color(.035f, .05f, .09f);
            var close = Object.Instantiate(camera.gameObject).GetComponent<Camera>();
            close.name = "Run_DetailCamera"; close.enabled = false;
            close.transform.position = new Vector3(0, .74f, -2.172792f);
            var listener = close.GetComponent<AudioListener>(); if (listener != null) Object.DestroyImmediate(listener);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Folder + "/YunaRunReview.unity");
            if (Hash(Gameplay) != before) throw new InvalidOperationException("Gameplay changed.");
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
                    player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
                    player.SetFloorDeathActive(false); player.AutoRunActive = true;
                    animation = Object.FindFirstObjectByType<YunaRunIllustration>(); collider = player.GetComponent<CharacterController>();
                    colliderCenter = collider.center; colliderHeight = collider.height; colliderRadius = collider.radius; colliderSkin = collider.skinWidth;
                    rootScale = player.transform.localScale;
                    game = GameObject.Find("Run_GameCamera").GetComponent<Camera>();
                    detail = GameObject.Find("Run_DetailCamera").GetComponent<Camera>();
                    var atlas = animation.frames[0].texture;
                    report.atlasWidth = atlas.width; report.atlasHeight = atlas.height;
                    report.atlasMiB = Profiler.GetRuntimeMemorySizeLong(atlas) / (1024f * 1024f);
                    report.atlasHash = Hash(Atlas); report.runSpeed = player.RunSpeed;
                    report.footstepInterval = player.FootstepInterval; report.strideFromController = 2f * player.RunSpeed * player.FootstepInterval;
                }
                if (++warm < Hz / 2) return;
                // Start on a pose boundary so the looping clip begins and ends on the same phase.
                if (!sampling)
                {
                    if (player.GaitPhase > .02f) return;
                    sampling = true;
                }
                if (++ticks % (Hz / 30) != 0) return;
                if (sample == 0) start = player.transform.position;
                animation.Refresh();
                game.transform.position = new Vector3(player.transform.position.x, .9f, -9.0533f);
                detail.transform.position = new Vector3(player.transform.position.x, .74f, -2.172792f);
                seen[animation.CurrentFrame] = true;
                report.maxDrop = Mathf.Max(report.maxDrop, animation.VisualDrop);
                report.maxBodyShift = Mathf.Max(report.maxBodyShift, Mathf.Abs(animation.BodyShift));
                report.maxLegShift = Mathf.Max(report.maxLegShift, Mathf.Abs(animation.LegShift));
                report.stayedGrounded &= player.IsGrounded;
                report.colliderUnchanged &= collider.center == colliderCenter && collider.height == colliderHeight
                    && collider.radius == colliderRadius && collider.skinWidth == colliderSkin;
                report.rootScaleUnchanged &= player.transform.localScale == rootScale;
                if (animation.ContactPose)
                {
                    report.minContactY = Mathf.Min(report.minContactY, animation.FootWorldY);
                    report.maxContactY = Mathf.Max(report.maxContactY, animation.FootWorldY);
                }
                report.samples.Add(new Sample { sample = sample, pose = animation.CurrentFrame,
                    x = player.transform.position.x, y = player.transform.position.y, phase = player.GaitPhase,
                    drop = animation.VisualDrop, footX = animation.FootWorldX, footY = animation.FootWorldY,
                    bodyShift = animation.BodyShift, legShift = animation.LegShift, anchorCellWeight = animation.AnchorCellWeight,
                    grounded = player.IsGrounded, contact = animation.ContactPose });
                Save(game, 1280, 720, "game"); Save(detail, 640, 640, "detail");
                if (!File.Exists(Path.Combine(Output, "pose-" + animation.CurrentFrame + ".png")))
                    File.Copy(Path.Combine(Frames, "detail", sample.ToString("D3") + ".png"), Path.Combine(Output, "pose-" + animation.CurrentFrame + ".png"));
                if (sample == 0 || sample == 60 || sample == 119)
                    File.Copy(Path.Combine(Frames, "game", sample.ToString("D3") + ".png"), Path.Combine(Output, "game-" + sample.ToString("D3") + ".png"), true);
                if (++sample >= Samples) Finish();
            }
            catch (Exception e) { Debug.LogException(e); Finish(true); }
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
            foreach (var pose in seen) if (pose) report.poseCount++;
            report.stayedAlive = player != null && !player.IsDead;
            report.travel = player != null ? player.transform.position.x - start.x : 0;
            float stride = 2f * report.runSpeed * report.footstepInterval;
            // The looping clip repeats after the last frame, so the loop covers Samples intervals of 1/30 s,
            // one more than the distance between the first and last captured sample.
            report.clipSamples = Samples;
            report.clipTravel = report.travel * Samples / Mathf.Max(1f, Samples - 1);
            report.markSpacing = report.clipTravel / Mathf.Max(1, Mathf.RoundToInt(report.clipTravel));
            float cyclesPerSample = stride > 0f ? report.travel / Mathf.Max(1f, Samples - 1) / stride : 0f;
            float loopCycles = cyclesPerSample * Samples;
            report.cyclesPerClip = loopCycles;
            report.loopPhaseResidual = Mathf.Abs(loopCycles - Mathf.Round(loopCycles));
            report.gameplayAfter = Hash(Gameplay); report.controllerAfter = Hash(Controller);
            report.success = !failed && sample == Samples && report.poseCount == 8 && report.stayedAlive && report.stayedGrounded
                && report.colliderUnchanged && report.rootScaleUnchanged && report.gameplayBefore == report.gameplayAfter
                && report.controllerBefore == report.controllerAfter && report.travel > 15f
                && report.loopPhaseResidual < .02f;
            File.WriteAllText(Path.Combine(Output, "run-proof.json"), JsonUtility.ToJson(report, true));
            Debug.Log("YUNA_RUN_REVIEW_DONE success=" + report.success);
            Time.captureDeltaTime = 0; SessionState.SetInt(Key + ".Exit", report.success ? 0 : 1);
            EditorApplication.ExitPlaymode();
        }
    }
}


