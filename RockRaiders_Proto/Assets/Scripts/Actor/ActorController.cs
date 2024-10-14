using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Assets.Scripts.Util;
using System;
using Assets.Scripts;
using Assets.Scripts.Weapons;
using static Assets.Scripts.Util.PhysicsExtensions;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Assets.Scripts.Actor;


public enum SelectedWeapon
{
    None,
    Main,
    Sidearm,
    Pack
}

public struct ProcSurfaceRotationResult
{
    public RaycastHit info;
    public Vector3 normalLeft;
    public bool inContact;
    public Vector3 normalRight;
}

public class ActorController : MonoBehaviour
{ 
    [SerializeAs("Animator")]
    [SerializeField]
    private Animator m_animator;

    [SerializeAs("MovementForce")]
    [SerializeField]
    private float m_moveForce;

    [SerializeAs("MaxSpeed")]
    [SerializeField]
    private float m_maxSpeed;

    [SerializeAs("MaxWalkSpeed")]
    [SerializeField]
    private float m_maxWalkSpeed;

    [SerializeAs("GravStrength")]
    [SerializeField]
    private float m_gravStrength;

    [SerializeAs("UICrosshair")]
    [SerializeField]
    private GameObject m_crosshair;

    [SerializeField]
    private Team m_team;

    private PlayerController m_controller;

    private Rigidbody m_rigidBody;
    
    private GameObject m_body;
    private GameObject m_head;
    private GameObject m_neck;
    private GameObject m_rArm;
    private GameObject m_lArm;
    private GameObject m_sideArmHolster;
    private GameObject m_mainWeaponHolster;
    private GameObject m_RHGrip;

    private bool m_facingRight = true;
    private bool m_movingFwd = true;

    private bool m_inProximity = false;
    private bool m_floating = false;

    private float m_currHeadRot = 0.0f;
    
    private double m_dropTimeOut = 1.0f;
    private bool m_canPickup;
    private double m_jumpTimeout = 2.0f;
    private bool m_canJump;

    private bool m_gravBootsToggle = false;
    private bool m_crouchToggle = false;

    [SerializeAs("JumpForce")]
    [SerializeField]
    private float m_jumpForce = 20.0f;

    private float m_dropForce = 3.0f;

    private Assets.Scripts.Timer m_dropTimer;
    private Assets.Scripts.Timer m_jumpTimer;

    private GameObject m_lowerBody;
    private RaycastHit m_surfaceInfo;

    private ActorNetwork m_network;
    private ActorState m_state;
    private ActorAnimController m_animController;

    public Team Team
    {
        get
        {
            return m_team;
        }
    }

    public Animator Animator
    {
        get
        {
            return m_animator;
        }
        set
        {
            m_animator = value;
        }
    }

    public GameObject Crosshair
    {
        get
        {
            return m_crosshair;
        }
        set
        {
            m_crosshair = value;
        }
    }

    public ActorState State
    {
        get
        {
            return m_state;
        }
    }

    public ActorController()
    {
        m_maxWalkSpeed = 10.0f;
        m_gravStrength = 9.8f;
        m_moveForce = 1.0f;
        m_maxSpeed = 1.0f;

        m_dropTimer = new Assets.Scripts.Timer();
        m_dropTimer.SetTimeSpan(TimeSpan.FromSeconds(m_dropTimeOut));
        m_dropTimer.OnTimerElapsed += this.DropTimer_OnTimerElapsed;
        m_canPickup = true;

        m_jumpTimer = new Assets.Scripts.Timer();
        m_jumpTimer.SetTimeSpan(TimeSpan.FromSeconds(m_jumpTimeout));
        m_jumpTimer.OnTimerElapsed += this.JumpTimer_OnTimerElapsed;
        m_canJump = true;

        m_state = new ActorState();

        m_team = Team.None;
    }

    private void JumpTimer_OnTimerElapsed(object sender, Assets.Scripts.Events.TimerElapsedEventArgs e)
    {
        m_jumpTimer.Stop();
        m_jumpTimer.ResetTimer();

        m_canJump = true;
    }

    private void DropTimer_OnTimerElapsed(object sender, Assets.Scripts.Events.TimerElapsedEventArgs e)
    {
        m_dropTimer.Stop();
        m_dropTimer.ResetTimer();

        m_canPickup = true;
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        m_body = this.gameObject.FindChild("Body");
        
        m_neck = m_body.FindChild("UpperBody.Neck");
        m_head = m_body.FindChild("UpperBody.Head");
        m_rArm = m_body.FindChild("UpperBody.RArm.UpperArmGroup");
        m_lArm = m_body.FindChild("UpperBody.LArm.UpperArmGroup");
        m_sideArmHolster = m_body.FindChild("SidearmHolster");
        m_mainWeaponHolster = m_body.FindChild("MainWeaponHolster");
        m_RHGrip = m_body.FindChild("RH_Grip");

        m_rigidBody = this.gameObject.GetComponent<Rigidbody>();
        m_lowerBody = m_body.FindChild("LowerBody");

        m_controller = this.GetComponent<PlayerController>();

        //m_controller.Start();

        m_animator = this.GetComponent<Animator>();
        m_network = this.GetComponent<ActorNetwork>();

        m_animController = new ActorAnimController(m_animator);
    }

    public virtual void FixedUpdate()
    {
        this.UpdateIsFloating(!m_movingFwd);

        if (m_controller != null)
        {
            this.ProcessSetActions();
        }

        this.OrientToCrosshair();
        this.ProcessMovement();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        m_dropTimer.Tick();
        m_jumpTimer.Tick();

        m_state.IsFloating = m_floating;
        m_state.IsCrouched = m_crouchToggle;
        //m_gravBootsToggle = false;

        m_animController.PlayAnimationForActorState(m_state);
    }

    public virtual void LateUpdate()
    {
        this.UpdateActorHeadRotation();

        if (m_state.SelectedWeapon != SelectedWeapon.None && !m_state.IsFloating)
        {
            this.UpdateActorArmRotation();
        }

        this.UpdateStoredItemWorldPos();
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            var rhsGameObj = contact.otherCollider.gameObject;

            if (m_gravBootsToggle == false && ((1 << rhsGameObj.layer) & LayerMask.GetMask("Level")) != 0)
            {
                m_floating = false;
            }

            if (this.IsWeapon(rhsGameObj) && m_canPickup)
            {
                this.PickupWeapon(rhsGameObj);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
    }

    private void OnCollisionExit(Collision collision)
    {
    }


    private void UpdateIsFloating(bool flipRotation)
    {
        var currPos = m_body.transform.position;
        var rayOrigin = new Vector3(currPos.x, currPos.y - 0.2f, currPos.z);
        var rotOffset = Quaternion.Euler(0, 90, 0);// flipRotation ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);

        var groundTouchedRay = new Ray(rayOrigin, -m_body.transform.up);

        var args = new ArcCastArgs
        {
            Centre = m_body.transform.position,
            Rotation = m_body.transform.rotation * rotOffset,
            Angle = 359,
            Radius = 2.0f,
            Resolution = 7,
            Layer = LayerMask.GetMask("Level"),
            DrawDebug = true,
            DebugColour = Color.blue
        };


        var touchingGround = PhysicsExtensions.ArcCast(args, out var hitInfo);

        if (touchingGround)
        {
            m_surfaceInfo = hitInfo;
        }

        if (touchingGround)
        {
            Debug.DrawRay(rayOrigin, groundTouchedRay.direction, Color.red, 0.0f, false);
        }
        else
        {
            Debug.DrawRay(rayOrigin, groundTouchedRay.direction, Color.green, 0.0f, false);
        }

        m_inProximity = touchingGround;
        
        if (m_inProximity)
        {
            m_floating = !touchingGround;
        }
        else
        {
            m_floating = true;
        }
    }

    private bool IsWeapon(GameObject gameObj)
    {
        return gameObj.GetComponent<Weapon>() != null;
    }

    private void UpdateActorHeadRotation()
    {
        var headTransform = m_head.GetComponent<Transform>();
        var neckTransform = m_neck.GetComponent<Transform>();

        var detlaVector = headTransform.position - m_crosshair.transform.position;
        var rotation = (Mathf.Atan2(detlaVector.x, detlaVector.y) * (180/Mathf.PI)) - 90;

        if (detlaVector.x < 0)
        {
            rotation = -rotation;
        }

        rotation = this.GetClampedHeadRotation(rotation);

        //Debug.Log("Head Rot : " + rotation);

        m_currHeadRot = rotation;

        headTransform.localRotation = Quaternion.AngleAxis(rotation, Vector3.forward);
    }

    private float GetClampedHeadRotation(float rotation)
    {
        if (m_facingRight)
        {
            return Mathf.Clamp(rotation, 150, 230);
        }
        else
        {
            return Mathf.Clamp(rotation, -40, 40);
        }
    }

    private void UpdateActorArmRotation()
    {
        var rArmTrans = m_rArm.GetComponent<Transform>();
        var lArmTrans = m_lArm.GetComponent<Transform>();

        var deltaVector = rArmTrans.position - m_crosshair.transform.position;
        var offset = m_facingRight ? (Mathf.PI * 0.5f) : (Mathf.PI * 0.5f);
        var rotation = -(Mathf.Atan2(deltaVector.x, deltaVector.y) + offset);
        var fwd = m_facingRight ? rArmTrans.forward : -rArmTrans.forward;
        rotation *= Mathf.Rad2Deg;

        rArmTrans.rotation = Quaternion.AngleAxis(rotation, fwd);
        lArmTrans.rotation = rArmTrans.rotation;

        if (!m_facingRight)
        {
            // This is seriously *special* stuff - but it works.
            var rArmPos = rArmTrans.position;
            rArmTrans.position = lArmTrans.position;
            lArmTrans.position = rArmPos;
        }
    }

    private ProcSurfaceRotationResult ProcessSurfaceRotation()
    {
        var result = new ProcSurfaceRotationResult();
        result.inContact = false;

        if (m_inProximity)
        {
            result.inContact = true;
            result.info = m_surfaceInfo;

            var normal = m_surfaceInfo.normal;

            var normLeft = (Quaternion.Euler(0, 0, -90) * normal).normalized * (m_moveForce / 10);
            var normRight = (Quaternion.Euler(0, 0, 90) * normal).normalized * (m_moveForce / 10);

            result.normalLeft = normLeft;
            result.normalRight = normRight;

            Debug.DrawRay(m_surfaceInfo.point, normal, Color.magenta, 0.0f, false);
            Debug.DrawRay(m_surfaceInfo.point, normLeft, Color.yellow, 0.0f, false);
            Debug.DrawRay(m_surfaceInfo.point, normRight, Color.blue, 0.0f, false);

            //m_rigidBody.AddForce(-normal * 4.0f, ForceMode.Force);
            //m_body.transform.position += normalLeft;
            //Debug.Log(normalLeft);

            var rot = m_body.transform.rotation;

            //if (!m_movingFwd)
            //{
            //    rot = m_body.transform.rotation * Quaternion.Euler(0, 180, 0);
            //}

            var tgtRot = Quaternion.FromToRotation(m_body.transform.up, m_surfaceInfo.normal) * rot;
            m_body.transform.rotation = Quaternion.Slerp(rot, tgtRot, 50 * Time.deltaTime);
        }

        return result;
    }

    private void ProcessMovement()
    {
        var moveX = m_controller?.MoveAxis.x * (m_moveForce) ?? 0.0f;
        var moveY = m_controller?.MoveAxis.y * (m_moveForce) ?? 0.0f;
        var moveZ = 0.0f;
        var moveVector = new Vector3(moveX, moveY, moveZ);

        //Debug.Log("Move Vector : " + moveVector);

        m_state.IsMoving = moveVector.magnitude > 0.0f;
        var canMoveX = m_maxSpeed > Mathf.Abs(m_rigidBody.velocity.x + moveVector.x);
        var canMoveY = m_maxSpeed > Mathf.Abs(m_rigidBody.velocity.y + moveVector.y);

        m_state.IsMovingForward = m_facingRight == true && Mathf.Abs(moveVector.x + (m_crosshair.transform.position.x - this.transform.position.x)) > Mathf.Abs(moveVector.x)
            || m_facingRight == false && Mathf.Abs(moveVector.x + (m_crosshair.transform.position.x - this.transform.position.x)) < Mathf.Abs(moveVector.x);

        if (m_inProximity && m_surfaceInfo.transform != null && m_gravBootsToggle == false)
        {
            var info = this.ProcessSurfaceRotation();

            var force = m_surfaceInfo.normal;
            force = force.normalized * (-m_gravStrength);
            m_rigidBody.AddForce(force, ForceMode.Force);


            var delta = m_surfaceInfo.normal - new Vector3(moveVector.x, moveVector.y, 0.0f);
            var cross = Vector3.Cross(delta, m_surfaceInfo.normal);
            var moveDirLeft = cross.z < 0.0f;
            //Debug.Log("Detla : " + delta + "| Cross: " + cross);

            var actMoveVector = (moveDirLeft ? info.normalLeft : info.normalRight) * (m_moveForce / 10);

            if (m_floating == false)
            {
                var rhsGameObj = info.info.collider.gameObject;
                var latchObj = rhsGameObj.FindChild("LatchObject");
                if (latchObj != null)
                {
                    m_network.SetParent(latchObj);
                }

                if (m_state.IsMoving && m_rigidBody.velocity.magnitude < m_maxWalkSpeed)
                {
                    m_rigidBody.AddRelativeForce(actMoveVector, ForceMode.Impulse);
                }
            }
            else
            {
                m_network.SetParent(null);
            }
        }
        else
        {
            if (m_state.IsMoving)
            {
                var moveVal = moveVector;
                m_rigidBody.AddForce(moveVal, ForceMode.Force);
            }
            else
            {
                m_rigidBody.velocity -= m_rigidBody.velocity * (0.95f * Time.deltaTime);
            }
        }


        Debug.DrawRay(m_body.transform.position, m_rigidBody.velocity, Color.cyan);
    }


    private void UpdateSelectedWeaponWorldPos()
    {
        GameObject selectedWeapon = null;

        switch (m_state.SelectedWeapon)
        {
            case SelectedWeapon.None:
                break;
            case SelectedWeapon.Main:
                var mainWep = m_state.Inventory.GetMainWeapon();
                selectedWeapon = mainWep;
                break;
            case SelectedWeapon.Sidearm:
                var sideArm = m_state.Inventory.GetSideArm();
                selectedWeapon = sideArm;
                break;
            case SelectedWeapon.Pack:
                break;
        }


        if (selectedWeapon != null)
        {
            var pos = m_crosshair.transform.position;
            var rhGripPos = m_RHGrip.transform.position;
            selectedWeapon.transform.position = rhGripPos;
            selectedWeapon.transform.LookAt(new Vector3(pos.x, pos.y, pos.z));
            selectedWeapon.transform.Rotate(new Vector3(0, 1, 0), -90);
        }
    }

    private void PickupWeapon(GameObject weaponObj)
    {
        var weapon = weaponObj.GetComponent<Weapon>();

        switch (weapon.WeaponSlot)
        {
            case WeaponSlot.Main:

                if (!m_state.Inventory.HasMainWeapon())
                {
                    m_state.Inventory.SetMainWeapon(weaponObj);
                }

                break;
            case WeaponSlot.Sidearm:

                if (!m_state.Inventory.HasSideArm())
                {
                    m_state.Inventory.SetSideArm(weaponObj);
                }

                break;
        }

        weaponObj.GetComponent<Rigidbody>().isKinematic = true;
        weaponObj.GetComponent<Rigidbody>().detectCollisions = false;
        weaponObj.transform.position = m_sideArmHolster.transform.position;
        weaponObj.transform.rotation = m_sideArmHolster.transform.rotation;
        weapon.Owner = this.gameObject;
    }

    private void UpdateStoredItemWorldPos()
    {
        var sideArmObj = m_state.Inventory.GetSideArm();

        if (sideArmObj != null)
        {
            sideArmObj.transform.position = m_sideArmHolster.transform.position;
            sideArmObj.transform.rotation = m_sideArmHolster.transform.rotation;
        }

        var mainWeaponObj = m_state.Inventory.GetMainWeapon();

        if (mainWeaponObj != null)
        {
            mainWeaponObj.transform.position = m_mainWeaponHolster.transform.position;
            mainWeaponObj.transform.rotation = m_mainWeaponHolster.transform.rotation;
        }

        this.UpdateSelectedWeaponWorldPos();
    }

    private void OrientToCrosshair()
    {
        var oldFacingRightVal = m_facingRight;

        //Debug.Log(this.transform.position.x - m_crosshair.transform.position.x);

        if (m_floating)
        {
            m_body.transform.LookAt(m_crosshair.transform);
            m_body.transform.Rotate(new Vector3(0, 1, 0), -90);
        }
        else
        {
            var delta = m_crosshair.transform.position - m_body.transform.position;
            var cross = Vector3.Cross(delta, m_body.transform.up);

            if (cross.z > 1.0f)
            {
                m_facingRight = true;
            }
            else if (cross.z < -1.0f)
            {
                m_facingRight = false;
            }

            //Debug.Log("Cross Product : " + cross);
            //Debug.Log("Current Rot : " + m_body.transform.rotation.eulerAngles);

            var angleY = Mathf.Round(m_body.transform.eulerAngles.y);
            var minY = 0;
            var maxY = 360;
            var midY = 180;

            var currFacingRight = angleY < midY && angleY >= minY;
            var currFacingLeft = angleY < maxY && angleY > midY;

            if (m_facingRight && currFacingRight == false)
            {
                m_body.transform.rotation *= Quaternion.Euler(0, angleY, 0);
            }

            if (!m_facingRight && currFacingLeft == false)
            {
                angleY = 180 - angleY;
                m_body.transform.rotation *= Quaternion.Euler(0, angleY, 0);
            }
        }
    }

    protected Animator GetAnimator()
    {
        return m_animator;
    }

    protected bool IsFacingRight()
    {
        return m_facingRight;
    }

    private void ProcessSetActions()
    {
        var actionList = m_controller.GetActiveControllerActions().ToList();

        foreach (var action in actionList)
        {
            switch (action)
            {
                case ControllerActions.Trigger:
                    this.FireSelectedWeapon();
                    break;
                case ControllerActions.Jump:
                    this.Jump();
                    break;
                case ControllerActions.Crouch:
                    m_crouchToggle = !m_crouchToggle;
                    break;
                case ControllerActions.Use:
                    break;
                case ControllerActions.Drop:
                    this.DropSelectedWeapon();
                    break;
                case ControllerActions.Throw:
                    break;
                case ControllerActions.Melee:
                    break;
                case ControllerActions.EquipMain:
                    m_state.SelectWeapon(SelectedWeapon.Main);
                    break;
                case ControllerActions.EquipSide:
                    m_state.SelectWeapon(SelectedWeapon.Sidearm);
                    break;
                case ControllerActions.EquipPack:
                    break;
                case ControllerActions.GravBoots:
                    m_gravBootsToggle = !m_gravBootsToggle;
                    break;
                case ControllerActions.Pause:
                    break;
            }
        }
    }

    private void FireSelectedWeapon()
    {
        var weaponObj = m_state.GetSelectedWeapon();

        if (weaponObj != null)
        {
            var weapon = weaponObj.GetComponent<Weapon>();
            weapon.TriggerState = TriggerState.Pulled;
        }
    }

    private void Jump()
    {
        if (m_canJump && m_floating == false)
        {
            m_rigidBody.AddForce(m_body.transform.up * m_jumpForce, ForceMode.Impulse);
            m_jumpTimer.Start();
            m_canJump = false;
        }
    }

    private void DropSelectedWeapon()
    {
        var weaponObj = m_state.GetSelectedWeapon();

        if (weaponObj != null)
        {
            var weapon = weaponObj.GetComponent<Weapon>();
            var rb = weaponObj.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddForce(weaponObj.transform.right.normalized * m_dropForce, ForceMode.Impulse);

            switch (m_state.SelectedWeapon)
            {
                case SelectedWeapon.None:
                    break;
                case SelectedWeapon.Main:
                    m_state.Inventory.ClearMainWeapon();
                    break;
                case SelectedWeapon.Sidearm:
                    m_state.Inventory.ClearSideArm();
                    break;
                case SelectedWeapon.Pack:
                    break;
            }

            m_state.SelectWeapon(SelectedWeapon.None);

            m_dropTimer.Start();
            m_canPickup = false;

            weapon.Owner = null;
        }

        
    }


}
