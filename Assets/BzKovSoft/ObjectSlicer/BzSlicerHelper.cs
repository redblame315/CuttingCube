using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	static class BzSlicerHelper
	{
		public static T GetSameComponentForDuplicate<T>(T c, GameObject original, GameObject duplicate)
			where T : Component
		{
			// remember hierarchy
			Stack<int> path = new Stack<int>();

			var g = c.gameObject;
			while (!object.ReferenceEquals(g, original))
			{
				path.Push(g.transform.GetSiblingIndex());
				g = g.transform.parent.gameObject;
			}

			// repeat hierarchy on duplicated object
			GameObject sameGO = duplicate;
			while (path.Count != 0)
			{
				sameGO = sameGO.transform.GetChild(path.Pop()).gameObject;
			}

			// get component index
			var cc = c.gameObject.GetComponents<T>();
			int componentIndex = -1;
			for (int i = 0; i < cc.Length; i++)
			{
				if (object.ReferenceEquals(c, cc[i]))
				{
					componentIndex = i;
					break;
				}
			}

			// return component with the same index on same gameObject
			return sameGO.GetComponents<T>()[componentIndex];
		}
	}
}
