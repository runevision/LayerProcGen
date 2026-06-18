using Runevision.Common;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
static class PathsBurstMethod {

	// PathPoint props: float4: innerWidth, outerWidth, splatWidth, centerElevation

	static float inverselerp(float a, float b, float value) {
		return (value - a) / (b - a);
	}

	static float smoothstep(float t) {
		t = math.saturate(t);
		return (t * t * (3.0f - 2.0f * t));
	}

	static float4 LineSegment(in DPoint p, in DPoint3 a, in DPoint3 b, in float4 propsA, in float4 propsB, ref float3 minDist) {
		float2 ap = (float2)(p - a.xz);
		float2 ab = (float2)(b.xz - a.xz);
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
		float height = math.lerp((float)a.y, (float)b.y, alongSegment);
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

	[BurstDiscard]
	static void DebugLogNonBurst() {
		Logg.LogError("Not burst compiled!");
	}

	[BurstCompile(CompileSynchronously = true)]
	public static void ComputePaths(
		in PointerArray<PathData> pathDatas,
		in PointerArray<PathPointB> pathPoints,
		int pathCount,
		int GridOffsetX,
		int GridOffsetY,
		int GridSizeX,
		int GridSizeY,
		in DFloat CellSize,
		// output
		ref PointerArray<float> heights,
		ref PointerArray<float3> dists,
		ref PointerArray<Splat> splats
	) {
		DebugLogNonBurst();
		for (int x = 0; x < GridSizeX; x++) {
			for (int y = 0; y < GridSizeY; y++) {
				int index = x + y * GridSizeX;

				DPoint p = new Point(GridOffsetX + x, GridOffsetY + y) * CellSize;

				float4 col = new float4(0,0,0,0);
				Splat splat = new Splat();
				float3 dist = dists[index];

				uint offset = 0;
				for (uint i = 0; i < pathCount; i++) {
					uint count = (uint)pathDatas[i].pointCount;
					DPoint boundsMin = pathDatas[i].boundsMin;
					DPoint boundsMax = pathDatas[i].boundsMax;
					if (p.x > boundsMin.x && p.y > boundsMin.y && p.x < boundsMax.x && p.y < boundsMax.y) {
						Splat currentSplat = pathDatas[i].splat;
						for (uint j = offset; j < offset + count - 1; j++) {
							PathPointB segA = pathPoints[j];
							PathPointB segB = pathPoints[j + 1];
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
					splats[index] = Splat.Lerp(splats[index], splat, math.saturate(col.w));
				}
			}
		}
	}
}
