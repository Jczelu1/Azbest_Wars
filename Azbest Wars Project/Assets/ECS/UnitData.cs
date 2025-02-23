using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public struct UnitData : IComponentData
{
    public float Speed;
    public int2 Postiton;
}