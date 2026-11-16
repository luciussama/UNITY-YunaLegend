using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YunaLegend.Editor.ToyRoom
{
    /// <summary>
    /// Escultor 3D de alta fidelidade (Character Designer Agent + Game Artist Agent + Technical Artist Agent)
    /// para Yuna e Nanda na Fase 2 (Toy Room).
    /// Gera malhas 3D organicas esculpidas com UV centrada na face, normais faciais suavizadas estilo ArcSys
    /// e texturas 1024x1024 pintadas a mao (rosto expressivo, olhos com iris em camadas, cabelo em mechas,
    /// vestido floral com renda, e fada Nanda completa com asas de filigrana iluminadas).
    /// </summary>
    public static class ToyRoomYunaSculptor
    {
        public const string ModelsFolder = "Assets/Art/ToyRoom/Models";
        public const string CustomModelPathFbx = ModelsFolder + "/Yuna_ToyRoom.fbx";
        public const string CustomModelPathGlb = ModelsFolder + "/Yuna_ToyRoom.glb";

        private const string FaceTexPath = "Assets/Art/ToyRoom/Materials/T_Yuna_Face_Painted.png";
        private const string DressTexPath = "Assets/Art/ToyRoom/Materials/T_Yuna_Dress_Painted.png";
        private const string HairTexPath = "Assets/Art/ToyRoom/Materials/T_Yuna_Hair_Painted.png";
        private const string NandaFaceTexPath = "Assets/Art/ToyRoom/Materials/T_Nanda_Face_Painted.png";
        private const string NandaWingTexPath = "Assets/Art/ToyRoom/Materials/T_Nanda_Wing_Painted.png";
        private const string NandaDressTexPath = "Assets/Art/ToyRoom/Materials/T_Nanda_Dress_Painted.png";

        private static float Pi => Mathf.PI;

        public static void BuildYuna3D(
            Transform parent,
            out Transform bodyRoot,
            out Transform legLeftPivot,
            out Transform legRightPivot,
            out Transform armLeftPivot,
            out Transform armRightPivot,
            out Transform ponytailPivot)
        {
            Directory.CreateDirectory(ModelsFolder);
            EnsurePaintedTextures();

            var bodyGo = new GameObject("Yuna_BodyRoot");
            bodyGo.transform.SetParent(parent, false);
            bodyRoot = bodyGo.transform;

            var customPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CustomModelPathFbx)
                ?? AssetDatabase.LoadAssetAtPath<GameObject>(CustomModelPathGlb);
            if (customPrefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(customPrefab, bodyRoot);
                inst.name = "Yuna_ImportedMesh";
                legLeftPivot = FindOrCreatePivot(bodyRoot, "Leg_L_Pivot", new Vector3(-0.08f, 0.38f, 0f));
                legRightPivot = FindOrCreatePivot(bodyRoot, "Leg_R_Pivot", new Vector3(0.08f, 0.38f, 0f));
                armLeftPivot = FindOrCreatePivot(bodyRoot, "Arm_L_Pivot", new Vector3(-0.14f, 0.86f, 0f));
                armRightPivot = FindOrCreatePivot(bodyRoot, "Arm_R_Pivot", new Vector3(0.14f, 0.86f, 0f));
                ponytailPivot = FindOrCreatePivot(bodyRoot, "Ponytail_Pivot", new Vector3(0f, 1.32f, -0.14f));
                return;
            }

            // Materiais pictoricos com texturas 1024x1024 pintadas a mao
            var faceMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, FaceTexPath, outlineWidth: 0.0042f);
            var skinMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.98f, 0.83f, 0.72f), outlineWidth: 0.0040f);
            var hairMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, HairTexPath, outlineWidth: 0.0048f);
            var dressMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, DressTexPath, outlineWidth: 0.0045f);
            var ivoryClothMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.97f, 0.91f, 0.80f), outlineWidth: 0.0040f);
            var coralMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.84f, 0.28f, 0.30f), outlineWidth: 0.0038f);
            var scrunchieMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.91f, 0.48f, 0.49f), outlineWidth: 0.0042f);
            var sockMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.96f, 0.91f, 0.82f), outlineWidth: 0.0035f);
            var shoeMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.56f, 0.15f, 0.17f), outlineWidth: 0.0042f);
            var soleMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.25f, 0.14f, 0.11f), outlineWidth: 0.0030f);
            var goldBuckleMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.96f, 0.80f, 0.35f), emission: new Color(0.18f, 0.12f, 0.03f), outlineWidth: 0.002f);
            var lashMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.18f, 0.09f, 0.07f), outlineWidth: 0f);

            // 1. Corpo / Vestido Esculpido (Busto + Saia Plissada em Sino + Gola Peter Pan + Laco Coral)
            BuildTorsoAndSkirt(bodyRoot, dressMat, ivoryClothMat, coralMat, skinMat, goldBuckleMat);

            // 2. Cabeca Expressiva com Rosto Pintado a Mao (inclinada -16 graus para leitura clara na camera isometrica)
            var headRoot = new GameObject("Head_Root").transform;
            headRoot.SetParent(bodyRoot, false);
            headRoot.localPosition = new Vector3(0f, 0.95f, 0.01f);
            headRoot.localRotation = Quaternion.Euler(-16f, 0f, 0f);

            BuildSculptedHeadAndFace(headRoot, faceMat, skinMat, lashMat, hairMat);

            // 3. Rabo de Cavalo Alto Cacheado Volumoso + Xuxinha Rosa Frisada
            ponytailPivot = new GameObject("Ponytail_Pivot").transform;
            ponytailPivot.SetParent(headRoot, false);
            ponytailPivot.localPosition = new Vector3(0f, 0.35f, -0.13f);
            BuildScrunchieAndCurlyPonytail(ponytailPivot, scrunchieMat, hairMat);

            // 4. Bracos Articulados com Mangas Bufantes Plissadas e Maozinhas Esculpidas
            armLeftPivot = new GameObject("Arm_L_Pivot").transform;
            armLeftPivot.SetParent(bodyRoot, false);
            armLeftPivot.localPosition = new Vector3(-0.138f, 0.88f, 0f);
            BuildSculptedArm(armLeftPivot, -1, ivoryClothMat, coralMat, skinMat);

            armRightPivot = new GameObject("Arm_R_Pivot").transform;
            armRightPivot.SetParent(bodyRoot, false);
            armRightPivot.localPosition = new Vector3(0.138f, 0.88f, 0f);
            BuildSculptedArm(armRightPivot, 1, ivoryClothMat, coralMat, skinMat);

            // 5. Pernas Articuladas com Meias de Babado e Sapatos Mary Jane com Fivela Dourada
            legLeftPivot = new GameObject("Leg_L_Pivot").transform;
            legLeftPivot.SetParent(bodyRoot, false);
            legLeftPivot.localPosition = new Vector3(-0.075f, 0.44f, 0f);
            BuildSculptedLegAndShoe(legLeftPivot, -1, skinMat, sockMat, shoeMat, soleMat, goldBuckleMat);

            legRightPivot = new GameObject("Leg_R_Pivot").transform;
            legRightPivot.SetParent(bodyRoot, false);
            legRightPivot.localPosition = new Vector3(0.075f, 0.44f, 0f);
            BuildSculptedLegAndShoe(legRightPivot, 1, skinMat, sockMat, shoeMat, soleMat, goldBuckleMat);
        }

        public static void BuildNandaFairySculpted(Transform parent)
        {
            EnsurePaintedTextures();

            var fairyRoot = new GameObject("Nanda_FairyBody").transform;
            fairyRoot.SetParent(parent, false);
            fairyRoot.localRotation = Quaternion.Euler(-12f, 165f, 0f);

            var faceMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.82f, 0.95f, 1.0f),
                NandaFaceTexPath,
                emission: new Color(0.06f, 0.20f, 0.32f),
                outlineWidth: 0.0025f);
            var dressMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.42f, 0.80f, 0.98f),
                NandaDressTexPath,
                emission: new Color(0.08f, 0.26f, 0.44f),
                outlineWidth: 0.0025f);
            var hairMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.28f, 0.72f, 0.96f),
                emission: new Color(0.08f, 0.28f, 0.48f),
                outlineWidth: 0.0028f);
            var skinMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.75f, 0.92f, 1.0f),
                emission: new Color(0.06f, 0.18f, 0.28f),
                outlineWidth: 0.0022f);
            var crownMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.70f, 0.94f, 1.0f),
                emission: new Color(0.18f, 0.50f, 0.75f),
                outlineWidth: 0f);
            var wingMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.48f, 0.86f, 1.0f),
                NandaWingTexPath,
                emission: new Color(0.12f, 0.38f, 0.62f),
                outlineWidth: 0f);

            // 1. Vestido de Petalas Luminosas Azuis da Nanda
            var dressMesh = CreateParametricMesh("Nanda_PetalDressMesh", 40, 20, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float y = Mathf.Lerp(0.06f, -0.18f, v);
                float flare = Mathf.Lerp(0.036f, 0.105f, Mathf.Pow(v, 0.8f));
                float petals = 1f + 0.14f * Mathf.Abs(Mathf.Cos(a * 4f)) * v;
                float petalHem = v > 0.7f ? -0.018f * Mathf.Abs(Mathf.Sin(a * 4f)) * ((v - 0.7f) / 0.3f) : 0f;
                return new Vector3(flare * petals * Mathf.Sin(a), y + petalHem, flare * petals * Mathf.Cos(a));
            });
            AttachMesh("Nanda_PetalDress", fairyRoot, dressMesh, dressMat);

            // 2. Cabeca Esculpida da Fada Nanda com Rosto Pintado a Mao
            var headMesh = CreateParametricMesh("Nanda_HeadMesh", 40, 24, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = (u - 0.5f) * 2f * Pi;
                Vector3 q = new Vector3(Mathf.Sin(p) * Mathf.Sin(a), Mathf.Cos(p), Mathf.Sin(p) * Mathf.Cos(a));
                float cheek = 1f + 0.06f * Mathf.Exp(-Mathf.Pow((q.y + 0.15f) * 3.2f, 2f)) * Mathf.Max(0f, q.z);
                return new Vector3(0f, 0.15f, 0f) + new Vector3(0.078f * q.x * cheek, 0.082f * q.y, 0.075f * q.z * cheek);
            }, blendArcSysFaceNormals: true);
            AttachMesh("Nanda_Head", fairyRoot, headMesh, faceMat);

            // 3. Cabelo Fluido Azul Celeste + Coroa Estelar
            var hairCap = CreateParametricMesh("Nanda_HairCapMesh", 40, 20, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float frontOpen = Mathf.Exp(-Mathf.Pow(a / 1.15f, 2f));
                float phi = 0.001f + v * Mathf.Lerp(2.35f, 0.92f, frontOpen);
                float waves = 1f + 0.04f * Mathf.Cos(a * 10f + phi * 2f);
                return new Vector3(
                    0.086f * Mathf.Sin(phi) * Mathf.Sin(a) * waves,
                    0.158f + 0.090f * Mathf.Cos(phi),
                    -0.006f + 0.084f * Mathf.Sin(phi) * Mathf.Cos(a) * waves);
            });
            AttachMesh("Nanda_HairCap", fairyRoot, hairCap, hairMat);

            for (int side = -1; side <= 1; side += 2)
            {
                var lockMesh = CreateCurveTubeMesh(
                    $"Nanda_HairLock_{side}",
                    new Vector3(side * 0.055f, 0.21f, 0.03f),
                    new Vector3(side * 0.092f, 0.15f, 0.02f),
                    new Vector3(side * 0.098f, 0.05f, -0.02f),
                    new Vector3(side * 0.075f, -0.05f, -0.05f),
                    0.022f,
                    0.55f,
                    12,
                    14);
                AttachMesh($"Nanda_HairLock_{side}", fairyRoot, lockMesh, hairMat);

                var armMesh = CreateCurveTubeMesh(
                    $"Nanda_Arm_{side}",
                    new Vector3(side * 0.038f, 0.045f, 0f),
                    new Vector3(side * 0.075f, 0.01f, 0.02f),
                    new Vector3(side * 0.105f, -0.03f, 0.04f),
                    new Vector3(side * 0.120f, -0.055f, 0.05f),
                    0.014f,
                    0.85f,
                    10,
                    10);
                AttachMesh($"Nanda_Arm_{side}", fairyRoot, armMesh, skinMat);

                var legMesh = CreateCurveTubeMesh(
                    $"Nanda_Leg_{side}",
                    new Vector3(side * 0.024f, -0.15f, 0f),
                    new Vector3(side * 0.028f, -0.21f, -0.02f),
                    new Vector3(side * 0.030f, -0.27f, -0.05f),
                    new Vector3(side * 0.026f, -0.32f, -0.07f),
                    0.015f,
                    0.88f,
                    10,
                    10);
                AttachMesh($"Nanda_Leg_{side}", fairyRoot, legMesh, skinMat);
            }

            // Coroa de luz estelar no topo da cabeca
            var crownMesh = CreateParametricMesh("Nanda_CrownMesh", 32, 8, (u, v) =>
            {
                float a = u * 2f * Pi;
                float spikes = 0.028f * Mathf.Pow(Mathf.Abs(Mathf.Cos(a * 4f)), 1.6f) * v;
                float r = 0.052f + 0.008f * v;
                return new Vector3(r * Mathf.Sin(a), 0.235f + 0.018f * v + spikes, r * Mathf.Cos(a));
            });
            AttachMesh("Nanda_StarlightCrown", fairyRoot, crownMesh, crownMat);

            // 4. Quatro Asas de Borboleta/Fada com Textura de Filigrana Azul Pintada a Mao
            for (int side = -1; side <= 1; side += 2)
            {
                string sideTag = side < 0 ? "L" : "R";
                var upperPivot = new GameObject($"Wing_Upper_{sideTag}").transform;
                upperPivot.SetParent(fairyRoot, false);
                upperPivot.localPosition = new Vector3(side * 0.025f, 0.04f, -0.035f);
                upperPivot.localRotation = Quaternion.Euler(0f, side * -22f, 0f);

                var upperWing = CreateParametricMesh($"Nanda_UpperWingMesh_{sideTag}", 32, 18, (u, v) =>
                {
                    float span = Mathf.Clamp(v, 0.01f, 0.99f);
                    float a = u * 2f * Pi;
                    float chord = Mathf.Cos(a);
                    float thickness = Mathf.Sin(a);
                    float envelope = Mathf.Sin(span * Pi);
                    float scallop = 1f + 0.08f * Mathf.Cos(span * Pi * 5f);
                    float x = side * (0.015f + 0.34f * span);
                    float y = 0.14f * Mathf.Sin(span * Pi * 0.75f) + 0.135f * chord * envelope * scallop;
                    float z = -0.02f * Mathf.Sin(span * Pi) + 0.014f * thickness * envelope;
                    return new Vector3(x, y, z);
                });
                AttachMesh($"WingMesh_Upper_{sideTag}", upperPivot, upperWing, wingMat);

                var lowerPivot = new GameObject($"Wing_Lower_{sideTag}").transform;
                lowerPivot.SetParent(fairyRoot, false);
                lowerPivot.localPosition = new Vector3(side * 0.022f, -0.02f, -0.038f);
                lowerPivot.localRotation = Quaternion.Euler(0f, side * -28f, 0f);

                var lowerWing = CreateParametricMesh($"Nanda_LowerWingMesh_{sideTag}", 28, 14, (u, v) =>
                {
                    float span = Mathf.Clamp(v, 0.01f, 0.99f);
                    float a = u * 2f * Pi;
                    float chord = Mathf.Cos(a);
                    float thickness = Mathf.Sin(a);
                    float envelope = Mathf.Sin(span * Pi);
                    float x = side * (0.012f + 0.23f * span);
                    float y = -0.09f * span + 0.085f * chord * envelope;
                    float z = -0.015f * Mathf.Sin(span * Pi) + 0.012f * thickness * envelope;
                    return new Vector3(x, y, z);
                });
                AttachMesh($"WingMesh_Lower_{sideTag}", lowerPivot, lowerWing, wingMat);
            }
        }

        private static void BuildTorsoAndSkirt(
            Transform bodyRoot,
            Material dressMat,
            Material ivoryMat,
            Material coralMat,
            Material skinMat,
            Material goldMat)
        {
            // Pescoco
            var neckMesh = CreateParametricMesh("Yuna_Neck", 24, 8, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float y = Mathf.Lerp(0.89f, 0.98f, v);
                return new Vector3(0.046f * Mathf.Sin(a), y, 0.044f * Mathf.Cos(a));
            });
            AttachMesh("Neck", bodyRoot, neckMesh, skinMat);

            // Busto + Saia Plissada Continua em Sino com Anagua Ondulada
            var dressMesh = CreateParametricMesh("Yuna_DressBell", 80, 42, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                if (v < 0.26f)
                {
                    float tv = v / 0.26f;
                    float y = Mathf.Lerp(0.91f, 0.72f, tv);
                    float rx = Mathf.Lerp(0.090f, 0.114f, Mathf.Sin(tv * Pi * 0.5f));
                    float rz = Mathf.Lerp(0.074f, 0.090f, Mathf.Sin(tv * Pi * 0.5f));
                    return new Vector3(rx * Mathf.Sin(a), y, rz * Mathf.Cos(a));
                }
                else
                {
                    float sv = (v - 0.26f) / 0.74f;
                    float y = Mathf.Lerp(0.72f, 0.33f, sv);
                    float flare = Mathf.Pow(sv, 0.84f);
                    float baseRx = Mathf.Lerp(0.114f, 0.295f, flare);
                    float baseRz = Mathf.Lerp(0.090f, 0.265f, flare);
                    float pleats = 1f + (0.018f + 0.036f * sv) * Mathf.Cos(a * 14f);
                    float hemWave = sv > 0.82f ? 0.012f * Mathf.Sin(a * 28f) * ((sv - 0.82f) / 0.18f) : 0f;
                    return new Vector3(baseRx * pleats * Mathf.Sin(a), y + hemWave, baseRz * pleats * Mathf.Cos(a));
                }
            });
            AttachMesh("Dress_BellSkirt", bodyRoot, dressMesh, dressMat);

            // Faixa de cintura coral-marfim com laco traseiro
            var sashMesh = CreateParametricMesh("Yuna_WaistSash", 48, 8, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float y = Mathf.Lerp(0.738f, 0.702f, v);
                return new Vector3(0.118f * Mathf.Sin(a), y, 0.094f * Mathf.Cos(a));
            });
            AttachMesh("Waist_Sash", bodyRoot, sashMesh, coralMat);

            // Gola Peter Pan ondulada em duas abas arredondadas
            var collarMesh = CreateParametricMesh("Yuna_Collar", 64, 12, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float frontNotch = 1f - 0.22f * Mathf.Exp(-Mathf.Pow(a / 0.28f, 2f));
                float scallop = 1f + 0.045f * Mathf.Cos(a * 18f);
                float r = Mathf.Lerp(0.070f, 0.152f, v) * scallop * frontNotch;
                float y = 0.908f - 0.036f * v + 0.005f * Mathf.Sin(a * 18f) * v;
                return new Vector3(r * Mathf.Sin(a), y, r * 0.88f * Mathf.Cos(a));
            });
            AttachMesh("PeterPan_Collar", bodyRoot, collarMesh, ivoryMat);

            // Broche dourado + Laco coral no peito (2 lacos + no central + 2 fitas)
            var broochMesh = CreateEllipsoidMesh("ChestBow_Brooch", new Vector3(0f, 0.852f, 0.108f), new Vector3(0.018f, 0.018f, 0.012f), 16);
            AttachMesh("ChestBow_Brooch", bodyRoot, broochMesh, goldMat);

            for (int side = -1; side <= 1; side += 2)
            {
                var loopMesh = CreateParametricMesh($"ChestBow_Loop_{side}", 24, 12, (u, v) =>
                {
                    float s = Mathf.Sin(v * Pi);
                    return new Vector3(
                        side * (0.010f + 0.054f * v),
                        0.852f + 0.025f * s * Mathf.Sin(u * 2f * Pi),
                        0.105f + 0.014f * s * Mathf.Cos(u * 2f * Pi));
                });
                AttachMesh($"ChestBow_Loop_{side}", bodyRoot, loopMesh, coralMat);

                var ribbonMesh = CreateCurveTubeMesh(
                    $"ChestBow_Tail_{side}",
                    new Vector3(side * 0.012f, 0.845f, 0.106f),
                    new Vector3(side * 0.030f, 0.808f, 0.112f),
                    new Vector3(side * 0.040f, 0.765f, 0.114f),
                    new Vector3(side * 0.046f, 0.728f, 0.108f),
                    0.013f,
                    0.35f,
                    12,
                    14);
                AttachMesh($"ChestBow_Tail_{side}", bodyRoot, ribbonMesh, coralMat);
            }
        }

        private static Vector3 EvaluateYunaHeadPoint(float u, float v, float outwardOffset = 0f)
        {
            float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
            float a = (u - 0.5f) * 2f * Pi;
            Vector3 q = new Vector3(Mathf.Sin(p) * Mathf.Sin(a), Mathf.Cos(p), Mathf.Sin(p) * Mathf.Cos(a));

            float cheekBulge = 1f + 0.085f * Mathf.Exp(-Mathf.Pow((q.y + 0.16f) * 3.4f, 2f)) * Mathf.Max(0f, q.z);
            float chinTaper = Mathf.SmoothStep(0.78f, 1.0f, Mathf.InverseLerp(-1.0f, -0.12f, q.y));
            q.x *= chinTaper * cheekBulge;
            q.z *= cheekBulge;

            Vector3 pos = new Vector3(0f, 0.165f, 0.012f) + Vector3.Scale(q, new Vector3(0.188f, 0.182f, 0.178f));

            // Narizinho delicado esculpido no centro da face
            if (q.z > 0.2f)
            {
                float nose = 0.016f * Mathf.Exp(-Mathf.Pow(pos.x / 0.020f, 2f) - Mathf.Pow((pos.y - 0.135f) / 0.020f, 2f));
                pos.z += nose;
            }

            if (Mathf.Abs(outwardOffset) > 0.0001f)
            {
                Vector3 radial = (pos - new Vector3(0f, 0.165f, 0.012f)).normalized;
                pos += radial * outwardOffset;
            }

            return pos;
        }

        private static void BuildSculptedHeadAndFace(
            Transform headRoot,
            Material faceMat,
            Material skinMat,
            Material lashMat,
            Material hairMat)
        {
            // 1. Cabeca Esculpida com UV Centrada na Face (u=0.5 centro frontal) e Textura Facial 1024x1024
            var headMesh = CreateParametricMesh("Yuna_Head", 72, 40, (u, v) => EvaluateYunaHeadPoint(u, v, 0f), blendArcSysFaceNormals: true);
            AttachMesh("Head_Mesh", headRoot, headMesh, faceMat);

            // 2. Orelhas Esculpidas + Cilios Superiores 3D Curvos Acima da Superficie (zero clipping!)
            for (int side = -1; side <= 1; side += 2)
            {
                var earMesh = CreateEllipsoidMesh(
                    $"Ear_{side}",
                    new Vector3(side * 0.184f, 0.150f, -0.005f),
                    new Vector3(0.028f, 0.042f, 0.024f),
                    18);
                AttachMesh($"Ear_{side}", headRoot, earMesh, skinMat);

                // Cilios superiores 3D esculpidos acompanhando exatamente a curvatura da cabeca
                float eyeCenterU = 0.5f + side * 0.078f;
                var lashArc = CreateCurveTubeMesh(
                    $"Eyelash3D_{side}",
                    EvaluateYunaHeadPoint(eyeCenterU - side * 0.036f, 0.492f, 0.004f),
                    EvaluateYunaHeadPoint(eyeCenterU - side * 0.012f, 0.468f, 0.006f),
                    EvaluateYunaHeadPoint(eyeCenterU + side * 0.018f, 0.472f, 0.006f),
                    EvaluateYunaHeadPoint(eyeCenterU + side * 0.042f, 0.498f, 0.005f),
                    0.0065f,
                    0.5f,
                    10,
                    14);
                AttachMesh($"Eyelash3D_{side}", headRoot, lashArc, lashMat);
            }

            // 3. Domo de Cabelo Volumoso + Cortina de Franja Esculpida Continua (sem buracos na testa!)
            var hairCap = CreateParametricMesh("Yuna_HairCap", 72, 32, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float frontFaceMask = Mathf.Exp(-Mathf.Pow(a / 0.92f, 2f));
                float partNotch = 0.04f * Mathf.Exp(-Mathf.Pow((a + 0.10f) / 0.10f, 2f));
                float maxPhi = Mathf.Lerp(2.32f, 1.14f - partNotch, frontFaceMask);
                float phi = 0.001f + v * maxPhi;
                float grooves = 1f + 0.028f * Mathf.Cos(a * 16f + phi * 2.0f);
                return new Vector3(
                    0.206f * Mathf.Sin(phi) * Mathf.Sin(a) * grooves,
                    0.184f + 0.226f * Mathf.Cos(phi),
                    -0.004f + 0.196f * Mathf.Sin(phi) * Mathf.Cos(a) * grooves);
            });
            AttachMesh("Hair_Cap", headRoot, hairCap, hairMat);

            // 4. Mechas de Franja Volumosas Sobrepostas Emoldurando a Testa e as Bochechas
            float partU = -0.018f;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int layer = 0; layer < 4; layer++)
                {
                    float startX = partU + side * (0.008f + layer * 0.024f);
                    float endX = side * (0.028f + layer * 0.040f);
                    float endY = 0.256f - layer * 0.012f;
                    float endZ = Mathf.Sqrt(Mathf.Max(0.01f, 0.195f * 0.195f - endX * endX * 0.65f));

                    var sweptBang = CreateCurveTubeMesh(
                        $"SweptBang_{side}_{layer}",
                        new Vector3(startX * 0.5f, 0.375f, 0.055f),
                        new Vector3(Mathf.Lerp(startX, endX, 0.45f), 0.378f, 0.158f),
                        new Vector3(Mathf.Lerp(startX, endX, 0.85f), 0.312f, endZ + 0.016f),
                        new Vector3(endX, endY, endZ),
                        0.046f - layer * 0.003f,
                        0.48f,
                        14,
                        18);
                    AttachMesh($"SweptBang_{side}_{layer}", headRoot, sweptBang, hairMat);
                }

                // Mechas Laterais Longas Emoldurando as Bochechas (Side Locks)
                var sideLock = CreateCurveTubeMesh(
                    $"SideLock_{side}",
                    new Vector3(side * 0.145f, 0.325f, 0.065f),
                    new Vector3(side * 0.212f, 0.245f, 0.088f),
                    new Vector3(side * 0.204f, 0.135f, 0.078f),
                    new Vector3(side * 0.162f, 0.045f, 0.052f),
                    0.042f,
                    0.52f,
                    14,
                    18);
                AttachMesh($"SideLock_{side}", headRoot, sideLock, hairMat);
            }
        }

        private static void BuildScrunchieAndCurlyPonytail(Transform ponytailPivot, Material scrunchieMat, Material hairMat)
        {
            var scrunchieMesh = CreateParametricMesh("Yuna_Scrunchie", 48, 18, (u, v) =>
            {
                float a = u * 2f * Pi;
                float b = v * 2f * Pi;
                float ruffle = 1f + 0.24f * Mathf.Sin(a * 14f);
                float majorR = 0.070f;
                float minorR = 0.028f * ruffle;
                float r = majorR + minorR * Mathf.Cos(b);
                return new Vector3(
                    r * Mathf.Cos(a),
                    minorR * Mathf.Sin(b) + 0.011f * Mathf.Cos(a * 14f),
                    r * Mathf.Sin(a) * 0.86f);
            });
            var scGo = AttachMesh("Ruffled_Scrunchie", ponytailPivot, scrunchieMesh, scrunchieMat);
            scGo.transform.localRotation = Quaternion.Euler(52f, 0f, 0f);

            var coreTail = CreateCurveTubeMesh(
                "Ponytail_Core",
                new Vector3(0f, 0.01f, -0.02f),
                new Vector3(0f, 0.15f, -0.19f),
                new Vector3(0f, -0.12f, -0.34f),
                new Vector3(0f, -0.34f, -0.23f),
                0.098f,
                0.82f,
                20,
                24);
            AttachMesh("Ponytail_Core", ponytailPivot, coreTail, hairMat);

            for (int i = 0; i < 11; i++)
            {
                float angle = i * 2f * Pi / 11f;
                float ox = Mathf.Cos(angle) * 0.058f;
                float oy = Mathf.Sin(angle) * 0.048f;
                float curlTwist = (i % 2 == 0 ? 1f : -1f) * 0.042f;

                var curlMesh = CreateCurveTubeMesh(
                    $"Ponytail_Curl_{i}",
                    new Vector3(ox * 0.4f, 0.01f + oy * 0.3f, -0.02f),
                    new Vector3(ox * 1.65f, 0.16f + oy, -0.19f),
                    new Vector3(ox * 1.48f + curlTwist, -0.14f + oy * 0.5f, -0.35f - Mathf.Abs(ox) * 0.3f),
                    new Vector3(ox * 0.72f - curlTwist, -0.35f + (i % 3) * 0.025f, -0.23f),
                    0.050f,
                    0.85f,
                    14,
                    22);
                AttachMesh($"Ponytail_Curl_{i}", ponytailPivot, curlMesh, hairMat);
            }
        }

        private static void BuildSculptedArm(Transform armPivot, int side, Material ivoryMat, Material coralMat, Material skinMat)
        {
            var sleeveMesh = CreateParametricMesh($"Sleeve_{side}", 32, 16, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = u * 2f * Pi;
                float gathers = 1f + 0.065f * Mathf.Cos(a * 10f) * Mathf.Sin(p);
                return new Vector3(
                    side * 0.025f + 0.070f * Mathf.Sin(p) * Mathf.Cos(a) * gathers,
                    -0.04f + 0.070f * Mathf.Cos(p),
                    0.070f * Mathf.Sin(p) * Mathf.Sin(a) * gathers);
            });
            AttachMesh("Puff_Sleeve", armPivot, sleeveMesh, ivoryMat);

            var cuffMesh = CreateParametricMesh($"SleeveCuff_{side}", 24, 6, (u, v) =>
            {
                float a = u * 2f * Pi;
                float r = 0.046f + 0.006f * v + 0.004f * Mathf.Sin(a * 12f);
                return new Vector3(side * 0.032f + r * Mathf.Cos(a), -0.105f - 0.018f * v, r * Mathf.Sin(a));
            });
            AttachMesh("Sleeve_Cuff", armPivot, cuffMesh, coralMat);

            var forearm = CreateCurveTubeMesh(
                $"Forearm_{side}",
                new Vector3(side * 0.035f, -0.11f, 0f),
                new Vector3(side * 0.045f, -0.18f, 0.012f),
                new Vector3(side * 0.050f, -0.24f, 0.024f),
                new Vector3(side * 0.048f, -0.29f, 0.034f),
                0.034f,
                0.88f,
                14,
                14);
            AttachMesh("Forearm", armPivot, forearm, skinMat);

            var handMesh = CreateEllipsoidMesh(
                $"Hand_{side}",
                new Vector3(side * 0.048f, -0.305f, 0.038f),
                new Vector3(0.030f, 0.034f, 0.026f),
                16);
            AttachMesh("Hand", armPivot, handMesh, skinMat);
        }

        private static void BuildSculptedLegAndShoe(
            Transform legPivot,
            int side,
            Material skinMat,
            Material sockMat,
            Material shoeMat,
            Material soleMat,
            Material goldBuckleMat)
        {
            var legMesh = CreateParametricMesh($"Leg_{side}", 24, 14, (u, v) =>
            {
                float a = u * 2f * Pi;
                float y = Mathf.Lerp(0f, -0.22f, v);
                float r = Mathf.Lerp(0.046f, 0.035f, v);
                return new Vector3(r * Mathf.Cos(a), y, r * Mathf.Sin(a));
            });
            AttachMesh("Leg_Upper", legPivot, legMesh, skinMat);

            var sockMesh = CreateParametricMesh($"Sock_{side}", 28, 12, (u, v) =>
            {
                float a = u * 2f * Pi;
                float y = Mathf.Lerp(-0.20f, -0.36f, v);
                float ruffle = v < 0.20f ? 0.007f * Mathf.Sin(a * 14f) : 0.0018f * Mathf.Cos(a * 18f);
                float r = 0.039f + ruffle;
                return new Vector3(r * Mathf.Cos(a), y, r * Mathf.Sin(a));
            });
            AttachMesh("Ribbed_Sock", legPivot, sockMesh, sockMat);

            var shoeUpper = CreateParametricMesh($"Shoe_{side}", 32, 16, (u, v) =>
            {
                float p = Mathf.Max(0.001f, v) * Pi * 0.5f;
                float a = u * 2f * Pi;
                return new Vector3(
                    0.058f * Mathf.Sin(p) * Mathf.Cos(a),
                    -0.425f + 0.084f * Mathf.Cos(p),
                    0.034f + 0.102f * Mathf.Sin(p) * Mathf.Sin(a));
            });
            AttachMesh("MaryJane_Shoe", legPivot, shoeUpper, shoeMat);

            var strapMesh = CreateCurveTubeMesh(
                $"ShoeStrap_{side}",
                new Vector3(-0.048f, -0.375f, 0.025f),
                new Vector3(-0.024f, -0.352f, 0.042f),
                new Vector3(0.024f, -0.352f, 0.042f),
                new Vector3(0.048f, -0.375f, 0.025f),
                0.010f,
                0.6f,
                10,
                10);
            AttachMesh("Shoe_Strap", legPivot, strapMesh, shoeMat);

            var buckleMesh = CreateEllipsoidMesh(
                $"ShoeBuckle_{side}",
                new Vector3(side * 0.044f, -0.365f, 0.032f),
                new Vector3(0.010f, 0.014f, 0.014f),
                12);
            AttachMesh("Shoe_Buckle", legPivot, buckleMesh, goldBuckleMat);

            var soleMesh = CreateEllipsoidMesh(
                $"Sole_{side}",
                new Vector3(0f, -0.425f, 0.034f),
                new Vector3(0.060f, 0.015f, 0.106f),
                24);
            AttachMesh("Shoe_Sole", legPivot, soleMesh, soleMat);
        }

        public static void EnsurePaintedTextures(bool forceRegenerate = false)
        {
            Directory.CreateDirectory("Assets/Art/ToyRoom/Materials");

            if (forceRegenerate || !File.Exists(FaceTexPath))
            {
                GeneratePaintedYunaFaceTexture(FaceTexPath);
            }

            if (forceRegenerate || !File.Exists(DressTexPath))
            {
                GeneratePaintedDressTexture(DressTexPath);
            }

            if (forceRegenerate || !File.Exists(HairTexPath))
            {
                GeneratePaintedHairTexture(HairTexPath);
            }

            if (forceRegenerate || !File.Exists(NandaFaceTexPath))
            {
                GeneratePaintedNandaFaceTexture(NandaFaceTexPath);
            }

            if (forceRegenerate || !File.Exists(NandaWingTexPath))
            {
                GeneratePaintedNandaWingTexture(NandaWingTexPath);
            }

            if (forceRegenerate || !File.Exists(NandaDressTexPath))
            {
                GeneratePaintedNandaDressTexture(NandaDressTexPath);
            }

            AssetDatabase.ImportAsset(FaceTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(DressTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(HairTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(NandaFaceTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(NandaWingTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(NandaDressTexPath, ImportAssetOptions.ForceUpdate);
        }

        private static void GeneratePaintedYunaFaceTexture(string path)
        {
            const int size = 1024;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            Color skinBase = new Color(0.98f, 0.84f, 0.73f);
            Color skinWarmShade = new Color(0.92f, 0.69f, 0.56f);
            Color blushRose = new Color(0.95f, 0.52f, 0.50f);
            Color scleraCream = new Color(1.0f, 0.98f, 0.95f);
            Color scleraShadow = new Color(0.86f, 0.77f, 0.74f);
            Color limbalDark = new Color(0.18f, 0.08f, 0.05f);
            Color irisUpper = new Color(0.34f, 0.15f, 0.08f);
            Color irisMid = new Color(0.58f, 0.29f, 0.14f);
            Color irisAmberGlow = new Color(0.92f, 0.58f, 0.24f);
            Color pupilColor = new Color(0.10f, 0.04f, 0.03f);
            Color lashInk = new Color(0.16f, 0.07f, 0.05f);
            Color browColor = new Color(0.32f, 0.16f, 0.10f);
            Color smileLine = new Color(0.76f, 0.32f, 0.30f);
            Color lowerLipTint = new Color(0.95f, 0.58f, 0.54f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1); // v=0 topo da cabeca, v=1 queixo (corresponde a uv.y da malha)
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1); // u=0.5 centro frontal da face

                    // Base de pele em aquarela com sombreado suave no topo da testa e queixo
                    float edgeShade = Mathf.Clamp01(Mathf.Abs(u - 0.5f) * 2.2f);
                    float topBottomShade = Mathf.Clamp01(Mathf.Max(0.38f - v, v - 0.75f) * 2.4f);
                    float paperGrain = (Mathf.PerlinNoise(u * 64f, v * 64f) - 0.5f) * 0.018f;
                    Color col = Color.Lerp(skinBase, skinWarmShade, Mathf.Clamp01(edgeShade * 0.45f + topBottomShade * 0.55f));
                    col.r = Mathf.Clamp01(col.r + paperGrain);
                    col.g = Mathf.Clamp01(col.g + paperGrain);
                    col.b = Mathf.Clamp01(col.b + paperGrain);

                    // Blush aquarelado nas bochechas
                    for (int side = -1; side <= 1; side += 2)
                    {
                        float bu = (u - (0.5f + side * 0.102f)) / 0.044f;
                        float bv = (v - 0.625f) / 0.055f;
                        float bd = bu * bu + bv * bv;
                        if (bd < 1f)
                        {
                            float blushAlpha = Mathf.Pow(1f - bd, 1.6f) * 0.68f;
                            col = Color.Lerp(col, blushRose, blushAlpha);
                        }

                        // Olhos Grandes Expressivos Estilo Storybook (Fase 1)
                        float eyeCx = 0.5f + side * 0.078f;
                        float eyeCy = 0.542f;
                        float eu = (u - eyeCx) / 0.042f;
                        float ev = (v - eyeCy) / 0.086f;
                        float ed = Mathf.Sqrt(eu * eu + ev * ev);

                        // Sobrancelhas arqueadas suaves
                        float browU = (u - eyeCx) * side;
                        float browArchY = 0.405f - 0.018f * Mathf.Cos(Mathf.Clamp(browU / 0.042f, -1f, 1f) * Pi * 0.5f) + 0.004f * (browU / 0.042f);
                        float browDist = Mathf.Abs(v - browArchY);
                        if (Mathf.Abs(browU) < 0.042f && browDist < 0.011f)
                        {
                            float taper = 1f - Mathf.Pow(Mathf.Abs(browU) / 0.042f, 1.6f);
                            float bMask = Mathf.SmoothStep(0.011f * taper, 0f, browDist);
                            col = Color.Lerp(col, browColor, bMask * 0.88f);
                        }

                        // Prega da palpebra dupla
                        float creaseY = eyeCy - 0.092f + 0.022f * eu * eu;
                        if (Mathf.Abs(eu) < 0.92f && Mathf.Abs(v - creaseY) < 0.005f)
                        {
                            col = Color.Lerp(col, browColor, 0.45f);
                        }

                        // Esclera + Iris em degradê + Pupila + Brilhos Estelares
                        if (ed <= 1.0f)
                        {
                            Color eyeCol = Color.Lerp(scleraCream, scleraShadow, Mathf.Clamp01((-ev + 0.25f) * 0.65f));

                            float iu = (u - eyeCx) / 0.033f;
                            float iv = (v - (eyeCy + 0.004f)) / 0.076f;
                            float id = Mathf.Sqrt(iu * iu + iv * iv);
                            if (id <= 1.0f)
                            {
                                float vertGrad = Mathf.Clamp01((iv + 0.85f) / 1.7f);
                                float angle = Mathf.Atan2(iv, iu);
                                float fibers = 0.5f + 0.5f * Mathf.Sin(angle * 22f + id * 6f);
                                Color irisCol = Color.Lerp(irisUpper, irisMid, Mathf.Clamp01(vertGrad * 1.4f));
                                irisCol = Color.Lerp(irisCol, irisAmberGlow, Mathf.Pow(vertGrad, 1.8f) * (0.75f + 0.25f * fibers));

                                // Anel limbal escuro na borda da iris
                                float limbal = Mathf.SmoothStep(0.76f, 0.98f, id);
                                irisCol = Color.Lerp(irisCol, limbalDark, limbal * 0.85f);

                                // Pupila aveludada central
                                if (id < 0.46f)
                                {
                                    float pBlend = Mathf.SmoothStep(0.46f, 0.34f, id);
                                    irisCol = Color.Lerp(irisCol, pupilColor, pBlend);
                                }

                                eyeCol = irisCol;
                            }

                            // Brilho principal (canto superior esquerdo) + brilho secundario (inferior direito)
                            float g1u = (u - (eyeCx - 0.012f)) / 0.012f;
                            float g1v = (v - (eyeCy - 0.026f)) / 0.025f;
                            if (g1u * g1u + g1v * g1v <= 1f)
                            {
                                eyeCol = Color.Lerp(eyeCol, Color.white, Mathf.SmoothStep(1f, 0.4f, g1u * g1u + g1v * g1v));
                            }

                            float g2u = (u - (eyeCx + 0.011f)) / 0.006f;
                            float g2v = (v - (eyeCy + 0.028f)) / 0.013f;
                            if (g2u * g2u + g2v * g2v <= 1f)
                            {
                                eyeCol = Color.Lerp(eyeCol, new Color(1f, 0.97f, 0.88f), Mathf.SmoothStep(1f, 0.3f, g2u * g2u + g2v * g2v));
                            }

                            float edgeSoft = Mathf.SmoothStep(1.0f, 0.88f, ed);
                            col = Color.Lerp(col, eyeCol, edgeSoft);
                        }

                        // Linha de cilios superiores caligrafada em nanquim sepia
                        float lashArchY = (eyeCy - 0.078f) + 0.056f * eu * eu;
                        float lashDist = Mathf.Abs(v - lashArchY);
                        if (Mathf.Abs(eu) <= 1.18f && lashDist < 0.016f)
                        {
                            float thickness = 0.014f * (1f - 0.35f * Mathf.Abs(eu));
                            if (lashDist < thickness)
                            {
                                col = Color.Lerp(col, lashInk, Mathf.SmoothStep(thickness, thickness * 0.25f, lashDist));
                            }
                        }
                    }

                    // Sombra e brilho delicado do narizinho (logo abaixo da linha dos olhos)
                    float nu = (u - 0.5f) / 0.012f;
                    float nv = (v - 0.588f) / 0.018f;
                    if (nu * nu + nv * nv < 1f)
                    {
                        Color noseShade = nv > 0f ? new Color(0.82f, 0.50f, 0.40f) : new Color(1.0f, 0.96f, 0.90f);
                        col = Color.Lerp(col, noseShade, (1f - (nu * nu + nv * nv)) * 0.72f);
                    }

                    // Sorriso doce e delicado estilo livro ilustrado (bem visivel na camera isometrica)
                    float mu = (u - 0.5f) / 0.038f;
                    if (Mathf.Abs(mu) <= 1.0f)
                    {
                        float smileCurveY = 0.648f - 0.018f * mu * mu;
                        float mDist = Mathf.Abs(v - smileCurveY);

                        // Labio inferior / boca sorridente rosada suave
                        float lipU = mu / 0.78f;
                        float lipV = (v - 0.658f) / 0.018f;
                        if (lipU * lipU + lipV * lipV < 1f && v >= smileCurveY - 0.002f)
                        {
                            col = Color.Lerp(col, new Color(0.92f, 0.44f, 0.43f), (1f - (lipU * lipU + lipV * lipV)) * 0.78f);
                        }

                        if (mDist < 0.009f)
                        {
                            float mAlpha = Mathf.SmoothStep(0.009f, 0.0020f, mDist) * (1f - 0.25f * mu * mu);
                            col = Color.Lerp(col, new Color(0.54f, 0.16f, 0.15f), mAlpha);
                        }
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GeneratePaintedDressTexture(string path)
        {
            const int size = 1024;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseCream = new Color(0.96f, 0.90f, 0.78f);
            Color foldShadow = new Color(0.82f, 0.71f, 0.56f);
            Color coralRed = new Color(0.82f, 0.26f, 0.28f);
            Color coralFlower = new Color(0.90f, 0.42f, 0.42f);
            Color leafGreen = new Color(0.45f, 0.64f, 0.44f);
            Color flowerCenter = new Color(0.98f, 0.84f, 0.36f);
            Color laceCream = new Color(0.99f, 0.96f, 0.88f);
            Color sepiaLine = new Color(0.34f, 0.18f, 0.13f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float pleat = 0.5f + 0.5f * Mathf.Sin(u * Pi * 2f * 14f);
                    float brush = (Mathf.PerlinNoise(u * 48f, v * 48f) - 0.5f) * 0.045f;
                    Color col = Color.Lerp(foldShadow, baseCream, Mathf.Lerp(0.32f, 1f, pleat));
                    col.r = Mathf.Clamp01(col.r + brush);
                    col.g = Mathf.Clamp01(col.g + brush);
                    col.b = Mathf.Clamp01(col.b + brush);

                    // Costuras e botoes frontais do corpete
                    if (v < 0.28f && Mathf.Abs(u - 0.5f) < 0.006f)
                    {
                        col = Color.Lerp(col, sepiaLine, 0.38f);
                    }

                    // Guirlanda de florzinhas de 5 petalas coral com folhinhas verdes
                    if (v > 0.56f && v < 0.76f)
                    {
                        float cellU = Mathf.Repeat(u * 14f, 1f) - 0.5f;
                        float cellV = (v - 0.66f) / 0.075f;
                        float r = Mathf.Sqrt(cellU * cellU + cellV * cellV);
                        float ang = Mathf.Atan2(cellV, cellU);
                        float petalRadius = 0.28f + 0.12f * Mathf.Cos(ang * 5f);
                        if (r < 0.09f)
                        {
                            col = flowerCenter;
                        }
                        else if (r < petalRadius)
                        {
                            col = Color.Lerp(coralRed, coralFlower, r / petalRadius);
                        }
                        else if (r < 0.44f && Mathf.Abs(Mathf.Sin(ang * 2f)) > 0.82f)
                        {
                            col = Color.Lerp(col, leafGreen, 0.75f);
                        }
                    }

                    // Barra ondulada coral-carmim + babado de renda bordada
                    float wave = 0.018f * Mathf.Sin(u * Pi * 2f * 18f);
                    float vWave = v + wave;
                    if (vWave >= 0.78f && vWave <= 0.90f)
                    {
                        col = coralRed;
                        if (Mathf.Abs(vWave - 0.78f) < 0.008f || Mathf.Abs(vWave - 0.90f) < 0.008f)
                        {
                            col = Color.Lerp(col, sepiaLine, 0.65f);
                        }
                    }
                    else if (vWave > 0.90f)
                    {
                        float lacePleat = 0.5f + 0.5f * Mathf.Sin(u * Pi * 2f * 36f);
                        col = Color.Lerp(foldShadow, laceCream, lacePleat);
                        float eyeletU = Mathf.Repeat(u * 36f, 1f) - 0.5f;
                        float eyeletV = (vWave - 0.94f) / 0.025f;
                        if (eyeletU * eyeletU + eyeletV * eyeletV < 0.16f)
                        {
                            col = Color.Lerp(col, foldShadow, 0.65f);
                        }
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GeneratePaintedHairTexture(string path)
        {
            const int size = 1024;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color deepEspresso = new Color(0.17f, 0.08f, 0.05f);
            Color richChestnut = new Color(0.38f, 0.19f, 0.11f);
            Color warmCaramel = new Color(0.62f, 0.36f, 0.20f);
            Color goldenHalo = new Color(0.78f, 0.50f, 0.28f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float lockGroup = 0.5f + 0.5f * Mathf.Sin(u * Pi * 2f * 18f);
                    float fineStrands = Mathf.PerlinNoise(u * 90f, v * 10f) * 0.6f
                        + Mathf.PerlinNoise(u * 210f, v * 20f) * 0.4f;
                    Color col = Color.Lerp(deepEspresso, richChestnut, fineStrands * (0.4f + 0.6f * lockGroup));

                    float halo1 = Mathf.Exp(-Mathf.Pow((v - 0.62f - 0.035f * Mathf.Sin(u * Pi * 20f)) / 0.09f, 2f));
                    float halo2 = Mathf.Exp(-Mathf.Pow((v - 0.32f + 0.025f * Mathf.Cos(u * Pi * 16f)) / 0.07f, 2f));
                    col = Color.Lerp(col, warmCaramel, (halo1 + halo2 * 0.6f) * fineStrands);
                    col = Color.Lerp(col, goldenHalo, halo1 * Mathf.Pow(fineStrands, 2f) * 0.55f);

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GeneratePaintedNandaFaceTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color fairySkin = new Color(0.88f, 0.97f, 1.0f);
            Color cyanBlush = new Color(0.52f, 0.86f, 1.0f);
            Color sapphireIris = new Color(0.12f, 0.48f, 0.88f);
            Color cyanGlow = new Color(0.45f, 0.95f, 1.0f);
            Color deepNavy = new Color(0.06f, 0.18f, 0.38f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    Color col = fairySkin;

                    for (int side = -1; side <= 1; side += 2)
                    {
                        float bu = (u - (0.5f + side * 0.095f)) / 0.042f;
                        float bv = (v - 0.58f) / 0.055f;
                        if (bu * bu + bv * bv < 1f)
                        {
                            col = Color.Lerp(col, cyanBlush, (1f - (bu * bu + bv * bv)) * 0.55f);
                        }

                        float eyeCx = 0.5f + side * 0.076f;
                        float eyeCy = 0.50f;
                        float eu = (u - eyeCx) / 0.038f;
                        float ev = (v - eyeCy) / 0.078f;
                        float ed = Mathf.Sqrt(eu * eu + ev * ev);
                        if (ed < 1f)
                        {
                            Color eyeCol = Color.Lerp(sapphireIris, cyanGlow, Mathf.Clamp01((ev + 0.8f) / 1.6f));
                            if (ed < 0.42f)
                            {
                                eyeCol = deepNavy;
                            }

                            float gu = (u - (eyeCx - 0.010f)) / 0.011f;
                            float gv = (v - (eyeCy - 0.022f)) / 0.022f;
                            if (gu * gu + gv * gv < 1f)
                            {
                                eyeCol = Color.white;
                            }

                            col = Color.Lerp(col, eyeCol, Mathf.SmoothStep(1f, 0.85f, ed));
                        }
                    }

                    float mu = (u - 0.5f) / 0.028f;
                    if (Mathf.Abs(mu) < 1f && Mathf.Abs(v - (0.66f - 0.012f * mu * mu)) < 0.008f)
                    {
                        col = Color.Lerp(col, sapphireIris, 0.75f);
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GeneratePaintedNandaWingTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color cellTint = new Color(0.58f, 0.92f, 1.0f);
            Color veinBright = new Color(0.92f, 0.99f, 1.0f);
            Color rimSapphire = new Color(0.22f, 0.64f, 0.96f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float border = Mathf.Max(Mathf.Abs(u - 0.5f) * 2f, Mathf.Abs(v - 0.5f) * 2f);
                    Color col = Color.Lerp(cellTint, rimSapphire, Mathf.Pow(border, 1.8f));

                    // Nervuras de filigrana de asa de fada
                    float radialVein = Mathf.Abs(Mathf.Sin((u - 0.5f) * Pi * 7f + v * 2.2f));
                    float crossVein = Mathf.Abs(Mathf.Sin(v * Pi * 9f + Mathf.Sin(u * Pi * 4f)));
                    if (radialVein < 0.12f || crossVein < 0.10f || border > 0.88f)
                    {
                        col = Color.Lerp(col, veinBright, 0.85f);
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GeneratePaintedNandaDressTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color topAzure = new Color(0.36f, 0.78f, 0.98f);
            Color bottomIce = new Color(0.82f, 0.97f, 1.0f);
            Color petalEdge = new Color(0.96f, 1.0f, 1.0f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float pleat = 0.5f + 0.5f * Mathf.Sin(u * Pi * 2f * 8f);
                    Color col = Color.Lerp(topAzure, bottomIce, v * (0.7f + 0.3f * pleat));
                    if (v > 0.84f)
                    {
                        col = Color.Lerp(col, petalEdge, (v - 0.84f) / 0.16f);
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        public static Mesh CreateEllipsoidMesh(string name, Vector3 center, Vector3 radius, int segments)
        {
            return CreateParametricMesh(name, segments, segments / 2, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = (u - 0.5f) * 2f * Pi;
                return center + new Vector3(
                    radius.x * Mathf.Sin(p) * Mathf.Sin(a),
                    radius.y * Mathf.Cos(p),
                    radius.z * Mathf.Sin(p) * Mathf.Cos(a));
            });
        }

        public static Mesh CreateCurveTubeMesh(
            string name,
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            float radius,
            float flattenZ,
            int sides,
            int segments)
        {
            return CreateParametricMesh(name, sides, segments, (u, v) =>
            {
                float t = Mathf.Clamp(v, 0.001f, 0.999f);
                Vector3 center = Bezier(p0, p1, p2, p3, t);
                Vector3 tangent = (Bezier(p0, p1, p2, p3, Mathf.Min(1f, t + 0.002f))
                    - Bezier(p0, p1, p2, p3, Mathf.Max(0f, t - 0.002f))).normalized;
                Vector3 refAxis = Mathf.Abs(Vector3.Dot(tangent, Vector3.right)) > 0.95f ? Vector3.up : Vector3.right;
                Vector3 bx = Vector3.Cross(tangent, refAxis).normalized;
                Vector3 bz = Vector3.Cross(tangent, bx).normalized;
                float taper = Mathf.Pow(Mathf.Sin(Pi * t), 0.38f) * (1f - 0.45f * t);
                float r = radius * taper;
                float a = u * 2f * Pi;
                return center + r * (Mathf.Cos(a) * bx + Mathf.Sin(a) * flattenZ * bz);
            });
        }

        private static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            float s = 1f - t;
            return s * s * s * a + 3f * s * s * t * b + 3f * s * t * t * c + t * t * t * d;
        }

        public static Mesh CreateParametricMesh(
            string meshName,
            int nu,
            int nv,
            Func<float, float, Vector3> evalPoint,
            bool blendArcSysFaceNormals = false)
        {
            var verts = new List<Vector3>((nu + 1) * (nv + 1));
            var norms = new List<Vector3>((nu + 1) * (nv + 1));
            var uvs = new List<Vector2>((nu + 1) * (nv + 1));
            var tris = new List<int>(nu * nv * 6);

            const float eps = 0.001f;
            Vector3 centroid = Vector3.zero;
            for (int j = 0; j <= nv; j++)
            {
                float v = (float)j / nv;
                for (int i = 0; i <= nu; i++)
                {
                    float u = (float)i / nu;
                    Vector3 p = evalPoint(u, v);
                    Vector3 du = evalPoint(u + eps, v) - evalPoint(u - eps, v);
                    Vector3 dv = evalPoint(u, Mathf.Min(1f, v + eps)) - evalPoint(u, Mathf.Max(0f, v - eps));
                    Vector3 n = Vector3.Cross(du.normalized, dv.normalized).normalized;
                    if (n.sqrMagnitude < 0.1f)
                    {
                        n = p.normalized;
                    }

                    verts.Add(p);
                    norms.Add(n);
                    uvs.Add(new Vector2(u, v));
                    centroid += p;
                }
            }

            if (verts.Count > 0)
            {
                centroid /= verts.Count;
            }

            Vector3 minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int k = 0; k < verts.Count; k++)
            {
                minBounds = Vector3.Min(minBounds, verts[k]);
                maxBounds = Vector3.Max(maxBounds, verts[k]);
            }

            float spanX = maxBounds.x - minBounds.x;
            float spanY = maxBounds.y - minBounds.y;
            float spanZ = maxBounds.z - minBounds.z;
            float avgAbsNy = 0f;
            for (int k = 0; k < norms.Count; k++)
            {
                avgAbsNy += Mathf.Abs(norms[k].y);
            }

            if (norms.Count > 0)
            {
                avgAbsNy /= norms.Count;
            }

            bool isShallowHorizontalSheet = avgAbsNy > 0.60f && spanY < 0.32f * Mathf.Max(spanX, spanZ);

            float outwardAlignment = 0f;
            for (int k = 0; k < verts.Count; k++)
            {
                if (isShallowHorizontalSheet)
                {
                    outwardAlignment += norms[k].y;
                }
                else
                {
                    outwardAlignment += Vector3.Dot(norms[k], verts[k] - centroid);
                }
            }

            bool flipWinding = outwardAlignment < 0f;
            for (int k = 0; k < verts.Count; k++)
            {
                Vector3 n = flipWinding ? -norms[k] : norms[k];
                Vector3 p = verts[k];
                if (blendArcSysFaceNormals && p.z > 0f)
                {
                    Vector3 proxyNormal = new Vector3(p.x * 1.4f, (p.y - 0.16f) * 0.85f + 0.22f, 0.88f).normalized;
                    n = Vector3.Slerp(n, proxyNormal, 0.68f).normalized;
                }

                norms[k] = n;
            }

            for (int j = 0; j < nv; j++)
            {
                for (int i = 0; i < nu; i++)
                {
                    int a = j * (nu + 1) + i;
                    int b = a + nu + 1;
                    if (flipWinding)
                    {
                        tris.Add(a);
                        tris.Add(b);
                        tris.Add(a + 1);
                        tris.Add(a + 1);
                        tris.Add(b);
                        tris.Add(b + 1);
                    }
                    else
                    {
                        tris.Add(a);
                        tris.Add(a + 1);
                        tris.Add(b);
                        tris.Add(a + 1);
                        tris.Add(b + 1);
                        tris.Add(b);
                    }
                }
            }

            var mesh = new Mesh { name = meshName };
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        public static GameObject AttachMesh(string name, Transform parent, Mesh mesh, Material mat)
        {
            Directory.CreateDirectory(ModelsFolder + "/GeneratedMeshes");
            string assetPath = $"{ModelsFolder}/GeneratedMeshes/{mesh.name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, assetPath);
                existing = mesh;
            }
            else
            {
                existing.Clear();
                existing.vertices = mesh.vertices;
                existing.normals = mesh.normals;
                existing.uv = mesh.uv;
                existing.triangles = mesh.triangles;
                existing.RecalculateBounds();
                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(mesh);
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = existing;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        private static Transform FindOrCreatePivot(Transform root, string name, Vector3 fallbackLocalPos)
        {
            var found = root.Find(name);
            if (found != null)
            {
                return found;
            }

            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.localPosition = fallbackLocalPos;
            return go.transform;
        }
    }
}
