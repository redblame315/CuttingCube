using System;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	class BzSliceColliderAdapter : IBzSliceAdapter
	{
		Matrix4x4 _ltw;
		Matrix4x4 _wtl;
		Vector3[] _vertices;

		public BzSliceColliderAdapter(Vector3[] vertices, GameObject gameObject)
		{
			_vertices = vertices;
			_ltw = gameObject.transform.localToWorldMatrix;
			_wtl = gameObject.transform.worldToLocalMatrix;
		}

		public Vector3 GetWorldPos(int index)
		{
			Vector3 position = _vertices[index];
			return _ltw.MultiplyPoint3x4(position);
		}

		public Vector3 GetLocalPos(BzMeshData meshData, int index)
		{
			return meshData.Vertices[index];
		}

		public Vector3 GetWorldPos(BzMeshData meshData, int index)
		{
			return _ltw.MultiplyPoint3x4(meshData.Vertices[index]);
		}

		public Vector3 InverseTransformDirection(Vector3 p)
		{
			return _wtl.MultiplyPoint3x4(p);
		}

		public bool Check(BzMeshData meshData)
		{
#if DEBUG
			int trCount = 0;
			for (int i = 0; i < meshData.SubMeshes.Length; i++)
			{
				trCount += meshData.SubMeshes[i].Length;
			}

			if (trCount < 3)
				throw new Exception("FFFFF3");

			if (trCount % 3 != 0)
				throw new Exception("FFFFF4");
#endif

			return true;
		}

		public void RebuildMesh(Mesh mesh, Material[] materials, Renderer meshRenderer)
		{
			throw new NotSupportedException();
		}

		public Vector3 GetObjectCenterInWorldSpace()
		{
			return _ltw.MultiplyPoint3x4(Vector3.zero);
		}
	}
}