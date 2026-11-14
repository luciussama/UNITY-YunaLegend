using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.Illustrated25D.Preview
{
    /// <summary>Opt-in ten-jump route and real reload proof for Music Box 08 + Castle 09 (with 2.75 m tension jump) and extended room background.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class PreviewCastle09Verification : MonoBehaviour
    {
        [Serializable] public class Attempt
        {
            public bool bear, books, furniture, dresser, blocks, drum, cushion, drawers, musicBox, castle, finished;
            public int jumps, cracks;
            public float maxMusicBoxAnchorGap, maxCastleAnchorGap, musicBoxLandingX, castleLandingX;
        }
        [Serializable] public class Report
        {
            public bool success, resetClean;
            public int hz = 60, errors, maxLegacy, roomPanels;
            public float musicBoxX, musicBoxTop, musicBoxSupportGap, castleX, castleTop, castleTensionGap;
            public float idleVisualMin, idleGapToSupport, landingVisualMin, landingGapToSupport;
            public string error;
            public List<Attempt> attempts = new List<Attempt>();
        }
        readonly Report report = new Report();
        IllustratedPreview preview;
        Keyboard keyboard;
        string output;
        int tick, releaseAt = -1, finishAt = -1, index, reloadWait;
        bool j1, j2, j3, j4, j5, j6, j7, j8, j9, j10, awaitingReload, tensionShot;
        Attempt attempt;
        DresserFootAlignment musicBoxFeet, castleFeet;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = System.Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-previewCastle09Verify");
            if (at < 0 || at + 1 >= args.Length || FindFirstObjectByType<PreviewCastle09Verification>() != null) return;
            var go = new GameObject("Castle09_ReadOnlyVerification"); DontDestroyOnLoad(go);
            var test = go.AddComponent<PreviewCastle09Verification>(); test.output = args[at + 1]; Directory.CreateDirectory(test.output);
            int hzAt = Array.IndexOf(args, "-previewHz"); if (hzAt >= 0) test.report.hz = int.Parse(args[hzAt + 1]);
            Time.captureDeltaTime = 1f / test.report.hz; Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            test.keyboard = InputSystem.AddDevice<Keyboard>(); InputSystem.EnableDevice(test.keyboard);
            Application.logMessageReceived += test.OnLog;
        }

        void LateUpdate()
        {
            if (awaitingReload && preview != null)
            { if (++reloadWait > report.hz * 5) End("R reload timeout"); return; }
            if (preview == null)
            {
                preview = FindFirstObjectByType<IllustratedPreview>();
                if (preview == null || !preview.Ready) { preview = null; return; }
                if (!preview.includeNextCastle09 || preview.MusicBox == null || preview.Castle == null) { End("08/09 scene missing"); return; }
                tick = 0; finishAt = -1; releaseAt = -1;
                j1 = j2 = j3 = j4 = j5 = j6 = j7 = j8 = j9 = j10 = tensionShot = false;
                awaitingReload = false; reloadWait = 0;
                attempt = new Attempt(); report.attempts.Add(attempt); Press();

                var drawers = preview.Drawers.GetComponent<DarkRoomPlatformMarker>();
                var mb = preview.MusicBox.GetComponent<DarkRoomPlatformMarker>();
                var castle = preview.Castle.GetComponent<DarkRoomPlatformMarker>();
                report.roomPanels = preview.Room != null ? preview.Room.FarPanels.Count : 0;
                report.musicBoxX = preview.MusicBox.transform.position.x;
                report.musicBoxTop = mb.GetTopCenterWorld().y;
                report.musicBoxSupportGap = mb.Support.MinX - drawers.Support.MaxX;
                report.castleX = preview.Castle.transform.position.x;
                report.castleTop = castle.GetTopCenterWorld().y;
                report.castleTensionGap = castle.Support.MinX - mb.Support.MaxX;

                var feet = preview.Player.GetComponents<DresserFootAlignment>();
                musicBoxFeet = feet.FirstOrDefault(f => f.supportIndex == 8);
                castleFeet = feet.FirstOrDefault(f => f.supportIndex == 9);
                if (musicBoxFeet == null || castleFeet == null) { End("Foot alignment components missing"); return; }
                if (index > 0) report.resetClean = !preview.Started && !preview.Furniture.HasCracked && !preview.Player.IsDead && !preview.Player.IsStumbling;
            }
            tick++;
            if (tick == releaseAt) Press();
            if (tick == report.hz / 2) preview.Begin();
            var p = preview.Player;
            if (preview.Started && !j1 && p.transform.position.x >= -.64f) { j1 = true; Jump(); }
            if (p.CurrentSupportIndex == 0) attempt.bear = true;
            if (attempt.bear && !j2 && p.transform.position.x >= 2.72f) { j2 = true; Jump(); }
            if (p.CurrentSupportIndex == 1) attempt.books = true;
            if (attempt.books && !j3 && p.IsGrounded && p.transform.position.x >= 5.95f) { j3 = true; Jump(); }
            if (p.CurrentSupportIndex == 2) attempt.furniture = true;
            var previous = preview.Furniture.GetComponent<DarkRoomPlatformMarker>();
            if (attempt.furniture && !j4 && p.IsGrounded && !p.IsStumbling && preview.Furniture.HasCracked
                && p.transform.position.x >= previous.Support.MaxX - .45f) { j4 = true; Jump(); }
            if (p.CurrentSupportIndex == 3 && p.IsGrounded) attempt.dresser = true;
            var dresserMarker = preview.Dresser.GetComponent<DarkRoomPlatformMarker>();
            if (attempt.dresser && !j5 && p.IsGrounded && p.CurrentSupportIndex == 3
                && p.transform.position.x >= dresserMarker.Support.MaxX - .45f) { j5 = true; Jump(); }
            if (p.CurrentSupportIndex == 4 && p.IsGrounded) attempt.blocks = true;
            var abcMarker = preview.Blocks.GetComponent<DarkRoomPlatformMarker>();
            if (attempt.blocks && !j6 && p.IsGrounded && p.CurrentSupportIndex == 4
                && p.transform.position.x >= abcMarker.Support.MaxX - .35f) { j6 = true; Jump(); }
            if (p.CurrentSupportIndex == 5 && p.IsGrounded) attempt.drum = true;
            var drumMarker = preview.Drum.GetComponent<DarkRoomPlatformMarker>();
            if (attempt.drum && !j7 && p.IsGrounded && p.CurrentSupportIndex == 5
                && p.transform.position.x >= drumMarker.Support.MaxX - .40f) { j7 = true; Jump(); }
            if (p.CurrentSupportIndex == 6 && p.IsGrounded) attempt.cushion = true;
            var cushionMarker = preview.Cushion.GetComponent<DarkRoomPlatformMarker>();
            if (attempt.cushion && !j8 && p.IsGrounded && p.CurrentSupportIndex == 6
                && p.transform.position.x >= cushionMarker.Support.MaxX - .40f) { j8 = true; Jump(); }
            if (p.CurrentSupportIndex == 7 && p.IsGrounded) attempt.drawers = true;
            var drawersMarker = preview.Drawers.GetComponent<DarkRoomPlatformMarker>();
            if (attempt.drawers && !j9 && p.IsGrounded && p.CurrentSupportIndex == 7
                && p.transform.position.x >= drawersMarker.Support.MaxX - .35f) { j9 = true; Jump(); }
            if (p.CurrentSupportIndex == 8 && p.IsGrounded)
            {
                attempt.maxMusicBoxAnchorGap = Mathf.Max(attempt.maxMusicBoxAnchorGap, Mathf.Abs(musicBoxFeet.AnchorGap));
                if (!attempt.musicBox)
                {
                    attempt.musicBox = true; attempt.musicBoxLandingX = p.transform.position.x;
                    StartCoroutine(Shot("attempt-" + index + "-musicbox-land.png"));
                }
            }
            var mbMarker = preview.MusicBox.GetComponent<DarkRoomPlatformMarker>();
            if (attempt.musicBox && !j10 && p.IsGrounded && p.CurrentSupportIndex == 8
                && p.transform.position.x >= mbMarker.Support.MaxX - .25f) { j10 = true; Jump(); }
            if (j10 && !attempt.castle && !tensionShot && p.transform.position.x >= mbMarker.Support.MaxX + 1.1f)
            {
                tensionShot = true;
                StartCoroutine(Shot("attempt-" + index + "-tension-jump.png"));
            }
            if (p.CurrentSupportIndex == 9 && p.IsGrounded)
            {
                attempt.maxCastleAnchorGap = Mathf.Max(attempt.maxCastleAnchorGap, Mathf.Abs(castleFeet.AnchorGap));
                if (!attempt.castle)
                {
                    attempt.castle = true; attempt.castleLandingX = p.transform.position.x;
                    var lr = preview.Player.GetComponent<YunaLegend.YunaIllustration.YunaIllustrationVisibilityResolver>();
                    var lp = lr.VisibleSource;
                    var lrend = lp == "Idle" ? lr.idleRenderer : lp == "Jump" ? lr.jump.painting : lp == "Stumble" ? lr.stumble.painting : lr.run.painting;
                    report.landingVisualMin = lrend.bounds.min.y;
                    report.landingGapToSupport = report.landingVisualMin - report.castleTop;
                    StartCoroutine(Shot("attempt-" + index + "-castle-land.png"));
                }
            }
            attempt.cracks = preview.Furniture.CrackCount;
            report.maxLegacy = Math.Max(report.maxLegacy, preview.LegacyRenderersVisible);
            if (preview.Finished && finishAt < 0) { attempt.finished = true; finishAt = tick + report.hz; }
            if (finishAt > 0 && tick == finishAt - 2)
            {
                var resolver = preview.Player.GetComponent<YunaLegend.YunaIllustration.YunaIllustrationVisibilityResolver>();
                report.idleVisualMin = resolver.idleRenderer.bounds.min.y;
                report.idleGapToSupport = report.idleVisualMin - report.castleTop;
                StartCoroutine(Shot("attempt-" + index + "-castle-idle.png"));
            }
            if (finishAt > 0 && tick >= finishAt)
            {
                if (index == 0) { index++; awaitingReload = true; Press(Key.R); return; }
                End(null); return;
            }
            if (p.IsDead) { StartCoroutine(Shot("failure.png")); End("Player died x=" + p.transform.position.x + " support=" + p.CurrentSupportIndex); }
            else if (tick > report.hz * 65) End("Timeout x=" + p.transform.position.x + " support=" + p.CurrentSupportIndex);
        }

        void Jump() { attempt.jumps++; Press(Key.Space); releaseAt = tick + Mathf.RoundToInt(.12f * report.hz); }
        void Press(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            var image = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(output, name), image.EncodeToPNG()); Destroy(image);
        }
        void End(string error)
        {
            report.error = error;
            report.success = error == null && report.errors == 0 && report.resetClean && report.maxLegacy == 0 && report.roomPanels >= 3 && report.attempts.Count == 2
                && report.attempts.All(a => a.bear && a.books && a.furniture && a.dresser && a.blocks && a.drum && a.cushion && a.drawers
                    && a.musicBox && a.castle && a.finished && a.jumps == 10 && a.cracks == 1 && a.maxMusicBoxAnchorGap < .005f && a.maxCastleAnchorGap < .005f);
            File.WriteAllText(Path.Combine(output, "verification.json"), JsonUtility.ToJson(report, true));
            Debug.Log("CASTLE09_VERIFY " + report.success + " " + error); enabled = false; Application.Quit(report.success ? 0 : 1);
        }
        void OnLog(string message, string stack, LogType type) { if (type == LogType.Exception || type == LogType.Error) report.errors++; }
        void OnDestroy() { Application.logMessageReceived -= OnLog; if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard); }
    }
}
