using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

public class GameObjectListAuthoring : MonoBehaviour
{
    private class Baker : Baker<GameObjectListAuthoring>
    {
        public override void Bake(GameObjectListAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new GameObjectList());
        }
    }
}
public class GameObjectList : IComponentData, IDisposable, ICloneable
{
    public List<GameObject> list = new List<GameObject>();

    public void Dispose()
    {
        list.Clear();
    }

    public object Clone()
    {
        GameObjectList clonedList = new GameObjectList
        {
            list = new List<GameObject>(list)
        };

        return clonedList;
    }
}
