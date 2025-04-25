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

[BurstCompile]
//[UpdateInGroup(typeof(TickSystemGroup))]
//[UpdateBefore(typeof(MoveSystem))]
public partial struct PathfindSystem : ISystem
{
    public static NativeArray<bool> shouldMove = new NativeArray<bool>(4, Allocator.Persistent);
    public static NativeArray<int2> destinations = new NativeArray<int2>(4, Allocator.Persistent);
    public static NativeArray<int> selectedUnits = new NativeArray<int>(4, Allocator.Persistent);
    public static NativeArray<byte> setMoveState = new NativeArray<byte>(4, Allocator.Persistent);
    private ComponentLookup<TeamData> _teamLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitStateData>();
        state.RequireForUpdate<LocalTransform>();
        _teamLookup = state.GetComponentLookup<TeamData>(true);
    }
    public void OnDestroy(ref SystemState state)
    {
        shouldMove.Dispose();
        destinations.Dispose();
        selectedUnits.Dispose();
        setMoveState.Dispose();
    }
    public void OnUpdate(ref SystemState state)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _teamLookup.Update(ref state);

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var ecbSystem = World.DefaultGameObjectInjectionWorld
            .GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        foreach (var (unitState, selected, team) in SystemAPI.Query<UnitStateData, SelectedData, TeamData>()){
            if (team.Team > 3) continue;
            if (!selected.Selected) continue;
            selectedUnits[team.Team]++;
        }
        var occupied = MainGridScript.Instance.Occupied;
        var isWalkable = MainGridScript.Instance.IsWalkable;
        for (int team = 0;team < 4; team++)
        {
            int requiredTiles = selectedUnits[team];
            if (!shouldMove[team]) continue;
            if (destinations[team].x == -1) continue;
            if (!occupied.IsInGrid(destinations[team]) || !isWalkable[destinations[team]])
            {
                shouldMove[team] = false;
                continue;
            }
            int2 startPos = destinations[team];
            NativeList<int2> queue = new NativeList<int2>(Allocator.Temp);
            NativeList<int2> positions = new NativeList<int2>(Allocator.Temp);
            NativeHashSet<int2> visited = new NativeHashSet<int2>(requiredTiles, Allocator.Temp);

            // Initialize the search with the starting position.
            queue.Add(startPos);
            positions.Add(startPos);
            int head = 0; // Use a head pointer for FIFO behavior

            // Breadth-first search loop.
            while (head < queue.Length && requiredTiles > 0)
            {
                int2 current = queue[head];
                head++;

                for (int i = 0; i < Pathfinder.directions.Length; i++)
                {
                    int2 offset = Pathfinder.directions[i];
                    int2 neighbor = current + offset;

                    // Skip if outside grid bounds.
                    if (!occupied.IsInGrid(neighbor))
                        continue;
                    //unwalkable
                    if (!isWalkable[neighbor])
                        continue;
                    if (visited.Contains(neighbor))
                        continue;
                    // Enqueue valid neighbor.
                    queue.Add(neighbor);
                    positions.Add(neighbor);
                    visited.Add(neighbor);
                    requiredTiles--;
                }
            }
            int j = 0;
            foreach (var (unitState, selected, teamData) in SystemAPI.Query<RefRW<UnitStateData>, SelectedData, TeamData>())
            {
                if (teamData.Team != team) continue;
                if (!selected.Selected) continue;
                if (j >= positions.Length)
                {
                    unitState.ValueRW.Destination = positions[0];
                }
                unitState.ValueRW.Destination = positions[j];
                j++;
            }
            selectedUnits[team] = 0;
            queue.Dispose();
            positions.Dispose();
            visited.Dispose();
        }


        //pathfind job
        //UnityEngine.Debug.Log(MainGridScript.Instance.Occupied[0]);

        PathfindJob pathfindJob = new PathfindJob()
        {
            gridSize = new int2(MainGridScript.Instance.Width, MainGridScript.Instance.Height),
            occupied = MainGridScript.Instance.Occupied,
            isWalkable = MainGridScript.Instance.IsWalkable,
            ecb = ecb,
            teamLookup = _teamLookup,
            shouldMove = shouldMove,
            destinations = destinations,
            setMoveState = setMoveState,
        };

        JobHandle pathJobHandle = pathfindJob.ScheduleParallel(state.Dependency);

        state.Dependency = pathJobHandle;
        pathJobHandle.Complete();
        for (int i = 0; i < 4; i++)
        {
            shouldMove[i] = false;
            setMoveState[i] = 255;
        }
        //Ensure ECB commands are played back safely
        ecbSystem.Update(state.WorldUnmanaged);
        stopwatch.Stop();
        //UnityEngine.Debug.Log($"Pathfinding took {stopwatch.Elapsed}");
    }
}

[BurstCompile]
public partial struct PathfindJob : IJobEntity
{
    const int AUTO_FIND_ENEMY_DISTANCE = 12;
    [ReadOnly]
    public NativeArray<bool> shouldMove;
    [ReadOnly]
    public NativeArray<int2> destinations;
    [ReadOnly]
    public NativeArray<byte> setMoveState;
    public int2 gridSize;
    [ReadOnly]
    public FlatGrid<Entity> occupied;
    [ReadOnly]
    public FlatGrid<bool> isWalkable;

    public EntityCommandBuffer.ParallelWriter ecb;

    [ReadOnly]
    public ComponentLookup<TeamData> teamLookup;

    

    public void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref UnitStateData unitState, ref GridPosition gridPosition, ref SelectedData selected)
    {
        int team = teamLookup[entity].Team;
        if (selected.Selected && setMoveState[team]!=255)
        {
            unitState.MovementState = setMoveState[team];
        }

        //pathfind to selected location
        if (selected.Selected && shouldMove[team])
        {
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            Pathfinder.FindPath(gridPosition.Position, unitState.Destination, gridSize, isWalkable.GridArray, occupied.GridArray, true, ref path);

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
            ecb.SetComponent(sortKey, entity, newPathData);

            path.Dispose();
        }
        //stop
        else if(unitState.MovementState == 2)
        {
            unitState.MovementState = 0;
            ecb.RemoveComponent<PathNode>(sortKey, entity);
            ecb.AddBuffer<PathNode>(sortKey, entity);
            unitState.PathIndex = 0;
            unitState.Stuck = 0;
            ecb.SetComponent(sortKey, entity, unitState);
        }
    }
}
