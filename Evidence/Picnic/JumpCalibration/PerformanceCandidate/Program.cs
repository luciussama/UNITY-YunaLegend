using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using YunaLegend.Picnic.Core;
CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;

var stage=new PicnicStageSpec();
string output=Path.GetDirectoryName(typeof(PicnicSimulation).Assembly.Location);
string target=Path.GetFullPath("Evidence/Picnic/JumpCalibration/PerformanceCandidate");
var csv=new StringBuilder("index,id,terrain,ground,wheel,phase_rad,slope,hill,waves,time_s,left_m,right_m,center_m,width_m,left_height_m,center_height_m,right_height_m,orbit_left_m,orbit_right_m,min_center_height_m,max_center_height_m\n");
foreach(var m in PicnicSupportMeasurement.Measure(stage,0f))
{
 var s=stage.Supports[m.Index];csv.AppendLine($"{m.Index},{m.Id},{s.Terrain},{s.Ground},{s.Wheel},{s.Angle:R},{s.Slope:R},{s.Hill:R},{s.Waves:R},{m.Time:R},{m.Left:R},{m.Right:R},{m.Center:R},{m.Width:R},{m.LeftHeight:R},{m.CenterHeight:R},{m.RightHeight:R},{m.OrbitLeft:R},{m.OrbitRight:R},{m.MinimumCenterHeight:R},{m.MaximumCenterHeight:R}");
}
File.WriteAllText(Path.Combine(target,"support-measurements.csv"),csv.ToString());
var edges=PicnicSupportMeasurement.Measure(stage,0f).OrderBy(m=>m.Center).ToArray();
var gaps=new StringBuilder("from_index,from_id,to_index,to_id,time_s,from_right_m,to_left_m,signed_gap_m,height_delta_m,moving_endpoint\n");
for(int i=0;i<edges.Length-1;i++)
{
 var a=edges[i];var b=edges[i+1];bool moving=stage.Supports[a.Index].Wheel>=0||stage.Supports[b.Index].Wheel>=0;
 gaps.AppendLine($"{a.Index},{a.Id},{b.Index},{b.Id},0,{a.Right:R},{b.Left:R},{a.GapTo(b):R},{b.LeftHeight-a.RightHeight:R},{moving}");
}
File.WriteAllText(Path.Combine(target,"adjacent-edge-gaps-at-t0.csv"),gaps.ToString());
if(args.Contains("--measure-only")){Console.WriteLine($"Measured {stage.Supports.Count} supports and {edges.Length-1} spatial neighbours; moving gaps are snapshots, not fixed jumps.");return;}
foreach(int hz in (args.Contains("--one-rate") ? new[]{30} : new[]{30,60,120,0}))
{
 var s=new PicnicSimulation();s.SkipNarrative();var events=new List<object>();float max=s.X;float last=0;var timer=Stopwatch.StartNew();long calls=0;double maxCall=0;long allocatedBefore=GC.GetAllocatedBytesForCurrentThread();ulong decisionHash=14695981039346656037UL;
 var jumps=new List<object>();int launch=-1;float launchX=0,launchY=0,launchTime=0;
 s.Event+=e=>{if(e.StartsWith("panic_")||e=="hit"||e=="defeat")events.Add(new{e,s.X,s.Y,s.Time,s.Hearts});};
 for(int f=0;f<(hz==0?60000:hz*900)&&s.Phase==PicnicPhase.Playing;f++)
 {
  var clock=Stopwatch.StartNew();var input=PicnicQaPilot.Read(s);clock.Stop();calls++;maxCall=Math.Max(maxCall,clock.Elapsed.TotalMilliseconds);
  if(input.Jump&&s.Grounded){launch=s.SupportIndex;launchX=s.X;launchY=s.Y;launchTime=s.Time;}
  decisionHash=(decisionHash ^ (ulong)((int)(input.Axis+1f)*2+(input.Jump?1:0)))*1099511628211UL;
  s.Step(hz==0?new[]{1f/24f,1f/40f,1f/57f,1f/120f}[f%4]:1f/hz,input.Axis,input.Jump);
  if(launch>=0&&s.Grounded){jumps.Add(new{from=launch,to=s.SupportIndex,takeoffX=launchX,takeoffY=launchY,takeoffTime=launchTime,landingX=s.X,landingY=s.Y,landingTime=s.Time,gap=stage.Supports[s.SupportIndex].Left(s.Time)-stage.Supports[launch].Right(launchTime)});launch=-1;}
  if(s.X>max+.1f){max=s.X;last=s.Time;}if(s.Time-last>12f)break;
 }
 var reachedEnding=s.Phase==PicnicPhase.Ending; if(reachedEnding)s.Step(PicnicNarrative.Ending.Duration+0.1f);
 Console.WriteLine(JsonSerializer.Serialize(new{hz,reachedEnding,s.Phase,s.X,s.Y,s.Time,s.SupportIndex,s.Hearts,s.Falls,s.Jumps,events,wallSeconds=timer.Elapsed.TotalSeconds,pilotCalls=calls,maxPilotMs=maxCall,allocatedBytes=GC.GetAllocatedBytesForCurrentThread()-allocatedBefore,decisionHash=decisionHash.ToString("X16")}));
 File.WriteAllText(Path.Combine(target,$"jump-points-{hz}.json"),JsonSerializer.Serialize(jumps,new JsonSerializerOptions{WriteIndented=true}));
}
