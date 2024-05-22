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
	/// Unity component that creates a <see cref="TopLayerDependency"/>.
	/// </summary>
	public class GenerationSource : MonoBehaviour {

		public LayerNamedReference layer;
		public Point size = Point.one;

		public TopLayerDependency dep { get; private set; }

		void OnEnable() {
			UpdateState();
		}

		void OnDisable() {
			if (dep != null)
				dep.isActive = false;
		}

		void UpdateState() {
			if (layer == null)
				return;

			// Get layer instance.
			AbstractChunkBasedDataLayer instance = layer.GetLayerInstance();

			// Create top layer dependency based on layer.
			if (instance != null && (dep == null || dep.layer != instance)) {
				if (dep != null)
					dep.isActive = false;
				dep = new TopLayerDependency(instance, size);
			}
		}

		void Update() {
			UpdateState();
			if (dep == null)
				return;

			Vector3 focusPos = transform.position;
			Point focus;
			if (LayerManagerBehavior.instance.generationPlane == LayerManagerBehavior.GenerationPlane.XZ)
				focus = (Point)(focusPos.xz());
			else
				focus = (Point)(focusPos.xy());
			dep.SetFocus(focus);
			dep.SetSize(Point.Max(Point.one, size));
		}
	}

}
