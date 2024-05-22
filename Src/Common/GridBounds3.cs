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
	public struct GridBounds3 : IEquatable<GridBounds3> {
		public Point3 min;
		public Point3 max;

		public bool empty { get { return max.x <= min.x || max.y <= min.y || max.z <= min.z; } }

		public Point3 size { get { return max - min; } }

		public Point3 center {
			get {
				return new Point3(
					Crd.Div(min.x + max.x, 2),
					Crd.Div(min.y + max.y, 2),
					Crd.Div(min.z + max.z, 2)
				);
			}
		}

		public static GridBounds3 Empty() {
			return new GridBounds3 {
				min = new Point3(int.MaxValue, int.MaxValue, int.MaxValue),
				max = new Point3(int.MinValue, int.MinValue, int.MinValue)
			};
		}

		public static GridBounds3 MinMax(int minX, int minY, int minZ, int maxX, int maxY, int maxZ) {
			return new GridBounds3(minX, minY, minZ, maxX - minX, maxY - minY, maxZ - minZ);
		}

		public static GridBounds3 MinMax(Point3 min, Point3 max) {
			return new GridBounds3(min, max - min);
		}

		public GridBounds3(int minX, int minY, int minZ, int width, int height, int depth) {
			min = new Point3(minX, minY, minZ);
			max = new Point3(minX + width, minY + height, minZ + depth);
		}

		public GridBounds3(Point3 min, Point3 size) {
			this.min = min;
			this.max = min + size;
		}

		public void Encapsulate(Point3 point) {
			min.x = Math.Min(min.x, point.x);
			min.y = Math.Min(min.y, point.y);
			min.z = Math.Min(min.z, point.z);
			max.x = Math.Max(max.x, point.x + 1);
			max.y = Math.Max(max.y, point.y + 1);
			max.z = Math.Max(max.z, point.z + 1);
		}

		public void Encapsulate(GridBounds3 bounds) {
			min.x = Math.Min(min.x, bounds.min.x);
			min.y = Math.Min(min.y, bounds.min.y);
			min.z = Math.Min(min.z, bounds.min.z);
			max.x = Math.Max(max.x, bounds.max.x);
			max.y = Math.Max(max.y, bounds.max.y);
			max.z = Math.Max(max.z, bounds.max.z);
		}

		public bool Contains(GridBounds3 bounds) {
			return
				min.x <= bounds.min.x &&
				min.y <= bounds.min.y &&
				min.z <= bounds.min.z &&
				max.x >= bounds.max.x &&
				max.y >= bounds.max.y &&
				max.z >= bounds.max.z;
		}

		public bool Contains(Point3 point) {
			return
				min.x <= point.x &&
				min.y <= point.y &&
				min.z <= point.z &&
				max.x > point.x &&
				max.y > point.y &&
				max.z > point.z;
		}

		public void Expand(int left, int right, int up, int down, int front, int back) {
			min.x -= left;
			min.y -= down;
			min.z -= front;
			max.x += right;
			max.y += up;
			max.z += back;
		}

		public GridBounds3 GetExpanded(int leftRight, int upDown, int frontBack) {
			GridBounds3 b = this;
			b.Expand(leftRight, leftRight, upDown, upDown, frontBack, frontBack);
			return b;
		}

		public GridBounds3 GetExpanded(int left, int right, int up, int down, int front, int back) {
			GridBounds3 b = this;
			b.Expand(left, right, up, down, front, back);
			return b;
		}

		public GridBounds3 GetDivided(Point3 scale) {
			return MinMax(
				Crd.Div(min.x, scale.x),
				Crd.Div(min.y, scale.y),
				Crd.Div(min.z, scale.z),
				Crd.DivUp(max.x, scale.x),
				Crd.DivUp(max.y, scale.y),
				Crd.DivUp(max.z, scale.z)
			);
		}

		public GridBounds3 GetIntersection(GridBounds3 other) {
			return MinMax(
				Math.Max(min.x, other.min.x),
				Math.Max(min.y, other.min.y),
				Math.Max(min.z, other.min.z),
				Math.Min(max.x, other.max.x),
				Math.Min(max.y, other.max.y),
				Math.Min(max.z, other.max.z)
			);
		}

		public GridBounds xy { get { return new GridBounds(min.xy, size.xy); } }
		public GridBounds xz { get { return new GridBounds(min.xz, size.xz); } }
		public GridBounds yz { get { return new GridBounds(min.yz, size.yz); } }

		public static bool operator ==(GridBounds3 a, GridBounds3 b) {
			return (a.min == b.min) && (a.max == b.max);
		}

		public static bool operator !=(GridBounds3 a, GridBounds3 b) {
			return (a.min != b.min) || (a.max != b.max);
		}

		public override bool Equals(object obj) {
			if (!(obj is GridBounds3 other))
				return false;
			return (min == other.min) && (max == other.max);
		}

		public bool Equals(GridBounds3 other) {
			return (min == other.min) && (max == other.max);
		}

		public override int GetHashCode() {
			return min.GetHashCode() ^ max.GetHashCode();
		}

		public override string ToString() {
			return "(min: " + min + ", max: " + max + ")";
		}
	}

}
