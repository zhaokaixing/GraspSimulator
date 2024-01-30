using UnityEngine;
using System;

namespace TriLib
{
    /// <summary>
    /// Represents a texture compression parameter.
    /// </summary>
    public enum TextureCompression
    {
        /// <summary>
        /// No texture compression will be applied.
        /// </summary>
        None,

        /// <summary>
        /// Normal-quality texture compression will be applied.
        /// </summary>
        NormalQuality,

        /// <summary>
        /// High-quality texture compression will be applied.
        /// </summary>
        HighQuality
    }

    /// <summary>
    /// Represents a <see cref="UnityEngine.Texture2D"/> post-loading event handle.
    /// </summary>
    public delegate void TextureLoadHandle(string sourcePath, Material material, string propertyName, Texture2D texture);

    /// <summary>
    /// Represents a  <see cref="UnityEngine.Texture2D"/> pre-loading event handle.
    /// </summary>
    public delegate void TexturePreLoadHandle(IntPtr scene, string path, string name, Material material, string propertyName, ref bool checkAlphaChannel, TextureWrapMode textureWrapMode = TextureWrapMode.Repeat, string basePath = null, TextureLoadHandle onTextureLoaded = null, TextureCompression textureCompression = TextureCompression.None, bool isNormalMap = false);

    /// <summary>
    /// Represents a class to load external textures.
    /// </summary>
    public static class Texture2DUtils
    {
        public static Texture2D ProcessTexture(
           EmbeddedTextureData embeddedTextureData,
           string name,
           ref bool hasAlphaChannel,
           bool isNormalMap = false,
           TextureWrapMode textureWrapMode = TextureWrapMode.Repeat,
           FilterMode textureFilterMode = FilterMode.Bilinear,
           TextureCompression textureCompression = TextureCompression.None,
           bool checkAlphaChannel = false,
           bool generateMipMaps = true
       )
        {
            Texture2D finalTexture2D = null;
            if (embeddedTextureData.DataPointer == IntPtr.Zero || embeddedTextureData.DataLength <= 0)
            {
#if TRILIB_OUTPUT_MESSAGES
                    Debug.LogWarningFormat("Texture '{0}' not found", name);
#endif
            }
            else
            {
                Texture2D tempTexture2D;
                if (ApplyTextureData(embeddedTextureData, out tempTexture2D))
                {
                    finalTexture2D = ProcessTextureData(tempTexture2D, name, ref hasAlphaChannel, textureWrapMode, textureFilterMode, textureCompression, isNormalMap, checkAlphaChannel, generateMipMaps);
                }
#if TRILIB_OUTPUT_MESSAGES
            Debug.LogErrorFormat("Unable to load texture '{0}'", name);
#endif
            }
            embeddedTextureData.Dispose();
            return finalTexture2D;
        }

        private static bool ApplyTextureData(EmbeddedTextureData embeddedTextureData, out Texture2D outputTexture2D)
        {
            if (embeddedTextureData.Data == null && embeddedTextureData.DataPointer == IntPtr.Zero)
            {
                outputTexture2D = null;
                return false;
            }
            try
            {
                outputTexture2D = new Texture2D(embeddedTextureData.Width, embeddedTextureData.Height, TextureFormat.RGBA32, false);
                if (embeddedTextureData.DataPointer != IntPtr.Zero)
                {
                    outputTexture2D.LoadRawTextureData(embeddedTextureData.DataPointer, embeddedTextureData.DataLength);
                }
                else
                {
                    outputTexture2D.LoadRawTextureData(embeddedTextureData.Data);
                }
                outputTexture2D.Apply();
                return true;
            }
#if TRILIB_OUTPUT_MESSAGES
                catch (Exception e)
                {
                    outputTexture2D = null;
                    Debug.LogErrorFormat("Invalid embedded texture data {0}", e);
                    return false;
                }
#else
            catch
            {
                outputTexture2D = null;
                return false;
            }
#endif
        }

        private static Texture2D ProcessTextureData(Texture2D texture2D, string name, ref bool hasAlphaChannel, TextureWrapMode textureWrapMode, FilterMode textureFilterMode, TextureCompression textureCompression, bool isNormalMap, bool checkAlphaChannel = false, bool generateMipMaps = false)
        {
            if (texture2D == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(name))
            {
                name = StringUtils.GenerateUniqueName(texture2D);
            }
            texture2D.name = name;
            texture2D.wrapMode = textureWrapMode;
            texture2D.filterMode = textureFilterMode;
            var colors = texture2D.GetPixels32();
            if (isNormalMap)
            {
                var tempTexture2D = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, generateMipMaps);
                tempTexture2D.name = texture2D.name;
                tempTexture2D.wrapMode = texture2D.wrapMode;
                tempTexture2D.filterMode = texture2D.filterMode;
                for (var i = 0; i < colors.Length; i++)
                {
                    var color = colors[i];
                    var r = color.r;
                    color.r = color.a;
                    color.a = r;
                    colors[i] = color;
                }
                tempTexture2D.SetPixels32(colors);
                tempTexture2D.Apply(generateMipMaps);
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(texture2D);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(texture2D);
                }
                texture2D = tempTexture2D;
            }
            if (!isNormalMap && generateMipMaps)
            {
                var tempTexture2D = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, true);
                tempTexture2D.name = texture2D.name;
                tempTexture2D.wrapMode = texture2D.wrapMode;
                tempTexture2D.filterMode = texture2D.filterMode;
                tempTexture2D.SetPixels32(colors);
                tempTexture2D.Apply(true);
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(texture2D);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(texture2D);
                }
                texture2D = tempTexture2D;
            }
            if (textureCompression != TextureCompression.None)
            {
                var isPowerOfTwo = IsPowerOf2(texture2D.width) && IsPowerOf2(texture2D.height);
                if (isPowerOfTwo)
                {
                    texture2D.Compress(textureCompression == TextureCompression.HighQuality);
                }
            }
            if (checkAlphaChannel)
            {
                hasAlphaChannel = false;
                foreach (var color in colors)
                {
                    if (color.a == 255) continue;
                    hasAlphaChannel = true;
                    break;
                }
            }
            return texture2D;
        }

        private static bool IsPowerOf2(int x)
        {
            return (x & (x - 1)) == 0;
        }

        public static Texture BuildHDRPMaskTexture(Texture2D metallicTexture, Texture2D occlusionTexture, Texture2D detailMaskTexture, Texture2D smoothnessTexture)
        {
            var width = 0;
            var height = 0;
            if (metallicTexture != null)
            {
                width = Mathf.Max(metallicTexture.width, width);
                height = Mathf.Max(metallicTexture.height, height);
            }
            if (occlusionTexture != null)
            {
                width = Mathf.Max(occlusionTexture.width, width);
                height = Mathf.Max(occlusionTexture.height, height);
            }
            if (detailMaskTexture != null)
            {
                width = Mathf.Max(detailMaskTexture.width, width);
                height = Mathf.Max(detailMaskTexture.height, height);
            }
            if (smoothnessTexture != null)
            {
                width = Mathf.Max(smoothnessTexture.width, width);
                height = Mathf.Max(smoothnessTexture.height, height);
            }
            if (width == 0 || height == 0)
            {
                return null;
            }
            var maskTexture = new Texture2D(width, height);
            var maskTexturePixels = maskTexture.GetPixels();
            for (var i = 0; i < maskTexturePixels.Length; i++)
            {
                var v = (i / width) / (float)height;
                var u = (i % width) / (float)width;
                var metallic = metallicTexture != null ? metallicTexture.GetPixelBilinear(u, v).r : 0f;
                var occlusion = occlusionTexture != null ? occlusionTexture.GetPixelBilinear(u, v).r : 0f;
                var detailMask = detailMaskTexture != null ? detailMaskTexture.GetPixelBilinear(u, v).r : 0f;
                var smoothness = smoothnessTexture != null ? smoothnessTexture.GetPixelBilinear(u, v).r : 0f;
                maskTexturePixels[i] = new Color(metallic, occlusion, detailMask, smoothness);
            }
            maskTexture.SetPixels(maskTexturePixels);
            maskTexture.Apply();
            return maskTexture;
        }
    }
}

