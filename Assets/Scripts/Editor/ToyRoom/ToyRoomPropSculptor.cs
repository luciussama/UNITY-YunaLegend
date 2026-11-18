using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.Editor.ToyRoom
{
    /// <summary>
    /// Escultor 3D de Mobiliario e Brinquedos da Toy Room (Environment Artist Agent + Game Artist Agent).
    /// Substitui primitivas geometricas por malhas 3D organicas chanfradas/torneadas com UVs e texturas pintadas a mao.
    /// </summary>
    public static class ToyRoomPropSculptor
    {
        private const string ChestTexPath = "Assets/Art/ToyRoom/Materials/T_Chest_Painted.png";
        private const string RobotTexPath = "Assets/Art/ToyRoom/Materials/T_Robot_Painted.png";
        private const string DuckTexPath = "Assets/Art/ToyRoom/Materials/T_Duck_Painted.png";
        private const string BunnyTexPath = "Assets/Art/ToyRoom/Materials/T_Bunny_Painted.png";
        private const string TrainTexPath = "Assets/Art/ToyRoom/Materials/T_Train_Painted.png";
        private const string JackTexPath = "Assets/Art/ToyRoom/Materials/T_Jack_Painted.png";

        private static float Pi => Mathf.PI;

        public static void EnsurePaintedPropTextures(bool forceRegenerate = false)
        {
            Directory.CreateDirectory("Assets/Art/ToyRoom/Materials");

            if (forceRegenerate || !File.Exists(ChestTexPath))
            {
                GenerateChestTexture(ChestTexPath);
            }

            if (forceRegenerate || !File.Exists(RobotTexPath))
            {
                GenerateRobotTexture(RobotTexPath);
            }

            if (forceRegenerate || !File.Exists(DuckTexPath))
            {
                GenerateDuckTexture(DuckTexPath);
            }

            if (forceRegenerate || !File.Exists(BunnyTexPath))
            {
                GenerateBunnyTexture(BunnyTexPath);
            }

            if (forceRegenerate || !File.Exists(TrainTexPath))
            {
                GenerateTrainTexture(TrainTexPath);
            }

            if (forceRegenerate || !File.Exists(JackTexPath))
            {
                GenerateJackTexture(JackTexPath);
            }

            AssetDatabase.ImportAsset(ChestTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(RobotTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(DuckTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(BunnyTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(TrainTexPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(JackTexPath, ImportAssetOptions.ForceUpdate);
        }

        public static void BuildSculptedToyChest(Transform chestGo, FurnitureSpec item, out Transform lidPivot)
        {
            EnsurePaintedPropTextures();

            var chestMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, ChestTexPath, outlineWidth: 0.005f);
            var brassMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.95f, 0.78f, 0.32f), "Assets/Materials/T_Furniture_Wood_Painted.png", outlineWidth: 0.004f);
            var gemMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.32f, 0.78f, 1.0f), emission: new Color(0.15f, 0.48f, 0.78f), outlineWidth: 0.003f);

            // Base esculpida chanfrada com pes torneados
            var bodyMesh = CreateBeveledBoxMesh("ToyChest_Body", new Vector3(item.Width, 0.68f, item.Depth), 0.08f, 24, 14);
            var bodyGo = ToyRoomYunaSculptor.AttachMesh("Chest_SculptedBody", chestGo, bodyMesh, chestMat);
            bodyGo.transform.localPosition = new Vector3(0f, 0.38f, 0f);

            // Cinta frontal de latao e fecho magico azul esculpido
            var lockPlate = CreateBeveledBoxMesh("ToyChest_LockPlate", new Vector3(0.34f, 0.28f, 0.08f), 0.025f, 16, 10);
            var lockGo = ToyRoomYunaSculptor.AttachMesh("Chest_LockPlate", chestGo, lockPlate, brassMat);
            lockGo.transform.localPosition = new Vector3(0f, 0.56f, -item.Depth * 0.50f);

            var gemMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh("ToyChest_Gem", new Vector3(0f, 0.58f, -item.Depth * 0.54f), new Vector3(0.10f, 0.12f, 0.05f), 18);
            ToyRoomYunaSculptor.AttachMesh("Chest_StarGem", chestGo, gemMesh, gemMat);

            // Tampa abobadada de bau de tesouro (Vaulted Lid)
            var pivotGo = new GameObject("LidPivot");
            pivotGo.transform.SetParent(chestGo, false);
            pivotGo.transform.localPosition = new Vector3(0f, 0.70f, item.Depth * 0.48f);
            lidPivot = pivotGo.transform;

            var vaultedLidMesh = ToyRoomYunaSculptor.CreateParametricMesh("ToyChest_VaultedLid", 36, 18, (u, v) =>
            {
                float a = (u - 0.5f) * 2f * Pi;
                float y = (v - 0.5f) * 2f;
                float arch = Mathf.Cos(Mathf.Clamp(Mathf.Sin(a), -1f, 1f) * Pi * 0.35f);
                float rx = item.Width * 0.52f;
                float rz = item.Depth * 0.52f;
                float x = rx * Mathf.Clamp(Mathf.Sin(a) * 1.25f, -1f, 1f);
                float z = -item.Depth * 0.48f + rz * Mathf.Clamp(Mathf.Cos(a) * 1.25f, -1f, 1f);
                float h = Mathf.Max(0f, Mathf.Cos(y * Pi * 0.5f)) * (0.22f + 0.14f * arch);
                float slat = 1f + 0.015f * Mathf.Cos(u * Pi * 24f);
                return new Vector3(x, h * slat, z);
            });
            ToyRoomYunaSculptor.AttachMesh("Chest_VaultedLid", lidPivot, vaultedLidMesh, chestMat);
        }

        public static void BuildSculptedFurniture(Transform parent, FurnitureSpec spec)
        {
            EnsurePaintedPropTextures();

            var oakMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.86f, 0.64f, 0.44f), "Assets/Materials/T_Furniture_Wood_Painted.png", outlineWidth: 0.0045f);
            var darkWalnutMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.58f, 0.38f, 0.24f), "Assets/Materials/T_DarkOak_Planks.png", outlineWidth: 0.0045f);
            var brassMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.95f, 0.80f, 0.36f), outlineWidth: 0.0035f);

            switch (spec.Id)
            {
                case "bookshelf":
                {
                    var bookSpinesA = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, "Assets/Materials/T_Book_Spines_Painted.png", outlineWidth: 0.004f);
                    var bookSpinesB = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.95f, 0.82f, 0.78f), "Assets/Materials/T_Book_Cover_Painted.png", outlineWidth: 0.004f);

                    var frameMesh = CreateArchedCabinetMesh("Bookshelf_ArchedFrame", spec.Width, 1.65f, spec.Depth, 0.18f);
                    ToyRoomYunaSculptor.AttachMesh("Bookshelf_Frame", parent, frameMesh, oakMat);

                    for (int row = 0; row < 3; row++)
                    {
                        float shelfY = 0.28f + row * 0.44f;
                        var shelfMesh = CreateBeveledBoxMesh($"Bookshelf_Shelf_{row}", new Vector3(spec.Width * 0.94f, 0.06f, spec.Depth * 0.92f), 0.015f, 20, 8);
                        var sGo = ToyRoomYunaSculptor.AttachMesh($"Shelf_{row}", parent, shelfMesh, darkWalnutMat);
                        sGo.transform.localPosition = new Vector3(0f, shelfY, 0f);

                        var booksMesh = BuildSculptedBookRowMesh($"Bookshelf_Row_{row}", spec.Width * 0.84f, 0.34f, spec.Depth * 0.72f, row);
                        var bGo = ToyRoomYunaSculptor.AttachMesh($"Books_Row_{row}", parent, booksMesh, row % 2 == 0 ? bookSpinesA : bookSpinesB);
                        bGo.transform.localPosition = new Vector3(0f, shelfY + 0.19f, -0.04f);
                    }
                    break;
                }

                case "wardrobe":
                {
                    var bodyMesh = CreateArchedCabinetMesh("Wardrobe_ArchedBody", spec.Width, 1.92f, spec.Depth, 0.24f);
                    ToyRoomYunaSculptor.AttachMesh("Wardrobe_Body", parent, bodyMesh, oakMat);

                    for (int side = -1; side <= 1; side += 2)
                    {
                        var doorMesh = CreateBeveledBoxMesh($"Wardrobe_Door_{side}", new Vector3(spec.Width * 0.42f, 1.48f, 0.08f), 0.025f, 20, 14);
                        var dGo = ToyRoomYunaSculptor.AttachMesh($"Door_{side}", parent, doorMesh, darkWalnutMat);
                        dGo.transform.localPosition = new Vector3(side * spec.Width * 0.22f, 0.88f, -spec.Depth * 0.48f);

                        var handleMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Wardrobe_Knob_{side}", new Vector3(side * 0.09f, 0.92f, -spec.Depth * 0.54f), new Vector3(0.045f, 0.065f, 0.045f), 14);
                        ToyRoomYunaSculptor.AttachMesh($"Knob_{side}", parent, handleMesh, brassMat);
                    }
                    break;
                }

                case "rocking-horse":
                {
                    var dappleWood = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.98f, 0.94f, 0.86f), "Assets/Materials/T_Pastel_ToyWood.png", outlineWidth: 0.0045f);
                    var saddleRed = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.86f, 0.24f, 0.26f), ChestTexPath, outlineWidth: 0.004f);
                    var maneGold = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.88f, 0.64f, 0.32f), "Assets/Materials/T_Bear_Fur_Painted.png", outlineWidth: 0.004f);

                    for (int side = -1; side <= 1; side += 2)
                    {
                        var rockerBow = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                            $"Horse_Rocker_{side}",
                            new Vector3(side * 0.22f, 0.18f, -0.72f),
                            new Vector3(side * 0.22f, 0.03f, -0.24f),
                            new Vector3(side * 0.22f, 0.03f, 0.24f),
                            new Vector3(side * 0.22f, 0.18f, 0.72f),
                            0.045f,
                            0.55f,
                            14,
                            20);
                        ToyRoomYunaSculptor.AttachMesh($"Rocker_{side}", parent, rockerBow, darkWalnutMat);

                        var frontLeg = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                            $"Horse_FrontLeg_{side}",
                            new Vector3(side * 0.14f, 0.54f, 0.22f),
                            new Vector3(side * 0.17f, 0.38f, 0.30f),
                            new Vector3(side * 0.20f, 0.22f, 0.38f),
                            new Vector3(side * 0.22f, 0.08f, 0.44f),
                            0.048f,
                            0.85f,
                            12,
                            12);
                        ToyRoomYunaSculptor.AttachMesh($"FrontLeg_{side}", parent, frontLeg, dappleWood);

                        var hindLeg = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                            $"Horse_HindLeg_{side}",
                            new Vector3(side * 0.14f, 0.54f, -0.22f),
                            new Vector3(side * 0.17f, 0.38f, -0.30f),
                            new Vector3(side * 0.20f, 0.22f, -0.38f),
                            new Vector3(side * 0.22f, 0.08f, -0.44f),
                            0.052f,
                            0.85f,
                            12,
                            12);
                        ToyRoomYunaSculptor.AttachMesh($"HindLeg_{side}", parent, hindLeg, dappleWood);
                    }

                    var horseBody = ToyRoomYunaSculptor.CreateParametricMesh("Horse_Body", 32, 20, (u, v) =>
                    {
                        float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                        float a = (u - 0.5f) * 2f * Pi;
                        float sway = -0.03f * Mathf.Sin(p);
                        return new Vector3(
                            0.18f * Mathf.Sin(p) * Mathf.Sin(a),
                            0.58f + sway + 0.16f * Mathf.Sin(p) * Mathf.Cos(a),
                            0.36f * Mathf.Cos(p));
                    });
                    ToyRoomYunaSculptor.AttachMesh("Horse_SculptedBody", parent, horseBody, dappleWood);

                    var saddleMesh = ToyRoomYunaSculptor.CreateParametricMesh("Horse_Saddle", 24, 14, (u, v) =>
                    {
                        float p = Mathf.Clamp(v, 0.05f, 0.95f) * Pi;
                        float a = (u - 0.5f) * Pi;
                        return new Vector3(
                            0.195f * Mathf.Sin(p) * Mathf.Sin(a),
                            0.60f + 0.15f * Mathf.Sin(p) * Mathf.Cos(a),
                            0.18f * Mathf.Cos(p));
                    });
                    ToyRoomYunaSculptor.AttachMesh("Horse_Saddle", parent, saddleMesh, saddleRed);

                    var neckHead = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                        "Horse_NeckAndHead",
                        new Vector3(0f, 0.62f, 0.26f),
                        new Vector3(0f, 0.88f, 0.36f),
                        new Vector3(0f, 1.06f, 0.42f),
                        new Vector3(0f, 0.96f, 0.60f),
                        0.125f,
                        0.78f,
                        18,
                        18);
                    ToyRoomYunaSculptor.AttachMesh("Horse_NeckHead", parent, neckHead, dappleWood);

                    var mane = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                        "Horse_Mane",
                        new Vector3(0f, 1.06f, 0.36f),
                        new Vector3(0f, 0.94f, 0.26f),
                        new Vector3(0f, 0.78f, 0.20f),
                        new Vector3(0f, 0.64f, 0.16f),
                        0.055f,
                        0.5f,
                        12,
                        14);
                    ToyRoomYunaSculptor.AttachMesh("Horse_Mane", parent, mane, maneGold);

                    var tail = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                        "Horse_Tail",
                        new Vector3(0f, 0.62f, -0.34f),
                        new Vector3(0f, 0.56f, -0.52f),
                        new Vector3(0f, 0.38f, -0.58f),
                        new Vector3(0f, 0.22f, -0.52f),
                        0.065f,
                        0.65f,
                        12,
                        14);
                    ToyRoomYunaSculptor.AttachMesh("Horse_Tail", parent, tail, maneGold);
                    break;
                }

                case "table":
                {
                    var clothMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, "Assets/Art/ToyRoom/Materials/T_Yuna_Dress_Painted.png", outlineWidth: 0.004f);

                    var topMesh = ToyRoomYunaSculptor.CreateParametricMesh("Table_ScallopedTop", 48, 16, (u, v) =>
                    {
                        float a = u * 2f * Pi;
                        float rx = spec.Width * 0.52f;
                        float rz = spec.Depth * 0.52f;
                        if (v < 0.65f)
                        {
                            float r = v / 0.65f;
                            return new Vector3(rx * r * Mathf.Cos(a), 0.68f + 0.02f * (1f - r), rz * r * Mathf.Sin(a));
                        }
                        else
                        {
                            float drape = (v - 0.65f) / 0.35f;
                            float scallop = 0.035f * Mathf.Sin(a * 16f) * drape;
                            return new Vector3(rx * (1f + 0.03f * drape) * Mathf.Cos(a), 0.68f - 0.16f * drape + scallop, rz * (1f + 0.03f * drape) * Mathf.Sin(a));
                        }
                    });
                    ToyRoomYunaSculptor.AttachMesh("Table_DrapedTop", parent, topMesh, clothMat);

                    var pedestalMesh = CreateTurnedColumnMesh("Table_TurnedPedestal", 0.18f, 0.66f, 28, 18);
                    ToyRoomYunaSculptor.AttachMesh("Table_Pedestal", parent, pedestalMesh, darkWalnutMat);

                    var teapotMesh = CreateTurnedColumnMesh("Table_Teapot", 0.14f, 0.24f, 24, 14);
                    var tpGo = ToyRoomYunaSculptor.AttachMesh("Teapot", parent, teapotMesh, brassMat);
                    tpGo.transform.localPosition = new Vector3(0f, 0.69f, 0f);
                    break;
                }

                case "armchair":
                {
                    var velvetMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.76f, 0.46f, 0.72f), "Assets/Materials/T_Velvet_Purple.png", outlineWidth: 0.0045f);
                    var cushionGold = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.96f, 0.82f, 0.48f), "Assets/Materials/T_TeddyPlush.png", outlineWidth: 0.004f);

                    var seatMesh = CreateTuftedCushionMesh("Armchair_PlushSeat", new Vector3(spec.Width * 0.88f, 0.42f, spec.Depth * 0.88f), 28, 16);
                    var seatGo = ToyRoomYunaSculptor.AttachMesh("Armchair_Seat", parent, seatMesh, velvetMat);
                    seatGo.transform.localPosition = new Vector3(0f, 0.28f, 0f);

                    var backMesh = CreateTuftedCushionMesh("Armchair_WingedBack", new Vector3(spec.Width * 0.94f, 0.86f, 0.34f), 28, 18);
                    var backGo = ToyRoomYunaSculptor.AttachMesh("Armchair_Backrest", parent, backMesh, velvetMat);
                    backGo.transform.localPosition = new Vector3(0f, 0.76f, spec.Depth * 0.34f);

                    for (int side = -1; side <= 1; side += 2)
                    {
                        var armScroll = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                            $"Armchair_ScrollArm_{side}",
                            new Vector3(side * spec.Width * 0.42f, 0.48f, spec.Depth * 0.36f),
                            new Vector3(side * spec.Width * 0.44f, 0.62f, 0.05f),
                            new Vector3(side * spec.Width * 0.46f, 0.64f, -spec.Depth * 0.32f),
                            new Vector3(side * spec.Width * 0.40f, 0.46f, -spec.Depth * 0.42f),
                            0.14f,
                            0.85f,
                            16,
                            16);
                        ToyRoomYunaSculptor.AttachMesh($"Arm_Scroll_{side}", parent, armScroll, velvetMat);
                    }

                    var pillowMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh("Armchair_Pillow", new Vector3(0.12f, 0.56f, 0.10f), new Vector3(0.22f, 0.18f, 0.10f), 20);
                    ToyRoomYunaSculptor.AttachMesh("Throw_Pillow", parent, pillowMesh, cushionGold);
                    break;
                }

                case "block-fortress":
                {
                    var stoneBlockMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, "Assets/Materials/T_Castle_Blocks.png", outlineWidth: 0.0045f);
                    var roofRedMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.86f, 0.28f, 0.32f), ChestTexPath, outlineWidth: 0.0045f);
                    var roofBlueMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.32f, 0.62f, 0.90f), RobotTexPath, outlineWidth: 0.0045f);

                    var keepMesh = CreateBeveledBoxMesh("Castle_Keep", new Vector3(spec.Width * 0.82f, 0.96f, spec.Depth * 0.76f), 0.06f, 24, 14);
                    var keepGo = ToyRoomYunaSculptor.AttachMesh("Castle_Keep", parent, keepMesh, stoneBlockMat);
                    keepGo.transform.localPosition = new Vector3(0f, 0.48f, 0f);

                    Vector3[] towerPositions =
                    {
                        new Vector3(-0.74f, 0f, 0.32f),
                        new Vector3(0.74f, 0f, -0.22f),
                        new Vector3(0f, 0f, 0.38f)
                    };
                    float[] towerHeights = { 1.45f, 1.65f, 1.85f };

                    for (int i = 0; i < towerPositions.Length; i++)
                    {
                        float h = towerHeights[i];
                        var towerMesh = CreateTurnedColumnMesh($"Castle_Tower_{i}", 0.28f, h, 24, 14);
                        var tGo = ToyRoomYunaSculptor.AttachMesh($"Tower_{i}", parent, towerMesh, stoneBlockMat);
                        tGo.transform.localPosition = towerPositions[i];

                        var coneMesh = CreateConeSpireMesh($"Castle_Spire_{i}", 0.34f, 0.48f, 24);
                        var cGo = ToyRoomYunaSculptor.AttachMesh($"Spire_{i}", parent, coneMesh, i % 2 == 0 ? roofRedMat : roofBlueMat);
                        cGo.transform.localPosition = towerPositions[i] + new Vector3(0f, h, 0f);
                    }
                    break;
                }
            }
        }

        public static void BuildSculptedToy(Transform parent, ToySpec spec)
        {
            EnsurePaintedPropTextures();

            switch (spec.Id)
            {
                case "teddy":
                {
                    var plushMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.74f, 0.46f, 0.24f), "Assets/Materials/T_Bear_Fur_Painted.png", outlineWidth: 0.003f);
                    plushMat.SetFloat("_BrushNoiseStrength", 0.0f);
                    var muzzleMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.98f, 0.90f, 0.76f), outlineWidth: 0.002f);
                    var buttonMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.14f, 0.07f, 0.05f), outlineWidth: 0f);
                    var shineMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, emission: new Color(0.4f, 0.4f, 0.4f), outlineWidth: 0f);
                    var ribbonMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.86f, 0.20f, 0.20f), outlineWidth: 0.0025f);

                    var root = new GameObject("Teddy_Root").transform;
                    root.SetParent(parent, false);
                    root.localRotation = Quaternion.Euler(0f, 180f, 0f);

                    var bellyMesh = ToyRoomYunaSculptor.CreateParametricMesh("Teddy_PlumpBody", 28, 18, (u, v) =>
                    {
                        float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                        float a = (u - 0.5f) * 2f * Pi;
                        float pear = 1f + 0.18f * Mathf.Clamp01(-Mathf.Cos(p));
                        return new Vector3(0f, 0.21f, 0f) + new Vector3(
                            0.17f * pear * Mathf.Sin(p) * Mathf.Sin(a),
                            0.19f * Mathf.Cos(p),
                            0.15f * pear * Mathf.Sin(p) * Mathf.Cos(a));
                    });
                    ToyRoomYunaSculptor.AttachMesh("Teddy_Body", root, bellyMesh, plushMat);

                    var headMesh = ToyRoomYunaSculptor.CreateParametricMesh("Teddy_Head", 28, 18, (u, v) =>
                    {
                        float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                        float a = (u - 0.5f) * 2f * Pi;
                        float cheeks = 1f + 0.12f * Mathf.Exp(-Mathf.Pow(Mathf.Cos(p) + 0.15f, 2f) * 6f);
                        return new Vector3(0f, 0.48f, 0.01f) + new Vector3(
                            0.175f * cheeks * Mathf.Sin(p) * Mathf.Sin(a),
                            0.155f * Mathf.Cos(p),
                            0.155f * cheeks * Mathf.Sin(p) * Mathf.Cos(a));
                    });
                    ToyRoomYunaSculptor.AttachMesh("Teddy_Head", root, headMesh, plushMat);

                    var snoutMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh("Teddy_Snout", new Vector3(0f, 0.45f, 0.15f), new Vector3(0.078f, 0.062f, 0.058f), 18);
                    ToyRoomYunaSculptor.AttachMesh("Teddy_Snout", root, snoutMesh, muzzleMat);

                    var noseMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh("Teddy_Nose", new Vector3(0f, 0.47f, 0.205f), new Vector3(0.028f, 0.022f, 0.020f), 14);
                    ToyRoomYunaSculptor.AttachMesh("Teddy_Nose", root, noseMesh, buttonMat);

                    for (int side = -1; side <= 1; side += 2)
                    {
                        var earMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Teddy_Ear_{side}", new Vector3(side * 0.145f, 0.60f, 0f), new Vector3(0.068f, 0.068f, 0.038f), 16);
                        ToyRoomYunaSculptor.AttachMesh($"Ear_{side}", root, earMesh, plushMat);

                        var innerEar = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Teddy_InnerEar_{side}", new Vector3(side * 0.145f, 0.60f, 0.022f), new Vector3(0.042f, 0.042f, 0.022f), 14);
                        ToyRoomYunaSculptor.AttachMesh($"InnerEar_{side}", root, innerEar, muzzleMat);

                        var eyeMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Teddy_Eye_{side}", new Vector3(side * 0.068f, 0.51f, 0.152f), new Vector3(0.022f, 0.024f, 0.014f), 14);
                        ToyRoomYunaSculptor.AttachMesh($"Eye_{side}", root, eyeMesh, buttonMat);

                        var eyeShine = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Teddy_EyeShine_{side}", new Vector3(side * 0.064f + 0.006f, 0.518f, 0.165f), new Vector3(0.007f, 0.007f, 0.005f), 8);
                        ToyRoomYunaSculptor.AttachMesh($"EyeShine_{side}", root, eyeShine, shineMat);

                        var armMesh = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                            $"Teddy_Arm_{side}",
                            new Vector3(side * 0.14f, 0.32f, 0f),
                            new Vector3(side * 0.19f, 0.26f, 0.06f),
                            new Vector3(side * 0.18f, 0.18f, 0.11f),
                            new Vector3(side * 0.14f, 0.14f, 0.14f),
                            0.055f,
                            0.85f,
                            12,
                            12);
                        ToyRoomYunaSculptor.AttachMesh($"Arm_{side}", root, armMesh, plushMat);

                        var legMesh = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                            $"Teddy_Leg_{side}",
                            new Vector3(side * 0.10f, 0.10f, 0.04f),
                            new Vector3(side * 0.12f, 0.08f, 0.12f),
                            new Vector3(side * 0.13f, 0.08f, 0.19f),
                            new Vector3(side * 0.13f, 0.10f, 0.24f),
                            0.062f,
                            0.90f,
                            12,
                            12);
                        ToyRoomYunaSculptor.AttachMesh($"Leg_{side}", root, legMesh, plushMat);

                        var pawPad = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Teddy_PawPad_{side}", new Vector3(side * 0.13f, 0.10f, 0.275f), new Vector3(0.045f, 0.052f, 0.018f), 14);
                        ToyRoomYunaSculptor.AttachMesh($"PawPad_{side}", root, pawPad, muzzleMat);
                    }

                    var bowMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh("Teddy_Bow", new Vector3(0f, 0.34f, 0.155f), new Vector3(0.095f, 0.042f, 0.032f), 16);
                    ToyRoomYunaSculptor.AttachMesh("Teddy_Bow", root, bowMesh, ribbonMat);
                    break;
                }

                case "train":
                {
                    var trainMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, TrainTexPath, outlineWidth: 0.0045f);
                    var brassMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.95f, 0.80f, 0.34f), outlineWidth: 0.0035f);

                    var boilerMesh = ToyRoomYunaSculptor.CreateParametricMesh("Train_Boiler", 28, 16, (u, v) =>
                    {
                        float a = u * 2f * Pi;
                        float z = Mathf.Lerp(-0.05f, 0.26f, v);
                        float band = 1f + 0.05f * Mathf.Exp(-Mathf.Pow(Mathf.Sin(v * Pi * 3f), 2f) * 18f);
                        float r = 0.125f * band;
                        return new Vector3(r * Mathf.Cos(a), 0.20f + r * Mathf.Sin(a), z);
                    });
                    ToyRoomYunaSculptor.AttachMesh("Train_Boiler", parent, boilerMesh, trainMat);

                    var cabMesh = CreateArchedCabinetMesh("Train_Cab", 0.28f, 0.38f, 0.24f, 0.06f);
                    var cabGo = ToyRoomYunaSculptor.AttachMesh("Train_Cab", parent, cabMesh, trainMat);
                    cabGo.transform.localPosition = new Vector3(0f, 0.06f, -0.14f);

                    var stackMesh = CreateTurnedColumnMesh("Train_Smokestack", 0.055f, 0.18f, 20, 12);
                    var stGo = ToyRoomYunaSculptor.AttachMesh("Train_Smokestack", parent, stackMesh, brassMat);
                    stGo.transform.localPosition = new Vector3(0f, 0.30f, 0.18f);

                    for (int side = -1; side <= 1; side += 2)
                    {
                        for (int w = 0; w < 2; w++)
                        {
                            var wheelMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Train_Wheel_{side}_{w}", new Vector3(side * 0.14f, 0.09f, -0.12f + w * 0.24f), new Vector3(0.025f, 0.085f, 0.085f), 18);
                            ToyRoomYunaSculptor.AttachMesh($"Wheel_{side}_{w}", parent, wheelMesh, brassMat);
                        }
                    }
                    break;
                }

                case "robot":
                {
                    var robotMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, RobotTexPath, outlineWidth: 0.0045f);
                    var glowGold = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(1.0f, 0.86f, 0.34f), emission: new Color(0.38f, 0.28f, 0.06f), outlineWidth: 0.003f);

                    var chestMesh = CreateBeveledBoxMesh("Robot_BeveledChest", new Vector3(0.28f, 0.32f, 0.22f), 0.045f, 24, 14);
                    var cGo = ToyRoomYunaSculptor.AttachMesh("Robot_Chest", parent, chestMesh, robotMat);
                    cGo.transform.localPosition = new Vector3(0f, 0.26f, 0f);

                    var headMesh = CreateBeveledBoxMesh("Robot_DomeHead", new Vector3(0.22f, 0.20f, 0.19f), 0.055f, 24, 14);
                    var hGo = ToyRoomYunaSculptor.AttachMesh("Robot_Head", parent, headMesh, robotMat);
                    hGo.transform.localPosition = new Vector3(0f, 0.52f, 0f);

                    for (int side = -1; side <= 1; side += 2)
                    {
                        var eyeMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Robot_Eye_{side}", new Vector3(side * 0.048f, 0.53f, 0.098f), new Vector3(0.028f, 0.028f, 0.015f), 14);
                        ToyRoomYunaSculptor.AttachMesh($"Eye_{side}", parent, eyeMesh, glowGold);

                        var armMesh = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                            $"Robot_AccordionArm_{side}",
                            new Vector3(side * 0.15f, 0.36f, 0f),
                            new Vector3(side * 0.21f, 0.30f, 0.04f),
                            new Vector3(side * 0.20f, 0.20f, 0.08f),
                            new Vector3(side * 0.16f, 0.14f, 0.10f),
                            0.042f,
                            0.9f,
                            12,
                            14);
                        ToyRoomYunaSculptor.AttachMesh($"Arm_{side}", parent, armMesh, robotMat);
                    }

                    var antennaBulb = ToyRoomYunaSculptor.CreateEllipsoidMesh("Robot_Antenna", new Vector3(0f, 0.67f, 0f), new Vector3(0.032f, 0.045f, 0.032f), 14);
                    ToyRoomYunaSculptor.AttachMesh("Robot_Antenna", parent, antennaBulb, glowGold);
                    break;
                }

                case "bunny":
                {
                    string wornBunnyTex = EnsureWornBunnyTexture();
                    var bunnyMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.66f, 0.52f, 0.44f), wornBunnyTex, outlineWidth: 0.004f);
                    bunnyMat.SetFloat("_BrushNoiseStrength", 0.025f);
                    var bellyMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.84f, 0.77f, 0.67f), wornBunnyTex, outlineWidth: 0.0025f);
                    var patchSageMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.58f, 0.64f, 0.56f), wornBunnyTex, outlineWidth: 0.0025f);
                    var patchOchreMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.76f, 0.64f, 0.48f), wornBunnyTex, outlineWidth: 0.0025f);
                    var stitchMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.24f, 0.16f, 0.12f), outlineWidth: 0f);
                    var innerEarMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.76f, 0.48f, 0.52f), outlineWidth: 0.002f);
                    var nosePink = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.76f, 0.42f, 0.48f), outlineWidth: 0.002f);
                    var eyeMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.10f, 0.07f, 0.06f), outlineWidth: 0f);
                    var shineMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, emission: new Color(0.42f, 0.42f, 0.42f), outlineWidth: 0f);

                    var root = new GameObject("Bunny_Root").transform;
                    root.SetParent(parent, false);
                    root.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    root.localScale = Vector3.one * 1.12f;

                    // Corpo rechonchudo 3D com barriguinha envelhecida
                    var bodyMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh("Bunny_PlumpBody", new Vector3(0f, 0.175f, 0f), new Vector3(0.145f, 0.165f, 0.150f), 24);
                    ToyRoomYunaSculptor.AttachMesh("Bunny_Body", root, bodyMesh, bunnyMat);

                    var bellyMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh("Bunny_Belly", new Vector3(0f, 0.170f, 0.055f), new Vector3(0.102f, 0.125f, 0.105f), 20);
                    ToyRoomYunaSculptor.AttachMesh("Bunny_Belly", root, bellyMesh, bellyMat);

                    // Costura central da barriga (rasgo remendado)
                    for (int s = 0; s < 4; s++)
                    {
                        float sy = 0.115f + s * 0.036f;
                        var seam = CreateBeveledBoxMesh($"Bunny_BellySeam_{s}", new Vector3(0.030f, 0.006f, 0.008f), 0.002f, 6, 4);
                        var seamGo = ToyRoomYunaSculptor.AttachMesh($"Bunny_BellySeam_{s}", root, seam, stitchMat);
                        seamGo.transform.localPosition = new Vector3(0f, sy, 0.158f);
                        seamGo.transform.localRotation = Quaternion.Euler(0f, 0f, (s % 2 == 0 ? 14f : -14f));
                    }

                    // Remendo de pano costurado na lateral da barriga
                    var bellyPatchMesh = CreateBeveledBoxMesh("Bunny_BellyPatch", new Vector3(0.072f, 0.068f, 0.014f), 0.005f, 10, 6);
                    var bellyPatchGo = ToyRoomYunaSculptor.AttachMesh("Bunny_BellyPatch", root, bellyPatchMesh, patchSageMat);
                    bellyPatchGo.transform.localPosition = new Vector3(-0.088f, 0.175f, 0.118f);
                    bellyPatchGo.transform.localRotation = Quaternion.Euler(6f, -34f, 12f);
                    AttachPatchStitches(bellyPatchGo.transform, stitchMat, 0.036f, 0.034f, 0.009f);

                    // Cabeca arredondada, remendo na testa e focinho gasto
                    var headMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh("Bunny_Head", new Vector3(0f, 0.390f, 0.020f), new Vector3(0.135f, 0.128f, 0.132f), 24);
                    ToyRoomYunaSculptor.AttachMesh("Bunny_Head", root, headMesh, bunnyMat);

                    var headPatchMesh = CreateBeveledBoxMesh("Bunny_HeadPatch", new Vector3(0.064f, 0.056f, 0.014f), 0.005f, 10, 6);
                    var headPatchGo = ToyRoomYunaSculptor.AttachMesh("Bunny_HeadPatch", root, headPatchMesh, patchOchreMat);
                    headPatchGo.transform.localPosition = new Vector3(0.068f, 0.452f, 0.112f);
                    headPatchGo.transform.localRotation = Quaternion.Euler(-18f, 26f, -15f);
                    AttachPatchStitches(headPatchGo.transform, stitchMat, 0.032f, 0.028f, 0.009f);

                    var muzzleMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh("Bunny_Muzzle", new Vector3(0f, 0.368f, 0.115f), new Vector3(0.062f, 0.048f, 0.048f), 18);
                    ToyRoomYunaSculptor.AttachMesh("Bunny_Muzzle", root, muzzleMesh, bellyMat);

                    var noseMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh("Bunny_Nose", new Vector3(0f, 0.386f, 0.156f), new Vector3(0.025f, 0.020f, 0.018f), 14);
                    ToyRoomYunaSculptor.AttachMesh("Bunny_Nose", root, noseMesh, nosePink);

                    // Rabinho de algodao gasto atras
                    var tailMesh = ToyRoomYunaSculptor.CreateEllipsoidMesh("Bunny_CottonTail", new Vector3(0f, 0.120f, -0.155f), new Vector3(0.068f, 0.068f, 0.068f), 18);
                    ToyRoomYunaSculptor.AttachMesh("Bunny_Tail", root, tailMesh, bellyMat);

                    for (int side = -1; side <= 1; side += 2)
                    {
                        // Orelhas longas 3D; a orelha direita fica levemente caida com costura de rasgo
                        var earOuter = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Bunny_EarOuter_{side}", Vector3.zero, new Vector3(0.045f, 0.155f, 0.032f), 18);
                        var earGo = ToyRoomYunaSculptor.AttachMesh($"Bunny_Ear_{side}", root, earOuter, bunnyMat);
                        earGo.transform.localPosition = side > 0
                            ? new Vector3(side * 0.072f, 0.578f, 0.012f)
                            : new Vector3(side * 0.062f, 0.595f, 0.005f);
                        earGo.transform.localRotation = side > 0
                            ? Quaternion.Euler(18f, 0f, -24f)
                            : Quaternion.Euler(6f, 0f, 12f);

                        var earInner = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Bunny_EarInner_{side}", new Vector3(0f, -0.005f, 0.016f), new Vector3(0.026f, 0.115f, 0.018f), 14);
                        ToyRoomYunaSculptor.AttachMesh($"Bunny_EarInner_{side}", earGo.transform, earInner, innerEarMat);

                        if (side > 0)
                        {
                            // Costuras em X num rasgo remendado na orelha caida
                            for (int st = 0; st < 3; st++)
                            {
                                var mend = CreateBeveledBoxMesh($"Bunny_EarMend_{st}", new Vector3(0.032f, 0.006f, 0.010f), 0.002f, 6, 4);
                                var mendGo = ToyRoomYunaSculptor.AttachMesh($"Bunny_EarMend_{st}", earGo.transform, mend, stitchMat);
                                mendGo.transform.localPosition = new Vector3(0.012f, 0.020f + st * 0.024f, 0.026f);
                                mendGo.transform.localRotation = Quaternion.Euler(0f, 0f, st % 2 == 0 ? 28f : -28f);
                            }
                        }

                        // Bracinhos e patinhas 3D
                        var arm = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Bunny_Arm_{side}", Vector3.zero, new Vector3(0.044f, 0.088f, 0.044f), 14);
                        var armGo = ToyRoomYunaSculptor.AttachMesh($"Bunny_Arm_{side}", root, arm, bunnyMat);
                        armGo.transform.localPosition = new Vector3(side * 0.125f, 0.215f, 0.065f);
                        armGo.transform.localRotation = Quaternion.Euler(-28f, 0f, side * 18f);

                        var foot = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Bunny_Foot_{side}", Vector3.zero, new Vector3(0.056f, 0.046f, 0.088f), 16);
                        var footGo = ToyRoomYunaSculptor.AttachMesh($"Bunny_Foot_{side}", root, foot, bunnyMat);
                        footGo.transform.localPosition = new Vector3(side * 0.088f, 0.052f, 0.105f);
                        footGo.transform.localRotation = Quaternion.Euler(0f, side * 18f, 0f);

                        var pawPad = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Bunny_PawPad_{side}", new Vector3(0f, 0f, 0.068f), new Vector3(0.036f, 0.030f, 0.022f), 12);
                        ToyRoomYunaSculptor.AttachMesh($"Bunny_PawPad_{side}", footGo.transform, pawPad, bellyMat);

                        // Olhos e brilho
                        var eye = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Bunny_Eye_{side}", new Vector3(side * 0.058f, 0.415f, 0.122f), new Vector3(0.018f, 0.022f, 0.014f), 14);
                        ToyRoomYunaSculptor.AttachMesh($"Bunny_Eye_{side}", root, eye, eyeMat);

                        var shine = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Bunny_EyeShine_{side}", new Vector3(side * 0.054f + 0.004f, 0.423f, 0.134f), new Vector3(0.006f, 0.006f, 0.004f), 8);
                        ToyRoomYunaSculptor.AttachMesh($"Bunny_EyeShine_{side}", root, shine, shineMat);
                    }
                    break;
                }

                case "duck":
                {
                    var duckMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(1.0f, 0.90f, 0.24f), DuckTexPath, outlineWidth: 0.003f);
                    duckMat.SetFloat("_BrushNoiseStrength", 0.008f);
                    var wingMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.98f, 0.78f, 0.14f), DuckTexPath, outlineWidth: 0.0025f);
                    wingMat.SetFloat("_BrushNoiseStrength", 0.008f);
                    var billMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.98f, 0.44f, 0.10f), outlineWidth: 0.0025f);
                    var cheekMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.98f, 0.56f, 0.58f), outlineWidth: 0f);
                    var crownMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(1.0f, 0.84f, 0.22f), emission: new Color(0.18f, 0.12f, 0.02f), outlineWidth: 0.002f);
                    var gemMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.90f, 0.18f, 0.28f), emission: new Color(0.25f, 0.04f, 0.08f), outlineWidth: 0f);
                    var eyeMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.10f, 0.07f, 0.06f), outlineWidth: 0f);
                    var shineMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, emission: new Color(0.5f, 0.5f, 0.5f), outlineWidth: 0f);

                    var root = new GameObject("Duck_Root").transform;
                    root.SetParent(parent, false);
                    root.localRotation = Quaternion.Euler(0f, -135f, 0f);
                    root.localScale = Vector3.one * 1.25f;

                    // Corpo 3D rechonchudo com peito arredondado e cauda arrebitada
                    var duckBody = ToyRoomYunaSculptor.CreateParametricMesh("Duck_SculptedBody", 36, 22, (u, v) =>
                    {
                        float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                        float a = (u - 0.5f) * 2f * Pi;
                        float cosA = Mathf.Cos(a);
                        float sinP = Mathf.Sin(p);
                        float cosP = Mathf.Cos(p);
                        float tailCurve = Mathf.Pow(Mathf.Clamp01(-cosA), 1.8f) * 0.13f;
                        float chestCurve = Mathf.Pow(Mathf.Clamp01(cosA), 1.5f) * 0.035f;
                        float rX = 0.185f * (1f - 0.26f * Mathf.Clamp01(-cosA));
                        float rZ = 0.245f;
                        return new Vector3(0f, 0.160f + tailCurve + chestCurve, 0f) + new Vector3(
                            rX * sinP * Mathf.Sin(a),
                            0.135f * cosP * (1f - 0.22f * Mathf.Clamp01(-cosA)),
                            rZ * sinP * cosA);
                    });
                    ToyRoomYunaSculptor.AttachMesh("Duck_Body", root, duckBody, duckMat);

                    // Cauda 3D arrebitada
                    var tailTuft = ToyRoomYunaSculptor.CreateEllipsoidMesh("Duck_TailTuft", new Vector3(0f, 0.265f, -0.22f), new Vector3(0.055f, 0.065f, 0.075f), 16);
                    var tailGo = ToyRoomYunaSculptor.AttachMesh("Duck_Tail", root, tailTuft, duckMat);
                    tailGo.transform.localRotation = Quaternion.Euler(-28f, 0f, 0f);

                    // Pescoco suave e cabeca arredondada
                    var duckNeck = ToyRoomYunaSculptor.CreateEllipsoidMesh("Duck_Neck", new Vector3(0f, 0.295f, 0.105f), new Vector3(0.092f, 0.095f, 0.092f), 18);
                    ToyRoomYunaSculptor.AttachMesh("Duck_Neck", root, duckNeck, duckMat);

                    var duckHead = ToyRoomYunaSculptor.CreateEllipsoidMesh("Duck_Head", new Vector3(0f, 0.405f, 0.125f), new Vector3(0.125f, 0.118f, 0.122f), 24);
                    ToyRoomYunaSculptor.AttachMesh("Duck_Head", root, duckHead, duckMat);

                    // Bico superior e inferior esculpidos
                    var upperBill = ToyRoomYunaSculptor.CreateEllipsoidMesh("Duck_UpperBill", new Vector3(0f, 0.388f, 0.248f), new Vector3(0.076f, 0.026f, 0.078f), 18);
                    ToyRoomYunaSculptor.AttachMesh("Duck_UpperBill", root, upperBill, billMat);
                    var lowerBill = ToyRoomYunaSculptor.CreateEllipsoidMesh("Duck_LowerBill", new Vector3(0f, 0.362f, 0.234f), new Vector3(0.060f, 0.018f, 0.064f), 16);
                    ToyRoomYunaSculptor.AttachMesh("Duck_LowerBill", root, lowerBill, billMat);

                    // Asas 3D volumosas e bochechas rosadas
                    for (int side = -1; side <= 1; side += 2)
                    {
                        var wing = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Duck_Wing_{side}", Vector3.zero, new Vector3(0.052f, 0.078f, 0.145f), 18);
                        var wingGo = ToyRoomYunaSculptor.AttachMesh($"Duck_Wing_{side}", root, wing, wingMat);
                        wingGo.transform.localPosition = new Vector3(side * 0.185f, 0.195f, -0.025f);
                        wingGo.transform.localRotation = Quaternion.Euler(10f, side * 14f, side * -18f);

                        var cheek = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Duck_Cheek_{side}", new Vector3(side * 0.095f, 0.388f, 0.188f), new Vector3(0.024f, 0.016f, 0.014f), 10);
                        ToyRoomYunaSculptor.AttachMesh($"Duck_Cheek_{side}", root, cheek, cheekMat);

                        var eye = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Duck_Eye_{side}", new Vector3(side * 0.074f, 0.430f, 0.218f), new Vector3(0.020f, 0.025f, 0.016f), 14);
                        ToyRoomYunaSculptor.AttachMesh($"Duck_Eye_{side}", root, eye, eyeMat);

                        var shine = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Duck_EyeShine_{side}", new Vector3(side * 0.070f + 0.005f, 0.438f, 0.231f), new Vector3(0.007f, 0.007f, 0.005f), 8);
                        ToyRoomYunaSculptor.AttachMesh($"Duck_EyeShine_{side}", root, shine, shineMat);
                    }

                    // Coroa real de 5 pontas com rubis
                    var crownMesh = ToyRoomYunaSculptor.CreateParametricMesh("Duck_RoyalCrown", 36, 10, (u, v) =>
                    {
                        float a = u * 2f * Pi;
                        float points = 0.032f * Mathf.Pow(Mathf.Abs(Mathf.Cos(a * 2.5f)), 1.5f);
                        float y = v * (0.048f + points);
                        float r = Mathf.Lerp(0.056f, 0.068f, v);
                        return new Vector3(r * Mathf.Sin(a), y, r * Mathf.Cos(a));
                    });
                    var crGo = ToyRoomYunaSculptor.AttachMesh("Duck_Crown", root, crownMesh, crownMat);
                    crGo.transform.localPosition = new Vector3(0f, 0.488f, 0.125f);

                    for (int g = 0; g < 5; g++)
                    {
                        float ga = g * (2f * Pi / 5f);
                        var gem = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Duck_CrownGem_{g}", new Vector3(0.068f * Mathf.Sin(ga), 0.562f, 0.125f + 0.068f * Mathf.Cos(ga)), new Vector3(0.011f, 0.011f, 0.011f), 8);
                        ToyRoomYunaSculptor.AttachMesh($"Duck_CrownGem_{g}", root, gem, gemMat);
                    }
                    break;
                }

                case "blocks":
                {
                    var b1 = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.95f, 0.46f, 0.44f), "Assets/Materials/T_ToyBlock_ABC.png", outlineWidth: 0.0045f);
                    var b2 = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.46f, 0.74f, 0.96f), "Assets/Materials/T_ToyBlock_ABC.png", outlineWidth: 0.0045f);
                    var b3 = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.98f, 0.86f, 0.42f), "Assets/Materials/T_ToyBlock_ABC.png", outlineWidth: 0.0045f);

                    var g1 = ToyRoomYunaSculptor.AttachMesh("Block_A", parent, CreateBeveledBoxMesh("ABC_BeveledBlock_A", Vector3.one * 0.22f, 0.032f, 20, 12), b1);
                    g1.transform.localPosition = new Vector3(-0.11f, 0.11f, 0f);
                    g1.transform.localRotation = Quaternion.Euler(0f, 12f, 0f);

                    var g2 = ToyRoomYunaSculptor.AttachMesh("Block_B", parent, CreateBeveledBoxMesh("ABC_BeveledBlock_B", Vector3.one * 0.22f, 0.032f, 20, 12), b2);
                    g2.transform.localPosition = new Vector3(0.12f, 0.11f, 0.04f);
                    g2.transform.localRotation = Quaternion.Euler(0f, -18f, 0f);

                    var g3 = ToyRoomYunaSculptor.AttachMesh("Block_C", parent, CreateBeveledBoxMesh("ABC_BeveledBlock_C", Vector3.one * 0.22f, 0.032f, 20, 12), b3);
                    g3.transform.localPosition = new Vector3(0.01f, 0.33f, 0.02f);
                    g3.transform.localRotation = Quaternion.Euler(0f, 6f, 0f);
                    break;
                }

                case "drum":
                {
                    var drumMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, "Assets/Materials/T_CircusDrum.png", outlineWidth: 0.0045f);
                    var hoopMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.92f, 0.68f, 0.30f), "Assets/Materials/T_Furniture_Wood_Painted.png", outlineWidth: 0.004f);

                    var drumShell = CreateTurnedColumnMesh("Drum_SculptedShell", 0.21f, 0.34f, 32, 16);
                    ToyRoomYunaSculptor.AttachMesh("Drum_Shell", parent, drumShell, drumMat);

                    for (int s = -1; s <= 1; s += 2)
                    {
                        var stick = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                            $"Drum_Stick_{s}",
                            new Vector3(-0.14f * s, 0.36f, -0.10f),
                            new Vector3(-0.04f * s, 0.37f, -0.02f),
                            new Vector3(0.06f * s, 0.38f, 0.05f),
                            new Vector3(0.15f * s, 0.39f, 0.12f),
                            0.016f,
                            1f,
                            10,
                            10);
                        ToyRoomYunaSculptor.AttachMesh($"Drumstick_{s}", parent, stick, hoopMat);
                    }
                    break;
                }

                default:
                {
                    var jackMat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, JackTexPath, outlineWidth: 0.003f);
                    var skinMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.99f, 0.85f, 0.76f), outlineWidth: 0.002f);
                    var goldMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.96f, 0.80f, 0.32f), outlineWidth: 0.0025f);
                    var eyeMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.12f, 0.08f, 0.06f), outlineWidth: 0f);
                    var blushMat = ToyRoomVisualFactory.CreatePainterlyMaterial(new Color(0.96f, 0.54f, 0.60f), outlineWidth: 0f);

                    var root = new GameObject("Jack_Root").transform;
                    root.SetParent(parent, false);
                    root.localRotation = Quaternion.Euler(0f, 180f, 0f);

                    var boxMesh = CreateBeveledBoxMesh("Jack_MusicBox", Vector3.one * 0.32f, 0.04f, 22, 12);
                    var boxGo = ToyRoomYunaSculptor.AttachMesh("Jack_Box", root, boxMesh, jackMat);
                    boxGo.transform.localPosition = new Vector3(0f, 0.16f, 0f);

                    var crankArm = CreateBeveledBoxMesh("Jack_Crank", new Vector3(0.10f, 0.025f, 0.025f), 0.008f, 8, 6);
                    var crGo = ToyRoomYunaSculptor.AttachMesh("Jack_Crank", root, crankArm, goldMat);
                    crGo.transform.localPosition = new Vector3(0.20f, 0.18f, 0f);

                    var jesterHead = ToyRoomYunaSculptor.CreateEllipsoidMesh("Jack_JesterHead", new Vector3(0f, 0.42f, 0f), new Vector3(0.095f, 0.095f, 0.095f), 20);
                    ToyRoomYunaSculptor.AttachMesh("Jack_JesterHead", root, jesterHead, skinMat);

                    for (int side = -1; side <= 1; side += 2)
                    {
                        var eye = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Jack_Eye_{side}", new Vector3(side * 0.036f, 0.435f, 0.085f), new Vector3(0.014f, 0.018f, 0.010f), 10);
                        ToyRoomYunaSculptor.AttachMesh($"Jack_Eye_{side}", root, eye, eyeMat);

                        var blush = ToyRoomYunaSculptor.CreateEllipsoidMesh($"Jack_Blush_{side}", new Vector3(side * 0.056f, 0.408f, 0.078f), new Vector3(0.018f, 0.012f, 0.008f), 10);
                        ToyRoomYunaSculptor.AttachMesh($"Jack_Blush_{side}", root, blush, blushMat);

                        var horn = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                            $"Jack_CapHorn_{side}",
                            new Vector3(side * 0.03f, 0.49f, 0f),
                            new Vector3(side * 0.09f, 0.56f, 0f),
                            new Vector3(side * 0.14f, 0.52f, 0f),
                            new Vector3(side * 0.16f, 0.44f, 0f),
                            0.032f,
                            0.85f,
                            12,
                            12);
                        ToyRoomYunaSculptor.AttachMesh($"JesterHorn_{side}", root, horn, goldMat);
                    }
                    break;
                }
            }
        }

        public static void BuildSculptedToySword(Transform swordRoot)
        {
            var bladeMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.96f, 0.88f, 0.68f),
                "Assets/Materials/T_Pastel_ToyWood.png",
                emission: new Color(0.28f, 0.22f, 0.10f),
                outlineWidth: 0.004f);
            var guardMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.46f, 0.82f, 0.92f),
                emission: new Color(0.14f, 0.32f, 0.42f),
                outlineWidth: 0.004f);
            var hiltMat = ToyRoomVisualFactory.CreatePainterlyMaterial(
                new Color(0.82f, 0.34f, 0.32f),
                ChestTexPath,
                outlineWidth: 0.004f);

            var bladeMesh = ToyRoomYunaSculptor.CreateParametricMesh("ToySword_Blade", 20, 18, (u, v) =>
            {
                float a = u * 2f * Pi;
                float taper = v < 0.82f ? 1f : Mathf.Cos((v - 0.82f) / 0.18f * Pi * 0.5f);
                float rx = 0.058f * taper;
                float rz = 0.018f * taper;
                float y = Mathf.Lerp(0.04f, 0.74f, v);
                return new Vector3(rx * Mathf.Cos(a), y, rz * Mathf.Sin(a));
            });
            ToyRoomYunaSculptor.AttachMesh("Sword_Blade", swordRoot, bladeMesh, bladeMat);

            var guardMesh = ToyRoomYunaSculptor.CreateCurveTubeMesh(
                "ToySword_WingedGuard",
                new Vector3(-0.20f, 0.05f, 0f),
                new Vector3(-0.07f, 0.01f, 0f),
                new Vector3(0.07f, 0.01f, 0f),
                new Vector3(0.20f, 0.05f, 0f),
                0.036f,
                0.65f,
                14,
                14);
            ToyRoomYunaSculptor.AttachMesh("Sword_Guard", swordRoot, guardMesh, guardMat);

            var gripMesh = CreateTurnedColumnMesh("ToySword_Grip", 0.034f, 0.24f, 16, 10);
            var gripGo = ToyRoomYunaSculptor.AttachMesh("Sword_Grip", swordRoot, gripMesh, hiltMat);
            gripGo.transform.localPosition = new Vector3(0f, -0.22f, 0f);
        }

        private static Mesh CreateBeveledBoxMesh(string name, Vector3 size, float bevel, int nu, int nv)
        {
            Vector3 half = size * 0.5f;
            float pExp = Mathf.Clamp(bevel / Mathf.Max(0.05f, half.x), 0.12f, 0.45f);
            return ToyRoomYunaSculptor.CreateParametricMesh(name, nu, nv, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = (u - 0.5f) * 2f * Pi;
                float sx = Mathf.Sin(p) * Mathf.Sin(a);
                float sy = Mathf.Cos(p);
                float sz = Mathf.Sin(p) * Mathf.Cos(a);
                return new Vector3(
                    half.x * SignedPow(sx, pExp),
                    half.y * SignedPow(sy, pExp),
                    half.z * SignedPow(sz, pExp));
            });
        }

        private static Mesh CreateArchedCabinetMesh(string name, float width, float height, float depth, float archHeight)
        {
            return ToyRoomYunaSculptor.CreateParametricMesh(name, 32, 20, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = (u - 0.5f) * 2f * Pi;
                float sx = SignedPow(Mathf.Sin(p) * Mathf.Sin(a), 0.18f);
                float sy = SignedPow(Mathf.Cos(p), 0.18f);
                float sz = SignedPow(Mathf.Sin(p) * Mathf.Cos(a), 0.18f);

                float x = width * 0.5f * sx;
                float z = depth * 0.5f * sz;
                float baseH = (sy + 1f) * 0.5f * height;
                float arch = sy > 0.2f ? archHeight * Mathf.Cos(sx * Pi * 0.5f) * ((sy - 0.2f) / 0.8f) : 0f;
                float molding = (baseH > height * 0.88f || baseH < height * 0.08f) ? 1.06f : 1.0f;
                return new Vector3(x * molding, baseH + arch, z * molding);
            });
        }

        private static Mesh CreateTurnedColumnMesh(string name, float radius, float height, int nu, int nv)
        {
            return ToyRoomYunaSculptor.CreateParametricMesh(name, nu, nv, (u, v) =>
            {
                float a = u * 2f * Pi;
                float y = v * height;
                float lathe = 1f + 0.12f * Mathf.Cos(v * Pi * 4f) + 0.06f * Mathf.Sin(v * Pi * 8f);
                float cap = v < 0.04f ? v / 0.04f : (v > 0.96f ? (1f - v) / 0.04f : 1f);
                float r = radius * lathe * Mathf.Sqrt(Mathf.Clamp01(cap));
                return new Vector3(r * Mathf.Cos(a), y, r * Mathf.Sin(a));
            });
        }

        private static Mesh CreateConeSpireMesh(string name, float radius, float height, int nu)
        {
            return ToyRoomYunaSculptor.CreateParametricMesh(name, nu, 12, (u, v) =>
            {
                float a = u * 2f * Pi;
                float r = radius * (1f - v) * (1f + 0.05f * Mathf.Sin(v * Pi));
                return new Vector3(r * Mathf.Cos(a), v * height, r * Mathf.Sin(a));
            });
        }

        private static Mesh CreateTuftedCushionMesh(string name, Vector3 size, int nu, int nv)
        {
            Vector3 half = size * 0.5f;
            return ToyRoomYunaSculptor.CreateParametricMesh(name, nu, nv, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = (u - 0.5f) * 2f * Pi;
                float sx = SignedPow(Mathf.Sin(p) * Mathf.Sin(a), 0.32f);
                float sy = SignedPow(Mathf.Cos(p), 0.32f);
                float sz = SignedPow(Mathf.Sin(p) * Mathf.Cos(a), 0.32f);
                float tufts = 1f - 0.04f * Mathf.Abs(Mathf.Sin(u * Pi * 12f) * Mathf.Sin(v * Pi * 6f));
                return new Vector3(half.x * sx * tufts, half.y * sy * tufts, half.z * sz * tufts);
            });
        }

        private static Mesh BuildSculptedBookRowMesh(string name, float width, float height, float depth, int seed)
        {
            return ToyRoomYunaSculptor.CreateParametricMesh(name, 36, 14, (u, v) =>
            {
                float p = Mathf.Clamp(v, 0.001f, 0.999f) * Pi;
                float a = (u - 0.5f) * 2f * Pi;
                float sx = SignedPow(Mathf.Sin(p) * Mathf.Sin(a), 0.18f);
                float sy = SignedPow(Mathf.Cos(p), 0.18f);
                float sz = SignedPow(Mathf.Sin(p) * Mathf.Cos(a), 0.18f);

                float bookIndex = Mathf.Floor((sx * 0.5f + 0.5f) * 9f);
                float hMod = 0.85f + 0.15f * Mathf.Sin(bookIndex * 2.3f + seed);
                float zMod = 0.90f + 0.10f * Mathf.Cos(bookIndex * 1.7f + seed);
                return new Vector3(width * 0.5f * sx, height * 0.5f * sy * hMod, depth * 0.5f * sz * zMod);
            });
        }

        private static float SignedPow(float val, float exp)
        {
            return Mathf.Sign(val) * Mathf.Pow(Mathf.Abs(val), exp);
        }

        private static void GenerateChestTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color crimsonWood = new Color(0.76f, 0.20f, 0.22f);
            Color darkCrimson = new Color(0.52f, 0.12f, 0.14f);
            Color brassGold = new Color(0.94f, 0.76f, 0.30f);
            Color brassDark = new Color(0.68f, 0.48f, 0.16f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float grain = Mathf.PerlinNoise(u * 48f, v * 12f);
                    Color col = Color.Lerp(darkCrimson, crimsonWood, grain);

                    bool isStrapU = Mathf.Abs(u - 0.18f) < 0.045f || Mathf.Abs(u - 0.82f) < 0.045f || Mathf.Abs(u - 0.50f) < 0.035f;
                    bool isStrapV = Mathf.Abs(v - 0.50f) < 0.05f || v < 0.07f || v > 0.93f;
                    if (isStrapU || isStrapV)
                    {
                        col = Color.Lerp(brassDark, brassGold, grain * 0.5f + 0.5f);
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateRobotTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color tealBlue = new Color(0.28f, 0.62f, 0.88f);
            Color steelShadow = new Color(0.16f, 0.40f, 0.64f);
            Color dialCream = new Color(0.98f, 0.94f, 0.84f);
            Color copper = new Color(0.88f, 0.54f, 0.26f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float brushed = Mathf.PerlinNoise(u * 64f, v * 24f);
                    Color col = Color.Lerp(steelShadow, tealBlue, 0.35f + 0.65f * brushed);

                    float du = (u - 0.5f) / 0.14f;
                    float dv = (v - 0.52f) / 0.14f;
                    float d = Mathf.Sqrt(du * du + dv * dv);
                    if (d < 1.0f)
                    {
                        col = d > 0.82f ? copper : dialCream;
                        if (d < 0.72f && Mathf.Abs(du - dv * 0.6f) < 0.08f && dv < 0.1f)
                        {
                            col = new Color(0.82f, 0.20f, 0.20f);
                        }
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateDuckTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color yellowBase = new Color(0.99f, 0.86f, 0.28f);
            Color warmGold = new Color(0.92f, 0.66f, 0.16f);
            Color wingCream = new Color(1.0f, 0.94f, 0.56f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float feathers = 0.5f + 0.5f * Mathf.Sin(u * Pi * 18f + Mathf.Sin(v * Pi * 12f));
                    Color col = Color.Lerp(warmGold, yellowBase, feathers);
                    if (v > 0.35f && v < 0.68f && Mathf.Abs(Mathf.Abs(u - 0.5f) - 0.24f) < 0.10f)
                    {
                        col = Color.Lerp(col, wingCream, 0.65f);
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateBunnyTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color creamFur = new Color(0.98f, 0.95f, 0.90f);
            Color warmTaupe = new Color(0.88f, 0.80f, 0.74f);
            Color earPink = new Color(0.96f, 0.68f, 0.74f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float plush = Mathf.PerlinNoise(u * 56f, v * 56f);
                    Color col = Color.Lerp(warmTaupe, creamFur, 0.35f + 0.65f * plush);
                    if (v > 0.65f && Mathf.Abs(u - 0.5f) < 0.22f)
                    {
                        col = Color.Lerp(col, earPink, 0.45f);
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateTrainTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color scarlet = new Color(0.86f, 0.22f, 0.22f);
            Color ironBlack = new Color(0.20f, 0.19f, 0.23f);
            Color brass = new Color(0.95f, 0.78f, 0.30f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    Color col = v < 0.52f ? scarlet : ironBlack;
                    if (Mathf.Abs(v - 0.52f) < 0.035f || Mathf.Abs(v - 0.78f) < 0.025f)
                    {
                        col = brass;
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateJackTexture(string path)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color purple = new Color(0.68f, 0.34f, 0.82f);
            Color teal = new Color(0.32f, 0.76f, 0.84f);
            Color gold = new Color(0.98f, 0.84f, 0.36f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float diamond = Mathf.Sin((u + v) * Pi * 8f) * Mathf.Sin((u - v) * Pi * 8f);
                    Color col = diamond > 0f ? purple : teal;
                    if (Mathf.Abs(diamond) < 0.12f)
                    {
                        col = gold;
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        /// <summary>
        /// Anexa um modelo 3D gerado via Hunyuan3D-2 (Assets/Art/ToyRoom/Models/Props/{propId}.obj + _Albedo.png)
        /// normalizando a escala para caber na pegada/altura canonica do objeto. Retorna false se o arquivo ainda nao existir.
        /// </summary>
        public static bool TryAttachGeneratedPropModel(Transform parent, string propId, float targetHeight, float maxFootprintX, float maxFootprintZ)
        {
            // bunny.obj e uma placa 2D achatada (0.016m em Z); usar sempre o modelo 3D esculpido
            if (propId == "bunny")
            {
                return false;
            }

            string objPath = $"Assets/Art/ToyRoom/Models/Props/{propId}.obj";
            if (!File.Exists(objPath))
            {
                return false;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
            if (prefab == null)
            {
                return false;
            }

            string texPath = $"Assets/Art/ToyRoom/Models/Props/{propId}_Albedo.png";
            if (propId == "bookshelf" && File.Exists(texPath))
            {
                RepairBookshelfAlbedoTexture(texPath);
            }

            if (File.Exists(texPath) && AssetDatabase.LoadAssetAtPath<Texture2D>(texPath) == null)
            {
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
            }

            var mat = ToyRoomVisualFactory.CreatePainterlyMaterial(Color.white, File.Exists(texPath) ? texPath : null, outlineWidth: 0.0018f);
            mat.SetFloat("_BrushNoiseStrength", 0.0f);
            EditorUtility.SetDirty(mat);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = $"Model3D_{propId}";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = GetGeneratedPropRotation(propId);
            instance.transform.localScale = Vector3.one;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            Bounds combined = new Bounds(parent.position, Vector3.zero);
            bool hasBounds = false;
            foreach (var r in renderers)
            {
                r.sharedMaterial = mat;
                if (!hasBounds)
                {
                    combined = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    combined.Encapsulate(r.bounds);
                }
            }

            if (hasBounds)
            {
                Vector3 size = combined.size;
                float scaleY = size.y > 0.001f ? targetHeight / size.y : 1f;
                float scaleX = size.x > 0.001f ? maxFootprintX / size.x : scaleY;
                float scaleZ = size.z > 0.001f ? maxFootprintZ / size.z : scaleY;
                float uniformScale = Mathf.Min(scaleY, Mathf.Min(scaleX, scaleZ));
                Vector3 localCenter = combined.center - parent.position;
                float localMinY = combined.min.y - parent.position.y;
                instance.transform.localScale = Vector3.one * uniformScale;
                instance.transform.localPosition = new Vector3(
                    -localCenter.x * uniformScale,
                    -localMinY * uniformScale,
                    -localCenter.z * uniformScale);
            }

            return true;
        }

        private static void RepairBookshelfAlbedoTexture(string texPath)
        {
            byte[] raw = File.ReadAllBytes(texPath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(raw))
            {
                UnityEngine.Object.DestroyImmediate(tex);
                return;
            }

            Color[] pixels = tex.GetPixels();
            int w = tex.width;
            int h = tex.height;
            bool changed = false;

            for (int y = 0; y < h; y++)
            {
                float v = (float)y / Mathf.Max(1, h - 1);
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    Color c = pixels[idx];
                    float maxC = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                    float minC = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                    if (minC > 0.55f && (maxC - minC) < 0.14f)
                    {
                        float u = (float)x / Mathf.Max(1, w - 1);
                        float grain = Mathf.Sin(v * 140f + Mathf.Sin(u * 24f) * 1.4f) * 0.035f
                                    + Mathf.Sin(u * 60f) * 0.015f;
                        pixels[idx] = new Color(
                            Mathf.Clamp01(0.71f + grain),
                            Mathf.Clamp01(0.41f + grain * 0.72f),
                            Mathf.Clamp01(0.15f + grain * 0.40f),
                            1f);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                tex.SetPixels(pixels);
                tex.Apply();
                File.WriteAllBytes(texPath, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
            }

            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void AttachPatchStitches(Transform patchTransform, Material stitchMat, float halfW, float halfH, float zOffset)
        {
            Vector3[] corners =
            {
                new Vector3(-halfW * 0.72f, halfH * 0.72f, zOffset),
                new Vector3(halfW * 0.72f, halfH * 0.72f, zOffset),
                new Vector3(-halfW * 0.72f, -halfH * 0.72f, zOffset),
                new Vector3(halfW * 0.72f, -halfH * 0.72f, zOffset)
            };

            for (int i = 0; i < corners.Length; i++)
            {
                var stitch = CreateBeveledBoxMesh($"PatchStitch_{i}", new Vector3(0.018f, 0.004f, 0.006f), 0.001f, 4, 4);
                var go = ToyRoomYunaSculptor.AttachMesh($"PatchStitch_{i}", patchTransform, stitch, stitchMat);
                go.transform.localPosition = corners[i];
                go.transform.localRotation = Quaternion.Euler(0f, 0f, i == 0 || i == 3 ? -45f : 45f);
            }
        }

        private static string EnsureWornBunnyTexture()
        {
            const string path = "Assets/Art/ToyRoom/Materials/T_Bunny_Worn_Painted.png";
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var baseCol = new Color(0.84f, 0.76f, 0.70f);
            var stainCol = new Color(0.56f, 0.43f, 0.34f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float weave = Mathf.Sin(u * 110f) * Mathf.Sin(v * 110f) * 0.03f;
                    float stainA = Mathf.Exp(-((u - 0.30f) * (u - 0.30f) + (v - 0.42f) * (v - 0.42f)) * 22f);
                    float stainB = Mathf.Exp(-((u - 0.72f) * (u - 0.72f) + (v - 0.68f) * (v - 0.68f)) * 18f);
                    float stainC = Mathf.Exp(-((u - 0.52f) * (u - 0.52f) + (v - 0.22f) * (v - 0.22f)) * 28f);
                    float stain = Mathf.Clamp01((stainA + stainB + stainC) * 0.75f);

                    Color col = Color.Lerp(baseCol, stainCol, stain);
                    col.r = Mathf.Clamp01(col.r + weave);
                    col.g = Mathf.Clamp01(col.g + weave);
                    col.b = Mathf.Clamp01(col.b + weave);
                    col.a = 1f;
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return path;
        }

        private static Quaternion GetGeneratedPropRotation(string propId)
        {
            switch (propId)
            {
                case "bookshelf":
                case "wardrobe":
                case "robot":
                case "toy-chest":
                case "toy-chest-open":
                    return Quaternion.Euler(0f, 180f, 0f);
                case "armchair":
                    return Quaternion.Euler(0f, 135f, 0f);
                default:
                    return Quaternion.identity;
            }
        }
    }
}
