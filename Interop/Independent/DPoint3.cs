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
	public partial struct DPoint3 : IEquatable<DPoint3> {
		public DFloat x, y, z;

		public DPoint3(DFloat x, DFloat y, DFloat z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public DFloat this[int index] {
			get {
				switch (index) {
					case 0: return x;
					case 1: return y;
					case 2: return z;
					default: throw new IndexOutOfRangeException();
				}
			}
			set {
				switch (index) {
					case 0: x = value; break;
					case 1: y = value; break;
					case 2: z = value; break;
					default: throw new IndexOutOfRangeException();
				}
			}
		}

		public DFloat[] array { get { return new[] { x, y, z }; } }

		public static DPoint3 zero = new DPoint3(0f, 0f, 0f);
		public static DPoint3 right = new DPoint3(1f, 0f, 0f);
		public static DPoint3 up = new DPoint3(0f, 1f, 0f);
		public static DPoint3 forward = new DPoint3(0f, 0f, 1f);
		public static DPoint3 one = new DPoint3(1f, 1f, 1f);

		// User-defined conversion from DPoint3 to Point3
		public static explicit operator Point3(DPoint3 p) {
			return new Point3((int)p.x, (int)p.y, (int)p.z);
		}
		//  User-defined conversion from Point3 to DPoint3
		public static implicit operator DPoint3(Point3 p) {
			return new DPoint3(p.x, p.y, p.z);
		}

		public static DPoint3 operator +(DPoint3 a, DPoint3 b) {
			return new DPoint3(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static DPoint3 operator -(DPoint3 a, DPoint3 b) {
			return new DPoint3(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static DPoint3 operator -(DPoint3 a) {
			return new DPoint3(-a.x, -a.y, -a.z);
		}

		public static DPoint3 operator *(DPoint3 a, int f) {
			return new DPoint3(a.x * f, a.y * f, a.z * f);
		}

		public static DPoint3 operator *(DPoint3 a, Point3 b) {
			return new DPoint3(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public static DPoint3 operator *(Point3 a, DPoint3 b) {
			return new DPoint3(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public static DPoint3 operator /(DPoint3 a, int f) {
			return new DPoint3(a.x / f, a.y / f, a.z / f);
		}

		public static bool operator ==(DPoint3 a, DPoint3 b) {
			return (a.x.value == b.x.value) && (a.y.value == b.y.value) && (a.z.value == b.z.value);
		}

		public static bool operator !=(DPoint3 a, DPoint3 b) {
			return (a.x.value != b.x.value) || (a.y.value != b.y.value) || (a.z.value != b.z.value);
		}

		public override bool Equals(object obj) {
			if (!(obj is DPoint3 other))
				return false;
			return (x == other.x) && (y == other.y) && (z == other.z);
		}

		public bool Equals(DPoint3 other) {
			return (x == other.x) && (y == other.y) && (z == other.z);
		}

		public DPoint xy { get { return new DPoint(x, y); } }
		public DPoint xz { get { return new DPoint(x, z); } }
		public DPoint yz { get { return new DPoint(y, z); } }

		public override string ToString() {
			return $"({x},{y},{z})";
		}

		public override int GetHashCode() {
			int result = 373;
			result = 37 * result + x.value;
			result = 37 * result + y.value;
			result = 37 * result + z.value;
			return result;
		}

		public static DPoint3 Min(DPoint3 a, DPoint3 b) {
			return new DPoint3(DFloat.Min(a.x, b.x), DFloat.Min(a.y, b.y), DFloat.Min(a.z, b.z));
		}

		public static DPoint3 Max(DPoint3 a, DPoint3 b) {
			return new DPoint3(DFloat.Max(a.x, b.x), DFloat.Max(a.y, b.y), DFloat.Max(a.z, b.z));
		}

		public DFloat sqrMagnitude {
			get {
				return x * x + y * y + z * z;
			}
		}

		public float magnitude { get { return (float)Math.Sqrt(sqrMagnitude); } }

		public static float Distance(DPoint3 a, DPoint3 b) {
			return (b - a).magnitude;
		}
	}

}
