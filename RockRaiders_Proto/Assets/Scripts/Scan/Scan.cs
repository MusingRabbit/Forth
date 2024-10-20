using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Scan
{
    public struct PointData
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Centre;
        public float Height;
        public Quaternion Rotation;
        public float Weight;
    }

    public abstract class Scan : MonoBehaviour
    {
        public abstract List<PointData> GetPoints();
    }
}
