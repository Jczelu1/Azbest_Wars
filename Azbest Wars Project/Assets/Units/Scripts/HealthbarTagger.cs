using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class HealthbarTagAuthoring : MonoBehaviour
{
    private class Baker : Baker<HealthbarTagAuthoring>
    {
        public override void Bake(HealthbarTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new HealthbarTag());
        }
    }
}
public struct HealthbarTag : IComponentData { }
