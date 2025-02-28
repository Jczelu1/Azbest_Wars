using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using Unity.Collections;


public class PathfinderTester : MonoBehaviour
{
    private Pathfinder _Pathfinder;

    [SerializeField]
    Tilemap walls;

    [SerializeField]
    private GameObject Guy;

    [SerializeField]
    private int width;

    [SerializeField]
    private int height;

    [SerializeField]
    private float cellSize;

    [SerializeField]
    private Vector3 gridOrigin;

    private NativeArray<bool> isWalkable;

    private bool changePath;
    private bool isWalking;

    private NativeList<int2> Path;
    void Start()
    {
        BoundsInt gridBounds = new BoundsInt();
        gridBounds.xMin = Mathf.FloorToInt(gridOrigin.x);
        gridBounds.yMin = Mathf.FloorToInt(gridOrigin.y);
        gridBounds.xMax = gridBounds.xMin + width;
        gridBounds.yMax = gridBounds.yMin + height;
        isWalkable = GetTilesOnTilemap(gridBounds);
        _Pathfinder = new Pathfinder();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Utils.GetMouseWorldPosition();
            int2 endPos = GetXY(mousePos);
            //Debug.Log(x + " " + y1);
            if (endPos.x < 0 || endPos.y < 0 || endPos.x > width || endPos.y > height) return;
            int2 startPos = GetXY(Guy.transform.position);
            //Debug.Log(x2 + " " + y2);
            float startTime = Time.realtimeSinceStartup;
            //Path = _Pathfinder.FindPath(startPos, endPos, new int2(width, height), isWalkable);
            Debug.Log("Time:" + (Time.realtimeSinceStartup - startTime) * 1000);
            if (isWalking)
            {
                changePath = true;
            }
            else
            {
                StartCoroutine(MoveOnPath());
            }
        }
    }
    IEnumerator MoveOnPath()
    {
        isWalking = true;
        for (int i = 1; i < Path.Length; i++)
        {
            if (changePath)
            {
                i = 1;
                changePath = false;
            }
            Guy.transform.position = GetWorldPosition(Path[i]);
            yield return new WaitForSeconds(.5f);
        }
        isWalking = false;
        if (changePath)
        {
            StartCoroutine(MoveOnPath());
        }

    }
    private NativeArray<bool> GetTilesOnTilemap(BoundsInt bounds)
    {
        //tilemap.CompressBounds();
        //bounds = tilemap.cellBounds;
        NativeArray<bool> spots = new NativeArray<bool>(bounds.size.x * bounds.size.y, Allocator.Temp);
        Debug.Log("Bounds:" + bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                int2 tilePos = new int2(bounds.xMin + x, bounds.yMin + y);

                if (walls.HasTile(new Vector3Int(tilePos.x, tilePos.y, 0)))
                {
                    spots[GetIndex(tilePos)] = false;
                }
                else
                {
                    spots[GetIndex(tilePos)] = false;
                }
            }
        }
        return spots;
    }
    private int GetIndex(int2 pos)
    {
        return pos.x + (pos.y * width);
    }
    public int2 GetXY(Vector3 worldPosition)
    {
        int2 res = new int2(0, 0);
        res.x = Mathf.FloorToInt((worldPosition.x - gridOrigin.x + cellSize * .5f) / cellSize);
        res.y = Mathf.FloorToInt((worldPosition.y - gridOrigin.y + cellSize * .5f) / cellSize);
        return res;
    }
    public Vector3 GetWorldPosition(int2 pos)
    {
        return new Vector3(pos.x * cellSize + gridOrigin.x, pos.y * cellSize + gridOrigin.y);
    }
}
