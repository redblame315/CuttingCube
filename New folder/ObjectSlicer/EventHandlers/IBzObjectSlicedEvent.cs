using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	/// <summary>
	/// Object Slicer event processor
	/// </summary>
	public interface IBzObjectSlicedEvent
	{
		/// <summary>
		/// called when the object successfully sliced
		/// </summary>
		void ObjectSliced(GameObject original, GameObject resutlNeg, GameObject resultPos);
	}
}
