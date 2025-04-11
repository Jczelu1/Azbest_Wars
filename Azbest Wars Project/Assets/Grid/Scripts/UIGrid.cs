using TMPro;
using System;
using UnityEngine;
using Unity.Mathematics;
using static UnityEditor.PlayerSettings;

public class UIGrid
{
    public int Height { get; private set; }
    public int Width { get; private set; }
    private float CellSize;
    private Vector2 GridPosition;
    private RectTransform UITransform;
    public UIGrid(int width, int height, RectTransform rectTransform)
    {
        Height = height;
        Width = width;
        UITransform = rectTransform;
        GridPosition = rectTransform.position/2;
        GridPosition.x -= rectTransform.rect.width/2;
        GridPosition.y -= rectTransform.rect.height/2;
        CellSize = rectTransform.rect.width/width;
        Debug.Log("asdfasfdasfdasf: " + CellSize);
    }


    public int2 GetXY(Vector2 localPosition)
    {
        int2 res = new int2();
        res.x = Mathf.FloorToInt((localPosition.x + (UITransform.rect.width / 2)) / CellSize);
        res.y = Mathf.FloorToInt((localPosition.y + (UITransform.rect.height / 2)) / CellSize);
        return res;
    }
    public Vector2 GetLocalPosition(int x, int y)
    {
        return new Vector2(x * CellSize - (UITransform.rect.width/2) + CellSize/2, y * CellSize - (UITransform.rect.height / 2) + CellSize / 2);

    }
    public Vector2 GetLocalPosition(int2 pos)
    {
        return new Vector2(pos.x * CellSize - (UITransform.rect.width / 2) + CellSize / 2, pos.y * CellSize - (UITransform.rect.height / 2) + CellSize / 2);
    }
    //public void ShowDebugLines()
    //{
    //    for (int x = 0; x < Width; x++)
    //    {
    //        Debug.DrawLine(GetWorldPosition(x, 0) - new Vector3(CellSize, CellSize) * .5f, GetWorldPosition(x, Height) - new Vector3(CellSize, CellSize) * .5f, Color.red, 100f);
    //    }
    //    for (int y = 0; y < Height; y++)
    //    {
    //        Debug.DrawLine(GetWorldPosition(0, y) - new Vector3(CellSize, CellSize) * .5f, GetWorldPosition(Width, y) - new Vector3(CellSize, CellSize) * .5f, Color.red, 100f);
    //    }
    //}
    public GameObject CreateSprite(GameObject selectPrefab, int2 position)
    {
        GameObject obj = GameObject.Instantiate(selectPrefab);
        obj.transform.SetParent(UITransform.transform);
        obj.GetComponent<RectTransform>().localPosition = GetLocalPosition(position);
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
