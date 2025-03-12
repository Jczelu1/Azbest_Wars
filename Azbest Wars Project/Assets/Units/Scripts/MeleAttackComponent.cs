using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class MeleAttackAuthoring : MonoBehaviour
{
    [SerializeField]
    float damage;
    [SerializeField]
    byte range;

    private class Baker : Baker<MeleAttackAuthoring>
    {
        public override void Bake(MeleAttackAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new MeleAttackData { Attacked = false, Damage = authoring.damage, Range = authoring.range });
        }
    }
}
public struct MeleAttackData : IComponentData
{
    public bool Attacked;
    public float Damage;
    public byte Range;
}
