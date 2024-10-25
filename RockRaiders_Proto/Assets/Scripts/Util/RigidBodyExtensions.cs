using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class RigidBodyExtensions
    {
        public static void ResetVelocity(this Rigidbody rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
