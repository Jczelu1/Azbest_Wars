using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(CaptureAreaSystem))]
public partial class WinConditionSystem : SystemBase
{
    public static int TotalWinAreas = -1;
    public static int RequiredWinAreas = 1;
    public static int CapturedWinAreas = -1;
    public static float TimeLeftSeconds = -1;
    public static float TimeTotalSeconds = -1;
    public static bool EndIfCompleted = false;
    public static bool Ended = false;
    public static bool Win = false;
    protected override void OnCreate()
    {
        RequireForUpdate<CaptureAreaData>();
    }
    protected override void OnUpdate()
    {
        if (TimeLeftSeconds == -1) return;
        TimeLeftSeconds -= TickSystemGroup.TimePerTick;
        if (TotalWinAreas == -1)
        {
            TotalWinAreas = 0;
            Entities.WithoutBurst().ForEach((ref CaptureAreaData captureArea, ref TeamData team) =>
            {
                if (!captureArea.WinCondition) return;
                TotalWinAreas++;

            }).Run();
        }
        CapturedWinAreas = 0;
        Entities.WithoutBurst().ForEach((ref CaptureAreaData captureArea, ref TeamData team) =>
        {
            if (!captureArea.WinCondition) return;
            if (team.Team != TeamManager.Instance.PlayerTeam) return;
            CapturedWinAreas++;
        }).Run();
        if(EndIfCompleted && CapturedWinAreas == RequiredWinAreas)
        {
            Ended = true;
            Win = true;
            return;
        }
        if(TimeLeftSeconds <= 0)
        {
            Ended = true;
            if(CapturedWinAreas == RequiredWinAreas)
            {
                Win = true;
            }
        }
    }
}
