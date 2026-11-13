using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;
using YunaLegend.DarkRoom;
using YunaLegend.EditorScripts;
using YunaLegend.YunaIllustration;
using Object = UnityEngine.Object;

namespace YunaLegend.NandaIllustration.Editor
{
    /// <summary>
    /// T04.1 isolated review: eight painted wing poses, deterministic cadence at
    /// 30/60/120 Hz, both facings, and a rendered mirror probe on the standard
    /// sprite path. The T03 fixture supplies only the true gameplay-scale camera
    /// and a static Yuna for scale; it is never saved.
    /// </summary>
    [InitializeOnLoad]
    public static class NandaFlightReviewCapture
    {
        const string Nanda = "Assets/Art/Illustrated25D/Nanda";
        const string Folder = Nanda + "/Flight";
        const string Runtime = Folder + "/Runtime";
        const string Atlas = Folder + "/Nanda_Flight_Atlas_v1.png";
        const string Fixture = "Assets/Art/Illustrated25D/Yuna/Joint/YunaT03JointReview.unity";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        const string Guide = "Assets/Scripts/Runtime/DarkRoom/NandaFairyGuide.cs";
        const string Source = "Evidence/25d/T01/nanda-blue-v1.png";
        const string ReviewScene = Folder + "/NandaFlightReview.unity";
        const string Key = "NandaT04FlightReview.Active";
        const int CellWidth = 512;
        const int CellHeight = 592;
        const float PixelsPerUnit = 880f;
        const float PivotY = .42061f;
        const float SwitchAt = 2.4f;
        const float SwitchDuration = .45f;
        const float DetailDepth = 1.2f;
        const float NandaHeight = 1.55f;

        static string Arg(string key, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, key);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
        }

        static int Hz => int.Parse(Arg("-t04Hz", "60"), CultureInfo.InvariantCulture);
        static string CompressionMode => Arg("-t04Compression", "hq").ToLowerInvariant();
        static string Output => Arg("-t04Output", "Evidence/25d/T04.1/capture");
        static string Frames => Arg("-t04Frames", "Build/25d/T04Nanda");
        static float Duration => float.Parse(Arg("-t04Seconds", "4.8"), CultureInfo.InvariantCulture);

        static string Hash(string path)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }

        [Serializable] class PoseChange
        {
            public float t;
            public int pose;
            public float direction;
            public bool flipX;
        }

        [Serializable] class Sample
        {
            public int frame;
            public float t;
            public int pose;
            public float cyclePhase, hover, direction, x, y;
            public bool flipX;
            public bool matchesExpected;
        }

        [Serializable] class TextureUse
        {
            public string name, path, sourceHash, graphicsFormat, importSettings;
            public int width, height;
            public float runtimeMiB;
        }

        [Serializable] class Report
        {
            public string method = "Unity PlayMode, deterministic evaluation of the painted Nanda flight loop on the accepted T03 fixture camera. The fixture supplies the 7.5 m production vertical span and a static Yuna for scale only; no gameplay, guide, light, trail or scene file is modified. Captures at 15 fps; simulation rate reported separately. Orientation is verified by a rendered mirror probe on the default sprite path and by requiring flipX to match direction on every sample.";
            public string compressionMode, gameplayBefore, gameplayAfter, guideBefore, guideAfter;
            public string sourceBefore, sourceAfter, atlasBefore, atlasAfter;
            public int simulationHz, captureFps = 15, framesRun, poseChangesRecorded;
            public float duration, cycleSeconds, hoverAmplitude, hoverPeriod;
            public bool allPosesSeen, sawRight, sawLeft, flipHonored, poseCadenceExact;
            public bool sourcePngsUnchanged, sceneUntouched, guideUntouched, budgetPassed;
            public int poseMismatches, flipViolations, cyclesObserved;
            public string flipDiagnostic;
            public List<int> posesSeen = new List<int>();
            public float residentMiB, budgetMiB = 16f;
            public TextureUse texture;
            public List<PoseChange> poseChanges = new List<PoseChange>();
            public List<Sample> samples = new List<Sample>();
            public bool success;
        }

        static Report report;
        static NandaFlightIllustration nanda;
        static GameObject nandaRoot;
        static Transform nandaVisual;
        static Camera game, detail;
        static Sprite[] frames;
        static YunaIdleIllustration idleCompanion;
        static int lastFrame = -1, tick, sample, lastPose = -1;
        static bool initialized;

        static NandaFlightReviewCapture()
        {
            EditorApplication.playModeStateChanged += change =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (change == PlayModeStateChange.EnteredPlayMode)
                {
                    report = new Report
                    {
                        compressionMode = SessionState.GetString(Key + ".Compression", ""),
                        gameplayBefore = SessionState.GetString(Key + ".Gameplay", ""),
                        guideBefore = SessionState.GetString(Key + ".Guide", ""),
                        sourceBefore = SessionState.GetString(Key + ".Source", ""),
                        atlasBefore = SessionState.GetString(Key + ".Atlas", ""),
                        simulationHz = Hz,
                        duration = Duration,
                        cycleSeconds = .8f,
                        hoverAmplitude = .035f,
                        hoverPeriod = 2.4f
                    };
                    Time.captureDeltaTime = 1f / Hz;
                    lastFrame = -1; tick = sample = 0; lastPose = -1; initialized = false;
                    EditorApplication.update -= Tick;
                    EditorApplication.update += Tick;
                }
                if (change == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Key, false);
                    EditorApplication.Exit(SessionState.GetInt(Key + ".Exit", 1));
                }
            };
        }

        public static void Run()
        {
            if (Hz != 30 && Hz != 60 && Hz != 120) throw new ArgumentException("t04Hz must be 30,60,120");
            if (CompressionMode != "uncompressed" && CompressionMode != "hq")
                throw new ArgumentException("t04Compression must be uncompressed or hq");
            Directory.CreateDirectory(Output);
            Directory.CreateDirectory(Frames + "/game");
            Directory.CreateDirectory(Frames + "/detail");

            var gameplayBefore = Hash(Gameplay);
            var guideBefore = Hash(Guide);
            var sourceBefore = Hash(Source);
            var atlasBefore = Hash(Atlas);

            ApplyAtlasImport();
            frames = LoadFrames();

            EditorSceneManager.OpenScene(Fixture);
            game = GameObject.Find("Run_GameCamera").GetComponent<Camera>();
            detail = GameObject.Find("Run_DetailCamera").GetComponent<Camera>();
            if (game == null || detail == null) throw new InvalidOperationException("T03 fixture cameras are missing.");
            game.transform.position = new Vector3(0f, 2.05f, -9.0533f);
            game.transform.rotation = Quaternion.identity;
            game.fieldOfView = 45f;

            var player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            var idle = Object.FindFirstObjectByType<YunaIdleIllustration>();
            idleCompanion = idle;
            var run = Object.FindFirstObjectByType<YunaRunIllustration>();
            var jump = Object.FindFirstObjectByType<YunaJumpIllustration>();
            var stumble = Object.FindFirstObjectByType<YunaStumbleIllustration>();
            var resolver = Object.FindFirstObjectByType<YunaIllustrationVisibilityResolver>();
            if (player == null || idle == null) throw new InvalidOperationException("T03 fixture Yuna is missing.");

            // Freeze Yuna as a static scale companion. This is not T04.2 accompaniment.
            player.enabled = false;
            player.RunDirection = 1f;
            if (run != null) { run.enabled = false; if (run.painting != null) run.painting.enabled = false; }
            if (jump != null) { jump.enabled = false; if (jump.painting != null) jump.painting.enabled = false; }
            if (stumble != null) { stumble.enabled = false; if (stumble.painting != null) stumble.painting.enabled = false; }
            if (resolver != null) { resolver.enabled = false; resolver.showIdle = true; }
            if (idle.skin != null) idle.skin.GetComponent<MeshRenderer>().enabled = true;
            idle.previewClock = false;

            nandaRoot = new GameObject("Nanda_FlightReview");
            nandaRoot.transform.position = new Vector3(1.65f, NandaHeight, .15f);
            nandaVisual = new GameObject("Painting").transform;
            nandaVisual.SetParent(nandaRoot.transform, false);
            var renderer = nandaVisual.gameObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 20;
            nanda = nandaRoot.AddComponent<NandaFlightIllustration>();
            nanda.painting = renderer;
            nanda.visualRoot = nandaVisual;
            nanda.frames = frames;
            nanda.previewClock = false;
            nanda.SetDirection(1f);
            nanda.Evaluate(0f, 1f);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ReviewScene);

            if (Hash(Gameplay) != gameplayBefore || Hash(Guide) != guideBefore
                || Hash(Source) != sourceBefore || Hash(Atlas) != atlasBefore)
                throw new InvalidOperationException("A protected source changed while authoring the T04 review.");

            SessionState.SetString(Key + ".Gameplay", gameplayBefore);
            SessionState.SetString(Key + ".Guide", guideBefore);
            SessionState.SetString(Key + ".Source", sourceBefore);
            SessionState.SetString(Key + ".Atlas", atlasBefore);
            SessionState.SetString(Key + ".Compression", CompressionMode);
            SessionState.SetBool(Key, true);
            SessionState.SetInt(Key + ".Exit", 1);
            EditorApplication.EnterPlaymode();
        }

        static void ApplyAtlasImport()
        {
            var importer = AssetImporter.GetAtPath(Atlas) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing atlas importer: " + Atlas);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.spritePivot = new Vector2(.5f, PivotY);
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.maxTextureSize = 2048;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.sRGBTexture = true;
            importer.crunchedCompression = false;
            importer.textureCompression = CompressionMode == "hq"
                ? TextureImporterCompression.CompressedHQ
                : TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        static Sprite[] LoadFrames()
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Atlas);
            if (texture == null) throw new InvalidOperationException("Missing atlas texture: " + Atlas);
            if (texture.width != 2048 || texture.height != 1184)
                throw new InvalidOperationException("Unexpected atlas size: " + texture.width + "x" + texture.height);
            if (!AssetDatabase.IsValidFolder(Runtime + "/Frames")) AssetDatabase.CreateFolder(Runtime, "Frames");
            var result = new Sprite[8];
            for (int i = 0; i < result.Length; i++)
            {
                int left = i % 4 * CellWidth;
                int top = i / 4 * CellHeight;
                var rect = new Rect(left, texture.height - top - CellHeight, CellWidth, CellHeight);
                var authored = Sprite.Create(texture, rect, new Vector2(.5f, PivotY), PixelsPerUnit, 0, SpriteMeshType.FullRect);
                authored.name = "Nanda_Flight_" + i;
                var path = Runtime + "/Frames/NandaFlight" + i + ".asset";
                var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (existing == null) AssetDatabase.CreateAsset(authored, path);
                else
                {
                    EditorUtility.CopySerialized(authored, existing);
                    Object.DestroyImmediate(authored);
                    EditorUtility.SetDirty(existing);
                    authored = existing;
                }
                result[i] = authored;
            }
            AssetDatabase.SaveAssets();
            return result;
        }

        static void Initialize()
        {
            initialized = true;
            game = GameObject.Find("Run_GameCamera").GetComponent<Camera>();
            detail = GameObject.Find("Run_DetailCamera").GetComponent<Camera>();
            if (nanda == null) nanda = Object.FindFirstObjectByType<NandaFlightIllustration>();
            if (nanda == null) throw new InvalidOperationException("T04 review Nanda is missing.");
            if ((frames == null || frames.Length == 0) && nanda.frames != null) frames = nanda.frames;
            if (nandaRoot == null) nandaRoot = nanda.gameObject;
            if (nandaVisual == null && nanda.painting != null) nandaVisual = nanda.painting.transform;
            if (idleCompanion != null) idleCompanion.Evaluate(0f);
            report.cycleSeconds = nanda.cycleSeconds;
            report.hoverAmplitude = nanda.hoverAmplitude;
            report.hoverPeriod = nanda.hoverPeriod;
            report.posesSeen.Clear();
            MeasureTexture();
        }

        static void MeasureTexture()
        {
            var texture = nanda.frames[0].texture;
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            report.texture = new TextureUse
            {
                name = "nanda flight atlas",
                path = Atlas,
                sourceHash = Hash(Atlas),
                graphicsFormat = texture.graphicsFormat.ToString(),
                importSettings = (importer == null ? "unknown" : importer.textureCompression + " mipmaps=" + importer.mipmapEnabled
                    + " readable=" + importer.isReadable + " max=" + importer.maxTextureSize), 
                width = texture.width,
                height = texture.height,
                runtimeMiB = Profiler.GetRuntimeMemorySizeLong(texture) / (1024f * 1024f)
            };
            report.residentMiB = report.texture.runtimeMiB;
            report.budgetPassed = report.residentMiB <= report.budgetMiB;
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            try
            {
                if (!initialized) Initialize();
                tick++;
                float t = tick / (float)Hz;

                float direction = t < SwitchAt ? 1f : -1f;
                float progress = Mathf.Clamp01((t - SwitchAt) / SwitchDuration);
                float smooth = progress * progress * (3f - 2f * progress);
                float side = Mathf.Lerp(1.65f, -1.65f, smooth);
                float arc = Mathf.Sin(progress * Mathf.PI) * .22f;
                nandaRoot.transform.position = new Vector3(side, NandaHeight + arc, .15f);
                nanda.Evaluate(t, direction);

                if (nanda.Pose != lastPose)
                {
                    if (lastPose >= 0)
                    {
                        report.poseChanges.Add(new PoseChange
                        {
                            t = t, pose = nanda.Pose, direction = direction, flipX = nanda.FlipX
                        });
                        report.poseChangesRecorded++;
                    }
                    lastPose = nanda.Pose;
                }

                float phase = Mathf.Repeat(t / nanda.cycleSeconds, 1f);
                int expectedPose = Mathf.Min(nanda.FrameCount - 1, Mathf.FloorToInt(phase * nanda.FrameCount + .0001f));
                bool expected = expectedPose == nanda.Pose;
                if (!expected) report.poseMismatches++;
                if (!report.posesSeen.Contains(nanda.Pose)) report.posesSeen.Add(nanda.Pose);
                report.allPosesSeen = report.posesSeen.Count == 8;
                if (direction > 0f && !nanda.FlipX) report.sawRight = true;
                if (direction < 0f && nanda.FlipX) report.sawLeft = true;
                if ((direction > 0f && nanda.FlipX) || (direction < 0f && !nanda.FlipX)) report.flipViolations++;

                StepCams();
                if (tick % Mathf.RoundToInt(Hz / report.captureFps) != 0) return;
                Save(game, 1280, 720, "game");
                Save(detail, 640, 640, "detail");
                report.samples.Add(new Sample
                {
                    frame = sample, t = t, pose = nanda.Pose, cyclePhase = nanda.CyclePhase,
                    hover = nanda.HoverOffset, direction = direction, flipX = nanda.FlipX,
                    x = nandaRoot.transform.position.x, y = nandaRoot.transform.position.y,
                    matchesExpected = expected
                });
                if (++sample >= Mathf.RoundToInt(Duration * report.captureFps)) Finish();
            }
            catch (Exception e) { Debug.LogException(e); Finish(true); }
        }

        static void StepCams()
        {
            game.transform.position = new Vector3(0f, 2.05f, -9.0533f);
            detail.transform.position = new Vector3(nandaRoot.transform.position.x,
                nandaRoot.transform.position.y + .02f, nandaRoot.transform.position.z - DetailDepth);
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
            report.framesRun = sample;
            try
            {
                report.flipHonored = VerifyFlip(out report.flipDiagnostic);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                report.flipDiagnostic = e.Message;
                report.flipHonored = false;
            }
            int expectedChanges = nanda == null ? 0
                : Mathf.FloorToInt(Duration / nanda.cycleSeconds * nanda.FrameCount + .0001f);
            report.cyclesObserved = nanda == null ? 0 : Mathf.FloorToInt((Duration - 1f / Hz) / nanda.cycleSeconds);
            report.poseCadenceExact = report.poseMismatches == 0 && report.poseChangesRecorded == expectedChanges;
            report.gameplayAfter = Hash(Gameplay);
            report.guideAfter = Hash(Guide);
            report.sourceAfter = Hash(Source);
            report.atlasAfter = Hash(Atlas);
            report.sourcePngsUnchanged = report.atlasBefore == report.atlasAfter && report.sourceBefore == report.sourceAfter;
            report.sceneUntouched = report.gameplayBefore == report.gameplayAfter;
            report.guideUntouched = report.guideBefore == report.guideAfter;
            report.success = !failed
                && report.framesRun == Mathf.RoundToInt(Duration * report.captureFps)
                && report.allPosesSeen && report.sawRight && report.sawLeft
                && report.flipHonored && report.flipViolations == 0
                && report.poseCadenceExact && report.poseChangesRecorded == expectedChanges
                && report.poseMismatches == 0 && report.budgetPassed
                && report.sourcePngsUnchanged && report.sceneUntouched && report.guideUntouched;
            File.WriteAllText(Path.Combine(Output, "flight-proof.json"), JsonUtility.ToJson(report, true));
            Time.captureDeltaTime = 0f;
            Debug.Log("NANDA_T04_FLIGHT_DONE success=" + report.success + " poses=" + report.posesSeen.Count
                + " changes=" + report.poseChangesRecorded + " residentMiB=" + report.residentMiB.ToString("F4"));
            SessionState.SetInt(Key + ".Exit", report.success ? 0 : 1);
            EditorApplication.ExitPlaymode();
        }

        static bool VerifyFlip(out string diagnostic)
        {
            diagnostic = "";
            if (nanda == null || nanda.painting == null || frames == null || frames.Length == 0)
            {
                diagnostic = "missing painting or frames";
                return false;
            }
            const int Size = 320;
            var probe = new GameObject("T04_FlightFlipProbe");
            var cameraObject = new GameObject("T04_FlightFlipProbeCamera");
            var renderer = probe.AddComponent<SpriteRenderer>();
            var probeCamera = cameraObject.AddComponent<Camera>();
            var temporary = new List<Texture2D>();
            try
            {
                probe.transform.position = new Vector3(10000f, 0f, 0f);
                renderer.sharedMaterial = nanda.painting.sharedMaterial;
                renderer.enabled = true;
                probeCamera.enabled = false;
                probeCamera.orthographic = true;
                probeCamera.orthographicSize = .45f;
                probeCamera.nearClipPlane = .1f;
                probeCamera.farClipPlane = 20f;
                probeCamera.clearFlags = CameraClearFlags.SolidColor;
                probeCamera.backgroundColor = Color.black;
                probeCamera.cullingMask = ~0;
                probeCamera.allowHDR = false;
                probeCamera.allowMSAA = false;
                cameraObject.transform.position = probe.transform.position + Vector3.back * 5f;
                cameraObject.transform.rotation = Quaternion.identity;

                Texture2D reference = null;
                int bestAsymmetry = 0, bestPose = -1;
                for (int pose = 0; pose < frames.Length; pose++)
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
                    diagnostic = "no asymmetric flight frame (best=" + bestAsymmetry + ")";
                    return false;
                }

                File.WriteAllBytes(Path.Combine(Output, "flip-probe-right.png"), reference.EncodeToPNG());
                renderer.sprite = frames[bestPose];
                renderer.flipX = true;
                var flipped = DarkRoomStartCapture.RenderDisplayImage(probeCamera, Size, Size);
                temporary.Add(flipped);
                File.WriteAllBytes(Path.Combine(Output, "flip-probe-left.png"), flipped.EncodeToPNG());
                int error = MirrorDifference(reference, flipped);
                int tolerance = Mathf.Max(2000, bestAsymmetry / 8);
                diagnostic = "pose=" + bestPose + " asymmetry=" + bestAsymmetry
                    + " mirrorError=" + error + " tolerance=" + tolerance;
                return error <= tolerance;
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
                throw new InvalidOperationException("Flight mirror probe texture size changed.");
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
    }
}
