using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class UnitTypeIdAuthoring : MonoBehaviour
{
    [SerializeField]
    int unitTypeId;

    private class Baker : Baker<UnitTypeIdAuthoring>
    {
        public override void Bake(UnitTypeIdAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new UnitTypeId { Id = authoring.unitTypeId });
        }
    }
}
public struct UnitTypeId : IComponentData
{
    public int Id;
}
