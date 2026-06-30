/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
#if GODOT4

using Runevision.Common;
using System;
using System.Collections;
using System.Collections.Concurrent;
using Godot;

namespace Runevision.LayerProcGen {

    /// <summary>
    /// Unity component that wraps the LayerManager class.
    /// </summary>
    public partial class LayerManagerBehavior : Node {

        protected readonly ConcurrentQueue<IEnumerator?> Coroutines = new();
        protected IEnumerator? activeCoroutine;
        
        public enum GenerationPlane { XY, XZ }

        public static LayerManagerBehavior instance { get; private set; }

        public static event Action OnUpdate;

        public LayerManager manager { get; private set; }

        [Export]
        public bool useParallelThreads = true;
        [Export]
        public GenerationPlane generationPlane;
        [Export]
		public Label debugQueueText;
        [Export]
		public Label debugStatusText;

        public override void _EnterTree()
        {
            manager = new LayerManager(useParallelThreads);
            instance = this;
        }

        public override void _ExitTree()
        {
            manager.OnDestroy();
        }

        public override void _Process(double delta)
        {
            if (activeCoroutine == null || !activeCoroutine.MoveNext())
                if (!Coroutines.TryDequeue(out activeCoroutine))
                    activeCoroutine = null;
            MainThreadActionQueue.ProcessQueue();
            DebugDrawer.xzMode = (generationPlane == GenerationPlane.XZ);
            OnUpdate?.Invoke();

            // DebugDraw2D.SetText("Action Queue", MainThreadActionQueue.idle ? string.Empty : MainThreadActionQueue.queueCount); alternative
			if (debugQueueText is { Visible: true })
				debugQueueText.Text = MainThreadActionQueue.idle ? string.Empty : "Action Queue: " + MainThreadActionQueue.queueCount;
			if (debugStatusText is { Visible: true })
				debugStatusText.Text = SimpleProfiler.GetStatus();
        }

        public void StartCoroutine(IEnumerator? coroutine)
        {
            Coroutines.Enqueue(coroutine);
        }
    }
}
#endif
