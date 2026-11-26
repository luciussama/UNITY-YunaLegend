using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.BearChase.Core;
using YunaLegend.BearChase.Runtime;

namespace YunaLegend.EditorScripts.BearChase
{
    /// <summary>
    /// Renders the complete 6.5s BearChase entrance cinematic (30 FPS, 195 frames) with the new
    /// 16-joint deformable 2D Yuna rig, plus keyframe stills for proportion & hip-pivot audit.
    /// </summary>
    public static class BearChaseEntranceVideoCapture
    {
        private const string EvidenceDir = "Evidence/BearChaseEntranceRig2D";

        [MenuItem("YunaLegend/BearChase/Capture Entrance Rig 2D Video")]
        public static void CaptureAll()
        {
            string framesDir = Path.Combine(EvidenceDir, "video_frames");
            if (Directory.Exists(framesDir))
            {
                Directory.Delete(framesDir, true);
            }
            Directory.CreateDirectory(framesDir);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("CaptureCam", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);

            var ctrlGo = new GameObject("BearChaseEntranceController", typeof(BearChaseEntranceController));
            var ctrl = ctrlGo.GetComponent<BearChaseEntranceController>();
            ctrl.EnsureBuilt();
            ctrl.SetCaptureCamera(cam);

            int width = 960;
            int height = 540;
            cam.aspect = width / (float)height;
            var rt = new RenderTexture(width, height, 24);
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            const int fps = 30;
            int totalFrames = Mathf.CeilToInt(BearChaseEntranceTimeline.TotalDuration * fps);

            for (int f = 0; f < totalFrames; f++)
            {
                float t = Mathf.Min(BearChaseEntranceTimeline.TotalDuration, f / (float)fps);
                BearChaseEntranceSample sample = BearChaseEntranceTimeline.Evaluate(t);
                ctrl.ApplySample(sample, true, t);
                Canvas.ForceUpdateCanvases();

                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                cam.targetTexture = null;
                RenderTexture.active = null;

                File.WriteAllBytes(Path.Combine(framesDir, $"frame_{f:D4}.png"), tex.EncodeToPNG());
            }

            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(rt);
            Debug.Log($"BEARCHASE_ENTRANCE_RIG2D_VIDEO_OK {totalFrames} frames -> {framesDir}");
        }
    }
}
