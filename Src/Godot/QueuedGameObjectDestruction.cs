using Godot;
using Runevision.LayerProcGen;

public class QueuedGameObjectDestruction: IQueuedAction {

    TransformWrapper transform;
    bool destroyMeshes;

    /// <summary>
    /// Called by MainThreadActionQueue.
    /// </summary>
    public void Process() {
        if (transform.transform != null) {
            transform.transform.QueueFree(); //TODO: Godot handles queue free of meshes on itself, I think
            // if (destroyMeshes)
                // transform.transform.DestroyIncludingMeshes();
            // else
                // transform.transform.gameObject.Destroy();
        }
    }

    /// <summary>
    /// Enqueue destruction on the main thread of the GameObject wrapped in the TransformWrapper.
    /// </summary>
    /// <param name="tr">The TransformWrapper wrapping the Transform of the GameObject.</param>
    /// <param name="destroyMeshes">If true, MeshFilter components are searched for non-persistent meshes, and these will be destroyed too.</param>
    public static void Enqueue(TransformWrapper tr, bool destroyMeshes) {
        MainThreadActionQueue.Enqueue(new QueuedGameObjectDestruction { transform = tr, destroyMeshes = destroyMeshes });
    }
}