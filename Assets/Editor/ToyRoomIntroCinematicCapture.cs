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
    /// Captura os quadros da cinematica oficial de abertura da Fase 2 (ToyRoom_Gameplay.unity)
    /// em sprite stop motion (960x540) nos mesmos instantes temporais do projeto legado
    /// para auditoria de paridade visual 100%.
    /// </summary>
    public static class ToyRoomIntroCinematicCapture
    {
        private const string OutputDir = "Logs/toyroom_intro_cinematic";
        private const int FrameW = 960;
        private const int FrameH = 540;

        [MenuItem("YunaLegend/Fase 2 - Toy Room/Capture Intro Stop Motion Frames")]
        public static void CaptureAll()
        {
            Directory.CreateDirectory(OutputDir);

            // Limpa capturas 3D antigas para nao restar nenhum vestigio da cena 3D excluida
            foreach (string oldFile in Directory.GetFiles(OutputDir, "*.png"))
            {
                File.Delete(oldFile);
            }

            EditorSceneManager.OpenScene("Assets/Scenes/ToyRoom_Gameplay.unity", OpenSceneMode.Single);
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var audioCtrl = UnityEngine.Object.FindFirstObjectByType<ToyRoomAudioController>();
            var caveatFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Caveat-Variable.ttf");

            var hostGo = new GameObject("__TempIntroStopMotionDirector");
            try
            {
                var director = hostGo.AddComponent<ToyRoomIntroCinematicController>();
                director.Configure(audioCtrl, caveatFont);
                director.BeginIntroCinematic();
                director.SetViewportOverride(FrameW, FrameH);

                // 1) TR_003 (t = 1.3s — Yuna entrando no tapete, Pose 0, cortina abrindo)
                director.EvaluateAtTime(1.30f);
                RenderFrameWithUi(cam, FrameW, FrameH, "01_TR003_arrival_entering.png");

                // 2) TR_003 (t = 2.8s — Yuna estabelecida no tapete olhando o quarto, Pose 0)
                director.EvaluateAtTime(2.80f);
                RenderFrameWithUi(cam, FrameW, FrameH, "02_TR003_arrival_settled.png");

                // 3) TR_004 (t = 5.0s — Pose 1, Menina: "Aqui também...")
                director.EvaluateAtTime(5.00f);
                RenderFrameWithUi(cam, FrameW, FrameH, "03_TR004_here_too.png");

                // 4) TR_005 (t = 9.3s — Pose 1, Menina: "Eu queria espaço pra brincar." + Nanda voando ao fundo)
                director.EvaluateAtTime(9.30f);
                RenderFrameWithUi(cam, FrameW, FrameH, "04_TR005_space_to_play.png");

                // 5) TR_006 Parte 1 (t = 12.8s — Pose 2 cabeca baixa, "A sala estava cheia. / Nenhum lugar esperava por ela.")
                director.EvaluateAtTime(12.80f);
                RenderFrameWithUi(cam, FrameW, FrameH, "05_TR006_poetic_room_full.png");

                // 6) TR_006 Pausa (t = 14.8s — Pose 2 respiro silencioso entre os dois cartoes poeticos)
                director.EvaluateAtTime(14.80f);
                RenderFrameWithUi(cam, FrameW, FrameH, "06_TR006_poetic_pause.png");

                // 7) TR_006 Parte 2 (t = 16.5s — Pose 2, "A caixa tinha um fundo. / O resto está vazio.")
                director.EvaluateAtTime(16.50f);
                RenderFrameWithUi(cam, FrameW, FrameH, "07_TR006_poetic_box_bottom.png");

                // 8) TR_007 (t = 19.3s — Pose 3 acenando/apontando, Nanda: "Vamos. Um de cada vez!")
                director.EvaluateAtTime(19.30f);
                RenderFrameWithUi(cam, FrameW, FrameH, "08_TR007_nanda_one_by_one.png");

                // 9) TR_008 (t = 21.5s — Pose 3 plano aberto final com musica subindo)
                director.EvaluateAtTime(21.50f);
                RenderFrameWithUi(cam, FrameW, FrameH, "09_TR008_pullback_music.png");

                // 10) TR_008 (t = 23.0s — Fade creme final para inicio da gameplay)
                director.EvaluateAtTime(23.00f);
                RenderFrameWithUi(cam, FrameW, FrameH, "10_TR008_warm_fade_transition.png");

                director.EndIntroCinematic();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(hostGo);
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Debug.Log("TOYROOM_INTRO_CINEMATIC_CAPTURE_OK " + Path.GetFullPath(OutputDir));
        }

        private static void RenderFrameWithUi(Camera cam, int width, int height, string fileName)
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Array.Sort(canvases, (a, b) => a.sortingOrder.CompareTo(b.sortingOrder));

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
                    if (a == 0)
                    {
                        continue;
                    }

                    if (a == 255)
                    {
                        scenePixels[p] = uiPixels[p];
                        scenePixels[p].a = 255;
                        continue;
                    }

                    float alpha = a / 255f;
                    float inv = 1f - alpha;
                    scenePixels[p].r = (byte)Mathf.Clamp(Mathf.RoundToInt((uiPixels[p].r * alpha) + (scenePixels[p].r * inv)), 0, 255);
                    scenePixels[p].g = (byte)Mathf.Clamp(Mathf.RoundToInt((uiPixels[p].g * alpha) + (scenePixels[p].g * inv)), 0, 255);
                    scenePixels[p].b = (byte)Mathf.Clamp(Mathf.RoundToInt((uiPixels[p].b * alpha) + (scenePixels[p].b * inv)), 0, 255);
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
