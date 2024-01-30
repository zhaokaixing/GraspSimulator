using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Reflection;

public class ManualGraspUI : MonoBehaviour
{
    public ModelLoader SubjectModelImporter;

    public GraspPlanner Planner { get
        {
            return SubjectModelImporter.GetPlanner();
        } 
    }

    public DOF HandModelDoF;

    public Text Quality;

    public Text ToggleQualityAutoCalculationBtnText;

    public Slider[] DoFSliders;
    public Slider[] WristTransformSliders;

    // dof tilting
    private bool _is_tiltingDoF;
    private int _tiltingDoFIndex;
    private float _DoFTiltingValue;

    // wrist tilting
    private bool _is_tiltingWrist;
    private int _tiltingWristIndex;
    private float _wristTiltingValue;

    private float TotalTime;

    public StabilityWithDirectionsOfInterestUI QualityMeasureUI;

    public PlannerSettingsUI PlannerSettings;

    public GameObject HandModel;

    // references of this UI group
    public Button ToggleBtn;
    public Toggle SingleStepToggle;
    public Toggle ContactPointVisualizeToggle;
    public Toggle ImaginaryContactPointVisualizeToggle;


    public Toggle ShowHand;

    public Button MakeContact;

    // score panel
    public Text QualityValue;
    public Text NextQualityValue;
    public Text TemperatureValue;
    public Text ContactPointsValue;
    public Text IterationsValue;



    // memory
    private double? _lastQualityValue;

    private void Start()
    {
        //InitMove();
    }
    private void OnEnable()
    {
        Planner.Is_AccumulatingContacts = true;
    }

    public void Awake()
    {
        AttachDoFSlidersToTilts();
        AttachWristTransformSlidersToTilts();
    }

    private void Update()
    {
        Quality.text = _planner.ScoreBuffer.ToString();
        HandleMouseInput();
    }

    private void FixedUpdate()
    {
        TotalTime += Time.deltaTime;
        if (TotalTime >= 3)
        {
            IsAutoMove();
            TotalTime = 0;
        }
        AutoMove();  
    }

    public void OnUpdateQualityBtnPress()
    {
        //Planner.Is_updatingScoreOnCollisionStay = true;
        Planner.CalculateQualityMeasure(true);
        //planner.Is_calculatingQualty = false;
    }
        

    public void SetHandleCollisionEnable(bool newVal)
    {
        var rb = HandModelDoF.gameObject.GetComponent<Rigidbody>();

        rb.isKinematic = !newVal;
    }

    private void HandleMouseInput()
    {
        if (_is_tiltingDoF)
        {
           
            if (!Input.GetMouseButton(0))
            {
                DoFSliders[_tiltingDoFIndex].value = 0.5f;
                _is_tiltingDoF = false;
                return;
            }

            if (_tiltingDoFIndex == 23)
            {
                {
                    HandModelDoF.DoFTilts[0] = _DoFTiltingValue;
                    HandModelDoF.UpdateBones(true);
                    HandModelDoF.WristTilts[0] = 1;
                    HandModelDoF.UpdateWrist();
                    HandModelDoF.WristTilts[0] = -1;
                    HandModelDoF.UpdateWrist();
                    
                }


                    HandModelDoF.DoFTilts[1] = _DoFTiltingValue;
                    HandModelDoF.UpdateBones(true);
                    HandModelDoF.WristTilts[0] = 1;
                    HandModelDoF.UpdateWrist();
                    HandModelDoF.WristTilts[0] = -1;
                    HandModelDoF.UpdateWrist();


                
                {
                    HandModelDoF.DoFTilts[2] = _DoFTiltingValue;
                    HandModelDoF.UpdateBones(true);
                    HandModelDoF.WristTilts[0] = 1;
                    HandModelDoF.UpdateWrist();
                    HandModelDoF.WristTilts[0] = -1;
                    HandModelDoF.UpdateWrist();
                }
                HandModelDoF.DoFTilts[3] = _DoFTiltingValue;
                HandModelDoF.UpdateBones(true);
                HandModelDoF.WristTilts[0] = 1;
                HandModelDoF.UpdateWrist();
                HandModelDoF.WristTilts[0] = -1;
                HandModelDoF.UpdateWrist();

                HandModelDoF.DoFTilts[4] = _DoFTiltingValue;
                HandModelDoF.UpdateBones(true);
                HandModelDoF.WristTilts[0] = 1;
                HandModelDoF.UpdateWrist();
                HandModelDoF.WristTilts[0] = -1;
                HandModelDoF.UpdateWrist();
            }
            else if (_tiltingDoFIndex == 24)
            {
                //UnityEngine.Debug.Log("The _tiltingDoFIndex is :" + _tiltingDoFIndex);
                HandModelDoF.DoFTilts[5] = _DoFTiltingValue;
                HandModelDoF.UpdateBones(true);
                HandModelDoF.WristTilts[0] = 1;
                HandModelDoF.UpdateWrist();
                HandModelDoF.WristTilts[0] = -1;
                HandModelDoF.UpdateWrist();

                HandModelDoF.DoFTilts[7] = _DoFTiltingValue;
                HandModelDoF.UpdateBones(true);
                HandModelDoF.WristTilts[0] = 1;
                HandModelDoF.UpdateWrist();
                HandModelDoF.WristTilts[0] = -1;
                HandModelDoF.UpdateWrist();

                if (TotalTime >= 1.5)
                {
                    HandModelDoF.DoFTilts[8] = _DoFTiltingValue;
                    HandModelDoF.UpdateBones(true);
                    HandModelDoF.WristTilts[0] = 1;
                    HandModelDoF.UpdateWrist();
                    HandModelDoF.WristTilts[0] = -1;
                    HandModelDoF.UpdateWrist();
                }
            }
            else if (_tiltingDoFIndex == 25)
            {
                //UnityEngine.Debug.Log("The _tiltingDoFIndex is :" + _tiltingDoFIndex);
                HandModelDoF.DoFTilts[9] = _DoFTiltingValue;
                HandModelDoF.UpdateBones(true);
                HandModelDoF.WristTilts[0] = 1;
                HandModelDoF.UpdateWrist();
                HandModelDoF.WristTilts[0] = -1;
                HandModelDoF.UpdateWrist();

                HandModelDoF.DoFTilts[11] = _DoFTiltingValue;
                HandModelDoF.UpdateBones(true);
                HandModelDoF.WristTilts[0] = 1;
                HandModelDoF.UpdateWrist();
                HandModelDoF.WristTilts[0] = -1;
                HandModelDoF.UpdateWrist();

                if (TotalTime >= 1.5) 
                {
                    HandModelDoF.DoFTilts[12] = _DoFTiltingValue;
                    HandModelDoF.UpdateBones(true);
                    HandModelDoF.WristTilts[0] = 1;
                    HandModelDoF.UpdateWrist();
                    HandModelDoF.WristTilts[0] = -1;
                    HandModelDoF.UpdateWrist();
                }

            }
            else if (_tiltingDoFIndex == 26)
            {
                //UnityEngine.Debug.Log("The _tiltingDoFIndex is :" + _tiltingDoFIndex);
                HandModelDoF.DoFTilts[14] = _DoFTiltingValue;
                HandModelDoF.UpdateBones(true);
                HandModelDoF.WristTilts[0] = 1;
                HandModelDoF.UpdateWrist();
                HandModelDoF.WristTilts[0] = -1;
                HandModelDoF.UpdateWrist();

                HandModelDoF.DoFTilts[16] = _DoFTiltingValue;
                HandModelDoF.UpdateBones(true);
                HandModelDoF.WristTilts[0] = 1;
                HandModelDoF.UpdateWrist();
                HandModelDoF.WristTilts[0] = -1;
                HandModelDoF.UpdateWrist();

                if (TotalTime >= 1.5)
                {
                    HandModelDoF.DoFTilts[17] = _DoFTiltingValue;
                    HandModelDoF.UpdateBones(true);
                    HandModelDoF.WristTilts[0] = 1;
                    HandModelDoF.UpdateWrist();
                    HandModelDoF.WristTilts[0] = -1;
                    HandModelDoF.UpdateWrist();
                }
            }
            else if (_tiltingDoFIndex == 27)
            {
                //UnityEngine.Debug.Log("The _tiltingDoFIndex is :" + _tiltingDoFIndex);
                HandModelDoF.DoFTilts[19] = _DoFTiltingValue;
                HandModelDoF.UpdateBones(true);
                HandModelDoF.WristTilts[0] = 1;
                HandModelDoF.UpdateWrist();
                HandModelDoF.WristTilts[0] = -1;
                HandModelDoF.UpdateWrist();

                HandModelDoF.DoFTilts[21] = _DoFTiltingValue;
                HandModelDoF.UpdateBones(true);
                HandModelDoF.WristTilts[0] = 1;
                HandModelDoF.UpdateWrist();
                HandModelDoF.WristTilts[0] = -1;
                HandModelDoF.UpdateWrist();

                if (TotalTime >= 2)
                {
                    HandModelDoF.DoFTilts[22] = _DoFTiltingValue;
                    HandModelDoF.UpdateBones(true);
                    HandModelDoF.WristTilts[0] = 1;
                    HandModelDoF.UpdateWrist();
                    HandModelDoF.WristTilts[0] = -1;
                    HandModelDoF.UpdateWrist();
                }
            }
            else
            {
                //UnityEngine.Debug.Log("The _tiltingDoFIndex is :" + _tiltingDoFIndex);
                //UnityEngine.Debug.Log("The _DoFTiltingValue is :" + _DoFTiltingValue);
                HandModelDoF.DoFTilts[_tiltingDoFIndex] = _DoFTiltingValue;
                HandModelDoF.UpdateBones(true);
                HandModelDoF.WristTilts[0] = 1;
                HandModelDoF.UpdateWrist();
                HandModelDoF.WristTilts[0] = -1;
                HandModelDoF.UpdateWrist();
            }
        }

        if (_is_tiltingWrist)
        {
            if(!Input.GetMouseButton(0))
            {
                WristTransformSliders[_tiltingWristIndex].value = 0.5f;
                _is_tiltingWrist = false;
                return;
            }

            HandModelDoF.WristTilts[_tiltingWristIndex] = _wristTiltingValue;
            HandModelDoF.UpdateWrist();
        }
    }

    public void ClearContacts()
    {
        Planner.ClearContacts();
        Planner.ClearPreviousContactPointsOnScreen();
        Planner.VisualiseContactPoints();
    }

    public void OnResetHandBtnPress()
    {
        HandModelDoF.ResetTransform();
        //ClearContacts();
    }

    private void AttachDoFSlidersToTilts()
    {
        for(var i=0; i< DoFSliders.Length; i++)
        {
            var slider = DoFSliders[i];
            slider.onValueChanged.AddListener(delegate
            {
                // 0.5 : center of the slider, representing static
                _DoFTiltingValue = slider.value - 0.5f;
                _tiltingDoFIndex = Array.IndexOf(DoFSliders, slider);
                _is_tiltingDoF = true;
            });
        }
    }

    private void AttachWristTransformSlidersToTilts()
    {
        for(var i=0; i<WristTransformSliders.Length; i++)
        {
            var slider = WristTransformSliders[i];
            slider.onValueChanged.AddListener(delegate
            {
                _wristTiltingValue = slider.value - 0.5f;
                _tiltingWristIndex = Array.IndexOf(WristTransformSliders, slider);
                //if(_tiltingWristIndex < 3)
                //{
                //    _wristTiltingValue *= 0.2f; // slows position movement
                //}
                _is_tiltingWrist = true;
            });
        }
    }



    private GraspPlanner _planner
    {
        get
        {
            return SubjectModelImporter.GetPlanner();
        }
    }


    public void OnReturnBtnPress()
    {
        _planner.Is_manualControl = false;
        var rb = HandModelDoF.gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        Planner.Is_AccumulatingContacts = false;

        _planner.ResetAll(true);
        _planner.AcceptedScore = double.NegativeInfinity;
        _planner.logger = null;
        PlannerSettings.ApplyLastSetting();

        var rb1 = SubjectModelImporter.gameObject.GetComponent<Rigidbody>();
        if (rb1 != null)
        {
            rb1.isKinematic = false;
        }
    }

    public void CalculateQuality()
    {
        _planner.Is_QualityUpdatedOnCollisionStay = true;

        //ToggleQualityAutoCalculationBtnText.text =
        //    _planner.Is_QualityUpdatedOnCollisionStay ? "Auto" : "Off";

    }

    static int MotionSimulateCount = 0;
    static int RunningCount = 0;
    public void MotionSimulator()
    {
        RunningCount = 0;
        for (int i = 0; i < 23; i++){
            Invoke("MotionSimulateFlag", 1);
            //MotionSimulateFlag();
            RunningCount++;
        }
        MotionSimulateCount = 0;
    }

    private void MotionSimulate()
    {   
        float DoFTiltingValue = -0.5f;
        if(RunningCount % 2 != 0)
        {
            DoFTiltingValue = -DoFTiltingValue;
        }
        for (int j = 0; j < 300; j++)
        {
            HandModelDoF.DoFTilts[MotionSimulateCount] = DoFTiltingValue;
            HandModelDoF.UpdateBones(true);
            HandModelDoF.WristTilts[0] = 1;
            HandModelDoF.UpdateWrist();
            HandModelDoF.WristTilts[0] = -1;
            HandModelDoF.UpdateWrist();
        }
        MotionSimulateCount++;
    }

    public void SavePosition()
    {
        HandModelDoF.SaveCurrentTransform();
        Planner.ContactPointFilter();
    }

    public void LastSavePosition()
    {
        HandModelDoF.LastSavedTransform();
    }

    bool _isMove = false;
    int clickCount = 0;
    int direction = 1;
    int _TiltingDoFIndex = 0;


    public void IsMove()
    {
        Planner.SurfaceContactPoint();
        Planner.FingerFlag = 1;
        Planner.MotionSimulateFlag = !Planner.MotionSimulateFlag;
        _isMove = !_isMove;
        _TiltingDoFIndex = 0;
        clickCount = 0;
    }

    private void IsAutoMove()
    {
        if (_isMove) {
            Planner.colMoveControl = true;
            clickCount++;

            switch (clickCount)
            {
                case 1:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 3;
                    direction = 1;
                    break;
                case 2:
                    _TiltingDoFIndex = 4;
                    direction = 1;
                    break;
                case 3:
                    _TiltingDoFIndex = 1;
                    direction = -1;
                    break;
                case 4:
                    _TiltingDoFIndex = 3;
                    direction = -1;
                    break;
                case 5:
                    _TiltingDoFIndex = 4;
                    direction = -1;
                    break;
                case 6:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 1;
                    direction = 1;
                    break;
                case 7:
                    _TiltingDoFIndex = 3;
                    direction = -1;
                    break;
                case 8:
                    _TiltingDoFIndex = 4;
                    direction = -1;
                    break;
                case 9:
                    _TiltingDoFIndex = 1;
                    direction = 1;
                    break;
                case 10://thumb 3
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 1;
                    direction = 1;
                    break;
                case 11:
                    _TiltingDoFIndex = 0;
                    direction = 1;
                    break;
                case 12:
                    _TiltingDoFIndex = 2;
                    direction = 1;
                    break;
                case 13:
                    _TiltingDoFIndex = 1;
                    direction = -1;
                    break;
                case 14:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 1;
                    direction = 1;
                    break;
                case 15:
                    _TiltingDoFIndex = 0;
                    direction = -1;
                    break;
                case 16:
                    _TiltingDoFIndex = 2;
                    direction = -1;
                    break;
                case 17:
                    _TiltingDoFIndex = 1;
                    direction = -1;
                    break;
                case 18://index 1
                    Planner.SurfaceContactPoint(1);
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 7;
                    direction = -1;
                    break;
                case 19:
                    _TiltingDoFIndex = 8;
                    direction = -1;
                    break;
                case 20:
                    _TiltingDoFIndex = 5;
                    direction = 1;
                    break;
                case 21:
                    _TiltingDoFIndex = 7;
                    direction = 1;
                    break;
                case 22:
                    _TiltingDoFIndex = 8;
                    direction = 1;
                    break;
                case 23:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 7;
                    direction = -1;
                    break;
                case 24:
                    _TiltingDoFIndex = 8;
                    direction = -1;
                    break;
                case 25:
                    _TiltingDoFIndex = 6;
                    direction = 1;
                    break;
                case 26:
                    _TiltingDoFIndex = 5;
                    direction = 1;
                    break;
                case 27:
                    _TiltingDoFIndex = 7;
                    direction = 1;
                    break;
                case 28:
                    _TiltingDoFIndex = 8;
                    direction = 1;
                    break;
                case 29:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 7;
                    direction = -1;
                    break;
                case 30:
                    _TiltingDoFIndex = 8;
                    direction = -1;
                    break;
                case 31:
                    _TiltingDoFIndex = 6;
                    direction = -1;
                    break;
                case 32:
                    _TiltingDoFIndex = 5;
                    direction = 1;
                    break;
                case 33:
                    _TiltingDoFIndex = 7;
                    direction = 1;
                    break;
                case 34:
                    _TiltingDoFIndex = 8;
                    direction = 1;
                    break;
                case 35:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 5;
                    direction = -1;
                    break;
                case 36:
                    _TiltingDoFIndex = 7;
                    direction = 1;
                    break;
                case 37:
                    _TiltingDoFIndex = 8;
                    direction = 1;
                    break;
                case 38:
                    _TiltingDoFIndex = 5;
                    direction = 1;
                    break;
                case 39:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 5;
                    direction = -1;
                    break;
                case 40:
                    _TiltingDoFIndex = 6;
                    direction = 1;
                    break;
                case 41:
                    _TiltingDoFIndex = 7;
                    direction = 1;
                    break;
                case 42:
                    _TiltingDoFIndex = 8;
                    direction = 1;
                    break;
                case 43:
                    _TiltingDoFIndex = 5;
                    direction = 1;
                    break;
                case 44:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 5;
                    direction = -1;
                    break;
                case 45:
                    _TiltingDoFIndex = 6;
                    direction = -1;
                    break;
                case 46:
                    _TiltingDoFIndex = 7;
                    direction = 1;
                    break;
                case 47:
                    _TiltingDoFIndex = 8;
                    direction = 1;
                    break;
                case 48:
                    _TiltingDoFIndex = 5;
                    direction = 1;
                    break;
                case 49://index 7
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 5;
                    direction = -1;
                    break;
                case 50:
                    _TiltingDoFIndex = 6;
                    direction = 1;
                    break;
                case 51:
                    _TiltingDoFIndex = 5;
                    direction = 1;
                    break;
                case 52:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 5;
                    direction = -1;
                    break;
                case 53:
                    _TiltingDoFIndex = 6;
                    direction = -1;
                    break;
                case 54:
                    _TiltingDoFIndex = 5;
                    direction = 1;
                    break;
                case 55:
                    Planner.SurfaceContactPoint(2);
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 11;
                    direction = -1;
                    break;
                case 56:
                    _TiltingDoFIndex = 12;
                    direction = -1;
                    break;
                case 57:
                    _TiltingDoFIndex = 9;
                    direction = 1;
                    break;
                case 58:
                    _TiltingDoFIndex = 11;
                    direction = 1;
                    break;
                case 59:
                    _TiltingDoFIndex = 12;
                    direction = 1;
                    break;
                case 60:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 11;
                    direction = -1;
                    break;
                case 61:
                    _TiltingDoFIndex = 12;
                    direction = -1;
                    break;
                case 62:
                    _TiltingDoFIndex = 10;
                    direction = 1;
                    break;
                case 63:
                    _TiltingDoFIndex = 9;
                    direction = 1;
                    break;
                case 64:
                    _TiltingDoFIndex = 11;
                    direction = 1;
                    break;
                case 65:
                    _TiltingDoFIndex = 12;
                    direction = 1;
                    break;
                case 66:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 11;
                    direction = -1;
                    break;
                case 67:
                    _TiltingDoFIndex = 12;
                    direction = -1;
                    break;
                case 68:
                    _TiltingDoFIndex = 10;
                    direction = -1;
                    break;
                case 69:
                    _TiltingDoFIndex = 9;
                    direction = 1;
                    break;
                case 70:
                    _TiltingDoFIndex = 11;
                    direction = 1;
                    break;
                case 71:
                    _TiltingDoFIndex = 12;
                    direction = 1;
                    break;
                case 72:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 9;
                    direction = -1;
                    break;
                case 73:
                    _TiltingDoFIndex = 11;
                    direction = 1;
                    break;
                case 74:
                    _TiltingDoFIndex = 12;
                    direction = 1;
                    break;
                case 75:
                    _TiltingDoFIndex = 9;
                    direction = 1;
                    break;
                case 76:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 9;
                    direction = -1;
                    break;
                case 77:
                    _TiltingDoFIndex = 10;
                    direction = 1;
                    break;
                case 78:
                    _TiltingDoFIndex = 11;
                    direction = 1;
                    break;
                case 79:
                    _TiltingDoFIndex = 12;
                    direction = 1;
                    break;
                case 80:
                    _TiltingDoFIndex = 9;
                    direction = 1;
                    break;
                case 81:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 9;
                    direction = -1;
                    break;
                case 82:
                    _TiltingDoFIndex = 10;
                    direction = -1;
                    break;
                case 83:
                    _TiltingDoFIndex = 11;
                    direction = 1;
                    break;
                case 84:
                    _TiltingDoFIndex = 12;
                    direction = 1;
                    break;
                case 85:
                    _TiltingDoFIndex = 9;
                    direction = 1;
                    break;
                case 86:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 9;
                    direction = -1;
                    break;
                case 87:
                    _TiltingDoFIndex = 10;
                    direction = 1;
                    break;
                case 88:
                    _TiltingDoFIndex = 9;
                    direction = 1;
                    break;
                case 89:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 9;
                    direction = -1;
                    break;
                case 90:
                    _TiltingDoFIndex = 10;
                    direction = -1;
                    break;
                case 91:
                    _TiltingDoFIndex = 9;
                    direction = 1;
                    break;
                case 92:
                    Planner.SurfaceContactPoint(3);
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 16;
                    direction = -1;
                    break;
                case 93:
                    _TiltingDoFIndex = 17;
                    direction = -1;
                    break;
                case 94:
                    _TiltingDoFIndex = 14;
                    direction = 1;
                    break;
                case 95:
                    _TiltingDoFIndex = 16;
                    direction = 1;
                    break;
                case 96:
                    _TiltingDoFIndex = 17;
                    direction = 1;
                    break;
                case 97:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 16;
                    direction = -1;
                    break;
                case 98:
                    _TiltingDoFIndex = 17;
                    direction = -1;
                    break;
                case 99:
                    _TiltingDoFIndex = 13;
                    direction = -1;
                    break;
                case 100:
                    _TiltingDoFIndex = 15;
                    direction = -1;
                    break;
                case 101:
                    _TiltingDoFIndex = 14;
                    direction = 1;
                    break;
                case 102:
                    _TiltingDoFIndex = 16;
                    direction = 1;
                    break;
                case 103:
                    _TiltingDoFIndex = 17;
                    direction = 1;
                    break;
                case 104:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 16;
                    direction = -1;
                    break;
                case 105:
                    _TiltingDoFIndex = 17;
                    direction = -1;
                    break;
                case 106:
                    _TiltingDoFIndex = 13;
                    direction = 1;
                    break;
                case 107:
                    _TiltingDoFIndex = 15;
                    direction = 1;
                    break;
                case 108:
                    _TiltingDoFIndex = 14;
                    direction = 1;
                    break;
                case 109:
                    _TiltingDoFIndex = 16;
                    direction = 1;
                    break;
                case 110:
                    _TiltingDoFIndex = 17;
                    direction = 1;
                    break;
                case 111:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 14;
                    direction = -1;
                    break;
                case 112:
                    _TiltingDoFIndex = 16;
                    direction = 1;
                    break;
                case 113:
                    _TiltingDoFIndex = 17;
                    direction = 1;
                    break;
                case 114:
                    _TiltingDoFIndex = 14;
                    direction = 1;
                    break;
                case 115:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 14;
                    direction = -1;
                    break;
                case 116:
                    _TiltingDoFIndex = 13;
                    direction = -1;
                    break;
                case 117:
                    _TiltingDoFIndex = 15;
                    direction = -1;
                    break;
                case 118:
                    _TiltingDoFIndex = 16;
                    direction = 1;
                    break;
                case 119:
                    _TiltingDoFIndex = 17;
                    direction = 1;
                    break;
                case 120:
                    _TiltingDoFIndex = 14;
                    direction = 1;
                    break;
                case 121:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 14;
                    direction = -1;
                    break;
                case 122:
                    _TiltingDoFIndex = 13;
                    direction = 1;
                    break;
                case 123:
                    _TiltingDoFIndex = 15;
                    direction = 1;
                    break;
                case 124:
                    _TiltingDoFIndex = 16;
                    direction = 1;
                    break;
                case 125:
                    _TiltingDoFIndex = 17;
                    direction = 1;
                    break;
                case 126:
                    _TiltingDoFIndex = 14;
                    direction = 1;
                    break;
                case 127:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 14;
                    direction = -1;
                    break;
                case 128:
                    _TiltingDoFIndex = 13;
                    direction = -1;
                    break;
                case 129:
                    _TiltingDoFIndex = 15;
                    direction = -1;
                    break;
                case 130:
                    _TiltingDoFIndex = 14;
                    direction = 1;
                    break;
                case 131:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 14;
                    direction = -1;
                    break;
                case 132:
                    _TiltingDoFIndex = 13;
                    direction = 1;
                    break;
                case 133:
                    _TiltingDoFIndex = 15;
                    direction = 1;
                    break;
                case 134:
                    _TiltingDoFIndex = 14;
                    direction = 1;
                    break;
                case 135:
                    Planner.SurfaceContactPoint(4);
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 21;
                    direction = -1;
                    break;
                case 136:
                    _TiltingDoFIndex = 22;
                    direction = -1;
                    break;
                case 137:
                    _TiltingDoFIndex = 19;
                    direction = 1;
                    break;
                case 138:
                    _TiltingDoFIndex = 21;
                    direction = 1;
                    break;
                case 139:
                    _TiltingDoFIndex = 22;
                    direction = 1;
                    break;
                case 140:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 21;
                    direction = -1;
                    break;
                case 141:
                    _TiltingDoFIndex = 22;
                    direction = -1;
                    break;
                case 142:
                    _TiltingDoFIndex = 18;
                    direction = -1;
                    break;
                case 143:
                    _TiltingDoFIndex = 20;
                    direction = -1;
                    break;
                case 144:
                    _TiltingDoFIndex = 19;
                    direction = 1;
                    break;
                case 145:
                    _TiltingDoFIndex = 21;
                    direction = 1;
                    break;
                case 146:
                    _TiltingDoFIndex = 22;
                    direction = 1;
                    break;
                case 147:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 21;
                    direction = -1;
                    break;
                case 148:
                    _TiltingDoFIndex = 22;
                    direction = -1;
                    break;
                case 149:
                    _TiltingDoFIndex = 18;
                    direction = 1;
                    break;
                case 150:
                    _TiltingDoFIndex = 20;
                    direction = 1;
                    break;
                case 151:
                    _TiltingDoFIndex = 19;
                    direction = 1;
                    break;
                case 152:
                    _TiltingDoFIndex = 21;
                    direction = 1;
                    break;
                case 153:
                    _TiltingDoFIndex = 22;
                    direction = 1;
                    break;
                case 154:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 19;
                    direction = -1;
                    break;
                case 155:
                    _TiltingDoFIndex = 21;
                    direction = 1;
                    break;
                case 156:
                    _TiltingDoFIndex = 22;
                    direction = 1;
                    break;
                case 157:
                    _TiltingDoFIndex = 19;
                    direction = 1;
                    break;
                case 158:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 19;
                    direction = -1;
                    break;
                case 159:
                    _TiltingDoFIndex = 18;
                    direction = -1;
                    break;
                case 160:
                    _TiltingDoFIndex = 20;
                    direction = -1;
                    break;
                case 161:
                    _TiltingDoFIndex = 21;
                    direction = 1;
                    break;
                case 162:
                    _TiltingDoFIndex = 22;
                    direction = 1;
                    break;
                case 163:
                    _TiltingDoFIndex = 19;
                    direction = 1;
                    break;
                case 164:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 19;
                    direction = -1;
                    break;
                case 165:
                    _TiltingDoFIndex = 18;
                    direction = 1;
                    break;
                case 166:
                    _TiltingDoFIndex = 20;
                    direction = 1;
                    break;
                case 167:
                    _TiltingDoFIndex = 21;
                    direction = 1;
                    break;
                case 168:
                    _TiltingDoFIndex = 22;
                    direction = 1;
                    break;
                case 169:
                    _TiltingDoFIndex = 19;
                    direction = 1;
                    break;
                case 170:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 19;
                    direction = -1;
                    break;
                case 171:
                    _TiltingDoFIndex = 18;
                    direction = -1;
                    break;
                case 172:
                    _TiltingDoFIndex = 20;
                    direction = -1;
                    break;
                case 173:
                    _TiltingDoFIndex = 19;
                    direction = 1;
                    break;
                case 174:
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    _TiltingDoFIndex = 19;
                    direction = -1;
                    break;
                case 175:
                    _TiltingDoFIndex = 18;
                    direction = 1;
                    break;
                case 176:
                    _TiltingDoFIndex = 20;
                    direction = 1;
                    break;
                case 177:
                    _TiltingDoFIndex = 19;
                    direction = 1;
                    break;
                case 178:
                    Planner.SurfaceContactPoint(5);
                    ClearContacts();
                    HandModelDoF.LastSavedTransform();
                    break;
                default:
                    break;
            }
        }
    }
    private void AutoMove()
    {
        if (Planner.colMoveControl && _isMove)
        {
            HandModelDoF.DoFTilts[_TiltingDoFIndex] = 0.8f * direction;
            HandModelDoF.UpdateBones(true);
            HandModelDoF.WristTilts[0] = 1;
            HandModelDoF.UpdateWrist();
            HandModelDoF.WristTilts[0] = -1;
            HandModelDoF.UpdateWrist();
        }
    }

    public void InitMove()
    {
        Type test_type = Planner.GetType();
        FieldInfo fieldInfo = test_type.GetField("_contacts", BindingFlags.NonPublic | BindingFlags.Instance);
        List<ContactPoint> _contacts = (List<ContactPoint>)fieldInfo.GetValue(Planner);
        fieldInfo = test_type.GetField("_contactPointsFilter", BindingFlags.NonPublic | BindingFlags.Instance);
        ContactPoint[] _contactPointsFilter = (ContactPoint[])fieldInfo.GetValue(Planner);
        int count = 0; 
        foreach (var contact in _contacts)
        {
            count++;
            Vector3 pos = contact.point;
            UnityEngine.Debug.Log("_contacts " + count + " = " + pos);
        }
        count = 0;
        foreach (var contact in _contactPointsFilter)
        {
            count++;
            Vector3 pos = contact.point;
            UnityEngine.Debug.Log("_contactPointsFilter " + count + " = " + pos);
        }
    }

    public void OnMakeContactBtnPress()
    {
        _planner.MakeContactOnClosestDefinedContact();
    }

    // when the return button is pressed
    public void OnReturn()
    {
        _planner.ResetAll(true);
        _planner.AcceptedScore = double.NegativeInfinity;
        _planner.logger = null;
        PlannerSettings.ApplyLastSetting();

        var rb = SubjectModelImporter.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    // update score
    private void UpdateScorePanelOnStepApplied()
    {
        var quality = _planner.AcceptedScore;
        //print(_planner.AcceptedScore);

        // reduced intensity colors
        Color decreased = new Color(165f / 255, 45f / 255, 45f / 255, 1);
        Color increased = new Color(53f / 255, 148f / 255, 117f / 255, 1);

        var lastQualityStr = QualityValue.text;
        QualityValue.text = quality.ToString();
        // change text color according to delta (positive is green negative is red)
        if (_lastQualityValue.HasValue)
        {
            double delta = quality - _lastQualityValue.Value;
            QualityValue.color = delta > 0 ? increased : decreased;
        }
        else
        {
            QualityValue.color = Color.black;
        }
        _lastQualityValue = quality;

    }

    private void UpdateScorePanelEachFrame()
    {
        var nextQuality = _planner.NextScore;
        var temperature = _planner.Temperature;
        var contactPoints = _planner.AccpetedContactCount;
        var iterations = _planner.Iterations;

        NextQualityValue.text = nextQuality.ToString();
        TemperatureValue.text = temperature.ToString();
        ContactPointsValue.text = contactPoints.ToString();
        IterationsValue.text = iterations.ToString();

    }

    // toggle planner state (running / paused)
    public void TooglePlanner()
    {
        _planner.TogglePlanner();
        UpdateToogleButtonText();
    }


    private void StepAcceptedEventHandler(object source, EventArgs args)
    {
        UpdateToogleButtonText();
        UpdateScorePanelOnStepApplied();
    }

    // Display the text (pause / continue) of the toogle button according to the state of the planner 
    private void UpdateToogleButtonText()
    {
        ToggleBtn.GetComponentInChildren<Text>().text = _planner.Is_planning ? "pause" : "continue";
    }

    public void SetSingleStep(bool newValue)
    {
        _planner.SetSingleStep(newValue);
    }

    public void ToggleContactPointIndicators(bool newVal)
    {
        _planner.SetContactIndicatorActive(newVal);

    }

    public void ToggleDirecionOfInterestIndicaators(bool newVal)
    {
        if (newVal)
        {
            QualityMeasureUI.VisualizeForceDirections();
        }
        else
        {
            QualityMeasureUI.ClearForceDirectionIndicators();
        }
    }


    private void UpdateContactPointIndicatorsVisibility()
    {
        var is_enabled = _planner.Is_contactPointsVisualized;

        if (_planner.Is_selectedContact)
        {
            ImaginaryContactPointVisualizeToggle.isOn = is_enabled;
        }
        else
        {
            ContactPointVisualizeToggle.isOn = is_enabled;
        }


    }

    private void UpdateSingleStepToggleLabel()
    {
        SingleStepToggle.isOn = _planner.Is_singleStep;
    }



}
