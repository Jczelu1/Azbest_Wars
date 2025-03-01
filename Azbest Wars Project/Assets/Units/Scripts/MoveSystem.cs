using System;
using System.Diagnostics;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
public partial struct MoveSystem : ISystem
{
    private BufferLookup<PathNode> _bufferLookup;

    public void OnCreate(ref SystemState state)
    {
        // Ensure system only updates if these components exist
        state.RequireForUpdate<PathData>();
        state.RequireForUpdate<LocalTransform>();

        // Initialize BufferLookup
        _bufferLookup = state.GetBufferLookup<PathNode>(false); // not Read-only
    }
    public void OnStartRunning(ref SystemState state)
    {
        
    }
    public void OnUpdate(ref SystemState state)
    {
        // Update the buffer lookup (needed for each frame)
        _bufferLookup.Update(ref state);
        if (MainGridScript.Instance.Clicked)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            PathfindJob pathfindJob = new PathfindJob()
            {
                pathLookup = _bufferLookup,
                destination = MainGridScript.Instance.ClickPosition,
                gridSize = new int2(MainGridScript.Instance.Width, MainGridScript.Instance.Height),
                walls = MainGridScript.Instance.IsWalkable.GridArray
            };
            JobHandle pathJobHandle = pathfindJob.Schedule(state.Dependency);
            pathJobHandle.Complete(); // Ensure the job is finished before measuring time

            stopwatch.Stop();
            UnityEngine.Debug.Log($"Pathfinding took {stopwatch.Elapsed}");

            state.Dependency = pathJobHandle;
        }

        // Schedule the job across all matching entities
        MoveJob moveJob = new MoveJob
        {
            pathLookup = _bufferLookup,
            gridOrigin = MainGridScript.Instance.GridOrigin,
            cellSize = MainGridScript.Instance.CellSize
        };
        state.Dependency = moveJob.ScheduleParallel(state.Dependency);
    }
}


[BurstCompile]
public partial struct MoveJob : IJobEntity
{
    [ReadOnly]
    public BufferLookup<PathNode> pathLookup;
    public float2 gridOrigin;
    public float cellSize;

    public void Execute(Entity entity, ref PathData pathData, ref LocalTransform transform, ref GridPosition gridPosition)
    {
        if (!pathLookup.HasBuffer(entity))
            return;

        DynamicBuffer<PathNode> pathBuffer = pathLookup[entity];

        if (pathData.PathIndex >= 0 && pathData.PathIndex < pathBuffer.Length)
        {
            float2 targetPosition = new float2(
                gridOrigin.x + cellSize * pathBuffer[pathData.PathIndex].PathPos.x,
                gridOrigin.y + cellSize * pathBuffer[pathData.PathIndex].PathPos.y
            );

            transform.Position = new float3(targetPosition.x, targetPosition.y, transform.Position.z);
            gridPosition.Position = pathBuffer[pathData.PathIndex].PathPos;
            //Debug.Log(pathData.PathIndex);
            pathData.PathIndex++;
        }
    }
}
[BurstCompile]
public partial struct PathfindJob : IJobEntity
{
    public BufferLookup<PathNode> pathLookup;
    public int2 destination;
    public int2 gridSize;
    public NativeArray<bool> walls;
    public void Execute(Entity entity, ref PathData pathData, ref GridPosition gridPosition)
    {
        if (!pathLookup.HasBuffer(entity))
            return;
        DynamicBuffer<PathNode> pathBuffer = pathLookup[entity];
        
        NativeList<int2> path = new NativeList<int2>(Allocator.TempJob);
        Pathfinder.FindPath(gridPosition.Position, destination, gridSize, walls, ref path);
        //if(path.Length == 0)
        //{
        //    path.Dispose();
        //    return;
        //}
        pathBuffer.Clear();
        for (int i = path.Length - 1; i > -1; i--)
        {
            pathBuffer.Add(new PathNode { PathPos = path[i] });
            //Debug.Log(path[i]);
        }
        pathData.PathIndex = 0;
        path.Dispose();
    }
}