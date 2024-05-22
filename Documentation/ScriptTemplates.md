## Script Templates

### Layer-and-chunk classes (no internal layers)

The template below contains just the minimal structure needed in a layer-and-chunk pair of classes without further example code.

```cs
using Runevision.Common;
using Runevision.LayerProcGen;

public class ExampleChunk : LayerChunk<ExampleLayer, ExampleChunk> {
	// Data for this chunk goes here.

	public override void Create(int level, bool destroy) {
		if (destroy) {
			// Destroy data for this chunk here.
			// Chunk objects are reused so keep data structures if possible
			// in order to avoid continuous memory allocations.
		}
		else {
			// Create data for this chunk here.
		}
	}
}

public class ExampleLayer : ChunkBasedDataLayer<ExampleLayer, ExampleChunk> {
	// Specify the world space dimensions of the chunks.
	public override int chunkW { get { return 256; } }
	public override int chunkH { get { return 256; } }

	// Data common for all chunks of this layer goes here.

	public ExampleLayer() {
		// Dependencies on other layers are set up here with appropriate padding.
		//AddLayerDependency(new LayerDependency(OtherLayer.instance, new Point(16, 16)));
	}

	// APIs for requesting data from this layer go here.
}
```

### Layer-and-chunk classes with internal layers

The template below is for a layer-and-chunk pair of classes with multiple internal layer levels.

```cs
using Runevision.Common;
using Runevision.LayerProcGen;

public class ExampleChunk : LayerChunk<ExampleLayer, ExampleChunk> {
    // Data for level 0 of this chunk goes here.
    
    // Data for level 1 of this chunk goes here.
    
    // Data for level 2 of this chunk goes here.
 
    public override void Create(int level, bool destroy) {
        if (level == (int)ExampleLayer.Levels.Level0) {
            if (destroy) {
                // Destroy data for level 0 of this chunk here.
            }
            else {
                // Create data for level 0 of this chunk here.
            }
        }
        if (level == (int)ExampleLayer.Levels.Level1) {
            if (destroy) {
                // Destroy data for level 1 of this chunk here.
            }
            else {
                // Create data for level 1 of this chunk here.
            }
        }
        if (level == (int)ExampleLayer.Levels.Level2) {
            if (destroy) {
                // Destroy data for level 2 of this chunk here.
            }
            else {
                // Create data for level 2 of this chunk here.
            }
        }
    }
}

public class ExampleLayer : ChunkBasedDataLayer<ExampleLayer, ExampleChunk> {
	// Specify the world space dimensions of the chunks.
	public override int chunkW { get { return 256; } }
	public override int chunkH { get { return 256; } }

	public enum Levels { Level0, Level1, Level2, Length }
	public override int GetLevelCount() { return (int)Levels.Length; }

	// Data common for all chunks of this layer goes here.

	public ExampleLayer() {
		// Dependencies on other layers are set up here with appropriate padding.
		//AddLayerDependency(new LayerDependency(OtherLayer.instance, new Point(16, 16)));
	}
	
	// Method for chunks to get neighbor chunks. Includes chunk itself.
	public IEnumerable<ExampleChunk> GetNeighborChunks(ExampleChunk chunk) {
	    for (int i = -1; i <= 1; i++) {
	        for (int j = -1; j <= 1; j++) {
	            yield return chunks[chunk.index.x + i, chunk.index.y + j];
	        }
	    }
	}

	// APIs for requesting data from this layer go here.
}
```

### Intermediary base classes

If you have functionality you want to share between all your layers, or just multiple of your layers, you can off course modify the ChunkBasedDataLayer and LayerChunk classes directly.

However, if you'd prefer not to alter the classes of the LayerProcGen framework, you can instead implement intermediary classes with custom functionality and make your layers derive from those. You can use the template below as a starting point for that.

```cs
// Custom base layer and chunk classes for multiple/all layers.

public abstract class MyBaseLayerChunk<L, C> : LayerChunk<L, C>
	where L : MyBaseChunkBasedDataLayer<L, C>, new()
	where C : MyBaseLayerChunk<L, C>, new()
{
	// Custom chunk functionality goes here.
}

public abstract class MyBaseChunkBasedDataLayer<L, C> : ChunkBasedDataLayer<L, C>
	where L : MyBaseChunkBasedDataLayer<L, C>, new()
	where C : MyBaseLayerChunk<L, C>, new()
{
	// Custom layer functionality goes here.
}
```
