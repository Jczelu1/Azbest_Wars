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
            if (unitState.Moved)
            {
                Debug.Log(subTickNumber);
                subTickNumber%=animator.WalkingAnimation.Count;
                spriteRenderer.sprite = animator.WalkingAnimation[subTickNumber];
            }
            else if (unitState.Attacked)
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