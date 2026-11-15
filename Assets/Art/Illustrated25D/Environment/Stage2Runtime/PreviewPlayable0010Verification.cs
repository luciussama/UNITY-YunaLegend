using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.Illustrated25D.Preview
{
    /// <summary>
    /// Verificacao automatizada do fluxo jogavel 00..10 (abertura, falas,
    /// corrida 00..09, cutscene do Castelo 09, transicao Fase 2 e pouso na Plataforma 10).
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public sealed class PreviewPlayable0010Verification : MonoBehaviour
    {
        [Serializable]
        public sealed class Report
        {
            public bool success;
            public int hz = 60;
            public int errors;
            public int maxLegacy;
            public bool openingStepsOk;
            public bool bear, books, furniture, dresser, blocks, drum, cushion, drawers, musicBox, castle;
            public bool castleCutsceneOk;
            public bool stage2Started;
            public bool trainTrestle10Landed;
            public bool stage2Completed;
            public float trainTrestle10X;
            public float trainTrestle10TopY;
            public float trainTrestle10GapFromCastle;
            public float trainTrestle10LandingX;
            public string error;
        }

        private readonly Report report = new Report();
        private IllustratedPreview preview;
        private LegacyPlayableStageFlow flow;
        private Keyboard keyboard;
        private string output;
        private int tick;
        private int releaseAt = -1;
        private int stage2WaitTicks;
        private int finishAt = -1;
        private bool j1, j2, j3, j4, j5, j6, j7, j8, j9, j10, j11;
        private bool shotOpen1, shotOpen2, shotOpen3, shotCue, shotCastle0, shotCastle1, shotP10, shotDone;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            var args = System.Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-previewPlayable0010Verify");
            if (at < 0 || at + 1 >= args.Length || FindFirstObjectByType<PreviewPlayable0010Verification>() != null) return;

            var go = new GameObject("Playable0010_Verification");
            DontDestroyOnLoad(go);
            var test = go.AddComponent<PreviewPlayable0010Verification>();
            test.output = args[at + 1];
            Directory.CreateDirectory(test.output);

            int hzAt = Array.IndexOf(args, "-previewHz");
            if (hzAt >= 0 && hzAt + 1 < args.Length) test.report.hz = int.Parse(args[hzAt + 1]);

            Time.captureDeltaTime = 1f / test.report.hz;
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            test.keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.EnableDevice(test.keyboard);
            Application.logMessageReceived += test.OnLog;
        }

        private void LateUpdate()
        {
            if (preview == null)
            {
                preview = FindFirstObjectByType<IllustratedPreview>();
                if (preview == null || !preview.Ready) { preview = null; return; }
                flow = preview.PlayableFlow;
                if (!preview.playableLegacyMode || flow == null || preview.TrainTrestle10 == null)
                {
                    End("Playable 00..10 flow or Platform 10 missing");
                    return;
                }

                var castleMarker = preview.Castle.GetComponent<DarkRoomPlatformMarker>();
                var p10Marker = preview.TrainTrestle10.GetComponent<DarkRoomPlatformMarker>();
                report.trainTrestle10X = preview.TrainTrestle10.transform.position.x;
                report.trainTrestle10TopY = p10Marker.GetTopCenterWorld().y;
                report.trainTrestle10GapFromCastle = p10Marker.Support.MinX - castleMarker.Support.MaxX;

                tick = 0;
                Press();
            }

            tick++;
            if (tick == releaseAt) Press();

            var p = preview.Player;
            report.maxLegacy = Math.Max(report.maxLegacy, preview.LegacyRenderersVisible);

            // Step through opening dialogues & capture screenshots
            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.OpeningNandaWake1 && !shotOpen1 && tick >= 4)
            {
                shotOpen1 = true;
                StartCoroutine(Shot("01-opening-nanda-1.png"));
            }
            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.OpeningNandaWake1 && tick == 10)
                flow.AdvanceStepForAutomation();

            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.OpeningPoeticIntro && !shotOpen2)
            {
                shotOpen2 = true;
                StartCoroutine(Shot("02-opening-poetic.png"));
            }
            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.OpeningPoeticIntro && tick == 18)
                flow.AdvanceStepForAutomation();

            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.OpeningNandaWake2 && !shotOpen3)
            {
                shotOpen3 = true;
                StartCoroutine(Shot("03-opening-nanda-2.png"));
            }
            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.OpeningNandaWake2 && tick == 26)
                flow.AdvanceStepForAutomation();

            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.PreJumpCue && !shotCue)
            {
                shotCue = true;
                report.openingStepsOk = shotOpen1 && shotOpen2 && shotOpen3;
                StartCoroutine(Shot("04-prejump-cue.png"));
            }
            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.PreJumpCue && tick == 34)
                flow.AdvanceStepForAutomation();

            // Stage 1 jumps (00..09)
            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.Stage1Gameplay)
            {
                if (!j1 && p.transform.position.x >= -0.64f) { j1 = true; Jump(); }
                if (p.CurrentSupportIndex == 0) report.bear = true;
                if (report.bear && !j2 && p.transform.position.x >= 2.72f) { j2 = true; Jump(); }
                if (p.CurrentSupportIndex == 1) report.books = true;
                if (report.books && !j3 && p.IsGrounded && p.transform.position.x >= 5.95f) { j3 = true; Jump(); }
                if (p.CurrentSupportIndex == 2) report.furniture = true;

                var furnMarker = preview.Furniture.GetComponent<DarkRoomPlatformMarker>();
                if (report.furniture && !j4 && p.IsGrounded && !p.IsStumbling && preview.Furniture.HasCracked
                    && p.transform.position.x >= furnMarker.Support.MaxX - 0.45f) { j4 = true; Jump(); }
                if (p.CurrentSupportIndex == 3 && p.IsGrounded) report.dresser = true;

                var dresserMarker = preview.Dresser.GetComponent<DarkRoomPlatformMarker>();
                if (report.dresser && !j5 && p.IsGrounded && p.CurrentSupportIndex == 3
                    && p.transform.position.x >= dresserMarker.Support.MaxX - 0.45f) { j5 = true; Jump(); }
                if (p.CurrentSupportIndex == 4 && p.IsGrounded) report.blocks = true;

                var abcMarker = preview.Blocks.GetComponent<DarkRoomPlatformMarker>();
                if (report.blocks && !j6 && p.IsGrounded && p.CurrentSupportIndex == 4
                    && p.transform.position.x >= abcMarker.Support.MaxX - 0.35f) { j6 = true; Jump(); }
                if (p.CurrentSupportIndex == 5 && p.IsGrounded) report.drum = true;

                var drumMarker = preview.Drum.GetComponent<DarkRoomPlatformMarker>();
                if (report.drum && !j7 && p.IsGrounded && p.CurrentSupportIndex == 5
                    && p.transform.position.x >= drumMarker.Support.MaxX - 0.40f) { j7 = true; Jump(); }
                if (p.CurrentSupportIndex == 6 && p.IsGrounded) report.cushion = true;

                var cushionMarker = preview.Cushion.GetComponent<DarkRoomPlatformMarker>();
                if (report.cushion && !j8 && p.IsGrounded && p.CurrentSupportIndex == 6
                    && p.transform.position.x >= cushionMarker.Support.MaxX - 0.40f) { j8 = true; Jump(); }
                if (p.CurrentSupportIndex == 7 && p.IsGrounded) report.drawers = true;

                var drawersMarker = preview.Drawers.GetComponent<DarkRoomPlatformMarker>();
                if (report.drawers && !j9 && p.IsGrounded && p.CurrentSupportIndex == 7
                    && p.transform.position.x >= drawersMarker.Support.MaxX - 0.35f) { j9 = true; Jump(); }
                if (p.CurrentSupportIndex == 8 && p.IsGrounded) report.musicBox = true;

                var mbMarker = preview.MusicBox.GetComponent<DarkRoomPlatformMarker>();
                if (report.musicBox && !j10 && p.IsGrounded && p.CurrentSupportIndex == 8
                    && p.transform.position.x >= mbMarker.Support.MaxX - 0.25f) { j10 = true; Jump(); }
                if (p.CurrentSupportIndex == 9 && p.IsGrounded) report.castle = true;
            }

            // Castle 09 Rest Cutscene
            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.CastleCutsceneStep0)
            {
                stage2WaitTicks++;
                if (!shotCastle0 && stage2WaitTicks >= 6)
                {
                    shotCastle0 = true;
                    StartCoroutine(Shot("05-castle-cutscene-nanda.png"));
                }
                if (stage2WaitTicks >= 16)
                {
                    stage2WaitTicks = 0;
                    flow.AdvanceStepForAutomation();
                }
            }
            else if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.CastleCutsceneStep1)
            {
                stage2WaitTicks++;
                if (!shotCastle1 && stage2WaitTicks >= 6)
                {
                    shotCastle1 = true;
                    StartCoroutine(Shot("06-castle-cutscene-yuna.png"));
                }
                if (stage2WaitTicks >= 16)
                {
                    stage2WaitTicks = 0;
                    report.castleCutsceneOk = shotCastle0 && shotCastle1;
                    flow.AdvanceStepForAutomation();
                }
            }

            // Stage 2 Gameplay -> Jump from Castle 09 onto TrainTrestle 10
            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.Stage2Gameplay)
            {
                report.stage2Started = true;
                var castleMarker = preview.Castle.GetComponent<DarkRoomPlatformMarker>();
                if (!j11 && p.IsGrounded && p.CurrentSupportIndex == 9
                    && p.transform.position.x >= castleMarker.Support.MaxX - 0.25f)
                {
                    j11 = true;
                    Jump();
                }

                if (p.CurrentSupportIndex == 10 && p.IsGrounded && !report.trainTrestle10Landed)
                {
                    report.trainTrestle10Landed = true;
                    report.trainTrestle10LandingX = p.transform.position.x;
                    if (!shotP10)
                    {
                        shotP10 = true;
                        StartCoroutine(Shot("07-stage2-platform10-land.png"));
                    }
                }
            }

            if (flow.Phase == LegacyPlayableStageFlow.FlowPhase.Stage2Completed)
            {
                report.stage2Completed = true;
                if (!shotDone)
                {
                    shotDone = true;
                    StartCoroutine(Shot("08-stage2-completed.png"));
                }
                if (finishAt < 0) finishAt = tick + 12;
                if (tick >= finishAt)
                {
                    End(null);
                    return;
                }
            }

            if (p.IsDead)
            {
                End($"Yuna died at phase={flow.Phase} support={p.CurrentSupportIndex} x={p.transform.position.x:F2}");
                return;
            }
            if (tick > report.hz * 60)
            {
                End($"Timeout at phase={flow.Phase} support={p.CurrentSupportIndex} x={p.transform.position.x:F2}");
            }
        }

        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            var image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(output, name), image.EncodeToPNG());
            Destroy(image);
        }

        private void Jump()
        {
            Press(Key.Space);
            releaseAt = tick + Mathf.Max(2, Mathf.RoundToInt(0.12f * report.hz));
        }

        private void Press(params Key[] keys)
        {
            if (keyboard == null) return;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLog;
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
        }

        private void OnLog(string msg, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors++;
        }

        private void End(string error)
        {
            Application.logMessageReceived -= OnLog;
            report.error = error;
            report.success = string.IsNullOrEmpty(error)
                && report.errors == 0
                && report.maxLegacy == 0
                && report.openingStepsOk
                && report.castleCutsceneOk
                && report.stage2Started
                && report.trainTrestle10Landed
                && report.stage2Completed;

            File.WriteAllText(
                Path.Combine(output, $"playable-00-10-{report.hz}hz.json"),
                JsonUtility.ToJson(report, true));
            Debug.Log($"PLAYABLE_00_10_VERIFY success={report.success} error={report.error}");
            Application.Quit(report.success ? 0 : 1);
        }
    }
}
