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
	/// A layer's dependency on another with a specified level and padding.
	/// </summary>
	public class LayerDependency {

		public AbstractChunkBasedDataLayer layer;
		public int level;
		public int vPadding;
		public int hPadding;

		public LayerDependency(AbstractChunkBasedDataLayer layer, int padding)
			: this(layer, padding, padding) { }
		public LayerDependency(AbstractChunkBasedDataLayer layer, int hPadding, int vPadding)
			: this(layer, hPadding, vPadding, layer.GetLevelCount() - 1) { }
		public LayerDependency(AbstractChunkBasedDataLayer layer, Point padding)
			: this(layer, padding.x, padding.y, layer.GetLevelCount() - 1) { }
		public LayerDependency(AbstractChunkBasedDataLayer layer, Point padding, int level)
			: this(layer, padding.x, padding.y, level) { }
		public LayerDependency(AbstractChunkBasedDataLayer layer, int hPadding, int vPadding, int level) {
			this.layer = layer;
			this.hPadding = hPadding;
			this.vPadding = vPadding;
			this.level = level;
		}
	}

}
