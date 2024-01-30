using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class GraspSceneInitialUI : MonoBehaviour
{

    public ModelLoader SubjectModelImporter;

    public GameObject PlanningCanvas;

    void Awake()
    {
        //SubjectModelImporter.SubjectImported += (src, subject) =>
        //{
        //    gameObject.SetActive(true);
        //};
    }

    private GraspPlanner _planner
    {
        get
        {
            return SubjectModelImporter.GetPlanner();
        }
    }

    public void OnManualControlBtnPress()
    {
        _planner.Is_manualControl = true;
    }

    public void StartContactOptimizer(int contactCount)
    {
        _planner.ContactCount = contactCount;

        gameObject.SetActive(false);
        PlanningCanvas.SetActive(true);
        StartPlanner(true);
    }


    public void StartPlanner(bool is_contactPointOptimizer = false)
    {
        var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        var loggerFolder = documentsFolder + Path.DirectorySeparatorChar + 
            "GraspSimulator" + Path.DirectorySeparatorChar + 
            "log";

        Directory.CreateDirectory(loggerFolder);

        if (!_planner.Is_planning)
        {
            _planner.Is_ContactPointOptimizer = is_contactPointOptimizer;

            var loggerPath = loggerFolder + Path.DirectorySeparatorChar
                + DateTime.Now.ToString().Replace('/', '_').Replace(' ', '_').Replace(':', '_') +
                ".csv"; 
            _planner.logger = new Logger(loggerPath);
            print("logger created at: " + loggerPath);
            _planner.logger.StartTimer();
            _planner.HandModel.SetActive(true);
            _planner.TogglePlanner();
        }
        
        
    }

}
