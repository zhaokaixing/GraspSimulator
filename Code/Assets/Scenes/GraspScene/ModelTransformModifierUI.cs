using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelTransformModifierUI : MonoBehaviour
{
    public ModelLoader modelLoader;

    public Slider ModelScale;

    void Awake()
    {
        modelLoader.ModelImported += (src, model) =>
        {
            gameObject.SetActive(true);
            FetchModelTransformInfo();
        };

        gameObject.SetActive(false);
    }

    public void FetchModelTransformInfo()
    {
        ModelScale.value = modelLoader.ModelLocalScale;
    }

    public void ApplyScale(float multiplier)
    {
        modelLoader.ApplyScale(multiplier);
    }



}
