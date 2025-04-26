using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine.Rendering;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[BurstCompile]
[UpdateInGroup(typeof(TickSystemGroup))]
public partial struct BuildingTypeSystem : ISystem
{
    public static bool started;
    public static List<DescriptionData> buildingTypesDescription;
    public void OnCreate(ref SystemState state)
    {
        started = false;
        state.RequireForUpdate<BuildingIdData>();
        buildingTypesDescription = new List<DescriptionData>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SetupSystem.startDelay != -1) return;
        if (!started)
        {
            var EntityManager = state.EntityManager;
            started = true;
            foreach (var (buildingId, description) in SystemAPI.Query<BuildingIdData, DescriptionData>())
            {
                while (buildingTypesDescription.Count <= buildingId.Id)
                {
                    buildingTypesDescription.Add(description);
                }
                buildingTypesDescription[buildingId.Id] = description;
            }
        }
    }

}

