/*
 * Copyright (c) 2024 Sythelux Rikd
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
#if GODOT4

using Godot;

namespace Runevision.Common;

public static class RunevisionGodotExtensions
{
    
    //@formatter:off
    // Select 2 components out of 3.
    public static Vector2 xy(this Vector3 v) { return new Vector2(v.X, v.Y); }
    public static Vector2 xz(this Vector3 v) { return new Vector2(v.X, v.Z); }
    public static Vector2 yz(this Vector3 v) { return new Vector2(v.Y, v.Z); }

    // Flatten one component out of 3.
    public static Vector3 oyz(this Vector3 v) { return new Vector3(0f, v.Y, v.Z); }
    public static Vector3 xoz(this Vector3 v) { return new Vector3(v.X, 0f, v.Z); }
    public static Vector3 xyo(this Vector3 v) { return new Vector3(v.X, v.Y, 0f); }

    // Expand 2 components to 3.
    public static Vector3 xyo(this Vector2 v) { return new Vector3(v.X, v.Y, 0f); }
    public static Vector3 xoy(this Vector2 v) { return new Vector3(v.X, 0f, v.Y); }
    public static Vector3 oxy(this Vector2 v) { return new Vector3(0f, v.X, v.Y); }
    //@formatter:on
    
    public static Vector3 Clamped(this Vector3 v, float length)
    {
        float l = v.Length();
        if (l > length)
            return v / l * length;
        return v;
    }

    public static void Destroy(this Node obj)
    {
        obj.QueueFree();
    }

    /// <summary>
    /// not necessary: https://docs.godotengine.org/en/3.2/classes/class_reference.html#class-reference
    /// Meshes are References
    /// Unlike Objects, References keep an internal reference counter so that they are automatically released when no longer in use, and only then. References therefore do not need to be freed manually with Object.free.
    /// </summary>
    /// <param name="go"></param>
    public static void DestroyIncludingMeshes(this Node go)
    {
        if (go == null)
            return;
        go.QueueFree();
        // MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>();
        // foreach (var filter in filters)
        // {
        //  // Only destroy meshes with negative instance IDs,
        //  // which means they were not loaded from disk.
        //  if (filter.sharedMesh.GetInstanceID() < 0)
        //   Object.Destroy(filter.sharedMesh);
        // }
        //
        // Object.Destroy(go);
    }
}
#endif
