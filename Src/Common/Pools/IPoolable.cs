/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace Runevision.Common {

	/// <summary>
	/// Represents a poolable object that can be used with the <see cref="ObjectPool"/> class.
	/// </summary>
	public interface IPoolable {
		/// <summary>
		/// Called by the pool when this IPoolable object is returned to the pool.
		/// </summary>
		void Reset();
	}

}
