/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace Runevision.Common {

	public static class CollectionExtensions {

		public static void Clear(this Array array) {
			Array.Clear(array, 0, array.Length);
		}

		public static T GetRandom<T>(this IList<T> list, RandomHash rng, int seed) {
			int n = rng.Range(0, list.Count, seed);
			return list[n];
		}

		public static void Shuffle<T>(this IList<T> list, RandomHash rng, int seed) {
			int n = list.Count;
			while (n > 1) {
				n--;
				int k = rng.Range(0, n + 1, n, seed);
				(list[n], list[k]) = (list[k], list[n]);
			}
		}

		public static int MaxIndex<T>(this IEnumerable<T> sequence) where T : IComparable<T> {
			int maxIndex = -1;
			T maxValue = default; // Immediately overwritten anyway
			int index = 0;
			foreach (T value in sequence) {
				if (value.CompareTo(maxValue) > 0 || maxIndex == -1) {
					maxIndex = index;
					maxValue = value;
				}
				index++;
			}
			return maxIndex;
		}

		public static int MinIndex<T>(this IEnumerable<T> sequence) where T : IComparable<T> {
			int minIndex = -1;
			T minValue = default; // Immediately overwritten anyway
			int index = 0;
			foreach (T value in sequence) {
				if (value.CompareTo(minValue) < 0 || minIndex == -1) {
					minIndex = index;
					minValue = value;
				}
				index++;
			}
			return minIndex;
		}
	}

}
