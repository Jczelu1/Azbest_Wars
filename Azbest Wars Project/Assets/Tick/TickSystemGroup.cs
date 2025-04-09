using Unity.Entities;
using UnityEngine;


public partial class TickSystemGroup : ComponentSystemGroup
{
    //time between updates in miliseconds
    public static uint Tickrate = 100;

    protected override void OnCreate()
    {
        base.OnCreate();
        RateManager = new RateUtils.VariableRateManager(Tickrate, false);
    }
}
public partial class SubTickSystemGroup : ComponentSystemGroup
{
    public static int subTickNumber = 0;
    public static bool subTickEnabled = true;
    protected override void OnCreate()
    {
        base.OnCreate();
        RateManager = new RateUtils.VariableRateManager(TickSystemGroup.Tickrate / 4, false);
    }
    protected override void OnUpdate()
    {
        if (subTickEnabled)
        {
            base.OnUpdate();
        }
    }
}
[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateBefore(typeof(PathfindSystem))]
public partial class TickManagerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        //Debug.Log("tick");
        SubTickSystemGroup.subTickNumber = 0;
    }
}
[UpdateInGroup(typeof(SubTickSystemGroup))]
public partial class SubTickManagerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        SubTickSystemGroup.subTickNumber++;
        
        //Debug.Log("subtick: " + SubTickSystemGroup.subTickNumber);
    }
}
