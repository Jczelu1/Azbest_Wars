using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MainGridScript : MonoBehaviour
{
    public static MainGridScript Instance { get; private set; }

    public Grid<MainGridCell> MainGrid;
    public int Width;
    public int Height;
    public float CellSize;
    public Vector2 GridOrigin;
    public FlatGrid<bool> IsWalkable;
    public FlatGrid<int> Occupied;
    [HideInInspector]
    public int2 ClickPosition;
    [HideInInspector]
    public bool Clicked = false;

    [SerializeField]
    private Tilemap Walls;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        MainGrid = new Grid<MainGridCell>(Width, Height, CellSize, GridOrigin);
        InitGrid();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        BoundsInt gridBounds = new BoundsInt();
        gridBounds.xMin = Mathf.FloorToInt(GridOrigin.x);
        gridBounds.yMin = Mathf.FloorToInt(GridOrigin.y);
        gridBounds.xMax = gridBounds.xMin + Width;
        gridBounds.yMax = gridBounds.yMin + Height;

        IsWalkable = GetTilesOnTilemap(gridBounds);
        Occupied = new FlatGrid<int>(Width, Height, Allocator.Persistent);
        //MainGrid.ShowDebugtext();
        MainGrid.ShowDebugLines();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Utils.GetMouseWorldPosition();
            int2 endPos = MainGrid.GetXY(mousePos);
            if (endPos.x == -1) return;
            ClickPosition = endPos;
            Clicked = true;
        }
    }

    private void InitGrid()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                MainGrid.SetValue(new int2(x, y), new MainGridCell());
            }
        }
    }

    private FlatGrid<bool> GetTilesOnTilemap(BoundsInt bounds)
    {
        FlatGrid<bool> spots = new FlatGrid<bool>(bounds.size.x, bounds.size.y, Allocator.Persistent);
        //Debug.Log("Bounds:" + bounds);

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
