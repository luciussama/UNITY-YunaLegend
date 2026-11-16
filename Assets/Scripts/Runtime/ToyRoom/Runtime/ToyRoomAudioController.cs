using UnityEngine;

namespace YunaLegend.ToyRoom.Runtime
{
    /// <summary>
    /// Reproduz SFX procedurais e leves para coleta, guarda no bau, estranhamento e revelacao da espada.
    /// </summary>
    public sealed class ToyRoomAudioController : MonoBehaviour
    {
        private AudioSource sfxSource;
        private AudioClip pickupClip;
        private AudioClip storeClip;
        private AudioClip strangeClip;
        private AudioClip swordClip;

        private void Awake()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 0.45f;

            pickupClip = CreateToneClip("SFX_Pickup", 523.25f, 659.25f, 0.14f);
            storeClip = CreateToneClip("SFX_Store", 659.25f, 987.77f, 0.26f);
            strangeClip = CreateToneClip("SFX_Strange", 311.13f, 233.08f, 0.48f);
            swordClip = CreateToneClip("SFX_Sword", 587.33f, 1174.66f, 0.55f);
        }

        public void PlayPickup() => PlayClip(pickupClip);
        public void PlayStore() => PlayClip(storeClip);
        public void PlayStrangeStinger() => PlayClip(strangeClip);
        public void PlaySwordReveal() => PlayClip(swordClip);

        private void PlayClip(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
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
