namespace BzKovSoft.ObjectSlicer.Polygon
{
	class LoopIndexCollection
	{
		public BzPolyLoop loop;
		public LinkedLoop<LoopIndex> items;
		public LoopIndexCollection(BzPolyLoop loop, LinkedLoop<LoopIndex> items)
		{
			this.loop = loop;
			this.items = items;
		}
	}
}