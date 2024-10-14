using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RockRaiders.Util.DataStructures.Data
{
    public struct Ray
    {
        private Vector3 m_direction;

        public Vector3 Origin { get; set; }

        public Vector3 Direction
        {
            get
            {
                return m_direction;
            }
            set
            {
                m_direction = Vector3.Normalize(value);
            }
        }

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            m_direction = Vector3.Normalize(direction);
        }

        public Vector3 GetPoint(float distance)
        {
            return Origin + Direction * distance;
        }

        public override string ToString()
        {
            return string.Format("Origin : {0}, Dir : {1}", Origin, m_direction);
        }
    }
}
