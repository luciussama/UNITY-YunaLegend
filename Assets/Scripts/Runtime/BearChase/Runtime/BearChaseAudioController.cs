using UnityEngine;
using YunaLegend.BearChase.Core;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Controlador de audio da Fase 4 usando assets legados migrados:
    /// musica Chase.mp3 entrando no corte para a corrida, passos variados da menina (girl-01..06)
    /// na entrada e corrida, passos pesados do urso (bear-01..06), despertar, arremesso, impacto e tropeço.
    /// </summary>
    public sealed class BearChaseAudioController : MonoBehaviour
    {
        [SerializeField] private AudioClip chaseMusic;
        [SerializeField] private AudioClip[] girlFootsteps;
        [SerializeField] private AudioClip[] bearFootsteps;
        [SerializeField] private AudioClip bearWakeClip;
        [SerializeField] private AudioClip throwClip;
        [SerializeField] private AudioClip toyLandClip;
        [SerializeField] private AudioClip stumbleHitClip;
        [SerializeField] private AudioClip defeatClip;
        [SerializeField] private AudioClip victoryClip;

        private AudioSource musicSource;
        private AudioSource sfxSource;
        private int lastEntranceStepCue;
        private int lastGirlClipIndex = -1;
        private int lastBearClipIndex = -1;
        private float girlStepTimer;
        private float bearStepTimer;
        private bool chaseMusicStarted;

        public bool IsMuted { get; private set; }

        public void Configure(
            AudioClip music,
            AudioClip[] girlSteps,
            AudioClip[] bearSteps,
            AudioClip wakeClip,
            AudioClip throwSfx,
            AudioClip landSfx,
            AudioClip hitSfx,
            AudioClip defeatSfx,
            AudioClip victorySfx)
        {
            chaseMusic = music;
            girlFootsteps = girlSteps;
            bearFootsteps = bearSteps;
            bearWakeClip = wakeClip;
            throwClip = throwSfx;
            toyLandClip = landSfx;
            stumbleHitClip = hitSfx;
            defeatClip = defeatSfx;
            victoryClip = victorySfx;
        }

        private void Awake()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.42f;
            musicSource.playOnAwake = false;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.65f;

            EnsureFallbackClips();
        }

        public void ResetForStage(bool skipEntrance)
        {
            lastEntranceStepCue = 0;
            girlStepTimer = 0f;
            bearStepTimer = 0f;
            chaseMusicStarted = false;

            if (skipEntrance)
            {
                EnsureChaseMusicPlaying();
            }
            else if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        public void UpdateFootsteps(BearChaseSimulation sim, float dt)
        {
            if (sim == null || IsMuted || sfxSource == null)
            {
                return;
            }

            if (sim.IsEntrancePhase)
            {
                int cue = sim.EntranceSample.StepCueNumber;
                if (cue > 0 && cue != lastEntranceStepCue)
                {
                    lastEntranceStepCue = cue;
                    PlayRandomClip(girlFootsteps, ref lastGirlClipIndex, 0.42f, 1.0f);
                }

                return;
            }

            if (sim.Phase != BearChasePhase.Chasing && sim.Phase != BearChasePhase.Stumble)
            {
                return;
            }

            EnsureChaseMusicPlaying();

            float cadence = Mathf.Max(0.18f, 3.0f / Mathf.Max(6f, sim.CurrentForwardSpeed));
            girlStepTimer += dt;
            if (girlStepTimer >= cadence)
            {
                girlStepTimer -= cadence;
                PlayRandomClip(girlFootsteps, ref lastGirlClipIndex, 0.34f, Random.Range(0.96f, 1.08f));
            }

            bearStepTimer += dt;
            float bearCadence = cadence * 1.12f;
            if (bearStepTimer >= bearCadence)
            {
                bearStepTimer -= bearCadence;
                PlayRandomClip(bearFootsteps, ref lastBearClipIndex, 0.28f, Random.Range(0.94f, 1.05f));
            }
        }

        public void EnsureChaseMusicPlaying()
        {
            if (chaseMusicStarted || chaseMusic == null || musicSource == null)
            {
                return;
            }

            chaseMusicStarted = true;
            musicSource.clip = chaseMusic;
            musicSource.Play();
        }

        public void PlayBearWake()
        {
            PlaySfx(bearWakeClip, 0.72f);
        }

        public void PlayThrow()
        {
            PlaySfx(throwClip, 0.50f);
        }

        public void PlayToyLand()
        {
            PlaySfx(toyLandClip, 0.42f);
        }

        public void PlayHit()
        {
            PlaySfx(stumbleHitClip, 0.72f);
        }

        public void PlayDefeat()
        {
            PlaySfx(defeatClip, 0.58f);
        }

        public void PlayVictory()
        {
            PlaySfx(victoryClip, 0.75f);
        }

        public void SetPaused(bool paused)
        {
            if (musicSource == null)
            {
                return;
            }

            if (paused)
            {
                musicSource.Pause();
            }
            else
            {
                musicSource.UnPause();
            }
        }

        public bool ToggleMute()
        {
            IsMuted = !IsMuted;
            if (musicSource != null)
            {
                musicSource.mute = IsMuted;
            }

            if (sfxSource != null)
            {
                sfxSource.mute = IsMuted;
            }

            return IsMuted;
        }

        private void PlayRandomClip(AudioClip[] clips, ref int lastIdx, float volume, float pitch)
        {
            if (clips == null || clips.Length == 0 || sfxSource == null)
            {
                return;
            }

            int idx = Random.Range(0, clips.Length);
            if (clips.Length > 1 && idx == lastIdx)
            {
                idx = (idx + 1) % clips.Length;
            }

            lastIdx = idx;
            AudioClip clip = clips[idx];
            if (clip != null)
            {
                sfxSource.pitch = pitch;
                sfxSource.PlayOneShot(clip, volume);
                sfxSource.pitch = 1f;
            }
        }

        private void PlaySfx(AudioClip clip, float volume)
        {
            if (!IsMuted && clip != null && sfxSource != null)
            {
                sfxSource.pitch = 1f;
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        private void EnsureFallbackClips()
        {
            if (bearWakeClip == null)
            {
                bearWakeClip = CreateToneClip("BearWakeFallback", 240f, 390f, 0.22f);
            }

            if (throwClip == null)
            {
                throwClip = CreateToneClip("BearThrowFallback", 320f, 520f, 0.14f);
            }

            if (toyLandClip == null)
            {
                toyLandClip = CreateToneClip("ToyLandFallback", 160f, 110f, 0.12f);
            }

            if (stumbleHitClip == null)
            {
                stumbleHitClip = CreateToneClip("StumbleHitFallback", 170f, 95f, 0.20f);
            }

            if (defeatClip == null)
            {
                defeatClip = CreateToneClip("DefeatFallback", 280f, 140f, 0.40f);
            }

            if (victoryClip == null)
            {
                victoryClip = CreateToneClip("ChaseVictoryFallback", 440f, 660f, 0.38f);
            }
        }

        private static AudioClip CreateToneClip(string name, float startHz, float endHz, float duration)
        {
            const int sampleRate = 22050;
            int count = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            var data = new float[count];
            float phase = 0f;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float hz = Mathf.Lerp(startHz, endHz, t);
                phase += 2f * Mathf.PI * hz / sampleRate;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = Mathf.Sin(phase) * env * 0.28f;
            }

            var clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
