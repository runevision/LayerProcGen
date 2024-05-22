/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace Runevision.Common {

	/// <summary>
	/// A simple wrapper for logging messages, warnings and errors.
	/// </summary>
	/// <remarks>
	/// Logs are output to the OS console and can be forwarded elsewhere too.
	/// In Unity it's forwarded to Unity's console window.
	/// </remarks>
	public static class Logg {
		public static event Action<string> ForwardMessage;
		public static event Action<string> ForwardWarning;
		public static event Action<string> ForwardError;

		static DateTime s_StartTime;

		static Logg() {
			s_StartTime = DateTime.Now;
		}

		public static void Log(string str, bool showInConsole = true) {
			Console.WriteLine((DateTime.Now - s_StartTime) + "   " + str);
			if (showInConsole)
				ForwardMessage?.Invoke((DateTime.Now - s_StartTime) + "   " + str);
		}

		public static void LogWarning(string str) {
			Console.WriteLine((DateTime.Now - s_StartTime) + "   Warning: " + str);
			ForwardWarning?.Invoke((DateTime.Now - s_StartTime) + "   " + str);
		}

		public static void LogError(string str) {
			Console.WriteLine((DateTime.Now - s_StartTime) + "   Error: " + str);
			ForwardError?.Invoke((DateTime.Now - s_StartTime) + "   " + str);
		}
	}

}
