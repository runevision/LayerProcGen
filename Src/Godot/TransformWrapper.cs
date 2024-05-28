using Godot;
using Runevision.Common;

/// <summary>
/// Transform stand-in that chunks can create outside of the main thread.
/// </summary>
public class TransformWrapper
{
    public Node3D? transform { get; private set; }

    Node3D layerParent;
    Point chunkIndex;

    public TransformWrapper(Node3D layerParent, Point chunkIndex) {
        this.layerParent = layerParent;
        this.chunkIndex = chunkIndex;
    }

    /// <summary>
    /// Creates the wrapper's own Transform if it doesn't exist, then adds the child.
    /// </summary>
    public void AddChild(Node3D child) {
        if (transform == null) {
            transform = new Node3D{Name="Chunk" + chunkIndex};
            layerParent.AddChild(transform);
            // transform.gameObject.layer = layerParent.gameObject.layer; -> only for collision objects in Godot, so we probably have to either change the type or make a child.
        }
        transform.AddChild(child);
        // child.gameObject.layer = transform.gameObject.layer; //same as above
    }
}