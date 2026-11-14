using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.Illustrated25D.Preview
{
    /// <summary>Opt-in five-jump route and real reload proof for the ABC stack after the dresser.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class PreviewAbcVerification : MonoBehaviour
    {
        [Serializable] public class Attempt
        {
            public bool bear,books,furniture,dresser,blocks,finished;
            public int jumps,cracks;
            public float maxDresserAnchorGap,maxAbcAnchorGap,landingX,abcLandingX;
        }
        [Serializable] public class Report
        {
            public bool success,resetClean;
            public int hz=60,errors,maxLegacy;
            public float dresserX,dresserTop,dresserWidth,dresserGap;
            public float abcX,abcTop,abcSupportWidth,abcSupportMin,abcSupportMax,abcSupportGap,abcStepUp;
            public float idleVisualMin,idleGapToSupport,landingVisualMin,landingGapToSupport;
            public string landingPose;
            public int abcSortingOrder,abcSortingLayer,idleSortingOrder,idleSortingLayer;
            public string error;
            public List<Attempt> attempts=new List<Attempt>();
        }
        readonly Report report=new Report();
        IllustratedPreview preview;
        Keyboard keyboard;
        string output;
        int tick,releaseAt=-1,finishAt=-1,index,reloadWait;
        bool first,second,third,fourth,fifth;
        bool awaitingReload;
        Attempt attempt;
        DresserFootAlignment dresserFeet,abcFeet;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args=System.Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-previewAbcVerify");
            if(at<0||at+1>=args.Length||FindFirstObjectByType<PreviewAbcVerification>()!=null)return;
            var go=new GameObject("Abc_ReadOnlyVerification");DontDestroyOnLoad(go);
            var test=go.AddComponent<PreviewAbcVerification>();test.output=args[at+1];Directory.CreateDirectory(test.output);
            int hzAt=Array.IndexOf(args,"-previewHz");if(hzAt>=0)test.report.hz=int.Parse(args[hzAt+1]);
            Time.captureDeltaTime=1f/test.report.hz;Application.runInBackground=true;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            test.keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.EnableDevice(test.keyboard);
            Application.logMessageReceived+=test.OnLog;
        }
        void LateUpdate()
        {
            if(awaitingReload && preview!=null)
            { if(++reloadWait>report.hz*5)End("R reload timeout"); return; }
            if(preview==null)
            {
                preview=FindFirstObjectByType<IllustratedPreview>();
                if(preview==null||!preview.Ready){preview=null;return;}
                if(!preview.includeNextBlocks||preview.Blocks==null||preview.Dresser==null){End("ABC scene missing");return;}
                tick=0;finishAt=-1;releaseAt=-1;first=second=third=fourth=fifth=false;
                awaitingReload=false;
                reloadWait=0;
                attempt=new Attempt();report.attempts.Add(attempt);Press();
                report.dresserX=preview.Dresser.transform.position.x;
                report.dresserTop=preview.Dresser.GetComponent<DarkRoomPlatformMarker>().GetTopCenterWorld().y;
                report.dresserWidth=preview.Dresser.Top.size.x;
                var furniture=preview.Furniture.GetComponent<DarkRoomPlatformMarker>();
                report.dresserGap=preview.Dresser.GetComponent<DarkRoomPlatformMarker>().Support.MinX-furniture.Support.MaxX;
                var abc=preview.Blocks.GetComponent<DarkRoomPlatformMarker>();
                report.abcX=preview.Blocks.transform.position.x;
                report.abcTop=abc.GetTopCenterWorld().y;
                report.abcSupportWidth=preview.Blocks.Top.size.x;
                report.abcSupportMin=abc.Support.MinX;report.abcSupportMax=abc.Support.MaxX;
                report.abcSupportGap=abc.Support.MinX-preview.Dresser.GetComponent<DarkRoomPlatformMarker>().Support.MaxX;
                report.abcStepUp=report.abcTop-report.dresserTop;
                report.abcSortingOrder=preview.Blocks.Painting.sortingOrder;
                report.abcSortingLayer=preview.Blocks.Painting.sortingLayerID;
                var idleRenderer=preview.Player.GetComponent<YunaLegend.YunaIllustration.YunaIllustrationVisibilityResolver>().idleRenderer;
                report.idleSortingOrder=idleRenderer.sortingOrder;
                report.idleSortingLayer=idleRenderer.sortingLayerID;
                Debug.Log("ABC_SORTING abc="+report.abcSortingOrder+"/"+report.abcSortingLayer
                    +" idle="+report.idleSortingOrder+"/"+report.idleSortingLayer+" renderer="+idleRenderer.GetType().Name);
                var feet=preview.Player.GetComponents<DresserFootAlignment>();
                dresserFeet=feet.FirstOrDefault(f=>f.supportIndex==3);
                abcFeet=feet.FirstOrDefault(f=>f.supportIndex==4);
                if(dresserFeet==null||abcFeet==null){End("Foot alignment components missing");return;}
                if(index>0) report.resetClean=!preview.Started&&!preview.Furniture.HasCracked&&!preview.Player.IsDead&&!preview.Player.IsStumbling;
            }
            tick++;
            if(tick==releaseAt)Press();
            if(tick==report.hz/2)preview.Begin();
            var p=preview.Player;
            if(preview.Started&&!first&&p.transform.position.x>=-.64f){first=true;Jump();}
            if(p.CurrentSupportIndex==0)attempt.bear=true;
            if(attempt.bear&&!second&&p.transform.position.x>=2.72f){second=true;Jump();}
            if(p.CurrentSupportIndex==1)attempt.books=true;
            if(attempt.books&&!third&&p.IsGrounded&&p.transform.position.x>=5.95f){third=true;Jump();}
            if(p.CurrentSupportIndex==2)attempt.furniture=true;
            var previous=preview.Furniture.GetComponent<DarkRoomPlatformMarker>();
            if(attempt.furniture&&!fourth&&p.IsGrounded&&!p.IsStumbling&&preview.Furniture.HasCracked
                &&p.transform.position.x>=previous.Support.MaxX-.45f){fourth=true;Jump();}
            if(p.CurrentSupportIndex==3&&p.IsGrounded)
            {
                attempt.maxDresserAnchorGap=Mathf.Max(attempt.maxDresserAnchorGap,Mathf.Abs(dresserFeet.AnchorGap));
                if(!attempt.dresser)
                {
                    attempt.dresser=true;attempt.landingX=p.transform.position.x;
                    StartCoroutine(Shot("attempt-"+index+"-dresser-land.png"));
                }
            }
            var dresserMarker=preview.Dresser.GetComponent<DarkRoomPlatformMarker>();
            if(attempt.dresser&&!fifth&&p.IsGrounded&&p.CurrentSupportIndex==3
                &&p.transform.position.x>=dresserMarker.Support.MaxX-.45f){fifth=true;Jump();}
            if(p.CurrentSupportIndex==4&&p.IsGrounded)
            {
                attempt.maxAbcAnchorGap=Mathf.Max(attempt.maxAbcAnchorGap,Mathf.Abs(abcFeet.AnchorGap));
                if(!attempt.blocks)
                {
                    attempt.blocks=true;attempt.abcLandingX=p.transform.position.x;
                    var lr=preview.Player.GetComponent<YunaLegend.YunaIllustration.YunaIllustrationVisibilityResolver>();
                    var lp=lr.VisibleSource;
                    var lrend=lp=="Idle"?lr.idleRenderer:lp=="Jump"?lr.jump.painting:lp=="Stumble"?lr.stumble.painting:lr.run.painting;
                    report.landingPose=lp;
                    report.landingVisualMin=lrend.bounds.min.y;
                    report.landingGapToSupport=report.landingVisualMin-report.abcTop;
                    StartCoroutine(Shot("attempt-"+index+"-abc-land.png"));
                }
            }
            attempt.cracks=preview.Furniture.CrackCount;
            report.maxLegacy=Mathf.Max(report.maxLegacy,preview.LegacyRenderersVisible);
            if(preview.Finished&&finishAt<0){attempt.finished=true;finishAt=tick+report.hz;}
            if(finishAt>0&&tick==finishAt-2)
            {
                var resolver=preview.Player.GetComponent<YunaLegend.YunaIllustration.YunaIllustrationVisibilityResolver>();
                report.idleVisualMin=resolver.idleRenderer.bounds.min.y;
                report.idleGapToSupport=report.idleVisualMin-report.abcTop;
                Debug.Log("ABC_CONTACT support="+report.abcTop.ToString("F4")
                    +" idleMin="+report.idleVisualMin.ToString("F4")+" gap="+report.idleGapToSupport.ToString("F4")
                    +" landing="+report.landingPose+" landMin="+report.landingVisualMin.ToString("F4")+" landGap="+report.landingGapToSupport.ToString("F4"));
                StartCoroutine(Shot("attempt-"+index+"-abc-idle.png"));
            }
            if(finishAt>0&&tick>=finishAt)
            {
                if(index==0){index++;awaitingReload=true;Press(Key.R);return;}
                End(null);return;
            }
            if(p.IsDead){StartCoroutine(Shot("failure.png"));End("Player died x="+p.transform.position.x+" support="+p.CurrentSupportIndex);}
            else if(tick>report.hz*40)End("Timeout x="+p.transform.position.x+" support="+p.CurrentSupportIndex);
        }
        void Jump(){attempt.jumps++;Press(Key.Space);releaseAt=tick+Mathf.RoundToInt(.12f*report.hz);}
        void Press(params Key[] keys)=>InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));
        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name),image.EncodeToPNG());Destroy(image);
        }
        void End(string error)
        {
            report.error=error;
            report.success=error==null&&report.errors==0&&report.resetClean&&report.maxLegacy==0&&report.attempts.Count==2
                &&report.attempts.All(a=>a.bear&&a.books&&a.furniture&&a.dresser&&a.blocks&&a.finished&&a.jumps==5&&a.cracks==1
                    &&a.maxDresserAnchorGap<.005f&&a.maxAbcAnchorGap<.005f);
            File.WriteAllText(Path.Combine(output,"verification.json"),JsonUtility.ToJson(report,true));
            Debug.Log("ABC_VERIFY "+report.success+" "+error);enabled=false;Application.Quit(report.success?0:1);
        }
        void OnLog(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error)report.errors++;}
        void OnDestroy(){Application.logMessageReceived-=OnLog;if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);}
    }
}
