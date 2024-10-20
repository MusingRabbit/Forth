using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCommandLine : MonoBehaviour
{
    private NetworkManager m_netManager;

    // Start is called before the first frame update
    void Start()
    {
        m_netManager = this.GetComponentInParent<NetworkManager>();

        if (Application.isEditor)
        {
            return;
        }

        var args = this.GetCommandLineArgs();

        if (args.TryGetValue("-mode", out var mode))
        {
            switch(mode)
            {
                case "server":
                    m_netManager.StartServer();
                    break;
                case "host":
                    m_netManager.StartHost();
                    break;
                case "client":
                    m_netManager.StartClient();
                    break;
            }
        }
    }

    private Dictionary<string,string> GetCommandLineArgs()
    {
        var result = new Dictionary<string, string>();

        var args = System.Environment.GetCommandLineArgs();

        for(int i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLower();

            if (arg.StartsWith('-'))
            {
                var value = i < args.Length - 1 ? args[i + 1].ToLower() : null;
                value = value?.StartsWith('-') ?? false ? null : value;

                result.Add(arg, value);
            }
        }

        return result;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
