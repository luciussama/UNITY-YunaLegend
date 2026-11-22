using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    /// <summary>
    /// Sintetizador em runtime (com cache de AudioClips) para os sons de passos e pousos
    /// especificos de cada objeto pisado pela Yuna no Quarto Escuro.
    /// </summary>
    public static class DarkRoomSurfaceAudioSynthesizer
    {
        private const int DefaultSampleRate = 22050;
        private static readonly Dictionary<int, AudioClip> StepClipCache = new Dictionary<int, AudioClip>();
        private static readonly Dictionary<int, AudioClip> LandClipCache = new Dictionary<int, AudioClip>();

        public static AudioClip GetOrCreateStepClip(int platformIndex, int stepCount, out DarkRoomFootstepAudioSpec spec)
        {
            spec = DarkRoomSurfaceSoundCatalog.EvaluateStepSpec(platformIndex, stepCount, isLanding: false);
            int variation = Mathf.Abs(stepCount) % 6;
            int cacheKey = ((int)spec.Kind * 32) + variation;

            if (StepClipCache.TryGetValue(cacheKey, out AudioClip cached) && cached != null)
            {
                return cached;
            }

            float[] pcm = DarkRoomSurfaceSoundCatalog.SynthesizeSamples(spec, DefaultSampleRate);
            var clip = AudioClip.Create(
                $"Step_{spec.Kind}_{variation}",
                pcm.Length,
                1,
                DefaultSampleRate,
                false);
            clip.SetData(pcm, 0);
            StepClipCache[cacheKey] = clip;
            return clip;
        }

        public static AudioClip GetOrCreateLandingClip(int platformIndex, int stepCount, out DarkRoomFootstepAudioSpec spec)
        {
            spec = DarkRoomSurfaceSoundCatalog.EvaluateStepSpec(platformIndex, stepCount, isLanding: true);
            int variation = Mathf.Abs(stepCount) % 4;
            int cacheKey = ((int)spec.Kind * 32) + variation;

            if (LandClipCache.TryGetValue(cacheKey, out AudioClip cached) && cached != null)
            {
                return cached;
            }

            float[] pcm = DarkRoomSurfaceSoundCatalog.SynthesizeSamples(spec, DefaultSampleRate);
            var clip = AudioClip.Create(
                $"Land_{spec.Kind}_{variation}",
                pcm.Length,
                1,
                DefaultSampleRate,
                false);
            clip.SetData(pcm, 0);
            LandClipCache[cacheKey] = clip;
            return clip;
        }
    }
}
