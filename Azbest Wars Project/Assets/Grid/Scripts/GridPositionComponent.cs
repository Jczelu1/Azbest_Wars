using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GridPositionComponent : MonoBehaviour
{
    //private Vector3 WorldPosition;
    [HideInInspector]
    public int2 startGridPosition;
    [SerializeField]
    int width = 1;
    [SerializeField]
    int height = 1;
    [SerializeField]
    bool isBuilding = false;

    private void Start()
    {
        UpdateGridPosition();
    }
    private void UpdateGridPosition()
    {
        if (MainGridScript.Instance == null) return;
        Vector3 position = transform.position;
        position.x -= ((float)width / 2) - .5f;
        position.y -= ((float)height / 2) - .5f;
        startGridPosition = MainGridScript.Instance.MainGrid.GetXY(position);
        if (isBuilding)
        {
            for(int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    MainGridScript.Instance.IsWalkable[new int2(x + startGridPosition.x, y + startGridPosition.y)] = false;
                }
            }
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
