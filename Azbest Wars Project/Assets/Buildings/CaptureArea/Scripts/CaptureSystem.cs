using NUnit.Framework;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(CaptureAreaSystem))]
public partial class CaptureSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<CaptureAreaData>();
    }
    bool areaMarked = false;
    public static Entity areaMarkerPrefab;
    protected override void OnUpdate()
     {
        if (!areaMarked)
        {
            areaMarked = true;
            Entities.WithoutBurst().ForEach((Entity entity, ref GridPosition gridPosition,  ref CaptureAreaData captureArea, ref TeamData team, ref DynamicBuffer<Child> children) =>
            {
                float2 gridOrigin = MainGridScript.Instance.GridOrigin;
                float cellSize = MainGridScript.Instance.CellSize;
                //top
                for (int dx = -captureArea.Size.x; dx <= captureArea.Size.x; dx++)
                {
                    float2 targetPosition;
                    targetPosition.x = (gridPosition.Position.x + dx) * cellSize + gridOrigin.x;
                    targetPosition.y = (gridPosition.Position.y + captureArea.Size.y) * cellSize + gridOrigin.y;

                    GameObject spriteObject = new GameObject("AreaMarker");
                    spriteObject.transform.position = new Vector3(targetPosition.x, targetPosition.y, 0);
                    SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = SpriteHolder.Instance.areaMarkerSprite;
                    EntityManager.GetComponentObject<GameObjectList>(entity).list.Add(spriteObject);
                }
                //right
                for (int dy = captureArea.Size.y-1; dy >= -captureArea.Size.y; dy--)
                {
                    float2 targetPosition;
                    targetPosition.x = (gridPosition.Position.x + captureArea.Size.x) * cellSize + gridOrigin.x;
                    targetPosition.y = (gridPosition.Position.y + dy) * cellSize + gridOrigin.y;

                    GameObject spriteObject = new GameObject("AreaMarker");
                    spriteObject.transform.position = new Vector3(targetPosition.x, targetPosition.y, 0);
                    SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = SpriteHolder.Instance.areaMarkerSprite;
                    EntityManager.GetComponentObject<GameObjectList>(entity).list.Add(spriteObject);
                }
                //bottom
                for (int dx = captureArea.Size.x-1; dx >= -captureArea.Size.x; dx--)
                {
                    float2 targetPosition;
                    targetPosition.x = (gridPosition.Position.x + dx) * cellSize + gridOrigin.x;
                    targetPosition.y = (gridPosition.Position.y + -captureArea.Size.y) * cellSize + gridOrigin.y;

                    GameObject spriteObject = new GameObject("AreaMarker");
                    spriteObject.transform.position = new Vector3(targetPosition.x, targetPosition.y, 0);
                    SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = SpriteHolder.Instance.areaMarkerSprite;
                    EntityManager.GetComponentObject<GameObjectList>(entity).list.Add(spriteObject);
                }
                //left
                for (int dy = -captureArea.Size.y + 1; dy <= captureArea.Size.y-1; dy++)
                {
                    float2 targetPosition;
                    targetPosition.x = (gridPosition.Position.x + -captureArea.Size.x) * cellSize + gridOrigin.x;
                    targetPosition.y = (gridPosition.Position.y + dy) * cellSize + gridOrigin.y;

                    GameObject spriteObject = new GameObject("AreaMarker");
                    spriteObject.transform.position = new Vector3(targetPosition.x, targetPosition.y, 0);
                    SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = SpriteHolder.Instance.areaMarkerSprite;
                    EntityManager.GetComponentObject<GameObjectList>(entity).list.Add(spriteObject);
                }
            }).Run();
        }
        Entities.WithoutBurst().ForEach((Entity entity, ref CaptureAreaData captureArea, ref TeamData team, ref DynamicBuffer<Child> children) =>
        {
            if (captureArea.Captured)
            {
                captureArea.Captured = false;
                team.Team = captureArea.CapturingTeam;
                EntityManager.GetComponentObject<SpriteRenderer>(entity).color = TeamManager.Instance.GetTeamColor(team.Team);

                foreach (var child in children)
                {
                    if (EntityManager.HasComponent<SpriteRenderer>(child.Value) && EntityManager.HasComponent<TeamData>(child.Value))
                    {
                        EntityManager.GetComponentObject<SpriteRenderer>(child.Value).color = TeamManager.Instance.GetTeamColor(team.Team);
                        EntityManager.SetComponentData<TeamData>(child.Value, team);
                    }
                    if (EntityManager.HasComponent<SpawnerData>(child.Value))
                    {
                        EntityManager.SetComponentData(child.Value, new SpawnerData
                        {
                            SpawnedUnit = 0,
                            SpawnRateMult = 2,
                            Queued = 0,
                            TimeToSpawn = 1,
                            MaxTimeToSpawn = 0,
                            SetFormation = -1,
                            NextSpawnedUnit = -1
                        });
                    }
                }
            }
            float capturePercentage = Mathf.Clamp01((float)captureArea.CaptureProgress / captureArea.RequiredCapture);
            GameObjectList gameObjectList = EntityManager.GetComponentObject<GameObjectList>(entity);
            int areaMarkers = gameObjectList.list.Count;
            int captureMarkers = Mathf.FloorToInt(capturePercentage * (areaMarkers - 1));
            foreach (GameObject g in EntityManager.GetComponentObject<GameObjectList>(entity).list)
            {
                if (captureMarkers > 0)
                {
                    g.GetComponent<SpriteRenderer>().color = TeamManager.Instance.GetTeamColor(captureArea.CapturingTeam);
                    captureMarkers--;
                }
                else
                {
                    g.GetComponent<SpriteRenderer>().color = TeamManager.Instance.GetTeamColor(team.Team);
                }
            }
        }).Run();
    }
}

