using Runevision.LayerProcGen;

public class PlayChunk : LayerChunk<PlayLayer, PlayChunk> {
	public PlayChunk() {}
}

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk> {
	public override int chunkW { get { return 8; } }
	public override int chunkH { get { return 8; } }

	public PlayLayer() {
		AddLayerDependency(new LayerDependency(LandscapeLayerA.instance,  256,  256));
		AddLayerDependency(new LayerDependency(LandscapeLayerB.instance,  512,  512));
		AddLayerDependency(new LayerDependency(LandscapeLayerC.instance, 1024, 1024));
		AddLayerDependency(new LayerDependency(LandscapeLayerD.instance, 2048, 2048));
	}
}
