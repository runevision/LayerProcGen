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

	/// <summary>
	/// Internal. Non-generic base class for all layers.
	/// </summary>
	public abstract class AbstractDataLayer {

		// Pertaining to all the different layers.

		static Dictionary<Type, AbstractDataLayer> s_LayerDict = new Dictionary<Type, AbstractDataLayer>();

		/// <summary>
		/// Check if a layer of the specified type exists without creating it as a side effect.
		/// </summary>
		public static bool HasLayer<T>() where T : AbstractDataLayer { return s_LayerDict.ContainsKey(typeof(T)); }

		/// <summary>
		/// An enumeration of all current layers.
		/// </summary>
		public static IEnumerable<AbstractDataLayer> layers { get { return s_LayerDict.Values; } }

		internal static void ResetInstances() {
			foreach (var instance in s_LayerDict.Values)
				instance.ResetInstance();
			s_LayerDict.Clear();
		}

		// Pertaining to one layer.

		internal abstract void ResetInstance();

		internal AbstractDataLayer() {
			if (s_LayerDict.ContainsKey(GetType()))
				Logg.LogError($"Layer {GetType().Name} already created!");

			s_LayerDict.Add(GetType(), this);
		}
	}

}
