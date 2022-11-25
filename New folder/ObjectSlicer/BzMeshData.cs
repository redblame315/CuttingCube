using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BzKovSoft.ObjectSlicer
{
	public class BzMeshData
	{
		public List<Vector3> Vertices;
		public List<Vector3> Normals;

		public List<Color> Colors;
		public List<Color32> Colors32;

		public List<Vector2> UV;
		public List<Vector2> UV2;
		public List<Vector2> UV3;
		public List<Vector2> UV4;
		public List<Vector4> Tangents;

		public List<BoneWeight> BoneWeights;
		public readonly Matrix4x4[] Bindposes;

		public int[][] SubMeshes;

		public Material[] Materials;

		public bool NormalsExists { get { return Normals != null; } }
		public bool ColorsExists { get { return Colors != null; } }
		public bool Colors32Exists { get { return Colors32 != null; } }
		public bool UVExists { get { return UV != null; } }
		public bool UV2Exists { get { return UV2 != null; } }
		public bool UV3Exists { get { return UV3 != null; } }
		public bool UV4Exists { get { return UV4 != null; } }
		public bool TangentsExists { get { return Tangents != null; } }
		public bool BoneWeightsExists { get { return BoneWeights != null; } }
		public bool MaterialsExists { get { return Materials != null; } }


		public BzMeshData(Mesh initFrom, Material[] materials)
		{
			Materials = materials;
			int vertCount = initFrom.vertexCount / 3;
			Bindposes = initFrom.bindposes;
			if (Bindposes.Length == 0) Bindposes = null;

			Vertices = new List<Vector3>(vertCount);
			Normals = new List<Vector3>(vertCount);
			Colors = new List<Color>();
			Colors32 = new List<Color32>();
			UV = new List<Vector2>(vertCount);
			UV2 = new List<Vector2>();
			UV3 = new List<Vector2>();
			UV4 = new List<Vector2>();
			Tangents = new List<Vector4>();
			BoneWeights = new List<BoneWeight>(Bindposes == null ? 0 : vertCount);

			initFrom.GetVertices(Vertices);
			initFrom.GetNormals(Normals);
			initFrom.GetColors(Colors);
			initFrom.GetColors(Colors32);
			initFrom.GetUVs(0, UV);
			initFrom.GetUVs(1, UV2);
			initFrom.GetUVs(2, UV3);
			initFrom.GetUVs(3, UV4);
			initFrom.GetTangents(Tangents);
			initFrom.GetBoneWeights(BoneWeights);

			SubMeshes = new int[initFrom.subMeshCount][];
			//for (int subMeshIndex = 0; subMeshIndex < initFrom.subMeshCount; ++subMeshIndex)
			//	SubMeshes[subMeshIndex] = initFrom.GetTriangles(subMeshIndex);

			if (Normals.Count == 0)     Normals = null;
			if (Colors.Count == 0)      Colors = null;
			if (Colors32.Count == 0)    Colors32 = null;
			if (UV.Count == 0)          UV = null;
			if (UV2.Count == 0)         UV2 = null;
			if (UV3.Count == 0)         UV3 = null;
			if (UV4.Count == 0)         UV4 = null;
			if (Tangents.Count == 0)    Tangents = null;
			if (BoneWeights.Count == 0) BoneWeights = null;
		}

		public Mesh GenerateMesh()
		{
			Mesh mesh = new Mesh();
			if (Vertices.Count > ushort.MaxValue)
			{
				mesh.indexFormat = IndexFormat.UInt32;
			}

			mesh.SetVertices(Vertices);
			if (NormalsExists)
				mesh.SetNormals(Normals);

			if (ColorsExists)
				mesh.SetColors(Colors);
			if (Colors32Exists)
				mesh.SetColors(Colors32);

			if (UVExists)
				mesh.SetUVs(0, UV);
			if (UV2Exists)
				mesh.SetUVs(1, UV2);
			if (UV3Exists)
				mesh.SetUVs(2, UV3);
			if (UV4Exists)
				mesh.SetUVs(3, UV4);

			if (TangentsExists)
				mesh.SetTangents(Tangents);

			if (BoneWeightsExists)
			{
				mesh.boneWeights = BoneWeights.ToArray();
				mesh.bindposes = Bindposes;
			}

			mesh.subMeshCount = SubMeshes.Length;
			for (int subMeshIndex = 0; subMeshIndex < SubMeshes.Length; ++subMeshIndex)
			{
				mesh.SetTriangles(SubMeshes[subMeshIndex], subMeshIndex);
			}

			return mesh;
		}
	}
}
