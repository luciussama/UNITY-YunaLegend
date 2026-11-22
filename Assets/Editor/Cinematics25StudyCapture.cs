using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using YunaLegend.ToyRoom.Runtime;

namespace YunaLegend.EditorScripts
{
    /// <summary>
    /// Disposable Play Mode study. It never creates or saves a scene asset.
    /// </summary>
    [InitializeOnLoad]
    public static class Cinematics25StudyCapture
    {
        private const string RequestedKey = "YunaLegend.Cinematics25.StudyRequested";
        private const int Width = 960;
        private const int Height = 540;
        private static readonly List<ExposureReceipt> Receipts = new List<ExposureReceipt>();
        private static readonly List<Texture2D> Captures = new List<Texture2D>();
        private static Camera camera;
        private static RenderTexture target;
        private static HandDrawnExposurePlayer player;
        private static string outputDirectory;
        private static double startTime;
        private static bool started;
        private static int expectedDrawings;

        [Serializable]
        private sealed class StudyManifest
        {
            public string output;
            public string[] frames;
        }

        [Serializable]
        private sealed class ExposureReceipt
        {
            public int drawing;
            public double realtimeSeconds;
            public int unityFrame;
        }

        [Serializable]
        private sealed class StudyReceipt
        {
            public string kind = "Unity Play Mode study, not completed cinematic";
            public int expectedDrawings;
            public int capturedDrawings;
            public int missedExposures;
            public int exposureRate = 25;
            public ExposureReceipt[] exposures;
        }

        static Cinematics25StudyCapture()
        {
            EditorApplication.playModeStateChanged += HandlePlayMode;
        }

        public static void Capture()
        {
            SessionState.SetBool(RequestedKey, true);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.playModeStartScene = null;
            EditorApplication.isPlaying = true;
        }

        private static void HandlePlayMode(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || started
                || !SessionState.GetBool(RequestedKey, false))
            {
                return;
            }

            started = true;
            try
            {
                BeginStudy();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                SessionState.SetBool(RequestedKey, false);
                EditorApplication.Exit(1);
            }
        }

        private static void BeginStudy()
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string manifestPath = Environment.GetEnvironmentVariable("YUNA_CIN_STUDY_MANIFEST");
            if (string.IsNullOrWhiteSpace(manifestPath))
            {
                manifestPath = Path.Combine(root, "Evidence/Cinematics25/study-manifest.json");
            }

            var manifest = JsonUtility.FromJson<StudyManifest>(File.ReadAllText(manifestPath));
            if (manifest.frames == null || manifest.frames.Length == 0)
            {
                throw new InvalidOperationException("A study manifest must identify every drawing.");
            }

            expectedDrawings = manifest.frames.Length;
            outputDirectory = Path.Combine(root, manifest.output);
            Directory.CreateDirectory(outputDirectory);
            Application.targetFrameRate = 100;
            QualitySettings.vSyncCount = 0;

            camera = new GameObject("StudyCamera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 941f / 2f;
            camera.aspect = 1672f / 941f;
            camera.transform.position = new Vector3(836f, 470.5f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = target;

            var canvasObject = new GameObject("StudyCanvas", typeof(RectTransform), typeof(Canvas));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            var canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.pivot = Vector2.zero;
            canvasRect.sizeDelta = new Vector2(1672f, 941f);

            var background = new GameObject("PreservedBackground", typeof(RectTransform), typeof(RawImage));
            background.transform.SetParent(canvas.transform, false);
            SetRect(background.GetComponent<RectTransform>(), Vector2.zero, new Vector2(1672f, 941f));
            background.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("ToyRoomIntro/room-v1");

            var actor = new GameObject("YunaStudy", typeof(RectTransform), typeof(Image));
            actor.transform.SetParent(canvas.transform, false);
            SetRect(actor.GetComponent<RectTransform>(), new Vector2(594f, 104f), new Vector2(512f, 512f));
            Sprite[] drawings = new Sprite[manifest.frames.Length];
            for (int frame = 0; frame < drawings.Length; frame++)
            {
                string path = Path.Combine(root, manifest.frames[frame]);
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(File.ReadAllBytes(path));
                texture.filterMode = FilterMode.Bilinear;
                drawings[frame] = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
            }

            player = actor.AddComponent<HandDrawnExposurePlayer>();
            player.Configure(actor.GetComponent<Image>(), drawings);
            player.OnDrawingPresented += CaptureExposure;
            player.OnCompleted += Complete;
            actor.GetComponent<Image>().sprite = drawings[0];
            Canvas.ForceUpdateCanvases();
            camera.Render();
            EditorApplication.update += BeginPlaybackWhenReady;
        }

        private static void BeginPlaybackWhenReady()
        {
            if (Time.frameCount < 5)
            {
                return;
            }

            EditorApplication.update -= BeginPlaybackWhenReady;
            startTime = EditorApplication.timeSinceStartup;
            player.Play();
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void CaptureExposure(int frame)
        {
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
            image.Apply();
            Captures.Add(image);
            RenderTexture.active = previous;
            Receipts.Add(new ExposureReceipt
            {
                drawing = frame,
                realtimeSeconds = EditorApplication.timeSinceStartup - startTime,
                unityFrame = Time.frameCount
            });
        }

        private static void Complete()
        {
            for (int index = 0; index < Captures.Count; index++)
            {
                int frame = Receipts[index].drawing;
                File.WriteAllBytes(Path.Combine(outputDirectory, $"frame-{frame:000}.png"), Captures[index].EncodeToPNG());
                UnityEngine.Object.Destroy(Captures[index]);
            }

            var receipt = new StudyReceipt
            {
                expectedDrawings = expectedDrawings,
                capturedDrawings = Receipts.Count,
                missedExposures = player.MissedExposures,
                exposures = Receipts.ToArray()
            };
            File.WriteAllText(Path.Combine(outputDirectory, "receipt.json"), JsonUtility.ToJson(receipt, true));
            SessionState.SetBool(RequestedKey, false);
            bool success = Receipts.Count == expectedDrawings && player.MissedExposures == 0;
            Debug.Log($"CINEMATICS25_STUDY captured={Receipts.Count} missed={player.MissedExposures}");
            EditorApplication.Exit(success ? 0 : 2);
        }
    }
}
