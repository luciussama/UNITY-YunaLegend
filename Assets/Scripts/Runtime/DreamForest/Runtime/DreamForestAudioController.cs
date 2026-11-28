using System.Collections.Generic;
using UnityEngine;
using YunaLegend.DreamForest.Core;

namespace YunaLegend.DreamForest.Runtime
{
    /// <summary>
    /// SRP Audio controller with 1:1 parity against legacy DreamAudio.js + GardenAudio.js:
    /// - Soundtrack switching: Misty Blues (Act 1/Glider), Bug attack (Nightmare), Blues Shuffle Loop (Sunlit Wake)
    /// - Spatial ambient insect wing loop (bee / beetle) modulated by proximity in Act 1
    /// - Full SFX set: jump-girl, jump-mud, mud-impact, wings-beetle, plus procedural AudioClips for
    ///   roach-step (55->28Hz), roach-stomp (62->24Hz), nightmare-detected (140->65Hz),
    ///   nightmare-scream (dual-formant 1050/2450Hz), fall (280->150Hz), checkpoint/complete (1046->1318Hz), hurt.
    /// NOTE: musicSource is intentionally the FIRST AudioSource added so DreamEntranceAmbience
    /// (GetComponent&lt;AudioSource&gt;()) binds to it cleanly.
    /// </summary>
    public sealed class DreamForestAudioController : MonoBehaviour
    {
        private AudioSource musicSource;
        private AudioSource sfxSource;
        private AudioSource wingLoopSource;

        private AudioClip dreamTrack;
        private AudioClip nightmareTrack;
        private AudioClip picnicWakeTrack;

        private AudioClip jumpSfx;
        private AudioClip jumpMudSfx;
        private AudioClip mudImpactSfx;
        private AudioClip wingsBeeSfx;
        private AudioClip wingsBeetleSfx;

        private AudioClip roachStepClip;
        private AudioClip roachStompClip;
        private AudioClip detectedClip;
        private AudioClip screamClip;
        private AudioClip fallClip;
        private AudioClip chimeClip;
        private AudioClip hurtClip;

        private DreamForestMusicScene currentScene = (DreamForestMusicScene)(-1);
        private bool muted;

        public bool IsMuted => muted;

        public void Build()
        {
            // Keep musicSource first for DreamEntranceAmbience compatibility
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.35f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.65f;

            wingLoopSource = gameObject.AddComponent<AudioSource>();
            wingLoopSource.loop = true;
            wingLoopSource.playOnAwake = false;
            wingLoopSource.volume = 0f;

            dreamTrack = DreamForestAtlasCatalog.LoadAudio("Misty Blues");
            nightmareTrack = DreamForestAtlasCatalog.LoadAudio("Bug attack");
            picnicWakeTrack = DreamForestAtlasCatalog.LoadAudio("Blues Shuffle Loop");

            jumpSfx = DreamForestAtlasCatalog.LoadAudio("jump-girl");
            jumpMudSfx = DreamForestAtlasCatalog.LoadAudio("jump-mud");
            mudImpactSfx = DreamForestAtlasCatalog.LoadAudio("mud-impact");
            wingsBeeSfx = DreamForestAtlasCatalog.LoadAudio("wings-bee-v2");
            wingsBeetleSfx = DreamForestAtlasCatalog.LoadAudio("wings-beetle");

            roachStepClip = BuildSweepTone("RoachStep", 55f, 28f, 0.22f, 0.42f);
            roachStompClip = BuildSweepTone("RoachStomp", 62f, 24f, 0.42f, 0.68f);
            detectedClip = BuildSweepTone("NightmareDetected", 140f, 65f, 0.35f, 0.35f);
            fallClip = BuildSweepTone("DreamFall", 280f, 150f, 0.20f, 0.28f);
            chimeClip = BuildSweepTone("DreamChime", 1046f, 1318f, 0.48f, 0.25f);
            hurtClip = BuildSweepTone("DreamHurt", 320f, 110f, 0.18f, 0.40f);
            screamClip = BuildNightmareScream("NightmareScream");
        }

        public void SetMuted(bool value)
        {
            muted = value;
            if (musicSource != null) musicSource.mute = muted;
            if (sfxSource != null) sfxSource.mute = muted;
            if (wingLoopSource != null) wingLoopSource.mute = muted;
        }

        public void Apply(DreamForestSimulation sim, bool paused)
        {
            if (musicSource == null) return;
            var desired = sim.ActiveMusicScene();
            if (desired != currentScene)
            {
                currentScene = desired;
                if (desired == DreamForestMusicScene.Silence)
                {
                    musicSource.Pause();
                }
                else
                {
                    musicSource.clip = desired == DreamForestMusicScene.Nightmare
                        ? nightmareTrack
                        : (desired == DreamForestMusicScene.PicnicWake ? picnicWakeTrack : dreamTrack);
                    if (musicSource.clip != null && !paused && !muted)
                    {
                        musicSource.Play();
                    }
                }
            }

            if (paused || muted)
            {
                if (musicSource.isPlaying) musicSource.Pause();
                if (wingLoopSource != null && wingLoopSource.isPlaying) wingLoopSource.Pause();
                return;
            }

            if (currentScene != DreamForestMusicScene.Silence && musicSource.clip != null && !musicSource.isPlaying)
            {
                musicSource.UnPause();
                if (!musicSource.isPlaying) musicSource.Play();
            }

            UpdateSpatialInsectWings(sim);
        }

        private void UpdateSpatialInsectWings(DreamForestSimulation sim)
        {
            if (wingLoopSource == null) return;
            if (sim.Nightmare != null || sim.Stage == DreamForestStage.Opening || sim.Stage == DreamForestStage.GlideLanding)
            {
                if (wingLoopSource.isPlaying) wingLoopSource.Pause();
                return;
            }

            float px = sim.Player.X;
            float bestDist = 720f;
            float bestPan = 0f;
            bool bestIsBee = true;

            var objs = sim.Level.Objects;
            for (int i = 0; i < objs.Length; i++)
            {
                var o = objs[i];
                if (o.Kind != DreamForestObjectKind.Bee && o.Kind != DreamForestObjectKind.Beetle) continue;
                var f = DreamForestLevelData.EvaluateObjectFrame(o, sim.Time, sim.Rides);
                float dx = (f.X + f.W * 0.5f) - px;
                float dist = Mathf.Abs(dx);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPan = Mathf.Clamp(dx / 550f, -0.75f, 0.75f);
                    bestIsBee = o.Kind == DreamForestObjectKind.Bee;
                }
            }

            if (bestDist < 700f)
            {
                AudioClip targetClip = bestIsBee ? (wingsBeeSfx ?? wingsBeetleSfx) : (wingsBeetleSfx ?? wingsBeeSfx);
                if (targetClip != null)
                {
                    if (wingLoopSource.clip != targetClip)
                    {
                        wingLoopSource.clip = targetClip;
                        wingLoopSource.Play();
                    }
                    else if (!wingLoopSource.isPlaying)
                    {
                        wingLoopSource.Play();
                    }
                    float prox = 1f - Mathf.Clamp01(bestDist / 700f);
                    wingLoopSource.volume = prox * prox * (bestIsBee ? 0.24f : 0.20f);
                    wingLoopSource.panStereo = bestPan;
                }
            }
            else if (wingLoopSource.isPlaying)
            {
                wingLoopSource.volume = Mathf.MoveTowards(wingLoopSource.volume, 0f, Time.deltaTime * 1.5f);
                if (wingLoopSource.volume <= 0.005f) wingLoopSource.Pause();
            }
        }

        public void HandleEvents(IReadOnlyList<DreamForestEvent> events)
        {
            if (muted || sfxSource == null || events == null) return;
            for (int i = 0; i < events.Count; i++)
            {
                switch (events[i].Kind)
                {
                    case DreamForestEventKind.Jump:
                        if (jumpSfx != null) sfxSource.PlayOneShot(jumpSfx, 0.42f);
                        break;
                    case DreamForestEventKind.MudTakeoff:
                        if (jumpMudSfx != null) sfxSource.PlayOneShot(jumpMudSfx, 0.55f);
                        break;
                    case DreamForestEventKind.MudImpact:
                        if (mudImpactSfx != null) sfxSource.PlayOneShot(mudImpactSfx, 0.78f);
                        break;
                    case DreamForestEventKind.BeetleBoarded:
                        if (wingsBeetleSfx != null) sfxSource.PlayOneShot(wingsBeetleSfx, 0.50f);
                        break;
                    case DreamForestEventKind.RoachStep:
                        if (roachStepClip != null) sfxSource.PlayOneShot(roachStepClip, 0.45f);
                        break;
                    case DreamForestEventKind.RoachStomp:
                        if (roachStompClip != null) sfxSource.PlayOneShot(roachStompClip, 0.75f);
                        break;
                    case DreamForestEventKind.NightmareDetected:
                        if (detectedClip != null) sfxSource.PlayOneShot(detectedClip, 0.48f);
                        break;
                    case DreamForestEventKind.NightmareScream:
                        sfxSource.Stop();
                        if (screamClip != null) sfxSource.PlayOneShot(screamClip, 0.70f);
                        break;
                    case DreamForestEventKind.Fall:
                        if (fallClip != null) sfxSource.PlayOneShot(fallClip, 0.40f);
                        break;
                    case DreamForestEventKind.Hurt:
                        if (hurtClip != null) sfxSource.PlayOneShot(hurtClip, 0.55f);
                        break;
                    case DreamForestEventKind.Checkpoint:
                    case DreamForestEventKind.Complete:
                        if (chimeClip != null) sfxSource.PlayOneShot(chimeClip, 0.40f);
                        break;
                }
            }
        }

        private static AudioClip BuildSweepTone(string name, float startHz, float endHz, float duration, float peak)
        {
            const int sr = 22050;
            int samples = Mathf.Max(64, Mathf.RoundToInt(sr * duration));
            var data = new float[samples];
            float phase = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float hz = startHz * Mathf.Pow(endHz / Mathf.Max(1f, startHz), t);
                phase += 2f * Mathf.PI * hz / sr;
                float env = Mathf.Sin(Mathf.Min(1f, t * 14f) * Mathf.PI * 0.5f) * Mathf.Pow(1f - t, 1.4f);
                data[i] = Mathf.Sin(phase) * peak * env;
            }
            var clip = AudioClip.Create(name, samples, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildNightmareScream(string name)
        {
            // Dual-formant vocalized fright sweep matching DreamAudio.scream() (670 -> 920 -> 430 Hz)
            const int sr = 22050;
            const float duration = 0.74f;
            int samples = Mathf.RoundToInt(sr * duration);
            var data = new float[samples];
            float p1 = 0f;
            float p2 = 0f;
            for (int i = 0; i < samples; i++)
            {
                float sec = i / (float)sr;
                float f0 = sec < 0.1f
                    ? Mathf.Lerp(670f, 920f, sec / 0.1f)
                    : 920f * Mathf.Pow(430f / 920f, (sec - 0.1f) / 0.64f);
                p1 += 2f * Mathf.PI * f0 / sr;
                p2 += 2f * Mathf.PI * (f0 * 2.25f) / sr;
                float env = sec < 0.035f
                    ? sec / 0.035f
                    : (sec < 0.30f ? Mathf.Lerp(1f, 0.65f, (sec - 0.035f) / 0.265f) : Mathf.Pow(Mathf.Clamp01(1f - (sec - 0.30f) / 0.44f), 2f));
                data[i] = (0.6f * Mathf.Sin(p1) + 0.4f * Mathf.Sin(p2)) * 0.38f * env;
            }
            var clip = AudioClip.Create(name, samples, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
