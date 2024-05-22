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
	/// All generation starts with one or more top level dependencies at the root.
	/// </summary>
	public class TopLayerDependency {

		public IChunkBasedDataLayer layer { get { return (IChunkBasedDataLayer)abstractLayer; } }
		public AbstractChunkBasedDataLayer abstractLayer { get; private set; }
		public int level { get; private set; }
		public Point focus { get; private set; }
		public Point size { get; private set; }
		public GridBounds chunkIndices { get; private set; }
		public bool changed { get; private set; }

		internal ChunkLevelData currentRootUsage;

		bool active;

		public bool isActive {
			get { return active; }
			set {
				if (active != value) {
					active = value;
					changed = true;
					if (active)
						LayerManager.instance.AddTopDependency(this);
				}
			}
		}

		public TopLayerDependency(AbstractChunkBasedDataLayer layer, Point size, int level) {
			abstractLayer = layer;
			this.size = size;
			this.level = level;
		}

		public TopLayerDependency(AbstractChunkBasedDataLayer layer, Point size)
			: this(layer, size, layer.GetLevelCount() - 1) { }

		public void SetFocus(Point focus) {
			if (isActive && focus == this.focus)
				return;
			this.focus = focus;
			UpdateLayerIndices();
		}

		public void SetSize(Point size) {
			if (isActive && size == this.size)
				return;
			this.size = size;
			UpdateLayerIndices();
		}

		public void SetPadding(Point padding) {
			if (isActive && padding * 2 == size)
				return;
			size = padding * 2;
			UpdateLayerIndices();
		}

		void UpdateLayerIndices() {
			GridBounds oldIndices = chunkIndices;
			GridBounds bounds = new GridBounds(focus - size / 2, size);
			chunkIndices = bounds.GetDivided(layer.chunkSize);
			if (chunkIndices != oldIndices || !isActive)
				changed = true;
			isActive = true;
		}

		internal void GetPendingBounds(out GridBounds bounds, out int level) {
			changed = false;
			bounds = new GridBounds(focus - size / 2, size);
			level = this.level;
		}
	}

}
