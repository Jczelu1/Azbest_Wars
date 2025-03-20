using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MeleAttackSystem))]
public partial class HealthbarSystem : SystemBase
{
    protected override void OnUpdate()
    {

        Entities.WithoutBurst().ForEach((Entity entity, ref HealthData healthData, ref TeamData team, in DynamicBuffer<Child> children) =>
        {
            SetColorSystem.lastTickTime = SystemAPI.Time.ElapsedTime;
            SetColorSystem.reset = true;
            EntityManager.GetComponentObject<SpriteRenderer>(entity).color = TeamColors.colors[team.Team];
            if (!healthData.Attacked)
            {
                return;
            }
            float healthPercentage = Mathf.Clamp01(healthData.Health / healthData.MaxValue);

            int spriteIndex = Mathf.FloorToInt(healthPercentage * (SpriteHolder.Instance.healthbarSprites.Length - 1));
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

public partial class SetColorSystem : SystemBase
{
    public static double lastTickTime = 0;
    public static bool reset = true;
    protected override void OnUpdate()
    {
        if(reset && SystemAPI.Time.ElapsedTime - lastTickTime > TickSystemGroup.Tickrate / 2)
        {
            reset = false;
            Entities.WithoutBurst().ForEach((Entity entity, ref HealthData healthData, ref TeamData team, in DynamicBuffer<Child> children) =>
            {
                if (healthData.Attacked)
                {
                    EntityManager.GetComponentObject<SpriteRenderer>(entity).color = new Color(1, 0, 0);
                    healthData.Attacked = false;
                    return;
                }
            }).Run();
        }
    }
}
