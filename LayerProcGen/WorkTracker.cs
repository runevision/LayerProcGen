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

namespace Runevision.LayerProcGen {

	public static class WorkTracker {
		static bool trackingWork;
		static object workLock = new object();
		static Dictionary<object, float> workEstimated = new Dictionary<object, float>();
		static Dictionary<object, float> workNeeded = new Dictionary<object, float>();
		static Dictionary<object, float> workDone = new Dictionary<object, float>();

		public static void AddWorkEstimated(float work, object key) {
			if (!trackingWork)
				return;
			lock (workLock) {
				workEstimated.TryGetValue(key, out float val);
				workEstimated[key] = val + work;
			}
		}

		public static void AddWorkNeeded(float work, object key) {
			if (!trackingWork)
				return;
			lock (workLock) {
				workNeeded.TryGetValue(key, out float val);
				workNeeded[key] = val + work;
			}
		}

		public static void AddWorkDone(float work, object key) {
			if (!trackingWork)
				return;
			lock (workLock) {
				workDone.TryGetValue(key, out float val);
				workDone[key] = val + work;
			}
		}

		public static void WorkIsKnown(object key) {
			if (!trackingWork)
				return;
			lock (workLock) {
				workNeeded.TryGetValue(key, out float val);
				workEstimated[key] = val;
			}
		}

		public static void StartTracking() {
			lock (workLock) {
				workEstimated.Clear();
				workNeeded.Clear();
				workDone.Clear();
				trackingWork = true;
			}
		}

		public static void StopTracking() {
			lock (workLock) {
				trackingWork = false;
				Logg.Log(GetResults(), false);
			}
		}

		public static float CalculateProgress() {
			float workNeededTotal = 0;
			float workDoneTotal = 0;
			lock (workLock) {
				foreach (var kvp in workEstimated) {
					workNeeded.TryGetValue(kvp.Key, out float needed);
					workNeededTotal += Math.Max(kvp.Value, needed);
					workDone.TryGetValue(kvp.Key, out float done);
					workDoneTotal += done;
				}
			}
			return workNeededTotal == 0 ? 0 : (workDoneTotal / workNeededTotal);
		}

		public static string GetResults() {
			string str = "WorkTracker results\n";
			foreach (var kvp in workEstimated) {
				workNeeded.TryGetValue(kvp.Key, out float needed);
				workDone.TryGetValue(kvp.Key, out float done);
				str += $"   {kvp.Key} : estimated: {kvp.Value}  need: {needed}  done: {done}\n";
			}
			return str;
		}
	}

}
