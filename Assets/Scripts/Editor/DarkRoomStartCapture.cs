using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomStartCapture
    {
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
            var previous = RenderTexture.active;
            var target = new RenderTexture(320, 240, 24);
            var gallery = new Texture2D(1600, 480, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.aspect = 4f / 3f;
                camera.transform.rotation = Quaternion.identity;
                for (int i = 0; i < root.transform.childCount; i++)
                {
                    var group = root.transform.GetChild(i);
                    var center = group.position;
                    center.y += i < 3 ? 0.35f : i < 6 ? -0.80f : 0f;
                    camera.transform.position = new Vector3(center.x, center.y, 1.7f);
                    camera.Render();
                    RenderTexture.active = target;
                    gallery.ReadPixels(new Rect(0, 0, 320, 240), (i % 5) * 320, (1 - i / 5) * 240, false);
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
                RenderTexture.active = previous;
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(gallery);
            }
        }

        public static void Capture()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            var camera = Camera.main;
            camera.GetComponent<SideScroll25DCamera>().SnapToTargetX(DarkRoomStartLayout.StartX, DarkRoomStartLayout.CameraOffset);
            var texture = new RenderTexture(1280, 720, 24);
            var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            camera.targetTexture = texture;
            camera.Render();
            RenderTexture.active = texture;
            image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            image.Apply();
            Directory.CreateDirectory("Evidence");
            File.WriteAllBytes("Evidence/start-camera.png", image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            Object.DestroyImmediate(image);
            Object.DestroyImmediate(texture);
            Debug.Log("START_CAMERA Evidence/start-camera.png (camera render only, OnGUI overlay excluded)");

            // Captura da Etapa 2 (Castelinho de blocos plat 9 e inicio da fuga plat 10..14)
            var builder = UnityEngine.Object.FindFirstObjectByType<DarkRoomBlockoutBuilder>();
            if (builder != null)
            {
                builder.ShowTemporalSection(DarkRoomPhase.Phase2Escape, 10);
            }
            camera.GetComponent<SideScroll25DCamera>().ConfigureSection(-1.5f);
            camera.GetComponent<SideScroll25DCamera>().SnapToTargetX(42.0f, -3.5f);

            var textureCastle = new RenderTexture(1280, 720, 24);
            var imageCastle = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            camera.targetTexture = textureCastle;
            camera.Render();
            RenderTexture.active = textureCastle;
            imageCastle.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            imageCastle.Apply();
            File.WriteAllBytes("Evidence/castle-camera.png", imageCastle.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(imageCastle);
            UnityEngine.Object.DestroyImmediate(textureCastle);
            Debug.Log("CASTLE_CAMERA Evidence/castle-camera.png (capture of Stage 2 takeoff at castle and escape)");
        }
    }
}
