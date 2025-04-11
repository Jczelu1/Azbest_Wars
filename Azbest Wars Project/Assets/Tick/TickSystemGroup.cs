using Unity.Entities;
using UnityEngine;


public partial class TickSystemGroup : ComponentSystemGroup
{
    //time between updates in miliseconds
    public static uint Tickrate { get; private set; } = 500;
    const uint BaseTickrate = 500;
    private static bool Pause = false;
    private static bool ChangeTickrate = false;

    public static void SetTickrate(byte level)
    {
        if(level > 8) return;
        if(level == 0)
        {
            Pause = true;
            SubTickSystemGroup.subTickEnabled = false;
            return;
        }
        Tickrate = (uint)(BaseTickrate / level);
        Pause = false;
        ChangeTickrate = true;
        if(level < 4)
        {
            SubTickSystemGroup.subTickEnabled = true;
            SubTickSystemGroup.ChangeTickrate = true;
        }
        else
        {
            SubTickSystemGroup.subTickEnabled = false;
        }
    }
    protected override void OnCreate()
    {
        base.OnCreate();
        RateManager = new RateUtils.VariableRateManager(BaseTickrate, false);
    }
    protected override void OnUpdate()
    {
        if (Pause) return;
        if (ChangeTickrate)
        {
            ChangeTickrate = false;
            RateManager = new RateUtils.VariableRateManager(Tickrate, false);
        }
        base.OnUpdate();
    }
}
public partial class SubTickSystemGroup : ComponentSystemGroup
{
    public static int subTickNumber = 0;
    public static bool subTickEnabled = true;
    public static bool ChangeTickrate = false;
    protected override void OnCreate()
    {
        base.OnCreate();
        RateManager = new RateUtils.VariableRateManager(TickSystemGroup.Tickrate / 4, false);
    }
    protected override void OnUpdate()
    {
        if (!subTickEnabled) return;
        if (ChangeTickrate)
        {
            ChangeTickrate = false;
            RateManager = new RateUtils.VariableRateManager(TickSystemGroup.Tickrate / 4, false);
        }
        base.OnUpdate();
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
