using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using YunaLegend.DarkRoom;

namespace YunaLegend.Tests
{
    public class YunaCharacterModelTests
    {
        GameObject instance;
        SkinnedMeshRenderer skin;
        YunaCharacterView view;
        [SetUp] public void Create()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Characters/Yuna/Yuna_Study.prefab");
            Assert.IsNotNull(prefab); instance = Object.Instantiate(prefab);
            skin = instance.GetComponent<SkinnedMeshRenderer>(); view = instance.GetComponent<YunaCharacterView>();
        }
        [TearDown] public void Destroy() => Object.DestroyImmediate(instance);
        Mesh Bake() { var m = new Mesh(); skin.BakeMesh(m); return m; }
        [Test] public void RestMeshHasApprovedScaleCompleteRigAndValidSkin()
        {
            view.ResetPose(); var mesh = Bake();
            Assert.AreEqual(1.40f, mesh.bounds.size.y, .001f); Assert.AreEqual(0, mesh.bounds.min.y, .001f);
            Assert.AreEqual(0, instance.GetComponentsInChildren<Collider>().Length);
            foreach (string bone in new[] {"Head", "Ponytail", "Bow", "LeftHand", "RightHand", "LeftFoot", "RightFoot", "LeftSkirt", "RightSkirt"})
                Assert.IsTrue(skin.bones.Any(b => b.name == bone), bone);
            Assert.AreEqual(skin.bones.Length, skin.sharedMesh.bindposes.Length);
            Assert.Greater(skin.sharedMesh.normals.Count(n => n.z > .5f), 100);
            Assert.Greater(skin.sharedMesh.normals.Count(n => n.z < -.5f), 100);
            foreach (var w in skin.sharedMesh.boneWeights)
            {
                Assert.AreEqual(1, w.weight0 + w.weight1 + w.weight2 + w.weight3, .0001f);
                Assert.Less(w.boneIndex0, skin.bones.Length); Assert.Less(w.boneIndex1, skin.bones.Length);
            }
            foreach (var v in mesh.vertices) Assert.IsFalse(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z));
            Object.DestroyImmediate(mesh);
        }
        [Test] public void AnimatedPosesDeformMeshWithoutMovingPhysicsRootAndResetRestoresVertices()
        {
            view.ResetPose(); var rest = Bake(); var vertices = rest.vertices;
            foreach (var pose in new[] {YunaStudyPose.Run, YunaStudyPose.Jump, YunaStudyPose.Fall, YunaStudyPose.Land, YunaStudyPose.Stumble})
            {
                view.Evaluate(pose, .18f); var animated = Bake();
                Assert.IsTrue(animated.vertices.Where((p, i) => Vector3.Distance(p, vertices[i]) > .02f).Any(), pose.ToString());
                Assert.AreEqual(Vector3.zero, instance.transform.position); Assert.AreEqual(Quaternion.identity, instance.transform.rotation);
                Object.DestroyImmediate(animated);
            }
            view.ResetPose(); var reset = Bake();
            for (int i = 0; i < vertices.Length; i++) Assert.Less(Vector3.Distance(vertices[i], reset.vertices[i]), .00001f);
            Object.DestroyImmediate(rest); Object.DestroyImmediate(reset);
        }
        [Test] public void RunningStanceShoeSolesRemainOnGroundAcrossCycle()
        {
            var boneWeights = skin.sharedMesh.boneWeights;
            foreach (float phase in new[] {0f, .1f, .24f, .4f, .5f, .64f, .9f})
            {
                view.Evaluate(YunaStudyPose.Run, phase); var mesh = Bake(); var vertices = mesh.vertices;
                string stance = phase < .5f ? "LeftFoot" : "RightFoot";
                int index = System.Array.FindIndex(skin.bones, b => b.name == stance);
                float minY = vertices.Where((p, i) => boneWeights[i].boneIndex0 == index && boneWeights[i].weight0 > .99f).Min(p => p.y);
                Assert.AreEqual(0, minY, .003f, $"{stance}, phase={phase}"); Object.DestroyImmediate(mesh);
            }
        }
        [Test] public void LandingBendsKneesWithBothSolesOnGround()
        {
            view.Evaluate(YunaStudyPose.Land, .2f); var mesh = Bake(); var weights = skin.sharedMesh.boneWeights; var vertices = mesh.vertices;
            foreach (string foot in new[] {"LeftFoot", "RightFoot"})
            {
                int index = System.Array.FindIndex(skin.bones, b => b.name == foot);
                Assert.AreEqual(0, vertices.Where((p, i) => weights[i].boneIndex0 == index && weights[i].weight0 > .99f).Min(p => p.y), .003f);
            }
            Object.DestroyImmediate(mesh);
        }
        [Test] public void ExistingFillStaysOnCameraSideInBothRunDirections()
        {
            var playerRoot = new GameObject("PresentationLightTest");
            try
            {
                playerRoot.AddComponent<Yuna25DPlayerController>();
                instance.transform.SetParent(playerRoot.transform, false);
                var lamp = new GameObject("Yuna_FillLight").transform; lamp.SetParent(playerRoot.transform, false);
                foreach (float angle in new[] {90f, -90f})
                {
                    playerRoot.transform.rotation = Quaternion.Euler(0, angle, 0);
                    view.UpdatePresentationLight();
                    Assert.AreEqual(-1.4f, lamp.position.z - playerRoot.transform.position.z, .0001f);
                    Assert.AreEqual(Vector3.zero, playerRoot.transform.position);
                }
            }
            finally { Object.DestroyImmediate(playerRoot); }
        }
    }
}
