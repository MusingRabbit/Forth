using Assets.Scripts.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class GameObjectExtensions
    {
        public static bool IsWeapon(this GameObject obj)
        {
            return obj.GetComponent<Weapon>() != null;
        }
    }
}
