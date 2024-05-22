/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;

namespace Runevision.Common {

	public static class RunevisionUnityExtensions {

		// Select 2 components out of 3.
		public static Vector2 xy(this Vector3 v) { return new Vector2(v.x, v.y); }
		public static Vector2 xz(this Vector3 v) { return new Vector2(v.x, v.z); }
		public static Vector2 yz(this Vector3 v) { return new Vector2(v.y, v.z); }

		// Flatten one component out of 3.
		public static Vector3 oyz(this Vector3 v) { return new Vector3(0f, v.y, v.z); }
		public static Vector3 xoz(this Vector3 v) { return new Vector3(v.x, 0f, v.z); }
		public static Vector3 xyo(this Vector3 v) { return new Vector3(v.x, v.y, 0f); }

		// Expand 2 components to 3.
		public static Vector3 xyo(this Vector2 v) { return new Vector3(v.x, v.y, 0f); }
		public static Vector3 xoy(this Vector2 v) { return new Vector3(v.x, 0f, v.y); }
		public static Vector3 oxy(this Vector2 v) { return new Vector3(0f, v.x, v.y); }

		public static Vector3 Clamped(this Vector3 v, float length) {
			float l = v.magnitude;
			if (l > length)
				return v / l * length;
			return v;
		}

		public static void Destroy(this Object obj) {
			Object.Destroy(obj);
		}

		public static void DestroyIncludingMeshes(this GameObject go) {
			if (go == null)
				return;
			MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>();
			foreach (var filter in filters) {
				// Only destroy meshes with negative instance IDs,
				// which means they were not loaded from disk.
				if (filter.sharedMesh.GetInstanceID() < 0)
					Object.Destroy(filter.sharedMesh);
			}
			Object.Destroy(go);
		}
	}

}
