/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace Runevision.Common {

	[Serializable]
	public partial struct Point : IEquatable<Point> {
		public int x, y;

		public Point(int x, int y) {
			this.x = x;
			this.y = y;
		}

		public int this[int index] {
			get {
				switch (index) {
					case 0: return x;
					case 1: return y;
					default: throw new IndexOutOfRangeException();
				}
			}
			set {
				switch (index) {
					case 0: x = value; break;
					case 1: y = value; break;
					default: throw new IndexOutOfRangeException();
				}
			}
		}

		public int[] array { get { return new[] { x, y }; } }

		public static Point zero = new Point(0, 0);
		public static Point right = new Point(1, 0);
		public static Point up = new Point(0, 1);
		public static Point one = new Point(1, 1);

		//  User-defined conversion from Point3 to Point
		public static explicit operator Point(Point3 p) {
			return new Point(p.x, p.y);
		}

		public static Point operator +(Point a, Point b) {
			return new Point(a.x + b.x, a.y + b.y);
		}

		public static Point operator -(Point a, Point b) {
			return new Point(a.x - b.x, a.y - b.y);
		}

		public static Point operator -(Point a) {
			return new Point(-a.x, -a.y);
		}

		public static Point operator *(Point a, int f) {
			return new Point(a.x * f, a.y * f);
		}

		public static Point operator /(Point a, int f) {
			return new Point(a.x / f, a.y / f);
		}

		public static Point operator *(Point a, Point b) {
			return new Point(a.x * b.x, a.y * b.y);
		}

		public static Point operator /(Point a, Point b) {
			return new Point(a.x / b.x, a.y / b.y);
		}

		public static bool operator ==(Point a, Point b) {
			return (a.x == b.x) && (a.y == b.y);
		}

		public static bool operator !=(Point a, Point b) {
			return (a.x != b.x) || (a.y != b.y);
		}

		public override bool Equals(object obj) {
			if (!(obj is Point other))
				return false;
			return (x == other.x) && (y == other.y);
		}

		public bool Equals(Point other) {
			return (x == other.x) && (y == other.y);
		}

		public Point3 xyo { get { return new Point3(x, y, 0); } }
		public Point3 xoy { get { return new Point3(x, 0, y); } }
		public Point3 oxy { get { return new Point3(0, x, y); } }

		public static Point Rotate(Point p, int quarters) {
			quarters = ((quarters % 4) + 4) % 4;
			switch (quarters) {
				case 1: return new Point(-p.y, p.x);
				case 2: return new Point(-p.x, -p.y);
				case 3: return new Point(p.y, -p.x);
				default: return p;
			}
		}

		public override string ToString() {
			return $"({x},{y})";
		}

		public override int GetHashCode() {
			int result = x;
			result = 46309 * result + y;
			return result;
		}

		public string GetASCII() {
			return "" +
				(char)(x / 64 + 32) + (char)(x % 64 + 32) +
				(char)(y / 64 + 32) + (char)(y % 64 + 32);
		}

		public static Point FromASCII(string ascii) {
			return new Point(
				 (ascii[0] - 32) * 64 + (ascii[1] - 32),
				 (ascii[2] - 32) * 64 + (ascii[3] - 32)
			);
		}

		public static Point Min(Point a, Point b) {
			return new Point(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
		}

		public static Point Max(Point a, Point b) {
			return new Point(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
		}

		public int sqrMagnitude {
			get {
				return x * x + y * y;
			}
		}

		public float magnitude { get { return (float)Math.Sqrt(sqrMagnitude); } }

		public static float Distance(DPoint a, DPoint b) {
			return (b - a).magnitude;
		}
	}

}
