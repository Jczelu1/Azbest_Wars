using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SubTickSystemGroup))]
[UpdateBefore(typeof(SubTickManagerSystem))]
public partial class StandardAnimatorSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithoutBurst().ForEach((Entity entity, in StandardAnimatorData animator) =>
        {
            SpriteRenderer spriteRenderer = EntityManager.GetComponentObject<SpriteRenderer>(entity);
            int subTickNumber = SubTickSystemGroup.subTickNumber;
            if (animator.Animation.Count != 0)
            {
                subTickNumber %= animator.Animation.Count;
                spriteRenderer.sprite = animator.Animation[subTickNumber];
            }
            else
            {
                spriteRenderer.sprite = animator.BaseSprite;
            }
        }).Run();
    }
}