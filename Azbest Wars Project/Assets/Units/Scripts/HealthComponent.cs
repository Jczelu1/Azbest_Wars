using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class HealthAuthoring : MonoBehaviour
{
    [SerializeField]
    float health;

    private class Baker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new HealthData { Health = authoring.health, MaxValue = authoring.health });
        }
    }
}
public struct HealthData : IComponentData
{
    public float Health;
    public float MaxValue;
}
