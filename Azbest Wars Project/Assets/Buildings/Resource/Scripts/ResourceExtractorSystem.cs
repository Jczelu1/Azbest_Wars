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
    }

    public void OnUpdate(ref SystemState state)
    {

        var job = new ExtractJob
        {
            resources = TeamManager.Instance.teamResources
        };

        JobHandle jobHandle = job.ScheduleParallel(state.Dependency);
        state.Dependency = jobHandle;
    }
    [BurstCompile]
    public partial struct ExtractJob : IJobEntity
    {
        //[NativeDisableParallelForRestriction]
        public NativeArray<int> resources;
        public void Execute(ref TeamData team, ref ResourceSourceData source)
        {
            if (team.Team > 3) return;
            resources[team.Team] += source.ResourcePerTick;
        }
    }

}