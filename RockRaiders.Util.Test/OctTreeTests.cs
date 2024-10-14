using RockRaiders.Util.DataStructures.Data;
using RockRaiders.Util.DataStructures.OctTree;
using System.Drawing;
using System.Numerics;

namespace RockRaiders.Util.Test
{
    struct TestData
    {
        public int Id;
        public Vector3 Pos;
        public Color Colour;
    }

    [TestClass]
    public class OctTreeTests
    {
        private Random m_rand;

        public OctTreeTests()
        {
            m_rand = new Random();
        }

        [TestMethod]
        public void OctTree_Ctor()
        {
            var tree = new OctTree<TestData>(Vector3.Zero, 100, 50, 1.0f);

            Assert.AreEqual(tree.BoundingBox.Min, Vector3.Zero);
            Assert.AreEqual(tree.BoundingBox.Max, new Vector3(100));
            
            Assert.AreEqual(tree.Count, 0);
        }

        [TestMethod]
        public void OctTree_Add_Single_InBounds()
        {
            var tree = new OctTree<TestData>(Vector3.Zero, 100, 50, 1.0f);

            var item1 = new OctTreeData<TestData> { 
                Data = new TestData { Id = m_rand.Next(), Pos = Vector3.One, Colour = Color.AliceBlue }, 
                BoundingBox = new BoundingBox(new Vector3(5), new Vector3(10))
            };

            tree.Add(item1);

            Assert.AreEqual(tree.Count, 1);
        }

        [TestMethod]
        public void OctTree_Add_Single_OutBounds()
        {
            var tree = new OctTree<TestData>(Vector3.Zero, 100, 50, 1.0f);

            var item1 = new OctTreeData<TestData>
            {
                Data = new TestData { Id = m_rand.Next(), Pos = Vector3.One, Colour = Color.AliceBlue },
                BoundingBox = new BoundingBox(new Vector3(101), new Vector3(120))
            };

            tree.Add(item1);

            Assert.AreEqual(tree.Count, 1);
            Assert.IsTrue(tree.BoundingBox.Contains(item1.BoundingBox.Centre));
        }


        [TestMethod]
        public void OctTree_Add_Multiple()
        {
            var tree = new OctTree<TestData>(Vector3.Zero, 100, 50, 1.0f);

            var item1 = new OctTreeData<TestData>
            {
                Data = new TestData { Id = m_rand.Next(), Pos = Vector3.One, Colour = Color.AliceBlue },
                BoundingBox = new BoundingBox(new Vector3(5), new Vector3(10))
            };

            var item2 = new OctTreeData<TestData>
            {
                Data = new TestData { Id = m_rand.Next(), Pos = Vector3.UnitZ, Colour = Color.Beige },
                BoundingBox = new BoundingBox(new Vector3(7), new Vector3(9))
            };

            var item3 = new OctTreeData<TestData>
            {
                Data = new TestData { Id = m_rand.Next(), Pos = Vector3.UnitX, Colour = Color.Brown },
                BoundingBox = new BoundingBox(new Vector3(1), new Vector3(10))
            };

            var item4 = new OctTreeData<TestData>
            {
                Data = new TestData { Id = m_rand.Next(), Pos = Vector3.UnitX, Colour = Color.Brown },
                BoundingBox = new BoundingBox(new Vector3(20), new Vector3(10))
            };

            var item5 = new OctTreeData<TestData>
            {
                Data = new TestData { Id = m_rand.Next(), Pos = Vector3.UnitX, Colour = Color.Brown },
                BoundingBox = new BoundingBox(new Vector3(10), new Vector3(50))
            };

            var item6 = new OctTreeData<TestData>
            {
                Data = new TestData { Id = m_rand.Next(), Pos = Vector3.UnitX, Colour = Color.Brown },
                BoundingBox = new BoundingBox(new Vector3(25), new Vector3(36))
            };

            tree.Add(item1);
            tree.Add(item2);
            tree.Add(item3);
            tree.Add(item4);
            tree.Add(item5);
            tree.Add(item6);

            Assert.AreEqual(tree.Count, 5);
        }
    }
}