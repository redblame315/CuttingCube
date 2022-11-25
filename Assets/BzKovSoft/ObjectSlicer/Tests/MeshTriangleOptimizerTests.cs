using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Tests
{
	public class MeshTriangleOptimizerTests
	{
		[Test]
		public void Optimize()
		{
			//  1         4           5         2
			//   |--------*-----------|--------|
			//   |        | \         \        |
			//   |   t0   /  \   t3    |  t4   |
			//   |       | |   \       \       |
			//   |      /  |    \       \      |
			//   |      /  \      \      |     |
			//   |     |    |      \     \     |
			//   |    /     |        \    \    |
			//   |    /     \         \    |   |
			//   |   |       |          \  \   |
			//   |  /   t1   |    t2     \  \  |
			//   |  /        \             \ | |
			//   | |          |             \\ |
			//   |/___________|_______________\|
			//  0             6                 3

			//Arrange
			var vertices = new Vector3[]
			{
				new Vector3(-10,-10, 0),
				new Vector3(-10, 10, 0),
				new Vector3( 10, 10, 0),
				new Vector3( 10,-10, 0),

				new Vector3( -5, 10, 0),  // 4
				new Vector3(  5, 10, 0),  // 5
				new Vector3(  0,-10, 0),  // 6
			};

			List<BzTriangle> trianglesSliced = new List<BzTriangle>();
			trianglesSliced.Add(new BzTriangle(1, 4, 0));
			trianglesSliced.Add(new BzTriangle(4, 6, 0));
			trianglesSliced.Add(new BzTriangle(4, 3, 6));
			trianglesSliced.Add(new BzTriangle(4, 5, 3));
			trianglesSliced.Add(new BzTriangle(5, 2, 3));

			var mesh = new Mesh();
			mesh.vertices = vertices;

			var meshData = new BzMeshData(mesh, null);
			var linkedLoops = new LinkedList<LinkedLoop<int>>();
			var linkedLoop = new LinkedLoop<int>();
			linkedLoops.AddLast(linkedLoop);
			linkedLoop.AddLast(2);
			linkedLoop.AddLast(5);
			linkedLoop.AddLast(4);
			linkedLoop.AddLast(1);

			//Act
			MeshTriangleOptimizer.OptimizeEdgeTriangles(linkedLoops, meshData, trianglesSliced);

			//Assert
			Assert.AreEqual(2, trianglesSliced.Count);
			var tr1 = trianglesSliced[0];
			var tr2 = trianglesSliced[1];

			bool case1 =
				TrAreEqual(tr1, new BzTriangle(1, 2, 0)) &&
				TrAreEqual(tr2, new BzTriangle(1, 3, 0));
			bool case2 =
				TrAreEqual(tr1, new BzTriangle(1, 2, 0)) &&
				TrAreEqual(tr2, new BzTriangle(2, 3, 0));

			Assert.That(case1 ^ case2);
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void DiffDirections(bool diffDir)
		{
			//  1         4         2
			//   |--------|--------|
			//   |        |\       |
			//   |   t0   | |  t3  |
			//   |       || \      |
			//   |      / |  |     |
			//   |      / |  \     |
			//   |     |  |   |    |
			//   |    /   |   \    |
			//   |    /   |    |   |
			//   |   |    |    \   |
			//   |  /   t1| t2  |  |
			//   |  /     |     \  |
			//   | |      |      | |
			//   |/_______|_     | |
			//  0         5 `"--..\|
			//                      3

			//Arrange
			var vertices = new Vector3[]
			{
				new Vector3(-10,-10, 0),
				new Vector3(-10, 10, 0),
				new Vector3( 10, 10, 0),
				new Vector3( 10, diffDir ? -15 : -10, 0),

				new Vector3(  0, 10, 0),  // 4
				new Vector3(  0,-10, 0),  // 5
			};

			List<BzTriangle> trianglesSliced = new List<BzTriangle>();
			trianglesSliced.Add(new BzTriangle(1, 4, 0));
			trianglesSliced.Add(new BzTriangle(4, 5, 0));
			trianglesSliced.Add(new BzTriangle(4, 3, 5));
			trianglesSliced.Add(new BzTriangle(4, 2, 3));

			var mesh = new Mesh();
			mesh.vertices = vertices;

			var meshData = new BzMeshData(mesh, null);
			var linkedLoops = new LinkedList<LinkedLoop<int>>();
			var linkedLoop = new LinkedLoop<int>();
			linkedLoops.AddLast(linkedLoop);
			linkedLoop.AddLast(2);
			linkedLoop.AddLast(4);
			linkedLoop.AddLast(1);

			//Act
			MeshTriangleOptimizer.OptimizeEdgeTriangles(linkedLoops, meshData, trianglesSliced);

			//Assert
			if (diffDir)
			{
				Assert.That(trianglesSliced.Count >= 3);
			}
			else
			{
				Assert.AreEqual(2, trianglesSliced.Count);
			}
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void BigAngle(bool mirror)
		{
			//   0     1                           2
			// --|-----|.-------------------------|--
			//   \      \ *._                    /
			//     \      \  *._                /
			//       \     \    *._            /
			//         \     \     *._        /
			//           \    \       *._    /
			//             \    \        *_ /
			//               \   \         |  3
			//                 \   \       |
			//                   \  \      |
			//                     \  \    |
			//                       \ \   |
			//                         \ \ |
			//                           \\|
			//                        -----*-----
			//                             4

			//Arrange
			float m = mirror ? -1f : 1f;
			var vertices = new Vector3[]
			{
				new Vector3(m * -10, 0, 0),
				new Vector3(m *  -5, 0, 0),
				new Vector3(m *  10, 0, 0),

				new Vector3(  0,-1, 0),
				new Vector3(  0,-5, 0),
			};

			List<BzTriangle> trianglesSliced = new List<BzTriangle>();
			trianglesSliced.Add(GetOrder(new BzTriangle(0, 1, 4), mirror));
			trianglesSliced.Add(GetOrder(new BzTriangle(1, 3, 4), mirror));
			trianglesSliced.Add(GetOrder(new BzTriangle(1, 2, 3), mirror));

			var mesh = new Mesh();
			mesh.vertices = vertices;

			var meshData = new BzMeshData(mesh, null);
			var linkedLoops = new LinkedList<LinkedLoop<int>>();
			var linkedLoop = new LinkedLoop<int>();
			linkedLoops.AddLast(linkedLoop);
			if (mirror)
			{
				linkedLoop.AddLast(0);
				linkedLoop.AddLast(1);
				linkedLoop.AddLast(2);
			}
			else
			{
				linkedLoop.AddLast(2);
				linkedLoop.AddLast(1);
				linkedLoop.AddLast(0);
			}

			//Act
			MeshTriangleOptimizer.OptimizeEdgeTriangles(linkedLoops, meshData, trianglesSliced);

			//Assert
			Assert.AreEqual(2, trianglesSliced.Count);
			var tr1 = trianglesSliced[0];
			var tr2 = trianglesSliced[1];

			bool case1 =
				TrAreEqual(tr1, GetOrder(new BzTriangle(0, 2, 3), mirror)) &&
				TrAreEqual(tr2, GetOrder(new BzTriangle(0, 3, 4), mirror));
			bool case2 =
				TrAreEqual(tr1, GetOrder(new BzTriangle(0, 3, 4), mirror)) &&
				TrAreEqual(tr2, GetOrder(new BzTriangle(0, 2, 3), mirror));

			Assert.That(case1 ^ case2);
		}

		BzTriangle GetOrder(BzTriangle tr, bool mirror)
		{
			if (!mirror)
			{
				return tr;
			}

			return new BzTriangle(tr.i2, tr.i1, tr.i3);
		}

		private bool TrAreEqual(BzTriangle trA, BzTriangle trB)
		{
			trA = GetOrdered(trA);
			trB = GetOrdered(trB);
			return
				trA.i1 == trB.i1 &
				trA.i2 == trB.i2 &
				trA.i3 == trB.i3;
		}

		private BzTriangle GetOrdered(BzTriangle tr)
		{
			if (tr.i1 <= tr.i2 & tr.i1 <= tr.i3)
			{
				return tr;
			}
			if (tr.i2 <= tr.i1 & tr.i2 <= tr.i3)
			{
				return new BzTriangle(tr.i2, tr.i3, tr.i1);
			}
			if (tr.i3 <= tr.i1 & tr.i3 <= tr.i2)
			{
				return new BzTriangle(tr.i3, tr.i1, tr.i2);
			}

			throw new InvalidOperationException();
		}
	}
}
