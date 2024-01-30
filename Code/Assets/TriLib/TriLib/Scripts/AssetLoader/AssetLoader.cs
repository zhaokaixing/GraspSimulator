using UnityEngine;

namespace TriLib
{
    /// <summary>
    /// Represents a synchronous asset loader.
    /// </summary>
    public class AssetLoader : AssetLoaderBase
    {
        /// <summary>
        /// Loads a <see cref="UnityEngine.GameObject"/> from input filename with defined options.
        /// @warning To ensure your materials will be loaded, don´t remove the material files included in the package.
        /// </summary>
        /// <param name="filename">Filename used to load the <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        /// <param name="wrapperGameObject">Use this field to load the new <see cref="UnityEngine.GameObject"/> into referenced <see cref="UnityEngine.GameObject"/>.</param> 
        /// <param name="basePath">Base path from the loaded file.</param>
        /// <param name="progressCallback">Callback used to retrieve file loading percentage.</param>
        /// <returns>Loaded <see cref="UnityEngine.GameObject"></see>.</returns>
        /// <example>
        /// @code
        /// protected void Awake() {
        ///     GameObject myGameObject;
        ///     try {
        ///         using (var assetLoader = new AssetLoader()) {
        ///             gameObject = assetLoader.LoadFromFile("mymodel.fbx");
        ///         }
        ///     } catch (Exception e) {
        ///         Debug.LogFormat("Unable to load mymodel.fbx. The loader returned: {0}", e);
        ///     }
        /// }
        /// @endcode
        /// </example>
        public GameObject LoadFromFile(string filename, AssetLoaderOptions options = null, GameObject wrapperGameObject = null, string basePath = null, AssimpInterop.ProgressCallback progressCallback = null)
        {
            if (basePath == null)
            {
                basePath = FileUtils.GetFileDirectory(filename);
            }
            InternalLoadFromFile(filename, basePath, options, wrapperGameObject != null, progressCallback);
            var loadedGameObject = BuildGameObject(options, basePath, wrapperGameObject);
            ReleaseImport();
            return loadedGameObject;
        }

        /// <summary>
        /// Loads a <see cref="UnityEngine.GameObject"/> from input byte array with defined options.
        /// @warning To ensure your materials will be loaded, don´t remove material files included in the package.
        /// </summary>
        /// <param name="fileBytes">Data used to load the <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="filename">Original file name, if you know it. Otherwise, use the original file extension instead. (Eg: ".FBX")</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        /// <param name="wrapperGameObject">Use this field to load the new <see cref="UnityEngine.GameObject"/> into referenced <see cref="UnityEngine.GameObject"/>.</param> 
        /// <param name="basePath">Base path from the loaded file.</param>
        /// <param name="dataCallback">Custom resource data retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="existsCallback">Custom resource size retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="loadTextureDataCallback">Pass this callback to load texture data from custom sources.</param>
        /// <param name="progressCallback">Callback used to retrieve file loading percentage.</param>
        /// <returns>Loaded <see cref="UnityEngine.GameObject"></see>.</returns>
        /// <example>
        /// @code
        /// protected void Awake() {
        ///     GameObject myGameObject;
        ///     try {
        ///         using (var assetLoader = new AssetLoader()) {
        ///             //In case you don't have a valid filename, set this to the file extension
        ///             //to help TriLib assigning a file loader to this file
        ///             //example value: ".FBX"
        /// 			var filename = "c:/models/mymodel.fbx";
        /// 			var fileData = File.ReadAllBytes(filename);
        ///             gameObject = assetLoader.LoadFromMemory(fleData, filename);
        ///         }
        ///     } catch (Exception e) {
        ///         Debug.LogFormat("Unable to load mymodel.fbx. The loader returned: {0}", e);
        ///     }
        /// }
        /// @endcode
        /// </example>
        public GameObject LoadFromMemory(byte[] fileBytes, string filename, AssetLoaderOptions options = null, GameObject wrapperGameObject = null, string basePath = null, AssimpInterop.DataCallback dataCallback = null, AssimpInterop.ExistsCallback existsCallback = null, LoadTextureDataCallback loadTextureDataCallback = null, AssimpInterop.ProgressCallback progressCallback = null)
        {
            if (basePath == null)
            {
                basePath = FileUtils.GetFileDirectory(filename);
            }
            InternalLoadFromMemory(fileBytes, filename, basePath, options, wrapperGameObject != null, dataCallback, existsCallback, loadTextureDataCallback, progressCallback);
            var loadedGameObject = BuildGameObject(options, basePath, wrapperGameObject);
            ReleaseImport();
            return loadedGameObject;
        }

        /// <summary>
        /// Loads a <see cref="UnityEngine.GameObject"/> from file (Accept ZIP files).
        /// </summary>
        /// <param name="filename">Filename used to load the <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        /// <param name="wrapperGameObject">Use this field to load the new <see cref="UnityEngine.GameObject"/> into referenced <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="basePath">Base path from the loaded file.</param>
        /// <param name="progressCallback">Callback used to retrieve file loading percentage.</param>
        /// <returns>Loaded <see cref="UnityEngine.GameObject"></see>.</returns>
        public GameObject LoadFromFileWithTextures(string filename, AssetLoaderOptions options = null, GameObject wrapperGameObject = null, string basePath = null, AssimpInterop.ProgressCallback progressCallback = null)
        {
            var fileData = FileUtils.LoadFileData(filename);
            var extension = FileUtils.GetFileExtension(filename);
            if (basePath == null)
            {
                basePath = FileUtils.GetFileDirectory(filename);
            }
            InternalLoadFromMemoryAndZip(fileData, extension, basePath, options, wrapperGameObject != null, null, null, null, progressCallback, filename);
            var loadedGameObject = BuildGameObject(options, extension, wrapperGameObject);
            ReleaseImport();
            return loadedGameObject;
        }

        /// <summary>
        /// Loads a <see cref="UnityEngine.GameObject"/> from input byte array (Accept ZIP files).
        /// </summary>
        /// <param name="fileData">File data.</param>
        /// <param name="assetExtension">Asset extension.</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        /// <param name="wrapperGameObject">Use this field to load the new <see cref="UnityEngine.GameObject"/> into referenced <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="basePath">Base path from the loaded file.</param>
        /// <param name="dataCallback">Custom resource data retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="existsCallback">Custom resource size retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="progressCallback">Callback used to retrieve file loading percentage.</param>
        /// <returns>Loaded <see cref="UnityEngine.GameObject"></see>.</returns>
        public GameObject LoadFromMemoryWithTextures(byte[] fileData, string assetExtension, AssetLoaderOptions options = null, GameObject wrapperGameObject = null, string basePath = null, AssimpInterop.DataCallback dataCallback = null, AssimpInterop.ExistsCallback existsCallback = null, AssimpInterop.ProgressCallback progressCallback = null)
        {
            if (basePath == null)
            {
                basePath = FileUtils.GetFileDirectory(assetExtension);
            }
            InternalLoadFromMemoryAndZip(fileData, assetExtension, basePath, options, wrapperGameObject != null, dataCallback, existsCallback, null, progressCallback);
            var loadedGameObject = BuildGameObject(options, assetExtension, wrapperGameObject);
            ReleaseImport();
            return loadedGameObject;
        }

#if !UNITY_EDITOR && UNITY_WEBGL
        /// <summary>
        /// Loads a <see cref="UnityEngine.GameObject"/> from browser files.
        /// </summary>
        /// <param name="filesCount">Browser files count.</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        /// <param name="wrapperGameObject">Use this field to load the new <see cref="UnityEngine.GameObject"/> into referenced <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="dataCallback">Custom resource data retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="existsCallback">Custom resource size retrieval callback. Pass this parameter when you need to load external data while loading from memory.</param>
        /// <param name="progressCallback">Callback used to retrieve file loading percentage.</param>
        /// <returns>Loaded <see cref="UnityEngine.GameObject"></see>.</returns>
        public GameObject LoadFromBrowserFilesWithTextures(int filesCount, AssetLoaderOptions options = null, GameObject wrapperGameObject = null, AssimpInterop.DataCallback dataCallback = null, AssimpInterop.ExistsCallback existsCallback = null, AssimpInterop.ProgressCallback progressCallback = null)
        {
            byte[] fileData;
            string fileName;
            string fileExtension;
            if (!GetSupportedBrowserFileData(filesCount, out fileData, out fileName, out fileExtension))
            {
                return null;
            }
            InternalLoadFromBrowserFiles(filesCount, fileData, fileName, fileExtension, options, wrapperGameObject != null, dataCallback, existsCallback, null, progressCallback);
            var loadedGameObject = BuildGameObject(options, fileExtension, wrapperGameObject);
            ReleaseImport();
            return loadedGameObject;
        }
#endif
    }
}
