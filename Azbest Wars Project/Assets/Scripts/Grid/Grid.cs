using TMPro;
using System;
using UnityEngine;
using UnityEngine.Diagnostics;
using static UnityEngine.Rendering.DebugUI;

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
    private TGridObject[,] gridArray;
    private Vector3 GridPosition;
    public TextMeshPro[,] gridText;

    private bool debugText = false;
    public Grid(int width, int height, float cellSize, Vector3 gridOrigin, bool debugLines = false, bool debugNumbers = false)
    {
        Height = height;
        Width = width;
        CellSize = cellSize;
        GridPosition = gridOrigin;
        gridArray = new TGridObject[Width, Height];
    }
    public void ShowDebugLines()
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            Debug.DrawLine(GetWorldPosition(x, 0) - new Vector3(CellSize, CellSize) * .5f, GetWorldPosition(x, Height) - new Vector3(CellSize, CellSize) * .5f, Color.red, 100f);
        }
        for (int y = 0; y < gridArray.GetLength(1); y++)
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
                gridText[x, y] = Utils.CreateWorldText(gridArray[x, y].ToString(), null, GetWorldPosition(x, y));
            }
        }
    }
    public void SetValue(int x, int y, TGridObject value)
    {
        if (x < 0 || y < 0 || x > Width || y > Height) return;
        gridArray[x, y] = value;
        if (OnGridValueChanged != null) OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
        if (debugText)
            gridText[x, y].text = value.ToString();
    }
    public void SetValue(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetValue(x, y, value);
    }
    public void SetDebugText(int x, int y, string text)
    {
        if (gridText[x, y] != null)
        {
            gridText[x, y].text = text;
        }
    }
    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition.x - GridPosition.x + CellSize * .5f) / CellSize);
        y = Mathf.FloorToInt((worldPosition.y - GridPosition.y + CellSize * .5f) / CellSize);
    }
    public TGridObject GetValue(int x, int y)
    {
        if (x < 0 || y < 0 || x > Width || y > Height) return default(TGridObject); ;
        return gridArray[x, y];
    }
    public TGridObject GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetValue(x, y);
    }
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * CellSize + GridPosition.x, y * CellSize + GridPosition.y);
    }

}
