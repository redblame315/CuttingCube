using System;
using System.Collections;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	/// <summary>
	/// Fixes weight and center of the mass of sliced objects. It works correct only for closed objects.
	/// </summary>
	[DisallowMultipleComponent]
	class BzFixMassSmart : MonoBehaviour, IBzObjectSlicedEvent
	{
		public void ObjectSliced(GameObject original, GameObject resultNeg, GameObject resultPos)
		{
			// we need to wait one fram to allow destroyed component to be destroyed.
			StartCoroutine(NextFrame(original, resultNeg, resultPos));
		}

		IEnumerator NextFrame(GameObject original, GameObject resultNeg, GameObject resultPos)
		{
			//returning null will make it wait 1 frame
			yield return null;

			Rigidbody rigidO = original.GetComponent<Rigidbody>();

			Rigidbody rigidA = resultNeg.GetComponent<Rigidbody>();
			Rigidbody rigidB = resultPos.GetComponent<Rigidbody>();

			float massO = rigidO.mass;
			Vector3 centerOfMassA;
			Vector3 centerOfMassB;

			float volumeA = VolumeOfMesh(resultNeg, out centerOfMassA);
			float volumeB = VolumeOfMesh(resultPos, out centerOfMassB);
			float volume = volumeA + volumeB;
			rigidA.mass = massO * (volume - volumeB) / volume;
			rigidB.mass = massO * (volume - volumeA) / volume;

			var lossyScale = original.transform.lossyScale;
			rigidA.centerOfMass = Vector3.Scale(centerOfMassA, lossyScale);
			rigidB.centerOfMass = Vector3.Scale(centerOfMassB, lossyScale);
		}

		/// <summary>
		/// Draw center of mass
		/// </summary>
		void OnDrawGizmosSelected()
		{
			Rigidbody rigid = this.GetComponent<Rigidbody>();
			if (rigid == null)
				return;

			Vector3 pos = this.transform.position + this.transform.TransformDirection(rigid.centerOfMass);
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(pos, 0.1f);
		}

		private static float VolumeOfMesh(GameObject gameObject, out Vector3 center)
		{
			MeshFilter[] filters = gameObject.GetComponentsInChildren<MeshFilter>();

			center = Vector3.zero;
			float volTotal = 0;
			int activeComponentCount = 0;

			for (int i = 0; i < filters.Length; i++)
			{
				var meshFilter = filters[i];

				if (meshFilter == null)
					continue;

				++activeComponentCount;
				var mesh = meshFilter.sharedMesh;

				Vector3 meshCenter;
				Vector3 scale = meshFilter.transform.lossyScale;
				float vol = VolumeOfMesh(mesh, scale.x * scale.y * scale.z, out meshCenter);

				var tmpCenter = meshFilter.gameObject.transform.TransformPoint(meshCenter);
				meshCenter = gameObject.transform.InverseTransformPoint(tmpCenter);

				center += meshCenter;
				volTotal += vol;
			}

			center = center / activeComponentCount;

			return volTotal;
		}

		private static float VolumeOfMesh(Mesh mesh, float scale, out Vector3 center)
		{
			center = Vector3.zero;

			var triangles = mesh.triangles;
			var vertices = mesh.vertices;
			float volTotal = 0;

			for (int i = 0; i < triangles.Length; i = i + 3)
			{
				var v1 = vertices[triangles[i + 0]];
				var v2 = vertices[triangles[i + 1]];
				var v3 = vertices[triangles[i + 2]];

				float vol = SignedVolumeOfTriangle(v1, v2, v3) * scale;
				volTotal += vol;

				Vector3 trCenter = GetTetrahedronCenter(v1, v2, v3);
				center += trCenter * vol;
			}

			center = center / volTotal;

			return Math.Abs(volTotal);
		}

		private static Vector3 GetTetrahedronCenter(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			return (v1 + v2 + v3) / 4f;
		}

		private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			var m = new Matrix4x4()
			{
				m00 = p1.x, m01 = p2.x, m02 = p3.x, m03 = 0f,
				m10 = p1.y, m11 = p2.y, m12 = p3.y, m13 = 0f,
				m20 = p1.z, m21 = p2.z, m22 = p3.z, m23 = 0f,
				m30 = 1f  , m31 = 1f  , m32 = 1f  , m33 = 1f
			};
			return (1f / 6f) * m.determinant;
		}
	}
}
