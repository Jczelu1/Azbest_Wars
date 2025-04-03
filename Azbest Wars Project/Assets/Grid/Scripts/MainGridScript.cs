using System.IO;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Entities;
using UnityEngine.InputSystem;
public class MainGridScript : MonoBehaviour
{
    public static MainGridScript Instance { get; private set; }

    //grid
    public DummyGrid MainGrid;
    public int Width;
    public int Height;
    public float CellSize;
    public Vector2 GridOrigin;
    public FlatGrid<Entity> Occupied;
    public FlatGrid<bool> IsWalkable;
    public int PlayerTeam = 0;

    //right click
    [HideInInspector]
    public int2 RightClickPosition;
    [HideInInspector]
    public bool RightClick = false;

    
    //selecting
    [HideInInspector]
    public bool Selected = false;
    [HideInInspector]
    public int2 SelectStartPosition;
    [HideInInspector]
    public int2 SelectEndPosition;
    private List<GameObject> SelectSprites = new List<GameObject>();
    [HideInInspector]
    public bool UpdateSelected = true;

    [SerializeField]
    private GameObject selectSpritePrefab;
    [SerializeField]
    private GameObject MoveToPrefab;
    [SerializeField]
    private GameObject CantMovePrefab;
    private GameObject MoveToObject;

    [HideInInspector]
    public byte SetMoveState = 255;

    [SerializeField]
    private Tilemap Walls;

    public InputAction leftClickAction;
    public InputAction rightClickAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        MainGrid = new DummyGrid(Width, Height, CellSize, GridOrigin);
        Occupied = new FlatGrid<Entity>(Width, Height, Allocator.Persistent);
        BoundsInt gridBounds = new BoundsInt();
        gridBounds.xMin = Mathf.FloorToInt(GridOrigin.x);
        gridBounds.yMin = Mathf.FloorToInt(GridOrigin.y);
        gridBounds.xMax = gridBounds.xMin + Width;
        gridBounds.yMax = gridBounds.yMin + Height;
        IsWalkable = new FlatGrid<bool>(Width, Height, Allocator.Persistent);
        GetTilesOnTilemap(gridBounds, ref IsWalkable);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        //MainGrid.ShowDebugtext();
        //MainGrid.ShowDebugLines();
        leftClickAction = InputSystem.actions.FindAction("LeftClick");
        rightClickAction = InputSystem.actions.FindAction("RightClick");
    }

    private void Update()
    {
        //pathfind
        if (Selected && rightClickAction.WasPressedThisFrame())
        {
            Vector3 mousePos = Utils.GetMouseWorldPosition();
            int2 endPos = MainGrid.GetXY(mousePos);
            if (endPos.x == -1) return;
            RightClickPosition = endPos;
            Destroy(MoveToObject);
            if (!IsWalkable[endPos])
            {
                MoveToObject = MainGrid.CreateSprite(CantMovePrefab, endPos);
                Destroy(MoveToObject, 0.5f);
                return;
            }
            RightClick = true;
            MoveToObject = MainGrid.CreateSprite(MoveToPrefab, endPos);
            Destroy(MoveToObject, 0.5f);
        }
        
        //select
        if (leftClickAction.WasPressedThisFrame())
        {
            Vector3 mousePos = Utils.GetMouseWorldPosition();
            int2 startPos = MainGrid.GetXY(mousePos);
            if (startPos.x == -1) return;
            SelectStartPosition = startPos;
            Selected = false;
            SelectSprites.Add(MainGrid.CreateSprite(selectSpritePrefab, startPos));
        }
        if (leftClickAction.WasReleasedThisFrame())
        {
            Debug.Log(SelectStartPosition + " " + SelectEndPosition);
            foreach (var sprite in SelectSprites)
            {
                Destroy(sprite);
            }
            SelectSprites.Clear();
            UpdateSelected = true;
            Selected = true;
        }
        if (leftClickAction.IsPressed())
        {
            Vector3 mousePos = Utils.GetMouseWorldPosition();
            int2 endPos = MainGrid.GetXY(mousePos);
            if (endPos.x == -1) return;
            if (endPos.x == SelectEndPosition.x && endPos.y == SelectEndPosition.y)
                return;
            SelectEndPosition = endPos;
            foreach (var sprite in SelectSprites)
            {
                Destroy(sprite);
            }
            SelectSprites.Clear();
            int minX = Mathf.Min(SelectStartPosition.x, SelectEndPosition.x);
            int maxX = Mathf.Max(SelectStartPosition.x, SelectEndPosition.x);
            int minY = Mathf.Min(SelectStartPosition.y, SelectEndPosition.y);
            int maxY = Mathf.Max(SelectStartPosition.y, SelectEndPosition.y);
            for (int x = minX; x <= maxX; x++)
            {
                SelectSprites.Add(MainGrid.CreateSprite(selectSpritePrefab, new int2(x, minY)));
                if (minY != maxY)
                    SelectSprites.Add(MainGrid.CreateSprite(selectSpritePrefab, new int2(x, maxY)));
            }
            for (int y = minY + 1; y < maxY; y++)
            {
                SelectSprites.Add(MainGrid.CreateSprite(selectSpritePrefab, new int2(minX, y)));
                if (minX != maxX)
                    SelectSprites.Add(MainGrid.CreateSprite(selectSpritePrefab, new int2(maxX, y)));
            }
        }

        if (Selected && Input.GetKeyDown(KeyCode.Q))
        {
            SetMoveState = 0;
        }
        if (Selected && Input.GetKeyDown(KeyCode.E))
        {
            SetMoveState = 1;
        }
        if (Selected && Input.GetKeyDown(KeyCode.R))
        {
            SetMoveState = 2;
        }
        if (Selected && Input.GetKeyDown(KeyCode.F))
        {
            SetMoveState = 3;
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
