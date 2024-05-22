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
	public partial struct DPoint : IEquatable<DPoint> {
		public DFloat x, y;

		public DPoint(DFloat x, DFloat y) {
			this.x = x;
			this.y = y;
		}

		public DFloat this[int index] {
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

		public DFloat[] array { get { return new[] { x, y }; } }

		public static DPoint zero = new DPoint(0f, 0f);
		public static DPoint right = new DPoint(1f, 0f);
		public static DPoint up = new DPoint(0f, 1f);
		public static DPoint one = new DPoint(1f, 1f);

		// User-defined conversion from DPoint to Point
		public static explicit operator Point(DPoint p) {
			return new Point((int)p.x, (int)p.y);
		}
		//  User-defined conversion from Point to DPoint
		public static implicit operator DPoint(Point p) {
			return new DPoint(p.x, p.y);
		}

		public static DPoint operator +(DPoint a, DPoint b) {
			return new DPoint(a.x + b.x, a.y + b.y);
		}

		public static DPoint operator -(DPoint a, DPoint b) {
			return new DPoint(a.x - b.x, a.y - b.y);
		}

		public static DPoint operator -(DPoint a) {
			return new DPoint(-a.x, -a.y);
		}

		public static DPoint operator *(DPoint a, int f) {
			return new DPoint(a.x * f, a.y * f);
		}

		public static DPoint operator *(DPoint a, Point b) {
			return new DPoint(a.x * b.x, a.y * b.y);
		}

		public static DPoint operator *(Point a, DPoint b) {
			return new DPoint(a.x * b.x, a.y * b.y);
		}

		public static DPoint operator /(DPoint a, int f) {
			return new DPoint(a.x / f, a.y / f);
		}

		public static bool operator ==(DPoint a, DPoint b) {
			return (a.x.value == b.x.value) && (a.y.value == b.y.value);
		}

		public static bool operator !=(DPoint a, DPoint b) {
			return (a.x.value != b.x.value) || (a.y.value != b.y.value);
		}

		public override bool Equals(object obj) {
			if (!(obj is DPoint other))
				return false;
			return (x == other.x) && (y == other.y);
		}

		public bool Equals(DPoint other) {
			return (x == other.x) && (y == other.y);
		}

		public DPoint3 xyo { get { return new DPoint3(x, y, 0); } }
		public DPoint3 xoy { get { return new DPoint3(x, 0, y); } }
		public DPoint3 oxy { get { return new DPoint3(0, x, y); } }

		public override string ToString() {
			return $"({x},{y})";
		}

		public override int GetHashCode() {
			int result = 373;
			result = 37 * result + x.value;
			result = 37 * result + y.value;
			return result;
		}

		public static DPoint Min(DPoint a, DPoint b) {
			return new DPoint(DFloat.Min(a.x, b.x), DFloat.Min(a.y, b.y));
		}

		public static DPoint Max(DPoint a, DPoint b) {
			return new DPoint(DFloat.Max(a.x, b.x), DFloat.Max(a.y, b.y));
		}

		public DFloat sqrMagnitude {
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
