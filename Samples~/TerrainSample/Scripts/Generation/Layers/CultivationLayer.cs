using Runevision.Common;
using Runevision.LayerProcGen;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CultivationChunk : LayerChunk<CultivationLayer, CultivationChunk> {
	public List<PathSpec> paths = new List<PathSpec>();

	float[,] heights;
	Vector3[,] dists;
	Vector4[,] splats;

	Point gridOrigin;

	public CultivationChunk() {
		heights = new float[layer.gridSize.y, layer.gridSize.x];
		dists = new Vector3[layer.gridSize.y, layer.gridSize.x];
		splats = new Vector4[layer.gridSize.y, layer.gridSize.x];
	}

	public override void Create(int level, bool destroy) {
		if (destroy) {
			foreach (var path in paths) {
				PathSpec pathCopy = path;
				ObjectPool<PathSpec>.GlobalReturn(ref pathCopy);
			}
			paths.Clear();
			heights.Clear();
			dists.Clear();
			splats.Clear();
		}
		else {
			Build();
		}
	}

	static ListPool<LocationSpec> locationSpecListPool = new ListPool<LocationSpec>(128);

	void Build() {
		// We use a grid that covers an area larger than the chunk itself,
		// so pathfinding can go beyond the chunk bounds.
		Point gridSize = layer.gridSize; // larger than gridChunkRes!
		gridOrigin = index * layer.gridChunkRes - layer.gridPadding;

		SimpleProfiler.ProfilerHandle ph;

		// Fill the grid with data from the GeoGridLayer.
		ph = SimpleProfiler.Begin(phc, "Retrieve Data");
		GeoGridLayer.instance.GetDataInBounds(
			this, new GridBounds(gridOrigin, gridSize), heights, dists, splats);
		SimpleProfiler.End(ph);

		// Define a height function and cost function for the pathfinding.

		float HeightFunction(DPoint p) {
			Point index = GetIndexOfPos(p);
#if UNITY_EDITOR
			Point indexClamped = Point.Min(Point.Max(index, Point.zero), gridSize - Point.one);
			if (indexClamped != index) {
				index = indexClamped;
				DebugDrawer.DrawRay(new Vector3((float)p.x, 0, (float)p.y), Vector3.up * 1000, Color.red, 1000);
				Logg.LogError("Accessing heights array out of bounds.");
			}
#endif
			return heights[index.y, index.x];
		}

		float CostFunction(DPoint p) {
			Point index = GetIndexOfPos(p);
#if UNITY_EDITOR
			Point indexClamped = Point.Min(Point.Max(index, Point.zero), gridSize - Point.one);
			if (indexClamped != index) {
				index = indexClamped;
				DebugDrawer.DrawRay(new Vector3((float)p.x, 0, (float)p.y), Vector3.up * 1000, Color.red, 1000);
				Logg.LogError("Accessing dists array out of bounds.");
			}
#endif
			// Penalty for overlapping locations.
			// From no cost at dist 6 to maximum cost at dist -6. 
			return Mathf.InverseLerp(6, -6, dists[index.y, index.x].z) * 20;
		}

		// Get connections between locations from the LocationLayer
		// and create a path for each connection.
		List<Location> connectionPairs = connectionPairsPool.Get();
		LocationLayer.instance.GetConnectionsOwnedInBounds(this, connectionPairs, bounds);
		for (int i = 0; i < connectionPairs.Count; i += 2) {
			Location locA = connectionPairs[i];
			Location locB = connectionPairs[i + 1];
			CreatePath(locA, locB, HeightFunction, CostFunction);
		}
		connectionPairsPool.Return(ref connectionPairs);
	}

	static ListPool<Location> connectionPairsPool = new ListPool<Location>(20);
	static ListPool<DPoint> dPointListPool = new ListPool<DPoint>(128);
	static ObjectPool<TerrainPathFinder> pathFinderPool = new ObjectPool<TerrainPathFinder>();

	void CreatePath(Location locA, Location locB,
		System.Func<DPoint, float> heightFunction,
		System.Func<DPoint, float> costFunction
	) {
		SimpleProfiler.ProfilerHandle ph;

		ph = SimpleProfiler.Begin(phc, "Path Planning");

		// Note: Ensure start and goal positions are multiples of cellSize,
		// or else the pathfinding will fail!
		float half = TerrainPathFinder.halfCellSize;
		Point a = Crd.RoundToPeriod((Point)(locA.position + (DPoint)(locA.frontDir * (locA.radius - half))), TerrainPathFinder.cellSize);
		Point b = Crd.RoundToPeriod((Point)(locB.position + (DPoint)(locB.frontDir * (locB.radius - half))), TerrainPathFinder.cellSize);

		// Perform pathfinding.
		List<DPoint> dPoints = dPointListPool.Get();
		TerrainPathFinder.FindFootPath(pathFinderPool, dPoints, a, b, TerrainPathFinder.cellSize, heightFunction, costFunction);
		if (dPoints == null) {
			Logg.LogError("Couldn't find path in index " + index);
			return;
		}
		if (dPoints.Count == 0) {
			Logg.LogError("No points in found path in index " + index);
			return;
		}

		// Adjust first and last point to be specific distance from respective locations.
		int last = dPoints.Count - 1;
		float2 dirA = math.normalize((float2)(dPoints[0]    - locA.position)) * (locA.radius - 2);
		float2 dirB = math.normalize((float2)(dPoints[last] - locB.position)) * (locB.radius - 2);
		dPoints[0]     = locA.position + (DPoint)dirA;
		dPoints[last]  = locB.position + (DPoint)dirB;

		// Create list of 3D points with heights from the 2D points.
		var points = PathSpec.pointListPool.Get();
		foreach (var dPoint in dPoints) {
			points.Add(GetPointWithHeight(dPoint, heightFunction));
		}
		dPointListPool.Return(ref dPoints); // 2D points are no longer needed.

		// Create a path deformation specification from the points.
		PathSpec path = ObjectPool<PathSpec>.GlobalGet().Init(points, 0, 2.5f, 1.5f, 2.7f, 0f);
		PathSpec.pointListPool.Return(ref points); // 3D points are no longer needed.
		path.CalculateBounds();
		paths.Add(path);

		SimpleProfiler.End(ph);
	}

	Point GetIndexOfPos(DPoint p) {
		return new Point(
			(int)(p.x / TerrainPathFinder.halfCellSize) - gridOrigin.x,
			(int)(p.y / TerrainPathFinder.halfCellSize) - gridOrigin.y
		);
	}

	Vector3 GetPointWithHeight(DPoint p, System.Func<DPoint, float> heightFunction) {
		return new Vector3((float)p.x, heightFunction(p), (float)p.y);
	}

	public void DebugDraw(float drawPaths, float drawOrigPaths, float drawPathBounds) {
		for (int p = 0; p < paths.Count; p++) {
			PathSpec path = paths[p];
			path.DebugDraw(drawPaths, drawOrigPaths, drawPathBounds);
		}
	}

	public void DrawHeights(GridBounds bounds) {
		int d = TerrainPathFinder.halfCellSize;
		Point o = layer.gridPadding;
		for (int i = 0; i < layer.gridChunkRes.x; i++) {
			int x = worldOffset.x + d * i;
			for (int j = 0; j < layer.gridChunkRes.y; j++) {
				int z = worldOffset.y + d * j;
				if (!bounds.Contains(new Point(x, z)))
					continue;
				DebugDrawer.DrawLine(
					new Vector3(x, heights[o.y + j, o.x + i], z),
					new Vector3(x + d, heights[o.y + j, o.x + i + 1], z),
					Color.white);
				DebugDrawer.DrawLine(
					new Vector3(x, heights[o.y + j, o.x + i], z),
					new Vector3(x, heights[o.y + j + 1, o.x + i], z + d),
					Color.white);
			}
		}
	}

	public void DrawDirections(GridBounds bounds) {
		int d = TerrainPathFinder.halfCellSize;
		Point o = layer.gridPadding;
		for (int i = 0; i < layer.gridChunkRes.x; i++) {
			int x = worldOffset.x + d * i;
			for (int j = 0; j < layer.gridChunkRes.y; j++) {
				int z = worldOffset.y + d * j;
				if (!bounds.Contains(new Point(x, z)))
					continue;
				Vector3 distV = dists[o.y + j, o.x + i];
				Vector2 dir = distV.xy().normalized;
				float height = heights[o.y + j, o.x + i];
				if (distV.z <= 0) {
					DebugDrawer.DrawLine(
						new Vector3(x, height, z),
						new Vector3(x + distV.x, height, z + distV.y),
						new Color(1, 0, 0, 0.2f));
				}
				else {
					DebugDrawer.DrawLine(
						new Vector3(x, height, z),
						new Vector3(x + dir.x * distV.z, height, z + dir.y * distV.z),
						new Color(1, 1, 0, 0.2f));
					DebugDrawer.DrawLine(
						new Vector3(x + dir.x * distV.z, height, z + dir.y * distV.z),
						new Vector3(x + distV.x, height, z + distV.y),
						new Color(1, 0, 0, 0.2f));
				}
			}
		}
	}
}

public class CultivationLayer : ChunkBasedDataLayer<CultivationLayer, CultivationChunk>, ILayerVisualization {
	public override int chunkW { get { return 360; } }
	public override int chunkH { get { return 360; } }

	public static readonly Point requiredPadding = new Point(180, 180);

	public Point gridChunkRes;
	public Point gridPadding;
	public Point gridSize;
	public Point worldSpacePadding;

	public Transform layerParent;

	static DebugToggle debugPaths = DebugToggle.Create(">Layers/CultivationLayer/Paths");
	static DebugToggle debugPathsRaw = DebugToggle.Create(">Layers/CultivationLayer/Paths Raw");
	static DebugToggle debugPathBounds = DebugToggle.Create(">Layers/CultivationLayer/Paths Bounds");
	static DebugToggle debugHeights = DebugToggle.Create(">Layers/CultivationLayer/Heights");
	static DebugToggle debugDirections = DebugToggle.Create(">Layers/CultivationLayer/Directions");

	public CultivationLayer() {
		gridChunkRes = chunkSize / TerrainPathFinder.halfCellSize;
		// Make the grid for each chunk cover an area extending further our than the chunk.
		// This ensures the pathfinding can succeed even when it extends partially beyond the chunk.
		worldSpacePadding = chunkSize;
		gridPadding = gridChunkRes;
		gridSize = gridChunkRes + gridPadding * 2;

		layerParent = new GameObject("CultivationLayer").transform;

		AddLayerDependency(new LayerDependency(GeoGridLayer.instance, worldSpacePadding, 0));
		AddLayerDependency(new LayerDependency(LocationLayer.instance, LocationLayer.requiredPadding, 2));
	}

	public void VisualizationUpdate() {
		VisualizationManager.BeginDebugDraw(this, 0);
		if (debugPaths.visible || debugPathsRaw.visible || debugPathBounds.visible)
			HandleAllChunks(0, c => c.DebugDraw(debugPaths.animAlpha, debugPathsRaw.animAlpha, debugPathBounds.animAlpha));
		VisualizationManager.EndDebugDraw();

		GridBounds focusBounds = GridBounds.Empty();
		if (debugHeights.enabled || debugDirections.enabled) {
			foreach (var dep in LayerManager.instance.topDependencies) {
				if (dep.layer == PlayLayer.instance) {
					focusBounds = new GridBounds(dep.focus - Point.one * 50, Point.one * 100);
					break;
				}
			}
		}
		if (debugHeights.enabled) {
			HandleChunksInBounds(null, focusBounds, 0, c => c.DrawHeights(focusBounds));
		}
		if (debugDirections.enabled) {
			HandleChunksInBounds(null, focusBounds, 0, c => c.DrawDirections(focusBounds));
		}
	}

	public void GetPathsOverlappingBounds(ILC q, List<PathSpec> outPaths, GridBounds bounds) {
		// Add paths within bounds.
		HandleChunksInBounds(q, bounds.GetExpanded(requiredPadding), 0, chunk => {
			foreach (var path in chunk.paths)
				if (bounds.Overlaps(path.bounds))
					outPaths.Add(path);
		});
	}
}
