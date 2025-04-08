using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class MeleAttackAuthoring : MonoBehaviour
{
    [SerializeField]
    float damage;
    [SerializeField]
    byte attackType;
    [SerializeField]
    byte attackCooldown = 0;
    [SerializeField]
    byte moveCooldown = 0;

    private class Baker : Baker<MeleAttackAuthoring>
    {
        public override void Bake(MeleAttackAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new MeleAttackData { Attacked = false, Damage = authoring.damage, AttackType = authoring.attackType, AttackCooldown = authoring.attackCooldown, MoveCooldown = authoring.moveCooldown, CurrentCooldown = 0 });
        }
    }
}
public struct MeleAttackData : IComponentData
{
    public bool Attacked;
    public float Damage;
    public byte AttackType;
    public byte AttackCooldown;
    public byte MoveCooldown;
    public byte CurrentCooldown;
}
