using UnityEditor;
using UnityEngine;

public class NeonBurrowAssetPostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Art/", System.StringComparison.Ordinal))
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 100f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spriteImportMode = assetPath.Contains("Background") ? SpriteImportMode.Single : SpriteImportMode.Multiple;
        importer.maxTextureSize = assetPath.Contains("Background") ? 2048 : 4096;
    }
}
