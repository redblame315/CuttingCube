using BzKovSoft.ObjectSlicer;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace BzKovSoft.CharacterSlicer
{
	public class CharacterComponentManagerFast : StaticComponentManager, IComponentManager
	{
		/// <summary>
		/// Creates a Component Manager for skinned mesh renderers.
		/// </summary>
		/// <param name="go">The game object being sliced</param>
		/// <param name="plane">The plane by which the slice will be performed</param>
		/// <param name="colliders">The colliders on the game object being sliced</param>
		public CharacterComponentManagerFast(GameObject go, Plane plane, Collider[] colliders)
			: base(go, plane, colliders)
		{
		}

		public new void OnSlicedMainThread(GameObject resultObjNeg, GameObject resultObjPos, Renderer[] renderersNeg, Renderer[] renderersPos)
		{
			var cldrsL = new List<Collider>();
			var cldrsR = new List<Collider>();

			RepairColliders(resultObjNeg, resultObjPos, cldrsL, cldrsR);

			OnCompletePerSide(cldrsL, resultObjNeg);
			OnCompletePerSide(cldrsR, resultObjPos);
		}

		private static void OnCompletePerSide(List<Collider> colliders, GameObject go)
		{
			Profiler.BeginSample("OnCompletePerSide");
			Rigidbody[] rigids = go.GetComponentsInChildren<Rigidbody>();
			List<Rigidbody> rigidsInside = new List<Rigidbody>();

			// ----------------------
			// prepare rigidsInside
			Profiler.BeginSample("prepare rigidsInside");
			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];

				bool haveColliders = RigidHaveColliders(rigid.transform, colliders);

				if (haveColliders)
				{
					rigidsInside.Add(rigid);
				}
			}
			Profiler.EndSample();

			// ----------------------
			// remove joints
			Profiler.BeginSample("remove joints");
			var freeEnds = new List<Rigidbody>();
			List<Joint> joints = new List<Joint>();
			go.GetComponentsInChildren<Joint>(false, joints);

			for (int i = 0; i < joints.Count; i++)
			{
				var joint = joints[i];

				if (joint.connectedBody == null)
				{
					continue;
					//throw new InvalidOperationException("Object " + joint.gameObject.name + " have a joint with empty connected body");
				}

				var rigidFrom = joint.GetComponent<Rigidbody>();
				var rigidTo = joint.connectedBody.GetComponent<Rigidbody>();

				var insideFrom = rigidsInside.Contains(rigidFrom);
				var insideTo = rigidsInside.Contains(rigidTo);

				if (!insideFrom | !insideTo)
				{
					if (insideFrom && !freeEnds.Contains(rigidFrom))
					{
						freeEnds.Add(rigidFrom);
					}

					if (insideTo && !freeEnds.Contains(rigidTo))
					{
						freeEnds.Add(rigidTo);
					}

					UnityEngine.Object.Destroy(joint);

					joints[i] = null; // mark as deleted
				}
			}
			Profiler.EndSample();

			// ----------------------
			// join parts
			Profiler.BeginSample("join parts");

			// find main (central) rigid
			Profiler.BeginSample("find main (central) rigid");
			int rigidIndex = 0;
			int subRigidCount = 0;
			for (int i = 0; i < freeEnds.Count; i++)
			{
				var part = freeEnds[i];
				int count = part.GetComponentsInChildren<Rigidbody>().Length;

				if (count > subRigidCount)
				{
					subRigidCount = count;
					rigidIndex = i;
				}
			}
			Profiler.EndSample();

			// connect
			Profiler.BeginSample("connect");
			Transform main = null;
			if (freeEnds.Count > 0)
			{
				main = freeEnds[rigidIndex].transform;

				for (int i = 0; i < freeEnds.Count; i++)
				{
					if (i == rigidIndex)
					{
						continue;
					}

					var part = freeEnds[i].transform;

					Profiler.BeginSample("IsAlreadyConnected");
					if (IsAlreadyConnected(part, main, joints, new HashSet<Transform>()))
					{
						Profiler.EndSample();
						continue;
					}
					Profiler.EndSample();

					Joint newJoint = CreateJoint(part, main);
					joints.Add(newJoint);
				}
			}
			Profiler.EndSample();
			Profiler.EndSample();

			// ----------------------
			// rearrange objects
			Profiler.BeginSample("rearrange objects");
			Transform oldRoot;
			oldRoot = go.transform.Find("rootChrSlr");
			if (oldRoot == null)
			{
				var animator = go.GetComponent<Animator>();
				oldRoot = animator.GetBoneTransform(HumanBodyBones.Hips);

				if (oldRoot == null)
				{
					for (int i = 0; i < go.transform.childCount; i++)
					{
						var rigid = go.transform.GetChild(i).GetComponent<Rigidbody>();
						if (rigid == null)
						{
							continue;
						}

						if (oldRoot != null)
						{
							throw new InvalidOperationException("Cannot find root object. Several objects with rigidbody was found");
						}

						oldRoot = rigid.transform;
					}
				}

				if (oldRoot == null)
				{
					throw new InvalidOperationException("No root with rigidbody found");
				}
			}

			var newRoot = new GameObject("rootChrSlr").transform;
			newRoot.SetParent(go.transform, false);
			for (int i = 0; i < freeEnds.Count; i++)
			{
				var freeEnd = freeEnds[i];
				freeEnd.transform.SetParent(newRoot, true);
			}

			if (main != null)
			{
				oldRoot.SetParent(main, true);
			}
			else
			{
				oldRoot.SetParent(newRoot, true);
			}
			Profiler.EndSample();

			// ----------------------
			// delete rigidbodies
			Profiler.BeginSample("delete rigidbodies");
			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];

				if (!rigidsInside.Contains(rigid))
				{
					UnityEngine.Object.Destroy(rigid);
					rigids[i] = null; // mark as deleted
				}
			}
			Profiler.EndSample();

			// ----------------------
			// set center of mass and weight
			Profiler.BeginSample("set center of mass and weight");
			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];
				if (rigid == null)
					continue;

				CenterOfMassColliderBasedHelper.CalculateCenter(rigid, colliders, rigids);
			}
			Profiler.EndSample();

			Profiler.EndSample();
		}

		private static bool IsAlreadyConnected(Transform from, Transform to, List<Joint> joints, HashSet<Transform> ocupied)
		{
			List<Transform> connectedItems = new List<Transform>();

			for (int i = 0; i < joints.Count; ++i)
			{
				var joint = joints[i];

				if (joint == null)
					continue;

				if (joint.transform == from)
				{
					connectedItems.Add(joint.connectedBody.transform);
				}
				if (joint.connectedBody == from)
				{
					connectedItems.Add(joint.transform);
				}
			}

			for (int i = 0; i < connectedItems.Count; i++)
			{
				var connectedItem = connectedItems[i];

				if (!ocupied.Add(connectedItem))
				{
					// we already had been there. Skip
					continue;
				}

				if (connectedItem == to)
				{
					return true; // connected !!!
				}

				var connected = IsAlreadyConnected(connectedItem, to, joints, ocupied);

				ocupied.Remove(connectedItem);

				if (connected)
					return true;
			}

			return false;
		}

		private static bool RigidHaveColliders(Transform tr, List<Collider> colliders)
		{
			var cldrs = tr.GetComponents<Collider>();
			for (int j = 0; j < cldrs.Length; j++)
			{
				Collider cldr = cldrs[j];
				if (colliders.Contains(cldr))
					return true;
			}

			for (int i = 0; i < tr.childCount; i++)
			{
				var ch = tr.GetChild(i);
				if (ch.GetComponent<Rigidbody>() != null)
				{
					continue;
				}

				if (RigidHaveColliders(ch, colliders))
					return true;
			}

			return false;
		}

		private static Joint CreateJoint(Transform itemA, Transform itemB)
		{
			Profiler.BeginSample("CreateJoint");
			var joint = itemA.gameObject.AddComponent<CharacterJoint>();
			var rigid = itemB.GetComponent<Rigidbody>();

			const float limit = 45f;
			joint.anchor = Vector3.zero;

			joint.autoConfigureConnectedAnchor = false;
			joint.connectedAnchor = new Vector3();

			joint.connectedBody = rigid;
			joint.lowTwistLimit = new SoftJointLimit { limit = -limit / 2f };
			joint.highTwistLimit = new SoftJointLimit { limit = limit / 2f };
			SoftJointLimit jl = new SoftJointLimit { limit = limit };
			joint.swing1Limit = jl;
			joint.swing2Limit = jl;

			SoftJointLimitSpring jls = new SoftJointLimitSpring { spring = 20f, damper = 1f };
			joint.twistLimitSpring = jls;
			joint.swingLimitSpring = jls;

			if (rigid == null)
				throw new InvalidOperationException();

			var dist = itemA.position - itemB.position;
			joint.anchor = itemA.InverseTransformDirection(-dist);
			Profiler.EndSample();

			return joint;
		}
	}
}