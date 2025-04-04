using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(MovementStateSystem))]
[BurstCompile]
public partial class SelectSystem : SystemBase
{
    public static bool updateSelect = true;
    protected override void OnCreate()
    {
        RequireForUpdate<SelectedData>();
    }

    protected override void OnUpdate()
    {
        if (!updateSelect)
            return;
        updateSelect = false;

        Entities.WithoutBurst().ForEach((Entity entity, ref SelectedData selected, in DynamicBuffer<Child> children) =>
        {
            foreach (var child in children)
            {
                selected.Selected = false;
                if (SystemAPI.HasComponent<SelectedTag>(child.Value))
                {
                    var sr = EntityManager.GetComponentObject<SpriteRenderer>(child.Value);
                    sr.sortingLayerID = SortingLayer.NameToID("Hidden");
                }
            }
        }).Run();

        int2 selectStart = MainGridScript.Instance.SelectStartPosition;
        int2 selectEnd = MainGridScript.Instance.SelectEndPosition;
        int playerTeam = MainGridScript.Instance.PlayerTeam;
        var occupied = MainGridScript.Instance.Occupied;

        int minX = math.min(selectStart.x, selectEnd.x);
        int maxX = math.max(selectStart.x, selectEnd.x);
        int minY = math.min(selectStart.y, selectEnd.y);
        int maxY = math.max(selectStart.y, selectEnd.y);

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

                    if (SystemAPI.HasBuffer<Child>(entity))
                    {
                        var children = SystemAPI.GetBuffer<Child>(entity);
                        foreach (var child in children)
                        {
                            if (SystemAPI.HasComponent<SelectedTag>(child.Value))
                            {
                                var sr = EntityManager.GetComponentObject<SpriteRenderer>(child.Value);
                                sr.sortingLayerID = SortingLayer.NameToID("Unit");
                            }
                        }
                    }
                }
            }
        }
        
    }
}
