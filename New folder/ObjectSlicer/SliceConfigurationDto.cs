using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// This component can be added to object with mesh renderer to configure its behaviour
	/// </summary>
	public class SliceConfigurationDto
	{
		public SliceType SliceType;
		public Material SliceMaterial;
		public bool CreateCap = true;
		public bool SkipIfNotClosed;
	}

	public enum SliceType
	{
		Slice,
		Duplicate,
		KeepOne,
	}
}
