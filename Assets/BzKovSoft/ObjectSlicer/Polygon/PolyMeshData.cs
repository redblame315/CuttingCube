using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Polygon
{
	public class PolyMeshData
	{
		public Vector3[] vertices;
		public Vector3[] normals;
		public Vector2[] uv;
		public int[] triangles;
		public BoneWeight[] boneWeights;
		public Vector4[] tangents;
	}
}