using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YunaLegend.Illustrated25D.Preview.Editor;

namespace YunaLegend.Illustrated25D.Environment.Editor
{
    public static class Playable0010BatchRunner
    {
        public static void BuildMacVerifyPlayer()
        {
            IllustratedPreviewBuild.SetupPlayableStage2P10();
            var output = "/tmp/YunaPlayable0010Verify.app";
            if (Directory.Exists(output)) Directory.Delete(output, true);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { IllustratedPreviewBuild.Playable0010Scene },
                locationPathName = output,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            });
            Debug.Log($"MAC_VERIFY_BUILD result={report.summary.result} errors={report.summary.totalErrors}");
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
