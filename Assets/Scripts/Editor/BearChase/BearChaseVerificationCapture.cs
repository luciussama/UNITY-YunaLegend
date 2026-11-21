using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.BearChase.Core;
using YunaLegend.BearChase.Runtime;

namespace YunaLegend.Editor.BearChase
{
    /// <summary>
    /// Utilitario de captura visual em batchmode para auditoria black-box do Art/Game/Character/Environment Artist Agent.
    /// </summary>
    public static class BearChaseVerificationCapture
    {
        private const string OutputDir = "/Users/sergio/.gemini/antigravity/brain/6d4df7a7-6616-4914-8048-607af834b851/scratch/bear_chase_audit";

        public static void BuildAndCaptureAll()
        {
            BearChaseSceneBuilder.BuildBearChaseScene();
            EditorSceneManager.OpenScene(BearChaseSceneBuilder.ScenePath, OpenSceneMode.Single);
            Directory.CreateDirectory(OutputDir);

            var cam = Camera.main;
            var camCtrl = Object.FindFirstObjectByType<BearChaseCameraController>();
            var player = Object.FindFirstObjectByType<BearChasePlayerController>();
            var bear = Object.FindFirstObjectByType<BearChaseBearAnimator>();
            var nanda = Object.FindFirstObjectByType<BearChaseNandaController>();
            var hud = Object.FindFirstObjectByType<BearChaseHudController>();
            var bootstrap = Object.FindFirstObjectByType<BearChaseStageBootstrap>();
            var entranceRoot = bootstrap != null ? bootstrap.EntranceRoomRoot : GameObject.Find("Entrance_Bedroom_Root");
            var chaseRoot = bootstrap != null ? bootstrap.ChaseCorridorRoot : GameObject.Find("Chase_Corridor_Root");

            if (cam == null || player == null || bear == null || chaseRoot == null)
            {
                Debug.LogError("[BearChaseVerificationCapture] Componentes principais ausentes na cena.");
                return;
            }

            Vector3 yunaStart = new Vector3(-2.65f, 0f, 0f);
            Vector3 yunaNudge = new Vector3(0.62f, 0f, 0f);
            Vector3 bearSleep = new Vector3(2.15f, 0f, 0.05f);
            Vector3 chaseOrigin = new Vector3(0f, 0f, 40f);

            var sim = new BearChaseSimulation();

            // 1. Entrada inicial (Yuna na esquerda perto da poltrona, Urso dormindo no tapete)
            if (entranceRoot != null) entranceRoot.SetActive(true);
            chaseRoot.SetActive(false);
            for (int i = 0; i < 6; i++)
            {
                sim.Step(0.05f);
            }
            UpdateAll(sim, camCtrl, player, bear, nanda, hud, yunaStart, yunaNudge, bearSleep, chaseOrigin, 0.02f);
            RenderToPng(cam, Path.Combine(OutputDir, "01_entrance_start.png"), 1280, 720);

            // 2. Entrada: Yuna toca no urso e o urso acorda assustado (t = 5.75s -> BearWakeUp)
            for (int i = 0; i < 109; i++)
            {
                sim.Step(0.05f);
            }
            UpdateAll(sim, camCtrl, player, bear, nanda, hud, yunaStart, yunaNudge, bearSleep, chaseOrigin, 0.02f);
            RenderToPng(cam, Path.Combine(OutputDir, "02_entrance_wakeup.png"), 1280, 720);

            // 3. Corrida 3D no corredor curvo da ToyRoom (com Yuna, Nanda, Urso e obstaculos visiveis)
            if (entranceRoot != null) entranceRoot.SetActive(false);
            chaseRoot.SetActive(true);
            sim.ResetFullStage(skipEntrance: true, useEasyMode: false);
            for (int i = 0; i < 56; i++)
            {
                sim.Step(0.05f);
                UpdateAll(sim, camCtrl, player, bear, nanda, hud, yunaStart, yunaNudge, bearSleep, chaseOrigin, 0.05f);
            }

            RenderToPng(cam, Path.Combine(OutputDir, "04_chase_running.png"), 1280, 720);

            // 4. Close-up lateral 3/4 do urso em corrida dentro do corredor iluminado
            Vector3 bearPos = bear.transform.position;
            cam.transform.position = bearPos + new Vector3(1.55f, 1.05f, 1.55f);
            cam.transform.LookAt(bearPos + new Vector3(0f, 0.58f, 0f));
            RenderToPng(cam, Path.Combine(OutputDir, "03_bear_closeup.png"), 1280, 720);

            Debug.Log($"[BearChaseVerificationCapture] Capturas salvas em {OutputDir}");
        }

        private static void UpdateAll(
            BearChaseSimulation sim,
            BearChaseCameraController camCtrl,
            BearChasePlayerController player,
            BearChaseBearAnimator bear,
            BearChaseNandaController nanda,
            BearChaseHudController hud,
            Vector3 yunaStart,
            Vector3 yunaNudge,
            Vector3 bearSleep,
            Vector3 chaseOrigin,
            float dt)
        {
            player.UpdatePlayer(sim, yunaStart, yunaNudge, chaseOrigin, dt);
            bear.UpdateBear(sim, bearSleep, chaseOrigin, dt);
            nanda?.UpdateNanda(sim.Phase, dt);
            Vector3 yunaChasePos = chaseOrigin + new Vector3(
                BearChaseConfig.TrackCenterX(sim.YunaZ) + sim.YunaLaneOffsetX,
                0f,
                sim.YunaZ);
            camCtrl?.UpdateCamera(sim, yunaChasePos, dt);
            hud?.UpdateVisualState(sim);
        }

        private static void RenderToPng(Camera cam, string path, int width, int height)
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
