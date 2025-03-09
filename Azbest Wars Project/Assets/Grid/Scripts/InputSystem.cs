using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MoveSystem))]
public partial struct InputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        MainGridScript.Instance.RightClick = false;
        //MainGridScript.Instance.Selected = false;
    }
}
