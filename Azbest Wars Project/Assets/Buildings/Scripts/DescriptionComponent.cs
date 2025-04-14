using System.Collections.Generic;
using System;
using Unity.Entities;
using UnityEngine;
public class DescriptionAuthoring : MonoBehaviour
{
    [SerializeField]
    string Name;
    [SerializeField]
    [TextArea]
    string description;
    [SerializeField]
    Sprite baseSprite;
    private class Baker : Baker<DescriptionAuthoring>
    {
        public override void Bake(DescriptionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new DescriptionData
            {
                BaseSprite = authoring.baseSprite,
                Name = authoring.Name,
                Description = authoring.description,
            });
        }
    }
}
public class DescriptionData : IComponentData, IDisposable, ICloneable
{
    public string Name;
    public string Description;
    public Sprite BaseSprite;
   

    public void Dispose()
    {
        //BaseSprite = null;
        //Description = null;
    }

    public object Clone()
    {
        DescriptionData clone = new DescriptionData { BaseSprite = this.BaseSprite, Description = this.Description, Name = this.Name };
        return clone;
    }
}
