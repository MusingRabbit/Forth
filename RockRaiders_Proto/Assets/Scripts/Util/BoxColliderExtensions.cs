using RockRaiders.Util.DataStructures.Data;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class BoxColliderExtensions
    {
        public static BoundingBox ToBoundingBox(this BoxCollider boxCollider)
        {
            var vMin = new System.Numerics.Vector3(boxCollider.bounds.min.x, boxCollider.bounds.min.y, boxCollider.bounds.min.z);
            var vMax = new System.Numerics.Vector3(boxCollider.bounds.max.x, boxCollider.bounds.max.y, boxCollider.bounds.max.z);

            return new BoundingBox(vMin, vMax);
        }
    }
}
