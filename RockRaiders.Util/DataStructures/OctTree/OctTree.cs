using RockRaiders.Util.DataStructures.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RockRaiders.Util.DataStructures.OctTree
{
    public class OctTree<T>
    {
        private OctTreeNode<T> m_root;
        private readonly float m_overlap;
        private readonly float m_initialSize;
        private readonly float m_minSize;
        private int m_count;

        public int Count
        {
            get
            {
                return m_root.GetAllItems().Count;
            }
        }

        public BoundingBox BoundingBox
        {
            get
            {
                return m_root.BoundingBox;
            }
        }

        public List<BoundingBox> GetChildBounds()
        {
            return m_root.GetChildBounds();
        }

        public OctTree(Vector3 worldPos, float worldSize, float minNodeSize, float overlap)
        {
            if (minNodeSize > worldSize)
            {
                throw new ArgumentException(nameof(minNodeSize) + " cannot be greater than " + nameof(worldSize));
            }

            m_count = 0;
            m_initialSize = worldSize;
            m_minSize = minNodeSize;
            m_overlap = overlap;
            m_root = new OctTreeNode<T>(worldPos, m_initialSize, m_minSize, m_overlap);
        }

        public void Add(OctTreeData<T> data)
        {
            int count = 0;

            var oldRoot = m_root;

            while (!m_root.Add(data)) //This doesn't sit well with me. Something smells.
            {
                if (count > 20)
                {
                    throw new TimeoutException("Operation is taking too long"); 
                }

                this.Grow(data.BoundingBox.Centre - m_root.Centre); //This doesn't sit well with me. Something smells.
                count++;
            }

            if (count > 0)
            {
                if (oldRoot.HasAnyItems())
                {
                    foreach (var item in oldRoot.GetAllItems())
                    {
                        m_root.Add(item);
                    }
                }
            }
        }

        public bool Remove(T item)
        {
            var result = m_root.Remove(item);

            if (result)
            {
                this.Shrink();
            }

            return result;
        }

        public bool Remove(OctTreeData<T> data)
        {
            var result = m_root.Remove(data);

            if (result)
            {
                this.Shrink();
            }

            return result;
        }

        public bool Contains(BoundingBox boundingBox)
        {
            return m_root.BoundingBox.Contains(boundingBox.Centre);
        }

        public bool Contains(Ray ray, float maxDistance)
        {
            return m_root.BoundingBox.IntersectRay(ray, out var distance) && distance < maxDistance;
        }

        public List<T> Query(Ray ray, float maxDistance)
        {
            return m_root.QueryByRay(ray, maxDistance);
        }

        public List<T> Query(BoundingBox boundingBox)
        {
            return m_root.QueryByBoundingBox(boundingBox);
        }

        private void Shrink()
        {
            throw new NotImplementedException();
        }

        private void Grow(Vector3 deltaV)
        {
            float half = m_root.BaseLength / 2;
            float newLength = m_root.BaseLength * 2;

            int xDir = deltaV.X >= 0 ? 1 : -1;
            int yDir = deltaV.Y >= 0 ? 1 : -1;
            int zDir = deltaV.Z >= 0 ? 1 : -1;

            float xPos = (float)(int)(deltaV.X / half);
            float yPos = (float)(int)(deltaV.Y / half);
            float zPos = (float)(int)(deltaV.Z / half);

            Vector3 newCentre = m_root.Centre + new Vector3(xDir * half, yDir * half, zDir * half);

            m_root = new OctTreeNode<T>(newCentre, newLength, m_minSize, m_overlap);
        }
    }
}
