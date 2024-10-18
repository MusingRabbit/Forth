using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Actor
{
    public class ActorCrosshair : MonoBehaviour
    {
        [SerializeField]
        private LayerMask m_layerMask;

        [SerializeField]
        private ActorCamera m_camera;

        [SerializeField]
        private GameObject m_debugObj;

        [SerializeField]
        private float m_maxDistance;

        private Vector3 m_point;

        public Vector3 AimPoint
        {
            get
            {
                return m_point;
            }
        }

        public ActorCrosshair()
        {
            m_maxDistance = 999.9f;
        }

        private void Start()
        {
        }

        private void UpdateQuadPosition()
        {
            m_debugObj.transform.position = m_point;
        }

        private void UpdateQuadRotaion()
        {
            var dir = m_point - m_camera.transform.position;

            m_debugObj.gameObject.transform.forward =- m_camera.transform.forward;
        }

        private void Update()
        {
            var dir = m_camera.transform.forward;
            var hit = Physics.Raycast(m_camera.transform.position, dir, out var hitInfo, m_maxDistance, m_layerMask);

            if (hit)
            {
                m_point = hitInfo.point;
            }

            this.UpdateQuadPosition();
            this.UpdateQuadRotaion();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(m_point, 0.5f);
        }
    }
}
