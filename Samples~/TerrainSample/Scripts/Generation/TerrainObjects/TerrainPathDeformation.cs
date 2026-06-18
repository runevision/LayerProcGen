using Runevision.Common;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

public struct PathData {
	public int pointCount;
	public Splat splat;
	public DPoint boundsMin;
	public DPoint boundsMax;
};

public struct PathPoint {
	public DPoint3 pos;
	public float innerWidth;
	public float outerWidth;
	public float splatWidth;
	public float centerElevation;
};

public struct PathPointB {
	public DPoint3 pos;
	public float4 props; // innerWidth, outerWidth, splatWidth, centerElevation

	public static explicit operator PathPointB(PathPoint p) {
		return new PathPointB() {
			pos = p.pos,
			props = new float4(p.innerWidth, p.outerWidth, p.splatWidth, p.centerElevation)
		};
	}
};

public static class TerrainPathDeformation {

	static ListPool<PathPoint> pathPointListPool = new ListPool<PathPoint>(4096);
	static ListPool<PathData> pathDataListPool = new ListPool<PathData>(128);

	public static void ApplyPaths(
		NativeArray<float> heights,
		NativeArray<float3> dists,
		NativeArray<Splat> splats,
		Point gridOffset,
		Point gridSize,
		DFloat cellSize,
		List<TerrainPath> paths,
		Func<PathPoint, PathPoint> postprocess = null,
		Func<PathData, PathData> postprocessPaths = null
	) {
		if (paths.Count == 0)
			return;

		List<PathData> pathDatas = pathDataListPool.Get();
		for (int i = 0; i < paths.Count; i++) {
			TerrainPath path = paths[i];
			int pathPointCount = path.pathPoints.Count;
			pathDatas.Add(new PathData() {
				pointCount = pathPointCount,
				splat = (
					path.type == 0 ?
					new Splat(0, 0, 1, 0) :
					new Splat(0, 0.7f, 0.3f, 0)
				),
				boundsMin = new DPoint(
					path.bounds.min.x - cellSize, path.bounds.min.y - cellSize),
				boundsMax = new DPoint(
					path.bounds.max.x + cellSize, path.bounds.max.y + cellSize)
			});
		}

		List<PathPoint> pathPoints = pathPointListPool.Get();
		for (int i = 0; i < paths.Count; i++) {
			TerrainPath path = paths[i];
			for (int j = 0; j < path.pathPoints.Count; j++) {
				pathPoints.Add(path.pathPoints[j]);
			}
		}

		if (postprocess != null) {
			for (int i = pathPoints.Count - 1; i >= 0; i--) {
				pathPoints[i] = postprocess(pathPoints[i]);
			}
		}

		if (postprocessPaths != null) {
			for (int i = pathDatas.Count - 1; i >= 0; i--) {
				pathDatas[i] = postprocessPaths(pathDatas[i]);
			}
		}

		NativeArray<PathPointB> pathPointsArray = new NativeArray<PathPointB>(pathPoints.Count, Allocator.Persistent);
		NativeArray<PathData> pathDatasArray = new NativeArray<PathData>(pathDatas.Count, Allocator.Persistent);

		UnityEngine.Profiling.Profiler.BeginSample("SetupPathData");
		for (int i = 0; i < pathPoints.Count; i++)
			pathPointsArray[i] = (PathPointB)pathPoints[i];
		for (int i = 0; i < pathDatas.Count; i++)
			pathDatasArray[i] = pathDatas[i];
		UnityEngine.Profiling.Profiler.EndSample();

		UnityEngine.Profiling.Profiler.BeginSample("Dispatch");
		PointerArray<float> heightsPointerArray = heights;
		PointerArray<float3> distsPointerArray = dists;
		PointerArray<Splat> splatsPointerArray = splats;
		PathsBurstMethod.ComputePaths(
			pathDatasArray,
			pathPointsArray,
			pathDatas.Count,
			gridOffset.x,
			gridOffset.y,
			gridSize.x,
			gridSize.y,
			cellSize,
			ref heightsPointerArray,
			ref distsPointerArray,
			ref splatsPointerArray);
		UnityEngine.Profiling.Profiler.EndSample();

		pathPointsArray.Dispose();
		pathDatasArray.Dispose();

		pathPointListPool.Return(ref pathPoints);
		pathDataListPool.Return(ref pathDatas);
	}
}
