/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Runevision.LayerProcGen {

	/// <summary>
	/// The central LayerManager that takes care of generating the required chunks.
	/// </summary>
	public class LayerManager {

		public static LayerManager instance;

		public bool useParallelThreads { get; private set; }
		public bool isGeneratingInBackground { get; private set; }
		public bool building { get { return isGeneratingInBackground || !MainThreadActionQueue.idle; } }

		public bool aborting { get; private set; }
		internal event Action disposeChunks;

		public List<TopLayerDependency> topDependencies { get; } = new List<TopLayerDependency>();
		public void AddTopDependency(TopLayerDependency d) {
			lock (topDependencies) {
				if (!topDependencies.Contains(d))
					topDependencies.Add(d);
			}
		}
		public void RemoveTopDependency(TopLayerDependency d) {
			lock (topDependencies) {
				if (topDependencies.Contains(d))
					topDependencies.Remove(d);
			}
		}

		Thread layerUpdateThread;

		public LayerManager(bool useParallelThreads) {
			instance = this;

			this.useParallelThreads = useParallelThreads;

			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += MyExceptionHandler;

			aborting = false;
			layerUpdateThread = new Thread(LayerUpdateThread);
			layerUpdateThread.Priority = ThreadPriority.Lowest;
			layerUpdateThread.Start();
		}

		static void MyExceptionHandler(object sender, UnhandledExceptionEventArgs args) {
			Exception e = (Exception)args.ExceptionObject;
			Logg.LogError($"Thread: {e.Message}\n{e.StackTrace}");
		}

		public void Unload() {
			if (aborting)
				return;
			aborting = true;
			layerUpdateThread.Join();

			// Call chunk Dispose methods to dispose native resources such as NativeArrays.
			// It may be impossible to avoid InvalidOperationExceptions in certain cases
			// https://issuetracker.unity3d.com/issues/array-is-marked-as-undisposable-when-using-asyncgpureadback-dot-requestintonativearray
			// so catch those and only warn about them.
			try {
				disposeChunks?.Invoke();
			}
			catch (InvalidOperationException e) {
				Logg.LogWarning("Caught exception in dispose: " + e.Message);
			}
			disposeChunks = null;

			AbstractDataLayer.ResetInstances();
			instance = null;
		}

		public void OnDestroy() {
			Unload();
		}

		void LayerUpdateThread() {
			List<TopLayerDependency> topDependenciesCopy = new List<TopLayerDependency>();
			while (!aborting) {
				bool changed = false;

				// Use a copy of topDependencies so we don't need to
				// lock the original for an extended duration.
				// topDependencies is being accessed (with lock) from
				// the main thread, so locking it here for multiple frames
				// can cause hiccups or deadlocks.
				topDependenciesCopy.Clear();
				lock (topDependencies) {
					topDependenciesCopy.AddRange(topDependencies);
				}

				for (int i = 0; i < topDependenciesCopy.Count; i++) {
					TopLayerDependency dep = topDependenciesCopy[i];
					if (dep.changed) {
						WorkTracker.AddWorkEstimated(100, dep.layer.GetType());
						changed = true;
					}
				}

				if (changed) {
					var ph = SimpleProfiler.Begin("LayerManager Process", 1);

					isGeneratingInBackground = true;

					// Process updated active top dependencies, including new ones.
					for (int i = 0; i < topDependenciesCopy.Count; i++) {
						TopLayerDependency dep = topDependenciesCopy[i];
						if (dep.changed && dep.isActive) {
							dep.abstractLayer.ProcessTopDependency(dep);
							WorkTracker.WorkIsKnown(dep.layer.GetType());
						}
					}
					// Process and remove inactive top dependencies.
					// We do this afterwards to avoid a chunk being briefly removed
					// in cases where one top layer dependency was removed but another
					// which depends on the same chunk was added at the same time.
					for (int i = 0; i < topDependenciesCopy.Count; i++) {
						TopLayerDependency dep = topDependenciesCopy[i];
						if (!dep.isActive) {
							dep.abstractLayer.ProcessTopDependency(dep);
							WorkTracker.WorkIsKnown(dep.layer.GetType());
							RemoveTopDependency(dep);
						}
					}

					SimpleProfiler.End(ph);

					if (SimpleProfiler.AnyCurrent()) {
						if (logGenerationStats.enabled)
							SimpleProfiler.Log();
						else
							SimpleProfiler.ResetStats();
					}
				}
				else {
					isGeneratingInBackground = false;
					if (!aborting)
						Thread.Sleep(5);
				}
			}
		}

		static DebugToggle logGenerationStats = DebugToggle.Create(">Logging/Generation Stats", true);
		static DebugButton logPoolStatsNow = DebugButton.Create(">Logging/Log Pool Stats", PoolManager.LogUsageStats);
	}

}
