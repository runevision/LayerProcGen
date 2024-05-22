using Runevision.Common;
using Runevision.LayerProcGen;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

// The GeoGridLayer is an intermediary layer between the LocationLayer and the CultivationLayer.
// Each CultivationLayer chunk needs data well outside of its own bounds.
// If they were each to calculate that themselves, there would be a lot of redundant
// calculations of the overlapping areas. The GeoGridLayer performs these calculations
// instead (essentially caching them) so multiple CultivationLayer chunks can use
// the same already calculated data.

public class GeoGridChunk : LayerChunk<GeoGridLayer, GeoGridChunk>, IDisposable {
	NativeArray<float> heightsNA;
	NativeArray<float3> distsNA;
	NativeArray<float4> splatsNA;
	public PointerArray2D<float> heights;
	public PointerArray2D<float3> dists;
	public PointerArray2D<float4> splats;

	public GeoGridChunk() {
		heights = new PointerArray2D<float>(layer.gridChunkRes.y, layer.gridChunkRes.x, out heightsNA);
		dists = new PointerArray2D<float3>(layer.gridChunkRes.y, layer.gridChunkRes.x, out distsNA);
		splats = new PointerArray2D<float4>(layer.gridChunkRes.y, layer.gridChunkRes.x, out splatsNA);
		LayerManager.instance.abort += Dispose;
	}

	public void Dispose() {
		if (heightsNA.IsCreated)
			heightsNA.Dispose();
		if (distsNA.IsCreated)
			distsNA.Dispose();
		if (splatsNA.IsCreated)
			splatsNA.Dispose();
	}

	public override void Create(int level, bool destroy) {
		if (destroy) {
			if (heightsNA.IsCreated) {
				heights.Clear();
				dists.Clear();
				splats.Clear();
			}
		}
		else {
			Build();
		}
	}

	static ListPool<LocationSpec> locationSpecListPool = new ListPool<LocationSpec>(64);

	void Build() {
		Point gridChunkRes = layer.gridChunkRes;
		Point gridOrigin = index * gridChunkRes;
		SimpleProfiler.ProfilerHandle ph;

		// Apply terrain noise.
		ph = SimpleProfiler.Begin(phc, "Height Noise");
		for (var zRes = 0; zRes < gridChunkRes.y; zRes++) {
			for (var xRes = 0; xRes < gridChunkRes.x; xRes++) {
				DPoint p = new DPoint(
					(gridOrigin.x + xRes) * TerrainPathFinder.halfCellSize,
					(gridOrigin.y + zRes) * TerrainPathFinder.halfCellSize
				);
				heights[zRes, xRes] = TerrainNoise.GetHeight(p);
				dists[zRes, xRes] = new float3(0f, 0f, 1000f);
			}
		}
		SimpleProfiler.End(ph);

		// Apply deformation from locations.
		ph = SimpleProfiler.Begin(phc, "Deform-LocationDeformation");
		List<LocationSpec> locationSpecs = locationSpecListPool.Get();
		LocationLayer.instance.GetLocationSpecsOverlappingBounds(this, locationSpecs, bounds);
		TerrainDeformation.ApplySpecs(
			heightsNA, distsNA, splatsNA,
			gridOrigin,
			gridChunkRes,
			Point.one * TerrainPathFinder.halfCellSize,
			locationSpecs,
			(SpecPoint p) => {
				p.innerWidth += 2;
				p.centerElevation = 0;
				return p;
			},
			(SpecData d) => {
				d.bounds = new float4(
					d.bounds.x - 2, d.bounds.y - 2, d.bounds.z + 2, d.bounds.w + 2);
				return d;
			});
		locationSpecListPool.Return(ref locationSpecs);
		SimpleProfiler.End(ph);
	}
}

public class GeoGridLayer : ChunkBasedDataLayer<GeoGridLayer, GeoGridChunk> {
	public override int chunkW { get { return 360; } }
	public override int chunkH { get { return 360; } }

	public Point gridChunkRes;

	public GeoGridLayer() {
		gridChunkRes = chunkSize / TerrainPathFinder.halfCellSize;

		AddLayerDependency(new LayerDependency(LocationLayer.instance, LocationLayer.requiredPadding, 1));
	}

	public void GetDataInBounds(ILC q, GridBounds bounds, float[,] heights, Vector3[,] dists, Vector4[,] splats) {
		HandleGridPoints(q, bounds, chunkSize / TerrainPathFinder.halfCellSize,
			(GeoGridChunk chunk, Point localPointInChunk, Point globalPoint) => {
				int x = globalPoint.x - bounds.min.x;
				int z = globalPoint.y - bounds.min.y;
				heights[z, x] = chunk.heights[localPointInChunk.y, localPointInChunk.x];
				dists[z, x] = chunk.dists[localPointInChunk.y, localPointInChunk.x];
				splats[z, x] = chunk.splats[localPointInChunk.y, localPointInChunk.x];
			}
		);
	}
}
