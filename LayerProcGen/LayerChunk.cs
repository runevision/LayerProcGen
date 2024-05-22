/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using Runevision.SaveState;
using System.Collections.Generic;

namespace Runevision.LayerProcGen {

	/// <summary>
	/// Internal. A level of a chunk, or alternatively a root usage of a layer.
	/// </summary>
	internal class ChunkLevelData : IPoolable {
		/// <summary>
		/// A provider chunk and level that another chunk and level depends on.
		/// </summary>
		internal struct ProviderStruct {
			public AbstractLayerChunk chunk;
			public int level;
			public ProviderStruct(AbstractLayerChunk chunk, int level) {
				this.chunk = chunk;
				this.level = level;
			}
		}

		/// The providers that this level of this chunk depends on.
		internal List<ProviderStruct> providers = new List<ProviderStruct>();

		/// How many other "users" depend on this level of this chunk.
		internal int userCount;

		/// Called when recycled into pool.
		public void Reset() {
			providers.Clear();
			userCount = 0;
		}
	}

	/// <summary>
	/// Represents a layer "user" that requires data within the specified bounds.
	/// </summary>
	public interface ILC {
		AbstractChunkBasedDataLayer abstractLayer { get; }
		GridBounds bounds { get; }
	}

	/// <summary>
	/// Internal. Non-generic class that LayerChunk inherits from.
	/// </summary>
	public abstract class AbstractLayerChunk : ILC, IPoolable {
		/// <summary>
		/// The coordinate index this chunk is for.
		/// </summary>
		/// <remarks>
		/// A 2D index where consecutive chunks have consecutive indexes.
		/// The chunk with its lower left corner at the world origin will have index (0, 0).
		/// Its neighbor to the left will have index (-1, 0) and to the right (1, 0) etc.
		/// </remarks>
		public Point index { get; internal set; }

		/// <summary>
		/// The level the chunk is currently generated up to (zero-based).
		/// </summary>
		/// <remarks>
		/// This property is not updated to a higher level until the call to Create has finished,
		/// so do not check it inside the Create method.
		/// Use the level parameter of the Create method instead.
		/// </remarks>
		public int level { get; internal set; } = -1;

		/// <summary>
		/// The layer for this chunk type. In derived classes the layer property can be used instead.
		/// </summary>
		public abstract AbstractChunkBasedDataLayer abstractLayer { get; }

		/// <summary>
		/// The position in world space units of the lower left corner of this chunk.
		/// </summary>
		/// <remarks>
		/// Based on index * layer.chunkSize.
		/// </remarks>
		public Point worldOffset {
			get {
				return new Point(index.x * abstractLayer.chunkW, index.y * abstractLayer.chunkH);
			}
		}

		/// <summary>
		/// The bounds in world space units of this chunk.
		/// </summary>
		/// <remarks>
		/// Based on worldOffset and layer.chunkSize.
		/// </remarks>
		public GridBounds bounds {
			get {
				return new GridBounds(worldOffset, abstractLayer.chunkSize);
			}
		}

		// Handle used for profiling.
		protected internal SimpleProfiler.ProfilerHandle phc { get; internal set; }
		// States used for save state handling in derived classes.
		protected internal List<StateObject> states { get; internal set; } = new List<StateObject>();

		// A lock per level, used for multithreading purposes.
		internal object[] levelLocks;

		// Each level of the chunk keeps track of what things it depends on,
		// and how many things depends on itself.
		ChunkLevelData[] chunkLevels;

		internal ChunkLevelData GetLevelData(int requestedLevel) {
			return chunkLevels[requestedLevel];
		}

		internal void SetLevelData(ChunkLevelData dependency, int requestedLevel) {
			chunkLevels[requestedLevel] = dependency;
		}

		internal AbstractLayerChunk() {
			int levelCount = abstractLayer.GetLevelCount();

			chunkLevels = new ChunkLevelData[levelCount];
			levelLocks = new object[levelCount];
			for (int i = 0; i < levelCount; i++) {
				levelLocks[i] = new object();
			}
		}

		internal void IncrementUserCountOfLevel(int requestedLevel) {
			lock (chunkLevels) {
				chunkLevels[requestedLevel].userCount++;
			}
		}

		internal void DecrementUserCountOfLevel(int requestedLevel) {
			lock (chunkLevels) {
				chunkLevels[requestedLevel].userCount--;
			}
			if (chunkLevels[requestedLevel].userCount <= 0)
				abstractLayer.RemoveChunkLevel(index, requestedLevel);
		}

		/// <summary>
		/// Called by the pool when this <see cref="IPoolable"/> object is returned to the pool.
		/// </summary>
		/// <remarks>
		/// AbstractLayerChunk resets internal chunk state in this method.
		/// Derived classes generally don't need to override this method as cleanup should
		/// happen in the Create method when the destroy parameter is true.
		/// If overriding this method anyway, ensure the the base method is called.
		/// </remarks>
		public virtual void Reset() {
			int levelCount = abstractLayer.GetLevelCount();
			for (int i = 0; i < levelCount; i++) {
				chunkLevels[i] = null;
			}
		}

		/// <summary>
		/// Create or destroy the specified level of this chunk.
		/// </summary>
		/// <param name="level">The level of the chunk to create or destroy.</param>
		/// <param name="destroy">True if destroying, false if creating.</param>
		/// <remarks>
		/// The central method for procedural generation of the chunk.
		/// </remarks>
		/// <example>
		/// If the chunk has only one level, the following is a useful pattern for the method.
		/// Placing the destruction code before the creation code will tend to place
		/// lines of code related to resource use closer to each other.
		/// <code>
		/// public override void Create(int level, bool destroy) {
		/// 	if (destroy) {
		/// 		// Destroy data for this chunk (or return to pools where applicable).
		/// 	}
		/// 	else {
		/// 		// Generate data for this chunk.
		/// 	}
		/// }
		/// </code>
		/// </example>
		/// <example>
		/// If the chunk has multiple levels, the following is a useful pattern for the method:
		/// <code>
		/// public override void Create(int level, bool destroy) {
		/// 	if (level == 0) {
		/// 		if (destroy) {
		/// 			// Destroy data for level 0 of this chunk (or return to pools where applicable).
		/// 		}
		/// 		else {
		/// 			// Generate data for level 0 of this chunk.
		/// 		}
		/// 	}
		/// 	if (level == 1) {
		/// 		if (destroy) {
		/// 			// Destroy data for level 1 of this chunk (or return to pools where applicable).
		/// 		}
		/// 		else {
		/// 			// Generate data for level 1 of this chunk.
		/// 		}
		/// 	}
		/// }
		/// </code>
		/// </example>
		public virtual void Create(int level, bool destroy) { }

		/// <summary>
		/// Example output: "[TerrainChunk (3,-4) level 0]"
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return $"[{GetType().PrettyName()} {index} level {level}]";
		}
	}

	/// <summary>
	/// In a layer-and-chunk pair of classes, the chunk inherits from this class.
	/// </summary>
	/// <typeparam name="L">The corresponding layer class.</typeparam>
	/// <typeparam name="C">The chunk class itself.</typeparam>
	/// <remarks>
	/// A layer contains a grid of chunks and takes care of generating and destroying them as appropriate.
	/// Each chunk generates and destroys its own data in its <see cref="Create"/> method.
	/// </remarks>
	public abstract class LayerChunk<L, C> : AbstractLayerChunk
		where L : ChunkBasedDataLayer<L, C>, new()
		where C : LayerChunk<L, C>, new()
	{
		/// <summary>
		/// The layer for this chunk type.
		/// </summary>
		/// <remarks>
		/// Typically the chunk will often reference its layer to get the chunkSize property,
		/// and other chunk-related properties specified in a layer due to them being
		/// identical for all chunks of that layer.
		/// </remarks>
		public L layer { get { return ChunkBasedDataLayer<L, C>.instance; } }

		/// <summary>
		/// Needed for C# covariance reasons. The layer property can be used instead.
		/// </summary>
		public override AbstractChunkBasedDataLayer abstractLayer { get { return layer; } }
	}

}
