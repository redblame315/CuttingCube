using System;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	public class BzManualMeshAdapter : IBzSliceAdapter
	{
		Vector3[] _vertices;

		public BzManualMeshAdapter(Vector3[] vertices)
		{
			_vertices = vertices;
		}

		public Vector3 GetWorldPos(int index)
		{
			Vector3 position = _vertices[index];
			return position;
		}

		public Vector3 GetLocalPos(BzMeshData meshData, int index)
		{
			return meshData.Vertices[index];
		}

		public Vector3 GetWorldPos(BzMeshData meshData, int index)
		{
			return meshData.Vertices[index];
		}

		public Vector3 InverseTransformDirection(Vector3 p)
		{
			return p;
		}

		public bool Check(BzMeshData meshData)
		{
			return true;
		}

		public void RebuildMesh(Mesh mesh, Material[] materials, Renderer meshRenderer)
		{
			throw new NotSupportedException();
		}

		public Vector3 GetObjectCenterInWorldSpace()
		{
			return Vector3.zero;
		}
	}
}