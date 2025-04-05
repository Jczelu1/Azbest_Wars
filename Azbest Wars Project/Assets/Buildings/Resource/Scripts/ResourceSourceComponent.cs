using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class ResourceSourceAuthoring : MonoBehaviour
{
    [SerializeField]
    byte resourcePerTick;

    private class Baker : Baker<ResourceSourceAuthoring>
    {
        public override void Bake(ResourceSourceAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ResourceSourceData { ResourcePerTick = authoring.resourcePerTick });
        }
    }
}
public struct ResourceSourceData : IComponentData
{
    public byte ResourcePerTick;
}
