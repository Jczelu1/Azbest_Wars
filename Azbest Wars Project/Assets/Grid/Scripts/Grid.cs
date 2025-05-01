using TMPro;
using System;
using UnityEngine;
using Unity.Mathematics;

public class Grid<TGridObject>
{
    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs
    {
        public int x;
        public int y;
    }

    public int Height { get; private set; }
    public int Width { get; private set; }
    private float CellSize;
    private TGridObject[,] GridArray;
    private Vector3 GridPosition;
    public TextMeshPro[,] gridText;

    private bool debugText = false;
    public Grid(int width, int height, float cellSize, Vector3 gridOrigin)
    {
        Height = height;
        Width = width;
        CellSize = cellSize;
        GridPosition = gridOrigin;
        GridArray = new TGridObject[Width, Height];
    }
    
    public void SetValue(int2 pos, TGridObject value)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x > Width || pos.y > Height) return;
        GridArray[pos.x, pos.y] = value;
        if (OnGridValueChanged != null) OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = pos.x, y = pos.y });
        if (debugText)
            gridText[pos.x, pos.y].text = value.ToString();
    }
    public void SetValue(Vector3 worldPosition, TGridObject value)
    {
        int2 pos = GetXY(worldPosition);
        SetValue(pos, value);
    }
    
    public int2 GetXY(Vector3 worldPosition)
    {
        int2 res = new int2();
        res.x = Mathf.FloorToInt((worldPosition.x - GridPosition.x + CellSize * .5f) / CellSize);
        res.y = Mathf.FloorToInt((worldPosition.y - GridPosition.y + CellSize * .5f) / CellSize);
        if (res.x < 0 || res.y < 0 || res.x > Width || res.y > Height) return new int2(-1,-1);
        return res;
    }
    public TGridObject GetValue(int2 pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x > Width || pos.y > Height) return default(TGridObject); ;
        return GridArray[pos.x, pos.y];
    }
    public TGridObject GetValue(Vector3 worldPosition)
    {
        int2 pos = GetXY(worldPosition);
        return GetValue(pos);
    }
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * CellSize + GridPosition.x, y * CellSize + GridPosition.y);
    }
    public void ShowDebugLines()
    {
        for (int x = 0; x < GridArray.GetLength(0); x++)
        {
            Debug.DrawLine(GetWorldPosition(x, 0) - new Vector3(CellSize, CellSize) * .5f, GetWorldPosition(x, Height) - new Vector3(CellSize, CellSize) * .5f, Color.red, 100f);
        }
        for (int y = 0; y < GridArray.GetLength(1); y++)
        {
            Debug.DrawLine(GetWorldPosition(0, y) - new Vector3(CellSize, CellSize) * .5f, GetWorldPosition(Width, y) - new Vector3(CellSize, CellSize) * .5f, Color.red, 100f);
        }
    }
    public void ShowDebugtext()
    {
        debugText = true;
        gridText = new TextMeshPro[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                gridText[x, y] = Utils.CreateWorldText(GridArray[x, y].ToString(), null, GetWorldPosition(x, y));
            }
        }
    }
    public void SetDebugText(int x, int y, string text)
    {
        if (gridText[x, y] != null)
        {
            gridText[x, y].text = text;
        }
    }
}
