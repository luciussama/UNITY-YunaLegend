using UnityEngine;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Reproduz a trilha sonora original do acervo legado (The Circle Game / Circular Dissonance)
    /// e os efeitos sonoros da Toy Room, respeitando pausa, volume e mudo.
    /// </summary>
    public sealed class ToyRoomAudioController : MonoBehaviour
    {
        [SerializeField] private AudioClip explorationMusicClip;
        [SerializeField] private AudioClip swordMusicClip;
        [SerializeField] private float musicVolume = 0.52f;
        [SerializeField] private float sfxVolume = 0.45f;

        private AudioSource musicSource;
        private AudioSource sfxSource;
        private AudioClip pickupClip;
        private AudioClip storeClip;
        private AudioClip strangeClip;
        private AudioClip swordClip;
        private AudioClip tenderChimeClip;
        private AudioClip portalWhooshClip;
        private AudioClip softMagicBurstClip;
        private bool isMuted;
        private bool isPaused;
        private bool swordMusicActive;
        private bool suppressAutoMusicOnStart;

        public bool IsMuted => isMuted;
        public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying && musicSource.clip != null;
        public string ActiveMusicTrackName => musicSource != null && musicSource.clip != null ? musicSource.clip.name : string.Empty;

        public void ConfigureMusic(AudioClip explorationClip, AudioClip swordClipAsset)
        {
            explorationMusicClip = explorationClip;
            swordMusicClip = swordClipAsset;
        }

        private void Awake()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = musicVolume;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = sfxVolume;

            pickupClip = CreateToneClip("SFX_Pickup", 523.25f, 659.25f, 0.14f);
            storeClip = CreateToneClip("SFX_Store", 659.25f, 987.77f, 0.26f);
            strangeClip = CreateToneClip("SFX_Strange", 311.13f, 233.08f, 0.48f);
            swordClip = CreateToneClip("SFX_Sword", 587.33f, 1174.66f, 0.55f);
            tenderChimeClip = CreateWarmChordClip("SFX_TenderChime", 523.25f, 659.25f, 783.99f, 0.85f);
            portalWhooshClip = CreatePortalWhooshClip("SFX_PortalWhoosh", 0.80f);
            softMagicBurstClip = CreateWarmChordClip("SFX_SoftMagicBurst", 659.25f, 987.77f, 1318.51f, 0.65f);
        }

        public void PlayTenderChime() => PlayClip(tenderChimeClip);

        public void PlayIntroPortalExitEffects()
        {
            suppressAutoMusicOnStart = true;
            StopMusic();
            PlayClip(portalWhooshClip);
            PlayClip(softMagicBurstClip);
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void SetExplorationMusicFade(float normalizedFade)
        {
            if (musicSource == null)
            {
                return;
            }

            if (!musicSource.isPlaying && explorationMusicClip != null)
            {
                swordMusicActive = false;
                musicSource.clip = explorationMusicClip;
                musicSource.loop = true;
                musicSource.mute = isMuted;
                musicSource.Play();
                if (isPaused)
                {
                    musicSource.Pause();
                }
            }

            musicSource.volume = musicVolume * Mathf.Clamp01(normalizedFade);
        }

        private void Start()
        {
            if (!suppressAutoMusicOnStart)
            {
                PlayExplorationMusic();
            }
        }

        public void PlayExplorationMusic()
        {
            swordMusicActive = false;
            PlayLoopingTrack(explorationMusicClip);
        }

        public void PlaySwordMusic()
        {
            if (swordMusicActive)
            {
                return;
            }

            swordMusicActive = true;
            PlayLoopingTrack(swordMusicClip != null ? swordMusicClip : explorationMusicClip);
        }

        public void SetMuted(bool muted)
        {
            isMuted = muted;
            if (musicSource != null)
            {
                musicSource.mute = isMuted;
            }

            if (sfxSource != null)
            {
                sfxSource.mute = isMuted;
            }
        }

        public void ToggleMute()
        {
            SetMuted(!isMuted);
        }

        public void SetPaused(bool paused)
        {
            isPaused = paused;
            if (musicSource == null)
            {
                return;
            }

            if (isPaused)
            {
                musicSource.Pause();
            }
            else if (musicSource.clip != null)
            {
                musicSource.UnPause();
            }
        }

        public void PlayPickup() => PlayClip(pickupClip);
        public void PlayStore() => PlayClip(storeClip);
        public void PlayStrangeStinger() => PlayClip(strangeClip);

        public void PlaySwordReveal()
        {
            // Regressao 2: Quando a menina encontra a espada, a musica deve parar e ficar apenas o silencio.
            // A nova musica ocorre apenas quando comeca a Arena Imaginaria.
            StopMusic();
        }

        private void PlayLoopingTrack(AudioClip clip)
        {
            if (musicSource == null || clip == null)
            {
                return;
            }

            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.mute = isMuted;
            musicSource.Play();

            if (isPaused)
            {
                musicSource.Pause();
            }
        }

        private void PlayClip(AudioClip clip)
        {
            if (sfxSource != null && clip != null && !isMuted)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        private static AudioClip CreateToneClip(string clipName, float startFreq, float endFreq, float duration)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            var samples = new float[sampleCount];

            float phase = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += (2f * Mathf.PI * freq) / sampleRate;
                float env = Mathf.Sin(t * Mathf.PI);
                samples[i] = Mathf.Sin(phase) * env * 0.35f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateWarmChordClip(string clipName, float f1, float f2, float f3, float duration)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            var samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float norm = (float)i / sampleCount;
                float env = Mathf.Sin(Mathf.Clamp01(norm / 0.12f) * Mathf.PI * 0.5f) * Mathf.Exp(-norm * 3.2f);
                float w1 = Mathf.Sin(2f * Mathf.PI * f1 * t);
                float w2 = Mathf.Sin(2f * Mathf.PI * f2 * Mathf.Max(0f, t - 0.06f));
                float w3 = Mathf.Sin(2f * Mathf.PI * f3 * Mathf.Max(0f, t - 0.12f));
                samples[i] = (w1 * 0.38f + w2 * 0.34f + w3 * 0.28f) * env * 0.26f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreatePortalWhooshClip(string clipName, float duration)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            var samples = new float[sampleCount];
            var rng = new System.Random(1337);
            float lowPass = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float norm = (float)i / sampleCount;
                float rawNoise = ((float)rng.NextDouble() * 2f - 1f) * (1f - norm);
                float alpha = Mathf.Lerp(0.42f, 0.06f, norm);
                lowPass += alpha * (rawNoise - lowPass);
                float env = norm < 0.1f ? (norm / 0.1f) : (1f - ((norm - 0.1f) / 0.9f));
                samples[i] = lowPass * env * 0.32f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
