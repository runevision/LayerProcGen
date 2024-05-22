/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace Runevision.Common {

	public interface IPool {
		int CountAll { get; }
		int CountActive { get; }
		int CountInactive { get; }
	}

	/// <summary>
	/// Represents an object pool that poolable objects can be retrieved from and returned to.
	/// </summary>
	/// <remarks>
	/// Used by creating a long-lived pool instance of a given type and, when needed,
	/// calling <c>Get</c> to retrieve an object from the pool and <c>Return</c>
	/// to return it to the pool. The following classes implement the interface:
	/// 
	/// <para><see cref="ObjectPool"/> can be used to pool objects that implement the
	/// <see cref="IPoolable"/> interface and support an empty constructor.
	/// Unlike the other pool types, ObjectPool also has static <c>GlobalGet</c> and
	/// <c>GlobalReturn</c> functions that can be used without creating a pool instance.</para>
	/// 
	/// <para><see cref="ArrayPool"/> can be used to pool arrays of a specific length specified
	/// in the pool constructor. <see cref="Array2DPool"/> and <see cref="Array3DPool"/>
	/// can be used for arrays of two and three dimensions.</para>
	/// 
	/// <para><see cref="ListPool"/> can be used to pool Lists of a specific capacity specified
	/// in the pool constructor.</para>
	/// 
	/// <para>The background for these classes is that, as many Unity developers know, it's best to
	/// avoid generating "garbage" to be collected by the .Net garbage collector.
	/// A common way to avoid this is to pool and reuse resources rather than creating new ones
	/// every time one is needed.</para>
	/// </remarks>
	public interface IPool<T> : IPool {
		/// <summary>
		/// Returns the object to the pool and sets the reference to null.
		/// </summary>
		void Return(ref T obj);

		/// <summary>
		/// Gets an existing object from the pool, or creates one if none are available.
		/// </summary>
		T Get();
	}

}
