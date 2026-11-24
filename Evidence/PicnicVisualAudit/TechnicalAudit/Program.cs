using System.Reflection;
using System.Text.Json;
using YunaLegend.Picnic.Core;
class Program
{
    static void Set(object o,string p,object value) => o.GetType().GetProperty(p).SetValue(o,value);
    static object State(PicnicSimulation s) => new {s.Phase,s.X,s.Y,s.Time,s.SupportIndex,s.Terrain,s.Hearts,s.Jumps,s.Falls};
    static PicnicSimulation Seed(int index, float x, float time=0f, bool easy=false)
    {
        var s=new PicnicSimulation(easy);s.SkipNarrative();Set(s,"Time",time);Set(s,"X",x);Set(s,"Y",s.Stage.Supports[index].Height(time,x));Set(s,"SupportIndex",index);return s;
    }
    static void Main()
    {
        var report=new Dictionary<string,object>();
        report["method"]="Unmodified C# Core compiled as net8 console; reflection used ONLY for synthetic segment/checkpoint setup, never continuous run. No Unity rendering/physics validated.";
        var continuous=new List<object>();
        foreach(int hz in new[]{30,60,120})
        {
            var s=new PicnicSimulation();s.SkipNarrative();
            for(int frame=0;frame<hz*900 && s.Phase==PicnicPhase.Playing;frame++){var input=PicnicQaPilot.Read(s);s.Step(1f/hz,input.Axis,input.Jump);}
            continuous.Add(new{hz,state=State(s),input=PicnicQaPilot.Read(s),support=s.Grounded?s.Stage.Supports[s.SupportIndex].Id:"air"});
        }
        report["continuous900Seconds"]=continuous;
        var stuck=new List<object>();
        var spec=new PicnicStageSpec();
        for(int i=0;i<spec.Supports.Count;i++)
        {
            var support=spec.Supports[i];if(support.Wheel>=0)continue;
            var s=Seed(i,support.Center(0));float max=s.X;
            for(int f=0;f<60*20 && s.Phase==PicnicPhase.Playing;f++){var input=PicnicQaPilot.Read(s);s.Step(1f/60,input.Axis,input.Jump);max=Math.Max(max,s.X);}
            if(max-support.Center(0)<support.Width*.5f+0.4f || s.Phase==PicnicPhase.Defeat)stuck.Add(new{index=i,id=support.Id,start=support.Center(0),progress=max-support.Center(0),state=State(s)});
        }
        report["synthetic20SecondSegmentsProblematic"]=stuck;
        var retries=new List<object>();
        for(int i=0;i<spec.Sections.Count;i++){var s=new PicnicSimulation();Set(s,"Section",i);Set(s,"Phase",PicnicPhase.Defeat);s.Retry();retries.Add(new{index=i,name=spec.Sections[i].Name,state=State(s),support=s.Grounded?spec.Supports[s.SupportIndex].Id:"air"});}
        report["syntheticCheckpointRetries"]=retries;
        var jumps=new List<object>();
        foreach(int hz in new[]{30,60,120})
        {
            var s=Seed(7,22.8f);float start=s.X;int landed=-1;
            for(int f=0;f<hz*2 && s.Phase==PicnicPhase.Playing;f++){s.Step(1f/hz,1f,f==0);if(f>1 && s.Grounded && s.SupportIndex!=7){landed=s.SupportIndex;break;}}
            jumps.Add(new{hz,start,landed,state=State(s)});
        }
        report["syntheticGrassToFirstStonePlayerJump"]=jumps;
        var panicRuns=new List<object>();
        foreach(int index in new[]{29,52,110})
        {
            var s=Seed(index,spec.Supports[index].Center(0));var events=new List<object>();
            s.Event+=e=>{if(e.StartsWith("panic_")||e=="ending_started")events.Add(new{e,s.X,s.Y,s.Time,s.SupportIndex,s.Panic,s.PanicIndex});};
            for(int f=0;f<60*20 && s.Phase==PicnicPhase.Playing;f++){var input=PicnicQaPilot.Read(s);s.Step(1f/60,input.Axis,input.Jump);}
            panicRuns.Add(new{seed=index,events,state=State(s)});
        }
        report["syntheticPanicEventRuns"]=panicRuns;
        var clouds=new List<object>();
        foreach(bool easy in new[]{false,true})
        {
            var s=Seed(19,spec.Supports[19].Center(0),easy:easy);s.CloudTimers[19]=0f;
            int last=-1;var transitions=new List<object>();
            for(int f=0;f<60*9;f++){s.Step(1f/60);int active=s.IsSupportActive(19)?1:0;if(last!=active){transitions.Add(new{elapsed=(f+1)/60f,active,age=s.CloudTimers[19]});last=active;}}
            clouds.Add(new{easy,transitions,state=State(s)});
        }
        report["syntheticCloudLifecycle"]=clouds;
        var riverAnts=new List<object>();
        foreach(var c in spec.Creatures.Where(c=>c.Kind==PicnicCreatureKind.Ant && c.X>200 && c.X<440 && (Math.Abs(c.Y-.74f)<.01f||Math.Abs(c.Y-.82f)<.01f||Math.Abs(c.Y-.86f)<.01f)))
        {
            var state=new PicnicCreatureState(c);float minY=100,maxY=-100,minX=10000,maxX=-10000;
            for(int f=0;f<60*10;f++){state.Step(1f/60,c.X);minY=Math.Min(minY,state.Y);maxY=Math.Max(maxY,state.Y);minX=Math.Min(minX,state.X);maxX=Math.Max(maxX,state.X);}
            riverAnts.Add(new{homeX=c.X,c.Y,minY,maxY,minX,maxX});
        }
        report["riverAntTenSeconds"]=riverAnts;
        var localJumps=new List<object>();
        foreach(int index in new[]{7,45,63,66,70})
        {
            var support=spec.Supports[index];float start=support.Right(0)-.31f;var s=Seed(index,start);int landed=-1;
            for(int f=0;f<120*2 && s.Phase==PicnicPhase.Playing;f++){s.Step(1f/120,1f,f==0);if(f>1&&s.Grounded&&s.SupportIndex!=index){landed=s.SupportIndex;break;}}
            localJumps.Add(new{sourceIndex=index,source=support.Id,start,landed,state=State(s)});
        }
        report["syntheticPlayerJumpsAtQaStalls"]=localJumps;
        var remoteClouds=new List<object>();
        foreach(bool easy in new[]{false,true})
        {
            var s=new PicnicSimulation(easy);s.SkipNarrative();s.CloudTimers[19]=0f;int last=-1;var transitions=new List<object>();
            for(int f=0;f<60*9;f++){s.Step(1f/60);int active=s.IsSupportActive(19)?1:0;if(last!=active){transitions.Add(new{elapsed=(f+1)/60f,active,age=s.CloudTimers[19]});last=active;}}
            remoteClouds.Add(new{easy,transitions,state=State(s)});
        }
        report["syntheticRemoteCloudLifecycleNoRescue"]=remoteClouds;
        Console.WriteLine(JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
    }
}


