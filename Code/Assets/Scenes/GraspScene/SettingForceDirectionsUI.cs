using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

using CoreScript = StabilityWithDirectionsOfInterestUI;

public class SettingForceDirectionsUI : MonoBehaviour
{
    // drag the parent object of the button which enters force direction edit mode here 
    //(the CoreScript is supposed to be attached to it)
    public CoreScript coreScript;

    public Text UserHelp;


    public GameObject SetAllDirecionApproxCountBox;
    public Button SetApproxCountBtn;
    public InputField VectorCountForApproximateAllDirections;

    private int _approxVectorCount;

    enum Mode
    {
        Normal,
        Adding,
        RotatingNewlyAdded,
        Removing,
        MoveSelect,
        Moving,
        RotateSelect,
        Rotating,
        ApproxAllDir,
    }


    private Mode _mode = Mode.Normal;

    private Camera _camera;

    private GameObject _subject { get { return coreScript.Subject; } }

    private Vector3? _closestPointOnSubjectFromCursor;

    // temp
    private GameObject _lastSelectedIndicator;

    private class ShowApproxVectorCount : MonoBehaviour
    {
        public SettingForceDirectionsUI settingForceDirectionsUI;

        void OnEnable()
        {
            settingForceDirectionsUI.VectorCountForApproximateAllDirections.text =
                settingForceDirectionsUI._approxVectorCount.ToString();
        }
    }

    void Start()
    {
        _camera = Camera.main;
        //SetApproxCountBtn.GetComponentInChildren<Text>().text = "\u2699".ToString();

        VectorCountForApproximateAllDirections.gameObject
            .AddComponent<ShowApproxVectorCount>()
            .settingForceDirectionsUI = this;


    }


    void Update()
    {
        HandleMouseMove();
        HandleMouseDown();
    }

    public void ClearAllForceDirections()
    {
        coreScript.RemoveAllForceDirections();
    }


    private void HandleMouseMove()
    {
        coreScript.HidePreviewIndicator();

        switch (_mode)
        {
            case Mode.Normal:
                UserHelp.text = "Red spheres denote the positions of the forces applied\n" +
                    "Arrows denote the directions of the forces";

                return;

            case Mode.Adding:
            case Mode.ApproxAllDir:
                UserHelp.text = "Add a new force at mouse cursor position.";
                FollowCursorWithPreviewIndicator();

                return;

            case Mode.RotatingNewlyAdded:
            case Mode.Rotating:
                UserHelp.text = "Rotate with mouse cursor.\nLeft-click to apply.";
                var cameraToCursorRay = _camera.ScreenPointToRay(Input.mousePosition);
                coreScript.RotateForceDirection(_lastSelectedIndicator, cameraToCursorRay.direction);
                return;

            case Mode.Moving:
                UserHelp.text = "Move with mouse cursor.\nLeft-click to apply.";
                FollowCursorWithIndicator(_lastSelectedIndicator);
                return;


            case Mode.Removing:
            case Mode.MoveSelect:
            case Mode.RotateSelect:
                UserHelp.text = "Use mouse cursor to select a force direction indicator";
                FindAndOutlineCloestIndicatorFromCursor();
                return;

            default:
                UserHelp.text = "";
                return;
        }
    }


    private void HandleMouseDown()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        switch (_mode)
        {
            case Mode.Adding:
                if (!_closestPointOnSubjectFromCursor.HasValue) return;

                _lastSelectedIndicator = coreScript.
                    AddForceDirection(new Ray(_closestPointOnSubjectFromCursor.Value, Vector3.down));

                _mode = Mode.RotatingNewlyAdded;
                return;

            case Mode.RotatingNewlyAdded:
                _mode = Mode.Adding;
                return;

            case Mode.Removing:
                coreScript.RemoveForceDirection(_lastSelectedIndicator);
                _lastSelectedIndicator = null;
                return;

            case Mode.MoveSelect:
                if (_lastSelectedIndicator)
                {
                    _mode = Mode.Moving;
                }

                return;

            case Mode.RotateSelect:
                if (_lastSelectedIndicator)
                {
                    _mode = Mode.Rotating;
                }

                return;

            case Mode.Moving:
                _mode = Mode.MoveSelect;
                _lastSelectedIndicator = null;
                return;
            case Mode.Rotating:
                _mode = Mode.RotateSelect;
                _lastSelectedIndicator = null;
                return;

            case Mode.ApproxAllDir:
                var point = _closestPointOnSubjectFromCursor;
                if (point.HasValue)
                {
                    coreScript.ApproximateAllDirections(_approxVectorCount, point.Value);
                    _mode = Mode.Normal;
                }
                
                return;
            default:
                return;
        }
    }

    private void FollowCursorWithIndicator(GameObject indicator)
    {
        _closestPointOnSubjectFromCursor = GetClosestPointOnSubjectFromCursor();
        if (_closestPointOnSubjectFromCursor.HasValue)
        {
            coreScript.MoveIndicator(indicator, _closestPointOnSubjectFromCursor.Value);
        }
    }

    private void FollowCursorWithPreviewIndicator()
    {
        _closestPointOnSubjectFromCursor = GetClosestPointOnSubjectFromCursor();
        if (_closestPointOnSubjectFromCursor.HasValue)
        {
            coreScript.ShowPreviewIndicator();
            coreScript.MovePreviewIndicator(_closestPointOnSubjectFromCursor.Value);
        }
    }

    private void FindAndOutlineCloestIndicatorFromCursor()
    {
        var closestIndicator = coreScript.GetClosestIndicatorFromCursor();
        if (closestIndicator != null)
        {
            if (closestIndicator != _lastSelectedIndicator)
            {
                CoreScript.DisableIndicatorOutline(_lastSelectedIndicator);
                _lastSelectedIndicator = closestIndicator;
                CoreScript.EnanbleIndicatorOutline(closestIndicator);
            }
        }
    }

    public void OnAddButtonPress()
    {
        _lastSelectedIndicator = null;
        _mode = Mode.Adding;
    }

    public void OnRemoveButtonPress()
    {
        _lastSelectedIndicator = null;
        _mode = Mode.Removing;
    }

    public void OnMoveButtonPress()
    {
        _lastSelectedIndicator = null;
        _mode = Mode.MoveSelect;
    }

    public void OnRotateButtonPress()
    {
        _lastSelectedIndicator = null;
        _mode = Mode.RotateSelect;
    }

    public void OnExitButtonPress()
    {
        _lastSelectedIndicator = null;
        _mode = Mode.Normal;
        coreScript.ClearForceDirectionIndicators();
    }

    public void OnAllDirectionsPress()
    {
        if(_approxVectorCount <= 0)
        {
            SetApproxCountBtn.onClick.Invoke();
            return;
        }
        _mode = Mode.ApproxAllDir;

    }

    public void OnApplySetAllDirectionsButtonPress()
    {
        try
        {
            var count = int.Parse(VectorCountForApproximateAllDirections.text);
            if (count <= 0) throw new FormatException();
            

            _approxVectorCount = count;

            VectorCountForApproximateAllDirections.image.color = Color.white;
            SetAllDirecionApproxCountBox.SetActive(false);
            _mode = Mode.ApproxAllDir;
        }
        catch (FormatException)
        {
            VectorCountForApproximateAllDirections.text = "positive integer";
            VectorCountForApproximateAllDirections.image.color = Color.red;
        }
    }

    public void OnRemoveAllPress()
    {
        coreScript.RemoveAllForceDirections();
    }

    public void ApproxOnCenterOfMass()
    {
        if(_approxVectorCount <=0)
        {
            SetApproxCountBtn.onClick.Invoke();
            return;
        }
        var rb = _subject.GetComponent<Rigidbody>();
        coreScript.ApproximateAllDirections(_approxVectorCount, rb.worldCenterOfMass);
    }

    public void AddGravityDirection()
    {
        var rb = _subject.GetComponent<Rigidbody>();
        coreScript.AddForceDirection(new Ray(rb.worldCenterOfMass, Vector3.down));
    }

    //private Vector3 GetMouseCursorWorldPosition()
    //{
    //    var mousePos = Input.mousePosition;
    //    mousePos.z = 1.0f;  //cannot be zero
    //    return _camera.ScreenToWorldPoint(mousePos);
    //}


    private Vector3? GetClosestPointOnSubjectFromCursor()
    {

        var cameraToCursorRay = _camera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (!Physics.Raycast(cameraToCursorRay, out hit)) return null;

        var colliderHit = hit.collider;

        if (!colliderHit.gameObject.transform.IsChildOf(_subject.transform)) return null;

        return hit.point;

    }
}
