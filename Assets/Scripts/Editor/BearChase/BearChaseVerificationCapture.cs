using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.BearChase.Core;
using YunaLegend.BearChase.Runtime;

namespace YunaLegend.Editor.BearChase
{
    /// <summary>
    /// Utilitario de captura visual HDR em batchmode cobrindo todos os criterios P1-P6
    /// para auditoria black-box do Art Director Agent.
    /// </summary>
    public static class BearChaseVerificationCapture
    {
        private const string DefaultOutputDir = "Evidence/bear-chase-audit";

        public static void BuildAndCaptureAll()
        {
            BearChaseSceneBuilder.BuildBearChaseScene();
            EditorSceneManager.OpenScene(BearChaseSceneBuilder.ScenePath, OpenSceneMode.Single);

            string outputDir = Environment.GetEnvironmentVariable("BEAR_CHASE_CAPTURE_DIR");
            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = DefaultOutputDir;
            }

            Directory.CreateDirectory(outputDir);

            Camera cam = Camera.main;
            var camCtrl = UnityEngine.Object.FindFirstObjectByType<BearChaseCameraController>(FindObjectsInactive.Include);
            var entranceCtrl = UnityEngine.Object.FindFirstObjectByType<BearChaseEntranceController>(FindObjectsInactive.Include);
            var sceneryCtrl = UnityEngine.Object.FindFirstObjectByType<BearChaseSceneryController>(FindObjectsInactive.Include);
            var lightingCtrl = UnityEngine.Object.FindFirstObjectByType<BearChaseLightingController>(FindObjectsInactive.Include);
            var obstacleCtrl = UnityEngine.Object.FindFirstObjectByType<BearChaseObstacleController>(FindObjectsInactive.Include);
            var player = UnityEngine.Object.FindFirstObjectByType<BearChasePlayerController>(FindObjectsInactive.Include);
            var bear = UnityEngine.Object.FindFirstObjectByType<BearChaseBearAnimator>(FindObjectsInactive.Include);
            var nanda = UnityEngine.Object.FindFirstObjectByType<BearChaseNandaController>(FindObjectsInactive.Include);
            var hud = UnityEngine.Object.FindFirstObjectByType<BearChaseHudController>(FindObjectsInactive.Include);
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<BearChaseStageBootstrap>(FindObjectsInactive.Include);

            if (cam == null || player == null || bear == null || entranceCtrl == null || sceneryCtrl == null || lightingCtrl == null)
            {
                Debug.LogError("[BearChaseVerificationCapture] Componentes principais ausentes na cena.");
                return;
            }

            Vector3 chaseOrigin = bootstrap != null ? bootstrap.ChaseOrigin : new Vector3(0f, 0f, 40f);
            var sim = new BearChaseSimulation();
            lightingCtrl.ApplyAtmosphere();
            entranceCtrl.EnsureBuilt();
            entranceCtrl.SetCaptureCamera(cam);
            hud?.EnsureBuilt();
            hud?.SetHearts(sim.Hearts, sim.MaxHearts);
            hud?.SetCaptureCamera(cam);
            Canvas.ForceUpdateCanvases();

            try
            {
                // 1-4: Sequencia completa da Entrada 2D Ilustrada (Estabelecimento -> Aproximacao -> Toque -> Despertar)
                CaptureAtEntranceTime(sim, 0.25f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin, cam, Path.Combine(outputDir, "01_entrance_start.png"));
                CaptureAtEntranceTime(sim, 2.35f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin, cam, Path.Combine(outputDir, "02_entrance_approach.png"));
                CaptureAtEntranceTime(sim, 4.65f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin, cam, Path.Combine(outputDir, "03_entrance_reach.png"));
                CaptureAtEntranceTime(sim, 5.45f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin, cam, Path.Combine(outputDir, "04_entrance_wakeup.png"));

                // Diagnostico de entrada (4 quadros consecutivos da caminhada 0 -> 1 -> 2 -> 3)
                CaptureAtEntranceTime(sim, 1.32f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin, cam, Path.Combine(outputDir, "diag_entrance_walk_f0.png"));
                CaptureAtEntranceTime(sim, 1.50f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin, cam, Path.Combine(outputDir, "diag_entrance_walk_f1.png"));
                CaptureAtEntranceTime(sim, 1.68f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin, cam, Path.Combine(outputDir, "diag_entrance_walk_f2.png"));
                CaptureAtEntranceTime(sim, 1.86f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin, cam, Path.Combine(outputDir, "diag_entrance_walk_f3.png"));

                // 5: Corrida com urso arremessando brinquedo em arco medio + sombra no piso + pista livre de estaticos
                sim.ResetFullStage(skipEntrance: true, useEasyMode: false);
                hud?.SetHearts(sim.Hearts, sim.MaxHearts);
                bool capturedThrow = false;
                for (int i = 0; i < 650; i++)
                {
                    SteerAwayFromNearestToy(sim);
                    StepAndSync(sim, 1f / 60f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin);
                    if (!capturedThrow && HasMidFlightToy(sim))
                    {
                        Canvas.ForceUpdateCanvases();
                        RenderHdrToPng(cam, Path.Combine(outputDir, "05_chase_throw_arc.png"), 1280, 720);
                        capturedThrow = true;
                        break;
                    }
                }

                // 6: Trecho de curva S + ondulacao vertical pronunciada (z ~ 85m+) com Yuna 100% visivel
                for (int i = 0; i < 420; i++)
                {
                    SteerAwayFromNearestToy(sim);
                    StepAndSync(sim, 1f / 60f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin);
                    if (sim.YunaZ >= 86f && sim.InvulnerabilityRemaining <= 0f)
                    {
                        break;
                    }
                }

                Canvas.ForceUpdateCanvases();
                RenderHdrToPng(cam, Path.Combine(outputDir, "06_chase_curve_elevation.png"), 1280, 720);

                // 7: Iluminacao atmosferica (pocas solares das janelas + brilho azul-ciano de Nanda + Yuna visivel)
                for (int i = 0; i < 160; i++)
                {
                    SteerAwayFromNearestToy(sim);
                    StepAndSync(sim, 1f / 60f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin);
                    if (i > 60 && sim.InvulnerabilityRemaining <= 0f)
                    {
                        break;
                    }
                }

                Canvas.ForceUpdateCanvases();
                RenderHdrToPng(cam, Path.Combine(outputDir, "07_chase_lighting_nanda.png"), 1280, 720);

                // 8: Close-up 3/4 dianteiro-lateral do urso dourado-mel erguendo a pata para arremessar o brinquedo
                for (int i = 0; i < 240; i++)
                {
                    SteerAwayFromNearestToy(sim);
                    StepAndSync(sim, 1f / 60f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin);
                    if (sim.BearAction == BearChaseBearAction.ThrowingToy && sim.BearThrowWindupProgress > 0.45f)
                    {
                        break;
                    }
                }

                Vector3 bearPos = bear.transform.position;
                Vector3 bearFwd = bear.transform.forward;
                Vector3 bearRight = bear.transform.right;
                cam.transform.position = bearPos + bearFwd * 2.05f + bearRight * 1.25f + Vector3.up * 0.95f;
                cam.transform.LookAt(bearPos + Vector3.up * 0.56f);
                Canvas.ForceUpdateCanvases();
                RenderHdrToPng(cam, Path.Combine(outputDir, "08_chase_bear_action.png"), 1280, 720);

                // Diagnostico de perseguição tardia (84% e 93% da pista) para verificar posicao relativa Yuna x Urso
                sim.ResetFullStage(skipEntrance: true, useEasyMode: false);
                hud?.SetHearts(sim.Hearts, sim.MaxHearts);
                bool captured84 = false;
                bool captured93 = false;
                for (int i = 0; i < 3600; i++)
                {
                    SteerAwayFromNearestToy(sim);
                    StepAndSync(sim, 1f / 60f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin);
                    if (!captured84 && sim.NormalizedProgress >= 0.84f)
                    {
                        Debug.Log($"[BearChaseVerificationCapture] diag 84% progress={sim.NormalizedProgress:F3} YunaZ={sim.YunaZ:F2} BearZ={sim.BearZ:F2} Lead={sim.BearZ - sim.YunaZ:F2}");
                        Canvas.ForceUpdateCanvases();
                        RenderHdrToPng(cam, Path.Combine(outputDir, "diag_chase_late_84pct.png"), 1280, 720);
                        captured84 = true;
                    }

                    if (!captured93 && sim.NormalizedProgress >= 0.93f)
                    {
                        Debug.Log($"[BearChaseVerificationCapture] diag 93% progress={sim.NormalizedProgress:F3} YunaZ={sim.YunaZ:F2} BearZ={sim.BearZ:F2} Lead={sim.BearZ - sim.YunaZ:F2}");
                        Canvas.ForceUpdateCanvases();
                        RenderHdrToPng(cam, Path.Combine(outputDir, "diag_chase_late_93pct.png"), 1280, 720);
                        captured93 = true;
                        break;
                    }

                    if (sim.Phase == BearChasePhase.Victory || sim.Phase == BearChasePhase.Fallen || sim.Phase == BearChasePhase.RetryModal)
                    {
                        break;
                    }
                }
            }
            finally
            {
                entranceCtrl.SetCaptureCamera(null);
                hud?.SetCaptureCamera(null);
            }

            Debug.Log($"[BearChaseVerificationCapture] 8 capturas salvas em {outputDir}");
        }

        private static bool HasMidFlightToy(BearChaseSimulation sim)
        {
            for (int i = 0; i < sim.ActiveThrownToys.Count; i++)
            {
                BearChaseThrownToyState t = sim.ActiveThrownToys[i];
                if (t.Phase == BearChaseThrownToyPhase.InFlight && t.NormalizedFlight >= 0.44f && t.NormalizedFlight <= 0.68f)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SteerAwayFromNearestToy(BearChaseSimulation sim)
        {
            float[] lanes = { -1.85f, 0f, 1.85f };
            float bestLane = 0f;
            float bestClearance = -1f;

            for (int l = 0; l < lanes.Length; l++)
            {
                float candidate = lanes[l];
                float minDz = float.MaxValue;
                for (int i = 0; i < sim.ActiveThrownToys.Count; i++)
                {
                    BearChaseThrownToyState t = sim.ActiveThrownToys[i];
                    if (t.Phase == BearChaseThrownToyPhase.Deflected || t.Phase == BearChaseThrownToyPhase.Expired)
                    {
                        continue;
                    }

                    float lane = t.Phase == BearChaseThrownToyPhase.InFlight ? t.TargetLaneX : t.CurrentLaneX;
                    if (Mathf.Abs(lane - candidate) < 0.95f)
                    {
                        float dz = t.CurrentZ - sim.YunaZ;
                        if (dz >= -0.6f && dz < minDz)
                        {
                            minDz = dz;
                        }
                    }
                }

                if (minDz > bestClearance)
                {
                    bestClearance = minDz;
                    bestLane = candidate;
                }
            }

            float diff = bestLane - sim.YunaLaneOffsetX;
            sim.SetLateralInput(Mathf.Abs(diff) < 0.05f ? 0f : Mathf.Sign(diff));
        }

        private static void CaptureAtEntranceTime(
            BearChaseSimulation sim,
            float targetTime,
            BearChaseEntranceController entranceCtrl,
            BearChaseSceneryController sceneryCtrl,
            BearChaseLightingController lightingCtrl,
            BearChaseObstacleController obstacleCtrl,
            BearChaseCameraController camCtrl,
            BearChasePlayerController player,
            BearChaseBearAnimator bear,
            BearChaseNandaController nanda,
            BearChaseHudController hud,
            Vector3 chaseOrigin,
            Camera cam,
            string filePath)
        {
            sim.ResetFullStage(skipEntrance: false, useEasyMode: false);
            int steps = Mathf.RoundToInt(targetTime * 60f);
            for (int i = 0; i < steps; i++)
            {
                StepAndSync(sim, 1f / 60f, entranceCtrl, sceneryCtrl, lightingCtrl, obstacleCtrl, camCtrl, player, bear, nanda, hud, chaseOrigin);
            }

            Canvas.ForceUpdateCanvases();
            RenderHdrToPng(cam, filePath, 1280, 720);
        }

        private static void StepAndSync(
            BearChaseSimulation sim,
            float dt,
            BearChaseEntranceController entranceCtrl,
            BearChaseSceneryController sceneryCtrl,
            BearChaseLightingController lightingCtrl,
            BearChaseObstacleController obstacleCtrl,
            BearChaseCameraController camCtrl,
            BearChasePlayerController player,
            BearChaseBearAnimator bear,
            BearChaseNandaController nanda,
            BearChaseHudController hud,
            Vector3 chaseOrigin)
        {
            sim.Step(dt);
            sceneryCtrl.ApplyPhaseVisibility(sim.Phase);
            entranceCtrl.ApplySample(sim.EntranceSample, sim.IsEntrancePhase);
            player.UpdatePlayer(sim, chaseOrigin, dt);
            bear.UpdateBear(sim, chaseOrigin, dt);
            nanda?.UpdateNanda(sim, chaseOrigin, dt);
            obstacleCtrl?.SyncToys(sim.ActiveThrownToys, chaseOrigin, sim.TrackSpec, !sim.IsEntrancePhase);
            lightingCtrl.UpdateWindowLights(sim);
            camCtrl?.UpdateCamera(sim, chaseOrigin, dt);
            hud?.HandlePhaseChanged(sim.Phase, sim.ShouldOfferEasyMode);
            hud?.UpdatePerFrameOverlays(sim);
        }

        private static void RenderHdrToPng(Camera cam, string path, int width, int height)
        {
            var hdr = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            var display = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture prevTarget = cam.targetTexture;
            RenderTexture prevActive = RenderTexture.active;
            bool prevSrgb = GL.sRGBWrite;

            try
            {
                cam.aspect = width / (float)height;
                cam.targetTexture = hdr;
                cam.Render();

                GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                Graphics.Blit(hdr, display);
                RenderTexture.active = display;

                var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
            }
            finally
            {
                cam.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                GL.sRGBWrite = prevSrgb;
                UnityEngine.Object.DestroyImmediate(hdr);
                UnityEngine.Object.DestroyImmediate(display);
            }
        }
    }
}
