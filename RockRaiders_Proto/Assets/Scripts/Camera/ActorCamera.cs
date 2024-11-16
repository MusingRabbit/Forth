﻿using Assets.Scripts.Actor;
using Assets.Scripts.Input;
using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts
{
    public class ActorCamera : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_targetActor;

        [SerializeField]
        private float m_rotationSpeed;

        [SerializeField]
        private float m_distance;

        [SerializeField]
        public bool m_limitYAngle;

        [SerializeField]
        private float m_maxYAngle;

        [SerializeField]
        private Vector2 m_offset;

        private PlayerInput m_controller;
        private ActorState m_state;
        private bool m_hasController;
        private GameObject m_body;
        

        private float m_rotX, m_rotY;

        private Vector3 m_tgtPos;
        private Quaternion m_tgtRot;

        private uint m_floatCounter = 0;
        private uint m_groundCounter = 0;

        public GameObject Target
        {
            get
            {
                return m_targetActor;
            }
            set
            {
                m_targetActor = value;
            }
        }

        public float RotationSpeed
        {
            get
            {
                return m_rotationSpeed;
            }
            set
            {
                m_rotationSpeed = value;
            }
        }

        public float Distance
        {
            get
            {
                return m_distance;
            }
            set
            {
                m_distance = value;
            }
        }

        public float MaxAngle
        {
            get
            {
                return m_maxYAngle;
            }
            set
            {
                m_maxYAngle = value;
            }
        }

        public Vector2 Offset
        {
            get
            {
                return m_offset;
            }
            set
            {
                m_offset = value;
            }
        }

        public Quaternion Rotation => Quaternion.Euler(m_rotX, m_rotY, 0.0f);

        public Quaternion YRot => Quaternion.Euler(0.0f, m_rotY, 0.0f);

        public Quaternion XRot => Quaternion.Euler(m_rotX, 0.0f, 0.0f);

        public ActorCamera()
        {
            m_rotationSpeed = 2.0f;
            m_distance = 5.0f;
            m_maxYAngle = 45.0f;
            m_offset = Vector2.zero;
        }

        private void Start()
        {
            this.transform.position = m_targetActor.transform.position;
            this.transform.rotation = m_targetActor.transform.rotation;
            m_body = m_targetActor.gameObject.FindChild("Body");

            m_controller = m_targetActor.GetComponent<PlayerInput>();
            m_state = m_targetActor.GetComponent<ActorState>();
            m_hasController = m_controller != null;
            this.transform.parent = m_body.transform;
        }

        private Vector2 GetRotationFromTransform(Transform rhs, Vector3 offset)
        {
            var result = new Vector2();

            var focusPos = rhs.position + offset;
            var direction = focusPos - this.transform.position;
            var targetRotation = Quaternion.LookRotation(direction);

            result.x = targetRotation.eulerAngles.x;
            result.y = targetRotation.eulerAngles.y;

            return result;
        }

        private void Update()
        {
            if (m_hasController)
            {
                m_floatCounter = 0;

                var offset = m_body.transform.rotation * new Vector3(m_offset.x, m_offset.y);
                var distance = m_body.transform.rotation * new Vector3(0, 0, m_distance);

                m_rotY = m_controller.LookAxis.x * m_rotationSpeed;

                if (m_state.IsFloating)
                {
                    m_rotX = -m_controller.LookAxis.y * m_rotationSpeed;
                }
                else
                {
                    m_rotX += -m_controller.LookAxis.y * m_rotationSpeed;
                }

                if (m_limitYAngle)
                {
                    m_rotX = Mathf.Clamp(m_rotX, -m_maxYAngle, m_maxYAngle);
                }


                var tgtRotation = Quaternion.Euler(m_rotX, m_rotY, 0);

                m_tgtPos = m_body.transform.position - (tgtRotation * m_body.transform.forward) + offset - distance;
                //m_tgtPos = m_body.transform.position - (tgtRotation * (m_body.transform.forward + distance)) + offset;

                m_tgtRot = m_body.transform.rotation * tgtRotation;

                

                this.transform.rotation = Quaternion.Lerp(this.transform.rotation, m_tgtRot, 50 * Time.deltaTime);


                if (m_groundCounter > uint.MaxValue)
                {
                    m_groundCounter = 1;
                }
                else
                {
                    m_groundCounter++;
                }

                this.transform.position = Vector3.Lerp(this.transform.position, m_tgtPos, 50 * Time.deltaTime);
            }
        }
    }
}
