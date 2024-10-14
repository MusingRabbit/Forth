using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts
{
    public class SessionGUI : MonoBehaviour
    {
        [SerializeField]
        private NetworkManager m_netManager;

        private void Awake()
        {
            m_netManager = GetComponent<NetworkManager>();
        }

        private void OnGUI()
        {
            if (m_netManager == null)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(10, 10, 300, 300));

            if (!m_netManager.IsClient && !m_netManager.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
            }

            GUILayout.EndArea();
        }

        private void StatusLabels()
        {
            var mode = m_netManager.IsHost ? "Host" : m_netManager.IsServer ? "Server" : "Client";

            var transportType = m_netManager.NetworkConfig.NetworkTransport.GetType().Name;
            GUILayout.Label("Transport: " + transportType);

            GUILayout.Label("Mode: " + mode);
        }

        private void StartButtons()
        {
            if (GUILayout.Button("Host"))
            {
                m_netManager.StartHost();
            }

            if (GUILayout.Button("Client"))
            {
                m_netManager.StartClient();
            }

            if (GUILayout.Button("Server"))
            {
                m_netManager.StartServer();
            }
        }
    }
}