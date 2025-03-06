using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
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
        state.RequireForUpdate<PathData>();
        state.RequireForUpdate<LocalTransform>();
        EntityQuery query = SystemAPI.QueryBuilder()
            .WithAll<PathData, LocalTransform>()
            .Build();
        state.RequireForUpdate(query);

        _bufferLookup = state.GetBufferLookup<PathNode>(true);
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
            // Get the ECB system singleton and create a parallel writer
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Schedule the pathfinding job
            PathfindJob pathfindJob = new PathfindJob()
            {
                //pathLookup = _bufferLookup,
                destination = MainGridScript.Instance.ClickPosition,
                gridSize = new int2(MainGridScript.Instance.Width, MainGridScript.Instance.Height),
                walls = MainGridScript.Instance.IsWalkable.GridArray,
                ecb = ecb  // Pass ParallelWriter ECB
            };

            JobHandle pathJobHandle = pathfindJob.ScheduleParallel(state.Dependency);

            // Ensure ECB commands are played back safely
            state.Dependency = pathJobHandle;
            pathJobHandle.Complete();
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Pathfinding took {stopwatch.Elapsed}");
        }

        // Schedule the move job in parallel
        MoveJob moveJob = new MoveJob()
        {
            pathLookup = _bufferLookup,
            gridOrigin = MainGridScript.Instance.GridOrigin,
            cellSize = MainGridScript.Instance.CellSize,
            occupied = MainGridScript.Instance.Occupied
        };
        state.Dependency = moveJob.Schedule(state.Dependency);
    }
    void InitializeOccupiedGrid(EntityQuery entityQuery, ref FlatGrid<int> occupied)
    {
        var entities = entityQuery.ToEntityArray(Allocator.Temp);
        var gridPositions = entityQuery.ToComponentDataArray<GridPosition>(Allocator.Temp);

        for (int i = 0; i < entities.Length; i++)
        {
            int index = occupied.GetIndex(gridPositions[i].Position);
            occupied.SetValue(index, 1);
        }

        entities.Dispose();
        gridPositions.Dispose();
    }
}


[BurstCompile]
public partial struct MoveJob : IJobEntity
{
    [ReadOnly]
    public BufferLookup<PathNode> pathLookup;
    public float2 gridOrigin;
    public float cellSize;
    //this will cause bugs
    [NativeDisableParallelForRestriction]
    public FlatGrid<int> occupied;
    public void Execute(Entity entity, ref PathData pathData, ref LocalTransform transform, ref GridPosition gridPosition)
    {
        if (!pathLookup.HasBuffer(entity))
            return;

        DynamicBuffer<PathNode> pathBuffer = pathLookup[entity];

        if (pathData.PathIndex >= 0 && pathData.PathIndex < pathBuffer.Length)
        {
            int2 targetCell = pathBuffer[pathData.PathIndex].PathPos;
            int targetIndex = occupied.GetIndex(targetCell);
            ref int cellRef = ref occupied.GetRef(targetIndex);
            //UnityEngine.Debug.Log(cellRef);

            //atomic exchange for thread safety
            if (Interlocked.CompareExchange(ref cellRef, 1, 0) == 0)
            {
                float2 targetPosition = new float2(
                    gridOrigin.x + cellSize * pathBuffer[pathData.PathIndex].PathPos.x,
                    gridOrigin.y + cellSize * pathBuffer[pathData.PathIndex].PathPos.y
                );
                transform.Position = new float3(targetPosition.x, targetPosition.y, transform.Position.z);
                int oldIndex = occupied.GetIndex(gridPosition.Position);
                ref int oldCellRef = ref occupied.GetRef(oldIndex);
                //use atomic exchange because why not
                Interlocked.Exchange(ref oldCellRef, 0);
                gridPosition.Position = pathBuffer[pathData.PathIndex].PathPos;
                //Debug.Log(pathData.PathIndex);
                pathData.PathIndex++;
            }
        }
        else
        {
            occupied.SetValue(gridPosition.Position, 1);
        }
    }
}
[BurstCompile]
public partial struct PathfindJob : IJobEntity
{
    public int2 destination;
    public int2 gridSize;
    [ReadOnly]
    public NativeArray<bool> walls;
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref PathData pathData, ref GridPosition gridPosition)
    {
        // Compute the new path.
        NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
        Pathfinder.FindPath(gridPosition.Position, destination, gridSize, walls, ref path);

        // Remove the existing PathNode dynamic buffer.
        ecb.RemoveComponent<PathNode>(sortKey, entity);
        // Re-add the dynamic buffer (this creates a new, empty buffer).
        ecb.AddBuffer<PathNode>(sortKey, entity);

        // Append the new path nodes in reverse order.
        for (int i = path.Length - 2; i >= 0; i--)
        {
            ecb.AppendToBuffer<PathNode>(sortKey, entity, new PathNode { PathPos = path[i] });
        }

        // Reset the path index.
        var newPathData = pathData;
        newPathData.PathIndex = 0;
        ecb.SetComponent(sortKey, entity, newPathData);

        path.Dispose();
    }
}
