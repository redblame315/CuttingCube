using BzKovSoft.ObjectSlicer.Polygon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace BzKovSoft.ObjectSlicer
{
	public class BzMeshDataDissector
	{
		const float MinWidth = 0.001f;
		readonly IBzSliceAdapter _adapter;
		Plane _plane;
		Material _defaultSliceMaterial;
		bool _sliced = false;

		BzMeshData _meshDataNeg;
		BzMeshData _meshDataPos;
		int[][] _subMeshes;

		public Material DefaultSliceMaterial
		{
			get { return _defaultSliceMaterial; }
			set { _defaultSliceMaterial = value; }
		}
		public IBzSliceAdapter Adapter { get { return _adapter; } }
		public BzMeshData SliceResultNeg { get { return _meshDataNeg; } }
		public BzMeshData SliceResultPos { get { return _meshDataPos; } }
		public SliceConfigurationDto Configuration { get; private set; }
		public List<PolyMeshData> CapsNeg { get; private set; }
		public List<PolyMeshData> CapsPos { get; private set; }

		public BzMeshDataDissector(Mesh mesh, Plane plane, Material[] materials, IBzSliceAdapter adapter, SliceConfigurationDto configuration)
		{
			_adapter = adapter;
			_plane = plane;
			Configuration = configuration;

			_meshDataNeg = new BzMeshData(mesh, materials);
			_meshDataPos = new BzMeshData(mesh, materials);

			_subMeshes = new int[mesh.subMeshCount][];
			for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; ++subMeshIndex)
			{
				_subMeshes[subMeshIndex] = mesh.GetTriangles(subMeshIndex);
			}
		}

		public SliceResult Slice()
		{
			if (_sliced)
				throw new InvalidOperationException("Object already sliced");

			_sliced = true;

			switch (Configuration.SliceType)
			{
				case SliceType.Slice:
					return SliceMesh(Configuration.SliceMaterial ?? _defaultSliceMaterial);

				case SliceType.KeepOne:
					return _plane.GetSide(_adapter.GetObjectCenterInWorldSpace()) ?
						SliceResult.Pos : SliceResult.Neg;

				case SliceType.Duplicate:
					return SliceResult.Duplicate;

				default: throw new NotSupportedException();
			}
		}

		private SliceResult SliceMesh(Material sectionViewMaterial)
		{
			Profiler.BeginSample("SliceMesh");
			var planeInverted = new Plane(-_plane.normal, -_plane.distance);

			bool skipIfNotClosed = Configuration.SkipIfNotClosed;

			BzMeshDataEditor meshEditorNeg = new BzMeshDataEditor(_meshDataNeg, _plane, _adapter, skipIfNotClosed);
			BzMeshDataEditor meshEditorPos = new BzMeshDataEditor(_meshDataPos, planeInverted, _adapter, skipIfNotClosed);

			for (int subMeshIndex = 0; subMeshIndex < _subMeshes.Length; ++subMeshIndex)
			{
				int[] newTriangles = _subMeshes[subMeshIndex];

				int trCount = newTriangles.Length / 3;
				var trianglesNeg = new List<BzTriangle>(trCount);
				var trianglesPos = new List<BzTriangle>(trCount);
				var trianglesNegSliced = new List<BzTriangle>(trCount / 10);
				var trianglesPosSliced = new List<BzTriangle>(trCount / 10);

				for (int i = 0; i < trCount; ++i)
				{
					int trIndex = i * 3;
					var bzTriangle = new BzTriangle(
						newTriangles[trIndex + 0],
						newTriangles[trIndex + 1],
						newTriangles[trIndex + 2]);

					Vector3 v1 = _adapter.GetWorldPos(bzTriangle.i1);
					Vector3 v2 = _adapter.GetWorldPos(bzTriangle.i2);
					Vector3 v3 = _adapter.GetWorldPos(bzTriangle.i3);
					bool side1 = _plane.GetSide(v1);
					bool side2 = _plane.GetSide(v2);
					bool side3 = _plane.GetSide(v3);
					bool PosSide = side1 | side2 | side3;
					bool NegSide = !side1 | !side2 | !side3;

					if (NegSide & PosSide)
					{
						bzTriangle.DivideByPlane(
							meshEditorNeg, meshEditorPos,
							trianglesNegSliced, trianglesPosSliced,
							side1, side2, side3);
					}
					else if (NegSide)
					{
						trianglesNeg.Add(bzTriangle);
					}
					else if (PosSide)
					{
						trianglesPos.Add(bzTriangle);
					}
					else
						throw new InvalidOperationException();
				}

				MeshTriangleOptimizer.OptimizeEdgeTriangles(meshEditorNeg.GetEdgeLoopsByIndex(), _meshDataNeg, trianglesNegSliced);
				MeshTriangleOptimizer.OptimizeEdgeTriangles(meshEditorPos.GetEdgeLoopsByIndex(), _meshDataPos, trianglesPosSliced);
				_meshDataNeg.SubMeshes[subMeshIndex] = MakeTriangleToList(trianglesNeg, trianglesNegSliced);
				_meshDataPos.SubMeshes[subMeshIndex] = MakeTriangleToList(trianglesPos, trianglesPosSliced);
			}

			if (Configuration.CreateCap)
			{
				CapsNeg = meshEditorNeg.CapSlice(sectionViewMaterial);
				CapsPos = meshEditorPos.CapSlice(sectionViewMaterial);
			}

			meshEditorNeg.DeleteUnusedVertices();
			meshEditorPos.DeleteUnusedVertices();

			Profiler.EndSample();

			if (!CheckNewMesh(_meshDataNeg))
			{
				return SliceResult.Pos;
			}
			if (!CheckNewMesh(_meshDataPos))
			{
				return SliceResult.Neg;
			}

			return SliceResult.Sliced;
		}

		public void RebuildNegMesh(Renderer meshRenderer)
		{
			var mesh = _meshDataNeg.GenerateMesh();
			_adapter.RebuildMesh(mesh, _meshDataNeg.Materials, meshRenderer);
		}

		public void RebuildPosMesh(Renderer meshRenderer)
		{
			var mesh = _meshDataPos.GenerateMesh();
			_adapter.RebuildMesh(mesh, _meshDataPos.Materials, meshRenderer);
		}

		private bool CheckNewMesh(BzMeshData meshData)
		{
			if (meshData.SubMeshes.All(s => s.Length == 0))
				return false;

			if (!CheckMinWidth(meshData))
				return false;

			return _adapter.Check(meshData);
		}

		private bool CheckMinWidth(BzMeshData meshData)
		{
			if (meshData.Vertices.Count < 3)
				return false;

			for (int si = 0; si < meshData.SubMeshes.Length; si++)
			{
				var subMesh = meshData.SubMeshes[si];
				for (int i = 0; i < subMesh.Length; i++)
				{
						var pos = _adapter.GetWorldPos(meshData, subMesh[i]);
					float dist = _plane.GetDistanceToPoint(pos);
					if (Math.Abs(dist) > MinWidth)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Get mesh triangle list from BzTriangle list
		/// </summary>
		static int[] MakeTriangleToList(List<BzTriangle> bzTriangles, List<BzTriangle> bzTrianglesExtra)
		{
			int[] triangles = new int[(bzTriangles.Count + bzTrianglesExtra.Count) * 3];
			for (int i = 0; i < bzTriangles.Count; ++i)
			{
				var tr = bzTriangles[i];
				int shift = i * 3;
				triangles[shift + 0] = tr.i1;
				triangles[shift + 1] = tr.i2;
				triangles[shift + 2] = tr.i3;
			}

			for (int i = 0; i < bzTrianglesExtra.Count; ++i)
			{
				var tr = bzTrianglesExtra[i];
				int shift = (bzTriangles.Count + i) * 3;
				triangles[shift + 0] = tr.i1;
				triangles[shift + 1] = tr.i2;
				triangles[shift + 2] = tr.i3;
			}

			return triangles;
		}
	}
}
