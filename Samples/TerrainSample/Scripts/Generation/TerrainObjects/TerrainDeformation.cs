using Runevision.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct SpecData {
	public int pointCount;
	public float4 splat;
	public float4 bounds;
};

public struct SpecPoint {
	public Vector3 pos;
	public float innerWidth;
	public float outerWidth;
	public float splatWidth;
	public float centerElevation;
};

public struct SpecPointB {
	public float3 pos;
	public float4 props; // innerWidth, outerWidth, splatWidth, centerElevation

	public static explicit operator SpecPointB(SpecPoint p) {
		return new SpecPointB() {
			pos = p.pos,
			props = new float4(p.innerWidth, p.outerWidth, p.splatWidth, p.centerElevation)
		};
	}
};

public static class TerrainDeformation {

	static ListPool<SpecPoint> specPointListPool = new ListPool<SpecPoint>(4096);
	static ListPool<SpecData> specDataListPool = new ListPool<SpecData>(128);

	public static void ApplySpecs(
		NativeArray<float> heights,
		NativeArray<float3> dists,
		NativeArray<float4> splats,
		Point gridOffset,
		Point gridSize,
		float2 cellSize,
		IReadOnlyList<DeformationSpec> specs,
		Func<SpecPoint, SpecPoint> postprocess = null,
		Func<SpecData, SpecData> postprocessSpecs = null
	) {
		if (specs.Count == 0)
			return;

		List<SpecData> specDatas = specDataListPool.Get();
		for (int i = 0; i < specs.Count; i++) {
			DeformationSpec spec = specs[i];
			int specPointCount = spec.points.Count;
			specDatas.Add(new SpecData() {
				pointCount = specPointCount,
				splat = spec.splat,
				bounds = new float4(
					spec.bounds.min.x - cellSize.x, spec.bounds.min.y - cellSize.y,
					spec.bounds.max.x + cellSize.x, spec.bounds.max.y + cellSize.y)
			});
		}

		List<SpecPoint> specPoints = specPointListPool.Get();
		for (int i = 0; i < specs.Count; i++) {
			DeformationSpec spec = specs[i];
			for (int j = 0; j < spec.points.Count; j++) {
				specPoints.Add(spec.points[j]);
			}
		}

		if (postprocess != null) {
			for (int i = specPoints.Count - 1; i >= 0; i--) {
				specPoints[i] = postprocess(specPoints[i]);
			}
		}

		if (postprocessSpecs != null) {
			for (int i = specDatas.Count - 1; i >= 0; i--) {
				specDatas[i] = postprocessSpecs(specDatas[i]);
			}
		}

		NativeArray<SpecPointB> specPointsArray = new NativeArray<SpecPointB>(specPoints.Count, Allocator.Persistent);
		NativeArray<SpecData> specDatasArray = new NativeArray<SpecData>(specDatas.Count, Allocator.Persistent);

		UnityEngine.Profiling.Profiler.BeginSample("SetupSpecData");
		for (int i = 0; i < specPoints.Count; i++)
			specPointsArray[i] = (SpecPointB)specPoints[i];
		for (int i = 0; i < specDatas.Count; i++)
			specDatasArray[i] = specDatas[i];
		UnityEngine.Profiling.Profiler.EndSample();

		UnityEngine.Profiling.Profiler.BeginSample("Dispatch");
		PointerArray<float> heightsPointerArray = heights;
		PointerArray<float3> distsPointerArray = dists;
		PointerArray<float4> splatsPointerArray = splats;
		TerrainDeformationMethod.ApplySpecs(
			specDatasArray,
			specPointsArray,
			(uint)specDatas.Count,
			gridOffset.x,
			gridOffset.y,
			(uint)gridSize.x,
			(uint)gridSize.y,
			cellSize,
			ref heightsPointerArray,
			ref distsPointerArray,
			ref splatsPointerArray);
		UnityEngine.Profiling.Profiler.EndSample();

		specPointsArray.Dispose();
		specDatasArray.Dispose();

		specPointListPool.Return(ref specPoints);
		specDataListPool.Return(ref specDatas);
	}
}
