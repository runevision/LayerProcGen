using Runevision.Common;
using System.Collections.Generic;
using UnityEngine;

public abstract class DeformationSpec {
	public Vector4 splat;
	
	public List<SpecPoint> points = new List<SpecPoint>();
	public GridBounds bounds;

	public GridBounds GetBounds() { return bounds; }

	public void CalculateBounds() {
		bounds = GridBounds.Empty();
		for (int i = 0; i < points.Count; i++) {
			Vector2 point = points[i].pos.xz();
			Vector2 padding = Vector2.one * points[i].outerWidth;
			bounds.Encapsulate((Point)(point - padding));
			bounds.Encapsulate((Point)(point + padding + Vector2.one));
		}
	}
}
