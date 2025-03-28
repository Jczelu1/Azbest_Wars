using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EntityBufferAuthoring : MonoBehaviour
{
    private class Baker : Baker<EntityBufferAuthoring>
    {
        public override void Bake(EntityBufferAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddBuffer<EntityData>(entity);
        }
    }
}
[InternalBufferCapacity(0)]
public struct EntityData : IBufferElementData
{
    public Entity entity;
}
