using Unity.Entities;
using UnityEngine;


public partial class TickSystemGroup : ComponentSystemGroup
{
    //time between updates in miliseconds
    public static uint Tickrate = 500;

    protected override void OnCreate()
    {
        base.OnCreate();
        RateManager = new RateUtils.VariableRateManager(Tickrate, false);
    }
 
}

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(MoveSystem))]
public partial class ClickManageSystem : SystemBase
{
    protected override void OnUpdate()
    {
        //Debug.Log("tick");
        MainGridScript.Instance.RightClick = false;
    }
}
