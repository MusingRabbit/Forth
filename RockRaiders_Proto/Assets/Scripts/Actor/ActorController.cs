using Assets.Scripts;
using Assets.Scripts.Actor;
using Assets.Scripts.Events;
using Assets.Scripts.Input;
using Assets.Scripts.Network;
using Assets.Scripts.Pickups;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Pickups.Weapons.Projectiles;
using Assets.Scripts.Services;
using Assets.Scripts.Util;
using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class ActorController : RRMonoBehaviour
{
    private PlayerInput m_controller;
    private ActorState m_state;
    private ActorGrounded m_grounded;
    private ActorAnimController m_animController;
    private ActorFloating m_floating;
    private ActorGroundRay m_groundRay;
    private ActorCrosshair m_crosshair;
    private ActorHealth m_health;
    private ActorPickup m_pickup;
    private ActorAudio m_audio;
    private Rigidbody m_rigidBody;


    private GameObject m_body;
    private GameObject m_head;
    private GameObject m_RHGrip;
    private GameObject m_rArm;
    private GameObject m_lArm;
    private GameObject m_neck;

    private BoxCollider m_headBoxCollider;

    public Team Team
    {
        get
        {
            return m_state.Team;
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

    }

    public override void Initialise()
    {
        // I know this is frowned up. So sue me - its only once on initialisation. Might change later, didn't want to have unnecessary duplicate serialized fields.
        m_body = this.gameObject.FindChild("Body");
        m_head = this.gameObject.FindChild("Head");
        m_neck = this.gameObject.FindChild("Neck");
        m_RHGrip = this.gameObject.FindChild("RH_Grip");
        m_rArm = this.gameObject.FindChild("Body.UpperBody.RArm.UpperArmGroup");
        m_lArm = this.gameObject.FindChild("Body.UpperBody.LArm.UpperArmGroup");

        m_controller = this.GetComponent<PlayerInput>();
        m_state = this.GetComponent<ActorState>();
        m_floating = this.GetComponent<ActorFloating>();
        m_grounded = this.GetComponent<ActorGrounded>();
        m_groundRay = this.GetComponent<ActorGroundRay>();
        m_rigidBody = this.GetComponent<Rigidbody>();
        m_animController = this.GetComponent<ActorAnimController>();
        m_crosshair = this.GetComponent<ActorCrosshair>();
        m_health = this.GetComponent<ActorHealth>();
        m_pickup = this.GetComponent<ActorPickup>();
        m_audio = this.GetComponent<ActorAudio>();

        m_pickup.OnItemPickedUp += Pickup_OnItemPickedUp;
        m_pickup.OnItemDropped += Pickup_OnItemDropped;

        m_headBoxCollider = m_head.GetComponent<BoxCollider>();

        m_grounded.Body = m_body;
        m_floating.Body = m_body;

        m_floating.Head = m_head;

        m_state.GravBootsEnabled = true;
    }

    // Start is called before the first frame update
    private void Start()
    {
        this.Initialise();
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_state.IsDead)
        {
            m_pickup.DropSelectedWeapon();
            m_pickup.DropCurrentPack();
            m_animController.PlayAnimationForActorState(m_state);
            m_state.IsMoving = false;
            return;
        }

        //m_dropTimer.Tick();

        if (m_controller != null)
        {
            this.ProcessControllerActions();
        }

        if (m_state.GravBootsEnabled && m_groundRay.Hit)
        {
            m_grounded.enabled = true;
            m_floating.enabled = false;
            m_state.IsFloating = false;
        }
        else
        {
            if (m_floating.enabled == false)
            {
                m_floating.ResetRoll();                     // Sets the 'up' vector to be whatever orientation the player is currently in.
            }

            m_grounded.enabled = false;
            m_floating.enabled = true;
            m_state.IsFloating = true;
        }

        m_animController.PlayAnimationForActorState(m_state);
        this.Debug_DrawVelocityVector();
    }

    private void LateUpdate()
    {
        var canAnimate = !(m_state.IsDying || m_state.IsDying);

        if (canAnimate)
        {
            this.UpdateActorHeadRotation();

            if (m_state.SelectedWeapon != SelectedWeapon.None && !m_state.IsFloating)
            {
                this.UpdateActorArmRotation();
            }
        }

        this.UpdateSelectedWeaponWorldPos();
    }

    public override void Reset()
    {
        m_controller.Reset();
        m_state.Reset();
        m_floating.Reset();
        m_grounded.Reset();
        m_groundRay.Reset();
        
        m_animController.Reset();
        m_crosshair.Reset();
        m_health.Reset();

        m_rigidBody.ResetVelocity();
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
                    m_pickup.DropSelectedWeapon();
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
        m_state.GravBootsEnabled = !m_state.GravBootsEnabled;
    }

    private void Weapon_OnShotFired(object sender, OnShotFiredEventArgs e)
    {
        this.HandleRecoil(e.ProjectileVelocity, e.ProjectileMass);
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
        }
    }

    private void UpdateActorArmRotation()
    {
        var rArmTrans = m_rArm.GetComponent<Transform>();
        var lArmTrans = m_lArm.GetComponent<Transform>();
        var rArmPos = rArmTrans.position;
        var crossPos = m_crosshair.AimPoint;

        var deltaVector = rArmPos - crossPos;
        var rotation = Quaternion.LookRotation(deltaVector, this.transform.up);

        //Debug.Log(rotation.eulerAngles);

        bool inverted = this.transform.up.y < 0.0f;

        var clampVal = Mathf.Clamp(rotation.eulerAngles.x, 0, 55);

        var rotAmt = Quaternion.Euler(0, 0, inverted ? -rotation.eulerAngles.x : rotation.eulerAngles.x);
        var rotAmtZ = Mathf.Abs(rotAmt.eulerAngles.z - 180 < 0 ? rotAmt.eulerAngles.z : rotAmt.eulerAngles.z - 360);

        //Debug.Log("rotAmtZ : " + rotAmtZ);

        if (rotAmtZ < 25)
        {
            rArmTrans.rotation *= rotAmt;
            lArmTrans.rotation = rArmTrans.rotation;
        }
    }

    private void UpdateActorHeadRotation()
    {
        var headTransform = m_head.GetComponent<Transform>();
        var neckTransform = m_neck.GetComponent<Transform>();

        var detlaVector = m_crosshair.AimPoint - headTransform.position;
        var rotation = Quaternion.LookRotation(detlaVector, headTransform.up);
        headTransform.rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z);
        headTransform.Rotate(0, -90, 0);
    }

    private void HandleRecoil(Vector3 velocity, float mass)
    {
        if (m_state.IsFloating)
        {
            m_rigidBody.AddForce(-velocity * mass, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            var gameObj = contact.otherCollider.gameObject;

            if (gameObj.IsPickupItem())
            {
                if (gameObj.IsWeapon())
                {
                    var weapon = gameObj.GetComponent<Weapon>();
                    m_pickup.PickupWeapon(weapon);
                }
                else
                {
                    var flag = gameObj.GetComponent<Flag>();

                    if (flag != null)
                    {
                        if (flag.Team == m_state.Team && !flag.Retreived)
                        {
                            NotificationService.Instance.Info($"{flag.Team} flag retreived");
                            flag.Retreived = true;
                            return;
                        }
                        else if (flag.Team != m_state.Team)
                        {
                            m_pickup.PickupPack(flag);
                        }
                    }
                    else
                    {
                        m_pickup.PickupPack(gameObj.GetComponent<PickupItem>());
                    }
                    
                }
            }

            if (gameObj.IsProjectile())
            {
                var projectile = gameObj.GetComponent<Projectile>();
                var projectileNet = gameObj.GetComponent<ProjectileNetwork>();
                var projCollider = gameObj.GetComponent<CapsuleCollider>();

                if (m_headBoxCollider.bounds.Intersects(projCollider.bounds)) // Check if it's a headshot.
                {
                    m_health.RegisterProjectileHitHead(gameObj);
                }
                else
                {
                    m_health.RegisterProjectileHit(gameObj, 1.0f);
                }

                NotificationService.Instance.Info("Projectile hit " + this.State.PlayerName);

                projectileNet.HitNetworkObjectId = this.GetComponent<NetworkObject>().NetworkObjectId;

                projectile.Despawn(collision);
            }
        }
    }

    public float GetCurrentMoveSpeed()
    {
        return m_state.IsFloating ? m_floating.MoveSpeed : m_grounded.MoveSpeed;
    }

    private void Pickup_OnItemDropped(object sender, OnPickupEventArgs e)
    {
        if (e.Item.gameObject.IsWeapon())
        {
            var weapon = e.Item.GetComponent<Weapon>();
            weapon.OnShotFired -= this.Weapon_OnShotFired;
        }
    }

    private void Pickup_OnItemPickedUp(object sender, OnPickupEventArgs e)
    {
        if (e.Item.gameObject.IsWeapon())
        {
            var weapon = e.Item.GetComponent<Weapon>();
            weapon.OnShotFired += this.Weapon_OnShotFired;
        }
    }
}
