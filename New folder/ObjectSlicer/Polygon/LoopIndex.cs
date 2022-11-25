using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Polygon
{
	class LoopIndex
	{
		public BzPolyLoop loop;
		public LoopNode<int> indexPointer;
		public Vector2 vector2d;

		public LoopIndex(BzPolyLoop loop, LoopNode<int> indexPointer, Vector2 vector2d)
		{
			this.loop = loop;
			this.indexPointer = indexPointer;
			this.vector2d = vector2d;
		}
	}
}