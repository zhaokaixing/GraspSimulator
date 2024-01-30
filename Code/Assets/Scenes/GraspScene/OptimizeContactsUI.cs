using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class OptimizeContactsUI : MonoBehaviour
{
    public GraspSceneInitialUI InitialUI;
    public InputField ContactCountInput;

    public void OnApplyContactCountBtnPress()
    {
        try
        {

            int count = int.Parse(ContactCountInput.text);
            if (count <= 0) throw new FormatException("contact count should be positive integer");
            ContactCountInput.image.color = Color.white;

            gameObject.SetActive(false);
            InitialUI.StartContactOptimizer(count);
        }
        catch (FormatException)
        {
            ContactCountInput.image.color = Color.red;
        }

        
    }
}
