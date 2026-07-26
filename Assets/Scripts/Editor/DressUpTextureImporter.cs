#if UNITY_EDITOR
using UnityEditor;

public class DressUpTextureImporter : AssetPostprocessor {
    private static readonly string[] TargetFolders = {
        "Assets/Resources/CharacterItemIcon",
        "Assets/Resources/CharacterItem",
    };

    void OnPreprocessTexture() {
        bool inTarget = false;
        foreach (var folder in TargetFolders) {
            if (assetPath.StartsWith(folder)) { inTarget = true; break; }
        }
        if (!inTarget) return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.isReadable = true;
    }
}
#endif
