/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace Runevision.Common {

	// T is the type of the elements in the list, not the lists themselves.
	public class ListPool<T> : IPool<List<T>> {

		public int CountAll { get; private set; }
		public int CountActive { get { return CountAll - CountInactive; } }
		public int CountInactive { get {
			lock (stack) {
				return stack.Count;
			}
		} }

		int capacity;
		Stack<List<T>> stack = new Stack<List<T>>();
		string name;

		public ListPool(int capacity) {
			this.capacity = capacity;
			name = GetType().PrettyName() + " cap " + this.capacity;
			lock (PoolManager.allPools) {
				PoolManager.allPools.Add(this);
			}
		}

		public List<T> Get() {
			lock (stack) {
				if (stack.Count > 0)
					return stack.Pop();
				else
					CountAll++;
			}
			List<T> list = new List<T>(capacity);
			PoolManager.TrackSourcePool(list, this);
			return list;
		}

		public void Return(ref List<T> list) {
			lock (stack) {
				if (stack.Contains(list))
					throw new System.Exception("Trying to return list that was already returned to the pool.");
			}
			list.Clear();
			if (list.Capacity > capacity) {
				Logg.LogWarning(list.GetType().PrettyName() + " capacity was increased from " + capacity + " to " + list.Capacity);
				capacity = list.Capacity;
			}
			lock (stack) {
				stack.Push(list);
				if (stack.Count > CountAll)
					Logg.LogWarning(list.GetType().PrettyName() + " has " + stack.Count + " in stack but only " + CountAll + " total.");
			}
			list = null;
		}

		public void Return(ref object obj) {
			List<T> list = (List<T>)obj;
			Return(ref list);
			obj = null;
		}

		public override string ToString() { return name; }
	}

}
