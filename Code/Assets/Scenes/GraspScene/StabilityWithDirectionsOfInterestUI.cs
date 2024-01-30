using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;




public class StabilityWithDirectionsOfInterestUI : MonoBehaviour
{
    // refs set in unity
    public GameObject ForceDirectionIndicatorPrefab;

    public ModelLoader SubjectModelImporter;
    //public CameraWireFrameMode cameraWireFrameMode;
    public Slider FrictionCoefficient;






    public GameObject Subject { get { return _planner.gameObject; } }

    private PlannerSettingsUI _plannerSettingsUI;

    private GraspPlanner _planner { get { return SubjectModelImporter.GetPlanner(); } }


    private List<Ray> _forceDirections { get { return _planner.QualityMeasureForceDirections; } }


    private List<GameObject> _forceDirectionIndicators = new List<GameObject>();

    private GameObject _additionPreviewIndicator;

    private Camera _camera;

    void Awake()
    {
        _plannerSettingsUI = GetComponentInParent<PlannerSettingsUI>();

        _camera = Camera.main;
    }

    public void FetchInfoFromPlanner()
    {
        FrictionCoefficient.value = _planner.FrictionCoefficient;
        FrictionCoefficient.GetComponentInChildren<Text>().text = FrictionCoefficient.value.ToString();
    }

    private void Start()
    {
        FetchInfoFromPlanner();
    }

    public void UpdateFrictionCoefficient(float value)
    {
        _planner.FrictionCoefficient = value;
        FrictionCoefficient.GetComponentInChildren<Text>().text = FrictionCoefficient.value.ToString();

    }



    public void RemoveAllForceDirections()
    {
        ClearForceDirectionIndicators();
        _forceDirections.Clear();
        _planner.UpdateQualityMeasureWrenchDirections();
    }

    // in radians
    public static Vector3 SphericalToCartesian(float radius, float theta, float phi)
    {
        float xyCast = radius * Mathf.Sin(theta);
        return new Vector3(xyCast * Mathf.Cos(phi), xyCast * Mathf.Sin(phi), radius * Mathf.Cos(theta));
    }


    public void ApproximateAllDirections(int numberOfVectors, Vector3 position)
    {
        int thetaCount = (int)Math.Floor(Math.Sqrt(numberOfVectors / 2));
        int phiCount = thetaCount * 2;
        float thetaInterval = (float)Math.PI / thetaCount;
        float phiInterval = (float)Math.PI * 2 / phiCount;

        const float radius = 1f;

        // north pole
        _forceDirections.Add(new Ray(position, new Vector3(0, 0, radius)));

        // south pole
        _forceDirections.Add(new Ray(position, new Vector3(0, 0, -radius)));

        float theta = 0;
        for (var i = 0; i < thetaCount; i++)
        {
            theta += thetaInterval;
            float phi = 0;
            for (var j = 0; j < phiCount + 1; j++)
            {
                var cartesian = SphericalToCartesian(radius, phi, theta);
                _forceDirections.Add(new Ray(position, cartesian));
                phi += phiInterval;
            }

        }

        print(string.Format("approximated with {0} vectors", ((thetaCount - 1) * (phiCount + 1)) + 2));

        _planner.UpdateQualityMeasureWrenchDirections();
        VisualizeForceDirections(true);

    }

    public void RemoveForceDirection(GameObject indicator)
    {
        var index = _forceDirectionIndicators.IndexOf(indicator);
        if (index < 0) return;

        if (_forceDirections.Count != _forceDirectionIndicators.Count)
            throw new Exception("_forceDirections.Count != _forceDirectionIndicators.Count");

        Destroy(indicator);
        _forceDirectionIndicators.RemoveAt(index);
        _forceDirections.RemoveAt(index);
        _planner.UpdateQualityMeasureWrenchDirections();
    }

    public GameObject AddForceDirection(Ray ray)
    {
        _forceDirections.Add(ray);
        _planner.UpdateQualityMeasureWrenchDirections();
        return VisualizeForceDirection(ray, true);
    }

    public void RotateForceDirection(GameObject indicator, Vector3 direction)
    {
        var index = GetIndicatorIndex(indicator);

        var point = _forceDirections[index].origin;
        _forceDirections[index] = new Ray(point, direction);
        _planner.UpdateQualityMeasureWrenchDirections();

        indicator.transform.up = direction;
    }

    //public void RotateLastForceDirection(Vector3 direction)
    //{

    //    int lastIndex = _forceDirections.Count - 1;
    //    var point = _forceDirections[lastIndex].origin;

    //    _forceDirections[lastIndex] = new Ray(point, direction);
    //    _planner.UpdateQualityMeasureWrenchDirections();

    //    var indicator = _forceDirectionIndicators[lastIndex];
    //    indicator.transform.up = direction;


    //    //VisualizeForceDirections();
    //}

    private void UpdateForeceDirection(GameObject indicator, int indicatorIndex)
    {
        _forceDirections[indicatorIndex] = 
            new Ray(indicator.transform.position, indicator.transform.up);
        _planner.UpdateQualityMeasureWrenchDirections();
    }

    private GameObject VisualizeForceDirection(Ray ray, bool is_editMode = false)
    {
        var indicator =
                Instantiate(ForceDirectionIndicatorPrefab, ray.origin, Quaternion.identity);

        indicator.transform.up = ray.direction;

        var collider = indicator.GetComponentInChildren<SphereCollider>();
        collider.enabled = is_editMode;
        //indicator.transform.up = forceWithPosition.force;

        _forceDirectionIndicators.Add(indicator);
        return indicator;
    }


    // is_editMode : if set to true, the indicators will be added sphere colliders so raycast could hit them
    public void VisualizeForceDirections(bool is_editMode = false)
    {
        ClearForceDirectionIndicators();
        foreach (var forceWithPosition in _forceDirections)
        {
            VisualizeForceDirection(forceWithPosition, is_editMode);
        }

        //cameraWireFrameMode.Is_enabled = true;
        //_planner.SetSubjectTransparency(0.1f);
    }

    public void ClearForceDirectionIndicators()
    {
        foreach (var indicator in _forceDirectionIndicators)
        {
            Destroy(indicator);
        }
        _forceDirectionIndicators.Clear();
    }

    public void ShowPreviewIndicator()
    {
        if(_additionPreviewIndicator == null)
        {
            _additionPreviewIndicator = Instantiate(ForceDirectionIndicatorPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            _additionPreviewIndicator.SetActive(true);
        }
    }

    public void HidePreviewIndicator()
    {
        if (_additionPreviewIndicator)
        {
            _additionPreviewIndicator.SetActive(false);
        }

    }

    private int GetIndicatorIndex(GameObject indicator)
    {
        var index = _forceDirectionIndicators.IndexOf(indicator);
        if (index < 0) throw new InvalidOperationException(
            "invalid force indicator:" + (indicator ? indicator.ToString() : "null"));
        return index;
    }

    public void MoveIndicator(GameObject indicator, Vector3 destination)
    {
        var index = GetIndicatorIndex(indicator);
        indicator.transform.position = destination;

        UpdateForeceDirection(indicator, index);

    }

    public void MovePreviewIndicator(Vector3 destination)
    {
        _additionPreviewIndicator.transform.position = destination;
    }

    public GameObject GetClosestIndicatorFromCursor()
    {
        var cameraToCursorRay = _camera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (!Physics.Raycast(cameraToCursorRay, out hit)) return null;

        var colliderHit = hit.collider;

        foreach(var indicator in _forceDirectionIndicators)
        {
            if (colliderHit.gameObject.transform.IsChildOf(indicator.transform)) return indicator;
        }
        

        return null;
    }


    //public GameObject GetClosestIndicatorFromWorldPosition(Vector3 position)
    //{
    //    var indicatorCount = _forceDirectionIndicators.Count;
    //    if (indicatorCount == 0) return null;

    //    GameObject closestIndicator = _forceDirectionIndicators
    //        .Aggregate((i1, i2) =>
    //       Vector3.Distance(i1.transform.position, position) < Vector3.Distance(i2.transform.position, position) ?
    //       i1 : i2);

    //    return closestIndicator;
        
    //}

    public static void EnanbleIndicatorOutline(GameObject indicator)
    {
        if(indicator == null) return;

        var outlineHandler = indicator.GetComponent<OutlineHandler>();
        if(outlineHandler == null)
        {
            outlineHandler = indicator.AddComponent<OutlineHandler>();
        }
        else
        {
            outlineHandler.SetEnable(true);
        }
    }

    public static void DisableIndicatorOutline(GameObject indicator)
    {
        if (indicator == null) return;

       var outlineHandler = indicator.GetComponent<OutlineHandler>();
        if(outlineHandler != null)
        {
            outlineHandler.SetEnable(false);
        }

    }

}
