using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class RandomValueAuthoring : MonoBehaviour
{
    private class Baker : Baker<RandomValueAuthoring>
    {
        public override void Bake(RandomValueAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new RandomValueData { value = 0 });
        }
    }
}
public struct RandomValueData : IComponentData
{
    public float value;
}
