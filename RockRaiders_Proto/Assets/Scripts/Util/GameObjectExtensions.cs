using Assets.Scripts.Pickups;
using Assets.Scripts.Pickups.Weapons;
using Assets.Scripts.Pickups.Weapons.Projectiles;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class GameObjectExtensions
    {
        public static bool IsPickupItem(this GameObject obj)
        {
            return obj.GetComponent<PickupItem>() != null;
        }

        public static bool IsWeapon(this GameObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj.GetComponent<Weapon>() != null;
        }

        public static bool IsProjectile(this GameObject obj)
        {
            return obj.GetComponent<Projectile>();
        }
    }
}
