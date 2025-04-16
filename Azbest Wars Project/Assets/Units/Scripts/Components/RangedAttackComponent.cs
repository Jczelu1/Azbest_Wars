using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class RangedAttackAuthoring : MonoBehaviour
{
    [SerializeField]
    float damage;
    [SerializeField]
    byte range;
    [SerializeField]
    byte attackCooldown = 0;
    [SerializeField]
    byte moveCooldown = 0;

    private class Baker : Baker<RangedAttackAuthoring>
    {
        public override void Bake(RangedAttackAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new RangedAttackData { Attacked = false, Damage = authoring.damage, Range = authoring.range, AttackCooldown = authoring.attackCooldown, MoveCooldown = authoring.moveCooldown, CurrentCooldown = 0 });
        }
    }
}
public struct RangedAttackData : IComponentData
{
    public bool Attacked;
    public float Damage;
    public byte Range;
    public byte AttackCooldown;
    public byte MoveCooldown;
    public byte CurrentCooldown;
}