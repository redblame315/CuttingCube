using BzKovSoft.ObjectSlicer;
using BzKovSoft.ObjectSlicer.Polygon;
using BzKovSoft.ObjectSlicer.Samples;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

namespace BzKovSoft.CharacterSlicer.Samples
{
	/// <summary>
	/// Example of implementation character sliceable object
	/// </summary>
	public class CharacterSlicerSampleFast : BzSliceableCharacterBase, IBzSliceableNoRepeat, IDeadable
	{
#pragma warning disable 0649
		[HideInInspector]
		[SerializeField]
		int _sliceId;
		[HideInInspector]
		[SerializeField]
		float _lastSliceTime = float.MinValue;
		[SerializeField]
		int _maxSliceCount = 3;
		[SerializeField]
		GameObject _bloodPrefub;
		[SerializeField]
		Vector3 _prefubDirection;
		[SerializeField]
		bool _convertToRagdoll = true;
		[SerializeField]
		bool _alignPrefSize = false;
#pragma warning restore 0649

		public bool IsDead { get; private set; }

		/// <summary>
		/// If your code do not use SliceId, it can relay on delay between last slice and new.
		/// If real delay is less than this value, slice will be ignored
		/// </summary>
		public float delayBetweenSlices = 1f;

		public void Slice(Plane plane, int sliceId, Action<BzSliceTryResult> callBack)
		{
			float currentSliceTime = Time.time;

			// we should prevent slicing same object:
			// - if _delayBetweenSlices was not exceeded
			// - with the same sliceId
			if ((sliceId == 0 & _lastSliceTime + delayBetweenSlices > currentSliceTime) |
				(sliceId != 0 & _sliceId == sliceId))
			{
				return;
			}

			// exit if it have LazyActionRunner
			if (GetComponent<LazyActionRunner>() != null)
				return;

			_lastSliceTime = currentSliceTime;
			_sliceId = sliceId;

			Slice(plane, callBack);
		}

		protected override BzSliceTryData PrepareData(Plane plane)
		{
			if (_maxSliceCount == 0)
				return null;

			// remember some date. Later we could use it after the slice is done.
			// here I add Stopwatch object to see how much time it takes
			ResultData addData = new ResultData();
			addData.stopwatch = Stopwatch.StartNew();

			// collider we want to participate in slicing
			var collidersArr = GetComponentsInChildren<Collider>();

			// create component manager.
			var componentManager = new CharacterComponentManagerFast(this.gameObject, plane, collidersArr);

			return new BzSliceTryData()
			{
				componentManager = componentManager,
				plane = plane,
				addData = addData,
			};
		}

		protected override void OnSliceFinishedWorkerThread(bool sliced, object addData)
		{
			((ResultData)addData).stopwatch.Stop();
		}

		protected override void OnSliceFinished(BzSliceTryResult result)
		{
			if (!result.sliced)
				return;

			var addData = (ResultData)result.addData;

			// add blood
			Profiler.BeginSample("AddBlood");
			AddBlood(result);
			Profiler.EndSample();

			var resultNeg = result.outObjectNeg.GetComponent<CharacterSlicerSampleFast>();
			var resultPos = result.outObjectPos.GetComponent<CharacterSlicerSampleFast>();
			var lazyActionNeg = result.outObjectNeg.GetComponent<LazyActionRunner>();
			var lazyActionPos = result.outObjectPos.GetComponent<LazyActionRunner>();

			// convert to ragdoll
			if (_convertToRagdoll & !IsDead)
			{
				Profiler.BeginSample("ConvertToRagdoll");
				ConvertToRagdoll(result.outObjectNeg, result.outObjectPos, lazyActionNeg, lazyActionPos);
				Profiler.EndSample();
			}

			// show elapsed time
			drawText += addData.stopwatch.ElapsedMilliseconds.ToString() + " - " + gameObject.name + Environment.NewLine;

			IsDead = true;
			resultNeg.IsDead = IsDead;
			resultPos.IsDead = IsDead;

			--_maxSliceCount;
			resultNeg._maxSliceCount = _maxSliceCount;
			resultPos._maxSliceCount = _maxSliceCount;
		}

		private void ConvertToRagdoll(GameObject resultNeg, GameObject resultPos, LazyActionRunner lazyRunnerNeg, LazyActionRunner lazyRunnerPos)
		{
			Animator animator = this.GetComponent<Animator>();
			Vector3 velocityContinue = animator.velocity;
			Vector3 angularVelocityContinue = animator.angularVelocity;

			ConvertToRagdoll(resultNeg, velocityContinue, angularVelocityContinue, lazyRunnerNeg);
			ConvertToRagdoll(resultPos, velocityContinue, angularVelocityContinue, lazyRunnerPos);
		}

		private void AddBlood(BzSliceTryResult result)
		{
			for (int i = 0; i < result.meshItems.Length; i++)
			{
				var meshItem = result.meshItems[i];

				if (meshItem == null)
					continue;

				for (int j = 0; j < meshItem.sliceEdgesNeg.Length; j++)
				{
					var meshData = meshItem.sliceEdgesNeg[j];
					SetBleedingObjects(meshData, meshItem.rendererNeg);
				}

				for (int j = 0; j < meshItem.sliceEdgesPos.Length; j++)
				{
					var meshData = meshItem.sliceEdgesPos[j];
					SetBleedingObjects(meshData, meshItem.rendererPos);
				}
			}
		}

		private void SetBleedingObjects(BzSliceEdgeResult edge, Renderer renderer)
		{
			if (_bloodPrefub == null)
			{
				return;
			}

			var meshRenderer = renderer as MeshRenderer;
			var skinnedRenderer = renderer as SkinnedMeshRenderer;

			GameObject blood = null;

			if (meshRenderer != null)
			{
				// add blood object
				Vector3 position = AVG(edge.vertices);
				Vector3 direction = AVG(edge.normals).normalized;
				var rotation = Quaternion.FromToRotation(_prefubDirection, direction);
				blood = Instantiate(_bloodPrefub, renderer.gameObject.transform);

				blood.transform.localPosition = position;
				blood.transform.localRotation = rotation;
			}
			else if (skinnedRenderer != null)
			{
				var bones = skinnedRenderer.bones;
				float[] weightSums = new float[bones.Length];
				for (int i = 0; i < edge.boneWeights.Length; i++)
				{
					var w = edge.boneWeights[i];
					weightSums[w.boneIndex0] += w.weight0;
					weightSums[w.boneIndex1] += w.weight1;
					weightSums[w.boneIndex2] += w.weight2;
					weightSums[w.boneIndex3] += w.weight3;
				}

				// detect most weightful bone for this PolyMeshData
				int maxIndex = 0;
				for (int i = 0; i < weightSums.Length; i++)
				{
					float maxValue = weightSums[maxIndex];
					float current = weightSums[i];

					if (current > maxValue)
						maxIndex = i;
				}
				Transform bone = bones[maxIndex];

				// add blood object to the bone
				Vector3 position = AVG(edge.vertices);
				Vector3 normal = AVG(edge.normals).normalized;
				var rotation = Quaternion.FromToRotation(_prefubDirection, normal);

				var m = skinnedRenderer.sharedMesh.bindposes[maxIndex];
				position = m.MultiplyPoint3x4(position);

				blood = Instantiate(_bloodPrefub, bone);
				blood.transform.localPosition = position;
				blood.transform.localRotation = rotation;
			}

			if (_alignPrefSize)
			{
				var parentScale = blood.transform.parent.lossyScale;
				var newScale = new Vector3(
					1f / parentScale.x,
					1f / parentScale.y,
					1f / parentScale.z);

				blood.transform.localScale = Vector3.Scale(newScale, blood.transform.localScale);
			}
		}

		private static Vector3 AVG(Vector3[] vertices)
		{
			Vector3 result = Vector3.zero;

			for (int i = 0; i < vertices.Length; i++)
			{
				result += vertices[i];
			}

			return result / vertices.Length;
		}

		private void ConvertToRagdoll(GameObject go, Vector3 velocityContinue, Vector3 angularVelocityContinue, LazyActionRunner lazyRunner)
		{
			Profiler.BeginSample("ConvertToRagdoll");
			// if your player is dead, you do not need animator or collision collider
			Animator animator = go.GetComponent<Animator>();
			Collider triggerCollider = go.GetComponent<Collider>();

			UnityEngine.Object.Destroy(animator);
			UnityEngine.Object.Destroy(triggerCollider);

			SetVelocity(go, velocityContinue, angularVelocityContinue);
			StartCoroutine(SmoothDepenetration(go));

			var collidersArr = go.GetComponentsInChildren<Collider>();
			for (int i = 0; i < collidersArr.Length; i++)
			{
				var collider = collidersArr[i];
				if (collider == triggerCollider)
					continue;

				collider.isTrigger = false;
			}

			// set rigid bodies as non kinematic
			var rigidsArr = go.GetComponentsInChildren<Rigidbody>();
			for (int i = 0; i < rigidsArr.Length; i++)
			{
				var rigid = rigidsArr[i];
				rigid.isKinematic = false;
			}
			Profiler.EndSample();
		}

		static void SetVelocity(GameObject go, Vector3 velocityContinue, Vector3 angularVelocityContinue)
		{
			var rigids = go.GetComponentsInChildren<Rigidbody>();
			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];

				rigid.velocity = velocityContinue;
				rigid.angularVelocity = angularVelocityContinue;
			}
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


		static string drawText = "-";

		void OnGUI()
		{
			GUI.Label(new Rect(10, 10, 2000, 2000), drawText);
		}

		// Sample of data that can be attached to slice request.
		// In this the Stopwatch is used to time duration of slice operation.
		class ResultData
		{
			public Stopwatch stopwatch;
		}
	}
}