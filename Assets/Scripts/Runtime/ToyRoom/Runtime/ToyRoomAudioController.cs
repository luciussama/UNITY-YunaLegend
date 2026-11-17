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
        private bool isMuted;
        private bool isPaused;
        private bool swordMusicActive;

        public bool IsMuted => isMuted;

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
        }

        private void Start()
        {
            PlayExplorationMusic();
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
            PlayClip(swordClip);
            PlaySwordMusic();
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
    }
}
