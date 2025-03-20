using Unity.Entities;
using UnityEngine;


public partial class TickSystemGroup : ComponentSystemGroup
{
    public static float Tickrate = .5f;

    protected override void OnCreate()
    {
        base.OnCreate();
        RateManager = new RateUtils.FixedRateCatchUpManager(Tickrate);
    }
    
    protected override void OnUpdate()
    {
        base.OnUpdate();
        //Debug.Log("tick");
        //MainGridScript.Instance.Clicked = false;
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
