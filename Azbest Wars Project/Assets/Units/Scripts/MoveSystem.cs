using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
public partial struct MoveSystem : ISystem
{
    private BufferLookup<PathNode> _bufferLookup;
    private ComponentLookup<TeamData> _teamLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitStateData>();
        state.RequireForUpdate<LocalTransform>();
        _bufferLookup = state.GetBufferLookup<PathNode>(true);
        _teamLookup = state.GetComponentLookup<TeamData>(true);
    }
    public void OnStartRunning(ref SystemState state)
    {
        
    }
    public void OnUpdate(ref SystemState state)
    {
        _bufferLookup.Update(ref state);
        _teamLookup.Update(ref state);

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();


        //pathfind job
        if (MainGridScript.Instance.RightClick)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            PathfindJob pathfindJob = new PathfindJob()
            {
                destination = MainGridScript.Instance.RightClickPosition,
                gridSize = new int2(MainGridScript.Instance.Width, MainGridScript.Instance.Height),
                occupied = MainGridScript.Instance.Occupied.GridArray,
                isWalkable = MainGridScript.Instance.IsWalkable.GridArray,
                ecb = ecb
            };

            JobHandle pathJobHandle = pathfindJob.ScheduleParallel(state.Dependency);

            //Ensure ECB commands are played back safely
            state.Dependency = pathJobHandle;
            pathJobHandle.Complete();
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Pathfinding took {stopwatch.Elapsed}");
        }
        //attack job
        var stopwatch2 = new Stopwatch();
        stopwatch2.Start();
        EnemyfindJob enemyJob = new EnemyfindJob()
        {
            gridSize = new int2(MainGridScript.Instance.Width, MainGridScript.Instance.Height),
            occupied = MainGridScript.Instance.Occupied,
            isWalkable = MainGridScript.Instance.IsWalkable,
            ecb = ecb,
            teamLookup = _teamLookup,
        };
        JobHandle enemyJobHandle = enemyJob.ScheduleParallel(state.Dependency);
        //Ensure ECB commands are played back safely
        state.Dependency = enemyJobHandle;
        enemyJobHandle.Complete();
        stopwatch2.Stop();
        //UnityEngine.Debug.Log($"Enemyfinding took {stopwatch2.Elapsed}");

        //unstuck job
        StuckPathfindJob stuckJob = new StuckPathfindJob()
        {
            gridSize = new int2(MainGridScript.Instance.Width, MainGridScript.Instance.Height),
            occupied = MainGridScript.Instance.Occupied.GridArray,
            isWalkable = MainGridScript.Instance.IsWalkable.GridArray,
            ecb = ecb 
        };
        JobHandle stuckJobHandle = stuckJob.ScheduleParallel(state.Dependency);
        //Ensure ECB commands are played back safely
        state.Dependency = stuckJobHandle;
        stuckJobHandle.Complete();


        //move job
        MoveJob moveJob = new MoveJob()
        {
            pathLookup = _bufferLookup,
            gridOrigin = MainGridScript.Instance.GridOrigin,
            cellSize = MainGridScript.Instance.CellSize,
            occupied = MainGridScript.Instance.Occupied
        };
        //not parallel because race conditions when moving to a tile
        state.Dependency = moveJob.Schedule(state.Dependency);
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
    public FlatGrid<Entity> occupied;
    public void Execute(Entity entity, ref UnitStateData unitState, ref LocalTransform transform, ref GridPosition gridPosition)
    {
        if (!pathLookup.HasBuffer(entity))
            return;

        DynamicBuffer<PathNode> pathBuffer = pathLookup[entity];

        if (unitState.PathIndex >= 0 && unitState.PathIndex < pathBuffer.Length)
        {
            unitState.Moving = true;
            int2 targetCell = pathBuffer[unitState.PathIndex].PathPos;
            //UnityEngine.Debug.Log(cellRef);

            if (occupied[targetCell] == Entity.Null)
            {
                float2 targetPosition = new float2(
                    gridOrigin.x + cellSize * pathBuffer[unitState.PathIndex].PathPos.x,
                    gridOrigin.y + cellSize * pathBuffer[unitState.PathIndex].PathPos.y
                );
                transform.Position = new float3(targetPosition.x, targetPosition.y, transform.Position.z);
                occupied[gridPosition.Position] = Entity.Null;
                gridPosition.Position = pathBuffer[unitState.PathIndex].PathPos;
                occupied.SetValue(gridPosition.Position, entity);
                //Debug.Log(pathData.PathIndex);
                unitState.PathIndex++;
                unitState.Stuck = 0;
            }
            else
            {
                if(unitState.Stuck != 2)
                    unitState.Stuck = 1;
            }
        }
        else
        {
            unitState.Moving = false;
            occupied.SetValue(gridPosition.Position, entity);
        }
    }
}
[BurstCompile]
public partial struct PathfindJob : IJobEntity
{
    public int2 destination;
    public int2 gridSize;
    [ReadOnly]
    public NativeArray<Entity> occupied;
    [ReadOnly]
    public NativeArray<bool> isWalkable;
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref UnitStateData unitState, ref GridPosition gridPosition, ref TeamData teamData, ref SelectedData selected)
    {
        //temporary
        if (teamData.Team != 1) return;
        if (!selected.Selected) return;
        selected.Selected = false;

        NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
        Pathfinder.FindPath(gridPosition.Position, destination, gridSize, isWalkable, occupied, true, ref path);

        //there is no other way to do this for some reason
        ecb.RemoveComponent<PathNode>(sortKey, entity);
        ecb.AddBuffer<PathNode>(sortKey, entity);

        // Append the new path nodes in reverse order.
        for (int i = path.Length - 2; i >= 0; i--)
        {
            ecb.AppendToBuffer<PathNode>(sortKey, entity, new PathNode { PathPos = path[i] });
        }

        //Reset pathData
        var newPathData = unitState;
        newPathData.PathIndex = 0;
        newPathData.Stuck = 0;
        newPathData.Destination = destination;
        ecb.SetComponent(sortKey, entity, newPathData);

        path.Dispose();
    }
}

[BurstCompile]
public partial struct StuckPathfindJob : IJobEntity
{
    public int2 gridSize;
    [ReadOnly]
    public NativeArray<Entity> occupied;
    [ReadOnly]
    public NativeArray<bool> isWalkable;
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref UnitStateData unitState, ref GridPosition gridPosition)
    {
        if (unitState.Stuck!=1)
            return;

        NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
        Pathfinder.FindPath(gridPosition.Position, unitState.Destination, gridSize, isWalkable, occupied, false, ref path);

        //there is no other way to do this for some reason
        ecb.RemoveComponent<PathNode>(sortKey, entity);
        ecb.AddBuffer<PathNode>(sortKey, entity);

        //no other path exists
        if (path.Length == 0)
        {
            unitState.Stuck = 2;
            path.Dispose();
            return;
        }
        //Append the new path nodes in reverse order.
        for (int i = path.Length - 2; i >= 0; i--)
        {
            ecb.AppendToBuffer<PathNode>(sortKey, entity, new PathNode { PathPos = path[i] });
        }

        //Reset pathData
        var newPathData = unitState;
        newPathData.PathIndex = 0;
        newPathData.Stuck = 0;
        ecb.SetComponent(sortKey, entity, newPathData);

        path.Dispose();
    }
}
[BurstCompile]
public partial struct EnemyfindJob : IJobEntity
{
    const int autoFindEnemyDistance = 8;
    public int2 gridSize;
    [ReadOnly]
    public FlatGrid<Entity> occupied;
    [ReadOnly]
    public FlatGrid<bool> isWalkable;
    public EntityCommandBuffer.ParallelWriter ecb;
    [ReadOnly]
    public ComponentLookup<TeamData> teamLookup;

    public void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref UnitStateData unitState, ref GridPosition gridPosition)
    {
        // Only process team 1 for now.
        int team = teamLookup[entity].Team;
        if (team != 1)
            return;

        int2 startPos = gridPosition.Position;
        // Maximum possible number of nodes in the search area
        int maxNodes = (autoFindEnemyDistance * 2 + 1) * (autoFindEnemyDistance * 2 + 1);
        NativeList<int2> queue = new NativeList<int2>(maxNodes, Allocator.Temp);
        NativeHashMap<int2, int2> searched = new NativeHashMap<int2, int2>(maxNodes, Allocator.Temp);

        // Initialize the search with the starting position.
        queue.Add(startPos);
        searched.Add(startPos, new int2(-1, -1));

        int2 enemyPosition = new int2(-1, -1);
        bool foundEnemy = false;
        int head = 0; // Use a head pointer for FIFO behavior

        // Breadth-first search loop.
        while (head < queue.Length && !foundEnemy)
        {
            int2 current = queue[head];
            head++;

            for (int i = 0; i < Pathfinder.directions.Length; i++)
            {
                int2 offset = Pathfinder.directions[i];
                int2 neighbor = current + offset;

                // Skip if already visited.
                if (searched.ContainsKey(neighbor))
                    continue;

                // Skip if outside grid bounds.
                if (!occupied.IsInGrid(neighbor))
                    continue;
                //unwalkable
                if (!isWalkable[neighbor])
                    continue;

                // Ensure neighbor is within the auto-find search radius.
                if (math.abs(neighbor.x - startPos.x) > autoFindEnemyDistance ||
                    math.abs(neighbor.y - startPos.y) > autoFindEnemyDistance)
                    continue;

                //maybe also continue if tile also occupied by other unit
                if (occupied[neighbor] != Entity.Null && occupied[neighbor] != entity)
                {
                    Entity enemyEntity = occupied[neighbor];
                    // Check if the enemy is on a different team.
                    if (teamLookup[enemyEntity].Team != team)
                    {
                        // Mark enemy as found
                        enemyPosition = neighbor;
                        searched.Add(neighbor, current);
                        foundEnemy = true;
                        break;
                    }
                }

                // Enqueue valid neighbor.
                queue.Add(neighbor);
                searched.Add(neighbor, current);
            }
        }

        // If no enemy was found, dispose temporary collections and exit.
        if (enemyPosition.x == -1)
        {
            queue.Dispose();
            searched.Dispose();
            return;
        }

        // Backtrack from the enemy position to build the path.
        NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
        int2 pathNode = enemyPosition;
        while (pathNode.x != -1)
        {
            path.Add(pathNode);
            pathNode = searched[pathNode];
        }
        if (path.Length <= 1)
        {
            queue.Dispose();
            searched.Dispose();
            return;
        }
        // Update the entity's path by removing the old path and appending new nodes in reverse order.
        ecb.RemoveComponent<PathNode>(sortKey, entity);
        ecb.AddBuffer<PathNode>(sortKey, entity);

        ecb.AppendToBuffer<PathNode>(sortKey, entity, new PathNode { PathPos = path[path.Length - 2] });
        //for (int i = path.Length - 2; i >= 1; i--)
        //{
        //    ecb.AppendToBuffer<PathNode>(sortKey, entity, new PathNode { PathPos = path[i] });
        //}


        // Reset pathData
        unitState.PathIndex = 0;
        unitState.Stuck = 0;
        unitState.Destination = enemyPosition;
        ecb.SetComponent(sortKey, entity, unitState);

        // Dispose of temporary native collections.
        queue.Dispose();
        searched.Dispose();
        path.Dispose();
    }
}
