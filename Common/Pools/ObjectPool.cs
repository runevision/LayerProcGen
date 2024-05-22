/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace Runevision.Common {

	public class ObjectPool<T> : IPool<T> where T : IPoolable, new() {

		// Singleton
		static ObjectPool<T> s_Instance;

		public static ObjectPool<T> instance {
			get {
				lock (PoolManager.allPools) {
					if (s_Instance == null)
						s_Instance = new ObjectPool<T>();
					return s_Instance;
				}
			}
		}

		/// <summary>
		/// Gets an existing object from the singleton pool of type T, or creates one if none are available.
		/// </summary>
		public static T GlobalGet() {
			return instance.Get();
		}

		/// <summary>
		/// Returns the object to the singleton pool of type T and sets the reference to null.
		/// </summary>
		public static void GlobalReturn(ref T element) {
			instance.Return(ref element);
		}

		/// <summary>
		/// Returns all the elements to the singleton pool of type T and calls Clear on the collection.
		/// </summary>
		public static void GlobalReturnAll(ICollection<T> elements) {
			instance.ReturnAll(elements);
		}

		// Member data

		public int CountAll { get; private set; }
		public int CountActive { get { return CountAll - CountInactive; } }
		public int CountInactive { get {
			lock (stack) {
				return stack.Count;
			}
		} }

		Stack<T> stack = new Stack<T>();
		string name;

		public ObjectPool() {
			name = GetType().PrettyName();
			lock (PoolManager.allPools) {
				PoolManager.allPools.Add(this);
			}
		}

		public T Get() {
			lock (stack) {
				if (stack.Count > 0)
					return stack.Pop();
				else
					CountAll++;
			}
			T element = new T();
			PoolManager.TrackSourcePool(element, this);
			return element;
		}

		public void Return(ref T element) {
			lock (stack) {
				if (stack.Count > 0 && ReferenceEquals(stack.Peek(), element))
					throw new System.Exception("Trying to return object that was already returned to the pool.");
				element.Reset();
				stack.Push(element);
				element = default;
			}
		}

		public void Return(ref object obj) {
			T typedObject = (T)obj;
			Return(ref typedObject);
			obj = null;
		}

		/// <summary>
		/// Returns all the elements to the pool and calls Clear on the collection.
		/// </summary>
		/// <param name="elements"></param>
		public void ReturnAll(ICollection<T> elements) {
			foreach (T element in elements) {
				T copy = element;
				Return(ref copy);
			}
			elements.Clear();
		}

		public override string ToString() { return name; }
	}

}
