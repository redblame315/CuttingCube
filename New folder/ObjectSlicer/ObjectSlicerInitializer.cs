using System;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Compile whole ObjectSlicer code. (invoke JIT)
	/// </summary>
	public class ObjectSlicerInitializer : MonoBehaviour
	{
		static bool _initialized;
		void Start()
		{
			if (_initialized)
				return;

			_initialized = true;
			Init();
		}

		static void Init()
		{
			var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			var slicer = go.AddComponent<ObjectSlicerInitializerObj>();
			slicer.asynchronously = true;
			slicer.defaultSliceMaterial = new Material(Shader.Find("Standard"));
			Action<BzSliceTryResult> action = (x) =>
			{
				if (!x.sliced)
					throw new InvalidOperationException();

				Destroy(x.outObjectNeg);
				Destroy(x.outObjectPos);
			};
			slicer.Slice(new Plane(Vector3.up, Vector3.zero), action);
		}

		class ObjectSlicerInitializerObj : BzSliceableObjectBase
		{
		}
	}
}