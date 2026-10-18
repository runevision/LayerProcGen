/*
 * Copyright (c) 2026 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace Runevision.Common {

	/// <summary>
	/// Interface for handling floating origin shifts, which should update all
	/// state that represents a position in the scene.
	/// </summary>
	/// <remarks>
	/// A floating origin entails a difference between:
	///
	/// World space, which can be very large and should be expressed in fixed
	/// point coordinates.
	///
	/// Scene space, which is what the engine sees and is handled in floating
	/// point coordinates.
	///
	/// The floating origin refers to the ability to specify and change the
	/// position of the scene origin in world space. An object's scene space
	/// coordinates are then equal their world space coordinates relative to
	/// the (world space) scene origin. The origin handlers need to enforce this
	/// whenever the scene origin changes.
	/// </remarks>
	public interface IOriginHandler {
		/// <summary>
		/// An origin handler can either be authoritative or delta-based.
		/// </summary>
		/// <remarks>
		/// An authoritative handler knows an object's world position in fixed
		/// point coordinates and calculates a new scene position based on that,
		/// disregarding the old origin parameter.
		/// </remarks>
		/// <example>
		/// <code>
		/// DPoint3 worldPosition;
		///
		/// public void HandleOriginShift(Point3 oldOrigin, Point3 newOrigin) {
		/// 	Vector3 scenePosition = (Vector3)(worldPosition - newOrigin);
		/// 	transform.position = scenePosition;
		/// }
		/// </code>
		/// </example>
		/// <remarks>
		/// A delta-based handler only knows an object's current scene position and
		/// subtracts a delta from it which is the new origin minus the old one.
		/// </remarks>
		/// <example>
		/// <code>
		/// public void HandleOriginShift(Point3 oldOrigin, Point3 newOrigin) {
		/// 	if (newOrigin == oldOrigin)
		/// 		return;
		/// 	Vector3 delta = (Vector3)(newOrigin - oldOrigin);
		/// 	transform.position = transform.position - delta;
		/// }
		/// </code>
		/// </example>
		/// <remarks>
		/// The two approaches produce almost identical results, but the
		/// delta-based approach can lead to subtle drifting. This is typically
		/// not a problem for dynamically moving objects like the player or NPCs.
		/// </remarks>
		/// <param name="oldOrigin">The old scene origin in world space.</param>
		/// <param name="newOrigin">The new scene origin in world space.</param>
		void HandleOriginShift(Point3 oldOrigin, Point3 newOrigin);
	}

	/// <summary>
	/// Class for handling floating origin in order to support large worlds
	/// without suffering floating point precision issues.
	/// </summary>
	/// <remarks>
	/// Individual behaviors that control objects' positions need to implement
	/// the <see cref="IOriginHandler"/> interface and register themselves to
	/// the FloatingOrigin, to be invoked when the origin is moved.
	/// </remarks>
	public static class FloatingOrigin {

		/// <summary>
		/// The current scene origin in world space.
		/// </summary>
		public static Point3 origin { get; private set; }

		static List<IOriginHandler> handlers = new List<IOriginHandler>();

		/// <summary>
		/// Implementors of the <see cref="IOriginHandler"/> interface must
		/// call this method to register themselves when they become enabled.
		/// </summary>
		public static void RegisterHandler(IOriginHandler handler) {
			handlers.Add(handler);
		}

		/// <summary>
		/// Implementors of the <see cref="IOriginHandler"/> interface must
		/// call this method to unregister themselves when they become disabled.
		/// </summary>
		public static void UnregisterHandler(IOriginHandler handler) {
			handlers.Remove(handler);
		}

		/// <summary>
		/// Set the origin and invoke the origin handlers with both old and new
		/// origin parameters being the new origin. This should have no effect on
		/// delta-based handlers, which are considered to already be positioned
		/// relative to the origin.
		/// </summary>
		public static void SetInitialOrigin(Point3 newOrigin) {
			origin = newOrigin;
			for (int i = handlers.Count - 1; i >= 0; i--) {
				handlers[i].HandleOriginShift(newOrigin, newOrigin);
			}
		}

		/// <summary>
		/// Set the origin and invoke the origin handlers.
		/// </summary>
		public static void SetOrigin(Point3 newOrigin) {
			if (newOrigin == origin)
				return;
			Point3 oldOrigin = origin;
			origin = newOrigin;
			for (int i = handlers.Count - 1; i >= 0; i--) {
				handlers[i].HandleOriginShift(oldOrigin, newOrigin);
			}
		}
	}

}
