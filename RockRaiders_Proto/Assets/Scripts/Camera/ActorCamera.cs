using Assets.Scripts.Actor;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private PlayerController m_controller;
        private ActorState m_state;
        private bool m_hasController;
        private GameObject m_body;
        

        private float m_rotX, m_rotY;

        private Vector3 m_tgtPos;
        private Quaternion m_tgtRot;

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

        public Quaternion PlanarRotaion => Quaternion.Euler(0.0f, m_rotY, 0.0f);

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

            m_controller = m_targetActor.GetComponent<PlayerController>();
            m_state = m_targetActor.GetComponent<ActorState>();
            m_hasController = m_controller != null;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (m_hasController)
            {
                if (m_state.IsFloating)
                {
                    m_rotX += -m_controller.LookAxis.y * m_rotationSpeed;

                    if (m_limitYAngle)
                    {
                        m_rotX = Mathf.Clamp(m_rotX, -m_maxYAngle, m_maxYAngle);
                    }

                    m_rotY += m_controller.LookAxis.x * m_rotationSpeed;

                    var tgtRotation = Quaternion.Euler(m_rotX, m_rotY, 0);
                    var focusPos = m_targetActor.transform.position + new Vector3(m_offset.x, m_offset.y);
                    m_tgtPos = focusPos - tgtRotation * new Vector3(0, 0, m_distance);
                    m_tgtRot = tgtRotation;

                    this.transform.rotation = Quaternion.Lerp(this.transform.rotation, m_tgtRot, 50 * Time.deltaTime);
                }
                else
                {
                    var offset = m_body.transform.rotation * new Vector3(m_offset.x, m_offset.y);
                    this.transform.parent = m_body.transform;

                    m_rotX += -m_controller.LookAxis.y * m_rotationSpeed;
                    m_rotY = m_controller.LookAxis.x * m_rotationSpeed;

                    Debug.Log("m_rotX:" + m_rotX);
                    Debug.Log("m_rotY:" + m_rotX);

                    var tgtRotation = Quaternion.Euler(m_rotX, m_rotY, 0);

                    m_tgtPos = (m_body.transform.position - (tgtRotation * m_body.transform.forward)) + offset;
                    m_tgtRot = m_body.transform.localRotation * tgtRotation;


                    this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, m_tgtRot, 50 * Time.deltaTime);

                    //this.transform.rotation = m_targetActor.transform.rotation;
                    //
                    //var camPos = m_targetActor.transform.position + new Vector3(m_offset.x, m_offset.y, -m_distance);
                    //this.transform.position = camPos;

                    //var baseRotation = m_targetActor.transform.rotation;
                    //var focusPos = m_targetActor.transform.position + new Vector3(m_offset.x, m_offset.y);
                    //this.transform.position = focusPos - (baseRotation * new Vector3(0, 0, m_distance));
                    ////this.transform.rotation = baseRotation;


                    //m_rotX += -m_controller.LookAxis.y * m_rotationSpeed;

                    //if (m_limitYAngle)
                    //{
                    //    m_rotX = Mathf.Clamp(m_rotX, -m_maxYAngle, m_maxYAngle);
                    //}

                    //m_rotY += m_controller.LookAxis.x * m_rotationSpeed;

                    //
                    //focusPos = m_targetActor.transform.position + new Vector3(m_offset.x, m_offset.y);
                    //this.transform.position = focusPos - tgtRotation * new Vector3(0, 0, m_distance);
                    //this.transform.rotation = tgtRotation;
                }

                
                this.transform.position = Vector3.Lerp(this.transform.position, m_tgtPos, 50 * Time.deltaTime);
            }
        }
    }
}
