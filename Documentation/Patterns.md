# Software Patterns for Layers

This page covers various aspects of using data layers which are not encoded as features in the framework but are just different ways of using of it.

## Data overlapping vs owned in bounds

Careful consideration is needed when designing the API of a layer which other layers use to request data within given bounds.

"All data in the given bounds" can imply different things depending on the intended usage.

Consider a game which has generated spline roads with two needs:

- The terrain must be flattened along the roads.
- Decorations such as lamp posts must be added along the roads.

### Overlapping bounds

For a given chunk to perform terrain deformation for roads within its own bounds, it needs spline data for all roads overlapping those bounds:

```cs
GetRoadsOverlappingBounds(...)
```

If a road spline overlaps multiple chunks of the layer that performs the terrain deformation, all the overlapping chunks must process that road in order to get seamless results across chunk boundaries.

```cs
public void GetRoadsOverlappingBounds(ILC q, List<Road> outRoads, GridBounds bounds) {
	HandleChunksInBounds(q, bounds, 0, chunk => {
		foreach (var road in chunk.roads)
			if (bounds.Overlaps(road.bounds))
				outRoads.Add(road);
	});
}
```

### Owned within bounds

For a given chunk to perform road decoration (placement of lamp posts) for roads within its own bounds, it needs only spline data for roads that are "owned" within those bounds.

```cs
GetPathsOwnedWithinBounds(...)
```

If a road spline overlaps multiple chunks of the layer that performs road decoration, only one of the chunks must process the road in order for the same road to not get double decorations.

To resolve this deterministically, the layer providing the road spline data must define an ownership “anchor point” for each road spline (for example the center of the road spline bounds) and only return road splines whose anchors are within the requested bounds.

This approach works without the road spline providing layer needing to know anything about the chunk sizes of the layers requesting the data.

```cs
public void GetRoadsOwnedWithinBounds(ILC q, List<Road> outRoads, GridBounds bounds) {
	HandleChunksInBounds(q, bounds, 0, chunk => {
		foreach (var road in chunk.roads)
			if (bounds.Contains(road.bounds.center))
				outRoads.Add(road);
	});
}
```

## Layer types

This documentation is generally assuming layers to be generation layers, but other patterns for using layers are possible, although probably only suitable for advanced users.

### Generation layer

The standard pattern for a data layer is that it generates its own data in the Create method of each chunk.

The chunk may optionally query data from other layers to use as input for its generation.

### Canvas layer

In this pattern a layer has chunks with certain data structures for data storage, but the chunks don't generate any data. Instead other layers can push data to the layer, and the layer will figure out which chunk to store it in. The name "canvas layer" is used since the layer is a "blank canvas" that other layers can "paint on".

Example usages:

- A layer storing spatial debug messages with associated positions in the world.
- A layer storing data for instantiating objects/Prefabs.

In general, potential use cases are when it's beneficial to have a single layer responsible for storing all data of a given type, but many layers can create data of that type.

Something needs to be responsible for using that data - a canvas "user". This can be a separate layer, or it can be a higher internal layer level of the canvas itself. In the latter case, be careful to specify levels of the layer dependencies correctly.

Things get complex if the canvas "user" requires the canvas data to be fully "filled in" within specific bounds. In this case the canvas "user" must specify layer dependencies on all the layers that write into the canvas layer.

It's also worth designing the canvas interface with these things in mind:

- Each data entry has a unique id (not just based on position) and different entries can't overwrite each other.
- A layer whose chunks push data entries to a canvas layer will also take care of requesting those entries removed again when those chunks are destroyed.
