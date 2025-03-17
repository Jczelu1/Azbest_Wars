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
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(MoveSystem))]
public partial struct PathfindSystem : ISystem
{
    private ComponentLookup<TeamData> _teamLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitStateData>();
        state.RequireForUpdate<LocalTransform>();
        _teamLookup = state.GetComponentLookup<TeamData>(true);
    }
    public void OnStartRunning(ref SystemState state)
    {

    }
    public void OnUpdate(ref SystemState state)
    {
        _teamLookup.Update(ref state);

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var ecbSystem = World.DefaultGameObjectInjectionWorld
            .GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();


        //pathfind job
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        PathfindJob pathfindJob = new PathfindJob()
        {
            rightClickPos = MainGridScript.Instance.RightClickPosition,
            gridSize = new int2(MainGridScript.Instance.Width, MainGridScript.Instance.Height),
            occupied = MainGridScript.Instance.Occupied,
            isWalkable = MainGridScript.Instance.IsWalkable,
            ecb = ecb,
            rightClick = MainGridScript.Instance.RightClick,
            teamLookup = _teamLookup
        };

        JobHandle pathJobHandle = pathfindJob.ScheduleParallel(state.Dependency);

        state.Dependency = pathJobHandle;
        pathJobHandle.Complete();
        //Ensure ECB commands are played back safely
        ecbSystem.Update(state.WorldUnmanaged);
        stopwatch.Stop();
        //UnityEngine.Debug.Log($"Pathfinding took {stopwatch.Elapsed}");
    }
}

[BurstCompile]
public partial struct PathfindJob : IJobEntity
{
    public bool rightClick;
    public int2 rightClickPos;
    public int2 gridSize;
    [ReadOnly]
    public FlatGrid<Entity> occupied;
    [ReadOnly]
    public FlatGrid<bool> isWalkable;

    public EntityCommandBuffer.ParallelWriter ecb;

    [ReadOnly]
    public ComponentLookup<TeamData> teamLookup;
    const int autoFindEnemyDistance = 8;

    public void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref UnitStateData unitState, ref GridPosition gridPosition, ref SelectedData selected)
    {
        //temporary
        int team = teamLookup[entity].Team;
        if (team != 1)
            return;

        //pathfind to selected location
        if (rightClick && selected.Selected)
        {
            UnityEngine.Debug.Log("pathfind");

            //maybe do this???
            unitState.MovementState = 0;

            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            Pathfinder.FindPath(gridPosition.Position, rightClickPos, gridSize, isWalkable.GridArray, occupied.GridArray, true, ref path);

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
            newPathData.Destination = rightClickPos;
            ecb.SetComponent(sortKey, entity, newPathData);

            path.Dispose();
        }


        //pathfind to enemy
        else if (unitState.MovementState == 1)
        {
            UnityEngine.Debug.Log("attack");
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
        else if (unitState.MovementState == 2)
        {
            UnityEngine.Debug.Log("retreat");
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
            int2 enemyDirection = math.sign(new int2(gridPosition.Position.x - enemyPosition.x, gridPosition.Position.y - enemyPosition.y));
            int2 bestOption = new int2(-1, -1);
            float bestDistance = math.lengthsq(gridPosition.Position - enemyPosition) + .1f;
            for (int i = 0; i < 8; i++)
            {
                int2 retreatDir = Pathfinder.directions[i];
                int2 newPos = gridPosition.Position + retreatDir;

                // Validate position
                if (!occupied.IsInGrid(newPos)) continue;
                if (!isWalkable[newPos]) continue;

                // Check if moving away from enemy
                float newDistance = math.lengthsq(newPos - enemyPosition);
                if (newDistance <= bestDistance) continue;
                bestDistance = newDistance;
                // Check occupancy
                //if (occupied[newPos] != Entity.Null && occupied[newPos] != entity) continue;

                bestOption = retreatDir + gridPosition.Position;
            }

            if (bestOption.x == -1)
            {
                queue.Dispose();
                searched.Dispose();
                return;
            }

            // Update the entity's path by removing the old path and appending new nodes in reverse order.
            ecb.RemoveComponent<PathNode>(sortKey, entity);
            ecb.AddBuffer<PathNode>(sortKey, entity);

            ecb.AppendToBuffer<PathNode>(sortKey, entity, new PathNode { PathPos = bestOption });
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
        }
        //stop
        else if(unitState.MovementState == 3)
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
}
