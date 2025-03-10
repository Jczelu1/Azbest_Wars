using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(MovementStateSystem))]
[BurstCompile]
public partial struct SelectSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SelectedData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        bool updateSelected = MainGridScript.Instance.UpdateSelected;
        if (!updateSelected)
            return;

        MainGridScript.Instance.UpdateSelected = false;


        int2 selectStart = MainGridScript.Instance.SelectStartPosition;
        int2 selectEnd = MainGridScript.Instance.SelectEndPosition;
        int playerTeam = MainGridScript.Instance.PlayerTeam;

        var occupied = MainGridScript.Instance.Occupied;

        int minX = math.min(selectStart.x, selectEnd.x);
        int maxX = math.max(selectStart.x, selectEnd.x);
        int minY = math.min(selectStart.y, selectEnd.y);
        int maxY = math.max(selectStart.y, selectEnd.y);

        JobHandle resetJobHandle = new ResetSelectJob().ScheduleParallel(state.Dependency);
        state.Dependency = resetJobHandle;
        resetJobHandle.Complete();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int2 pos = new int2(x, y);
                Entity entity = occupied[pos];
                if (entity != Entity.Null &&
                    SystemAPI.HasComponent<SelectedData>(entity) &&
                    SystemAPI.GetComponent<TeamData>(entity).Team == playerTeam)
                {
                    SystemAPI.SetComponent(entity, new SelectedData { Selected = true });
                }
            }
        }
    }

    [BurstCompile]
    public partial struct ResetSelectJob : IJobEntity
    {
        public void Execute(Entity entity, ref SelectedData selected)
        {
            selected.Selected = false;
        }
    }
}