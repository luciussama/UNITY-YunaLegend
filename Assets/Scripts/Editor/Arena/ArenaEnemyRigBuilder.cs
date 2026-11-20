using System.IO;
using UnityEditor;
using UnityEngine;
using YunaLegend.Arena.Core;
using YunaLegend.Arena.Runtime;

namespace YunaLegend.Editor.Arena
{
    /// <summary>
    /// Constroi esqueletos articulados (SkinnedMeshRenderer) sob medida para os 3 chefes
    /// Hunyuan3D da Arena: Robo (quadril, passos, bracos/luvas de RocketPunch),
    /// Dinossauro (passos, cauda 3 segmentos, espinha/pescoco arqueavel, mandibula) e
    /// Urso (quadril, joelhos, pes, cotovelos, bracos, cabeca com olhar fixo).
    /// </summary>
    public static class ArenaEnemyRigBuilder
    {
        private const string GeneratedMeshesDir = "Assets/Art/Arena/Models/GeneratedMeshes";

        public static GameObject BuildRiggedEnemy(
            string name,
            ArenaEnemyId enemyId,
            Transform parent,
            string objPath,
            Material material,
            float targetHeight)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            var sourceMesh = AssetDatabase.LoadAssetAtPath<Mesh>(objPath);
            if (sourceMesh == null)
            {
                AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);
                sourceMesh = AssetDatabase.LoadAssetAtPath<Mesh>(objPath);
            }

            if (sourceMesh == null)
            {
                Debug.LogWarning($"[ArenaEnemyRigBuilder] Malha nao encontrada: {objPath}");
                return go;
            }

            // Prepara copia da malha (para o Dinossauro, alinha -43 deg em Y para o eixo cauda->focinho apontar 100% para +Z)
            Mesh workingMesh = Object.Instantiate(sourceMesh);
            if (enemyId == ArenaEnemyId.Dinosaur)
            {
                PreRotateMeshY(workingMesh, -43f);
            }

            Bounds b = workingMesh.bounds;
            float h = Mathf.Max(0.001f, b.size.y);
            float scale = targetHeight / h;

            var rigRoot = new GameObject("RigScaleRoot").transform;
            rigRoot.SetParent(go.transform, false);
            rigRoot.localScale = Vector3.one * scale;
            rigRoot.localPosition = new Vector3(-b.center.x * scale, -b.min.y * scale, -b.center.z * scale);

            Transform[] bones;
            ArenaEnemyRigAnimator.RigBones rig;
            string meshAssetPath;

            switch (enemyId)
            {
                case ArenaEnemyId.Robot:
                    bones = CreateRobotSkeleton(rigRoot, b, out rig);
                    meshAssetPath = $"{GeneratedMeshesDir}/Robot_SkinnedMesh.asset";
                    break;
                case ArenaEnemyId.Dinosaur:
                    bones = CreateDinosaurSkeleton(rigRoot, b, out rig);
                    meshAssetPath = $"{GeneratedMeshesDir}/Dinosaur_SkinnedMesh.asset";
                    break;
                default:
                    bones = CreateBearSkeleton(rigRoot, b, out rig);
                    meshAssetPath = $"{GeneratedMeshesDir}/Bear_SkinnedMesh.asset";
                    break;
            }

            Mesh skinnedMesh = BuildWeightedMesh(workingMesh, enemyId, b, bones, rigRoot, meshAssetPath);
            Object.DestroyImmediate(workingMesh);

            var smrGo = new GameObject($"{name}_SkinnedMesh");
            smrGo.transform.SetParent(rigRoot, false);
            var smr = smrGo.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = skinnedMesh;
            smr.sharedMaterial = material;
            smr.bones = bones;
            smr.rootBone = rig.Hips;
            smr.updateWhenOffscreen = true;
            smr.forceMatrixRecalculationPerRender = true;

            var animator = go.AddComponent<ArenaEnemyRigAnimator>();
            animator.Configure(enemyId, rig);

            return go;
        }

        private static Transform[] CreateRobotSkeleton(Transform root, Bounds b, out ArenaEnemyRigAnimator.RigBones rig)
        {
            rig = default;
            float minY = b.min.y;
            float h = b.size.y;
            float cx = b.center.x;
            float cz = b.center.z;

            // Indices: 0 Hips, 1 Chest, 2 Head, 3 UpperArmL, 4 ForearmL, 5 HandL,
            //          6 UpperArmR, 7 ForearmR, 8 HandR, 9 ThighL, 10 FootL, 11 ThighR, 12 FootR
            var hips = CreateBone("Hips", root, new Vector3(cx, minY + h * 0.30f, cz));
            var chest = CreateBone("Chest", hips, new Vector3(0f, h * 0.14f, 0f));
            var head = CreateBone("Head", chest, new Vector3(0f, h * 0.24f, 0f));

            var upperArmL = CreateBone("UpperArm_L", chest, new Vector3(-b.extents.x * 0.56f, h * 0.14f, 0f));
            var forearmL = CreateBone("Forearm_L", upperArmL, new Vector3(-b.extents.x * 0.16f, -h * 0.16f, 0f));
            var handL = CreateBone("Hand_L", forearmL, new Vector3(-b.extents.x * 0.08f, -h * 0.14f, 0f));

            var upperArmR = CreateBone("UpperArm_R", chest, new Vector3(b.extents.x * 0.56f, h * 0.14f, 0f));
            var forearmR = CreateBone("Forearm_R", upperArmR, new Vector3(b.extents.x * 0.16f, -h * 0.16f, 0f));
            var handR = CreateBone("Hand_R", forearmR, new Vector3(b.extents.x * 0.08f, -h * 0.14f, 0f));

            var thighL = CreateBone("Thigh_L", hips, new Vector3(-b.extents.x * 0.24f, -h * 0.04f, 0f));
            var footL = CreateBone("Foot_L", thighL, new Vector3(0f, -h * 0.18f, 0f));

            var thighR = CreateBone("Thigh_R", hips, new Vector3(b.extents.x * 0.24f, -h * 0.04f, 0f));
            var footR = CreateBone("Foot_R", thighR, new Vector3(0f, -h * 0.18f, 0f));

            rig.Hips = hips;
            rig.Chest = chest;
            rig.Head = head;
            rig.UpperArmL = upperArmL;
            rig.ForearmL = forearmL;
            rig.HandL = handL;
            rig.UpperArmR = upperArmR;
            rig.ForearmR = forearmR;
            rig.HandR = handR;
            rig.ThighL = thighL;
            rig.FootL = footL;
            rig.ThighR = thighR;
            rig.FootR = footR;

            return new[]
            {
                hips, chest, head,
                upperArmL, forearmL, handL,
                upperArmR, forearmR, handR,
                thighL, footL, thighR, footR
            };
        }

        private static Transform[] CreateDinosaurSkeleton(Transform root, Bounds b, out ArenaEnemyRigAnimator.RigBones rig)
        {
            rig = default;
            float minY = b.min.y;
            float h = b.size.y;
            // Apos pre-rotacao de -43 deg em Y, o eixo cauda->focinho aponta 100% para +Z e o tronco fica centrado em X ~ +0.05
            float bodyCx = b.center.x;
            float bodyCz = 0.08f;

            // Indices: 0 Hips, 1 Spine, 2 Neck, 3 Head, 4 Jaw,
            //          5 ThighL, 6 ThighR, 7 Tail1, 8 Tail2, 9 Tail3
            var hips = CreateBone("Hips", root, new Vector3(bodyCx, minY + h * 0.28f, bodyCz));
            var spine = CreateBone("Spine", hips, new Vector3(0f, h * 0.16f, 0.03f));
            var neck = CreateBone("Neck", spine, new Vector3(0f, h * 0.14f, 0.04f));
            var head = CreateBone("Head", neck, new Vector3(0f, h * 0.10f, 0.04f));
            var jaw = CreateBone("Jaw", head, new Vector3(0f, -h * 0.06f, 0.08f));

            var thighL = CreateBone("Thigh_L", hips, new Vector3(-0.14f, -h * 0.06f, 0f));
            var thighR = CreateBone("Thigh_R", hips, new Vector3(0.14f, -h * 0.06f, 0f));

            // Cauda estritamente atras no eixo -Z
            var tail1 = CreateBone("Tail_1", hips, new Vector3(0f, -h * 0.06f, -0.22f));
            var tail2 = CreateBone("Tail_2", tail1, new Vector3(0f, -h * 0.02f, -0.15f));
            var tail3 = CreateBone("Tail_3", tail2, new Vector3(0f, 0.01f, -0.14f));

            rig.Hips = hips;
            rig.Spine = spine;
            rig.Neck = neck;
            rig.Head = head;
            rig.Jaw = jaw;
            rig.ThighL = thighL;
            rig.ThighR = thighR;
            rig.Tail1 = tail1;
            rig.Tail2 = tail2;
            rig.Tail3 = tail3;

            return new[]
            {
                hips, spine, neck, head, jaw,
                thighL, thighR, tail1, tail2, tail3
            };
        }

        private static Transform[] CreateBearSkeleton(Transform root, Bounds b, out ArenaEnemyRigAnimator.RigBones rig)
        {
            rig = default;
            float minY = b.min.y;
            float h = b.size.y;
            float cx = b.center.x;
            float cz = b.center.z + 0.04f;

            // Indices: 0 Hips, 1 Spine, 2 Chest, 3 Head,
            //          4 UpperArmL, 5 ForearmL, 6 HandL, 7 UpperArmR, 8 ForearmR, 9 HandR,
            //          10 ThighL, 11 ShinL, 12 FootL, 13 ThighR, 14 ShinR, 15 FootR
            // Pivo da cabeca posicionado exatamente na constricao do pescoco (ny = 0.60, abaixo do queixo/boca)
            var hips = CreateBone("Hips", root, new Vector3(cx, minY + h * 0.30f, cz));
            var spine = CreateBone("Spine", hips, new Vector3(0f, h * 0.12f, 0f));
            var chest = CreateBone("Chest", spine, new Vector3(0f, h * 0.12f, 0f));
            var head = CreateBone("Head", chest, new Vector3(0f, h * 0.06f, 0.02f));

            var upperArmL = CreateBone("UpperArm_L", chest, new Vector3(-0.24f, -0.02f, 0f));
            var forearmL = CreateBone("Forearm_L", upperArmL, new Vector3(-0.08f, -h * 0.10f, 0.02f));
            var handL = CreateBone("Hand_L", forearmL, new Vector3(-0.05f, -h * 0.08f, 0.02f));

            var upperArmR = CreateBone("UpperArm_R", chest, new Vector3(0.24f, -0.02f, 0f));
            var forearmR = CreateBone("Forearm_R", upperArmR, new Vector3(0.08f, -h * 0.10f, 0.02f));
            var handR = CreateBone("Hand_R", forearmR, new Vector3(0.05f, -h * 0.08f, 0.02f));

            var thighL = CreateBone("Thigh_L", hips, new Vector3(-0.12f, -h * 0.04f, 0f));
            var shinL = CreateBone("Shin_L", thighL, new Vector3(0f, -h * 0.12f, 0f));
            var footL = CreateBone("Foot_L", shinL, new Vector3(0f, -h * 0.11f, 0.02f));

            var thighR = CreateBone("Thigh_R", hips, new Vector3(0.12f, -h * 0.04f, 0f));
            var shinR = CreateBone("Shin_R", thighR, new Vector3(0f, -h * 0.12f, 0f));
            var footR = CreateBone("Foot_R", shinR, new Vector3(0f, -h * 0.11f, 0.02f));

            rig.Hips = hips;
            rig.Spine = spine;
            rig.Chest = chest;
            rig.Head = head;
            rig.UpperArmL = upperArmL;
            rig.ForearmL = forearmL;
            rig.HandL = handL;
            rig.UpperArmR = upperArmR;
            rig.ForearmR = forearmR;
            rig.HandR = handR;
            rig.ThighL = thighL;
            rig.ShinL = shinL;
            rig.FootL = footL;
            rig.ThighR = thighR;
            rig.ShinR = shinR;
            rig.FootR = footR;

            return new[]
            {
                hips, spine, chest, head,
                upperArmL, forearmL, handL,
                upperArmR, forearmR, handR,
                thighL, shinL, footL,
                thighR, shinR, footR
            };
        }

        private static Mesh BuildWeightedMesh(
            Mesh sourceMesh,
            ArenaEnemyId enemyId,
            Bounds b,
            Transform[] bones,
            Transform rigRoot,
            string assetPath)
        {
            Directory.CreateDirectory(GeneratedMeshesDir);

            var mesh = Object.Instantiate(sourceMesh);
            mesh.name = Path.GetFileNameWithoutExtension(assetPath);

            Vector3[] verts = mesh.vertices;
            var weights = new BoneWeight[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                weights[i] = enemyId switch
                {
                    ArenaEnemyId.Robot => ComputeRobotWeight(verts[i], b),
                    ArenaEnemyId.Dinosaur => ComputeDinosaurWeight(verts[i], b),
                    _ => ComputeBearWeight(verts[i], b)
                };
            }

            mesh.boneWeights = weights;

            var bindPoses = new Matrix4x4[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                bindPoses[i] = bones[i].worldToLocalMatrix * rigRoot.localToWorldMatrix;
            }

            mesh.bindposes = bindPoses;
            mesh.RecalculateBounds();

            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, assetPath);
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

        private static BoneWeight ComputeRobotWeight(Vector3 p, Bounds b)
        {
            float ny = Mathf.Clamp01((p.y - b.min.y) / Mathf.Max(0.001f, b.size.y));
            float ax = Mathf.Abs(p.x - b.center.x);

            // Cabeca + visor + antena
            if (ny >= 0.67f)
            {
                float t = Mathf.InverseLerp(0.67f, 0.72f, ny);
                return BlendWeight(1, 1f - t, 2, t);
            }

            // Bracos de mola + luvas de boxe vermelhas (fora do cilindro azul do tronco/quadril |x| <= 0.42)
            float armThresholdX = ny < 0.26f ? 0.425f : 0.445f;
            if (ny >= 0.11f && ny < 0.62f && ax > armThresholdX)
            {
                bool isLeft = p.x < b.center.x;
                int ua = isLeft ? 3 : 6;
                int fa = isLeft ? 4 : 7;
                int ha = isLeft ? 5 : 8;

                if (ny < 0.31f)
                {
                    // Toda a luva vermelha move-se como bloco rigido unico no RocketPunch (zero esticamento!)
                    return SingleWeight(ha);
                }

                if (ny < 0.46f)
                {
                    float t = Mathf.InverseLerp(0.38f, 0.46f, ny);
                    return BlendWeight(fa, 1f - t, ua, t);
                }

                // Transicao suave apenas na raiz do ombro (ny >= 0.46)
                float shoulderT = Mathf.InverseLerp(0.44f, 0.51f, ax);
                return BlendWeight(1, 1f - shoulderT, ua, shoulderT);
            }

            // Pernas e botas abaixo do quadril
            if (ny < 0.28f)
            {
                bool isLeft = p.x < b.center.x;
                int thigh = isLeft ? 9 : 11;
                int foot = isLeft ? 10 : 12;

                if (ny < 0.11f)
                {
                    float t = Mathf.InverseLerp(0.07f, 0.11f, ny);
                    return BlendWeight(foot, 1f - t, thigh, t);
                }

                float hipT = Mathf.InverseLerp(0.22f, 0.28f, ny);
                return BlendWeight(thigh, 1f - hipT, 0, hipT);
            }

            // Quadril -> Torso
            if (ny < 0.39f)
            {
                float t = Mathf.InverseLerp(0.32f, 0.39f, ny);
                return BlendWeight(0, 1f - t, 1, t);
            }

            return SingleWeight(1);
        }

        private static void PreRotateMeshY(Mesh mesh, float yawDegrees)
        {
            Quaternion rot = Quaternion.Euler(0f, yawDegrees, 0f);
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = rot * verts[i];
            }

            mesh.vertices = verts;

            Vector3[] norms = mesh.normals;
            if (norms != null && norms.Length == verts.Length)
            {
                for (int i = 0; i < norms.Length; i++)
                {
                    norms[i] = (rot * norms[i]).normalized;
                }

                mesh.normals = norms;
            }

            mesh.RecalculateBounds();
        }

        private static BoneWeight ComputeDinosaurWeight(Vector3 p, Bounds b)
        {
            float ny = Mathf.Clamp01((p.y - b.min.y) / Mathf.Max(0.001f, b.size.y));

            // Cauda (apos pre-rotacao -43 deg, a cauda fica estritamente em Z < -0.20)
            if (ny < 0.34f && p.z < -0.20f)
            {
                float tailDepth = -p.z - 0.20f;
                if (tailDepth < 0.08f)
                {
                    float tHip = Mathf.InverseLerp(0f, 0.08f, tailDepth);
                    return BlendWeight(0, 1f - tHip, 7, tHip);
                }

                if (tailDepth > 0.26f) return SingleWeight(9); // Tail_3 (ponta)
                if (tailDepth > 0.16f)
                {
                    float t = Mathf.InverseLerp(0.16f, 0.26f, tailDepth);
                    return BlendWeight(8, 1f - t, 9, t);
                }

                float tBase = Mathf.InverseLerp(0.08f, 0.16f, tailDepth);
                return BlendWeight(7, 1f - tBase, 8, tBase);
            }

            // Pernas e pes abaixo do quadril (simetricas em torno de b.center.x)
            if (ny < 0.22f)
            {
                bool isLeft = p.x < b.center.x;
                int thigh = isLeft ? 5 : 6;
                float hipT = Mathf.InverseLerp(0.14f, 0.22f, ny);
                return BlendWeight(thigh, 1f - hipT, 0, hipT);
            }

            // Cabeca inteira (sem rasgar a boca fechada da pelucia; a mordida articula Spine+Neck+Head inteiros)
            if (ny >= 0.62f)
            {
                float t = Mathf.InverseLerp(0.62f, 0.70f, ny);
                return BlendWeight(2, 1f - t, 3, t);
            }

            // Pescoco
            if (ny >= 0.48f)
            {
                float t = Mathf.InverseLerp(0.48f, 0.56f, ny);
                return BlendWeight(1, 1f - t, 2, t);
            }

            // Tronco / Espinha
            if (ny >= 0.32f)
            {
                float t = Mathf.InverseLerp(0.32f, 0.42f, ny);
                return BlendWeight(0, 1f - t, 1, t);
            }

            return SingleWeight(0);
        }

        private static BoneWeight ComputeBearWeight(Vector3 p, Bounds b)
        {
            float ny = Mathf.Clamp01((p.y - b.min.y) / Mathf.Max(0.001f, b.size.y));
            float ax = Mathf.Abs(p.x - b.center.x);

            // Cabeca (articulada estritamente na constricao do pescoco ny=0.575..0.615, abaixo do queixo/boca!
            // Acima de ny=0.615 toda a cabeca/focinho/boca e 100% rigida no osso Head -> ZERO deformacao facial!)
            if (ny >= 0.575f)
            {
                float t = Mathf.InverseLerp(0.575f, 0.615f, ny);
                return BlendWeight(2, 1f - t, 3, t);
            }

            // Bracos + Cotovelos + Patas (estritamente fora da barriga |x| > 0.275 com blend amplo na axila)
            if (ny >= 0.32f && ny < 0.565f && ax > 0.275f)
            {
                bool isLeft = p.x < b.center.x;
                int ua = isLeft ? 4 : 7;
                int fa = isLeft ? 5 : 8;
                int ha = isLeft ? 6 : 9;

                if (ax < 0.335f)
                {
                    float shoulderT = Mathf.InverseLerp(0.275f, 0.335f, ax);
                    return BlendWeight(2, 1f - shoulderT, ua, shoulderT);
                }

                if (ny < 0.39f)
                {
                    float t = Mathf.InverseLerp(0.35f, 0.39f, ny);
                    return BlendWeight(ha, 1f - t, fa, t);
                }

                if (ny < 0.48f)
                {
                    float t = Mathf.InverseLerp(0.43f, 0.48f, ny);
                    return BlendWeight(fa, 1f - t, ua, t);
                }

                return SingleWeight(ua);
            }

            // Pernas + Joelhos + Pes abaixo do quadril
            if (ny < 0.30f)
            {
                bool isLeft = p.x < b.center.x;
                int thigh = isLeft ? 10 : 13;
                int shin = isLeft ? 11 : 14;
                int foot = isLeft ? 12 : 15;

                if (ny < 0.10f)
                {
                    float t = Mathf.InverseLerp(0.06f, 0.10f, ny);
                    return BlendWeight(foot, 1f - t, shin, t);
                }

                if (ny < 0.20f)
                {
                    float t = Mathf.InverseLerp(0.16f, 0.20f, ny);
                    return BlendWeight(shin, 1f - t, thigh, t);
                }

                float hipT = Mathf.InverseLerp(0.24f, 0.30f, ny);
                return BlendWeight(thigh, 1f - hipT, 0, hipT);
            }

            // Quadril -> Barriga -> Peito
            if (ny < 0.44f)
            {
                float t = Mathf.InverseLerp(0.36f, 0.44f, ny);
                return BlendWeight(0, 1f - t, 1, t);
            }

            float chestT = Mathf.InverseLerp(0.48f, 0.56f, ny);
            return BlendWeight(1, 1f - chestT, 2, chestT);
        }

        private static Transform CreateBone(string name, Transform parent, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            return go.transform;
        }

        private static BoneWeight SingleWeight(int b0)
        {
            return new BoneWeight { boneIndex0 = b0, weight0 = 1f };
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
    }
}
