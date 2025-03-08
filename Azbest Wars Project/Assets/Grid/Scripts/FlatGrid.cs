using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct FlatGrid<T> where T : struct
{
    public int Width;
    public int Height;
    public NativeArray<T> GridArray;

    public FlatGrid(int width, int height, Allocator allocator)
    {
        Width = width;
        Height = height;
        GridArray = new NativeArray<T>(width*height, allocator, NativeArrayOptions.ClearMemory);
    }
    public int GetIndex(int2 pos)
    {
        return pos.x + (pos.y * Width);
    }
    public T GetValue(int2 pos)
    {
        return GridArray[GetIndex(pos)];
    }

    public void SetValue(int2 pos, T val)
    {
        GridArray[GetIndex(pos)] = val;
    }
    //unsafe public ref T GetRef(int index)
    //{
    //    void* ptr = NativeArrayUnsafeUtility.GetUnsafePtr(GridArray);
    //    return ref UnsafeUtility.ArrayElementAsRef<T>(ptr, index);
    //}
    public bool IsInGrid(int2 pos)
    {
        return
            pos.x >= 0 && pos.y >= 0 &&
            pos.x < Width && pos.y < Height;
    }
    public T this[int2 key]
    {
        get => GetValue(key);
        set => SetValue(key, value);
    }
    public T this[int key]
    {
        get => GridArray[key];
        set => GridArray[key] = value;
    }
}
