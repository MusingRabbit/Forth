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

/// <summary>
/// Controller component for actor entity.
/// </summary>
public class ActorController : RRMonoBehaviour
{
    /// <summary>
    /// Represents the players control input
    /// </summary>
    private PlayerInput m_controller;
    
    /// <summary>
    /// The actors current state (data) component.
    /// </summary>
    private ActorState m_state;

    /// <summary>
    /// The actors' grounded behaviour.
    /// </summary>
    private ActorGrounded m_grounded;

    /// <summary>
    /// The actors' floating behaviour
    /// </summary>
    private ActorFloating m_floating;

    /// <summary>
    /// The actors' animation controller
    /// </summary>
    private ActorAnimController m_animController;
    
    /// <summary>
    /// The actors ground ray component. This is for detecting ground proximty and orientation.
    /// </summary>
    private ActorGroundRay m_groundRay;

    /// <summary>
    /// The actors crosshair component.
    /// </summary>
    private ActorCrosshair m_crosshair;

    /// <summary>
    /// The actors' health component.
    /// </summary>
    private ActorHealth m_health;

    /// <summary>
    /// The actors' pickup component.
    /// </summary>
    private ActorPickup m_pickup;

    /// <summary>
    /// The actors' audio component.
    /// </summary>
    private ActorAudio m_audio;

    /// <summary>
    /// The actors rigidbody component.
    /// </summary>
    private Rigidbody m_rigidBody;

    //Body Parts...
    private GameObject m_body;
    private GameObject m_head;
    private GameObject m_RHGrip;
    private GameObject m_rArm;
    private GameObject m_lArm;
    private GameObject m_neck;

    /// <summary>
    /// Actors' head boxcollider component.
    /// </summary>
    private BoxCollider m_headBoxCollider;

    public ActorController()
    {

    }

    /// <summary>
    /// Initialises the actors' controller component.
    /// </summary>
    public override void Initialise()
    {
        // I know this is frowned upon. So sue me - its only once on initialisation. Might change later, didn't want to have unnecessary duplicate serialized fields.
        m_body = this.gameObject.FindChild("Body");
        m_head = this.gameObject.FindChild("Head");
        m_neck = this.gameObject.FindChild("Neck");
        m_RHGrip = this.gameObject.FindChild("RH_Grip");
        m_rArm = this.gameObject.FindChild("Body.UpperBody.RArm.UpperArmGroup");
        m_lArm = this.gameObject.FindChild("Body.UpperBody.LArm.UpperArmGroup");

        // Get all components
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
        m_headBoxCollider = m_head.GetComponent<BoxCollider>();

        m_grounded.Body = m_body;
        m_floating.Body = m_body;

        m_floating.Head = m_head;

        m_state.GravBootsEnabled = true;

        // Event subscription
        m_pickup.OnItemPickedUp += this.Pickup_OnItemPickedUp;
        m_pickup.OnItemDropped += this.Pickup_OnItemDropped;
    }

    // Start is called before the first frame update
    private void Start()
    {
        this.Initialise();
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_state.IsDead) // If actor is dead, drop everything, play death animation, and return.
        {
            m_pickup.DropSelectedWeapon(false);
            m_pickup.DropCurrentPack();
            m_animController.PlayAnimationForActorState(m_state);
            m_state.IsMoving = false;
            return;
        }

        if (m_controller != null) // If actor has controller attatched, process control inputs.
        {
            this.ProcessControllerActions();
        }

        if (m_state.GravBootsEnabled && m_groundRay.Hit) //If actor is touching a surface, and grav boots are enabled - enable actor grounded behaviour
        {
            m_state.FeetOnGround = true;
            m_grounded.enabled = true;
            m_floating.enabled = false;
            m_state.IsFloating = false;
        }
        else // Actor is in a floating state
        {
            if (m_floating.enabled == false)
            {
                m_floating.ResetRoll();                     // Sets the 'up' vector to be whatever orientation the player is currently in.
            }

            m_state.FeetOnGround = false;
            m_grounded.enabled = false;
            m_floating.enabled = true;
            m_state.IsFloating = true;
        }

        m_animController.PlayAnimationForActorState(m_state);   // Selects and plays the correct animation for the current actor state.
        this.Debug_DrawVelocityVector();
    }

    /// <summary>
    /// Is called by unity every physics update/time-step.
    /// </summary>
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

    /// <summary>
    /// Resets all components referenced by this ActorController
    /// </summary>
    public override void Reset()
    {
        m_controller.Reset();
        m_state.Reset();
        m_floating.Reset();
        m_grounded.Reset();
        m_groundRay.Reset();
        m_rigidBody.ResetVelocity();
        m_animController.Reset();
        m_crosshair.Reset();
        m_health.Reset();
        m_pickup.Reset();

        m_rigidBody.ResetVelocity();
    }

    /// <summary>
    /// Draws the actors' current velocity vector on screen.
    /// </summary>
    private void Debug_DrawVelocityVector()
    {
        Debug.DrawRay(m_body.transform.position, m_rigidBody.velocity, Color.cyan);
    }

    /// <summary>
    /// Processes all of the concurrent controller actions made active by the controller and translates them into behaviours performed by the actor.
    /// </summary>
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
                    m_pickup.DropSelectedWeapon(false);
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

    /// <summary>
    /// Fires the actors' currently selected weapon
    /// </summary>
    private void FireSelectedWeapon()
    {
        var weaponObj = m_state.GetSelectedWeapon();

        if (weaponObj != null)
        {
            var weapon = weaponObj.GetComponent<Weapon>();
            weapon.TriggerState = TriggerState.Pulled;
        }
    }

    /// <summary>
    /// Causes the actor to perform a jump
    /// </summary>
    private void Jump()
    {
        if (m_grounded.enabled)
        {
            m_grounded.Jump();
        }
    }

    /// <summary>
    /// Causes the actor to thrust upward.
    /// </summary>
    private void ThrustUp()
    {
        if (m_floating.enabled)
        {
            m_floating.ThrustUp();
        }
    }

    /// <summary>
    /// Causes the actor to crouch / thrust downward.
    /// </summary>
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

    /// <summary>
    /// Toggles the actors' gravity boots on or off.
    /// </summary>
    private void ToggleGravBoots()
    {
        m_state.GravBootsEnabled = !m_state.GravBootsEnabled;
    }

    /// <summary>
    /// Updates the world position of the currently selected weapon
    /// </summary>
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

    /// <summary>
    /// Updates the actors' arm orientation such that it faces the crosshair position.
    /// </summary>
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

    /// <summary>
    /// Updates the actors' head rotation such that it faces the corsshair's aimpoint.
    /// </summary>
    private void UpdateActorHeadRotation()
    {
        var headTransform = m_head.GetComponent<Transform>();
        var neckTransform = m_neck.GetComponent<Transform>();

        var detlaVector = m_crosshair.AimPoint - headTransform.position;
        var rotation = Quaternion.LookRotation(detlaVector, headTransform.up);
        headTransform.rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z);
        headTransform.Rotate(0, -90, 0);
    }

    /// <summary>
    /// Handles gun recoil.
    /// </summary>
    /// <param name="projectileVelocity">velocity of the projectile fired.</param>
    /// <param name="projectileMass">Mass of the projectile fired</param>
    private void HandleRecoil(Vector3 projectileVelocity, float projectileMass)
    {
        if (m_state.IsFloating)
        {
            m_rigidBody.AddForce(-projectileVelocity * projectileMass, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Triggers whenever a collision occurs with the actor entity's sphere collider.
    /// </summary>
    /// <param name="collision">Collision details</param>
    private void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            var gameObj = contact.otherCollider.gameObject;

            if (gameObj == this.gameObject)
            {
                return;
            }

            if (gameObj.IsPickupItem())     // if the collision object is a flag pickup, pickup the flag.  TODO : Move this into Flag.cs
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
                        m_pickup.PickupPack(flag, false);
                    }
                }
                else
                {
                    m_pickup.PickupPack(gameObj.GetComponent<PickupItem>(), false);
                }
            }

            if (gameObj.IsProjectile()) //if gameobject is a projectile - handle in interaction,and register the hit.
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

                NotificationService.Instance.Info("Projectile hit " + m_state.PlayerName);

                projectileNet.HitNetworkObjectId = this.GetComponent<NetworkObject>().NetworkObjectId;

                projectile.Despawn(collision);
            }
        }
    }

    /// <summary>
    /// Gets the current movement speed of the actor.
    /// </summary>
    /// <returns></returns>
    public float GetCurrentMoveSpeed()
    {
        return m_rigidBody.velocity.magnitude;

        //return m_state.IsFloating ? m_floating.MoveForce : m_grounded.MoveSpeed;
    }

    /// <summary>
    /// Triggered when actor has dropped an item
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event Args : <see cref="OnPickupEventArgs"/></param>
    private void Pickup_OnItemDropped(object sender, OnPickupEventArgs e)
    {
        if (e.Item.gameObject.IsWeapon())
        {
            var weapon = e.Item.GetComponent<Weapon>();
            weapon.OnShotFired -= this.Weapon_OnShotFired;
        }
    }

    /// <summary>
    /// Triggered when actor has picked up an item
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event Args : <see cref="OnPickupEventArgs"/></param>
    private void Pickup_OnItemPickedUp(object sender, OnPickupEventArgs e)
    {
        if (e.Item.gameObject.IsWeapon())
        {
            var weapon = e.Item.GetComponent<Weapon>();
            weapon.OnShotFired += this.Weapon_OnShotFired;
        }
    }

    /// <summary>
    /// Triggered when actor has distcharged their weapon.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event arguments</param>
    private void Weapon_OnShotFired(object sender, OnShotFiredEventArgs e)
    {
        this.HandleRecoil(e.ProjectileVelocity, e.ProjectileMass);
    }
}
