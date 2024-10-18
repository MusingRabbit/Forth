using Assets.Scripts;
using Assets.Scripts.Actor;
using Assets.Scripts.Util;
using Assets.Scripts.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    private PlayerController m_controller;
    private ActorState m_state;
    private ActorGrounded m_grounded;
    private ActorAnimController m_animController;
    private ActorFloating m_floating;
    private ActorGroundRay m_groundRay;
    private ActorCrosshair m_crosshair;
    private Rigidbody m_rigidBody;
    private double m_dropTimeOut = 1.0f;
    private float m_dropForce = 3.0f;

    private float m_currHeadRot = 0.0f;

    private Timer m_dropTimer;

    private bool m_canPickup;
    private bool m_gravbootsToggle;


    private GameObject m_body;
    private GameObject m_head;
    private GameObject m_RHGrip;
    private GameObject m_rArm;
    private GameObject m_lArm;
    private GameObject m_neck;

    [SerializeField]
    private Team m_team;

    public Team Team
    {
        get
        {
            return m_team;
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
        m_dropTimer = new Timer();
        m_dropTimer.SetTimeSpan(TimeSpan.FromSeconds(m_dropTimeOut));
        m_dropTimer.OnTimerElapsed += this.DropTimer_OnTimerElapsed;
        m_canPickup = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_body = this.gameObject.FindChild("Body");
        m_head = this.gameObject.FindChild("Head");
        m_neck = this.gameObject.FindChild("Neck");
        m_RHGrip = this.gameObject.FindChild("RH_Grip");
        m_rArm = this.gameObject.FindChild("Body.UpperBody.RArm.UpperArmGroup");
        m_lArm = this.gameObject.FindChild("Body.UpperBody.LArm.UpperArmGroup");
        

        m_controller = this.GetComponent<PlayerController>();
        m_state = this.GetComponent<ActorState>();
        m_floating = this.GetComponent<ActorFloating>();
        m_grounded = this.GetComponent<ActorGrounded>();
        m_groundRay = this.GetComponent<ActorGroundRay>();
        m_rigidBody = this.GetComponent<Rigidbody>();
        m_animController = this.GetComponent<ActorAnimController>();
        m_crosshair = this.GetComponent<ActorCrosshair>();

        m_grounded.Body = m_body;
        m_floating.Body = m_body;

        m_floating.Head = m_head;
        m_grounded.Head = m_head;
    }

    // Update is called once per frame
    void Update()
    {
        m_dropTimer.Tick();

        m_state.Team = m_team;

        if (m_controller != null)
        {
            this.ProcessControllerActions();
        }


        if (m_groundRay.Hit)
        {
            m_grounded.enabled = true;
            m_floating.enabled = false;
            m_state.IsFloating = false;
        }
        else
        {
            m_grounded.enabled = false;
            m_floating.enabled = true;
            m_state.IsFloating = true;
        }

        m_animController.PlayAnimationForActorState(m_state);
        this.Debug_DrawVelocityVector();
    }

    private void LateUpdate()
    {
        this.UpdateActorHeadRotation();

        if (m_state.SelectedWeapon != SelectedWeapon.None && !m_state.IsFloating)
        {
            this.UpdateActorArmRotation();
        }

        this.UpdateSelectedWeaponWorldPos();
    }

    private void Debug_DrawVelocityVector()
    {
        Debug.DrawRay(m_body.transform.position, m_rigidBody.velocity, Color.cyan);
    }

    private void ProcessControllerActions()
    {
        var actionList = m_controller.GetActiveControllerActions();

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
                case ControllerActions.ThrustUp:
                    this.ThrustUp();
                    break;
                case ControllerActions.Crouch:
                    this.Crouch();
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
                    this.ToggleGravBoots();
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
        if (m_grounded.enabled)
        {
            m_grounded.Jump();
        }
    }

    private void ThrustUp()
    {
        if (m_floating.enabled)
        {
            m_floating.ThrustUp();
        }
    }

    private void Crouch()
    {
        if (m_grounded.enabled)
        {
            m_grounded.Crouch();
        }

        if (m_floating.enabled)
        {
            m_floating.ThrustDown();
        }
    }

    private void ToggleGravBoots()
    {
        m_gravbootsToggle = !m_gravbootsToggle;
    }

    private void DropTimer_OnTimerElapsed(object sender, Assets.Scripts.Events.TimerElapsedEventArgs e)
    {
        m_dropTimer.Stop();
        m_dropTimer.ResetTimer();

        m_canPickup = true;
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
        weapon.Owner = this.gameObject;
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
            var pos = m_crosshair.AimPoint;
            var rhGripPos = m_RHGrip.transform.position;
            selectedWeapon.transform.position = rhGripPos;
            selectedWeapon.transform.LookAt(new Vector3(pos.x, pos.y, pos.z), this.transform.up);
            selectedWeapon.transform.Rotate(new Vector3(0, 1, 0), -90);
        }
    }

    private void UpdateActorArmRotation()
    {
        var rArmTrans = m_rArm.GetComponent<Transform>();
        var lArmTrans = m_lArm.GetComponent<Transform>();
        var rArmPos = rArmTrans.position;
        var crossPos = m_crosshair.AimPoint;

        var deltaVector = rArmPos - crossPos;
        //deltaVector.y = 0;
        var rotation = Quaternion.LookRotation(deltaVector);

        Debug.Log(rotation.eulerAngles);

        rArmTrans.rotation *= Quaternion.Euler(0, 0, rotation.eulerAngles.x);
        lArmTrans.rotation = rArmTrans.rotation;
        // This is seriously *special* stuff - but it works.
        //rArmTrans.position = lArmTrans.position;
        //lArmTrans.position = rArmPos;
    }

    private void UpdateActorHeadRotation()
    {
        var headTransform = m_head.GetComponent<Transform>();
        var neckTransform = m_neck.GetComponent<Transform>();

        var detlaVector = headTransform.position - m_crosshair.transform.position;
        var rotation = (Mathf.Atan2(detlaVector.x, detlaVector.y) * (180 / Mathf.PI)) - 90;

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
        return Mathf.Clamp(rotation, 150, 230);
    }


    private void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            var gameObj = contact.otherCollider.gameObject;

            if (gameObj.IsWeapon() && m_canPickup)
            {
                this.PickupWeapon(gameObj);
            }
        }
    }
}
