//using Unity.Collections;
//using Unity.Mathematics;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class UnitSystemAuthoring : MonoBehaviour
//{
//    [SerializeField] 
//    Tilemap walls;

//    [SerializeField]
//    private int width;

//    [SerializeField]
//    private int height;

//    [SerializeField]
//    private float cellSize;

//    [SerializeField]
//    private Vector3 gridOrigin;


//    void Start()
//    {
//        UnitSystem.width = width;
//        UnitSystem.height = height;
//        UnitSystem.cellSize = cellSize;
//        UnitSystem.gridOrigin = gridOrigin;
//        BoundsInt gridBounds = new BoundsInt();
//        gridBounds.xMin = Mathf.FloorToInt(gridOrigin.x);
//        gridBounds.yMin = Mathf.FloorToInt(gridOrigin.y);
//        gridBounds.xMax = gridBounds.xMin + width;
//        gridBounds.yMax = gridBounds.yMin + height;
//        UnitSystem.isWalkable = GetTilesOnTilemap(gridBounds);
//    }
//    private FlatGrid<bool> GetTilesOnTilemap(BoundsInt bounds)
//    {
//        //tilemap.CompressBounds();
//        //bounds = tilemap.cellBounds;
//        FlatGrid<bool> spots = new FlatGrid<bool>(bounds.size.x, bounds.size.y);
//        //Debug.Log("Bounds:" + bounds);

//        for (int x = 0; x < bounds.size.x; x++)
//        {
//            for (int y = 0; y < bounds.size.y; y++)
//            {
//                int2 tilePos = new int2(bounds.xMin + x, bounds.yMin + y);

//                if (walls.HasTile(new Vector3Int(tilePos.x, tilePos.y, 0)))
//                {
//                    spots.SetValue(tilePos, false);
//                }
//                else
//                {
//                    spots.SetValue(tilePos, true);
//                }
//            }
//        }
//        return spots;
//    }
//}
