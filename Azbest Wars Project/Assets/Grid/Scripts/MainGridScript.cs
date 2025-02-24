using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MainGridScript : MonoBehaviour
{
    public static Grid<MainGridCell> MainGrid;
    public int Width;
    public int Height;
    public float CellSize;
    public Vector2 GridOrigin;
    public FlatGrid<bool> IsWalkable;
    public static int2 ClickPosition;
    public static bool Clicked = false;

    [SerializeField]
    Tilemap Walls;

    public static Pathfinder MainPathfinder;

    private void Start()
    {
        MainGrid = new Grid<MainGridCell>(Width, Height, CellSize, GridOrigin);
        InitGrid();
        MainPathfinder = new Pathfinder();
        BoundsInt gridBounds = new BoundsInt();
        gridBounds.xMin = Mathf.FloorToInt(GridOrigin.x);
        gridBounds.yMin = Mathf.FloorToInt(GridOrigin.y);
        gridBounds.xMax = gridBounds.xMin + Width;
        gridBounds.yMax = gridBounds.yMin + Height;
        IsWalkable = GetTilesOnTilemap(gridBounds);
        MainGrid.ShowDebugtext();
        MainGrid.ShowDebugLines();
    }
    private void Update()
    {
        Clicked = false;
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Utils.GetMouseWorldPosition();
            int2 endPos = MainGrid.GetXY(mousePos);
            //Debug.Log(x + " " + y1);
            if (endPos.x==-1) return;
            Clicked = true;
        }
    }
    private void InitGrid()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                MainGrid.SetValue(new int2(x,y), new MainGridCell());
            }
        }
    }
    //public static NativeList<int2> FindPath(int2 start, int2 end)
    //{
    //    MainPathfinder.FindPath(start, end, new int2(Width, Height);
    //}
    private FlatGrid<bool> GetTilesOnTilemap(BoundsInt bounds)
    {
        //tilemap.CompressBounds();
        //bounds = tilemap.cellBounds;
        FlatGrid<bool> spots = new FlatGrid<bool>(bounds.size.x, bounds.size.y);
        Debug.Log("Bounds:" + bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                int2 tilePos = new int2(bounds.xMin + x, bounds.yMin + y);

                if (Walls.HasTile(new Vector3Int(tilePos.x, tilePos.y, 0)))
                {
                    spots.SetValue(tilePos, false);
                    MainGrid.GetValue(tilePos).IsWalkable = false;
                }
                else
                {
                    spots.SetValue(tilePos, true);
                    MainGrid.GetValue(tilePos).IsWalkable = true;
                }
            }
        }
        return spots;
    }
}
