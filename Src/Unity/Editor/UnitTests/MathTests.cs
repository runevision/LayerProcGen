/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using NUnit.Framework;
using Runevision.Common;
using UnityEngine;

namespace Runevision.Testing {

	public class MathTests {

		// DFloat

		[Test]
		public void DFloatEqualsInt() {
			Assert.IsTrue(new DFloat(5) == 5);
			Assert.IsTrue(new DFloat(5) != 4);
		}

		[Test]
		public void DFloatEqualsFloat() {
			Assert.IsTrue(new DFloat(5.5f) == 5.5f);
			Assert.IsTrue(new DFloat(5.5f) != 5.4f);
		}

		[Test]
		public void DFloatMuliplyInt() {
			Assert.AreEqual(new DFloat(5) * 2, new DFloat(10));
			Assert.AreEqual(new DFloat(0.5f) * 6, new DFloat(3));
		}

		[Test]
		public void DFloatDivideInt() {
			Assert.AreEqual(new DFloat(5) / 2, new DFloat(2.5f));
			Assert.AreEqual(new DFloat(10) / 5, new DFloat(2));
		}

		[Test]
		public void DFloatLerp() {
			Assert.AreEqual(DFloat.Lerp(2, 10, 0.25f), new DFloat(4));
		}

		// DPoint

		[Test]
		public void DPointEqualsVector2() {
			Assert.IsTrue(new DPoint(5, 4.5f) == new Vector2(5, 4.5f));
			Assert.IsTrue(new DPoint(5, 4.5f) != new Vector2(5, 3));
		}

		[Test]
		public void DPointEqualsPoint() {
			Assert.IsTrue(new DPoint(5, 4) == new Point(5, 4));
			Assert.IsTrue(new DPoint(5, 4) != new Point(5, 3));
		}

		[Test]
		public void DPointMagnitude() {
			Assert.IsTrue(new DPoint(3, 4).magnitude == 5);
			Assert.IsTrue(new DPoint(3, 4).magnitude != 6);
		}
	}

}

