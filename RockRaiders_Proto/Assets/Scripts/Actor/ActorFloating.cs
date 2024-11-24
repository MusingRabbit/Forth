using Assets.Scripts.Input;
using Assets.Scripts.Services;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Assets.Scripts.Actor
{
    public class ActorFloating : RRMonoBehaviour
    {
        /// <summary>
        /// The force/impulse that propels actors' through space.
        /// </summary>
        [SerializeField]
        private float m_moveForce;

        /// <summary>
        /// The maximum speed that an actor may move at.
        /// </summary>
        [SerializeField]
        private float m_maxSpeed;

        /// <summary>
        /// The rate at which an actor may rotate.
        /// </summary>
        [SerializeField]
        private float m_rotationSpeed;

        /// <summary>
        /// This actors' camera component.
        /// </summary>
        private ActorCamera m_actorCamera;

        /// <summary>
        /// Body
        /// </summary>
        [SerializeField]
        private GameObject m_body;

        /// <summary>
        /// Head
        /// </summary>
        [SerializeField]
        private GameObject m_head;

        /// <summary>
        /// The player input / controller component
        /// </summary>
        private PlayerInput m_controller;

        /// <summary>
        /// The actors rigidbody component
        /// </summary>
        private Rigidbody m_rigidBody;

        /// <summary>
        /// The actors state component
        /// </summary>
        private ActorState m_state;

        /// <summary>
        /// The current target rotation to which the actor will orient themselves toward.
        /// </summary>
        private Quaternion m_tgtRotation;

        /// <summary>
        /// Gets or sets the actors' head gameobject
        /// </summary>
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

        /// <summary>
        /// Gets or sets the actors' body gameobject.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the actors' current camera
        /// </summary>
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

        /// <summary>
        /// The current 'up' vector for this actor.
        /// </summary>
        private Vector3 m_upVector;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActorFloating()
        {
            m_tgtRotation = Quaternion.identity;
            m_upVector = Vector3.up;
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public override void Initialise()
        {
            m_controller = this.GetComponent<PlayerInput>();
            m_rigidBody = this.GetComponent<Rigidbody>();
            m_state = this.GetComponent<ActorState>();
        }

        /// <summary>
        /// Start / Initialisation - called prior to first frame in scene.
        /// </summary>
        private void Start()
        {
            this.Initialise();
        }

        /// <summary>
        /// Resets the actors' floating behaviour
        /// </summary>
        public override void Reset()
        {
            m_moveForce = 20.0f;
            m_maxSpeed = 50.0f;
            m_rotationSpeed = 50.0f;
            m_tgtRotation = Quaternion.identity;
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private void Update()
        {
            if (m_state.IsDead) // If the actor is dead - do nothing.
            {
                return;
            }

            // Rotate and move character
            this.UpdateRotation();
        }

        /// <summary>
        /// Called every physics frame
        /// </summary>
        private void FixedUpdate()
        {
            this.UpdateMovement();
        }

        /// <summary>
        /// In charge of handling actor movement.
        /// </summary>
        private void UpdateMovement()
        {
            var moveInput = new Vector3(m_controller.MoveAxis.x, 0, m_controller.MoveAxis.y).normalized;
            var isThrusting = moveInput.magnitude > 0;

            if (isThrusting && m_rigidBody.velocity.magnitude <= m_maxSpeed)
            {
                var moveDir = this.transform.rotation * moveInput;
                m_rigidBody.AddForce(moveDir * m_moveForce, ForceMode.Impulse);
            }
            else
            {
                m_rigidBody.AddForce(-m_rigidBody.velocity.normalized * (m_moveForce * 0.5f), ForceMode.Acceleration);
            }
          
        }

        /// <summary>
        /// In charge of handling actor orientation.
        /// </summary>
        private void UpdateRotation()
        {
            if (this.UpdateRollRotation())
            {
                var tgtRot = (this.transform.rotation * m_actorCamera.XRot);

                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, tgtRot, m_rotationSpeed * Time.deltaTime);

                m_upVector = this.transform.up;
            }
        }

        /// <summary>
        /// Handles player rool input.
        /// </summary>
        /// <returns>Is roll complete? (true/false)</returns>
        private bool UpdateRollRotation()
        {
            bool result = false;

            if (m_controller.RollLeft == (int)ActionState.Active)
            {
                this.RollLeft();
                result = true;
            }

            if (m_controller.RollRight == (int)ActionState.Active)
            {
                this.RollRight();
                result = true;
            }


            Debug.DrawLine(this.transform.position, this.transform.position + m_upVector, Color.yellow);

            var tgtRot = Quaternion.FromToRotation(this.transform.up, m_upVector) * this.transform.rotation * m_actorCamera.YRot;

            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, tgtRot, m_rotationSpeed * Time.deltaTime);

            result = (this.transform.up - m_upVector).magnitude <= Vector3.one.magnitude;

            return result;
        }

        /// <summary>
        /// Resets the actors roll by resetting the up vector to (0,1,0) (Vector3.up).
        /// </summary>
        public void ResetRoll()
        {
            m_upVector = this.transform.up;
        }

        /// <summary>
        /// Rolls the actor left.
        /// </summary>
        public void RollLeft()
        {
            var rot = this.transform.rotation * Quaternion.Euler(0, 0, 1);
            m_upVector = (rot * Vector3.up);
        }

        /// <summary>
        /// Rolls the actor right.
        /// </summary>
        public void RollRight()
        {
            var rot = this.transform.rotation * Quaternion.Euler(0, 0, -1);
            m_upVector = (rot * Vector3.up);
        }

        /// <summary>
        /// Thrusts the actor upward.
        /// </summary>
        public void ThrustUp()
        {
            m_rigidBody.AddForce(this.transform.up.normalized * m_moveForce * Time.deltaTime, ForceMode.Impulse);
        }
        
        /// <summary>
        /// Thrusts the actor downward.
        /// </summary>
        public void ThrustDown()
        {
            m_rigidBody.AddForce(-this.transform.up.normalized * m_moveForce * Time.deltaTime, ForceMode.Impulse);
        }
    }
}
