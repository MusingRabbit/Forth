using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Actor
{
    public class ActorFloating : MonoBehaviour
    {
        [SerializeField]
        private float m_moveForce;

        [SerializeField]
        private float m_maxSpeed;

        [SerializeField]
        private float m_rotationSpeed;

        [SerializeField]
        private GameObject m_body;

        [SerializeField]
        private GameObject m_head;

        [SerializeField]
        private GameObject m_actorCamera;

        private PlayerController m_controller;
        private Rigidbody m_rigidBody;

        private ActorCamera m_tpCamera;
        private Quaternion m_tgtRotation;

        public GameObject Head
        {
            get
            {
                return m_head;
            }
            set
            {
                m_head = value;
            }
        }

        public GameObject Body
        {
            get
            {
                return m_body;
            }
            set
            {
                m_body = value;
            }
        }

        public GameObject Camera
        {
            get
            {
                return m_actorCamera;
            }
            set
            {
                m_actorCamera = value;
            }
        }


        public ActorFloating()
        {
            m_moveForce = 10.0f;
            m_maxSpeed = 22.0f;
            m_rotationSpeed = 50.0f;
        }

        private void Start()
        {
            m_controller = this.GetComponent<PlayerController>();
            m_rigidBody = this.GetComponent<Rigidbody>();
            m_tpCamera = m_actorCamera.GetComponent<ActorCamera>();
        }

        private void Update()
        {
            var moveInput = new Vector3(m_controller.MoveAxis.x, 0, m_controller.MoveAxis.y).normalized;
            var isMoving = moveInput.magnitude > 0;

            m_tgtRotation = m_tpCamera.Rotation;

            if (isMoving)
            {
                var moveDir = m_tpCamera.Rotation * moveInput;
                m_rigidBody.AddForce(moveDir * (m_moveForce * Time.deltaTime), ForceMode.Force);
            }

            m_rigidBody.AddForce(-m_rigidBody.velocity * (0.95f * Time.deltaTime), ForceMode.Force);

            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, m_tgtRotation, m_rotationSpeed * Time.deltaTime);
        }

    }
}
