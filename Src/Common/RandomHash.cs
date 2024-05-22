/*
 * C# implementation of xxHash optimized for producing random numbers
 * from one or more input integers.
 * Copyright (C) 2024 Rune Skovbo Johansen.
 *
 * Based on:
 * xxHashSharp - A pure C# implementation of xxhash
 * Copyright (C) 2014 Seok-Ju, Yun. (https://github.com/noricube/xxHashSharp)
 *
 * Based on:
 * xxHash - Extremely Fast Hash algorithm
 * Copyright (C) 2012-2014 Yann Collet (https://github.com/Cyan4973/xxHash)
 *
 * BSD 2-Clause License (https://www.opensource.org/licenses/bsd-license.php)
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 *       copyright notice, this list of conditions and the following
 *       disclaimer in the documentation and/or other materials provided
 *       with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;

namespace Runevision.Common {

	public struct RandomHash {
		public uint seed;

		const uint Prime1 = 2654435761U;
		const uint Prime2 = 2246822519U;
		const uint Prime3 = 3266489917U;
		const uint Prime4 = 668265263U;
		const uint Prime5 = 374761393U;

		public RandomHash(int seed) {
			this.seed = (uint)seed;
		}
		public RandomHash(uint seed) {
			this.seed = seed;
		}

		public uint GetHash(int a) { return GetHash((uint)a); }
		public uint GetHash(uint a) {
			uint h32 = seed + Prime5;

			h32 += 4u;

			h32 += a * Prime3;
			h32 = RotateLeft(h32, 17) * Prime4;

			h32 ^= h32 >> 15;
			h32 *= Prime2;
			h32 ^= h32 >> 13;
			h32 *= Prime3;
			h32 ^= h32 >> 16;

			return h32;
		}

		public uint GetHash(int a, int b) { return GetHash((uint)a, (uint)b); }
		public uint GetHash(uint a, uint b) {
			uint h32 = seed + Prime5;

			h32 += 8u;

			h32 += a * Prime3;
			h32 = RotateLeft(h32, 17) * Prime4;
			h32 += b * Prime3;
			h32 = RotateLeft(h32, 17) * Prime4;

			h32 ^= h32 >> 15;
			h32 *= Prime2;
			h32 ^= h32 >> 13;
			h32 *= Prime3;
			h32 ^= h32 >> 16;

			return h32;
		}

		public uint GetHash(int a, int b, int c) { return GetHash((uint)a, (uint)b, (uint)c); }
		public uint GetHash(uint a, uint b, uint c) {
			uint h32 = seed + Prime5;

			h32 += 12u;

			h32 += a * Prime3;
			h32 = RotateLeft(h32, 17) * Prime4;
			h32 += b * Prime3;
			h32 = RotateLeft(h32, 17) * Prime4;
			h32 += c * Prime3;
			h32 = RotateLeft(h32, 17) * Prime4;

			h32 ^= h32 >> 15;
			h32 *= Prime2;
			h32 ^= h32 >> 13;
			h32 *= Prime3;
			h32 ^= h32 >> 16;

			return h32;
		}

		public uint GetHash(int a, int b, int c, int d) { return GetHash((uint)a, (uint)b, (uint)c, (uint)d); }
		public uint GetHash(uint a, uint b, uint c, uint d) {
			uint v1 = seed + Prime1 + Prime2;
			uint v2 = seed + Prime2;
			uint v3 = seed + 0u;
			uint v4 = seed - Prime1;

			v1 = CalcSubHash(v1, a);
			v2 = CalcSubHash(v2, b);
			v3 = CalcSubHash(v3, c);
			v4 = CalcSubHash(v4, d);

			uint h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);

			h32 += 16u;

			h32 ^= h32 >> 15;
			h32 *= Prime2;
			h32 ^= h32 >> 13;
			h32 *= Prime3;
			h32 ^= h32 >> 16;

			return h32;
		}

		public uint GetHash(params uint[] buf) {
			uint h32;
			int index = 0;
			int len = buf.Length;

			if (len >= 4) {
				int limit = len - 4;
				uint v1 = seed + Prime1 + Prime2;
				uint v2 = seed + Prime2;
				uint v3 = seed + 0;
				uint v4 = seed - Prime1;

				do {
					v1 = CalcSubHash(v1, buf[index]);
					index++;
					v2 = CalcSubHash(v2, buf[index]);
					index++;
					v3 = CalcSubHash(v3, buf[index]);
					index++;
					v4 = CalcSubHash(v4, buf[index]);
					index++;
				} while (index <= limit);

				h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
			}
			else {
				h32 = seed + Prime5;
			}

			h32 += (uint)len * 4;

			while (index < len) {
				h32 += buf[index] * Prime3;
				h32 = RotateLeft(h32, 17) * Prime4;
				index++;
			}

			h32 ^= h32 >> 15;
			h32 *= Prime2;
			h32 ^= h32 >> 13;
			h32 *= Prime3;
			h32 ^= h32 >> 16;

			return h32;
		}

		public uint GetHash(params int[] buf) {
			uint h32;
			int index = 0;
			int len = buf.Length;

			if (len >= 4) {
				int limit = len - 4;
				uint v2 = seed + Prime2;
				uint v1 = seed + Prime1 + Prime2;
				uint v3 = seed + 0;
				uint v4 = seed - Prime1;

				do {
					v1 = CalcSubHash(v1, (uint)buf[index]);
					index++;
					v2 = CalcSubHash(v2, (uint)buf[index]);
					index++;
					v3 = CalcSubHash(v3, (uint)buf[index]);
					index++;
					v4 = CalcSubHash(v4, (uint)buf[index]);
					index++;
				} while (index <= limit);

				h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
			}
			else {
				h32 = seed + Prime5;
			}

			h32 += (uint)len * 4;

			while (index < len) {
				h32 += (uint)buf[index] * Prime3;
				h32 = RotateLeft(h32, 17) * Prime4;
				index++;
			}

			h32 ^= h32 >> 15;
			h32 *= Prime2;
			h32 ^= h32 >> 13;
			h32 *= Prime3;
			h32 ^= h32 >> 16;

			return h32;
		}

		public uint GetHash(byte[] buf) {
			uint h32;
			int index = 0;
			int len = buf.Length;

			if (len >= 16) {
				int limit = len - 16;
				uint v1 = seed + Prime1 + Prime2;
				uint v2 = seed + Prime2;
				uint v3 = seed + 0;
				uint v4 = seed - Prime1;

				do {
					v1 = CalcSubHash(v1, buf, index);
					index += 4;
					v2 = CalcSubHash(v2, buf, index);
					index += 4;
					v3 = CalcSubHash(v3, buf, index);
					index += 4;
					v4 = CalcSubHash(v4, buf, index);
					index += 4;
				} while (index <= limit);

				h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
			}
			else {
				h32 = seed + Prime5;
			}

			h32 += (uint)len;

			while (index <= len - 4) {
				h32 += BitConverter.ToUInt32(buf, index) * Prime3;
				h32 = RotateLeft(h32, 17) * Prime4;
				index += 4;
			}

			while (index < len) {
				h32 += buf[index] * Prime5;
				h32 = RotateLeft(h32, 11) * Prime1;
				index++;
			}

			h32 ^= h32 >> 15;
			h32 *= Prime2;
			h32 ^= h32 >> 13;
			h32 *= Prime3;
			h32 ^= h32 >> 16;

			return h32;
		}

		static uint CalcSubHash(uint value, byte[] buf, int index) {
			uint readValue = BitConverter.ToUInt32(buf, index);
			value += readValue * Prime2;
			value = RotateLeft(value, 13);
			value *= Prime1;
			return value;
		}

		static uint CalcSubHash(uint value, uint readValue) {
			value += readValue * Prime2;
			value = RotateLeft(value, 13);
			value *= Prime1;
			return value;
		}

		static uint RotateLeft(uint value, int count) {
			return (value << count) | (value >> (32 - count));
		}

		public int GetInt(params int[] data) { return (int)GetHash(data); }
		public int GetInt(int data) { return (int)GetHash(data); }
		public int GetInt(int x, int y) { return (int)GetHash(x, y); }
		public int GetInt(int x, int y, int z) { return (int)GetHash(x, y, z); }

		public float Value(params int[] data) {
			return GetHash(data) / (float)uint.MaxValue;
		}
		// Potentially optimized overloads for few parameters.
		public float Value(int data) {
			return GetHash(data) / (float)uint.MaxValue;
		}
		public float Value(int x, int y) {
			return GetHash(x, y) / (float)uint.MaxValue;
		}
		public float Value(int x, int y, int z) {
			return GetHash(x, y, z) / (float)uint.MaxValue;
		}

		public int Range(int min, int max, params int[] data) {
			return min + (int)(GetHash(data) % (max - min));
		}
		// Potentially optimized overloads for few parameters.
		public int Range(int min, int max, int data) {
			return min + (int)(GetHash(data) % (max - min));
		}
		public int Range(int min, int max, int x, int y) {
			return min + (int)(GetHash(x, y) % (max - min));
		}
		public int Range(int min, int max, int x, int y, int z) {
			return min + (int)(GetHash(x, y, z) % (max - min));
		}

		public float Range(float min, float max, params int[] data) {
			return min + (GetHash(data) * (max - min)) / uint.MaxValue;
		}
		// Potentially optimized overloads for few parameters.
		public float Range(float min, float max, int data) {
			return min + (GetHash(data) * (max - min)) / uint.MaxValue;
		}
		public float Range(float min, float max, int x, int y) {
			return min + (GetHash(x, y) * (max - min)) / uint.MaxValue;
		}
		public float Range(float min, float max, int x, int y, int z) {
			return min + (GetHash(x, y, z) * (max - min)) / uint.MaxValue;
		}
	}

}
