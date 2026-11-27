using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using YunaLegend.DreamForest.Core;
using YunaLegend.DreamForest.Runtime;
using YunaLegend.DreamForestEntrance.Runtime;

namespace YunaLegend.DreamForestEntrance.Editor
{
    /// <summary>Bounded editor observer of the real bootstrap clock; never mutates simulation state.</summary>
    public static class DreamEntranceMotionCapture
    {
        private static DreamForestStageBootstrap bootstrap;
        private static DreamForestSimulation simulation;
        private static DreamEntranceView view;
        private static StreamWriter timeline;
        private static string folder;
        private static double start;
        private static double next;
        private static double finish;
        private static int frame;
        private static bool previousBackground;

        public static void Begin(DreamForestStageBootstrap owner)
        {
            if (timeline!=null) return;
            bootstrap=owner;
            var field=typeof(DreamForestStageBootstrap).GetField("simulation",BindingFlags.NonPublic|BindingFlags.Instance);
            simulation=(DreamForestSimulation)field.GetValue(bootstrap);
            view=bootstrap.GetComponentInChildren<DreamEntranceView>(true);
            folder=Path.GetFullPath("Evidence/DreamForestEntrance/PlayMode");
            Directory.CreateDirectory(folder);
            timeline=new StreamWriter(Path.Combine(folder,"timeline.csv"));
            timeline.WriteLine("frame,elapsed,beat,age,openingTime,stage");
            timeline.AutoFlush=true;
            previousBackground=Application.runInBackground;
            Application.runInBackground=true;
            start=EditorApplication.timeSinceStartup;
            next=start;
            finish=0;
            frame=0;
            EditorApplication.update+=Tick;
        }

        private static void Tick()
        {
            try
            {
                if (!EditorApplication.isPlaying || bootstrap==null)
                {
                    Stop();
                    return;
                }
                double now=EditorApplication.timeSinceStartup;
                if (finish>0)
                {
                    if (now>=finish)
                    {
                        Stop();
                        EditorApplication.isPlaying=false;
                    }
                    return;
                }
                if (now-start>=45 || simulation.Stage!=DreamForestStage.Opening)
                {
                    bool inactive=!view.Visible;
                    bool restored=bootstrap.GetComponentInChildren<DreamForestHudController>(true).gameObject.activeSelf
                        && bootstrap.GetComponentInChildren<DreamForestActorController>(true).gameObject.activeSelf
                        && bootstrap.GetComponentInChildren<DreamForestSceneryController>(true).gameObject.activeSelf;
                    bool passed=simulation.Stage==DreamForestStage.Running && inactive && restored;
                    string result=$"PLAYMODE_ENTRANCE_{(passed ? "OK" : "FAILED")}; frames={frame}; stage={simulation.Stage}; entranceInactive={inactive}; gameplayRestored={restored}; elapsed={now-start:F3}s. Offscreen view, real bootstrap clock.";
                    File.WriteAllText(Path.Combine(folder,"result.txt"),result);
                    Debug.Log(result);
                    finish=now+2;
                    return;
                }
                if (now<next) return;
                next=now+1.0/12;
                DreamEntranceAudit.Render(view,simulation,1200,720,Path.Combine(folder,$"frame-{frame:D4}.png"));
                timeline.WriteLine(FormattableString.Invariant($"{frame},{now-start:F5},{simulation.Opening.Index},{simulation.Opening.Age:F5},{simulation.Opening.Time:F5},{simulation.Stage}"));
                frame++;
            }
            catch(Exception ex)
            {
                File.WriteAllText(Path.Combine(folder,"capture-error.txt"),ex.ToString());
                Stop();
                EditorApplication.isPlaying=false;
                Debug.LogException(ex);
            }
        }

        private static void Stop()
        {
            EditorApplication.update-=Tick;
            timeline?.Dispose();
            timeline=null;
            Application.runInBackground=previousBackground;
            SessionState.SetBool("DreamEntranceAudit.Motion",false);
        }
    }
}
