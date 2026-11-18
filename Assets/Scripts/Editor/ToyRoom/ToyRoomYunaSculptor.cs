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
        public const string CustomModelPathObj = ModelsFolder + "/Yuna_ToyRoom.obj";
        public const string CustomModelPathGlb = ModelsFolder + "/Yuna_ToyRoom.glb";
        public const string CustomModelAlbedoPath = ModelsFolder + "/Yuna_ToyRoom_Albedo.png";

        private const string FaceTexPath = "Assets/Art/ToyRoom/Materials/T_Yuna_Face_Painted.png";
        private const string DressTexPath = "Assets/Art/ToyRoom/Materials/T_Yuna_Dress_Painted.png";
        private const string CollarSleeveTexPath = "Assets/Art/ToyRoom/Materials/T_Yuna_CollarSleeve_Painted.png";
        private const string CoralRibbonTexPath = "Assets/Art/ToyRoom/Materials/T_Yuna_CoralRibbon_Painted.png";
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
            EnsurePaintedTextures(forceRegenerate: true);

            var bodyGo = new GameObject("Yuna_BodyRoot");
            bodyGo.transform.SetParent(parent, false);
            // Escala de fantasia um pouco maior para destacar Yuna na sala sem fechar o enquadramento
            bodyGo.transform.localScale = Vector3.one * 1.28f;
            bodyRoot = bodyGo.transform;

            var customPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CustomModelPathFbx)
                ?? AssetDatabase.LoadAssetAtPath<GameObject>(CustomModelPathObj)
                ?? AssetDatabase.LoadAssetAtPath<GameObject>(CustomModelPathGlb);
            if (customPrefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(customPrefab, bodyRoot);
                inst.name = "Yuna_ImportedMesh";
                if (File.Exists(CustomModelAlbedoPath))
                {
                    var importedMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, CustomModelAlbedoPath, outlineWidth: 0.0025f);
                    importedMat.SetColor("_ShadowColor", new Color(0.76f, 0.64f, 0.62f, 1f));
                    EditorUtility.SetDirty(importedMat);
                    foreach (var mr in inst.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        mr.sharedMaterial = importedMat;
                    }
                }
                ToyRoomYunaRigBuilder.AttachSkinnedRig(
                    inst,
                    bodyRoot,
                    out _,
                    out legLeftPivot,
                    out legRightPivot,
                    out armLeftPivot,
                    out armRightPivot,
                    out ponytailPivot);
                return;
            }

            // Materiais pictoricos com texturas 1024x1024 pintadas a mao
            var faceMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, FaceTexPath, outlineWidth: 0.0030f);
            var skinMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.98f, 0.83f, 0.72f), outlineWidth: 0.0030f);
            var hairMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, HairTexPath, outlineWidth: 0.0026f);
            hairMat.SetColor("_ShadowColor", new Color(0.78f, 0.62f, 0.52f, 1f));
            hairMat.SetColor("_RimColor", new Color(1.0f, 0.84f, 0.60f, 1f));
            EditorUtility.SetDirty(hairMat);

            var hairStrandMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, HairTexPath, outlineWidth: 0.0007f);
            hairStrandMat.SetColor("_ShadowColor", new Color(0.78f, 0.62f, 0.52f, 1f));
            hairStrandMat.SetColor("_RimColor", new Color(1.0f, 0.84f, 0.60f, 1f));
            hairStrandMat.SetColor("_OutlineColor", new Color(0.28f, 0.13f, 0.08f, 1f));
            EditorUtility.SetDirty(hairStrandMat);

            var dressMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, DressTexPath, outlineWidth: 0.0035f);
            var collarSleeveMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, CollarSleeveTexPath, outlineWidth: 0.0032f);
            var coralRibbonMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, CoralRibbonTexPath, outlineWidth: 0.0028f);
            var scrunchieMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, CoralRibbonTexPath, outlineWidth: 0.0032f);
            var sockMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.95f, 0.90f, 0.80f), outlineWidth: 0.0028f);
            var shoeMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.50f, 0.13f, 0.15f), outlineWidth: 0.0035f);
            var soleMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.24f, 0.13f, 0.10f), outlineWidth: 0.0025f);

            // 1. Corpo / Vestido Esculpido (Busto + Saia Plissada em Sino + Gola Peter Pan + Laco Coral)
            BuildTorsoAndSkirt(bodyRoot, dressMat, collarSleeveMat, coralRibbonMat, skinMat);

            // 2. Cabeca Expressiva com Rosto Pintado a Mao
            var headRoot = new GameObject("Head_Root").transform;
            headRoot.SetParent(bodyRoot, false);
            headRoot.localPosition = new Vector3(0f, 0.935f, 0.01f);
            headRoot.localRotation = Quaternion.Euler(-4f, 0f, 0f);

            BuildSculptedHeadAndFace(headRoot, faceMat, skinMat, hairMat, hairStrandMat);

            // 3. Rabo de Cavalo Alto Cacheado Volumoso + Xuxinha Coral-Rosa Frisada
            ponytailPivot = new GameObject("Ponytail_Pivot").transform;
            ponytailPivot.SetParent(headRoot, false);
            ponytailPivot.localPosition = new Vector3(-0.010f, 0.285f, -0.125f);
            BuildScrunchieAndCurlyPonytail(ponytailPivot, scrunchieMat, hairMat, hairStrandMat);

            // 4. Bracos Articulados com Mangas Bufantes Plissadas e Maozinhas Esculpidas
            armLeftPivot = new GameObject("Arm_L_Pivot").transform;
            armLeftPivot.SetParent(bodyRoot, false);
            armLeftPivot.localPosition = new Vector3(-0.132f, 0.865f, 0f);
            BuildSculptedArm(armLeftPivot, -1, collarSleeveMat, coralRibbonMat, skinMat);

            armRightPivot = new GameObject("Arm_R_Pivot").transform;
            armRightPivot.SetParent(bodyRoot, false);
            armRightPivot.localPosition = new Vector3(0.132f, 0.865f, 0f);
            BuildSculptedArm(armRightPivot, 1, collarSleeveMat, coralRibbonMat, skinMat);

            // 5. Pernas Articuladas com Meias Caneladas de Babado e Sapatos Mary Jane Vinho
            legLeftPivot = new GameObject("Leg_L_Pivot").transform;
            legLeftPivot.SetParent(bodyRoot, false);
            legLeftPivot.localPosition = new Vector3(-0.075f, 0.44f, 0f);
            BuildSculptedLegAndShoe(legLeftPivot, -1, skinMat, sockMat, shoeMat, soleMat);

            legRightPivot = new GameObject("Leg_R_Pivot").transform;
            legRightPivot.SetParent(bodyRoot, false);
            legRightPivot.localPosition = new Vector3(0.075f, 0.44f, 0f);
            BuildSculptedLegAndShoe(legRightPivot, 1, skinMat, sockMat, shoeMat, soleMat);
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
            Material skinMat)
        {
            // Pescoco continuo ate o busto (sem buraco na gola)
            var neckMesh = CreateParametricMesh("Yuna_Neck", 24, 10, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float y = Mathf.Lerp(0.86f, 0.97f, v);
                float r = Mathf.Lerp(0.056f, 0.044f, v);
                return new Vector3(r * Mathf.Sin(a), y, r * 0.95f * Mathf.Cos(a));
            });
            AttachMesh("Neck", bodyRoot, neckMesh, skinMat);

            // Busto com pregas convergindo ACIMA do cinto + Saia com pregas nascendo ABAIXO do cinto
            var dressMesh = CreateParametricMesh("Yuna_DressBell", 96, 54, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                if (v < 0.28f)
                {
                    float tv = v / 0.28f;
                    float y = Mathf.Lerp(0.922f, 0.72f, tv);
                    // O corpete abre um pouco no peito e franze ao entrar no cinto
                    float bustCurve = Mathf.Sin(tv * Pi * 0.65f);
                    float rx = Mathf.Lerp(0.062f, 0.118f, bustCurve);
                    float rz = Mathf.Lerp(0.056f, 0.100f, bustCurve);
                    // Pregas suaves no corpete acima do cinto (convergindo na cintura)
                    float upperGatherAmp = 0.042f * Mathf.SmoothStep(0.18f, 0.92f, tv);
                    float upperPleats = 1f + upperGatherAmp * (0.65f * Mathf.Cos(a * 10f) + 0.35f * Mathf.Cos(a * 20f));
                    return new Vector3(rx * upperPleats * Mathf.Sin(a), y, rz * upperPleats * Mathf.Cos(a));
                }
                else
                {
                    float sv = (v - 0.28f) / 0.72f;
                    float y = Mathf.Lerp(0.72f, 0.34f, sv);
                    float flare = Mathf.Pow(sv, 0.76f);
                    float baseRx = Mathf.Lerp(0.118f, 0.318f, flare);
                    float baseRz = Mathf.Lerp(0.098f, 0.288f, flare);

                    // Pregas principais (10) + franzido fino logo abaixo do cinto (20)
                    float mainPleat = Mathf.Cos(a * 10f);
                    float fineGather = Mathf.Cos(a * 20f) * (1f - 0.55f * sv);
                    float pleatShape = 0.68f * mainPleat + 0.32f * fineGather;
                    float pleatAmp = Mathf.Lerp(0.040f, 0.070f, Mathf.Pow(sv, 0.62f));
                    float pleats = 1f + pleatAmp * pleatShape;

                    float hemWave = sv > 0.84f ? 0.015f * Mathf.Sin(a * 20f) * ((sv - 0.84f) / 0.16f) : 0f;
                    return new Vector3(baseRx * pleats * Mathf.Sin(a), y + hemWave, baseRz * pleats * Mathf.Cos(a));
                }
            });
            AttachMesh("Dress_BellSkirt", bodyRoot, dressMesh, dressMat);

            // Anagua/babado creme franzido visivel sob a barra da saia
            var underskirtMesh = CreateParametricMesh("Yuna_UnderskirtRuffle", 80, 12, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float y = Mathf.Lerp(0.365f, 0.298f, v);
                float r = Mathf.Lerp(0.268f, 0.336f, v) * (1f + 0.055f * Mathf.Cos(a * 24f));
                return new Vector3(r * Mathf.Sin(a), y, r * 0.91f * Mathf.Cos(a));
            });
            AttachMesh("Dress_UnderskirtRuffle", bodyRoot, underskirtMesh, ivoryMat);

            // Cinto em linho creme com costuras horizontais, apertando entre as pregas de cima e de baixo
            var sashMesh = CreateParametricMesh("Yuna_WaistSash", 64, 12, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float y = Mathf.Lerp(0.750f, 0.700f, v);
                float stitchRidge = 1f + 0.028f * Mathf.Sin(v * Pi * 5f);
                return new Vector3(0.112f * stitchRidge * Mathf.Sin(a), y, 0.093f * stitchRidge * Mathf.Cos(a));
            });
            AttachMesh("Waist_Sash", bodyRoot, sashMesh, ivoryMat);

            // Gola Peter Pan fofa em linho creme com pregas radiais e encontro frontal fechado para o laco
            var collarMesh = CreateParametricMesh("Yuna_Collar", 84, 16, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float frontNotch = 1f - 0.30f * Mathf.Exp(-Mathf.Pow(a / 0.19f, 2f));
                float scallop = 1f + 0.042f * Mathf.Cos(a * 14f);
                float r = Mathf.Lerp(0.054f, 0.148f, v) * scallop * frontNotch;
                float archOverShoulders = 0.012f * Mathf.Sin(v * Pi);
                float pleatWave = 0.008f * Mathf.Cos(a * 14f) * Mathf.Pow(v, 0.7f);
                float y = 0.926f - 0.048f * Mathf.Pow(v, 1.05f) + archOverShoulders + pleatWave;
                return new Vector3(r * Mathf.Sin(a), y, r * 0.88f * Mathf.Cos(a));
            });
            AttachMesh("PeterPan_Collar", bodyRoot, collarMesh, ivoryMat);

            // Enfeite/debrum coral ondulado acompanhando toda a borda da gola (inclusive o encontro frontal)
            var collarTrimMesh = CreateParametricMesh("Yuna_CollarTrim", 84, 10, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float b = v * 2f * Pi;
                float frontNotch = 1f - 0.30f * Mathf.Exp(-Mathf.Pow(a / 0.19f, 2f));
                float scallop = 1f + 0.045f * Mathf.Cos(a * 14f);
                float baseR = 0.148f * scallop * frontNotch;
                float tubeR = 0.0085f;
                float r = baseR + tubeR * Mathf.Cos(b);
                float y = 0.878f + 0.008f * Mathf.Cos(a * 14f) + 0.022f * Mathf.Exp(-Mathf.Pow(a / 0.19f, 2f)) + tubeR * Mathf.Sin(b);
                return new Vector3(r * Mathf.Sin(a), y, r * 0.88f * Mathf.Cos(a));
            });
            AttachMesh("PeterPan_CollarTrim", bodyRoot, collarTrimMesh, coralMat);

            // Gravatinha / Laco de fita coral-salmao assentado SOBRE a golinha e o enfeite vermelho
            var bowKnotMesh = CreateEllipsoidMesh("ChestBow_Knot", new Vector3(0f, 0.892f, 0.104f), new Vector3(0.018f, 0.017f, 0.017f), 16);
            AttachMesh("ChestBow_Knot", bodyRoot, bowKnotMesh, coralMat);

            for (int side = -1; side <= 1; side += 2)
            {
                // Alca larga achatada sobre a gola, abrindo em leque com vinco central franzido
                var loopMesh = CreateParametricMesh($"ChestBow_Loop_{side}", 28, 14, (u, v) =>
                {
                    float s = Mathf.Sin(v * Pi);
                    float fan = Mathf.Lerp(0.45f, 1.25f, v);
                    float pinch = 1f - 0.35f * Mathf.Exp(-Mathf.Pow((u - 0.25f) * 4f, 2f));
                    return new Vector3(
                        side * (0.010f + 0.056f * v),
                        0.894f + 0.025f * s * fan * pinch * Mathf.Sin(u * 2f * Pi) - 0.008f * v,
                        0.104f + 0.016f * v + 0.012f * s * Mathf.Cos(u * 2f * Pi));
                });
                AttachMesh($"ChestBow_Loop_{side}", bodyRoot, loopMesh, coralMat);

                // Ponta da gravatinha passando SOBRE a borda da gola e do enfeite coral
                var ribbonMesh = CreateCurveTubeMesh(
                    $"ChestBow_Tail_{side}",
                    new Vector3(side * 0.008f, 0.884f, 0.108f),
                    new Vector3(side * 0.022f, 0.854f, 0.120f),
                    new Vector3(side * 0.032f, 0.824f, 0.124f),
                    new Vector3(side * 0.038f, 0.798f, 0.118f),
                    0.017f,
                    0.28f,
                    14,
                    14);
                AttachMesh($"ChestBow_Tail_{side}", bodyRoot, ribbonMesh, coralMat);
            }
        }

        private static Vector3 EvaluateYunaHeadPoint(float u, float v, float outwardOffset = 0f)
        {
            float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
            float a = (u - 0.5f) * 2f * Pi;
            Vector3 q = new Vector3(Mathf.Sin(p) * Mathf.Sin(a), Mathf.Cos(p), Mathf.Sin(p) * Mathf.Cos(a));

            // Bochechas infantis bem cheinhas + queixo curto e arredondado (proporcao storybook em 3/4)
            float cheekBulge = 1f + 0.140f * Mathf.Exp(-Mathf.Pow((q.y + 0.22f) * 2.8f, 2f)) * Mathf.Max(0f, q.z);
            float chinTaper = Mathf.SmoothStep(0.80f, 1.0f, Mathf.InverseLerp(-1.0f, -0.16f, q.y));
            q.x *= chinTaper * cheekBulge;
            q.z *= cheekBulge;

            // Queixo mais curto para proporcao infantil fofa em 3/4
            float yScale = q.y < 0f ? 0.142f : 0.175f;
            Vector3 pos = new Vector3(0f, 0.155f, 0.010f) + new Vector3(q.x * 0.194f, q.y * yScale, q.z * 0.176f);

            // Narizinho arrebitado delicado logo abaixo da linha dos olhos
            if (q.z > 0.2f)
            {
                float nose = 0.017f * Mathf.Exp(-Mathf.Pow(pos.x / 0.018f, 2f) - Mathf.Pow((pos.y - 0.128f) / 0.018f, 2f));
                pos.z += nose;
            }

            if (Mathf.Abs(outwardOffset) > 0.0001f)
            {
                Vector3 radial = (pos - new Vector3(0f, 0.155f, 0.010f)).normalized;
                pos += radial * outwardOffset;
            }

            return pos;
        }

        private static void BuildSculptedHeadAndFace(
            Transform headRoot,
            Material faceMat,
            Material skinMat,
            Material hairMat,
            Material hairStrandMat)
        {
            // 1. Cabeca Esculpida com UV Centrada na Face e Textura Facial 1024x1024
            var headMesh = CreateParametricMesh("Yuna_Head", 72, 40, (u, v) => EvaluateYunaHeadPoint(u, v, 0f), blendArcSysFaceNormals: true);
            AttachMesh("Head_Mesh", headRoot, headMesh, faceMat);

            // 2. Orelhas Esculpidas bem visiveis na frente dos cachos da nuca
            for (int side = -1; side <= 1; side += 2)
            {
                var earGo = AttachMesh(
                    $"Ear_{side}",
                    headRoot,
                    CreateEllipsoidMesh(
                        $"Ear_{side}",
                        new Vector3(side * 0.194f, 0.128f, 0.002f),
                        new Vector3(0.030f, 0.046f, 0.024f),
                        18),
                    skinMat);
                earGo.transform.localRotation = Quaternion.Euler(0f, side * -18f, side * 12f);
            }

            // 3. Domo de Cabelo Cheio, Macio e Arredondado (sem ondulacao artificial de "macarrao")
            var hairCap = CreateParametricMesh("Yuna_HairCap", 80, 36, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float frontFaceMask = Mathf.Exp(-Mathf.Pow(a / 1.02f, 2f));
                float maxPhi = Mathf.Lerp(2.42f, 1.04f, frontFaceMask);
                float phi = 0.001f + v * maxPhi;
                float crownLift = Mathf.Exp(-phi * 1.4f) * 0.022f;
                float gentleLocks = 1f + 0.009f * Mathf.Cos(a * 6f);
                return new Vector3(
                    0.222f * Mathf.Sin(phi) * Mathf.Sin(a) * gentleLocks,
                    0.192f + (0.232f + crownLift) * Mathf.Cos(phi),
                    -0.010f + 0.212f * Mathf.Sin(phi) * Mathf.Cos(a) * gentleLocks);
            });
            AttachMesh("Hair_Cap", headRoot, hairCap, hairMat);

            // 4. Mechas em Fita (Ribbon Locks) assentadas no topo da cabeca (sem levantar do cranio)
            Vector3 crownOrigin = new Vector3(-0.012f, 0.415f, -0.015f);
            for (int c = 0; c < 10; c++)
            {
                float ang = c * 2f * Pi / 10f + 0.20f;
                float dirX = Mathf.Sin(ang);
                float dirZ = Mathf.Cos(ang);
                float reach = dirZ > 0.30f ? 0.148f : 0.212f;
                float dropY = dirZ > 0.30f ? 0.318f : 0.205f;

                var crownLock = CreateHairRibbonLockMesh(
                    $"CrownLock_{c}",
                    crownOrigin,
                    crownOrigin + new Vector3(dirX * 0.09f, 0.006f, dirZ * 0.09f),
                    new Vector3(dirX * reach * 0.85f, Mathf.Lerp(0.40f, dropY, 0.48f), dirZ * reach * 0.85f - 0.008f),
                    new Vector3(dirX * reach, dropY, dirZ * reach - 0.008f),
                    0.062f,
                    0.014f,
                    0f,
                    14,
                    20);
                AttachMesh($"CrownLock_{c}", headRoot, crownLock, hairStrandMat);
            }

            // 5. Franja Macia e Cheia Abracando a Testa com Gravidade Natural (zero pontas arrepiadas)
            var bangSpecs = new (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float w, float th, float hook)[]
            {
                // Onda principal varrida para a direita com caimento macio junto ao rosto
                (new Vector3(-0.02f, 0.402f, 0.065f), new Vector3(0.03f, 0.370f, 0.178f), new Vector3(0.075f, 0.302f, 0.194f), new Vector3(0.098f, 0.246f, 0.174f), 0.074f, 0.018f, 0.010f),
                (new Vector3(0.01f, 0.398f, 0.060f), new Vector3(0.078f, 0.362f, 0.170f), new Vector3(0.128f, 0.295f, 0.180f), new Vector3(0.155f, 0.240f, 0.150f), 0.070f, 0.018f, 0.012f),
                (new Vector3(0.04f, 0.390f, 0.050f), new Vector3(0.125f, 0.348f, 0.148f), new Vector3(0.175f, 0.280f, 0.152f), new Vector3(0.196f, 0.230f, 0.120f), 0.064f, 0.017f, 0.010f),
                // Onda lateral esquerda emoldurando a testa
                (new Vector3(-0.025f, 0.402f, 0.065f), new Vector3(-0.055f, 0.366f, 0.174f), new Vector3(-0.090f, 0.295f, 0.188f), new Vector3(-0.112f, 0.244f, 0.164f), 0.070f, 0.018f, -0.010f),
                (new Vector3(-0.045f, 0.395f, 0.055f), new Vector3(-0.108f, 0.354f, 0.158f), new Vector3(-0.155f, 0.280f, 0.162f), new Vector3(-0.178f, 0.228f, 0.128f), 0.066f, 0.018f, -0.010f),
                // Mecha central suave sobre a testa
                (new Vector3(-0.015f, 0.404f, 0.070f), new Vector3(0.005f, 0.372f, 0.182f), new Vector3(0.026f, 0.312f, 0.198f), new Vector3(0.042f, 0.264f, 0.184f), 0.058f, 0.016f, 0.006f)
            };
            for (int b = 0; b < bangSpecs.Length; b++)
            {
                var s = bangSpecs[b];
                var bangMesh = CreateHairRibbonLockMesh($"BangRibbon_{b}", s.p0, s.p1, s.p2, s.p3, s.w, s.th, s.hook, 14, 22);
                AttachMesh($"BangRibbon_{b}", headRoot, bangMesh, hairStrandMat);
            }

            // 6. Mecha em C na Bochecha (na frente da orelha) + Cachinhos Suaves na Nuca (abaixo da orelha)
            for (int side = -1; side <= 1; side += 2)
            {
                var cheekCurl = CreateHairRibbonLockMesh(
                    $"CheekCurl_{side}",
                    new Vector3(side * 0.165f, 0.265f, 0.062f),
                    new Vector3(side * 0.194f, 0.195f, 0.080f),
                    new Vector3(side * 0.184f, 0.122f, 0.084f),
                    new Vector3(side * 0.154f, 0.095f, 0.096f),
                    0.036f,
                    0.013f,
                    -side * 0.010f,
                    12,
                    20);
                AttachMesh($"CheekCurl_{side}", headRoot, cheekCurl, hairStrandMat);

                for (int n = 0; n < 3; n++)
                {
                    float ny = 0.095f - n * 0.018f;
                    float nz = -0.055f - n * 0.032f;
                    var napeCurl = CreateHairRibbonLockMesh(
                        $"NapeHookCurl_{side}_{n}",
                        new Vector3(side * 0.150f, ny + 0.025f, nz),
                        new Vector3(side * 0.188f, ny - 0.015f, nz - 0.015f),
                        new Vector3(side * 0.184f, ny - 0.048f, nz + 0.012f),
                        new Vector3(side * 0.194f, ny - 0.036f, nz + 0.028f),
                        0.040f - n * 0.004f,
                        0.016f,
                        side * 0.014f,
                        12,
                        18);
                    AttachMesh($"NapeHookCurl_{side}_{n}", headRoot, napeCurl, hairStrandMat);
                }
            }
        }

        private static void BuildScrunchieAndCurlyPonytail(
            Transform ponytailPivot,
            Material scrunchieMat,
            Material hairMat,
            Material hairStrandMat)
        {
            // Xuxinha coral-rosa fofa e franzida
            var scrunchieMesh = CreateParametricMesh("Yuna_Scrunchie", 56, 22, (u, v) =>
            {
                float a = u * 2f * Pi;
                float b = v * 2f * Pi;
                float folds = 1f + 0.30f * Mathf.Sin(a * 12f + Mathf.Sin(b * 2f));
                float majorR = 0.102f;
                float minorR = 0.046f * folds;
                float r = majorR + minorR * Mathf.Cos(b);
                return new Vector3(
                    r * Mathf.Cos(a),
                    minorR * 1.15f * Mathf.Sin(b) + 0.014f * Mathf.Cos(a * 12f),
                    r * Mathf.Sin(a) * 0.90f);
            });
            var scGo = AttachMesh("Ruffled_Scrunchie", ponytailPivot, scrunchieMesh, scrunchieMat);
            scGo.transform.localRotation = Quaternion.Euler(46f, -10f, 0f);

            // 1. MASSA CHEIA E VOLUMOSA DE CACHOS (forma de nuvem cheia ao vento, esculpida em gomos/ondas)
            var cloudMassMesh = CreateParametricMesh("Ponytail_CloudMass", 72, 44, (u, v) =>
            {
                float t = Mathf.Clamp(v, 0.001f, 0.999f);
                float a = u * 2f * Pi;

                // Espinha dorsal do volume: sai da xuxinha, arqueia cheio para tras e cai em gota larga arredondada
                Vector3 spine = Bezier(
                    new Vector3(0f, 0.01f, -0.02f),
                    new Vector3(0f, 0.14f, -0.18f),
                    new Vector3(0f, 0.02f, -0.34f),
                    new Vector3(0f, -0.22f, -0.29f),
                    t);

                // Envelope bem cheio e arredondado (largo no meio/baixo como nuvem de cachos, sem pontas finas)
                float env = Mathf.Sin(Pi * Mathf.Pow(t, 0.65f));
                float rx = 0.068f + 0.155f * env;
                float ry = 0.068f + 0.165f * env;

                // Lobulos organicos de cachos esculpidos na propria massa cheia
                float curlLobes = 1f
                    + 0.13f * Mathf.Sin(a * 7f + t * 8.5f) * env
                    + 0.08f * Mathf.Cos(a * 11f - t * 11.0f) * env;

                Vector3 tangent = (Bezier(
                    new Vector3(0f, 0.01f, -0.02f),
                    new Vector3(0f, 0.14f, -0.18f),
                    new Vector3(0f, 0.02f, -0.34f),
                    new Vector3(0f, -0.22f, -0.29f),
                    Mathf.Min(1f, t + 0.005f)) - spine).normalized;
                if (tangent.sqrMagnitude < 0.01f)
                {
                    tangent = Vector3.back;
                }
                Vector3 bx = Vector3.Cross(tangent, Vector3.up).normalized;
                if (bx.sqrMagnitude < 0.01f)
                {
                    bx = Vector3.right;
                }
                Vector3 by = Vector3.Cross(bx, tangent).normalized;

                return spine + bx * (rx * curlLobes * Mathf.Cos(a)) + by * (ry * curlLobes * Mathf.Sin(a));
            });
            AttachMesh("Ponytail_CloudMass", ponytailPivot, cloudMassMesh, hairMat);

            // 2. CACHINHOS EM C / CARACOIS ARREDONDADOS BROTANDO DA BORDA DA MASSA CHEIA (como na foto!)
            for (int i = 0; i < 18; i++)
            {
                float ringT = 0.28f + 0.64f * ((i % 3) / 2f); // distribuidos do topo, meio e base da massa
                float angle = (i * 2f * Pi / 18f) + (i % 3) * 0.45f;
                Vector3 anchor = Bezier(
                    new Vector3(0f, 0.01f, -0.02f),
                    new Vector3(0f, 0.14f, -0.18f),
                    new Vector3(0f, 0.02f, -0.34f),
                    new Vector3(0f, -0.22f, -0.29f),
                    ringT);

                float env = Mathf.Sin(Pi * Mathf.Pow(ringT, 0.65f));
                Vector3 outDir = new Vector3(Mathf.Cos(angle) * 0.145f * env, Mathf.Sin(angle) * 0.145f * env, -0.06f * env);
                Vector3 curlDir = new Vector3(outDir.x * 1.45f, outDir.y * 1.35f - 0.055f, outDir.z - 0.065f);
                float hookSign = (i % 2 == 0) ? 1f : -1f;

                var cCurlMesh = CreateStorybookSpiralCurlMesh(
                    $"Ponytail_RimCurl_{i}",
                    anchor + outDir * 0.45f,
                    anchor + outDir * 1.10f + Vector3.up * 0.03f,
                    anchor + curlDir,
                    anchor + curlDir + new Vector3(hookSign * 0.045f, 0.035f, 0.040f),
                    0.072f,
                    0.044f,
                    0.014f,
                    hookSign * 0.028f,
                    angle,
                    12,
                    20);
                AttachMesh($"Ponytail_RimCurl_{i}", ponytailPivot, cCurlMesh, hairStrandMat);
            }
        }

        private static Mesh CreateHairRibbonLockMesh(
            string name,
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            float width,
            float thickness,
            float tipHookOffset,
            int sides,
            int segments)
        {
            return CreateParametricMesh(name, sides, segments, (u, v) =>
            {
                float t = Mathf.Clamp(v, 0.001f, 0.999f);
                Vector3 center = Bezier(p0, p1, p2, p3, t);
                Vector3 tangent = (Bezier(p0, p1, p2, p3, Mathf.Min(1f, t + 0.003f))
                    - Bezier(p0, p1, p2, p3, Mathf.Max(0f, t - 0.003f))).normalized;
                Vector3 radialOut = center.normalized;
                Vector3 bx = Vector3.Cross(tangent, radialOut).normalized;
                if (bx.sqrMagnitude < 0.01f)
                {
                    bx = Vector3.right;
                }
                Vector3 bz = Vector3.Cross(bx, tangent).normalized;

                // Curvinha em gancho (J-hook) nos ultimos 28% da ponta da mecha
                float hookT = Mathf.Clamp01((t - 0.72f) / 0.28f);
                center += bx * (tipHookOffset * hookT * hookT) + bz * (Mathf.Abs(tipHookOffset) * 0.45f * hookT * hookT);

                float taper = Mathf.Pow(Mathf.Sin(Pi * t), 0.30f) * (1f - 0.82f * Mathf.Pow(t, 2.2f));
                float a = u * 2f * Pi;
                // Secao em fita meia-lua com 3 vincos longitudinais de fios
                float strandGroove = 1f + 0.14f * Mathf.Cos(a * 3f);
                float xOff = Mathf.Cos(a) * width * 0.5f * taper;
                float zOff = (Mathf.Sin(a) * thickness * 0.5f * strandGroove - 0.25f * thickness * Mathf.Cos(a) * Mathf.Cos(a)) * taper;
                return center + bx * xOff + bz * zOff;
            });
        }

        private static Mesh CreateStorybookSpiralCurlMesh(
            string name,
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            float width,
            float thickness,
            float waveAmplitude,
            float endSpiralRadius,
            float phaseOffset,
            int sides,
            int segments)
        {
            return CreateParametricMesh(name, sides, segments, (u, v) =>
            {
                float t = Mathf.Clamp(v, 0.001f, 0.999f);
                Vector3 spine = Bezier(p0, p1, p2, p3, t);
                Vector3 tangent = (Bezier(p0, p1, p2, p3, Mathf.Min(1f, t + 0.003f))
                    - Bezier(p0, p1, p2, p3, Mathf.Max(0f, t - 0.003f))).normalized;
                Vector3 refAxis = Mathf.Abs(Vector3.Dot(tangent, Vector3.right)) > 0.95f ? Vector3.up : Vector3.right;
                Vector3 bx = Vector3.Cross(tangent, refAxis).normalized;
                Vector3 bz = Vector3.Cross(tangent, bx).normalized;

                // Onda S ampla ao longo da mecha + caracol enrolado na ponta (ultimos 35%)
                float sWave = Mathf.Sin(phaseOffset + t * 2.2f * 2f * Pi) * waveAmplitude * Mathf.Sin(Pi * t);
                float cWave = Mathf.Cos(phaseOffset + t * 2.2f * 2f * Pi) * waveAmplitude * 0.55f * Mathf.Sin(Pi * t);
                Vector3 center = spine + bx * sWave + bz * cWave;

                if (t > 0.65f)
                {
                    float st = (t - 0.65f) / 0.35f;
                    float spiralAng = st * 1.65f * Pi;
                    center += bx * (Mathf.Sin(spiralAng) * endSpiralRadius * st)
                        + Vector3.up * ((1f - Mathf.Cos(spiralAng)) * Mathf.Abs(endSpiralRadius) * 0.85f * st);
                }

                float taper = Mathf.Pow(Mathf.Sin(Pi * t), 0.28f) * (1f - 0.78f * Mathf.Pow(t, 2.0f));
                float a = u * 2f * Pi;
                float groove = 1f + 0.16f * Mathf.Cos(a * 4f);
                return center + bx * (Mathf.Cos(a) * width * 0.5f * taper) + bz * (Mathf.Sin(a) * thickness * 0.5f * groove * taper);
            });
        }

        private static void BuildSculptedArm(Transform armPivot, int side, Material ivoryMat, Material coralMat, Material skinMat)
        {
            // Manga bufante em linho texturizado com 10 pregas profundas saindo do ombro e do punho
            var sleeveMesh = CreateParametricMesh($"Sleeve_{side}", 48, 22, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = u * 2f * Pi;
                float gatherAmp = 0.115f * Mathf.Pow(Mathf.Sin(p), 0.75f);
                float gathers = 1f + gatherAmp * Mathf.Cos(a * 10f);
                float puffProfile = 1f + 0.16f * Mathf.Sin(p * 0.85f);
                return new Vector3(
                    side * 0.022f + 0.078f * Mathf.Sin(p) * Mathf.Cos(a) * gathers * puffProfile,
                    -0.042f + 0.072f * Mathf.Cos(p),
                    0.076f * Mathf.Sin(p) * Mathf.Sin(a) * gathers * puffProfile);
            });
            AttachMesh("Puff_Sleeve", armPivot, sleeveMesh, ivoryMat);

            // Elastico/debrum coral franzido abracando o punho da manga
            var cuffBandMesh = CreateParametricMesh($"SleeveCuffBand_{side}", 36, 8, (u, v) =>
            {
                float a = u * 2f * Pi;
                float b = v * Pi;
                float wave = 0.0045f * Mathf.Cos(a * 12f);
                float r = 0.047f + 0.006f * Mathf.Sin(b) + wave;
                return new Vector3(side * 0.032f + r * Mathf.Cos(a), -0.098f - 0.016f * v + 0.002f * Mathf.Sin(a * 12f), r * Mathf.Sin(a));
            });
            AttachMesh("Sleeve_CuffBand", armPivot, cuffBandMesh, coralMat);

            // Babadinho creme ondulado em linho texturizado saindo sob o elastico coral
            var cuffRuffleMesh = CreateParametricMesh($"SleeveCuffRuffle_{side}", 40, 8, (u, v) =>
            {
                float a = u * 2f * Pi;
                float r = 0.048f + 0.017f * v + 0.0085f * Mathf.Cos(a * 12f) * v;
                float yWave = 0.004f * Mathf.Sin(a * 12f) * v;
                return new Vector3(side * 0.032f + r * Mathf.Cos(a), -0.110f - 0.022f * v + yWave, r * Mathf.Sin(a));
            });
            AttachMesh("Sleeve_CuffRuffle", armPivot, cuffRuffleMesh, ivoryMat);

            // Antebraco infantil proporcional com leve curva natural para frente
            var forearm = CreateCurveTubeMesh(
                $"Forearm_{side}",
                new Vector3(side * 0.034f, -0.112f, 0f),
                new Vector3(side * 0.048f, -0.170f, 0.022f),
                new Vector3(side * 0.054f, -0.225f, 0.052f),
                new Vector3(side * 0.048f, -0.268f, 0.082f),
                0.045f,
                0.94f,
                16,
                16);
            AttachMesh("Forearm", armPivot, forearm, skinMat);

            // Maozinha esculpida (palma fofa + polegar + dedinhos dobrados)
            var palmMesh = CreateEllipsoidMesh(
                $"HandPalm_{side}",
                new Vector3(side * 0.048f, -0.284f, 0.095f),
                new Vector3(0.030f, 0.032f, 0.032f),
                16);
            AttachMesh("Hand_Palm", armPivot, palmMesh, skinMat);

            var fingersMesh = CreateEllipsoidMesh(
                $"HandFingers_{side}",
                new Vector3(side * 0.042f, -0.296f, 0.114f),
                new Vector3(0.026f, 0.022f, 0.024f),
                14);
            AttachMesh("Hand_Fingers", armPivot, fingersMesh, skinMat);

            var thumbMesh = CreateEllipsoidMesh(
                $"HandThumb_{side}",
                new Vector3(side * 0.022f, -0.274f, 0.112f),
                new Vector3(0.015f, 0.018f, 0.016f),
                12);
            AttachMesh("Hand_Thumb", armPivot, thumbMesh, skinMat);
        }

        private static void BuildSculptedLegAndShoe(
            Transform legPivot,
            int side,
            Material skinMat,
            Material sockMat,
            Material shoeMat,
            Material soleMat)
        {
            var legMesh = CreateParametricMesh($"Leg_{side}", 24, 14, (u, v) =>
            {
                float a = u * 2f * Pi;
                float y = Mathf.Lerp(0f, -0.22f, v);
                float r = Mathf.Lerp(0.052f, 0.040f, v);
                return new Vector3(r * Mathf.Cos(a), y, r * Mathf.Sin(a));
            });
            AttachMesh("Leg_Upper", legPivot, legMesh, skinMat);

            // Meia canelada com babado fofo no topo
            var sockMesh = CreateParametricMesh($"Sock_{side}", 32, 14, (u, v) =>
            {
                float a = u * 2f * Pi;
                float y = Mathf.Lerp(-0.195f, -0.365f, v);
                float ruffle = v < 0.24f ? 0.011f * Mathf.Sin(a * 14f) * (1f - v / 0.24f) : 0.0022f * Mathf.Cos(a * 18f);
                float r = 0.044f + ruffle;
                return new Vector3(r * Mathf.Cos(a), y, r * Mathf.Sin(a));
            });
            AttachMesh("Ribbed_Sock", legPivot, sockMesh, sockMat);

            // Sapatinho Mary Jane vinho arredondado e fofo
            var shoeUpper = CreateParametricMesh($"Shoe_{side}", 32, 16, (u, v) =>
            {
                float p = Mathf.Max(0.001f, v) * Pi * 0.5f;
                float a = u * 2f * Pi;
                return new Vector3(
                    0.062f * Mathf.Sin(p) * Mathf.Cos(a),
                    -0.425f + 0.082f * Mathf.Cos(p),
                    0.036f + 0.106f * Mathf.Sin(p) * Mathf.Sin(a));
            });
            AttachMesh("MaryJane_Shoe", legPivot, shoeUpper, shoeMat);

            var strapMesh = CreateCurveTubeMesh(
                $"ShoeStrap_{side}",
                new Vector3(-0.050f, -0.375f, 0.024f),
                new Vector3(-0.025f, -0.352f, 0.042f),
                new Vector3(0.025f, -0.352f, 0.042f),
                new Vector3(0.050f, -0.375f, 0.024f),
                0.011f,
                0.6f,
                10,
                10);
            AttachMesh("Shoe_Strap", legPivot, strapMesh, shoeMat);

            // Botao redondo vinho na tira (igual a arte conceitual)
            var buttonMesh = CreateEllipsoidMesh(
                $"ShoeButton_{side}",
                new Vector3(side * 0.048f, -0.366f, 0.030f),
                new Vector3(0.011f, 0.014f, 0.014f),
                12);
            AttachMesh("Shoe_Button", legPivot, buttonMesh, shoeMat);

            var soleMesh = CreateEllipsoidMesh(
                $"Sole_{side}",
                new Vector3(0f, -0.426f, 0.036f),
                new Vector3(0.064f, 0.017f, 0.110f),
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

            if (forceRegenerate || !File.Exists(CollarSleeveTexPath))
            {
                GeneratePaintedCollarSleeveTexture(CollarSleeveTexPath);
            }

            if (forceRegenerate || !File.Exists(CoralRibbonTexPath))
            {
                GeneratePaintedCoralRibbonTexture(CoralRibbonTexPath);
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
            AssetDatabase.ImportAsset(CollarSleeveTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(CoralRibbonTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(HairTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(NandaFaceTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(NandaWingTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(NandaDressTexPath, ImportAssetOptions.ForceUpdate);
        }

        private static void GeneratePaintedYunaFaceTexture(string path)
        {
            const int size = 1024;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            Color skinBase = new Color(0.99f, 0.85f, 0.74f);
            Color skinWarmShade = new Color(0.93f, 0.70f, 0.57f);
            Color blushRose = new Color(0.95f, 0.53f, 0.50f);
            Color scleraCream = new Color(1.0f, 0.99f, 0.97f);
            Color scleraShadow = new Color(0.84f, 0.76f, 0.74f);
            Color limbalDark = new Color(0.16f, 0.07f, 0.04f);
            Color irisUpper = new Color(0.28f, 0.12f, 0.06f);
            Color irisMid = new Color(0.55f, 0.27f, 0.13f);
            Color irisAmberGlow = new Color(0.88f, 0.52f, 0.20f);
            Color pupilColor = new Color(0.08f, 0.03f, 0.02f);
            Color lashInk = new Color(0.14f, 0.06f, 0.04f);
            Color browColor = new Color(0.36f, 0.18f, 0.11f);
            Color mouthInterior = new Color(0.52f, 0.16f, 0.17f);
            Color tongueRose = new Color(0.90f, 0.45f, 0.46f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1); // v=0 topo da cabeca, v=1 queixo (corresponde a uv.y da malha)
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1); // u=0.5 centro frontal da face

                    // Base de pele em aquarela quente com sombreado suave nas bordas
                    float edgeShade = Mathf.Clamp01(Mathf.Abs(u - 0.5f) * 2.1f);
                    float topBottomShade = Mathf.Clamp01(Mathf.Max(0.36f - v, v - 0.76f) * 2.2f);
                    float paperGrain = (Mathf.PerlinNoise(u * 64f, v * 64f) - 0.5f) * 0.014f;
                    Color col = Color.Lerp(skinBase, skinWarmShade, Mathf.Clamp01(edgeShade * 0.40f + topBottomShade * 0.50f));
                    col.r = Mathf.Clamp01(col.r + paperGrain);
                    col.g = Mathf.Clamp01(col.g + paperGrain);
                    col.b = Mathf.Clamp01(col.b + paperGrain);

                    // Blush rosado amplo e fofo nas bochechas
                    for (int side = -1; side <= 1; side += 2)
                    {
                        float bu = (u - (0.5f + side * 0.098f)) / 0.048f;
                        float bv = (v - 0.605f) / 0.046f;
                        float bd = bu * bu + bv * bv;
                        if (bd < 1f)
                        {
                            float blushAlpha = Mathf.Pow(1f - bd, 1.5f) * 0.62f;
                            col = Color.Lerp(col, blushRose, blushAlpha);
                        }

                        // Olhos Grandes Expressivos Estilo Storybook (compensando proporcao UV da esfera para ficarem altos e redondos)
                        float eyeCx = 0.5f + side * 0.064f;
                        float eyeCy = 0.512f;
                        float eu = (u - eyeCx) / 0.032f;
                        float ev = (v - eyeCy) / 0.090f;
                        float ed = Mathf.Sqrt(eu * eu + ev * ev);

                        // Sobrancelhas castanhas suaves e arqueadas acima dos olhos
                        float browU = (u - eyeCx) * side;
                        float browArchY = 0.398f - 0.016f * Mathf.Cos(Mathf.Clamp(browU / 0.034f, -1f, 1f) * Pi * 0.5f) + 0.005f * (browU / 0.034f);
                        float browDist = Mathf.Abs(v - browArchY);
                        if (Mathf.Abs(browU) < 0.034f && browDist < 0.012f)
                        {
                            float taper = 1f - Mathf.Pow(Mathf.Abs(browU) / 0.034f, 1.5f);
                            float bMask = Mathf.SmoothStep(0.012f * taper, 0f, browDist);
                            col = Color.Lerp(col, browColor, bMask * 0.92f);
                        }

                        // Prega delicada da palpebra superior
                        float creaseY = eyeCy - 0.098f + 0.024f * eu * eu;
                        if (Mathf.Abs(eu) < 0.88f && Mathf.Abs(v - creaseY) < 0.005f)
                        {
                            col = Color.Lerp(col, browColor, 0.42f);
                        }

                        // Esclera + Iris castanho-mel em degrade + Pupila + Brilhos duplos nitidos
                        if (ed <= 1.0f)
                        {
                            Color eyeCol = Color.Lerp(scleraCream, scleraShadow, Mathf.Clamp01((-ev + 0.28f) * 0.62f));

                            float iu = (u - (eyeCx - side * 0.002f)) / 0.025f;
                            float iv = (v - (eyeCy + 0.004f)) / 0.078f;
                            float id = Mathf.Sqrt(iu * iu + iv * iv);
                            if (id <= 1.0f)
                            {
                                float vertGrad = Mathf.Clamp01((iv + 0.80f) / 1.6f);
                                float angle = Mathf.Atan2(iv, iu);
                                float fibers = 0.5f + 0.5f * Mathf.Sin(angle * 24f + id * 5f);
                                Color irisCol = Color.Lerp(irisUpper, irisMid, Mathf.Clamp01(vertGrad * 1.35f));
                                irisCol = Color.Lerp(irisCol, irisAmberGlow, Mathf.Pow(vertGrad, 1.7f) * (0.72f + 0.28f * fibers));

                                float limbal = Mathf.SmoothStep(0.75f, 0.98f, id);
                                irisCol = Color.Lerp(irisCol, limbalDark, limbal * 0.88f);

                                if (id < 0.48f)
                                {
                                    float pBlend = Mathf.SmoothStep(0.48f, 0.35f, id);
                                    irisCol = Color.Lerp(irisCol, pupilColor, pBlend);
                                }

                                eyeCol = irisCol;
                            }

                            // Brilho principal nitido + brilho secundario
                            float g1u = (u - (eyeCx - 0.009f)) / 0.009f;
                            float g1v = (v - (eyeCy - 0.026f)) / 0.026f;
                            if (g1u * g1u + g1v * g1v <= 1f)
                            {
                                eyeCol = Color.Lerp(eyeCol, Color.white, Mathf.SmoothStep(1f, 0.35f, g1u * g1u + g1v * g1v));
                            }

                            float g2u = (u - (eyeCx + 0.008f)) / 0.0045f;
                            float g2v = (v - (eyeCy + 0.026f)) / 0.012f;
                            if (g2u * g2u + g2v * g2v <= 1f)
                            {
                                eyeCol = Color.Lerp(eyeCol, new Color(1f, 0.98f, 0.92f), Mathf.SmoothStep(1f, 0.3f, g2u * g2u + g2v * g2v));
                            }

                            float edgeSoft = Mathf.SmoothStep(1.0f, 0.88f, ed);
                            col = Color.Lerp(col, eyeCol, edgeSoft);
                        }

                        // Cilios superiores caligrafados bem encorpados com pontinhas externas curvadas
                        float lashArchY = (eyeCy - 0.086f) + 0.064f * eu * eu;
                        float lashDist = Mathf.Abs(v - lashArchY);
                        if (Mathf.Abs(eu) <= 1.18f && lashDist < 0.028f)
                        {
                            float thickness = 0.025f * (1f - 0.18f * Mathf.Abs(eu));
                            if (lashDist < thickness)
                            {
                                col = Color.Lerp(col, lashInk, Mathf.SmoothStep(thickness, thickness * 0.20f, lashDist));
                            }
                        }

                        // 2 pontinhas delicadas de cilios no canto externo
                        float outerX = eyeCx + side * 0.034f;
                        float flickU = (u - outerX) * side;
                        if (flickU >= 0f && flickU < 0.013f)
                        {
                            float flick1Y = (eyeCy - 0.020f) - 0.018f * (flickU / 0.013f);
                            float flick2Y = (eyeCy - 0.004f) - 0.012f * (flickU / 0.013f);
                            if (Mathf.Abs(v - flick1Y) < 0.006f || (flickU < 0.010f && Mathf.Abs(v - flick2Y) < 0.005f))
                            {
                                col = Color.Lerp(col, lashInk, 0.92f);
                            }
                        }
                    }

                    // Sombra e brilho delicado do narizinho arrebitado
                    float nu = (u - 0.5f) / 0.010f;
                    float nv = (v - 0.585f) / 0.016f;
                    if (nu * nu + nv * nv < 1f)
                    {
                        Color noseShade = nv > 0f ? new Color(0.84f, 0.54f, 0.44f) : new Color(1.0f, 0.96f, 0.90f);
                        col = Color.Lerp(col, noseShade, (1f - (nu * nu + nv * nv)) * 0.65f);
                    }

                    // Sorriso aberto alegre e contagiante (fiel a arte conceitual)
                    float mu = (u - 0.5f) / 0.036f;
                    if (Mathf.Abs(mu) <= 1.0f)
                    {
                        float upperLipY = 0.630f + 0.010f * mu * mu;
                        float lowerMouthY = 0.678f - 0.038f * mu * mu;

                        if (v >= upperLipY && v <= lowerMouthY && lowerMouthY > upperLipY)
                        {
                            Color mCol = mouthInterior;
                            float tongueU = mu / 0.66f;
                            float tongueV = (v - 0.664f) / 0.018f;
                            if (tongueU * tongueU + tongueV * tongueV < 1f)
                            {
                                mCol = Color.Lerp(mCol, tongueRose, Mathf.SmoothStep(1f, 0.35f, tongueU * tongueU + tongueV * tongueV));
                            }

                            if (v < upperLipY + 0.008f && Mathf.Abs(mu) < 0.72f)
                            {
                                mCol = Color.Lerp(mCol, new Color(0.99f, 0.97f, 0.94f), 0.88f);
                            }

                            col = Color.Lerp(col, mCol, 0.95f);
                        }

                        if (Mathf.Abs(v - upperLipY) < 0.0045f || (lowerMouthY > upperLipY && Mathf.Abs(v - lowerMouthY) < 0.0040f))
                        {
                            col = Color.Lerp(col, new Color(0.46f, 0.15f, 0.14f), 0.78f);
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
            Color baseCream = new Color(0.95f, 0.89f, 0.77f);
            Color foldShadow = new Color(0.77f, 0.65f, 0.50f);
            Color deepGatherShadow = new Color(0.67f, 0.54f, 0.40f);
            Color coralBand = new Color(0.79f, 0.33f, 0.34f);
            Color coralBandEdge = new Color(0.65f, 0.23f, 0.25f);
            Color coralFlower = new Color(0.89f, 0.44f, 0.43f);
            Color coralFlowerLight = new Color(0.94f, 0.60f, 0.56f);
            Color laceCream = new Color(0.98f, 0.94f, 0.86f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);

                    // Trama de linho pintada à mão em todo o tecido
                    float weaveX = Mathf.Sin(u * 520f) * 0.014f;
                    float weaveY = Mathf.Sin(v * 520f) * 0.014f;
                    float watercolorGrain = (Mathf.PerlinNoise(u * 48f, v * 48f) - 0.5f) * 0.042f;
                    float fabricTex = weaveX + weaveY + watercolorGrain;

                    Color col;
                    if (v < 0.28f)
                    {
                        // Corpete: pregas suaves convergindo acima do cinto + oclusao logo acima da borda do cinto
                        float tv = v / 0.28f;
                        float mainPleat = 0.5f + 0.5f * Mathf.Cos(u * Pi * 2f * 10f);
                        float fineGather = 0.5f + 0.5f * Mathf.Cos(u * Pi * 2f * 20f);
                        float upperPleatMask = Mathf.Lerp(1.0f, 0.65f * mainPleat + 0.35f * fineGather, Mathf.SmoothStep(0.15f, 0.90f, tv));
                        float aboveWaistAO = Mathf.Exp(-(1f - tv) * 5.8f) * (1f - upperPleatMask) * 0.65f;
                        col = Color.Lerp(foldShadow, baseCream, Mathf.Lerp(0.32f, 1.0f, upperPleatMask));
                        col = Color.Lerp(col, deepGatherShadow, aboveWaistAO);
                    }
                    else
                    {
                        // Saia: pregas alinhadas com a malha 3D (10 principais + 20 franzidas abaixo do cinto)
                        float sv = (v - 0.28f) / 0.72f;
                        float mainPleat = 0.5f + 0.5f * Mathf.Cos(u * Pi * 2f * 10f);
                        float fineGather = 0.5f + 0.5f * Mathf.Cos(u * Pi * 2f * 20f);
                        float pleatMask = Mathf.Lerp(fineGather * 0.6f + mainPleat * 0.4f, mainPleat, Mathf.SmoothStep(0f, 0.45f, sv));

                        // Oclusao mais funda nos sulcos logo abaixo do cinto (v = 0.28..0.40)
                        float underWaistAO = Mathf.Exp(-sv * 6.5f) * (1f - pleatMask) * 0.70f;
                        col = Color.Lerp(foldShadow, baseCream, Mathf.Lerp(0.18f, 1.0f, pleatMask));
                        col = Color.Lerp(col, deepGatherShadow, underWaistAO);
                    }

                    col.r = Mathf.Clamp01(col.r + fabricTex);
                    col.g = Mathf.Clamp01(col.g + fabricTex);
                    col.b = Mathf.Clamp01(col.b + fabricTex);

                    // Duas fileiras alternadas de florzinhas coral simples de 5 petalas (em escala metrica 1:1 para nao esticar na saia)
                    for (int row = 0; row < 2; row++)
                    {
                        float rowCenterV = row == 0 ? 0.61f : 0.715f;
                        float freq = row == 0 ? 12f : 14f;
                        float phase = row == 0 ? 0f : 0.5f;
                        float circMeters = row == 0 ? 1.32f : 1.54f;
                        float flowerRadiusMeters = row == 0 ? 0.022f : 0.017f;

                        float dx = (Mathf.Repeat(u * freq + phase, 1f) - 0.5f) * (circMeters / freq);
                        float dy = (v - rowCenterV) * 0.58f;
                        float rNorm = Mathf.Sqrt(dx * dx + dy * dy) / flowerRadiusMeters;
                        float ang = Mathf.Atan2(dy, dx);
                        float petalRadius = 0.72f + 0.32f * Mathf.Cos(ang * 5f);
                        if (rNorm < petalRadius)
                        {
                            Color fCol = Color.Lerp(coralFlowerLight, coralFlower, rNorm / petalRadius);
                            col = Color.Lerp(col, fCol, 0.90f);
                        }
                    }

                    // Barra ondulada coral-rosada suave perto da extremidade inferior
                    float wave = 0.016f * Mathf.Sin(u * Pi * 2f * 16f);
                    float vWave = v + wave;
                    if (vWave >= 0.79f && vWave <= 0.89f)
                    {
                        float edgeSoft = Mathf.Min(
                            Mathf.SmoothStep(0.79f, 0.805f, vWave),
                            Mathf.SmoothStep(0.89f, 0.875f, vWave));
                        Color bandCol = Color.Lerp(coralBandEdge, coralBand, edgeSoft);
                        col = Color.Lerp(col, bandCol, 0.94f);
                    }
                    else if (vWave > 0.89f)
                    {
                        float lacePleat = 0.5f + 0.5f * Mathf.Sin(u * Pi * 2f * 28f);
                        col = Color.Lerp(foldShadow, laceCream, Mathf.Lerp(0.45f, 1f, lacePleat));
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GeneratePaintedCollarSleeveTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseCream = new Color(0.95f, 0.89f, 0.77f);
            Color foldShadow = new Color(0.76f, 0.64f, 0.49f);
            Color deepGather = new Color(0.66f, 0.53f, 0.39f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float pleat = 0.5f + 0.5f * Mathf.Cos(u * Pi * 2f * 10f);
                    float endGather = Mathf.Pow(Mathf.Abs(v - 0.5f) * 2f, 1.5f);
                    float weave = (Mathf.Sin(u * 380f) + Mathf.Sin(v * 380f)) * 0.012f
                        + (Mathf.PerlinNoise(u * 36f, v * 36f) - 0.5f) * 0.038f;

                    Color col = Color.Lerp(foldShadow, baseCream, Mathf.Lerp(0.24f, 1.0f, pleat));
                    col = Color.Lerp(col, deepGather, endGather * (1f - pleat) * 0.65f);
                    col.r = Mathf.Clamp01(col.r + weave);
                    col.g = Mathf.Clamp01(col.g + weave);
                    col.b = Mathf.Clamp01(col.b + weave);
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GeneratePaintedCoralRibbonTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color coralLight = new Color(0.92f, 0.54f, 0.52f);
            Color coralMid = new Color(0.84f, 0.38f, 0.39f);
            Color coralShadow = new Color(0.62f, 0.22f, 0.24f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float ribbonRib = Mathf.Sin(u * 420f) * 0.015f;
                    float crease = Mathf.Exp(-Mathf.Pow((v - 0.5f) * 4.5f, 2f)) * 0.35f;
                    float edgeShade = Mathf.Pow(Mathf.Abs(v - 0.5f) * 2f, 2.2f) * 0.55f;
                    float grain = (Mathf.PerlinNoise(u * 40f, v * 40f) - 0.5f) * 0.032f;

                    Color col = Color.Lerp(coralMid, coralLight, crease);
                    col = Color.Lerp(col, coralShadow, edgeShade);
                    col.r = Mathf.Clamp01(col.r + ribbonRib + grain);
                    col.g = Mathf.Clamp01(col.g + ribbonRib + grain);
                    col.b = Mathf.Clamp01(col.b + ribbonRib + grain);
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
            Color deepEspresso = new Color(0.16f, 0.08f, 0.04f);
            Color richChestnut = new Color(0.39f, 0.20f, 0.10f);
            Color warmCaramel = new Color(0.62f, 0.36f, 0.19f);
            Color goldenHalo = new Color(0.80f, 0.52f, 0.29f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    // Fios longos e suaves (sem ondas curtas de "macarrao")
                    float waveU = u + 0.022f * Mathf.Sin(v * Pi * 1.6f + u * Pi * 2f);
                    float lockGroup = 0.5f + 0.5f * Mathf.Sin(waveU * Pi * 2f * 10f);
                    float fineStrands = 0.5f + 0.5f * Mathf.Sin(waveU * Pi * 2f * 72f + Mathf.PerlinNoise(waveU * 14f, v * 5f) * 2.2f);
                    float painterlyStrands = Mathf.Lerp(
                        Mathf.PerlinNoise(waveU * 24f, v * 6f),
                        fineStrands,
                        0.42f);

                    Color col = Color.Lerp(deepEspresso, richChestnut, painterlyStrands * (0.35f + 0.65f * lockGroup));

                    float halo1 = Mathf.Exp(-Mathf.Pow((v - 0.58f - 0.035f * Mathf.Sin(u * Pi * 12f)) / 0.14f, 2f));
                    float halo2 = Mathf.Exp(-Mathf.Pow((v - 0.28f + 0.025f * Mathf.Cos(u * Pi * 10f)) / 0.11f, 2f));
                    col = Color.Lerp(col, warmCaramel, (halo1 * 0.78f + halo2 * 0.52f) * painterlyStrands);
                    col = Color.Lerp(col, goldenHalo, halo1 * Mathf.Pow(painterlyStrands, 1.6f) * 0.48f);

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
