namespace BzKovSoft.ObjectSlicer.Samples
{
	/// <summary>
	/// Manager for SliceId ids
	/// </summary>
	public static class SliceIdProvider
	{
		static int _sliceId = 0;
		public static int GetNewSliceId()
		{
			return ++_sliceId;
		}
	}
}