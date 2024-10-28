using Assets.Scripts.Pickups.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Assets.Scripts.Util
{
    public static  class ActorInventoryExtensions
    {
        public static ActorInventoryState ToActorInventoryState(this NetActorInventory netActInv)
        {
            NetworkObject mainWeapon = null;
            NetworkObject sidearm = null;

            if (netActInv.MainWeaponType != WeaponType.None)
            {
                mainWeapon = NetworkManager.Singleton.SpawnManager.SpawnedObjects[netActInv.MainWeaponNetworkObjectId];
            }

            if (netActInv.SideArmType != WeaponType.None)
            {
                sidearm = NetworkManager.Singleton.SpawnManager.SpawnedObjects[netActInv.SidearmNetworkObjectId];
            }

            return new ActorInventoryState
            {
                MainWeaponType = netActInv.MainWeaponType,
                MainWeapon = mainWeapon?.gameObject.GetComponent<Weapon>(),
                SideArmType = netActInv.SideArmType,
                SideArm = sidearm?.gameObject.GetComponent<Weapon>()
            };
        }

        public static NetActorInventory ToNetActorInventory(this ActorInventoryState state)
        {
            return new NetActorInventory
            {
                MainWeaponType = state.MainWeaponType,
                SideArmType = state.SideArmType,
                Pack = state.PackType,

                MainWeaponNetworkObjectId = state.MainWeapon?.GetComponent<NetworkObject>().NetworkObjectId ?? ulong.MaxValue,
                SidearmNetworkObjectId = state.SideArm?.GetComponent<NetworkObject>().NetworkObjectId ?? ulong.MaxValue
            };
        }
    }
}
