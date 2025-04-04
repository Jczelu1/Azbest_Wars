using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(PathfindSystem))]
public partial class ArtificialIdiot : SystemBase
{
    byte AITeam = 1;
    byte groupSize = 4;
    public static int2 moveToPosition;
    public static bool move;
    NativeList<Entity> captureAreas = new NativeList<Entity>(Allocator.Persistent);
    protected override void OnCreate()
    {

    }
    protected override void OnUpdate()
    {
        if(captureAreas.Length == 0)
        {
            Entities.WithoutBurst().ForEach((Entity entity, ref CaptureAreaData captureArea) =>
            {
                captureAreas.Add(entity);
                Debug.Log("adding area");
            }).Run();
        }
        var occupied = MainGridScript.Instance.Occupied;
        int2 groupPosition = new int2(-1, -1);
        bool grouped = false;
        Entities.WithoutBurst().ForEach((Entity entity, ref UnitStateData unitState, ref GridPosition gridPosition, ref TeamData team, ref SelectedData selected) =>
        {
            if (grouped) return;
            if (team.Team != AITeam) return;
            if (selected.Selected) return;
            if(unitState.Moved || unitState.Stuck != 0) return;
            selected.Selected = true;
            groupPosition = gridPosition.Position;
            Debug.Log("pos: "+ gridPosition.Position);
            for (int dx = -groupSize; dx <= groupSize; dx++)
            {
                for (int dy = -groupSize; dy <= groupSize; dy++)
                {
                    int2 pos = new int2 { x = dx + gridPosition.Position.x, y = dy + gridPosition.Position.y };
                    if (occupied.IsInGrid(pos))
                    {
                        if (occupied[pos] != Entity.Null)
                        {
                            Entity foundEntity = occupied[pos];
                            if (EntityManager.GetComponentData<TeamData>(foundEntity).Team != AITeam)
                                continue;
                            EntityManager.SetComponentData<SelectedData>(foundEntity, selected);
                            Debug.Log("add entities");
                        }
                    }
                }
            }
            float minDistance = float.MaxValue;
            int2 moveToPos = new int2(-1, -1);
            foreach(Entity e in captureAreas)
            {
                if (EntityManager.GetComponentData<TeamData>(e).Team == AITeam) continue;
                int2 pos = EntityManager.GetComponentData<GridPosition>(e).Position;
                pos.y -= 1;
                float distance = math.distancesq(pos, groupPosition);
                if(distance < minDistance)
                {
                    minDistance = distance;
                    moveToPos = pos;
                }
            }
            Debug.Log("chosen: " + moveToPos);
            if (moveToPos.x == -1) return;
            PathfindSystem.shouldMove[AITeam] = true;
            PathfindSystem.destinations[AITeam] = moveToPos;
            grouped = true;
        }).Run();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        captureAreas.Dispose();
    }
}
