using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace YunaLegend.EditorScripts
{
    public static class WebPreviewBuild
    {
        [MenuItem("YunaLegend/Build Web de avaliacao")]
        public static void Build()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new InvalidOperationException("Instale WebGL Build Support para esta versao do Unity pelo Hub.");

            string output = Environment.GetEnvironmentVariable("YUNA_WEB_OUTPUT");
            if (string.IsNullOrWhiteSpace(output)) output = "Build/WebPreview";
            Directory.CreateDirectory(output);
            var previousCompression = PlayerSettings.WebGL.compressionFormat;
            var previousBackground = PlayerSettings.runInBackground;
            try
            {
                // Local preview favors a simple HTTP server, without compression-header dependencies.
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                PlayerSettings.runInBackground = true;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { "Assets/Scenes/DarkRoom_Gameplay.unity" },
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.Development
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException($"Build Web falhou: {report.summary.result}, {report.summary.totalErrors} erros.");

                // For automated browser preview, keep the drawing buffer available so snapshots do not read discarded WebGL buffers.
                PatchHtmlPreserveDrawingBuffer(Path.Combine(output, "index.html"));

                Debug.Log($"YUNA_WEB_BUILD_OK: {Path.GetFullPath(output)}");
            }
            finally
            {
                PlayerSettings.WebGL.compressionFormat = previousCompression;
                PlayerSettings.runInBackground = previousBackground;
            }
        }

        private static void PatchHtmlPreserveDrawingBuffer(string htmlPath)
        {
            if (!File.Exists(htmlPath)) return;
            string text = File.ReadAllText(htmlPath);
            const string marker = "var canvas = document.querySelector(\"#unity-canvas\");";
            const string patch =
                "(function() {\n" +
                "  const orig = HTMLCanvasElement.prototype.getContext;\n" +
                "  HTMLCanvasElement.prototype.getContext = function(t, a) {\n" +
                "    if (t === 'webgl' || t === 'webgl2') a = Object.assign({}, a, { preserveDrawingBuffer: true });\n" +
                "    return orig.call(this, t, a);\n" +
                "  };\n" +
                "})();\n" +
                "var canvas = document.querySelector(\"#unity-canvas\");";
            if (!text.Contains("preserveDrawingBuffer: true") && text.Contains(marker))
            {
                text = text.Replace(marker, patch);
                File.WriteAllText(htmlPath, text);
            }
        }
    }
}
