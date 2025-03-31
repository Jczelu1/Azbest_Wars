using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

public class StandardAnimatorAuthoring : MonoBehaviour
{
    [SerializeField]
    Sprite baseSprite;
    [SerializeField]
    List<Sprite> animationLoop;
    private class Baker : Baker<StandardAnimatorAuthoring>
    {
        public override void Bake(StandardAnimatorAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new StandardAnimatorData
            {
                BaseSprite = authoring.baseSprite,
                Animation = new List<Sprite>(authoring.animationLoop),
            });
        }
    }
}
public class StandardAnimatorData : IComponentData, IDisposable, ICloneable
{
    public Sprite BaseSprite;
    public List<Sprite> Animation;

    public void Dispose()
    {
        Animation.Clear();
    }

    public object Clone()
    {
        StandardAnimatorData clonee = new StandardAnimatorData
        {
            BaseSprite = BaseSprite,
            Animation = new List<Sprite>(Animation),
        };
        return clonee;
    }
}
