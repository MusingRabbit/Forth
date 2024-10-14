using RockRaiders.Util.DataStructures.Data;
using RockRaiders.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RockRaiders.Util.DataStructures.OctTree
{
    public class OctTreeData<T>
    {
        public T Data { get; set; }
        public BoundingBox BoundingBox { get; set; }
    }

    public class OctTreeNode<T> : IDisposable
    {
        private Vector3 m_centre;
        private float m_baseLength;
        private float m_looseness;
        private float m_minSize;
        private float m_adjLength;
        private BoundingBox m_boundingBox;
        private readonly List<OctTreeData<T>> m_items;
        private List<OctTreeNode<T>> m_children;
        private BoundingBox[] m_childBounds;
        private int m_itemCountCap;

        public Vector3 Centre
        {
            get
            {
                return m_centre;
            }
        }

        public float BaseLength
        {
            get
            {
                return m_baseLength;
            }
        }

        public BoundingBox BoundingBox
        {
            get
            {
                return m_boundingBox;
            }
        }

        public OctTreeNode(Vector3 centre, float baseLength, float minSize, float looseness)
        {
            m_items = new List<OctTreeData<T>>();
            m_children = new List<OctTreeNode<T>>(8);
            this.Init(centre, baseLength, minSize, looseness);
        }

        private void Init(Vector3 centre, float baseLength, float minSize, float looseness)
        {
            m_baseLength = baseLength;
            m_minSize = minSize;
            m_looseness = looseness;
            m_adjLength = m_looseness * m_baseLength;

            m_centre = centre;

            var size = new Vector3(m_adjLength, m_adjLength, m_adjLength);
            m_boundingBox = new BoundingBox(m_centre, size);

            var quatBaseLength = m_baseLength / 4.0f;
            var childActLength = (m_baseLength / 2) * m_looseness;
            var childActSize = new Vector3(childActLength, childActLength, childActLength);

            m_childBounds = new BoundingBox[8];
            m_childBounds[0] = new BoundingBox(this.Centre + new Vector3(-quatBaseLength, quatBaseLength, -quatBaseLength), childActSize);
            m_childBounds[1] = new BoundingBox(this.Centre + new Vector3(quatBaseLength, quatBaseLength, -quatBaseLength), childActSize);
            m_childBounds[2] = new BoundingBox(this.Centre + new Vector3(-quatBaseLength, quatBaseLength, quatBaseLength), childActSize);
            m_childBounds[3] = new BoundingBox(this.Centre + new Vector3(quatBaseLength, quatBaseLength, quatBaseLength), childActSize);
            m_childBounds[4] = new BoundingBox(this.Centre + new Vector3(-quatBaseLength, -quatBaseLength, -quatBaseLength), childActSize);
            m_childBounds[5] = new BoundingBox(this.Centre + new Vector3(quatBaseLength, -quatBaseLength, -quatBaseLength), childActSize);
            m_childBounds[6] = new BoundingBox(this.Centre + new Vector3(-quatBaseLength, -quatBaseLength, quatBaseLength), childActSize);
            m_childBounds[7] = new BoundingBox(this.Centre + new Vector3(quatBaseLength, -quatBaseLength, quatBaseLength), childActSize);
        }

        public bool Add(OctTreeData<T> data)
        {
            if (!m_boundingBox.Contains(data.BoundingBox))
            {
                return false;
            }

            this.Insert(data);

            return true;
        }

        private OctTreeNode<T> FindChildOfBestFit(Vector3 objBoundsCentre)
        {
            int idx = (objBoundsCentre.X <= this.Centre.X ? 0 : 1)
                + (objBoundsCentre.Y >= this.Centre.Y ? 0 : 4)
                + (objBoundsCentre.Z <= this.Centre.Z ? 0 : 2);

            return m_children[idx];
        }

        private void SplitAndRedistribute()
        {
            this.CreateChildren();

            foreach(var item in m_items)
            {
                var childNode = this.FindChildOfBestFit(item.BoundingBox.Centre);

                if (childNode.BoundingBox.Contains(item.BoundingBox))
                {
                    childNode.Insert(item);
                    m_items.Remove(item);
                }
            }
        }

        private void Insert(OctTreeData<T> data)
        {
            if (!this.HasChildren())
            {
                if (m_items.Count < m_itemCountCap || (m_baseLength / 2) < m_minSize)
                {
                    m_items.Add(data);
                    return;
                }

                this.SplitAndRedistribute();
            }

            var childNode = this.FindChildOfBestFit(data.BoundingBox.Centre);
            
            if (childNode.BoundingBox.Contains(data.BoundingBox))
            {
                childNode.Insert(data);
            }
            else
            {
                m_items.Add(data);
            }

        }

        public bool Remove(T obj)
        {
            var result = false;

            for (int i = 0; i < m_items.Count; i++)
            {
                var currItem = m_items[i];

                if (currItem.Data?.Equals(obj) ?? false)
                {
                    if (m_items.Remove(currItem))
                    {
                        result = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < 8; i++)
            {
                var childNode = m_children[i];
                if (childNode.Remove(obj))
                {
                    result = true;
                    break;
                }
            }

            if (result && !this.HasChildren())
            {
                if (this.ShouldMerge())
                {
                    this.Merge();
                }
            }

            return result;
        }

        public bool Remove(OctTreeData<T> data)
        {
            if (!m_boundingBox.Contains(data.BoundingBox))
            {
                return false;
            }

            return Remove(data.Data);
        }

        public void CreateChildren()
        {
            m_children.Clear();

            var newLength = m_baseLength / 2;
            
            for (int i = 0; i < 8; i++)
            {
                m_children.Add(new OctTreeNode<T>(m_childBounds[i].Centre, newLength, m_minSize, m_looseness));
            }
        }

        private bool ShouldMerge()
        {
            int totalItemCount = m_items.Count;

            foreach(var node in m_children)
            {
                if (node.m_children != null)
                {
                    return false;
                }

                totalItemCount += node.m_items.Count;
            }

            return totalItemCount <= m_itemCountCap;
        }

        public void Shrink(float minLength)
        {
            if (BaseLength < (2 * minLength))
            {
                return;
            }

            if (m_items.Any() == m_children.Any() == false)
            {
                return;
            }

            var childActiveCount = 0;
            var lastActiveIdx = -1;

            for (int i = 0; i < m_children.Count; i++)
            {
                var child = m_children[i];

                if (child.HasAnyItems())
                {
                    lastActiveIdx = i;
                    childActiveCount++;
                }
            }

            if (childActiveCount == 1)
            {
                var lastChild = m_children[lastActiveIdx];
                var allItems = lastChild.GetAllItems();

                foreach (var item in allItems)
                {
                    if (!lastChild.BoundingBox.Contains(item.BoundingBox))
                    {
                        return;
                    }

                }


                this.Init(lastChild.Centre, m_baseLength / 2, m_minSize, m_looseness);
                this.SetChildren(lastChild.GetChildren().ToList());
            }
        }

        private void Merge()
        {
            for (int i = 0; i < 8; i++)
            {
                var child = m_children[i];
                int numItems = child.m_items.Count;

                for (int j = numItems - 1; j >= 0; j--)
                {
                    var curObj = child.m_items[j];
                    m_items.Add(curObj);
                }
            }

            m_children.Clear();
        }

        private void ClearChildren()
        {
            if (!this.HasChildren())
            {
                return;
            }

            for (int i = 0; i < 8; i ++)
            {
                m_children[i].Dispose();
            }

            m_children.Clear();
        }


        public bool HasChildren()
        {
            return m_children.Any();
        }

        public List<BoundingBox> GetChildBounds()
        {
            var result = new List<BoundingBox>();

            if (this.HasChildren())
            {
                foreach(var child in m_children)
                {
                    result.AddRange(child.GetChildBounds());
                }
            }

            result.Add(m_boundingBox);
            return result;
        }

        public bool Any()
        {
            if (m_items.Any())
            {
                return true;
            }

            if (this.HasChildren())
            {
                foreach(var child in m_children)
                {
                    if (child.Any())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Collides(BoundingBox bounds)
        {
            if (!m_boundingBox.Intersects(bounds))
            {
                return false;
            }

            for (int i = 0; i < m_items.Count; i ++)
            {
                var currItem = m_items[i];

                if (currItem.BoundingBox.Intersects(bounds))
                {
                    return true;
                }
            }

            for (int i = 0; i < m_children.Count; i++)
            {
                if (m_children[i].Collides(bounds))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Collides(Ray ray, float maxDistance = float.PositiveInfinity)
        {
            float distance;

            if (m_boundingBox.IntersectRay(ray, out distance) && distance <= maxDistance)
            {
                return true;
            }

            foreach (var item in m_items)
            {
                if (item.BoundingBox.IntersectRay(ray, out distance) && distance <= maxDistance)
                {
                    return true;
                }
            }

            foreach(var child in m_children)
            {
                if (child.Collides(ray, maxDistance))
                {
                    return true;
                }
            }

            return false;
        }

        public List<T> QueryByBoundingBox(BoundingBox boundingBox)
        {
            var result = new List<T>();

            if (!m_boundingBox.Intersects(boundingBox))
            {
                return result;
            }

            foreach(var item in m_items)
            {
                if (item.BoundingBox.Intersects(boundingBox))
                {
                    result.Add(item.Data);
                }
            }

            foreach (var child in m_children)
            {
                result.AddRange(child.QueryByBoundingBox(boundingBox));
            }

            return result;
        }

        public List<T> QueryByRay(Ray ray, float maxDistance = float.PositiveInfinity)
        {
            var result = new List<T>();
            var distance = 0.0f;

            if (!m_boundingBox.IntersectRay(ray))
            {
                return result;
            }

            foreach(var item in m_items)
            {
                if (item.BoundingBox.IntersectRay(ray, out distance) || distance > maxDistance)
                {
                    result.Add(item.Data);
                }
            }

            foreach(var child in m_children)
            {
                result.AddRange(child.QueryByRay(ray, maxDistance));
            }

            return result;
        }

        public IEnumerable<OctTreeNode<T>> GetChildren()
        {
            return m_children;
        }

        public void SetChildren(IEnumerable<OctTreeNode<T>> nodes)
        {
            if (nodes.Count() != 8)
            {
                throw new ArgumentOutOfRangeException(nameof(nodes), "Invalid node count. Expected 8 nodes");
            }

            m_children.Clear();

            foreach(var node in nodes)
            {
                m_children.Add(node);
            }
        }

        public List<OctTreeData<T>> GetAllItems()
        {
            var result = new List<OctTreeData<T>>();

            foreach(var item in m_items)
            {
                result.Add(item);
            }

            foreach (var child in m_children)
            {
                result.AddRange(child.GetAllItems());
            }

            return result;
        }

        public bool HasAnyItems()
        {
            if (m_items.Any())
            {
                return true;
            }

            foreach(var child in m_children)
            {
                if (child.HasAnyItems())
                {
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            m_items.Clear();
            this.ClearChildren();
        }
    }
}
