using Runevision.Common;
using Unity.Mathematics;

public class TerrainNoise {
	public static float GetHeight(DPoint p) {
		float freq = 0.002f;
		float amplitude = 0.5f;
		float result = 0;
		float scalar = 1f;
		// Add a few octaves of noise.
		// Scale higher frequencies by output of lower frequencies
		// to make terrain more bumpy at higher altitudes.
		for (int i = 0; i < 6; i++) {
			float noiseVal = noise.snoise(new float2((float)p.x, (float)p.y) * freq);
			result += noiseVal * scalar * amplitude;
			scalar *= noiseVal * 0.5f + 1.0f;
			freq *= 2f;
			amplitude *= 0.45f;
		}
		return result * 100;
	}
}
