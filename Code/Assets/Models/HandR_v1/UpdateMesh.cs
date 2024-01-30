using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class UpdateMesh : MonoBehaviour
{
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private Mesh _mesh;


    void Start()
    {
        _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        _mesh = _skinnedMeshRenderer.sharedMesh;
        GetComponent<MeshCollider>().sharedMesh = _mesh;

        UnityEngine.Debug.Log("mesh name "+ _mesh.name);
        UnityEngine.Debug.Log("mesh vertices " + _mesh.vertices[0]);
    }

    /*void OnCollisionEnter(Collision collision)
    {
        UnityEngine.Debug.Log("detect collision");
    }

    void OnTriggerEnter(Collider other)
    {
        UnityEngine.Debug.Log("detect collision 2");
    }*/

    public void ApplySkinningToTriangles()
    {
        ApplySkinningToTriangles(_skinnedMeshRenderer, _mesh);
    }

    void ApplySkinningToTriangles(SkinnedMeshRenderer skin, Mesh mesh)
    {
        // Make a CWeightList for each bone in the skinned mesh  
        var vertices = mesh.vertices;
        var bindposes = mesh.bindposes;
        var boneWeights = mesh.boneWeights;
        var bones = skin.bones;

        // Start by initializing all vertices to 'empty'
        var newVert = new Vector3[vertices.Length];
        for (int i = 0; i < newVert.Length; i++)
        {
            newVert[i] = new Vector3(0, 0, 0);
        }

        // Create a bone weight list for each bone, ready for quick calculation during an update...
        Vector3 localPt;

        for (int i = 0; i < vertices.Length; i++)
        {
            BoneWeight bw = boneWeights[i];

            if (Math.Abs(bw.weight0) > 0.00001)
            {
                localPt = bindposes[bw.boneIndex0].MultiplyPoint3x4(vertices[i]);
                newVert[i] += bones[bw.boneIndex0].transform.localToWorldMatrix.MultiplyPoint3x4(localPt) * bw.weight0;
            }

            if (Math.Abs(bw.weight1) > 0.00001)
            {
                localPt = bindposes[bw.boneIndex1].MultiplyPoint3x4(vertices[i]);
                newVert[i] += bones[bw.boneIndex1].transform.localToWorldMatrix.MultiplyPoint3x4(localPt) * bw.weight1;

            }

            if (Math.Abs(bw.weight2) > 0.00001)
            {
                localPt = bindposes[bw.boneIndex2].MultiplyPoint3x4(vertices[i]);
                newVert[i] += bones[bw.boneIndex2].transform.localToWorldMatrix.MultiplyPoint3x4(localPt) * bw.weight2;
            }

            if (Math.Abs(bw.weight3) > 0.00001)
            {
                localPt = bindposes[bw.boneIndex3].MultiplyPoint3x4(vertices[i]);
                newVert[i] += bones[bw.boneIndex3].transform.localToWorldMatrix.MultiplyPoint3x4(localPt) * bw.weight3;
            }
        }

        mesh.vertices = newVert;
    }
}
