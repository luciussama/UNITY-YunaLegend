using System;
using UnityEngine;
using UnityEngine.Rendering;
using YunaLegend.Picnic.Core;

namespace YunaLegend.Picnic.Runtime
{
    /// <summary>
    /// 16-joint articulated 2D skeletal puppet view for Yuna in the Garden (Picnic) stage.
    /// Combines Unity's anatomical bone hierarchy with lightweight 2D mesh deformation (LBS)
    /// for bending knees, ankles, elbows, flexible chest/skirt sway, and 2-bone S-wave pigtails.
    /// </summary>
    public sealed class PicnicYunaRigView : MonoBehaviour
    {
        private const string ResourceRoot = "Picnic/YunaRig2D/";
        private const float BaseHipsY = 0.44f;

        private Transform rigRoot;
        private Transform hipsBone;
        private Transform torsoBone;
        private Transform chestBone;
        private Transform headBone;
        private Transform pigtailBackBone;
        private Transform pigtailFarBone;
        private Transform armFrontBone;
        private Transform armBackBone;
        private Transform legFrontBone;
        private Transform legBackBone;

        private SpriteRenderer headRenderer;
        private Sprite headHappySprite;
        private Sprite headStrainSprite;
        private Sprite headScaredSprite;
        private Sprite headPanicSprite;

        private DeformablePartMesh legFrontMesh;
        private DeformablePartMesh legBackMesh;
        private DeformablePartMesh armFrontMesh;
        private DeformablePartMesh armBackMesh;
        private DeformablePartMesh torsoMesh;
        private DeformablePartMesh pigtailBackMesh;
        private DeformablePartMesh pigtailFarMesh;
        private DeformablePartMesh[] allMeshes;

        private bool isBuilt;
        private bool hasSmoothedPose;
        private PicnicYunaRigPose smoothedPose;
        private bool wasOnGround = true;
        private float landingSquashTimer;

        public bool IsBuilt => isBuilt;

        public bool TryBuild(int baseSortingOrder = 12)
        {
            if (isBuilt)
            {
                return true;
            }

            headHappySprite = LoadPartSprite("yuna_head_happy", new Vector2(0.50f, 0.04f));
            headStrainSprite = LoadPartSprite("yuna_head_strain", new Vector2(0.50f, 0.04f));
            headScaredSprite = LoadPartSprite("yuna_head_scared", new Vector2(0.50f, 0.04f));
            headPanicSprite = LoadPartSprite("yuna_head_panic", new Vector2(0.50f, 0.04f));
            Texture2D pigtailBackTex = LoadPartTexture("yuna_pigtail_back");
            Texture2D pigtailFarTex = LoadPartTexture("yuna_pigtail_far");
            Texture2D torsoTex = LoadPartTexture("yuna_torso_dress");
            Texture2D armFrontTex = LoadPartTexture("yuna_arm_front");
            Texture2D armBackTex = LoadPartTexture("yuna_arm_back");
            Texture2D legFrontTex = LoadPartTexture("yuna_leg_front");
            Texture2D legBackTex = LoadPartTexture("yuna_leg_back");

            if (headHappySprite == null || torsoTex == null || legFrontTex == null || armFrontTex == null)
            {
                return false;
            }

            if (headScaredSprite == null) headScaredSprite = headHappySprite;
            if (headStrainSprite == null) headStrainSprite = headScaredSprite;
            if (headPanicSprite == null) headPanicSprite = headScaredSprite;

            var rootGo = new GameObject("YunaRig2D");
            rootGo.transform.SetParent(transform, false);
            rigRoot = rootGo.transform;

            var sortingGroup = rootGo.AddComponent<SortingGroup>();
            sortingGroup.sortingOrder = baseSortingOrder;

            // True anatomical waist/hip axis (Y = 0.44 inside the dress waist) so legs pivot from
            // below the waist underneath the skirt rather than from the skirt hem, with soles at Y = 0.
            hipsBone = CreateBone("Bone_Hips", rigRoot, new Vector3(0f, BaseHipsY, 0f));

            legBackBone = CreateBone("Bone_LegBack", hipsBone, new Vector3(0.018f, 0f, 0f));
            legBackMesh = CreateDeformablePart("Part_LegBack", legBackBone, legBackTex ?? legFrontTex,
                new Vector2(0.45f, 0.92f), 0.48f, 1, 4, 16);

            legFrontBone = CreateBone("Bone_LegFront", hipsBone, new Vector3(-0.018f, 0f, 0f));
            legFrontMesh = CreateDeformablePart("Part_LegFront", legFrontBone, legFrontTex,
                new Vector2(0.45f, 0.92f), 0.48f, 2, 4, 16);

            torsoBone = CreateBone("Bone_Torso", hipsBone, new Vector3(0f, -0.17f, 0f));

            torsoMesh = CreateDeformablePart("Part_TorsoDress", torsoBone, torsoTex,
                new Vector2(0.52f, 0.14f), 0.36f, 4, 8, 12);

            // Waist-level upper torso articulation (Bone_Chest at dress waistband y = 0.155 inside Bone_Torso)
            // so shoulders, arms, head and pigtails arch naturally together from the waist on jump impulse.
            chestBone = CreateBone("Bone_Chest", torsoBone, new Vector3(0f, 0.155f, 0f));

            armBackBone = CreateBone("Bone_ArmBack", chestBone, new Vector3(0.115f, 0.120f, 0f));
            armBackMesh = CreateDeformablePart("Part_ArmBack", armBackBone, armBackTex ?? armFrontTex,
                new Vector2(0.50f, 0.84f), 0.30f, 3, 4, 14);

            // Natural neck clearance seated cleanly on the blouse collar (0.155 + 0.149 = 0.304)
            headBone = CreateBone("Bone_Head", chestBone, new Vector3(0.012f, 0.149f, 0f));

            pigtailFarBone = CreateBone("Bone_PigtailFar", headBone, new Vector3(0.155f, 0.285f, 0f));
            pigtailFarMesh = CreateDeformablePart("Part_PigtailFar", pigtailFarBone, pigtailFarTex ?? pigtailBackTex,
                new Vector2(0.24f, 0.84f), 0.31f, 0, 4, 12);

            headRenderer = AttachStaticSprite("Part_Head", headBone, headHappySprite, 0.42f, 6);

            pigtailBackBone = CreateBone("Bone_PigtailBack", headBone, new Vector3(-0.115f, 0.285f, 0f));
            pigtailBackMesh = CreateDeformablePart("Part_PigtailBack", pigtailBackBone, pigtailBackTex,
                new Vector2(0.76f, 0.84f), 0.36f, 7, 4, 12);

            armFrontBone = CreateBone("Bone_ArmFront", chestBone, new Vector3(-0.085f, 0.120f, 0f));
            armFrontMesh = CreateDeformablePart("Part_ArmFront", armFrontBone, armFrontTex,
                new Vector2(0.50f, 0.84f), 0.30f, 8, 4, 14);

            allMeshes = new[]
            {
                pigtailFarMesh,
                legBackMesh,
                legFrontMesh,
                armBackMesh,
                torsoMesh,
                pigtailBackMesh,
                armFrontMesh,
            };

            isBuilt = true;
            return true;
        }

        public void ApplyPose(
            PicnicYunaRigPose targetPose,
            int facing,
            float alpha,
            float deltaTime,
            float extraTiltDeg = 0f,
            float extraBounceY = 0f,
            bool onGround = true)
        {
            if (!isBuilt)
            {
                return;
            }

            if (!wasOnGround && onGround)
            {
                landingSquashTimer = 0.16f;
            }
            wasOnGround = onGround;

            if (landingSquashTimer > 0f && deltaTime > 0f)
            {
                landingSquashTimer = Mathf.Max(0f, landingSquashTimer - deltaTime);
            }

            PicnicYunaRigPose pose = targetPose;
            if (hasSmoothedPose && deltaTime > 0f && deltaTime < 0.25f)
            {
                float blend = 1f - Mathf.Exp(-20f * deltaTime);
                pose = LerpPose(smoothedPose, targetPose, blend);
            }

            smoothedPose = pose;
            hasSmoothedPose = true;

            float landRatio = landingSquashTimer > 0f ? Mathf.Sin((landingSquashTimer / 0.16f) * Mathf.PI) : 0f;
            float landScaleX = 1f + landRatio * 0.10f;
            float landScaleY = 1f - landRatio * 0.10f;

            float dir = facing < 0 ? -1f : 1f;
            rigRoot.localScale = new Vector3(dir * pose.ScaleX * landScaleX, pose.ScaleY * landScaleY, 1f);
            rigRoot.localPosition = new Vector3(0f, extraBounceY, 0f);
            rigRoot.localRotation = Quaternion.Euler(0f, 0f, extraTiltDeg);

            hipsBone.localPosition = new Vector3(0f, BaseHipsY + pose.HipsBobY - landRatio * 0.025f, 0f);
            torsoBone.localRotation = Quaternion.Euler(0f, 0f, pose.TorsoLeanDeg);
            chestBone.localRotation = Quaternion.Euler(0f, 0f, pose.ChestBendDeg * 0.72f);
            headBone.localRotation = Quaternion.Euler(0f, 0f, pose.HeadTiltDeg);
            pigtailBackBone.localRotation = Quaternion.Euler(0f, 0f, pose.BackPigtailDeg);
            pigtailFarBone.localRotation = Quaternion.Euler(0f, 0f, -pose.FrontPigtailDeg * 0.85f);
            legFrontBone.localRotation = Quaternion.Euler(0f, 0f, pose.FrontLegDeg);
            legBackBone.localRotation = Quaternion.Euler(0f, 0f, pose.BackLegDeg);
            armFrontBone.localRotation = Quaternion.Euler(0f, 0f, pose.FrontArmDeg);
            armBackBone.localRotation = Quaternion.Euler(0f, 0f, pose.BackArmDeg);

            // Real 2D mesh skinning for secondary joints (knees, ankles, elbows, flexible torso/skirt, pigtail tips)
            legFrontMesh.DeformThreeBoneChain(kneeV: 0.46f, kneeDeg: pose.FrontKneeDeg, ankleV: 0.21f, ankleDeg: pose.FrontAnkleDeg);
            legBackMesh.DeformThreeBoneChain(kneeV: 0.46f, kneeDeg: pose.BackKneeDeg, ankleV: 0.21f, ankleDeg: pose.BackAnkleDeg);
            armFrontMesh.DeformTwoBoneChain(jointV: 0.48f, bendDeg: pose.FrontElbowDeg);
            armBackMesh.DeformTwoBoneChain(jointV: 0.48f, bendDeg: pose.BackElbowDeg);
            torsoMesh.DeformTorsoAndSkirt(pose.ChestBendDeg, pose.SkirtSwayDeg, pose.SkirtFlare + landRatio * 0.08f);
            pigtailBackMesh.DeformTwoBoneChain(jointV: 0.46f, bendDeg: pose.BackPigtailTipDeg - pose.BackPigtailDeg);
            pigtailFarMesh.DeformTwoBoneChain(jointV: 0.46f, bendDeg: -(pose.FrontPigtailTipDeg - pose.FrontPigtailDeg) * 0.85f);

            if (headRenderer != null)
            {
                headRenderer.sprite = ResolveHeadSprite(pose.Expression);
                headRenderer.color = new Color(1f, 1f, 1f, alpha);
            }

            Color tint = new Color(1f, 1f, 1f, alpha);
            for (int i = 0; i < allMeshes.Length; i++)
            {
                allMeshes[i]?.SetColor(tint);
            }
        }

        private void OnDestroy()
        {
            if (allMeshes == null) return;
            for (int i = 0; i < allMeshes.Length; i++)
            {
                allMeshes[i]?.Dispose();
            }
        }

        private static Transform CreateBone(string name, Transform parent, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go.transform;
        }

        private static SpriteRenderer AttachStaticSprite(
            string name,
            Transform bone,
            Sprite sprite,
            float targetHeightUnits,
            int sortingOrder)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(bone, false);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            if (sprite != null && sprite.bounds.size.y > 0.0001f)
            {
                float s = targetHeightUnits / sprite.bounds.size.y;
                go.transform.localScale = new Vector3(s, s, 1f);
            }
            return sr;
        }

        private static DeformablePartMesh CreateDeformablePart(
            string name,
            Transform bone,
            Texture2D tex,
            Vector2 pivot,
            float targetHeightUnits,
            int sortingOrder,
            int cols,
            int rows)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(bone, false);
            return new DeformablePartMesh(go, tex, pivot, targetHeightUnits, sortingOrder, cols, rows);
        }

        private static Texture2D LoadPartTexture(string partName)
        {
            Texture2D tex = Resources.Load<Texture2D>(ResourceRoot + partName);
            if (tex != null)
            {
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
            }
            return tex;
        }

        private static Sprite LoadPartSprite(string partName, Vector2 pivot)
        {
            Texture2D tex = LoadPartTexture(partName);
            if (tex == null)
            {
                return null;
            }
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), pivot, 100f);
        }

        private static PicnicYunaRigPose LerpPose(PicnicYunaRigPose a, PicnicYunaRigPose b, float t)
        {
            return new PicnicYunaRigPose(
                b.Motion,
                Mathf.Lerp(a.HipsBobY, b.HipsBobY, t),
                Mathf.Lerp(a.TorsoLeanDeg, b.TorsoLeanDeg, t),
                Mathf.Lerp(a.ChestBendDeg, b.ChestBendDeg, t),
                Mathf.Lerp(a.SkirtSwayDeg, b.SkirtSwayDeg, t),
                Mathf.Lerp(a.SkirtFlare, b.SkirtFlare, t),
                Mathf.Lerp(a.HeadTiltDeg, b.HeadTiltDeg, t),
                Mathf.Lerp(a.FrontPigtailDeg, b.FrontPigtailDeg, t),
                Mathf.Lerp(a.FrontPigtailTipDeg, b.FrontPigtailTipDeg, t),
                Mathf.Lerp(a.BackPigtailDeg, b.BackPigtailDeg, t),
                Mathf.Lerp(a.BackPigtailTipDeg, b.BackPigtailTipDeg, t),
                Mathf.Lerp(a.FrontLegDeg, b.FrontLegDeg, t),
                Mathf.Lerp(a.FrontKneeDeg, b.FrontKneeDeg, t),
                Mathf.Lerp(a.FrontAnkleDeg, b.FrontAnkleDeg, t),
                Mathf.Lerp(a.BackLegDeg, b.BackLegDeg, t),
                Mathf.Lerp(a.BackKneeDeg, b.BackKneeDeg, t),
                Mathf.Lerp(a.BackAnkleDeg, b.BackAnkleDeg, t),
                Mathf.Lerp(a.FrontArmDeg, b.FrontArmDeg, t),
                Mathf.Lerp(a.FrontElbowDeg, b.FrontElbowDeg, t),
                Mathf.Lerp(a.BackArmDeg, b.BackArmDeg, t),
                Mathf.Lerp(a.BackElbowDeg, b.BackElbowDeg, t),
                Mathf.Lerp(a.ScaleX, b.ScaleX, t),
                Mathf.Lerp(a.ScaleY, b.ScaleY, t),
                b.Expression);
        }

        private Sprite ResolveHeadSprite(PicnicYunaExpression expression)
        {
            switch (expression)
            {
                case PicnicYunaExpression.Strain:
                    return headStrainSprite ?? headScaredSprite ?? headHappySprite;
                case PicnicYunaExpression.Scared:
                    return headScaredSprite ?? headHappySprite;
                case PicnicYunaExpression.Panic:
                    return headPanicSprite ?? headScaredSprite ?? headHappySprite;
                case PicnicYunaExpression.Happy:
                default:
                    return headHappySprite;
            }
        }

        /// <summary>
        /// Subdivided 2D quad mesh with smooth Linear Blend Skinning (LBS) for limbs, skirt, and hair.
        /// </summary>
        private sealed class DeformablePartMesh
        {
            private readonly Mesh mesh;
            private readonly Material material;
            private readonly Vector3[] baseVertices;
            private readonly Vector3[] workVertices;
            private readonly float[] vertexU;
            private readonly float[] vertexV;
            private readonly float width;
            private readonly float height;
            private readonly Vector2 pivot;

            public DeformablePartMesh(
                GameObject go,
                Texture2D texture,
                Vector2 pivot,
                float targetHeight,
                int sortingOrder,
                int cols,
                int rows)
            {
                this.pivot = pivot;
                height = targetHeight;
                width = texture != null && texture.height > 0
                    ? targetHeight * (texture.width / (float)texture.height)
                    : targetHeight;

                int vxCount = cols + 1;
                int vyCount = rows + 1;
                int count = vxCount * vyCount;

                baseVertices = new Vector3[count];
                workVertices = new Vector3[count];
                vertexU = new float[count];
                vertexV = new float[count];
                var uvs = new Vector2[count];
                var tris = new int[cols * rows * 6];

                int idx = 0;
                for (int y = 0; y <= rows; y++)
                {
                    float v = y / (float)rows;
                    float py = (v - pivot.y) * height;
                    for (int x = 0; x <= cols; x++)
                    {
                        float u = x / (float)cols;
                        float px = (u - pivot.x) * width;
                        baseVertices[idx] = new Vector3(px, py, 0f);
                        workVertices[idx] = baseVertices[idx];
                        vertexU[idx] = u;
                        vertexV[idx] = v;
                        uvs[idx] = new Vector2(u, v);
                        idx++;
                    }
                }

                int ti = 0;
                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        int a = y * vxCount + x;
                        int b = a + 1;
                        int c = a + vxCount;
                        int d = c + 1;
                        tris[ti++] = a; tris[ti++] = c; tris[ti++] = b;
                        tris[ti++] = b; tris[ti++] = c; tris[ti++] = d;
                    }
                }

                mesh = new Mesh { name = go.name + "_Mesh" };
                mesh.MarkDynamic();
                mesh.vertices = workVertices;
                mesh.uv = uvs;
                mesh.triangles = tris;
                mesh.RecalculateBounds();

                var mf = go.GetComponent<MeshFilter>();
                mf.sharedMesh = mesh;

                material = new Material(Shader.Find("Sprites/Default"))
                {
                    mainTexture = texture,
                    color = Color.white,
                };
                var mr = go.GetComponent<MeshRenderer>();
                mr.sharedMaterial = material;
                mr.sortingOrder = sortingOrder;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }

            public void DeformTwoBoneChain(float jointV, float bendDeg)
            {
                float jointY = (jointV - pivot.y) * height;
                float rad = bendDeg * Mathf.Deg2Rad;
                for (int i = 0; i < baseVertices.Length; i++)
                {
                    Vector3 p = baseVertices[i];
                    float w = SmoothStep(jointV + 0.11f, jointV - 0.11f, vertexV[i]);
                    if (w > 0.0001f)
                    {
                        p = RotateAround2D(p, 0f, jointY, rad * w);
                    }
                    workVertices[i] = p;
                }
                Commit();
            }

            public void DeformThreeBoneChain(float kneeV, float kneeDeg, float ankleV, float ankleDeg)
            {
                float kneeY = (kneeV - pivot.y) * height;
                float ankleY = (ankleV - pivot.y) * height;
                float kneeRad = kneeDeg * Mathf.Deg2Rad;
                float ankleRad = ankleDeg * Mathf.Deg2Rad;

                // Transformed ankle pivot after knee rotation
                Vector3 anklePivotRot = RotateAround2D(new Vector3(0f, ankleY, 0f), 0f, kneeY, kneeRad);

                for (int i = 0; i < baseVertices.Length; i++)
                {
                    Vector3 p = baseVertices[i];
                    float wKnee = SmoothStep(kneeV + 0.10f, kneeV - 0.10f, vertexV[i]);
                    if (wKnee > 0.0001f)
                    {
                        p = RotateAround2D(p, 0f, kneeY, kneeRad * wKnee);
                    }

                    float wAnkle = SmoothStep(ankleV + 0.06f, ankleV - 0.06f, vertexV[i]);
                    if (wAnkle > 0.0001f)
                    {
                        p = RotateAround2D(p, anklePivotRot.x, anklePivotRot.y, ankleRad * wAnkle);
                    }
                    workVertices[i] = p;
                }
                Commit();
            }

            public void DeformTorsoAndSkirt(float chestBendDeg, float skirtSwayDeg, float skirtFlare)
            {
                float waistV = 0.56f;
                float waistY = (waistV - pivot.y) * height;
                float chestRad = chestBendDeg * Mathf.Deg2Rad;
                float swayRad = skirtSwayDeg * Mathf.Deg2Rad;

                for (int i = 0; i < baseVertices.Length; i++)
                {
                    Vector3 p = baseVertices[i];
                    float v = vertexV[i];

                    // Upper chest flexes gently above the waist
                    float wChest = SmoothStep(waistV - 0.06f, waistV + 0.18f, v);
                    if (wChest > 0.0001f)
                    {
                        p = RotateAround2D(p, 0f, waistY, chestRad * wChest);
                    }

                    // Lower skirt ruffles sway and flare organically below the waist
                    float wSkirt = SmoothStep(waistV + 0.02f, 0.04f, v);
                    if (wSkirt > 0.0001f)
                    {
                        p.x *= 1f + skirtFlare * wSkirt;
                        p = RotateAround2D(p, 0f, waistY, swayRad * wSkirt);
                    }

                    workVertices[i] = p;
                }
                Commit();
            }

            public void SetColor(Color color)
            {
                if (material != null)
                {
                    material.color = color;
                }
            }

            public void Dispose()
            {
                if (mesh != null) UnityEngine.Object.DestroyImmediate(mesh);
                if (material != null) UnityEngine.Object.DestroyImmediate(material);
            }

            private void Commit()
            {
                mesh.vertices = workVertices;
                mesh.RecalculateBounds();
            }

            private static Vector3 RotateAround2D(Vector3 p, float cx, float cy, float rad)
            {
                float dx = p.x - cx;
                float dy = p.y - cy;
                float c = Mathf.Cos(rad);
                float s = Mathf.Sin(rad);
                return new Vector3(cx + dx * c - dy * s, cy + dx * s + dy * c, p.z);
            }

            private static float SmoothStep(float edge0, float edge1, float x)
            {
                float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
                return t * t * (3f - 2f * t);
            }
        }
    }
}
