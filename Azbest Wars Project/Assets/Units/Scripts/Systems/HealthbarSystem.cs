using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MeleAttackSystem))]
[UpdateAfter(typeof(RangedAttackSystem))]
public partial class HealthbarSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithoutBurst().ForEach((Entity entity, ref HealthData healthData, ref TeamData team, in DynamicBuffer<Child> children) =>
        {
            if(healthData.Health > 0)
                EntityManager.GetComponentObject<SpriteRenderer>(entity).color = TeamManager.Instance.GetTeamColor(team.Team);
            if (!healthData.Attacked)
            {
                return;
            }
            float healthPercentage = Mathf.Clamp01(healthData.Health / healthData.MaxValue);

            int spriteIndex = Mathf.RoundToInt(healthPercentage * (SpriteHolder.Instance.healthbarSprites.Length - 1));
            foreach (var child in children)
            {
                if (!EntityManager.HasComponent<HealthbarTag>(child.Value))
                    continue;

                if (EntityManager.HasComponent<SpriteRenderer>(child.Value))
                {
                    var spriteRenderer = EntityManager.GetComponentObject<SpriteRenderer>(child.Value);
                    spriteRenderer.sprite = SpriteHolder.Instance.healthbarSprites[spriteIndex];
                }
            }
        }).Run();
    }
}
[UpdateInGroup(typeof(SubTickSystemGroup))]
[UpdateBefore(typeof(SubTickManagerSystem))]
public partial class SetColorSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if(SubTickSystemGroup.subTickNumber == 2)
        {
            Entities.WithoutBurst().ForEach((Entity entity, ref HealthData healthData, ref TeamData team, in DynamicBuffer<Child> children) =>
            {
                if (healthData.Attacked)
                {
                    EntityManager.GetComponentObject<SpriteRenderer>(entity).color = new Color(1, 0, 0);
                    healthData.Attacked = false;
                }
            }).Run();
        }
        else if(SubTickSystemGroup.subTickNumber == 0)
        {
            Entities.WithoutBurst().ForEach((Entity entity, ref HealthData healthData, ref TeamData team, in DynamicBuffer<Child> children) =>
            {
                if (healthData.Health <= 0)
                {
                    EntityManager.GetComponentObject<SpriteRenderer>(entity).color = new Color(1, 0, 0);
                }
            }).Run();
        }
    }
}
