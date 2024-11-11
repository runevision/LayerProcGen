#if GODOT4
using Godot;

namespace Runevision.Common;

public static class ProjectionExtensions
{
    public static Vector3 MultiplyPoint3x4(this Projection m, Vector3 v)
    {
        return new Vector3
        {
            X = (m.X.X * v.X + m.X.Y * v.Y + m.X.Z * v.Z) + m.X.W,
            Y = (m.Y.X * v.X + m.Y.Y * v.Y + m.Y.Z * v.Z) + m.Y.W,
            Z = (m.Z.X * v.X + m.Z.Y * v.Y + m.Z.Z * v.Z) + m.Z.W
        };
    }

    public static Projection TRS(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        var v0 = rotation * new Vector3(scale.X, 0, 0);
        var v1 = rotation * new Vector3(0, scale.Y, 0);
        var v2 = rotation * new Vector3(0, 0, scale.Z);
        
        var c0 = new Vector4(v0.X, v0.Y, v0.Z, 0);
        var c1 = new Vector4(v1.X, v1.Y, v1.Z, 0);
        var c2 = new Vector4(v2.X, v2.Y, v2.Z, 0);
        var c3 = new Vector4(position.X, position.Y, position.Z, 1);

        return new Projection(c0, c1, c2, c3);
    }
}
#endif
