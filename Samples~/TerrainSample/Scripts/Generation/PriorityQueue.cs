using System;
using System.Collections.Generic;

public class PriorityQueue<T> {

	struct PriorityElement<E> : IComparable<PriorityElement<E>> {
		public E element;
		public float priority;

		public PriorityElement(E element, float priority) {
			this.element = element;
			this.priority = priority;
		}

		public int CompareTo(PriorityElement<E> other) {
			return -priority.CompareTo(other.priority);
		}
	}

	List<PriorityElement<T>> set = new List<PriorityElement<T>>();

	public int Count { get { return set.Count; } }

	public void Enqueue(T element, float priority) {
		var value = new PriorityElement<T>(element, priority);
		int index = set.BinarySearch(value);
		if (index < 0)
			set.Insert(~index, value);
		else
			set.Insert(index, value);
	}

	public T Dequeue() {
		var result = set[set.Count - 1];
		set.RemoveAt(set.Count - 1);
		return result.element;
	}

	public void Clear() {
		set.Clear();
	}
}
