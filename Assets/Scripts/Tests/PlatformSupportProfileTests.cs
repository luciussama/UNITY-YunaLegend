using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Tests
{
    public class PlatformSupportProfileTests
    {
        [Test]
        public void CachedSupportFollowsMovingOrTiltingColliderParts()
        {
            var root=new GameObject("MovingSupportProbe");
            try
            {
                var marker=root.AddComponent<DarkRoomPlatformMarker>();
                marker.spec=new PlatformSpec(999,"Probe",DarkRoomPhase.Phase1Ascent,0,0,6,1,.4f,Color.white,"");
                var contact=new GameObject("Contacto_Fisico");contact.transform.SetParent(root.transform,false);
                var part=new GameObject("MovingTop");part.transform.SetParent(contact.transform,false);
                var box=part.AddComponent<BoxCollider>();box.size=new Vector3(2,.4f,1);
                marker.InvalidateSupport();float before=marker.Support.PointNear(0).y;
                part.transform.localPosition=Vector3.down*.08f;part.transform.localRotation=Quaternion.Euler(0,0,5f);
                Physics.SyncTransforms();var point=marker.Support.PointNear(0);
                Assert.Less(point.y,before-.05f);
                Assert.IsTrue(box.Raycast(new Ray(point+Vector3.up,Vector3.down),out var hit,2f));
                Assert.AreEqual(point.y,hit.point.y,.002f);
            }
            finally {Object.DestroyImmediate(root);}
        }

        [Test]
        public void SeparateAreasDoNotCreateABridgeAcrossTheGap()
        {
            var p=PlatformSupportProfile.FromShapes(new[]{
                PlatformSupportShape.Box(new Vector3(-2,0,0),Quaternion.identity,new Vector3(2,.4f,1)),
                PlatformSupportShape.Box(new Vector3(2,1,0),Quaternion.identity,new Vector3(2,.4f,1))},Matrix4x4.identity);
            Assert.AreEqual(2,p.Regions.Count);
            Assert.IsFalse(p.TryGetPoint(0,out _));
            Assert.IsTrue(p.TryGetPoint(-2,out var left));Assert.IsTrue(p.TryGetPoint(2,out var right));
            Assert.AreEqual(1f,right.y-left.y,.001f);
            Assert.Greater(Mathf.Abs(p.PointNear(0).x),.9f);
        }

        [Test]
        public void HighestExposedFacesHideLowerOverlappingSurfaces()
        {
            var p=PlatformSupportProfile.FromShapes(new[]{
                PlatformSupportShape.Box(Vector3.zero,Quaternion.identity,new Vector3(4,.4f,1)),
                PlatformSupportShape.Box(Vector3.up,Quaternion.identity,new Vector3(2,.4f,1))},Matrix4x4.identity);
            Assert.IsTrue(p.TryGetPoint(0,out var middle));Assert.IsTrue(p.TryGetPoint(1.5f,out var side));
            Assert.AreEqual(1.2f,middle.y,.001f);Assert.AreEqual(.2f,side.y,.001f);
        }

        [Test]
        public void InclinedContactMatchesPhysicalRaycastAcrossItsWidth()
        {
            var go=new GameObject("TiltedContactProbe");
            try
            {
                go.transform.rotation=Quaternion.Euler(0,0,-12f);
                var box=go.AddComponent<BoxCollider>();box.size=new Vector3(2,.2f,1);
                Physics.SyncTransforms();var p=PlatformSupportProfile.FromColliders(new[]{box});
                for(float x=p.MinX+.05f;x<p.MaxX-.05f;x+=.1f)
                {
                    Assert.IsTrue(p.TryGetPoint(x,out var point));
                    Assert.IsTrue(box.Raycast(new Ray(point+Vector3.up,Vector3.down),out var hit,2f));
                    Assert.AreEqual(point.y,hit.point.y,.002f);
                }
                Assert.Greater(p.PointNear(p.MinX).y,p.PointNear(p.MaxX).y);
            }
            finally {Object.DestroyImmediate(go);}
        }

        [Test]
        public void AuthoredCircuitAndSavedPhysicalContactsAgreeForAll38Objects()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            try
            {
                var specs=DarkRoomThreeStageCircuitGenerator.GenerateStage1Platforms()
                    .Concat(DarkRoomThreeStageCircuitGenerator.GenerateStage2Platforms())
                    .Concat(DarkRoomThreeStageCircuitGenerator.GenerateStage3Platforms()).ToArray();
                var markers=Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                foreach(var marker in markers)
                {
                    var spec=specs.Single(s=>s.index==marker.spec.index);
                    var authored=DarkRoomPlatformGeometry.GetSupportProfile(spec);var actual=marker.Support;
                    Assert.AreEqual(authored.MinX,actual.MinX,.002f,$"left {spec.index}");
                    Assert.AreEqual(authored.MaxX,actual.MaxX,.002f,$"right {spec.index}");
                    Assert.AreEqual(authored.PointNear(spec.x+spec.contact.offsetX).y,marker.GetTopCenterWorld().y,.002f,$"height {spec.index}");
                }
                Assert.IsTrue(specs.Single(s=>s.index==1).contact.IsCustom,"The generator must carry the book's contact too.");
            }
            finally {EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);}
        }

        [Test]
        public void BearAndBooksExposeOnlyPointsSupportedByTheirRealColliders()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            try
            {
                Physics.SyncTransforms();
                var markers=Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                foreach(var marker in markers.Where(m=>m.spec.index<=1))
                {
                    var colliders=marker.transform.Find("Contacto_Fisico").GetComponentsInChildren<Collider>();
                    foreach(var region in marker.Support.Regions)
                    for(float x=region.MinX+.02f;x<region.MaxX-.02f;x+=.05f)
                    {
                        var point=region.At(x);float distance=float.MaxValue;
                        foreach(var c in colliders)if(c.Raycast(new Ray(point+Vector3.up,Vector3.down),out var hit,3f))distance=Mathf.Min(distance,hit.distance);
                        Assert.AreEqual(1f,distance,.003f,$"contact {marker.spec.index} at{x}");
                    }
                }
            }
            finally {EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);}
        }
    }
}
