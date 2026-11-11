using System.Collections.Generic;
using UnityEngine;

namespace YunaLegend.DarkRoom
{
    public static partial class DarkRoomPlatformArt
    {
        private sealed class FurnitureMeshBucket
        {
            readonly List<CombineInstance> parts=new List<CombineInstance>();
            public void Add(Mesh mesh,Vector3 position,Vector3 scale,Quaternion rotation) =>
                parts.Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(position,rotation,scale)});
            public void Box(Vector3 p,Vector3 size) => Add(PrimitiveMesh(PrimitiveType.Cube),p,size,Quaternion.identity);
            public void Build(Transform root,string name,Material material)
            {
                var mesh=new Mesh{name=name};mesh.CombineMeshes(parts.ToArray(),true,true);mesh.RecalculateBounds();
                AddPart(root,name,mesh,material,Vector3.zero,Quaternion.identity,Vector3.one);
            }
        }

        private static GameObject BuildYieldingFurniture(PlatformGeometry geometry,Material[] materials,Transform parent)
        {
            var root=new GameObject("Visual_3D");root.transform.SetParent(parent,false);
            float w=geometry.Width,h=geometry.Height,d=geometry.Depth,bottom=-h*.5f;
            var wood=Mat(materials,PlatformArtMaterialIndex.FurnitureWood);
            var shade=Mat(materials,PlatformArtMaterialIndex.FurnitureShade);
            var trim=Mat(materials,PlatformArtMaterialIndex.FurnitureTrim);
            var brass=Mat(materials,PlatformArtMaterialIndex.FurnitureBrass);
            var body=new FurnitureMeshBucket();var faces=new FurnitureMeshBucket();var mould=new FurnitureMeshBucket();var hardware=new FurnitureMeshBucket();
            body.Box(new Vector3(0,-.01f,d*.45f),new Vector3(w-.30f,h-.38f,.12f));
            foreach(float x in new[]{-w*.48f,-w*.15f,w*.15f,w*.48f})
                body.Box(new Vector3(x,-.01f,0),new Vector3(.16f,h-.34f,d-.14f));
            body.Box(new Vector3(0,bottom+.20f,0),new Vector3(w-.10f,.18f,d-.10f));
            foreach(float x in new[]{-w*.445f,w*.445f})
            {
                body.Box(new Vector3(x,bottom+.10f,0),new Vector3(.42f,.20f,d*.76f));
                mould.Box(new Vector3(x,bottom+.24f,-d*.40f),new Vector3(.48f,.10f,.22f));
            }
            foreach(float y in new[]{bottom+.29f,-.25f,.37f})
                faces.Box(new Vector3(0,y,.01f),new Vector3(w*.28f,.10f,d-.28f));
            foreach(float side in new[]{-1f,1f})
            for(int row=0;row<3;row++)
            {
                float x=side*w*.315f,y=bottom+.58f+row*.60f,bw=w*.285f,bh=.51f,z=-d*.5f+.01f;
                faces.Box(new Vector3(x,y,z),new Vector3(bw,bh,.14f));
                // Physical frame pieces, never a painted box standing in for a drawer.
                foreach(float yy in new[]{y-bh*.41f,y+bh*.41f})mould.Box(new Vector3(x,yy,z-.08f),new Vector3(bw*.92f,.035f,.035f));
                foreach(float xx in new[]{x-bw*.46f,x+bw*.46f})mould.Box(new Vector3(xx,y,z-.08f),new Vector3(.035f,bh*.85f,.035f));
                hardware.Add(FurnitureRingMesh(),new Vector3(x,y-.025f,z-.13f),Vector3.one,Quaternion.identity);
                hardware.Add(PrimitiveMesh(PrimitiveType.Sphere),new Vector3(x,y+.07f,z-.11f),new Vector3(.10f,.10f,.07f),Quaternion.identity);
            }
            foreach(float y in new[]{bottom+.30f,h*.5f-.20f})mould.Box(new Vector3(0,y,-d*.5f-.04f),new Vector3(w,.06f,.08f));
            foreach(float x in new[]{-w*.485f,-w*.155f,w*.155f,w*.485f})mould.Box(new Vector3(x,-.01f,-d*.5f-.025f),new Vector3(.055f,h-.45f,.05f));
            body.Build(root.transform,"Estrutura_Madeira",shade);faces.Build(root.transform,"Gavetas_e_Prateleiras",wood);
            mould.Build(root.transform,"Molduras",trim);hardware.Build(root.transform,"Puxadores_Latao",brass);
            var spec=new PlatformSpec(2,"Furniture",DarkRoomPhase.Phase1Ascent,0,0,w,d,h,Color.white,"");
            foreach(var shape in GetSupportShapes(spec))
                AddPart(root.transform,shape.name,PrimitiveMesh(PrimitiveType.Cube),wood,shape.position,shape.rotation,shape.boxSize);
            var splinters=new GameObject("Farpas");splinters.transform.SetParent(root.transform,false);
            var shards=new FurnitureMeshBucket();
            for(int i=0;i<5;i++)
                shards.Add(PrimitiveMesh(PrimitiveType.Cube),new Vector3((i-2)*.035f,h*.5f-.055f-i*.008f,-d*.5f-.018f),new Vector3(.015f,.17f,.025f),Quaternion.Euler(0,0,(i-2)*19f));
            shards.Build(splinters.transform,"Fibras_Quebradas",trim);splinters.SetActive(false);
            return root;
        }

        private static Mesh FurnitureRingMesh()
        {
            const string key="furniture-brass-ring";if(MeshCache.TryGetValue(key,out var cached)&&cached!=null)return cached;
            const int around=20,tube=8;var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
            for(int a=0;a<=around;a++)for(int b=0;b<=tube;b++)
            {
                float u=a*Mathf.PI*2/around,w=b*Mathf.PI*2/tube,r=.10f+.022f*Mathf.Cos(w);
                v.Add(new Vector3(Mathf.Cos(u)*r,Mathf.Sin(u)*r,.022f*Mathf.Sin(w)));uv.Add(new Vector2((float)a/around,(float)b/tube));
            }
            for(int a=0;a<around;a++)for(int b=0;b<tube;b++)
            {
                int i=a*(tube+1)+b,j=i+tube+1;t.AddRange(new[]{i,j,i+1,i+1,j,j+1});
            }
            var mesh=new Mesh{name=key};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            MeshCache[key]=mesh;return mesh;
        }
    }
}
