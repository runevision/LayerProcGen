using Runevision.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPathFinder : IPoolable {

	public const int cellSize = 6;
	public const int halfCellSize = cellSize / 2;

	DPoint start, goal;
	Func<DPoint, float> heightFunction;
	Func<DPoint, float> costFunction;
	HashSet<DPoint> closedset = new HashSet<DPoint>(); // The set of nodes already evaluated.
	PriorityQueue<DPoint> openset = new PriorityQueue<DPoint>(); // The set of tentative nodes to be evaluated.
	Dictionary<DPoint, float> g_score = new Dictionary<DPoint, float>(); // Actual cost from start to node.
	Dictionary<DPoint, DPoint> came_from = new Dictionary<DPoint, DPoint>(); // Distance from start along optimal.

	// Do not reset this one; we want to reuse the cached heights.
	// Important to use different pools for pathfinders used for different things!
	// Otherwise the cached heights are going to be incorrect.
	Dictionary<DPoint, float> cachedHeights = new Dictionary<DPoint, float>();

	public void Reset() {
		start = default;
		goal = default;
		heightFunction = null;
		costFunction = null;
		closedset.Clear();
		openset.Clear();
		g_score.Clear();
		came_from.Clear();
	}

	public static void FindFootPath(ObjectPool<TerrainPathFinder> pool,
		List<DPoint> outPath, DPoint start, DPoint goal, int cellSize,
		Func<DPoint, float> heightFunction,
		Func<DPoint, float> costFunction
	) {
		TerrainPathFinder pf = pool.Get();
		pf.heightFunction = heightFunction;
		pf.costFunction = costFunction;
		pf.FindPath(outPath, start, goal, cellSize, pf.GetFootCost, footAdjacents);
		pool.Return(ref pf);
	}

	public static void FindWaterPath(ObjectPool<TerrainPathFinder> pool,
		List<DPoint> outPath, DPoint start, DPoint goal, int cellSize,
		Func<DPoint, float> heightFunction,
		Func<DPoint, float> costFunction
	) {
		TerrainPathFinder pf = pool.Get();
		pf.heightFunction = heightFunction;
		pf.costFunction = costFunction;
		pf.FindPath(outPath, start, goal, cellSize, pf.GetWaterCost, waterAdjacents);
		pool.Return(ref pf);
	}

	void FindPath(List<DPoint> outPath, DPoint start, DPoint goal, int cellSize,
		Func<DPoint, DPoint, bool, float> cost,
		DPoint[] adjacents
	) {
		// Special handling if already at goal
		if (start == goal)
			return;

		this.start = start;
		this.goal = goal;

		float maxDistSqr = 60 * 60;
		float directDistCost = cost(start, goal, true);
		openset.Enqueue(start, directDistCost);
		g_score[start] = 0;
		int examined = 0;
		int discarded = 0;
		while (openset.Count > 0 && examined < 10000) {
			examined++;
			// Get the node in openset having the lowest f-score value:
			DPoint cur = openset.Dequeue();

			// Check if the shortest path has been found - then reconstruct it:
			if (cur == goal) {
				ConstructPath(outPath);
				//DebugDrawClosedSet ();
				return;
			}

			// Switch cur to the closed list:
			closedset.Add(cur);

			for (int i = 0; i < adjacents.Length; i++) {
				DPoint neighbor = cur + adjacents[i] * cellSize;
				if (closedset.Contains(neighbor))
					continue;

				if (TooFarAway(start, goal, maxDistSqr, neighbor)) {
					discarded++;
					continue;
				}

				float tentative_g_score = g_score[cur] + cost(cur, neighbor, false);
				if (!g_score.ContainsKey(neighbor) || tentative_g_score < g_score[neighbor]) {
					// Queue up neighbor with estimated total distance from start to goal through neighbor:
					openset.Enqueue(neighbor, tentative_g_score + cost(neighbor, goal, true));
					came_from[neighbor] = cur;
					g_score[neighbor] = tentative_g_score;
				}
			}
		}
		Logg.LogError("Could not find path from " + start + " to " + goal + " (" + examined + " examined, openset count " + openset.Count + ").");
		DebugDrawClosedSet();
	}

	void DebugDrawClosedSet() {
		foreach (DPoint p in closedset) {
			DPoint o;
			if (came_from.TryGetValue(p, out o)) {
				DebugDrawer.DrawLine(((Vector2)o).xoy(), ((Vector2)p).xoy(), Color.yellow, 1000);
				DebugDrawer.DrawCircle(((Vector2)p), 1f, 4, Color.red, 1000);
			}
		}
	}

	bool TooFarAway(DPoint start, DPoint goal, float maxDistSqr, DPoint neighbor) {
		// Discard points too far away from line segment between start and goal.
		float lineSegmentSqrDist = Maths.LineSegmentPointSqrDist(start, goal, neighbor);
		if (lineSegmentSqrDist > maxDistSqr) {
			closedset.Add(neighbor);
			return true;
		}
		return false;
	}

	void ConstructPath(List<DPoint> outPath) {
		DPoint p = goal;
		outPath.Add(p);
		while (came_from.ContainsKey(p)) {
			p = came_from[p];
			outPath.Add(p);
		}
		//Debug.Log ("Pathfinding examined: " + examined + " discarded: " + discarded);
		outPath.Reverse();
	}

	static DPoint[] footAdjacents = new DPoint[] {
		// Cardinal * 2
		new DPoint (+2, 0),
		new DPoint (-2, 0),
		new DPoint (0, +2),
		new DPoint (0, -2),

		// Diagonal * 2
		new DPoint (+2, +2),
		new DPoint (-2, +2),
		new DPoint (-2, -2),
		new DPoint (+2, -2),

		// "Knight chess move"
		new DPoint (+2, +1),
		new DPoint (-2, +1),
		new DPoint (-2, -1),
		new DPoint (+2, -1),
		new DPoint (+1, +2),
		new DPoint (-1, +2),
		new DPoint (-1, -2),
		new DPoint (+1, -2),

		// Cardinal * 3
		new DPoint (+3, 0),
		new DPoint (-3, 0),
		new DPoint (0, +3),
		new DPoint (0, -3),

		// Diagonal * 3
		new DPoint (+3, +3),
		new DPoint (-3, +3),
		new DPoint (-3, -3),
		new DPoint (+3, -3),
	};

	static DPoint[] waterAdjacents = new DPoint[] {
		// Cardinal
		new DPoint (+1, 0),
		new DPoint (-1, 0),
		new DPoint (0, +1),
		new DPoint (0, -1),

		// Diagonal
		new DPoint (+1, +1),
		new DPoint (-1, +1),
		new DPoint (-1, -1),
		new DPoint (+1, -1),

		// "Knight chess move"
		new DPoint (+2, +1),
		new DPoint (-2, +1),
		new DPoint (-2, -1),
		new DPoint (+2, -1),
		new DPoint (+1, +2),
		new DPoint (-1, +2),
		new DPoint (-1, -2),
		new DPoint (+1, -2),
	};

	// Dynamic partitions based on length of segment would be more correct,
	// but it seems it causes the pathfinding to often fail.
	/*float GetFootCost (DPoint a, DPoint b, bool lowerBound) {
		if (lowerBound)
			return GetCostDirect (a, b, true);
		int partitions = Mathf.Max (Mathf.Abs ((int)(b.x - a.x)), Mathf.Abs ((int)(b.y - a.y))) / cellSize;
		float cost = 0;
		for (int i = 0; i < partitions; i++) {
			cost += GetCostDirect (
				((a *  i     ) + (b * (partitions - i    ))) / partitions,
				((a * (i + 1)) + (b * (partitions - i - 1))) / partitions,
				false
			);
		}
		return cost;
	}*/

	float GetFootCost(DPoint a, DPoint b, bool lowerBound) {
		if (lowerBound)
			return GetCostDirect(a, b, true);
		return GetCostDirect(a, (a + b) / 2, false) + GetCostDirect((a + b) / 2, b, false);
	}

	public float steepnessCostPower = 2.0f;
	public float steepnessCostMultiplier = 6.0f;

	float GetCostDirect(DPoint a, DPoint b, bool lowerBound) {
		float flatDist = DPoint.Distance(a, b);
		if (flatDist == 0)
			return 0;

		// Penalty from cost function.
		float avgCost = (costFunction(a) + costFunction(b)) * 0.5f;

		// Penalty for steepness.
		float heightDiff = Mathf.Abs(
			GetHeight(a, heightFunction, cachedHeights) -
			GetHeight(b, heightFunction, cachedHeights)
		);
		if (lowerBound) {
			// For a given height difference, a certain (flat) path length provides the lowest cost.
			// If the steepnessCostPower is 2, the lower bound cost path length is
			// heightDiff * steepnessCostMultiplier, so we ensure the length is at minimum that long.
			flatDist = Mathf.Max(flatDist, heightDiff * steepnessCostMultiplier);
		}
		float slope = heightDiff / flatDist;
		float slopePenalty = Mathf.Pow(slope * steepnessCostMultiplier, steepnessCostPower);

		return flatDist * (1f + slopePenalty) + avgCost * 12;
	}

	float GetWaterCost(DPoint a, DPoint b, bool lowerBound) {
		float flatDist = DPoint.Distance(a, b);
		if (flatDist == 0)
			return 0;

		// Penalty for being far away from line between start and goal.
		Vector2 sg = (Vector2)(goal - start);
		Vector2 middle = 0.5f * (Vector2)(a + b);
		Vector2 sm = (Vector2)(middle - start);
		float sidewaysPenalty = (sm - Vector2.Dot(sm, sg) / Vector2.Dot(sg, sg) * sg).sqrMagnitude * 0.001f;

		// Penalty from cost function.
		float avgCost = (costFunction(a) + costFunction(b)) * 0.5f;

		// Penalty for sideways steepness.
		Vector2 side = new Vector2(sg.y, -sg.x).normalized;
		float sideSlope =
			GetHeight(middle - side, heightFunction, cachedHeights) -
			GetHeight(middle + side, heightFunction, cachedHeights);
		float sideSlopePenalty = Mathf.Pow(Mathf.Abs(sideSlope) * 1f, 1f);

		// Penalty for going upwards.
		float heightDiff = Mathf.Max(0f,
			GetHeight(b, heightFunction, cachedHeights) -
			GetHeight(a, heightFunction, cachedHeights)
		);
		float slope = heightDiff / flatDist;
		float slopePenalty = Mathf.Pow(slope * 5000f, 1f);

		return flatDist * (1f + slopePenalty) * (1f + sideSlopePenalty) * (1f + sidewaysPenalty) * (1f + avgCost);
	}

	float GetCostLowerBound(DPoint a, DPoint b) {
		//return DPoint.Distance (a, b);
		return GetFootCost(a, b, true);
	}

	float GetHeight(DPoint p, Func<DPoint, float> heightFunction, Dictionary<DPoint, float> cachedHeights) {
		float height;
		if (!cachedHeights.TryGetValue(p, out height)) {
			height = heightFunction(p);
			cachedHeights[p] = height;
		}
		return height;
	}
}
