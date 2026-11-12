using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace YunaLegend.EditorScripts
{
    /// <summary>
    /// Build WebGL pronta para GitHub Pages (e para navegador mobile).
    /// GitHub Pages nao permite cabecalhos customizados: compressao gzip + fallback de
    /// descompressao no JS, threads desligadas (sem SharedArrayBuffer/COOP+COEP), memoria
    /// inicial/maxima calibrada para celular e cena de entrada primeiro.
    /// </summary>
    public static class WebCiBuild
    {
        private const string TitleScene = "Assets/Scenes/TitleScreen.unity";
        private const string GameplayScene = "Assets/Scenes/DarkRoom_Gameplay.unity";

        [MenuItem("YunaLegend/Build Web para GitHub Pages")]
        public static void Build()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new InvalidOperationException("Instale WebGL Build Support pelo Unity Hub.");

            string output = Environment.GetEnvironmentVariable("YUNA_WEB_OUTPUT");
            if (string.IsNullOrWhiteSpace(output)) output = "WebGL";

            int initialMb = ReadInt("YUNA_WEB_INITIAL_MB", 256);
            int maxMb = ReadInt("YUNA_WEB_MAX_MB", 1024);

            var previousCompression = PlayerSettings.WebGL.compressionFormat;
            var previousFallback = PlayerSettings.WebGL.decompressionFallback;
            var previousThreads = PlayerSettings.WebGL.threadsSupport;
            var previousException = PlayerSettings.WebGL.exceptionSupport;
            var previousBackground = PlayerSettings.runInBackground;
            var previousTemplate = PlayerSettings.WebGL.template;

            try
            {
                // Compressao gzip + fallback: funciona em Pages sem configurar cabecalhos.
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = true;
                // Sem threads: evita exigir COOP/COEP (Pages nao envia esses cabecalhos).
                PlayerSettings.WebGL.threadsSupport = false;
                PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
                PlayerSettings.runInBackground = true;
                PlayerSettings.WebGL.template = "APPLICATION:Default";
                ApplyMemory(initialMb, maxMb);

                Directory.CreateDirectory(output);
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { TitleScene, GameplayScene },
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                });

                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Build WebGL falhou: {report.summary.result}, {report.summary.totalErrors} erros.");

                PatchMobileIndex(Path.Combine(output, "index.html"));
                Debug.Log($"YUNA_WEB_PAGES_OK: {Path.GetFullPath(output)}");
            }
            finally
            {
                PlayerSettings.WebGL.compressionFormat = previousCompression;
                PlayerSettings.WebGL.decompressionFallback = previousFallback;
                PlayerSettings.WebGL.threadsSupport = previousThreads;
                PlayerSettings.WebGL.exceptionSupport = previousException;
                PlayerSettings.runInBackground = previousBackground;
                PlayerSettings.WebGL.template = previousTemplate;
                ApplyMemory(32, 2048);
            }
        }

        private static int ReadInt(string name, int fallback)
        {
            string raw = Environment.GetEnvironmentVariable(name);
            return int.TryParse(raw, out int value) && value > 0 ? value : fallback;
        }

        private static void ApplyMemory(int initialMb, int maxMb)
        {
            // Propriedades disponiveis em Unity 6; se ausentes em outra versao, o build
            // continua com os valores do ProjectSettings.
            var type = typeof(PlayerSettings.WebGL);
            var initial = type.GetProperty("initialMemorySize");
            var maximum = type.GetProperty("maximumMemorySize");
            if (maximum != null && maximum.CanWrite) maximum.SetValue(null, maxMb);
            if (initial != null && initial.CanWrite) initial.SetValue(null, Mathf.Min(initialMb, maxMb));
        }

        /// <summary>
        /// Reforca o HTML para celular: viewport com safe-area, tela cheia, fundo escuro,
        /// bloqueio de selecao/zoom e aviso de girar o aparelho em retrato.
        /// </summary>
        private static void PatchMobileIndex(string indexPath)
        {
            if (!File.Exists(indexPath)) return;
            string html = File.ReadAllText(indexPath);

            const string viewport =
                "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no, viewport-fit=cover\">";
            if (html.Contains("name=\"viewport\""))
            {
                html = System.Text.RegularExpressions.Regex.Replace(
                    html, "<meta name=\"viewport\"[^>]*>", viewport);
            }
            else if (html.Contains("</head>"))
            {
                html = html.Replace("</head>", "  " + viewport + "\n</head>");
            }

            const string mobileStyle = @"
<style id=""yuna-mobile"">
  html, body { margin: 0; padding: 0; height: 100%; background: #0b0705; overflow: hidden; }
  body { -webkit-user-select: none; user-select: none; -webkit-tap-highlight-color: transparent; }
  #unity-container { position: fixed !important; inset: 0 !important; transform: none !important; }
  #unity-container, #unity-canvas { width: 100vw !important; height: 100vh !important; height: 100dvh !important; }
  #unity-canvas { touch-action: none; display: block; }
  #yuna-rotate { position: fixed; inset: 0; display: none; align-items: center; justify-content: center;
    background: #0b0705; color: #f2e6c8; font-family: Georgia, serif; font-size: 18px; text-align: center;
    z-index: 50; padding: 24px; }
  #unity-footer { display: none; }
</style>
";
            if (!html.Contains("yuna-mobile") && html.Contains("</head>"))
                html = html.Replace("</head>", mobileStyle + "</head>");

            const string rotateOverlay = "<div id=\"yuna-rotate\">Gire o aparelho para a horizontal<br/>para jogar O Quarto dos Brinquedos.</div>";
            if (!html.Contains("yuna-rotate") && html.Contains("<body"))
            {
                int bodyEnd = html.IndexOf('>', html.IndexOf("<body", StringComparison.Ordinal));
                if (bodyEnd > 0) html = html.Insert(bodyEnd + 1, "\n  " + rotateOverlay);
            }

            File.WriteAllText(indexPath, html);
        }
    }
}
