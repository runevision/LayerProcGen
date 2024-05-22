# Layers and Chunks

You implement pairs of layers and chunks, e.g. ExampleLayer and ExampleChunk. A layer contains chunks of the corresponding type.

## Layer and chunk classes

A layer class must inherit from `ChunkBasedDataLayer` and a chunk class must inherit from `LayerChunk`. Both of those base classes must be used with generic type parameters of the layer and chunk themselves, e.g. `<ExampleLayer, ExampleChunk>`.

Here is a simplified example. See the [Script Templates](ScriptTemplates.md) page for more complete templates or the [Getting Started](GettingStarted.md) page for a simple working example.

```cs
public class ExampleChunk : LayerChunk<ExampleLayer, ExampleChunk> {
	// Data for this chunk goes here.
 
	public override void Create(int level, bool destroy) {
		// Generate or destroy data here.
	}
}
 
public class ExampleLayer : ChunkBasedDataLayer<ExampleLayer, ExampleChunk> {
	// Specify the world space dimensions of the chunks.
	public override int chunkW { get { return 256; } }
	public override int chunkH { get { return 256; } }
 
	public ExampleLayer() {
		// Dependencies on other layers are set up here.
	}
 
	// APIs for requesting data from this layer go here.
}
```

Each layer stores a [RollingGrid](#Runevision.LayerProcGen.RollingGrid) of corresponding chunks. This is automatically handled by the layer base class, though you can optionally specify the size of the grid in the base class constructor.

## Chunk size

Chunks are always rectangular and all chunks in a given layer have the same world space size. This size is specified in the layer by overriding the two properties `chunkW` and `chunkH`.

The size is specified in integers, meaning the smallest possible size is one unit, but apart from that, chunks can have any size.

```cs
// Specify the world space dimensions of the chunks.
public override int chunkW { get { return 256; } }
public override int chunkH { get { return 256; } }
```

There is also a convenience property `chunkSize` of type [Point](#Runevision.Common.Point) which combines the width and height.

## Chunk lifetime

A chunk is generated (invoking its generation code) when it’s depended on, and it's destroyed (recycled) when it’s no longer depended on.

```cs
// Chunk Create method where the generation happens.
public override void Create(int level, bool destroy) {
	if (destroy) {
		// Destroy data for this chunk here.
	}
	else {
		// Create data for this chunk here.
	}
}
```

The following pages will cover how to setup layer dependencies that allow chunks to request data from other layers, as well as the concept of internal layer levels, which is a special and optional concept where a single class can implement multiple layers at once.
