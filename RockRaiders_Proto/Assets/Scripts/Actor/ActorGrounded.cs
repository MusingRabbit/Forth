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

    internal class ActorGrounded : MonoBehaviour
    {
        [SerializeField]
        private float m_moveSpeed;

        [SerializeField]
        private float m_jumpForce;

        [SerializeField]
        private float m_jumpTimeout;

        [SerializeField]
        private float m_gravStrength;

        [SerializeField]
        private float m_rotationSpeed;

        private bool m_crouchToggle;

        private PlayerController m_controller;
        private ActorState m_state;
        private Rigidbody m_rigidBody;
        private ActorGroundRay m_groundRay;
        
        private bool m_canJump;

        private Timer m_jumpTimer;

        [SerializeField]
        private GameObject m_body;

        [SerializeField]
        private GameObject m_head;

        [SerializeField]
        private ActorCamera m_camera;

        private SurfaceRotationInfo m_surfaceInfo;
        private Quaternion m_tgtSurfRot;
        private Quaternion m_tgtLookatRot;
        private Vector3 m_moveVector;

        private Dictionary<int, Collision> m_colDict;

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

        public ActorGrounded()
        {
            m_colDict = new Dictionary<int, Collision>();

            m_jumpTimer = new Timer();
            m_jumpTimer.SetTimeSpan(TimeSpan.FromSeconds(m_jumpTimeout));
            m_jumpTimer.OnTimerElapsed += this.JumpTimer_OnTimerElapsed;
            m_canJump = true;

            m_moveSpeed = 5.0f;
            m_jumpForce = 20.0f;
            m_jumpTimeout = 3.0f;
            m_gravStrength = 20.0f;
            m_rotationSpeed = 150.0f;
        }

        private void Start()
        {
            m_controller = this.GetComponent<PlayerController>();
            m_state = this.GetComponent<ActorState>();
            m_rigidBody = this.GetComponent<Rigidbody>();
            m_groundRay = this.GetComponent<ActorGroundRay>();
        }



        private void Update()
        {
            var moveX = m_controller?.MoveAxis.x * (m_moveSpeed * Time.deltaTime) ?? 0.0f;
            var moveZ = m_controller?.MoveAxis.y * (m_moveSpeed * Time.deltaTime) ?? 0.0f;
            m_moveVector = new Vector3(moveX, 0.0f, moveZ);

            m_state.IsMoving = m_moveVector.magnitude > 0.0f;

            m_jumpTimer.Tick();
            m_state.IsCrouched = m_crouchToggle;

            this.UpdateSurfaceRotaion();

            this.UpdateMovement();
        }

        private void UpdateSurfaceRotaion()
        {
            if (m_groundRay.Hit)
            {
                m_surfaceInfo = this.GetSurfaceRotationInfo();

                //this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, info.TargetRotation, 50.0f * Time.deltaTime);

                m_tgtSurfRot = ((m_surfaceInfo.TargetRotation * this.transform.rotation)); //* m_camera.PlanarRotaion);


                m_tgtLookatRot = m_camera.PlanarRotaion;

                var rotation = m_tgtSurfRot * m_tgtLookatRot;


                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, rotation, m_rotationSpeed * Time.deltaTime);
            }
        }

        private void UpdateMovement()
        {
            m_moveVector = Vector3.zero;

            if (m_state.IsMoving && m_rigidBody.velocity.magnitude < m_moveSpeed)
            {
                m_moveVector = this.GetMoveVector(m_surfaceInfo, m_controller.MoveAxis);
            }
            else
            {
                m_rigidBody.velocity -= m_rigidBody.velocity * (0.98f * Time.deltaTime);
            }

            if (m_groundRay.Hit)
            {

                var gravity = (m_groundRay.Normal.normalized * (-m_gravStrength * m_rigidBody.mass));
                m_rigidBody.AddForce(gravity * Time.deltaTime, ForceMode.Force);

                m_rigidBody.AddForce(m_moveVector * (m_moveSpeed * Time.deltaTime), ForceMode.Impulse);
            }

        }

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

        private SurfaceRotationInfo GetSurfaceRotationInfo()
        {
            var result = new SurfaceRotationInfo();

            //var deltaV = m_groundRay.Positon - m_body.transform.position;
            //deltaV.y = deltaV.y - (m_groundRay.Positon.y - m_body.transform.position.y);

            //var deltaRot = Quaternion.FromToRotation(m_body.transform.position, m_groundRay.HitInfo.point);

            var normal = m_groundRay.Normal;

            var normRgt = -Vector3.Cross(this.transform.forward, m_groundRay.Normal);
            var normfwd = Vector3.Cross(normRgt, m_groundRay.Normal);

            //var v1 = new Vector2(normfwd.x, normfwd.z);
            //var v2 = new Vector2(this.transform.forward.x, this.transform.forward.z);

            //var angleBetween = v2.y < 0 ? -Vector2.Angle(v1, v2) : Vector2.Angle(v1, v2);
            //Debug.Log("V1 : " + v1);
            //Debug.Log("V2 : " + v2);
            
            //Debug.Log("Angle Between : " + angleBetween);

            //normfwd = Quaternion.Euler(0, angleBetween, 0) * normfwd;


            var normBck = -normfwd.normalized;

            //var normRgt = Vector3.Cross(normal, normfwd);
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

        internal void Jump()
        {
            if (m_canJump)
            {
                m_rigidBody.AddForce(m_body.transform.up * m_jumpForce, ForceMode.Impulse);
                m_jumpTimer.Start();
                m_canJump = false;
            }
        }

        private void JumpTimer_OnTimerElapsed(object sender, Events.TimerElapsedEventArgs e)
        {
            m_jumpTimer.Stop();
            m_jumpTimer.ResetTimer();

            m_canJump = true;
        }

        internal void ToggleCrouch()
        {
            m_crouchToggle = !m_crouchToggle;
        }

        private void OnCollisionEnter(Collision collision)
        {
            m_colDict.TryAdd(collision.collider.GetInstanceID(), collision);

            var mask = LayerMask.GetMask("Level");

            if ((mask & (1 << collision.collider.gameObject.layer)) != 0)
            {
                m_state.FeetOnGround = true;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            bool feetOnGround = false;

            m_colDict.Remove(collision.collider.GetInstanceID());

            foreach(var kvp in m_colDict)
            {
                var mask = LayerMask.GetMask("Level");

                if ((mask & (1 << kvp.Value.collider.gameObject.layer)) != 0)
                {
                    feetOnGround = true;
                    break;
                }
            }

            m_state.FeetOnGround = feetOnGround;
        }

    }
}
