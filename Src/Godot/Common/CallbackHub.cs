/*
 * Godot adaptation copyright (c) 2024 Sythelux Rikd
 *
 * Based on:
 * LayerProcGen copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
#if GODOT4
using System;
using Godot;
using Runevision.Common;

[Tool]
public partial class CallbackHub : Node
{

    public static event Action<double>? update;
    public static event Action<double>? lateUpdate;
    public static event Action<double>? fixedUpdate;

    static CallbackHub? instance;

    static int mainThreadID;
    public static bool isMainThread
    {
        get => System.Environment.CurrentManagedThreadId == mainThreadID;
    }

    public override void _Ready()
    {
        mainThreadID = System.Environment.CurrentManagedThreadId;
        instance ??= this;
        
        //Hooks have to be setup here, there is no hook for "OnSceneLoad" etc.
        update += delta => DebugOption.UpdateAnimValues((float)delta);
    }

    public override void _Process(double delta)
    {
        update?.Invoke(delta);
        lateUpdate?.Invoke(delta); //no lateupdate in Godot natively, but we can do it after all the updates 
    }

    public override void _PhysicsProcess(double delta)
    {
        fixedUpdate?.Invoke(delta);
    }

    public static void ExecuteOnMainThread(Action? action)
    {
        if (action == null)
            return;
        if (isMainThread)
        {
            // If already on main thread, call action right away.
            action();
            return;
        }

        // Otherwise add a callback that calls action and then removes itself.
        Action<double>? handler = null;
        handler = _ =>
        {
            action();
            update -= handler;
        };
        update += handler;
    }
}
#endif
