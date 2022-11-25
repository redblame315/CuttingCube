using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace BzKovSoft.ObjectSlicer.Polygon
{
	public class BzPolyLoop
	{
		private readonly List<int> triangles;
		public readonly LinkedLoop<int> edgeLoop;
		public readonly Vector2[] polyVertices2D;
		public readonly BzMeshData meshData;

		public bool OuterLoop { get; private set; }
		public float Volume { get; private set; }
		public Vector2 Center { get; private set; }
		/// <summary>
		/// True if it is possible to create a polygon
		/// </summary>
		public bool Created { get; private set; }

		/// <param name="vertices">Chain of vertices for polygon</param>
		/// <param name="normal">Normal the polygon is facing to</param>
		public BzPolyLoop(BzMeshData meshData, LinkedLoop<int> edgeLoop, Vector3 normal, IBzSliceAdapter adapter)
		{
			this.meshData = meshData;
			this.edgeLoop = edgeLoop;

			if (edgeLoop.size < 3)
				return;

			Profiler.BeginSample("ConvertV3ToV2");
			polyVertices2D = ConvertV3ToV2(adapter, normal);
			Profiler.EndSample();

			Profiler.BeginSample("MakeMesh");
			var newTriangles1 = MakeMesh(true);
			var newTriangles2 = MakeMesh(false);
			Profiler.EndSample();

			// get triangle list with more vertices
			OuterLoop = newTriangles1.Count >= newTriangles2.Count;
			if (OuterLoop)
				triangles = newTriangles1;
			else
				triangles = newTriangles2;

			if (triangles.Count != 0)
			{
				CalculateMetodata();
				Created = true;
			}
		}

		public bool IsInside(BzPolyLoop outer)
		{
			var innerCenter = Center;

			for (int i = 0; i < outer.triangles.Count; i += 3)
			{
				int i1 = outer.triangles[i];
				int i2 = outer.triangles[i + 1];
				int i3 = outer.triangles[i + 2];
				Vector2 v1 = outer.polyVertices2D[i1];
				Vector2 v2 = outer.polyVertices2D[i2];
				Vector2 v3 = outer.polyVertices2D[i3];

				if (PointInTriangle(ref innerCenter, ref v1, ref v2, ref v3))
				{
					return true;
				}
			}

			return false;
		}

		private void CalculateMetodata()
		{
			float volumeTotal = 0;
			for (int i = 0; i < triangles.Count; i += 3)
			{
				int i1 = triangles[i];
				int i2 = triangles[i + 1];
				int i3 = triangles[i + 2];
				Vector2 v1 = polyVertices2D[i1];
				Vector2 v2 = polyVertices2D[i2];
				Vector2 v3 = polyVertices2D[i3];
				float volume = VolumeOfTriangle(v1, v2, v3);
				volumeTotal += volume;
			}
			Volume = volumeTotal;

			Vector2 vAcc = Vector2.zero;
			for (int i = 0; i < polyVertices2D.Length; i++)
			{
				vAcc += polyVertices2D[i];
			}

			Center = vAcc / polyVertices2D.Length;
		}

		private static float VolumeOfTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
		{
			// TODO: replase with simplier implementation
			var m = new Matrix4x4()
			{
				m00 = p1.x, m01 = p2.x, m02 = p3.x, m03 = 0f,
				m10 = p1.y, m11 = p2.y, m12 = p3.y, m13 = 0f,
				m20 = 1f  , m21 = 1f  , m22 = 1f  , m23 = 0f,
				m30 = 1f  , m31 = 1f  , m32 = 1f  , m33 = 1f
			};
			return Mathf.Abs((1f / 6f) * m.determinant);
		}

		/// <summary>
		/// Try to make mesh
		/// </summary>
		/// <param name="right">Clockwise if True</param>
		/// <returns>True if polygon was created</returns>
		private List<int> MakeMesh(bool right)
		{
			var newTriangles = new List<int>(polyVertices2D.Length - 2);
			if (polyVertices2D.Length < 3)
				return newTriangles;

			var linkList = new LinkedLoop<int>();
			for (int i = 0; i < polyVertices2D.Length; ++i)
				linkList.AddLast(i);


			var node = linkList.first;
			int counter = 0;
			while (linkList.size > 2 & counter <= linkList.size)
			{
				var node1 = node;
				var node2 = node1.next;
				var node3 = node2.next;

				var i1 = node1.value;
				var i2 = node2.value;
				var i3 = node3.value;

				++counter;

				bool allowed = IsAllowedToCreateTriangle(linkList, i1, i2, i3, right);

				if (allowed)
				{
					CreateTriangle(newTriangles, i1, i2, i3, right);
					node2.Remove();
					node = node3;
					counter = 0;
				}
				else
					node = node2;
			}

			return newTriangles;
		}

		/// <summary>
		/// Transfom vertices from modal space to plane space and convert them to Vector2[]
		/// </summary>
		private Vector2[] ConvertV3ToV2(IBzSliceAdapter adapter, Vector3 normal)
		{
			normal = adapter.InverseTransformDirection(normal);

			Quaternion rotation;
			if (Vector3.Dot(normal, Vector3.back) < 0f)
			{
				// required to have deterministic texture rotation
				rotation = Quaternion.Euler(180f, 0f, 0f) * Quaternion.FromToRotation(-normal, Vector3.back);
			}
			else
			{
				rotation = Quaternion.FromToRotation(normal, Vector3.back);
			}

			var v2s = new Vector2[edgeLoop.size];

			var edge = edgeLoop.first;
			for (int i = 0; i < edgeLoop.size; i++)
			{
				Vector3 v3 = adapter.GetLocalPos(meshData, edge.value);
				v3 = rotation * v3;
				v2s[i] = v3;

				edge = edge.next;
			}

			return v2s;
		}

		/// <param name="right">Clockwise if True</param>
		private static void CreateTriangle(List<int> triangles, int i1, int i2, int i3, bool right)
		{
			triangles.Add(i1);

			if (right)
			{
				triangles.Add(i2);
				triangles.Add(i3);
			}
			else
			{
				triangles.Add(i3);
				triangles.Add(i2);
			}
		}

		/// <summary>
		/// Check if triangle in right sequence and other points does not in it
		/// </summary>
		/// <param name="right">Clockwise if True</param>
		private bool IsAllowedToCreateTriangle(LinkedLoop<int> linkList, int i1, int i2, int i3, bool right)
		{
			Vector2 v1 = polyVertices2D[i1];
			Vector2 v2 = polyVertices2D[i2];
			Vector2 v3 = polyVertices2D[i3];

			Vector3 vA = v1 - v2;
			Vector3 vB = v3 - v2;
			Vector3 vC = Vector3.Cross(vB, vA);

			if (right & vC.z >= 0)
				return false;

			if (!right & vC.z < 0)
				return false;

			var node = linkList.first;
			int counter = linkList.size;
			while (counter != 0)
			{
				--counter;

				int i = node.value;
				node = node.next;

				if (i == i1 | i == i2 | i == i3)
					continue;

				var p = polyVertices2D[i];
				bool b1 = PointInTriangle(ref p, ref v1, ref v2, ref v3);
				if (b1)
					return false;
			}

			return true;
		}

		/// <summary>
		/// True if point resides inside a triangle
		/// </summary>
		static bool PointInTriangle(ref Vector2 pt, ref Vector2 v1, ref Vector2 v2, ref Vector2 v3)
		{
			float s1 = SideOfLine(ref pt, ref v1, ref v2);
			float s2 = SideOfLine(ref pt, ref v2, ref v3);
			float s3 = SideOfLine(ref pt, ref v3, ref v1);

			return
				(s1 >= 0 & s2 >= 0 & s3 >= 0) |
				(s1 <= 0 & s2 <= 0 & s3 <= 0);
		}

		/// <summary>
		/// It is 0 on the line, and +1 on one side, -1 on the other side.
		/// </summary>
		static float SideOfLine(ref Vector2 p, ref Vector2 a, ref Vector2 b)
		{
			return (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
		}
	}
}