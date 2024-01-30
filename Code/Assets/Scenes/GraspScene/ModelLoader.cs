using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TriLib;
using TriLib.Samples;

public class ModelLoader : MonoBehaviour
{
    public GameObject ImportedGameObject { get; private set; }


    private Vector3 _modelOriginalLocalScale;
    public float ModelLocalScale { get; private set; } = 0.005f;

  

    [SerializeField]
    private Transform _blendShapesContainerTransform = null;
    /// <summary>
    /// <see cref="AnimationText"/> prefab reference.
    /// </summary>

    [SerializeField]
    private BlendShapeControl _blendShapeControlPrefab = null;



    public event EventHandler<GameObject> ModelImported;

    public GraspPlanner GetPlanner()
    {
        if (ImportedGameObject != gameObject)
        {
            return ImportedGameObject.GetComponent<GraspPlanner>();
        }
        else
        {
            return GetComponent<GraspPlanner>();
        }
    }

    protected virtual void OnSubjectImported()
    {
        if (ModelImported != null)
        {
            ModelImported(this, ImportedGameObject);
        }
    }

    private void Awake()
    {
        ImportedGameObject = gameObject;

        AttachMeshColliders();
        AdjustTransform(true);
        //AttachScripts();
    }

    private void HideInitialSampleSubject(object source, GameObject importedSubject)
    {
        gameObject.SetActive(false);
    }




    void Start()
    {
        ModelImported += HideInitialSampleSubject;

        //LoadIocal(@"C:\github\GraspSimulator\Assets\Models\ExsampleObjects\iphone5c\Iphone 5cAmended.fbx");
    }

    private void DestroyPreviousModel()
    {
        if (ImportedGameObject != gameObject)
        {
            Destroy(ImportedGameObject);
            ImportedGameObject = null;
        }
    }


    /// <summary>
    /// Loads a model from the given filename, file bytes or browser files.
    /// </summary>
    /// <param name="filename">Model filename.</param>
    /// <param name="fileBytes">Model file bytes.</param>
    /// <param name="browserFilesCount">Browser files count.</param>
    public void LoadIocal(string filename, byte[] fileBytes = null, int browserFilesCount = -1)
    {
        DestroyPreviousModel();
        gameObject.SetActive(true);

        var assetLoaderOptions = GetAssetLoaderOptions();
        using (var assetLoader = new AssetLoader())
        {
            assetLoader.OnMetadataProcessed += AssetLoader_OnMetadataProcessed;
            try
            {
                if (fileBytes != null && fileBytes.Length > 0)
                {
                    ImportedGameObject = assetLoader.LoadFromMemoryWithTextures(fileBytes, FileUtils.GetFileExtension(filename), assetLoaderOptions, ImportedGameObject);
                }
                else if (!string.IsNullOrEmpty(filename))
                {
                    ImportedGameObject = assetLoader.LoadFromFileWithTextures(filename, assetLoaderOptions);
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
        PostLoadSetup();
 
    }

    private void HandleException(Exception exception)
    {
        print(exception);
    }

    private void CheckForValidModel(AssetLoaderBase assetLoader)
    {
        if (assetLoader.MeshData == null || assetLoader.MeshData.Length == 0)
        {
            throw new Exception("File contains no meshes");
        }
        //print(assetLoader.MeshData);
    }

    private void PostLoadSetup()
    {
        var mainCamera = Camera.main;
        //print(PlaceHolderGameObject.transform);

        
  

        //print(ImportedGameObject.transform.position);


        //mainCamera.FitToBounds(ImportedGameObject.transform, 3f);
        //_backgroundCanvas.planeDistance = mainCamera.farClipPlane * 0.99f;

        DestroyItems();


        var skinnedMeshRenderers = ImportedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        //print(skinnedMeshRenderers);
        if (skinnedMeshRenderers != null)
        {
            var hasBlendShapes = false;
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                //print(skinnedMeshRenderer.gameObject);
                if (!hasBlendShapes && skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                {

                    hasBlendShapes = true;
                }
                for (var i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                {
                    CreateBlendShapeItem(skinnedMeshRenderer, skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i), i);
                }
            }
        }


        AttachMeshColliders();
        AdjustTransform();
        AttachScripts();

        OnSubjectImported();
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
    /// Destroys all objects in the containers.
    /// </summary>
    public void DestroyItems()
    {
        //foreach (Transform innerTransform in _containerTransform)
        //{
        //    Destroy(innerTransform.gameObject);
        //}
        //foreach (Transform innerTransform in _blendShapesContainerTransform)
        //{
        //    Destroy(innerTransform.gameObject);
        //}
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

        // transparency mode
        assetLoaderOptions.MaterialTransparencyMode = MaterialTransparencyMode.Alpha;

        // shading
        assetLoaderOptions.MaterialShadingMode = MaterialShadingMode.Roughness;


        assetLoaderOptions.AddAssetUnloader = true;
        assetLoaderOptions.AdvancedConfigs.Add(AssetAdvancedConfig.CreateConfig(AssetAdvancedPropertyClassNames.FBXImportDisableDiffuseFactor, true));
        return assetLoaderOptions;
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
        //Debug.Log("Found metadata of type [" + metadataType + "] at index [" + metadataIndex + "] and key [" + metadataKey + "] with value [" + metadataValue + "]");
    }

    private void AttachMeshColliders()
    {
        var meshFilters = ImportedGameObject.GetComponentsInChildren<MeshFilter>();

        foreach (var meshFilter in meshFilters)
        {
            GameObject go = meshFilter.gameObject;
            go.AddComponent<MeshCollider>();
            var mc = go.GetComponent<MeshCollider>();
            mc.sharedMesh = meshFilter.mesh;
            mc.convex = true;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    var rb = GetComponent<Rigidbody>();

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(rb.worldCenterOfMass, 1);
    //}


    // called after attachment of mesh colliders 
    // because calculation of centerOfMass is dependent on  these colliders (maybe ?)
    private void AdjustTransform(bool is_original = false)
    {

        Rigidbody newRb = ImportedGameObject.GetComponent<Rigidbody>(); ;
        if(newRb == null)
        {
            ImportedGameObject.AddComponent<Rigidbody>();
            newRb = ImportedGameObject.GetComponent<Rigidbody>();

            newRb.useGravity = false;
            newRb.isKinematic = false;
            newRb.constraints = RigidbodyConstraints.FreezeAll;
        }


        if (is_original)
        {
            Physics.SyncTransforms();
            newRb.ResetCenterOfMass();
            return;
        }


        var oldRb = GetComponent<Rigidbody>();


        _modelOriginalLocalScale = ImportedGameObject.transform.localScale;

        ApplyScale(ModelLocalScale);

        ImportedGameObject.transform.position = oldRb.worldCenterOfMass;

        Physics.SyncTransforms();
        newRb.ResetCenterOfMass();

        var centerOfMassOffset = oldRb.worldCenterOfMass - newRb.worldCenterOfMass;
        ImportedGameObject.transform.position += centerOfMassOffset;

    }

    public void ApplyScale(float multiplier)
    {
        if (ImportedGameObject == null) return;

        ModelLocalScale = multiplier;

        Rigidbody rb = ImportedGameObject.GetComponent<Rigidbody>();
        var oldCenterOfMass = rb.worldCenterOfMass;

        ImportedGameObject.transform.localScale = _modelOriginalLocalScale * multiplier;
        Physics.SyncTransforms();
        rb.ResetCenterOfMass();
        var centerOfMassOffset = oldCenterOfMass - rb.worldCenterOfMass;
        ImportedGameObject.transform.position += centerOfMassOffset;
    }

    private void AttachScripts()
    {
        ImportedGameObject.AddComponent<GraspPlanner>();
        GraspPlanner newPlanner = ImportedGameObject.GetComponent<GraspPlanner>();
        GraspPlanner thisPlanner = gameObject.GetComponent<GraspPlanner>();

        newPlanner.Init(thisPlanner.HandModel, ImportedGameObject, thisPlanner.ContactPointPrefab, thisPlanner.ContactPointPrefabRed);
    }


}
