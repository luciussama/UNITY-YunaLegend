using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.Camera25D;
using YunaLegend.DarkRoom;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomStartCapture
    {
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
