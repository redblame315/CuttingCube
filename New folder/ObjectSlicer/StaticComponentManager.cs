using BzKovSoft.ObjectSlicer.MeshGenerator;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Manages the components of an object with static mesh
	/// </summary>
	public class StaticComponentManager : IComponentManager
	{
		protected readonly GameObject _originalObject;
		protected readonly Plane _plane;
		protected readonly ColliderSliceResult[] _colliderResults;

		public bool Success { get { return _colliderResults != null; } }

		/// <summary>
		/// Creates a Static Component Manager.
		/// </summary>
		/// <param name="go">The game object being sliced</param>
		/// <param name="plane">The plane by which the slice will be performed</param>
		/// <param name="colliders">The colliders on the game object being sliced</param>
		public StaticComponentManager(GameObject go, Plane plane, Collider[] colliders)
		{
			_originalObject = go;
			_plane = plane;

			_colliderResults = SliceColliders(plane, colliders);
		}

		public void OnSlicedWorkerThread(SliceTryItem[] items)
		{
			for (int i = 0; i < _colliderResults.Length; i++)
			{
				var collider = _colliderResults[i];

				if (collider.SliceResult == SliceResult.Sliced)
				{
					collider.SliceResult = collider.meshDissector.Slice();
				}
			}
		}

		public void OnSlicedMainThread(GameObject resultObjNeg, GameObject resultObjPos, Renderer[] renderersNeg, Renderer[] renderersPos)
		{
			var cldrsA = new List<Collider>();
			var cldrsB = new List<Collider>();
			RepairColliders(resultObjNeg, resultObjPos, cldrsA, cldrsB);
		}

		protected void RepairColliders(GameObject resultNeg, GameObject resultPos,
			List<Collider> collidersNeg, List<Collider> collidersPos)
		{
			Profiler.BeginSample("RepairColliders");
			var lazyRunnerNeg = resultNeg.GetComponent<LazyActionRunner>();
			var lazyRunnerPos = resultPos.GetComponent<LazyActionRunner>();

			for (int i = 0; i < _colliderResults.Length; i++)
			{
				var collider = _colliderResults[i];

				Collider colliderNeg = BzSlicerHelper.GetSameComponentForDuplicate(collider.OriginalCollider, _originalObject, resultNeg);
				Collider colliderPos = BzSlicerHelper.GetSameComponentForDuplicate(collider.OriginalCollider, _originalObject, resultPos);
				var goNeg = colliderNeg.gameObject;
				var goPos = colliderPos.gameObject;

				if (collider.SliceResult == SliceResult.Sliced)
				{
					Mesh resultMeshNeg = collider.meshDissector.SliceResultNeg.GenerateMesh();
					Mesh resultMeshPos = collider.meshDissector.SliceResultPos.GenerateMesh();

					var SlicedColliderNeg = new MeshColliderConf(resultMeshNeg, collider.OriginalCollider.material);
					var SlicedColliderPos = new MeshColliderConf(resultMeshPos, collider.OriginalCollider.material);

					Physics.IgnoreCollision(colliderNeg, colliderPos, true);
					var actionNeg = new Action(() =>
					{
						AddCollider(SlicedColliderNeg, goNeg, colliderNeg);
					});
					var actionPos = new Action(() =>
					{
						AddCollider(SlicedColliderPos, goPos, colliderPos);
					});

					if (lazyRunnerNeg != null)
						lazyRunnerNeg.AddLazyAction(actionNeg);
					else
						actionNeg();

					if (lazyRunnerPos != null)
						lazyRunnerPos.AddLazyAction(actionPos);
					else
						actionPos();

				}

				if (collider.SliceResult == SliceResult.Sliced)
				{
					collidersNeg.Add(colliderNeg);
					collidersPos.Add(colliderPos);
				}
				else if (collider.SliceResult == SliceResult.Neg)
				{
					collidersNeg.Add(colliderNeg);
					UnityEngine.Object.Destroy(colliderPos);
				}
				else if (collider.SliceResult == SliceResult.Pos)
				{
					collidersPos.Add(colliderPos);
					UnityEngine.Object.Destroy(colliderNeg);
				}
				else
					throw new InvalidOperationException();
			}
			Profiler.EndSample();
		}

		private static void AddCollider(MeshColliderConf colliderConf, GameObject go, Collider originalCollider)
		{
			Profiler.BeginSample("Action: AddCollider");

			UnityEngine.Object.Destroy(originalCollider);
			var collider = go.AddComponent<MeshCollider>();
			collider.sharedMaterial = colliderConf.Material;
			collider.convex = true;
			collider.sharedMesh = colliderConf.Mesh;

			Profiler.EndSample();
		}

		private static ColliderSliceResult[] SliceColliders(Plane plane, Collider[] colliders)
		{
			ColliderSliceResult[] results = new ColliderSliceResult[colliders.Length];
			bool ColliderExistsNeg = false;
			bool ColliderExistsPos = false;

			for (int i = 0; i < colliders.Length; i++)
			{
				var collider = colliders[i];

				var colliderB = collider as BoxCollider;
				var colliderS = collider as SphereCollider;
				var colliderC = collider as CapsuleCollider;
				var colliderM = collider as MeshCollider;

				ColliderSliceResult result;
				if (colliderB != null)
				{
					var mesh = Cube.Create(colliderB.size, colliderB.center);
					result = PrepareSliceCollider(collider, mesh, plane);
				}
				else if (colliderS != null)
				{
					var mesh = IcoSphere.Create(colliderS.radius, colliderS.center);
					result = PrepareSliceCollider(collider, mesh, plane);
				}
				else if (colliderC != null)
				{
					var mesh = Capsule.Create(colliderC.radius, colliderC.height, colliderC.direction, colliderC.center);
					result = PrepareSliceCollider(collider, mesh, plane);
				}
				else if (colliderM != null)
				{
					Mesh mesh = UnityEngine.Object.Instantiate(colliderM.sharedMesh);
					result = PrepareSliceCollider(collider, mesh, plane);
				}
				else
					throw new NotSupportedException("Not supported collider type '" + collider.GetType().Name + "'");

				ColliderExistsNeg |= result.SliceResult == SliceResult.Sliced | result.SliceResult == SliceResult.Neg;
				ColliderExistsPos |= result.SliceResult == SliceResult.Sliced | result.SliceResult == SliceResult.Pos;
				results[i] = result;
			}

			bool sliced = ColliderExistsNeg & ColliderExistsPos;
			return sliced ? results : null;
		}

		protected static ColliderSliceResult PrepareSliceCollider(Collider collider, Mesh mesh, Plane plane)
		{
			var result = new ColliderSliceResult();
			IBzSliceAdapter adapter = new BzSliceColliderAdapter(mesh.vertices, collider.gameObject);
			SliceConfigurationDto conf = BzSliceConfiguration.GetDefault();
			BzMeshDataDissector meshDissector = new BzMeshDataDissector(mesh, plane, null, adapter, conf);

			result.SliceResult = SliceResult.Sliced;
			result.OriginalCollider = collider;
			result.meshDissector = meshDissector;

			return result;
		}

		protected class ColliderSliceResult
		{
			public Collider OriginalCollider;
			public BzMeshDataDissector meshDissector;
			public SliceResult SliceResult;
		}

		protected class MeshColliderConf
		{
			public MeshColliderConf(Mesh mesh, PhysicMaterial material)
			{
				Mesh = mesh;
				Material = material;
			}
			public readonly Mesh Mesh;
			public readonly PhysicMaterial Material;
		}
	}
}
