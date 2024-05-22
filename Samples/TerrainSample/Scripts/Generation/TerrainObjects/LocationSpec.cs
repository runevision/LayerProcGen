using Runevision.Common;
using UnityEngine;

public class LocationSpec : DeformationSpec, IPoolable {
	
	public void Reset() {
		splat = default;
		bounds = default;
		points.Clear();
	}
	
	public LocationSpec Init(Vector3 point, int type, float width, float slopeWidth, float splatWidth, float centerElevation) {
		splat = (
			type == 0 ?
			new Vector4(0, 0, 1, 0) :
			new Vector4(0, 0.7f, 0.3f, 0)
		);

		float halfInnerWidth = width * 0.5f;
		float halfOuterWidth = width * 0.5f + slopeWidth;
		float halfSplatWidth = splatWidth * 0.5f;
		points.Add(new SpecPoint() {
			pos = point - Vector3.right * 0.01f,
			innerWidth = halfInnerWidth,
			outerWidth = halfOuterWidth,
			splatWidth = halfSplatWidth,
			centerElevation = centerElevation
		});
		points.Add(new SpecPoint() {
			pos = point + Vector3.right * 0.01f,
			innerWidth = halfInnerWidth,
			outerWidth = halfOuterWidth,
			splatWidth = halfSplatWidth,
			centerElevation = centerElevation
		});
		return this;
	}
}
