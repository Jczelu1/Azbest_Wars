using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine.Rendering;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Linq;

[BurstCompile]
[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MoveSystem))]
public partial struct SpawnerSystem : ISystem
{
    const int Max_Spawn_Range = 3;
    public static NativeList<UnitTypeData> unitTypes;
    bool started;
    public void OnCreate(ref SystemState state) 
    {
        started = false;
        unitTypes = new NativeList<UnitTypeData>(Allocator.Persistent);
    }

    public void OnDestroy(ref SystemState state)
    {
        unitTypes.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        float2 gridOrigin = MainGridScript.Instance.GridOrigin;
        float cellSize = MainGridScript.Instance.CellSize;
        FlatGrid<Entity> occupied = MainGridScript.Instance.Occupied;
        FlatGrid<bool> isWalkable = MainGridScript.Instance.IsWalkable;

        var entityManager = state.EntityManager;
        if (!started)
        {
            started = true;
            foreach (var unitType in SystemAPI.Query<UnitTypeData>())
            {
                while(unitTypes.Length <= unitType.Id)
                {
                    unitTypes.Add(unitType);
                }
                unitTypes[unitType.Id] = unitType;
            }
        }
        // Iterate over all spawner entities.
        foreach (var (spawner, gridPosition, teamData, entity) in SystemAPI.Query<RefRW<SpawnerData>, GridPosition, TeamData>().WithEntityAccess())
        {
            byte team = teamData.Team;
            if (entityManager.GetComponentData<TeamData>(entity).Team > 3) continue;
            int unitId = spawner.ValueRO.SpawnedUnit;
            //type exists
            if (unitTypes.Length <= unitId) continue;
            UnitTypeData unitType = unitTypes[unitId];
            //start producing new unit
            if(spawner.ValueRO.MaxTimeToSpawn == 0)
            {
                if(spawner.ValueRO.Queued == 0)
                {
                    continue;
                }
                //can afford
                if (TeamManager.Instance.teamResources[team] < unitType.Cost)
                    continue;
                //cost
                TeamManager.Instance.teamResources[team] -= unitType.Cost;
                spawner.ValueRW.MaxTimeToSpawn = unitType.TimeToSpawn;
                spawner.ValueRW.TimeToSpawn = unitType.TimeToSpawn;
            }
            //spawn new unit
            if (spawner.ValueRO.TimeToSpawn <= 0)
            {
                int maxNodes = (Max_Spawn_Range * 2 + 1) * (Max_Spawn_Range * 2 + 1);
                int2 startPos = gridPosition.Position;
                startPos.y -= 1;
                NativeList<int2> queue = new NativeList<int2>(maxNodes, Allocator.Temp);
                NativeHashSet<int2> searched = new NativeHashSet<int2>(maxNodes, Allocator.Temp);

                // Initialize the search with the starting position.
                queue.Add(startPos);
                searched.Add(startPos);

                int2 newPosition = new int2(-1, -1);
                bool foundPosition = false;
                int head = 0; // Use a head pointer for FIFO behavior

                // Breadth-first search loop.
                while (head < queue.Length && !foundPosition)
                {
                    int2 current = queue[head];
                    head++;
                    // Skip if outside grid bounds.
                    if (!occupied.IsInGrid(current))
                        continue;
                    //unwalkable
                    if (!isWalkable[current])
                        continue;


                    if (occupied[current] == Entity.Null)
                    {
                        newPosition = current;
                        foundPosition = true;
                        break;
                    }
                    for (int i = 0; i < Pathfinder.directions.Length; i++)
                    {
                        int2 offset = Pathfinder.directions[i];
                        int2 neighbor = current + offset;

                        // Skip if already visited.
                        if (searched.Contains(neighbor))
                            continue;
                        // Ensure neighbor is within the auto-find search radius.
                        if (math.abs(neighbor.x - startPos.x) > Max_Spawn_Range ||
                            math.abs(neighbor.y - startPos.y) > Max_Spawn_Range ||
                            neighbor.y > startPos.y)
                            continue;
                        // Enqueue valid neighbor.
                        queue.Add(neighbor);
                        searched.Add(neighbor);
                    }
                }

                // Check if the new position is valid.
                if (newPosition.x == -1)
                {
                    continue;
                }
                
                // Instantiate the prefab directly using the EntityManager.
                Entity newEntity = entityManager.Instantiate(unitType.Prefab);

                // Calculate the target world position.
                float2 targetPosition = new float2(
                    gridOrigin.x + cellSize * newPosition.x,
                    gridOrigin.y + cellSize * newPosition.y
                );

                // Set the position component directly.
                entityManager.SetComponentData(
                    newEntity,
                    LocalTransform.FromPosition(new float3(targetPosition.x, targetPosition.y, 0))
                );
                GridPosition newGridPosition = entityManager.GetComponentData<GridPosition>(newEntity);
                newGridPosition.Position.x = newPosition.x;
                newGridPosition.Position.y = newPosition.y;
                entityManager.SetComponentData(
                    newEntity,
                    newGridPosition
                );
                entityManager.SetComponentData(
                    newEntity,
                    teamData
                );
                entityManager.SetComponentData(
                    newEntity,
                    new FormationData { Formation = spawner.ValueRO.SetFormation}
                );

                //set team color
                entityManager.GetComponentObject<SpriteRenderer>(newEntity).color = TeamManager.Instance.GetTeamColor(team);
                //disable select sprite

                //testing only
                //UnitStateData unitState = entityManager.GetComponentData<UnitStateData>(newEntity);
                //unitState.MovementState = 1;
                //entityManager.SetComponentData(
                //    newEntity,
                //    unitState
                //);

                occupied[newPosition] = newEntity;
                spawner.ValueRW.MaxTimeToSpawn = 0;
                spawner.ValueRW.Queued--;
            }
            //decrement counter
            else
            {
                spawner.ValueRW.TimeToSpawn--;
            }
        }
    }

}
