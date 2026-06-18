using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

[Serializable]
public struct Splat : IEquatable<Splat> {
	public float a;
	public float b;
	public float c;
	public float d;

	public static readonly Splat zero;

	public Splat(float a, float b, float c, float d) {
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
	}

	public unsafe float this[int index] {
		get {
			if ((uint)index >= 5u) {
				throw new ArgumentException("index must be between[0...3]");
			}

			fixed (Splat* ptr = &this) {
				return *(float*)((byte*)ptr + index * 4);
			}
		}
		set {
			if ((uint)index >= 5u) {
				throw new ArgumentException("index must be between[0...4]");
			}

			fixed (float* ptr = &a) {
				ptr[index] = value;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Splat operator *(Splat lhs, Splat rhs) {
		return new Splat(lhs.a * rhs.a, lhs.b * rhs.b, lhs.c * rhs.c, lhs.d * rhs.d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Splat operator *(Splat lhs, float rhs) {
		return new Splat(lhs.a * rhs, lhs.b * rhs, lhs.c * rhs, lhs.d * rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Splat operator *(float lhs, Splat rhs) {
		return new Splat(lhs * rhs.a, lhs * rhs.b, lhs * rhs.c, lhs * rhs.d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Splat operator /(Splat lhs, float rhs) {
		return new Splat(lhs.a / rhs, lhs.b / rhs, lhs.c / rhs, lhs.d / rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Splat operator +(Splat lhs, Splat rhs) {
		return new Splat(lhs.a + rhs.a, lhs.b + rhs.b, lhs.c + rhs.c, lhs.d + rhs.d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Splat operator -(Splat lhs, Splat rhs) {
		return new Splat(lhs.a - rhs.a, lhs.b - rhs.b, lhs.c - rhs.c, lhs.d - rhs.d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Splat operator -(Splat val) {
		return new Splat(0f - val.a, 0f - val.b, 0f - val.c, 0f - val.d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Splat operator +(Splat val) {
		return new Splat(val.a, val.b, val.c, val.d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Splat rhs) {
		if (a == rhs.a && b == rhs.b && c == rhs.c) {
			return d == rhs.d;
		}

		return false;
	}

	public override bool Equals(object o) {
		if (o is Splat rhs) {
			return Equals(rhs);
		}

		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Splat Lerp(Splat x, Splat y, float s) {
        return x + s * (y - x);
    }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() {
		// TODO: Fix to include e in hash.
		return (int)math.hash(new float4(a, b, c, d));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString() {
		return $"Splat({a}f, {b}f, {c}f, {d}f)";
	}
}
