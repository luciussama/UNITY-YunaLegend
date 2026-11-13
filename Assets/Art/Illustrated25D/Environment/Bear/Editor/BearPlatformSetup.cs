using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.EditorScripts
{
    public static class BearPlatformSetup
    {
        private const string Folder = "Assets/Art/Illustrated25D/Environment/Bear";
        private const string TexturePath = Folder + "/Bear_Platform_v1.png";
        private const string MaterialPath = Folder + "/Bear_Platform_Painted.mat";
        private const string ContactMeshPath = Folder + "/Bear_Platform_Contact.asset";
        private const string VisualMeshPath = Folder + "/Bear_Platform_Visual.asset";

        public static void ApplyAndSave() => Run();

        public static void Run()
        {
            ConfigureTexture();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (texture == null) throw new System.InvalidOperationException("Missing bear painting: " + TexturePath);
            var material = ConfigureMaterial(texture);
            var contact = SaveMesh(ContactMeshPath, IllustratedBearPlatformProfile.ContactMesh);
            var visual = SaveMesh(VisualMeshPath, IllustratedBearPlatformProfile.VisualMesh);

            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            var marker = Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(m => m.spec.index == 0);
            ApplyToMarker(marker, texture, material, contact, visual);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            var support = marker.Support;
            Debug.Log(string.Format("T05_BEAR_SETUP_SAVED width={0:F6} depth={1:F6} crownY={2:F6} regions={3} segments={4} minX={5:F6} maxX={6:F6} collider={7}",
                IllustratedBearPlatformProfile.VisibleWidth, IllustratedBearPlatformProfile.PhysicalDepth,
                marker.GetTopCenterWorld().y, support.Regions.Count, support.Segments.Count, support.MinX, support.MaxX,
                marker.transform.Find("Contacto_Fisico").GetComponentsInChildren<Collider>().Single().GetType().Name));
        }

        private static void ConfigureTexture()
        {
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static Material ConfigureMaterial(Texture2D texture)
        {
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new System.InvalidOperationException("No transparent unlit shader available for the bear.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "Bear_Platform_Painted" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else
            {
                material.shader = shader;
            }
            material.SetTexture("_MainTex", texture);
            material.SetColor("_Color", new Color(0.94f, 0.94f, 0.97f, 1f));
            material.renderQueue = 3000;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Mesh SaveMesh(string path, Mesh source)
        {
            if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null) AssetDatabase.DeleteAsset(path);
            var copy = Object.Instantiate(source);
            copy.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(copy, path);
            return copy;
        }

        private static void ApplyToMarker(DarkRoomPlatformMarker marker, Texture2D texture, Material material, Mesh contact, Mesh visual)
        {
            var authored = DarkRoomThreeStageCircuitGenerator.GenerateStage1Platforms()[0];
            var spec = marker.spec;
            spec.width = authored.width;
            spec.height = authored.height;
            spec.depth = authored.depth;
            spec.contact = authored.contact;
            spec.x = authored.x;
            spec.y = authored.y;
            marker.spec = spec;
            marker.transform.position = new Vector3(spec.x, spec.y, marker.transform.position.z);

            var legacy = marker.transform.Find("Visual_3D");
            if (legacy != null)
            {
                legacy.name = "Visual_3D_Referencia_Desativada";
                legacy.gameObject.SetActive(false);
            }

            var illustrated = marker.transform.Find("Visual_Ilustrado_Urso");
            if (illustrated != null) Object.DestroyImmediate(illustrated.gameObject);
            var visualGo = new GameObject("Visual_Ilustrado_Urso");
            visualGo.transform.SetParent(marker.transform, false);
            visualGo.transform.localPosition = Vector3.zero;
            visualGo.AddComponent<MeshFilter>().sharedMesh = visual;
            var renderer = visualGo.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var component = marker.GetComponent<IllustratedBearPlatform>();
            if (component == null) component = marker.gameObject.AddComponent<IllustratedBearPlatform>();
            component.paintedTexture = texture;
            component.contactMesh = contact;
            component.visualMesh = visual;
            component.crownHeightAboveFloor = IllustratedBearPlatformProfile.CrownHeightAboveFloor;
            component.visibleWidth = IllustratedBearPlatformProfile.VisibleWidth;
            component.contactOffsetX = IllustratedBearPlatformProfile.ContactOffsetX;

            var contactRoot = marker.transform.Find("Contacto_Fisico");
            if (contactRoot == null)
            {
                contactRoot = new GameObject("Contacto_Fisico").transform;
                contactRoot.SetParent(marker.transform, false);
            }
            foreach (var collider in contactRoot.GetComponents<Collider>()) Object.DestroyImmediate(collider);
            while (contactRoot.childCount > 0) Object.DestroyImmediate(contactRoot.GetChild(0).gameObject);
            contactRoot.localPosition = Vector3.zero;
            contactRoot.localRotation = Quaternion.identity;
            contactRoot.localScale = Vector3.one;
            var contactGo = new GameObject("Silhueta_Fisica_Urso");
            contactGo.transform.SetParent(contactRoot, false);
            var meshCollider = contactGo.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = contact;
            meshCollider.convex = false;

            marker.InvalidateSupport();
            EditorUtility.SetDirty(component);
            EditorUtility.SetDirty(marker);
        }
    }
}
