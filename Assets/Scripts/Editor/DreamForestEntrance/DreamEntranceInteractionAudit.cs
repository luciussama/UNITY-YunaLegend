using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YunaLegend.DreamForest.Core;
using YunaLegend.DreamForest.Runtime;
using YunaLegend.DreamForestEntrance.Runtime;

namespace YunaLegend.DreamForestEntrance.Editor
{
    /// <summary>Real Play Mode UI event-path checks for the Art Director's requested corrections.</summary>
    public static class DreamEntranceInteractionAudit
    {
        private static readonly string Folder=Path.GetFullPath("Evidence/DreamForestEntrance/Interactions");
        private static DreamForestStageBootstrap bootstrap;
        private static DreamForestSimulation sim;
        private static DreamEntranceView view;
        private static EntrancePhaseMenuView menu;
        private static Button flag;
        private static Button pause;
        private static EntranceAdvanceSurface surface;
        private static double start;
        private static double next;
        private static float clock;
        private static int state;
        private static int originalBootstrap;
        private static string originalScene;
        private static bool running;
        private static bool previousBackground;

        public static void Begin()
        {
            if(running) return;
            bootstrap=UnityEngine.Object.FindFirstObjectByType<DreamForestStageBootstrap>();
            if(bootstrap==null) return;
            sim=ReadSimulation(bootstrap);
            view=bootstrap.GetComponentInChildren<DreamEntranceView>(true);
            menu=bootstrap.GetComponentInChildren<EntrancePhaseMenuView>(true);
            surface=bootstrap.GetComponentInChildren<EntranceAdvanceSurface>(true);
            foreach(var button in view.GetComponentsInChildren<Button>(true))
            {
                if(button.name=="Phases") flag=button;
                if(button.name=="Pause") pause=button;
            }
            originalScene=SceneManager.GetActiveScene().name;
            originalBootstrap=bootstrap.GetInstanceID();
            Directory.CreateDirectory(Folder);
            previousBackground=Application.runInBackground;
            Application.runInBackground=true;
            if(Application.isBatchMode)
            {
                // Batch has no foreground Game View; supply the same focus event as the focused player.
                bootstrap.SendMessage("OnApplicationFocus",true);
                EditorApplication.QueuePlayerLoopUpdate();
            }
            start=EditorApplication.timeSinceStartup;
            next=start;
            state=0;
            running=true;
            EditorApplication.update+=Tick;
        }

        private static void Tick()
        {
            try
            {
                if(!EditorApplication.isPlaying)
                {
                    Stop();
                    return;
                }
                double now=EditorApplication.timeSinceStartup;
                if(Application.isBatchMode) EditorApplication.QueuePlayerLoopUpdate();
                if(now-start>30) throw new TimeoutException("Entrance interaction audit exceeded 30 seconds.");
                if(now<next) return;
                switch(state)
                {
                    case 0:
                        if(sim.Opening==null || sim.Opening.Index<2) return;
                        Click(flag.gameObject,-1);
                        Require(menu.IsOpen,"Mouse opens local selector");
                        clock=sim.Opening.Time;
                        DreamEntranceAudit.Render(view,sim,1200,720,Path.Combine(Folder,"selector-mouse.png"));
                        state=1;
                        next=now+.4;
                        break;
                    case 1:
                        Require(SceneManager.GetActiveScene().name==originalScene,"Opening selector preserves scene");
                        Require(sim.Opening.Time>clock,"Cinematic clock continues while selector is open");
                        Submit(flag.gameObject);
                        Require(!menu.IsOpen,"Keyboard submit closes selector");
                        Submit(flag.gameObject);
                        Require(menu.IsOpen,"Keyboard submit opens selector");
                        Click(flag.gameObject,1);
                        Require(!menu.IsOpen,"Touch click closes selector");
                        Click(pause.gameObject,-1);
                        clock=sim.Opening.Time;
                        state=2;
                        next=now+.4;
                        break;
                    case 2:
                        Require(Math.Abs(sim.Opening.Time-clock)<.00001,"Pause freezes acting clock");
                        DreamEntranceAudit.Render(view,sim,1200,720,Path.Combine(Folder,"paused-veil.png"),true);
                        ExecuteEvents.Execute(surface.gameObject,Pointer(1),ExecuteEvents.pointerDownHandler);
                        Require(!sim.Opening.Dismissed,"Touch cannot dismiss a paused narration");
                        Submit(pause.gameObject);
                        clock=sim.Opening.Time;
                        state=3;
                        next=now+.4;
                        break;
                    case 3:
                        Require(sim.Opening.Time>clock,"Keyboard resume restores clock");
                        int beat=sim.Opening.Index;
                        ExecuteEvents.Execute(surface.gameObject,Pointer(1),ExecuteEvents.pointerDownHandler);
                        Require(sim.Opening.Dismissed && sim.Opening.Index==beat,"One touch dismisses only the visible narration");
                        Click(flag.gameObject,1);
                        Require(menu.IsOpen,"Touch opens selector");
                        menu.GetComponentInChildren<Dropdown>(true).value=6;
                        state=4;
                        next=now+.4;
                        break;
                    case 4:
                        var replacement=UnityEngine.Object.FindFirstObjectByType<DreamForestStageBootstrap>();
                        Require(replacement!=null && replacement.GetInstanceID()!=originalBootstrap,"Scene changes only after dropdown selection");
                        Require(SceneManager.GetActiveScene().name==originalScene,"Dream Forest selection restarts Dream Forest");
                        Require(ReadSimulation(replacement).Stage==DreamForestStage.Opening,"Selected phase restarts at entrance");
                        File.WriteAllText(Path.Combine(Folder,"result.txt"),"ENTRANCE_INTERACTIONS_OK: mouse/touch pointer paths and keyboard Submit; selector open/close preserves scene and clock; paused clock/narration frozen; resume works; one-touch narration; scene reload only after dropdown selection. Native UI events in PlayMode, not physical hardware injection.");
                        Stop();
                        EditorApplication.isPlaying=false;
                        if (Application.isBatchMode) EditorApplication.delayCall+=()=>EditorApplication.Exit(0);
                        break;
                }
            }
            catch(Exception ex)
            {
                File.WriteAllText(Path.Combine(Folder,"result.txt"),"ENTRANCE_INTERACTIONS_FAILED\n"+ex);
                Stop();
                EditorApplication.isPlaying=false;
                Debug.LogException(ex);
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        private static DreamForestSimulation ReadSimulation(DreamForestStageBootstrap owner)
        {
            return (DreamForestSimulation)typeof(DreamForestStageBootstrap).GetField("simulation",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(owner);
        }

        private static PointerEventData Pointer(int id)
        {
            return new PointerEventData(EventSystem.current) { pointerId=id,button=PointerEventData.InputButton.Left };
        }

        private static void Click(GameObject target,int id)
        {
            var data=Pointer(id);
            ExecuteEvents.Execute(target,data,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target,data,ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(target,data,ExecuteEvents.pointerClickHandler);
        }

        private static void Submit(GameObject target)
        {
            ExecuteEvents.Execute(target,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);
        }

        private static void Require(bool condition,string assertion)
        {
            if(!condition) throw new InvalidOperationException(assertion);
        }

        private static void Stop()
        {
            running=false;
            EditorApplication.update-=Tick;
            Application.runInBackground=previousBackground;
            SessionState.SetBool("DreamEntranceAudit.Interactions",false);
        }
    }
}
