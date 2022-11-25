using BzKovSoft.ObjectSlicer;
using UnityEngine;

namespace BzKovSoft.CharacterSlicer
{
	/// <summary>
	/// Base class for sliceable character
	/// </summary>
	public abstract class BzSliceableCharacterBase : BzSliceableBase
	{
		private void Awake()
		{
			var animator = GetComponent<Animator>();
			if (animator != null && animator.updateMode != AnimatorUpdateMode.AnimatePhysics)
				UnityEngine.Debug.LogWarning("Recomended to use Animator.UpdateMode = AnimatePhysics for your sliceable character");
		}

		protected override AdapterAndMesh GetAdapterAndMesh(Renderer renderer)
		{
			var skinnedRenderer = renderer as SkinnedMeshRenderer;
			if (skinnedRenderer != null)
			{
				var result = new AdapterAndMesh();
				result.mesh = skinnedRenderer.sharedMesh;
				result.adapter = new BzSliceSkinnedMeshAdapter(skinnedRenderer);
				return result;
			}

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

		protected override BzSliceTryData PrepareData(Plane plane)
		{
			// collider we want to participate in slicing
			var collidersArr = GetComponentsInChildren<Collider>();

			// create component manager.
			var componentManager = new CharacterComponentManagerFast(this.gameObject, plane, collidersArr);

			return new BzSliceTryData()
			{
				componentManager = componentManager,
				plane = plane,
			};
		}
	}
}