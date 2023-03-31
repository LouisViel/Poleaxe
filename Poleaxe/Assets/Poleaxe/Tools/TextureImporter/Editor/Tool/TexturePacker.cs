using System;
using UnityEditor;
using UnityEngine;
using Poleaxe.Editor.Helper;
using Poleaxe.Editor.Utils.ResourceCreator;

using UnityTextureImporter = UnityEditor.TextureImporter;

namespace Poleaxe.TextureImporter.Editor
{
    public static class TexturePacker
    {
        ////////////////////////////////////////////
        ///////////////  STRUCTURES  ///////////////
        ////////////////////////////////////////////

        #region Structures

        private enum GrayScaleVerification
        {
            R,
            G,
            B,
            RGB,
            Fail,
        }

        #endregion Structures

        ////////////////////////////////////////////
        ///////////////  CONSTANTS  ////////////////
        ////////////////////////////////////////////

        #region Constants

        const string toolName = nameof(TexturePacker);
        static readonly ResourceCreatorData resourceData = new ResourceCreatorData {
            containerDirectoryName = "Temp", isTemp = true, isEditor = true
        };

        #endregion Constants

        ////////////////////////////////////////////
        ////////////  GET RESOURCE PATH ////////////
        ////////////////////////////////////////////

        #region GetResourcePath

        public static string GetResourcePath()
        {
            return ResourceCreator<ScriptableObject>.EnsureGetPath(resourceData);
        }

        #endregion GetResourcePath

        ////////////////////////////////////////////
        //////////////  CREATE ASSET ///////////////
        ////////////////////////////////////////////

        #region CreateAsset

        public static string CreateAsset(Texture2D texture, string path) => CreateAsset(texture, path, true);
        public static string CreateAsset(Texture2D texture, string path, bool uniquePath)
        {
            byte[] textureData = texture.EncodeToPNG();
            if (textureData == null) throw new ArgumentException($"[{toolName}] Texture {texture.name} is invalid");
            string assetpath = PathHelper.WriteAllBytes(path, $"{texture.name}.png", textureData, uniquePath);
            AssetDatabase.ImportAsset(assetpath);
            return assetpath;
        }

        #endregion CreateAsset

        ////////////////////////////////////////////
        /////////  ENSURE IMPORT SETTINGS  /////////
        ////////////////////////////////////////////

        #region EnsureImportSettings

        public static Texture2D EnsureImportSettings(Texture2D texture) => EnsureImportSettings(texture, GetResourcePath());
        public static Texture2D EnsureImportSettings(Texture2D texture, string tempDirPath)
        {
            string assetpath = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(assetpath)) assetpath = CreateAsset(texture, tempDirPath);
            UnityTextureImporter importer = (UnityTextureImporter)AssetImporter.GetAtPath(assetpath);
            importer.sRGBTexture = false;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetpath);
        }

        #endregion EnsureImportSettings

        ////////////////////////////////////////////
        /////////////  INVERSE VALUE  //////////////
        ////////////////////////////////////////////

        #region InverseValue

        public static Texture2D InverseValue(Texture2D texture)
        {
            Vector2Int texelSize = GetTexelSize(texture);
            Texture2D finalTexture = new Texture2D(texelSize.x, texelSize.y, TextureFormat.RGBAFloat, true);
            Color32[] pixels = texture.GetPixels32();
            for (int x = 0; x < texelSize.x; x++) {
                for (int y = 0; y < texelSize.y; y++) {
                    Color32 pixel = pixels[x + y * texelSize.x];
                    float R = 1f - pixel.r, G = 1f - pixel.g, B = 1f - pixel.b, A = 1f - pixel.a;
                    finalTexture.SetPixel(x, y, new Color(R, G, B, A));
                }
            }
            finalTexture.Apply();
            return finalTexture;
        }

        #endregion InverseValue

        ////////////////////////////////////////////
        /////////////  PACK TEXTURES  //////////////
        ////////////////////////////////////////////

        #region PackTextures

        public static Texture2D PackTextures(
            Texture2D R_Texture = null, Texture2D G_Texture = null, Texture2D B_Texture = null, Texture2D A_Texture = null,
            float R_Default = 0f, float G_Default = 0f, float B_Default = 0f, float A_Default = 0f
        ) {

            Vector2Int texelSize = GetTexelSize(R_Texture, G_Texture, B_Texture, A_Texture);
            Texture2D finalTexture = new Texture2D(texelSize.x, texelSize.y, TextureFormat.RGBAFloat, true);

            Texture2D redTexture = GetNiceTexture(R_Texture, texelSize);
            Texture2D greenTexture = GetNiceTexture(G_Texture, texelSize);
            Texture2D blueTexture = GetNiceTexture(B_Texture, texelSize);
            Texture2D alphaTexture = GetNiceTexture(A_Texture, texelSize);

            Color32[] rPixels = redTexture == null ? new Color32[0] : redTexture.GetPixels32();
            Color32[] gPixels = greenTexture == null ? new Color32[0] : greenTexture.GetPixels32();
            Color32[] bPixels = blueTexture == null ? new Color32[0] : blueTexture.GetPixels32();
            Color32[] aPixels = alphaTexture == null ? new Color32[0] : alphaTexture.GetPixels32();

            float redDefault = Mathf.Clamp(R_Default, 0f, 1f);
            float greenDefault = Mathf.Clamp(G_Default, 0f, 1f);
            float blueDefault = Mathf.Clamp(B_Default, 0f, 1f);
            float alphaDefault = Mathf.Clamp(A_Default, 0f, 1f);

            bool canR = rPixels.Length > 0, canG = gPixels.Length > 0,
                canB = bPixels.Length > 0, canA = aPixels.Length > 0;

            for (int x = 0; x < texelSize.x; x++) {
                for (int y = 0; y < texelSize.y; y++) {
                    int index = x + y * texelSize.x;
                    float R = canR ? rPixels[index].r : redDefault;
                    float G = canG ? gPixels[index].r : greenDefault;
                    float B = canB ? bPixels[index].r : blueDefault;
                    float A = canA ? aPixels[index].r : alphaDefault;
                    finalTexture.SetPixel(x, y, new Color(R, G, B, A));
                }
            }

            finalTexture.Apply();
            return finalTexture;
        }

        #endregion PackTextures

        ////////////////////////////////////////////
        ////////////  UNPACK TEXTURES  /////////////
        ////////////////////////////////////////////

        #region UnpackTexture

        public static Texture2D UnpackTexture(Texture2D texture, TextureRemappingChannel channel)
        {
            Vector2Int texelSize = GetTexelSize(texture);
            Texture2D finalTexture = new Texture2D(texelSize.x, texelSize.y, TextureFormat.RGBAFloat, true);
            if (channel != TextureRemappingChannel.None) {
                int index = (int)channel - 1;
                Color32[] pixels = texture.GetPixels32();
                for (int x = 0; x < texture.width; x++) {
                    for (int y = 0; y < texture.height; y++) {
                        float g = pixels[x + y * texture.width][index];
                        Color c = new Color(g, g, g, 1);
                        finalTexture.SetPixel(x, y, c);
                    }
                }
            }
            finalTexture.Apply();
            return finalTexture;
        }

        #endregion UnpackTexture

        ////////////////////////////////////////////
        ///////  UNPACK TEXTURES TO CHANNEL  ///////
        ////////////////////////////////////////////

        #region UnpackTextureToChannel

        public static Texture2D UnpackTextureToChannel(Texture2D texture, TextureRemappingChannel fromChannel, TextureRemappingChannel toChannel)
        {
            Vector2Int texelSize = GetTexelSize(texture);
            Texture2D finalTexture = new Texture2D(texelSize.x, texelSize.y, TextureFormat.RGBAFloat, true);
            if (fromChannel != TextureRemappingChannel.None && toChannel != TextureRemappingChannel.None) {
                int index = (int)fromChannel - 1, target = (int)toChannel - 1;
                Color32[] pixels = texture.GetPixels32();
                for (int x = 0; x < texture.width; x++) {
                    for (int y = 0; y < texture.height; y++) {
                        Color c = new Color(0f, 0f, 0f, 1);
                        c[target] = pixels[x + y * texture.width][index];
                        finalTexture.SetPixel(x, y, c);
                    }
                }
            }
            finalTexture.Apply();
            return finalTexture;
        }

        #endregion UnpackTextureToChannel

        ////////////////////////////////////////////
        ////////////  GET TEXEL SIZE  //////////////
        ////////////////////////////////////////////

        #region GetTexelSize

        public static Vector2Int GetTexelSize(params Texture2D[] textures)
        {
            int texelCount = 0;
            Vector2 texelSize = Vector2.zero;

            foreach (Texture2D texture in textures) {
                if (texture == null) continue;
                texelSize += texture.texelSize;
                ++texelCount;
            }

            if (texelCount <= 0) return Vector2Int.zero;
            texelSize /= texelCount;

            int x = Mathf.CeilToInt(texelSize.x);
            int y = Mathf.CeilToInt(texelSize.y);
            return new Vector2Int(x, y);
        }

        #endregion GetTexelSize

        ////////////////////////////////////////////
        /////  DUPLICATE AND RESIZE TEXTURE  ///////
        ////////////////////////////////////////////

        #region DuplicateAndResizeTexture

        public static Texture2D DuplicateAndResizeTexture(Texture2D texture, Vector2Int texelSize)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(texelSize.x, texelSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Graphics.Blit(texture, renderTex);

            Texture2D readableAndResizedTexture = new Texture2D(texelSize.x, texelSize.y);
            readableAndResizedTexture.ReadPixels(new Rect(0f, 0f, renderTex.width, renderTex.height), 0, 0);
            readableAndResizedTexture.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableAndResizedTexture;
        }

        #endregion DuplicateAndResizeTexture

        ////////////////////////////////////////////
        ///////  GRAYSCALE LINEAR TEXTURE  /////////
        ////////////////////////////////////////////

        #region GrayscaleLinearTexture

        public static Texture2D GrayscaleLinearTexture(Texture2D texture, Vector2Int texelSize)
        {
            Texture2D result = null;
            if (texture != null) {
                GrayScaleVerification channel = IsGrayscale(texture);
                if (channel == GrayScaleVerification.RGB) return DuplicateAndResizeTexture(texture, texelSize);
                result = new Texture2D(texelSize.x, texelSize.y);
                Color32[] pixels = texture.GetPixels32();
                if (channel == GrayScaleVerification.Fail) {
                    for (int x = 0; x < texture.width; x++) {
                        for (int y = 0; y < texture.height; y++) {
                            Color32 pixel = pixels[x + y * texture.width];
                            float g = (pixel.r + pixel.g + pixel.b) / 3f;
                            Color c = new Color(g, g, g, 1);
                            result.SetPixel(x, y, c);
                        }
                    }
                } else {
                    int index = (int)channel;
                    for (int x = 0; x < texture.width; x++) {
                        for (int y = 0; y < texture.height; y++) {
                            float g = pixels[x + y * texture.width][index];
                            Color c = new Color(g, g, g, 1);
                            result.SetPixel(x, y, c);
                        }
                    }
                }
                result.Apply();
            }
            return result;
        }

        private static GrayScaleVerification IsGrayscale(Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();
            if (pixels.Length <= 0) return GrayScaleVerification.RGB;
            GrayScaleVerification channel = GetGrayscaleVerification(pixels[0]), previous = channel;
            if (channel == GrayScaleVerification.Fail) return GrayScaleVerification.Fail;
            for (int x = 0; x < texture.width; x++) {
                for (int y = 0; y < texture.height; y++) {
                    Color32 pixel = pixels[x + y * texture.width];
                    channel = GetGrayscaleVerification(pixel);
                    if (channel == GrayScaleVerification.Fail) return GrayScaleVerification.Fail;
                    if (channel != previous) {
                        if (previous == GrayScaleVerification.RGB) previous = channel;
                        else return GrayScaleVerification.Fail;
                    }
                }
            }
            return GrayScaleVerification.Fail;
        }

        private static GrayScaleVerification GetGrayscaleVerification(Color32 pixel)
        {
            if (pixel.r == pixel.g && pixel.r == pixel.b) return GrayScaleVerification.RGB;
            if (pixel.r == 0f && pixel.g == 0f) return GrayScaleVerification.B;
            if (pixel.r == 0f && pixel.b == 0f) return GrayScaleVerification.G;
            if (pixel.g == 0f && pixel.b == 0f) return GrayScaleVerification.R;
            return GrayScaleVerification.Fail;
        }

        #endregion GrayscaleLinearTexture

        ////////////////////////////////////////////
        //////////  GET NICE TEXTURES  /////////////
        ////////////////////////////////////////////

        #region GetNiceTexture

        private static Texture2D GetNiceTexture(Texture2D texture, Vector2Int texelSize)
        {
            Texture2D result = null;
            if (texture != null) {
                result = DuplicateAndResizeTexture(texture, texelSize);
                result = GrayscaleLinearTexture(result, texelSize);
            }
            return result;
        }

        #endregion GetNiceTexture
    }
}