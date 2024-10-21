using System;
using UnityEngine;

namespace Assets.Scripts.Actor
{
    public class ActorAnimController : RRMonoBehaviour
    {
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

        [SerializeField]
        private Animator m_animator;

        public ActorAnimController()
        {
        }

        public override void Initialise()
        {
        }


        public override void Reset()
        {

        }

        public void PlayAnimationForActorState(ActorState state)
        {
            m_animator.Play(this.GetAnimationForActorState(state));
        }

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