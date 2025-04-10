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

    [SerializeField]
    CameraMover cameraMover;
    //grid
    public DummyGrid MainGrid;
    public int Width;
    public int Height;
    public float CellSize;
    public Vector2 GridOrigin;
    public FlatGrid<Entity> Occupied;
    public FlatGrid<bool> IsWalkable;

    //UI
    [SerializeField]
    RectTransform MinimapTransform;
    UIGrid Minimap;
    [SerializeField]
    RectTransform MinimapCameraMarker;
    
    //selecting
    [HideInInspector]
    public bool Selected = false;
    [HideInInspector]
    public int2 SelectStartPosition;
    [HideInInspector]
    public int2 SelectEndPosition;
    private List<GameObject> SelectSprites = new List<GameObject>();
    [HideInInspector]

    [SerializeField]
    private GameObject selectSpritePrefab;
    [SerializeField]
    private GameObject MoveToPrefab;
    [SerializeField]
    private GameObject CantMovePrefab;
    private GameObject MoveToObject;

    [SerializeField]
    private Tilemap Walls;

    public InputAction leftClickAction;
    public InputAction rightClickAction;
    public InputAction multiselectAction;

    private void Awake()
    {
        Instance = this;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        MainGrid = new DummyGrid(Width, Height, CellSize, GridOrigin);
        Occupied = new FlatGrid<Entity>(Width, Height, Allocator.Persistent);
        BoundsInt gridBounds = new BoundsInt();
        gridBounds.xMin = Mathf.FloorToInt(GridOrigin.x);
        gridBounds.yMin = Mathf.FloorToInt(GridOrigin.y);
        gridBounds.xMax = gridBounds.xMin + Width;
        gridBounds.yMax = gridBounds.yMin + Height;
        IsWalkable = new FlatGrid<bool>(Width, Height, Allocator.Persistent);
        GetTilesOnTilemap(gridBounds, ref IsWalkable);
    }

    private void Start()
    {
        //MainGrid.ShowDebugtext();
        //MainGrid.ShowDebugLines();
        leftClickAction = InputSystem.actions.FindAction("LeftClick");
        rightClickAction = InputSystem.actions.FindAction("RightClick");
        multiselectAction = InputSystem.actions.FindAction("Multiselect");

        Vector2 minCameraPosition = new Vector2(); 
        Vector2 maxCameraPosition = new Vector2();
        minCameraPosition.x = GridOrigin.x + 7.5f - 2f;
        minCameraPosition.y = GridOrigin.y + 4f - 2f;
        maxCameraPosition.x = GridOrigin.x + Width - 7.5f + 2f;
        maxCameraPosition.y = GridOrigin.y + Height - 4f + 2f;
        cameraMover.minCameraPosition = minCameraPosition;
        cameraMover.maxCameraPosition = maxCameraPosition;

        Minimap = new UIGrid(32, 32, MinimapTransform);
    }
    private void Update()
    {
        //pathfind
        if (Selected && rightClickAction.WasPressedThisFrame())
        {
            Vector3 mousePos = Utils.GetMouseWorldPosition();
            int2 endPos = MainGrid.GetXY(mousePos);
            if (!Occupied.IsInGrid(endPos))
            {
                MoveToObject = MainGrid.CreateSprite(CantMovePrefab, endPos);
                Destroy(MoveToObject, 0.5f);
                return;
            }
            //RightClickPosition = endPos;
            Destroy(MoveToObject);
            if (!IsWalkable[endPos])
            {
                MoveToObject = MainGrid.CreateSprite(CantMovePrefab, endPos);
                Destroy(MoveToObject, 0.5f);
                return;
            }
            PathfindSystem.shouldMove[TeamManager.Instance.PlayerTeam] = true;
            PathfindSystem.destinations[TeamManager.Instance.PlayerTeam] = endPos;
            PathfindSystem.setMoveState[TeamManager.Instance.PlayerTeam] = 0;
            MoveToObject = MainGrid.CreateSprite(MoveToPrefab, endPos);
            Destroy(MoveToObject, 0.5f);
        }
        
        //select
        if (leftClickAction.WasPressedThisFrame())
        {
            Vector3 mousePos = Utils.GetMouseWorldPosition();
            int2 startPos = MainGrid.GetXY(mousePos);
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
            Selected = true;
            SelectSystem.updateSelect = true;
            if (!multiselectAction.IsPressed())
            {
                SelectSystem.resetSelect = true;
            }
        }
        if (leftClickAction.IsPressed())
        {
            Vector3 mousePos = Utils.GetMouseWorldPosition();
            int2 endPos = MainGrid.GetXY(mousePos);
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
        //update minimap
        int2 cameraGridPosition = MainGrid.GetXY(cameraMover.GetPosition());
        cameraGridPosition.x = (int)(cameraGridPosition.x * ((float)Minimap.Width / Width));
        cameraGridPosition.y = (int)(cameraGridPosition.y * ((float)Minimap.Height / Height));
        //Debug.Log(cameraGridPosition);
        MinimapCameraMarker.localPosition = Minimap.GetLocalPosition(cameraGridPosition);
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
    public void MinimapPressed()
    {
        // Debug.Log(Utils.GetMouseLocalPosition(MinimapTransform));
        Debug.Log(Utils.GetMouseLocalPosition(MinimapTransform));
        int2 clickPos = Minimap.GetXY(Utils.GetMouseLocalPosition(MinimapTransform));
        Debug.Log(clickPos);
        clickPos.x = (int)(clickPos.x * ((float)Width / Minimap.Width));
        clickPos.y = (int)(clickPos.y * ((float)Height / Minimap.Height));
        //Debug.Log(clickPos);
        MoveCameraTo(clickPos);
    }
    private void MoveCameraTo(int2 pos)
    {
        Vector3 cameraPosition;
        cameraPosition.z = -10;
        cameraPosition.x = GridOrigin.x + pos.x * CellSize - 0.5f;
        cameraPosition.y = GridOrigin.y + pos.y * CellSize;
        cameraPosition.x = Mathf.Clamp(cameraPosition.x, cameraMover.minCameraPosition.x, cameraMover.maxCameraPosition.y);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, cameraMover.minCameraPosition.y, cameraMover.maxCameraPosition.y);
        cameraMover.MoveCamera(cameraPosition);
    }
}
