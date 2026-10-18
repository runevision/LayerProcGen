/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace Runevision.Common {

	[Serializable]
	public struct DFloat : IEquatable<DFloat> {
		public const int Res = 256;
		public const int HalfRes = 128;
		public const float DivFloat = 1f / Res;
		public const double DivDouble = 1d / Res;

		public int value;

		public DFloat(int value) {
			#if DEBUG
			if (value > 8388607 || value < -8388607)
				throw new OverflowException($"Int {value} is too large to convert to DFloat. The allowed range is +/- 8388606.");
			#endif
			this.value = value * Res;
		}

		public DFloat(float value) {
			#if DEBUG
			if (value > 8388607 || value < -8388607)
				throw new OverflowException($"Float {value} is too large to convert to DFloat. The allowed range is +/- 8388606.");
			#endif
			this.value = (int)Math.Floor(value * Res);
		}

		public DFloat(double value) {
			#if DEBUG
			if (value > 8388607 || value < -8388607)
				throw new OverflowException($"Double {value} is too large to convert to DFloat. The allowed range is +/- 8388606.");
			#endif
			this.value = (int)Math.Floor(value * Res);
		}

		public static explicit operator float(DFloat f) {
			return f.value * DivFloat;
		}

		public static implicit operator DFloat(float f) {
			return new DFloat(f);
		}

		public static implicit operator double(DFloat f) {
			return f.value * DivDouble;
		}

		public static implicit operator DFloat(double f) {
			return new DFloat(f);
		}

		// Private use. Identical method in Crd class can't be used due to
		// assembly definition dependency directions.
		static int Div(int x, int divisor) {
			return (x - ((divisor - 1) & (x >> 31))) / divisor;
		}

		// Private use. Identical method in Crd class can't be used due to
		// assembly definition dependency directions.
		static int DivUp(int x, int divisor) {
			return (x + ((divisor - 1) & ~(x >> 31))) / divisor;
		}

		// Private use.
		static long Div(long x, long divisor) {
			return (x - ((divisor - 1) & (x >> 63))) / divisor;
		}

		public static explicit operator int(DFloat f) {
			return Div(f.value, Res);
		}

		public int FloorToInt() {
			return Div(value, Res);
		}

		public int CeilToInt() {
			return DivUp(value, Res);
		}

		public int RoundToInt() {
			return Div(value + HalfRes, Res);
		}

		public static implicit operator DFloat(int f) {
			return new DFloat(f);
		}

		public static DFloat operator +(DFloat a, DFloat b) {
			return new DFloat { value = a.value + b.value };
		}

		public static DFloat operator -(DFloat a, DFloat b) {
			return new DFloat { value = a.value - b.value };
		}

		public static DFloat operator -(DFloat a) {
			return new DFloat { value = -a.value };
		}

		public static bool operator <(DFloat a, DFloat b) {
			return a.value < b.value;
		}

		public static bool operator >(DFloat a, DFloat b) {
			return a.value > b.value;
		}

		public static bool operator <=(DFloat a, DFloat b) {
			return a.value <= b.value;
		}

		public static bool operator >=(DFloat a, DFloat b) {
			return a.value >= b.value;
		}

		public static DFloat operator *(DFloat a, int f) {
			return new DFloat { value = a.value * f };
		}

		public static DFloat operator /(DFloat a, int f) {
			return new DFloat { value = Div(a.value, f) };
		}

		public static DPoint operator *(Point a, DFloat f) {
			return new DPoint(f * a.x, f * a.y);
		}

		public static DFloat operator *(DFloat a, DFloat f) {
			// Multiply with more bits, truncate afterwards.
			return new DFloat { value = (int)((long)(a.value) * (long)(f.value) / Res) };
		}

		public static DFloat operator /(DFloat a, DFloat f) {
			// Multiply with more bits, truncate afterwards.
			return new DFloat { value = (int)Div((long)(a.value) * Res, (long)f.value) };
		}

		public static bool operator ==(DFloat a, DFloat b) {
			return (a.value == b.value);
		}

		public static bool operator !=(DFloat a, DFloat b) {
			return (a.value != b.value);
		}

		public override bool Equals(object obj) {
			if (!(obj is DFloat other))
				return false;
			return (value == other.value);
		}

		public bool Equals(DFloat other) {
			return (value == other.value);
		}

		public override string ToString() {
			return (value * DivDouble).ToString("0.000");
		}

		public override int GetHashCode() {
			return value;
		}

		public static DFloat Abs(DFloat a) {
			return new DFloat { value = Math.Abs(a.value) };
		}

		public static DFloat Min(DFloat a, DFloat b) {
			return new DFloat { value = Math.Min(a.value, b.value) };
		}

		public static DFloat Max(DFloat a, DFloat b) {
			return new DFloat { value = Math.Max(a.value, b.value) };
		}

		public static DFloat Average(DFloat a, DFloat b) {
			return a + (b - a) / 2;
		}

		public static DFloat Clamp(DFloat val, DFloat min, DFloat max) {
			return new DFloat { value = Math.Min(Math.Max(val.value, min.value), max.value) };
		}

		public static DFloat Lerp(DFloat a, DFloat b, DFloat lerp) {
			return new DFloat { value = a.value + (((b.value - a.value) * lerp.value) / Res) };
		}
	}

}
