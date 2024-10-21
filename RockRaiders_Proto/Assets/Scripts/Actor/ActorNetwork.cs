using Assets.Scripts;
using Assets.Scripts.Actor;
using Assets.Scripts.Factory;
using Assets.Scripts.Input;
using Unity.Netcode;
using UnityEngine;

internal struct ActorNetData : INetworkSerializable
{
    public Vector3 Position;
    public Quaternion Rotation;
    public NetPlayerInput Controller;
    public Vector3 CrosshairPosition;
    public int SelectedWeapon;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) 
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref this.Position);
        serializer.SerializeValue(ref this.Rotation);
        serializer.SerializeValue(ref this.CrosshairPosition);
        serializer.SerializeValue(ref this.Controller);
        serializer.SerializeValue(ref this.SelectedWeapon);
    }
}

internal struct NetworkIdData : INetworkSerializable
{
    public ulong NetworkId;
    public bool HasNetworkId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref this.NetworkId);
        serializer.SerializeValue(ref this.HasNetworkId);
    }
}

public class ActorNetwork : NetworkBehaviour
{
    private NetworkVariable<ActorNetData> m_playerState;

    private PlayerInput m_controller;
    private ActorController m_actorController;
    private ActorSpawnManager m_actorManager;

    private ActorCrosshair m_crosshair;

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
            this.transform.position = state.Position;
            this.transform.rotation = state.Rotation;

            state.Controller.LookX = 0;                     //TODO : This is a hack. Send ActorCamera position instead....
            state.Controller.LookY = 0;
            m_crosshair.UpdateAimpointFromCamera = false;
            m_crosshair.AimPoint = state.CrosshairPosition;

            m_controller.SetStateFromNetPlayerInput(state.Controller);
            m_actorController.State.SelectWeapon((SelectedWeapon)state.SelectedWeapon);
        }
    }

    private void SendState()
    {
        var state = new ActorNetData
        {
            Position = this.transform.position,
            Rotation = this.transform.rotation,
            Controller = m_controller.GetNetPlayerInput(),
            CrosshairPosition = m_crosshair.AimPoint,
            SelectedWeapon = (int)m_actorController.GetComponent<ActorState>().SelectedWeapon
        };

        if (this.IsServer || !m_serverAuth)
        {
            m_playerState.Value = state;
        }
        else
        {
            this.TransmitServerRpc(state);
        }
    }

    public void SetParent(GameObject parent)
    {
        if (!this.IsOwner)
        {
            return;
        }

        if (this.IsServer)
        {
            this.gameObject.transform.parent = parent?.transform;
        }
        else
        {
            var netObj = parent?.GetComponent<NetworkObject>();

            var data = new NetworkIdData
            {
                HasNetworkId = netObj != null,
                NetworkId = netObj?.NetworkObjectId ?? uint.MaxValue
            };

            this.SetParentServerRpc(data);
        }
    }

    [ServerRpc]
    private void SetParentServerRpc(NetworkIdData data)
    {
        if (!data.HasNetworkId)
        {
            this.gameObject.transform.parent = null;
            return;
        }

        //var originalScale = this.gameObject.transform.localScale;
        var netObj = this.GetNetworkObject(data.NetworkId);
        this.gameObject.transform.parent = netObj.gameObject.transform;
        //this.gameObject.transform.localScale = originalScale;
    }

    [ServerRpc]
    private void TransmitServerRpc(ActorNetData state)
    {
        m_playerState.Value = state;
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
            Debug.Log("Actor manager is null. Fix this hack.");
            m_actorManager = ActorSpawnManager.Instance;
        }

        m_controller = this.GetComponent<PlayerInput>();

        if (this.IsOwner)
        {
            m_actorManager.PrepareLocalPlayerActor(this.gameObject);
        }
        else
        {
            m_actorManager.PrepareRemotePlayerActor(this.gameObject);
        }

        base.OnNetworkSpawn();
    }
}
