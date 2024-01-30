#pragma warning disable 612, 618, 67
using System;
using System.Collections.Generic;
using System.IO;
using STB;
using UnityEngine;
using UnityEngine.Rendering;
using AOT;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
#if TRILIB_USE_ZIP
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
using System.IO.Compression;
#else
using ICSharpCode.SharpZipLib.Zip;
#endif
#endif
namespace TriLib
{
    /// <summary>
    /// Represents an <see cref="AssetLoader"/> asset loaded event handler.
    /// </summary>
    public delegate void ObjectLoadedHandle(GameObject loadedGameObject);

    /// <summary>
    /// Represents an <see cref="AssetLoader"/> mesh creation event handler.
    /// </summary>
    public delegate void MeshCreatedHandle(uint meshIndex, Mesh mesh);

    /// <summary>
    /// Represents an <see cref="AssetLoader"/> material created event handler.
    /// </summary>
    public delegate void MaterialCreatedHandle(uint materialIndex, bool isOverriden, Material material);

    /// <summary>
    /// Represents an <see cref="AssetLoader"/> avatar created event handler.
    /// </summary>
    public delegate void AvatarCreatedHandle(Avatar avatar, Animator animator);

    /// <summary>
    /// Represents an <see cref="AssetLoader"/> animation created event handler.
    /// </summary>
    public delegate void AnimationClipCreatedHandle(uint animationClipIndex, AnimationClip animationClip);

#if UNITY_EDITOR
    /// <summary>
    /// Represents a callback used to create <see cref="UnityEditor.Animations.AnimatorController"/> on editor.
    /// </summary>
    /// <returns></returns>
    public delegate AnimatorController AnimatorControllerCreationHandle();

    /// <summary>
    /// Represents an <see cref="AssetLoader"/> animator controller created event handler.
    /// </summary>
    public delegate void AnimatorControllerCreatedHandle(AnimatorController animatorController, Animator animator);
#endif


    /// <summary>
    /// Represents an <see cref="AssetLoader"/> metadata processed event handler.
    /// </summary>
    /// <param name="metadataType">The <see cref="AssimpMetadataType"/> of the metadata.</param>
    /// <param name="metadataIndex">The index of the metadata</param>
    /// <param name="metadataKey">The key of the metadata</param>
    /// <param name="metadataValue">The value of the metadata</param>
    public delegate void MetadataProcessedHandle(AssimpMetadataType metadataType, uint metadataIndex, string metadataKey, object metadataValue);

    /// <summary>
    /// Represents a blend-shape key created event handler.
    /// </summary>
    /// <param name="mesh"><see cref="UnityEngine.Mesh"/> where the blend-shape key has been created.</param>
    /// <param name="name">Blend-shape name</param>
    /// <param name="vertices">Blend-shape key vertices.</param>
    /// <param name="normals">Blend-shape key normals.</param>
    /// <param name="tangents">Blend-shape key tangents.</param>
    /// <param name="biTangents">Blend-shape key bi-tangents.</param>
    public delegate void BlendShapeKeyCreatedHandle(Mesh mesh, string name, Vector3[] vertices, Vector3[] normals, Vector4[] tangents, Vector4[] biTangents);

    /// <summary>
    /// Callback used to retrieve custom embedded texture data.
    /// </summary>
    /// <param name="path">Texture path.</param>
    /// <param name="basePath">Texture base path.</param>
    /// <returns>A new <see cref="EmbeddedTextureData"/></returns>
    public delegate EmbeddedTextureData LoadTextureDataCallback(string path, string basePath);

    /// <summary>
    /// Event used to pass a custom texture data when processing a texture entry.
    /// </summary>
    public delegate EmbeddedTextureData EmbeddedTextureLoadCallback(string path);

    /// <summary>
    /// Base asset loader.
    /// </summary>
    public class AssetLoaderBase : IDisposable
    {
        /// <summary>
        /// Assimp uses this prefix when loading files from memory.
        /// </summary>
        protected const string AssimpFilenameMagicString = "$$$___magic___$$$";

        /// <summary>
        /// Main scene <see cref="TriLib.NodeData"/>.
        /// </summary>
        public NodeData RootNodeData;

        /// <summary>
        /// Processed <see cref="TriLib.MaterialData"/> list.
        /// </summary>
        public MaterialData[] MaterialData;

        /// <summary>
        /// Processed <see cref="TriLib.MeshData"/> list.
        /// </summary>
        public MeshData[] MeshData;

        /// <summary>
        /// Processed <see cref="TriLib.AnimationData"/> list.
        /// </summary>
        public AnimationData[] AnimationData;

        /// <summary>
        /// Processed <see cref="TriLib.CameraData"/> list.
        /// </summary>
        public CameraData[] CameraData;

        /// <summary>
        /// Processed <see cref="TriLib.AssimpMetadata"/> list.
        /// </summary>
        public AssimpMetadata[] Metadata;

        /// <summary>
        /// Processed nodes path dictionary.
        /// </summary>
        public Dictionary<string, string> NodesPath;

        /// <summary>
        /// Loaded <see cref="UnityEngine.Material"/> for a given name dictionary.
        /// </summary>
        public Dictionary<string, Material> LoadedMaterials;

        /// <summary>
        /// Loaded <see cref="UnityEngine.Texture2D"/> for a given name dictionary.
        /// </summary>
        public Dictionary<string, Texture2D> LoadedTextures;

        /// <summary>
        /// Loaded bone names for a given <see cref="UnityEngine.SkinnedMeshRenderer"/> dictionary.
        /// </summary>
        public Dictionary<SkinnedMeshRenderer, IList<string>> LoadedBoneNames;

        /// <summary>
        /// <see cref="UnityEngine.GameObject"/> and <see cref="MeshData"/> relationship dictionary used to apply blend-shape animations.
        /// </summary>
        public Dictionary<string, MeshData> MeshDataConnections;

        /// <summary>
        /// Loaded <see cref="EmbeddedTextureData"/> for a given name dictionary.
        /// </summary>
        public Dictionary<string, EmbeddedTextureData> EmbeddedTextures;

        /// <summary>
        /// Stores a <see cref="FileLoadData"></see> reference list for callbacks.
        /// </summary>
        public static ConcurrentList<FileLoadData> FilesLoadData = new ConcurrentList<FileLoadData>();

        /// <summary>
        /// Base Diffuse <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardBaseMaterial;

        /// <summary>
        /// Base Specular <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardSpecularMaterial;

        /// <summary>
        /// Base Diffuse Alpha <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardBaseAlphaMaterial;

        /// <summary>
        /// Base Specular Alpha <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardSpecularAlphaMaterial;

        /// <summary>
        /// Base Diffuse Cutout <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardBaseCutoutMaterial;

        /// <summary>
        /// Base Diffuse Fade <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardBaseFadeMaterial;

        /// <summary>
        /// Base Specular Cutout <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardSpecularCutoutMaterial;

        /// <summary>
        /// Base Specular Fade <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardSpecularFadeMaterial;

        /// <summary>
        /// Base Roughness <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardRoughnessMaterial;

        /// <summary>
        /// Base Roughness Cutout <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardRoughnessCutoutMaterial;

        /// <summary>
        /// Base Roughness Fade <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardRoughnessFadeMaterial;

        /// <summary>
        /// Base Roughness Alpha <see cref="UnityEngine.Material"/> used to load materials.
        /// </summary>
        public static Material StandardRoughnessAlphaMaterial;

        /// <summary>
        /// <see cref="UnityEngine.Texture"/> used to show when no texture is found.
        /// </summary>
        public static Texture2D NotFoundTexture;

        /// <summary>
        /// Used to temporarily store nodes id.
        /// </summary>
        public uint NodeId;

        /// <summary>
        /// Used to temporarily indicate if there are any bones assigned to loaded meshes.
        /// </summary>
        public bool HasBoneInfo;

        /// <summary>
        /// Used to temporarily indicate if there are any blend shapes assigned to loaded meshes.
        /// </summary>
        public bool HasBlendShapes;

        /// <summary>
        /// Pointer to Assimp loaded scene.
        /// </summary>
        protected IntPtr Scene;

        /// <summary>
        /// Use this field to assign the callback that will be triggered when a texture looks up for embedded data.
        /// </summary>
        [Obsolete("Please use the loadTextureDataCallback callback from AssetLoader classes loading methods instead.")]
        public event EmbeddedTextureLoadCallback EmbeddedTextureLoad;

        /// <summary>
        /// Use this field to assign the event that occurs when a mesh is loaded.
        /// </summary>
        public event MeshCreatedHandle OnMeshCreated;

        /// <summary>
        /// Gets a value indicating whether this instance has on mesh created event.
        /// </summary>
        /// <value><c>true</c> if this instance has on mesh created event; otherwise, <c>false</c>.</value>
        protected bool HasOnMeshCreated
        {
            get
            {
                return OnMeshCreated != null;
            }
        }

        /// <summary>
        /// Triggers the on mesh created event.
        /// </summary>
        /// <param name="meshIndex">Mesh index.</param>
        /// <param name="mesh">Mesh.</param>
        protected void TriggerOnMeshCreated(uint meshIndex, Mesh mesh)
        {
            if (OnMeshCreated != null)
            {
                OnMeshCreated(meshIndex, mesh);
            }
        }

        /// <summary>
        /// Use this field to assign the event that occurs when a material is created.
        /// </summary>
        public event MaterialCreatedHandle OnMaterialCreated;

        /// <summary>
        /// Gets a value indicating whether this instance has on material created event.
        /// </summary>
        /// <value><c>true</c> if this instance has on material created event; otherwise, <c>false</c>.</value>
        protected bool HasOnMaterialCreated
        {
            get
            {
                return OnMaterialCreated != null;
            }
        }

        /// <summary>
        /// Triggers the on material created event.
        /// </summary>
        /// <param name="materialIndex">Material index.</param>
        /// <param name="isOverriden">If set to <c>true</c> is overriden.</param>
        /// <param name="material">Material.</param>
        protected void TriggerOnMaterialCreated(uint materialIndex, bool isOverriden, Material material)
        {
            if (OnMaterialCreated != null)
            {
                OnMaterialCreated(materialIndex, isOverriden, material);
            }
        }

        /// <summary>
        /// Use this field to assign the event that occurs when a texture is loaded.
        /// </summary>
        public event TextureLoadHandle OnTextureLoaded;

        /// <summary>
        /// Gets a value indicating whether this instance has on texture loaded event.
        /// </summary>
        /// <value><c>true</c> if this instance has on texture loaded event; otherwise, <c>false</c>.</value>
        protected bool HasOnTextureLoaded
        {
            get
            {
                return OnTextureLoaded != null;
            }
        }

        /// <summary>
        /// Triggers the on texture loaded event.
        /// </summary>
        /// <param name="sourcePath">Source path.</param>
        /// <param name="material">Material.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="texture">Texture.</param>
        protected void TriggerOnTextureLoaded(string sourcePath, Material material, string propertyName, Texture2D texture)
        {
            if (OnTextureLoaded != null)
            {
                OnTextureLoaded(sourcePath, material, propertyName, texture);
            }
        }

#if UNITY_EDITOR
        public event AnimatorControllerCreatedHandle OnAnimatorControllerCreated;

        public bool HasOnAnimatorControllerCreated
        {
            get
            {
                return OnAnimatorControllerCreated != null;
            }
        }

        public void TriggerOnAnimatorControllerCreated(AnimatorController animatorController, Animator animator)
        {
            if (OnAnimatorControllerCreated != null)
            {
                OnAnimatorControllerCreated(animatorController, animator);
            }
        }

        public AnimatorControllerCreationHandle AnimatorControllerCreation;

        public bool HasAnimatorControllerCreation
        {
            get
            {
                return AnimatorControllerCreation != null;
            }
        }

        public AnimatorController CallAnimatorControllerCreation()
        {
            if (AnimatorControllerCreation != null)
            {
                return AnimatorControllerCreation();
            }
            return null;
        }
#endif

        public event AvatarCreatedHandle OnAvatarCreated;

        public bool HasOnAvatarCreated
        {
            get
            {
                return OnAvatarCreated != null;
            }
        }

        public void TriggerOnAvatarCreated(Avatar avatar, Animator animator)
        {
            if (OnAvatarCreated != null)
            {
                OnAvatarCreated(avatar, animator);
            }
        }

        /// <summary>
        /// Use this field to assign the event that occurs when an animation is created.
        /// </summary>
        public event AnimationClipCreatedHandle OnAnimationClipCreated;

        /// <summary>
        /// Gets a value indicating whether this instance has on animation clip created event.
        /// </summary>
        /// <value><c>true</c> if this instance has on animation clip created event; otherwise, <c>false</c>.</value>
        protected bool HasOnAnimationClipCreated
        {
            get
            {
                return OnAnimationClipCreated != null;
            }
        }

        /// <summary>
        /// Triggers the on animation clip created event.
        /// </summary>
        /// <param name="animationClipIndex">Animation clip index.</param>
        /// <param name="animationClip">Animation clip.</param>
        protected void TriggerOnAnimationClipCreated(uint animationClipIndex, AnimationClip animationClip)
        {
            if (OnAnimationClipCreated != null)
            {
                OnAnimationClipCreated(animationClipIndex, animationClip);
            }
        }

        /// <summary>
        /// Use this field to assign the event that occurs when the asset is loaded.
        /// </summary>
        public event ObjectLoadedHandle OnObjectLoaded;

        /// <summary>
        /// Gets a value indicating whether this instance has on object loaded event.
        /// </summary>
        /// <value><c>true</c> if this instance has on object loaded event; otherwise, <c>false</c>.</value>
        protected bool HasOnObjectLoaded
        {
            get
            {
                return OnObjectLoaded != null;
            }
        }

        /// <summary>
        /// Triggers the on object loaded event.
        /// </summary>
        /// <param name="loadedGameObject">Created <see cref="UnityEngine.GameObject"/>.</param>
        protected void TriggerOnObjectLoaded(GameObject loadedGameObject)
        {
            if (OnObjectLoaded != null)
            {
                OnObjectLoaded(loadedGameObject);
            }
        }

        /// <summary>
        /// Use this field to assign the event that will occurs when each file metadata is processed.
        /// </summary>
        public event MetadataProcessedHandle OnMetadataProcessed;

        /// <summary>
        /// Gets a value indicating whether this instance has on metadata processed event.
        /// </summary>
        /// <value><c>true</c> if this instance has on metadata processed event; otherwise, <c>false</c>.</value>
        protected bool HasOnMetadataProcessed
        {
            get
            {
                return OnMetadataProcessed != null;
            }
        }

        /// <summary>
        /// Triggers the on metadata processed event.
        /// </summary>
        /// <param name="metadataType">Metadata type.</param>
        /// <param name="metadataIndex">Metadata index.</param>
        /// <param name="metadataKey">Metadata key.</param>
        /// <param name="metadataValue">Metadata value.</param>
        protected void TriggerOnMetadataProcessed(AssimpMetadataType metadataType, uint metadataIndex, string metadataKey, object metadataValue)
        {
            if (OnMetadataProcessed != null)
            {
                OnMetadataProcessed(metadataType, metadataIndex, metadataKey, metadataValue);
            }
        }

        /// <summary>
        /// Use this field to assign the event that occurs when a blend-shape key is created.
        /// </summary>
        public event BlendShapeKeyCreatedHandle OnBlendShapeKeyCreated;

        /// <summary>
        /// Gets a value indicating whether this instance has on blend-shape key created event.
        /// </summary>
        /// <value><c>true</c> if this instance has on blend-shape loaded event; otherwise, <c>false</c>.</value>
        protected bool HasOnBlendShapeKeyCreated
        {
            get
            {
                return OnBlendShapeKeyCreated != null;
            }
        }

        /// <summary>
        /// Triggers the on blend-shape key created event.
        /// </summary>
        /// <param name="mesh"><see cref="UnityEngine.Mesh"/> where the blend-shape key has been created.</param>
        /// <param name="name">Blend-shape name</param>
        /// <param name="vertices">Blend-shape key vertices.</param>
        /// <param name="normals">Blend-shape key normals.</param>
        /// <param name="tangents">Blend-shape key tangents.</param>
        /// <param name="biTangents">Blend-shape key bi-tangents.</param>
        protected void TriggerOnBlendShapeKeyCreated(Mesh mesh, string name, Vector3[] vertices, Vector3[] normals, Vector4[] tangents, Vector4[] biTangents)
        {
            if (OnBlendShapeKeyCreated != null)
            {
                OnBlendShapeKeyCreated(mesh, name, vertices, normals, tangents, biTangents);
            }
        }

        /// <summary>
        /// Checks whether the given file extension is supported.
        /// </summary>
        /// <returns><c>true</c>, if the extension is supported. Otherwise, <c>false</c>.</returns>
        public static bool IsExtensionSupported(string extension)
        {
            return AssimpInterop.ai_IsExtensionSupported(extension);
        }

        /// <summary>
        /// Returns a string of supported file extensions.
        /// </summary>
        /// <returns>Supported file extensions.</returns>
        public static string GetSupportedFileExtensions()
        {
            string supportedFileExtensions;
            AssimpInterop.ai_GetExtensionList(out supportedFileExtensions);
            return supportedFileExtensions;
        }

        /// <summary>
        /// Ensure all materials are loaded when calling AssetLoader statically.
        /// </summary>
        static AssetLoaderBase()
        {
            LoadAllStandardMaterials();
        }

        /// <summary>
        /// Tries to load all TriLib base resources.
        /// @warning To ensure TriLib works properly, don't forget to import TriLib 'Resources' folder to the project.
        /// </summary>
        private static void LoadAllStandardMaterials()
        {
            if (!LoadNotFoundTexture())
            {
                throw new Exception("Please import 'NotFound' asset from TriLib package 'TriLib\\Resources' to the project.");
            }
            if (!LoadStandardMaterials())
            {
                throw new Exception("Please import all material assets from TriLib package 'TriLib\\Resources' to the project.");
            }
        }

        /// <summary>
        /// Tries to load all TriLib standard base materials.
        /// </summary>
        /// <returns><c>true</c> if all materials have been found. Otherwise, <c>false</c></returns>
        private static bool LoadStandardMaterials()
        {
            if (StandardBaseMaterial == null)
            {
                StandardBaseMaterial = Resources.Load("StandardMaterial") as Material;
            }
            if (StandardBaseAlphaMaterial == null)
            {
                StandardBaseAlphaMaterial = Resources.Load("StandardBaseAlphaMaterial") as Material;
            }
            if (StandardBaseCutoutMaterial == null)
            {
                StandardBaseCutoutMaterial = Resources.Load("StandardBaseCutoutMaterial") as Material;
            }
            if (StandardBaseFadeMaterial == null)
            {
                StandardBaseFadeMaterial = Resources.Load("StandardBaseFadeMaterial") as Material;
            }
            if (StandardSpecularMaterial == null)
            {
                StandardSpecularMaterial = Resources.Load("StandardSpecularMaterial") as Material;
            }
            if (StandardSpecularAlphaMaterial == null)
            {
                StandardSpecularAlphaMaterial = Resources.Load("StandardSpecularAlphaMaterial") as Material;
            }
            if (StandardSpecularCutoutMaterial == null)
            {
                StandardSpecularCutoutMaterial = Resources.Load("StandardSpecularCutoutMaterial") as Material;
            }
            if (StandardSpecularFadeMaterial == null)
            {
                StandardSpecularFadeMaterial = Resources.Load("StandardSpecularFadeMaterial") as Material;
            }
            if (StandardRoughnessMaterial == null)
            {
                StandardRoughnessMaterial = Resources.Load("StandardRoughnessMaterial") as Material;
            }
            if (StandardRoughnessAlphaMaterial == null)
            {
                StandardRoughnessAlphaMaterial = Resources.Load("StandardRoughnessAlphaMaterial") as Material;
            }
            if (StandardRoughnessCutoutMaterial == null)
            {
                StandardRoughnessCutoutMaterial = Resources.Load("StandardRoughnessCutoutMaterial") as Material;
            }
            if (StandardRoughnessFadeMaterial == null)
            {
                StandardRoughnessFadeMaterial = Resources.Load("StandardRoughnessFadeMaterial") as Material;
            }
            return
                StandardBaseMaterial != null &&
                StandardBaseAlphaMaterial != null &&
                StandardBaseCutoutMaterial != null &&
                StandardBaseFadeMaterial != null &&
                StandardSpecularMaterial != null &&
                StandardSpecularAlphaMaterial != null &&
                StandardSpecularCutoutMaterial != null &&
                StandardSpecularFadeMaterial != null &&
                StandardRoughnessMaterial != null &&
                StandardRoughnessCutoutMaterial != null &&
                StandardRoughnessAlphaMaterial != null &&
                StandardRoughnessFadeMaterial != null;
        }

        /// <summary>
        /// Loads the <see cref="UnityEngine.Texture"/> resource to show in case of unknown textures.
        /// @warning Don´t remove the __NotFound.asset_ included in the package.
        /// </summary>
        private static bool LoadNotFoundTexture()
        {
            if (NotFoundTexture == null)
            {
                NotFoundTexture = Resources.Load("NotFound") as Texture2D;
            }
            return NotFoundTexture != null;
        }

        /// <summary>
        /// Internally loads a file from memory into its data representation.
        /// </summary>
        /// <param name="fileBytes">File data to load.</param>
        /// <param name="filename">Filename, in case it doesn't exist, the file extension should be used (eg: ".FBX").</param>
        /// <param name="basePath">Base path from the loaded file.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions" /> used to load the file.</param>
        /// <param name="usesWrapperGameObject">Pass <c>true</c> when using a wrapper <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="dataCallback">Custom resource data retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="existsCallback">Custom resource size retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="loadTextureDataCallback">Pass this callback to load texture data from custom sources.</param>
        /// <param name="progressCallback">Callback used to retrieve file loading percentage.</param>
        /// <param name="customFileLoadData">Custom <see cref="FileLoadData"></see> used to store additional asset source information.</param>
        protected void InternalLoadFromMemory(byte[] fileBytes, string filename, string basePath, AssetLoaderOptions options = null, bool usesWrapperGameObject = false, AssimpInterop.DataCallback dataCallback = null, AssimpInterop.ExistsCallback existsCallback = null, LoadTextureDataCallback loadTextureDataCallback = null, AssimpInterop.ProgressCallback progressCallback = null, FileLoadData customFileLoadData = null)
        {
            Dispose();
            var fileLoadData = customFileLoadData ?? new GCFileLoadData
            {
                Filename = filename,
                BasePath = basePath
            };
            var fileId = FilesLoadData.Count;
            FilesLoadData.Add(fileLoadData);
            try
            {
                Scene = ImportFileFromMemory(fileBytes, FileUtils.GetFileExtension(filename), options, dataCallback, existsCallback, fileId, progressCallback);
            }
            catch (Exception exception)
            {
                throw new Exception("Error parsing file.", exception);
            }
            if (Scene == IntPtr.Zero)
            {
                var error = AssimpInterop.ai_GetErrorString();
                throw new Exception(string.Format("Error loading asset. Assimp returns: [{0}]", error));
            }
            LoadInternal(basePath, options, usesWrapperGameObject, loadTextureDataCallback, filename);
            FilesLoadData[fileId] = null;
        }

#if !UNITY_EDITOR && UNITY_WEBGL
        /// <summary>
        /// Internally loads a model from browser files with custom embedded data loading.
        /// </summary>
        /// <param name="filesCount">Browser files count.</param>
        /// <param name="fileData">Main browser file byte data.</param>
        /// <param name="fileName">Main browser file name.</param>
        /// <param name="assetExtension">Main browser file extension.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to load the file.</param>
        /// <param name="usesWrapperGameObject">Pass <c>true</c> when using a wrapper <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="dataCallback">Custom resource data retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="existsCallback">Custom resource size retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="loadTextureDataCallback">Pass this callback to load texture data from custom sources.</param>
        /// <param name="progressCallback">Callback used to retrieve file loading percentage.</param>
        /// <returns></returns>
        protected void InternalLoadFromBrowserFiles(int filesCount, byte[] fileData, string fileName, string assetExtension, AssetLoaderOptions options = null, bool usesWrapperGameObject = false, AssimpInterop.DataCallback dataCallback = null, AssimpInterop.ExistsCallback existsCallback = null, LoadTextureDataCallback loadTextureDataCallback = null, AssimpInterop.ProgressCallback progressCallback = null)
        {
            FileLoadData fileLoadData = null;
#if TRILIB_USE_ZIP
            if (assetExtension.ToLowerInvariant() == ".zip")
            {
                fileLoadData = SetupZipLoading(ref fileData, assetExtension, null, ref loadTextureDataCallback);
            }
#endif
            if (fileLoadData == null) {
                fileLoadData = new BrowserLoadData
                {
                    FilesCount = filesCount,
                    Filename = fileName
                };
                if (loadTextureDataCallback == null)
                {
                    loadTextureDataCallback = delegate (string path, string basePath)
                    {
                        var fileShortName = FileUtils.GetShortFilename(path).ToLowerInvariant();
                        for (var i = 0; i < filesCount; i++)
                        {
                            if (JSHelper.Instance.GetBrowserFileName(i).ToLowerInvariant() == fileShortName)
                            {
                                var checkFileData = JSHelper.Instance.GetBrowserFileData(i);
                                var embeddedTextureData = new EmbeddedTextureData();
                                embeddedTextureData.DataPointer = STBImageLoader.LoadTextureDataFromByteArray(checkFileData, out embeddedTextureData.Width, out embeddedTextureData.Height, out embeddedTextureData.NumChannels, out embeddedTextureData.DataLength);
                                embeddedTextureData.OnDataDisposal = STBImageLoader.UnloadTextureData;
                                return embeddedTextureData;
                            }
                        }
                        return null;
                    };
                }
            }
            InternalLoadFromMemory(fileData, fileName, null, options, usesWrapperGameObject, dataCallback, existsCallback, loadTextureDataCallback, progressCallback, fileLoadData);
        }

        /// <summary>
        /// Gets the first supported browser file name, data and extension.
        /// </summary>
        /// <param name="filesCount">Browser files count.</param>
        /// <param name="fileData">Supported browser file byte data.</param>
        /// <param name="fileName">Supported browser file name.</param
        /// <param name="fileExtension">Supported browser file extension.</param>
        /// <returns><c>true</c> when a supported file has been found, otherwise, returns <c>false</c></returns>
        protected static bool GetSupportedBrowserFileData(int filesCount, out byte[] fileData, out string fileName, out string fileExtension)
        {
            var supportedExtensions = GetSupportedFileExtensions() + ";*.zip;";
            for (var i = 0; i < filesCount; i++)
            {
                fileName = JSHelper.Instance.GetBrowserFileName(i);
                fileExtension = FileUtils.GetFileExtension(fileName);
                if (supportedExtensions.Contains("*" + fileExtension + ";"))
                {
                    fileData = JSHelper.Instance.GetBrowserFileData(i);
                    return true;
                }
            }
            fileData = null;
            fileName = null;
            fileExtension = null;
            return false;
        }
#endif

#if TRILIB_USE_ZIP
        /// <summary>
        /// Setups the file to be treated as a ZIP file.
        /// </summary>
        /// <param name="data">File byte data.</param>
        /// <param name="assetExtension">Reference to set the asset extension as .zip.</param>
        /// <param name="rootBasePath">File base path.</param>
        /// <param name="loadTextureDataCallback">Callback to use when loading textures.</param>
        /// <returns>A new <see cref="ZipGCFileLoadData"></see> used to store ZIP loading information.</returns>
        private ZipGCFileLoadData SetupZipLoading(ref byte[] data, string assetExtension, string rootBasePath, ref LoadTextureDataCallback loadTextureDataCallback)
        {
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
            ZipArchive zipFile = null;
#else
            ZipFile zipFile = null;
#endif
#if UNITY_EDITOR || (!NETFX_CORE && !NET_4_6 && !NET_STANDARD_2_0) || ENABLE_IL2CPP || ENABLE_MONO
            ZipConstants.DefaultCodePage = 0;
#endif
            var supportedExtensions = GetSupportedFileExtensions();
            var memoryStream = new MemoryStream(data);
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
                    zipFile = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                    foreach (ZipArchiveEntry zipEntry in zipFile.Entries)
                    {
#else
            zipFile = new ZipFile(memoryStream);
            foreach (ZipEntry zipEntry in zipFile)
            {
                if (!zipEntry.IsFile)
                {
                    continue;
                }
#endif
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
                    var fileFullName = zipEntry.FullName;
#else
                var fileFullName = zipEntry.Name;
#endif
                if (fileFullName.Contains("__MACOSX"))
                {
                    continue;
                }
                var entryExtension = FileUtils.GetFileExtension(fileFullName);
                if (supportedExtensions.Contains("*" + entryExtension + ";"))
                {
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
                        var zipStream = zipEntry.Open();
#else
                    var zipStream = zipFile.GetInputStream(zipEntry);
#endif
                    data = StreamUtils.ReadFullStream(zipStream);
                    assetExtension = entryExtension;
                    zipStream.Dispose();
                }
            }
            if (loadTextureDataCallback == null)
            {
                loadTextureDataCallback = delegate (string path, string basePath)
                {
                    if (zipFile != null)
                    {
                        var fileShortName = FileUtils.GetShortFilename(path).ToLowerInvariant();
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
                        foreach (ZipArchiveEntry zipEntry in zipFile.Entries)
                        {
#else
                        foreach (ZipEntry zipEntry in zipFile)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue;
                            }
#endif
                            if (FileUtils.GetShortFilename(zipEntry.Name).ToLowerInvariant() == fileShortName)
                            {
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
                                var zipStream = zipEntry.Open();
#else
                                var zipStream = zipFile.GetInputStream(zipEntry);
#endif
                                var uncompressedData = StreamUtils.ReadFullStream(zipStream);
                                var embeddedTextureData = new EmbeddedTextureData();
                                embeddedTextureData.DataPointer = STBImageLoader.LoadTextureDataFromByteArray(uncompressedData, out embeddedTextureData.Width, out embeddedTextureData.Height, out embeddedTextureData.NumChannels, out embeddedTextureData.DataLength);
                                embeddedTextureData.OnDataDisposal = STBImageLoader.UnloadTextureData;
                                zipStream.Dispose();
                                return embeddedTextureData;
                            }
                        }
                    }
                    return null;
                };
                return new ZipGCFileLoadData
                {
                    ZipFile = zipFile,
                    Filename = assetExtension,
                    BasePath = rootBasePath
                };
            }
            return null;
        }
#endif

        /// <summary>
        /// Internally loads a model from memory with custom embedded texture loading for ZIP files.
        /// </summary>
        /// <param name="data">Loaded file data.</param>
        /// <param name="assetExtension">Loaded file extension.</param>
        /// <param name="basePath">Base path from the loaded file.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to load the file.</param>
        /// <param name="usesWrapperGameObject">Pass <c>true</c> when using a wrapper <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="dataCallback">Custom resource data retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="existsCallback">Custom resource size retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="loadTextureDataCallback">Pass this callback to load texture data from custom sources.</param>
        /// <param name="progressCallback">Callback used to retrieve file loading percentage.</param>
        /// <param name="filename">Source filename.</param>
        /// <returns></returns>
        protected void InternalLoadFromMemoryAndZip(byte[] data, string assetExtension, string basePath, AssetLoaderOptions options = null, bool usesWrapperGameObject = false, AssimpInterop.DataCallback dataCallback = null, AssimpInterop.ExistsCallback existsCallback = null, LoadTextureDataCallback loadTextureDataCallback = null, AssimpInterop.ProgressCallback progressCallback = null, string filename = null)
        {
            FileLoadData fileLoadData = null;
#if TRILIB_USE_ZIP
            if (assetExtension.ToLowerInvariant() == ".zip")
            {
                fileLoadData = SetupZipLoading(ref data, assetExtension, basePath, ref loadTextureDataCallback);
            }
#endif
            InternalLoadFromMemory(data, assetExtension ?? filename, basePath, options, usesWrapperGameObject, dataCallback, existsCallback, loadTextureDataCallback, progressCallback, fileLoadData);
        }

        /// <summary>
        /// Internally loads a file from file system into it's data representation.
        /// </summary>
        /// <param name="filename">Filename to load.</param>
        /// <param name="basePath">Base path from the loaded file.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions" /> used to load the file.</param>
        /// <param name="usesWrapperGameObject">Pass <c>true</c> when using a wrapper <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="progressCallback">Callback used to retrieve file loading percentage.</param>
        protected void InternalLoadFromFile(string filename, string basePath, AssetLoaderOptions options = null, bool usesWrapperGameObject = false, AssimpInterop.ProgressCallback progressCallback = null)
        {
            Dispose();
            try
            {
                Scene = ImportFile(filename, options, progressCallback);
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("Error parsing file: {0}", filename), exception);
            }
            if (Scene == IntPtr.Zero)
            {
                var error = AssimpInterop.ai_GetErrorString();
                throw new Exception(string.Format("Error loading asset. Assimp returns: [{0}]", error));
            }
            LoadInternal(basePath, options, usesWrapperGameObject, null, filename);
        }

        /// <summary>
        /// Builds the <see cref="UnityEngine.GameObject"/> from pre-loaded data.
        /// </summary>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to build the <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="basePath">Base path from the loaded file.</param>
        /// <param name="wrapperGameObject">Wrapper <see cref="UnityEngine.GameObject"/> to build the object into.</param>
        /// <returns>When options is null or options.UseOriginalPositionRotationAndScale is <c>false</c>, returns the built <see cref="UnityEngine.GameObject"/>. When options.UseOriginalPositionRotationAndScale is <c>true</c> returns the wrapper <see cref="UnityEngine.GameObject"/> created for the object.</returns>
        protected GameObject BuildGameObject(AssetLoaderOptions options, string basePath = null, GameObject wrapperGameObject = null)
        {
            if (HasOnMetadataProcessed && Metadata != null && (options == null || !options.DontLoadMetadata))
            {
                foreach (var metadata in Metadata)
                {
                    ProcessMetadata(metadata);
                }
            }
            if (MaterialData != null && (options == null || !options.DontLoadMaterials))
            {
                LoadAllStandardMaterials();
                LoadedMaterials = new Dictionary<string, Material>();
                LoadedTextures = new Dictionary<string, Texture2D>();
                foreach (var materialData in MaterialData)
                {
                    TransformMaterialData(materialData, options, basePath);
                }
            }
            if (MeshData != null && (options == null || !options.DontLoadMeshes))
            {
                foreach (var meshData in MeshData)
                {
                    TransformMeshData(meshData, options);
                }
            }
            GameObject transformWrapperGameObject = null;
            GameObject gameObject = null;
            if (RootNodeData != null)
            {
                if (options != null && options.UseOriginalPositionRotationAndScale)
                {
                    transformWrapperGameObject = new GameObject();
                }
                else
                {
                    Debug.LogWarning("Deprecation warning: Please use an AssetLoaderOptions instance when loading your model, and set it's UseOriginalPositionRotationAndScale field to true.\nThis new field ensures your model will use the original model local position, rotation and scale, by adding a wrapper on top of the loaded GameObject.");
                }
                gameObject = TransformNodeData(RootNodeData, options, transformWrapperGameObject ?? wrapperGameObject);
                if (gameObject != null)
                {
                    if (LoadedBoneNames != null && LoadedBoneNames.Count > 0)
                    {
                        SetupSkinnedMeshRendererTransforms(gameObject);
                    }
                    if (transformWrapperGameObject == null)
                    {
                        LoadContextOptions(gameObject, options);
                    }
                    else
                    {
                        transformWrapperGameObject.name = string.Format("{0}_Wrapper", gameObject.name);
                        transformWrapperGameObject.transform.SetParent(wrapperGameObject != null ? wrapperGameObject.transform : gameObject.transform, false);

                        LoadContextOptions(transformWrapperGameObject, options);
                    }
                }
            }
            if (AnimationData != null && (options == null || !options.DontLoadAnimations))
            {
                foreach (var animationData in AnimationData)
                {
                    TransformAnimationData(animationData, options, gameObject, transformWrapperGameObject != null || wrapperGameObject != null);
                }
            }
            var topGameObject = transformWrapperGameObject ?? wrapperGameObject ?? gameObject;
            var loadedGameObject = transformWrapperGameObject ?? gameObject;
            if (gameObject != null)
            {
                if (options == null || !options.DontApplyAnimations)
                {
                    SetupAnimations(topGameObject, options);
                }
                if (CameraData != null && (options == null || !options.DontLoadCameras))
                {
                    foreach (var cameraData in CameraData)
                    {
                        TransformCameraData(gameObject, cameraData, options);
                    }
                }
                if (options != null && options.AddAssetUnloader)
                {
                    topGameObject.AddComponent<AssetUnloader>();
                }
                if (HasOnObjectLoaded)
                {
                    TriggerOnObjectLoaded(loadedGameObject);
                }
            }
            return loadedGameObject;
        }

        /// <summary>
        /// Setups the <see cref="UnityEngine.SkinnedMeshRenderer"/> bone transforms.
        /// </summary>
        /// <param name="gameObject"><see cref="UnityEngine.GameObject"/> where the bones will be searched.</param>
        protected virtual void SetupSkinnedMeshRendererTransforms(GameObject gameObject)
        {
            foreach (var loadedSkinnedMeshRenderer in LoadedBoneNames)
            {
                var skinnedMeshRenderer = loadedSkinnedMeshRenderer.Key;
                var boneNames = loadedSkinnedMeshRenderer.Value;
                var boneCount = boneNames.Count;
                var transforms = new List<Transform>(boneCount);
                var rootTransform = skinnedMeshRenderer.transform;
                var bestChildCount = 0;
                for (var i = 0; i < boneCount; i++)
                {
                    var boneName = boneNames[i];
                    if (boneName == null)
                    {
                        continue;
                    }
                    var transform = gameObject.transform.FindDeepChild(boneName);
                    if (transform == null)
                    {
                        continue;
                    }
                    transforms.Add(transform);
                    for (; ; )
                    {
                        var hasNonTransformComponent = false;
                        var components = transform.GetComponents(typeof(Component));
                        foreach (var component in components)
                        {
                            if (component.GetType() != typeof(Transform))
                            {
                                hasNonTransformComponent = true;
                                break;
                            }
                        }
                        if (hasNonTransformComponent)
                        {
                            break;
                        }
                        var childCount = CountChild(transform);
                        if (childCount > bestChildCount)
                        {
                            rootTransform = transform;
                            bestChildCount = childCount;
                        }
                        transform = transform.parent;
                        if (transform == gameObject.transform || transform == null)
                        {
                            break;
                        }
                    }
                }
                skinnedMeshRenderer.rootBone = rootTransform;
                skinnedMeshRenderer.bones = transforms.ToArray();
            }
        }

        /// <summary>
        /// Applies transform from <see cref="AssetLoaderOptions"/> into given <see cref="UnityEngine.GameObject" />.
        /// </summary>
        /// <param name="gameObject"><see cref="UnityEngine.GameObject" /> to transform.</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the transform.</param>
        private static void LoadContextOptions(GameObject gameObject, AssetLoaderOptions options)
        {
            gameObject.transform.rotation = Quaternion.Euler(options == null ? new Vector3(0f, 180f, 0f) : options.RotationAngles);
            gameObject.transform.localScale = Vector3.one * (options == null ? 1f : options.Scale);
        }

        /// <summary>
        /// Processes the given metadata by triggering the OnMetadataProcessed event.
        /// </summary>
        /// <param name="metadata"><see cref="TriLib.AssimpMetadata"/> to process.</param>
        protected virtual void ProcessMetadata(AssimpMetadata metadata)
        {
            TriggerOnMetadataProcessed(metadata.MetadataType, metadata.MetadataIndex, metadata.MetadataKey, metadata.MetadataValue);
        }

        /// <summary>
        /// Setups animation components and <see cref="UnityEngine.AnimationClip"/> clips into the given <see cref="UnityEngine.GameObject"/>.
        /// </summary>
        /// <param name="gameObject"><see cref="UnityEngine.GameObject"/> to add the component to.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to process the components.</param>
        protected virtual void SetupAnimations(GameObject gameObject, AssetLoaderOptions options)
        {
            if (options == null || !options.DontLoadAnimations && AnimationData != null && AnimationData.Length > 0)
            {
                if (options == null || options.UseLegacyAnimations)
                {
                    var unityAnimation = gameObject.GetComponent<Animation>();
                    if (unityAnimation == null)
                    {
                        unityAnimation = gameObject.AddComponent<Animation>();
                    }
                    AnimationClip defaultClip = null;
                    if (AnimationData != null)
                    {
                        for (var c = 0; c < AnimationData.Length; c++)
                        {
                            var unityAnimationClip = AnimationData[c].AnimationClip;
                            if (unityAnimationClip == null)
                            {
                                continue;
                            }
                            unityAnimation.AddClip(unityAnimationClip, unityAnimationClip.name);
                            if (c == 0)
                            {
                                defaultClip = unityAnimationClip;
                            }
                        }
                    }
                    unityAnimation.clip = defaultClip;
                    if (options == null || options.AutoPlayAnimations)
                    {
                        unityAnimation.Play();
                    }
                }
                else if (!options.UseLegacyAnimations)
                {
                    var unityAnimator = gameObject.GetComponent<Animator>();
                    if (unityAnimator == null)
                    {
                        unityAnimator = gameObject.AddComponent<Animator>();
                    }
                    if (options.AnimatorController != null)
                    {
                        unityAnimator.runtimeAnimatorController = options.AnimatorController;
                    }

#if UNITY_EDITOR
                    else if (HasAnimatorControllerCreation)
                    {
                        var animatorController = CallAnimatorControllerCreation();
                        unityAnimator.runtimeAnimatorController = animatorController;
                        if (HasOnAnimatorControllerCreated)
                        {
                            TriggerOnAnimatorControllerCreated(animatorController, unityAnimator);
                        }
                        if (animatorController.layers.Length > 0)
                        {
                            if (AnimationData != null)
                            {
                                foreach (var animationData in AnimationData)
                                {
                                    var unityAnimationClip = animationData.AnimationClip;
                                    if (unityAnimationClip == null)
                                    {
                                        continue;
                                    }
                                    animatorController.AddMotion(unityAnimationClip);
                                }
                            }
                        }
                    }
#endif
                    if (!options.DontGenerateAvatar)
                    {
                        if (options.Avatar != null)
                        {
                            unityAnimator.avatar = options.Avatar;
                        }
                        else
                        {
                            var avatar = AvatarBuilder.BuildGenericAvatar(gameObject, "");
                            avatar.name = FixName(gameObject.name);
                            unityAnimator.avatar = avatar;
                            if (HasOnAvatarCreated)
                            {
                                TriggerOnAvatarCreated(avatar, unityAnimator);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Transforms given <see cref="TriLib.NodeData"/> into a <see cref="UnityEngine.GameObject"/>.
        /// </summary>
        /// <param name="nodeData"><see cref="TriLib.NodeData"/> to be transformed.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to transform the data.</param>
        /// <param name="existingGameObject"><see cref="UnityEngine.GameObject"> used to add the components to instead of adding to a new object.</see></param>
        /// <returns>The transformed <see cref="UnityEngine.GameObject"/>.</returns>
        protected virtual GameObject TransformNodeData(NodeData nodeData, AssetLoaderOptions options, GameObject existingGameObject = null)
        {
            var gameObject = new GameObject { name = nodeData.Name };
            if (nodeData.Metadata != null && (options == null || !options.DontAddMetadataCollection && !options.DontLoadMetadata))
            {
                var metadataCollection = gameObject.AddComponent<AssimpMetadataCollection>();
                if (nodeData == RootNodeData)
                {
                    foreach (var metadata in Metadata)
                    {
                        if (!metadataCollection.ContainsKey(metadata.MetadataKey))
                        {
                            metadataCollection.Add(metadata.MetadataKey, metadata);
                        }
#if TRILIB_OUTPUT_MESSAGES
                        else
                        {
                            Debug.LogErrorFormat("Duplicated metadata found:{0}", metadata.MetadataKey);
                        }
#endif
                    }
                }
                foreach (var metadata in nodeData.Metadata)
                {
                    if (!metadataCollection.ContainsKey(metadata.MetadataKey))
                    {
                        metadataCollection.Add(metadata.MetadataKey, metadata);
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogErrorFormat("Duplicated metadata found: {0} for GameObject: {1}", metadata.MetadataKey, gameObject.name);
                    }
#endif
                }
            }
            var parentGameObject = existingGameObject != null ? existingGameObject : (nodeData.Parent == null ? null : nodeData.Parent.GameObject);
            if (parentGameObject != null)
            {
                gameObject.transform.SetParent(parentGameObject.transform, false);
            }
            gameObject.transform.LoadMatrix(nodeData.Matrix);
            if (nodeData.Meshes != null && nodeData.Meshes.Length > 0 && MeshData != null && MeshData.Length > 0)
            {
                var vertexCount = 0;
                foreach (var meshIndex in nodeData.Meshes)
                {
                    var meshData = MeshData[meshIndex];
                    vertexCount += meshData.Vertices.Length;
                }
                var combineMeshes = options == null || options.CombineMeshes;
                var canCombine16BitsMesh = vertexCount < 65536 && combineMeshes;
#if UNITY_2017_3_OR_NEWER
#if UNITY_ANDROID
                var use32BitsIndexFormat = options != null && options.Use32BitsIndexFormat;
#else
                var use32BitsIndexFormat = options == null || options.Use32BitsIndexFormat;
#endif
                var canCombine32BitsMesh = use32BitsIndexFormat && combineMeshes;
                var useCombineInstances = !HasBlendShapes && (canCombine32BitsMesh || canCombine16BitsMesh);
#else
                var useCombineInstances = !HasBlendShapes && canCombine16BitsMesh;
#endif
                if (useCombineInstances)
                {
                    Material lastMaterial = null;
                    List<string> combinedNodeNames = null;
                    var singleMaterial = true;
                    var combineInstances = new CombineInstance[nodeData.Meshes.Length];
                    var combinedMaterials = new Material[nodeData.Meshes.Length];
                    for (var i = 0; i < nodeData.Meshes.Length; i++)
                    {
                        var meshIndex = nodeData.Meshes[i];
                        if (meshIndex >= MeshData.Length)
                        {
                            continue;
                        }
                        var meshData = MeshData[meshIndex];
                        if (meshData.HasBoneInfo && meshData.BoneNames.Length > 0)
                        {
                            if (combinedNodeNames == null)
                            {
                                combinedNodeNames = new List<string>();
                            }
                            combinedNodeNames.AddRange(meshData.BoneNames);
                        }
                        var combineInstance = new CombineInstance
                        {
                            mesh = meshData.Mesh,
                            transform = Matrix4x4.identity
                        };
                        combineInstances[i] = combineInstance;
                        if (MaterialData == null || MaterialData.Length == 0 || meshData.MaterialIndex >= MaterialData.Length)
                        {
                            continue;
                        }
                        var materialData = MaterialData[meshData.MaterialIndex];
                        var material = materialData.Material;
                        if (lastMaterial != null && material != lastMaterial)
                        {
                            singleMaterial = false;
                        }
                        combinedMaterials[i] = material;
                        lastMaterial = material;
                    }
                    var combinedMesh = new Mesh();
#if UNITY_2017_3_OR_NEWER
                    if (use32BitsIndexFormat)
                    {
                        combinedMesh.indexFormat = IndexFormat.UInt32;
                    }
#endif
                    combinedMesh.CombineMeshes(combineInstances, singleMaterial);
                    combinedMesh.name = FixName(nodeData.Name);
                    CreateMeshComponents(gameObject, options, combinedMesh, HasBoneInfo || HasBlendShapes, combinedMaterials, combinedNodeNames, singleMaterial ? combinedMaterials[0] : null);
                }
                else
                {
                    for (var i = 0; i < nodeData.Meshes.Length; i++)
                    {
                        var meshIndex = nodeData.Meshes[i];
                        var meshData = MeshData[meshIndex];
                        var material = MaterialData == null ? null : MaterialData[meshData.MaterialIndex].Material;
                        var subMeshName = "SubMesh_" + i;
                        var subGameObject = new GameObject { name = subMeshName };
                        meshData.SubMeshName = subMeshName;
                        subGameObject.transform.SetParent(gameObject.transform, false);
                        var connectionKey = gameObject.name + "*" + i;
                        CreateMeshComponents(subGameObject, options, meshData.Mesh, HasBoneInfo || HasBlendShapes, null, meshData.BoneNames, material, meshData, connectionKey);
                    }
                }

            }
            nodeData.GameObject = gameObject;
            if (nodeData.Children != null)
            {
                foreach (var childNodeData in nodeData.Children)
                {
                    TransformNodeData(childNodeData, options);
                }
            }
            return gameObject;
        }

        ///<summary>
        /// Counts the <see cref="UnityEngine.Transform" /> total children recursively.
        ///</summary>
        ///<param name="transform">The <see cref="UnityEngine.Transform" /> to count the children.</param>
        private static int CountChild(Transform transform)
        {
            var childCount = transform.childCount;
            foreach (Transform child in transform)
            {
                childCount += CountChild(child);
            }
            return childCount;
        }

        /// <summary>
        /// Creates mesh rendering components into given <see cref="UnityEngine.GameObject"/>.
        /// </summary>
        /// <param name="gameObject"><see cref="UnityEngine.GameObject"/> to create the components at.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to create the components.</param>
        /// <param name="mesh"><see cref="UnityEngine.Mesh"/> to add to the created component.</param>
        /// <param name="hasBoneInfo">If <c>true</c>, creates a <see cref="UnityEngine.SkinnedMeshRenderer"/>, otherwise, creates a <see cref="UnityEngine.MeshRenderer"/>.</param>
        /// <param name="combinedMaterials"><see cref="UnityEngine.Material"/> list to assign to the created component.</param>
        /// <param name="boneNames">Bone names loaded for the given component.</param>
        /// <param name="singleMaterial">Single <see cref="UnityEngine.Material"/> to assign to the component.</param>
        /// <param name="meshData"><see cref="MeshData"/> used to store a reference to the created <see cref="UnityEngine.SkinnedMeshRenderer"/>.</param>
        /// <param name="connectionKey">Key used to build a <see cref="MeshData"/> connection.</param>
        protected virtual void CreateMeshComponents(GameObject gameObject, AssetLoaderOptions options, Mesh mesh,
            bool hasBoneInfo, Material[] combinedMaterials, IList<string> boneNames = null, Material singleMaterial = null, MeshData meshData = null, string connectionKey = null)
        {
            if (hasBoneInfo && (options == null || !options.DontLoadSkinning))
            {
                var skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.sharedMesh = mesh;
                skinnedMeshRenderer.quality = SkinQuality.Bone4;
                if (boneNames != null)
                {
                    if (LoadedBoneNames == null)
                    {
                        LoadedBoneNames = new Dictionary<SkinnedMeshRenderer, IList<string>>();
                    }
                    LoadedBoneNames.Add(skinnedMeshRenderer, boneNames);
                }
                if (meshData != null && connectionKey != null)
                {
                    if (MeshDataConnections == null)
                    {
                        MeshDataConnections = new Dictionary<string, MeshData>();
                    }
                    MeshDataConnections.Add(connectionKey, meshData);
                }
                if (singleMaterial != null)
                {
                    skinnedMeshRenderer.sharedMaterial = singleMaterial;
                }
                else
                {
                    skinnedMeshRenderer.sharedMaterials = combinedMaterials;
                }
            }
            else
            {
                var meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = mesh;
                var meshRenderer = gameObject.AddComponent<MeshRenderer>();
                if (singleMaterial != null)
                {
                    meshRenderer.sharedMaterial = singleMaterial;
                }
                else
                {
                    meshRenderer.sharedMaterials = combinedMaterials;
                }
                if (options != null && options.GenerateMeshColliders)
                {
                    var meshCollider = gameObject.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = mesh;
                    meshCollider.convex = options.ConvexMeshColliders;
                }
            }
            if (HasOnMeshCreated)
            {
                TriggerOnMeshCreated(0, mesh);
            }
        }

        /// <summary>
        /// Transforms the given <see cref="TriLib.CameraData"/> into a <see cref="UnityEngine.Camera"/>.
        /// </summary>
        /// <param name="gameObject"><see cref="UnityEngine.GameObject"/> to add the <see cref="UnityEngine.Camera"/> component into.</param>
        /// <param name="cameraData"><see cref="TriLib.CameraData"/> to be transformed.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to transform the camera.</param>
        protected virtual void TransformCameraData(GameObject gameObject, CameraData cameraData, AssetLoaderOptions options)
        {
            var cameraTransform = gameObject.transform.FindDeepChild(cameraData.Name);
            if (cameraTransform == null)
            {
                return;
            }
            var camera = cameraTransform.gameObject.AddComponent<Camera>();
            camera.aspect = cameraData.Aspect;
            camera.nearClipPlane = cameraData.NearClipPlane;
            camera.farClipPlane = cameraData.FarClipPlane;
            camera.fieldOfView = Mathf.Rad2Deg * cameraData.FieldOfView;
            camera.transform.localPosition = cameraData.LocalPosition;
            camera.transform.LookAt(cameraData.Forward, cameraData.Up);
            cameraData.Camera = camera;
        }

        /// <summary>
        /// Fixes animation curve length issues (curves containing only one key or with length too small).
        /// </summary>
        /// <param name="animationLength">Final animation length.</param>
        /// <param name="curve">Curve to fix.</param>
        /// <returns>Fixed curve.</returns>
        private static AnimationCurve FixCurve(float animationLength, AnimationCurve curve)
        {
            if (Mathf.Approximately(animationLength, 0f))
            {
                animationLength = 1f;
            }
            if (curve.keys.Length == 1)
            {
                curve.AddKey(new Keyframe(animationLength, curve.keys[0].value));
            }
            return curve;
        }

        /// <summary>
        /// Transforms the given <see cref="TriLib.AnimationData"/> into a <see cref="UnityEngine.AnimationClip"/>.
        /// </summary>
        /// <param name="animationData"><see cref="TriLib.AnimationData"/> to be transformed.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to transform the animation.</param>
        /// <param name="gameObject">Transformed <see cref="UnityEngine.GameObject" />.</param>
        /// <param name="useWrapperGameObject">Set to <c>true</c> when using a wrapper <see cref="UnityEngine.GameObject"/>.</param>
        protected virtual void TransformAnimationData(AnimationData animationData, AssetLoaderOptions options, GameObject gameObject, bool useWrapperGameObject = false)
        {
            var animationClip = new AnimationClip
            {
                name = animationData.Name,
                frameRate = animationData.FrameRate,
                wrapMode = animationData.WrapMode,
                legacy = animationData.Legacy
            };
            foreach (var animationChannelData in animationData.ChannelData)
            {
                if (!NodesPath.ContainsKey(animationChannelData.NodeName))
                {
                    continue;
                }
                var nodePath = NodesPath[animationChannelData.NodeName];
                if (useWrapperGameObject)
                {
                    nodePath = gameObject.name + "/" + nodePath;
                }
                foreach (var animationCurveData in animationChannelData.CurveData)
                {
                    var propertyName = animationCurveData.Key;
                    var curveData = animationCurveData.Value;
                    var animationCurve = FixCurve(animationData.Length, new AnimationCurve { keys = curveData.Keyframes });
                    curveData.AnimationCurve = animationCurve;
                    animationClip.SetCurve(nodePath, typeof(Transform), propertyName, animationCurve);
                }
            }
            if (animationData.MorphData.Length > 0)
            {
                foreach (var animationMorphData in animationData.MorphData)
                {
                    MeshData meshData;
                    string nodePath;
                    string meshNodeName;
                    if (!animationMorphData.NodeName.Contains("*"))
                    {
                        meshNodeName = animationMorphData.NodeName + "*0";
                    }
                    else
                    {
                        meshNodeName = animationMorphData.NodeName;
                    }
                    string nodeName = meshNodeName.Substring(0, meshNodeName.LastIndexOf("*", StringComparison.Ordinal));
                    if (MeshDataConnections != null && MeshDataConnections.ContainsKey(meshNodeName) && NodesPath.ContainsKey(nodeName))
                    {
                        meshData = MeshDataConnections[meshNodeName];
                        nodePath = NodesPath[nodeName];
                    }
                    else
                    {
                        continue;
                    }
                    var curves = new Dictionary<MorphData, List<Keyframe>>();
                    foreach (var animationMorphDataKvp in animationMorphData.MorphChannelKeys)
                    {
                        var time = animationMorphDataKvp.Key;
                        var morphChannelKey = animationMorphDataKvp.Value;
                        for (var i = 0; i < morphChannelKey.Indices.Length; i++)
                        {
                            var index = morphChannelKey.Indices[i];
                            var weight = morphChannelKey.Weights[i];
                            if (index > meshData.MorphsData.Length)
                            {
                                continue;
                            }
                            List<Keyframe> keyFrames;
                            var morphData = meshData.MorphsData[index];
                            if (curves.ContainsKey(morphData))
                            {
                                keyFrames = curves[morphData];
                            }
                            else
                            {
                                keyFrames = new List<Keyframe>();
                                curves.Add(morphData, keyFrames);
                            }
                            keyFrames.Add(new Keyframe(time, weight));
                        }
                    }
                    foreach (var curveKvp in curves)
                    {
                        var animationCurve = FixCurve(animationData.Length, new AnimationCurve { keys = curveKvp.Value.ToArray() });
                        animationClip.SetCurve(string.Format("{0}/{1}", nodePath, meshData.SubMeshName), typeof(SkinnedMeshRenderer), string.Format("blendShape.{0}", curveKvp.Key.Name), animationCurve);
                    }
                }
            }
            if (options != null && options.EnsureQuaternionContinuity)
            {
                animationClip.EnsureQuaternionContinuity();
            }
            if (HasOnAnimationClipCreated)
            {
                TriggerOnAnimationClipCreated(0, animationClip);
            }
            animationData.AnimationClip = animationClip;
        }

        /// <summary>
        /// Transforms the given <see cref="TriLib.MeshData"/> into a <see cref="UnityEngine.Mesh"/>.
        /// </summary>
        /// <param name="meshData"><see cref="TriLib.MeshData"/> to be transformed.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to transform the mesh.</param>
        protected virtual void TransformMeshData(MeshData meshData, AssetLoaderOptions options)
        {
            var mesh = new Mesh();
#if UNITY_2017_3_OR_NEWER
            if (options == null || options.Use32BitsIndexFormat)
            {
                mesh.indexFormat = IndexFormat.UInt32;
            }
#endif
            mesh.name = meshData.Name;
            mesh.vertices = meshData.Vertices;
            mesh.normals = meshData.Normals;
            mesh.uv4 = meshData.Uv3;
            mesh.uv3 = meshData.Uv2;
            mesh.uv2 = meshData.Uv1;
            mesh.uv = meshData.Uv;
            mesh.tangents = meshData.Tangents;
            mesh.colors = meshData.Colors;
            mesh.boneWeights = meshData.BoneWeights;
            mesh.bindposes = meshData.BindPoses;
            mesh.triangles = meshData.Triangles;
            if ((options == null || !options.DontLoadBlendShapes) && meshData.MorphsData != null)
            {
                foreach (var morphData in meshData.MorphsData)
                {
                    mesh.AddBlendShapeFrame(morphData.Name, morphData.Weight, morphData.Vertices, morphData.Normals, morphData.Tangents);
                    if (HasOnBlendShapeKeyCreated)
                    {
                        TriggerOnBlendShapeKeyCreated(mesh, meshData.Name, meshData.Vertices, meshData.Normals, meshData.Tangents, meshData.BiTangents);
                    }
                }
            }
            meshData.Mesh = mesh;
        }

        /// <summary>
        /// Checks if project is using HDRP render pipeline.
        /// </summary>
        /// <returns><c>true</c> when project is using HDRP render pipeline.</returns>
        protected virtual bool IsUsingHDRP()
        {
            return GraphicsSettings.renderPipelineAsset != null && GraphicsSettings.renderPipelineAsset.name == "HDRenderPipelineAsset";
        }

        /// <summary>
        /// Override this method to rename Material Property names when using different Rendering Pipelines or working with custom Materials.
        /// </summary>
        /// <param name="property">Material Property name.</param>
        /// <returns>Renamed Material Property name, when applicable. Otherwise, returns the original Property name.</returns>
        protected virtual string RemapMaterialProperty(string property)
        {
            if (IsUsingHDRP())
            {
                switch (property)
                {
                    case "_MainTex":
                        return "_BaseColorMap";
                    case "_BumpMap":
                        return "_NormalMap";
                    case "_EmissionMap":
                        return "_EmissiveColorMap";
                    case "_SpecGlossMap":
                        return "_SpecularColorMap";
                    case "_Displacement":
                        return "_HeightMap";
                    case "_Color":
                        return "_BaseColor";
                    case "_BumpScale":
                        return "_NormalScale";
                    case "_EmissionColor":
                        return "_EmissiveColor";
                    case "_SpecColor":
                        return "_SpecularColor";
                }
            }
            return property;
        }

        /// <summary>
        /// Transforms the given <see cref="TriLib.MaterialData"/> into a <see cref="UnityEngine.Material"/>.
        /// </summary>
        /// <param name="materialData"><see cref="TriLib.MaterialData"/> to be transformed.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to transform the material.</param>
        /// <param name="basePath">Loaded asset base path.</param>
        protected virtual void TransformMaterialData(MaterialData materialData, AssetLoaderOptions options, string basePath = null)
        {
            var dummy = false;
            var hasAlphaChannelOnTextures = false;

            var diffuseTexture = materialData.DiffuseInfoLoaded ? LoadTextureFromFile(materialData.DiffusePath, materialData.Name, options, materialData.DiffuseEmbeddedTextureData, materialData.DiffuseWrapMode, ref hasAlphaChannelOnTextures, false, options != null && (options.ScanForAlphaMaterials || options.ApplyAlphaMaterials)) : null;
            var emissionTexture = materialData.EmissionInfoLoaded ? LoadTextureFromFile(materialData.EmissionPath, materialData.Name, options, materialData.EmissionEmbeddedTextureData, materialData.EmissionWrapMode, ref dummy, false) : null;
            var specularTexture = materialData.SpecularInfoLoaded ? LoadTextureFromFile(materialData.SpecularPath, materialData.Name, options, materialData.SpecularEmbeddedTextureData, materialData.SpecularWrapMode, ref dummy, false) : null;
            var normalTexture = materialData.NormalInfoLoaded ? LoadTextureFromFile(materialData.NormalPath, materialData.Name, options, materialData.NormalEmbeddedTextureData, materialData.NormalWrapMode, ref dummy, true) : null;
            var heightTexture = materialData.HeightInfoLoaded ? LoadTextureFromFile(materialData.HeightPath, materialData.Name, options, materialData.HeightEmbeddedTextureData, materialData.HeightWrapMode, ref dummy, false) : null;
            var occlusionTexture = materialData.OcclusionInfoLoaded ? LoadTextureFromFile(materialData.OcclusionPath, materialData.Name, options, materialData.OcclusionEmbeddedTextureData, materialData.OcclusionWrapMode, ref dummy, false) : null;
            var metallicTexture = materialData.MetallicInfoLoaded ? LoadTextureFromFile(materialData.MetallicPath, materialData.Name, options, materialData.MetallicEmbeddedTextureData, materialData.MetallicWrapMode, ref dummy, false) : null;

            var hasAlpha = hasAlphaChannelOnTextures || materialData.AlphaLoaded && materialData.Alpha < 1f;
            var hasSpecular = materialData.SpecularColorLoaded || !string.IsNullOrEmpty(materialData.SpecularPath);

            var material = LoadMaterial(materialData.Name, options, hasAlpha, hasSpecular);
            if (options == null || options.ApplyDiffuseTexture)
            {
                material.SetTexture(RemapMaterialProperty("_MainTex"), diffuseTexture);
            }
            else
            {
                material.SetTexture(RemapMaterialProperty("_MainTex"), null);
            }
            if (options == null || options.ApplyEmissionTexture)
            {
                material.SetTexture(RemapMaterialProperty("_EmissionMap"), emissionTexture);
            }
            else
            {
                material.SetTexture(RemapMaterialProperty("_EmissionMap"), null);
            }
            if (options == null || options.ApplySpecularTexture)
            {
                material.SetTexture(RemapMaterialProperty("_SpecGlossMap"), specularTexture);
            }
            else
            {
                material.SetTexture(RemapMaterialProperty("_SpecGlossMap"), null);
            }
            if (options == null || options.ApplyNormalTexture)
            {
                material.SetTexture(RemapMaterialProperty("_BumpMap"), normalTexture);
            }
            else
            {
                material.SetTexture(RemapMaterialProperty("_BumpMap"), null);
            }
            if (options == null || options.ApplyDisplacementTexture)
            {
                material.SetTexture(RemapMaterialProperty("_Displacement"), heightTexture);
            }
            else
            {
                material.SetTexture(RemapMaterialProperty("_Displacement"), null);
            }
            if ((options == null || options.ApplyDiffuseColor) && materialData.DiffuseColorLoaded)
            {
                var color = materialData.DiffuseColor;
                if ((options == null || options.ApplyColorAlpha && !options.DisableAlphaMaterials) && materialData.AlphaLoaded)
                {
                    color.a = materialData.Alpha;
                }
                material.SetColor(RemapMaterialProperty("_Color"), color);
            }
            if ((options == null || options.ApplyEmissionColor) && materialData.EmissionColorLoaded)
            {
                material.SetColor(RemapMaterialProperty("_EmissionColor"), materialData.EmissionColor);
            }
            if ((options == null || options.ApplySpecularColor) && materialData.SpecularColorLoaded)
            {
                material.SetColor(RemapMaterialProperty("_SpecColor"), materialData.SpecularColor);
            }
            if ((options == null || options.ApplyNormalScale) && materialData.BumpScaleLoaded)
            {
                material.SetFloat(RemapMaterialProperty("_BumpScale"), materialData.BumpScale);
            }
            if (options == null || options.ApplyOcclusionTexture)
            {
                material.SetTexture(RemapMaterialProperty("_OcclusionMap"), occlusionTexture);
            }
            else
            {
                material.SetTexture(RemapMaterialProperty("_OcclusionMap"), null);
            }
            if (options == null || options.ApplyMetallicTexture)
            {
                material.SetTexture(RemapMaterialProperty("_MetallicGlossMap"), metallicTexture);
            }
            else
            {
                material.SetTexture(RemapMaterialProperty("_MetallicGlossMap"), null);
            }
            if ((options == null || options.ApplyGlossiness) && materialData.GlossinessLoaded)
            {
                material.SetFloat(RemapMaterialProperty("_Glossiness"), materialData.Glossiness);
            }
            if ((options == null || options.ApplyGlossinessScale) && materialData.GlossMapScaleLoaded)
            {
                material.SetFloat(RemapMaterialProperty("_GlossMapScale"), materialData.GlossMapScale);
            }
            materialData.Material = material;
        }

        /// <summary>
        /// Creates a new <see cref="UnityEngine.Material"/> or loads an existing <see cref="UnityEngine.Material"/>  with the given name.
        /// </summary>
        /// <param name="name">Material name.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to load the material.</param>
        /// <param name="hasAlpha">If <c>true</c>, creates/loads an alpha material.</param>
        /// <param name="hasSpecular">If <c>true</c>, creates/loads a specular material.</param>
        /// <returns>The created/loaded <see cref="UnityEngine.Material"/>.</returns>
        protected virtual Material LoadMaterial(string name, AssetLoaderOptions options, bool hasAlpha, bool hasSpecular)
        {
            Material material;
            if (LoadedMaterials.ContainsKey(name))
            {
                material = LoadedMaterials[name];
            }
            else
            {
                var materialShadingMode = options == null ? MaterialShadingMode.Standard : options.UseStandardSpecularMaterial ? MaterialShadingMode.Specular : options.MaterialShadingMode;
                var materialTransparencyMode = options == null ? MaterialTransparencyMode.Cutout : options.UseCutoutMaterials ? MaterialTransparencyMode.Cutout : options.MaterialTransparencyMode;
                if (options != null && !options.DisableAlphaMaterials && hasAlpha)
                {
                    switch (materialShadingMode)
                    {
                        case MaterialShadingMode.Roughness:
                            switch (materialTransparencyMode)
                            {
                                case MaterialTransparencyMode.Alpha:
                                    material = new Material(StandardRoughnessAlphaMaterial);
                                    break;
                                case MaterialTransparencyMode.Cutout:
                                    material = new Material(StandardRoughnessCutoutMaterial);
                                    break;
                                default:
                                    material = new Material(StandardRoughnessFadeMaterial);
                                    break;
                            }
                            break;
                        case MaterialShadingMode.Specular:
                            switch (materialTransparencyMode)
                            {
                                case MaterialTransparencyMode.Alpha:
                                    material = new Material(StandardSpecularAlphaMaterial);
                                    break;
                                case MaterialTransparencyMode.Cutout:
                                    material = new Material(StandardSpecularCutoutMaterial);
                                    break;
                                default:
                                    material = new Material(StandardSpecularFadeMaterial);
                                    break;
                            }
                            break;
                        default:
                            switch (materialTransparencyMode)
                            {
                                case MaterialTransparencyMode.Alpha:
                                    material = new Material(StandardBaseAlphaMaterial);
                                    break;
                                case MaterialTransparencyMode.Cutout:
                                    material = new Material(StandardBaseCutoutMaterial);
                                    break;
                                default:
                                    material = new Material(StandardBaseFadeMaterial);
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    switch (materialShadingMode)
                    {
                        case MaterialShadingMode.Roughness:
                            material = new Material(StandardRoughnessMaterial);
                            break;
                        case MaterialShadingMode.Specular:
                            material = new Material(StandardSpecularMaterial);
                            break;
                        default:
                            material = new Material(StandardBaseMaterial);
                            break;
                    }
                }
                material.name = name;
                LoadedMaterials.Add(name, material);
            }
            if (HasOnMaterialCreated)
            {
                TriggerOnMaterialCreated(0, false, material);
            }
            return material;
        }

        /// <summary>
        /// Creates a new <see cref="UnityEngine.Texture2D"/> or loads an existing <see cref="UnityEngine.Texture2D"/> with the given path.
        /// </summary>
        /// <param name="path">Path to load the texture from.</param>
        /// <param name="name">Texture name.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to load the texture.</param>
        /// <param name="embeddedTextureData"><see cref="TriLib.EmbeddedTextureData"/> to load the texture from.</param>
        /// <param name="textureWrapMode"><see cref="UnityEngine.TextureWrapMode"/> to assign to the texture.</param>
        /// <param name="hasAlphaChannel">Changed to <c>true</c> when the texture has alpha pixels.</param>
        /// <param name="isNormalMap">If <c>true</c>, applies special processing to the texture and treat it as a normal map.</param>
        /// <param name="checkAlphaChannel">If <c>true</c>, checks for any alpha pixel on loaded texture and assigns the value back to this variable.</param>
        /// <returns>The created/loaded <see cref="UnityEngine.Texture2D"/></returns>
        protected virtual Texture2D LoadTextureFromFile(string path, string name, AssetLoaderOptions options, EmbeddedTextureData embeddedTextureData, TextureWrapMode textureWrapMode, ref bool hasAlphaChannel, bool isNormalMap, bool checkAlphaChannel = false)
        {
            Texture2D texture = null;
            if (LoadedTextures.ContainsKey(path))
            {
                texture = LoadedTextures[path];
            }
            else if (embeddedTextureData != null)
            {
                if (!checkAlphaChannel)
                {
                    hasAlphaChannel = embeddedTextureData.NumChannels == 4;
                }
                texture = Texture2DUtils.ProcessTexture(
                    embeddedTextureData,
                    name,
                    ref hasAlphaChannel,
                    isNormalMap,
                    textureWrapMode,
                    options != null ? options.TextureFilterMode : FilterMode.Bilinear,
                    options != null ? options.TextureCompression : TextureCompression.NormalQuality,
                    checkAlphaChannel,
                    options == null || options.GenerateMipMaps
                    );
                if (texture != null)
                {
                    LoadedTextures.Add(path, texture);
                }
            }
            if (texture != null && HasOnTextureLoaded)
            {
                TriggerOnTextureLoaded(path, null, null, texture);
            }
            return texture;
        }

        /// <summary>
        /// Gets the default post process steps.
        /// </summary>
        /// <returns>The default post process steps.</returns>
        private static uint GetDefaultPostProcessSteps()
        {
            return (uint)(AssimpPostProcessSteps.FlipWindingOrder | AssimpPostProcessSteps.MakeLeftHanded | AssimpProcessPreset.TargetRealtimeMaxQuality);
        }

        /// <summary>
        /// Builds a property store used to pass advanced configs to native plugins.
        /// </summary>
        /// <param name="options">Input options.</param>
        /// <returns>The property store native pointer.</returns>
        private static IntPtr BuildPropertyStore(AssetLoaderOptions options)
        {
            var propertyStore = AssimpInterop.ai_CreatePropertyStore();
            foreach (var advancedConfig in options.AdvancedConfigs)
            {
                AssetAdvancedConfigType assetAdvancedConfigType;
                string className;
                string description;
                string group;
                bool hasDefaultValue;
                bool hasMinValue;
                bool hasMaxValue;
                object defaultValue;
                object minValue;
                object maxValue;
                AssetAdvancedPropertyMetadata.GetOptionMetadata(advancedConfig.Key, out assetAdvancedConfigType, out className, out description, out group, out defaultValue, out minValue, out maxValue, out hasDefaultValue, out hasMinValue, out hasMaxValue);
                switch (assetAdvancedConfigType)
                {
                    case AssetAdvancedConfigType.AiComponent:
                        AssimpInterop.ai_SetImportPropertyInteger(propertyStore, advancedConfig.Key, advancedConfig.IntValue << 1);
                        break;
                    case AssetAdvancedConfigType.AiPrimitiveType:
                        AssimpInterop.ai_SetImportPropertyInteger(propertyStore, advancedConfig.Key, advancedConfig.IntValue << 1);
                        break;
                    case AssetAdvancedConfigType.AiUVTransform:
                        AssimpInterop.ai_SetImportPropertyInteger(propertyStore, advancedConfig.Key, advancedConfig.IntValue << 1);
                        break;
                    case AssetAdvancedConfigType.Bool:
                        AssimpInterop.ai_SetImportPropertyInteger(propertyStore, advancedConfig.Key, advancedConfig.BoolValue ? 1 : 0);
                        break;
                    case AssetAdvancedConfigType.Integer:
                        AssimpInterop.ai_SetImportPropertyInteger(propertyStore, advancedConfig.Key, advancedConfig.IntValue);
                        break;
                    case AssetAdvancedConfigType.Float:
                        AssimpInterop.ai_SetImportPropertyFloat(propertyStore, advancedConfig.Key, advancedConfig.FloatValue);
                        break;
                    case AssetAdvancedConfigType.String:
                        AssimpInterop.ai_SetImportPropertyString(propertyStore, advancedConfig.Key, advancedConfig.StringValue);
                        break;
                    case AssetAdvancedConfigType.AiMatrix:
                        AssimpInterop.ai_SetImportPropertyMatrix(propertyStore, advancedConfig.Key, advancedConfig.TranslationValue, advancedConfig.RotationValue, advancedConfig.ScaleValue);
                        break;
                }
            }
            return propertyStore;
        }

        /// <summary>
        /// Imports the file based on given options and returns the Assimp scene native pointer.
        /// </summary>
        /// <param name="fileBytes">File data used to load the <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="fileHint">File format hint. Eg: ".fbx".</param>
        /// <param name="dataCallback">Callback used to retrieve file data from C#.</param>
        /// <param name="existsCallback">Callback used to retrieve file size from C#.</param>
        /// <param name="fileId">Generated <see cref="FileLoadData"/> id.</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        /// <param name="progressCallback">Callback used to retrieve file loading percentage.</param>
        /// <returns>Assimp scene pointer.</returns>
        private static IntPtr ImportFileFromMemory(byte[] fileBytes, string fileHint, AssetLoaderOptions options, AssimpInterop.DataCallback dataCallback, AssimpInterop.ExistsCallback existsCallback, int fileId, AssimpInterop.ProgressCallback progressCallback)
        {
            IntPtr scene;
            if (options != null && options.AdvancedConfigs != null)
            {
                var propertyStore = BuildPropertyStore(options);
                scene = AssimpInterop.ai_ImportFileFromMemoryWithProperties(fileBytes, (uint)options.PostProcessSteps, fileHint, propertyStore, dataCallback ?? DefaultDataCallback, existsCallback ?? DefaultExistsCallback, fileId, progressCallback);
                AssimpInterop.ai_CreateReleasePropertyStore(propertyStore);
            }
            else
            {
                scene = AssimpInterop.ai_ImportFileFromMemory(fileBytes, options == null ? GetDefaultPostProcessSteps() : (uint)options.PostProcessSteps, fileHint, dataCallback ?? DefaultDataCallback, existsCallback ?? DefaultExistsCallback, fileId, progressCallback);
            }
            return scene;
        }

        /// <summary>
        /// Imports the file based on given options and returns the Assimp scene native pointer.
        /// </summary>
        /// <param name="filename">Filename used to load the <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        /// <param name="progressCallback">Callback used to retrieve file loading percentage.</param>
        /// <returns>Assimp scene pointer.</returns>
        private static IntPtr ImportFile(string filename, AssetLoaderOptions options, AssimpInterop.ProgressCallback progressCallback)
        {
            IntPtr scene;
            if (options != null && options.AdvancedConfigs != null)
            {
                var propertyStore = BuildPropertyStore(options);
                scene = AssimpInterop.ai_ImportFileEx(filename, (uint)options.PostProcessSteps, IntPtr.Zero, propertyStore, progressCallback ?? DefaultProgressCallback);
                AssimpInterop.ai_CreateReleasePropertyStore(propertyStore);
            }
            else
            {
                scene = AssimpInterop.ai_ImportFile(filename, options == null ? GetDefaultPostProcessSteps() : (uint)options.PostProcessSteps, progressCallback ?? DefaultProgressCallback);
            }
            return scene;
        }

        /// <summary>
        /// Processes the importing and creates the internal data representation.
        /// </summary>
        /// <param name="basePath">Base model path.</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        /// <param name="usesWrapperGameObject">Pass <c>true</c> when using a wrapper <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="loadTextureDataCallback">Pass this callback to load texture data from custom sources.</param>
        /// <param name="filename">Source filename.</param>
        private void LoadInternal(string basePath, AssetLoaderOptions options, bool usesWrapperGameObject = false, LoadTextureDataCallback loadTextureDataCallback = null, string filename = null)
        {
            Metadata = BuildMetadata(IntPtr.Zero);
            if (AssimpInterop.aiScene_HasMaterials(Scene) && (options == null || !options.DontLoadMaterials))
            {
                MaterialData = new MaterialData[AssimpInterop.aiScene_GetNumMaterials(Scene)];
                EmbeddedTextures = new Dictionary<string, EmbeddedTextureData>();
                BuildMaterials(basePath, options, loadTextureDataCallback);
            }
            if (AssimpInterop.aiScene_HasMeshes(Scene) && (options == null || !options.DontLoadMeshes))
            {
                MeshData = new MeshData[AssimpInterop.aiScene_GetNumMeshes(Scene)];
                BuildMeshes();
                BuildBones();
            }
            if (AssimpInterop.aiScene_HasAnimation(Scene) && (options == null || !options.DontLoadAnimations))
            {
                AnimationData = new AnimationData[AssimpInterop.aiScene_GetNumAnimations(Scene)];
                BuildAnimations(options);
            }
            if (AssimpInterop.aiScene_HasCameras(Scene) && (options == null || !options.DontLoadCameras))
            {
                CameraData = new CameraData[AssimpInterop.aiScene_GetNumCameras(Scene)];
                BuildCameras();
            }
            var shortFileName = filename != null ? FileUtils.GetFilenameWithoutExtension(filename) : null;
            BuildObjects(options, usesWrapperGameObject, shortFileName);
        }

        /// <summary>
        /// Builds the root <see cref="TriLib.NodeData"/>.
        /// </summary>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to process the <see cref="TriLib.NodeData"/>.</param>
        /// <param name="usesWrapperGameObject">Pass <c>true</c> when using a wrapper <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="fileShortName">Short source file name.</param>
        private void BuildObjects(AssetLoaderOptions options, bool usesWrapperGameObject = false, string fileShortName = null)
        {
            NodesPath = new Dictionary<string, string>();
            var rootNode = AssimpInterop.aiScene_GetRootNode(Scene);
            RootNodeData = BuildObject(RootNodeData, rootNode, options, usesWrapperGameObject);
            if (fileShortName != null)
            {
                RootNodeData.Name = RootNodeData.Name.Replace(AssimpFilenameMagicString, fileShortName);
            }
        }

        /// <summary>
        /// Builds the a new <see cref="TriLib.NodeData"/> from the given Assimp node pointer.
        /// </summary>
        /// <param name="parentNodeData">Parent <see cref="TriLib.NodeData"/>, if exists.</param>
        /// <param name="node">Assimp node pointer.</param>
        /// <param name="options"><see cref="TriLib.AssetLoaderOptions"/> used to process the <see cref="TriLib.NodeData"/>.</param>
        /// <param name="usesWrapperGameObject">Pass <c>true</c> when using a wrapper <see cref="UnityEngine.GameObject"/>.</param>
        /// <returns>The built <see cref="TriLib.NodeData"/>.</returns>
        private NodeData BuildObject(NodeData parentNodeData, IntPtr node, AssetLoaderOptions options, bool usesWrapperGameObject = false)
        {
            var nodeId = NodeId++;
            var nodeName = AssimpInterop.aiNode_GetName(node);
            var fixedNodeName = FixNodeName(nodeName, nodeId);
            var matrix = AssimpInterop.aiNode_GetTransformation(node);
            var nodePath = parentNodeData != null ? string.Format(parentNodeData.Path != null ? "{0}/{1}" : "{1}", parentNodeData.Path, nodeName) : usesWrapperGameObject ? string.Format("{0}", nodeName) : null;
            NodesPath.Add(fixedNodeName, nodePath);
            var meshCount = AssimpInterop.aiNode_GetNumMeshes(node);
            var nodeData = new NodeData
            {
                Name = fixedNodeName,
                Path = nodePath,
                Matrix = matrix,
                Meshes = new uint[meshCount]
            };
            nodeData.Metadata = BuildMetadata(node);
            for (uint m = 0; m < meshCount; m++)
            {
                var meshIndex = AssimpInterop.aiNode_GetMeshIndex(node, m);
                nodeData.Meshes[m] = meshIndex;
            }
            var childrenCount = AssimpInterop.aiNode_GetNumChildren(node);
            if (childrenCount > 0)
            {
                nodeData.Children = new NodeData[childrenCount];
                for (uint c = 0; c < childrenCount; c++)
                {
                    var childNode = AssimpInterop.aiNode_GetChildren(node, c);
                    var childNodeData = BuildObject(nodeData, childNode, options, usesWrapperGameObject);
                    childNodeData.Parent = nodeData;
                    nodeData.Children[c] = childNodeData;
                }
            }
            return nodeData;
        }

        /// <summary>
        /// Builds the <see cref="TriLib.AssimpMetadata"/> list for given node.
        /// </summary>
        /// <param name="node">The Assimp node pointer to load metadata from. Or -1 to load scene metadata.</param>
        /// <returns>The built metadata array.</returns>
        private AssimpMetadata[] BuildMetadata(IntPtr node)
        {
            if (!HasOnMetadataProcessed)
            {
                return null;
            }
            var metadataCount = node == IntPtr.Zero ? AssimpInterop.aiScene_GetMetadataCount(Scene) : AssimpInterop.aiNode_GetMetadataCount(node);
            var metadata = new AssimpMetadata[metadataCount];
            for (uint i = 0; i < metadataCount; i++)
            {
                var metadataKey = node == IntPtr.Zero ? AssimpInterop.aiScene_GetMetadataKey(Scene, i) : AssimpInterop.aiNode_GetMetadataKey(node, i);
                var metadataType = node == IntPtr.Zero ? AssimpInterop.aiScene_GetMetadataType(Scene, i) : AssimpInterop.aiNode_GetMetadataType(node, i);
                object metadataValue;
                switch (metadataType)
                {
                    case AssimpMetadataType.AI_BOOL:
                        metadataValue = node == IntPtr.Zero ? AssimpInterop.aiScene_GetMetadataBoolValue(Scene, i) : AssimpInterop.aiNode_GetMetadataBoolValue(node, i);
                        break;
                    case AssimpMetadataType.AI_INT32:
                        metadataValue = node == IntPtr.Zero ? AssimpInterop.aiScene_GetMetadataInt32Value(Scene, i) : AssimpInterop.aiNode_GetMetadataInt32Value(node, i);
                        break;
                    case AssimpMetadataType.AI_UINT64:
                        metadataValue = node == IntPtr.Zero ? AssimpInterop.aiScene_GetMetadataInt64Value(Scene, i) : AssimpInterop.aiNode_GetMetadataInt64Value(node, i);
                        break;
                    case AssimpMetadataType.AI_FLOAT:
                        metadataValue = node == IntPtr.Zero ? AssimpInterop.aiScene_GetMetadataFloatValue(Scene, i) : AssimpInterop.aiNode_GetMetadataFloatValue(node, i);
                        break;
                    case AssimpMetadataType.AI_DOUBLE:
                        metadataValue = node == IntPtr.Zero ? AssimpInterop.aiScene_GetMetadataDoubleValue(Scene, i) : AssimpInterop.aiNode_GetMetadataDoubleValue(node, i);
                        break;
                    case AssimpMetadataType.AI_AIVECTOR3D:
                        metadataValue = node == IntPtr.Zero ? AssimpInterop.aiScene_GetMetadataVectorValue(Scene, i) : AssimpInterop.aiNode_GetMetadataVectorValue(node, i);
                        break;
                    default:
                        metadataValue = node == IntPtr.Zero ? AssimpInterop.aiScene_GetMetadataStringValue(Scene, i) : AssimpInterop.aiNode_GetMetadataStringValue(node, i);
                        break;
                }
                metadata[i] = new AssimpMetadata(metadataType, i, metadataKey, metadataValue);
            }
            return metadata;
        }

        /// <summary>
        /// Builds the <see cref="TriLib.MeshData"/> list for given scene.
        /// </summary>
        private void BuildMeshes()
        {
            var meshCount = AssimpInterop.aiScene_GetNumMeshes(Scene);
            for (uint m = 0; m < meshCount; m++)
            {
                var meshData = new MeshData();
                var mesh = AssimpInterop.aiScene_GetMesh(Scene, m);
                var meshName = AssimpInterop.aiMesh_GetName(mesh);
                meshData.Name = FixName(meshName, m);
                var materialIndex = AssimpInterop.aiMesh_GetMatrialIndex(mesh);
                meshData.MaterialIndex = materialIndex;
                var vertexCount = AssimpInterop.aiMesh_VertexCount(mesh);
                var hasNormals = AssimpInterop.aiMesh_HasNormals(mesh);
                if (hasNormals)
                {
                    meshData.Normals = new Vector3[vertexCount];
                }
                var hasTangentsAndBitangents = AssimpInterop.aiMesh_HasTangentsAndBitangents(mesh);
                if (hasTangentsAndBitangents)
                {
                    meshData.Tangents = new Vector4[vertexCount];
                    meshData.BiTangents = new Vector4[vertexCount];
                }
                var hasTextureCoords0 = AssimpInterop.aiMesh_HasTextureCoords(mesh, 0);
                if (hasTextureCoords0)
                {
                    meshData.Uv = new Vector2[vertexCount];
                }
                var hasTextureCoords1 = AssimpInterop.aiMesh_HasTextureCoords(mesh, 1);
                if (hasTextureCoords1)
                {
                    meshData.Uv1 = new Vector2[vertexCount];
                }
                var hasTextureCoords2 = AssimpInterop.aiMesh_HasTextureCoords(mesh, 2);
                if (hasTextureCoords2)
                {
                    meshData.Uv2 = new Vector2[vertexCount];
                }
                var hasTextureCoords3 = AssimpInterop.aiMesh_HasTextureCoords(mesh, 3);
                if (hasTextureCoords3)
                {
                    meshData.Uv3 = new Vector2[vertexCount];
                }
                var hasVertexColors = AssimpInterop.aiMesh_HasVertexColors(mesh, 0);
                if (hasVertexColors)
                {
                    meshData.Colors = new Color[vertexCount];
                }
                meshData.Vertices = new Vector3[vertexCount];
                for (uint v = 0; v < vertexCount; v++)
                {
                    meshData.Vertices[v] = AssimpInterop.aiMesh_GetVertex(mesh, v);
                    if (hasNormals)
                    {
                        meshData.Normals[v] = AssimpInterop.aiMesh_GetNormal(mesh, v);
                    }
                    if (hasTangentsAndBitangents)
                    {
                        meshData.Tangents[v] = AssimpInterop.aiMesh_GetTangent(mesh, v);
                        meshData.BiTangents[v] = AssimpInterop.aiMesh_GetBitangent(mesh, v);
                    }
                    if (hasTextureCoords0)
                    {
                        meshData.Uv[v] = AssimpInterop.aiMesh_GetTextureCoord(mesh, 0, v);
                    }
                    if (hasTextureCoords1)
                    {
                        meshData.Uv1[v] = AssimpInterop.aiMesh_GetTextureCoord(mesh, 1, v);
                    }
                    if (hasTextureCoords2)
                    {
                        meshData.Uv2[v] = AssimpInterop.aiMesh_GetTextureCoord(mesh, 2, v);
                    }
                    if (hasTextureCoords3)
                    {
                        meshData.Uv3[v] = AssimpInterop.aiMesh_GetTextureCoord(mesh, 3, v);
                    }
                    if (hasVertexColors)
                    {
                        meshData.Colors[v] = AssimpInterop.aiMesh_GetVertexColor(mesh, 0, v);
                    }
                }
                if (AssimpInterop.aiMesh_HasFaces(mesh))
                {
                    var facesCount = AssimpInterop.aiMesh_GetNumFaces(mesh);
                    meshData.Triangles = new int[facesCount * 3];
                    for (uint f = 0; f < facesCount; f++)
                    {
                        var face = AssimpInterop.aiMesh_GetFace(mesh, f);
                        var indexCount = AssimpInterop.aiFace_GetNumIndices(face);
                        if (indexCount > 3)
                        {
                            throw new UnityException("More than three face indices is not supported. Please enable \"Triangulate\" in your \"AssetLoaderOptions\" \"PostProcessSteps\" field");
                        }
                        for (uint i = 0; i < indexCount; i++)
                        {
                            meshData.Triangles[f * 3 + i] = (int)AssimpInterop.aiFace_GetIndex(face, i);
                        }
                    }
                }
                MorphData[] morphsData;
                var morphMeshCount = AssimpInterop.aiMesh_GetAnimMeshCount(mesh);
                if (morphMeshCount > 0)
                {
                    HasBlendShapes = true;
                    morphsData = new MorphData[morphMeshCount];
                    for (uint i = 0; i < morphMeshCount; i++)
                    {
                        var animMesh = AssimpInterop.aiMesh_GetAnimMesh(mesh, i);
                        var animVertexCount = AssimpInterop.aiAnimMesh_GetVerticesCount(animMesh);
                        Vector3[] vertices;
                        if (AssimpInterop.aiAnimMesh_HasPositions(animMesh) && meshData.Vertices != null)
                        {
                            vertices = new Vector3[animVertexCount];
                            for (uint j = 0; j < animVertexCount; j++)
                            {
                                var animValue = AssimpInterop.aiAnimMesh_GetVertex(animMesh, j);
                                var originalValue = j < meshData.Vertices.Length ? meshData.Vertices[j] : Vector3.zero;
                                var offset = animValue - originalValue;
                                vertices[j] = offset;
                            }
                        }
                        else
                        {
                            vertices = null;
                        }
                        Vector3[] normals;
                        if (AssimpInterop.aiAnimMesh_HasNormals(animMesh) && meshData.Normals != null)
                        {
                            normals = new Vector3[animVertexCount];
                            for (uint j = 0; j < animVertexCount; j++)
                            {
                                var animValue = AssimpInterop.aiAnimMesh_GetNormal(animMesh, j);
                                var originalValue = j < meshData.Normals.Length ? meshData.Normals[j] : Vector3.zero;
                                var offset = animValue - originalValue;
                                normals[j] = offset;
                            }
                        }
                        else
                        {
                            normals = null;
                        }
                        Vector3[] tangents;
                        if (AssimpInterop.aiAnimMesh_HasTangentsAndBitangents(animMesh) && meshData.Tangents != null)
                        {
                            tangents = new Vector3[animVertexCount];
                            for (uint j = 0; j < animVertexCount; j++)
                            {
                                var animValue = AssimpInterop.aiAnimMesh_GetTangent(animMesh, j);
                                var originalValue = j < meshData.Tangents.Length ? meshData.Tangents[j] : Vector4.zero;
                                var offset = new Vector4(animValue.x, animValue.y, animValue.z) - originalValue;
                                tangents[j] = offset;
                            }
                        }
                        else
                        {
                            tangents = null;
                        }
                        var morphData = new MorphData();
                        morphData.Name = AssimpInterop.aiAnimMesh_GetName(animMesh);
                        morphData.Vertices = vertices;
                        morphData.Normals = normals;
                        morphData.Tangents = tangents;
                        morphData.Weight = AssimpInterop.aiAnimMesh_GetWeight(animMesh);
                        morphsData[i] = morphData;
                    }
                }
                else
                {
                    morphsData = null;
                }
                meshData.MorphsData = morphsData;
                MeshData[m] = meshData;
            }
        }

        /// <summary>
        /// Builds the <see cref="TriLib.CameraData"/> list for given scene.
        /// </summary>
        private void BuildCameras()
        {
            for (uint c = 0; c < AssimpInterop.aiScene_GetNumCameras(Scene); c++)
            {
                var camera = AssimpInterop.aiScene_GetCamera(Scene, c);
                var cameraName = AssimpInterop.aiCamera_GetName(camera);
                var cameraData = new CameraData
                {
                    Name = cameraName,
                    Aspect = AssimpInterop.aiCamera_GetAspect(camera),
                    NearClipPlane = AssimpInterop.aiCamera_GetClipPlaneNear(camera),
                    FarClipPlane = AssimpInterop.aiCamera_GetClipPlaneFar(camera),
                    FieldOfView = AssimpInterop.aiCamera_GetHorizontalFOV(camera),
                    LocalPosition = AssimpInterop.aiCamera_GetPosition(camera),
                    Forward = AssimpInterop.aiCamera_GetLookAt(camera),
                    Up = AssimpInterop.aiCamera_GetUp(camera)
                };
                CameraData[c] = cameraData;
            }
        }

        /// <summary>
        /// Builds the <see cref="TriLib.MaterialData"/> list for given scene.
        /// </summary>
        /// <param name="basePath">Base model path.</param>
        /// <param name="options">Options used to load the material.</param>
        /// <param name="loadTextureDataCallback">Pass this callback to load texture data from custom sources.</param>
        protected virtual void BuildMaterials(string basePath, AssetLoaderOptions options, LoadTextureDataCallback loadTextureDataCallback = null)
        {
            for (uint m = 0; m < AssimpInterop.aiScene_GetNumMaterials(Scene); m++)
            {
                var materialData = new MaterialData();
                var material = AssimpInterop.aiScene_GetMaterial(Scene, m);
                if (options != null && options.LoadRawMaterialProperties)
                {
                    var numProperties = AssimpInterop.aiMaterial_GetNumProperties(material);
                    if (numProperties > 0)
                    {
                        materialData.Properties = new IMaterialProperty[numProperties];
                        for (uint p = 0; p < numProperties; p++)
                        {
                            var assimpMaterialProperty = AssimpInterop.aiMaterial_GetProperty(material, p);
                            var propertyName = AssimpInterop.aiMaterialProperty_GetKey(assimpMaterialProperty);
                            var propertyType = AssimpInterop.aiMaterialProperty_GetType(assimpMaterialProperty);
                            var propertyIndex = AssimpInterop.aiMaterialProperty_GetIndex(assimpMaterialProperty);
                            var propertySemantic = AssimpInterop.aiMaterialProperty_GetSemantic(assimpMaterialProperty);
                            var propertyDataSize = AssimpInterop.aiMaterialProperty_GetDataSize(assimpMaterialProperty);
                            var propertyDataPointer = AssimpInterop.aiMaterialProperty_GetDataPointer(assimpMaterialProperty);
                            IMaterialProperty materialProperty;
                            switch (propertyType)
                            {
                                case aiPropertyTypeInfo.aiPTI_Float:
                                    var floatDataSize = (int)(propertyDataSize / 4);
                                    if (floatDataSize == 1)
                                    {
                                        var floatData = AssimpInterop.GetNewFloat(propertyDataPointer);
                                        materialProperty = new MaterialProperty<float>(propertyName, floatData, propertyIndex, propertySemantic);
                                    }
                                    else
                                    {
                                        var floatData = AssimpInterop.ReadFloatArray(propertyDataPointer, floatDataSize);
                                        materialProperty = new MaterialProperty<float[]>(propertyName, floatData, propertyIndex, propertySemantic);
                                    }
                                    break;
                                case aiPropertyTypeInfo.aiPTI_Double:
                                    var doubleDataSize = (int)(propertyDataSize / 8);
                                    if (doubleDataSize == 1)
                                    {
                                        var doubleData = AssimpInterop.GetNewDouble(propertyDataPointer);
                                        materialProperty = new MaterialProperty<double>(propertyName, doubleData, propertyIndex, propertySemantic);
                                    }
                                    else
                                    {
                                        var doubleData = AssimpInterop.ReadDoubleArray(propertyDataPointer, doubleDataSize);
                                        materialProperty = new MaterialProperty<double[]>(propertyName, doubleData, propertyIndex, propertySemantic);
                                    }
                                    break;
                                case aiPropertyTypeInfo.aiPTI_String:
                                    string stringData;
                                    AssimpInterop.aiMaterial_GetString(material, propertyName, propertySemantic, propertyIndex, out stringData);
                                    materialProperty = new MaterialProperty<string>(propertyName, stringData, propertyIndex, propertySemantic);
                                    break;
                                case aiPropertyTypeInfo.aiPTI_Integer:
                                    var intDataSize = (int)(propertyDataSize / 4);
                                    if (intDataSize == 1)
                                    {
                                        var intData = AssimpInterop.GetNewInt32(propertyDataPointer);
                                        materialProperty = new MaterialProperty<int>(propertyName, intData, propertyIndex, propertySemantic);
                                    }
                                    else
                                    {
                                        var intData = AssimpInterop.ReadIntArray(propertyDataPointer, intDataSize);
                                        materialProperty = new MaterialProperty<int[]>(propertyName, intData, propertyIndex, propertySemantic);
                                    }
                                    break;
                                default:
                                    var byteDataSize = (int)propertyDataSize;
                                    if (byteDataSize == 1)
                                    {
                                        var byteData = AssimpInterop.GetNewByte(propertyDataPointer);
                                        materialProperty = new MaterialProperty<byte>(propertyName, byteData, propertyIndex, propertySemantic);
                                    }
                                    else
                                    {
                                        var byteData = AssimpInterop.ReadByteArray(propertyDataPointer, byteDataSize);
                                        materialProperty = new MaterialProperty<byte[]>(propertyName, byteData, propertyIndex, propertySemantic);
                                    }
                                    break;
                            }
                            materialData.Properties[p] = materialProperty;
                        }
                    }
                }
                string materialName = null;
                if (AssimpInterop.aiMaterial_HasName(material))
                {
                    if (!AssimpInterop.aiMaterial_GetName(material, out materialName))
                    {
#if TRILIB_OUTPUT_MESSAGES
                        Debug.LogWarning("Error loading material name");
#endif
                    }
                }
                materialName = FixName(materialName, m);
                materialData.Name = materialName;
                var alphaLoaded = false;
                if (AssimpInterop.aiMaterial_HasOpacity(material))
                {
                    float tmpAlpha;
                    if (AssimpInterop.aiMaterial_GetOpacity(material, out tmpAlpha))
                    {
                        materialData.Alpha = tmpAlpha;
                        alphaLoaded = true;
                    }
                }
                materialData.AlphaLoaded = alphaLoaded;

                var diffuseInfoLoaded = false;
                var numDiffuse = AssimpInterop.aiMaterial_GetNumTextureDiffuse(material);
                if (numDiffuse > 0)
                {
                    string path;
                    uint textureMapping;
                    uint uvIndex;
                    float blendMode;
                    uint op;
                    uint mapMode;
                    if (AssimpInterop.aiMaterial_GetTextureDiffuse(material, 0, out path, out textureMapping,
                        out uvIndex,
                        out blendMode, out op, out mapMode))
                    {
                        var wrapMode = mapMode == (uint)TextureWrapMode.Clamp
                            ? TextureWrapMode.Clamp
                            : TextureWrapMode.Repeat;
                        var textureName = StringUtils.GenerateUniqueName(path);
                        materialData.DiffusePath = path;
                        materialData.DiffuseWrapMode = wrapMode;
                        materialData.DiffuseName = textureName;
                        materialData.DiffuseBlendMode = blendMode;
                        materialData.DiffuseOp = op;
                        diffuseInfoLoaded = true;
                        EmbeddedTextureData embeddedTextureData;
                        if (EmbeddedTextureLoad != null)
                        {
                            embeddedTextureData = EmbeddedTextureLoad.Invoke(path);
                        }
                        else
                        {
                            var texture = AssimpInterop.aiScene_GetEmbeddedTexture(Scene, path);
                            if (texture != IntPtr.Zero)
                            {
                                embeddedTextureData = LoadEmbeddedTextureData(texture, path);
                            }
                            else
                            {
                                embeddedTextureData = loadTextureDataCallback == null ? LoadTextureData(path, basePath) : loadTextureDataCallback(path, basePath);
                            }
                        }
                        materialData.DiffuseEmbeddedTextureData = embeddedTextureData;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarningFormat("Error loading diffuse texture {0}", m);
                    }
#endif
                }
                materialData.DiffuseInfoLoaded = diffuseInfoLoaded;

                var diffuseColorLoaded = false;
                if (AssimpInterop.aiMaterial_HasDiffuse(material))
                {
                    Color colorDiffuse;
                    if (AssimpInterop.aiMaterial_GetDiffuse(material, out colorDiffuse))
                    {
                        materialData.DiffuseColor = colorDiffuse;
                        diffuseColorLoaded = true;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarning("Error loading diffuse color");
                    }
#endif
                }
                materialData.DiffuseColorLoaded = diffuseColorLoaded;

                var emissionColorLoaded = false;
                var hasEmissive = AssimpInterop.aiMaterial_HasEmissive(material);
                if (hasEmissive)
                {
                    Color colorEmissive;
                    if (AssimpInterop.aiMaterial_GetEmissive(material, out colorEmissive))
                    {
                        materialData.EmissionColor = colorEmissive;
                        emissionColorLoaded = true;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarning("Error loading emissive color");
                    }
#endif
                }
                materialData.EmissionColorLoaded = emissionColorLoaded;

                var emissiveInfoLoaded = false;
                var numEmissive = AssimpInterop.aiMaterial_GetNumTextureEmissive(material);
                if (numEmissive > 0)
                {
                    string path;
                    uint textureMapping;
                    uint uvIndex;
                    float blendMode;
                    uint op;
                    uint mapMode;
                    if (AssimpInterop.aiMaterial_GetTextureEmissive(material, 0, out path, out textureMapping,
                        out uvIndex,
                        out blendMode, out op, out mapMode))
                    {
                        var wrapMode = mapMode == (uint)TextureWrapMode.Clamp
                            ? TextureWrapMode.Clamp
                            : TextureWrapMode.Repeat;
                        var textureName = StringUtils.GenerateUniqueName(path);
                        materialData.EmissionPath = path;
                        materialData.EmissionWrapMode = wrapMode;
                        materialData.EmissionName = textureName;
                        materialData.EmissionBlendMode = blendMode;
                        materialData.EmissionOp = op;
                        emissiveInfoLoaded = true;
                        EmbeddedTextureData embeddedTextureData;
                        if (EmbeddedTextureLoad != null)
                        {
                            embeddedTextureData = EmbeddedTextureLoad.Invoke(path);
                        }
                        else
                        {
                            var texture = AssimpInterop.aiScene_GetEmbeddedTexture(Scene, path);
                            if (texture != IntPtr.Zero)
                            {
                                embeddedTextureData = LoadEmbeddedTextureData(texture, path);
                            }
                            else
                            {
                                embeddedTextureData = loadTextureDataCallback == null ? LoadTextureData(path, basePath) : loadTextureDataCallback(path, basePath);
                            }
                        }
                        materialData.EmissionEmbeddedTextureData = embeddedTextureData;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarning("Error loading emissive texture");
                    }
#endif
                }
                materialData.EmissionInfoLoaded = emissiveInfoLoaded;

                var specColorLoaded = false;
                var hasSpecular = AssimpInterop.aiMaterial_HasSpecular(material);
                if (hasSpecular)
                {
                    Color colorSpecular;
                    if (AssimpInterop.aiMaterial_GetSpecular(material, out colorSpecular))
                    {
                        materialData.SpecularColor = colorSpecular;
                        specColorLoaded = true;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarning("Error loading specular color");
                    }
#endif
                }
                materialData.SpecularColorLoaded = specColorLoaded;

                var specularInfoLoaded = false;
                var numSpecular = AssimpInterop.aiMaterial_GetNumTextureSpecular(material);
                if (numSpecular > 0)
                {
                    string path;
                    uint textureMapping;
                    uint uvIndex;
                    float blendMode;
                    uint op;
                    uint mapMode;
                    if (AssimpInterop.aiMaterial_GetTextureSpecular(material, 0, out path, out textureMapping,
                        out uvIndex,
                        out blendMode, out op, out mapMode))
                    {
                        var wrapMode = mapMode == (uint)TextureWrapMode.Clamp
                            ? TextureWrapMode.Clamp
                            : TextureWrapMode.Repeat;
                        var textureName = StringUtils.GenerateUniqueName(path);
                        materialData.SpecularPath = path;
                        materialData.SpecularWrapMode = wrapMode;
                        materialData.SpecularName = textureName;
                        materialData.SpecularBlendMode = blendMode;
                        materialData.SpecularOp = op;
                        specularInfoLoaded = true;
                        EmbeddedTextureData embeddedTextureData;
                        if (EmbeddedTextureLoad != null)
                        {
                            embeddedTextureData = EmbeddedTextureLoad.Invoke(path);
                        }
                        else
                        {
                            var texture = AssimpInterop.aiScene_GetEmbeddedTexture(Scene, path);
                            if (texture != IntPtr.Zero)
                            {
                                embeddedTextureData = LoadEmbeddedTextureData(texture, path);
                            }
                            else
                            {
                                embeddedTextureData = loadTextureDataCallback == null ? LoadTextureData(path, basePath) : loadTextureDataCallback(path, basePath);
                            }
                        }
                        materialData.SpecularEmbeddedTextureData = embeddedTextureData;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarning("Error loading specular texture");
                    }
#endif
                }
                materialData.SpecularInfoLoaded = specularInfoLoaded;

                var normalInfoLoaded = false;
                var numNormals = AssimpInterop.aiMaterial_GetNumTextureNormals(material);
                if (numNormals > 0)
                {
                    string path;
                    uint textureMapping;
                    uint uvIndex;
                    float blendMode;
                    uint op;
                    uint mapMode;
                    if (AssimpInterop.aiMaterial_GetTextureNormals(material, 0, out path, out textureMapping,
                        out uvIndex,
                        out blendMode, out op, out mapMode))
                    {
                        var wrapMode = mapMode == (uint)TextureWrapMode.Clamp
                            ? TextureWrapMode.Clamp
                            : TextureWrapMode.Repeat;
                        var textureName = StringUtils.GenerateUniqueName(path);
                        materialData.NormalPath = path;
                        materialData.NormalWrapMode = wrapMode;
                        materialData.NormalName = textureName;
                        materialData.NormalBlendMode = blendMode;
                        materialData.NormalOp = op;
                        normalInfoLoaded = true;
                        EmbeddedTextureData embeddedTextureData;
                        if (EmbeddedTextureLoad != null)
                        {
                            embeddedTextureData = EmbeddedTextureLoad.Invoke(path);
                        }
                        else
                        {
                            var texture = AssimpInterop.aiScene_GetEmbeddedTexture(Scene, path);
                            if (texture != IntPtr.Zero)
                            {
                                embeddedTextureData = LoadEmbeddedTextureData(texture, path);
                            }
                            else
                            {
                                embeddedTextureData = loadTextureDataCallback == null ? LoadTextureData(path, basePath) : loadTextureDataCallback(path, basePath);
                            }
                        }
                        materialData.NormalEmbeddedTextureData = embeddedTextureData;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarning("Error loading normals texture");
                    }
#endif
                }
                materialData.NormalInfoLoaded = normalInfoLoaded;

                var heightInfoLoaded = false;
                var numHeight = AssimpInterop.aiMaterial_GetNumTextureHeight(material);
                if (numHeight > 0)
                {
                    string path;
                    uint textureMapping;
                    uint uvIndex;
                    float blendMode;
                    uint op;
                    uint mapMode;
                    if (AssimpInterop.aiMaterial_GetTextureHeight(material, 0, out path, out textureMapping,
                        out uvIndex,
                        out blendMode, out op, out mapMode))
                    {
                        var wrapMode = mapMode == (uint)TextureWrapMode.Clamp
                            ? TextureWrapMode.Clamp
                            : TextureWrapMode.Repeat;
                        var textureName = StringUtils.GenerateUniqueName(path);
                        materialData.HeightPath = path;
                        materialData.HeightWrapMode = wrapMode;
                        materialData.HeightName = textureName;
                        materialData.HeightBlendMode = blendMode;
                        materialData.HeightOp = op;
                        heightInfoLoaded = true;
                        EmbeddedTextureData embeddedTextureData;
                        if (EmbeddedTextureLoad != null)
                        {
                            embeddedTextureData = EmbeddedTextureLoad.Invoke(path);
                        }
                        else
                        {
                            var texture = AssimpInterop.aiScene_GetEmbeddedTexture(Scene, path);
                            if (texture != IntPtr.Zero)
                            {
                                embeddedTextureData = LoadEmbeddedTextureData(texture, path);
                            }
                            else
                            {
                                embeddedTextureData = loadTextureDataCallback == null ? LoadTextureData(path, basePath) : loadTextureDataCallback(path, basePath);
                            }
                        }
                        materialData.HeightEmbeddedTextureData = embeddedTextureData;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarning("Error loading height texture");
                    }
#endif
                }
                materialData.HeightInfoLoaded = heightInfoLoaded;

                var occlusionInfoLoaded = false;
                var numOcclusion = AssimpInterop.aiMaterial_GetNumTextureOcclusion(material);
                if (numOcclusion > 0)
                {
                    string path;
                    uint textureMapping;
                    uint uvIndex;
                    float blendMode;
                    uint op;
                    uint mapMode;
                    if (AssimpInterop.aiMaterial_GetTextureOcclusion(material, 0, out path, out textureMapping,
                        out uvIndex,
                        out blendMode, out op, out mapMode))
                    {
                        var wrapMode = mapMode == (uint)TextureWrapMode.Clamp
                            ? TextureWrapMode.Clamp
                            : TextureWrapMode.Repeat;
                        var textureName = StringUtils.GenerateUniqueName(path);
                        materialData.OcclusionPath = path;
                        materialData.OcclusionWrapMode = wrapMode;
                        materialData.OcclusionName = textureName;
                        materialData.OcclusionBlendMode = blendMode;
                        materialData.OcclusionOp = op;
                        occlusionInfoLoaded = true;
                        EmbeddedTextureData embeddedTextureData;
                        if (EmbeddedTextureLoad != null)
                        {
                            embeddedTextureData = EmbeddedTextureLoad.Invoke(path);
                        }
                        else
                        {
                            var texture = AssimpInterop.aiScene_GetEmbeddedTexture(Scene, path);
                            if (texture != IntPtr.Zero)
                            {
                                embeddedTextureData = LoadEmbeddedTextureData(texture, path);
                            }
                            else
                            {
                                embeddedTextureData = loadTextureDataCallback == null ? LoadTextureData(path, basePath) : loadTextureDataCallback(path, basePath);
                            }
                        }
                        materialData.OcclusionEmbeddedTextureData = embeddedTextureData;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarning("Error loading occlusion texture");
                    }
#endif
                }
                materialData.OcclusionInfoLoaded = occlusionInfoLoaded;

                var metallicInfoLoaded = false;
                var numMetallic = AssimpInterop.aiMaterial_GetNumTextureMetallic(material);
                if (numMetallic > 0)
                {
                    string path;
                    uint textureMapping;
                    uint uvIndex;
                    float blendMode;
                    uint op;
                    uint mapMode;
                    if (AssimpInterop.aiMaterial_GetTextureMetallic(material, 0, out path, out textureMapping,
                        out uvIndex,
                        out blendMode, out op, out mapMode))
                    {
                        var wrapMode = mapMode == (uint)TextureWrapMode.Clamp
                            ? TextureWrapMode.Clamp
                            : TextureWrapMode.Repeat;
                        var textureName = StringUtils.GenerateUniqueName(path);
                        materialData.MetallicPath = path;
                        materialData.MetallicWrapMode = wrapMode;
                        materialData.MetallicName = textureName;
                        materialData.MetallicBlendMode = blendMode;
                        materialData.MetallicOp = op;
                        metallicInfoLoaded = true;
                        EmbeddedTextureData embeddedTextureData;
                        if (EmbeddedTextureLoad != null)
                        {
                            embeddedTextureData = EmbeddedTextureLoad.Invoke(path);
                        }
                        else
                        {
                            var texture = AssimpInterop.aiScene_GetEmbeddedTexture(Scene, path);
                            if (texture != IntPtr.Zero)
                            {
                                embeddedTextureData = LoadEmbeddedTextureData(texture, path);
                            }
                            else
                            {
                                embeddedTextureData = loadTextureDataCallback == null ? LoadTextureData(path, basePath) : loadTextureDataCallback(path, basePath);
                            }
                        }
                        materialData.MetallicEmbeddedTextureData = embeddedTextureData;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarning("Error loading metallic texture");
                    }
#endif
                }
                materialData.MetallicInfoLoaded = metallicInfoLoaded;

                var bumpScaleLoaded = false;
                if (AssimpInterop.aiMaterial_HasBumpScaling(material))
                {
                    float bumpScaling;
                    if (AssimpInterop.aiMaterial_GetBumpScaling(material, out bumpScaling))
                    {
                        if (Mathf.Approximately(bumpScaling, 0f))
                        {
                            bumpScaling = 1f;
                        }
                        materialData.BumpScale = bumpScaling;
                        bumpScaleLoaded = true;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarning("Error loading bump scaling");
                    }
#endif
                }
                materialData.BumpScaleLoaded = bumpScaleLoaded;

                var shininessLoaded = false;
                if (AssimpInterop.aiMaterial_HasShininess(material))
                {
                    float shininess;
                    if (AssimpInterop.aiMaterial_GetShininess(material, out shininess))
                    {
                        materialData.Glossiness = shininess;
                        shininessLoaded = true;
                    }
#if TRILIB_OUTPUT_MESSAGES
                    else
                    {
                        Debug.LogWarning("Error loading shininess");
                    }
#endif
                }
                materialData.GlossinessLoaded = shininessLoaded;

                var shininessStrengthLoaded = false;
                if (AssimpInterop.aiMaterial_HasShininessStrength(material))
                {
                    float shininessStrength;
                    if (AssimpInterop.aiMaterial_GetShininessStrength(material, out shininessStrength))
                    {
                        materialData.GlossMapScale = shininessStrength;
                        shininessStrengthLoaded = true;
                    }
                    else
                    {
#if TRILIB_OUTPUT_MESSAGES
                        Debug.LogWarning("Error loading shininess strength");
#endif
                    }
                }
                materialData.GlossMapScaleLoaded = shininessStrengthLoaded;

                MaterialData[m] = materialData;
            }
        }

        /// <summary>
        /// Tries to load texture data from the given path, searching from basePath.
        /// </summary>
        /// <param name="path">Texture relative path.</param>
        /// <param name="basePath">Model absolute path.</param>
        /// <returns>A new <see cref="TriLib.EmbeddedTextureData"/>.</returns>
        protected EmbeddedTextureData LoadTextureData(string path, string basePath)
        {
            var filename = FileUtils.GetFilename(path);
            if (EmbeddedTextures.ContainsKey(filename))
            {
                return EmbeddedTextures[filename];
            }
            var finalPath = path;
            var data = FileUtils.LoadFileData(finalPath);
            if (data.Length == 0 && basePath != null)
            {
                finalPath = Path.Combine(basePath, path);
                data = FileUtils.LoadFileData(finalPath);
            }
            if (data.Length == 0)
            {
                finalPath = filename;
                data = FileUtils.LoadFileData(finalPath);
            }
            if (data.Length == 0 && basePath != null)
            {
                finalPath = Path.Combine(basePath, filename);
                data = FileUtils.LoadFileData(finalPath);
            }
            if (data.Length == 0)
            {
#if TRILIB_OUTPUT_MESSAGES
                    Debug.LogWarningFormat("Texture '{0}' not found", path);
#endif
                return null;
            }
            var embeddedTextureData = new EmbeddedTextureData();
            embeddedTextureData.DataPointer = STBImageLoader.LoadTextureDataFromByteArray(data, out embeddedTextureData.Width, out embeddedTextureData.Height, out embeddedTextureData.NumChannels, out embeddedTextureData.DataLength);
            embeddedTextureData.OnDataDisposal = STBImageLoader.UnloadTextureData;
            EmbeddedTextures.Add(filename, embeddedTextureData);
            return embeddedTextureData;
        }

        /// <summary>
        /// Loads an embedded texture data.
        /// </summary>
        /// <param name="texture">Assimp texture pointer.</param>
        /// <param name="textureName">Used in case of embedded textures with no name.</param>
        /// <returns>A new <see cref="TriLib.EmbeddedTextureData"/>.</returns>
        protected EmbeddedTextureData LoadEmbeddedTextureData(IntPtr texture, string textureName)
        {
            var filename = FileUtils.GetFilename(AssimpInterop.aiMaterial_GetEmbeddedTextureName(texture));
            if (string.IsNullOrEmpty(filename))
            {
                filename = FileUtils.GetFilename(textureName);
            }
            if (EmbeddedTextures.ContainsKey(filename))
            {
                return EmbeddedTextures[filename];
            }
            var embeddedTextureData = new EmbeddedTextureData();
            var isRawData = !AssimpInterop.aiMaterial_IsEmbeddedTextureCompressed(texture);
            var dataLength = AssimpInterop.aiMaterial_GetEmbeddedTextureDataSize(texture);
            var dataPointer = AssimpInterop.aiMaterial_GetEmbeddedTextureDataPointer(texture);
            if (!isRawData)
            {
                embeddedTextureData.DataPointer = STBImageLoader.LoadTextureFromDataPointer(dataPointer, (int)dataLength, out embeddedTextureData.Width, out embeddedTextureData.Height, out embeddedTextureData.NumChannels, out embeddedTextureData.DataLength);
                embeddedTextureData.OnDataDisposal = STBImageLoader.UnloadTextureData;
            }
            else
            {
                embeddedTextureData.DataPointer = dataPointer;
                embeddedTextureData.Width = AssimpInterop.aiMaterial_GetEmbeddedTextureWidth(texture);
                embeddedTextureData.Height = AssimpInterop.aiMaterial_GetEmbeddedTextureHeight(texture);
            }
            AssimpInterop.aiMaterial_ReleaseEmbeddedTexture(texture);
            EmbeddedTextures.Add(filename, embeddedTextureData);
            return embeddedTextureData;
        }

        /// <summary>
        /// Builds the bones and binding poses and assigns to the <see cref="TriLib.MeshData"/> list.
        /// </summary>
        private void BuildBones()
        {
            var meshCount = AssimpInterop.aiScene_GetNumMeshes(Scene);
            for (uint m = 0; m < meshCount; m++)
            {
                var meshData = MeshData[m];
                var mesh = AssimpInterop.aiScene_GetMesh(Scene, m);
                var hasBoneInfo = AssimpInterop.aiMesh_HasBones(mesh);
                meshData.HasBoneInfo = hasBoneInfo;
                if (hasBoneInfo)
                {
                    HasBoneInfo = true;
                    var vertexCount = AssimpInterop.aiMesh_VertexCount(mesh);
                    var boneCount = AssimpInterop.aiMesh_GetNumBones(mesh);
                    meshData.BindPoses = new Matrix4x4[boneCount];
                    meshData.BoneNames = new string[boneCount];
                    meshData.BoneWeights = new BoneWeight[vertexCount];
                    var unityBonesInVertices = new int[vertexCount];
                    for (uint b = 0; b < boneCount; b++)
                    {
                        var bone = AssimpInterop.aiMesh_GetBone(mesh, b);
                        var boneName = AssimpInterop.aiBone_GetName(bone);
                        meshData.BoneNames[b] = boneName;
                        var unityBindPose = AssimpInterop.aiBone_GetOffsetMatrix(bone);
                        meshData.BindPoses[b] = unityBindPose;
                        var vertexWeightCount = AssimpInterop.aiBone_GetNumWeights(bone);
                        for (uint w = 0; w < vertexWeightCount; w++)
                        {
                            var wInt = (int)b;
                            var vertexWeight = AssimpInterop.aiBone_GetWeights(bone, w);
                            var weightValue = AssimpInterop.aiVertexWeight_GetWeight(vertexWeight);
                            var weightVertexId = AssimpInterop.aiVertexWeight_GetVertexId(vertexWeight);
                            BoneWeight unityBoneWeight;
                            var unityCurrentBonesInVertex = unityBonesInVertices[weightVertexId];
                            switch (unityCurrentBonesInVertex)
                            {
                                case 0:
                                    unityBoneWeight = new BoneWeight
                                    {
                                        boneIndex0 = wInt,
                                        weight0 = weightValue
                                    };
                                    meshData.BoneWeights[weightVertexId] = unityBoneWeight;
                                    break;
                                case 1:
                                    unityBoneWeight = meshData.BoneWeights[weightVertexId];
                                    unityBoneWeight.boneIndex1 = wInt;
                                    unityBoneWeight.weight1 = weightValue;
                                    meshData.BoneWeights[weightVertexId] = unityBoneWeight;
                                    break;
                                case 2:
                                    unityBoneWeight = meshData.BoneWeights[weightVertexId];
                                    unityBoneWeight.boneIndex2 = wInt;
                                    unityBoneWeight.weight2 = weightValue;
                                    meshData.BoneWeights[weightVertexId] = unityBoneWeight;
                                    break;
                                case 3:
                                    unityBoneWeight = meshData.BoneWeights[weightVertexId];
                                    unityBoneWeight.boneIndex3 = wInt;
                                    unityBoneWeight.weight3 = weightValue;
                                    meshData.BoneWeights[weightVertexId] = unityBoneWeight;
                                    break;
                                default:
#if TRILIB_OUTPUT_MESSAGES
                                    Debug.LogWarningFormat("Vertex {0} has more than 4 bone weights. This is not supported", weightVertexId);
#endif
                                    unityBoneWeight = meshData.BoneWeights[weightVertexId];
                                    unityBoneWeight.boneIndex3 = wInt;
                                    unityBoneWeight.weight3 = weightValue;
                                    meshData.BoneWeights[weightVertexId] = unityBoneWeight;
                                    break;
                            }
                            unityBonesInVertices[weightVertexId]++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds the <see cref="TriLib.AnimationData"/> list for given scene.
        /// </summary>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        private void BuildAnimations(AssetLoaderOptions options)
        {
            var animationCount = AssimpInterop.aiScene_GetNumAnimations(Scene);
            for (uint a = 0; a < animationCount; a++)
            {
                var sceneAnimation = AssimpInterop.aiScene_GetAnimation(Scene, a);
                var ticksPerSecond = AssimpInterop.aiAnimation_GetTicksPerSecond(sceneAnimation);
                if (ticksPerSecond <= 0)
                {
                    ticksPerSecond = 60f;
                }
                var durationInTicks = AssimpInterop.aiAnimation_GetDuraction(sceneAnimation);
                var totalTime = durationInTicks / ticksPerSecond;
                var animationChannelCount = AssimpInterop.aiAnimation_GetNumChannels(sceneAnimation);
                var morphChannelCount = AssimpInterop.aiAnimation_GetNumMorphChannels(sceneAnimation);
                var sceneAnimationName = AssimpInterop.aiAnimation_GetName(sceneAnimation);
                sceneAnimationName = FixName(sceneAnimationName, a);
                var animationData = new AnimationData
                {
                    Name = sceneAnimationName,
                    Legacy = options == null || options.UseLegacyAnimations,
                    FrameRate = ticksPerSecond,
                    Length = totalTime,
                    ChannelData = new AnimationChannelData[animationChannelCount],
                    MorphData = new MorphChannelData[morphChannelCount]
                };
                for (uint n = 0; n < animationChannelCount; n++)
                {
                    var nodeAnimationChannel = AssimpInterop.aiAnimation_GetAnimationChannel(sceneAnimation, n);
                    var nodeName = AssimpInterop.aiNodeAnim_GetNodeName(nodeAnimationChannel);
                    AnimationChannelData channelData = new AnimationChannelData
                    {
                        CurveData = new Dictionary<string, AnimationCurveData>(),
                        NodeName = nodeName
                    };
                    var numPositionKeys = AssimpInterop.aiNodeAnim_GetNumPositionKeys(nodeAnimationChannel);
                    if (numPositionKeys > 0)
                    {
                        var unityPositionCurveX = new AnimationCurveData(numPositionKeys);
                        var unityPositionCurveY = new AnimationCurveData(numPositionKeys);
                        var unityPositionCurveZ = new AnimationCurveData(numPositionKeys);
                        for (uint p = 0; p < numPositionKeys; p++)
                        {
                            var positionKey = AssimpInterop.aiNodeAnim_GetPositionKey(nodeAnimationChannel, p);
                            var time = AssimpInterop.aiVectorKey_GetTime(positionKey) / ticksPerSecond;
                            var unityVector3 = AssimpInterop.aiVectorKey_GetValue(positionKey);
                            unityPositionCurveX.AddKey(time, unityVector3[0]);
                            unityPositionCurveY.AddKey(time, unityVector3[1]);
                            unityPositionCurveZ.AddKey(time, unityVector3[2]);
                        }
                        channelData.SetCurve("localPosition.x", unityPositionCurveX);
                        channelData.SetCurve("localPosition.y", unityPositionCurveY);
                        channelData.SetCurve("localPosition.z", unityPositionCurveZ);
                    }
                    var numRotationKeys = AssimpInterop.aiNodeAnim_GetNumRotationKeys(nodeAnimationChannel);
                    if (numRotationKeys > 0)
                    {
                        var unityRotationCurveX = new AnimationCurveData(numRotationKeys);
                        var unityRotationCurveY = new AnimationCurveData(numRotationKeys);
                        var unityRotationCurveZ = new AnimationCurveData(numRotationKeys);
                        var unityRotationCurveW = new AnimationCurveData(numRotationKeys);
                        for (uint r = 0; r < numRotationKeys; r++)
                        {
                            var rotationKey = AssimpInterop.aiNodeAnim_GetRotationKey(nodeAnimationChannel, r);
                            var time = AssimpInterop.aiQuatKey_GetTime(rotationKey) / ticksPerSecond;
                            var unityQuaternion = AssimpInterop.aiQuatKey_GetValue(rotationKey);
                            unityRotationCurveX.AddKey(time, unityQuaternion[1]);
                            unityRotationCurveY.AddKey(time, unityQuaternion[2]);
                            unityRotationCurveZ.AddKey(time, unityQuaternion[3]);
                            unityRotationCurveW.AddKey(time, unityQuaternion[0]);
                        }
                        channelData.SetCurve("localRotation.x", unityRotationCurveX);
                        channelData.SetCurve("localRotation.y", unityRotationCurveY);
                        channelData.SetCurve("localRotation.z", unityRotationCurveZ);
                        channelData.SetCurve("localRotation.w", unityRotationCurveW);
                    }
                    var numScalingKeys = AssimpInterop.aiNodeAnim_GetNumScalingKeys(nodeAnimationChannel);
                    if (numScalingKeys > 0)
                    {

                        var unityScaleCurveX = new AnimationCurveData(numScalingKeys);
                        var unityScaleCurveY = new AnimationCurveData(numScalingKeys);
                        var unityScaleCurveZ = new AnimationCurveData(numScalingKeys);
                        for (uint s = 0; s < numScalingKeys; s++)
                        {
                            var scaleKey = AssimpInterop.aiNodeAnim_GetScalingKey(nodeAnimationChannel, s);
                            var time = AssimpInterop.aiVectorKey_GetTime(scaleKey) / ticksPerSecond;
                            var unityVector3 = AssimpInterop.aiVectorKey_GetValue(scaleKey);
                            unityScaleCurveX.AddKey(time, unityVector3[0]);
                            unityScaleCurveY.AddKey(time, unityVector3[1]);
                            unityScaleCurveZ.AddKey(time, unityVector3[2]);
                        }
                        channelData.SetCurve("localScale.x", unityScaleCurveX);
                        channelData.SetCurve("localScale.y", unityScaleCurveY);
                        channelData.SetCurve("localScale.z", unityScaleCurveZ);
                    }
                    animationData.ChannelData[n] = channelData;
                }
                for (uint n = 0; n < morphChannelCount; n++)
                {
                    var meshAnimationChannel = AssimpInterop.aiAnimation_GetMeshMorphAnim(sceneAnimation, n);
                    var nodeName = AssimpInterop.aiMeshMorphAnim_GetName(meshAnimationChannel);
                    var morphChannelData = new MorphChannelData
                    {
                        MorphChannelKeys = new Dictionary<float, MorphChannelKey>(),
                        NodeName = nodeName
                    };
                    var numKeys = AssimpInterop.aiMeshMorphAnim_GetNumKeys(meshAnimationChannel);
                    for (uint d = 0; d < numKeys; d++)
                    {
                        var morphKey = AssimpInterop.aiMeshMorphAnim_GetMeshMorphKey(meshAnimationChannel, d);
                        var time = AssimpInterop.aiMeshMorphKey_GetTime(morphKey) / ticksPerSecond;
                        var numIndices = AssimpInterop.aiMeshMorphKey_GetNumValues(morphKey);
                        var morphChannelKey = new MorphChannelKey
                        {
                            Indices = new uint[numIndices],
                            Weights = new float[numIndices]
                        };
                        for (uint i = 0; i < numIndices; i++)
                        {
                            morphChannelKey.Indices[i] = AssimpInterop.aiMeshMorphKey_GetValue(morphKey, i);
                            morphChannelKey.Weights[i] = AssimpInterop.aiMeshMorphKey_GetWeight(morphKey, i);
                        }
                        morphChannelData.MorphChannelKeys.Add(time, morphChannelKey);
                    }
                    animationData.MorphData[n] = morphChannelData;
                }
                animationData.WrapMode = options != null ? options.AnimationWrapMode : WrapMode.Loop;
                AnimationData[a] = animationData;
            }
        }

        /// <summary>
        /// Generates a unique node name, if the given name is empty.
        /// </summary>
        /// <param name="name">Node name to check.</param>
        /// <param name="nodeId">Node id to use when the node name is empty or when it already exists.</param>
        /// <returns>Generated name if given name is empty. Otherwise, returns the given name.</returns>
        protected virtual string FixNodeName(string name, uint nodeId)
        {
            if (string.IsNullOrEmpty(name))
            {
                return nodeId.ToString();
            }
            if (NodesPath != null && NodesPath.ContainsKey(name))
            {
                return name + nodeId;
            }
            return name;
        }

        /// <summary>
        /// Generates a unique name, if the given name is empty.
        /// </summary>
        /// <param name="name">Name to check.</param>
        /// <param name="id">Id to generate the unique name.</param>
        /// <returns>Generated name if given name is empty. Otherwise, returns the given name.</returns>
        protected virtual string FixName(string name, uint id)
        {
            return string.IsNullOrEmpty(name) ? StringUtils.GenerateUniqueName(id) : name;
        }

        /// <summary>
        /// Generates a unique name using GUIDs, if the given name is empty.
        /// </summary>
        /// <param name="name">Name to check.</param>
        /// <returns>Generated name if given name is empty. Otherwise, returns the given name.</returns>
        protected virtual string FixName(string name)
        {
            return string.IsNullOrEmpty(name) ? new Guid().ToString() : name;
        }

        /// <summary>
        /// Dummy progress callback method.
        /// </summary>
        /// <param name="progress">Progress value.</param>
        [MonoPInvokeCallback(typeof(AssimpInterop.ProgressCallback))]
        private static void DefaultProgressCallback(float progress)
        {

        }

        /// <summary>
        /// The standard resource data retrieval callback.
        /// </summary>
        /// <param name="resourceFilename">Requested resource filename.</param>
        /// <param name="resourceId">Requested resource generated ID.</param>
        /// <param name="fileSize">Requested resource output fiel size.</param>
        /// <returns>The resource data <see cref="System.IntPtr"/>, if exists.</returns>
        [MonoPInvokeCallback(typeof(AssimpInterop.DataCallback))]
        private static IntPtr DefaultDataCallback(string resourceFilename, int resourceId, ref int fileSize)
        {
            var fileLoadData = FilesLoadData[resourceId];
            if (resourceFilename == fileLoadData.Filename || resourceFilename.StartsWith(AssimpFilenameMagicString))
            {
                return IntPtr.Zero;
            }
            var resourceShortFilename = FileUtils.GetFilename(resourceFilename).ToLowerInvariant();
            var resourceFileBytes = FileUtils.LoadFileData(Path.Combine(fileLoadData.BasePath ?? "", resourceShortFilename));
            if (resourceFileBytes.Length == 0)
            {
#if TRILIB_USE_ZIP
                var zipGCFileLoadData = fileLoadData as ZipGCFileLoadData;
                if (zipGCFileLoadData != null && zipGCFileLoadData.ZipFile != null)
                {
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
                    foreach (ZipArchiveEntry zipEntry in zipGCFileLoadData.ZipFile.Entries)
                    {
#else
                    foreach (ZipEntry zipEntry in zipGCFileLoadData.ZipFile)
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue;
                        }
#endif
                        if (FileUtils.GetShortFilename(zipEntry.Name).ToLowerInvariant() == resourceShortFilename)
                        {
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
                            var zipStream = zipEntry.Open();
#else
                            var zipStream = zipGCFileLoadData.ZipFile.GetInputStream(zipEntry);
#endif
                            resourceFileBytes = StreamUtils.ReadFullStream(zipStream);
                            zipStream.Dispose();
                            break;
                        }
                    }
                }
#endif
#if !UNITY_EDITOR && UNITY_WEBGL
                var browserLoadData = fileLoadData as BrowserLoadData;
                if (browserLoadData != null) {
                    for (var i = 0; i < browserLoadData.FilesCount; i++)
                    {
                        if (JSHelper.Instance.GetBrowserFileName(i).ToLowerInvariant() == resourceShortFilename)
                        {
                            resourceFileBytes = JSHelper.Instance.GetBrowserFileData(i);
                            break;
                        }
                    }
                }
#endif
            }
            if (resourceFileBytes.Length == 0)
            {
                return IntPtr.Zero;
            }
            fileSize = resourceFileBytes.Length;
            var dataBuffer = AssimpInterop.LockGc(resourceFileBytes);
            fileLoadData.AddBuffer(dataBuffer);
            return dataBuffer.AddrOfPinnedObject();
        }

        /// <summary>
        /// The standard callback to defines if a resource exists.
        /// </summary>
        /// <param name="resourceFilename">Requested resource filename.</param>
        /// <param name="resourceId">Requested resource generated ID.</param>
        /// <returns><c>true</c> if resource exists, otherwise <c>false</c>.</returns>
        [MonoPInvokeCallback(typeof(AssimpInterop.ExistsCallback))]
        private static bool DefaultExistsCallback(string resourceFilename, int resourceId)
        {
            var fileLoadData = FilesLoadData[resourceId];
            if (resourceFilename == fileLoadData.Filename || resourceFilename.StartsWith(AssimpFilenameMagicString))
            {
                return false;
            }
            var resourceShortFilename = FileUtils.GetFilename(resourceFilename);
            var exists = File.Exists(Path.Combine(fileLoadData.BasePath ?? "", resourceShortFilename));
            if (!exists)
            {
#if TRILIB_USE_ZIP
                var zipGCFileLoadData = fileLoadData as ZipGCFileLoadData;
                if (zipGCFileLoadData != null && zipGCFileLoadData.ZipFile != null)
                {
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
                    foreach (ZipArchiveEntry zipEntry in zipGCFileLoadData.ZipFile.Entries)
                    {
#else
                    foreach (ZipEntry zipEntry in zipGCFileLoadData.ZipFile)
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue;
                        }
#endif
                        if (FileUtils.GetShortFilename(zipEntry.Name).ToLowerInvariant() == resourceShortFilename)
                        {
                            exists = true;
                            break;
                        }
                    }
                }
#endif
#if !UNITY_EDITOR && UNITY_WEBGL
                var browserLoadData = fileLoadData as BrowserLoadData;
                if (browserLoadData != null) {
                    for (var i = 0; i < browserLoadData.FilesCount; i++)
                    {
                        if (JSHelper.Instance.GetBrowserFileName(i).ToLowerInvariant() == resourceShortFilename)
                        {
                            exists = true;
                            break;
                        }
                    }
                }
#endif
            }
            return exists;
        }

        /// <summary>
        /// Releases allocated resources.
        /// </summary>
        protected void ReleaseImport()
        {
            if (Scene != IntPtr.Zero)
            {
                AssimpInterop.ai_ReleaseImport(Scene);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            RootNodeData = null;
            MaterialData = null;
            MeshData = null;
            AnimationData = null;
            CameraData = null;
            Metadata = null;
            NodesPath = null;
            LoadedMaterials = null;
            LoadedTextures = null;
            LoadedBoneNames = null;
            MeshDataConnections = null;
            EmbeddedTextures = null;
            NodeId = 0;
            HasBoneInfo = false;
            HasBlendShapes = false;
            Scene = IntPtr.Zero;
        }
    }
}
#pragma warning restore 612, 618, 67