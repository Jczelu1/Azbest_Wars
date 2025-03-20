using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MeleAttackSystem))]
public partial struct HealthSystem : ISystem
{
    public ComponentLookup<HealthbarTag> healthbarLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HealthData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var ecbSystem = World.DefaultGameObjectInjectionWorld
            .GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        var job = new DieJob
        {
            ecb = ecb,
            occupied = MainGridScript.Instance.Occupied
        };

        state.Dependency = job.Schedule(state.Dependency);
    }
    [BurstCompile]
    public partial struct DieJob : IJobEntity
    {
        public EntityCommandBuffer ecb;

        public FlatGrid<Entity> occupied;
        public void Execute(Entity entity, ref HealthData healthData, ref GridPosition gridPosition, in DynamicBuffer<Child> children)
        {
            if(healthData.Health <= 0)
            {
                ecb.DestroyEntity(entity);
                occupied[gridPosition.Position] = Entity.Null;
                foreach (var child in children)
                {
                    ecb.DestroyEntity(child.Value);
                }
            }
        }
    }
    [BurstCompile]
    public partial struct UpdateHealthBarJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        //[ReadOnly] public ComponentLookup<HealthbarTag> HealthbarTagLookup;

        public void Execute(Entity entity, in DynamicBuffer<Child> children, ref HealthData healthData, in LocalTransform localTransform, [ChunkIndexInQuery] int chunkIndex, ComponentLookup<HealthbarTag> healthbarLookup)
        {
            //if (!HealthLookup.TryGetComponent(entity, out var healthData)) return;
            foreach (var child in children)
            {
                if(!healthbarLookup.HasComponent(child.Value)) continue;

                float healthPercentage = math.clamp(healthData.Health / healthData.MaxValue, 0f, 1f);
                //new float3(math.clamp(healthData.Value / healthData.MaxValue, 0f, 1f), 1f, 1f)
                ECB.SetComponent(chunkIndex, child.Value, new LocalTransform
                {
                    Scale = healthPercentage
                });
                
            }
        }
    }
}
