using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.ToyRoom.Runtime;

namespace YunaLegend.Editor.ToyRoom
{
    /// <summary>
    /// Utilitario de auditoria em Play Mode real que faz Yuna caminhar na ToyRoom_Gameplay,
    /// captura 16 frames sequenciais da camera de jogo e encerra o Play Mode para gerar GIF de prova.
    /// </summary>
    [InitializeOnLoad]
    public static class ToyRoomWalkProofCapture
    {
        private const string TriggerPath = "Temp/trigger_walk_proof.txt";
        private const string FramesDir = "/Users/sergio/.gemini/antigravity/brain/1d17e175-ea96-41be-9e53-6cf1d38c269a/scratch/walk_frames";
        private const string ReadyPath = "/Users/sergio/.gemini/antigravity/brain/1d17e175-ea96-41be-9e53-6cf1d38c269a/scratch/walk_proof_ready.txt";
        private const int TotalFrames = 24;

        private static bool isCapturing;
        private static int capturedCount;
        private static double nextCaptureTime;

        static ToyRoomWalkProofCapture()
        {
            if (!Application.isBatchMode)
            {
                EditorApplication.update += Tick;
            }
        }

        public static void CaptureBatchmodeWalkProof()
        {
            EditorSceneManager.OpenScene(ToyRoomSceneBuilder.ScenePath, OpenSceneMode.Single);
            Directory.CreateDirectory(FramesDir);
            foreach (string old in Directory.GetFiles(FramesDir, "frame_*.png"))
            {
                File.Delete(old);
            }

            var player = Object.FindFirstObjectByType<ToyRoomPlayerController>();
            var animator = Object.FindFirstObjectByType<YunaToyRoomAnimator>();
            var cam = Camera.main;
            if (player == null || animator == null || cam == null)
            {
                Debug.LogError("[ToyRoomWalkProofCapture] Player, Animator ou Camera.main ausente.");
                return;
            }

            // Posiciona Yuna caminhando em 3/4 diante de camera fixa para comprovar deslocamento + passadas
            Vector3 startPos = new Vector3(-1.35f, 0f, -1.15f);
            Vector3 moveDir = new Vector3(0.90f, 0f, -0.35f).normalized;
            player.transform.position = startPos;
            player.transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);

            Vector3 midPos = startPos + moveDir * 1.15f;
            cam.transform.position = midPos + new Vector3(0.35f, 1.35f, -4.15f);
            cam.transform.LookAt(midPos + Vector3.up * 0.65f);
            cam.fieldOfView = 29f;

            const float dt = 0.055f;
            for (int i = 0; i < TotalFrames; i++)
            {
                player.transform.position = startPos + moveDir * (i * 0.10f);
                animator.SimulateWalkStep(dt, isMoving: true, isCarrying: false);

                string outFile = Path.Combine(FramesDir, $"frame_{i:00}.png");
                CaptureCamera(cam, outFile, 720, 480);
            }

            Debug.Log($"[ToyRoomWalkProofCapture] Batchmode capturou {TotalFrames} frames em {FramesDir}");
        }

        [MenuItem("YunaLegend/Fase 2 - Toy Room/Capturar Prova de Caminhada", false, 30)]
        public static void StartWalkProof()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (File.Exists(ReadyPath))
            {
                File.Delete(ReadyPath);
            }

            Directory.CreateDirectory(FramesDir);
            foreach (string old in Directory.GetFiles(FramesDir, "frame_*.png"))
            {
                File.Delete(old);
            }

            EditorSceneManager.OpenScene(ToyRoomSceneBuilder.ScenePath, OpenSceneMode.Single);
            isCapturing = true;
            capturedCount = 0;
            nextCaptureTime = 0;
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            if (!isCapturing)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(TriggerPath))
                {
                    File.Delete(TriggerPath);
                    StartWalkProof();
                }

                return;
            }

            if (!EditorApplication.isPlaying)
            {
                return;
            }

            var player = Object.FindFirstObjectByType<ToyRoomPlayerController>();
            if (player != null)
            {
                player.ControlsEnabled = true;
                player.ScriptedMoveInput = new Vector2(0.75f, -0.55f);
            }

            if (EditorApplication.timeSinceStartup < nextCaptureTime)
            {
                return;
            }

            if (nextCaptureTime <= 0)
            {
                nextCaptureTime = EditorApplication.timeSinceStartup + 0.35;
                return;
            }

            nextCaptureTime = EditorApplication.timeSinceStartup + 0.11;

            Camera cam = Camera.main;
            if (cam != null)
            {
                string outFile = Path.Combine(FramesDir, $"frame_{capturedCount:00}.png");
                CaptureCamera(cam, outFile, 960, 540);
                capturedCount++;
            }

            if (capturedCount >= TotalFrames)
            {
                isCapturing = false;
                if (player != null)
                {
                    player.ScriptedMoveInput = null;
                }

                EditorApplication.isPlaying = false;
                File.WriteAllText(ReadyPath, $"captured={capturedCount}\n");
                Debug.Log($"[ToyRoomWalkProofCapture] {capturedCount} frames capturados em {FramesDir}");
            }
        }

        private static void CaptureCamera(Camera cam, string path, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
    }
}
