#pragma warning disable 649
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace TriLib
{
    namespace Samples
    {
        /// <summary>
        /// Represents the asset loader UI component.
        /// </summary>
        [RequireComponent(typeof(AssetDownloader))]
        public class AssetLoaderWindow : MonoBehaviour
        {
            // drag the placeholder subject to this field in Unity
            public ModelLoader LocalLoader;


            /// <summary>
            /// Class singleton.
            /// </summary>
            public static AssetLoaderWindow Instance { get; private set; }

            /// <summary>
            /// Turn on this field to enable async loading.
            /// </summary>
            public bool Async;

            /// <summary>
            /// "Load local asset button" reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.Button _loadLocalAssetButton;
            /// <summary>
            /// "Load remote button" reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.Button _loadRemoteAssetButton;
            /// <summary>
            /// "Spinning text" reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.Text _spinningText;
            /// <summary>
            /// "Transparency dropdown" reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.Dropdown _transparencyModeDropdown;
            /// <summary>
            /// "Shading dropdown" reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.Dropdown _shadingDropdown;
            /// <summary>
            /// "Spin X toggle" reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.Toggle _spinXToggle;
            /// <summary>
            /// "Spin Y toggle" reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.Toggle _spinYToggle;
            /// <summary>
            /// "Reset rotation button" reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.Button _resetRotationButton;
            /// <summary>
            /// "Stop animation button" reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.Button _stopAnimationButton;
            /// <summary>
            /// "Animations text" reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.Text _animationsText;
            /// <summary>
            /// "Blend Shapes text" reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.Text _blendShapesText;
            /// <summary>
            /// "Animations scroll rect "reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.ScrollRect _animationsScrollRect;
            /// <summary>
            /// "Blend Shapes scroll rect "reference.
            /// </summary>
            [SerializeField]
            private UnityEngine.UI.ScrollRect _blendShapesScrollRect;
            /// <summary>
            /// "Animations scroll rect container" reference.
            /// </summary>
            [SerializeField]
            private Transform _containerTransform;
            /// <summary>
            /// "Blend Shapes scroll rect container" reference.
            /// </summary>
            [SerializeField]
            private Transform _blendShapesContainerTransform;
            /// <summary>
            /// <see cref="AnimationText"/> prefab reference.
            /// </summary>
            [SerializeField]
            private AnimationText _animationTextPrefab;
            /// <summary>
            /// <see cref="BlendShapeControl"/> prefab reference.
            /// </summary>
            [SerializeField]
            private BlendShapeControl _blendShapeControlPrefab;
            /// <summary>
            /// "Background (gradient) canvas" reference.
            /// </summary>
            //[SerializeField]
            //private Canvas _backgroundCanvas;
            /// <summary>
            /// Loaded Game Object reference.
            /// </summary>
            public GameObject PlaceHolderGameObject;
            /// <summary>
            /// Total loading time Text reference.
            /// </summary>
            [SerializeField]
            private Text _loadingTimeText;

            /// <summary>
            /// WebGL drag and drop Text reference.
            /// </summary>
            [SerializeField]
            private Text _dragAndDropText;

            /// <summary>
            /// Loading timer.
            /// </summary>
            private Stopwatch _loadingTimer = new Stopwatch();

            /// <summary>
            /// Handles events from <see cref="AnimationText"/>.
            /// </summary>
            /// <param name="animationName">Chosen animation name.</param>
            public void HandleEvent(string animationName)
            {
                PlaceHolderGameObject.GetComponent<Animation>().Play(animationName);
                _stopAnimationButton.interactable = true;
            }

            /// <summary>
            /// Handles events from <see cref="BlendShapeControl"/>.
            /// </summary>
            /// <param name="skinnedMeshRenderer">Skinned Mesh Renderer to set the blend shape.</param>
            /// <param name="index">Blend Shape index.</param>
            /// <param name="value">Blend Shape value.</param>
            public void HandleBlendEvent(SkinnedMeshRenderer skinnedMeshRenderer, int index, float value)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(index, value);
            }

            /// <summary>
            /// Destroys all objects in the containers.
            /// </summary>
            public void DestroyItems()
            {
                foreach (Transform innerTransform in _containerTransform)
                {
                    Destroy(innerTransform.gameObject);
                }
                foreach (Transform innerTransform in _blendShapesContainerTransform)
                {
                    Destroy(innerTransform.gameObject);
                }
            }


            /// <summary>
            /// Initializes variables.
            /// </summary>
            protected void Awake()
            {
                _loadLocalAssetButton.onClick.AddListener(LoadLocalAssetButtonClick);
                _loadRemoteAssetButton.onClick.AddListener(LoadRemoteAssetButtonClick);
                _stopAnimationButton.onClick.AddListener(StopAnimationButtonClick);
                _resetRotationButton.onClick.AddListener(ResetRotationButtonClick);
#if !UNITY_EDITOR && UNITY_WEBGL
                _dragAndDropText.gameObject.SetActive(true);
#endif
                HideControls();
                Instance = this;

                //// hide this UI on model imported
                //LocalLoader.SubjectImported += (src, subject) =>
                //{
                //    gameObject.SetActive(false);
                //};
            }

            /// <summary>
            /// Spins the loaded Game Object if options are enabled.
            /// </summary>
            protected void Update()
            {
                if (PlaceHolderGameObject != null)
                {
                    PlaceHolderGameObject.transform.Rotate(_spinXToggle.isOn ? 20f * Time.deltaTime : 0f,
                        _spinYToggle.isOn ? -20f * Time.deltaTime : 0f, 0f, Space.World);
                }
            }

            /// <summary>
            /// Hides user controls.
            /// </summary>
            private void HideControls()
            {
                _loadLocalAssetButton.interactable = true;
                _loadRemoteAssetButton.interactable = true;
                _spinningText.gameObject.SetActive(false);
                _spinXToggle.gameObject.SetActive(false);
                _spinYToggle.gameObject.SetActive(false);
                _resetRotationButton.gameObject.SetActive(false);
                _stopAnimationButton.gameObject.SetActive(false);
                _animationsText.gameObject.SetActive(false);
                _animationsScrollRect.gameObject.SetActive(false);
                _blendShapesText.gameObject.SetActive(false);
                _blendShapesScrollRect.gameObject.SetActive(false);
            }

            /// <summary>
            /// Handles "Load local asset button" click event and tries to load an asset at chosen path.
            /// </summary>
            private void LoadLocalAssetButtonClick()
            {

                var fileOpenDialog = FileOpenDialog.Instance;
                fileOpenDialog.Title = "Please select a File";
                fileOpenDialog.Filter = AssetLoaderBase.GetSupportedFileExtensions() + ";*.zip;";

                fileOpenDialog.ShowFileOpenDialog(delegate (string filename)
                {
                     LocalLoader.LoadIocal(filename);

                     //LoadInternal(filename);


                }
                );

            }

            //#if !UNITY_EDITOR && UNITY_WEBGL
            /// <summary>
            /// Loads the model from browser files.
            /// </summary>
            /// <param name="filesCount">Browser files count.</param>
            public void LoadFromBrowserFiles(int filesCount)
            {
                LoadInternal(null, null, filesCount);
            }
            //#endif

            /// <summary>
            /// Executes the post model loading steps.
            /// </summary>
            private void FullPostLoadSetup()
            {
                if (PlaceHolderGameObject != null)
                {
                    PostLoadSetup();
                    ShowLoadingTime();
                }
                else
                {
                    HideLoadingTime();
                }
            }

            /// <summary>
            /// Displays a <see cref="System.Exception"/>.
            /// </summary>
            /// <param name="exception"><see cref="System.Exception"/> to display.</param>
            private void HandleException(Exception exception)
            {
                //if (PlaceHolderGameObject != null)
                //{
                //    Destroy(PlaceHolderGameObject);
                //}
                //PlaceHolderGameObject = null;

                HideLoadingTime();
                ErrorDialog.Instance.ShowDialog(exception.ToString());
            }

            /// <summary>
            /// Checks for a valid model (a model should contain meshes to be displayed).
            /// </summary>
            /// <param name="assetLoader"><see cref="AssetLoaderBase"/> used to load the model.</param>
            private void CheckForValidModel(AssetLoaderBase assetLoader)
            {
                if (assetLoader.MeshData == null || assetLoader.MeshData.Length == 0)
                {
                    throw new Exception("File contains no meshes");
                }
            }

            /// <summary>
            /// Loads a model from the given filename, file bytes or browser files.
            /// </summary>
            /// <param name="filename">Model filename.</param>
            /// <param name="fileBytes">Model file bytes.</param>
            /// <param name="browserFilesCount">Browser files count.</param>
            private void LoadInternal(string filename, byte[] fileBytes = null, int browserFilesCount = -1)
            {
                PreLoadSetup();
                var assetLoaderOptions = GetAssetLoaderOptions();
                if (!Async)
                {
                    using (var assetLoader = new AssetLoader())
                    {
                        assetLoader.OnMetadataProcessed += AssetLoader_OnMetadataProcessed;
                        try
                        {
                            if (fileBytes != null && fileBytes.Length > 0)
                            {
                                PlaceHolderGameObject = assetLoader.LoadFromMemoryWithTextures(fileBytes, FileUtils.GetFileExtension(filename), assetLoaderOptions, PlaceHolderGameObject);
                            }
                            else if (!string.IsNullOrEmpty(filename))
                            {
                                PlaceHolderGameObject = assetLoader.LoadFromFileWithTextures(filename, assetLoaderOptions);
                            }
                            else
                            {
                                throw new Exception("File not selected");
                            }
                            CheckForValidModel(assetLoader);
                        }
                        catch (Exception exception)
                        {
                            HandleException(exception);
                        }
                    }
                    FullPostLoadSetup();
                }
                else
                {
                    using (var assetLoader = new AssetLoaderAsync())
                    {
                        assetLoader.OnMetadataProcessed += AssetLoader_OnMetadataProcessed;
                        try
                        {

                            if (fileBytes != null && fileBytes.Length > 0)
                            {
                                assetLoader.LoadFromMemoryWithTextures(fileBytes, FileUtils.GetFileExtension(filename), assetLoaderOptions, null, delegate (GameObject loadedGameObject)
                                {
                                    CheckForValidModel(assetLoader);
                                    PlaceHolderGameObject = loadedGameObject;
                                    FullPostLoadSetup();
                                });
                            }
                            else if (!string.IsNullOrEmpty(filename))
                            {
                                assetLoader.LoadFromFileWithTextures(filename, assetLoaderOptions, null, delegate (GameObject loadedGameObject)
                                {
                                    CheckForValidModel(assetLoader);
                                    PlaceHolderGameObject = loadedGameObject;
                                    FullPostLoadSetup();
                                });
                            }
                            else
                            {
                                throw new Exception("File not selected");
                            }
                        }
                        catch (Exception exception)
                        {
                            HandleException(exception);
                        }
                    }
                }
            }

            /// <summary>
            /// Shows the total asset loading time.
            /// </summary>
            private void ShowLoadingTime()
            {
                _loadingTimeText.text = string.Format("Loading time: {0:00}:{1:00}.{2:00}", _loadingTimer.Elapsed.Minutes, _loadingTimer.Elapsed.Seconds, _loadingTimer.Elapsed.Milliseconds / 10);
                _loadingTimer.Stop();
            }

            /// <summary>
            /// Hides the total asset loading time.
            /// </summary>
            private void HideLoadingTime()
            {
                _loadingTimeText.text = null;
            }

            /// <summary>
            /// Event assigned to FBX metadata loading. Editor debug purposes only.
            /// </summary>
            /// <param name="metadataType">Type of loaded metadata</param>
            /// <param name="metadataIndex">Index of loaded metadata</param>
            /// <param name="metadataKey">Key of loaded metadata</param>
            /// <param name="metadataValue">Value of loaded metadata</param>
            private void AssetLoader_OnMetadataProcessed(AssimpMetadataType metadataType, uint metadataIndex, string metadataKey, object metadataValue)
            {
                Debug.Log("Found metadata of type [" + metadataType + "] at index [" + metadataIndex + "] and key [" + metadataKey + "] with value [" + metadataValue + "]");
            }

            /// <summary>
            /// Gets the asset loader options.
            /// </summary>
            /// <returns>The asset loader options.</returns>
            private AssetLoaderOptions GetAssetLoaderOptions()
            {
                var assetLoaderOptions = AssetLoaderOptions.CreateInstance();
                assetLoaderOptions.DontLoadCameras = false;
                assetLoaderOptions.DontLoadLights = false;
                assetLoaderOptions.UseOriginalPositionRotationAndScale = true;
                switch (_transparencyModeDropdown.value)
                {
                    case 0:
                        assetLoaderOptions.DisableAlphaMaterials = true;
                        break;
                    case 1:
                        assetLoaderOptions.MaterialTransparencyMode = MaterialTransparencyMode.Alpha;
                        break;
                    case 2:
                        assetLoaderOptions.MaterialTransparencyMode = MaterialTransparencyMode.Cutout;
                        break;
                    case 3:
                        assetLoaderOptions.MaterialTransparencyMode = MaterialTransparencyMode.Fade;
                        break;
                }
                switch (_shadingDropdown.value)
                {
                    case 1:
                        assetLoaderOptions.MaterialShadingMode = MaterialShadingMode.Roughness;
                        break;
                    case 2:
                        assetLoaderOptions.MaterialShadingMode = MaterialShadingMode.Specular;
                        break;
                }
                assetLoaderOptions.AddAssetUnloader = true;
                assetLoaderOptions.AdvancedConfigs.Add(AssetAdvancedConfig.CreateConfig(AssetAdvancedPropertyClassNames.FBXImportDisableDiffuseFactor, true));
                return assetLoaderOptions;
            }

            /// <summary>
            /// Pre Load setup.
            /// </summary>
            private void PreLoadSetup()
            {
                _loadingTimer.Reset();
                _loadingTimer.Start();
                HideControls();
                //if (PlaceHolderGameObject != null)
                //{
                //    Destroy(PlaceHolderGameObject);
                //    PlaceHolderGameObject = null;
                //}
            }

            /// <summary>
            /// Post load setup.
            /// </summary>
            private void PostLoadSetup()
            {
                var mainCamera = Camera.main;
                print(PlaceHolderGameObject.transform);

                mainCamera.FitToBounds(PlaceHolderGameObject.transform, 3f);
                //_backgroundCanvas.planeDistance = mainCamera.farClipPlane * 0.99f;
                _spinningText.gameObject.SetActive(true);
                _spinXToggle.isOn = false;
                _spinXToggle.gameObject.SetActive(true);
                _spinYToggle.isOn = false;
                _spinYToggle.gameObject.SetActive(true);
                _resetRotationButton.gameObject.SetActive(true);
                DestroyItems();
                var rootAnimation = PlaceHolderGameObject.GetComponent<Animation>();
                if (rootAnimation != null)
                {
                    _animationsText.gameObject.SetActive(true);
                    _animationsScrollRect.gameObject.SetActive(true);
                    _stopAnimationButton.gameObject.SetActive(true);
                    _stopAnimationButton.interactable = true;
                    foreach (AnimationState animationState in rootAnimation)
                    {
                        CreateItem(animationState.name);
                    }
                }
                var skinnedMeshRenderers = PlaceHolderGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinnedMeshRenderers != null)
                {
                    var hasBlendShapes = false;
                    foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                    {
                        if (!hasBlendShapes && skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                        {
                            _blendShapesText.gameObject.SetActive(true);
                            _blendShapesScrollRect.gameObject.SetActive(true);
                            hasBlendShapes = true;
                        }
                        for (var i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                        {
                            CreateBlendShapeItem(skinnedMeshRenderer, skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i), i);
                        }
                    }
                }
            }

            /// <summary>
            /// Handles "Load remote asset button" click event and tries to load the asset at chosen URI.
            /// </summary>
            private void LoadRemoteAssetButtonClick()
            {
                URIDialog.Instance.ShowDialog(delegate (string assetUri, string assetExtension)
                {
                    var assetDownloader = GetComponent<AssetDownloader>();
                    assetDownloader.DownloadAsset(assetUri, assetExtension, LoadDownloadedAsset, null, GetAssetLoaderOptions());
                    _loadLocalAssetButton.interactable = false;
                    _loadRemoteAssetButton.interactable = false;
                });
            }

            /// <summary>
            /// Loads the downloaded asset.
            /// </summary>
            /// <param name="loadedGameObject">Loaded game object.</param>
            private void LoadDownloadedAsset(GameObject loadedGameObject)
            {
                PreLoadSetup();
                if (loadedGameObject != null)
                {
                    PlaceHolderGameObject = loadedGameObject;
                    PostLoadSetup();
                }
                else
                {
                    var assetDownloader = GetComponent<AssetDownloader>();
                    ErrorDialog.Instance.ShowDialog(assetDownloader.Error);
                }
            }

            /// <summary>
            /// Creates a <see cref="AnimationText"/> item in the container.
            /// </summary>
            /// <param name="text">Text of the <see cref="AnimationText"/> item.</param>
            private void CreateItem(string text)
            {
                var instantiated = Instantiate(_animationTextPrefab, _containerTransform);
                instantiated.Text = text;
            }

            /// <summary>
            /// Creates a <see cref="BlendShapeControl"/> item in the container.
            /// </summary>
            /// <param name="skinnedMeshRenderer"><see cref="UnityEngine.SkinnedMeshRenderer"/> assigned to the control.</param>
            /// <param name="name">Blend Shape name assigned to the control.</param>
            /// <param name="index">Blend Shape index assigned to the control.</param>
            private void CreateBlendShapeItem(SkinnedMeshRenderer skinnedMeshRenderer, string name, int index)
            {
                var instantiated = Instantiate(_blendShapeControlPrefab, _blendShapesContainerTransform);
                instantiated.SkinnedMeshRenderer = skinnedMeshRenderer;
                instantiated.Text = name;
                instantiated.BlendShapeIndex = index;
                instantiated.BlendShapeIndex = index;
            }

            /// <summary>
            /// Handles the "Reset Rotation button" click event and stops the loaded Game Object spinning. 
            /// </summary>
            private void ResetRotationButtonClick()
            {
                _spinXToggle.isOn = false;
                _spinYToggle.isOn = false;
                PlaceHolderGameObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            /// <summary>
            /// Handles the "Stop Animation button" click event and stops the loaded Game Object animation.
            /// </summary>
            private void StopAnimationButtonClick()
            {
                PlaceHolderGameObject.GetComponent<Animation>().Stop();
                _stopAnimationButton.interactable = false;
            }
        }
    }
}
#pragma warning restore 649
