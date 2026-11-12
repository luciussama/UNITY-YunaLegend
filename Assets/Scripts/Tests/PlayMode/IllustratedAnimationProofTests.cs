using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using YunaLegend.Illustrated25D;

namespace YunaLegend.RoutePhysics.Tests
{
    /// <summary>
    /// T02.3 contracts: the skin deforms only its own mesh, the sole never crosses the support line,
    /// and the proof components never enter the Gameplay scene.
    /// </summary>
    public class IllustratedAnimationProofTests
    {
        GameObject host;
        Sprite sprite;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("painted-skin-test");
            var texture = new Texture2D(64, 96, TextureFormat.RGBA32, false);
            sprite = Sprite.Create(texture, new Rect(0, 0, 64, 96), new Vector2(.5f, .02f), 64f);
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
            if (sprite != null) Object.DestroyImmediate(sprite.texture);
            if (sprite != null) Object.DestroyImmediate(sprite);
        }

        PaintedMeshDeformer Build()
        {
            var skin = host.AddComponent<PaintedMeshDeformer>();
            skin.columns = 12;
            skin.rows = 36;
            skin.Build(sprite, null);
            return skin;
        }

        [Test]
        public void BindPoseMatchesSpriteBounds()
        {
            var skin = Build();
            Assert.AreEqual(13 * 37, skin.VertexCount, "Grid vertices");
            Assert.AreEqual(12 * 36 * 2, skin.TriangleCount, "Grid triangles");
            Assert.AreEqual(sprite.bounds.min.y, skin.MinLocalY, .0001f, "Neutral bottom");
            Assert.AreEqual(sprite.bounds.max.y, skin.MaxLocalY, .0001f, "Neutral top");
            Assert.AreEqual(sprite.bounds.size.x * .5f, skin.MaxAbsLocalX, .0001f, "Neutral half width");
        }

        [Test]
        public void SoleStaysOnSupportForEveryProvisionalPose()
        {
            var skin = Build();
            var poses = new[]
            {
                new[] { 20f, 20f, 12f, 1.05f, .95f, .05f, .12f },
                new[] { -20f, -20f, -12f, .9f, 1.05f, -.05f, .12f },
                new[] { 7f, 12f, -2.5f, 1f, 1f, .004f, .055f },
                new[] { 6f, -10f, -3f, 1.045f, .975f, 0f, 0f }
            };
            foreach (var pose in poses)
            {
                skin.lean = pose[0]; skin.sideBandSwing = pose[1]; skin.headTilt = pose[2];
                skin.squashY = pose[3]; skin.squashX = pose[4]; skin.breathe = pose[5]; skin.bodyCompression = pose[6];
                skin.Refresh();
                Assert.GreaterOrEqual(skin.MinSoleLocalY, -.0001f, "Sole crossed the support line");
                Assert.LessOrEqual(skin.MaxAbsLocalX, sprite.bounds.size.x, "Deformation widened the painting out of bounds");
                Assert.LessOrEqual(skin.MaxLocalY, sprite.bounds.max.y * 1.08f, "Deformation stretched the painting out of bounds");
                Assert.IsFalse(float.IsNaN(skin.MinLocalY) || float.IsInfinity(skin.MinLocalY), "Deformed mesh is not finite");
            }
        }

        [Test]
        public void DeformationNeverMovesParentColliderOrPivot()
        {
            var parent = new GameObject("player-root");
            parent.transform.SetPositionAndRotation(new Vector3(3f, .5f, 0f), Quaternion.Euler(0f, 35f, 0f));
            parent.transform.localScale = Vector3.one;
            host.transform.SetParent(parent.transform, false);
            var startPosition = parent.transform.position;
            var startRotation = parent.transform.rotation;
            var startScale = parent.transform.localScale;
            var skin = Build();
            var renderer = skin.GetComponent<MeshRenderer>();
            Assert.IsNull(skin.GetComponent<Collider>(), "Skin must not own a collider");

            skin.lean = 18f; skin.sideBandSwing = 16f; skin.bodyCompression = .08f; skin.squashY = .94f;
            skin.flipX = true;
            skin.Refresh();

            Assert.AreEqual(startPosition, parent.transform.position, "Root moved");
            Assert.AreEqual(startRotation, parent.transform.rotation, "Root rotated");
            Assert.AreEqual(startScale, parent.transform.localScale, "Root scaled");
            Assert.AreEqual(Vector3.zero, skin.transform.localPosition, "Pivot offset");
            Assert.AreEqual(Quaternion.identity, skin.transform.localRotation, "Pivot rotated");
            Assert.IsNotNull(renderer);
            Object.DestroyImmediate(parent);
        }

        [Test]
        public void GameplaySceneHasNoProofComponents()
        {
            string scenePath = Path.Combine(Application.dataPath, "Scenes", "DarkRoom_Gameplay.unity");
            Assert.IsTrue(File.Exists(scenePath), "Gameplay scene missing");
            var scene = File.ReadAllText(scenePath);
            foreach (var script in new[] { "IllustratedPaintedAnimation.cs", "PaintedMeshDeformer.cs" })
            {
                string metaPath = Path.Combine(Application.dataPath, "Scripts", "Runtime", "Illustrated25D", script + ".meta");
                Assert.IsTrue(File.Exists(metaPath), "Missing meta for " + script);
                var guid = Regex.Match(File.ReadAllText(metaPath), "guid: ([0-9a-f]{32})").Groups[1].Value;
                Assert.IsFalse(string.IsNullOrEmpty(guid), "No GUID in " + script);
                Assert.IsFalse(scene.Contains(guid), "Gameplay scene contains proof component " + script);
            }
        }
    }
}
