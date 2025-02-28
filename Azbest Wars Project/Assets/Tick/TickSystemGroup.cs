using Unity.Entities;

public partial class TickSystemGroup : ComponentSystemGroup
{
    private float Tickrate = 0.5f;

    protected override void OnCreate()
    {
        base.OnCreate();
        RateManager = new RateUtils.FixedRateCatchUpManager(Tickrate);
    }
}
