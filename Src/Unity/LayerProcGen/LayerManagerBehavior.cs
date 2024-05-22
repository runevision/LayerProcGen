/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using System;
using UnityEngine;
#if PACKAGE_UNITY_UI
using UnityEngine.UI;
#endif

namespace Runevision.LayerProcGen {

	/// <summary>
	/// Unity component that wraps the LayerManager class.
	/// </summary>
	public class LayerManagerBehavior : MonoBehaviour {

		public enum GenerationPlane { XY, XZ }

		public static LayerManagerBehavior instance { get; private set; }

		public static event Action OnUpdate;

		public LayerManager manager { get; private set; }

		public bool useParallelThreads = true;
		public GenerationPlane generationPlane;
#if PACKAGE_UNITY_UI
		public Text debugQueueText;
		public Text debugStatusText;
#endif

		void Awake() {
			manager = new LayerManager(useParallelThreads);
			instance = this;
		}

		void OnDestroy() {
			manager.OnDestroy();
		}

		protected virtual void Update() {
			MainThreadActionQueue.ProcessQueue();
			DebugDrawer.xzMode = (generationPlane == GenerationPlane.XZ);
			OnUpdate?.Invoke();

#if PACKAGE_UNITY_UI
			if (debugQueueText && debugQueueText.enabled)
				debugQueueText.text = MainThreadActionQueue.idle ? string.Empty
					: "Action Queue: " + MainThreadActionQueue.queueCount;
			if (debugStatusText && debugStatusText.enabled)
				debugStatusText.text = SimpleProfiler.GetStatus();
#endif
		}
	}
}
