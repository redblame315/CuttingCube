using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Base class for sliceable object
	/// </summary>
	public abstract class BzSliceableObjectBase : BzSliceableBase
	{
		protected override AdapterAndMesh GetAdapterAndMesh(Renderer renderer)
		{
			var meshRenderer = renderer as MeshRenderer;

			if (meshRenderer != null)
			{
				var result = new AdapterAndMesh();
				result.mesh = meshRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh;
				result.adapter = new BzSliceMeshFilterAdapter(result.mesh.vertices, meshRenderer);
				return result;
			}

			return null;
		}
	}
}