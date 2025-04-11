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
    private BufferLookup<PathNode> _pathLookup;
    private ComponentLookup<UnitStateData> _unitStateLookup;
    private ComponentLookup<LocalTransform> _transformLookup;
    private ComponentLookup<GridPosition> _gridPositionLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitStateData>();
        state.RequireForUpdate<LocalTransform>();

        _pathLookup = state.GetBufferLookup<PathNode>(true);
        _unitStateLookup = state.GetComponentLookup<UnitStateData>(false);
        _transformLookup = state.GetComponentLookup<LocalTransform>(false);
        _gridPositionLookup = state.GetComponentLookup<GridPosition>(false);
    }

    public void OnStartRunning(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        _pathLookup.Update(ref state);
        _unitStateLookup.Update(ref state);
        _transformLookup.Update(ref state);
        _gridPositionLookup.Update(ref state);

        MoveJob moveJob = new MoveJob()
        {
            pathLookup = _pathLookup,
            unitStateLookup = _unitStateLookup,
            transformLookup = _transformLookup,
            gridPositionLookup = _gridPositionLookup,
            gridOrigin = MainGridScript.Instance.GridOrigin,
            cellSize = MainGridScript.Instance.CellSize,
            occupied = MainGridScript.Instance.Occupied,
            moveTransform = !SubTickSystemGroup.subTickEnabled
        };
        //not parallel because race conditions when moving to a tile
        state.Dependency = moveJob.Schedule(state.Dependency);
    }
    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        [ReadOnly]
        public BufferLookup<PathNode> pathLookup;

        public ComponentLookup<UnitStateData> unitStateLookup;
        public ComponentLookup<LocalTransform> transformLookup;
        public ComponentLookup<GridPosition> gridPositionLookup;

        public float2 gridOrigin;
        public float cellSize;
        public bool moveTransform;

        [NativeDisableParallelForRestriction]
        public FlatGrid<Entity> occupied;

        public void Execute(Entity entity)
        {
            if (!pathLookup.HasBuffer(entity))
                return;

            Move(entity);
        }

        private bool Move(Entity entity)
        {
            var unitState = unitStateLookup[entity];
            if (unitState.MoveProcessed) return false;
            unitState.MoveProcessed = true;
            unitStateLookup[entity] = unitState;
            var transform = transformLookup[entity];
            var gridPosition = gridPositionLookup[entity];
            var pathBuffer = pathLookup[entity];

            unitState.Moved = false;

            if (unitState.PathIndex >= 0 && unitState.PathIndex < pathBuffer.Length)
            {
                int2 targetCell = pathBuffer[unitState.PathIndex].PathPos;

                bool canMove = true;
                if (occupied[targetCell] != Entity.Null)
                {
                    canMove = Move(occupied[targetCell]);
                }

                if(canMove){
                    // Handle rotation
                    if (targetCell.x - gridPosition.Position.x < 0)
                        transform.Rotation = Quaternion.Euler(0, 0, 0);
                    else if (targetCell.x - gridPosition.Position.x > 0)
                        transform.Rotation = Quaternion.Euler(0, 180, 0);
                    else if (targetCell.y - gridPosition.Position.y < 0)
                        transform.Rotation = Quaternion.Euler(0, 180, 0);
                    else
                        transform.Rotation = Quaternion.Euler(0, 0, 0);

                    // Handle movement
                    if (moveTransform)
                    {
                        float2 targetPosition = new float2(
                            gridOrigin.x + cellSize * targetCell.x,
                            gridOrigin.y + cellSize * targetCell.y
                        );
                        transform.Position = new float3(targetPosition.x, targetPosition.y, transform.Position.z);
                    }

                    // Update occupied grid and position
                    occupied[gridPosition.Position] = Entity.Null;
                    gridPosition.Position = targetCell;
                    occupied.SetValue(gridPosition.Position, entity);

                    unitState.PathIndex++;
                    unitState.Stuck = 0;
                    unitState.Moved = true;
                    unitStateLookup[entity] = unitState;
                    transformLookup[entity] = transform;
                    gridPositionLookup[entity] = gridPosition;
                    return true;
                }
                else
                {
                    if (unitState.Stuck != 2)
                        unitState.Stuck = 1;
                    unitStateLookup[entity] = unitState;
                    transformLookup[entity] = transform;
                    gridPositionLookup[entity] = gridPosition;
                    return false;
                }
            }
            else
            {
                occupied.SetValue(gridPosition.Position, entity);
                unitStateLookup[entity] = unitState;
                transformLookup[entity] = transform;
                gridPositionLookup[entity] = gridPosition;
                return false;
            }
        }
    }
}

