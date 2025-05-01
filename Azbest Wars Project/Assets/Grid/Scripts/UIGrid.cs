using UnityEngine;
using Unity.Mathematics;

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
    public GameObject CreateSprite(GameObject selectPrefab, int2 position)
    {
        GameObject obj = GameObject.Instantiate(selectPrefab);
        obj.transform.SetParent(UITransform.transform);
        obj.GetComponent<RectTransform>().localPosition = GetLocalPosition(position);
        return obj;
    }
}
