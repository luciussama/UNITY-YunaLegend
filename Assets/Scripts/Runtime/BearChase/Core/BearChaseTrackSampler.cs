using System;

namespace YunaLegend.BearChase.Core
{
    /// <summary>
    /// Amostrador matematico unico da topografia curva e ondulada do corredor (em metros).
    /// Compartilhado entre Core, Runtime e Editor para garantir alinhamento exato.
    /// </summary>
    public static class BearChaseTrackSampler
    {
        public static float ComputeEnvelope(float z, BearChaseTrackSpec spec = null)
        {
            BearChaseTrackSpec s = spec ?? BearChaseTrackSpec.Default;
            float clampedZ = Math.Max(0f, Math.Min(s.TrackLength + 20f, z));
            float fadeIn = Math.Min(1f, clampedZ / s.EnvelopeFadeIn);
            float openingEnd = Math.Min(s.TrackLength, BearChaseTrackSpec.FinaleStartZ);
            float fadeOut = Math.Min(1f, Math.Max(0f, (openingEnd - clampedZ) / s.EnvelopeFadeOut));
            return fadeIn * fadeOut;
        }

        public static float CenterX(float z, BearChaseTrackSpec spec = null)
        {
            BearChaseTrackSpec s = spec ?? BearChaseTrackSpec.Default;
            float clampedZ = Math.Max(0f, Math.Min(s.TrackLength + 20f, z));
            float envelope = ComputeEnvelope(clampedZ, s);
            double primary = Math.Sin(clampedZ * 0.038) * 5.6;
            double secondary = Math.Sin(clampedZ * 0.017 + 0.85) * 2.9;
            float finaleZ = clampedZ - BearChaseTrackSpec.FinaleStartZ;
            double finale = 5.5 * Math.Sin(finaleZ * 0.125) * FinaleEnvelope(clampedZ, s);
            return (float)((primary + secondary) * envelope + finale);
        }

        public static float ElevationY(float z, BearChaseTrackSpec spec = null)
        {
            BearChaseTrackSpec s = spec ?? BearChaseTrackSpec.Default;
            float clampedZ = Math.Max(0f, Math.Min(s.TrackLength + 20f, z));
            float envelope = ComputeEnvelope(clampedZ, s);
            double primary = Math.Sin(clampedZ * 0.044) * 1.55;
            double secondary = Math.Sin(clampedZ * 0.020 + 1.25) * 0.95;
            float finaleZ = clampedZ - BearChaseTrackSpec.FinaleStartZ;
            double finale = 2.55 * Math.Sin(finaleZ * 0.135) * FinaleEnvelope(clampedZ, s);
            return (float)((primary + secondary) * envelope + finale);
        }

        public static float TangentSlopeX(float z, BearChaseTrackSpec spec = null)
        {
            const float eps = 0.5f;
            return (CenterX(z + eps, spec) - CenterX(z - eps, spec)) / (2f * eps);
        }

        public static float TangentSlopeY(float z, BearChaseTrackSpec spec = null)
        {
            const float eps = 0.5f;
            return (ElevationY(z + eps, spec) - ElevationY(z - eps, spec)) / (2f * eps);
        }

        public static float YawRadians(float z, BearChaseTrackSpec spec = null)
        {
            return (float)Math.Atan2(TangentSlopeX(z, spec), 1.0);
        }

        public static float PitchRadians(float z, BearChaseTrackSpec spec = null)
        {
            return (float)-Math.Atan2(TangentSlopeY(z, spec), 1.0);
        }

        public static float BankDegrees(float z, BearChaseTrackSpec spec = null)
        {
            const float eps = 1.0f;
            float curvature = (TangentSlopeX(z + eps, spec) - TangentSlopeX(z - eps, spec)) / (2f * eps);
            float raw = -curvature * 115f;
            return Math.Max(-5.5f, Math.Min(5.5f, raw));
        }

        public static float ThrowPressure(float z, BearChaseTrackSpec spec = null)
        {
            BearChaseTrackSpec s = spec ?? BearChaseTrackSpec.Default;
            // Look slightly ahead so the bear starts its windup on entry to the tight sector.
            float sampleZ = z + 4f;
            float envelope = FinaleEnvelope(sampleZ, s);
            if (envelope <= 0f)
            {
                return 0f;
            }

            float turnRate = Math.Abs(TangentSlopeX(sampleZ + 1f, s) - TangentSlopeX(sampleZ - 1f, s)) * 0.5f;
            float rampGrade = Math.Abs(TangentSlopeY(sampleZ, s));
            float pressure = Math.Max(SmoothUnit((turnRate - 0.025f) / 0.055f), SmoothUnit((rampGrade - 0.12f) / 0.20f));
            return envelope * pressure;
        }

        private static float FinaleEnvelope(float z, BearChaseTrackSpec spec)
        {
            float entry = SmoothUnit((z - BearChaseTrackSpec.FinaleStartZ) / 16f);
            float exit = SmoothUnit((spec.TrackLength - z) / spec.EnvelopeFadeOut);
            return entry * exit;
        }

        private static float SmoothUnit(float value)
        {
            float t = Math.Max(0f, Math.Min(1f, value));
            return t * t * (3f - 2f * t);
        }
    }
}
