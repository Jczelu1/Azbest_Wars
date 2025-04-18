using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;





[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MeleAttackSystem))]
[UpdateAfter(typeof(RangedAttackSystem))]
public partial class ArrowSystem : SystemBase
{
    public static NativeList<Entity> HitArrowPrefabs;
    public static NativeList<Entity> MissArrowPrefabs;
    public NativeList<Arrow> SpawnedArrows;

    protected override void OnCreate()
    {
        if (HitArrowPrefabs.IsCreated == false)
            HitArrowPrefabs = new NativeList<Entity>(Allocator.Persistent);
        if (MissArrowPrefabs.IsCreated == false)
            MissArrowPrefabs = new NativeList<Entity>(Allocator.Persistent);

        SpawnedArrows = new NativeList<Arrow>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        if (HitArrowPrefabs.IsCreated) HitArrowPrefabs.Dispose();
        if (MissArrowPrefabs.IsCreated) MissArrowPrefabs.Dispose();
        if (SpawnedArrows.IsCreated) SpawnedArrows.Dispose();
    }

    protected override void OnUpdate()
    {
        // Cache prefab counts
        int hitCount = HitArrowPrefabs.Length;
        int missCount = MissArrowPrefabs.Length;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        for (int i = SpawnedArrows.Length - 1; i >= 0; i--)
        {
            var arrow = SpawnedArrows[i];

            if (arrow.Miss)
            {
                // Choose a random miss prefab
                var prefab = missCount > 0 ?
                             MissArrowPrefabs[UnityEngine.Random.Range(0, missCount)] : Entity.Null;

                if (prefab != Entity.Null)
                {
                    // Instantiate and position at target
                    var instance = entityManager.Instantiate(prefab);
                    if (entityManager.HasComponent<GridPosition>(arrow.Target))
                    {
                        var targetgp = entityManager.GetComponentData<GridPosition>(arrow.Target);
                        int2 position = targetgp.Position;
                        var transform = entityManager.GetComponentData<LocalTransform>(instance);
                        transform.Position = MainGridScript.Instance.MainGrid.GetWorldPosition(position);
                        entityManager.SetComponentData(instance, transform);
                    }
                }
            }
            else
            {
                // Choose a random hit prefab
                var prefab = hitCount > 0 ?
                             HitArrowPrefabs[UnityEngine.Random.Range(0, hitCount)] : Entity.Null;

                if (prefab != Entity.Null)
                {
                    // Instantiate and parent to the target entity
                    var instance = entityManager.Instantiate(prefab);
                    entityManager.AddComponentData(instance, new Parent { Value = arrow.Target });
                    entityManager.SetComponentData(instance, new LocalTransform { Position = float3.zero, Rotation = quaternion.identity, Scale = 1f });
                }
            }
            // Remove processed arrow
            SpawnedArrows.RemoveAtSwapBack(i);
        }
    }
}
