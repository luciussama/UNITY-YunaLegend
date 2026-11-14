using UnityEngine;

namespace YunaLegend.Illustrated25D.Environment
{
    [DefaultExecutionOrder(330)]
    public sealed class DresserFloorReceiver : MonoBehaviour
    {
        RoomBackgroundT054 room;
        PenumbraLightingT054 lighting;
        Material material;
        Mesh mesh;
        public static void Install(RoomBackgroundT054 room, PenumbraLightingT054 lighting, float centreX,
            float halfWidth = 2.2f, string objectName = "Reflexo_Piso_Comoda")
        {
            var go = new GameObject(objectName); go.transform.SetParent(room.transform, false);
            go.transform.position = new Vector3(centreX, DresserPlatform.FloorY-.24f, .06f);
            var receiver = go.AddComponent<DresserFloorReceiver>(); receiver.room=room; receiver.lighting=lighting;
            receiver.material = new Material(Shader.Find("YunaLegend/DresserFloorReceiver"));
            receiver.material.mainTexture=room.Far.sprite.texture;
            receiver.mesh=new Mesh { name=objectName+"_NoCollider" };
            receiver.mesh.vertices=new[]{new Vector3(-halfWidth,-.44f,0),new Vector3(halfWidth,-.44f,0),
                new Vector3(-halfWidth,.30f,0),new Vector3(halfWidth,.30f,0)};
            receiver.mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.one};
            receiver.mesh.triangles=new[]{0,2,1,1,2,3}; receiver.mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh=receiver.mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=receiver.material;renderer.sortingOrder=-170;
            receiver.LateUpdate();
        }
        void LateUpdate()
        {
            if(room==null || material==null) return;
            var b=room.Far.bounds;
            material.SetVector("_RoomBounds",new Vector4(b.min.x,b.min.y,b.size.x,b.size.y));
            material.SetFloat("_Strength",lighting!=null?lighting.moonStrength:0);
            material.SetFloat("_FloorY",DresserPlatform.FloorY);
        }
        void OnDestroy() { if(material!=null)Destroy(material);if(mesh!=null)Destroy(mesh); }
    }
}
