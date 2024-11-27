using System;
using UnityEngine;

namespace Assets.Scripts.Actor
{
    /// <summary>
    /// Animation controller for actor entity.
    /// This behaviour is responsible for selecting the correct animation for any given actor state.
    /// </summary>
    public class ActorAnimController : RRMonoBehaviour
    {
        private bool m_deathAnimPlayed = false; 

        //String constants...
        private const string ANIM_IDLE = "Idle";
        private const string ANIM_CROUCH_IDLE = "Actor_Crouch_Idle";

        private const string ANIM_SIDEARM = "Actor_SideArm";
        private const string ANIM_MAINWEP = "Actor_MainWep";
        private const string ANIM_CROUCH_MAINWEP = "Crouch_MainWep";
        private const string ANIM_CROUCH_SIDEARM = "Crouch_SideArm";

        private const string ANIM_RUN = "Run";
        private const string ANIM_BACK = "Backpedal";
        private const string ANIM_SIDEARM_RUN = "SideArm_Run";
        private const string ANIM_SIDEARM_BACK = "SideArm_Back";
        private const string ANIM_MAINWEP_RUN = "MainWep_Run";
        private const string ANIM_MAINWEP_BACK = "MainWep_Back";

        private const string ANIM_CROUCH_RUN = "Actor_Crouch_Run";
        private const string ANIM_CROUCH_BACK = "Actor_Crouch_Back";
        private const string ANIM_CROUCH_SIDEARM_RUN = "Crouch_SideArm_Run";
        private const string ANIM_CROUCH_SIDEARM_BACK = "Crouch_SideArm_Back";
        private const string ANIM_CROUCH_MAINWEP_RUN = "Crouch_MainWep_Run";
        private const string ANIM_CROUCH_MAINWEP_BACK = "Crouch_MainWep_Back";

        private const string ANIM_FLOAT_SIDEARM = "Float_SideArm";
        private const string ANIM_FLOAT_MAINWEP = "Float_MainWep";
        private const string ANIM_FLOAT_EMPTY = "Float_Empty";

        private const string ANIM_DEAD = "Dead";
        private const string ANIM_FLOAT_DEAD = "Float_Dead";

        [SerializeField]
        private Animator m_animator;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActorAnimController()
        {
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        public override void Initialise()
        {
            m_animator = this.GetComponent<Animator>();
        }


        public override void Reset()
        {

        }

        /// <summary>
        /// Gets the animation best fitting the current actor state and plays it.
        /// </summary>
        /// <param name="state">Actor state component<see cref="ActorState"/></param>
        public void PlayAnimationForActorState(ActorState state)
        {

            if (state.IsDead)
            {
                if (state.IsFloating)
                {
                    m_animator.Play(ANIM_FLOAT_DEAD);
                    m_deathAnimPlayed = true;
                }
                else
                {
                    if (!m_deathAnimPlayed)
                    {
                        m_animator.Play(ANIM_DEAD);
                        m_deathAnimPlayed = true;
                    }
                }
            }
            else
            {
                m_deathAnimPlayed = false;
                m_animator.Play(this.GetAnimationForActorState(state));
            }
        }

        /// <summary>
        /// Gets the animation name for the current actor state.
        /// </summary>
        /// <param name="state">Actor state component <see cref="ActorState"/></param>
        /// <returns>Animation name</returns>
        private string GetAnimationForActorState(ActorState state)
        {
            if (state.IsMoving)
            {
                if (state.IsFloating || state.FeetOnGround == false)
                {
                    return this.GetFloatAnimation(state);
                }
                else
                {
                    return this.GetRunAnimationForActorState(state);
                }
            }
            else
            {
                return this.GetIdleAnimationForActorState(state);
            }
        }

        /// <summary>
        /// Gets the crouch animation name for specified actor state.
        /// </summary>
        /// <param name="state">Actor state component <see cref="ActorState"/></param>
        /// <returns>Animation name</returns>
        /// <exception cref="InvalidOperationException">Animation not found</exception>
        private string GetIdleCrouchAnimationForActorState(ActorState state)
        {
            switch (state.SelectedWeapon)
            {
                case SelectedWeapon.None:
                    return ANIM_CROUCH_IDLE;
                case SelectedWeapon.Main:
                    return ANIM_CROUCH_MAINWEP;
                case SelectedWeapon.Sidearm:
                    return ANIM_CROUCH_SIDEARM;
                case SelectedWeapon.Pack:
                    return ANIM_CROUCH_SIDEARM;
            }

            throw new InvalidOperationException($"Invalid {nameof(SelectedWeapon)}");
        }

        /// <summary>
        /// Gets the run animation name for specified actor state.
        /// </summary>
        /// <param name="state">Actor state component <see cref="ActorState"/></param>
        /// <returns>Animation name</returns>
        private string GetRunAnimationForActorState(ActorState state)
        {
            if (state.IsCrouched)
            {
                return this.GetCrouchRunAnimation(state);
            }
            else
            {
                return this.GetRunAnimation(state);
            }
        }

        /// <summary>
        /// Gets the idle animation name for specified actor state.
        /// </summary>
        /// <param name="state">Actor state component <see cref="ActorState"/></param>
        /// <returns>Animation name</returns>
        private string GetIdleAnimationForActorState(ActorState state)
        {
            if (state.IsCrouched)
            {
                return this.GetIdleCrouchAnimationForActorState(state);
            }

            switch (state.SelectedWeapon)
            {
                case SelectedWeapon.None:
                    return this.GetIdleEmptyAnimation(state);
                case SelectedWeapon.Main:
                    return this.GetIdleMainWeaponAnimation(state);
                case SelectedWeapon.Sidearm:
                    return this.GetIdleSideArmAnimation(state);
                case SelectedWeapon.Pack:
                    break;
            }

            throw new InvalidOperationException($"Invalid {nameof(SelectedWeapon)}");

        }

        /// <summary>
        /// Gets the run animation name for specified actor state.
        /// </summary>
        /// <param name="state">Actor state component <see cref="ActorState"/></param>
        /// <returns>Animation name</returns>
        private string GetRunAnimation(ActorState state)
        {
            switch (state.SelectedWeapon)
            {
                case SelectedWeapon.None:
                    return state.IsMovingForward ? ANIM_RUN : ANIM_BACK;
                case SelectedWeapon.Main:
                    return state.IsMovingForward ? ANIM_MAINWEP_RUN : ANIM_MAINWEP_BACK;
                case SelectedWeapon.Sidearm:
                    return state.IsMovingForward ? ANIM_SIDEARM_RUN : ANIM_SIDEARM_BACK;
                case SelectedWeapon.Pack:
                    return state.IsMovingForward ? ANIM_SIDEARM_RUN : ANIM_SIDEARM_BACK;
                default:
                    throw new InvalidOperationException($"Invalid {nameof(SelectedWeapon)}");
            }
        }

        /// <summary>
        /// Gets the crouch animation name for specified actor state.
        /// </summary>
        /// <param name="state">Actor state component <see cref="ActorState"/></param>
        /// <returns>Animation name</returns>
        private string GetCrouchRunAnimation(ActorState state)
        {
            switch (state.SelectedWeapon)
            {
                case SelectedWeapon.None:
                    return state.IsMovingForward ? ANIM_CROUCH_RUN : ANIM_CROUCH_BACK;
                case SelectedWeapon.Main:
                    return state.IsMovingForward ? ANIM_CROUCH_MAINWEP_RUN : ANIM_CROUCH_MAINWEP_BACK;
                case SelectedWeapon.Sidearm:
                    return state.IsMovingForward ? ANIM_CROUCH_SIDEARM_RUN : ANIM_CROUCH_SIDEARM_BACK;
                case SelectedWeapon.Pack:
                    return state.IsMovingForward ? ANIM_CROUCH_SIDEARM_RUN : ANIM_CROUCH_SIDEARM_BACK;
                default:
                    throw new InvalidOperationException($"Invalid {nameof(SelectedWeapon)}");
            }
        }

        /// <summary>
        /// Gets the floating animation name for specified actor state.
        /// </summary>
        /// <param name="state">Actor state component <see cref="ActorState"/></param>
        /// <returns>Animation name</returns>
        private string GetFloatAnimation(ActorState state)
        {
            switch (state.SelectedWeapon)
            {
                case SelectedWeapon.None:
                    return ANIM_FLOAT_EMPTY;
                case SelectedWeapon.Main:
                    return ANIM_FLOAT_SIDEARM;
                case SelectedWeapon.Sidearm:
                    return ANIM_FLOAT_MAINWEP;
                case SelectedWeapon.Pack:
                    return ANIM_FLOAT_SIDEARM;
                default:
                    throw new InvalidOperationException($"Invalid {nameof(SelectedWeapon)}");
            }
        }

        /// <summary>
        /// Gets the idle empty animation name for specified actor state.
        /// </summary>
        /// <param name="state">Actor state component <see cref="ActorState"/></param>
        /// <returns>Animation name</returns>
        private string GetIdleEmptyAnimation(ActorState state)
        {
            if (state.IsFloating)
            {
                return ANIM_FLOAT_EMPTY;
            }
            else
            {
                return state.IsCrouched ? ANIM_CROUCH_IDLE : ANIM_IDLE;
            }
        }

        /// <summary>
        /// Gets the idle main weapon animation for specified actor state.
        /// </summary>
        /// <param name="state">Actor state component <see cref="ActorState"/></param>
        /// <returns>Animation name</returns>
        private string GetIdleMainWeaponAnimation(ActorState state)
        {
            if (state.IsFloating)
            {
                return ANIM_FLOAT_MAINWEP;
            }
            else
            {
                return state.IsCrouched ? ANIM_CROUCH_MAINWEP : ANIM_MAINWEP;
            }
        }

        /// <summary>
        /// Gets the idle side arm animation name for specified actor state.
        /// </summary>
        /// <param name="state">Actor state component <see cref="ActorState"/></param>
        /// <returns>Animation name</returns>
        private string GetIdleSideArmAnimation(ActorState state)
        {
            if (state.IsFloating)
            {
                return ANIM_FLOAT_SIDEARM;
            }
            else
            {
                return state.IsCrouched ? ANIM_CROUCH_SIDEARM : ANIM_SIDEARM;
            }
        }
    }
}