using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Entities;
public class MainGridScript : MonoBehaviour
{
    public static MainGridScript Instance { get; private set; }

    public DummyGrid MainGrid;
    public int Width;
    public int Height;
    public float CellSize;
    public Vector2 GridOrigin;
    public FlatGrid<Entity> Occupied;
    public FlatGrid<bool> IsWalkable;
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
        MainGrid = new DummyGrid(Width, Height, CellSize, GridOrigin);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        BoundsInt gridBounds = new BoundsInt();
        gridBounds.xMin = Mathf.FloorToInt(GridOrigin.x);
        gridBounds.yMin = Mathf.FloorToInt(GridOrigin.y);
        gridBounds.xMax = gridBounds.xMin + Width;
        gridBounds.yMax = gridBounds.yMin + Height;

        Occupied = new FlatGrid<Entity>(Width, Height, Allocator.Persistent);
        IsWalkable = new FlatGrid<bool>(Width, Height, Allocator.Persistent);
        GetTilesOnTilemap(gridBounds, ref IsWalkable);
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
    private void OnDestroy()
    {
        Occupied.GridArray.Dispose();
        IsWalkable.GridArray.Dispose();
    }

    private void GetTilesOnTilemap(BoundsInt bounds, ref FlatGrid<bool> isWalkable)
    {
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                int2 tilePos = new int2(bounds.xMin + x, bounds.yMin + y);

                if (Walls.HasTile(new Vector3Int(tilePos.x, tilePos.y, 0)))
                {
                    isWalkable.SetValue(tilePos, false);
                }
                else
                {
                    isWalkable.SetValue(tilePos, true);
                }
            }
        }
    }
}
