using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class NetworkHelper
    {
        public static GameObject GetPlayerObjectByNetworkObjectId(ulong networkObjectId)
        {
            GameObject result = null;
            var spawnedObjs = NetworkManager.Singleton.SpawnManager.SpawnedObjects;

            if (networkObjectId > 0 && spawnedObjs.ContainsKey(networkObjectId))
            {
                result = spawnedObjs[networkObjectId].gameObject;
            }
            else
            {
                //return null;
                Debugger.Break();
                throw new Exception("No GameObject could be found for provided 'PlayerNetworkObjectId'");
            }

            return result;
        }
    }
}
