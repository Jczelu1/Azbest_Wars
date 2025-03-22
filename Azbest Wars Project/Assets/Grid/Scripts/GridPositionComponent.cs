using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GridPositionComponent : MonoBehaviour
{
    //private Vector3 WorldPosition;
    private int2 startGridPosition;
    [SerializeField]
    int width = 1;
    [SerializeField]
    int height = 1;

    private void Start()
    {
        UpdateGridPosition();
    }
    private void UpdateGridPosition()
    {
        if (MainGridScript.Instance != null && MainGridScript.Instance.MainGrid != null)
        {
            Vector3 position = transform.position;
            position.x -= width / 2 - .5f;
            position.y -= height / 2 - .5f;
            startGridPosition = MainGridScript.Instance.MainGrid.GetXY(position);
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
                Position = authoring.startGridPosition,
                Size = new int2(authoring.width, authoring.height)
            });
        }
    }
}
public struct GridPosition : IComponentData
{
    public int2 Position;
    public int2 Size;
}
