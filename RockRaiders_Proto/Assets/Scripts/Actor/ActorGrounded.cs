using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Actor
{
    internal class ActorGrounded : MonoBehaviour
    {
        [SerializeField]
        private float m_moveSpeed;

        private PlayerController m_controller;
        private ActorState m_state;

        private RaycastHit m_groundInfo;

        public RaycastHit GroundRayCastHit
        {
            get
            {
                return m_groundInfo;
            }
            set
            {
                m_groundInfo = value;
            }
        }

        public ActorGrounded()
        {
            
        }

        private void Start()
        {
            m_controller = this.GetComponent<PlayerController>();
            m_state = this.GetComponent<ActorState>();
        }

        private void Update()
        {
            
        }

        private void UpdateMovement()
        {
            var moveX = m_controller?.MoveAxis.x * (m_moveSpeed * Time.deltaTime) ?? 0.0f;
            var moveZ = m_controller?.MoveAxis.y * (m_moveSpeed * Time.deltaTime) ?? 0.0f;
            var moveVector = new Vector3(moveX, 0.0f, moveZ);

            m_state.IsMoving = moveVector.magnitude > 0.0f;

            if (m_groundInfo.transform != null)
            {

            }

        }

        private void ProcessSurfaceRotation()
        {

        }
    }
}
