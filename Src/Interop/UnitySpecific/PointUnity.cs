/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if UNITY_2019_4_OR_NEWER
#if PACKAGE_UNITY_MATHEMATICS
using Unity.Mathematics;
#endif
using UnityEngine;

namespace Runevision.Common {

	public partial struct Point {

		// User-defined conversion from Point to Vector3
		public static explicit operator Vector3(Point p) {
			return new Vector3(p.x, p.y, 0);
		}

		//  User-defined conversion from Vector3 to Point
		public static explicit operator Point(Vector3 p) {
			return new Point(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
		}

		// User-defined conversion from Point to Vector2
		public static implicit operator Vector2(Point p) {
			return new Vector2(p.x, p.y);
		}

		//  User-defined conversion from Vector2 to Point
		public static explicit operator Point(Vector2 p) {
			return new Point(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
		}

		public static Point GetRoundedPoint(Vector2 p) {
			return new Point(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));
		}

#if PACKAGE_UNITY_MATHEMATICS
		// User-defined conversion from Point to float3
		public static explicit operator float3(Point p) {
			return new float3(p.x, p.y, 0);
		}

		//  User-defined conversion from float3 to Point
		public static explicit operator Point(float3 p) {
			return new Point(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
		}

		// User-defined conversion from Point to float2
		public static implicit operator float2(Point p) {
			return new float2(p.x, p.y);
		}

		//  User-defined conversion from float2 to Point
		public static explicit operator Point(float2 p) {
			return new Point(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
		}

		public static Point GetRoundedPoint(float2 p) {
			return new Point(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));
		}
#endif
	}

}
#endif
