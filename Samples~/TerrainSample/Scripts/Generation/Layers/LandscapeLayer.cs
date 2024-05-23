using Runevision.Common;
using Runevision.LayerProcGen;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

public struct QueuedTerrainCallback<L, C> : IQueuedAction
	where L : LandscapeLayer<L, C>, new()
	where C : LandscapeChunk<L, C>, new()
{
	public float[,] heightmap;
	public float[,,] splatmap;
	public int[,] detailMap;
	public TreeInstance[] treeInstances;
	public Vector3 position;
	public TransformWrapper chunkParent;
	public L layer;
	public Point index;

	public QueuedTerrainCallback(
		float[,] heightmap,
		float[,,] splatmap,
		int[,] detailMap,
		TreeInstance[] treeInstances,
		TransformWrapper chunkParent,
		Vector3 position,
		L layer,
		Point index
	) {
		this.heightmap = heightmap;
		this.splatmap = splatmap;
		this.detailMap = detailMap;
		this.treeInstances = treeInstances;
		this.chunkParent = chunkParent;
		this.position = position;
		this.layer = layer;
		this.index = index;
	}

	static Terrain GetOrCreateTerrain(L layer) {
		int unusedCount = layer.unusedTerrains.Count;
		if (unusedCount > 0) {
			Terrain existingTerrain = layer.unusedTerrains[unusedCount - 1];
			layer.unusedTerrains.RemoveAt(unusedCount - 1);
			Logg.Log("Reusing terrain", false);
			return existingTerrain;
		}
		Logg.Log("Creating new terrain", false);

		// Set heights.
		//TerrainData data = new TerrainData(); // Doesn't work anymore for grass detail.
		TerrainData data = Object.Instantiate(TerrainResources.instance.terrainData);
		data.heightmapResolution = layer.gridResolution + 1;
		data.alphamapResolution = layer.gridResolution + 1;
		data.SetDetailResolution(layer.gridResolution, 32);
		data.size = new Vector3(
			layer.chunkW * 128 / 124,
			layer.terrainHeight,
			layer.chunkH * 128 / 124
		);

		// Set splat maps.
		var grassSplat = new TerrainLayer();
		var cliffSplat = new TerrainLayer();
		var pathSplat = new TerrainLayer();
		grassSplat.diffuseTexture = TerrainResources.instance.grassTex;
		cliffSplat.diffuseTexture = TerrainResources.instance.cliffTex;
		pathSplat.diffuseTexture = TerrainResources.instance.pathTex;
		grassSplat.tileSize = Vector2.one * 3.0f;
		cliffSplat.tileSize = Vector2.one * 6.0f;
		pathSplat.tileSize  = Vector2.one * 2.0f;
		data.terrainLayers = new TerrainLayer[] { grassSplat, cliffSplat, pathSplat };

		// Set detail maps.
		if (layer.lodLevel == 0) {
			DetailPrototype grassDetail = new DetailPrototype();
			grassDetail.prototypeTexture = TerrainResources.instance.grassDetail;
			grassDetail.healthyColor = new Color(0.9f, 1.0f, 1.1f);
			grassDetail.dryColor = new Color(1.1f, 1.0f, 0.9f);
			grassDetail.minHeight = 0.3f;
			grassDetail.maxHeight = 0.6f;
			grassDetail.minWidth = 0.4f;
			grassDetail.maxWidth = 0.7f;
			data.detailPrototypes = new DetailPrototype[] { grassDetail };
			#if UNITY_2022_3_OR_NEWER
			data.SetDetailScatterMode(DetailScatterMode.InstanceCountMode);
			#endif
			data.wavingGrassAmount = 0.03f;
			data.wavingGrassSpeed = 30;
			data.wavingGrassStrength = 4;
			data.wavingGrassTint = Color.white * 0.7f;
		}

		// Create GameObject and Terrain.
		GameObject newTerrainGameObject;
		Terrain terrain;
		if (layer.lodLevel == 0) {
			newTerrainGameObject = Terrain.CreateTerrainGameObject(data);
			terrain = newTerrainGameObject.GetComponent<Terrain>();
		}
		else {
			newTerrainGameObject = new GameObject();
			terrain = newTerrainGameObject.AddComponent<Terrain>();
			terrain.terrainData = data;
		}
		newTerrainGameObject.name = "Terrain" + layer.lodLevel;
		newTerrainGameObject.SetActive(false);

		// Setup Terrain component.
		terrain.allowAutoConnect = false;
		terrain.drawInstanced = true;
		terrain.groupingID = layer.lodLevel;
		terrain.heightmapPixelError = 8;
		terrain.materialTemplate = TerrainResources.instance.material;
		terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
		terrain.detailObjectDistance = 50;
		terrain.treeBillboardDistance = 70;
		terrain.treeDistance = 2000;

		return terrain;
	}

	public void Process() {
		LayerManagerBehavior.instance.StartCoroutine(ProcessRoutine());
	}

	public IEnumerator ProcessRoutine() {
		Terrain terrain = GetOrCreateTerrain(layer);

		Profiler.BeginSample("Get terrainData");
		var data = terrain.terrainData;
		Profiler.EndSample();

		int sliceCount = 16;
		int res = heightmap.GetLength(0);
		int sliceSize = (res - 1) / sliceCount;
		float[,] slice = new float[sliceSize + 1, res];
		for (int i = 0; i < sliceCount; i++) {
			Profiler.BeginSample("CopyIntoSlice");
			int offset = i * sliceSize;
			for (int z = 0; z < sliceSize + 1; z++) {
				for (int x = 0; x < res; x++) {
					slice[z, x] = heightmap[z + offset, x];
				}
			}
			Profiler.EndSample();
			Profiler.BeginSample("SetHeightsDelayLOD");
			data.SetHeightsDelayLOD(0, offset, slice);
			Profiler.EndSample();
			yield return null;
		}
		Profiler.BeginSample("SyncHeightmap");
		data.SyncHeightmap();
		Profiler.EndSample();

		Profiler.BeginSample("SetAlphamaps");
		data.SetAlphamaps(0, 0, splatmap);
		Profiler.EndSample();

		if (layer.lodLevel == 0) {
			Profiler.BeginSample("SetDetailLayer");
			data.SetDetailLayer(0, 0, 0, detailMap);
			Profiler.EndSample();
		}

		Profiler.BeginSample("Set treeInstances");
		if (treeInstances != null)
			data.treeInstances = treeInstances;
		Profiler.EndSample();

		chunkParent.AddChild(terrain.transform);
		terrain.transform.position = position;

		Profiler.BeginSample("Flush");
		terrain.Flush();
		Profiler.EndSample();

		Profiler.BeginSample("Register");
		TerrainLODManager.instance.RegisterChunk(layer.lodLevel, index, terrain);
		Profiler.EndSample();
	}
}

public struct QueuedTerrainRecycleCallback<L, C> : IQueuedAction
	where L : LandscapeLayer<L, C>, new()
	where C : LandscapeChunk<L, C>, new()
{
	public TransformWrapper chunkParent;
	public L layer;
	public Point index;

	public void Process() {
		if (chunkParent.transform == null) {
			// If terrain was not finished instantiating, we have to wait destroying it.
			MainThreadActionQueue.EnqueueNextFrame(this);
			return;
		}
		Terrain terrain = chunkParent.transform.GetComponentInChildren<Terrain>(true);
		terrain.transform.parent = null;
		terrain.gameObject.SetActive(false);
		layer.unusedTerrains.Add(terrain);
		Object.Destroy(chunkParent.transform.gameObject);
		TerrainLODManager.instance.UnregisterChunk(layer.lodLevel, index);
	}
}

[BurstCompile]
public abstract class LandscapeChunk<L, C> : LayerChunk<L, C>
	where L : LandscapeLayer<L, C>, new()
	where C : LandscapeChunk<L, C>, new()
{
	public TransformWrapper chunkParent;

	NativeArray<float> heightsNA;
	NativeArray<float3> distsNA;
	NativeArray<float4> splatsNA;
	public PointerArray2D<float> heights;
	public PointerArray2D<float3> dists;
	public PointerArray2D<float4> splats;
	float[,] heightsArray;
	float[,,] splatsArray;
	int[,] detailMap;

	public LandscapeChunk() {
		heights = new PointerArray2D<float>(layer.gridResolution + 1, layer.gridResolution + 1, out heightsNA);
		dists = new PointerArray2D<float3>(layer.gridResolution + 1, layer.gridResolution + 1, out distsNA);
		splats = new PointerArray2D<float4>(layer.gridResolution + 1, layer.gridResolution + 1, out splatsNA);
		heightsArray = new float[layer.gridResolution + 1, layer.gridResolution + 1];
		splatsArray = new float[layer.gridResolution + 1, layer.gridResolution + 1, 3];
		detailMap = new int[layer.gridResolution, layer.gridResolution];
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
			QueuedTerrainRecycleCallback<L, C> action =
				new QueuedTerrainRecycleCallback<L, C>() {
					chunkParent = chunkParent, layer = layer, index = index };
			MainThreadActionQueue.Enqueue(action);

			if (heightsNA.IsCreated) {
				heights.Clear();
				dists.Clear();
				heightsArray.Clear();
				splats.Clear();
				splatsArray.Clear();
				detailMap.Clear();
			}
		}
		else {
			chunkParent = new TransformWrapper(layer.layerParent, index);
			Build();
		}
	}

	const int GridOffset = 4;

	static ListPool<LocationSpec> locationSpecListPool = new ListPool<LocationSpec>(128);
	static ListPool<PathSpec> pathSpecListPool = new ListPool<PathSpec>(128);

	void Build() {
		SimpleProfiler.ProfilerHandle ph;

		DPoint cellSize = (DPoint)layer.chunkSize / layer.chunkResolution;
		DPoint terrainOrigin = index * layer.chunkSize - cellSize * GridOffset;

		// Apply noise heights.
		ph = SimpleProfiler.Begin(phc, "Height Noise");
		HeightNoise(terrainOrigin, cellSize, layer.gridResolution, ref heights, ref dists);
		SimpleProfiler.End(ph);

		if (layer.lodLevel < 3) {
			// Apply deformation from locations.
			ph = SimpleProfiler.Begin(phc, "Deform-Locations");
			List<LocationSpec> locationSpecs = locationSpecListPool.Get();
			LocationLayer.instance.GetLocationSpecsOverlappingBounds(this, locationSpecs, bounds);
			TerrainDeformation.ApplySpecs(
				heightsNA, distsNA, splatsNA,
				index * layer.chunkResolution - Point.one * GridOffset,
				Point.one * (layer.gridResolution + 1),
				((float2)layer.chunkSize) / layer.chunkResolution,
				locationSpecs,
				(SpecPoint p) => {
					p.centerElevation = 0;
					return p;
				});
			locationSpecListPool.Return(ref locationSpecs);
			SimpleProfiler.End(ph);

			if (layer.lodLevel < 2) {
				// Apply deformation from paths.
				ph = SimpleProfiler.Begin(phc, "Deform-Paths");
				List<PathSpec> pathSpecs = pathSpecListPool.Get();
				CultivationLayer.instance.GetPathsOverlappingBounds(this, pathSpecs, bounds);
				TerrainDeformation.ApplySpecs(
					heightsNA, distsNA, splatsNA,
					index * layer.chunkResolution - Point.one * GridOffset,
					Point.one * (layer.gridResolution + 1),
					((float2)layer.chunkSize) / layer.chunkResolution,
					pathSpecs);
				pathSpecListPool.Return(ref pathSpecs);
				SimpleProfiler.End(ph);
			}
		}

		RandomHash rand = new RandomHash(123);
		RandomHash rand2 = new RandomHash(234);

		ph = SimpleProfiler.Begin(phc, "Splat Noise (GetNormal)");
		HandleSplats(terrainOrigin, cellSize, layer.gridResolution, ref heights, ref splats);
		SimpleProfiler.End(ph);

		

		ph = SimpleProfiler.Begin(phc, "Handle Edges");
		float lowering = 1 << layer.lodLevel;
		HandleEdges(0, lowering * 1.00f, ref heights);
		HandleEdges(1, lowering * 0.20f, ref heights);
		HandleEdges(2, lowering * 0.04f, ref heights);
		HandleEdges(3, lowering * 0.02f, ref heights);
		SimpleProfiler.End(ph);

		ph = SimpleProfiler.Begin(phc, "Copy Heights");
		unsafe {
			fixed (float* heightsPointer = &heightsArray[0, 0]) {
				var heightsPointerArray = new PointerArray2D<float>(heightsPointer, heightsArray);
				CopyHeights(layer.gridResolution, layer.terrainBaseHeight, layer.terrainHeight, heights, ref heightsPointerArray);
			}
		}
		SimpleProfiler.End(ph);

		ph = SimpleProfiler.Begin(phc, "Copy Splats");
		unsafe {
			fixed (float* splatsPointer = &splatsArray[0, 0, 0]) {
				var splatsPointerArray = new PointerArray3D<float>(splatsPointer, splatsArray);
				CopySplats(layer.gridResolution, splats, ref splatsPointerArray);
			}
		}
		SimpleProfiler.End(ph);

		if (layer.lodLevel < 1) {
			ph = SimpleProfiler.Begin(phc, "Generate Details");
			unsafe {
				fixed (int* detailMapPointer = &detailMap[0, 0]) {
					var detailMapPointerArray = new PointerArray2D<int>(detailMapPointer, detailMap);
					GenerateDetails(layer.gridResolution, rand, splats, ref detailMapPointerArray);
				}
			}
			SimpleProfiler.End(ph);
		}

		float height = layer.terrainBaseHeight;
		float posOffset = -GridOffset * layer.chunkW / layer.chunkResolution;
		QueuedTerrainCallback<L, C> action = new QueuedTerrainCallback<L, C>(
			heightsArray, splatsArray, detailMap, null, chunkParent,
			new Vector3(index.x * layer.chunkW + posOffset, height, index.y * layer.chunkH + posOffset),
			layer, index
		);
		MainThreadActionQueue.Enqueue(action);
	}

	[BurstCompile]
	static void HeightNoise(
		in DPoint terrainOrigin, in DPoint cellSize, int gridResolution,
		ref PointerArray2D<float> heights, ref PointerArray2D<float3> dists
	) {
		for (var zRes = 0; zRes <= gridResolution; zRes++) {
			for (var xRes = 0; xRes <= gridResolution; xRes++) {
				DPoint p = terrainOrigin + new Point(xRes, zRes) * cellSize;
				heights[zRes, xRes] = TerrainNoise.GetHeight(p);
				dists[zRes, xRes] = new float3(0f, 0f, 1000f);
			}
		}
	}

	[BurstCompile]
	static void HandleSplats(
		in DPoint terrainOrigin, in DPoint cellSize, int gridResolution,
		ref PointerArray2D<float> heights, ref PointerArray2D<float4> splats
	) {
		// Skip edges in iteration - we need those for calculating normal only.
		float doubleCellSize = 2f * (float)cellSize.x;
		for (var zRes = 1; zRes <= gridResolution; zRes++) {
			for (var xRes = 1; xRes <= gridResolution; xRes++) {
				float4 current = splats[zRes, xRes];
				GetNormal(xRes, zRes, doubleCellSize, heights, out float3 normal);

				// Handle grass vs cliff based on steepness.
				float cliff = normal.y < 0.65f ? 1f : 0f;
				float4 terrainSplat = new float4(1f - cliff, cliff, 0f, 0f);

				// Reduce path splat where there's cliff splat.
				current.z = Mathf.Min(current.z, 1f - cliff);

				// Apply terrain splats (grass/cliff) with remaining unused weight.
				float usedWeight = current.x + current.y + current.z + current.w;
				current += terrainSplat * (1f - usedWeight);

				splats[zRes, xRes] = current;
			}
		}
	}

	[BurstCompile]
	static void GetNormal(int x, int z, float doubleCellSize, in PointerArray2D<float> heights, out float3 normal) {
		normal = math.normalize(new float3(
			heights[z, x + 1] - heights[z, x - 1],
			doubleCellSize,
			heights[z + 1, x] - heights[z - 1, x]
		));
	}

	[BurstCompile]
	static void HandleEdges(int fromEdge, float lowerDist, ref PointerArray2D<float> heights) {
		for (int i = fromEdge; i < heights.Width - fromEdge; i++) {
			heights[fromEdge, i] -= lowerDist;
			heights[i, fromEdge] -= lowerDist;
			heights[heights.Width - fromEdge - 1, i] -= lowerDist;
			heights[i, heights.Width - fromEdge - 1] -= lowerDist;
		}
	}

	[BurstCompile]
	static void CopyHeights(
		int resolution, float terrainBaseHeight, float terrainHeight,
		in PointerArray2D<float> heights,
		ref PointerArray2D<float> heightsArray
	) {
		float invTerrainHeight = 1f / terrainHeight;
		for (var zRes = 0; zRes < resolution + 1; zRes++) {
			for (var xRes = 0; xRes < resolution + 1; xRes++) {
				heightsArray[zRes, xRes] = (heights[zRes, xRes] - terrainBaseHeight) * invTerrainHeight;
			}
		}
	}

	[BurstCompile]
	static void CopySplats(
		int resolution,
		in PointerArray2D<float4> splats,
		ref PointerArray3D<float> splatsArray
	) {
		for (var zRes = 0; zRes < resolution + 1; zRes++) {
			for (var xRes = 0; xRes < resolution + 1; xRes++) {
				splatsArray[zRes, xRes, 0] = splats[zRes, xRes].x;
				splatsArray[zRes, xRes, 1] = splats[zRes, xRes].y;
				splatsArray[zRes, xRes, 2] = splats[zRes, xRes].z;
			}
		}
	}

	[BurstCompile]
	static void GenerateDetails(
		int resolution, in RandomHash rand,
		in PointerArray2D<float4> splats,
		ref PointerArray2D<int> detailMap
	) {
		for (int x = GridOffset; x < resolution - GridOffset; x++) {
			for (int z = GridOffset; z < resolution - GridOffset; z++) {
				float4 splatsAvg = 0.25f * (splats[z, x] + splats[z + 1, x] + splats[z, x + 1] + splats[z + 1, x + 1]);
				float grassSplatAvg = splatsAvg.x;
				if (grassSplatAvg > 0.4f) {
					float grassSplatMax = math.max(
						math.max(splats[z, x    ].x, splats[z + 1, x    ].x),
						math.max(splats[z, x + 1].x, splats[z + 1, x + 1].x)
					);
					float grassDetailVal = grassSplatMax * 10f + rand.Range(-0.5f, 0.5f, x, z, 9);
					detailMap[z, x] = Mathf.RoundToInt(grassDetailVal);
				}
				else {
					detailMap[z, x] = 0;
				}
			}
		}
	}
}

public abstract class LandscapeLayer<L, C> : ChunkBasedDataLayer<L, C>
	where L : LandscapeLayer<L, C>, new()
	where C : LandscapeChunk<L, C>, new()
{
	public abstract int lodLevel { get; }

	public const int GridResolution = 256;
	public int gridResolution = GridResolution;
	public int chunkResolution = GridResolution - 8;
	public float terrainBaseHeight = -100;
	public float terrainHeight = 200;

	public Transform layerParent;

	public List<Terrain> unusedTerrains = new List<Terrain>();

	public LandscapeLayer() {
		layerParent = new GameObject(GetType().Name).transform;
		if (lodLevel < 2)
			AddLayerDependency(new LayerDependency(CultivationLayer.instance, CultivationLayer.requiredPadding, 0));
		if (lodLevel < 3)
			AddLayerDependency(new LayerDependency(LocationLayer.instance, LocationLayer.requiredPadding, 1));
	}

	public Terrain GetTerrainAtWorldPos(Vector3 worldPos) {
		if (GetChunkOfGridPoint(null,
			Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.z),
			chunkW, chunkH, out C chunk, out Point point)
		) {
			return chunk.chunkParent.transform.GetComponentInChildren<Terrain>();
		}
		return null;
	}
}

public class LandscapeLayerA : LandscapeLayer<LandscapeLayerA, LandscapeChunkA> {
	public override int lodLevel { get { return 0; } }
	public override int chunkW { get { return 124; } } // 128 - 4
	public override int chunkH { get { return 124; } }
}

public class LandscapeLayerB : LandscapeLayer<LandscapeLayerB, LandscapeChunkB> {
	public override int lodLevel { get { return 1; } }
	public override int chunkW { get { return 248; } } // 256 - 8
	public override int chunkH { get { return 248; } }
}

public class LandscapeLayerC : LandscapeLayer<LandscapeLayerC, LandscapeChunkC> {
	public override int lodLevel { get { return 2; } }
	public override int chunkW { get { return 496; } } // 512 - 16
	public override int chunkH { get { return 496; } }
}

public class LandscapeLayerD : LandscapeLayer<LandscapeLayerD, LandscapeChunkD> {
	public override int lodLevel { get { return 3; } }
	public override int chunkW { get { return 992; } } // 1024 - 32
	public override int chunkH { get { return 992; } }
}

public class LandscapeChunkA : LandscapeChunk<LandscapeLayerA, LandscapeChunkA> { }
public class LandscapeChunkB : LandscapeChunk<LandscapeLayerB, LandscapeChunkB> { }
public class LandscapeChunkC : LandscapeChunk<LandscapeLayerC, LandscapeChunkC> { }
public class LandscapeChunkD : LandscapeChunk<LandscapeLayerD, LandscapeChunkD> { }
