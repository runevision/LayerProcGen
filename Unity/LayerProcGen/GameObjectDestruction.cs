/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;

namespace Runevision.LayerProcGen {

	/// <summary>
	/// A GameObject destruction to enqueue to be performed on the main thread.
	/// </summary>
	public struct QueuedGameObjectDestruction : IQueuedAction {

		TransformWrapper transform;
		bool destroyMeshes;

		/// <summary>
		/// Called by MainThreadActionQueue.
		/// </summary>
		public void Process() {
			if (transform.transform != null) {
				if (destroyMeshes)
					transform.transform.gameObject.DestroyIncludingMeshes();
				else
					transform.transform.gameObject.Destroy();
			}
		}

		/// <summary>
		/// Enqueue destruction on the main thread of the GameObject wrapped in the TransformWrapper.
		/// </summary>
		/// <param name="tr">The TransformWrapper wrapping the Transform of the GameObject.</param>
		/// <param name="destroyMeshes">If true, MeshFilter components are searched for non-persistent meshes, and these will be destroyed too.</param>
		public static void Enqueue(TransformWrapper tr, bool destroyMeshes) {
			MainThreadActionQueue.Enqueue(new QueuedGameObjectDestruction { transform = tr, destroyMeshes = destroyMeshes });
		}
	}

}
