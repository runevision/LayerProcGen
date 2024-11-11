#if GODOT4
using Godot;

namespace Runevision.LayerProcGen;

internal interface IGodotInstance
{
    public Node? LayerRoot();
}
#endif
