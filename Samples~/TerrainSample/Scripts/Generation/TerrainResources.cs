using UnityEngine;

[CreateAssetMenu]
public class TerrainResources : SingletonAsset<TerrainResources> {
	public Texture2D grassTex;
	public Texture2D cliffTex;
	public Texture2D pathTex;
	public Texture2D grassDetail;

	[Space]
	public Material material;
	public TerrainData terrainData;
}
