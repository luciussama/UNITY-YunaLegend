using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using YunaLegend.YunaIllustration;

namespace YunaLegend.Illustrated25D.Preview
{
    /// <summary>Opt-in executable smoke test; absent from normal player input and timing.</summary>
    [DefaultExecutionOrder(310)]
    public sealed class PreviewVerification : MonoBehaviour
    {
        [Serializable] public class Proof
        {
            public int hz,frames,maxLegacy,maxVisible,errors;
            public bool success,bear,books,furniture,cracked,stumbled,finished,reset,idleCompanionNear;
            public float maxSag,maxLocalPause;
            public float bearFloorGap, booksFloorGap, shadowFacing;
            public float bearAboveTrim, booksAboveTrim;
            public bool paintedContact;
            public int cracks;
            public string error;
            public List<string> events=new List<string>();
        }
        static Proof proof;
        static string output;
        static bool resetting;
        IllustratedPreview preview;
        Keyboard keyboard;
        int tick,releaseAt=-1,finishAt=-1;
        bool first,second,third;
        float seconds;
        void Start()
        {
            var args=System.Environment.GetCommandLineArgs();
            int at=Array.IndexOf(args,"-previewVerify");
            if(at<0){enabled=false;return;}
            output=at+1<args.Length?args[at+1]:Application.persistentDataPath;
            Directory.CreateDirectory(output);
            int hzAt=Array.IndexOf(args,"-previewHz");
            int hz=hzAt>=0?int.Parse(args[hzAt+1]):60;
            if(proof==null)proof=new Proof{hz=hz};
            Application.logMessageReceived+=OnLog;
            Time.captureDeltaTime=1f/hz;
            preview=GetComponent<IllustratedPreview>();
            Application.runInBackground=true;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            keyboard=InputSystem.AddDevice<Keyboard>();
            InputSystem.EnableDevice(keyboard);
        }
        void LateUpdate()
        {
            if(!preview.Ready)
            { if(preview.Failure!=null) End(preview.Failure);return; }
            tick++;seconds+=Time.deltaTime;proof.frames++;
            if(tick==2)
            {
                var bear = FindFirstObjectByType<YunaLegend.Illustrated25D.Environment.IllustratedBearPlatform>();
                var books = FindFirstObjectByType<YunaLegend.Illustrated25D.Environment.IllustratedBookStackPlatform>();
                var bearBase = bear.transform.Find("Visual_Ilustrado_Urso");
                var bookBase = books.transform.Find("Livros_Base_T054");
                if (bookBase == null) bookBase = books.transform.Find("Visual_Ilustrado_Livros");
                proof.bearFloorGap = bearBase.GetComponent<Renderer>().bounds.min.y - preview.Room.FloorWorldY;
                proof.booksFloorGap = bookBase.GetComponent<Renderer>().bounds.min.y - preview.Room.FloorWorldY;
                var shadow = GameObject.Find(YunaLegend.Illustrated25D.Environment.ContactShadowT054.RootName).transform.GetChild(0);
                proof.shadowFacing = Mathf.Abs(Vector3.Dot(shadow.forward, Camera.main.transform.forward));
                // Independent measured landmark in the source PNG: bottom of trim,
                // row 654/724. A base at the old row 642 must fail this audit.
                var roomBounds = preview.Room.Far.bounds;
                float trimY = roomBounds.min.y + roomBounds.size.y * (70f / 724f);
                proof.bearAboveTrim = bearBase.GetComponent<Renderer>().bounds.min.y - trimY;
                proof.booksAboveTrim = bookBase.GetComponent<Renderer>().bounds.min.y - trimY;
                proof.paintedContact = Mathf.Abs(proof.bearFloorGap) < .001f
                    && Mathf.Abs(proof.booksFloorGap) < .001f && proof.shadowFacing > .99f
                    && proof.bearAboveTrim < -.04f && proof.booksAboveTrim < -.04f;
                Shot(resetting?"07-retry":"01-ready");
            }
            if(resetting)
            {
                if(tick<10)return;
                proof.reset=!preview.Started&&!preview.Furniture.HasCracked&&preview.Furniture.CrackCount==0
                    &&!preview.Player.IsDead&&!preview.Player.IsStumbling;
                End(null);return;
            }
            if(tick==Mathf.RoundToInt(proof.hz*.75f))Shot("01-ready-stable");
            if(tick==proof.hz)
            {
                proof.idleCompanionNear=Vector3.Distance(preview.Flight.transform.position,preview.Player.transform.position)<3f;
                preview.Begin();proof.events.Add("Begin");
            }
            if(tick==releaseAt)InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            var p=preview.Player;
            if(!first&&preview.Started&&p.transform.position.x>=-.64f){first=true;Jump();}
            if(p.CurrentSupportIndex==0&&!proof.bear){proof.bear=true;Shot("02-bear");}
            if(proof.bear&&!second&&p.transform.position.x>=2.72f){second=true;Jump();}
            if(p.CurrentSupportIndex==1&&!proof.books){proof.books=true;Shot("03-books");}
            if(proof.books&&!third&&p.IsGrounded&&p.transform.position.x>=5.95f){third=true;Jump();}
            if(p.CurrentSupportIndex==2&&!proof.furniture){proof.furniture=true;Shot("04-furniture");}
            if(preview.Furniture.HasCracked&&!proof.cracked){proof.cracked=true;Shot("05-cracked");}
            proof.stumbled|=p.IsStumbling;
            proof.maxSag=Mathf.Max(proof.maxSag,preview.Furniture.CurrentSag);
            proof.maxLocalPause=Mathf.Max(proof.maxLocalPause,p.StumbleDelayRemaining);
            proof.cracks=preview.Furniture.CrackCount;
            proof.maxLegacy=Mathf.Max(proof.maxLegacy,preview.LegacyRenderersVisible);
            var resolver=p.GetComponent<YunaIllustrationVisibilityResolver>();
            proof.maxVisible=Mathf.Max(proof.maxVisible,resolver.VisibleCount);
            if(preview.Finished&&finishAt<0){proof.finished=true;finishAt=tick+proof.hz;Shot("06-finished");}
            if(finishAt>0&&tick>=finishAt){resetting=true;preview.Retry();return;}
            if(p.IsDead){Shot("failure");End("Player died at "+p.transform.position+" velocity="+p.Velocity+" support="+p.CurrentSupportIndex);}
            else if(seconds>24)End("Timeout x="+p.transform.position.x+" support="+p.CurrentSupportIndex);
        }
        void Jump()
        {
            proof.events.Add("jump x="+preview.Player.transform.position.x+" y="+preview.Player.transform.position.y
                +" keyboardEnabled="+keyboard.enabled+" focus="+Application.isFocused);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));releaseAt=tick+Mathf.RoundToInt(.12f*proof.hz);
        }
        void Shot(string name)
        {
            proof.events.Add(name+" x="+preview.Player.transform.position.x);
            var camera=Camera.main;
            var hdr=new RenderTexture(1280,720,24,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear);
            var display=new RenderTexture(1280,720,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            var previous=camera.targetTexture; var active=RenderTexture.active; bool srgb=GL.sRGBWrite;
            try
            {
                camera.targetTexture=hdr; camera.Render();
                GL.sRGBWrite=true; Graphics.Blit(hdr,display); RenderTexture.active=display;
                var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
                var pixels=image.GetPixels32();
                using(var stream=File.Create(Path.Combine(output,name+".ppm")))
                {
                    var header=System.Text.Encoding.ASCII.GetBytes("P6\n1280 720\n255\n");stream.Write(header,0,header.Length);
                    var row=new byte[1280*3];
                    for(int y=719;y>=0;y--)
                    {
                        for(int x=0;x<1280;x++)
                        { var c=pixels[y*1280+x];row[x*3]=c.r;row[x*3+1]=c.g;row[x*3+2]=c.b; }
                        stream.Write(row,0,row.Length);
                    }
                }
                Destroy(image);
            }
            finally
            { camera.targetTexture=previous;RenderTexture.active=active;GL.sRGBWrite=srgb;hdr.Release();display.Release();Destroy(hdr);Destroy(display); }
        }
        void End(string error)
        {
            proof.error=error;
            proof.success=error==null&&proof.bear&&proof.books&&proof.furniture&&proof.cracked&&proof.stumbled
                &&proof.finished&&proof.reset&&proof.idleCompanionNear&&proof.cracks==1
                &&proof.maxLegacy==0&&proof.maxVisible==1&&proof.errors==0&&proof.paintedContact;
            File.WriteAllText(Path.Combine(output,"verification.json"),JsonUtility.ToJson(proof,true));
            Debug.Log("ILLUSTRATED_PREVIEW_VERIFY "+proof.success+" "+error);
            enabled=false; Application.Quit(proof.success?0:1);
        }
        void OnLog(string condition,string stack,LogType type)
        { if(type==LogType.Error||type==LogType.Exception){proof.errors++;proof.events.Add(condition);} }
        void OnDestroy()
        { Application.logMessageReceived-=OnLog;if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard); }
    }
}
