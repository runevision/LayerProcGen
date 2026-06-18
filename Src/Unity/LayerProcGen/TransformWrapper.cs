/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using UnityEngine;

namespace Runevision.LayerProcGen {

	/// <summary>
	/// Transform stand-in that chunks can create outside of the main thread.
	/// </summary>
	public class TransformWrapper : IOriginHandler {

		public Transform transform { get; private set; }

		Transform layerParent;
		Point chunkIndex;
		Point3 worldPosition;

		public TransformWrapper(Transform layerParent, Point chunkIndex, Point3 worldPosition = default) {
			this.layerParent = layerParent;
			this.chunkIndex = chunkIndex;
			this.worldPosition = worldPosition;
		}

		/// <summary>
		/// Creates the wrapper's own Transform if it doesn't exist, then adds the child.
		/// The child's position is interpreted as a local position relative to the wrapper's.
		/// </summary>
		public void AddChild(Transform child, bool matchParentLayer = false) {
			// Ignore if layerParent is null. This can sometimes happen while unloading a scene.
			if (layerParent == null)
				return;
			if (transform == null) {
				transform = new GameObject("Chunk" + chunkIndex).transform;
				transform.SetParent(layerParent, false);
				transform.position = (Vector3)(worldPosition - FloatingOrigin.origin);
				transform.gameObject.layer = layerParent.gameObject.layer;
				FloatingOrigin.RegisterHandler(this);
			}
			child.SetParent(transform, false);
			if (matchParentLayer)
				child.gameObject.layer = transform.gameObject.layer;
		}

		/// <summary>
		/// Destroys the Transform and unregisters the wrapper.
		/// Must be called from the main thread.
		/// </summary>
		public void Destroy() {
			if (transform != null)
				transform.gameObject.Destroy();
			FloatingOrigin.UnregisterHandler(this);
		}

		public void HandleOriginShift(Point3 oldOrigin, Point3 newOrigin) {
			if (transform != null)
				transform.position = (Vector3)(worldPosition - newOrigin);
		}
	}

}
