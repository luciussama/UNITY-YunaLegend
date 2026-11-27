using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using YunaLegend.Picnic.Core;
using YunaLegend.Picnic.Runtime;
using YunaLegend.Presentation;
using YunaLegend.ToyRoom.Core;
using YunaLegend.ToyRoom.Runtime;

namespace YunaLegend.UiAudit.Editor
{
    public static class DialogueBalloonUiAuditCapture
    {
        public static void CaptureAll()
        {
            string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Evidence", "UiDialogueAudit"));
            Directory.CreateDirectory(outDir);

            CapturePicnicStory(outDir, "picnic-story-desktop-1280x720.png", 1280, 720, false);
            CapturePicnicStory(outDir, "picnic-story-mobile-390x844.png", 390, 844, true);
            CaptureToyRoomDialogue(outDir, "toyroom-dialogue-desktop-1280x720.png", 1280, 720, false);
            CaptureToyRoomDialogue(outDir, "toyroom-dialogue-mobile-390x844.png", 390, 844, true);

            Debug.Log("[UiDialogueAudit] UI_DIALOGUE_AUDIT_OK: " + outDir);
        }

        private static void CapturePicnicStory(string outDir, string fileName, int width, int height, bool mobile)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = CreateCam(new Color(0.55f, 0.78f, 0.94f, 1f));

            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;
            cam.targetTexture = rt;

            var hudGo = new GameObject("PicnicNarrativeAndHud", typeof(PicnicNarrativeAndHudController));
            var hud = hudGo.GetComponent<PicnicNarrativeAndHudController>();
            hud.Build();
            hud.SetViewportOverride(width, height);

            var sim = new PicnicSimulation(false);
            hud.Apply(sim, false);
            AttachCanvasesToCamera(cam);
            Canvas.ForceUpdateCanvases();

            if (mobile)
            {
                ApplyMobilePresenter(width, height);
            }

            Canvas.ForceUpdateCanvases();
            CaptureFromBoundTarget(cam, rt, prevTarget, prevActive, width, height, Path.Combine(outDir, fileName));
        }

        private static void CaptureToyRoomDialogue(string outDir, string fileName, int width, int height, bool mobile)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = CreateCam(new Color(0.18f, 0.14f, 0.24f, 1f));
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;
            cam.targetTexture = rt;

            var hudGo = new GameObject("ToyRoomHud", typeof(ToyRoomHudController));
            var hud = hudGo.GetComponent<ToyRoomHudController>();
            hud.ConfigureFont(null);
            hud.SetViewportOverride(width, height);
            hud.UpdateToyCounter(3, 8);
            hud.ShowBeat(new ToyRoomNarrativeBeat(
                ToyRoomBeatKind.Dialogue,
                ToyRoomSpeaker.Nanda,
                "Vamos. Um de cada vez!",
                2.5f));
            AttachCanvasesToCamera(cam);
            Canvas.ForceUpdateCanvases();

            if (mobile)
            {
                ApplyMobilePresenter(width, height);
            }

            Canvas.ForceUpdateCanvases();
            CaptureFromBoundTarget(cam, rt, prevTarget, prevActive, width, height, Path.Combine(outDir, fileName));
        }

        private static Camera CreateCam(Color bg)
        {
            var camGo = new GameObject("AuditCam", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bg;
            cam.orthographic = true;
            return cam;
        }

        private static void AttachCanvasesToCamera(Camera cam)
        {
            foreach (Canvas c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                c.renderMode = RenderMode.ScreenSpaceCamera;
                c.worldCamera = cam;
                c.planeDistance = 5f;
            }
        }

        private static void ApplyMobilePresenter(int width, int height)
        {
            var presenterGo = new GameObject("MobileDialoguePresentation", typeof(MobileDialoguePresenter));
            var presenter = presenterGo.GetComponent<MobileDialoguePresenter>();
            presenter.SetMobileViewportOverride(true, width, height);
            Canvas.ForceUpdateCanvases();
            presenter.ApplyNow();
        }

        private static void CaptureFromBoundTarget(Camera cam, RenderTexture rt, RenderTexture prevTarget, RenderTexture prevActive, int width, int height, string filePath)
        {
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            File.WriteAllBytes(filePath, tex.EncodeToPNG());
            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(rt);
        }
    }
}
