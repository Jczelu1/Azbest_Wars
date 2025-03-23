using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine.Rendering;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MoveSystem))]
public partial struct SpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state) 
    {

    }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        float2 gridOrigin = MainGridScript.Instance.GridOrigin;
        float cellSize = MainGridScript.Instance.CellSize;
        FlatGrid<Entity> occupied = MainGridScript.Instance.Occupied;
        FlatGrid<bool> isWalkable = MainGridScript.Instance.IsWalkable;

        var entityManager = state.EntityManager;

        // Iterate over all spawner entities.
        foreach (var (spawner, gridPosition, entity) in SystemAPI.Query<RefRW<SpawnerData>, GridPosition>().WithEntityAccess())
        {
            if (spawner.ValueRO.NextSpawnTime == 0)
            {
                // Calculate a new grid position one cell down.
                int2 newPosition = gridPosition.Position;
                newPosition.y -= 1;

                // Check if the new position is valid.
                if (!isWalkable[newPosition] || occupied[newPosition] != Entity.Null)
                {
                    continue;
                }
                // Instantiate the prefab directly using the EntityManager.
                Entity newEntity = entityManager.Instantiate(spawner.ValueRW.Prefab);

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
                TeamData team = entityManager.GetComponentData<TeamData>(entity);
                entityManager.SetComponentData(
                    newEntity,
                    team
                );

                //set team color
                entityManager.GetComponentObject<SpriteRenderer>(newEntity).color = TeamColors.Instance.teamColors[team.Team];
                //disable select sprite

                //testing only
                UnitStateData unitState = entityManager.GetComponentData<UnitStateData>(newEntity);
                unitState.MovementState = 1;
                entityManager.SetComponentData(
                    newEntity,
                    unitState
                );

                occupied[newPosition] = newEntity;

                // Reset the spawn timer.
                spawner.ValueRW.NextSpawnTime = spawner.ValueRO.SpawnRate;
            }
            else
            {
                spawner.ValueRW.NextSpawnTime -= 1;
            }
        }
    }

}
