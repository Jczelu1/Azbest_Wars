using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

//[UpdateInGroup(typeof(TickSystemGroup))]
//[UpdateAfter(typeof(MoveSystem))]
public partial class MapTextureSystem : SystemBase
{
    public static Texture2D baseTexture = null;
    public static Texture2D mapTexture = null;

    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<GridPosition>();
    }
    protected override void OnUpdate()
    {
        if (SetupSystem.startDelay != -1) return;
        int width = MainGridScript.Instance.Width;
        int height = MainGridScript.Instance.Height;
        if (baseTexture == null)
        {
            baseTexture = new Texture2D(width, height);
            baseTexture.filterMode = FilterMode.Point;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (MainGridScript.Instance.IsWalkable[new int2(x, y)])
                    {
                        baseTexture.SetPixel(x, y, new Color(0, 0, 0));
                    }
                    else
                    {
                        baseTexture.SetPixel(x, y, new Color(104f / 255, 104f / 255, 104f / 255));
                    }
                }
            }
            baseTexture.Apply();
        }
        int playerTeam = TeamManager.Instance.PlayerTeam;
        mapTexture = new Texture2D(width, height);
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.SetPixels(baseTexture.GetPixels());
        Entities.WithoutBurst().ForEach((Entity entity, ref CaptureAreaData captureArea, ref GridPosition gridPosition, ref TeamData team) =>
        {
            for (int dx = -captureArea.Size.x; dx <= captureArea.Size.x; dx++)
            {
                for (int dy = -captureArea.Size.y; dy <= captureArea.Size.y; dy++)
                {
                    int2 pos = new int2 { x = dx + gridPosition.Position.x, y = dy + gridPosition.Position.y };
                    if (MainGridScript.Instance.Occupied.IsInGrid(pos))
                    {
                        if (MainGridScript.Instance.IsWalkable[pos])
                            mapTexture.SetPixel(pos.x, pos.y, TeamManager.Instance.GetTeamColorLow(team.Team));
                    }
                }
            }
        }).Run();
        var teamLookup = GetComponentLookup<TeamData>(isReadOnly: true);
        var selectedLookup = GetComponentLookup<SelectedData>(isReadOnly: true);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (MainGridScript.Instance.Occupied[new int2(x, y)] != Entity.Null)
                {
                    Entity unit = MainGridScript.Instance.Occupied[new int2(x, y)];
                    if (teamLookup.HasComponent(unit))
                    {
                        if(selectedLookup.HasComponent(unit) && selectedLookup[unit].Selected && teamLookup[unit].Team == playerTeam)
                        {
                            mapTexture.SetPixel(x, y, TeamManager.Instance.selectedColor);
                            continue;
                        }
                        mapTexture.SetPixel(x, y, TeamManager.Instance.GetTeamColor(teamLookup[unit].Team));
                    }
                }
            }
        }
        Entities.WithoutBurst().ForEach((Entity entity, ref BuildingIdData buildingId, ref GridPosition gridPosition, ref SelectedData selected) =>
        {
            if (!selected.Selected) return;
            for (int dx = 0; dx < gridPosition.Size.x; dx++)
            {
                for (int dy = 0; dy < gridPosition.Size.y; dy++)
                {
                    int2 pos = new int2 { x = dx + gridPosition.Position.x, y = dy + gridPosition.Position.y };
                    if (MainGridScript.Instance.Occupied.IsInGrid(pos))
                    {
                        mapTexture.SetPixel(pos.x, pos.y, TeamManager.Instance.selectedColor);
                    }
                }
            }
        }).Run();
        mapTexture.Apply();
    }
}
