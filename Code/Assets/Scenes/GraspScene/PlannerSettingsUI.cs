using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlannerSettingsUI : MonoBehaviour
{

    public ModelLoader SubjectModelImporter;
    public GraspPlanner Planner
    {
        get
        {
            return SubjectModelImporter.GetPlanner();
        }
    }

    // UI component refs
    public InputField InitialTemperature;
    public InputField Alpha;
    public InputField Epsilon;
    public Toggle EpsilonEnable;
    public InputField MaxIterations;
    public Toggle MaxIterationEnable;
    public InputField ValidStepThreshold;

    public Toggle EigenEnable;
    public Slider EigenPCCount;

    public InputField WirstPositionRange;
    public InputField ScoreMultiplier;
    public InputField DistancePenaltyMultiplier;



    // memory
    private double _initialTemperature;
    private double _alpha;
    private double _epsilon;
    private bool _is_epsilonEnabled;
    private int _maxIterations;
    private bool _is_maxIterationsEnabled;
    private double _validStepThreshold;


    // indicates whether a setting has been successfully applied to the planner or not,
    // if not (_is_lastSettingValid = true), then calling ApplyLastSettings does nothing
    private bool _is_lastSettingValid; 


    // presets
    private Color _inputFieldOK;
    private Color _inputFieldErr;
    private Color _inputFieldDisabled;


    void Start()
    {
        _is_lastSettingValid = false;

        _inputFieldOK = Color.white;
        _inputFieldErr = Color.red;
        _inputFieldDisabled = Color.gray;

    }

    void OnEnable()
    {
        UpdateEigenInfo();

        FetchWristPositionHalfRange();
        FetchScoreMultiplier();
        FetchDistancePenaltyMultiplier();
    }

    private void FetchWristPositionHalfRange()
    {
        var range = Planner.TransformHalfRangeForEachAxis;

        WirstPositionRange.text = range.ToString();
    }

    public void SetWristPositionHalfRange(string newValue)
    {
        try
        {
            var newHalfRange = float.Parse(newValue);
            Planner.TransformHalfRangeForEachAxis = newHalfRange;
        }
        catch (FormatException)
        {
            FetchWristPositionHalfRange();
        }
    }

    private void FetchScoreMultiplier()
    {
        var multiplier = Planner.ScoreMultiplier;

        ScoreMultiplier.text = multiplier.ToString();
    }


    public void SetScoreMultiplier(string newValue)
    {
        try
        {
            var multiplier = float.Parse(newValue);
            Planner.ScoreMultiplier = multiplier;
        }
        catch (FormatException)
        {
            FetchScoreMultiplier();
        }
    }

    private void FetchDistancePenaltyMultiplier()
    {
        var multiplier = Planner.DistancePenaltyMultiplier;

        DistancePenaltyMultiplier.text = multiplier.ToString();
    }

    public void SetDistancePenaltyMultiplier(string newValue)
    {
        try
        {
            var multiplier = float.Parse(newValue);
            Planner.DistancePenaltyMultiplier = multiplier;
        }
        catch (FormatException)
        {
            FetchDistancePenaltyMultiplier();
        }
    }


    private void UpdateEigenInfo()
    {
        EigenEnable.isOn = Planner.Is_Eigen;
        EigenPCCount.value = Planner.EigenPCCount;
        EigenPCCount.GetComponentInChildren<Text>().text = EigenPCCount.value.ToString();
    }

    public void SetEigenEnabled(bool value)
    {
        Planner.Is_Eigen = value;
    }

    public void ChangeEigenPCCount()
    {
        Planner.EigenPCCount = (int) EigenPCCount.value;
        EigenPCCount.GetComponentInChildren<Text>().text = EigenPCCount.value.ToString();
    }


    public void UpdateEpsilonEnabled()
    {
        _is_epsilonEnabled = EpsilonEnable.isOn;
        if (_is_epsilonEnabled)
        {
            Epsilon.readOnly = false;
            Epsilon.image.color = _inputFieldOK;
        }
        else
        {
            Epsilon.readOnly = true;
            Epsilon.image.color = _inputFieldDisabled;
        }
    }

    public void UpdateMaxIterationsEnabled()
    {
        _is_maxIterationsEnabled = MaxIterationEnable.isOn;
        if (_is_maxIterationsEnabled)
        {
            MaxIterations.readOnly = false;
            MaxIterations.image.color = _inputFieldOK;
        }
        else
        {
            MaxIterations.image.color = _inputFieldDisabled;
            MaxIterations.readOnly = true;
        }
    }

    public void OnEntry()
    {
        Invoke("LoadOptimizerHyperparameters", 0.05f);
    }

    public void OnApply()
    {
        try
        {
            ParseOptimizerHyperparameters();
            ApplyOptimizerHyperparameters();
            _is_lastSettingValid = true;

        }catch(FormatException fe)
        {
            // TODO message box pop up
            print(fe.Message);
            _is_lastSettingValid = false;
        }
    }
    public void OnReset()
    {
        Planner.ResetAll();
        LoadOptimizerHyperparameters();

        OnEnable();
    }

    private void LoadOptimizerHyperparameters()
    {
        FetchOptimizerHyperparameters();
        UpdateEpsilonEnabled();
        UpdateMaxIterationsEnabled();
    }

    // throws FormatException if parse on a component failed
    private void ParseOptimizerHyperparameters()
    {
        // initial temperature
        if (!ParseInputFieldDouble(InitialTemperature, out _initialTemperature) ||
            _initialTemperature <= 0)
        {
            InitialTemperature.image.color = _inputFieldErr;
            throw new FormatException("Temperature must be a valid positive number");

        }
        else
        {
            InitialTemperature.image.color = _inputFieldOK;
        }
 
        // alpha
        if(!ParseInputFieldDouble(Alpha, out _alpha) ||
            _alpha >= 1 || _alpha <= 0)
        {
            Alpha.image.color = _inputFieldErr;
            throw new FormatException("Alpha must be a number between 0 and 1");
        }
        else
        {
            Alpha.image.color = _inputFieldOK;
        }

        // epsilon
        if (_is_epsilonEnabled)
        {
            if (!ParseInputFieldDouble(Epsilon, out _epsilon) ||
                _epsilon <= 0)
            {
                Epsilon.image.color = _inputFieldErr;
                throw new FormatException("Epsilon must be a positive number");
            }
            else
            {
                Epsilon.image.color = _inputFieldOK;
            }
        }


        // max iterations
        if (_is_maxIterationsEnabled)
        {
            if (!ParseInputFieldInt(MaxIterations, out _maxIterations) ||
    _maxIterations <= 0)
            {
                MaxIterations.image.color = _inputFieldErr;
                throw new FormatException("Max Iterations must be a positive integer number");
            }
            else
            {
                MaxIterations.image.color = _inputFieldOK;
            }
        }


        // Valid step threshold
        if (!ParseInputFieldDouble(ValidStepThreshold, out _validStepThreshold))
        {
            ValidStepThreshold.image.color = _inputFieldErr;
            throw new FormatException("Valid Step Threshold must be a number");
        }
        else
        {
            ValidStepThreshold.image.color = _inputFieldOK;
        }

    }

    // Apply the last valid settings to the planner
    public void ApplyLastSetting()
    {
        if (_is_lastSettingValid)
        {
            ApplyOptimizerHyperparameters();
        }
    }

    // Apply data to GraspPlanner
    private void ApplyOptimizerHyperparameters()
    {
        Planner.Temperature = _initialTemperature;
        Planner.Alpha = _alpha;
        if (_is_epsilonEnabled)
        {
            Planner.Epsilon = _epsilon;
        }
        else
        {
            Planner.Epsilon = null;
        }

        if (_is_maxIterationsEnabled)
        {
            Planner.MaxIterations = _maxIterations;
        }
        else
        {
            Planner.MaxIterations = null;
        }

        Planner.ValidStepThreshold = _validStepThreshold;
    }

    

    // return true on succeed, false otherwise
    private bool ParseInputFieldDouble(InputField inputField, out double value)
    {
        try
        {
            value = double.Parse(inputField.text);
            return true;
        }
        catch (FormatException)
        {
            value = 0;
            return false;
        }
    }

    private bool ParseInputFieldInt(InputField inputField, out int value)
    {
        try
        {
            value = int.Parse(inputField.text);
            return true;
        }
        catch (FormatException)
        {
            value = 0;
            return false;
        }
    }

    // fetch data from GraspPlanner
    private void FetchOptimizerHyperparameters()
    {
        // initial temperature
        _initialTemperature = Planner.Temperature;
        InitialTemperature.text = _initialTemperature.ToString();
        // alpha
        _alpha = Planner.Alpha;
        Alpha.text = _alpha.ToString();
        // epsilon
        _is_epsilonEnabled = Planner.Epsilon.HasValue;
        if (_is_epsilonEnabled)
        {
            _epsilon = Planner.Epsilon.Value;
            Epsilon.text = _epsilon.ToString();
            EpsilonEnable.isOn = true;
        }
        else
        {
            _epsilon = 0;
            Epsilon.text = "N/A";
            EpsilonEnable.isOn = false;
        }

        // max iterations
        _is_maxIterationsEnabled = Planner.MaxIterations.HasValue;
        if (_is_maxIterationsEnabled)
        {
            _maxIterations = Planner.MaxIterations.Value;
            MaxIterations.text = _maxIterations.ToString();
            MaxIterationEnable.isOn = true;
        }
        else
        {
            _maxIterations = 0;
            MaxIterations.text = "N/A";
            MaxIterationEnable.isOn = false;
        }

        // valid step threshold
        _validStepThreshold = Planner.ValidStepThreshold;
        ValidStepThreshold.text = _validStepThreshold.ToString();
    }


}
