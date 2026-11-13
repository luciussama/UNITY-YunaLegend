using System;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Illustrated25D.Furniture
{
    [DefaultExecutionOrder(200)]
    public sealed class FurnitureIllustrationCandidate : MonoBehaviour
    {
        public const string RootName = "Furniture_Illustrated_T053";
        Transform[] contacts, panels;
        Transform fibres;
        YieldingWoodFurniture yielding;
        Material woodMaterial, bodyMaterial;
        Sprite bodySprite;
        Mesh[] meshes;

        public void Install(Texture2D body, Texture2D wood)
        {
            if (transform.Find(RootName) != null) throw new InvalidOperationException("T053 already installed");
            var marker = GetComponent<DarkRoomPlatformMarker>();
            if (marker == null || marker.spec.index != 2) throw new InvalidOperationException("Furniture marker2 required");
            if (Mathf.Abs(marker.spec.width-8.4f)>.001f || Mathf.Abs(marker.spec.height-2.3f)>.001f || Mathf.Abs(marker.spec.depth-2.4f)>.001f)
                throw new InvalidOperationException("Candidate support contract is width8.4 depth2.4 height2.3m");
            yielding = GetComponent<YieldingWoodFurniture>();
            if (yielding == null) throw new InvalidOperationException("Keep existing YieldingWoodFurniture");
            yielding.ResetForRetry();
            var root = new GameObject(RootName).transform; root.SetParent(transform, false);
            bodyMaterial = new Material(Shader.Find("Sprites/Default")) {mainTexture=body};
            woodMaterial = new Material(Shader.Find("Unlit/Texture")) {mainTexture=wood};
            bodySprite = Sprite.Create(body,new Rect(0,0,body.width,body.height),new Vector2(.5f,1),200,0,SpriteMeshType.FullRect);
            var face = new GameObject("Static_painted_drawers_and_feet").AddComponent<SpriteRenderer>();
            face.transform.SetParent(root,false); face.sprite=bodySprite;face.sharedMaterial=bodyMaterial;
            float scale=8.9f/(body.width/200f); face.transform.localScale=Vector3.one*scale;
            face.transform.localPosition=new Vector3(0,0.99f,-1.2f);
            contacts=new Transform[2];panels=new Transform[2];meshes=new Mesh[2];
            for(int i=0;i<2;i++)
            {
                string name=i==0?"Tampo_Esquerdo":"Tampo_Direito";
                contacts[i]=transform.Find("Contacto_Fisico/"+name);
                var legacy=transform.Find("Visual_3D/"+name);
                if(contacts[i]==null || legacy==null)throw new InvalidOperationException("Missing legacy hinges");
                foreach(var c in contacts[i].GetComponents<Collider>())c.enabled=false;
                meshes[i]=BuildPanel(i==0,contacts[i].localPosition);
                var collider=contacts[i].gameObject.AddComponent<MeshCollider>();collider.sharedMesh=meshes[i];collider.convex=false;
                var panel=new GameObject(name+"_Painted");panel.transform.SetParent(root,false);panels[i]=panel.transform;
                panel.AddComponent<MeshFilter>().sharedMesh=meshes[i];panel.AddComponent<MeshRenderer>().sharedMaterial=woodMaterial;
            }
            fibres=new GameObject("Localized_grain_fibres_no_support").transform;fibres.SetParent(root,false);
            for(int i=0;i<5;i++)
            {
                var fibre=GameObject.CreatePrimitive(PrimitiveType.Cube);fibre.name="Fibre_"+i;
                fibre.transform.SetParent(fibres,false);DestroyOwned(fibre.GetComponent<Collider>());
                fibre.transform.localPosition=new Vector3((i-2)*.025f,1.05f-i*.012f,-1.22f);
                fibre.transform.localScale=new Vector3(.011f,.10f+i*.012f,.012f);
                fibre.transform.localRotation=Quaternion.Euler(0,0,(i-2)*17);
                fibre.GetComponent<Renderer>().sharedMaterial=woodMaterial;
            }
            // Keep the legacy hinge transforms active for the existing event, but hide only its renderers.
            foreach(var r in transform.Find("Visual_3D").GetComponentsInChildren<Renderer>(true))r.enabled=false;
            marker.InvalidateSupport();SyncPose();Physics.SyncTransforms();
        }

        public void SyncPose()
        {
            if(contacts==null)return;
            for(int i=0;i<2;i++)
            {
                panels[i].localPosition=contacts[i].localPosition;
                panels[i].localRotation=contacts[i].localRotation;
            }
            fibres.gameObject.SetActive(yielding.HasCracked);
        }
        void LateUpdate()=>SyncPose();

        public static Mesh BuildPanel(bool left,Vector3 origin)
        {
            // Irregular fatigue edge varies with depth; both rest-state panels share this exact boundary.
            float[] z={-1.2f,-.82f,-.43f,0,.36f,.78f,1.2f};
            float[] edge={-.018f,.026f,-.012f,.009f,-.031f,.018f,-.008f};
            var vertices=new Vector3[z.Length*4];var uv=new Vector2[vertices.Length];
            var indices=new System.Collections.Generic.List<int>();
            for(int i=0;i<z.Length;i++)
            {
                float a=left?-4.2f:edge[i],b=left?edge[i]:4.2f;
                Vector3[] strip={new Vector3(a,1.15f,z[i]),new Vector3(b,1.15f,z[i]),new Vector3(a,0.99f,z[i]),new Vector3(b,0.99f,z[i])};
                for(int k=0;k<4;k++){vertices[i*4+k]=strip[k]-origin;uv[i*4+k]=new Vector2((strip[k].x+4.2f)/8.4f,(strip[k].z+1.2f)/2.4f);}
                if(i==0)continue;
                int p=(i-1)*4,q=i*4;
                Quad(indices,p,q,q+1,p+1);Quad(indices,p+2,p+3,q+3,q+2);
                Quad(indices,p,p+2,q+2,q);Quad(indices,p+1,q+1,q+3,p+3);
            }
            Quad(indices,0,1,3,2);int last=(z.Length-1)*4;Quad(indices,last,last+2,last+3,last+1);
            var mesh=new Mesh{name=left?"T053_Left_grain_edge":"T053_Right_grain_edge"};
            mesh.vertices=vertices;mesh.uv=uv;mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        static void Quad(System.Collections.Generic.List<int> t,int a,int b,int c,int d){t.AddRange(new[]{a,b,c,a,c,d});}
        static void DestroyOwned(UnityEngine.Object o){if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
        void OnDestroy()
        {
            if(meshes!=null)foreach(var mesh in meshes)if(mesh!=null)DestroyOwned(mesh);
            if(bodySprite!=null)DestroyOwned(bodySprite);
            if(bodyMaterial!=null)DestroyOwned(bodyMaterial);if(woodMaterial!=null)DestroyOwned(woodMaterial);
        }
    }
}
