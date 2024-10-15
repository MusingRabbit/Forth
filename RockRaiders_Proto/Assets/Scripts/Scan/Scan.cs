using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Scan
{
    public struct PointData
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Quaternion Rotation;
        public float Weight;
    }

    public abstract class Scan : MonoBehaviour
    {
        public abstract List<PointData> GetPoints();
    }
}
