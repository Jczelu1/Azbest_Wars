using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(PathfindSystem))]
[BurstCompile]
public partial class SelectSystem : SystemBase
{
    public static bool updateSelect = false;
    public static bool resetSelect = true;
    public static int unitsSelected = 0;
    public static int buildingsSelected = 0;
    public static bool spawnerSelected = false;
    public static Entity selectedEntity = Entity.Null;
    protected override void OnCreate()
    {
        RequireForUpdate<SelectedData>();
    }

    protected override void OnUpdate()
    {
        //reset select
        if (resetSelect)
        {
            unitsSelected = 0;
            buildingsSelected = 0;
            resetSelect = false;
            selectedEntity = Entity.Null;
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
        }

        //select
        if (updateSelect)
        {
            updateSelect = false;
            int2 selectStart = MainGridScript.Instance.SelectStartPosition;
            int2 selectEnd = MainGridScript.Instance.SelectEndPosition;
            int playerTeam = TeamManager.Instance.PlayerTeam;
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
                    if (!occupied.IsInGrid(pos)) continue;
                    Entity entity = occupied[pos];
                    if (entity != Entity.Null &&
                        SystemAPI.HasComponent<SelectedData>(entity) &&
                        SystemAPI.GetComponent<TeamData>(entity).Team == playerTeam)
                    {
                        SystemAPI.SetComponent(entity, new SelectedData { Selected = true });
                        unitsSelected++;
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
            if (unitsSelected != 0) return;
            Entities.WithoutBurst().ForEach((Entity entity, ref SelectedData selected, in DynamicBuffer<Child> children, in GridPosition gridPosition, in TeamData team) =>
            {
                //temporary
                if (!EntityManager.HasComponent<SpawnerData>(entity))
                {
                    return;
                }
                if (team.Team != playerTeam) return;
                int entityMinX = gridPosition.Position.x;
                int entityMinY = gridPosition.Position.y;
                int entityMaxX = entityMinX + gridPosition.Size.x - 1;
                int entityMaxY = entityMinY + gridPosition.Size.y - 1;

                bool isOverlapping = entityMinX <= maxX && entityMaxX >= minX &&
                                     entityMinY <= maxY && entityMaxY >= minY;
                if(isOverlapping)
                {
                    buildingsSelected++;
                    selected.Selected = true;
                    selectedEntity = entity;
                    foreach (var child in children)
                    {
                        if (EntityManager.HasComponent<SelectedTag>(child.Value))
                        {
                            var sr = EntityManager.GetComponentObject<SpriteRenderer>(child.Value);
                            sr.sortingLayerID = SortingLayer.NameToID("Unit");
                        }
                    }
                }
            }).Run();
        }
    }
    [UpdateInGroup(typeof(TickSystemGroup))]
    [UpdateAfter(typeof(SpawnerSystem))]
    [BurstCompile]
    public partial class UpdateSpawnerUISystem : SystemBase
    {
        protected override void OnUpdate()
        {
            spawnerSelected = false;
            if (SelectSystem.selectedEntity != Entity.Null)
            {
                if (SystemAPI.HasComponent<SpawnerData>(selectedEntity))
                {
                    SpawnerData spawner = SystemAPI.GetComponent<SpawnerData>(selectedEntity);
                    SpawnerInputController.Queued = spawner.Queued;
                    SpawnerInputController.UnitType = spawner.SpawnedUnit;
                    spawnerSelected = true;
                }
            }
        }
    }
}
