using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Runevision.Common {

	/// <summary>
	/// Tool for measuring where time is spent in long-running processes.
	/// </summary>
	/// <remarks>
	/// <para>Measure code by wrapping it in <see cref="o:SimpleProfiler.Begin"/> and
	/// <see cref="SimpleProfiler.End"/>.
	/// Times for entries with the same sectionName will be added together.
	/// Usage across different threads is supported.</para>
	///
	/// <para><see cref="SimpleProfiler.GetStatus"/> can return a string at any time which shows
	/// a hierarchy of monitored calls, and a number of dots to the right of each call
	/// which corresponds to how many threads are currently inside that call.
	/// For sufficiently slow bottlenecks, this makes it possible to spot them by eye,
	/// and just gives an intuitive feel for the flow of the threaded code execution.</para>
	///
	/// <para>In generation of chunks, the chunk stores a <see cref="ProfilerHandle"/> which can
	/// be used as the parentHandle parameter of the <see cref="o:SimpleProfiler.Begin"/> method.
	/// In turn, the <see cref="SimpleProfiler.End"/> call must be supplied the handle returned by
	/// SimpleProfiler.Begin, in order for the profiler to be able to connect matching
	/// Begin and End calls happening simultaneously across many threads.</para>
	///
	/// <para>The LayerManager will call <see cref="SimpleProfiler.Log"/>
	/// whenever generation has completed, which logs the times to the system Console
	/// (not Unity's console).</para>
	/// </remarks>
	public static class SimpleProfiler {

		public class ProfilerInfo : IPoolable {
			public int level;
			public string name;
			public int key;
			public ProfilerInfo parent;
			public int calls;
			public int current;
			public long allocation;
			public long duration;
			public int priority;
			public void Reset() {
				level = 0;
				name = null;
				key = 0;
				parent = null;
				calls = 0;
				current = 0;
				allocation = 0;
				duration = 0;
				priority = 0;
			}
		}

		public struct ProfilerHandle {
			public ProfilerInfo info;
			public long lastMemory;
			public long lastTime;
		}

		public static event Action<string> ForwardBeginSample;
		public static event Action ForwardEndSample;
		public static event Action<string, string> ForwardBeginThread;
		public static event Action ForwardEndThread;
		public static bool isActive = true;

		static ProfilerInfo root = ObjectPool<ProfilerInfo>.GlobalGet();
		static ProfilerHandle rootHandle = new ProfilerHandle() { info = root };
		static Dictionary<int, ProfilerInfo> dict = new Dictionary<int, ProfilerInfo>();
		static StringBuilder statusBuilder = new StringBuilder();
		static int currentTotal;

		public static bool AnyCurrent() {
			return currentTotal == 0;
		}

		public static string GetStatus() {
			statusBuilder.Clear();
			lock (dict) {
				StatusRecursive(statusBuilder, root, -1);
			}
			return statusBuilder.ToString();
		}

		public static ProfilerHandle Begin(string sectionName, int priority = 0) {
			return Begin(rootHandle, sectionName, priority);
		}

		public static ProfilerHandle Begin(ProfilerHandle parentHandle, string sectionName, int priority = 0) {
			if (!isActive)
				return default;

			if (ForwardBeginSample != null)
				ForwardBeginSample(sectionName);

			ProfilerInfo parent = parentHandle.info;
			int key = (parent.key * 2) ^ sectionName.GetHashCode();
			ProfilerInfo info;
			lock (dict) {
				if (!dict.TryGetValue(key, out info)) {
					info = ObjectPool<ProfilerInfo>.GlobalGet();
					dict[key] = info;
					info.name = sectionName;
					info.key = key;
					info.parent = parent;
					info.level = parent.level + 1;
					info.allocation = 0;
					info.duration = 0;
					info.priority = priority;
				}
			}
			lock (root) {
				info.current++;
				currentTotal++;
			}
			ProfilerHandle handle = new ProfilerHandle {
				info = info,
				lastMemory = GC.GetTotalMemory(false),
				lastTime = DateTime.Now.Ticks
			};
			return handle;
		}

		public static void End(ProfilerHandle handle) {
			if (!isActive || handle.info == null)
				return;

			if (ForwardEndSample != null)
				ForwardEndSample();

			long allocation = Math.Max(0, GC.GetTotalMemory(false) - handle.lastMemory);
			long duration = DateTime.Now.Ticks - handle.lastTime;
			if (duration < 0) {
				Logg.LogError("Negative duration " + DateTime.Now.Ticks + " " + handle.lastTime);
			}
			ProfilerInfo info = handle.info;
			lock (root) {
				info.calls++;
				info.allocation += allocation;
				info.duration += duration;
				info.current--;
				currentTotal--;
			}
		}

		public static void BeginThread(string threadGroupName, string threadName) {
			if (ForwardBeginThread != null)
				ForwardBeginThread(threadGroupName, threadName);
		}

		public static void EndThread() {
			if (ForwardEndThread != null)
				ForwardEndThread();
		}

		const int NameLength = 50;
		public static void Log() {
			StringBuilder logString = new StringBuilder(
				("Profiler stats " + DateTime.Now).PadRight(NameLength) + "  calls         ms     alloc\n"
			);
			lock (dict) {
				LogRecursive(logString, root, -1);
				Console.WriteLine(logString.ToString());
				foreach (var info in dict.Values) {
					ProfilerInfo infoCopy = info;
					ObjectPool<ProfilerInfo>.GlobalReturn(ref infoCopy);
				}
				dict.Clear();
			}
		}

		static void LogRecursive(StringBuilder logString, ProfilerInfo info, int level) {
			if (level >= 0) {
				int indent = level * 2;
				float duration = (float)info.duration / TimeSpan.TicksPerMillisecond;
				string namePadded = info.name.PadRight(Math.Max(0, NameLength - indent), '.');
				logString.AppendFormat("{0}{1} {4,6} {2,10:n1} {3,9}\n",
					new string(' ', indent),
					namePadded,
					duration,
					SizeSuffix(info.allocation),
					info.calls
				);
			}
			foreach (var child in dict.Values.Where(e => e.parent == info)
				.OrderByDescending(e => e.priority)
				.ThenByDescending(e => e.duration)
			)
				LogRecursive(logString, child, level + 1);
		}

		static void StatusRecursive(StringBuilder logString, ProfilerInfo info, int level) {
			if (level >= 0) {
				int indent = level * 2;
				logString.AppendFormat("{0}{1} {2}\n",
					new string(' ', indent),
					info.name,
					new string('•', info.current)
				);
			}
			foreach (var child in dict.Values.Where(e => e.parent == info))
				StatusRecursive(logString, child, level + 1);
		}

		static readonly string[] SizeSuffixes = { "B ", "KB", "MB", "GB", "TB" };
		static string SizeSuffix(long value) {
			int sign = Math.Sign(value);
			value *= sign;
			int mag = Math.Max(0, (int)Math.Log(value, 1000));
			decimal adjustedSize = (decimal)value / (1L << (mag * 10));

			return $"{sign * adjustedSize:n1} {SizeSuffixes[mag]}";
		}
	}

}
