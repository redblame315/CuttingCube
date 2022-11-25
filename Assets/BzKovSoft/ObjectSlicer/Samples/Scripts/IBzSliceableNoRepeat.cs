using System;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Samples
{
	/// <summary>
	/// Asynchronously sliceable object
	/// </summary>
	public interface IBzSliceableNoRepeat
	{
		/// <summary>
		/// Start slicing the object
		/// </summary>
		/// <param name="plane">Plane by which you are going to slice</param>
		/// <param name="sliceId">To prevent multiple slice requests you should use sliceId,
		/// you can pass 0 to ignore it</param>
		/// <param name="callBack">Method that will be called when the slice will be done</param>
		void Slice(Plane plane, int slicdId, Action<BzSliceTryResult> callBack);
	}
}
