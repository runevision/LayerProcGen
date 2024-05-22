/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Runevision.LayerProcGen {

	/// <summary>
	/// Interface for actions enqueued to be processed on the main thread.
	/// </summary>
	public interface IQueuedAction {
		/// <summary>
		/// Called by MainThreadActionQueue.
		/// </summary>
		void Process();
	}

	/// <summary>
	/// A callback to be enqueued to be called on the main thread.
	/// Automatically created when using the methods on
	/// MainThreadActionQueue that take a System.Action as parameter.
	/// </summary>
	// TODO use pooled class rather than struct to avoid boxing.
	public struct QueuedCallback : IQueuedAction {
		public Action callback;

		public QueuedCallback(Action callback) {
			this.callback = callback;
		}

		/// <summary>
		/// Called by MainThreadActionQueue.
		/// </summary>
		public void Process() {
			callback();
		}
	}

	/// <summary>
	/// Enqueue actions from generation threads to be processed on the main thread.
	/// </summary>
	/// <remarks>
	/// Actions can be scheduled in the regular queue, in the fast track queue or
	/// for immediate processing next frame.
	/// Actions scheduled for next frame are always processed in that frame.
	/// If there is remaining budget after that, fast track actions are processed,
	/// and if further remaining budget, actions from the regular queue.
	/// Actions not processed remain in their respective queues for consideration
	/// in the following frame.
	/// </remarks>
	public static class MainThreadActionQueue {

		/// <summary>
		/// The available budget per frame in milliseconds for main thread actions.
		/// Additional actions are processed as long as there is remaining budget,
		/// so the last action processed will likely exceed the budget slightly.
		/// </summary>
		public static int budgetPerFrame { get; set; } = 1;

		/// <summary>
		/// Are all the queues currently empty?
		/// </summary>
		public static bool idle { get { return queueCount == 0; } }

		/// <summary>
		/// The current total amount of queued actions in all queue types.
		/// </summary>
		public static int queueCount { get {
			lock (s_Queue)
			lock (s_FastTrack)
			lock (s_NextFrame) {
				return s_Queue.Count + s_FastTrack.Count + s_NextFrame.Count;
			}
		} }

		static Queue<IQueuedAction> s_Queue = new Queue<IQueuedAction>();
		static Queue<IQueuedAction> s_FastTrack = new Queue<IQueuedAction>();
		static Queue<IQueuedAction> s_NextFrame = new Queue<IQueuedAction>();

		/// <summary>
		/// Enqueue a System.Action on the regular queue, to be performed on the main thread.
		/// </summary>
		public static void Enqueue(Action action) {
			Enqueue(new QueuedCallback(action));
		}

		/// <summary>
		/// Enqueue an IQueuedAction on the regular queue, to be performed on the main thread.
		/// </summary>
		public static void Enqueue(IQueuedAction action) {
			lock (s_Queue) {
				s_Queue.Enqueue(action);
			}
			WorkTracker.AddWorkNeeded(1, typeof(IQueuedAction));
		}

		/// <summary>
		/// Enqueue a System.Action on the fast track queue, to be performed on the main thread
		/// prior to actions in the regular queue.
		/// </summary>
		public static void EnqueueFastTrack(Action action) {
			EnqueueFastTrack(new QueuedCallback(action));
		}

		/// <summary>
		/// Enqueue an IQueuedAction on the fast track queue, to be performed on the main thread
		/// prior to actions in the regular queue.
		/// </summary>
		public static void EnqueueFastTrack(IQueuedAction action) {
			lock (s_FastTrack) {
				s_FastTrack.Enqueue(action);
			}
			WorkTracker.AddWorkNeeded(1, typeof(IQueuedAction));
		}

		/// <summary>
		/// Enqueue a System.Action to be performed on the main thread next frame.
		/// </summary>
		public static void EnqueueNextFrame(Action action) {
			EnqueueNextFrame(new QueuedCallback(action));
		}

		/// <summary>
		/// Enqueue an IQueuedAction to be performed on the main thread next frame.
		/// </summary>
		public static void EnqueueNextFrame(IQueuedAction action) {
			lock (s_NextFrame) {
				s_NextFrame.Enqueue(action);
			}
			WorkTracker.AddWorkNeeded(1, typeof(IQueuedAction));
		}

		/// <summary>
		/// Timer that measures time spent on main thread actions. Reset at the end of ProcessQueue.
		/// </summary>
		/// <remarks>
		/// If you perform other work on the main thread that you want to make use of the same
		/// per-frame buget as the main thread action queue actions, you can call Start and Stop
		/// on this timer before and after that work is performed, respectively. This will reduce
		/// the available budget for the queued main thread actions, possibly postponing them to later,
		/// except for the ones scheduled for next frame.
		/// </remarks>
		public static readonly Stopwatch watch = new Stopwatch();

		/// <summary>
		/// ProcessQueue must be called once per frame on the main thread, typically handled
		/// by the engine-specific part of the framework.
		/// </summary>
		public static void ProcessQueue() {
			lock (s_FastTrack) {
				lock (s_NextFrame) {
					// TODO: Next Frame actions should be guaranteed to be processed regardless of budget.
					while (s_NextFrame.Count > 0) {
						s_FastTrack.Enqueue(s_NextFrame.Dequeue());
					}
				}
			}
			watch.Start();
			//UnityEngine.Profiling.Profiler.BeginSample("Fast Track Actions");
			while (true) {
				IQueuedAction i;
				lock (s_FastTrack) {
					if (s_FastTrack.Count == 0 || (watch.ElapsedMilliseconds >= budgetPerFrame))
						break;
					i = s_FastTrack.Dequeue();
				}
				//UnityEngine.Profiling.Profiler.BeginSample(i.GetType().Name);
				i.Process();
				WorkTracker.AddWorkDone(1, typeof(IQueuedAction));
				//UnityEngine.Profiling.Profiler.EndSample();
			}
			//UnityEngine.Profiling.Profiler.EndSample();
			//UnityEngine.Profiling.Profiler.BeginSample("Queued Actions");
			while (true) {
				IQueuedAction i;
				lock (s_Queue) {
					if (s_Queue.Count == 0 || (watch.ElapsedMilliseconds >= budgetPerFrame))
						break;
					i = s_Queue.Dequeue();
				}
				//UnityEngine.Profiling.Profiler.BeginSample(i.GetType().Name);
				i.Process();
				WorkTracker.AddWorkDone(1, typeof(IQueuedAction));
				//UnityEngine.Profiling.Profiler.EndSample();
			}
			//UnityEngine.Profiling.Profiler.EndSample();
			watch.Reset();
		}
	}

}
