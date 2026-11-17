using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using YunaLegend.ToyRoom.Runtime;

namespace YunaLegend.Editor.ToyRoom
{
    /// <summary>
    /// Constroi o esqueleto (17 ossos) e o SkinnedMeshRenderer sobre a malha importada Yuna_ToyRoom.obj,
    /// garantindo que a saia acompanhe apenas quadril/coluna (sem grudar nas pernas) e que
    /// pernas, joelhos, pes, bracos, maos, quadril e cabeca se articulem naturalmente.
    /// </summary>
    public static class ToyRoomYunaRigBuilder
    {
        private const string SkinnedMeshAssetPath = ToyRoomYunaSculptor.ModelsFolder + "/GeneratedMeshes/Yuna_SkinnedMesh.asset";

        public static void AttachSkinnedRig(
            GameObject importedInstance,
            Transform bodyRoot,
            out YunaToyRoomAnimator.RigBones rigBones,
            out Transform legLeftPivot,
            out Transform legRightPivot,
            out Transform armLeftPivot,
            out Transform armRightPivot,
            out Transform ponytailPivot)
        {
            rigBones = default;
            var sourceFilter = importedInstance.GetComponentInChildren<MeshFilter>(true);
            var sourceRenderer = importedInstance.GetComponentInChildren<MeshRenderer>(true);
            Debug.Log($"[ToyRoomYunaRigBuilder] AttachSkinnedRig: filter={sourceFilter != null}, mesh={(sourceFilter != null && sourceFilter.sharedMesh != null)}, rend={sourceRenderer != null}, children={importedInstance.transform.childCount}");
            if (sourceFilter == null || sourceFilter.sharedMesh == null || sourceRenderer == null)
            {
                legLeftPivot = FindOrCreate(bodyRoot, "Leg_L_Pivot", new Vector3(-0.08f, 0.43f, 0f));
                legRightPivot = FindOrCreate(bodyRoot, "Leg_R_Pivot", new Vector3(0.08f, 0.43f, 0f));
                armLeftPivot = FindOrCreate(bodyRoot, "Arm_L_Pivot", new Vector3(-0.14f, 0.79f, 0f));
                armRightPivot = FindOrCreate(bodyRoot, "Arm_R_Pivot", new Vector3(0.14f, 0.79f, 0f));
                ponytailPivot = FindOrCreate(bodyRoot, "Ponytail_Pivot", new Vector3(0f, 1.15f, -0.14f));
                return;
            }

            var rigRoot = new GameObject("Yuna_RigRoot").transform;
            rigRoot.SetParent(bodyRoot, false);

            var bones = CreateSkeletonHierarchy(rigRoot, out rigBones);
            var skinnedMesh = BuildWeightedMesh(sourceFilter.sharedMesh, bones, sourceFilter.transform, bodyRoot);

            var smrGo = new GameObject("Yuna_SkinnedMesh");
            smrGo.transform.SetParent(bodyRoot, false);
            var smr = smrGo.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = skinnedMesh;
            smr.sharedMaterial = sourceRenderer.sharedMaterial;
            smr.bones = bones;
            smr.rootBone = rigBones.Hips;
            smr.updateWhenOffscreen = true;

            Object.DestroyImmediate(importedInstance);

            legLeftPivot = rigBones.ThighL;
            legRightPivot = rigBones.ThighR;
            armLeftPivot = rigBones.UpperArmL;
            armRightPivot = rigBones.UpperArmR;
            ponytailPivot = rigBones.Head;
        }

        private static Transform[] CreateSkeletonHierarchy(Transform rigRoot, out YunaToyRoomAnimator.RigBones rig)
        {
            var hips = CreateBone("Hips", rigRoot, new Vector3(0f, 0.56f, 0f));
            var spine = CreateBone("Spine", hips, new Vector3(0f, 0.11f, 0f));
            var chest = CreateBone("Chest", spine, new Vector3(0f, 0.10f, 0f));
            var neck = CreateBone("Neck", chest, new Vector3(0f, 0.10f, 0f));
            var head = CreateBone("Head", neck, new Vector3(0f, 0.07f, 0f));

            var upperArmL = CreateBone("UpperArm_L", chest, new Vector3(-0.145f, 0.02f, 0f));
            var forearmL = CreateBone("Forearm_L", upperArmL, new Vector3(-0.100f, -0.12f, 0f));
            var handL = CreateBone("Hand_L", forearmL, new Vector3(-0.090f, -0.10f, 0f));

            var upperArmR = CreateBone("UpperArm_R", chest, new Vector3(0.145f, 0.02f, 0f));
            var forearmR = CreateBone("Forearm_R", upperArmR, new Vector3(0.100f, -0.12f, 0f));
            var handR = CreateBone("Hand_R", forearmR, new Vector3(0.090f, -0.10f, 0f));

            var thighL = CreateBone("Thigh_L", hips, new Vector3(-0.080f, -0.13f, 0f));
            var shinL = CreateBone("Shin_L", thighL, new Vector3(0f, -0.19f, 0f));
            var footL = CreateBone("Foot_L", shinL, new Vector3(0f, -0.18f, 0.02f));

            var thighR = CreateBone("Thigh_R", hips, new Vector3(0.075f, -0.13f, 0f));
            var shinR = CreateBone("Shin_R", thighR, new Vector3(0f, -0.19f, 0f));
            var footR = CreateBone("Foot_R", shinR, new Vector3(0f, -0.18f, 0.02f));

            rig = new YunaToyRoomAnimator.RigBones
            {
                Hips = hips,
                Spine = spine,
                Chest = chest,
                Neck = neck,
                Head = head,
                UpperArmL = upperArmL,
                ForearmL = forearmL,
                HandL = handL,
                UpperArmR = upperArmR,
                ForearmR = forearmR,
                HandR = handR,
                ThighL = thighL,
                ShinL = shinL,
                FootL = footL,
                ThighR = thighR,
                ShinR = shinR,
                FootR = footR
            };

            return new[]
            {
                hips, spine, chest, neck, head,
                upperArmL, forearmL, handL,
                upperArmR, forearmR, handR,
                thighL, shinL, footL,
                thighR, shinR, footR
            };
        }

        private static Mesh BuildWeightedMesh(Mesh sourceMesh, Transform[] bones, Transform meshSpace, Transform bodyRoot)
        {
            Directory.CreateDirectory(ToyRoomYunaSculptor.ModelsFolder + "/GeneratedMeshes");

            var mesh = Object.Instantiate(sourceMesh);
            mesh.name = "Yuna_SkinnedMesh";

            Vector3[] verts = mesh.vertices;
            var weights = new BoneWeight[verts.Length];

            // Posicoes locais em bodyRoot (0..1.32m), independente da posicao/rotacao/escala do Player na cena
            Vector3[] bonePos = new Vector3[bones.Length];
            for (int b = 0; b < bones.Length; b++)
            {
                bonePos[b] = bodyRoot.InverseTransformPoint(bones[b].position);
            }

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 p = bodyRoot.InverseTransformPoint(meshSpace.TransformPoint(verts[i]));
                weights[i] = ComputeVertexWeight(p, bonePos);
            }

            mesh.boneWeights = weights;

            var bindPoses = new Matrix4x4[bones.Length];
            for (int b = 0; b < bones.Length; b++)
            {
                bindPoses[b] = bones[b].worldToLocalMatrix * meshSpace.localToWorldMatrix;
            }

            mesh.bindposes = bindPoses;
            mesh.RecalculateBounds();

            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(SkinnedMeshAssetPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, SkinnedMeshAssetPath);
                return mesh;
            }

            existing.Clear();
            existing.vertices = mesh.vertices;
            existing.normals = mesh.normals;
            existing.uv = mesh.uv;
            existing.triangles = mesh.triangles;
            existing.boneWeights = mesh.boneWeights;
            existing.bindposes = mesh.bindposes;
            existing.RecalculateBounds();
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(mesh);
            return existing;
        }

        private static BoneWeight ComputeVertexWeight(Vector3 p, Vector3[] bonePos)
        {
            // Indices: 0 Hips, 1 Spine, 2 Chest, 3 Neck, 4 Head
            // 5..7 UpperArmL/ForearmL/HandL, 8..10 UpperArmR/ForearmR/HandR
            // 11..13 ThighL/ShinL/FootL, 14..16 ThighR/ShinR/FootR

            // 1. Cabeca e Pescoco
            if (p.y >= 0.90f)
            {
                return SingleWeight(4);
            }

            if (p.y >= 0.85f && Mathf.Abs(p.x) < 0.12f)
            {
                float t = Mathf.InverseLerp(0.85f, 0.90f, p.y);
                return BlendWeight(3, 1f - t, 4, t);
            }

            // 2. Bracos e Maos (fora do cone do tronco/saia, sem capturar vertices da saia)
            float skirtRadius = Mathf.Lerp(0.310f, 0.150f, Mathf.InverseLerp(0.35f, 0.72f, p.y));
            bool isArmRegion = p.y >= 0.48f && p.y < 0.86f && Mathf.Abs(p.x) > Mathf.Max(0.155f, skirtRadius + 0.018f);
            if (isArmRegion)
            {
                bool isLeft = p.x < 0f;
                int ua = isLeft ? 5 : 8;
                int fa = isLeft ? 6 : 9;
                int ha = isLeft ? 7 : 10;

                float ax = Mathf.Abs(p.x);
                if (ax < 0.175f && p.y > 0.74f)
                {
                    float t = Mathf.InverseLerp(0.145f, 0.175f, ax);
                    return BlendWeight(2, 1f - t, ua, t);
                }

                float dUpper = DistanceToSegment(p, bonePos[ua], bonePos[fa]);
                float dFore = DistanceToSegment(p, bonePos[fa], bonePos[ha]);
                float dHand = Vector3.Distance(p, bonePos[ha] + new Vector3(isLeft ? -0.04f : 0.04f, -0.03f, 0f));

                if (ax > 0.30f && dHand < dFore)
                {
                    float t = Mathf.InverseLerp(0.30f, 0.35f, ax);
                    return BlendWeight(fa, 1f - t, ha, t);
                }

                float wU = 1f / (dUpper * dUpper + 0.0028f);
                float wF = 1f / (dFore * dFore + 0.0028f);
                float sum = wU + wF;
                return BlendWeight(ua, wU / sum, fa, wF / sum);
            }

            // 3. Pernas abaixo da saia — isolando a barra da saia (que fica fora dos cilindros das pernas)!
            float legCenterX = p.x < 0f ? -0.075f : 0.075f;
            float dxLeg = p.x - legCenterX;
            float dzLeg = p.z;
            float distToLegAxis = Mathf.Sqrt(dxLeg * dxLeg + dzLeg * dzLeg);
            bool isLegVertex = p.y < 0.255f || (p.y < 0.375f && distToLegAxis < 0.082f);

            if (isLegVertex)
            {
                bool isLeft = p.x < 0f;
                int thigh = isLeft ? 11 : 14;
                int shin = isLeft ? 12 : 15;
                int foot = isLeft ? 13 : 16;

                if (p.y < 0.085f)
                {
                    float t = Mathf.InverseLerp(0.055f, 0.085f, p.y);
                    return BlendWeight(foot, 1f - t, shin, t);
                }

                if (p.y < 0.25f)
                {
                    float t = Mathf.InverseLerp(0.21f, 0.25f, p.y);
                    return BlendWeight(shin, 1f - t, thigh, t);
                }

                return SingleWeight(thigh);
            }

            // 4. Saia (y entre 0.365 e 0.72): acompanha APENAS Hips e Spine (nunca pernas!)
            if (p.y < 0.72f)
            {
                float t = Mathf.InverseLerp(0.54f, 0.72f, p.y);
                return BlendWeight(0, 1f - t, 1, t);
            }

            // 5. Torso superior (y entre 0.72 e 0.86)
            float chestT = Mathf.InverseLerp(0.72f, 0.80f, p.y);
            if (p.y < 0.80f)
            {
                return BlendWeight(1, 1f - chestT, 2, chestT);
            }

            float neckT = Mathf.InverseLerp(0.80f, 0.86f, p.y);
            return BlendWeight(2, 1f - neckT, 3, neckT);
        }

        private static BoneWeight SingleWeight(int boneIndex)
        {
            return new BoneWeight { boneIndex0 = boneIndex, weight0 = 1f };
        }

        private static BoneWeight BlendWeight(int b0, float w0, int b1, float w1)
        {
            float sum = w0 + w1;
            if (sum <= 0.0001f)
            {
                return SingleWeight(b0);
            }

            return new BoneWeight
            {
                boneIndex0 = b0,
                weight0 = w0 / sum,
                boneIndex1 = b1,
                weight1 = w1 / sum
            };
        }

        private static float DistanceToSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 1e-6f)
            {
                return Vector3.Distance(p, a);
            }

            float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / lenSq);
            return Vector3.Distance(p, a + ab * t);
        }

        private static Transform CreateBone(string name, Transform parent, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go.transform;
        }

        private static Transform FindOrCreate(Transform parent, string name, Vector3 localPos)
        {
            var found = parent.Find(name);
            return found != null ? found : CreateBone(name, parent, localPos);
        }
    }
}
