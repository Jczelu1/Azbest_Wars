using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SubTickSystemGroup))]
[UpdateBefore(typeof(SubTickManagerSystem))]
public partial class UnitAnimatorSystem : SystemBase
{
    public static List<UnitAnimatorData> animators = new List<UnitAnimatorData>();
    public static bool started = false;
    protected override void OnCreate()
    {
        RequireForUpdate<UnitAnimatorData>();
    }
    protected override void OnUpdate()
    {
        if (SetupSystem.startDelay != -1) return;
        if (!started)
        {
            started = true;
            animators.Clear();
            Entities.WithoutBurst().ForEach((in UnitTypeData unitType, in UnitAnimatorData animator) =>
            {
                while (animators.Count <= unitType.Id)
                {
                    animators.Add(animator);
                }
                animators[unitType.Id] = animator;
            }).Run();
        }
        Entities.WithoutBurst().ForEach((Entity entity, in UnitStateData unitState, in UnitTypeId typeId) =>
        {
            if (animators.Count <= typeId.Id) return;
            UnitAnimatorData animator = animators[typeId.Id];
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