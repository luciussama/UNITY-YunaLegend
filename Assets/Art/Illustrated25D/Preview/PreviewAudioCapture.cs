using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YunaLegend.Illustrated25D.Preview
{
    /// <summary>Opt-in capture of the final audio mix for evidence. Verification only; never changes gameplay audio.</summary>
    public sealed class PreviewAudioCapture : MonoBehaviour
    {
        readonly List<float> samples = new List<float>(48000 * 2 * 40);
        readonly object gate = new object();
        readonly List<Marker> marks = new List<Marker>();
        string wavPath;
        int rate, channels;
        int capturedFrames;
        bool recording, wrote;
        static PreviewAudioCapture instance;

        [Serializable]
        public struct Marker
        {
            public string label;
            public int frame;
        }

        public static PreviewAudioCapture Begin(string destination)
        {
            if (instance != null) return instance;
            var go = new GameObject("T06_AudioCapture");
            DontDestroyOnLoad(go);
            go.AddComponent<AudioListener>();
            instance = go.AddComponent<PreviewAudioCapture>();
            instance.wavPath = destination;
            instance.rate = Mathf.Max(8000, AudioSettings.outputSampleRate);
            instance.recording = true;
            instance.DisableOtherListeners();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            return instance;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => DisableOtherListeners();

        void DisableOtherListeners()
        {
            foreach (var listener in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
                if (listener != null && listener.gameObject != gameObject) listener.enabled = false;
        }

        void LateUpdate()
        {
            var target = Camera.main;
            if (target != null) transform.position = target.transform.position;
        }

        public void Mark(string label)
        {
            if (!recording) return;
            lock (gate) marks.Add(new Marker { label = label, frame = capturedFrames });
        }

        void OnAudioFilterRead(float[] data, int channelCount)
        {
            if (!recording || channelCount <= 0) return;
            lock (gate)
            {
                if (channels == 0) channels = channelCount;
                if (capturedFrames >= rate * 120) return;
                samples.AddRange(data);
                capturedFrames += data.Length / channels;
            }
        }

        public void StopAndWrite()
        {
            if (wrote) return;
            wrote = true;
            recording = false;
            Thread.Sleep(80);
            float[] data;
            List<Marker> captured;
            lock (gate)
            {
                data = samples.ToArray();
                captured = new List<Marker>(marks);
            }
            int channelCount = Mathf.Max(1, channels);
            WriteWav(wavPath, data, channelCount, rate);
            var json = new StringBuilder();
            json.Append("{\n");
            json.Append("  \"wav\": \"").Append(Path.GetFileName(wavPath)).Append("\",\n");
            json.Append("  \"sampleRate\": ").Append(rate).Append(",\n");
            json.Append("  \"channels\": ").Append(channelCount).Append(",\n");
            json.Append("  \"frames\": ").Append(data.Length / channelCount).Append(",\n");
            json.Append("  \"seconds\": ").Append((data.Length / (double)channelCount / rate).ToString("F3", CultureInfo.InvariantCulture)).Append(",\n");
            json.Append("  \"marks\": [");
            for (int i = 0; i < captured.Count; i++)
            {
                if (i > 0) json.Append(',');
                json.Append("\n    {\"label\": \"").Append(captured[i].label).Append("\", \"frame\": ").Append(captured[i].frame)
                    .Append(", \"seconds\": ").Append((captured[i].frame / (double)rate).ToString("F3", CultureInfo.InvariantCulture)).Append('}');
            }
            json.Append("\n  ]\n}\n");
            File.WriteAllText(Path.ChangeExtension(wavPath, ".json"), json.ToString());
            Debug.Log("T06_AUDIO_CAPTURE frames=" + (data.Length / channelCount) + " marks=" + captured.Count);
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this) instance = null;
        }

        static void WriteWav(string file, float[] data, int channelCount, int sampleRate)
        {
            var directory = Path.GetDirectoryName(file);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            using var stream = new FileStream(file, FileMode.Create, FileAccess.Write);
            using var writer = new BinaryWriter(stream);
            int dataBytes = data.Length * 2;
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataBytes);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channelCount);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channelCount * 2);
            writer.Write((short)(channelCount * 2));
            writer.Write((short)16);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataBytes);
            for (int i = 0; i < data.Length; i++)
                writer.Write((short)Mathf.Clamp(data[i] * 32767f, -32768f, 32767f));
        }
    }
}
