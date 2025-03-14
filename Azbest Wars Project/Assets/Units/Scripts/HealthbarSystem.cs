using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
public partial class HealthbarSystem : SystemBase
{
    protected override void OnUpdate()
    {

        Entities.WithoutBurst().ForEach((Entity entity, ref HealthData healthData, in DynamicBuffer<Child> children) =>
        {
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
