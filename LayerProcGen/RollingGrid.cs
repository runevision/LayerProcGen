/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using System.Collections;
using System.Collections.Generic;

namespace Runevision.LayerProcGen {

	/// <summary>
	/// Represents an infinite 2D array, used by layers to store chunks in.
	/// </summary>
	public class RollingGrid<T> : IEnumerable<T> where T : class {

		struct Cell {
			public Point[] points;
			public T[] values;
			public int count;
		}

		int sizeX, sizeY;
		Cell[,] grid;
		HashSet<T> set = new HashSet<T>();

		public RollingGrid(int sizeX, int sizeY, int maxOverlap = 3) {
			this.sizeX = sizeX;
			this.sizeY = sizeY;
			grid = new Cell[sizeX, sizeY];
			for (int i = 0; i < sizeX; i++) {
				for (int j = 0; j < sizeY; j++) {
					grid[i, j].points = new Point[maxOverlap];
					grid[i, j].values = new T[maxOverlap];
				}
			}
		}

		public T this[int x, int y] {
			get {
				int modX = Crd.Mod(x, sizeX);
				int modY = Crd.Mod(y, sizeY);
				Cell cell = grid[modX, modY];
				for (int i = 0; i < cell.count; i++) {
					if (cell.points[i].x == x && cell.points[i].y == y)
						return cell.values[i];
				}
				return null;
			}
			set {
				int modX = Crd.Mod(x, sizeX);
				int modY = Crd.Mod(y, sizeY);
				Cell cell = grid[modX, modY];

				if (value == null) {
					for (int i = 0; i < cell.count; i++) {
						if (cell.points[i].x == x && cell.points[i].y == y) {
							set.Remove(cell.values[i]);
							if (cell.count > 1) {
								// When removing value, move last value into removed value's place.
								cell.points[i] = cell.points[cell.count - 1];
								cell.values[i] = cell.values[cell.count - 1];
								cell.values[cell.count - 1] = null;
							}
							else {
								cell.values[i] = null;
							}
							cell.count--;
							grid[modX, modY] = cell;
							return;
						}
					}
				}
				else {
					// Check if we already contain point
					for (int i = 0; i < cell.count; i++) {
						if (cell.points[i].x == x && cell.points[i].y == y)
							return;
					}

					// Point is new.
					// Check if there's no room.
					if (cell.count == cell.points.Length)
						throw new System.Exception($"Max overlap exceeded in {GetType()}.");

					// Store point and value
					set.Add(value);
					cell.points[cell.count] = new Point(x, y);
					cell.values[cell.count] = value;
					cell.count++;
					grid[modX, modY] = cell;
				}
			}
		}

		public T this[Point p] {
			get { return this[p.x, p.y]; }
			set { this[p.x, p.y] = value; }
		}

		public IEnumerator<T> GetEnumerator() {
			return set.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return set.GetEnumerator();
		}
	}

}
