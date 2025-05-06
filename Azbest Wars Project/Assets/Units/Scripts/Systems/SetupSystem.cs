using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[BurstCompile]
public partial class SetupSystem : SystemBase
{
    public static float delay = 2;
    public static bool unpauseOnSetup = false;
    public static bool started = false;
    protected override void OnCreate()
    {
        RequireForUpdate<GridPosition>();
    }

    protected override void OnUpdate()
    {
        if (delay >= 0)
        {
            delay -= SystemAPI.Time.DeltaTime;
        }
        else if (!started)
        {
            Debug.Log("SETUP");
            started = true;
            Entities.WithoutBurst().ForEach((Entity entity, ref GridPosition gridPosition, ref LocalToWorld worldTransform) =>
            {
                Vector3 position = worldTransform.Position;
                position.x -= ((float)gridPosition.Size.x / 2) - .5f;
                position.y -= ((float)gridPosition.Size.y / 2) - .5f;
                gridPosition.Position = MainGridScript.Instance.MainGrid.GetXY(position);
                if (gridPosition.isBuilding)
                {
                    for (int x = 0; x < gridPosition.Size.x; x++)
                    {
                        for (int y = 0; y < gridPosition.Size.y; y++)
                        {
                            MainGridScript.Instance.IsWalkable[new int2(x + gridPosition.Position.x, y + gridPosition.Position.y)] = false;
                        }
                    }
                }
            }).Run();
            if (unpauseOnSetup)
            {
                TickSystemGroup.SetTickrate(2);
            }
            TickSystemGroup.TickStep = true;
        }
    }
}
