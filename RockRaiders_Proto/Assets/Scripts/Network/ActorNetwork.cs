using Assets.Scripts.Actor;
using Assets.Scripts.Input;
using Assets.Scripts.Managers;
using Assets.Scripts.Network;
using Assets.Scripts.Services;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;


namespace Assets.Scripts.Network
{
    public struct ActorStateData : INetworkSerializable, IEquatable<ActorStateData>
    {
        public bool IsReady;
        public NetActorInventory Inventory;
        public int SelectedWeapon;
        public int Hitpoints;
        public Team Team;
        public Color Colour;
        public bool IsDying;
        public bool IsDead;

        public bool Equals(ActorStateData other)
        {
            var invEq = this.Inventory.Equals(other.Inventory);
            var wpnEq = this.SelectedWeapon.Equals(other.SelectedWeapon);
            var hpEq = this.Hitpoints.Equals(other.Hitpoints);
            var teamEq = this.Team.Equals(other.Team);
            var tintEq = this.Colour.Equals(other.Colour);
            var dyingEq = this.IsDying == other.IsDying;
            var isDeadEq = this.IsDead == other.IsDead;

            return invEq && wpnEq && hpEq && teamEq && tintEq && dyingEq && isDeadEq;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.IsReady);
            serializer.SerializeValue(ref this.Inventory);
            serializer.SerializeValue(ref this.SelectedWeapon);
            serializer.SerializeValue(ref this.Hitpoints);
            serializer.SerializeValue(ref this.Team);
            serializer.SerializeValue(ref this.Colour);
            serializer.SerializeValue(ref this.IsDying);
            serializer.SerializeValue(ref this.IsDead);
        }
    }

    public struct ActorControlData : INetworkSerializable
    {
        public Vector3 CrosshairPosition;
        public NetPlayerInput Controller;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.Controller);
            serializer.SerializeValue(ref this.CrosshairPosition);
        }
    }

    public struct ActorTransformData : INetworkSerializable
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.Position);
            serializer.SerializeValue(ref this.Rotation);
        }
    }

    public struct PlayerNetData : INetworkSerializable
    {
        public FixedString128Bytes PlayerName;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.PlayerName);
        }
    }

    internal struct SetParentRpcData : INetworkSerializable
    {
        public ulong ParentNetworkId;
        public ulong ChildNetworkId;

        public bool HasParentId => ParentNetworkId != ulong.MaxValue;
        public bool HasChildId => ChildNetworkId != ulong.MaxValue;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.ParentNetworkId);
            serializer.SerializeValue(ref this.ChildNetworkId);
        }
    }

    public class ActorNetwork : NetworkBehaviour
    {
        private NetworkVariable<PlayerNetData> m_playerData;
        private NetworkVariable<ActorStateData> m_actorState;
        private NetworkVariable<ActorControlData> m_actorControl;
        private NetworkVariable<ActorTransformData> m_actorTransform;

        private PlayerInput m_controller;
        private ActorController m_actorController;
        private ActorGrounded m_actorGrounded;
        private ActorHealth m_health;
        private ActorSpawnManager m_actorManager;
        private ActorState m_state;
        private ActorPainter m_paint;
        private ActorCrosshair m_crosshair;

        private Timer m_updateHealthTimer;

        [SerializeField]
        private bool m_serverAuth;

        public ActorSpawnManager ActorSpawnManager
        {
            get
            {
                return m_actorManager;
            }
            set
            {
                m_actorManager = value;
            }
        }

        public ActorNetwork()
        {
            m_updateHealthTimer = new Timer(TimeSpan.FromSeconds(0.125f));
            m_updateHealthTimer.AutoReset = false;
        }

        private void Awake()
        {
            var permission = NetworkVariableWritePermission.Server;//this.IsServer ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
            m_actorState = new NetworkVariable<ActorStateData>(writePerm: permission);
            m_actorControl = new NetworkVariable<ActorControlData>(writePerm: permission);
            m_actorTransform = new NetworkVariable<ActorTransformData>(writePerm: permission);

            m_playerData = new NetworkVariable<PlayerNetData>(writePerm: NetworkVariableWritePermission.Owner);
        }

        private void Start()
        {
            m_state = this.GetComponent<ActorState>();
            m_paint = this.GetComponent<ActorPainter>();
            m_crosshair = this.GetComponent<ActorCrosshair>();
            m_actorController = this.GetComponent<ActorController>();
            m_health = this.GetComponent<ActorHealth>();
            m_actorGrounded = this.GetComponent<ActorGrounded>();

            if (m_controller == null)
            {
                m_controller = this.GetComponent<PlayerInput>();
            }

            m_updateHealthTimer.Start();

            this.PrepareActor();
        }

        private void Update()
        {
            m_updateHealthTimer.Tick();

            if (this.IsOwner)
            {
                this.SendState();
            }
            else
            {
                this.UpdateActor();
            }
        }

        private void UpdateActorTransform()
        {
            var state = m_actorTransform.Value;

            this.transform.position = state.Position;
            this.transform.rotation = state.Rotation;
        }

        private void UpdateActorControl()
        {
            var state = m_actorControl.Value;
            m_crosshair.UpdateAimpointFromCamera = false;
            m_crosshair.AimPoint = state.CrosshairPosition;
            state.Controller.LookX = 0;                     //TODO : This is a hack. Send ActorCamera position instead....
            state.Controller.LookY = 0;
            m_controller.SetStateFromNetPlayerInput(state.Controller);
        }

        private void UpdateActorState()
        {
            var state = m_actorState.Value;

            m_state.Inventory.SetInventoryFromActorInventoryState(state.Inventory.ToActorInventoryState());
            m_state.SelectWeapon((SelectedWeapon)state.SelectedWeapon);

            var color = m_paint.GetColour();

            if (color != state.Colour)
            {
                NotificationService.Instance.Info($"Updating player color : {state.Colour}");
                m_paint.Paint(state.Colour);
            }

            var team = m_state.Team;

            if (team != state.Team)
            {
                NotificationService.Instance.Info($"Updating player team : {state.Team}");
                m_state.Team = state.Team;
            }
        }

        private void UpdateActorHealth()
        {
            var state = m_actorState.Value;
            m_health.Hitpoints.SetHitPoints(state.Hitpoints);
        }

        private void UpdateActor()
        {
            var playerData = m_playerData.Value;

            if (playerData.PlayerName != string.Empty)
            {
                m_state.PlayerName = playerData.PlayerName.ToString();
            }

            if (m_actorState.Value.IsReady)
            {
                this.UpdateActorTransform();
                
                this.UpdateActorControl();

                if (m_updateHealthTimer.Elapsed)
                {
                    m_updateHealthTimer.ResetTimer();
                    this.UpdateActorHealth();
                    this.UpdateActorState();
                }
            }
        }

        public void Reset()
        {
            if (this.IsOwner)
            {
                var permission = NetworkVariableWritePermission.Server;//this.IsServer ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
                m_actorState = new NetworkVariable<ActorStateData>(writePerm: permission);
                m_actorControl = new NetworkVariable<ActorControlData>(writePerm: permission);
                m_actorTransform = new NetworkVariable<ActorTransformData>(writePerm: permission);

                m_playerData = new NetworkVariable<PlayerNetData>(writePerm: NetworkVariableWritePermission.Owner);
            }
        }

        private ActorStateData GetActorState()
        {
            return new ActorStateData
            {
                IsReady = true,
                SelectedWeapon = (int)this.GetComponent<ActorState>().SelectedWeapon,
                Hitpoints = m_health.Hitpoints.Current,
                Inventory = m_state.Inventory.GetNetActorInventory(),
                Colour = m_paint.GetColour(),
                Team = m_state.Team,
                IsDead = m_state.IsDead,
                IsDying = m_state.IsDying
            };
        }

        private ActorTransformData GetActorTransform()
        {
            return new ActorTransformData
            {
                Position = this.transform.position,
                Rotation = this.transform.rotation,
            };
        }

        private ActorControlData GetActorControl()
        {
            return new ActorControlData
            {
                Controller = m_controller.GetNetPlayerInput(),
                CrosshairPosition = m_crosshair.AimPoint,
            };
        }

        public void SetState(ActorStateData state)
        {
            m_actorState.Value = state;
        }

        private void SendActorTransform()
        {
            var state = this.GetActorTransform();

            if (this.IsServer)
            {
                m_actorTransform.Value = state;
            }
            else
            {
                this.TransmitActorTransformServerRpc(this.OwnerClientId, m_actorTransform.Value);
            }
        }

        private void SendActorControl()
        {
            var state = this.GetActorControl();

            if (this.IsServer)
            {
                m_actorControl.Value = state;
            }
            else
            {
                this.TransmitActorControlServerRpc(this.OwnerClientId, m_actorControl.Value);
            }
        }

        private void SendActorState()
        {
            var state = this.GetActorState();

            if (this.IsServer)
            {
                m_actorState.Value = state;
            }
            else
            {
                this.TransmitActorStateServerRpc(this.OwnerClientId, m_actorState.Value);
            }
        }


        private void SendState()
        {
            var playerNetData = new PlayerNetData { PlayerName = GameManager.Instance.Settings.Game.PlayerName ?? string.Empty };

            var state = this.GetActorState();
            var transform = this.GetActorTransform();
            var control = this.GetActorControl();

            if (this.IsServer) //|| !m_serverAuth)
            {
                m_actorState.Value = state;
                m_actorTransform.Value = transform;
                m_actorControl.Value = control;
                m_playerData.Value = playerNetData;
                m_state.PlayerName = playerNetData.PlayerName.ToString();
                m_actorState.SetDirty(!m_actorControl.Value.Equals(state));
            }
            else
            {
                this.TransmitActorStateServerRpc(this.OwnerClientId, state);
                this.TransmitActorControlServerRpc(this.OwnerClientId, control);
                this.TransmitActorTransformServerRpc(this.OwnerClientId, transform);
                this.TransmitPlayerNetDataServerRpc(playerNetData);
            }
        }

        public void SetParent(GameObject child, GameObject parent)
        {
            if (!this.IsOwner)
            {
                return;
            }

            if (this.IsServer)
            {
                child.transform.parent = parent?.transform;
            }
            else
            {
                var netObj = parent?.GetComponent<NetworkObject>();
                var childNetObj = child?.GetComponent<NetworkObject>();

                var data = new SetParentRpcData
                {
                    ParentNetworkId = netObj?.NetworkObjectId ?? uint.MaxValue,
                    ChildNetworkId = childNetObj?.NetworkObjectId ?? uint.MaxValue
                };

                this.SetParentServerRpc(data);
            }
        }



        [Rpc(SendTo.Server)]
        private void SetParentServerRpc(SetParentRpcData data)
        {
            if (!data.HasChildId)
            {
                return;
            }

            var childNetObj = this.GetNetworkObject(data.ChildNetworkId);

            if (!data.HasParentId)
            {
                childNetObj.gameObject.transform.parent = null;
                return;
            }

            var parentNetObj = this.GetNetworkObject(data.ParentNetworkId);
            childNetObj.transform.parent = parentNetObj.gameObject.transform;
        }

        [Rpc(SendTo.Server)]
        private void TransmitPlayerNetDataServerRpc(PlayerNetData playerNetData)
        {
            //NotificationService.Instance.Info("Player Name : " + playerName);
            this.SendPlayerNetDataToClientRpc(playerNetData);
        }

        [Rpc(SendTo.Server)]
        private void TransmitActorTransformServerRpc(ulong ownerClientId, ActorTransformData clientState)
        {
            var serverState = this.GetActorTransform();
            var rb = this.GetComponent<Rigidbody>();

            var tolerance = m_actorController.GetCurrentMoveSpeed();
            var fDiff = (clientState.Position - serverState.Position).magnitude;

            if (fDiff <= rb.velocity.magnitude + tolerance)
            {
                serverState.Position = clientState.Position;
            }
            else
            {
                NotificationService.Instance.Info($"Change in position ({fDiff}) > current velocity ({rb.velocity.magnitude}). Correcting.");
                clientState.Position = serverState.Position;
                this.SendCorrectedActorTransformToClientRpc(serverState);
            }

            serverState.Rotation = clientState.Rotation;
            m_actorTransform.Value = serverState;
        }

        [Rpc(SendTo.Server)]
        private void TransmitActorControlServerRpc(ulong ownerClientId, ActorControlData clientState)
        {
            var serverState = this.GetActorControl();

            serverState.CrosshairPosition = clientState.CrosshairPosition;
            serverState.Controller = clientState.Controller;

            m_actorControl.Value = serverState;
        }


        [Rpc(SendTo.Server)]
        private void TransmitActorStateServerRpc(ulong clientId, ActorStateData clientState)
        {
            var serverState = this.GetActorState();

            bool inValid = false;

            if (serverState.SelectedWeapon != clientState.SelectedWeapon)
            {
                serverState.SelectedWeapon = clientState.SelectedWeapon;
                //inValid = true;
            }

            if (!serverState.Inventory.Equals(clientState.Inventory))
            {
                NotificationService.Instance.Info($"Inventory mismatch : Sending corrected inventory state to client");
                clientState.Inventory = serverState.Inventory;

                inValid = true;
            }

            if (clientState.Hitpoints != serverState.Hitpoints)
            {
                NotificationService.Instance.Info($"State HP : {serverState.Hitpoints}->{clientState.Hitpoints}");
                clientState.Hitpoints = serverState.Hitpoints;
                inValid = true;
            }

            if (clientState.IsDead != serverState.IsDead)
            {
                NotificationService.Instance.Info($"IsDead : {clientState.IsDead}->{serverState.IsDead} ");
                clientState.IsDead = serverState.IsDead;
                clientState.Hitpoints = 0;
                inValid = true;
            }

            if (clientState.IsDead != serverState.IsDying)
            {
                NotificationService.Instance.Info($"IsDying : {clientState.IsDead}->{serverState.IsDead} ");
                clientState.IsDying = serverState.IsDying;
                clientState.Hitpoints = 0;
                inValid = true;
            }

            if (clientState.Colour != serverState.Colour)
            {
                NotificationService.Instance.Info($"Colour mismatch : Sending corrected colour to client");
                inValid = true;
            }

            if (clientState.Team != serverState.Team)
            {
                NotificationService.Instance.Info($"Team mismatch : Sending corrected team to client");
                clientState.Team = serverState.Team;
                inValid = true;
            }

            if (inValid)
            {
                NotificationService.Instance.Info($"Invalid actor state : Sending correct state to client {clientId}");
                this.SendCorrectedActorStateToClientRpc(serverState);
            }


            m_actorState.Value = serverState;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SendPlayerNetDataToClientRpc(PlayerNetData playerNetData)
        {
            if (m_state != null)
            {
                m_state.PlayerName = playerNetData.PlayerName.ToString();
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SendCorrectedActorTransformToClientRpc(ActorTransformData actorState)
        {
            this.transform.position = actorState.Position;
            this.transform.rotation = actorState.Rotation;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SendCorrectedActorStateToClientRpc(ActorStateData actorState)
        {
            if (m_health != null)
            {
                m_health.Hitpoints.SetHitPoints(actorState.Hitpoints);
            }

            if (m_state != null)
            {
                var serverInvState = actorState.Inventory.ToActorInventoryState();
                m_state.Inventory.SetInventoryFromActorInventoryState(serverInvState);
                m_state.SelectWeapon((SelectedWeapon)actorState.SelectedWeapon);
                m_state.Team = actorState.Team;
                m_state.IsDying = actorState.IsDying;
                m_state.IsDead = actorState.IsDead;
            }

            if (m_paint != null)
            {
                m_paint.Paint(actorState.Colour);
            }
        }

        public override void OnNetworkDespawn()
        {
            var childComponents = this.GetComponents<RRMonoBehaviour>();

            foreach (var behaviour in childComponents)
            {
                behaviour.Reset();
            }

            base.OnNetworkDespawn();
        }

        private void PrepareActor()
        {
            if (m_actorManager == null)
            {
                m_actorManager = ActorSpawnManager.Instance;
            }

            m_controller = this.GetComponent<PlayerInput>();

            if (this.IsOwner)
            {
                NotificationService.Instance.Info("Preparing local player actor");
                m_actorManager.PrepareLocalPlayerActor(this.gameObject);
            }
            else
            {
                NotificationService.Instance.Info("Preparing remote player actor");
                m_actorManager.PrepareRemotePlayerActor(this.gameObject);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }
    }

}