using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

class SpawnerAuthoring : MonoBehaviour
{
    public int SpawnRate;
    public int SpawnedUnit;
    public int Queued;
    
    class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SpawnerData
            {
                SpawnedUnit = authoring.SpawnedUnit,
                NextSpawnTime = authoring.SpawnRate,
                SpawnRate = authoring.SpawnRate,
                Queued = authoring.Queued
            });
        }
    }
}

public struct SpawnerData : IComponentData
{
    public int SpawnedUnit;
    public int Queued;
    public int NextSpawnTime;
    public int SpawnRate;
}
