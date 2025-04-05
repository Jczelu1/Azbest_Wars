using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public class UnitTypeAuthoring : MonoBehaviour
{
    [SerializeField]
    string unitName;
    [SerializeField]
    int cost;
    [SerializeField]
    GameObject prefab;

    private class Baker : Baker<UnitTypeAuthoring>
    {
        public override void Bake(UnitTypeAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new UnitTypeData {
                UnitName = authoring.unitName,
                Cost = authoring.cost,
                Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}
public struct UnitTypeData : IComponentData
{
    public FixedString64Bytes UnitName;
    public int Cost;
    public Entity Prefab;
}
