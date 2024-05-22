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

	public partial struct DPoint3 {

		// User-defined conversion from DPoint3 to Vector3
		public static explicit operator Vector3(DPoint3 p) {
			return new Vector3((float)p.x, (float)p.y, (float)p.z);
		}

		//  User-defined conversion from Vector3 to DPoint3
		public static implicit operator DPoint3(Vector3 p) {
			return new DPoint3(p.x, p.y, p.z);
		}

#if PACKAGE_UNITY_MATHEMATICS
		// User-defined conversion from DPoint3 to float3
		public static explicit operator float3(DPoint3 p) {
			return new float3((float)p.x, (float)p.y, (float)p.z);
		}

		//  User-defined conversion from float3 to DPoint3
		public static implicit operator DPoint3(float3 p) {
			return new DPoint3(p.x, p.y, p.z);
		}
#endif
	}

}
#endif
