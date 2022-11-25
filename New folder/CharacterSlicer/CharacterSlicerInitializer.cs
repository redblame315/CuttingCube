using BzKovSoft.ObjectSlicer;
using System;
using UnityEngine;

namespace BzKovSoft.CharacterSlicer
{
	/// <summary>
	/// Compile whole CharacterSlicer code. (invoke JIT)
	/// </summary>
	public class CharacterSlicerInitializer : MonoBehaviour
	{
		static bool _initialized;
		void Start()
		{
			Init();
		}

		public static void Init()
		{
			if (_initialized)
				return;

			_initialized = true;

			var go = new GameObject();
			var b1 = new GameObject();
			var b2 = new GameObject();
			b1.transform.parent = go.transform;
			b2.transform.parent = b1.transform;

			var r = go.AddComponent<SkinnedMeshRenderer>();
			r.sharedMesh = GetMesh();
			r.rootBone = b1.transform;
			r.bones = new[]
			{
				b1.transform,
				b2.transform,
			};

			var animator = go.AddComponent<Animator>();
			animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

			go.AddComponent<Rigidbody>().isKinematic = true;
			b1.AddComponent<Rigidbody>().isKinematic = true;
			b2.AddComponent<Rigidbody>().isKinematic = true;

			go.AddComponent<BoxCollider>();

			var slicer = go.AddComponent<CharacterSlicerInitializerObj>();
			slicer.asynchronously = false;
			slicer.defaultSliceMaterial = new Material(Shader.Find("Standard"));
			Action<BzSliceTryResult> action = (x) =>
			{
				if (!x.sliced)
					throw new InvalidOperationException("Not sliced");

				Destroy(x.outObjectNeg);
				Destroy(x.outObjectPos);
			};

			slicer.Slice(new Plane(Vector3.up, Vector3.zero), action);
		}

		private static Mesh GetMesh()
		{
			var mesh = new Mesh();

			mesh.vertices = new[]
			{
				new Vector3(-1, -1, 0),
				new Vector3( 0,  1, 0),
				new Vector3( 1, -1, 0),
			};
			mesh.triangles = new[] { 0, 1, 2 };
			mesh.boneWeights = new[]
			{
				new BoneWeight { boneIndex0 = 0, weight0 = 1 },
				new BoneWeight { boneIndex0 = 1, weight0 = 1 },
				new BoneWeight { boneIndex0 = 0, weight0 = 1 },
			};
			mesh.bindposes = new[]
			{
				Matrix4x4.identity,
				Matrix4x4.identity,
			};
			return mesh;
		}

		class CharacterSlicerInitializerObj : BzSliceableCharacterBase
		{
		}
	}
}