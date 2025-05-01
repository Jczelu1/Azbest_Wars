using System;
using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

public class UnitAnimatorAuthoring : MonoBehaviour
{
    [SerializeField]
    Sprite baseSprite;
    [SerializeField]
    List<Sprite> walkingAnimation;
    [SerializeField]
    List<Sprite> attackingAnimation;
    private class Baker : Baker<UnitAnimatorAuthoring>
    {
        public override void Bake(UnitAnimatorAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new UnitAnimatorData { 
                BaseSprite = authoring.baseSprite,
                WalkingAnimation = new List<Sprite>(authoring.walkingAnimation),
                AttackingAnimation = new List<Sprite>(authoring.attackingAnimation),
            });
        }
    }
}
public class UnitAnimatorData : IComponentData, IDisposable, ICloneable
{
    public Sprite BaseSprite;
    public List<Sprite> WalkingAnimation;
    public List<Sprite> AttackingAnimation;

    public void Dispose()
    {
        WalkingAnimation.Clear();
        AttackingAnimation.Clear();
    }

    public object Clone()
    {
        UnitAnimatorData clonee = new UnitAnimatorData
        {
            BaseSprite = BaseSprite,
            WalkingAnimation = new List<Sprite>(WalkingAnimation),
            AttackingAnimation = new List<Sprite>(AttackingAnimation)
        };
        return clonee;
    }
}
