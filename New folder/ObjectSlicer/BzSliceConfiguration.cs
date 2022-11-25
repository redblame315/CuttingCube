using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// This component can be added to object with mesh renderer to configure its behaviour
	/// </summary>
	[DisallowMultipleComponent]
	public class BzSliceConfiguration : MonoBehaviour
	{
#pragma warning disable 0649
		public SliceType SliceType;
		public Material SliceMaterial;
		public bool CreateCap = true;
		public bool SkipIfNotClosed;
#pragma warning restore 0649

		public SliceConfigurationDto GetDto()
		{
			return new SliceConfigurationDto
			{
				SliceType = SliceType,
				SliceMaterial = SliceMaterial == null ? null : SliceMaterial,
				CreateCap = CreateCap,
				SkipIfNotClosed = SkipIfNotClosed,
			};
		}

		public static SliceConfigurationDto GetDefault()
		{
			return new SliceConfigurationDto
			{
				SliceType = SliceType.Slice,
				SliceMaterial = null,
				CreateCap = true,
				SkipIfNotClosed = false,
			};
		}
	}
}
