using UnityEngine;
using Unity.Entities;

public class SpotterAuthoring : MonoBehaviour
{
    [SerializeField]
    byte spotRange;
    private class Baker : Baker<SpotterAuthoring>
    {
        public override void Bake(SpotterAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SpotterData { SpotRange = authoring.spotRange });
        }
    }
}
public struct SpotterData : IComponentData
{
    public byte SpotRange;
}
