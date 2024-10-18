using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Util
{
    internal class Quad : MonoBehaviour
    {
        [SerializeField]
        private float m_width = 1;

        [SerializeField]
        private float m_height = 1;

        private float m_oldWidth = 1;
        private float m_oldHeight = 1;

        private MeshFilter m_meshFilter;

        public void Start()
        {
            m_meshFilter = this.GetComponent<MeshFilter>();
            m_meshFilter.mesh = new Mesh();

            this.Create();
        }

        private void Update()
        {
            var changed = m_oldHeight != m_height || m_oldWidth != m_width;

            if (changed)
            {
                this.Create();
                m_oldWidth = m_width;
                m_oldHeight = m_height;
            }
        }

        private void Create()
        {
            var mesh = m_meshFilter.mesh;

            var vertices = new Vector3[4]
            {
                new Vector3(0, 0, 0),
                new Vector3(m_width, 0, 0),
                new Vector3(0, m_height, 0),
                new Vector3(m_width, m_height, 0)
            };

            mesh.vertices = vertices;

            var tris = new int[6]
            {
                // lower left triangle
                0, 2, 1,
                // upper right triangle
                2, 3, 1
            };

            mesh.triangles = tris;

            var normals = new Vector3[4]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };

            mesh.normals = normals;

            var uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            mesh.uv = uv;
        }
    }
}
