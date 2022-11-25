using System;
using System.Collections.Generic;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// One triange from a mesh
	/// </summary>
	public struct BzTriangle
	{
		public readonly int i1;
		public readonly int i2;
		public readonly int i3;

		/// <summary>
		/// Create triangle from 3 indexes of a mesh
		/// </summary>
		public BzTriangle(int i1, int i2, int i3)
		{
			this.i1 = i1;
			this.i2 = i2;
			this.i3 = i3;
		}

		/// <summary>
		/// Cut this trianle by plane (plane from MeshPeparator)
		/// </summary>
		/// <param name="tr">Out triangle 1 (always exists)</param>
		/// <param name="trNegExtra">Out triangle 2 (can be null)</param>
		public void DivideByPlane(BzMeshDataEditor meshDataEditorNeg, BzMeshDataEditor meshDataEditorPos,
			List<BzTriangle> trianglesNegSliced, List<BzTriangle> trianglesPosSliced,
			bool _side1, bool _side2, bool _side3)
		{

			if (!_side1 & _side2 & _side3)
			{
				CalculateOneTr(meshDataEditorNeg, i1, i2, i3, trianglesNegSliced);
				CalculateTwoTr(meshDataEditorPos, i1, i2, i3, trianglesPosSliced);
			}
			else if (_side1 & !_side2 & _side3)
			{
				CalculateOneTr(meshDataEditorNeg, i2, i3, i1, trianglesNegSliced);
				CalculateTwoTr(meshDataEditorPos, i2, i3, i1, trianglesPosSliced);
			}
			else if (_side1 & _side2 & !_side3)
			{
				CalculateOneTr(meshDataEditorNeg, i3, i1, i2, trianglesNegSliced);
				CalculateTwoTr(meshDataEditorPos, i3, i1, i2, trianglesPosSliced);
			}


			else if (_side1 & !_side2 & !_side3)
			{
				CalculateTwoTr(meshDataEditorNeg, i1, i2, i3, trianglesNegSliced);
				CalculateOneTr(meshDataEditorPos, i1, i2, i3, trianglesPosSliced);
			}
			else if (!_side1 & _side2 & !_side3)
			{
				CalculateTwoTr(meshDataEditorNeg, i2, i3, i1, trianglesNegSliced);
				CalculateOneTr(meshDataEditorPos, i2, i3, i1, trianglesPosSliced);
			}
			else if (!_side1 & !_side2 & _side3)
			{
				CalculateTwoTr(meshDataEditorNeg, i3, i1, i2, trianglesNegSliced);
				CalculateOneTr(meshDataEditorPos, i3, i1, i2, trianglesPosSliced);
			}
			else
				throw new InvalidOperationException();
		}

		/// <summary>
		/// Calculate when two point right. The result is one triangle
		/// </summary>
		static void CalculateOneTr(BzMeshDataEditor meshDataEditor, int inV, int outA, int outB, List<BzTriangle> trianglesSliced)
		{
			//  outA\^^^^^^^^^^^^^/outB
			//       \           /
			//        \         /
			//      ---------------
			//    new2  \     /  new1
			//           \   /
			//            \ /
			//            inV

			int new1 = meshDataEditor.GetIndexFor(inV, outB);
			int new2 = meshDataEditor.GetIndexFor(inV, outA);

			if (new1 != inV & new2 != inV)
			{
				var tr = new BzTriangle(inV, new2, new1);
				trianglesSliced.Add(tr);
			}

			// save edge to chash
			if (new1 != new2)
			{

				meshDataEditor.CapEdges.Add(new IndexVector(new1, new2));
			}
		}

		/// <summary>
		/// Calculate when one point right. The result is two triangles
		/// </summary>
		static void CalculateTwoTr(BzMeshDataEditor meshDataEditor, int outV, int inA, int inB, List<BzTriangle> trianglesSliced)
		{
			//            outV
			//            / \
			//           /   \
			//    new2  /     \  new1
			//      ----.----------
			//        /   .     \
			//       /       .   \
			//   inB/___________._\inA

			int new1 = meshDataEditor.GetIndexFor(inA, outV);
			int new2 = meshDataEditor.GetIndexFor(inB, outV);

			if (new1 == outV | new2 == outV)
			{
				var tr = new BzTriangle(inB, outV, inA);
				trianglesSliced.Add(tr);
			}
			else
			{
				if (new1 != inA)
				{
					var tr1 = new BzTriangle(inA, new2, new1);
					trianglesSliced.Add(tr1);
				}
				if (new2 != inB)
				{
					var tr2 = new BzTriangle(inB, new2, inA);
					trianglesSliced.Add(tr2);
				}
			}

			// save edge to chash
			if (new2 != new1)
			{
				meshDataEditor.CapEdges.Add(new IndexVector(new1, new2));
			}
		}
	}
}
