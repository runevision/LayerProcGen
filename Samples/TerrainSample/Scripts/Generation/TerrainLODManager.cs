using Runevision.Common;
using Runevision.LayerProcGen;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLODManager : MonoBehaviour {

	public static TerrainLODManager instance;

	class TerrainInfo {
		public Terrain terrain;
	}

	struct TerrainLODLayer {
		public IChunkBasedDataLayer layer;
		public Dictionary<Point, TerrainInfo> chunks;
	}

	TerrainLODLayer[] layers;
	bool anyRegistrationChanges = false;
	GridBounds lastLowerLevelBounds;

	DebugToggle debugLODBounds = DebugToggle.Create(">Visualizations/Terrain LOD Bounds");

	void Awake() {
		instance = this;
		layers = new TerrainLODLayer[4];
		SetupLODLayer(0, LandscapeLayerA.instance);
		SetupLODLayer(1, LandscapeLayerB.instance);
		SetupLODLayer(2, LandscapeLayerC.instance);
		SetupLODLayer(3, LandscapeLayerD.instance);
	}

	public void SetupLODLayer(int lodLevel, IChunkBasedDataLayer layer) {
		layers[lodLevel] = new TerrainLODLayer() {
			layer = layer,
			chunks = new Dictionary<Point, TerrainInfo>()
		};
	}

	public void RegisterChunk(int lodLevel, Point p, Terrain terrain) {
		layers[lodLevel].chunks[p] = new TerrainInfo() { terrain = terrain };
		anyRegistrationChanges = true;
	}

	public void UnregisterChunk(int lodLevel, Point p) {
		if (layers[lodLevel].chunks.TryGetValue(p, out TerrainInfo info)) {
			layers[lodLevel].chunks.Remove(p);
			anyRegistrationChanges = true;
		}
	}

	void LateUpdate() {
		if (anyRegistrationChanges) {
			anyRegistrationChanges = false;

			// Find bounds of current terrain chunks in all layers.
			GridBounds lowestLayerBounds = GridBounds.Empty();
			int divisor = 1;
			for (int i = layers.Length - 1; i >= 0; i--) {
				foreach (var kvp in layers[i].chunks) {
					Point index = new Point(
						Crd.Div(kvp.Key.x, divisor),
						Crd.Div(kvp.Key.y, divisor)
					);
					lowestLayerBounds.Encapsulate(index);
				}
				divisor *= 2;
			}
			lastLowerLevelBounds = lowestLayerBounds;

			// Activate and deactivate terrain chunks.
			UnityEngine.Profiling.Profiler.BeginSample("HandleActivations");
			int level = layers.Length - 1;
			for (int x = lowestLayerBounds.min.x; x < lowestLayerBounds.max.x; x++) {
				for (int y = lowestLayerBounds.min.y; y < lowestLayerBounds.max.y; y++) {
					HandleAreaIfCovered(level, new Point(x, y));
				}
			}
			UnityEngine.Profiling.Profiler.EndSample();
		}

		// Debug draw.
		if (debugLODBounds.visible) {
			DebugDrawer.alpha = debugLODBounds.animAlpha;
			var lowestLayer = layers[layers.Length - 1].layer;
			VisualizationManager.BeginDebugDraw(lowestLayer, 0);
			DebugDrawer.DrawRect(
				lastLowerLevelBounds.min * lowestLayer.chunkW,
				lastLowerLevelBounds.max * lowestLayer.chunkW,
				0,
				Color.yellow);
			VisualizationManager.EndDebugDraw();

			for (int i = 0; i < layers.Length; i++) {
				VisualizationManager.BeginDebugDraw(layers[i].layer, 0);
				foreach (var kvp in layers[i].chunks) {
					TerrainInfo info = kvp.Value;
					Terrain terrain = info.terrain;
					bool active = terrain.gameObject.activeSelf;
					if (active ||
						(VisualizationManager.instance && VisualizationManager.instance.debugSeparate.visible)
					) {
						Vector2 pos = terrain.transform.position.xz();
						Vector2 size = terrain.terrainData.size.xz() * 0.5f;

						// Draw rect.
						if (active)
							DebugDrawer.DrawRect(pos, pos + size * 2, 0, levelColors[i]);

						// Draw cross in LOD color.
						Vector3 center = (pos + size).xoy();
						float crossSize = size.x * (active ? 1f : 0.3f);
						DebugDrawer.DrawCross(center, crossSize, levelColors[i]);
					}
				}
				VisualizationManager.EndDebugDraw();
			}
			DebugDrawer.alpha = 1f;
		}
	}

	Color[] levelColors = new Color[] {
		new Color(1.0f, 0.9f, 0.1f),
		new Color(0.1f, 1.0f, 0.7f),
		new Color(0.1f, 0.3f, 1.0f),
		new Color(0.8f, 0.1f, 0.5f)
	};

	// Returns true if full area is handled.
	bool HandleAreaIfCovered(int lodLevel, Point index, bool alreadyChecked = false, TerrainInfo selfInfo = null) {
		if (lodLevel < 0)
			return false;

		if (!alreadyChecked)
			layers[lodLevel].chunks.TryGetValue(index, out selfInfo);

		// If at lowest LOD level, just handle self.
		if (lodLevel == 0) {
			if (selfInfo == null)
				return false;
			SetTerrainActiveStatus(selfInfo, true);
			return true;
		}

		int subLevel = lodLevel - 1;
		Point subPointA = index * 2;
		Point subPointB = subPointA + Point.right;
		Point subPointC = subPointA + Point.up;
		Point subPointD = subPointA + Point.one;
		if (selfInfo == null) {
			// We know here that own chunk is not available, so each sub-chunk is the
			// highest potentially available and is allowed to be used individually.
			bool fullAreaHandled = true;
			fullAreaHandled &= HandleAreaIfCovered(subLevel, subPointA);
			fullAreaHandled &= HandleAreaIfCovered(subLevel, subPointB);
			fullAreaHandled &= HandleAreaIfCovered(subLevel, subPointC);
			fullAreaHandled &= HandleAreaIfCovered(subLevel, subPointD);
			return fullAreaHandled;
		}

		// By now we know that own chunk is available, so only use sub-chunks if they cover
		// the full area that own chunk covers, otherwise use own chunk.
		var subChunks = layers[subLevel].chunks;
		if (subChunks.TryGetValue(subPointA, out TerrainInfo subInfoA) & // All four must be evaluated
			subChunks.TryGetValue(subPointB, out TerrainInfo subInfoB) & // so no && here
			subChunks.TryGetValue(subPointC, out TerrainInfo subInfoC) &
			subChunks.TryGetValue(subPointD, out TerrainInfo subInfoD)
		) {
			// All sub-chunks are available, so use those and deactivate own chunk.
			SetTerrainActiveStatus(selfInfo, false);
			HandleAreaIfCovered(subLevel, subPointA, true, subInfoA);
			HandleAreaIfCovered(subLevel, subPointB, true, subInfoB);
			HandleAreaIfCovered(subLevel, subPointC, true, subInfoC);
			HandleAreaIfCovered(subLevel, subPointD, true, subInfoD);
			return true;
		}

		// Not all sub-chunks are available, so use own chunk.
		// Only deactivate sub-chunks if own chunk wasn't already active.
		// If it was already active, sub-chunks can't be active too.
		if (SetTerrainActiveStatus(layers[lodLevel].chunks[index], true)) {
			DisableRecursive(subLevel, subPointA, true, subInfoA);
			DisableRecursive(subLevel, subPointB, true, subInfoB);
			DisableRecursive(subLevel, subPointC, true, subInfoC);
			DisableRecursive(subLevel, subPointD, true, subInfoD);
		}
		return true;
	}

	void DisableRecursive(int lodLevel, Point index, bool alreadyChecked = false, TerrainInfo info = null) {
		if (lodLevel < 0)
			return;

		if (!alreadyChecked)
			layers[lodLevel].chunks.TryGetValue(index, out info);

		if (info != null) {
			// If we can deactivate own chunk, it means own chunk was active before,
			// and that sub-chunks couldn't be active, so no need to handle those.
			if (SetTerrainActiveStatus(info, false))
				return;
		}
		int subLevel = lodLevel - 1;
		Point subPointA = index * 2;
		Point subPointB = subPointA + Point.right;
		Point subPointC = subPointA + Point.up;
		Point subPointD = subPointA + Point.one;
		DisableRecursive(subLevel, subPointA);
		DisableRecursive(subLevel, subPointB);
		DisableRecursive(subLevel, subPointC);
		DisableRecursive(subLevel, subPointD);
	}

	bool SetTerrainActiveStatus(TerrainInfo info, bool active) {
		if (info.terrain.gameObject.activeSelf == active)
			return false;

		UnityEngine.Profiling.Profiler.BeginSample(active ? "Activate" : "Deactivate");
		info.terrain.gameObject.SetActive(active);
		UnityEngine.Profiling.Profiler.EndSample();
		return true;
	}
}
