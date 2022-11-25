using System.Collections;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	/// <summary>
	/// After the slice some collider could intersect each other and jump apart. This script designed to avoid this effect 
	/// </summary>
	[DisallowMultipleComponent]
	class BzSmoothDepenetration : MonoBehaviour, IBzObjectSlicedEvent
	{
		public void ObjectSliced(GameObject original, GameObject resultNeg, GameObject resultPos)
		{
			StartCoroutine(SmoothDepenetration(resultNeg));
			StartCoroutine(SmoothDepenetration(resultPos));
		}

		public static IEnumerator SmoothDepenetration(GameObject go)
		{
			var rigids = go.GetComponentsInChildren<Rigidbody>();
			var maxVelocitys = new float[rigids.Length];
			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];
				maxVelocitys[i] = rigid.maxDepenetrationVelocity;
				rigid.maxDepenetrationVelocity = 0.1f;
			}

			yield return new WaitForSeconds(1);

			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];
				if (rigid == null)
					continue;

				float maxVel = maxVelocitys[i];
				rigid.maxDepenetrationVelocity = maxVel;
			}
		}
	}
}
