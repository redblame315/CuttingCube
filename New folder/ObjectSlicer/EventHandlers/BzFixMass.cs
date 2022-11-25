using System.Collections;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	/// <summary>
	/// Fixes weight and center of the mass of sliced objects.
	/// </summary>
	[DisallowMultipleComponent]
	public class BzFixMass : MonoBehaviour, IBzObjectSlicedEvent
	{
		public void ObjectSliced(GameObject original, GameObject resultNeg, GameObject resultPos)
		{
			// we need to wait one fram to allow destroyed component to be destroyed.
			StartCoroutine(NextFrame(resultNeg, resultPos));
		}

		IEnumerator NextFrame(GameObject resultNeg, GameObject resultPos)
		{
			//returning null will make it wait 1 frame
			yield return null;

			Mesh meshA = resultNeg.GetComponent<MeshFilter>().sharedMesh;
			Mesh meshB = resultPos.GetComponent<MeshFilter>().sharedMesh;
			Vector3 sizeAv = meshA.bounds.size;
			Vector3 sizeBv = meshB.bounds.size;

			float sizeRateA = sizeAv.x * sizeAv.y * sizeAv.z;
			float sizeRateB = sizeBv.x * sizeBv.y * sizeBv.z;
			sizeRateA = sizeRateA / (sizeRateA + sizeRateB);
			sizeRateB = 1f - sizeRateA;

			Rigidbody rigidA = resultNeg.GetComponent<Rigidbody>();
			Rigidbody rigidB = resultPos.GetComponent<Rigidbody>();
			rigidA.mass = rigidA.mass * sizeRateA;
			rigidB.mass = rigidB.mass * sizeRateB;

			rigidA.centerOfMass = CalculateCenterOfMass(meshA);
			rigidB.centerOfMass = CalculateCenterOfMass(meshB);
			rigidA.centerOfMass = Vector3.Scale(rigidA.centerOfMass, resultNeg.transform.localScale);
			rigidB.centerOfMass = Vector3.Scale(rigidB.centerOfMass, resultPos.transform.localScale);
		}

		/// <summary>
		/// Draw center of mass
		/// </summary>
		//void OnDrawGizmosSelected()
		//{
		//	Rigidbody rigid = this.GetComponent<Rigidbody>();
		//	if (rigid == null)
		//		return;
		//
		//	Vector3 pos = this.transform.position + this.transform.TransformDirection(rigid.centerOfMass);
		//	Gizmos.color = Color.yellow;
		//	Gizmos.DrawSphere(pos, 0.1f);
		//}

		private Vector3 CalculateCenterOfMass(Mesh mesh)
		{
			var vertices = mesh.vertices;
			if (vertices.Length == 0)
				return Vector3.zero;

			Vector3 result = vertices[0];
			for (int i = 1; i < vertices.Length; i++)
			{
				result = (result + vertices[i]);
			}
			// not correct enough

			return result / vertices.Length;
		}
	}
}
