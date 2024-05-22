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

	public partial struct Point3 {

		// User-defined conversion from Point to Vector3
		public static implicit operator Vector3(Point3 p) {
			return new Vector3(p.x, p.y, p.z);
		}

		//  User-defined conversion from Vector3 to Point
		public static explicit operator Point3(Vector3 p) {
			return new Point3(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y), Mathf.FloorToInt(p.z));
		}

#if PACKAGE_UNITY_MATHEMATICS
		// User-defined conversion from Point to float3
		public static implicit operator float3(Point3 p) {
			return new float3(p.x, p.y, p.z);
		}

		//  User-defined conversion from float3 to Point
		public static explicit operator Point3(float3 p) {
			return new Point3(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y), Mathf.FloorToInt(p.z));
		}
#endif
	}

}
#endif
