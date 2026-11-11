using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Tests
{
    public class YieldingFurnitureTests
    {
        [Test]
        public void LongFurnitureHasRecalibratedEntryAndExitAndRestsOnTheFloor()
        {
            var route=DarkRoomThreeStageCircuitGenerator.GenerateStage1Platforms();
            var book=DarkRoomPlatformGeometry.GetSupportProfile(route[1]);
            var table=DarkRoomPlatformGeometry.GetSupportProfile(route[2]);
            var next=DarkRoomPlatformGeometry.GetSupportProfile(route[3]);
            Assert.AreEqual(8.4f,route[2].width,.001f);Assert.AreEqual(2,table.Regions.Count);
            Assert.IsFalse(table.TryGetPoint(route[2].x,out _));
            Assert.AreEqual(1.25f,PlatformSupportProfile.Gap(book,table,1f),.002f);
            Assert.AreEqual(1.35f,PlatformSupportProfile.Gap(table,next,1f),.002f);
            Assert.AreEqual(DarkRoomReturnLayout.FloorY,route[2].y-route[2].height*.5f,.002f);
        }

        [Test]
        public void BentBoardsKeepVisibleAndPhysicalTopFacesCoincident()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            try
            {
                var furniture=Object.FindFirstObjectByType<YieldingWoodFurniture>();Assert.IsNotNull(furniture);
                furniture.Preview(.075f,true);var marker=furniture.GetComponent<DarkRoomPlatformMarker>();
                foreach(string name in new[]{"Tampo_Esquerdo","Tampo_Direito"})
                {
                    var visual=furniture.transform.Find("Visual_3D/"+name);
                    var box=furniture.transform.Find("Contacto_Fisico/"+name).GetComponent<BoxCollider>();
                    foreach(float x in new[]{-.4f,0f,.4f})
                    {
                        var visible=visual.TransformPoint(new Vector3(x,.5f,0));
                        var physical=box.transform.TransformPoint(new Vector3(x*box.size.x,box.size.y*.5f,0));
                        Assert.Less(Vector3.Distance(visible,physical),.001f);
                    }
                }
                Assert.IsFalse(marker.Support.TryGetPoint(furniture.transform.position.x,out _));
                Assert.Greater(marker.Support.Regions[1].MinX-marker.Support.Regions[0].MaxX,.08f);
                furniture.ResetForRetry();Assert.IsFalse(furniture.HasCracked);Assert.AreEqual(0f,furniture.CurrentSag);
            }
            finally {EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);}
        }

        [Test]
        public void FurnitureArtworkAndWoodCrackArePersistedWithoutExcessiveObjectSplitting()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            try
            {
                var furniture=Object.FindFirstObjectByType<YieldingWoodFurniture>();Assert.IsNotNull(furniture);
                var clip=(AudioClip)new SerializedObject(furniture).FindProperty("woodCrack").objectReferenceValue;
                Assert.IsNotNull(clip);Assert.AreEqual("wood_furniture_crack",clip.name);
                Assert.That(clip.length,Is.InRange(.23f,.25f));
                var renderers=furniture.transform.Find("Visual_3D").GetComponentsInChildren<MeshRenderer>(true);
                Assert.LessOrEqual(renderers.Length,7);
                Assert.IsTrue(renderers.Any(r=>r.sharedMaterial!=null&&r.sharedMaterial.GetTexture("_BaseMap")!=null&&r.sharedMaterial.GetTexture("_BaseMap").name=="T_Furniture_Wood_Painted"));
            }
            finally {EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);}
        }
    }
}
