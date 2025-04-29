using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

[UpdateInGroup(typeof(SubTickSystemGroup))]
[UpdateBefore(typeof(SubTickManagerSystem))]
public partial class ArrowSystem : SystemBase
{
    public static NativeList<Entity> SpawnedArrows;
    public static NativeList<Entity> hitPrefabs;
    public static NativeList<Entity> missPrefabs;
    public static bool started = false;

    protected override void OnCreate()
    {
        RequireForUpdate<RangedAttackData>();
        RequireForUpdate<ArrowPrefabBuffer>();
        SpawnedArrows = new NativeList<Entity>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        if(SpawnedArrows.IsCreated) SpawnedArrows.Dispose();
        if(hitPrefabs.IsCreated) hitPrefabs.Dispose();
        if(missPrefabs.IsCreated) missPrefabs.Dispose();
    }

    protected override void OnUpdate()
    {
        if (SetupSystem.startDelay != -1) return;
        if (!started)
        {
            started = true;
            Entity queryEntity = SystemAPI.GetSingletonEntity<ArrowPrefabBuffer>();
            var buffer = base.EntityManager.GetBuffer<ArrowPrefabBuffer>(queryEntity);
            // Cache prefab counts
            if (hitPrefabs.IsCreated) hitPrefabs.Dispose();
            if (missPrefabs.IsCreated) missPrefabs.Dispose();
            hitPrefabs = new NativeList<Entity>(Allocator.Persistent);
            missPrefabs = new NativeList<Entity>(Allocator.Persistent);
            foreach (var prefab in buffer)
            {
                if (prefab.IsHit) hitPrefabs.Add(prefab.Prefab);
                else missPrefabs.Add(prefab.Prefab);
            }
        }
        if(SubTickSystemGroup.subTickNumber == 1)
        {
            for (int i = RangedAttackSystem.SpawnArrows.Length - 1; i >= 0; i--)
            {
                var arrow = RangedAttackSystem.SpawnArrows[i];
                if (!EntityManager.HasComponent<GridPosition>(arrow.Target)) continue;
                if (arrow.Miss)
                {
                    // Choose a random miss prefab
                    var prefab = missPrefabs.Length > 0 ?
                                 missPrefabs[UnityEngine.Random.Range(0, missPrefabs.Length)] : Entity.Null;

                    if (prefab != Entity.Null)
                    {
                        // Instantiate and position at target
                        var instance = EntityManager.Instantiate(prefab);
                        var targetgp = EntityManager.GetComponentData<GridPosition>(arrow.Target);
                        int2 position = targetgp.Position;
                        var transform = EntityManager.GetComponentData<LocalTransform>(instance);
                        transform.Position = MainGridScript.Instance.MainGrid.GetWorldPosition(position);
                        EntityManager.SetComponentData(instance, transform);
                        EntityManager.GetComponentObject<SpriteRenderer>(instance).color = TeamManager.Instance.GetTeamColor(arrow.Team);

                        SpawnedArrows.Add(instance);
                    }
                }
                else
                {
                    // Choose a random hit prefab
                    var prefab = hitPrefabs.Length > 0 ?
                                 hitPrefabs[UnityEngine.Random.Range(0, hitPrefabs.Length)] : Entity.Null;

                    if (prefab != Entity.Null)
                    {
                        // Instantiate and parent to the target entity
                        var instance = EntityManager.Instantiate(prefab);
                        EntityManager.AddComponentData(instance, new Parent { Value = arrow.Target });
                        EntityManager.SetComponentData(instance, new LocalTransform { Position = float3.zero, Rotation = quaternion.identity, Scale = 1f });
                        EntityManager.GetComponentObject<SpriteRenderer>(instance).color = TeamManager.Instance.GetTeamColor(arrow.Team);
                        SpawnedArrows.Add(instance);                    
                    }
                }
                // Remove processed arrow
                //SpawnedArrows.RemoveAtSwapBack(i);
            }
        }
        
    }
}
