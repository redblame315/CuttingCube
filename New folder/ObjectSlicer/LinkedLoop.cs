using System;
using System.Collections.Generic;

namespace BzKovSoft.ObjectSlicer
{
	public class LinkedLoop<T>
	{
		public int size;
		public LoopNode<T> first;
		public LoopNode<T> last;

		public LinkedLoop()
		{
		}

		public LinkedLoop(List<T> list)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				LoopNode<T> node = new LoopNode<T>(this, list[i]);
				AddLast(node);
			}
		}

		public void InsertAfter(LoopNode<T> position, LoopNode<T> nodeFrom, LoopNode<T> nodeTo)
		{
#if DEBUG
			if (position.list != this)
			{
				throw new ArgumentException("position argument must belong to current colliction");
			}

			if (nodeFrom.list == this)
			{
				throw new ArgumentException("nodeFrom argument must not belong to current colliction");
			}

			if (nodeFrom.list != nodeTo.list)
			{
				throw new ArgumentException("nodeFrom and nodeTo arguments must belong to the same colliction");
			}
#endif

			var node = nodeFrom;
			int counter = 1;
			for (; ; )
			{
				if (node == nodeTo)
				{
					break;
				}

				if (counter > node.list.size)
					throw new InvalidOperationException();

				node = node.next;
				++counter;
			}

			var otherList = nodeFrom.list;
			size += counter;
			otherList.size -= counter;

			if (otherList.size == 0)
			{
				otherList.first = null;
				otherList.last = null;
			}
			else
			{
				throw new InvalidOperationException("Not full utilization is not supportet now.");
			}

			if (position == last)
			{
				last = nodeTo;
			}

			var posNext = position.next;
			position.next = nodeFrom;
			posNext.previous = nodeTo;
			nodeFrom.previous = position;
			nodeTo.next = posNext;

			node = nodeFrom;
			for (; ; )
			{
				node.list = this;

				if (node == nodeTo)
				{
					break;
				}

				node = node.next;
			}
		}

		public void AddLast(LoopNode<T> node)
		{
			size++;

			if (first == null)
			{
				first = node;
			}

			if (last == null)
			{
				last = node;
			}

			node.previous = last;
			node.next = first;

			first.previous = node;
			last.next = node;
			last = node;
		}

		public void AddFirst(T value)
		{
			size++;
			var node = new LoopNode<T>(this, value);

			if (first == null)
			{
				first = node;
			}

			if (last == null)
			{
				last = node;
			}

			node.previous = last;
			node.next = first;

			first.previous = node;
			last.next = node;
			first = node;
		}

		public void InsertAfter(LoopNode<T> node, T value)
		{
#if DEBUG
			if (node.list != this)
			{
				throw new InvalidOperationException("'node' should be of the same list");
			}
#endif

			size++;

			var newNode = new LoopNode<T>(this, value);
			if (last == node)
			{
				last = newNode;
			}

			newNode.previous = node;
			newNode.next = node.next;
			node.next.previous = newNode;
			node.next = newNode;
		}

		public void AddLast(T value)
		{
			var node = new LoopNode<T>(this, value);
			AddLast(node);
		}

		public override string ToString()
		{
			return "Size: " + size.ToString();
		}

	}

	public static class LinkedLoop
	{
		/// <summary>
		/// Concat items of two collections
		/// </summary>
		public static LinkedLoop<T> ConcatList<T>(LinkedLoop<T> listA, LinkedLoop<T> listB)
		{
			if (listA.size == 0)
				return listB;

			if (listB.size == 0)
				return listA;

			var result = new LinkedLoop<T>();
			result.size = listA.size + listB.size;

			result.first = listA.first;
			result.last = listB.last;

			result.first.previous = result.last;
			result.last.next = result.first;

			listA.last.next = listB.first;
			listB.first.previous = listA.last;

			// change node owners
			var current = result.first;
			do
			{
				current.list = result;
				current = current.next;
			}
			while (current != result.first);

			// empty list A and B
			listA.first = null;
			listA.last = null;
			listA.size = 0;

			listB.first = null;
			listB.last = null;
			listB.size = 0;

			return result;
		}
	}

	public class LoopNode<T>
	{
		public LinkedLoop<T> list;
		public T value;
		public LoopNode<T> previous;
		public LoopNode<T> next;

		public LoopNode(LinkedLoop<T> list, T value)
		{
			this.list = list;
			this.value = value;
		}

		public void Remove()
		{
			--list.size;

			if (list.size == 0)
			{
				list.first = null;
				list.last = null;
				list = null;
				next = null;
				previous = null;
			}
			else
			{
				if (list.first == this)
					list.first = next;

				if (list.last == this)
					list.last = previous;

				next.previous = previous;
				previous.next = next;
			}
		}

		public override string ToString()
		{
			return "Node: " + value.ToString();
		}
	}
}
