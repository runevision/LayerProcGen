using Runevision.Common;
using Runevision.LayerProcGen;
using System.Collections.Generic;

public class PointsChunk : LayerChunk<PointsLayer, PointsChunk> {
	// Data for this chunk goes here.
	// This could be any data structure, a List of points is just an example.
	public List<Point> pointList = new List<Point>();

	public override void Create(int level, bool destroy) {
		if (destroy) {
			// Destroy data for this chunk here.
			// Chunk objects are reused so keep data structures if possible
			// in order to avoid continuous memory allocations.
			pointList.Clear();
		}
		else {
			// Create data for this chunk here.
			for (int i = 0; i < 3; i++) {
				// bounds and index are useful properties of the base LayerChunk class.
				Point point = new Point(
					// The first two Range arguments specify the range.
					// The remaining arguments are input for the hash function.
					layer.rand.Range(bounds.min.x, bounds.max.x, index.x, index.y, i * 2),
					layer.rand.Range(bounds.min.y, bounds.max.y, index.x, index.y, i * 2 + 1));
				pointList.Add(point);
			}
		}
	}
}

public class PointsLayer : ChunkBasedDataLayer<PointsLayer, PointsChunk> {
	// Specify the world space dimensions of the chunks.
	public override int chunkW { get { return 256; } }
	public override int chunkH { get { return 256; } }

	// Data common for all chunks of this layer goes here.
	public RandomHash rand = new RandomHash(1234);

	public PointsLayer() {
		// Dependencies on other layers are set up here with appropriate padding.
		//AddLayerDependency(new LayerDependency(OtherLayer.instance, new Point(16, 16)));

		// Register to get a call per frame (Unity-specific).
		LayerManagerBehavior.OnUpdate -= Update;
		LayerManagerBehavior.OnUpdate += Update;
	}

	// This method is called every frame on the main thread (Unity-specific).
	void Update() {
		// Draw the points in all chunks.
		HandleAllChunks(0, c => {
			foreach (Point p in c.pointList)
				DebugDrawer.DrawCross(p, 16f, UnityEngine.Color.green);
		});
	}

	// APIs for other layers to query data from this layer goes here.
	public void GetPointsInBounds(ILC q, List<Point> outPoints, GridBounds bounds) {
		// Add data within bounds.
		HandleChunksInBounds(q, bounds, 0, chunk => {
			foreach (var point in chunk.pointList)
				if (bounds.Contains(point))
					outPoints.Add(point);
		});
	}
}
