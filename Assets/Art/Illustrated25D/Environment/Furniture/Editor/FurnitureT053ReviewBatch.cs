using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Furniture.Editor
{
    public static class FurnitureT053ReviewBatch
    {
        const string Folder="Assets/Art/Illustrated25D/Environment/Furniture/";
        const string Output="Evidence/25d/T05.3/";
        [Serializable] class Audit
        {
            public string method="EditMode pose inspection using legacy Preview; NOT live traversal";
            public bool resetRestored,noSupportInsideOpenGap,poseMatched;
            public float sag,localPauseReference=YieldingWoodFurniture.StepDelay,recoveryReference=YieldingWoodFurniture.RecoveryDuration;
            public int intactRegions,brokenRegions;
            public bool runtimeTraversalValidated=false;
        }
        public static void Run()
        {
            try
            {
                Directory.CreateDirectory(Output);
                EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
                var marker=UnityEngine.Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(m=>m.spec.index==2);
                var furniture=marker.GetComponent<YieldingWoodFurniture>();
                var candidate=marker.gameObject.AddComponent<FurnitureIllustrationCandidate>();
                Import("furniture-body-v1.png");Import("furniture-top-wood-v1.png");
                candidate.Install(AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"furniture-body-v1.png"),AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"furniture-top-wood-v1.png"));
                furniture.enabled=false;
                // Do not render other objects/characters as though this were an integrated pilot.
                var root=marker.transform.Find(FurnitureIllustrationCandidate.RootName);
                foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
                    if(!r.transform.IsChildOf(root))r.enabled=false;
                var camera=new GameObject("T053_InspectionCamera").AddComponent<Camera>();
                camera.fieldOfView=45;camera.aspect=16f/9;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.06f,.065f,.12f);
                var p=marker.transform.position;
                camera.transform.position=p+new Vector3(0,2.2f,-12.7f);camera.transform.LookAt(p+Vector3.up*.25f);
                var audit=new Audit();audit.intactRegions=marker.Support.Regions.Count;
                Capture(camera,"01-intact-gameplay-scale");
                Capture(camera,"05-intact-support-overlay",marker.Support);
                furniture.Preview(.075f,true);candidate.SyncPose();Physics.SyncTransforms();
                audit.sag=furniture.CurrentSag;audit.brokenRegions=marker.Support.Regions.Count;
                audit.noSupportInsideOpenGap=!marker.Support.TryGetPoint(p.x+.009f,out _);
                audit.poseMatched=true;
                foreach(string name in new[]{"Tampo_Esquerdo","Tampo_Direito"})
                {
                    var contact=marker.transform.Find("Contacto_Fisico/"+name);var visual=root.Find(name+"_Painted");
                    audit.poseMatched &= Vector3.Distance(contact.position,visual.position)<.00001f && Quaternion.Angle(contact.rotation,visual.rotation)<.001f;
                }
                Capture(camera,"02-broken-gameplay-scale");
                Capture(camera,"06-broken-support-overlay",marker.Support);
                camera.transform.position=p+new Vector3(0,2.9f,-7.8f);camera.transform.LookAt(p+Vector3.up*.55f);Capture(camera,"03-broken-detail");
                furniture.ResetForRetry();candidate.SyncPose();Physics.SyncTransforms();
                audit.resetRestored=!furniture.HasCracked && furniture.CrackCount==0 && furniture.CurrentSag==0 && marker.Support.TryGetPoint(p.x+.009f,out _);
                Capture(camera,"04-reset-detail");
                File.WriteAllText(Output+"editmode-audit.json",JsonUtility.ToJson(audit,true));
                if(!audit.resetRestored || !audit.poseMatched || !audit.noSupportInsideOpenGap || audit.brokenRegions<2)throw new Exception("T053 pose/support/reset inspection failed");
                Debug.Log("T053_EDITMODE_INSPECTION_PASS (no runtime traversal claimed)");
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        static void Import(string name)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Folder+name);
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;importer.isReadable=false;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.filterMode=FilterMode.Bilinear;
            importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
        }
        static void Capture(Camera camera,string name,PlatformSupportProfile support=null)
        {
            var target=new RenderTexture(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGBA32,false);
            var previous=RenderTexture.active;var previousTarget=camera.targetTexture;
            try
            {
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,1280,720),0,0);
                if(support!=null)foreach(var segment in support.Segments)
                {
                    var a=camera.WorldToScreenPoint(segment.left);var b=camera.WorldToScreenPoint(segment.right);
                    int steps=Mathf.CeilToInt(Vector2.Distance(a,b));
                    for(int i=0;i<=steps;i++)
                    {
                        var p=Vector3.Lerp(a,b,steps==0?0:(float)i/steps);
                        for(int dy=-2;dy<=2;dy++)
                        {
                            int x=Mathf.RoundToInt(p.x),y=Mathf.RoundToInt(p.y)+dy;
                            if(x>=0&&x<1280&&y>=0&&y<720)image.SetPixel(x,y,Color.cyan);
                        }
                    }
                }
                image.Apply();File.WriteAllBytes(Output+name+".png",image.EncodeToPNG());
            }
            finally {camera.targetTexture=previousTarget;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(image);target.Release();UnityEngine.Object.DestroyImmediate(target);}
        }
    }
}
