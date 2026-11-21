using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using YunaLegend.BearChase.Core;
using YunaLegend.BearChase.Runtime;
using YunaLegend.Editor.ToyRoom;
using YunaLegend.ToyRoom.Core;

namespace YunaLegend.Editor.BearChase
{
    /// <summary>
    /// Constroi malhas continuas persistidas (.asset) para piso, teto, paredes e rodapes
    /// seguindo curvas horizontais e subidas/descidas de BearChaseTrackSampler,
    /// mais janelas em arco emissoras de luz, feixes solares, tapetes e moveis laterais.
    /// </summary>
    public static class BearChaseTrackMeshBuilder
    {
        private const string MeshesDir = "Assets/Art/BearChase/Meshes";

        public static GameObject BuildCorridor(
            BearChaseMaterialFactory matFactory,
            Vector3 chaseOrigin,
            out BearChaseLightingController.WindowLightAnchor[] windowAnchors)
        {
            Directory.CreateDirectory(MeshesDir);
            BearChaseTrackSpec track = BearChaseTrackSpec.Default;

            var root = new GameObject("Chase_Corridor_Root");
            root.transform.position = chaseOrigin;

            Material floorMat = matFactory.GetOrCreatePainterly(
                "CorridorFloor",
                Color.white,
                BearChaseAssetProcessor.CorridorFloorTexPath,
                Vector2.one,
                outlineWidth: 0f,
                glossStrength: 0.15f,
                glossPower: 18f,
                ambientScale: 0.78f);

            Material wallMat = matFactory.GetOrCreatePainterly(
                "CorridorWallpaper",
                new Color(0.95f, 0.91f, 0.84f),
                $"{BearChaseAssetProcessor.TexturesDir}/wallpaper-v1.png",
                Vector2.one,
                outlineWidth: 0f,
                glossStrength: 0f,
                ambientScale: 0.75f);

            Material baseboardMat = matFactory.GetOrCreatePainterly(
                "CorridorBaseboard",
                new Color(0.96f, 0.88f, 0.78f),
                $"{BearChaseAssetProcessor.TexturesDir}/oak-trim-v1.png",
                Vector2.one,
                outlineWidth: 0f,
                glossStrength: 0.08f,
                ambientScale: 0.74f);

            Material ceilingMat = matFactory.GetOrCreatePainterly(
                "CorridorCeiling",
                new Color(1.0f, 0.90f, 0.78f),
                BearChaseAssetProcessor.CeilingTexPath,
                Vector2.one,
                emission: new Color(0.16f, 0.11f, 0.08f),
                outlineWidth: 0f,
                glossStrength: 0f,
                ambientScale: 1.25f);

            Material rugMat = matFactory.GetOrCreateCutoutLit(
                "CorridorRug",
                Color.white,
                $"{BearChaseAssetProcessor.TexturesDir}/rectangular-rug-v1.png");

            Material windowMat = matFactory.GetOrCreateCutoutLit(
                "CorridorWindow",
                Color.white,
                $"{BearChaseAssetProcessor.TexturesDir}/window-curtains-v1.png",
                emission: new Color(0.24f, 0.36f, 0.46f));

            Material sunbeamMat = matFactory.GetOrCreateSunbeam(
                "CorridorSunbeam",
                new Color(1.0f, 0.90f, 0.62f, 0.14f),
                BearChaseAssetProcessor.SoftGlowTexPath);

            Material sunPoolMat = matFactory.GetOrCreateSunbeam(
                "CorridorSunPool",
                new Color(1.0f, 0.88f, 0.54f, 0.18f),
                BearChaseAssetProcessor.SoftGlowTexPath);

            BuildContinuousShell(root.transform, track, floorMat, wallMat, baseboardMat, ceilingMat);
            BuildStartAndExitWalls(root.transform, track, matFactory, wallMat, baseboardMat);
            windowAnchors = BuildWindowsAndSunbeams(root.transform, chaseOrigin, track, windowMat, sunbeamMat, sunPoolMat);
            BuildSideDecorFurnitureAndRugs(root.transform, track, matFactory, rugMat);

            return root;
        }

        private static void BuildContinuousShell(
            Transform parent,
            BearChaseTrackSpec track,
            Material floorMat,
            Material wallMat,
            Material baseboardMat,
            Material ceilingMat)
        {
            const float step = 1.25f;
            float zStart = -14f;
            float zEnd = track.TrackLength + 24f;
            int count = Mathf.CeilToInt((zEnd - zStart) / step) + 1;

            var stations = new List<TrackStation>(count);
            float arcLen = 0f;
            Vector3 prevCenter = Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                float z = Mathf.Min(zEnd, zStart + i * step);
                float cx = BearChaseTrackSampler.CenterX(z, track);
                float cy = BearChaseTrackSampler.ElevationY(z, track);
                float yaw = BearChaseTrackSampler.YawRadians(z, track);
                Vector3 center = new Vector3(cx, cy, z);
                if (i > 0)
                {
                    arcLen += Vector3.Distance(prevCenter, center);
                }

                prevCenter = center;
                Vector3 right = new Vector3(Mathf.Cos(yaw), 0f, -Mathf.Sin(yaw));
                stations.Add(new TrackStation
                {
                    Center = center,
                    Right = right,
                    ArcLen = arcLen
                });
            }

            Mesh floorMesh = SaveOrUpdateMesh("BearChase_FloorMesh", BuildFloorMesh(stations, track.CorridorHalfWidth + 0.35f));
            Mesh ceilingMesh = SaveOrUpdateMesh("BearChase_CeilingMesh", BuildCeilingMesh(stations, track.CorridorHalfWidth + 0.35f, track.WallHeight));
            Mesh wallLMesh = SaveOrUpdateMesh("BearChase_WallLeftMesh", BuildWallMesh(stations, -track.CorridorHalfWidth, 0.86f, track.WallHeight + 0.06f, faceRight: true));
            Mesh wallRMesh = SaveOrUpdateMesh("BearChase_WallRightMesh", BuildWallMesh(stations, track.CorridorHalfWidth, 0.86f, track.WallHeight + 0.06f, faceRight: false));
            Mesh baseLMesh = SaveOrUpdateMesh("BearChase_BaseboardLeftMesh", BuildWallMesh(stations, -track.CorridorHalfWidth + 0.08f, -0.02f, 0.92f, faceRight: true, vTile: 1.0f));
            Mesh baseRMesh = SaveOrUpdateMesh("BearChase_BaseboardRightMesh", BuildWallMesh(stations, track.CorridorHalfWidth - 0.08f, -0.02f, 0.92f, faceRight: false, vTile: 1.0f));

            AttachMeshObject("Corridor_Floor", parent, floorMesh, floorMat, receiveShadows: true, castShadows: false);
            AttachMeshObject("Corridor_Ceiling", parent, ceilingMesh, ceilingMat, receiveShadows: false, castShadows: false);
            AttachMeshObject("Corridor_Wall_Left", parent, wallLMesh, wallMat, receiveShadows: true, castShadows: false);
            AttachMeshObject("Corridor_Wall_Right", parent, wallRMesh, wallMat, receiveShadows: true, castShadows: false);
            AttachMeshObject("Corridor_Baseboard_Left", parent, baseLMesh, baseboardMat, receiveShadows: true, castShadows: false);
            AttachMeshObject("Corridor_Baseboard_Right", parent, baseRMesh, baseboardMat, receiveShadows: true, castShadows: false);
        }

        private static void BuildStartAndExitWalls(
            Transform parent,
            BearChaseTrackSpec track,
            BearChaseMaterialFactory matFactory,
            Material wallMat,
            Material baseboardMat)
        {
            float startX = BearChaseTrackSampler.CenterX(-12f, track);
            float startY = BearChaseTrackSampler.ElevationY(-12f, track);
            CreateBox("Corridor_StartBackWall", parent, new Vector3(startX, startY + track.WallHeight * 0.5f, -12.5f), new Vector3(track.CorridorHalfWidth * 2.4f, track.WallHeight, 0.3f), wallMat);
            CreateBox("Corridor_StartBackBaseboard", parent, new Vector3(startX, startY + 0.45f, -12.3f), new Vector3(track.CorridorHalfWidth * 2.4f, 0.9f, 0.12f), baseboardMat);

            float endX = BearChaseTrackSampler.CenterX(track.TrackLength + 18f, track);
            float endY = BearChaseTrackSampler.ElevationY(track.TrackLength + 18f, track);
            Material exitGlowMat = matFactory.GetOrCreatePainterly(
                "CorridorExitGlow",
                new Color(1f, 0.96f, 0.84f),
                null,
                Vector2.one,
                emission: new Color(1.45f, 1.30f, 1.02f),
                outlineWidth: 0f);
            CreateBox("Corridor_EndBrightDoor", parent, new Vector3(endX, endY + 2.3f, track.TrackLength + 18f), new Vector3(5.2f, 4.6f, 0.35f), exitGlowMat);
        }

        private static BearChaseLightingController.WindowLightAnchor[] BuildWindowsAndSunbeams(
            Transform parent,
            Vector3 chaseOrigin,
            BearChaseTrackSpec track,
            Material windowMat,
            Material sunbeamMat,
            Material sunPoolMat)
        {
            var windowsRoot = new GameObject("Corridor_Windows");
            windowsRoot.transform.SetParent(parent, false);
            var anchors = new List<BearChaseLightingController.WindowLightAnchor>();

            int index = 0;
            for (float z = 8f; z <= track.TrackLength - 4f; z += 18f)
            {
                bool leftSide = (index % 2) == 0;
                index++;

                float cx = BearChaseTrackSampler.CenterX(z, track);
                float cy = BearChaseTrackSampler.ElevationY(z, track);
                float yaw = BearChaseTrackSampler.YawRadians(z, track);
                float pitch = BearChaseTrackSampler.PitchRadians(z, track);
                Vector3 right = new Vector3(Mathf.Cos(yaw), 0f, -Mathf.Sin(yaw));
                float wallOffset = leftSide ? -(track.CorridorHalfWidth - 0.14f) : (track.CorridorHalfWidth - 0.14f);
                Vector3 wallPos = new Vector3(cx, cy + 2.65f, z) + right * wallOffset;

                var winGo = new GameObject($"Window_{index:00}");
                winGo.transform.SetParent(windowsRoot.transform, false);
                winGo.transform.localPosition = wallPos;
                winGo.transform.localRotation = Quaternion.Euler(
                    pitch * Mathf.Rad2Deg,
                    yaw * Mathf.Rad2Deg + (leftSide ? -90f : 90f),
                    0f);

                GameObject pane = GameObject.CreatePrimitive(PrimitiveType.Quad);
                pane.name = "ArchedFrameQuad";
                pane.transform.SetParent(winGo.transform, false);
                pane.transform.localScale = new Vector3(2.25f, 2.55f, 1f);
                Object.DestroyImmediate(pane.GetComponent<Collider>());
                var paneRend = pane.GetComponent<Renderer>();
                paneRend.sharedMaterial = windowMat;
                paneRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                // Feixe diagonal suave descendo para o corredor
                GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Quad);
                beam.name = "Sunbeam";
                beam.transform.SetParent(winGo.transform, false);
                beam.transform.localPosition = new Vector3(0f, -0.75f, -2.0f);
                beam.transform.localRotation = Quaternion.Euler(42f, 0f, 0f);
                beam.transform.localScale = new Vector3(2.6f, 4.4f, 1f);
                Object.DestroyImmediate(beam.GetComponent<Collider>());
                var beamRend = beam.GetComponent<Renderer>();
                beamRend.sharedMaterial = sunbeamMat;
                beamRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                // Poca solar suave no piso abaixo da janela
                float poolOffset = leftSide ? -1.45f : 1.45f;
                Vector3 poolLocalPos = new Vector3(cx, cy + 0.028f, z) + right * poolOffset;
                GameObject pool = GameObject.CreatePrimitive(PrimitiveType.Quad);
                pool.name = $"SunPool_{index:00}";
                pool.transform.SetParent(windowsRoot.transform, false);
                pool.transform.localPosition = poolLocalPos;
                pool.transform.localRotation = Quaternion.Euler(90f + pitch * Mathf.Rad2Deg, yaw * Mathf.Rad2Deg, 0f);
                pool.transform.localScale = new Vector3(4.2f, 4.4f, 1f);
                Object.DestroyImmediate(pool.GetComponent<Collider>());
                var poolRend = pool.GetComponent<Renderer>();
                poolRend.sharedMaterial = sunPoolMat;
                poolRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                Vector3 inward = leftSide ? right : -right;
                Vector3 spotWorldPos = chaseOrigin + wallPos + inward * 0.45f + new Vector3(0f, 0.35f, 0f);
                Vector3 aimWorldPos = chaseOrigin + new Vector3(cx, cy + 0.75f, z) + (-inward * 2.2f);
                anchors.Add(new BearChaseLightingController.WindowLightAnchor
                {
                    LocalZ = z,
                    WorldPosition = spotWorldPos,
                    AimTarget = aimWorldPos
                });
            }

            return anchors.ToArray();
        }

        private static void BuildSideDecorFurnitureAndRugs(
            Transform parent,
            BearChaseTrackSpec track,
            BearChaseMaterialFactory matFactory,
            Material rugMat)
        {
            var decorRoot = new GameObject("Corridor_SideDecor");
            decorRoot.transform.SetParent(parent, false);

            Material cabinetMat = matFactory.GetOrCreatePainterly(
                "SideCabinetWood",
                Color.white,
                BearChaseAssetProcessor.CabinetWoodTexPath,
                Vector2.one,
                outlineWidth: 0.002f);

            Material velvetMat = matFactory.GetOrCreatePainterly(
                "SideArmchairVelvet",
                Color.white,
                BearChaseAssetProcessor.RedVelvetTexPath,
                Vector2.one,
                outlineWidth: 0.002f);

            ToyRoomVisualFactory.BeginMaterialScope("Assets/Art/BearChase/Materials", "M_BearChase_SideProp_");
            try
            {
                int slot = 0;
                for (float z = 14f; z <= track.TrackLength - 18f; z += 19f)
                {
                    float cx = BearChaseTrackSampler.CenterX(z, track);
                    float cy = BearChaseTrackSampler.ElevationY(z, track);
                    float yaw = BearChaseTrackSampler.YawRadians(z, track);
                    float pitch = BearChaseTrackSampler.PitchRadians(z, track);
                    Vector3 right = new Vector3(Mathf.Cos(yaw), 0f, -Mathf.Sin(yaw));

                    bool onLeft = (slot % 2) == 0;
                    float sideOffset = onLeft ? -3.88f : 3.88f;
                    var propGo = new GameObject($"SideDecor_{slot:00}");
                    propGo.transform.SetParent(decorRoot.transform, false);
                    propGo.transform.localPosition = new Vector3(cx, cy, z) + right * sideOffset;

                    float faceInwardYaw = yaw * Mathf.Rad2Deg + (onLeft ? 90f : -90f);
                    propGo.transform.localRotation = Quaternion.Euler(pitch * Mathf.Rad2Deg, faceInwardYaw, 0f);

                    int kind = slot % 3;
                    if (kind == 0)
                    {
                        ToyRoomPropSculptor.BuildSculptedFurniture(propGo.transform, new FurnitureSpec("bookshelf", "Estante", new ToyRoomPoint(0f, 0f), 1.45f, 0.68f));
                        propGo.transform.localScale = Vector3.one * 1.12f;
                        ApplyMaterialToNonBooks(propGo, cabinetMat);
                    }
                    else if (kind == 1)
                    {
                        ToyRoomPropSculptor.BuildSculptedFurniture(propGo.transform, new FurnitureSpec("armchair", "Poltrona", new ToyRoomPoint(0f, 0f), 1.25f, 1.05f));
                        propGo.transform.localScale = Vector3.one * 1.04f;
                        ApplyMaterialToNonLegs(propGo, velvetMat);
                    }
                    else
                    {
                        ToyRoomPropSculptor.BuildSculptedFurniture(propGo.transform, new FurnitureSpec("table", "Mesinha", new ToyRoomPoint(0f, 0f), 1.05f, 1.05f));
                        propGo.transform.localScale = Vector3.one * 1.04f;
                        ApplyMaterialToAll(propGo, cabinetMat);
                    }

                    if (slot % 2 == 1)
                    {
                        float rugZ = z + 8f;
                        float rcx = BearChaseTrackSampler.CenterX(rugZ, track);
                        float rcy = BearChaseTrackSampler.ElevationY(rugZ, track);
                        float ryaw = BearChaseTrackSampler.YawRadians(rugZ, track);
                        float rpitch = BearChaseTrackSampler.PitchRadians(rugZ, track);

                        GameObject rug = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        rug.name = $"SideRug_{slot:00}";
                        rug.transform.SetParent(decorRoot.transform, false);
                        rug.transform.localPosition = new Vector3(rcx, rcy + 0.018f, rugZ);
                        rug.transform.localRotation = Quaternion.Euler(90f + rpitch * Mathf.Rad2Deg, ryaw * Mathf.Rad2Deg, 0f);
                        rug.transform.localScale = new Vector3(3.6f, 2.5f, 1f);
                        Object.DestroyImmediate(rug.GetComponent<Collider>());
                        var rRend = rug.GetComponent<Renderer>();
                        rRend.sharedMaterial = rugMat;
                        rRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }

                    slot++;
                }
            }
            finally
            {
                ToyRoomVisualFactory.EndMaterialScope();
            }
        }

        private static void ApplyMaterialToNonBooks(GameObject root, Material mat)
        {
            Renderer[] rends = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                if (!rends[i].name.Contains("Book"))
                {
                    rends[i].sharedMaterial = mat;
                }
            }
        }

        private static void ApplyMaterialToAll(GameObject root, Material mat)
        {
            Renderer[] rends = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                rends[i].sharedMaterial = mat;
            }
        }

        private static void ApplyMaterialToNonLegs(GameObject root, Material mat)
        {
            Renderer[] rends = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                if (!rends[i].name.Contains("Leg"))
                {
                    rends[i].sharedMaterial = mat;
                }
            }
        }

        private struct TrackStation
        {
            public Vector3 Center;
            public Vector3 Right;
            public float ArcLen;
        }

        private static Mesh BuildFloorMesh(List<TrackStation> stations, float halfWidth)
        {
            const int cols = 5;
            var verts = new List<Vector3>(stations.Count * cols);
            var uvs = new List<Vector2>(stations.Count * cols);
            var tris = new List<int>((stations.Count - 1) * (cols - 1) * 6);

            for (int i = 0; i < stations.Count; i++)
            {
                TrackStation st = stations[i];
                float v = st.ArcLen / 3.4f;
                for (int c = 0; c < cols; c++)
                {
                    float uFrac = c / (float)(cols - 1);
                    float xOffset = Mathf.Lerp(-halfWidth, halfWidth, uFrac);
                    verts.Add(st.Center + st.Right * xOffset);
                    uvs.Add(new Vector2(uFrac * 2.0f, v));
                }

                if (i == 0)
                {
                    continue;
                }

                int prevRow = (i - 1) * cols;
                int currRow = i * cols;
                for (int c = 0; c < cols - 1; c++)
                {
                    int a = prevRow + c;
                    int b = prevRow + c + 1;
                    int d = currRow + c;
                    int e = currRow + c + 1;

                    tris.Add(a);
                    tris.Add(d);
                    tris.Add(b);
                    tris.Add(b);
                    tris.Add(d);
                    tris.Add(e);
                }
            }

            var mesh = new Mesh { name = "BearChase_FloorMesh" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildCeilingMesh(List<TrackStation> stations, float halfWidth, float height)
        {
            const int cols = 5;
            var verts = new List<Vector3>(stations.Count * cols);
            var uvs = new List<Vector2>(stations.Count * cols);
            var tris = new List<int>((stations.Count - 1) * (cols - 1) * 6);

            for (int i = 0; i < stations.Count; i++)
            {
                TrackStation st = stations[i];
                float v = st.ArcLen / 4.2f;
                for (int c = 0; c < cols; c++)
                {
                    float uFrac = c / (float)(cols - 1);
                    float xOffset = Mathf.Lerp(-halfWidth, halfWidth, uFrac);
                    verts.Add(st.Center + st.Right * xOffset + new Vector3(0f, height, 0f));
                    uvs.Add(new Vector2(uFrac, v));
                }

                if (i == 0)
                {
                    continue;
                }

                int prevRow = (i - 1) * cols;
                int currRow = i * cols;
                for (int c = 0; c < cols - 1; c++)
                {
                    int a = prevRow + c;
                    int b = prevRow + c + 1;
                    int d = currRow + c;
                    int e = currRow + c + 1;

                    tris.Add(a);
                    tris.Add(b);
                    tris.Add(d);
                    tris.Add(b);
                    tris.Add(e);
                    tris.Add(d);
                }
            }

            var mesh = new Mesh { name = "BearChase_CeilingMesh" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildWallMesh(
            List<TrackStation> stations,
            float lateralOffset,
            float yMin,
            float yMax,
            bool faceRight,
            float vTile = 1.6f)
        {
            var verts = new List<Vector3>(stations.Count * 2);
            var uvs = new List<Vector2>(stations.Count * 2);
            var tris = new List<int>((stations.Count - 1) * 6);

            for (int i = 0; i < stations.Count; i++)
            {
                TrackStation st = stations[i];
                Vector3 basePos = st.Center + st.Right * lateralOffset;
                float u = st.ArcLen / 4.2f;

                verts.Add(basePos + new Vector3(0f, yMin, 0f));
                uvs.Add(new Vector2(u, 0f));

                verts.Add(basePos + new Vector3(0f, yMax, 0f));
                uvs.Add(new Vector2(u, vTile));

                if (i == 0)
                {
                    continue;
                }

                int prev = (i - 1) * 2;
                int curr = i * 2;

                if (faceRight)
                {
                    tris.Add(prev);
                    tris.Add(prev + 1);
                    tris.Add(curr);
                    tris.Add(prev + 1);
                    tris.Add(curr + 1);
                    tris.Add(curr);
                }
                else
                {
                    tris.Add(prev);
                    tris.Add(curr);
                    tris.Add(prev + 1);
                    tris.Add(prev + 1);
                    tris.Add(curr);
                    tris.Add(curr + 1);
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh SaveOrUpdateMesh(string assetName, Mesh generated)
        {
            string path = $"{MeshesDir}/{assetName}.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                generated.name = assetName;
                AssetDatabase.CreateAsset(generated, path);
                return generated;
            }

            existing.Clear();
            existing.vertices = generated.vertices;
            existing.normals = generated.normals;
            existing.uv = generated.uv;
            existing.triangles = generated.triangles;
            existing.RecalculateBounds();
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(generated);
            return existing;
        }

        private static void AttachMeshObject(
            string name,
            Transform parent,
            Mesh mesh,
            Material mat,
            bool receiveShadows,
            bool castShadows)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.receiveShadows = receiveShadows;
            mr.shadowCastingMode = castShadows
                ? UnityEngine.Rendering.ShadowCastingMode.On
                : UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static void CreateBox(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }
}
