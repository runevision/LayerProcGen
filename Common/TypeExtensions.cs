/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;

namespace Runevision.Common {

	public static class TypeExtensions {

		public static string PrettyName(this Type t) {
			if (t.IsArray) {
				return PrettyName(t.GetElementType()) + "[]";
			}

			if (t.IsGenericType) {
				return string.Format(
					"{0}<{1}>",
					t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.InvariantCulture)),
					string.Join(", ", t.GetGenericArguments().Select(PrettyName)));
			}

			return t.Name;
		}
	}

}
