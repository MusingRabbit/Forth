using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class MathHelper
    {
        public static Quaternion GetQuaternionFromVelocity(Vector3 vecocityVector, float deltaTime)
        {
            var s = (deltaTime * 0.5f);
            var half = new Vector3(vecocityVector.x * s, vecocityVector.y * s, vecocityVector.z * s);

            var mag = half.magnitude;
            var norm = half.normalized;

            return new Quaternion(Mathf.Cos(mag), half.x, half.y, half.z);
        }
    }
}
