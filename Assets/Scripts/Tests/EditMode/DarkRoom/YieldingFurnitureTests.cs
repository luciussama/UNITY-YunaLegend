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

    }
}
