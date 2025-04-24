using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[BurstCompile]
public partial struct ResourceExtractorSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ResourceSourceData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        for(int i = 0; i < 4; i++)
        {
            TeamManager.Instance.teamResourceGains[i] = 0;
        }
        var job = new ExtractJob
        {
            resources = TeamManager.Instance.teamResources,
            resourceGains = TeamManager.Instance.teamResourceGains
        };

        JobHandle jobHandle = job.Schedule(state.Dependency);
        state.Dependency = jobHandle;
        jobHandle.Complete();
    }
    [BurstCompile]
    public partial struct ExtractJob : IJobEntity
    {
        //[NativeDisableParallelForRestriction]
        public NativeArray<int> resources;
        public NativeArray<int> resourceGains;
        public void Execute(in TeamData team, in ResourceSourceData source)
        {
            if (team.Team > 3) return;
            resources[team.Team] += source.ResourcePerTick;
            resourceGains[team.Team] += source.ResourcePerTick;
        }
    }

}