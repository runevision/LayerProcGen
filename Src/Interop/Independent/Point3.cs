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
	public partial struct Point3 : IEquatable<Point3> {
		public int x, y, z;

		public Point3(int x, int y, int z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public int this[int index] {
			get {
				switch (index) {
					case 0: return x;
					case 1: return y;
					case 2: return z;
					default:
						throw new IndexOutOfRangeException();
				}
			}

			set {
				switch (index) {
					case 0: x = value; break;
					case 1: y = value; break;
					case 2: z = value; break;
					default:
						throw new IndexOutOfRangeException();
				}
			}
		}

		public int[] array { get { return new[] { x, y, z }; } }

		public static Point3 zero = new Point3(0, 0, 0);
		public static Point3 right = new Point3(1, 0, 0);
		public static Point3 up = new Point3(0, 1, 0);
		public static Point3 forward = new Point3(0, 0, 1);
		public static Point3 one = new Point3(1, 1, 1);

		public static Point3 operator +(Point3 a, Point3 b) {
			return new Point3(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static Point3 operator -(Point3 a, Point3 b) {
			return new Point3(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static Point3 operator -(Point3 a) {
			return new Point3(-a.x, -a.y, -a.z);
		}

		public static Point3 operator *(Point3 a, int f) {
			return new Point3(a.x * f, a.y * f, a.z * f);
		}

		public static Point3 operator *(Point3 a, Point3 b) {
			return new Point3(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public static bool operator ==(Point3 a, Point3 b) {
			return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
		}

		public static bool operator !=(Point3 a, Point3 b) {
			return (a.x != b.x) || (a.y != b.y) || (a.z != b.z);
		}

		public override bool Equals(Object obj) {
			if (!(obj is Point3 other))
				return false;
			return (x == other.x) && (y == other.y) && (z == other.z);
		}

		public bool Equals(Point3 other) {
			return (x == other.x) && (y == other.y) && (z == other.z);
		}

		public Point xy { get { return new Point(x, y); } }
		public Point xz { get { return new Point(x, z); } }
		public Point yz { get { return new Point(y, z); } }

		public static Point3 RotateY(Point3 p, int quarters) {
			quarters = ((quarters % 4) + 4) % 4;
			switch (quarters) {
				case 1: return new Point3(-p.z, p.y, p.x);
				case 2: return new Point3(-p.x, p.y, -p.z);
				case 3: return new Point3(p.z, p.y, -p.x);
				default: return p;
			}
		}

		public static Point3 Cross(Point3 a, Point3 b) {
			return new Point3(
				a.y * b.z - a.z * b.y,
				a.z * b.x - a.x - b.z,
				a.x * b.y - a.y - b.x
			);
		}

		public static int Dot(Point3 a, Point3 b) {
			return a.x * b.x + a.y * b.y + a.z * b.z;
		}

		public override string ToString() {
			return $"({x},{y},{z})";
		}

		public override int GetHashCode() {
			int result = x;
			result = 1289 * result + y;
			result = 1289 * result + z;
			return result;
		}

		public static Point3 Min(Point3 a, Point3 b) {
			return new Point3(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
		}

		public static Point3 Max(Point3 a, Point3 b) {
			return new Point3(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
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
