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
	public class TransformWrapper {

		public Transform transform { get; private set; }

		Transform layerParent;
		Point chunkIndex;

		public TransformWrapper(Transform layerParent, Point chunkIndex) {
			this.layerParent = layerParent;
			this.chunkIndex = chunkIndex;
		}

		/// <summary>
		/// Creates the wrapper's own Transform if it doesn't exist, then adds the child.
		/// </summary>
		public void AddChild(Transform child) {
			if (transform == null) {
				transform = new GameObject("Chunk" + chunkIndex).transform;
				transform.SetParent(layerParent, false);
				transform.gameObject.layer = layerParent.gameObject.layer;
			}
			child.SetParent(transform, false);
			child.gameObject.layer = transform.gameObject.layer;
		}
	}

}
