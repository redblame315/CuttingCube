using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.CharacterSlicer
{
	public static class CenterOfMassColliderBasedHelper
	{
		public static bool CalculateCenter(Rigidbody rigidbody, List<Collider> possibleColliders, Rigidbody[] possibleRigids)
		{
			var colliders = GetColliders(rigidbody.transform, possibleColliders, possibleRigids);

			if (colliders.Count == 0)
			{
				rigidbody.mass = 0.001f;
				return false;
			}

			Vector3 origin = rigidbody.transform.position;
			Vector3 v = Vector3.zero;

			for (int i = 0; i < colliders.Count; i++)
			{
				var collider = colliders[i];
				Vector3 center = GetColliderCenter(collider);
				center = collider.transform.TransformPoint(center) - origin;
				v += center;
			}

			v /= colliders.Count;

			rigidbody.centerOfMass = rigidbody.transform.InverseTransformDirection(v);

			return true;
		}

		private static Vector3 GetColliderCenter(Collider collider)
		{
			BoxCollider bCldr = collider as BoxCollider;
			SphereCollider sCldr = collider as SphereCollider;
			CapsuleCollider cCldr = collider as CapsuleCollider;
			MeshCollider mCldr = collider as MeshCollider;

			if (!object.ReferenceEquals(bCldr, null))
			{
				return bCldr.center;
			}

			if (!object.ReferenceEquals(sCldr, null))
			{
				return sCldr.center;
			}

			if (!object.ReferenceEquals(cCldr, null))
			{
				return cCldr.center;
			}

			if (!object.ReferenceEquals(mCldr, null))
			{
				return mCldr.sharedMesh.bounds.center;
			}

			UnityEngine.Debug.LogError("Collider type '" + collider.GetType().Name + "' not supported");
			return Vector3.zero;
		}

		private static List<Collider> GetColliders(Transform transform, List<Collider> possibleColliders, Rigidbody[] possibleRigids)
		{
			List<Collider> colliders = new List<Collider>();

			GetCollidersRec(transform, colliders, possibleColliders, possibleRigids);

			return colliders;
		}

		private static void GetCollidersRec(Transform transform, List<Collider> colliders, List<Collider> possibleColliders, Rigidbody[] possibleRigids)
		{
			var currentColliders = transform.GetComponents<Collider>();

			for (int i = 0; i < currentColliders.Length; i++)
			{
				var cldr = currentColliders[i];
				if (possibleColliders.Contains(cldr))
				{
					colliders.Add(cldr);
				}
			}

			for (int i = 0; i < transform.childCount; i++)
			{
				var child = transform.GetChild(i);
				if (GetComponent<Rigidbody>(child, possibleRigids) != null)
				{
					continue;
				}

				GetCollidersRec(child, colliders, possibleColliders, possibleRigids);
			}
		}

		private static T GetComponent<T>(Transform child, T[] possibleComponents)
			where T : Component
		{
			var components = child.GetComponents<T>();

			if (components == null)
				return null;

			for (int i = 0; i < components.Length; i++)
			{
				var component = components[i];

				for (int j = 0; j < possibleComponents.Length; j++)
				{
					if (possibleComponents[j] == component)
						return component;
				}
			}

			return null;
		}
	}
}
