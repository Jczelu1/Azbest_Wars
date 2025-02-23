using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class UnitAuthoring : MonoBehaviour
{
    [SerializeField] float Speed;

    private class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitData
            {
                Speed = authoring.Speed
            });
        }
    }
}
