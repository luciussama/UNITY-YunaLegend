using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using YunaLegend.DarkRoom;
using YunaLegend.EditorScripts;
using YunaLegend.Illustrated25D;
using YunaLegend.YunaIllustration;
using Object = UnityEngine.Object;

namespace YunaLegend.YunaIllustration.Editor
{
    /// <summary>Isolated stationary visual review; no traversal or foot-placement claim.</summary>
    [InitializeOnLoad]
    public static class YunaIdleReviewCapture
    {
        const string Folder = "Assets/Art/Illustrated25D/Yuna/Idle";
        const string Scene = Folder + "/YunaIdleReview.unity";
        const string Key = "YunaIdleReview.Active";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";
        static string Output => Arg("-idleOutput", "Evidence/25d/T03.1");
        static string Frames => Arg("-idleFrames", "Build/25d/IdleFrames");
        static int lastFrame = -1, warm, sample;
        static Vector3 rootPosition, rootScale, colliderCenter;
        static Quaternion rootRotation;
        static float colliderHeight, colliderRadius, minSole = float.MaxValue;
        static YunaIdleIllustration idle;
        static Yuna25DPlayerController player;
        static CharacterController collider;
        static Camera game, detail;
        static Report report = new Report();

        [Serializable] class Report
        {
            public string method = "Unity PlayMode, stationary isolated scene; 120 deterministic samples at 30 Hz using the runtime evaluator. Physical controller disabled only in this review fixture. No traversal, contact solver or device-performance coverage.";
            public string gameplayBefore, gameplayAfter, paintingHash, blinkHash;
            public bool rootUnchanged = true, colliderUnchanged = true, periodicLoop, success;
            public int frames, fullBlinkFrames;
            public int blinkTextureWidth, blinkTextureHeight;
            public float minMeshSoleLocalY, textureMemoryMiB, cameraHeightPixels;
            public string note = "The closed-eye generated image is sampled only inside a feathered eye rectangle. All other pixels and alpha come from the approved original. CPU/GPU timings not measured by this capture.";
        }

        static YunaIdleReviewCapture()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
                {
                    sample = warm = 0; lastFrame = -1;
                    report = new Report { gameplayBefore = SessionState.GetString(Key + ".Hash", "") };
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
            Directory.CreateDirectory(Output);
            Directory.CreateDirectory(Frames + "/game");
            Directory.CreateDirectory(Frames + "/detail");
            AssetDatabase.Refresh();
            var before = Hash(Gameplay);
            EditorSceneManager.OpenScene(IllustratedYunaProof.ScenePath);
            var root = Object.FindFirstObjectByType<Yuna25DPlayerController>();
            root.enabled = false;
            root.AutoRunActive = false;
            var old = root.transform.Find("Body_Visual");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var body = new GameObject("Yuna_Idle_Visual_Candidate");
            body.transform.SetParent(root.transform, false);
            var renderer = body.AddComponent<PaintedMeshDeformer>();
            var paint = AssetDatabase.LoadAssetAtPath<Sprite>(IllustratedYunaProof.TexturePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(Folder + "/Yuna_Blink_v1.png");
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.sRGBTexture = true; importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; importer.isReadable = false;
            importer.wrapMode = TextureWrapMode.Clamp; importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(Folder + "/Yuna_Blink_v1.png", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var blinkTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "/Yuna_Blink_v1.png");
            if (blinkTexture == null)
                throw new InvalidOperationException("Blink texture import missing: " + AssetDatabase.GetMainAssetTypeAtPath(Folder + "/Yuna_Blink_v1.png"));
            var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Idle_Painted.mat");
            if (material == null)
            {
                material = new Material(Shader.Find("YunaLegend/IllustratedIdleCandidate"));
                AssetDatabase.CreateAsset(material, Folder + "/Idle_Painted.mat");
            }
            material.SetTexture("_MainTex", paint.texture);
            material.SetTexture("_BlinkTex", blinkTexture);
            material.SetFloat("_Blink", 0f);
            renderer.Build(paint, material);
            var animation = body.AddComponent<YunaIdleIllustration>();
            animation.skin = renderer; animation.previewClock = true;
            animation.Evaluate(0);
            var camera = Camera.main;
            camera.name = "Approved_A_GameplayScale";
            camera.orthographic = false; camera.fieldOfView = 45;
            camera.transform.SetPositionAndRotation(new Vector3(0, .9f, -9.0533f), Quaternion.identity);
            camera.backgroundColor = new Color(.035f, .05f, .09f);
            var close = Object.Instantiate(camera.gameObject).GetComponent<Camera>();
            close.name = "DetailCamera"; close.enabled = false;
            close.transform.position = new Vector3(0, .74f, -2.172792f);
            var listener = close.GetComponent<AudioListener>();
            if (listener != null) Object.DestroyImmediate(listener);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Scene);
            if (Hash(Gameplay) != before) throw new InvalidOperationException("Gameplay changed.");
            SessionState.SetString(Key + ".Hash", before);
            SessionState.SetBool(Key, true);
            SessionState.SetInt(Key + ".Exit", 1);
            EditorApplication.EnterPlaymode();
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying || lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            try
            {
                if (++warm < 20) return;
                if (idle == null)
                {
                    idle = Object.FindFirstObjectByType<YunaIdleIllustration>();
                    idle.previewClock = false;
                    player = Object.FindFirstObjectByType<Yuna25DPlayerController>();
                    collider = player.GetComponent<CharacterController>();
                    rootPosition = player.transform.position; rootRotation = player.transform.rotation;
                    rootScale = player.transform.localScale; colliderCenter = collider.center;
                    colliderHeight = collider.height; colliderRadius = collider.radius;
                    game = GameObject.Find("Approved_A_GameplayScale").GetComponent<Camera>();
                    detail = GameObject.Find("DetailCamera").GetComponent<Camera>();
                    Time.captureDeltaTime = 1f / 30f;
                    report.paintingHash = Hash(IllustratedYunaProof.TexturePath);
                    report.blinkHash = Hash(Folder + "/Yuna_Blink_v1.png");
                    var blink = (Texture2D)idle.skin.material.GetTexture("_BlinkTex");
                    report.blinkTextureWidth = blink.width; report.blinkTextureHeight = blink.height;
                    report.textureMemoryMiB = (Profiler.GetRuntimeMemorySizeLong(idle.skin.sprite.texture)
                        + Profiler.GetRuntimeMemorySizeLong(idle.skin.material.GetTexture("_BlinkTex"))) / (1024f * 1024f);
                    report.cameraHeightPixels = 720f * 1.4f / 7.5f;
                    idle.Evaluate(0); var mesh = idle.skin.GetComponent<MeshFilter>().sharedMesh;
                    var initial = mesh.vertices;
                    idle.Evaluate(4); var final = mesh.vertices;
                    report.periodicLoop = initial.Length == final.Length;
                    for (int i = 0; i < initial.Length; i++)
                        report.periodicLoop &= (initial[i] - final[i]).sqrMagnitude < 1e-12f;
                }
                idle.Evaluate(sample / 30f);
                if (idle.Blink > .99f) report.fullBlinkFrames++;
                minSole = Mathf.Min(minSole, idle.skin.MinSoleLocalY);
                report.rootUnchanged &= player.transform.position == rootPosition
                    && player.transform.rotation == rootRotation && player.transform.localScale == rootScale;
                report.colliderUnchanged &= collider.center == colliderCenter
                    && collider.height == colliderHeight && collider.radius == colliderRadius;
                Save(game, 1280, 720, "game");
                Save(detail, 640, 640, "detail");
                if (sample == 0 || sample == 60 || sample == 69)
                {
                    File.Copy(Path.Combine(Frames, "game", sample.ToString("D3") + ".png"), Path.Combine(Output, "game-" + sample.ToString("D3") + ".png"), true);
                    File.Copy(Path.Combine(Frames, "detail", sample.ToString("D3") + ".png"), Path.Combine(Output, "detail-" + sample.ToString("D3") + ".png"), true);
                }
                if (++sample >= 120) Finish();
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
            report.frames = sample; report.minMeshSoleLocalY = minSole;
            report.gameplayAfter = Hash(Gameplay);
            report.success = !failed && sample == 120 && report.rootUnchanged && report.colliderUnchanged
                && report.periodicLoop && report.fullBlinkFrames > 0 && report.gameplayAfter == report.gameplayBefore
                && report.blinkTextureWidth == 1024 && report.blinkTextureHeight == 1536;
            File.WriteAllText(Path.Combine(Output, "idle-proof.json"), JsonUtility.ToJson(report, true));
            Debug.Log("YUNA_IDLE_REVIEW_DONE success=" + report.success);
            Time.captureDeltaTime = 0;
            SessionState.SetInt(Key + ".Exit", report.success ? 0 : 1);
            EditorApplication.ExitPlaymode();
        }
    }
}
