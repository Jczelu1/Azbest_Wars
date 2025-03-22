using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public int SpawnRate;
    class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SpawnerData
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                NextSpawnTime = 0,
                SpawnRate = authoring.SpawnRate
            });
        }
    }
}

public struct SpawnerData : IComponentData
{
    public Entity Prefab;
    public int NextSpawnTime;
    public int SpawnRate;
}
