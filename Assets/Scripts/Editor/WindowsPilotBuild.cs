using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace YunaLegend.EditorScripts
{
    /// <summary>Windows validation build of the pilot (menu + gameplay), no asset changes.</summary>
    public static class WindowsPilotBuild
    {
        const string Title = "Assets/Scenes/TitleScreen.unity";
        const string Gameplay = "Assets/Scenes/DarkRoom_Gameplay.unity";

        public static void Run()
        {
            var root = Directory.GetCurrentDirectory();
            var output = Path.Combine(root, "Build", "Windows", "YunaLegend-Pilot", "YunaLegend.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new InvalidOperationException("StandaloneWindows64 build module is not installed.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { Title, Gameplay },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            var summary = report.summary;
            Debug.Log("PILOT_BUILD_RESULT " + summary.result + " size=" + summary.totalSize
                + " errors=" + summary.totalErrors + " output=" + output);
            if (summary.result != BuildResult.Succeeded)
                foreach (var step in report.steps)
                    foreach (var message in step.messages)
                        if (message.type == LogType.Error || message.type == LogType.Exception)
                            Debug.LogError(step.name + ": " + message.content);
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
