using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomAudioAssetBuilder
    {
        private const string AudioFolder = "Assets/Audio/SFX";
        private const int SampleRate = 44100;

        public static void GenerateAllSfx()
        {
            if (!Directory.Exists(AudioFolder))
            {
                Directory.CreateDirectory(AudioFolder);
            }

            // 1. Passos de Madeira (3 variacoes para evitar efeito metronomo)
            SaveWav(GenerateFootstepWood(420f, 0.07f), Path.Combine(AudioFolder, "footstep_wood_1.wav"));
            SaveWav(GenerateFootstepWood(465f, 0.07f), Path.Combine(AudioFolder, "footstep_wood_2.wav"));
            SaveWav(GenerateFootstepWood(385f, 0.08f), Path.Combine(AudioFolder, "footstep_wood_3.wav"));

            // 2. Salto Balistico (whoosh aereo suave com subida de frequencia)
            SaveWav(GenerateJumpSfx(), Path.Combine(AudioFolder, "jump_airy.wav"));

            // 3. Aterrissagens diferenciadas por superficie
            SaveWav(GenerateLandWoodSfx(), Path.Combine(AudioFolder, "land_wood.wav"));
            SaveWav(GenerateLandPlushSfx(), Path.Combine(AudioFolder, "land_plush.wav"));
            SaveWav(GenerateDrumStrikeSfx(), Path.Combine(AudioFolder, "drum_strike.wav"));

            // 4. Queda Cinematica Comica da Porta Falsa (thud no chao de carvalho + squeak)
            SaveWav(GenerateComicFallThudSfx(), Path.Combine(AudioFolder, "comic_fall_thud.wav"));

            // 5. Brilho Magico da Fadinha Nanda (arpejo de sinos celestiais)
            SaveWav(GenerateFairyTwinkleSfx(), Path.Combine(AudioFolder, "fairy_twinkle.wav"));

            // 6. Derrota por queda ou esmagamento
            SaveWav(GenerateGameOverSfx(), Path.Combine(AudioFolder, "game_over_fall.wav"));

            AssetDatabase.Refresh();
            Debug.Log("[DarkRoomAudioAssetBuilder] Todos os 10 arquivos de audio SFX gerados com sucesso!");
        }

        // =========================================================================
        // SINTESE DE FORMAS DE ONDA (PCM 16-BIT 44.1kHz)
        // =========================================================================

        private static float[] GenerateFootstepWood(float baseFreq, float duration)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random();

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 65f); // Decaimento percussivo rapido
                float click = (i < 40) ? (float)(rand.NextDouble() * 2.0 - 1.0) * 0.4f : 0f;
                float woodTone = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.7f;
                float woodSub = Mathf.Sin(2f * Mathf.PI * (baseFreq * 0.5f) * t) * 0.3f;
                samples[i] = Mathf.Clamp((click + woodTone + woodSub) * env * 0.55f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateJumpSfx()
        {
            float duration = 0.16f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random();

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / duration; // 0..1
                float env = Mathf.Sin(t * Mathf.PI); // Envelope em sino suave
                float freq = Mathf.Lerp(280f, 620f, t * t); // Sweep ascendente
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SampleRate);
                float breath = ((float)rand.NextDouble() * 2f - 1f) * 0.25f;
                samples[i] = (tone * 0.75f + breath) * env * 0.50f;
            }
            return samples;
        }

        private static float[] GenerateLandWoodSfx()
        {
            float duration = 0.14f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random();

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 38f);
                float impact = (i < 80) ? (float)(rand.NextDouble() * 2.0 - 1.0) * 0.5f : 0f;
                float thud = Mathf.Sin(2f * Mathf.PI * 130f * t) * 0.75f;
                float sub = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.45f;
                samples[i] = Mathf.Clamp((impact + thud + sub) * env * 0.65f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateLandPlushSfx()
        {
            float duration = 0.13f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 30f);
                float thudLow = Mathf.Sin(2f * Mathf.PI * 85f * t) * 0.8f;
                float feltTone = Mathf.Sin(2f * Mathf.PI * 170f * t) * 0.2f;
                samples[i] = (thudLow + feltTone) * env * 0.50f;
            }
            return samples;
        }

        private static float[] GenerateDrumStrikeSfx()
        {
            float duration = 0.26f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random();

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float envTone = Mathf.Exp(-t * 18f);
                float envSnare = Mathf.Exp(-t * 32f);

                float head = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.7f;
                float harmonic = Mathf.Sin(2f * Mathf.PI * 340f * t) * 0.3f;
                float snare = ((float)rand.NextDouble() * 2f - 1f) * envSnare * 0.5f;

                samples[i] = Mathf.Clamp((head + harmonic) * envTone * 0.65f + snare, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateComicFallThudSfx()
        {
            float duration = 0.42f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random();

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;

                // Parte 1: Thud pesado no piso de madeira (0 a 0.2s)
                float thudEnv = Mathf.Exp(-t * 22f);
                float impactNoise = (i < 120) ? (float)(rand.NextDouble() * 2.0 - 1.0) * 0.6f : 0f;
                float thud = (Mathf.Sin(2f * Mathf.PI * 95f * t) + impactNoise) * thudEnv * 0.7f;

                // Parte 2: Squeak cômico agudo logo após o impacto (0.12s a 0.38s)
                float squeak = 0f;
                if (t > 0.10f && t < 0.38f)
                {
                    float sqT = t - 0.10f;
                    float sqEnv = Mathf.Sin(sqT / 0.28f * Mathf.PI);
                    float sqFreq = Mathf.Lerp(520f, 880f, Mathf.Sin(sqT * 18f));
                    squeak = Mathf.Sin(2f * Mathf.PI * sqFreq * t) * sqEnv * 0.35f;
                }

                samples[i] = Mathf.Clamp(thud + squeak, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateFairyTwinkleSfx()
        {
            float duration = 0.45f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            float[] notes = new float[] { 1318.51f, 1760.00f, 2093.00f, 2637.02f }; // E6, A6, C7, E7
            float noteDelay = 0.055f;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float sampleAcc = 0f;

                for (int n = 0; n < notes.Length; n++)
                {
                    float noteT = t - (n * noteDelay);
                    if (noteT > 0f)
                    {
                        float env = Mathf.Exp(-noteT * 12f);
                        float bell = Mathf.Sin(2f * Mathf.PI * notes[n] * noteT);
                        float shimmer = Mathf.Sin(2f * Mathf.PI * (notes[n] * 2.01f) * noteT) * 0.25f;
                        sampleAcc += (bell + shimmer) * env * 0.20f;
                    }
                }

                samples[i] = Mathf.Clamp(sampleAcc, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateGameOverSfx()
        {
            float duration = 0.40f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / duration; // 0..1
                float env = (1.0f - t);
                float freq = Mathf.Lerp(480f, 140f, t); // Slide descendente de cartoon
                float tone = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SampleRate);
                samples[i] = tone * env * 0.45f;
            }
            return samples;
        }

        // =========================================================================
        // ESCRITOR DE ARQUIVO WAV PADRAO (16-BIT PCM MONO)
        // =========================================================================

        private static void SaveWav(float[] samples, string filepath)
        {
            using (var stream = new FileStream(filepath, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                int subChunk2Size = samples.Length * 2; // 2 bytes per sample (16 bit)
                int chunkSize = 36 + subChunk2Size;

                // RIFF header
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(chunkSize);
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });

                // fmt subchunk
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16); // SubChunk1Size (16 for PCM)
                writer.Write((short)1); // AudioFormat (1 for PCM)
                writer.Write((short)1); // NumChannels (1 = Mono)
                writer.Write(SampleRate); // SampleRate
                writer.Write(SampleRate * 2); // ByteRate (SampleRate * NumChannels * BitsPerSample/8)
                writer.Write((short)2); // BlockAlign (NumChannels * BitsPerSample/8)
                writer.Write((short)16); // BitsPerSample

                // data subchunk
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(subChunk2Size);

                for (int i = 0; i < samples.Length; i++)
                {
                    short val = (short)Mathf.Clamp(samples[i] * 32767f, -32768f, 32767f);
                    writer.Write(val);
                }
            }
        }
    }
}
