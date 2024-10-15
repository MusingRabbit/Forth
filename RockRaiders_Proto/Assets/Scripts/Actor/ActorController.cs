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
    private GameObject m_body;
    private GameObject m_head;

    private PlayerController m_controller;
    private ActorState m_state;
    private ActorGrounded m_grounded;
    private ActorAnimController m_animController;
    private ActorFloating m_floating;
    private ActorGroundRay m_groundRay;
    private Rigidbody m_rigidBody;
    private double m_dropTimeOut = 1.0f;
    private float m_dropForce = 3.0f;

    private Timer m_dropTimer;

    private bool m_canPickup;
    private bool m_gravbootsToggle;

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

        m_controller = this.GetComponent<PlayerController>();
        m_state = this.GetComponent<ActorState>();
        m_floating = this.GetComponent<ActorFloating>();
        m_grounded = this.GetComponent<ActorGrounded>();
        m_groundRay = this.GetComponent<ActorGroundRay>();
        m_rigidBody = this.GetComponent<Rigidbody>();
        m_animController = this.GetComponent<ActorAnimController>();

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
            //var rhsGameObj = m_groundRay.HitInfo.collider?.gameObject;
            //if (rhsGameObj != null)
            //{
            //    this.transform.parent = rhsGameObj.transform.parent.transform;
            //    //m_network.SetParent(latchObj);
            //}


            m_grounded.enabled = true;
            m_floating.enabled = false;
            m_state.IsFloating = false;
        }
        else
        {
            this.transform.parent = null;
            //m_network.SetParent(null);

            m_grounded.enabled = false;
            m_floating.enabled = true;
            m_state.IsFloating = true;
        }

        m_animController.PlayAnimationForActorState(m_state);
        this.Debug_DrawVelocityVector();
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
                case ControllerActions.Crouch:
                    this.ToggleCrouch();
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

    private void ToggleCrouch()
    {
        if (m_grounded.enabled)
        {
            m_grounded.ToggleCrouch();
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
}
