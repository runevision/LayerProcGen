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

	public partial struct DPoint {

		// User-defined conversion from DPoint to Vector3
		public static explicit operator Vector3(DPoint p) {
			return new Vector3((float)p.x, (float)p.y, 0);
		}

		//  User-defined conversion from Vector3 to DPoint
		public static explicit operator DPoint(Vector3 p) {
			return new DPoint(p.x, p.y);
		}

		// User-defined conversion from DPoint to Vector2
		public static explicit operator Vector2(DPoint p) {
			return new Vector2((float)p.x, (float)p.y);
		}

		//  User-defined conversion from Vector2 to DPoint
		public static implicit operator DPoint(Vector2 p) {
			return new DPoint(p.x, p.y);
		}

#if PACKAGE_UNITY_MATHEMATICS
		// User-defined conversion from DPoint to float3
		public static explicit operator float3(DPoint p) {
			return new float3((float)p.x, (float)p.y, 0);
		}

		//  User-defined conversion from float3 to DPoint
		public static explicit operator DPoint(float3 p) {
			return new DPoint(p.x, p.y);
		}

		// User-defined conversion from DPoint to float2
		public static explicit operator float2(DPoint p) {
			return new float2((float)p.x, (float)p.y);
		}

		//  User-defined conversion from float2 to DPoint
		public static implicit operator DPoint(float2 p) {
			return new DPoint(p.x, p.y);
		}
#endif
	}

}
#endif
