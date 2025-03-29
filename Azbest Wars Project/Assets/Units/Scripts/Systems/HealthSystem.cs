using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(MeleAttackSystem))]
[UpdateBefore(typeof(PathfindSystem))]
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

        JobHandle jobHandle = job.Schedule(state.Dependency);
        state.Dependency = jobHandle;
        jobHandle.Complete();
        ecbSystem.Update(state.WorldUnmanaged);
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
  
}
