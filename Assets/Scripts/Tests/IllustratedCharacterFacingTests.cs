using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Tests
{
    public class IllustratedCharacterFacingTests
    {
        [TestCase(1f, 90f, false)]
        [TestCase(-1f, -90f, true)]
        public void PhysicalYawDoesNotTurnPaintingEdgeOn(float direction, float yaw, bool flip)
        {
            var root = new GameObject("Physics", typeof(CharacterController), typeof(Yuna25DPlayerController));
            try
            {
                var player = root.GetComponent<Yuna25DPlayerController>();
                player.RunDirection = direction;
                root.transform.SetPositionAndRotation(new Vector3(3, 4, 0), Quaternion.Euler(0, yaw, 0));
                var pose = new GameObject("Body_Visual"); pose.transform.SetParent(root.transform, false);
                var painting = new GameObject("Painting", typeof(SpriteRenderer)); painting.transform.SetParent(pose.transform, false);
                var view = painting.AddComponent<IllustratedCharacterFacing>();
                const BindingFlags lifecycle = BindingFlags.Instance | BindingFlags.NonPublic;
                typeof(IllustratedCharacterFacing).GetMethod("Awake", lifecycle).Invoke(view, null);
                typeof(IllustratedCharacterFacing).GetMethod("LateUpdate", lifecycle).Invoke(view, null);
                Assert.Less(Vector3.Angle(painting.transform.right, Vector3.right), .01f, "The painting's width must remain parallel to the running plane.");
                Assert.AreEqual(flip, painting.GetComponent<SpriteRenderer>().flipX);
                Assert.Less(Quaternion.Angle(root.transform.rotation, Quaternion.Euler(0, yaw, 0)), .01f, "Presentation must not change the physical root.");
                Assert.AreEqual(new Vector3(3, 4, 0), root.transform.position);
                Assert.AreEqual(Quaternion.identity, pose.transform.localRotation, "Pose parent is preserved for later animation.");
                Assert.AreEqual(direction, player.RunDirection);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
