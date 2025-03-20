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

            AddComponent(entity, new HealthData { Health = authoring.health, MaxValue = authoring.health, Attacked = false });
        }
    }
}
public struct HealthData : IComponentData
{
    public float Health;
    public float MaxValue;
    public bool Attacked;
    public bool Dead;
}
