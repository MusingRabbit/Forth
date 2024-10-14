using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RockRaiders.Util.DataStructures.Data
{
    public struct BoundingBox
    {
        public Vector3 Centre { get; set; }

        public Vector3 Dimensions { get; set; }

        public Vector3 Size
        {
            get
            {
                return Dimensions * 2;
            }
            set
            {
                Dimensions = value * 0.5f;
            }
        }

        public Vector3 Min
        {
            get
            {
                return Centre - Dimensions;
            }
            set
            {
                SetMinMax(value, Max);
            }
        }

        public Vector3 Max
        {
            get
            {
                return Centre + Dimensions;
            }
            set
            {
                SetMinMax(Min, value);
            }
        }

        public BoundingBox(Vector3 min, Vector3 max)
        {
            SetMinMax(min, max);
        }

        public void Encapsulate(Vector3 point)
        {
            SetMinMax(Vector3.Min(Min, point), Vector3.Max(Max, point));
        }

        public void SetMinMax(Vector3 min, Vector3 max)
        {
            Dimensions = (max - min) * 0.5f;
            Centre = min + Dimensions;
        }

        public void Encapsulate(BoundingBox box)
        {
            Encapsulate(box.Centre - box.Dimensions);
            Encapsulate(box.Centre + box.Dimensions);
        }

        public void Expand(float amount)
        {
            amount *= 0.5f;
            Dimensions += new Vector3(amount, amount, amount);
        }

        public void Expand(Vector3 amount)
        {
            Dimensions += amount * 0.5f;
        }

        public bool Contains(Vector3 point)
        {
            return
                Min.X <= point.X && Max.X >= point.X &&
                Min.Y <= point.Y && Max.Y >= point.Y &&
                Min.Z <= point.Z && Max.Z >= point.Z;
        }

        public bool Intersects(BoundingBox box)
        {
            return
                Min.X <= box.Max.X && Max.X >= box.Min.X &&
                Min.Y <= box.Max.Y && Max.Y >= box.Min.Y &&
                Min.Z <= box.Max.Z && Max.Z >= box.Min.Z;
        }

        public bool IntersectRay(Ray ray)
        {
            float distance;
            return IntersectRay(ray, out distance);
        }

        public bool IntersectRay(Ray ray, out float distance)
        {
            Vector3 dirFrac = new Vector3(1f / ray.Direction.X, 1f / ray.Direction.Y, 1f / ray.Direction.Z);

            float t1 = (Min.X - ray.Origin.X) * dirFrac.X;
            float t2 = (Max.X - ray.Origin.X) * dirFrac.X;
            float t3 = (Min.Y - ray.Origin.Y) * dirFrac.Y;
            float t4 = (Min.Y - ray.Origin.Y) * dirFrac.Y;
            float t5 = (Min.Z - ray.Origin.Z) * dirFrac.Z;
            float t6 = (Min.Z - ray.Origin.Z) * dirFrac.Z;

            float tMin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tMax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            // If tmax < 0, ray (line) is intersecting Bounding Box, but the whole Bounding Box is behind the bounding box.
            if (tMax < 0)
            {
                distance = tMax;
                return false;
            }

            // If tMin > tMax, the ray doesn't intersect the Bounding Box
            if (tMin > tMax)
            {
                distance = tMax;
                return false;
            }

            distance = tMin;
            return true;
        }

        public override int GetHashCode()
        {
            return Centre.GetHashCode() ^ Dimensions.GetHashCode() << 2;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (!(obj is BoundingBox))
            {
                return false;
            }
            else
            {
                var rhs = (BoundingBox)obj;
                return Centre.Equals(rhs.Centre) && Dimensions.Equals(rhs.Dimensions);
            }
        }

        public override string ToString()
        {
            return string.Format("Centre : {0}, Dimensions : {1}", Centre, Dimensions);
        }

        public static bool operator ==(BoundingBox lhs, BoundingBox rhs)
        {
            return lhs.Centre == rhs.Centre && lhs.Dimensions == rhs.Dimensions;
        }

        public static bool operator !=(BoundingBox lhs, BoundingBox rhs)
        {
            return !(lhs == rhs);
        }
    }
}
