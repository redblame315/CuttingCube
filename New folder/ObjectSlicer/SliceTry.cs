using System;
using System.Threading;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	public class SliceTry
	{
		public SliceTryItem[] items;
		bool _finished;
		public BzSliceTryData sliceData;
		public Action<BzSliceTryResult> callBack;
		public bool sliced;
		public Vector3 position;
		public Quaternion rotation;

		public bool Finished
		{
			get
			{
				Thread.MemoryBarrier();
				return _finished;
			}
			set
			{
				_finished = value;
				Thread.MemoryBarrier();
			}
		}
	}
}