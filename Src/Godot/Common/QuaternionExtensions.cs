#if GODOT4
using Godot;

namespace Runevision.Common;

public class QuaternionExtensions
{
    public static Quaternion LookRotation(Vector3 forward, Vector3? up = default)
    {
        up ??= Vector3.Up;
        
        var xDir = forward;
        var zDir = xDir.Cross((Vector3)up);
        var yDir = zDir.Cross(xDir);
        var matrix = new Basis(xDir, yDir, zDir);
        return new Quaternion(matrix);
    }
}
#endif
