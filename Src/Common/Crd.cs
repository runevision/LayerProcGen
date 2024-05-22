/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace Runevision.Common {

	// These are methods useful for processing integer coordinates.
	// Unlike built-in '/' and '%' operators, the methods here work the same for
	// the entire number range, no matter if the input is positive or negative.
	public static class Crd {

		// Div is like the division operator '/' except it rounds down instead of towards zero,
		// meaning the remainder is always zero or positive, never negative.
		//
		// Example with a positive dividend (division operator versus Div):
		// 9 / 4 = 2 (remainder 1)
		// Div(9, 4) = 2 (remainder 1)
		//
		// Example with a negative dividend (division operator versus Div):
		// -9 / 4 = -2 (remainder -1)
		// Div(-9, 4) = -3 (remainder 3)
		public static int Div(int x, int divisor) {
			return (x - (((x % divisor) + divisor) % divisor)) / divisor;
		}

		public static Point Div(Point p, int divisor) {
			return new Point(Div(p.x, divisor), Div(p.y, divisor));
		}

		public static Point Div(Point p, Point divisor) {
			return new Point(Div(p.x, divisor.x), Div(p.y, divisor.y));
		}

		// DivUp is like the division operator '/' except it rounds up instead of towards zero,
		// meaning the remainder is always zero or negative, never positive.
		//
		// Example with a positive dividend (division operator versus Div):
		// 9 / 4 = 2 (remainder 1)
		// DivUp(9, 4) = 3 (remainder -3)
		//
		// Example with a negative dividend (division operator versus Div):
		// -9 / 4 = -2 (remainder -1)
		// DivUp(-9, 4) = -2 (remainder -1)
		public static int DivUp(int x, int divisor) {
			return (x - (((x % divisor) - divisor) % divisor)) / divisor;
		}

		public static Point DivUp(Point p, int divisor) {
			return new Point(DivUp(p.x, divisor), DivUp(p.y, divisor));
		}

		public static Point DivUp(Point p, Point divisor) {
			return new Point(DivUp(p.x, divisor.x), DivUp(p.y, divisor.y));
		}

		// Mod (modulo) is like the remainder operator '%' except it's always zero or positive,
		// due to being the remainder of the Div method, which rounds down instead of towards zero.
		//
		// Example with a positive input (remainder operator versus Mod):
		// 9 % 4 = 1
		// Mod(9, 4) = 1
		//
		// Example with a negative input (remainder operator versus Mod):
		// -9 % 4 = -1
		// Mod(-9, 4) = 3
		public static int Mod(int x, int period) {
			return ((x % period) + period) % period;
		}

		public static Point Mod(Point p, int period) {
			return new Point(Mod(p.x, period), Mod(p.y, period));
		}

		public static Point Mod(Point p, Point period) {
			return new Point(Mod(p.x, period.x), Mod(p.y, period.y));
		}

		// RoundToPeriod rounds towards the nearest multiple of period; up if half-way in between.
		//
		// Example with a positive input:
		// RoundToPeriod(9, 4) = 8
		//
		// Example with a negative input:
		// RoundToPeriod(-9, 4) = -8
		public static int RoundToPeriod(int x, int period) {
			x += period / 2;
			return x - (((x % period) + period) % period);
		}

		public static Point RoundToPeriod(Point p, int period) {
			return new Point (RoundToPeriod(p.x, period), RoundToPeriod(p.y, period));
		}

		public static Point RoundToPeriod(Point p, Point period) {
			return new Point (RoundToPeriod(p.x, period.x), RoundToPeriod(p.y, period.y));
		}
	}

}
