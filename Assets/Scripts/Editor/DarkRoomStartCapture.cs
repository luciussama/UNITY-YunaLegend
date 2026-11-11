using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomStartCapture
    {
        // URP uses an external target's format for its intermediate color buffer.
        // An 8-bit target clips emission before Bloom/tonemapping, so render to HDR
        // first and encode the final display image into an sRGB texture afterwards.
        public static Texture2D RenderDisplayImage(Camera camera, int width, int height)
        {
            var hdr = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            var display = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            bool previousSrgb = GL.sRGBWrite;
            try
            {
                camera.targetTexture = hdr;
                camera.Render();
                GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                Graphics.Blit(hdr, display);
                RenderTexture.active = display;
                var image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                return image;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                GL.sRGBWrite = previousSrgb;
                Object.DestroyImmediate(hdr);
                Object.DestroyImmediate(display);
            }
        }

        public static void SaveCameraFraming()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            var camera = Camera.main;
            camera.aspect = 16f / 9f;
            var side = camera.GetComponent<SideScroll25DCamera>();
            side.SetTarget(Object.FindFirstObjectByType<Yuna25DPlayerController>().transform);
            side.SnapToRouteFraming(0);
            UnityEditor.EditorUtility.SetDirty(side);
            UnityEditor.EditorUtility.SetDirty(camera);
            EditorSceneManager.MarkSceneDirty(camera.gameObject.scene);
            EditorSceneManager.SaveScene(camera.gameObject.scene);
            Debug.Log("CLOSE_CAMERA_SAVED: initial framing persisted without rebuilding art, audio or platforms.");
        }

        public static void CaptureWallDecorations()
        {
            Capture();
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            var root = GameObject.Find("Decoracao_Parede_Infantil");
            if (root == null || root.transform.childCount != 10)
                throw new System.InvalidOperationException("Expected ten wall decoration groups in the saved scene.");
            if (root.GetComponentsInChildren<Collider>(true).Length != 0 || root.GetComponentsInChildren<Light>(true).Length != 0)
                throw new System.InvalidOperationException("Wall decorations must have no colliders or lights.");
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null)
                    throw new System.InvalidOperationException("Missing textured material: " + renderer.name);
                if (renderer.bounds.min.z < 4.5f)
                    throw new System.InvalidOperationException("Wall decoration entered the running lane: " + renderer.name);
            }
            Debug.Log("WALL_DECOR_VALIDATION: 10 groups, textured renderers, zero colliders/lights, all geometry behind Z=4.5.");

            var camera = Camera.main;
            var originalPosition = camera.transform.position;
            var originalRotation = camera.transform.rotation;
            float originalAspect = camera.aspect;
            var gallery = new Texture2D(1600, 480, TextureFormat.RGB24, false);
            try
            {
                camera.aspect = 4f / 3f;
                camera.transform.rotation = Quaternion.identity;
                for (int i = 0; i < root.transform.childCount; i++)
                {
                    var group = root.transform.GetChild(i);
                    var center = group.position;
                    center.y += i < 3 ? 0.35f : i < 6 ? -0.80f : 0f;
                    camera.transform.position = new Vector3(center.x, center.y, 1.7f);
                    var tile = RenderDisplayImage(camera, 320, 240);
                    gallery.SetPixels((i % 5) * 320, (1 - i / 5) * 240, 320, 240, tile.GetPixels());
                    Object.DestroyImmediate(tile);
                }
                gallery.Apply();
                File.WriteAllBytes("Evidence/wall-decor-gallery.png", gallery.EncodeToPNG());
                Debug.Log("WALL_DECOR_GALLERY Evidence/wall-decor-gallery.png (saved-scene lighting, no added lights)");
            }
            finally
            {
                camera.targetTexture = null;
                camera.aspect = originalAspect;
                camera.transform.SetPositionAndRotation(originalPosition, originalRotation);
                Object.DestroyImmediate(gallery);
            }
        }

        public static void Capture()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            var camera = Camera.main;
            camera.aspect = 16f / 9f;
            camera.GetComponent<SideScroll25DCamera>().SnapToTargetX(DarkRoomStartLayout.StartX, DarkRoomStartLayout.CameraOffset);
            var image = RenderDisplayImage(camera, 1280, 720);
            Directory.CreateDirectory("Evidence");
            File.WriteAllBytes("Evidence/start-camera.png", image.EncodeToPNG());
            Object.DestroyImmediate(image);
            Debug.Log("START_CAMERA Evidence/start-camera.png (camera render only, OnGUI overlay excluded)");

            // Captura da Etapa 2 (Castelinho de blocos plat 9 e inicio da fuga plat 10..14)
            var builder = UnityEngine.Object.FindFirstObjectByType<DarkRoomBlockoutBuilder>();
            if (builder != null)
            {
                builder.ShowTemporalSection(DarkRoomPhase.Phase2Escape, 9);
            }
            // Position both actors as well as the camera; otherwise a close frame
            // would incorrectly use Yuna's starting-floor height for the castle.
            var markers = Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Object.FindFirstObjectByType<Yuna25DPlayerController>().transform.position = markers.Single(m => m.spec.index == 9).GetTopCenterWorld() + Vector3.up * 0.03f;
            Object.FindFirstObjectByType<NandaFairyGuide>().transform.position = markers.Single(m => m.spec.index == 10).GetTopCenterWorld() + Vector3.up * 1.2f;
            camera.GetComponent<SideScroll25DCamera>().ConfigureSection(-1.5f);
            camera.GetComponent<SideScroll25DCamera>().SnapToRouteFraming(10);

            var imageCastle = RenderDisplayImage(camera, 1280, 720);
            File.WriteAllBytes("Evidence/castle-camera.png", imageCastle.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(imageCastle);
            Debug.Log("CASTLE_CAMERA Evidence/castle-camera.png (capture of Stage 2 takeoff at castle and escape)");
        }
    }
}
