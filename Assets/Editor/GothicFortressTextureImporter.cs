using UnityEditor;
using UnityEngine;

namespace VexUnbound.Editor
{
    public sealed class GothicFortressTextureImporter : AssetPostprocessor
    {
        private const string TextureRoot = "Assets/Resources/Environment/GothicFortress/Textures/";

        public override uint GetVersion()
        {
            return 2;
        }

        [MenuItem("Vex Unbound/Reimport Gothic Fortress Textures")]
        public static void ReimportTextures()
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { TextureRoot.TrimEnd('/') });
            foreach (string guid in textureGuids)
            {
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
            }

            Debug.Log($"Reimported {textureGuids.Length} gothic fortress textures.");
        }

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(TextureRoot))
            {
                return;
            }

            TextureImporter importer = (TextureImporter)assetImporter;
            bool isNormalMap = assetPath.Contains("_nor_gl_");
            bool isLinearData = isNormalMap || assetPath.Contains("_ao_");
            importer.textureType = isNormalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !isLinearData;
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = 1024;
            android.format = isNormalMap ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC2_RGB4;
            android.textureCompression = TextureImporterCompression.Compressed;
            android.compressionQuality = 50;
            importer.SetPlatformTextureSettings(android);
        }
    }
}
