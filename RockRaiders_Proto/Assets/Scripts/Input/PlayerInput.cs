using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts.Input
{
    public enum ActionState
    {
        InActive = 0,
        Active = 1
    }

    public enum ControllerActions
    {
        Trigger = 0,                  // Actions / Triggers whatever is equiped
        Jump,                       // Causes the player character to jump
        Crouch,                     // Causes the player to crouch
        ThrustUp,                   // Causes the player to thrust upward
        ThrustDown,                 // Causes the player to thrust downward
        RollLeft,
        RollRight,
        Use,                        // Causes the player character to use environmental object
        Drop,                       // Causes the player character to drop whatever is in their hand
        Throw,                      // Causes the player character to throw whatever is in their hand
        Melee,                      // Causes the player character to perform a melee attack with whatever is equipped
        EquipMain,                  // Equips the weapon in the 'Main Weapon' inventory slot
        EquipSide,                  // Equips the player characters sidearm from the inventory slot
        EquipPack,                  // Equips / Activates the player characters pack. Some packs are passive buffs... and this action will do nothing.
        GravBoots,                  // Toggles the player characters' grav boots
        Pause,                      // Pauses the game / Brings up game menu... (Wont pause in multiplayer)
        Count                       // Not in use
    }

    public struct NetPlayerInput : INetworkSerializable
    {
        public float MoveX;
        public float MoveY;
        public float LookX;
        public float LookY;
        public int Trigger;
        public int Jump;
        public int Crouch;
        public int Use;
        public int Drop;
        public int Throw;
        public int Melee;
        public int EquipMain;
        public int EquipSide;
        public int EquipPack;
        public int GravBoots;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref MoveX);
            serializer.SerializeValue(ref MoveY);
            serializer.SerializeValue(ref LookX);
            serializer.SerializeValue(ref LookY);
            serializer.SerializeValue(ref Trigger);
            serializer.SerializeValue(ref Jump);
            serializer.SerializeValue(ref Crouch);
            serializer.SerializeValue(ref Use);
            serializer.SerializeValue(ref Drop);
            serializer.SerializeValue(ref Throw);
            serializer.SerializeValue(ref Melee);
            serializer.SerializeValue(ref EquipMain);
            serializer.SerializeValue(ref EquipSide);
            serializer.SerializeValue(ref EquipPack);
            serializer.SerializeValue(ref GravBoots);
        }
    }

    public class PlayerInput : MonoBehaviour, IPlayerInput
    {
        private Vector2 moveAxis;
        private Vector2 lookAxis;

        private Dictionary<ControllerActions, ActionState> m_actionDict;

        public int Trigger
        {
            get
            {
                return (int)m_actionDict[ControllerActions.Trigger];
            }
            set
            {
                m_actionDict[ControllerActions.Trigger] = (ActionState)value;
            }
        }

        public int Jump
        {
            get
            {
                return (int)m_actionDict[ControllerActions.Jump];
            }
            set
            {
                m_actionDict[ControllerActions.Jump] = (ActionState)value;
            }
        }

        public int Crouch
        {
            get
            {
                return (int)m_actionDict[ControllerActions.Crouch];
            }
            set
            {
                m_actionDict[ControllerActions.Crouch] = (ActionState)value;
            }
        }

        public int Use
        {
            get
            {
                return (int)m_actionDict[ControllerActions.Use];
            }
            set
            {
                m_actionDict[ControllerActions.Use] = (ActionState)value;
            }
        }

        public int Drop
        {
            get
            {
                return (int)m_actionDict[ControllerActions.Drop];
            }
            set
            {
                m_actionDict[ControllerActions.Drop] = (ActionState)value;
            }
        }

        public int Throw
        {
            get
            {
                return (int)m_actionDict[ControllerActions.Throw];
            }
            set
            {
                m_actionDict[ControllerActions.Throw] = (ActionState)value;
            }
        }

        public int Melee
        {
            get
            {
                return (int)m_actionDict[ControllerActions.Melee];
            }
            set
            {
                m_actionDict[ControllerActions.Melee] = (ActionState)value;
            }
        }

        public int EquipMain
        {
            get
            {
                return (int)m_actionDict[ControllerActions.EquipMain];
            }
            set
            {
                m_actionDict[ControllerActions.EquipMain] = (ActionState)value;
            }
        }

        public int EquipSide
        {
            get
            {
                return (int)m_actionDict[ControllerActions.EquipSide];
            }
            set
            {
                m_actionDict[ControllerActions.EquipSide] = (ActionState)value;
            }
        }

        public int EquipPack
        {
            get
            {
                return (int)m_actionDict[ControllerActions.EquipPack];
            }
            set
            {
                m_actionDict[ControllerActions.EquipPack] = (ActionState)value;
            }
        }

        public int GravBoots
        {
            get
            {
                return (int)m_actionDict[ControllerActions.GravBoots];
            }
            set
            {
                m_actionDict[ControllerActions.GravBoots] = (ActionState)value;
            }
        }

        public int RollLeft
        {
            get
            {
                return (int)m_actionDict[ControllerActions.RollLeft];
            }
            set
            {
                m_actionDict[ControllerActions.RollLeft] = (ActionState)value;
            }
        }

        public int RollRight
        {
            get
            {
                return (int)m_actionDict[ControllerActions.RollRight];
            }
            set
            {
                m_actionDict[ControllerActions.RollRight] = (ActionState)value;
            }
        }


        public Vector2 LookAxis
        {
            get
            {
                return lookAxis;
            }
            set
            {
                lookAxis = value;
            }
        }

        public Vector2 MoveAxis
        {
            get
            {
                return moveAxis;
            }
            set
            {
                moveAxis = value;
            }
        }


        public PlayerInput()
        {
            m_actionDict = new Dictionary<ControllerActions, ActionState>();

            moveAxis = Vector2.zero;
            lookAxis = Vector2.zero;

            for (int i = 0; i < (int)ControllerActions.Count; i++)
            {
                m_actionDict.Add((ControllerActions)i, ActionState.InActive);
            }
        }

        public void Start()
        {
        }

        public void Reset()
        {
            var values = Enum.GetValues(typeof(ControllerActions));

            foreach(var value in values)
            {
                m_actionDict[(ControllerActions)value] = ActionState.InActive;
            }

            moveAxis.x = 0;
            moveAxis.y = 0;
            lookAxis.x = 0;
            lookAxis.y = 0;
        }

        public ActionState GetActionState(ControllerActions action)
        {
            Assert.IsTrue(m_actionDict.ContainsKey(action), $"{action} does not exist within the action dictionary!");
            return m_actionDict[action];
        }

        public void SetActionState(ControllerActions action, ActionState state = ActionState.Active)
        {
            Assert.IsTrue(m_actionDict.ContainsKey(action), $"{action} does not exist within the action dictionary!");
            m_actionDict[action] = state;
        }

        public List<ControllerActions> GetActiveControllerActions()
        {
            var result = new List<ControllerActions>();

            foreach (var kvp in m_actionDict)
            {
                if (kvp.Value == ActionState.Active)
                {
                    result.Add(kvp.Key);
                }
            }

            return result;
        }

        public NetPlayerInput GetNetPlayerInput()
        {
            return new NetPlayerInput
            {
                LookX = lookAxis.x,
                LookY = lookAxis.y,
                MoveX = moveAxis.x,
                MoveY = moveAxis.y,
                Drop = Drop,
                EquipMain = EquipMain,
                EquipPack = EquipPack,
                EquipSide = EquipSide,
                GravBoots = GravBoots,
                Melee = Melee,
                Use = Use,
                Trigger = Trigger,
                Jump = Jump,
                Crouch = Crouch,
                Throw = Throw
            };
        }

        public void SetStateFromNetPlayerInput(NetPlayerInput netController)
        {
            moveAxis.x = netController.MoveX;
            moveAxis.y = netController.MoveY;
            lookAxis.x = netController.LookX;
            lookAxis.y = netController.LookY;
            Drop = netController.Drop;
            EquipMain = netController.EquipMain;
            EquipPack = netController.EquipPack;
            EquipSide = netController.EquipSide;
            GravBoots = netController.GravBoots;
            Melee = netController.Melee;
            Use = netController.Use;
            Trigger = netController.Trigger;
            Jump = netController.Jump;
            Throw = netController.Throw;
        }
    }
}