/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Runevision.Common {

	public static class PoolManager {
		public static List<IPool> allPools = new List<IPool>();
		//static Dictionary<object, IPool> sourcePools = new();

		public static void TrackSourcePool(object element, IPool pool) {
			/*lock (sourcePools) {
				sourcePools[element] = pool;
			}*/
		}

		/*public static void Return (object obj) {
			lock (sourcePools) {
				if (sourcePools.TryGetValue (obj, out IPool pool)) {
					pool.Return (obj);
				}
				else {
					Logg.LogError ("Could not return " + obj + " to pool as it's not tracked.");
				}
			}
		}

		public static void ReturnAllAndClear (this IEnumerable<IPoolable> list) {
			foreach (var element in list)
				Return (element);
			var iList = list as IList<IPoolable>;
			if (iList != null)
				iList.Clear ();
		}

		public static void ReturnToPool (this IPoolable element) {
			Return (element);
		}*/

		public static void LogUsageStats() {
			lock (PoolManager.allPools) {
				var sorted = PoolManager.allPools.OrderBy(e => e.ToString());
				StringBuilder logString = new StringBuilder($"Pool stats {DateTime.Now}\n");
				foreach (IPool pool in sorted) {
					logString.AppendFormat("{1,5} capacity ({2,5} active, {3,5} inactive)  {0}\n",
						pool, pool.CountAll, pool.CountActive, pool.CountInactive);
				}
				Console.WriteLine(logString.ToString());
			}
		}
	}

}
