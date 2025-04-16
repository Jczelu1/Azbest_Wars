using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//[UpdateInGroup(typeof(TickSystemGroup))]
//[UpdateBefore(typeof(PathfindSystem))]
//[UpdateBefore(typeof(MoveSystem))]
[BurstCompile]
public partial class SetupSystem : SystemBase
{
    public static bool started = false;
    protected override void OnCreate()
    {
        RequireForUpdate<GridPosition>();
    }

    protected override void OnUpdate()
    {
        if (!started)
        {
            Entities.WithoutBurst().ForEach((Entity entity, ref GridPosition gridPosition, ref LocalToWorld worldTransform) =>
            {
                Vector3 position = worldTransform.Position;
                Debug.Log(position);
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

            started = true;
        }
    }
}
