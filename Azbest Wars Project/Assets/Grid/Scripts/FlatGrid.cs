using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct FlatGrid<T> where T : struct
{
    public int Width;
    public int Height;
    public NativeArray<T> GridArray;

    public FlatGrid(int width, int height)
    {
        Width = width;
        Height = height;
        GridArray = new NativeArray<T>(width*height, Allocator.Temp);
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
}
