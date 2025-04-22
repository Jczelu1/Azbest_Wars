using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

class SpawnerAuthoring : MonoBehaviour
{
    public float SpawnRateMult;
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
                SpawnRateMult = authoring.SpawnRateMult,
                Queued = authoring.Queued,
                TimeToSpawn = 11,
                MaxTimeToSpawn = 0,
                SetFormation = -1,
                NextSpawnedUnit = -1,
                SpawnedThisTick = false
            });
        }
    }
}

public struct SpawnerData : IComponentData
{
    public int SpawnedUnit;
    public int NextSpawnedUnit;
    public int Queued;
    public int TimeToSpawn;
    public int MaxTimeToSpawn;
    public float SpawnRateMult;
    public int SetFormation;
    public bool SpawnedThisTick;
}
