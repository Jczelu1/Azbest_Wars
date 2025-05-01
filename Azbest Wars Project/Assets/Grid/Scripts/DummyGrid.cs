using UnityEngine;
using Unity.Mathematics;

public class DummyGrid
{
    public int Height { get; private set; }
    public int Width { get; private set; }
    private float CellSize;
    private Vector3 GridPosition;
    public DummyGrid(int width, int height, float cellSize, Vector3 gridOrigin)
    {
        Height = height;
        Width = width;
        CellSize = cellSize;
        GridPosition = gridOrigin;
    }


    public int2 GetXY(Vector3 worldPosition)
    {
        int2 res = new int2();
        res.x = Mathf.FloorToInt((worldPosition.x - GridPosition.x + CellSize * .5f) / CellSize);
        res.y = Mathf.FloorToInt((worldPosition.y - GridPosition.y + CellSize * .5f) / CellSize);
        return res;
    }
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * CellSize + GridPosition.x, y * CellSize + GridPosition.y);
    }
    public Vector3 GetWorldPosition(int2 pos)
    {
        return new Vector3(pos.x * CellSize + GridPosition.x, pos.y * CellSize + GridPosition.y);
    }
    public void ShowDebugLines()
    {
        for (int x = 0; x < Width; x++)
        {
            Debug.DrawLine(GetWorldPosition(x, 0) - new Vector3(CellSize, CellSize) * .5f, GetWorldPosition(x, Height) - new Vector3(CellSize, CellSize) * .5f, Color.red, 100f);
        }
        for (int y = 0; y < Height; y++)
        {
            Debug.DrawLine(GetWorldPosition(0, y) - new Vector3(CellSize, CellSize) * .5f, GetWorldPosition(Width, y) - new Vector3(CellSize, CellSize) * .5f, Color.red, 100f);
        }
    }
    public GameObject CreateSprite(GameObject selectPrefab, int2 position)
    {
        GameObject obj = GameObject.Instantiate(selectPrefab);
        obj.transform.position = GetWorldPosition(position);
        return obj;
    }
    //public void ShowDebugtext()
    //{
    //    debugText = true;
    //    gridText = new TextMeshPro[Width, Height];
    //    for (int x = 0; x < Width; x++)
    //    {
    //        for (int y = 0; y < Height; y++)
    //        {
    //            gridText[x, y] = Utils.CreateWorldText(GridArray[x, y].ToString(), null, GetWorldPosition(x, y));
    //        }
    //    }
    //}
    //public void SetDebugText(int x, int y, string text)
    //{
    //    if (gridText[x, y] != null)
    //    {
    //        gridText[x, y].text = text;
    //    }
    //}
}
