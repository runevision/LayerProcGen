/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;
using UnityEngine.Profiling;

namespace Runevision.Common {

	public static class ForwardingUnityWrapper {

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		static void Register() {
			Logg.ForwardMessage += Debug.Log;
			Logg.ForwardWarning += str => Debug.LogWarning(str + "\n\n" + StackTraceUtility.ExtractStackTrace());
			Logg.ForwardError += str => Debug.LogError(str + "\n\n" + StackTraceUtility.ExtractStackTrace());

			SimpleProfiler.ForwardBeginThread += (threadGroupName, threadName) =>
				Profiler.BeginThreadProfiling(threadGroupName, threadName);
			SimpleProfiler.ForwardEndThread += () => Profiler.EndThreadProfiling();
			SimpleProfiler.ForwardBeginSample += str => Profiler.BeginSample(str);
			SimpleProfiler.ForwardEndSample += () => Profiler.EndSample();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void RegisterAfterSceneLoad() {
			CallbackHub.GuaranteeInstance();
			CallbackHub.update += () => DebugOption.UpdateAnimValues(Time.deltaTime);
		}
	}

}
