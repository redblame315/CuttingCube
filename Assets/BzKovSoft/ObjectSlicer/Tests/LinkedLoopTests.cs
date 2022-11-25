using NUnit.Framework;

namespace BzKovSoft.ObjectSlicer.Tests
{
	public class LinkedLoopTests
	{
		[Test]
		public void AddItem()
		{
			//Arrange
			LinkedLoop<int> list = new LinkedLoop<int>();

			//Act
			list.AddLast(1);
			list.AddLast(2);
			list.AddLast(3);

			//Assert
			Assert.AreEqual(3, list.size);

			var item1 = list.first;
			var item2 = item1.next;
			var item3 = item2.next;

			Assert.AreEqual(1, item1.value);
			Assert.AreEqual(2, item2.value);
			Assert.AreEqual(3, item3.value);

			Assert.AreEqual(item1.next, item2);
			Assert.AreEqual(item1.previous, item3);

			Assert.AreEqual(item2.next, item3);
			Assert.AreEqual(item2.previous, item1);

			Assert.AreEqual(item3.next, item1);
			Assert.AreEqual(item3.previous, item2);
		}

		[Test]
		public void RemoveAndZeroingSize()
		{
			//Arrange
			LinkedLoop<int> list = new LinkedLoop<int>();

			//Act
			list.AddLast(1);

			var item1 = list.first;
			item1.Remove();

			//Assert
			Assert.AreEqual(0, list.size);
			Assert.IsNull(list.first);
			Assert.IsNull(list.last);

			Assert.IsNull(item1.next);
			Assert.IsNull(item1.previous);
		}

		[Test]
		public void InsertLoopIntoLoopAfter()
		{
			//Arrange
			LinkedLoop<int> list1 = new LinkedLoop<int>();
			LinkedLoop<int> list2 = new LinkedLoop<int>();
			list1.AddLast(1);
			list1.AddLast(2);
			list1.AddLast(3);
			list2.AddLast(4);
			list2.AddLast(5);
			list2.AddLast(6);

			//Act
			list1.InsertAfter(list1.last, list2.first, list2.last);

			//Assert
			Assert.AreEqual(6, list1.size);
			Assert.AreEqual(0, list2.size);
			CheckConsistancy(list1);
			CheckConsistancy(list2);

			var items = GetArray(list1);

			Assert.AreEqual(1, items[0].value);
			Assert.AreEqual(2, items[1].value);
			Assert.AreEqual(3, items[2].value);
			Assert.AreEqual(4, items[3].value);
			Assert.AreEqual(5, items[4].value);
			Assert.AreEqual(6, items[5].value);
		}

		[Test]
		public void InsertValueAfter()
		{
			//Arrange
			LinkedLoop<int> list = new LinkedLoop<int>();
			list.AddLast(1);
			list.AddLast(3);

			//Act
			list.InsertAfter(list.first, 2);

			//Assert
			Assert.AreEqual(3, list.size);
			CheckConsistancy(list);

			var n = list.first;
			for (int i = 0; i < list.size; i++)
			{
				Assert.AreEqual(i + 1, n.value);
				n = n.next;
			}
		}

		[Test]
		public void InsertValueAfterLast()
		{
			//Arrange
			LinkedLoop<int> list = new LinkedLoop<int>();
			list.AddLast(1);
			list.AddLast(2);

			//Act
			list.InsertAfter(list.last, 3);

			//Assert
			Assert.AreEqual(3, list.size);
			CheckConsistancy(list);

			var n = list.first;
			for (int i = 0; i < list.size; i++)
			{
				Assert.AreEqual(i + 1, n.value);
				n = n.next;
			}
		}

		[Test]
		public void RemoveInMiddle()
		{
			//Arrange
			LinkedLoop<int> list = new LinkedLoop<int>();

			//Act
			list.AddLast(1);
			list.AddLast(2);
			list.AddLast(3);
			list.AddLast(4);

			var item1 = list.first;
			var item2 = item1.next;
			var item3 = item2.next;
			var item4 = item3.next;
			item2.Remove();

			//Assert
			Assert.AreEqual(3, list.size);

			Assert.AreEqual(1, item1.value);
			Assert.AreEqual(3, item3.value);
			Assert.AreEqual(4, item4.value);

			Assert.AreEqual(item1.next, item3);
			Assert.AreEqual(item1.previous, item4);

			Assert.AreEqual(item3.next, item4);
			Assert.AreEqual(item3.previous, item1);

			Assert.AreEqual(item4.next, item1);
			Assert.AreEqual(item4.previous, item3);
		}

		[Test]
		public void RemoveFirst()
		{
			//Arrange
			LinkedLoop<int> list = new LinkedLoop<int>();

			//Act
			list.AddLast(1);
			list.AddLast(2);
			list.AddLast(3);
			list.AddLast(4);

			var item1 = list.first;
			var item2 = item1.next;
			var item3 = item2.next;
			var item4 = item3.next;
			item1.Remove();

			//Assert
			Assert.AreEqual(3, list.size);

			Assert.AreEqual(2, item2.value);
			Assert.AreEqual(3, item3.value);
			Assert.AreEqual(4, item4.value);

			Assert.AreEqual(item2.next, item3);
			Assert.AreEqual(item2.previous, item4);

			Assert.AreEqual(item3.next, item4);
			Assert.AreEqual(item3.previous, item2);

			Assert.AreEqual(item4.next, item2);
			Assert.AreEqual(item4.previous, item3);
		}

		[Test]
		public void RemoveLast()
		{
			//Arrange
			LinkedLoop<int> list = new LinkedLoop<int>();

			//Act
			list.AddLast(1);
			list.AddLast(2);
			list.AddLast(3);
			list.AddLast(4);

			var item1 = list.first;
			var item2 = item1.next;
			var item3 = item2.next;
			var item4 = item3.next;
			item4.Remove();

			//Assert
			Assert.AreEqual(3, list.size);

			Assert.AreEqual(1, item1.value);
			Assert.AreEqual(2, item2.value);
			Assert.AreEqual(3, item3.value);

			Assert.AreEqual(item1.next, item2);
			Assert.AreEqual(item1.previous, item3);

			Assert.AreEqual(item2.next, item3);
			Assert.AreEqual(item2.previous, item1);

			Assert.AreEqual(item3.next, item1);
			Assert.AreEqual(item3.previous, item2);
		}

		[Test]
		public void RemoveConcat()
		{
			//Arrange
			LinkedLoop<int> list1 = new LinkedLoop<int>();
			LinkedLoop<int> list2 = new LinkedLoop<int>();

			//Act
			list1.AddLast(1);
			list1.AddLast(2);
			list2.AddLast(3);
			list2.AddLast(4);

			LinkedLoop<int> listS = LinkedLoop.ConcatList(list1, list2);

			//Assert
			var item1 = listS.first;
			var item2 = item1.next;
			var item3 = item2.next;
			var item4 = item3.next;

			Assert.AreEqual(4, listS.size);

			Assert.AreEqual(1, item1.value);
			Assert.AreEqual(2, item2.value);
			Assert.AreEqual(3, item3.value);
			Assert.AreEqual(4, item4.value);

			Assert.AreEqual(item1.next, item2);
			Assert.AreEqual(item1.previous, item4);

			Assert.AreEqual(item2.next, item3);
			Assert.AreEqual(item2.previous, item1);

			Assert.AreEqual(item3.next, item4);
			Assert.AreEqual(item3.previous, item2);

			Assert.AreEqual(item4.next, item1);
			Assert.AreEqual(item4.previous, item3);
		}

		private static void CheckConsistancy(LinkedLoop<int> list)
		{
			if (list.size == 0)
			{
				Assert.IsNull(list.first);
				Assert.IsNull(list.last);
				return;
			}
			else
			{
				Assert.IsNotNull(list.first);
				Assert.IsNotNull(list.last);
			}

			var node = list.first;
			int count = 0;

			for (; ; )
			{
				++count;
				if (list.last == node | count > list.size)
				{
					break;
				}

				node = node.next;
			}

			Assert.AreEqual(list.size, count);

			LoopNode<int>[] items = GetArray(list);

			for (int i = 0; i < count; i++)
			{
				int indexPrev = i == 0 ? items.Length - 1 : i - 1;
				int indexNext = i == items.Length - 1 ? 0 : i + 1;
				var n1 = items[indexPrev];
				var n2 = items[i];
				var n3 = items[indexNext];

				Assert.AreEqual(n1, n2.previous);
				Assert.AreEqual(n3, n2.next);
				Assert.AreEqual(list, n2.list);
			}
		}

		private static LoopNode<int>[] GetArray(LinkedLoop<int> list)
		{
			var node = list.first;
			var items = new LoopNode<int>[list.size];
			for (int i = 0; i < list.size; i++)
			{
				items[i] = node;
				node = node.next;
			}

			return items;
		}
	}
}