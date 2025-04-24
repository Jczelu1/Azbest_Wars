using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(PathfindSystem))]
[BurstCompile]
public partial class SelectSystem : SystemBase
{
    public static bool updateSelect = false;
    public static bool resetSelect = true;
    public static int unitsSelected = 0;
    public static int unitTypeSelected = -2;
    public static byte movementStateSelected = 255;
    public static int buildingTypeSelected = -2;
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
            DescriptionController.closeDescription();
            unitsSelected = 0;
            buildingsSelected = 0;
            resetSelect = false;
            selectedEntity = Entity.Null;
            spawnerSelected = false;
            SpawnerInputController.UIClosedByPlayer = false;
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
            unitTypeSelected = -2;
            buildingTypeSelected = -2;
            movementStateSelected = 255;

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
                        byte movementState = SystemAPI.GetComponent<UnitStateData>(entity).MovementState;
                        if (movementStateSelected == 255)
                        {
                            movementStateSelected = movementState;
                        }
                        else if (movementStateSelected != movementState)
                        {
                            movementStateSelected = 254;
                        }
                        int unitTypeId = SystemAPI.GetComponent<UnitTypeId>(entity).Id;
                        if (unitTypeSelected == -2)
                        {
                            unitTypeSelected = unitTypeId;
                        }
                        else if(unitTypeSelected != unitTypeId)
                        {
                            unitTypeSelected = -1;
                        }
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
            UnitStateInput.currentMovementState = movementStateSelected;
            if (unitTypeSelected > -1)
            {
                DescriptionController.showDescription(unitTypeSelected, false);
            }
            else
            {
                DescriptionController.closeDescription();
            }
            if (unitsSelected != 0) return;
            Entities.WithoutBurst().ForEach((Entity entity, ref SelectedData selected, in DynamicBuffer<Child> children, in GridPosition gridPosition, in TeamData team, in BuildingIdData buildingId) =>
            {
                //temporary
                //if (!EntityManager.HasComponent<SpawnerData>(entity))
                //{
                //    return;
                //}
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
                    if(buildingTypeSelected == -2)
                    {
                        buildingTypeSelected = buildingId.Id;
                        if(buildingsSelected == 1)
                        {
                            spawnerSelected = buildingId.IsSpawner;
                        }
                        else
                        {
                            spawnerSelected = false;
                        }
                    }
                    else if(buildingTypeSelected != buildingId.Id)
                    {
                        spawnerSelected = false;
                        buildingTypeSelected = -1;
                    }
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
            if (buildingTypeSelected > -1)
            {
                DescriptionController.showDescription(buildingTypeSelected, true);
            }
            else
            {
                DescriptionController.closeDescription();
            }
        }
    }
    [UpdateInGroup(typeof(TickSystemGroup))]
    [UpdateAfter(typeof(SpawnerSystem))]
    [BurstCompile]
    public partial class UpdateSpawnerUISystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (SelectSystem.selectedEntity != Entity.Null && buildingsSelected == 1)
            {
                if (SystemAPI.HasComponent<SpawnerData>(selectedEntity))
                {
                    if(SystemAPI.GetComponent<TeamData>(selectedEntity).Team != TeamManager.Instance.PlayerTeam)
                    {
                        spawnerSelected = false;
                        SelectSystem.selectedEntity = Entity.Null;
                        return;
                    }
                    if (spawnerSelected)
                    {
                        SpawnerData spawner = SystemAPI.GetComponent<SpawnerData>(selectedEntity);
                        SpawnerInputController.Queued = spawner.Queued;
                        SpawnerInputController.UnitType = spawner.NextSpawnedUnit == -1 ? spawner.SpawnedUnit : spawner.NextSpawnedUnit;
                        if(spawner.MaxTimeToSpawn == 0)
                            SpawnerInputController.ProductionProgress = 1;
                        else
                            SpawnerInputController.ProductionProgress = (float)spawner.TimeToSpawn / spawner.MaxTimeToSpawn;
                    }
                }
            }
        }
    }
}
