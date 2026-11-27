using UnityEngine;
using YunaLegend.DreamForest.Core;
using YunaLegend.DreamForestEntrance.Core;

namespace YunaLegend.DreamForestEntrance.Runtime
{
    /// <summary>Audio adapter limited to the entrance: quiet hum and the final music crossfade.</summary>
    public sealed class DreamEntranceAmbience : MonoBehaviour
    {
        private AudioSource ambience;
        private AudioSource music;
        private AudioClip buffer;
        private bool wasOpening;

        public void Build(AudioSource musicSource)
        {
            music=musicSource;
            ambience=gameObject.AddComponent<AudioSource>();
            ambience.playOnAwake=false;
            ambience.loop=true;
            ambience.spatialBlend=0;
            const int rate=48000;
            buffer=AudioClip.Create("Dream entrance quiet wind",rate,1,rate,false);
            buffer.SetData(DreamEntranceSound.CreateSamples(rate),0);
            ambience.clip=buffer;
            ambience.volume=0;
        }

        public void Apply(DreamForestSimulation sim,bool paused,bool muted)
        {
            bool opening=sim.Stage==DreamForestStage.Opening && sim.Opening!=null;
            if (!opening)
            {
                if (ambience.isPlaying) ambience.Stop();
                if (wasOpening && music!=null) music.volume=.35f;
                wasOpening=false;
                return;
            }
            wasOpening=true;
            float mix=DreamEntranceSound.MusicMix(sim.Opening.Index,sim.Opening.Age);
            if (music!=null) music.volume=.35f*mix;
            if (paused || muted)
            {
                ambience.Stop();
                ambience.volume=0;
                return;
            }
            if (!ambience.isPlaying) ambience.Play();
            float blend=1-Mathf.Exp(-Time.unscaledDeltaTime/.12f);
            ambience.volume=Mathf.Lerp(ambience.volume,.5f*(1-mix),blend);
        }

        private void OnDestroy()
        {
            if (buffer!=null) Destroy(buffer);
        }
    }
}
