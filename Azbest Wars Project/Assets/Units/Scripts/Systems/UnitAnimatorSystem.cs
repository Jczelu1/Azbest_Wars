using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SubTickSystemGroup))]
[UpdateBefore(typeof(SubTickManagerSystem))]
public partial class UnitAnimatorSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithoutBurst().ForEach((Entity entity, ref UnitStateData unitState, in UnitAnimatorData animator) =>
        {
            SpriteRenderer spriteRenderer = EntityManager.GetComponentObject<SpriteRenderer>(entity);
            int subTickNumber = SubTickSystemGroup.subTickNumber;
            if (unitState.Moved && animator.WalkingAnimation.Count != 0)
            {
                subTickNumber %= animator.WalkingAnimation.Count;
                spriteRenderer.sprite = animator.WalkingAnimation[subTickNumber];
            }
            else if (unitState.Attacked && animator.AttackingAnimation.Count != 0)
            {
                subTickNumber %= animator.AttackingAnimation.Count;
                spriteRenderer.sprite = animator.AttackingAnimation[subTickNumber];
            }
            else
            {
                spriteRenderer.sprite = animator.BaseSprite;
            }
        }).Run();
    }
}
[UpdateInGroup(typeof(TickSystemGroup))]
public partial class UnitAnimatorResetSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (SubTickSystemGroup.subTickEnabled) return;
        Entities.WithoutBurst().ForEach((Entity entity, ref UnitStateData unitState, ref HealthData healthData, in UnitAnimatorData animator) =>
        {
            SpriteRenderer spriteRenderer = EntityManager.GetComponentObject<SpriteRenderer>(entity);
            spriteRenderer.sprite = animator.BaseSprite;
            healthData.Attacked = false;
        }).Run();
    }
}