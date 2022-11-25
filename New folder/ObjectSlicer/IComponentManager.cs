using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Manage components for sliced objects
	/// </summary>
	public interface IComponentManager
	{
		bool Success { get; }

		/// <summary>
		/// Asynchronous call
		/// </summary>
		void OnSlicedWorkerThread(SliceTryItem[] items);

		/// <summary>
		/// Synchronous call
		/// </summary>
		void OnSlicedMainThread(GameObject resultObjNeg, GameObject resultObjPos, Renderer[] renderersNeg, Renderer[] renderersPos);
	}
}
