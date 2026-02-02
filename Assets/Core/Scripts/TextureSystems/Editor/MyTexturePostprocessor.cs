using UnityEditor;

// This script automatically sets compression settings for textures imported into a specific folder.
public class MyTexturePostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        //Check if the texture is in a folder for mobile assets
        //if (assetPath.Contains("Image"))
        //    {
        //        TextureImporter textureImporter = (TextureImporter)assetImporter;
        //        textureImporter.textureType = TextureImporterType.Sprite;

        //        TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
        //        platformSettings.name = "Android";
        //        platformSettings.overridden = true;
        //        platformSettings.maxTextureSize = 64;
        //        platformSettings.format = TextureImporterFormat.ETC2_RGBA8Crunched;

        //        textureImporter.SetPlatformTextureSettings(platformSettings);
        //    }
    }
}
