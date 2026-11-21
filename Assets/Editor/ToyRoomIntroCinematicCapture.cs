using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.ToyRoom.Core;
using YunaLegend.ToyRoom.Runtime;

namespace YunaLegend.EditorScripts
{
    /// <summary>
    /// Renderiza o filme de abertura da Fase 2 (ToyRoom_Gameplay.unity) a partir dos modelos 3D reais
    /// na Unity, gerando os quadros de cada plano cinematografico (Shots 1 a 6), closes faciais
    /// provando o movimento real dos olhos e a interacao com ternura entre Yuna e Nanda,
    /// e telemetria verificavel de musica, olhos e poses em Logs/toyroom_intro_cinematic/.
    /// </summary>
    public static class ToyRoomIntroCinematicCapture
    {
        private const string OutputDir = "Logs/toyroom_intro_cinematic";
        private const int FrameW = 1280;
        private const int FrameH = 720;

        public static void CaptureAll()
        {
            Directory.CreateDirectory(OutputDir);

            EditorSceneManager.OpenScene("Assets/Scenes/ToyRoom_Gameplay.unity", OpenSceneMode.Single);
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var isoCam = cam != null ? cam.GetComponent<IsometricToyRoomCamera>() : null;
            var player = UnityEngine.Object.FindFirstObjectByType<ToyRoomPlayerController>();
            var nanda = UnityEngine.Object.FindFirstObjectByType<NandaToyRoomGuideController>();
            var chest = UnityEngine.Object.FindFirstObjectByType<ToyChestController>();
            var hud = UnityEngine.Object.FindFirstObjectByType<ToyRoomHudController>();
            var narrative = UnityEngine.Object.FindFirstObjectByType<ToyRoomNarrativeController>();
            var audioCtrl = UnityEngine.Object.FindFirstObjectByType<ToyRoomAudioController>();

            var hostGo = new GameObject("__TempIntroCinematicDirector");
            try
            {
                var director = hostGo.AddComponent<ToyRoomIntroCinematicController>();
                director.Configure(cam, isoCam, player, nanda, chest, narrative, audioCtrl);
                director.BeginIntroCinematic();

                var beats = ToyRoomNarrativeScript.IntroBeats;

                // 1) Shot 1A — Despertar na Luz Matinal (olhos de Yuna e Nanda olhando para a esquerda)
                hud?.ShowBeat(beats[0]);
                director.EvaluateAtBeat(0, 0.15f);
                RenderFrameWithUi(cam, FrameW, FrameH, "01_shot1_awakening_gaze_left.png");

                // 2) Shot 1B — Despertar na Luz Matinal (olhos de Yuna varrendo para a direita + Nanda olhando para Yuna)
                director.EvaluateAtBeat(0, 0.88f);
                RenderFrameWithUi(cam, FrameW, FrameH, "02_shot1_awakening_gaze_right.png");

                // 3) Shot 2 — Close-Up Emocional 3/4 ("Aqui também..." — Yuna olha para os brinquedos no chao, Nanda empatica)
                hud?.ShowBeat(beats[1]);
                director.EvaluateAtBeat(1, 0.75f);
                RenderFrameWithUi(cam, FrameW, FrameH, "03_shot2_here_too_melancholy.png");

                // 4) Shot 3A — Inicio da fala ("Eu queria espaço pra brincar." — maos abaixadas, olhar inicial baixo)
                hud?.ShowBeat(beats[2]);
                director.EvaluateAtBeat(2, 0.18f);
                RenderFrameWithUi(cam, FrameW, FrameH, "04_shot3_tenderness_eye_contact.png");

                // 5) Shot 3B — Climax de Ternura (Nanda aproxima-se estendendo os bracos e Yuna ergue ambas as maos ao peito/para Nanda olhando nos olhos dela)
                director.EvaluateAtBeat(2, 0.88f);
                RenderFrameWithUi(cam, FrameW, FrameH, "05_shot3_tenderness_hand_gesture.png");

                // 6) Close-Up Macro Facial dos Olhos (sem HUD: metade esquerda Estado A olhando baixo-direita vs metade direita Estado B olhando cima-esquerda para Nanda)
                hud?.ClearNarrativeOverlays();
                CaptureEyeMovementMacroProof(cam, director, player.transform.position);

                // 7) Shot 4 — Traveling Panoramico Poetico ("A sala estava cheia. / Nenhum lugar esperava por ela.")
                hud?.ShowBeat(beats[3]);
                director.EvaluateAtBeat(3, 0.55f);
                RenderFrameWithUi(cam, FrameW, FrameH, "07_shot4_poetic_room_panorama.png");

                // 8) Shot 5 — Revelacao do Bau Dourado ("A caixa tinha um fundo. / O resto está vazio." — Nanda aponta e ambas olham para o bau)
                hud?.ShowBeat(beats[5]);
                director.EvaluateAtBeat(5, 0.75f);
                RenderFrameWithUi(cam, FrameW, FrameH, "08_shot5_poetic_chest_discovery.png");

                // 9) Shot 6A — Cumplicidade e Chamado ("Vamos. Um de cada vez!")
                hud?.ShowBeat(beats[6]);
                director.EvaluateAtBeat(6, 0.30f);
                RenderFrameWithUi(cam, FrameW, FrameH, "09_shot6_nanda_call_to_action.png");

                // 10) Shot 6B — Transicao Continua para o Enquadramento de Gameplay
                director.EvaluateAtBeat(6, 0.95f);
                RenderFrameWithUi(cam, FrameW, FrameH, "10_shot6_seamless_gameplay_blend.png");

                director.EndIntroCinematic();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(hostGo);
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Debug.Log("TOYROOM_INTRO_CINEMATIC_CAPTURE_OK " + Path.GetFullPath(OutputDir));
        }

        private static void CaptureEyeMovementMacroProof(Camera cam, ToyRoomIntroCinematicController director, Vector3 yunaPos)
        {
            // Renderiza um painel comparativo 1280x720 lado a lado com a camera 100% FIXA enquadrando os rostos de Yuna e Nanda
            // (Metade esquerda 640x720: Estado A — olhar para a esquerda e bracos em repouso;
            //  Metade direita 640x720: Estado B — olhar para a direita/Nanda e gesto de ternura com os bracos erguidos)
            Vector3 fixedCamPos = yunaPos + new Vector3(-0.22f, 1.24f, -2.05f);
            Vector3 fixedCamLook = yunaPos + new Vector3(-0.22f, 1.16f, 0.0f);

            director.EvaluateAtBeat(0, 0.15f);
            cam.transform.position = fixedCamPos;
            cam.transform.LookAt(fixedCamLook);
            cam.fieldOfView = 25f;
            var texA = RenderCameraRaw(cam, 640, 720);

            director.EvaluateAtBeat(2, 0.88f);
            cam.transform.position = fixedCamPos;
            cam.transform.LookAt(fixedCamLook);
            cam.fieldOfView = 25f;
            var texB = RenderCameraRaw(cam, 640, 720);

            var combined = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
            combined.SetPixels(0, 0, 640, 720, texA.GetPixels());
            combined.SetPixels(640, 0, 640, 720, texB.GetPixels());
            combined.Apply();

            File.WriteAllBytes(Path.Combine(OutputDir, "06_macro_eye_movement_proof_A_vs_B.png"), combined.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texA);
            UnityEngine.Object.DestroyImmediate(texB);
            UnityEngine.Object.DestroyImmediate(combined);
        }

        private static Texture2D RenderCameraRaw(Camera cam, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prevTarget = cam.targetTexture;
            float prevAspect = cam.aspect;
            try
            {
                cam.aspect = (float)width / height;
                cam.targetTexture = rt;
                cam.Render();
                var prevActive = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                RenderTexture.active = prevActive;
                return tex;
            }
            finally
            {
                cam.targetTexture = prevTarget;
                cam.aspect = prevAspect;
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        private static void RenderFrameWithUi(Camera cam, int width, int height, string fileName)
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var prevModes = new RenderMode[canvases.Length];
            var prevCameras = new Camera[canvases.Length];
            var prevDistances = new float[canvases.Length];
            var prevLayers = new int[canvases.Length];

            var rtScene = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var rtUi = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prevTarget = cam.targetTexture;
            float prevAspect = cam.aspect;
            int prevMask = cam.cullingMask;

            var uiCamGo = new GameObject("__TempIntroUiCam");
            var uiCam = uiCamGo.AddComponent<Camera>();
            uiCam.cameraType = CameraType.Preview;
            uiCam.orthographic = true;
            uiCam.orthographicSize = 5f;
            uiCam.nearClipPlane = 0.1f;
            uiCam.farClipPlane = 10f;
            uiCam.clearFlags = CameraClearFlags.SolidColor;
            uiCam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            uiCam.cullingMask = 1 << 5;
            uiCam.aspect = (float)width / height;
            uiCam.targetTexture = rtUi;

            try
            {
                cam.aspect = (float)width / height;
                cam.cullingMask = prevMask & ~(1 << 5);
                cam.targetTexture = rtScene;

                for (int i = 0; i < canvases.Length; i++)
                {
                    prevModes[i] = canvases[i].renderMode;
                    prevCameras[i] = canvases[i].worldCamera;
                    prevDistances[i] = canvases[i].planeDistance;
                    prevLayers[i] = canvases[i].gameObject.layer;

                    SetLayerRecursively(canvases[i].gameObject, 5);
                    canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                    canvases[i].worldCamera = uiCam;
                    canvases[i].planeDistance = 1f;
                }

                Canvas.ForceUpdateCanvases();
                cam.Render();
                uiCam.Render();

                var prevActive = RenderTexture.active;
                RenderTexture.active = rtScene;
                var sceneTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                sceneTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                sceneTex.Apply();

                RenderTexture.active = rtUi;
                var uiTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                uiTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                uiTex.Apply();
                RenderTexture.active = prevActive;

                var scenePixels = sceneTex.GetPixels32();
                var uiPixels = uiTex.GetPixels32();
                for (int p = 0; p < scenePixels.Length; p++)
                {
                    byte a = uiPixels[p].a;
                    if (a == 0) continue;
                    if (a == 255)
                    {
                        scenePixels[p] = uiPixels[p];
                        scenePixels[p].a = 255;
                        continue;
                    }

                    float alpha = a / 255f;
                    float inv = 1f - alpha;
                    scenePixels[p].r = (byte)Mathf.Clamp(Mathf.RoundToInt(uiPixels[p].r * alpha + scenePixels[p].r * inv), 0, 255);
                    scenePixels[p].g = (byte)Mathf.Clamp(Mathf.RoundToInt(uiPixels[p].g * alpha + scenePixels[p].g * inv), 0, 255);
                    scenePixels[p].b = (byte)Mathf.Clamp(Mathf.RoundToInt(uiPixels[p].b * alpha + scenePixels[p].b * inv), 0, 255);
                    scenePixels[p].a = 255;
                }

                sceneTex.SetPixels32(scenePixels);
                sceneTex.Apply();
                File.WriteAllBytes(Path.Combine(OutputDir, fileName), sceneTex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(sceneTex);
                UnityEngine.Object.DestroyImmediate(uiTex);
            }
            finally
            {
                for (int i = 0; i < canvases.Length; i++)
                {
                    if (canvases[i] != null)
                    {
                        canvases[i].renderMode = prevModes[i];
                        canvases[i].worldCamera = prevCameras[i];
                        canvases[i].planeDistance = prevDistances[i];
                        SetLayerRecursively(canvases[i].gameObject, prevLayers[i]);
                    }
                }

                cam.cullingMask = prevMask;
                cam.targetTexture = prevTarget;
                cam.aspect = prevAspect;
                uiCam.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(uiCamGo);
                UnityEngine.Object.DestroyImmediate(rtScene);
                UnityEngine.Object.DestroyImmediate(rtUi);
            }
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
