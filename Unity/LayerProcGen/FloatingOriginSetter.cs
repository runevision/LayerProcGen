/*
 * Copyright (c) 2026 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using UnityEngine;

namespace Runevision.LayerProcGen {

	/// <summary>
	/// Unity component that can move the <see cref="FloatingOrigin"/>
	/// based on the given transform position.
	/// </summary>
	public class FloatingOriginSetter : MonoBehaviour {

		/// <summary>
		/// The current origin as a 2D point in the generation plane.
		/// </summary>
		public Point origin;

		/// <summary>
		/// The origin is shifted if the transform position is further than this
		/// distance from the current origin (along the cardinal directions in
		/// the generation plane).
		/// </summary>
		public int incrementDist = 1000;

		bool useXY { get {
			LayerManagerBehavior instance = LayerManagerBehavior.instance;
			if (!instance) {
				Debug.LogError("No LayerManagerBehavior component present in scene. Cannot determine generation plane.", this);
				return false;
			}
			return instance.generationPlane == LayerManagerBehavior.GenerationPlane.XY;
		}}

		/// <summary>
		/// The origin as a 3D position where one axis is zero. This function
		/// depends on a LayerManagerBehavior being present in the scene
		/// to determine whether generation happens in the XY or XZ plane.
		/// </summary>
		public Point3 originAs3D {
			get { return useXY ? origin.xyo : origin.xoy; }
		}

		void OnEnable() {
			FloatingOrigin.SetInitialOrigin(originAs3D);
		}

		void FixedUpdate() {
			Vector3 pos3D = transform.position;
			Vector2 pos = useXY ? pos3D.xy() : pos3D.xz();

			// Check if the transform position has exceeded the increment range.
			if (Mathf.Abs(pos.x) < incrementDist && Mathf.Abs(pos.y) < incrementDist)
				return;

			// Calculate the nearest increment multiple.
			// This is better than incrementing only where the position has exceeded
			// the increment distance, since it will prevent two consecutive shifts
			// (one on each axis) shortly after each other.
			Point inc = new Point(
				Mathf.RoundToInt(pos.x / incrementDist),
				Mathf.RoundToInt(pos.y / incrementDist));

			// Get the current authoritative floating origin and add the increment.
			origin = useXY ? FloatingOrigin.origin.xy : FloatingOrigin.origin.xz;
			origin += inc * incrementDist;

			// Get the 2D point as a Point3.
			Point3 newOrigin = originAs3D;

			// Calculate the expected new position for this transform.
			Point3 delta = newOrigin - FloatingOrigin.origin;
			Vector3 expectedNewSelfPos = transform.position - (Vector3)delta;

			// Set the floating origin to the new origin.
			FloatingOrigin.SetOrigin(newOrigin);

			// Handle the origin shift of this transform if it wasn't done
			// by an origin handler.
			if ((transform.position - expectedNewSelfPos).sqrMagnitude > 0.1f) {
				transform.position = expectedNewSelfPos;
			}
		}
	}

}
