using System;
using System.Diagnostics;
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
public partial struct MoveSystem : ISystem
{
    private BufferLookup<PathNode> _bufferLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitStateData>();
        state.RequireForUpdate<LocalTransform>();
        _bufferLookup = state.GetBufferLookup<PathNode>(true);
    }
    public void OnStartRunning(ref SystemState state)
    {
        
    }
    public void OnUpdate(ref SystemState state)
    {
        _bufferLookup.Update(ref state);
        //move job
        MoveJob moveJob = new MoveJob()
        {
            pathLookup = _bufferLookup,
            gridOrigin = MainGridScript.Instance.GridOrigin,
            cellSize = MainGridScript.Instance.CellSize,
            occupied = MainGridScript.Instance.Occupied
        };
        //not parallel because race conditions when moving to a tile
        state.Dependency = moveJob.Schedule(state.Dependency);
    }
}


[BurstCompile]
public partial struct MoveJob : IJobEntity
{
    [ReadOnly]
    public BufferLookup<PathNode> pathLookup;
    public float2 gridOrigin;
    public float cellSize;
    //this will cause bugs
    [NativeDisableParallelForRestriction]
    public FlatGrid<Entity> occupied;
    public void Execute(Entity entity, ref UnitStateData unitState, ref LocalTransform transform, ref GridPosition gridPosition)
    {
        if (!pathLookup.HasBuffer(entity))
            return;

        DynamicBuffer<PathNode> pathBuffer = pathLookup[entity];

        if (unitState.PathIndex >= 0 && unitState.PathIndex < pathBuffer.Length)
        {
            UnityEngine.Debug.Log("move");
            unitState.Moving = true;
            int2 targetCell = pathBuffer[unitState.PathIndex].PathPos;
            //UnityEngine.Debug.Log(cellRef);

            if (occupied[targetCell] == Entity.Null)
            {
                float2 targetPosition = new float2(
                    gridOrigin.x + cellSize * pathBuffer[unitState.PathIndex].PathPos.x,
                    gridOrigin.y + cellSize * pathBuffer[unitState.PathIndex].PathPos.y
                );
                transform.Position = new float3(targetPosition.x, targetPosition.y, transform.Position.z);
                occupied[gridPosition.Position] = Entity.Null;
                gridPosition.Position = pathBuffer[unitState.PathIndex].PathPos;
                occupied.SetValue(gridPosition.Position, entity);
                //Debug.Log(pathData.PathIndex);
                unitState.PathIndex++;
                unitState.Stuck = 0;
            }
            else
            {
                if(unitState.Stuck != 2)
                    unitState.Stuck = 1;
            }
        }
        else
        {
            unitState.Moving = false;
            occupied.SetValue(gridPosition.Position, entity);
        }
    }
}