using BzKovSoft.ObjectSlicer.Polygon;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Slicer result
	/// </summary>
	public class BzSliceTryResult
	{
		public BzSliceTryResult(bool sliced, object addData)
		{
			this.sliced = sliced;
			this.addData = addData;
		}

		public BzMeshSliceResult[] meshItems;
		public readonly bool sliced;
		public readonly object addData;
		public GameObject outObjectNeg;
		public GameObject outObjectPos;
	}

	public class BzMeshSliceResult
	{
		public BzSliceEdgeResult[] sliceEdgesNeg;
		public Renderer rendererNeg;

		public BzSliceEdgeResult[] sliceEdgesPos;
		public Renderer rendererPos;
	}

	/// <summary>
	/// Slice edge details
	/// </summary>
	public class BzSliceEdgeResult
	{
		public Vector3[] vertices;
		public Vector3[] normals;
		public BoneWeight[] boneWeights;
	}
}