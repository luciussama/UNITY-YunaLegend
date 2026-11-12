using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Camera25D;

namespace YunaLegend.EditorScripts
{
    [InitializeOnLoad]
    public static class DarkRoomFurnitureCapture
    {
        static string Output => Argument("-yunaFurnitureOutput","Evidence/furniture02-capture");
        static string Frames => Argument("-yunaFurnitureFrames","Build/FurnitureCaptureFrames");
        static string Argument(string name,string fallback)
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,name);return i>=0&&i+1<args.Length?args[i+1]:fallback;
        }
        static Yuna25DPlayerController player;
        static YieldingWoodFurniture furniture;
        static DarkRoomPlatformMarker marker;
        static SideScroll25DCamera side;
        static Camera camera;
        static double readyTime;
        static int frame,lastFrame=-1,exitFrame=-1;
        static bool initialized,jumpedIn,jumpedOut;
        static readonly List<Sample> samples=new List<Sample>();
        static readonly HashSet<string> saved=new HashSet<string>();
        [Serializable] class Report { public string method;public Sample[] samples; }
        [Serializable] class Sample
        {
            public int frame,support,cracks;public float gameTime,sag,delay,poseAngle,timeScale;
            public Vector3 player,camera;public bool cracked,stumbling;public string file;
        }
        static DarkRoomFurnitureCapture()
        {
            EditorApplication.playModeStateChanged+=state=> {
                if(!SessionState.GetBool("FurnitureCaptureRun",false))return;
                if(state==PlayModeStateChange.EnteredPlayMode){readyTime=EditorApplication.timeSinceStartup+.2;EditorApplication.update+=Tick;}
                if(state==PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool("FurnitureCaptureRun",false);Time.captureDeltaTime=0;EditorApplication.Exit(0);
                }
            };
        }
        public static void Run()
        {
            Directory.CreateDirectory(Output);Directory.CreateDirectory(Frames);
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            var cabinet=UnityEngine.Object.FindFirstObjectByType<YieldingWoodFurniture>();
            camera=Camera.main;var p=cabinet.transform.position;
            var key=new GameObject("FurnitureInspectionLight").AddComponent<Light>();key.type=LightType.Point;key.range=15;key.intensity=5;key.transform.position=p+new Vector3(-2,3,-4);
            var fill=new GameObject("FurnitureInspectionFill").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.4f;fill.color=new Color(.8f,.85f,1f);fill.transform.rotation=Quaternion.Euler(30,-30,0);
            camera.aspect=16f/9f;camera.fieldOfView=45;
            camera.transform.position=p+new Vector3(0,1.5f,-8.2f);camera.transform.LookAt(p+Vector3.up*.1f);Save("07-front-inspection");
            camera.transform.position=p+new Vector3(5.3f,2.7f,-8.6f);camera.transform.LookAt(p+Vector3.up*.2f);Save("08-three-quarter-inspection");
            cabinet.Preview(.075f,true);Save("09-broken-preview-inspection");
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            SessionState.SetBool("FurnitureCaptureRun",true);EditorApplication.EnterPlaymode();
        }
        static void QueueJump()
        {
            typeof(Yuna25DPlayerController).GetField("jumpBufferCounter",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(player,.14f);
        }
        static void Tick()
        {
            if(!EditorApplication.isPlaying||(!initialized&&EditorApplication.timeSinceStartup<readyTime)||Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;
            try
            {
                if(!initialized)
                {
                    player=UnityEngine.Object.FindFirstObjectByType<Yuna25DPlayerController>();
                    furniture=UnityEngine.Object.FindFirstObjectByType<YieldingWoodFurniture>();marker=furniture.GetComponent<DarkRoomPlatformMarker>();
                    var opening=UnityEngine.Object.FindFirstObjectByType<DarkRoomOpeningSequence>();opening.StopAllCoroutines();opening.enabled=false;
                    var manager=UnityEngine.Object.FindFirstObjectByType<DarkRoomProgressionManager>();manager.BindPlayer(null);manager.enabled=false;
                    var guide=UnityEngine.Object.FindFirstObjectByType<NandaFairyGuide>();guide.SetTargetPlatform(2);
                    player.OnPlatformLanded+=index=>guide.SetTargetPlatform(index+1);
                    var book=UnityEngine.Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(m=>m.spec.index==1);
                    player.ResetPlayer(book.GetTakeoffPoint(1f,.35f)+Vector3.up*.04f);player.RestoreRouteSupport(1);
                    player.AutoRunActive=true;player.SetRunSpeed(0);player.SetJumpHeight(2.4f);player.SetFloorDeathActive(false);
                    furniture.ResetForRetry();camera=Camera.main;camera.aspect=16f/9f;side=camera.GetComponent<SideScroll25DCamera>();
                    side.ScreenCrushActive=false;side.SetScrollState(true,1f,0);side.SnapToRouteFraming(2);
                    Time.timeScale=1;Time.captureDeltaTime=1f/60f;initialized=true;return;
                }
                frame++;
                if(frame==20){Save("00-before-entry");player.SetRunSpeed(3.6f);side.SetScrollState(true,1f,3.6f);QueueJump();jumpedIn=true;}
                if(jumpedIn&&player.CurrentSupportIndex==2)
                {
                    Key("01-on-furniture");
                    float progress=(player.transform.position.x-(marker.transform.position.x-marker.spec.width*.5f))/marker.spec.width;
                    if(progress>.42f&&!furniture.HasCracked)Key("02-before-crack");
                    if(furniture.HasCracked)Key("03-crack-pause");
                    var body=player.transform.Find("Body_Visual");
                    var view=body.GetComponent<YunaCharacterView>();
                    float lean=view!=null?view.PoseLeanDegrees:Mathf.DeltaAngle(0,body.localEulerAngles.x);
                    if(player.StumbleDelayRemaining<=0&&player.IsStumbling&&Mathf.Abs(lean)>8)Key("04-stumble");
                    if(!jumpedOut&&player.transform.position.x>=marker.GetTakeoffPoint(1f,.35f).x){QueueJump();jumpedOut=true;}
                }
                if(player.CurrentSupportIndex>=3)
                {
                    Key("05-normal-exit");if(exitFrame<0)exitFrame=frame;player.AutoRunActive=false;
                }
                if(frame%3==0&&frame>=18)RecordFrame();
                if(player.IsDead)throw new Exception("Furniture evidence traversal died before exit.");
                if(exitFrame>=0&&frame-exitFrame>8)
                {
                    File.WriteAllText(Path.Combine(Output,"live-traversal.json"),JsonUtility.ToJson(new Report {
                        method="Controlled live Play Mode traversal at captureDeltaTime1/60. Real player/furniture/guide/camera updates; injected jump buffer at book edge and furniture exit. Progression/UI disabled. Inspection07-09 use temporary lights; live frames use scene lights only. No scene saved.",samples=samples.ToArray()},true));
                    Debug.Log("FURNITURE_CAPTURE_COMPLETE frames="+samples.Count+" cracks="+furniture.CrackCount+" exit="+player.CurrentSupportIndex);
                    EditorApplication.update-=Tick;EditorApplication.ExitPlaymode();
                }
                if(frame>420)throw new Exception("Furniture capture timed out.");
            }
            catch(Exception e){Debug.LogException(e);SessionState.SetBool("FurnitureCaptureRun",false);EditorApplication.Exit(1);}
        }
        static void Key(string name){if(saved.Add(name))Save(name);}
        static void Save(string name)
        {
            var tex=DarkRoomStartCapture.RenderDisplayImage(camera,1280,720);File.WriteAllBytes(Path.Combine(Output,name+".png"),tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);
        }
        static void RecordFrame()
        {
            string path=Path.Combine(Frames,"frame-"+frame.ToString("D4")+".png");
            var tex=DarkRoomStartCapture.RenderDisplayImage(camera,960,540);File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);
            var body=player.transform.Find("Body_Visual");
            samples.Add(new Sample {frame=frame,support=player.CurrentSupportIndex,cracks=furniture.CrackCount,gameTime=(frame-20)/60f,
                player=player.transform.position,camera=camera.transform.position,sag=furniture.CurrentSag,delay=player.StumbleDelayRemaining,
                poseAngle=body.GetComponent<YunaCharacterView>()!=null?body.GetComponent<YunaCharacterView>().PoseLeanDegrees:Mathf.DeltaAngle(0,body.localEulerAngles.x),timeScale=Time.timeScale,cracked=furniture.HasCracked,stumbling=player.IsStumbling,file=path});
        }
    }
}
