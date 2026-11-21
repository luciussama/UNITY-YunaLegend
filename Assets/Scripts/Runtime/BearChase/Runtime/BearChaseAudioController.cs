using UnityEngine;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Controlador de audio da Fase 4: musica de fundo e efeitos curtos sintetizados
    /// para acordar do urso, colisao/tropeço e vitoria.
    /// </summary>
    public sealed class BearChaseAudioController : MonoBehaviour
    {
        [SerializeField] private AudioClip chaseMusic;
        private AudioSource musicSource;
        private AudioSource sfxSource;
        private AudioClip wakeClip;
        private AudioClip hitClip;
        private AudioClip victoryClip;

        public bool IsMuted { get; private set; }

        public void Configure(AudioClip music)
        {
            chaseMusic = music;
        }

        private void Awake()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.34f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.55f;

            wakeClip = CreateToneClip("BearWake", 240f, 390f, 0.22f);
            hitClip = CreateToneClip("StumbleHit", 170f, 95f, 0.20f);
            victoryClip = CreateToneClip("ChaseVictory", 440f, 660f, 0.38f);
        }

        private void Start()
        {
            if (chaseMusic != null && musicSource != null)
            {
                musicSource.clip = chaseMusic;
                musicSource.Play();
            }
        }

        public void PlayBearWake()
        {
            if (!IsMuted && wakeClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(wakeClip);
            }
        }

        public void PlayHit()
        {
            if (!IsMuted && hitClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(hitClip);
            }
        }

        public void PlayVictory()
        {
            if (!IsMuted && victoryClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(victoryClip);
            }
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
                data[i] = Mathf.Sin(phase) * env * 0.35f;
            }

            var clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
