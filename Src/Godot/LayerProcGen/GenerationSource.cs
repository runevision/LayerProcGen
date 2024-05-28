/*
 * Copyright (c) 2024 Rune Skovbo Johansen, Sythelux Rikd
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
#if GODOT4

using Runevision.Common;
using Godot;


namespace Runevision.LayerProcGen;

/// <summary>
/// Unity component that creates a <see cref="TopLayerDependency"/>.
/// </summary>

public partial class GenerationSource : Node3D
{
    public LayerNamedReference layer = new LayerNamedReference();
    public Point size = Point.zero;

    public TopLayerDependency dep { get; private set; }

    public override void _EnterTree()
    {
        if(!Engine.IsEditorHint())
            UpdateState();
    }

    public override void _ExitTree()
    {
        if (Engine.IsEditorHint()) return;
        if (dep != null)
            dep.isActive = false;
    }

    void UpdateState()
    {
        if (layer == null)
            return;

        // Get layer instance.
        AbstractChunkBasedDataLayer instance = layer.GetLayerInstance();

        // Create top layer dependency based on layer.
        if (instance != null && (dep == null || dep.layer != instance))
        {
            if (dep != null)
                dep.isActive = false;
            dep = new TopLayerDependency(instance, size);
            if (instance is IGodotInstance gInst)
                CallDeferred("add_child", gInst.LayerRoot());
        }
    }

    public override void _Process(double delta)
    {
        if(Engine.IsEditorHint()) return;
        base._Process(delta);
        UpdateState();
        if (dep == null)
            return;

        Vector3 focusPos = Position;
        Point focus;
        if (LayerManagerBehavior.instance?.generationPlane == LayerManagerBehavior.GenerationPlane.XZ)
            focus = (Point)(focusPos.xz());
        else
            focus = (Point)(focusPos.xy());
        dep.SetFocus(focus);
        dep.SetSize(Point.Max(Point.one, size));
    }
}
#endif