using System;

namespace YunaLegend.Qa.Core
{
    /// <summary>
    /// Configuracao imutavel e estritamente opt-in do Modo QA WebGL,
    /// lida exclusivamente da query string da URL (ex.: ?qa=1&qa_autoplay=1&qa_arena_final=1).
    /// Quando nenhum parametro QA esta presente, permanece completamente inerte.
    /// </summary>
    public readonly struct WebQaOptions
    {
        public static readonly WebQaOptions Disabled = new WebQaOptions(
            enabled: false,
            autoPlay: false,
            arenaFinalWave: false,
            startScene: string.Empty);

        public bool Enabled { get; }
        public bool AutoPlay { get; }
        public bool ArenaFinalWave { get; }
        public string StartScene { get; }

        public WebQaOptions(bool enabled, bool autoPlay, bool arenaFinalWave, string startScene)
        {
            Enabled = enabled;
            AutoPlay = enabled && autoPlay;
            ArenaFinalWave = enabled && arenaFinalWave;
            StartScene = enabled && !string.IsNullOrWhiteSpace(startScene)
                ? startScene.Trim()
                : string.Empty;
        }

        public static WebQaOptions ParseFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Disabled;
            }

            int queryIndex = url.IndexOf('?');
            if (queryIndex < 0 || queryIndex >= url.Length - 1)
            {
                return Disabled;
            }

            string query = url.Substring(queryIndex + 1);
            int hashIndex = query.IndexOf('#');
            if (hashIndex >= 0)
            {
                query = query.Substring(0, hashIndex);
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return Disabled;
            }

            bool qaFlag = false;
            bool autoPlay = false;
            bool arenaFinalWave = false;
            string startScene = string.Empty;

            string[] pairs = query.Split('&');
            for (int i = 0; i < pairs.Length; i++)
            {
                string pair = pairs[i];
                if (string.IsNullOrWhiteSpace(pair))
                {
                    continue;
                }

                int eqIndex = pair.IndexOf('=');
                string rawKey = eqIndex >= 0 ? pair.Substring(0, eqIndex) : pair;
                string rawValue = eqIndex >= 0 ? pair.Substring(eqIndex + 1) : "1";

                string key = Uri.UnescapeDataString(rawKey).Trim().ToLowerInvariant();
                string value = Uri.UnescapeDataString(rawValue).Trim();

                switch (key)
                {
                    case "qa":
                        qaFlag = IsTruthy(value);
                        break;

                    case "qa_autoplay":
                        autoPlay = IsTruthy(value);
                        break;

                    case "qa_arena_final":
                        arenaFinalWave = IsTruthy(value);
                        break;

                    case "qa_scene":
                        startScene = value;
                        break;
                }
            }

            bool enabled = qaFlag || autoPlay || arenaFinalWave || !string.IsNullOrWhiteSpace(startScene);
            if (!enabled)
            {
                return Disabled;
            }

            return new WebQaOptions(
                enabled: true,
                autoPlay: autoPlay,
                arenaFinalWave: arenaFinalWave,
                startScene: startScene);
        }

        private static bool IsTruthy(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "on", StringComparison.OrdinalIgnoreCase);
        }
    }
}
