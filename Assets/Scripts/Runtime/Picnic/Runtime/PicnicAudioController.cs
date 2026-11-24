using System;
using UnityEngine;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>
    /// 1:1 faithful Unity port of legacy GardenAudio.js + insectSoundscape.js:
    /// looping Blues Shuffle music, jump SFX, procedural stomp/fall/hurt/checkpoint tones,
    /// and proximity-attenuated stereo-panned insect wing loops (bees, dives, chasing beetle).
    /// </summary>
    public sealed class PicnicAudioController : MonoBehaviour
    {
        private AudioSource musicSource;
        private AudioSource sfxSource;
        private AudioSource[] wingSources;

        private AudioClip jumpClip;
        private AudioClip beeClip;
        private AudioClip diveClip;
        private AudioClip beetleClip;

        private AudioClip stompClip;
        private AudioClip fallClip;
        private AudioClip hurtClip;
        private AudioClip chimeClip;

        private bool paused;
        private float lastJumpTime = -10f;

        public void Build()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.clip = Resources.Load<AudioClip>("PicnicAudio/picnic-music");

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            jumpClip = Resources.Load<AudioClip>("PicnicAudio/jump-girl");
            beeClip = Resources.Load<AudioClip>("PicnicAudio/wings-bee");
            diveClip = Resources.Load<AudioClip>("PicnicAudio/wings-dive");
            beetleClip = Resources.Load<AudioClip>("PicnicAudio/wings-beetle");

            stompClip = CreateSweepClip("Stomp", 680f, 1000f, 0.12f, 0.28f);
            fallClip = CreateSweepClip("Fall", 280f, 150f, 0.22f, 0.25f);
            hurtClip = CreateSweepClip("Hurt", 240f, 95f, 0.18f, 0.35f);
            chimeClip = CreateSweepClip("Chime", 1046f, 1318f, 0.45f, 0.22f);

            wingSources = new AudioSource[3];
            for (int i = 0; i < wingSources.Length; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.loop = true;
                src.playOnAwake = false;
                src.volume = 0f;
                wingSources[i] = src;
            }

            ApplyMasterVolume();
            if (musicSource.clip != null)
            {
                musicSource.Play();
            }
        }

        public void SetPaused(bool isPaused)
        {
            paused = isPaused;
            if (musicSource != null)
            {
                if (paused) musicSource.Pause();
                else if (!musicSource.isPlaying && musicSource.clip != null) musicSource.UnPause();
            }
            if (wingSources != null && paused)
            {
                for (int i = 0; i < wingSources.Length; i++)
                {
                    if (wingSources[i].isPlaying) wingSources[i].Pause();
                }
            }
        }

        public void OnDomainEvent(string evt)
        {
            if (paused || sfxSource == null || string.IsNullOrEmpty(evt)) return;
            float master = MasterVolume();
            if (evt == "jump")
            {
                if (Time.unscaledTime - lastJumpTime >= 0.18f && jumpClip != null)
                {
                    sfxSource.PlayOneShot(jumpClip, 0.45f * master);
                    lastJumpTime = Time.unscaledTime;
                }
                return;
            }
            if (evt.StartsWith("stomp", StringComparison.Ordinal) || evt.StartsWith("enrage", StringComparison.Ordinal))
            {
                sfxSource.PlayOneShot(stompClip, 0.5f * master);
            }
            else if (evt.StartsWith("hurt", StringComparison.Ordinal))
            {
                sfxSource.PlayOneShot(hurtClip, 0.65f * master);
            }
            else if (evt == "fall")
            {
                sfxSource.PlayOneShot(fallClip, 0.55f * master);
            }
            else if (evt.StartsWith("checkpoint", StringComparison.Ordinal) || evt == "complete")
            {
                sfxSource.PlayOneShot(chimeClip, 0.5f * master);
            }
        }

        public void Apply(PicnicSimulation sim)
        {
            ApplyMasterVolume();
            if (paused || sim == null || sim.IsReadingStory || wingSources == null)
            {
                StopWings();
                return;
            }
            UpdateInsectWings(sim);
        }

        private void ApplyMasterVolume()
        {
            float master = MasterVolume();
            if (musicSource != null)
            {
                musicSource.volume = 0.35f * master;
                if (!paused && !musicSource.isPlaying && musicSource.clip != null)
                {
                    musicSource.Play();
                }
            }
        }

        private void UpdateInsectWings(PicnicSimulation sim)
        {
            float master = MasterVolume();
            int used = 0;

            // Beetle chase has top priority
            if (sim.Encounters.Panic != null && beetleClip != null && used < wingSources.Length)
            {
                AssignWing(wingSources[used++], beetleClip, sim.Encounters.Panic.X - sim.PlayerX,
                    sim.Encounters.Panic.Y - (sim.PlayerY - 44f), 1.0f, 1.02f, master, 1.0f);
            }
            else if (sim.Encounters.Exit != null && beetleClip != null && used < wingSources.Length)
            {
                float energy = Mathf.Clamp01(1f - sim.Encounters.Exit.Age);
                AssignWing(wingSources[used++], beetleClip, sim.Encounters.Exit.X - sim.PlayerX,
                    sim.Encounters.Exit.Y - (sim.PlayerY - 44f), energy, 0.84f + 0.18f * energy, master, 1.0f);
            }

            // Nearest bees
            foreach (var bee in sim.Encounters.Bees)
            {
                if (!bee.Alive || used >= wingSources.Length) continue;
                float dx = bee.X - sim.PlayerX;
                float dy = (bee.Y - (sim.PlayerY - 44f)) * 0.8f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist >= 800f) continue;

                bool diving = bee.Phase == PicnicBeePhase.Dive;
                var clip = diving && diveClip != null ? diveClip : beeClip;
                if (clip == null) continue;
                float energy = diving ? 1.0f : 0.65f;
                AssignWing(wingSources[used++], clip, dx, dy, energy, 1.0f + 0.12f * energy, master, 1.6f);
            }

            for (int i = used; i < wingSources.Length; i++)
            {
                if (wingSources[i].isPlaying) wingSources[i].Stop();
            }
        }

        private static void AssignWing(AudioSource src, AudioClip clip, float dx, float dy,
            float energy, float pitch, float master, float boost)
        {
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float atten = Mathf.Max(0f, 1f - dist / 800f);
            float vol = 0.26f * energy * atten * atten * boost * master;
            if (vol <= 0.002f)
            {
                if (src.isPlaying) src.Stop();
                return;
            }
            if (src.clip != clip)
            {
                src.clip = clip;
                src.Play();
            }
            else if (!src.isPlaying)
            {
                src.Play();
            }
            src.volume = Mathf.Clamp01(vol);
            src.panStereo = Mathf.Clamp(dx / 600f, -0.85f, 0.85f);
            src.pitch = pitch;
        }

        private void StopWings()
        {
            if (wingSources == null) return;
            for (int i = 0; i < wingSources.Length; i++)
            {
                if (wingSources[i] != null && wingSources[i].isPlaying) wingSources[i].Stop();
            }
        }

        private static float MasterVolume()
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat("YunaLegend.MasterVolume", 1f));
        }

        private static AudioClip CreateSweepClip(string name, float startHz, float endHz, float duration, float peak)
        {
            const int sampleRate = 22050;
            int samples = Mathf.Max(64, Mathf.RoundToInt(sampleRate * duration));
            var data = new float[samples];
            double phase = 0.0;
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float hz = Mathf.Lerp(startHz, endHz, t);
                phase += 2.0 * Math.PI * hz / sampleRate;
                float env = (1f - t) * (1f - t);
                data[i] = (float)Math.Sin(phase) * peak * env;
            }
            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
