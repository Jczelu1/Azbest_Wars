using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MoveSystem))]
public partial class MapTextureSystem : SystemBase
{
    public static Texture2D baseTexture = null;
    public static Texture2D mapTexture = null;

    protected override void OnCreate()
    {
        base.OnCreate();
    }
    protected override void OnUpdate()
    {
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
        mapTexture = new Texture2D(width, height);
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.SetPixels(baseTexture.GetPixels());
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
                        if(selectedLookup.HasComponent(unit) && selectedLookup[unit].Selected)
                        {
                            mapTexture.SetPixel(x, y, TeamManager.Instance.selectedColor);
                            continue;
                        }
                        mapTexture.SetPixel(x, y, TeamManager.Instance.GetTeamColor(teamLookup[unit].Team));
                    }
                }
            }
        }
        mapTexture.Apply();
    }
}
