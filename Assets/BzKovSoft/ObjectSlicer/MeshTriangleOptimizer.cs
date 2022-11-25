using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	public static class MeshTriangleOptimizer
	{
		const float _angleTrashold = 0.00001f;
		const string _indexNotFoundTxt = "Index not found";
		const string _middleAndSameRoot = "Middle triangles found, but root vertices is the same";

		public static void OptimizeEdgeTriangles(LinkedList<LinkedLoop<int>> edgeLoopsByIndex, BzMeshData meshData, List<BzTriangle> bzTriangles)
		{
			bool[] trToDelete = new bool[bzTriangles.Count];

			var edgeLoopsNode = edgeLoopsByIndex.First;
			while (edgeLoopsNode != null)
			{
				var edgeLoop = edgeLoopsNode.Value;
				edgeLoopsNode = edgeLoopsNode.Next;

				var edge = edgeLoop.first;
				int counter = edgeLoop.size;
				while (counter > 0 & edgeLoop.size >= 3)
				{
					var edgeItem1 = edge;
					var edgeItem2 = edgeItem1.next;
					var edgeItem3 = edgeItem2.next;

					int i1 = edgeItem1.value;
					int i2 = edgeItem2.value;
					int i3 = edgeItem3.value;

					var v1 = meshData.Vertices[i1];
					var v2 = meshData.Vertices[i2];
					var v3 = meshData.Vertices[i3];

					var dir1 = v2 - v1;
					var dir2 = v3 - v2;
					float angle = GetAngleRad(dir1, dir2);

					if (angle < _angleTrashold && EmptyRedundantIndex(i3, i2, i1, meshData.Vertices, bzTriangles, trToDelete))
					{
						edgeItem2.Remove();
					}
					else
					{
						edge = edge.next;
						--counter;
					}
				}
			}

			// remove empty
			int count = 0;
			for (int i = 0; i < bzTriangles.Count; i++)
			{
				if (!trToDelete[i])
				{
					var value = bzTriangles[i];
					bzTriangles[count] = value;
					++count;
				}
			}

			bzTriangles.RemoveRange(count, bzTriangles.Count - count);
		}

		private static bool EmptyRedundantIndex(int indexLeft, int indexMiddle, int indexRight, List<Vector3> vertices, List<BzTriangle> bzTriangles, bool[] trToDelete)
		{
			// make redundants empty
			int trIndexRight = -1;
			int trIndexLeft = -1;
			List<int> innerItems = null;
			for (int i = 0; i < bzTriangles.Count; i++)
			{
				var tr = bzTriangles[i];
				if (trToDelete[i])
				{
					continue;
				}

				if (tr.i1 == indexMiddle | tr.i2 == indexMiddle | tr.i3 == indexMiddle)
				{
					if (tr.i1 == indexRight | tr.i2 == indexRight | tr.i3 == indexRight)
					{
						trIndexRight = i;
					}
					else if (tr.i1 == indexLeft | tr.i2 == indexLeft | tr.i3 == indexLeft)
					{
						trIndexLeft = i;
					}
					else
					{
						if (innerItems == null)
						{
							innerItems = new List<int>();
						}
						innerItems.Add(i);
					}
				}
			}

			if (trIndexLeft == -1 | trIndexRight == -1)
			{
				return false;
			}

			int i2, i3;
			var trRight = bzTriangles[trIndexRight];
			int rootBoneRight;
			GetIndexesOrdered(trRight, indexMiddle, out i2, out rootBoneRight);

			var trLeft = bzTriangles[trIndexLeft];
			int rootBoneLeft;
			GetIndexesOrdered(trLeft, indexMiddle, out rootBoneLeft, out i3);

			if (innerItems == null)
			{
				if (rootBoneRight != rootBoneLeft)
				{
					return false;
				}

				// recreate triangles
				bzTriangles[trIndexLeft] = new BzTriangle(indexLeft, indexRight, rootBoneRight);
				trToDelete[trIndexRight] = true;
			}
			else if (rootBoneRight == rootBoneLeft)
			{
				Debug.LogError(_middleAndSameRoot);
			}
			else
			{
				var vRootRight = vertices[rootBoneRight];
				var vRootLeft = vertices[rootBoneLeft];
				var dir = vRootLeft - vRootRight;

				var unorderedSegments = new List<KeyValuePair<int, int>>(bzTriangles.Count);
				for (int i = 0; i < innerItems.Count; i++)
				{
					var trIndex = innerItems[i];
					var tr = bzTriangles[trIndex];
					if (trToDelete[trIndex])
					{
						continue;
					}

					if (GetIndexesOrdered(tr, indexMiddle, out i2, out i3))
					{
						var vMid = vertices[i3];
						var dirMid = vMid - vRootRight;
						if (GetAngleRad(dir, dirMid) > _angleTrashold)
						{
							return false;
						}
						unorderedSegments.Add(new KeyValuePair<int, int>(i2, i3));
					}
				}

				int segPosition = rootBoneRight;

				while (unorderedSegments.Count != 0)
				{
					bool segFound = false;
					for (int i = 0; i < unorderedSegments.Count; i++)
					{
						var seg = unorderedSegments[i];
						if (seg.Key == segPosition)
						{
							segPosition = seg.Value;
							segFound = true;
							unorderedSegments.RemoveAt(i);
							break;
						}
					}

					if (!segFound)
					{
						return false;
					}
				}

				if (segPosition != rootBoneLeft)
				{
					return false;
				}

				for (int i = 0; i < innerItems.Count; i++)
				{
					var trIndex = innerItems[i];
					trToDelete[trIndex] = true;
				}

				// recreate triangles
				var vLeft  = vertices[indexLeft];
				var vRight = vertices[indexRight];
				var dirLeft  = vRootLeft  - vLeft;
				var dirRight = vRootRight - vRight;
				var dirRootLeft  = vRootRight - vRootLeft;
				var dirRootRight = vRootLeft - vRootRight;

				float angelLeft  = GetAngleRad(dirLeft, dirRootLeft);
				float angelRight = GetAngleRad(dirRight, dirRootRight);
				if (angelLeft > angelRight)
				{
					bzTriangles[trIndexLeft] = new BzTriangle(indexLeft, rootBoneRight, rootBoneLeft);
					bzTriangles[trIndexRight] = new BzTriangle(indexLeft, indexRight, rootBoneRight);
				}
				else
				{
					bzTriangles[trIndexLeft] = new BzTriangle(indexLeft, indexRight, rootBoneLeft);
					bzTriangles[trIndexRight] = new BzTriangle(indexRight, rootBoneRight, rootBoneLeft);
				}
			}

			return true;
		}

		private static float GetAngleRad(Vector3 dir1, Vector3 dir2)
		{
			float dot = Vector3.Dot(dir1, dir2);
			dot /= dir1.magnitude * dir2.magnitude;
			return Mathf.Acos(dot);
		}

		private static bool GetIndexesOrdered(BzTriangle tr, int i1, out int i2, out int i3)
		{
			if (tr.i1 == i1)
			{
				i2 = tr.i2;
				i3 = tr.i3;
			}
			else if (tr.i2 == i1)
			{
				i2 = tr.i3;
				i3 = tr.i1;
			}
			else if (tr.i3 == i1)
			{
				i2 = tr.i1;
				i3 = tr.i2;
			}
			else
			{
				i2 = -1;
				i3 = -2;
				return false;
			}
			return true;
		}
	}
}
