using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(MoveSystem))]
public partial class SelectSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<SelectedData>();
    }

    protected override void OnUpdate()
    {
        if (MainGridScript.Instance.Selected)
        {
            MainGridScript.Instance.Selected = false;

            int minX = Mathf.Min(MainGridScript.Instance.SelectStartPosition.x, MainGridScript.Instance.SelectEndPosition.x);
            int maxX = Mathf.Max(MainGridScript.Instance.SelectStartPosition.x, MainGridScript.Instance.SelectEndPosition.x);
            int minY = Mathf.Min(MainGridScript.Instance.SelectStartPosition.y, MainGridScript.Instance.SelectEndPosition.y);
            int maxY = Mathf.Max(MainGridScript.Instance.SelectStartPosition.y, MainGridScript.Instance.SelectEndPosition.y);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Entity entity = MainGridScript.Instance.Occupied[new int2(x, y)];
                    if (entity != Entity.Null && SystemAPI.HasComponent<SelectedData>(entity) && SystemAPI.GetComponent<TeamData>(entity).Team == MainGridScript.Instance.PlayerTeam)
                    {
                        SystemAPI.SetComponent<SelectedData>(entity, new SelectedData { Selected = true});
                    }
                }
            }
        }
    }
}
