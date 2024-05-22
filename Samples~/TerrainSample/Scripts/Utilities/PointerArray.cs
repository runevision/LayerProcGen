using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

// Unity NativeArrays can't be used directly in Unity Burst methods (only in Burst jobs),
// so we have to pass pointers to arrays instead.
// 
// This PointerArray wrapper can be created from a NativeArray via simple implicit assignment:
// 
// NativeArray<float> floatNativeArray = new NativeArray<float>(1000, Allocator.Persistent);
// PointerArray<float> floatPointerArray = floatNativeArray;
// 
// The PointerArray can then be used like an array itself, including getting the Length.
[NoAlias]
public unsafe struct PointerArray<T> where T : unmanaged {
	[NoAlias]
	public readonly T* Array;
	public readonly int Length;

	public T this[uint index] {
		get { return Array[index]; }
		set { Array[index] = value; }
	}

	public T this[int index] {
		get { return Array[index]; }
		set { Array[index] = value; }
	}

	public PointerArray(T* pointer, int length) {
		Array = pointer;
		Length = length;
	}

	public PointerArray(T* pointer, T[] source) {
		Array = pointer;
		Length = source.Length;
	}

	public PointerArray(int length, out NativeArray<T> outputNativeArray) {
		Length = length;
		outputNativeArray = new NativeArray<T>(Length, Allocator.Persistent);
		Array = (T*)outputNativeArray.GetUnsafePtr();
	}

	public static implicit operator PointerArray<T>(NativeArray<T> a) {
		return new PointerArray<T>((T*)a.GetUnsafePtr(), a.Length);
	}

	public void Clear() {
		UnsafeUtility.MemClear(Array, Length * UnsafeUtility.SizeOf<T>());
	}
}

[NoAlias]
public unsafe struct PointerArray2D<T> where T : unmanaged {
	[NoAlias]
	public readonly T* Array;
	public readonly int Width;
	public readonly int Height;
	public readonly int Length; // Width * Height

	public T this[uint index] {
		get { return Array[index]; }
		set { Array[index] = value; }
	}

	public T this[int index] {
		get { return Array[index]; }
		set { Array[index] = value; }
	}

	public T this[uint y, uint x] {
		get { return Array[y * Width + x]; }
		set { Array[y * Width + x] = value; }
	}

	public T this[int y, int x] {
		get { return Array[y * Width + x]; }
		set { Array[y * Width + x] = value; }
	}

	public PointerArray2D(T* pointer, int height, int width) {
		Array = pointer;
		Height = height;
		Width = width;
		Length = Width * Height;
	}

	public PointerArray2D(T* pointer, T[,] source) {
		Array = pointer;
		Height = source.GetLength(0);
		Width = source.GetLength(1);
		Length = Width * Height;
	}

	public PointerArray2D(int height, int width, out NativeArray<T> outputNativeArray) {
		Height = height;
		Width = width;
		Length = Width * Height;
		outputNativeArray = new NativeArray<T>(Length, Allocator.Persistent);
		Array = (T*)outputNativeArray.GetUnsafePtr();
	}

	public void Clear() {
		UnsafeUtility.MemClear(Array, Length * UnsafeUtility.SizeOf<T>());
	}
}

[NoAlias]
public unsafe struct PointerArray3D<T> where T : unmanaged {
	[NoAlias]
	public readonly T* Array;
	public readonly int Width;
	public readonly int Height;
	public readonly int Depth;
	public readonly int Length; // Width * Height * Depth

	public T this[uint index] {
		get { return Array[index]; }
		set { Array[index] = value; }
	}

	public T this[int index] {
		get { return Array[index]; }
		set { Array[index] = value; }
	}

	public T this[uint z, uint y, uint x] {
		get { return Array[z * Width * Height + y * Width + x]; }
		set { Array[z * Width * Height + y * Width + x] = value; }
	}

	public T this[int z, int y, int x] {
		get { return Array[z * Width * Height + y * Width + x]; }
		set { Array[z * Width * Height + y * Width + x] = value; }
	}

	public PointerArray3D(T* pointer, int depth, int height, int width) {
		Array = pointer;
		Width = width;
		Height = height;
		Depth = depth;
		Length = Width * Height * Depth;
	}

	public PointerArray3D(T* pointer, T[,,] source) {
		Array = pointer;
		Depth = source.GetLength(0);
		Height = source.GetLength(1);
		Width = source.GetLength(2);
		Length = Width * Height * Depth;
	}

	public PointerArray3D(int depth, int height, int width, out NativeArray<T> outputNativeArray) {
		Depth = depth;
		Height = height;
		Width = width;
		Length = Width * Height * Depth;
		outputNativeArray = new NativeArray<T>(Length, Allocator.Persistent);
		Array = (T*)outputNativeArray.GetUnsafePtr();
	}

	public void Clear() {
		UnsafeUtility.MemClear(Array, Length * UnsafeUtility.SizeOf<T>());
	}
}
