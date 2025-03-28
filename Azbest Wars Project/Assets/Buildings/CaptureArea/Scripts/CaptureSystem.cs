using NUnit.Framework;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(CaptureAreaSystem))]
public partial class CaptureSystem : SystemBase
{
    bool areaMarked = false;
    public static Entity areaMarkerPrefab;
    protected override void OnUpdate()
     {
        if (!areaMarked)
        {
            areaMarked = true;
            Entities.WithoutBurst().ForEach((Entity entity, ref GridPosition gridPosition,  ref CaptureAreaData captureArea, ref TeamData team, ref DynamicBuffer<Child> children) =>
            {
                Debug.Log("asdfasfd");
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
            if (!captureArea.Captured) return;
            captureArea.Captured = false;
            team.Team = captureArea.CapturingTeam;
            EntityManager.GetComponentObject<SpriteRenderer>(entity).color = TeamColors.GetTeamColor(team.Team);

            foreach (var child in children)
            {
                if (EntityManager.HasComponent<SpriteRenderer>(child.Value) && EntityManager.HasComponent<TeamData>(child.Value))
                {
                    EntityManager.GetComponentObject<SpriteRenderer>(child.Value).color = TeamColors.GetTeamColor(team.Team);
                    EntityManager.SetComponentData<TeamData>(child.Value, team);
                }
            }
        }).Run();
    }
}

