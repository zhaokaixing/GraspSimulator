using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCollisionDetection : MonoBehaviour
{
    public Vector3 touchpos_t, touchpos_i, touchpos_m, touchpos_r, touchpos_p;
    public Vector3 worldcontact_T, worldcontact_I, worldcontact_M, worldcontact_R, worldcontact_P;

    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private Mesh _mesh;

    // Start is called before the first frame update
    void Start()
    {
        _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        _mesh = _skinnedMeshRenderer.sharedMesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
    {
        UnityEngine.Debug.Log("Collision Detection");
    }
}
