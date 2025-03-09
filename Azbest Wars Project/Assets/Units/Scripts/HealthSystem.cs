using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
public partial struct HealthSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HealthData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        //var ecb = new EntityCommandBuffer(Allocator.TempJob).AsParallelWriter();

        //var job = new UpdateHealthBarJob
        //{
        //    ECB = ecb,
        //    //HealthbarTagLookup = state.GetComponentLookup<HealthbarTag>(true),
        //};

        //state.Dependency = job.ScheduleParallel(state.Dependency);

        //state.Dependency.Complete(); // Ensure all changes are applied
        //new EntityCommandBuffer(Allocator.TempJob).Playback(state.EntityManager);
    }

    [BurstCompile]
    public partial struct UpdateHealthBarJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        //[ReadOnly] public ComponentLookup<HealthbarTag> HealthbarTagLookup;

        public void Execute(Entity entity, in DynamicBuffer<Child> children, ref HealthData healthData, in LocalTransform localTransform, [ChunkIndexInQuery] int chunkIndex)
        {
            //if (!HealthLookup.TryGetComponent(entity, out var healthData)) return;
            healthData.Health = healthData.Health - 1;
            foreach (var child in children)
            {
                //if (!HealthbarTagLookup.HasComponent(child.Value)) continue;

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
