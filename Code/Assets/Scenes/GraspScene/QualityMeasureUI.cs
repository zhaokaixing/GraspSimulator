using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class QualityMeasureUI : MonoBehaviour
{
    public GameObject StabilityWithDoIAdditionalUI;

    private PlannerSettingsUI _plannerSettingsUI;

    private GraspPlanner Planner { get
        {
            return _plannerSettingsUI.Planner;
        } }


    void Awake()
    {
        _plannerSettingsUI = GetComponentInParent<PlannerSettingsUI>();   
    }


    public void OnQualityMeasureSelected(int option)
    {
        switch (option)
        {
            case 0:
                Planner.SetSelectedContact(false);
                StabilityWithDoIAdditionalUI.SetActive(false);
                return;
            case 1:
                Planner.SetSelectedContact(true);
                StabilityWithDoIAdditionalUI.SetActive(true);
                return;
            default:
                throw new InvalidOperationException("unknown quality measure index: " + option);
        }
    }


}
