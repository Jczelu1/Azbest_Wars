using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine.Tilemaps;
using System.IO;

public partial struct UnitSystem : ISystem
{
    private Pathfinder pathfinder;

    public static int width;

    public static int height;

    public static float cellSize;

    public static float3 gridOrigin;

    public static FlatGrid<bool> isWalkable;

    private bool changePath;
    private bool isWalking;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitData>();
        pathfinder = new Pathfinder();
    }
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log(SystemAPI.Time.DeltaTime);
        UnitJob job = new UnitJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        job.ScheduleParallel();
    }
    public partial struct UnitJob : IJobEntity
    {
        public float DeltaTime;
        public int2 startPos;
        public int2 endPos;
        public void Execute(ref UnitData data, ref LocalTransform transform)
        {

            transform = transform.Translate(new float3(1f,1f,0f)*data.Speed*DeltaTime);
        }
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
