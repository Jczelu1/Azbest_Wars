using System;
using System.Linq;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SubTickSystemGroup))]
[UpdateBefore(typeof(SubTickManagerSystem))]
public partial struct SmoothMoveSystem : ISystem
{
    public static bool enabled = true;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitStateData>();
        state.RequireForUpdate<LocalTransform>();
    }
    public void OnUpdate(ref SystemState state)
    {
        if (!enabled) return;
        //move job
        SmoothMoveJob moveJob = new SmoothMoveJob()
        {
            gridOrigin = MainGridScript.Instance.GridOrigin,
            cellSize = MainGridScript.Instance.CellSize,
            subTickNumber = SubTickSystemGroup.subTickNumber
        };
        state.Dependency = moveJob.ScheduleParallel(state.Dependency);
    }
}


[BurstCompile]
public partial struct SmoothMoveJob : IJobEntity
{
    public float2 gridOrigin;
    public float cellSize;
    public int subTickNumber;
    public void Execute(Entity entity, ref UnitStateData unitState, ref LocalTransform transform, ref GridPosition gridPosition)
    {
        if (gridPosition.Position.x == -1) return;
        if (subTickNumber > 3) return;
        // Calculate the target position based on grid position
        float2 targetPosition = new float2(
            gridOrigin.x + cellSize * gridPosition.Position.x,
            gridOrigin.y + cellSize * gridPosition.Position.y
        );
        if (subTickNumber == 3)
        {
            transform.Position.x = targetPosition.x;
            transform.Position.y = targetPosition.y;
            return;
        }
        float2 currentPosition = transform.Position.xy;

        float2 toTarget = targetPosition - currentPosition;
        transform.Position.x += math.sign(toTarget.x) * cellSize / 4;
        transform.Position.y += math.sign(toTarget.y) * cellSize / 4;
        
        
    }
}
