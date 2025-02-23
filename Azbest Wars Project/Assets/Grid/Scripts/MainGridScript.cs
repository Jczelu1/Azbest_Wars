using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MainGridScript : MonoBehaviour
{
    public Grid<MainGridCell> MainGrid;
    public int Width;
    public int Height;
    public float CellSize;
    public Vector2 GridOrigin;
    public FlatGrid<bool> IsWalkable;

    [SerializeField]
    Tilemap Walls;

    private Pathfinder _Pathfinder;

    private void Start()
    {
        MainGrid = new Grid<MainGridCell>(Width, Height, CellSize, GridOrigin);
        _Pathfinder = new Pathfinder();
        BoundsInt gridBounds = new BoundsInt();
        gridBounds.xMin = Mathf.FloorToInt(GridOrigin.x);
        gridBounds.yMin = Mathf.FloorToInt(GridOrigin.y);
        gridBounds.xMax = gridBounds.xMin + Width;
        gridBounds.yMax = gridBounds.yMin + Height;
        IsWalkable = GetTilesOnTilemap(gridBounds);
    }
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
                    MainGrid.GetValue(x,y).IsWalkable = false;
                }
                else
                {
                    spots.SetValue(tilePos, true);
                    MainGrid.GetValue(x, y).IsWalkable = true;
                }
            }
        }
        return spots;
    }
}
