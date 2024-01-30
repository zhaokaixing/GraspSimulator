using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlwaysOnUI : MonoBehaviour
{
    public CameraControl cameraControl;

    public Toggle CameraControlToggle;

    void Awake()
    {
        CameraControlToggle.isOn = cameraControl.Is_enabled;
    }

    void Update()
    {
        HandleKeyboardShortCuts();
    }

    private void HandleKeyboardShortCuts()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleCameraControl();
        }

    }

    public void OnExitBtnPress()
    {
        Application.Quit();
    }

    public void ToggleCameraControl()
    {
        cameraControl.SetActive( !cameraControl.Is_enabled );
        CameraControlToggle.isOn = cameraControl.Is_enabled;
    }
}
