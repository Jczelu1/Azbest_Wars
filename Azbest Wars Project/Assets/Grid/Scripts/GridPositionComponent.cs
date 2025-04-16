using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GridPositionComponent : MonoBehaviour
{
    //private Vector3 WorldPosition;
    [SerializeField]
    int width = 1;
    [SerializeField]
    int height = 1;
    [SerializeField]
    bool isBuilding = false;

    private void Start()
    {
    }

    private class Baker : Baker<GridPositionComponent>
    {
        public override void Bake(GridPositionComponent authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new GridPosition
            {
                Position = new int2(-1,-1),
                Size = new int2(authoring.width, authoring.height),
                isBuilding = authoring.isBuilding,
            });
        }
    }
}
public struct GridPosition : IComponentData
{
    public int2 Position;
    public int2 Size;
    public bool isBuilding;
}
