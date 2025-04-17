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
[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(MoveSystem))]
public partial struct PathfindSystem : ISystem
{
    public static NativeArray<bool> shouldMove = new NativeArray<bool>(4, Allocator.Persistent);
    public static NativeArray<int2> destinations = new NativeArray<int2>(4, Allocator.Persistent);
    public static NativeArray<int> selectedUnits = new NativeArray<int>(4, Allocator.Persistent);
    public static NativeArray<byte> setMoveState = new NativeArray<byte>(4, Allocator.Persistent);
    private ComponentLookup<TeamData> _teamLookup;
    private ComponentLookup<MeleAttackData> _meleAttackLookup;
    private ComponentLookup<RangedAttackData> _rangedAttackLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitStateData>();
        state.RequireForUpdate<LocalTransform>();
        _teamLookup = state.GetComponentLookup<TeamData>(true);
        _meleAttackLookup = state.GetComponentLookup<MeleAttackData>(true);
        _rangedAttackLookup = state.GetComponentLookup<RangedAttackData>(true);
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
        _meleAttackLookup.Update(ref state);
        _rangedAttackLookup.Update(ref state);

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
            meleAttackLookup = _meleAttackLookup,
            rangedAttackLookup = _rangedAttackLookup,
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
    [ReadOnly]
    public ComponentLookup<MeleAttackData> meleAttackLookup;
    [ReadOnly]
    public ComponentLookup<RangedAttackData> rangedAttackLookup;
    const int autoFindEnemyDistance = 8;

    public void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref UnitStateData unitState, ref GridPosition gridPosition, ref SelectedData selected)
    {
        unitState.MoveProcessed = false;
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


        //pathfind to enemy
        else if (unitState.MovementState == 1)
        {
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
                        else if (unitState.Stuck == 1) continue;
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
            if (rangedAttackLookup.HasComponent(entity))
            {
                int maxrange = rangedAttackLookup[entity].Range;
                int range = math.max(math.abs(gridPosition.Position.x - enemyPosition.x), math.abs(gridPosition.Position.y - enemyPosition.y));
                if (range <= maxrange)
                {
                    queue.Dispose();
                    searched.Dispose();
                    return;
                }
            }

            // Backtrack from the enemy position to build the path.
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            int2 pathNode = enemyPosition;
            while (pathNode.x != -1)
            {
                path.Add(pathNode);
                pathNode = searched[pathNode];
            }
            if (path.Length <= 2)
            {
                queue.Dispose();
                searched.Dispose();
                return;
            }

            //maybe do this
            if (meleAttackLookup.HasComponent(entity))
            {
                if (meleAttackLookup[entity].AttackType == 2 && path.Length <= 3)
                {
                    queue.Dispose();
                    searched.Dispose();
                    return;
                }
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


        //retreat
        //else if (unitState.MovementState == 2)
        //{
        //    int2 startPos = gridPosition.Position;
        //    // Maximum possible number of nodes in the search area
        //    int maxNodes = (autoFindEnemyDistance * 2 + 1) * (autoFindEnemyDistance * 2 + 1);
        //    NativeList<int2> queue = new NativeList<int2>(maxNodes, Allocator.Temp);
        //    NativeHashMap<int2, int2> searched = new NativeHashMap<int2, int2>(maxNodes, Allocator.Temp);

        //    // Initialize the search with the starting position.
        //    queue.Add(startPos);
        //    searched.Add(startPos, new int2(-1, -1));

        //    int2 enemyPosition = new int2(-1, -1);
        //    bool foundEnemy = false;
        //    int head = 0; // Use a head pointer for FIFO behavior

        //    // Breadth-first search loop.
        //    while (head < queue.Length && !foundEnemy)
        //    {
        //        int2 current = queue[head];
        //        head++;

        //        for (int i = 0; i < Pathfinder.directions.Length; i++)
        //        {
        //            int2 offset = Pathfinder.directions[i];
        //            int2 neighbor = current + offset;

        //            // Skip if already visited.
        //            if (searched.ContainsKey(neighbor))
        //                continue;

        //            // Skip if outside grid bounds.
        //            if (!occupied.IsInGrid(neighbor))
        //                continue;
        //            //unwalkable
        //            if (!isWalkable[neighbor])
        //                continue;

        //            // Ensure neighbor is within the auto-find search radius.
        //            if (math.abs(neighbor.x - startPos.x) > autoFindEnemyDistance ||
        //                math.abs(neighbor.y - startPos.y) > autoFindEnemyDistance)
        //                continue;

        //            //maybe also continue if tile also occupied by other unit
        //            if (occupied[neighbor] != Entity.Null && occupied[neighbor] != entity)
        //            {
        //                Entity enemyEntity = occupied[neighbor];
        //                // Check if the enemy is on a different team.
        //                if (teamLookup[enemyEntity].Team != team)
        //                {
        //                    // Mark enemy as found
        //                    enemyPosition = neighbor;
        //                    searched.Add(neighbor, current);
        //                    foundEnemy = true;
        //                    break;
        //                }
        //            }

        //            // Enqueue valid neighbor.
        //            queue.Add(neighbor);
        //            searched.Add(neighbor, current);
        //        }
        //    }

        //    // If no enemy was found, dispose temporary collections and exit.
        //    if (enemyPosition.x == -1)
        //    {
        //        queue.Dispose();
        //        searched.Dispose();
        //        return;
        //    }
        //    int2 enemyDirection = math.sign(new int2(gridPosition.Position.x - enemyPosition.x, gridPosition.Position.y - enemyPosition.y));
        //    int2 bestOption = new int2(-1, -1);
        //    float bestDistance = math.lengthsq(gridPosition.Position - enemyPosition) + .1f;
        //    for (int i = 0; i < 8; i++)
        //    {
        //        int2 retreatDir = Pathfinder.directions[i];
        //        int2 newPos = gridPosition.Position + retreatDir;

        //        // Validate position
        //        if (!occupied.IsInGrid(newPos)) continue;
        //        if (!isWalkable[newPos]) continue;
        //        if (unitState.Stuck == 1 && occupied[newPos] != Entity.Null) continue;

        //        // Check if moving away from enemy
        //        float newDistance = math.lengthsq(newPos - enemyPosition);
        //        if (newDistance <= bestDistance) continue;
        //        bestDistance = newDistance;
        //        // Check occupancy
        //        //if (occupied[newPos] != Entity.Null && occupied[newPos] != entity) continue;

        //        bestOption = retreatDir + gridPosition.Position;
        //    }

        //    if (bestOption.x == -1)
        //    {
        //        queue.Dispose();
        //        searched.Dispose();
        //        return;
        //    }

        //    // Update the entity's path by removing the old path and appending new nodes in reverse order.
        //    ecb.RemoveComponent<PathNode>(sortKey, entity);
        //    ecb.AddBuffer<PathNode>(sortKey, entity);

        //    ecb.AppendToBuffer<PathNode>(sortKey, entity, new PathNode { PathPos = bestOption });
        //    //for (int i = path.Length - 2; i >= 1; i--)
        //    //{
        //    //    ecb.AppendToBuffer<PathNode>(sortKey, entity, new PathNode { PathPos = path[i] });
        //    //}

        //    // Reset pathData
        //    unitState.PathIndex = 0;
        //    unitState.Stuck = 0;
        //    unitState.Destination = enemyPosition;
        //    ecb.SetComponent(sortKey, entity, unitState);

        //    // Dispose of temporary native collections.
        //    queue.Dispose();
        //    searched.Dispose();
        //}
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

        //unstuck
        else if (unitState.Stuck == 1)
        {
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            Pathfinder.FindPath(gridPosition.Position, unitState.Destination, gridSize, isWalkable.GridArray, occupied.GridArray, false, ref path);
            
            //no other path exists
            if (path.Length == 0)
            {
                unitState.Stuck = 2;
                //maybe do
                //unitState.MovementState = 2;
                path.Dispose();
                return;
            }

            ecb.RemoveComponent<PathNode>(sortKey, entity);
            ecb.AddBuffer<PathNode>(sortKey, entity);

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
}
