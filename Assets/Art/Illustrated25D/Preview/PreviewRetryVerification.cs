using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using YunaLegend.NandaIllustration;
using YunaLegend.YunaIllustration;

namespace YunaLegend.Illustrated25D.Preview
{
    /// <summary>Opt-in regression proof across real scene reloads. Never changes the physical controller.</summary>
    [DefaultExecutionOrder(320)]
    public sealed class PreviewRetryVerification : MonoBehaviour
    {
        [Serializable] public class Attempt
        {
            public string purpose;
            public bool bear, books, furniture, stumbled, finished, died, reset, contactAligned = true;
            public int cracks;
            public float sag, pause, retryDelay, retryPose;
        }
        [Serializable] public class Report
        {
            public int hz, frames, errors, maxLegacy, maxVisible, roomCount;
            public bool success, pauseInput, resumeInput, recoveryReset, pauseReset, failureReset;
            public bool audioCapture;
            public string error, capture = "End-of-frame player with UI, 30 fps, silent";
            public List<Attempt> attempts = new List<Attempt>();
            public List<string> events = new List<string>();
            public List<string> landings = new List<string>();
            public List<string> audioSources = new List<string>();
        }
        static Report report;
        static int attemptIndex;
        static string output;
        static Dictionary<string, Vector3> rest;
        static float restGait;
        static PreviewRetryVideo video;
        static PreviewAudioCapture audioCapture;
        IllustratedPreview preview;
        Keyboard keyboard;
        int tick, releaseAt = -1, finishAt = -1, retryAt = -1;
        bool first, second, third, retryQueued, paused;
        Vector3 pausePosition;
        int lastSupport = -100, lastCracks;
        Attempt attempt;
        NandaAccompaniment companion;

        void Start()
        {
            var args = System.Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-previewRetryVerify");
            if (at < 0) { enabled = false; return; }
            output = args[at + 1]; Directory.CreateDirectory(output);
            if (report == null)
            {
                int hzAt = Array.IndexOf(args, "-previewHz");
                report = new Report { hz = hzAt < 0 ? 60 : int.Parse(args[hzAt + 1]) };
                int captureAt = Array.IndexOf(args, "-previewVideoEncoder");
                if (captureAt >= 0)
                {
                    video = new GameObject("T063_ContinuousVideo").AddComponent<PreviewRetryVideo>();
                    DontDestroyOnLoad(video.gameObject);
                    video.Open(args[captureAt + 1], Path.Combine(output, "continuous.mp4"), report.hz);
                }
                int audioAt = Array.IndexOf(args, "-previewAudioCapture");
                if (audioAt >= 0)
                {
                    audioCapture = PreviewAudioCapture.Begin(Path.Combine(output, "gameplay-audio.wav"));
                    report.audioCapture = true;
                    report.capture = "End-of-frame player with UI, 30 fps, audio mix WAV captured";
                }
            }
            attempt = new Attempt { purpose = new[] { "R during local pause", "R during recovery", "Complete after resets", "Miss jump then retry", "Complete after failure" }[attemptIndex] };
            report.attempts.Add(attempt);
            audioCapture?.Mark("attempt-" + attemptIndex + "-start");
            Application.logMessageReceived += OnLog;
            Time.captureDeltaTime = 1f / report.hz;
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            keyboard = InputSystem.AddDevice<Keyboard>(); InputSystem.EnableDevice(keyboard);
            preview = GetComponent<IllustratedPreview>();
        }

        void LateUpdate()
        {
            if (!preview.Ready) { if (preview.Failure != null) End(preview.Failure); return; }
            tick++; report.frames++;
            var p = preview.Player;
            companion = preview.Flight.GetComponent<NandaAccompaniment>();
            if (tick == 2)
            {
                if (!CheckReset()) { End("Residual state after reload, attempt " + attemptIndex); return; }
                Event("ready/reset validated");
                foreach (var source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                    report.audioSources.Add(source.gameObject.name + "|" + (source.clip != null ? source.clip.name : "null")
                        + "|playing=" + source.isPlaying + "|volume=" + source.volume.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            }
            if (tick == releaseAt) Press();
            if (retryQueued) return;
            if (tick == report.hz) { Press(Key.Enter); releaseAt = tick + 2; Event("begin via Enter"); }

            // Check real P/Esc input once, before the first jump.
            if (attemptIndex == 2 && tick == report.hz + 8)
            { pausePosition = p.transform.position; Press(Key.P); releaseAt = tick + 2; paused = true; }
            if (paused && tick == report.hz + 16)
            {
                report.pauseInput = Time.timeScale == 0 && Vector3.Distance(pausePosition, p.transform.position) < .15f;
                Press(Key.Escape); releaseAt = tick + 2;
            }
            if (paused && tick == report.hz + 20)
            { report.resumeInput = Time.timeScale == 1; paused = false; Event("P/Esc checked"); }

            if (attemptIndex != 3)
            {
                if (!first && preview.Started && p.transform.position.x >= -.64f) { first = true; Jump(); }
                if (attempt.bear && !second && p.transform.position.x >= 2.72f) { second = true; Jump(); }
                if (attempt.books && !third && p.IsGrounded && p.transform.position.x >= 5.95f) { third = true; Jump(); }
            }
            if (p.CurrentSupportIndex == 0 && !attempt.bear) { attempt.bear = true; Event("bear"); }
            if (p.CurrentSupportIndex == 1 && !attempt.books) { attempt.books = true; Event("books"); }
            if (p.CurrentSupportIndex == 2 && !attempt.furniture) { attempt.furniture = true; Event("furniture"); }
            attempt.stumbled |= p.IsStumbling;
            attempt.sag = Mathf.Max(attempt.sag, preview.Furniture.CurrentSag);
            attempt.pause = Mathf.Max(attempt.pause, p.StumbleDelayRemaining);
            attempt.cracks = Mathf.Max(attempt.cracks, preview.Furniture.CrackCount);
            if (attempt.cracks > lastCracks)
            {
                lastCracks = attempt.cracks;
                audioCapture?.Mark("crack-" + attemptIndex + "-" + attempt.cracks);
            }
            if (p.CurrentSupportIndex != lastSupport)
            {
                if (p.CurrentSupportIndex >= 0)
                {
                    var landed = FindObjectsByType<YunaLegend.DarkRoom.DarkRoomPlatformMarker>(FindObjectsSortMode.None)
                        .FirstOrDefault(m => m.spec.index == p.CurrentSupportIndex);
                    if (landed != null)
                    {
                        var landing = landed.GetLandingPoint(p.RunDirection);
                        string dx = Mathf.Abs(p.transform.position.x - landing.x).ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                        string x = p.transform.position.x.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                        string lx = landing.x.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                        report.landings.Add("attempt=" + attemptIndex + " index=" + p.CurrentSupportIndex + " grounded=" + p.IsGrounded
                            + " x=" + x + " landingX=" + lx + " dx=" + dx);
                    }
                }
                lastSupport = p.CurrentSupportIndex;
            }
            report.maxLegacy = Mathf.Max(report.maxLegacy, preview.LegacyRenderersVisible);
            report.maxVisible = Mathf.Max(report.maxVisible, p.GetComponent<YunaIllustrationVisibilityResolver>().VisibleCount);
            foreach (string side in new[] { "Esquerdo", "Direito" })
            {
                var contact = preview.Furniture.transform.Find("Contacto_Fisico/Tampo_" + side);
                var painted = preview.Furniture.transform.Find("Furniture_Illustrated_T053/Tampo_" + side + "_Painted");
                attempt.contactAligned &= Vector3.Distance(contact.position, painted.position) < .0001f
                    && Quaternion.Angle(contact.rotation, painted.rotation) < .01f;
            }
            if (attempt.cracks > 1) { End("Duplicate crack"); return; }
            if (attemptIndex == 0 && preview.Furniture.HasCracked && p.StumbleDelayRemaining > 1f / report.hz)
            { Retry("R in pause"); return; }
            if (attemptIndex == 1 && preview.Furniture.HasCracked && p.IsStumbling && p.StumbleDelayRemaining == 0 && p.StumblePoseAmount > .25f)
            { Retry("R in recovery"); return; }
            if (preview.Finished && finishAt < 0)
            { attempt.finished = true; finishAt = tick + report.hz; Event("finished"); }
            if (finishAt > 0 && tick >= finishAt)
            {
                if (attemptIndex == 4) End(null); else Retry("R after finish");
                return;
            }
            if (p.IsDead)
            {
                attempt.died = true;
                if (attemptIndex != 3) { End("Unexpected death at " + p.transform.position); return; }
                if (retryAt < 0) { retryAt = tick + report.hz; Event("missed jump / failure UI"); }
                if (tick >= retryAt) Retry("R after failure");
            }
            if (tick > 30 * report.hz) End("Timeout after " + (tick / report.hz) + " s de simulacao em " + p.transform.position);
        }

        bool CheckReset()
        {
            var p = preview.Player;
            var pose = preview.Furniture.GetComponentsInChildren<Transform>(true)
                .Where(t => t.name.StartsWith("Tampo_")).ToDictionary(t => Relative(t), t => t.localPosition);
            if (rest == null) { rest = pose; restGait = p.GaitPhase; }
            report.roomCount = FindObjectsByType<YunaLegend.Illustrated25D.Environment.RoomBackgroundT054>(FindObjectsSortMode.None).Length;
            bool clean = !preview.Started && !preview.Finished && !preview.Furniture.HasCracked
                && preview.Furniture.CrackCount == 0 && preview.Furniture.CurrentSag == 0 && !preview.Furniture.IsTraversing
                && !p.IsDead && !p.IsStumbling && p.StumbleDelayRemaining == 0 && p.StumblePoseAmount == 0
                && Vector3.Distance(p.transform.position, YunaLegend.DarkRoom.DarkRoomStartLayout.Position) < .01f
                && Mathf.Abs(p.GaitPhase - restGait) < .001f && Time.timeScale == 1
                && !preview.Furniture.GetComponent<AudioSource>().isPlaying
                && companion.PresentationClock <= 4f / report.hz
                && Vector3.Distance(companion.transform.position, p.transform.position) < 3
                && report.roomCount == 1 && pose.Count == rest.Count
                && pose.All(kv => rest.ContainsKey(kv.Key) && Vector3.Distance(kv.Value, rest[kv.Key]) < .0001f)
                && !preview.Furniture.transform.Find("Furniture_Illustrated_T053/Localized_grain_fibres_no_support").gameObject.activeSelf;
            attempt.reset = clean;
            if (attemptIndex == 1) report.pauseReset = clean;
            if (attemptIndex == 2) report.recoveryReset = clean;
            if (attemptIndex == 4) report.failureReset = clean;
            return clean;
        }
        string Relative(Transform t)
        {
            string result = t.name;
            while (t.parent != preview.Furniture.transform) { t = t.parent; result = t.name + "/" + result; }
            return result;
        }
        void Press(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        void Jump() { Press(UnityEngine.InputSystem.Key.Space); releaseAt = tick + Mathf.RoundToInt(.12f * report.hz); Event("jump"); }
        void Retry(string reason)
        {
            attempt.retryDelay = preview.Player.StumbleDelayRemaining;
            attempt.retryPose = preview.Player.StumblePoseAmount;
            Event(reason); attemptIndex++; retryQueued = true; Press(UnityEngine.InputSystem.Key.R);
        }
        void Event(string message) => report.events.Add("attempt=" + attemptIndex + " frame=" + report.frames + " x=" + preview.Player.transform.position.x.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + " " + message);
        void End(string error)
        {
            report.error = error;
            report.success = error == null && report.attempts.Count == 5 && report.errors == 0
                && report.pauseReset && report.recoveryReset && report.failureReset && report.pauseInput && report.resumeInput
                && report.maxLegacy == 0 && report.maxVisible == 1
                && report.attempts.All(a => a.reset && a.contactAligned)
                && report.attempts.Where((a, i) => i != 3).All(a => a.bear && a.books && a.furniture && a.stumbled && a.cracks == 1)
                && report.attempts[2].finished && report.attempts[4].finished
                && Mathf.Abs(report.attempts[2].sag - .075f) < .001f
                && Mathf.Abs(report.attempts[2].pause - .08f) < .001f && report.attempts[3].died;
            audioCapture?.Mark("attempt-" + attemptIndex + "-end");
            audioCapture?.StopAndWrite();
            File.WriteAllText(Path.Combine(output, "verification.json"), JsonUtility.ToJson(report, true));
            video?.Close();
            Debug.Log("T063_VERIFY " + report.success + " " + error);
            enabled = false; Application.Quit(report.success ? 0 : 1);
        }
        void OnLog(string condition, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception) { report.errors++; report.events.Add(condition); } }
        void OnDestroy()
        { Application.logMessageReceived -= OnLog; if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard); }
    }

    public sealed class PreviewRetryVideo : MonoBehaviour
    {
        System.Diagnostics.Process encoder;
        int hz, frame;
        public void Open(string executable, string destination, int simulationHz)
        {
            hz = simulationHz;
            encoder = new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo {
                FileName = executable, Arguments = "-y -loglevel error -f rawvideo -pixel_format rgba -video_size 1280x720 -framerate 30 -i pipe:0 -vf vflip -c:v libx264 -preset fast -crf 20 -pix_fmt yuv420p -movflags +faststart \"" + destination + "\"",
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true } };
            encoder.Start(); StartCoroutine(Capture());
        }
        IEnumerator Capture()
        {
            while (encoder != null)
            {
                yield return new WaitForEndOfFrame();
                if (encoder == null || frame++ % (hz / 30) != 0) continue;
                var image = ScreenCapture.CaptureScreenshotAsTexture();
                var bytes = image.GetPixels32();
                var raw = new byte[bytes.Length * 4];
                for (int i = 0; i < bytes.Length; i++)
                { raw[i*4] = bytes[i].r; raw[i*4+1] = bytes[i].g; raw[i*4+2] = bytes[i].b; raw[i*4+3] = 255; }
                encoder.StandardInput.BaseStream.Write(raw, 0, raw.Length); Destroy(image);
            }
        }
        public void Close()
        {
            if (encoder == null) return;
            encoder.StandardInput.Close(); encoder.WaitForExit(10000); encoder.Dispose(); encoder = null;
        }
        void OnDestroy() => Close();
    }
}
