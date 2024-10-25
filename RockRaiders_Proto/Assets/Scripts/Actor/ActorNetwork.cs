using Assets.Scripts;
using Assets.Scripts.Actor;
using Assets.Scripts.Factory;
using Assets.Scripts.Input;
using Assets.Scripts.Services;
using Unity.Netcode;
using UnityEngine;

internal struct ActorNetData : INetworkSerializable
{
    public bool IsReady;
    public Vector3 Position;
    public Quaternion Rotation;
    public NetPlayerInput Controller;
    public NetActorInventory Inventory;
    public Vector3 CrosshairPosition;
    public int SelectedWeapon;
    public int Hitpoints;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) 
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref this.IsReady);
        serializer.SerializeValue(ref this.Position);
        serializer.SerializeValue(ref this.Rotation);
        serializer.SerializeValue(ref this.Controller);
        serializer.SerializeValue(ref this.Inventory);
        serializer.SerializeValue(ref this.CrosshairPosition);
        serializer.SerializeValue(ref this.SelectedWeapon);
        serializer.SerializeValue(ref this.Hitpoints);
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
    private NetworkVariable<ActorNetData> m_playerState;

    private string m_playerName;
    private PlayerInput m_controller;
    private ActorController m_actorController;
    private ActorHealth m_health;
    private ActorSpawnManager m_actorManager;
    private ActorCrosshair m_crosshair;

    [SerializeField]
    private bool m_serverAuth;
    private ActorNetData m_lastState;

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

    public string PlayerName
    {
        get
        {
            return m_playerName;
        }
        set
        {
            m_playerName = value;

            if (!this.IsServer)
            {
                this.UpdatePlayerNameServerRpc(value);
            }
        }
    }

    public ActorNetwork()
    {

    }

    private void Awake()
    {
        var permission = m_serverAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        m_playerState = new NetworkVariable<ActorNetData>(writePerm: permission);
    }

    private void Start()
    {
        m_crosshair = this.GetComponent<ActorCrosshair>();
        m_actorController = this.GetComponent<ActorController>();
        m_health = this.GetComponent<ActorHealth>();

        if (m_controller == null)
        {
            m_controller = this.GetComponent<PlayerInput>();
        }
    }

    private void Update()
    {
        if (this.IsOwner)
        {
            this.SendState();
        }
        else
        {
            var state = m_playerState.Value;

            if (state.IsReady)
            {
                this.transform.position = state.Position;
                this.transform.rotation = state.Rotation;


                m_crosshair.UpdateAimpointFromCamera = false;
                m_crosshair.AimPoint = state.CrosshairPosition;

                m_health.Hitpoints.SetHitPoints(state.Hitpoints);

                state.Controller.LookX = 0;                     //TODO : This is a hack. Send ActorCamera position instead....
                state.Controller.LookY = 0;
                m_controller.SetStateFromNetPlayerInput(state.Controller);
                m_actorController.State.SelectWeapon((SelectedWeapon)state.SelectedWeapon);
                m_actorController.State.PlayerName = m_playerName;
            }
        }
    }

    private ActorNetData GetState()
    {
        return new ActorNetData
        {
            IsReady = true,
            Position = this.transform.position,
            Rotation = this.transform.rotation,
            Controller = m_controller.GetNetPlayerInput(),
            CrosshairPosition = m_crosshair.AimPoint,
            SelectedWeapon = (int)this.GetComponent<ActorState>().SelectedWeapon,
            Hitpoints = m_health.Hitpoints.Current,
        };
    }

    private void SendState()
    {
        var state = this.GetState();
        m_lastState = state;

        if (this.IsServer || !m_serverAuth)
        {
            m_playerState.Value = state;
        }
        else
        {
            this.TransmitServerRpc(state);
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

    [ServerRpc]
    private void UpdatePlayerNameServerRpc(string name)
    {
        m_playerName = name;
    }

    [ServerRpc]
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

    [ServerRpc]
    private void TransmitServerRpc(ActorNetData clientState)
    {
        NotificationService.Instance.Info("");

        var serverState = this.GetState();

        var rb = this.GetComponent<Rigidbody>();

        var allowance = 1.0f;
        var fDiff = (clientState.Position - serverState.Position).magnitude;

        if (fDiff <= rb.velocity.magnitude + allowance)
        {
            serverState.Position = clientState.Position;
        }
        else
        {
            
            NotificationService.Instance.Info($"Change in position ({fDiff}) > current velocity ({rb.velocity.magnitude}). Correcting.");
            clientState.Position = serverState.Position;
        }

        serverState.Rotation = clientState.Rotation;
        serverState.CrosshairPosition = clientState.CrosshairPosition;
        serverState.Controller = clientState.Controller;
        serverState.SelectedWeapon = clientState.SelectedWeapon;

        NotificationService.Instance.Info($"State HP : {m_lastState.Hitpoints}->{serverState.Hitpoints}");

        m_playerState.Value = serverState;

        this.TransmitCorrectedStateClientRpc(serverState);
    }

    [ClientRpc]
    private void TransmitCorrectedStateClientRpc(ActorNetData state)
    {
        if (this.IsServer)
        {
            return;
        }

        NotificationService.Instance.Info("");

        if (m_lastState.Position != state.Position)
        {
            this.transform.position = state.Position;
        }

        if (m_lastState.Hitpoints != state.Hitpoints)
        {
            NotificationService.Instance.Info($"Correcting HP : {m_lastState.Hitpoints}->{state.Hitpoints}");
            m_health.Hitpoints.Current = state.Hitpoints;
        }

        var hasMainWeapon = m_actorController.State.Inventory.HasMainWeapon();
        var hasSideArm = m_actorController.State.Inventory.HasSideArm();

        if (hasMainWeapon && state.Inventory.MainSlot == WeaponType.None)
        {
            m_actorController.State.Inventory.ClearMainWeapon();
        }

        if (hasSideArm && state.Inventory.SideArm == WeaponType.None)
        {
            m_actorController.State.Inventory.ClearSideArm();
        }
    }

    public override void OnNetworkDespawn()
    {
        var childComponents = this.GetComponents<RRMonoBehaviour>();

        foreach(var behaviour in childComponents)
        {
            behaviour.Reset();
        }

        base.OnNetworkDespawn();
    }

    public override void OnNetworkSpawn()
    {
        if (m_actorManager == null)
        {
            m_actorManager = ActorSpawnManager.Instance;
        }

        m_controller = this.GetComponent<PlayerInput>();

        if (this.IsOwner)
        {
            NotificationService.Instance.Info("Preparing local player actor ");
            m_actorManager.PrepareLocalPlayerActor(this.gameObject);
        }
        else
        {
            NotificationService.Instance.Info("Preparing remote player actor");
            m_actorManager.PrepareRemotePlayerActor(this.gameObject);
        }

        base.OnNetworkSpawn();
    }
}
