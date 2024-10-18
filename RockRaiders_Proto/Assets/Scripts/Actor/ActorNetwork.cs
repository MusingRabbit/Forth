using Assets.Scripts.Actor;
using Assets.Scripts.Factory;
using Assets.Scripts.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    //private Crosshair m_crosshair;
    private ActorSpawnManager m_actorManager;

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
        m_actorController = this.GetComponent<ActorController>();
        //m_crosshair = m_actorController.Crosshair.GetComponent<Crosshair>();

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
            //var crosshair = m_actorController.GetComponent<Crosshair>();
            var state = m_playerState.Value;
            this.transform.position = state.Position;
            this.transform.rotation = state.Rotation;
            m_controller.SetStateFromNetPlayerInput(state.Controller);
            //m_crosshair.gameObject.transform.position = state.CrosshairPosition;
            //m_crosshair.enabled = false;

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
            //CrosshairPosition = m_crosshair.transform.position,
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
