using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	public interface IBzSliceAdapter
	{
		Vector3 GetWorldPos(int index);
		Vector3 GetLocalPos(BzMeshData meshData, int index);
		Vector3 GetWorldPos(BzMeshData meshData, int index);
		Vector3 InverseTransformDirection(Vector3 p);
		bool Check(BzMeshData meshData);
		void RebuildMesh(Mesh mesh, Material[] materials, Renderer meshRenderer);
		Vector3 GetObjectCenterInWorldSpace();
	}
}