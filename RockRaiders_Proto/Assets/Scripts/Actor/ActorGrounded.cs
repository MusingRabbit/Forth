using Assets.Scripts.Events;
using Assets.Scripts.Input;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Actor
{
    public struct SurfaceRotationInfo
    {
        public bool InContact;
        public Vector3 NormalForward;
        public Vector3 NoramlBack;
        public Vector3 NormalRight;
        public Vector3 NormalLeft;
        public Quaternion TargetRotation;
    }

    /// <summary>
    /// Actor grounded behaviour
    /// Manages the actors behaviour when in a 'grounded' state
    /// </summary>
    internal class ActorGrounded : RRMonoBehaviour, IGravityWell
    {
        /// <summary>
        /// Maximum move speed of the actor
        /// </summary>
        [SerializeField]
        private float m_maxMoveSpeed;

        [SerializeField]
        private float m_acceleration;

        /// <summary>
        /// The actors' jump force/impulse
        /// </summary>
        [SerializeField]
        private float m_jumpForce;

        /// <summary>
        /// The minumum amount of time between actor jumps
        /// </summary>
        [SerializeField]
        private float m_jumpTimeout;

        /// <summary>
        /// The strength of grav boots. The amount of 'gravity' / pullforce they have.
        /// </summary>
        [SerializeField]
        private float m_gravBootStrength;

        /// <summary>
        /// The maximum speed at which the actor will rotate.
        /// </summary>
        [SerializeField]
        private float m_rotationSpeed;

        /// <summary>
        /// The actor's body gameobject
        /// </summary>
        [SerializeField]
        private GameObject m_body;

        /// <summary>
        /// The actors camera component
        /// </summary>
        private ActorCamera m_actorCamera;

        /// <summary>
        /// Player Input / Controller component
        /// </summary>
        private PlayerInput m_controller;

        /// <summary>
        /// Actor state component
        /// </summary>
        private ActorState m_state;

        /// <summary>
        /// Actors rigidbody component
        /// </summary>
        private Rigidbody m_rigidBody;

        /// <summary>
        /// Actors' ground ray component - this is used to check for nearby walkable surfaces.
        /// </summary>
        private ActorGroundRay m_groundRay;

        /// <summary>
        /// Whether the actor can jump
        /// </summary>
        private bool m_canJump;

        /// <summary>
        /// Whether the actor is crouching.
        /// </summary>
        private bool m_crouch;

        /// <summary>
        /// The timer that tracks time between jumps.
        /// </summary>
        private Timer m_jumpTimer;

        /// <summary>
        /// Surface rotation information.
        /// </summary>
        private SurfaceRotationInfo m_surfaceInfo;

        /// <summary>
        /// Target surface rotation
        /// </summary>
        private Quaternion m_tgtSurfRot;

        /// <summary>
        /// Target lookAt rotation
        /// </summary>
        private Quaternion m_tgtLookatRot;

        /// <summary>
        /// Current move vector.
        /// </summary>
        private Vector3 m_moveVector;

        /// <summary>
        /// Stores whether in gravity well
        /// </summary>
        private bool m_inGravityWell;

        /// <summary>
        /// Gets or sets the actors' body gameobject
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
        /// Gets or sets the actors' camera component
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
        /// Gets the max move speed.
        /// </summary>
        public float MoveSpeed
        {
            get
            {
                return m_maxMoveSpeed;
            }
        }

        public bool InGravityWell
        {
            get
            {
                return m_inGravityWell;
            }
            set
            {
                m_inGravityWell = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActorGrounded()
        {
            m_jumpTimer = new Timer();
            m_jumpTimer.SetTimeSpan(TimeSpan.FromSeconds(m_jumpTimeout));
            m_jumpTimer.OnTimerElapsed += this.JumpTimer_OnTimerElapsed;
            m_canJump = true;
            m_acceleration = 10f;
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public override void Initialise()
        {
            m_controller = this.GetComponent<PlayerInput>();
            m_state = this.GetComponent<ActorState>();
            m_rigidBody = this.GetComponent<Rigidbody>();
            m_groundRay = this.GetComponent<ActorGroundRay>();
        }


        /// <summary>
        /// Start / Initialisation - called once prior to first frame in scene.
        /// </summary>
        private void Start()
        {
            this.Initialise();
        }

        /// <summary>
        /// Resets the actors' grounded behaviour
        /// </summary>
        public override void Reset()
        {
            m_canJump = true;
            m_crouch = false;
            m_moveVector = Vector3.zero;
            m_tgtLookatRot = Quaternion.identity;
            m_tgtSurfRot = Quaternion.identity;
        }

        /// <summary>
        /// Called every frame
        /// </summary>
        private void Update()
        {
            if (m_state.IsDead) // If actor is dead, apply any gravity and return.
            {
                this.ApplyGravBoots();
                return;
            }

            var moveX = m_controller?.MoveAxis.x * (m_maxMoveSpeed * Time.deltaTime) ?? 0.0f;
            var moveZ = m_controller?.MoveAxis.y * (m_maxMoveSpeed * Time.deltaTime) ?? 0.0f;
            m_moveVector = new Vector3(moveX, 0.0f, moveZ);

            m_state.IsMoving = m_moveVector.magnitude > 0.0f;
            m_state.IsCrouched = m_crouch;
            m_state.FeetOnGround = m_groundRay.Hit;

            if (m_groundRay.Hit)
            {
                this.UpdateSurfaceRotaion();
                this.UpdateMovement();
            }

            m_crouch = false;
            m_jumpTimer.Tick();
        }

        /// <summary>
        /// Updates the surface rotation information used to calculate the actor's rotations relative to the surface they walk on.
        /// </summary>
        private void UpdateSurfaceRotaion()
        {
            m_surfaceInfo = this.GetSurfaceRotationInfo();

            m_tgtSurfRot = ((m_surfaceInfo.TargetRotation * this.transform.rotation)); 

            var rotation = m_tgtSurfRot;
            var surfaceRot = Quaternion.RotateTowards(this.transform.rotation, m_tgtSurfRot, m_rotationSpeed * Time.deltaTime);
            var horiRot = m_actorCamera.YRot;

            this.transform.rotation = surfaceRot * horiRot;
        }

        /// <summary>
        /// Updates actor movement relative to the surface being walked upon
        /// </summary>
        private void UpdateMovement()
        {
            m_moveVector = Vector3.zero;
            var moveSpeed = m_state.IsCrouched ? m_maxMoveSpeed / 3 : m_maxMoveSpeed;

            if (m_state.IsMoving && m_rigidBody.velocity.magnitude < moveSpeed)
            {
                m_moveVector = this.GetMoveVector(m_surfaceInfo, m_controller.MoveAxis);
                m_rigidBody.AddForce(m_moveVector * (m_acceleration), ForceMode.Acceleration);
            }
            else
            {
                m_rigidBody.AddForce(-(m_rigidBody.velocity * (1 / m_acceleration)), ForceMode.Acceleration);
                //m_rigidBody.velocity -= (m_rigidBody.velocity / 10) * Time.deltaTime;
            }

            if (m_groundRay.Hit)
            {
                this.ApplyGravBoots();
            }
        }

        /// <summary>
        /// Applies gravity (from grav-boots)
        /// </summary>
        private void ApplyGravBoots()
        {
            var gravity = (m_groundRay.Normal.normalized * (-m_gravBootStrength * m_rigidBody.mass));
            m_rigidBody.AddForce(gravity * Time.deltaTime, ForceMode.Force);
        }

        /// <summary>
        /// Gets move vector based upon the inputs current move axis and surface rotation.
        /// </summary>
        /// <param name="info">Surface Rotation Info <see cref="SurfaceRotationInfo"/></param>
        /// <param name="moveAxis">Move Axis</param>
        /// <returns>3D Move world vector</returns>
        private Vector3 GetMoveVector(SurfaceRotationInfo info, Vector2 moveAxis)
        {
            var result = new Vector3();

            if (Mathf.Abs(moveAxis.x) > 0)
            {
                result += (info.NormalRight * moveAxis.x);
            }

            if (Mathf.Abs(moveAxis.y) > 0)
            {
                result += (info.NormalForward * moveAxis.y);
            }

            return result.normalized;
        }

        /// <summary>
        /// Gets the surface rotation info process by the actors' 'GroundRay' component
        /// </summary>
        /// <returns>Suface Rotation Information <see cref="SurfaceRotationInfo"/></returns>
        private SurfaceRotationInfo GetSurfaceRotationInfo()
        {
            var result = new SurfaceRotationInfo();
            var normal = m_groundRay.Normal;

            var normRgt = -Vector3.Cross(this.transform.forward, m_groundRay.Normal);
            var normfwd = Vector3.Cross(normRgt, m_groundRay.Normal);

            var normBck = -normfwd.normalized;
            var normLft = -normRgt;

            Debug.DrawRay(m_groundRay.Positon, normal, Color.magenta, 0.0f, false);
            Debug.DrawRay(m_groundRay.Positon, normfwd, Color.yellow, 0.0f, false);
            Debug.DrawRay(m_groundRay.Positon, normBck, Color.blue, 0.0f, false);
            Debug.DrawRay(m_groundRay.Positon, normRgt, Color.blue, 0.0f, false);
            Debug.DrawRay(m_groundRay.Positon, normLft, Color.blue, 0.0f, false);

            result.TargetRotation = Quaternion.FromToRotation(this.transform.up, normal);

            result.NormalForward = normfwd;
            result.NoramlBack = normBck;
            result.NormalRight = normRgt;
            result.NormalLeft = normLft;

            return result;
        }

        /// <summary>
        /// Makes the actor perform a jump.
        /// </summary>
        public void Jump()
        {
            if (m_canJump)
            {
                m_rigidBody.AddForce(m_body.transform.up * m_jumpForce, ForceMode.Impulse);
                m_jumpTimer.Start();
                m_canJump = false;
            }
        }

        /// <summary>
        /// Triggers when the jump timer has elapsed, and the actor can perform another jump.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event Args <see cref="TimerElapsedEventArgs"/></param>
        private void JumpTimer_OnTimerElapsed(object sender, TimerElapsedEventArgs e)
        {
            m_jumpTimer.Stop();
            m_jumpTimer.ResetTimer();

            m_canJump = true;
        }

        /// <summary>
        /// Makes the actor crouch.
        /// </summary>
        public void Crouch()
        {
            m_crouch = true;
        }

        //private void OnTriggerEnter(Collider other)
        //{


        //    //m_state.FeetOnGround = other.gameObject.layer != LayerMask.GetMask("Players");
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    //m_state.FeetOnGround = false;
        //}

        //private void OnCollisionEnter(Collision collision)
        //{

        //    //m_colDict.TryAdd(collision.collider.GetInstanceID(), collision);

        //    //var mask = LayerMask.GetMask("Level", "Asteroid_Mesh_Rock", "Level_Buildings");

        //    //if ((mask & (1 << collision.collider.gameObject.layer)) != 0)
        //    //{
        //    //    m_state.FeetOnGround = true;
        //    //}
        //}

        //private void OnCollisionExit(Collision collision)
        //{
        //    //bool feetOnGround = false;

        //    //m_colDict.Remove(collision.collider.GetInstanceID());

        //    //foreach (var kvp in m_colDict)
        //    //{
        //    //    var mask = LayerMask.GetMask("Level", "Asteroid_Mesh_Rock", "Level_Buildings");

        //    //    if ((mask & (1 << kvp.Value.collider.gameObject.layer)) != 0)
        //    //    {
        //    //        feetOnGround = true;
        //    //        break;
        //    //    }
        //    //}

        //    //m_state.FeetOnGround = feetOnGround;
        //}
    }
}
