/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using Runevision.SaveState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Runevision.LayerProcGen {

	public interface IChunkBasedDataLayer {
		int chunkW { get; }
		int chunkH { get; }
		Point chunkSize { get; }
		int GetLevelCount();

		void HandleDependenciesForLevel(int level, Action<LayerDependency> func);
		void HandleAllAbstractChunks(int minChunkLevel, Action<AbstractLayerChunk> func);

		/// <summary>
		/// Returns true if the layer is loaded at the highest level
		/// at the specified <paramref name="position"/> in world units.
		/// </summary>
		bool IsLoadedAtPosition(DPoint position);

		/// <summary>
		/// Returns true if the layer is loaded at least up to <paramref name="level"/>
		/// at the specified <paramref name="position"/> in world units.
		/// </summary>
		bool IsLoadedAtPosition(DPoint position, int level);
	}
	
	/// <summary>
	/// Internal. Non-generic class that ChunkBasedDataLayer inherits from.
	/// </summary>
	public abstract class AbstractChunkBasedDataLayer : AbstractDataLayer {
		public abstract int chunkW { get; }
		public abstract int chunkH { get; }
		public Point chunkSize { get { return new Point(chunkW, chunkH); } }

		/// <summary>
		/// Override to specify the number of generation levels in the layer. Default is 1.
		/// </summary>
		public virtual int GetLevelCount() { return 1; }

		internal AbstractChunkBasedDataLayer() { }

		internal abstract void ProcessTopDependency(TopLayerDependency dep);

		internal abstract void RemoveChunkLevel(Point index, int level);

		internal abstract void EnsureLoadedInBounds(GridBounds bounds, int level, ChunkLevelData levelData);
	}

	/// <summary>
	/// In a layer-and-chunk pair of classes, the layer inherits from this class.
	/// </summary>
	/// <typeparam name="L">The layer class itself.</typeparam>
	/// <typeparam name="C">The corresponding chunk class.</typeparam>
	// Generic class. Things that depend on the type of the chunks (such as the chunks grid)
	// have to be handled here rather than in the abstract class.
	public abstract class ChunkBasedDataLayer<L, C> : AbstractChunkBasedDataLayer, IChunkBasedDataLayer
		where L : ChunkBasedDataLayer<L, C>, new()
		where C : LayerChunk<L, C>, new()
	{
		static L s_Instance;
		public static L instance {
			get {
				if (s_Instance == null)
					s_Instance = new L();
				return s_Instance;
			}
		}

		internal override void ResetInstance() {
			s_Instance = null;
		}

		protected readonly RollingGrid<C> chunks;

		protected readonly List<LayerDependency>[] dependencies;

		/// <summary>
		/// Call from constructor to add a dependency on another layer.
		/// The dependency is added to the lowest level of the current layer.
		/// </summary>
		protected void AddLayerDependency(LayerDependency dependency) {
			dependencies[0].Add(dependency);
		}

		/// <summary>
		/// Call from constructor to add a dependency on another layer.
		/// The dependency is added to the specified <paramref name="ownLevel"/> of the current layer.
		/// </summary>
		protected void AddLayerDependency(int ownLevel, LayerDependency dependency) {
			dependencies[ownLevel].Add(dependency);
		}
		
		/// <summary>
		/// The layer constructor in inherited classes can be used to setup dependencies on other layers.
		/// </summary>
		/// <param name="rollingGridWidth">The width of the rolling grid 
		/// the chunks are stored in. The default is 32.</param>
		/// <param name="rollingGridHeight">The height of the rolling grid 
		/// the chunks are stored in. If set to 0, the value of rollingGridWidth is used.
		/// The default is 0.</param>
		/// <param name="rollingGridMaxOverlap">The max overlap of the rolling grid 
		/// the chunks are stored in. The default is 3.</param>
		protected ChunkBasedDataLayer(
			int rollingGridWidth = 32, int rollingGridHeight = 0, int rollingGridMaxOverlap = 3
		) {
			if (rollingGridHeight == 0)
				rollingGridHeight = rollingGridWidth;
			chunks = new RollingGrid<C>(rollingGridWidth, rollingGridHeight, rollingGridMaxOverlap);

			int levelCount = GetLevelCount();
			dependencies = new List<LayerDependency>[levelCount];
			for (int i = 0; i < levelCount; i++)
				dependencies[i] = new List<LayerDependency>();
		}

		/// <summary>
		/// Try to get the chunk at the specified index.
		/// </summary>
		/// <param name="index">An integer coordinate that indexes into the grid of chunks.</param>
		/// <param name="chunk">The requested chunk, or null.</param>
		/// <returns>True if the chunk exists and has been generated to at least level 0.</returns>
		protected bool TryGetChunk(Point index, out C chunk) {
			chunk = chunks[index];
			return (chunk != null && chunk.level >= 0);
		}

		void CreateAndRegisterChunk(Point index, int level) {
			ChunkLevelData levelData = ObjectPool<ChunkLevelData>.GlobalGet();
			EnsureChunkProviders(index, level, levelData);

			if (LayerManager.instance.aborting)
				return;

			var ph = SimpleProfiler.Begin($"{GetType().Name} {level} Chunk");
			C chunk = chunks[index];
			if (chunk == null) {
				Logg.LogError("Chunk is null in CreateAndRegisterChunk");
			}

			if (chunk.level < level) {
				if (chunk.level != level - 1)
					Logg.LogError($"{chunk}: raising internal level from {chunk.level} to {level}");
				chunk.phc = ph;
				chunk.Create(level, false);
				lock (chunks) {
					chunk.level = level;
					chunk.SetLevelData(levelData, level);
				}
			}
			SimpleProfiler.End(ph);
		}

		internal sealed override void RemoveChunkLevel(Point index, int level) {
			C chunk = chunks[index];
			if (chunk != null) {
				if (level == 0) {
					lock (chunks) {
						foreach (StateObject state in chunk.states)
							state.Unload();
						chunks[index] = null;
					}
				}

				ChunkLevelData levelData = chunk.GetLevelData(level);
				if (chunk.level != level)
					Logg.LogError($"{chunk}: lowering internal level from {chunk.level} to {level - 1}");
				lock (chunks) {
					chunk.level = level - 1;
				}
				chunk.Create(level, true);
				foreach (ChunkLevelData.ProviderStruct provider in levelData.providers)
					provider.chunk.DecrementUserCountOfLevel(provider.level);

				ObjectPool<ChunkLevelData>.GlobalReturn(ref levelData);

				if (level == 0)
					ObjectPool<C>.GlobalReturn(ref chunk);
			}
			else {
				Logg.LogError ($"Chunk {index} is already null in {GetType ().Name}.");
			}
		}

		void EnsureChunkProviders(Point index, int level, ChunkLevelData levelData) {
			GridBounds chunkBounds = new GridBounds(index.x * chunkW, index.y * chunkH, chunkW, chunkH);

			// Internal dependency on lower level of own layer.
			if (level > 0) {
				GridBounds requiredBoundsInternal = chunkBounds;
				requiredBoundsInternal.Expand(1, 1, 1, 1);
				EnsureLoadedInBounds(requiredBoundsInternal, level - 1, levelData);
			}

			// External dependencies on other layers.
			foreach (LayerDependency dependency in dependencies[level]) {
				GridBounds requiredBounds = chunkBounds;
				requiredBounds.Expand(dependency.hPadding, dependency.hPadding, dependency.vPadding, dependency.vPadding);
				dependency.layer.EnsureLoadedInBounds(requiredBounds, dependency.level, levelData);
			}
		}

		internal sealed override void ProcessTopDependency(TopLayerDependency dep) {
			dep.GetPendingBounds(out GridBounds requiredBounds, out int requiredLevel);
			ChunkLevelData oldRootUsage = dep.currentRootUsage;

			if (dep.isActive) {
				ChunkLevelData newRootUsage = ObjectPool<ChunkLevelData>.GlobalGet();

				// Load according to new dependencies.
				EnsureLoadedInBounds(requiredBounds, requiredLevel, newRootUsage);

				dep.currentRootUsage = newRootUsage;
			}
			else {
				dep.currentRootUsage = null;
			}

			if (oldRootUsage != null) {
				// Release old dependencies.
				foreach (ChunkLevelData.ProviderStruct provider in oldRootUsage.providers)
					provider.chunk.DecrementUserCountOfLevel(provider.level);

				// Remove old dependencies.
				ObjectPool<ChunkLevelData>.GlobalReturn(ref oldRootUsage);
			}
		}

		internal sealed override void EnsureLoadedInBounds(GridBounds bounds, int level, ChunkLevelData levelData) {
			if (LayerManager.instance.aborting)
				return;

			// Load inside bounds.
			GridBounds indices = bounds.GetDivided(new Point(chunkW, chunkH));
			List<Point> createIndices = new List<Point>();
			List<Point> dependIndices = new List<Point>();
			for (int x = indices.min.x; x < indices.max.x; x++) {
				for (int y = indices.min.y; y < indices.max.y; y++) {
					Point index = new Point(x, y);
					dependIndices.Add(index);
					C chunk;
					lock (chunks) {
						chunk = chunks[index];
						if (chunk == null) {
							chunk = ObjectPool<C>.GlobalGet();
							chunk.index = index;
							chunks[index] = chunk;
						}
					}
					if (chunk.level < level) {
						createIndices.Add(index);
						WorkTracker.AddWorkNeeded(1, GetType());
					}
				}
			}

			Point center = bounds.center;
			createIndices = createIndices.OrderBy(i => Math.Pow(i.x * chunkW - center.x, 2) + Math.Pow(i.y * chunkH - center.y, 2)).ToList();

			if (!LayerManager.instance.useParallelThreads) {
				foreach (Point index in createIndices) {
					CreateAndRegisterChunk(index, level);
					WorkTracker.AddWorkDone(1, GetType());
				}
			}
			else {
				Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(createIndices),
					index => {
						if (LayerManager.instance.aborting)
							return;
						SimpleProfiler.BeginThread("Gen", $"{GetType().Name} {level} {index}");
						lock (chunks[index].levelLocks[level]) {
							if (chunks[index].level < level)
								CreateAndRegisterChunk(index, level);
							WorkTracker.AddWorkDone(1, GetType());
						}
						SimpleProfiler.EndThread();
					});
			}

			if (LayerManager.instance.aborting)
				return;

			foreach (Point index in dependIndices) {
				C chunk = chunks[index];
				chunk.IncrementUserCountOfLevel(level);
				levelData.providers.Add(new ChunkLevelData.ProviderStruct(chunk, level));
			}
		}

		/// <summary>
		/// Call this method if a chunk <paramref name="q"/> has called methods that
		/// rely on other chunks that are not currently generated.
		/// Based on the bounds of q and the <paramref name="requested"/> world bounds
		/// of the current layer, the method will calculate which layer dependency
		/// has to be added to ensure the required chunks are generated in time.
		/// </summary>
		protected void WarnAboutMissingDependencies(ILC q, GridBounds requested) {
			if (q == null)
				return;
			GridBounds requester = q.bounds;
			int top = Math.Max(0, requested.max.y - requester.max.y);
			int bottom = Math.Min(0, requested.min.y - requester.min.y);
			int right = Math.Max(0, requested.max.x - requester.max.x);
			int left = Math.Min(0, requested.min.x - requester.min.x);
			int hPadding = Math.Max(-left, right);
			int vPadding = Math.Max(top, -bottom);

			// In earlier iterations it was under consideration to automatically
			// add the dependency or expand the padding of the existing dependency.
			// However, it's better to be explicitly aware of dependencies and their
			// required paddings. If the code requests data outside of explicit set up
			// dependencies, it's good to be aware that the dependencies must be increased,
			// since it can have significant performance impact. And it can also happen
			// that it's due to a bug, and the bug should be fixed, rather than any
			// dependencies being added or expanded.
			Logg.LogError(
				$"Layer {q.abstractLayer.GetType().Name} requires chunks from {GetType().Name} that are not available.\n"
				+ $"It needs a dependency with padding {hPadding},{vPadding}.\n"
				+ $"Requested bounds: {requested}, bounds of requester chunk: {requester}\n"
				+ $"{Environment.StackTrace}");
		}

		/// <summary>
		/// Handle all loaded chunks.
		/// </summary>
		protected void HandleAllChunks(int minChunkLevel, Action<C> func) {
			lock (chunks) {
				foreach (C chunk in chunks) {
					if (chunk != null && chunk.level >= minChunkLevel)
						func(chunk);
				}
			}
		}

		/// <summary>
		/// Handle all loaded chunks.
		/// </summary>
		public void HandleAllAbstractChunks(int minChunkLevel, Action<AbstractLayerChunk> func) {
			lock (chunks) {
				foreach (C chunk in chunks) {
					if (chunk != null && chunk.level >= minChunkLevel)
						func(chunk);
				}
			}
		}

		/// <summary>
		/// Handle all layer dependencies.
		/// </summary>
		public void HandleDependenciesForLevel(int level, Action<LayerDependency> func) {
			lock (dependencies) {
				foreach (LayerDependency dependency in dependencies[level]) {
					func(dependency);
				}
			}
		}

		/// <summary>
		/// Handle chunks that overlap the given <paramref name="worldBounds"/> specified in world units.
		/// </summary>
		protected void HandleChunksInBounds(ILC q, GridBounds worldBounds, int minChunkLevel, Action<C> func) {
			Point minChunkIndex = new Point(
				Crd.Div(worldBounds.min.x, chunkW),
				Crd.Div(worldBounds.min.y, chunkH)
			);
			Point maxChunkIndex = new Point(
				Crd.DivUp(worldBounds.max.x, chunkW),
				Crd.DivUp(worldBounds.max.y, chunkH)
			);
			bool missingAnyChunks = false;
			lock (chunks) {
				for (int x = minChunkIndex.x; x < maxChunkIndex.x; x++) {
					for (int y = minChunkIndex.y; y < maxChunkIndex.y; y++) {
						C chunk = chunks[x, y];
						if (chunk != null && chunk.level >= minChunkLevel)
							func(chunk);
						else
							missingAnyChunks = true;
					}
				}
			}
			if (missingAnyChunks) {
				WarnAboutMissingDependencies(q, worldBounds);
			}
		}

		/// <summary>
		/// Assuming an infinite grid with a resolution per chunk of <paramref name="chunkGridSize"/>,
		/// output the <paramref name="chunk"/> and <paramref name="localPointInChunk"/> of the given global <paramref name="gridPoint"/>.
		/// If iterating over many grid points, consider instead using <see cref="HandleGridPoints"/>.
		/// </summary>
		protected bool GetChunkOfGridPoint(ILC q, Point gridPoint, Point chunkGridSize, out C chunk, out Point localPointInChunk) {
			return GetChunkOfGridPoint(q, gridPoint.x, gridPoint.y, chunkGridSize.x, chunkGridSize.y, out chunk, out localPointInChunk);
		}

		/// <summary>
		/// Assuming an infinite grid with a resolution per chunk of <paramref name="chunkGridW"/> by <paramref name="chunkGridH"/>,
		/// output the <paramref name="chunk"/> and <paramref name="localPointInChunk"/> of the given global point.
		/// If iterating over many grid points, consider instead using <see cref="HandleGridPoints"/>.
		/// </summary>
		protected bool GetChunkOfGridPoint(ILC q, int x, int y, int chunkGridW, int chunkGridH, out C chunk, out Point localPointInChunk) {
			Point chunkIndex = new Point(
				Crd.Div(x, chunkGridW),
				Crd.Div(y, chunkGridH)
			);
			if (TryGetChunk(chunkIndex, out chunk)) {
				localPointInChunk = new Point(x - chunkIndex.x * chunkGridW, y - chunkIndex.y * chunkGridH);
				return true;
			}
			if (q != null) {
				int cellW = chunkW / chunkGridW;
				int cellH = chunkH / chunkGridH;
				GridBounds requested = new GridBounds(x * cellW, y * cellH, cellW, cellH);
				WarnAboutMissingDependencies(q, requested);
			}
			localPointInChunk = Point.zero;
			return false;
		}

		protected delegate void HandleGridPointInChunk(C chunk, Point localPointInChunk, Point globalPoint);

		/// <summary>
		/// Assuming an infinite grid with a resolution per chunk of <paramref name="chunkGridSize"/>,
		/// call the handler function once for each of the grid points within the <paramref name="gridBounds"/>.
		/// This is more efficient than calling GetChunkOfGridPoint for each grid point.
		/// </summary>
		protected void HandleGridPoints(ILC q, GridBounds gridBounds, Point chunkGridSize, HandleGridPointInChunk handler, bool callForNullChunks = false) {
			if (gridBounds.empty)
				return;
			// Min and max chunk indices (max is inclusive)
			Point minChunkIndex = new Point(
				Crd.Div(gridBounds.min.x, chunkGridSize.x),
				Crd.Div(gridBounds.min.y, chunkGridSize.y)
			);
			Point maxChunkIndex = new Point(
				Crd.Div(gridBounds.max.x - 1, chunkGridSize.x),
				Crd.Div(gridBounds.max.y - 1, chunkGridSize.y)
			);
			bool missingAnyChunks = false;
			lock (chunks) {
				for (int i = minChunkIndex.x; i <= maxChunkIndex.x; i++) {
					// Min and max grid indices in chunk (max is exclusive)
					int gridXMin = Math.Max(gridBounds.min.x - chunkGridSize.x * i, 0);
					int gridXMax = Math.Min(gridBounds.max.x - chunkGridSize.x * i, chunkGridSize.x);
					for (int j = minChunkIndex.y; j <= maxChunkIndex.y; j++) {
						// Min and max grid indices in chunk (max is exclusive)
						int gridYMin = Math.Max(gridBounds.min.y - chunkGridSize.y * j, 0);
						int gridYMax = Math.Min(gridBounds.max.y - chunkGridSize.y * j, chunkGridSize.y);

						Point chunkOrigin = new Point(i * chunkGridSize.x, j * chunkGridSize.y);
						C chunk = chunks[i, j];
						if (chunk != null && chunk.level >= 0) {
							// Iterate over grid points in chunk.
							for (int x = gridXMin; x < gridXMax; x++) {
								for (int y = gridYMin; y < gridYMax; y++) {
									handler(chunk, new Point(x, y), new Point(x, y) + chunkOrigin);
								}
							}
						}
						else {
							missingAnyChunks = true;
							if (callForNullChunks) {
								for (int x = gridXMin; x < gridXMax; x++) {
									for (int y = gridYMin; y < gridYMax; y++) {
										handler(null, new Point(x, y), new Point(x, y) + chunkOrigin);
									}
								}
							}
						}
					}
				}
			}
			if (missingAnyChunks && q != null) {
				GridBounds requested = GridBounds.MinMax(
					gridBounds.min.x * chunkW / chunkGridSize.x,
					gridBounds.min.y * chunkH / chunkGridSize.y,
					gridBounds.max.x * chunkW / chunkGridSize.x,
					gridBounds.max.y * chunkH / chunkGridSize.y);
				WarnAboutMissingDependencies(q, requested);
			}
		}

		/// <summary>
		/// Returns true if the layer is loaded at the highest level
		/// at the specified <paramref name="position"/> in world units.
		/// </summary>
		public bool IsLoadedAtPosition(DPoint position) {
			return IsLoadedAtPosition(position, GetLevelCount() - 1);
		}

		/// <summary>
		/// Returns true if the layer is loaded at least up to <paramref name="level"/>
		/// at the specified <paramref name="position"/> in world units.
		/// </summary>
		public bool IsLoadedAtPosition(DPoint position, int level) {
			var chunkIndex = new Point(Crd.Div((int)position.x, chunkW), Crd.Div((int)position.y, chunkH));
			return TryGetChunk(chunkIndex, out C _);
		}
	}

}
