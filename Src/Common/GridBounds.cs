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
	public struct GridBounds : IEquatable<GridBounds> {
		public Point min;
		public Point max;

		public bool empty { get { return max.x <= min.x || max.y <= min.y; } }

		public Point size { get { return max - min; } }

		public Point center { get { return new Point(Crd.Div(min.x + max.x, 2), Crd.Div(min.y + max.y, 2)); } }

		public static GridBounds Empty() {
			return new GridBounds {
				min = new Point(int.MaxValue, int.MaxValue),
				max = new Point(int.MinValue, int.MinValue)
			};
		}

		public static GridBounds MinMax(int minX, int minY, int maxX, int maxY) {
			return new GridBounds(minX, minY, maxX - minX, maxY - minY);
		}

		public static GridBounds MinMax(Point min, Point max) {
			return new GridBounds(min, max - min);
		}

		public GridBounds(int minX, int minY, int width, int height) {
			min = new Point(minX, minY);
			max = new Point(minX + width, minY + height);
		}

		public GridBounds(Point min, Point size) {
			this.min = min;
			this.max = min + size;
		}

		public void Encapsulate(Point point) {
			min.x = Math.Min(min.x, point.x);
			min.y = Math.Min(min.y, point.y);
			max.x = Math.Max(max.x, point.x + 1);
			max.y = Math.Max(max.y, point.y + 1);
		}

		public void Encapsulate(GridBounds bounds) {
			min.x = Math.Min(min.x, bounds.min.x);
			min.y = Math.Min(min.y, bounds.min.y);
			max.x = Math.Max(max.x, bounds.max.x);
			max.y = Math.Max(max.y, bounds.max.y);
		}

		public bool Contains(GridBounds bounds) {
			return
				min.x <= bounds.min.x &&
				min.y <= bounds.min.y &&
				max.x >= bounds.max.x &&
				max.y >= bounds.max.y;
		}

		public bool Contains(Point point) {
			return
				min.x <= point.x &&
				min.y <= point.y &&
				max.x > point.x &&
				max.y > point.y;
		}

		public bool Overlaps(GridBounds bounds) {
			return
				min.x <= bounds.max.x &&
				min.y <= bounds.max.y &&
				max.x >= bounds.min.x &&
				max.y >= bounds.min.y;
		}

		public void Expand(int left, int right, int up, int down) {
			min.x -= left;
			min.y -= down;
			max.x += right;
			max.y += up;
		}

		public GridBounds GetExpanded(Point padding) {
			GridBounds b = this;
			b.Expand(padding.x, padding.x, padding.y, padding.y);
			return b;
		}

		public GridBounds GetExpanded(int leftRight, int upDown) {
			GridBounds b = this;
			b.Expand(leftRight, leftRight, upDown, upDown);
			return b;
		}

		public GridBounds GetExpanded(int left, int right, int up, int down) {
			GridBounds b = this;
			b.Expand(left, right, up, down);
			return b;
		}

		public GridBounds GetDivided(Point scale) {
			return MinMax(
				Crd.Div(min.x, scale.x),
				Crd.Div(min.y, scale.y),
				Crd.DivUp(max.x, scale.x),
				Crd.DivUp(max.y, scale.y)
			);
		}

		public GridBounds GetIntersection(GridBounds other) {
			return MinMax(
				Math.Max(min.x, other.min.x),
				Math.Max(min.y, other.min.y),
				Math.Min(max.x, other.max.x),
				Math.Min(max.y, other.max.y)
			);
		}

		public static bool operator ==(GridBounds a, GridBounds b) {
			return (a.min == b.min) && (a.max == b.max);
		}

		public static bool operator !=(GridBounds a, GridBounds b) {
			return (a.min != b.min) || (a.max != b.max);
		}

		public override bool Equals(object obj) {
			if (!(obj is GridBounds other))
				return false;
			return (min == other.min) && (max == other.max);
		}

		public bool Equals(GridBounds other) {
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
