using Runevision.Common;
using System.Collections.Generic;
using UnityEngine;

public class PathSpec : DeformationSpec, IPoolable {

	public List<Vector3> origPoints = new List<Vector3>(); // For debugging.
	public static ListPool<Vector3> pointListPool = new ListPool<Vector3>(512);

	public void Reset() {
		splat = default;
		bounds = default;
		origPoints.Clear();
		points.Clear();
	}

	public PathSpec Init(List<Vector3> points, int type, float width, float slopeWidth, float splatWidth, float centerElevation, int splineSamples = 3) {
		origPoints.AddRange(points);
		PostprocessPathPoints(points, width * 0.5f + slopeWidth * 0.75f);
		List<Vector3> splinePoints = pointListPool.Get();
		GetSplinePoints(points, splinePoints, splineSamples);

		splat = (
			type == 0 ?
			new Vector4(0, 0, 1, 0) :
			new Vector4(0, 0.7f, 0.3f, 0)
		);
		float halfInnerWidth = width * 0.5f;
		float halfOuterWidth = width * 0.5f + slopeWidth;
		float halfSplatWidth = splatWidth * 0.5f;
		for (int i = 0; i < splinePoints.Count; i++) {
			base.points.Add(new SpecPoint() {
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

	void PostprocessPathPoints(List<Vector3> points, float pathWidth) {
		// Store offsets and apply afterwards to avoid order-dependent process.
		// The changing of one corner should not affect the calculation of the next.
		List<Vector3> offsets = pointListPool.Get();
		offsets.Add(Vector3.zero);
		for (int i = points.Count - 2; i >= 1; i--) {
			Vector3 a = points[i + 1];
			Vector3 b = points[i];
			Vector3 c = points[i - 1];

			Vector3 dir1 = Vector3.ProjectOnPlane(a - b, Vector3.up).normalized;
			Vector3 dir2 = Vector3.ProjectOnPlane(c - b, Vector3.up).normalized;
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
		pointListPool.Return(ref offsets);
	}

	void GetSplinePoints(List<Vector3> points, List<Vector3> spline, int samples) {
		int count = points.Count;
		for (int i = 1; i < count; i++) {
			Vector3 a = points[Mathf.Max(i - 2, 0)];
			Vector3 b = points[i - 1];
			Vector3 c = points[i];
			Vector3 d = points[Mathf.Min(i + 1, count - 1)];
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

			spline.Add(b);
			for (int j = 1; j < samples; j++)
				spline.Add(Maths.Bezier(b, b + bt, c + ct, c, (float)j / samples));
		}
		spline.Add(points[points.Count - 1]);
	}

	public void DebugDraw(float drawPath, float drawOrigPath, float drawBounds) {
		if (drawPath > 0) {
			DebugDrawer.alpha = drawPath;
			DrawPath(points, Color.red, Color.cyan);
			for (int i = 1; i < points.Count - 1; i++) {
				Vector3 dir = points[i + 1].pos - points[i - 1].pos;
				dir = new Vector3(dir.z, 0, -dir.x).normalized * points[i].innerWidth;
				DebugDrawer.DrawRay(
					points[i].pos - dir,
					dir * 2,
					Color.Lerp(Color.red * 0.8f, Color.cyan * 0.8f, i / (points.Count + 1f)));
			}
		}
		if (drawOrigPath > 0) {
			DebugDrawer.alpha = drawOrigPath;
			DrawPath(origPoints, Color.cyan, Color.red);
			for (int i = 0; i < origPoints.Count; i++) {
				DebugDrawer.DrawCircle(origPoints[i], 0.1f, 4, Color.white);
			}
		}
		if (drawBounds > 0) {
			DebugDrawer.alpha = drawBounds;
			DebugDrawer.DrawRect(
				bounds.min,
				bounds.max,
				Mathf.Min(points[0].pos.y, points[points.Count - 1].pos.y),
				Color.gray);
		}
		DebugDrawer.alpha = 1f;
	}

	void DrawPath(List<SpecPoint> pathPoints, Color startColor, Color endColor) {
		for (int i = 1; i < pathPoints.Count; i++) {
			DebugDrawer.DrawLine(pathPoints[i - 1].pos, pathPoints[i].pos,
				Color.Lerp(startColor, endColor, (i + 0.5f) / pathPoints.Count));
		}
	}

	void DrawPath(List<Vector3> pathPoints, Color startColor, Color endColor) {
		for (int i = 1; i < pathPoints.Count; i++) {
			DebugDrawer.DrawLine(pathPoints[i - 1], pathPoints[i],
				Color.Lerp(startColor, endColor, (i + 0.5f) / pathPoints.Count));
		}
	}
}
