using Runevision.Common;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPath : IPoolable {
	public int type;
	public List<DPoint3> origPoints = new List<DPoint3>(); // For debugging.
	public List<PathPoint> pathPoints = new List<PathPoint>();
	public GridBounds bounds;

	public GridBounds GetBounds() { return bounds; }

	public static ListPool<DPoint3> pointListPool = new ListPool<DPoint3>(512);
	public static ListPool<Vector3> vectorListPool = new ListPool<Vector3>(512);

	public TerrainPath() { }

	public void Reset() {
		type = 0;
		origPoints.Clear();
		pathPoints.Clear();
		bounds = new GridBounds();
	}

	public TerrainPath Init(List<DPoint3> points, int type, float width, float slopeWidth, float splatWidth, float centerElevation, int splineSamples = 3) {
		origPoints.AddRange(points);
		PostprocessPathPoints(points, width * 0.5f + slopeWidth * 0.75f);
		List<DPoint3> splinePoints = pointListPool.Get();
		GetSplinePoints(points, splinePoints, splineSamples);

		this.type = type;
		float halfInnerWidth = width * 0.5f;
		float halfOuterWidth = width * 0.5f + slopeWidth;
		float halfSplatWidth = splatWidth * 0.5f;
		for (int i = 0; i < splinePoints.Count; i++) {
			pathPoints.Add(new PathPoint() {
				pos = splinePoints[i],
				innerWidth = halfInnerWidth,
				outerWidth = halfOuterWidth,
				splatWidth = halfSplatWidth,
				centerElevation = centerElevation
			});
		}
		pointListPool.Return(ref splinePoints);
		return this;
	}

	public TerrainPath InitPoint(DPoint3 point, int type, float width, float slopeWidth, float splatWidth, float centerElevation) {
		this.type = type;
		float halfInnerWidth = width * 0.5f;
		float halfOuterWidth = width * 0.5f + slopeWidth;
		float halfSplatWidth = splatWidth * 0.5f;
		pathPoints.Add(new PathPoint() {
			pos = point - Vector3.right * 0.01f,
			innerWidth = halfInnerWidth,
			outerWidth = halfOuterWidth,
			splatWidth = halfSplatWidth,
			centerElevation = centerElevation
		});
		pathPoints.Add(new PathPoint() {
			pos = point + Vector3.right * 0.01f,
			innerWidth = halfInnerWidth,
			outerWidth = halfOuterWidth,
			splatWidth = halfSplatWidth,
			centerElevation = centerElevation
		});
		return this;
	}

	public void CalculateBounds() {
		bounds = GridBounds.Empty();
		for (int i = 0; i < pathPoints.Count; i++) {
			DPoint point = pathPoints[i].pos.xz;
			Vector2 padding = Vector2.one * pathPoints[i].outerWidth;
			bounds.Encapsulate((Point)(point - padding));
			bounds.Encapsulate((Point)(point + padding + Vector2.one));
		}
	}

	void PostprocessPathPoints(List<DPoint3> points, float pathWidth) {
		// Store offsets and apply afterwards to avoid order-dependent process.
		// The changing of one corner should not affect the calculation of the next.
		List<Vector3> offsets = vectorListPool.Get();
		offsets.Add(Vector3.zero);
		for (int i = points.Count - 2; i >= 1; i--) {
			DPoint3 a = points[i + 1];
			DPoint3 b = points[i];
			DPoint3 c = points[i - 1];

			Vector3 dir1 = Vector3.ProjectOnPlane((Vector3)(a - b), Vector3.up).normalized;
			Vector3 dir2 = Vector3.ProjectOnPlane((Vector3)(c - b), Vector3.up).normalized;
			float halfAngle = Vector3.Angle(dir1, dir2) * 0.5f;
			float sep = Mathf.Cos(halfAngle * Mathf.Deg2Rad);
			if (sep < 0.8f) {
				offsets.Add(Vector3.zero);
				continue;
			}

			// Duplicate middle point.
			points.Insert(i, points[i]);
			// Move former of the duplicated points a little bit away from duplicate.
			Vector3 offset = (dir2 - dir1).normalized * sep * pathWidth * 0.5f;
			offsets.Add(-offset);
			offsets.Add(offset);
		}
		offsets.Add(Vector3.zero);
		for (int i = 0; i < points.Count; i++) {
			points[i] += offsets[points.Count - 1 - i];
		}
		vectorListPool.Return(ref offsets);
	}

	void GetSplinePoints(List<DPoint3> points, List<DPoint3> spline, int samples) {
		int count = points.Count;
		for (int i = 1; i < count; i++) {
			// Subtract first point now, and add it back afterward. This keeps spline floating
			// point calculations in local space rather than arbitrarily large world coords.
			DPoint3 worldA = points[Mathf.Max(i - 2, 0)];
			Vector3 a = Vector3.zero;
			Vector3 b = (Vector3)(points[i - 1] - worldA);
			Vector3 c = (Vector3)(points[i] - worldA);
			Vector3 d = (Vector3)(points[Mathf.Min(i + 1, count - 1)] - worldA);
			Vector3 ab = b - a;
			Vector3 bc = c - b;
			Vector3 cd = d - c;
			float abl = ab.magnitude;
			float bcl = bc.magnitude;
			float cdl = cd.magnitude;
			Vector3 abn = abl == 0 ? Vector3.zero : ab / abl;
			Vector3 bcn = bcl == 0 ? Vector3.zero : bc / bcl;
			Vector3 cdn = cdl == 0 ? Vector3.zero : cd / cdl;
			Vector3 bt = (abn + bcn).normalized * (i == 1 ? bcl : Mathf.Min(abl, bcl)) / 3;
			Vector3 ct = -(bcn + cdn).normalized * (i == count - 1 ? bcl : Mathf.Min(bcl, cdl)) / 3;

			// Make first and last tangent horizontal.
			if (i == 1)
				bt.y = 0;
			if (i == count - 1)
				ct.y = 0;

			spline.Add(worldA + b);
			for (int j = 1; j < samples; j++)
				spline.Add(worldA + Maths.Bezier(b, b + bt, c + ct, c, (float)j / samples));
		}
		spline.Add(points[points.Count - 1]);
	}

	public void DebugDraw(float drawPath, float drawOrigPath, float drawBounds) {
		if (drawPath > 0) {
			DebugDrawer.alpha = drawPath;
			DrawPath(pathPoints, Color.red, Color.cyan);
			for (int i = 1; i < pathPoints.Count - 1; i++) {
				Vector3 dir = (Vector3)(pathPoints[i + 1].pos - pathPoints[i - 1].pos);
				dir = new Vector3(dir.z, 0, -dir.x).normalized * pathPoints[i].innerWidth;
				DebugDrawer.DrawRay(
					pathPoints[i].pos - (Point3)dir,
					dir * 2,
					Color.Lerp(Color.red * 0.8f, Color.cyan * 0.8f, i / (pathPoints.Count + 1f)));
			}
		}
		if (drawOrigPath > 0) {
			DebugDrawer.alpha = drawOrigPath;
			DrawPath(origPoints, Color.cyan, Color.red);
			for (int i = 0; i < origPoints.Count; i++) {
				DebugDrawer.DrawCircle((Vector3)origPoints[i], 0.1f, 4, Color.white);
			}
		}
		if (drawBounds > 0) {
			DebugDrawer.alpha = drawBounds;
			DebugDrawer.DrawRect(
				bounds.min,
				bounds.max,
				Mathf.Min((float)pathPoints[0].pos.y, (float)pathPoints[pathPoints.Count - 1].pos.y),
				Color.gray);
		}
		DebugDrawer.alpha = 1f;
	}

	void DrawPath(List<PathPoint> pathPoints, Color startColor, Color endColor) {
		for (int i = 1; i < pathPoints.Count; i++) {
			DebugDrawer.DrawLine(pathPoints[i - 1].pos, pathPoints[i].pos,
				Color.Lerp(startColor, endColor, (i + 0.5f) / pathPoints.Count));
		}
	}

	void DrawPath(List<DPoint3> pathPoints, Color startColor, Color endColor) {
		for (int i = 1; i < pathPoints.Count; i++) {
			DebugDrawer.DrawLine(pathPoints[i - 1], pathPoints[i],
				Color.Lerp(startColor, endColor, (i + 0.5f) / pathPoints.Count));
		}
	}
}
