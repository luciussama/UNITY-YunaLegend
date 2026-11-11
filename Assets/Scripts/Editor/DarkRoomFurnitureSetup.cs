using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Camera25D;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomFurnitureSetup
    {
        public static void ConfigureMaterials()
        {
            const string path="Assets/Materials/T_Furniture_Wood_Painted.png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.textureShape=TextureImporterShape.Texture2D;
            importer.wrapMode=TextureWrapMode.Repeat;importer.mipmapEnabled=true;importer.sRGBTexture=true;
            importer.maxTextureSize=2048;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=4;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            CreateMaterial("M_Furniture_Wood",texture,Color.white,new Color(.08f,.05f,.025f),false);
            CreateMaterial("M_Furniture_Shade",texture,new Color(.65f,.52f,.40f),new Color(.035f,.02f,.009f),false);
            CreateMaterial("M_Furniture_Trim",texture,new Color(1f,.86f,.61f),new Color(.1f,.065f,.02f),false);
            CreateMaterial("M_Furniture_Brass",null,new Color(.75f,.48f,.16f),new Color(.045f,.025f,.006f),true);
        }

        static void CreateMaterial(string name,Texture2D texture,Color color,Color emission,bool metal)
        {
            string path="Assets/Materials/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find(metal?"Universal Render Pipeline/Lit":"Universal Render Pipeline/Simple Lit"));AssetDatabase.CreateAsset(mat,path);}
            mat.SetTexture("_BaseMap",texture);mat.SetColor("_BaseColor",color);
            mat.SetFloat("_Smoothness",metal?.38f:.12f);mat.SetFloat("_Metallic",metal?.65f:0f);
            if(!metal){mat.SetFloat("_SpecularHighlights",1f);mat.DisableKeyword("_SPECULAR_COLOR");}
            mat.SetTexture("_EmissionMap",texture);mat.SetColor("_EmissionColor",emission);mat.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(mat);
        }

        [MenuItem("YunaLegend/Aplicar movel02 com cedimento")]
        public static void ApplyAndSave()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");ConfigureMaterials();
            var builder=Object.FindFirstObjectByType<DarkRoomBlockoutBuilder>();var serialized=new SerializedObject(builder);
            var array=serialized.FindProperty("platformArtMaterials");array.arraySize=21;
            string[] names={"M_Furniture_Wood","M_Furniture_Shade","M_Furniture_Trim","M_Furniture_Brass"};
            for(int i=0;i<4;i++)array.GetArrayElementAtIndex(17+i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/"+names[i]+".mat");
            var crack=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/wood_furniture_crack.wav");
            serialized.FindProperty("furnitureCrackSfx").objectReferenceValue=crack;serialized.ApplyModifiedPropertiesWithoutUndo();
            var materials=Enumerable.Range(0,array.arraySize).Select(i=>(Material)array.GetArrayElementAtIndex(i).objectReferenceValue).ToArray();
            var specs=DarkRoomThreeStageCircuitGenerator.GenerateStage1Platforms()
                .Concat(DarkRoomThreeStageCircuitGenerator.GenerateStage2Platforms())
                .Concat(DarkRoomThreeStageCircuitGenerator.GenerateStage3Platforms()).ToDictionary(s=>s.index);
            foreach(var marker in Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                marker.spec=specs[marker.spec.index];marker.transform.position=new Vector3(marker.spec.x,marker.spec.y,0);
                if(marker.spec.index==2)
                {
                    Object.DestroyImmediate(marker.transform.Find("Visual_3D").gameObject);
                    DarkRoomPlatformArt.Build(DarkRoomPlatformGeometry.Get(2),marker.spec.contact,materials,marker.transform);
                }
                var contact=marker.transform.Find("Contacto_Fisico");
                foreach(var c in contact.GetComponents<Collider>())Object.DestroyImmediate(c);
                while(contact.childCount>0)Object.DestroyImmediate(contact.GetChild(0).gameObject);
                contact.localPosition=Vector3.zero;contact.localRotation=Quaternion.identity;contact.localScale=Vector3.one;
                DarkRoomPlatformArt.BuildContact(marker.spec,contact);marker.InvalidateSupport();
                if(marker.spec.index==2)
                {
                    var furniture=marker.GetComponent<YieldingWoodFurniture>();if(furniture==null)furniture=marker.gameObject.AddComponent<YieldingWoodFurniture>();
                    furniture.Configure(crack);furniture.ResetForRetry();EditorUtility.SetDirty(furniture);
                }
                EditorUtility.SetDirty(marker);
            }
            var side=Object.FindFirstObjectByType<SideScroll25DCamera>();side.RouteDistanceScale=1.05f;side.SnapToRouteFraming(0);
            EditorUtility.SetDirty(builder);AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("FURNITURE02_SAVED: real split top8.4m; localized stumble; recalibrated route; no general audio regeneration.");
        }
    }
}
