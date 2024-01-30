using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Outline = cakeslice.Outline;

public class OutlineHandler : MonoBehaviour
{
    private List<Outline> _outlines;


    void Start()
    {
        _outlines = new List<Outline>();

        var renderers = GetComponentsInChildren<Renderer>();
        foreach(var renderer in renderers)
        {
            var outline = renderer.gameObject.AddComponent<Outline>();
            _outlines.Add(outline);
        }
    }

    public void SetEnable(bool is_enabled)
    {
        foreach(var outline in _outlines)
        {
            outline.enabled = is_enabled;
        }
    }
}
