using System;

namespace YunaLegend.DarkRoom
{
    public enum DarkRoomSurfaceSoundKind
    {
        HardWood = 0,
        PlushSoft = 1,
        PaperBooks = 2,
        ToyDrum = 3,
        MusicBoxTine = 4,
        XylophoneKey = 5,
        CardstockCastle = 6,
        CrystalChandelier = 7,
        TinMetal = 8
    }

    public readonly struct DarkRoomFootstepAudioSpec
    {
        public DarkRoomSurfaceSoundKind Kind { get; }
        public float FundamentalHz { get; }
        public float SecondaryHz { get; }
        public float TertiaryHz { get; }
        public float DurationSeconds { get; }
        public float DecayRate { get; }
        public float NoiseStrength { get; }
        public float Volume { get; }

        public DarkRoomFootstepAudioSpec(
            DarkRoomSurfaceSoundKind kind,
            float fundamentalHz,
            float secondaryHz,
            float tertiaryHz,
            float durationSeconds,
            float decayRate,
            float noiseStrength,
            float volume)
        {
            Kind = kind;
            FundamentalHz = fundamentalHz;
            SecondaryHz = secondaryHz;
            TertiaryHz = tertiaryHz;
            DurationSeconds = durationSeconds;
            DecayRate = decayRate;
            NoiseStrength = noiseStrength;
            Volume = volume;
        }
    }

    /// <summary>
    /// Catalogo acustico puro (Data-Driven, sem dependencia de MonoBehaviour) que mapeia cada
    /// plataforma do Quarto Escuro (0..37) para o timbre fisico real dos passos e pousos da menina.
    /// </summary>
    public static class DarkRoomSurfaceSoundCatalog
    {
        private static readonly float[] XylophoneScaleHz =
        {
            523.25f, // C5
            587.33f, // D5
            659.25f, // E5
            783.99f, // G5
            880.00f, // A5
            1046.50f // C6
        };

        private static readonly float[] MusicBoxScaleHz =
        {
            987.77f,  // B5
            1174.66f, // D6
            1318.51f, // E6
            1567.98f, // G6
            1760.00f  // A6
        };

        private static readonly float[] CrystalScaleHz =
        {
            1396.91f, // F6
            1760.00f, // A6
            2093.00f, // C7
            2637.02f  // E7
        };

        public static DarkRoomSurfaceSoundKind ResolveSurfaceKind(int platformIndex)
        {
            return platformIndex switch
            {
                0 or 6 or 12 or 23 or 27 or 35 => DarkRoomSurfaceSoundKind.PlushSoft,
                1 or 15 or 16 or 24 or 25 or 36 => DarkRoomSurfaceSoundKind.PaperBooks,
                5 or 26 => DarkRoomSurfaceSoundKind.ToyDrum,
                8 or 31 => DarkRoomSurfaceSoundKind.MusicBoxTine,
                28 => DarkRoomSurfaceSoundKind.XylophoneKey,
                30 => DarkRoomSurfaceSoundKind.CardstockCastle,
                17 => DarkRoomSurfaceSoundKind.CrystalChandelier,
                14 or 18 or 29 or 32 => DarkRoomSurfaceSoundKind.TinMetal,
                _ => DarkRoomSurfaceSoundKind.HardWood
            };
        }

        public static DarkRoomFootstepAudioSpec EvaluateStepSpec(int platformIndex, int stepCount, bool isLanding = false)
        {
            DarkRoomSurfaceSoundKind kind = ResolveSurfaceKind(platformIndex);
            int safeStep = Math.Abs(stepCount);
            float landBoost = isLanding ? 1.22f : 1.0f;

            switch (kind)
            {
                case DarkRoomSurfaceSoundKind.XylophoneKey:
                {
                    // Barra de xilofone/metalofone (harmônicos inarmônicos 1 : 2.76 : 5.40)
                    float f0 = XylophoneScaleHz[safeStep % XylophoneScaleHz.Length];
                    return new DarkRoomFootstepAudioSpec(
                        kind,
                        f0,
                        f0 * 2.76f,
                        f0 * 5.40f,
                        isLanding ? 0.34f : 0.26f,
                        13.5f,
                        0.12f,
                        0.56f * landBoost);
                }

                case DarkRoomSurfaceSoundKind.ToyDrum:
                {
                    // Membrana de tambor infantil (190-235 Hz + esteira snare ruidosa)
                    float f0 = 195f + ((safeStep % 3) * 18f);
                    return new DarkRoomFootstepAudioSpec(
                        kind,
                        f0,
                        f0 * 1.58f,
                        f0 * 2.42f,
                        isLanding ? 0.24f : 0.16f,
                        22.0f,
                        0.46f,
                        0.54f * landBoost);
                }

                case DarkRoomSurfaceSoundKind.MusicBoxTine:
                {
                    // Pente metalico de caixa de musica (pinça aguda cristalina)
                    float f0 = MusicBoxScaleHz[safeStep % MusicBoxScaleHz.Length];
                    return new DarkRoomFootstepAudioSpec(
                        kind,
                        f0,
                        f0 * 2.98f,
                        f0 * 5.85f,
                        isLanding ? 0.36f : 0.28f,
                        11.5f,
                        0.08f,
                        0.48f * landBoost);
                }

                case DarkRoomSurfaceSoundKind.CardstockCastle:
                {
                    // Cartas de baralho empilhadas (estalo crisp de papel cartao + friccao)
                    float f0 = 540f + ((safeStep % 3) * 65f);
                    return new DarkRoomFootstepAudioSpec(
                        kind,
                        f0,
                        1850f + ((safeStep % 2) * 320f),
                        3400f,
                        isLanding ? 0.12f : 0.085f,
                        46.0f,
                        0.68f,
                        0.50f * landBoost);
                }

                case DarkRoomSurfaceSoundKind.CrystalChandelier:
                {
                    // Pingentes de cristal facetado (parciais inarmonicos brilhantes 1 : 2.76 : 5.40)
                    float f0 = CrystalScaleHz[safeStep % CrystalScaleHz.Length];
                    return new DarkRoomFootstepAudioSpec(
                        kind,
                        f0,
                        f0 * 2.76f,
                        f0 * 5.12f,
                        isLanding ? 0.38f : 0.28f,
                        10.5f,
                        0.06f,
                        0.46f * landBoost);
                }

                case DarkRoomSurfaceSoundKind.TinMetal:
                {
                    // Lata / metal oco (robo de lata, trilho, globo, varao)
                    float f0 = 310f + ((safeStep % 3) * 35f);
                    return new DarkRoomFootstepAudioSpec(
                        kind,
                        f0,
                        f0 * 2.14f,
                        f0 * 3.82f,
                        isLanding ? 0.16f : 0.11f,
                        32.0f,
                        0.24f,
                        0.48f * landBoost);
                }

                case DarkRoomSurfaceSoundKind.PaperBooks:
                {
                    // Capa dura de livro e folhear de papel
                    float f0 = 230f + ((safeStep % 3) * 24f);
                    return new DarkRoomFootstepAudioSpec(
                        kind,
                        f0,
                        f0 * 1.85f,
                        1600f,
                        isLanding ? 0.13f : 0.09f,
                        42.0f,
                        0.38f,
                        0.46f * landBoost);
                }

                case DarkRoomSurfaceSoundKind.PlushSoft:
                {
                    // Pelucia / veludo / tecido macio (grave abafado)
                    float f0 = 92f + ((safeStep % 3) * 11f);
                    return new DarkRoomFootstepAudioSpec(
                        kind,
                        f0,
                        f0 * 1.65f,
                        240f,
                        isLanding ? 0.14f : 0.095f,
                        34.0f,
                        0.15f,
                        0.44f * landBoost);
                }

                default:
                {
                    // Madeira macica (penteadeira, comoda, blocos ABC, castelinho)
                    float f0 = 390f + ((safeStep % 3) * 38f);
                    return new DarkRoomFootstepAudioSpec(
                         DarkRoomSurfaceSoundKind.HardWood,
                        f0,
                        f0 * 0.52f,
                        f0 * 1.48f,
                        isLanding ? 0.12f : 0.075f,
                        58.0f,
                        0.22f,
                        0.45f * landBoost);
                }
            }
        }

        public static float[] SynthesizeSamples(DarkRoomFootstepAudioSpec spec, int sampleRate = 22050)
        {
            int safeRate = Math.Max(8000, sampleRate);
            int sampleCount = Math.Max(64, (int)Math.Round(safeRate * Math.Max(0.04f, spec.DurationSeconds)));
            var samples = new float[sampleCount];
            uint rng = (uint)(1469598103u ^ ((int)spec.Kind * 16777619) ^ (int)spec.FundamentalHz);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / safeRate;
                float u = (float)i / sampleCount;

                // Ataque percussivo rapido (2.5ms) + decaimento exponencial multi-taxa
                float attack = Math.Min(1f, t / 0.0025f);
                float env1 = attack * (float)Math.Exp(-t * spec.DecayRate);
                float env2 = attack * (float)Math.Exp(-t * spec.DecayRate * 1.65f);
                float env3 = attack * (float)Math.Exp(-t * spec.DecayRate * 2.45f);

                // Pequeno pitch drop inicial em membranas (tambor/pelucia/madeira)
                float pitchSweep = (spec.Kind == DarkRoomSurfaceSoundKind.ToyDrum || spec.Kind == DarkRoomSurfaceSoundKind.PlushSoft)
                    ? (1f + (0.28f * (float)Math.Exp(-t * 45f)))
                    : 1f;

                float w1 = (float)Math.Sin(2.0 * Math.PI * spec.FundamentalHz * pitchSweep * t) * env1;
                float w2 = (float)Math.Sin(2.0 * Math.PI * spec.SecondaryHz * t) * env2 * 0.46f;
                float w3 = (float)Math.Sin(2.0 * Math.PI * spec.TertiaryHz * t) * env3 * 0.24f;

                rng ^= rng << 13;
                rng ^= rng >> 17;
                rng ^= rng << 5;
                float white = ((rng & 0xFFFF) / 32768f) - 1f;
                float noiseEnv = attack * (float)Math.Exp(-t * (spec.DecayRate * 1.9f));
                float noise = white * noiseEnv * spec.NoiseStrength;

                float mixed = (w1 * 0.68f + w2 + w3 + noise) * spec.Volume;
                if (mixed > 0.96f) mixed = 0.96f;
                if (mixed < -0.96f) mixed = -0.96f;
                samples[i] = mixed;
            }

            return samples;
        }
    }
}
