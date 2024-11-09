using Assets.Scripts.Input;
using UnityEngine;
using UnityEngine.ProBuilder;

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

        private ActorState m_state;

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

        public float MoveSpeed { get { return m_moveForce; } }

        public ActorFloating()
        {
            m_tgtRotation = Quaternion.identity;
        }

        public override void Initialise()
        {
            m_controller = this.GetComponent<PlayerInput>();
            m_rigidBody = this.GetComponent<Rigidbody>();
            m_state = this.GetComponent<ActorState>();
        }

        private void Start()
        {
            this.Initialise();
        }

        public override void Reset()
        {
            m_moveForce = 20.0f;
            m_maxSpeed = 50.0f;
            m_rotationSpeed = 50.0f;
            m_tgtRotation = Quaternion.identity;
        }

        private void Update()
        {
            if (m_state.IsDead)
            {
                return;
            }

            var moveInput = new Vector3(m_controller.MoveAxis.x, 0, m_controller.MoveAxis.y).normalized;
            var isMoving = moveInput.magnitude > 0;

            //m_tgtRotation = m_actorCamera.Rotation;

            this.UpdateRotation();

            if (isMoving && m_rigidBody.velocity.magnitude <= m_maxSpeed)
            {
                var moveDir = this.transform.rotation * moveInput;
                m_rigidBody.AddForce(moveDir * (m_moveForce * Time.deltaTime), ForceMode.Impulse);
            }

            m_rigidBody.AddForce(-m_rigidBody.velocity * (m_moveForce * 0.3f * Time.deltaTime), ForceMode.Impulse);

            //this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, m_tgtRotation, m_rotationSpeed * Time.deltaTime);

            
        }

        private void UpdateRotation()
        {
            //this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, info.TargetRotation, 50.0f * Time.deltaTime);

            var tgtRot = ((Quaternion.FromToRotation(this.transform.up, Vector3.up) * this.transform.rotation)); //* m_camera.PlanarRotaion);

            var rotation = tgtRot * m_actorCamera.Rotation;


            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, rotation, m_rotationSpeed * Time.deltaTime);
        }

        public void ThrustUp()
        {
            m_rigidBody.AddForce(this.transform.up.normalized * m_moveForce * Time.deltaTime, ForceMode.Impulse);
        }

        public void ThrustDown()
        {
            m_rigidBody.AddForce(-this.transform.up.normalized * m_moveForce * Time.deltaTime, ForceMode.Impulse);
        }
    }
}
