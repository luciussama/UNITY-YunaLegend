using System;
using UnityEngine;
using YunaLegend.Qa.Core;

namespace YunaLegend.Qa.Runtime
{
    /// <summary>
    /// Adaptador fino de runtime para o Modo QA opt-in.
    /// Le Application.absoluteURL (e YUNA_QA_URL em editor/testes) e emite eventos
    /// estruturados no console apenas quando o modo QA foi ativado explicitamente.
    /// </summary>
    public static class WebQaRuntime
    {
        private static bool initialized;
        private static WebQaOptions cachedOptions = WebQaOptions.Disabled;

        public static WebQaOptions Current
        {
            get
            {
                if (!initialized)
                {
                    Refresh();
                }

                return cachedOptions;
            }
        }

        public static void Refresh()
        {
            string url = Application.absoluteURL;
            if (string.IsNullOrWhiteSpace(url))
            {
                url = Environment.GetEnvironmentVariable("YUNA_QA_URL");
            }

            cachedOptions = WebQaOptions.ParseFromUrl(url);
            initialized = true;

            if (cachedOptions.Enabled)
            {
                Debug.Log(
                    $"[QA_EVENT] qa_enabled autoplay={cachedOptions.AutoPlay} " +
                    $"arena_final={cachedOptions.ArenaFinalWave} scene={cachedOptions.StartScene}");
            }
        }

        public static void LogEvent(string eventName, string details = "")
        {
            if (!Current.Enabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(details))
            {
                Debug.Log($"[QA_EVENT] {eventName}");
                return;
            }

            Debug.Log($"[QA_EVENT] {eventName} {details}");
        }
    }
}
