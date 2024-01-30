using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlanningUI : MonoBehaviour
{

    public StabilityWithDirectionsOfInterestUI QualityMeasureUI;

    public ModelLoader SubjectModelImporter;
    private GraspPlanner _planner { 
        get {
           return SubjectModelImporter.GetPlanner();
        } 
    }

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


    void OnEnable()
    {
        UpdateContactPointIndicatorsVisibility();
        UpdateToogleButtonText();
        UpdateSingleStepToggleLabel();


        bool is_selectedContact = _planner.Is_selectedContact;
        MakeContact.gameObject.SetActive(is_selectedContact);
        ContactPointVisualizeToggle.gameObject.SetActive(!is_selectedContact);
        ImaginaryContactPointVisualizeToggle.gameObject.SetActive(is_selectedContact);

        ShowHand.isOn = HandModel.activeSelf;
        HandModel.GetComponent<DOF>().ResetTransform();

        var rb = SubjectModelImporter.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }


    }

    void Start()
    {
        

        _planner.StepAccepted += StepAcceptedEventHandler;

        SubjectModelImporter.ModelImported += (src, args) =>
        {
            _planner.StepAccepted += StepAcceptedEventHandler;
        };

        //UpdateToogleButtonText();
        //UpdateSingleStepToggleLabel();
        _lastQualityValue = null;

        
    }


    void Update()
    {
        if (_planner.Is_planning)
        {
            UpdateScorePanelEachFrame();
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
        if(rb != null)
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
        Color decreased = new Color(165f/255, 45f/255, 45f/255, 1);
        Color increased = new Color(53f/255, 148f/255, 117f/255, 1);

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
