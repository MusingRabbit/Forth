using Assets.Scripts.Actor;
using Assets.Scripts.Input;
using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts
{
    public class ActorCamera : MonoBehaviour
    {
        /// <summary>
        /// Stores the target actor
        /// </summary>
        [SerializeField]
        private GameObject m_targetActor;

        /// <summary>
        /// Stores the rotation speed
        /// </summary>
        [SerializeField]
        private float m_rotationSpeed;

        /// <summary>
        /// Stores the distance from the actor
        /// </summary>
        [SerializeField]
        private float m_distance;

        /// <summary>
        /// Stores whether to limit the y angle
        /// </summary>
        [SerializeField]
        public bool m_limitYAngle;

        /// <summary>
        /// Stores the y angle limit
        /// </summary>
        [SerializeField]
        private float m_maxYAngle;

        /// <summary>
        /// Stores the (x,y) offset of the camera
        /// </summary>
        [SerializeField]
        private Vector2 m_offset;

        /// <summary>
        /// Stores the player controller 
        /// </summary>
        private PlayerInput m_controller;

        /// <summary>
        /// Stores a reference to the actors' state.
        /// </summary>
        private ActorState m_state;

        /// <summary>
        /// Stores whether camera has player controller
        /// </summary>
        private bool m_hasController;

        /// <summary>
        /// Stores reference to target actors' body
        /// </summary>
        private GameObject m_body;
        
        /// <summary>
        /// Stores rotation euler x & y values
        /// </summary>
        private float m_rotX, m_rotY;

        /// <summary>
        /// Stores the target position
        /// </summary>
        private Vector3 m_tgtPos;

        /// <summary>
        /// Stores the target rotation
        /// </summary>
        private Quaternion m_tgtRot;

        /// <summary>
        /// Gets or sets the current camera target
        /// </summary>
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

        public Quaternion Rotation => Quaternion.Euler(m_rotX, m_rotY, 0.0f);

        /// <summary>
        /// Gets the cameras planar / y rotation
        /// </summary>
        public Quaternion YRot => Quaternion.Euler(0.0f, m_rotY, 0.0f);

        /// <summary>
        /// Gets the cameras x rotation
        /// </summary>
        public Quaternion XRot => Quaternion.Euler(m_rotX, 0.0f, 0.0f);

        /// <summary>
        /// Constructor
        /// </summary>
        public ActorCamera()
        {
            m_rotationSpeed = 2.0f;
            m_distance = 5.0f;
            m_maxYAngle = 45.0f;
            m_offset = Vector2.zero;
        }

        /// <summary>
        /// Called once before first frame in scene.
        /// </summary>
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

        /// <summary>
        /// Update - Called every frame
        /// </summary>
        private void Update()
        {
            if (m_hasController)
            {
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

                m_tgtRot = m_body.transform.rotation * tgtRotation;

                this.transform.rotation = Quaternion.Lerp(this.transform.rotation, m_tgtRot, m_rotationSpeed * Time.deltaTime);


                this.transform.position = Vector3.Lerp(this.transform.position, m_tgtPos, 50 * Time.deltaTime);
            }
        }
    }
}
