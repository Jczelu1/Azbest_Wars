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
[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(PathfindSystem))]
public partial struct MovementStateSystem : ISystem
{
    public static byte SetMoveState = 255;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitStateData>();
    }
    public void OnStartRunning(ref SystemState state)
    {

    }
    public void OnUpdate(ref SystemState state)
    {
        if (SetMoveState != 255)
        {
            Debug.Log(MainGridScript.Instance.SetMoveState);
            SetMoveStateJob setJob = new SetMoveStateJob()
            {
                stateNum = MainGridScript.Instance.SetMoveState
            };
            state.Dependency = setJob.ScheduleParallel(state.Dependency);

            SetMoveState = 255;
        }
        
    }
}


[BurstCompile]
public partial struct SetMoveStateJob : IJobEntity
{
    public byte stateNum;
    public void Execute(Entity entity, ref UnitStateData unitState, ref SelectedData selected)
    {
        if (!selected.Selected) return;
        unitState.MovementState = stateNum;
    }
}
