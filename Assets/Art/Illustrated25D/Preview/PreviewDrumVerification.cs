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
    /// <summary>Opt-in six-jump route and real reload proof for Drum 05 after the ABC stack.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class PreviewDrumVerification : MonoBehaviour
    {
        [Serializable] public class Attempt
        {
            public bool bear, books, furniture, dresser, blocks, drum, finished;
            public int jumps, cracks;
            public float maxAbcAnchorGap, maxDrumAnchorGap, abcLandingX, drumLandingX;
        }
        [Serializable] public class Report
        {
            public bool success, resetClean;
            public int hz = 60, errors, maxLegacy;
            public float abcX, abcTop, drumX, drumTop, drumSupportWidth, drumSupportMin, drumSupportMax, drumSupportGap, drumStepUp;
            public float idleVisualMin, idleGapToSupport, landingVisualMin, landingGapToSupport;
            public string landingPose;
            public int drumSortingOrder, idleSortingOrder;
            public string error;
            public List<Attempt> attempts = new List<Attempt>();
        }
        readonly Report report = new Report();
        IllustratedPreview preview;
        Keyboard keyboard;
        string output;
        int tick, releaseAt = -1, finishAt = -1, index, reloadWait;
        bool first, second, third, fourth, fifth, sixth;
        bool awaitingReload;
        Attempt attempt;
        DresserFootAlignment abcFeet, drumFeet;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = System.Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-previewDrumVerify");
            if (at < 0 || at + 1 >= args.Length || FindFirstObjectByType<PreviewDrumVerification>() != null) return;
            var go = new GameObject("Drum_ReadOnlyVerification"); DontDestroyOnLoad(go);
            var test = go.AddComponent<PreviewDrumVerification>(); test.output = args[at + 1]; Directory.CreateDirectory(test.output);
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
                if (!preview.includeNextDrum || preview.Drum == null || preview.Blocks == null) { End("Drum scene missing"); return; }
                tick = 0; finishAt = -1; releaseAt = -1; first = second = third = fourth = fifth = sixth = false;
                awaitingReload = false; reloadWait = 0;
                attempt = new Attempt(); report.attempts.Add(attempt); Press();

                var abc = preview.Blocks.GetComponent<DarkRoomPlatformMarker>();
                var drum = preview.Drum.GetComponent<DarkRoomPlatformMarker>();
                report.abcX = preview.Blocks.transform.position.x;
                report.abcTop = abc.GetTopCenterWorld().y;
                report.drumX = preview.Drum.transform.position.x;
                report.drumTop = drum.GetTopCenterWorld().y;
                report.drumSupportWidth = preview.Drum.Top.size.x;
                report.drumSupportMin = drum.Support.MinX;
                report.drumSupportMax = drum.Support.MaxX;
                report.drumSupportGap = drum.Support.MinX - abc.Support.MaxX;
                report.drumStepUp = report.drumTop - report.abcTop;
                report.drumSortingOrder = preview.Drum.Painting.sortingOrder;
                var idleRenderer = preview.Player.GetComponent<YunaLegend.YunaIllustration.YunaIllustrationVisibilityResolver>().idleRenderer;
                report.idleSortingOrder = idleRenderer.sortingOrder;

                var feet = preview.Player.GetComponents<DresserFootAlignment>();
                abcFeet = feet.FirstOrDefault(f => f.supportIndex == 4);
                drumFeet = feet.FirstOrDefault(f => f.supportIndex == 5);
                if (abcFeet == null || drumFeet == null) { End("Foot alignment components missing"); return; }
                if (index > 0) report.resetClean = !preview.Started && !preview.Furniture.HasCracked && !preview.Player.IsDead && !preview.Player.IsStumbling;
            }
            tick++;
            if (tick == releaseAt) Press();
            if (tick == report.hz / 2) preview.Begin();
            var p = preview.Player;
            if (preview.Started && !first && p.transform.position.x >= -.64f) { first = true; Jump(); }
            if (p.CurrentSupportIndex == 0) attempt.bear = true;
            if (attempt.bear && !second && p.transform.position.x >= 2.72f) { second = true; Jump(); }
            if (p.CurrentSupportIndex == 1) attempt.books = true;
            if (attempt.books && !third && p.IsGrounded && p.transform.position.x >= 5.95f) { third = true; Jump(); }
            if (p.CurrentSupportIndex == 2) attempt.furniture = true;
            var previous = preview.Furniture.GetComponent<DarkRoomPlatformMarker>();
            if (attempt.furniture && !fourth && p.IsGrounded && !p.IsStumbling && preview.Furniture.HasCracked
                && p.transform.position.x >= previous.Support.MaxX - .45f) { fourth = true; Jump(); }
            if (p.CurrentSupportIndex == 3 && p.IsGrounded) attempt.dresser = true;
            var dresserMarker = preview.Dresser.GetComponent<DarkRoomPlatformMarker>();
            if (attempt.dresser && !fifth && p.IsGrounded && p.CurrentSupportIndex == 3
                && p.transform.position.x >= dresserMarker.Support.MaxX - .45f) { fifth = true; Jump(); }
            if (p.CurrentSupportIndex == 4 && p.IsGrounded)
            {
                attempt.maxAbcAnchorGap = Mathf.Max(attempt.maxAbcAnchorGap, Mathf.Abs(abcFeet.AnchorGap));
                if (!attempt.blocks) { attempt.blocks = true; attempt.abcLandingX = p.transform.position.x; }
            }
            var abcMarker = preview.Blocks.GetComponent<DarkRoomPlatformMarker>();
            if (attempt.blocks && !sixth && p.IsGrounded && p.CurrentSupportIndex == 4
                && p.transform.position.x >= abcMarker.Support.MaxX - .35f) { sixth = true; Jump(); }
            if (p.CurrentSupportIndex == 5 && p.IsGrounded)
            {
                attempt.maxDrumAnchorGap = Mathf.Max(attempt.maxDrumAnchorGap, Mathf.Abs(drumFeet.AnchorGap));
                if (!attempt.drum)
                {
                    attempt.drum = true; attempt.drumLandingX = p.transform.position.x;
                    var lr = preview.Player.GetComponent<YunaLegend.YunaIllustration.YunaIllustrationVisibilityResolver>();
                    var lp = lr.VisibleSource;
                    var lrend = lp == "Idle" ? lr.idleRenderer : lp == "Jump" ? lr.jump.painting : lp == "Stumble" ? lr.stumble.painting : lr.run.painting;
                    report.landingPose = lp;
                    report.landingVisualMin = lrend.bounds.min.y;
                    report.landingGapToSupport = report.landingVisualMin - report.drumTop;
                    StartCoroutine(Shot("attempt-" + index + "-drum-land.png"));
                }
            }
            attempt.cracks = preview.Furniture.CrackCount;
            report.maxLegacy = Math.Max(report.maxLegacy, preview.LegacyRenderersVisible);
            if (preview.Finished && finishAt < 0) { attempt.finished = true; finishAt = tick + report.hz; }
            if (finishAt > 0 && tick == finishAt - 2)
            {
                var resolver = preview.Player.GetComponent<YunaLegend.YunaIllustration.YunaIllustrationVisibilityResolver>();
                report.idleVisualMin = resolver.idleRenderer.bounds.min.y;
                report.idleGapToSupport = report.idleVisualMin - report.drumTop;
                Debug.Log("DRUM_CONTACT support=" + report.drumTop.ToString("F4")
                    + " idleMin=" + report.idleVisualMin.ToString("F4") + " gap=" + report.idleGapToSupport.ToString("F4")
                    + " landing=" + report.landingPose + " landMin=" + report.landingVisualMin.ToString("F4") + " landGap=" + report.landingGapToSupport.ToString("F4"));
                StartCoroutine(Shot("attempt-" + index + "-drum-idle.png"));
            }
            if (finishAt > 0 && tick >= finishAt)
            {
                if (index == 0) { index++; awaitingReload = true; Press(Key.R); return; }
                End(null); return;
            }
            if (p.IsDead) { StartCoroutine(Shot("failure.png")); End("Player died x=" + p.transform.position.x + " support=" + p.CurrentSupportIndex); }
            else if (tick > report.hz * 45) End("Timeout x=" + p.transform.position.x + " support=" + p.CurrentSupportIndex);
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
            report.success = error == null && report.errors == 0 && report.resetClean && report.maxLegacy == 0 && report.attempts.Count == 2
                && report.attempts.All(a => a.bear && a.books && a.furniture && a.dresser && a.blocks && a.drum && a.finished && a.jumps == 6 && a.cracks == 1
                    && a.maxAbcAnchorGap < .005f && a.maxDrumAnchorGap < .005f);
            File.WriteAllText(Path.Combine(output, "verification.json"), JsonUtility.ToJson(report, true));
            Debug.Log("DRUM_VERIFY " + report.success + " " + error); enabled = false; Application.Quit(report.success ? 0 : 1);
        }
        void OnLog(string message, string stack, LogType type) { if (type == LogType.Exception || type == LogType.Error) report.errors++; }
        void OnDestroy() { Application.logMessageReceived -= OnLog; if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard); }
    }
}
