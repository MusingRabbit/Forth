using RockRaiders.Util.DataStructures.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockRaiders.Util.Extensions
{
    public static class BoundingBoxExtensions
    {
        public static bool Contains(this BoundingBox lhs, BoundingBox rhs)
        {
            return lhs.Contains(rhs.Min) && lhs.Contains(rhs.Max);
        }
    }
}
