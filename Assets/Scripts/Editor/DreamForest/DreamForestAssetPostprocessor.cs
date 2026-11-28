using System.IO;
using UnityEditor;
using UnityEngine;

namespace YunaLegend.DreamForest.Editor
{
    /// <summary>
    /// Ensures all Dream Forest PNGs import at exact native pixel dimensions (no NPOT rescaling)
    /// and enables Read/Write on textures needed for feathered backdrop generation and visual contract checks.
    /// </summary>
    public sealed class DreamForestAssetPostprocessor : AssetPostprocessor
    {
        private const string FolderPrefix = "Assets/Art/DreamForest/Resources/DreamForest/";

        private static readonly string[] ReadableTextures =
        {
            "forest-blue-v1.png",
            "girl-running-v1.png",
            "girl-actions-v1.png",
            "leaf-platform-v1.png",
        };

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(FolderPrefix)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;

            string fileName = Path.GetFileName(assetPath);
            bool needRead = false;
            for (int i = 0; i < ReadableTextures.Length; i++)
            {
                if (string.Equals(fileName, ReadableTextures[i], System.StringComparison.OrdinalIgnoreCase))
                {
                    needRead = true;
                    break;
                }
            }
            importer.isReadable = needRead;
        }

        public static void ReimportAllDreamForestTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/DreamForest/Resources/DreamForest" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}
