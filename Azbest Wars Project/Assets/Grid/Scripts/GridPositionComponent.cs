using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GridPositionComponent : MonoBehaviour
{
    private Vector3 WorldPosition;
    private int2 startGridPosition;

    private void Start()
    {
        UpdateGridPosition();
    }
    private void UpdateGridPosition()
    {
        if (MainGridScript.Instance != null && MainGridScript.Instance.MainGrid != null)
        {
            startGridPosition = MainGridScript.Instance.MainGrid.GetXY(transform.position);
        }
    }

    private class Baker : Baker<GridPositionComponent>
    {
        public override void Bake(GridPositionComponent authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            authoring.UpdateGridPosition();
            AddComponent(entity, new GridPosition
            {
                Position = authoring.startGridPosition
            });
        }
    }
}
public struct GridPosition : IComponentData
{
    public int2 Position;
}
