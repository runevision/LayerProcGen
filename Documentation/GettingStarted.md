# Getting Started in Unity

Follow this guide to quickly get simple procedural generation of chunks up and running in Unity.

## Create layer and chunk classes

First we need to create a layer-and-chunk pair of classes.

Below is a minimal example that creates three random points within each chunk, and where the layer has an API for retrieving all points within given bounds.

You can save this example code as a file named `PointsLayer.cs`:

```cs
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
```

## Add components that set things in motion

Next, the framework needs two things to be set in motion: The LayerManager and a TopLayerDependecy.

* Create an empty GameObject named "LayerManager" and add the **LayerManagerBehavior** component. This component is a wrapper for the `LayerManager` class. Keep the default settings.
* Also add the **DebugDrawer** component to the GameObject. This ensures we can see things drawn with the `DebugDrawer` utility class.
* Create an empty GameObject named "GenerationSource" and add the **GenerationSource** component. This component creates a TopLayerDependency object that establishes a dependency on the specified layer at the specified size.
	* In the `Layer` dropdown, select your `PointsLayer`.
	* Set the `Size` property to 2000, 2000.

## Test the generation

Now you're ready to test the generation!

* Enter Play Mode in Unity.
* Ensure the Scene View is looking at the XY plane (or the XZ plane if you specified that in the LayerManager component).
* Select the "Generation Source" GameObject and frame select it by pressing the F key.
* You should be able to see a bunch of green crosses.
* You can now drag the GenerationSource GameObject around, and observe the generated area of green crosses follow along.

Bonus - setting up debug visualizations:

* Stop Play Mode and select the LayerManager GameObject.
* Add the **VisualizationManager** component.
	* Add an entry to the `Layers` list in the component.
	* Choose the `PointsLayer` in the dropdown.
	* Choose a color for the layer.
* Open the **Debug Options** window via the menu item `Window > Debug Options`.
* Enter Play Mode.
* In the Debug Options window, enable `LayerVis` and `Layers > PointsLayer` to see chunk bounds and the layer bounds.
