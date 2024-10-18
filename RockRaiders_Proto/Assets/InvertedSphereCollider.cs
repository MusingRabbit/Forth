using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InvertedSphereCollider : MonoBehaviour
{
    private MeshCollider m_meshCollider;

    // Start is called before the first frame update
    void Start()
    {
        m_meshCollider = this.GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        if (m_meshCollider == null)
        {
            m_meshCollider = this.GetComponent<MeshCollider>();
        }

        var mesh = m_meshCollider.sharedMesh;

        mesh.triangles = mesh.triangles.Reverse().ToArray();

        mesh.normals = mesh.normals.Reverse().ToArray();
    }
}
