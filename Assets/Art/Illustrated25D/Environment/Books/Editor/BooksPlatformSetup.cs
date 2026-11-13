using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using YunaLegend.DarkRoom;
using YunaLegend.Illustrated25D.Environment;

namespace YunaLegend.EditorScripts
{
    public static class BooksPlatformSetup
    {
        private const string Folder = "Assets/Art/Illustrated25D/Environment/Books";
        private const string TexturePath = Folder + "/Books_Platform_v1.png";
        private const string MaterialPath = Folder + "/Books_Platform_Painted.mat";
        private const string ContactMeshPath = Folder + "/Books_Platform_Contact.asset";
        private const string VisualMeshPath = Folder + "/Books_Platform_Visual.asset";

        public static void ApplyAndSave() => Run();

        public static void Run()
        {
            ConfigureTexture();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (texture == null) throw new System.InvalidOperationException("Missing books painting: " + TexturePath);
            var material = ConfigureMaterial(texture);
            var contact = SaveMesh(ContactMeshPath, IllustratedBookStackPlatformProfile.ContactMesh);
            var visual = SaveMesh(VisualMeshPath, IllustratedBookStackPlatformProfile.VisualMesh);

            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            var marker = Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(m => m.spec.index == 1);
            ApplyToMarker(marker, texture, material, contact, visual);
            RegenerateRoute(contact);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            var support = marker.Support;
            var collider = marker.transform.Find("Contacto_Fisico").GetComponentsInChildren<Collider>().Single();
            Debug.Log(string.Format("T05_BOOKS_SETUP_SAVED width={0:F6} height={1:F6} support={2:F6} tilt={3:F6} offsetX={4:F6} depth={5:F6} marker=({6:F6},{7:F6}) supportY={8:F6} regions={9} segments={10} collider={11}",
                IllustratedBookStackPlatformProfile.VisibleWidth, IllustratedBookStackPlatformProfile.VisibleHeight,
                IllustratedBookStackPlatformProfile.SupportWidth, IllustratedBookStackPlatformProfile.SupportTiltDegrees,
                IllustratedBookStackPlatformProfile.ContactOffsetX, IllustratedBookStackPlatformProfile.PhysicalDepth,
                marker.transform.position.x, marker.transform.position.y, marker.GetTopCenterWorld().y,
                support.Regions.Count, support.Segments.Count, collider.GetType().Name));
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
            if (shader == null) throw new System.InvalidOperationException("No transparent unlit shader available for the books.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "Books_Platform_Painted" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else
            {
                material.shader = shader;
            }
            material.SetTexture("_MainTex", texture);
            material.SetColor("_Color", new Color(0.96f, 0.95f, 0.96f, 1f));
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
            var legacy = marker.transform.Find("Visual_3D");
            if (legacy != null)
            {
                legacy.name = "Visual_3D_Referencia_Desativada";
                legacy.gameObject.SetActive(false);
            }

            var illustrated = marker.transform.Find("Visual_Ilustrado_Livros");
            if (illustrated != null) Object.DestroyImmediate(illustrated.gameObject);
            var visualGo = new GameObject("Visual_Ilustrado_Livros");
            visualGo.transform.SetParent(marker.transform, false);
            visualGo.transform.localPosition = Vector3.zero;
            visualGo.AddComponent<MeshFilter>().sharedMesh = visual;
            var renderer = visualGo.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var component = marker.GetComponent<IllustratedBookStackPlatform>();
            if (component == null) component = marker.gameObject.AddComponent<IllustratedBookStackPlatform>();
            component.paintedTexture = texture;
            component.contactMesh = contact;
            component.visualMesh = visual;
            component.supportWidth = IllustratedBookStackPlatformProfile.SupportWidth;
            component.supportTiltDegrees = IllustratedBookStackPlatformProfile.SupportTiltDegrees;
            component.contactOffsetX = IllustratedBookStackPlatformProfile.ContactOffsetX;
            component.supportHeightAboveCentre = IllustratedBookStackPlatformProfile.SupportHeightAboveCentre;
            component.physicalDepth = IllustratedBookStackPlatformProfile.PhysicalDepth;
            EditorUtility.SetDirty(component);
            EditorUtility.SetDirty(marker);
        }

        private static void RegenerateRoute(Mesh contact)
        {
            var specs = DarkRoomThreeStageCircuitGenerator.GenerateStage1Platforms()
                .Concat(DarkRoomThreeStageCircuitGenerator.GenerateStage2Platforms())
                .Concat(DarkRoomThreeStageCircuitGenerator.GenerateStage3Platforms())
                .ToDictionary(s => s.index);
            var markers = Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var marker in markers)
            {
                if (!specs.TryGetValue(marker.spec.index, out var spec)) continue;
                marker.spec = spec;
                marker.transform.position = new Vector3(spec.x, spec.y, 0f);
                if (marker.spec.index == 1) RebuildBooksContact(marker, contact);
                marker.InvalidateSupport();
                EditorUtility.SetDirty(marker);
            }
        }

        private static void RebuildBooksContact(DarkRoomPlatformMarker marker, Mesh contact)
        {
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
            var contactGo = new GameObject("Capa_Fisica_Livros");
            contactGo.transform.SetParent(contactRoot, false);
            var meshCollider = contactGo.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = contact;
            meshCollider.convex = false;
        }
    }
}
