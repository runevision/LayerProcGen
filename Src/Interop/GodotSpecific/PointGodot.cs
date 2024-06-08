/*
 * Copyright (c) 2024 Rune Skovbo Johansen, Sythelux Rikd
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if GODOT4
using Godot;

namespace Runevision.Common;

public partial struct Point {

	// User-defined conversion from Point to Vector3
	public static explicit operator Vector3(Point p) {
		return new Vector3(p.x, p.y, 0);
	}

	//  User-defined conversion from Vector3 to Point
	public static explicit operator Point(Vector3 p) {
		return new Point(Mathf.FloorToInt(p.X), Mathf.FloorToInt(p.Y));
	}

	// User-defined conversion from Point to Vector2
	public static implicit operator Vector2(Point p) {
		return new Vector2(p.x, p.y);
	}

	//  User-defined conversion from Vector2 to Point
	public static explicit operator Point(Vector2 p) {
		return new Point(Mathf.FloorToInt(p.X), Mathf.FloorToInt(p.Y));
	}

	public static Point GetRoundedPoint(Vector2 p) {
		return new Point(Mathf.RoundToInt(p.X), Mathf.RoundToInt(p.Y));
	}
}
#endif
