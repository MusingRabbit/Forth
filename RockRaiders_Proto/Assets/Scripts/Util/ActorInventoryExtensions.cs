using Assets.Scripts.Pickups;
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
            NetworkObject pack = null;

            var spawnedObjs = NetworkManager.Singleton.SpawnManager.SpawnedObjects;

            if (netActInv.MainWeaponType != WeaponType.None && netActInv.MainWeaponNetworkObjectId > 0)
            {
                mainWeapon = spawnedObjs[netActInv.MainWeaponNetworkObjectId];
            }

            if (netActInv.SideArmType != WeaponType.None && netActInv.SidearmNetworkObjectId > 0)
            {
                sidearm = spawnedObjs[netActInv.SidearmNetworkObjectId];
            }

            if (netActInv.PackType != PackType.None && netActInv.PackNetworkObjectId > 0)
            {
                pack = spawnedObjs[netActInv.PackNetworkObjectId];
            }

            return new ActorInventoryState
            {
                MainWeaponType = netActInv.MainWeaponType,
                MainWeapon = mainWeapon?.gameObject.GetComponent<Weapon>(),
                SideArmType = netActInv.SideArmType,
                SideArm = sidearm?.gameObject.GetComponent<Weapon>(),
                PackType = netActInv.PackType,
                Pack = pack?.gameObject.GetComponent<PickupItem>()
            };
        }

        public static NetActorInventory ToNetActorInventory(this ActorInventoryState state)
        {
            return new NetActorInventory
            {
                MainWeaponType = state.MainWeaponType,
                SideArmType = state.SideArmType,
                PackType = state.PackType,

                MainWeaponNetworkObjectId = state.MainWeapon?.GetComponent<NetworkObject>().NetworkObjectId ?? 0,
                SidearmNetworkObjectId = state.SideArm?.GetComponent<NetworkObject>().NetworkObjectId ?? 0,
                PackNetworkObjectId = state.Pack?.GetComponent<NetworkObject>().NetworkObjectId ?? 0
            };
        }
    }
}
