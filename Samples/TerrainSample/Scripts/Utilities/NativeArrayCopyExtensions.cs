using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static class NativeArrayCopyExtensions {

	[WriteAccessRequired]
	public static unsafe void CopyFrom<natT, arrT>(this NativeArray<natT> dst, arrT[] src)
		where natT : struct
		where arrT : struct
	{
		SanityCheck<natT, arrT>(dst, src, src.Length);
		GCHandle gCHandle = GCHandle.Alloc(src, GCHandleType.Pinned);
		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
		UnsafeUtility.MemCpy(
			(byte*)dst.GetUnsafePtr(),
			(byte*)(void*)intPtr,
			dst.Length * UnsafeUtility.SizeOf<natT>());
		gCHandle.Free();
	}

	[WriteAccessRequired]
	public static unsafe void CopyFrom<natT, arrT>(this NativeArray<natT> dst, arrT[,] src)
		where natT : struct
		where arrT : struct
	{
		SanityCheck<natT, arrT>(dst, src, src.Length);
		GCHandle gCHandle = GCHandle.Alloc(src, GCHandleType.Pinned);
		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
		UnsafeUtility.MemCpy(
			(byte*)dst.GetUnsafePtr(),
			(byte*)(void*)intPtr,
			dst.Length * UnsafeUtility.SizeOf<natT>());
		gCHandle.Free();
	}

	[WriteAccessRequired]
	public static unsafe void CopyTo<natT, arrT>(this NativeArray<natT> src, arrT[] dst)
		where natT : struct
		where arrT : struct
	{
		SanityCheck<natT, arrT>(src, dst, dst.Length);
		GCHandle gCHandle = GCHandle.Alloc(dst, GCHandleType.Pinned);
		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
		UnsafeUtility.MemCpy(
			(byte*)(void*)intPtr,
			(byte*)src.GetUnsafePtr(),
			src.Length * UnsafeUtility.SizeOf<natT>());
		gCHandle.Free();
	}

	[WriteAccessRequired]
	public static unsafe void CopyTo<natT, arrT>(this NativeArray<natT> src, arrT[,] dst)
		where natT : struct
		where arrT : struct
	{
		SanityCheck<natT, arrT>(src, dst, dst.Length);
		GCHandle gCHandle = GCHandle.Alloc(dst, GCHandleType.Pinned);
		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
		UnsafeUtility.MemCpy(
			(byte*)(void*)intPtr,
			(byte*)src.GetUnsafePtr(),
			src.Length * UnsafeUtility.SizeOf<natT>());
		gCHandle.Free();
	}

	// Common
	static void SanityCheck<natT, arrT>(NativeArray<natT> nat, object array, int arrayLength)
		where natT : struct
		where arrT : struct
	{
		if (array == null) {
			throw new ArgumentNullException(nameof(array));
		}

		if (arrayLength < 0) {
			throw new ArgumentOutOfRangeException(nameof(arrayLength), "length must be equal or greater than zero.");
		}

		if (arrayLength != nat.Length) {
			throw new Exception("Array length and NativeArray length do not match.");
		}

		if (UnsafeUtility.SizeOf<arrT>() != UnsafeUtility.SizeOf<natT>()) {
			throw new Exception("Element types of array and NativeArray are not of same size.");
		}
	}
}
