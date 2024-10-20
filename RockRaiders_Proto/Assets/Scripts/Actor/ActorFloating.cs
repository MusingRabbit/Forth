using Assets.Scripts.Input;
using UnityEngine;

namespace Assets.Scripts.Actor
{
    public class ActorFloating : RRMonoBehaviour
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
        private ActorCamera m_actorCamera;

        private PlayerInput m_controller;
        private Rigidbody m_rigidBody;

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

        public ActorCamera ActorCamera
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

        public override void Initialise()
        {
            m_controller = this.GetComponent<PlayerInput>();
            m_rigidBody = this.GetComponent<Rigidbody>();
        }

        private void Start()
        {
            this.Initialise();
        }

        private void Update()
        {
            var moveInput = new Vector3(m_controller.MoveAxis.x, 0, m_controller.MoveAxis.y).normalized;
            var isMoving = moveInput.magnitude > 0;

            m_tgtRotation = m_actorCamera.Rotation;

            if (isMoving && m_rigidBody.velocity.magnitude <= m_maxSpeed)
            {
                var moveDir = m_actorCamera.Rotation * moveInput;
                m_rigidBody.AddForce(moveDir * (m_moveForce * Time.deltaTime), ForceMode.Force);
            }

            m_rigidBody.AddForce(-m_rigidBody.velocity * (m_moveForce * 0.3f * Time.deltaTime), ForceMode.Force);

            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, m_tgtRotation, m_rotationSpeed * Time.deltaTime);
        }

        public void ThrustUp()
        {
            m_rigidBody.AddForce(this.transform.up.normalized * m_moveForce * Time.deltaTime, ForceMode.Force);
        }

        public void ThrustDown()
        {
            m_rigidBody.AddForce(-this.transform.up.normalized * m_moveForce * Time.deltaTime, ForceMode.Force);
        }
    }
}
