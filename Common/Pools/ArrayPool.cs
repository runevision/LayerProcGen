/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace Runevision.Common {

	// T is the type of the elements in the array, not the arrays themselves.
	public class ArrayPool<T> : IPool<T[]> {

		public int CountAll { get; private set; }
		public int CountActive { get { return CountAll - CountInactive; } }
		public int CountInactive { get {
			lock (stack) {
				return stack.Count;
			}
		} }

		int arrayLength;
		Stack<T[]> stack = new Stack<T[]>();
		string name;

		public ArrayPool(int arrayLength) {
			this.arrayLength = arrayLength;
            name = GetType().PrettyName() + " [" + this.arrayLength + "]";
			lock (PoolManager.allPools) {
				PoolManager.allPools.Add(this);
			}
		}

		public T[] Get() {
			lock (stack) {
				if (stack.Count > 0)
					return stack.Pop();
				else
					CountAll++;
			}
			T[] array = new T[arrayLength];
			PoolManager.TrackSourcePool(array, this);
			return array;
		}

		public void Return(ref T[] array) {
			lock (stack) {
				if (stack.Contains(array))
					throw new System.Exception("Trying to return array that was already returned to the pool.");
				array.Clear();
				stack.Push(array);
				array = null;
			}
		}

		public void Return(ref object obj) {
			var arr = (T[])obj;
			Return(ref arr);
			obj = null;
		}

		public override string ToString() { return name; }
	}

	// T is the type of the elements in the array, not the arrays themselves.
	public class Array2DPool<T> : IPool<T[,]> {

		public int CountAll { get; private set; }
		public int CountActive { get { return CountAll - CountInactive; } }
		public int CountInactive { get {
			lock (stack) {
				return stack.Count;
			}
		} }

		int arrayLength1;
		int arrayLength2;
		Stack<T[,]> stack = new Stack<T[,]>();
		string name;

		public Array2DPool(int arrayLength1, int arrayLength2) {
			this.arrayLength1 = arrayLength1;
			this.arrayLength2 = arrayLength2;
            name = GetType().PrettyName() + " [" + this.arrayLength1 + "," + this.arrayLength2 + "]";
			lock (PoolManager.allPools) {
				PoolManager.allPools.Add(this);
			}
		}

		public T[,] Get() {
			lock (stack) {
				if (stack.Count > 0)
					return stack.Pop();
				else
					CountAll++;
			}
			T[,] array = new T[arrayLength1, arrayLength2];
			PoolManager.TrackSourcePool(array, this);
			return array;
		}

		public void Return(ref T[,] array) {
			lock (stack) {
				if (stack.Contains(array))
					throw new System.Exception("Trying to return array that was already returned to the pool.");
				array.Clear();
				stack.Push(array);
				array = null;
			}
		}

		public void Return(ref object obj) {
			var arr = (T[,])obj;
			Return(ref arr);
			obj = null;
		}

		public override string ToString() { return name; }
	}

	// T is the type of the elements in the array, not the arrays themselves.
	public class Array3DPool<T> : IPool<T[,,]> {

		public int CountAll { get; private set; }
		public int CountActive { get { return CountAll - CountInactive; } }
		public int CountInactive { get {
			lock (stack) {
				return stack.Count;
			}
		} }

		int arrayLength1;
		int arrayLength2;
		int arrayLength3;
		Stack<T[,,]> stack = new Stack<T[,,]>();
		string name;

		public Array3DPool(int arrayLength1, int arrayLength2, int arrayLength3) {
			this.arrayLength1 = arrayLength1;
			this.arrayLength2 = arrayLength2;
			this.arrayLength3 = arrayLength3;
            name = GetType().PrettyName() + " [" + this.arrayLength1 + "," + this.arrayLength2 + "," + this.arrayLength3 + "]";
			lock (PoolManager.allPools) {
				PoolManager.allPools.Add(this);
			}
		}

		public T[,,] Get() {
			lock (stack) {
				if (stack.Count > 0)
					return stack.Pop();
				else
					CountAll++;
			}
			T[,,] array = new T[arrayLength1, arrayLength2, arrayLength3];
			PoolManager.TrackSourcePool(array, this);
			return array;
		}

		public void Return(ref T[,,] array) {
			lock (stack) {
				if (stack.Contains(array))
					throw new System.Exception("Trying to return array that was already returned to the pool.");
				array.Clear();
				stack.Push(array);
				array = null;
			}
		}

		public void Return(ref object obj) {
			var arr = (T[,,])obj;
			Return(ref arr);
			obj = null;
		}

		public override string ToString() { return name; }
	}

}
