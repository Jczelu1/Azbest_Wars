using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine.Rendering;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
[UpdateInGroup(typeof(TickSystemGroup))]
public partial struct SpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

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
                Entity newEntity = entityManager.Instantiate(spawner.ValueRO.Prefab);

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
