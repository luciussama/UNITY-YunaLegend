using UnityEngine;

namespace YunaLegend.Picnic.Runtime
{
    public sealed class PicnicAudioController : MonoBehaviour
    {
        private AudioSource music;
        public void Build()
        {
            music = gameObject.AddComponent<AudioSource>();
            music.clip = Resources.Load<AudioClip>("Picnic/garden-music");
            music.loop = true;
            music.volume = 0.34f;
            AudioListener.volume = PlayerPrefs.GetFloat("YunaLegend.MasterVolume", 1f);
            music.Play();
        }
        public void SetPaused(bool paused)
        {
            if (paused) music.Pause();
            else music.UnPause();
        }
    }
}
