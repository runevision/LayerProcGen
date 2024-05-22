using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
class TerrainDeformationMethod {

	// PathPoint props: float4: innerWidth, outerWidth, splatWidth, centerElevation

	static float mod(float a, float b) {
		return a - b * math.floor(a / b);
	}

	static float inverselerp(float a, float b, float value) {
		return (value - a) / (b - a);
	}

	static float smoothstep(float t) {
		t = math.saturate(t);
		return (t * t * (3.0f - 2.0f * t));
	}

	static float4 LineSegment(in float2 p, in float3 a, in float3 b, in float4 propsA, in float4 propsB, ref float3 minDist) {
		float2 ap = p - a.xz;
		float2 ab = b.xz - a.xz;
		float sqrSegmentLength = math.dot(ab, ab);

		// Where on the segment are we? (0 = a, 1 = b)
		float alongSegment = math.dot(ap, ab) / sqrSegmentLength;
		alongSegment = math.max(-4.0f, math.min(5.0f, alongSegment)); // avoid rare errors
		float alongSegmentClamped = math.saturate(alongSegment);

		// Interpolated properties.
		float4 props = math.lerp(propsA, propsB, alongSegmentClamped);

		// Distance to segment.
		float2 vectorToLine = ab * alongSegmentClamped - ap;
		float dist = math.length(vectorToLine);
		float distToEdge = dist - props.x;
		if (distToEdge < minDist.z) {
			// minDist.z is the distance to the edge (innerWidth).
			minDist.z = distToEdge;
			// minDist.xy is a vector to the center (not edge)
			minDist.xy = vectorToLine;
		}
		// Early out if not within outer dist.
		if (dist >= props.y)
			return new float4();

		// The weight increases from 0 to 1 from outerWidth to innerWidth
		// and continues beyond 1 inside innerWidth.
		float weight = 1.0f - math.min(1, (dist - props.x) / (props.y - props.x));
		// Weight has bias to high influences.
		weight = math.pow(weight, 8.0f);

		// Calculate influence.
		float influence = inverselerp(props.y, props.x, dist);
		influence = smoothstep(influence);

		// Interpolated height.
		float height = math.lerp(a.y, b.y, alongSegment);
		// Calculate and apply height elevation at center.
		float distUnclamped = math.length(ap - ab * alongSegment);
		float centerInfluence = inverselerp(props.x, props.x * 0.5f, distUnclamped);
		centerInfluence = smoothstep(centerInfluence);
		height += props.w * centerInfluence;

		// Calculate splat.
		float splat = math.saturate(inverselerp(props.z + 0.0f, props.z - 0.5f, dist));

		// Output weight, influence, height, splat.
		return new float4(weight, influence * weight, height * weight, splat);
	}

	[BurstCompile]
	public static void ApplySpecs(
		in PointerArray<SpecData> specDatas,
		in PointerArray<SpecPointB> specPoints,
		uint specCount,
		int GridOffsetX,
		int GridOffsetY,
		uint GridSizeX,
		uint GridSizeY,
		in float2 CellSize,
		// output
		ref PointerArray<float> heights,
		ref PointerArray<float3> dists,
		ref PointerArray<float4> splats
	) {
		for (uint x = 0; x < GridSizeX; x++) {
			for (uint y = 0; y < GridSizeY; y++) {
				uint index = x + y * GridSizeX;

				// We have to cast unsigned id to floats before adding to signed grid offset,
				// otherwise it appears we lose negative numbers.
				float2 p = new float2((GridOffsetX + (float)x), (GridOffsetY + (float)y)) * CellSize;

				float4 col = new float4(0,0,0,0);
				float4 splat = new float4(0,0,0,0);
				float3 dist = dists[index];

				uint offset = 0;
				for (uint i = 0; i < specCount; i++) {
					uint count = (uint)specDatas[i].pointCount;
					float4 bounds = specDatas[i].bounds;
					if (p.x > bounds.x && p.y > bounds.y && p.x < bounds.z && p.y < bounds.w) {
						float4 currentSplat = specDatas[i].splat;
						for (uint j = offset; j < offset + count - 1; j++) {
							SpecPointB segA = specPoints[j];
							SpecPointB segB = specPoints[j + 1];
							float4 output = LineSegment(p, segA.pos, segB.pos, segA.props, segB.props, ref dist);
							col += output;
							splat += currentSplat * output.x;
						}
					}
					offset += count;
				}

				dists[index] = dist;

				float height = col.z / col.x;
				float influence = col.y / col.x;
				splat = splat / col.x;

				if (influence > 0) {
					heights[index] = math.lerp(heights[index], height, influence);
					splats[index] = math.lerp(splats[index], splat, math.saturate(col.w));
				}
			}
		}
	}
}
