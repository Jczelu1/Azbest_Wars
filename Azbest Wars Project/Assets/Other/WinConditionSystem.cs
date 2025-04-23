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
    public static int CapturedWinAreas = 0;
    public static float TimeLeftSeconds = -1;
    public static float TimeTotalSeconds = -1;
    public static bool EndIfCompleted = false;
    public static bool Ended = false;
    public static bool Win = false;
    public static int startDelay = 2;
    protected override void OnCreate()
    {
        RequireForUpdate<CaptureAreaData>();
    }
    protected override void OnUpdate()
    {
        if (SetupSystem.startDelay != -1) return;
        if(startDelay > 0)
        {
            Debug.Log(startDelay);
            startDelay--;
            return;
        }
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
        bool PlayerHasSpawner = false;
        bool EnemyHasSpawner = false;
        bool PlayerHasUnits = false;
        bool EnemyHasUnits = false;
        Entities.WithoutBurst().ForEach((in CaptureAreaData captureArea, in TeamData team) =>
        {
            if (captureArea.HasSpawner)
            {
                if(team.Team == TeamManager.Instance.PlayerTeam)
                {
                    PlayerHasSpawner = true;
                }
                else if(team.Team == TeamManager.Instance.AITeam)
                {
                    EnemyHasSpawner = true;
                }
            }
            if (!captureArea.WinCondition) return;
            if (team.Team != TeamManager.Instance.PlayerTeam) return;
            CapturedWinAreas++;
        }).Run();
        Entities.WithoutBurst().ForEach((in UnitStateData unitState, in TeamData team) =>
        {
            if(!PlayerHasUnits && team.Team == TeamManager.Instance.PlayerTeam)
            {
                PlayerHasUnits = true;
            }
            if(!EnemyHasUnits && team.Team == TeamManager.Instance.AITeam)
            {
                EnemyHasUnits = true;
            }
        }).Run();
        if (!EnemyHasUnits && !EnemyHasSpawner)
        {
            Debug.Log("win1");
            Ended = true;
            Win = true;
            return;
        }
        else if (!PlayerHasUnits && !PlayerHasSpawner)
        {
            Ended = true;
            Win = false;
            return;
        }

        if (EndIfCompleted && CapturedWinAreas == RequiredWinAreas)
        {
            Debug.Log("win2");
            Ended = true;
            Win = true;
            return;
        }
        if(TimeLeftSeconds <= 0)
        {
            Ended = true;
            if(CapturedWinAreas == RequiredWinAreas)
            {
                Debug.Log("win3");
                Win = true;
            }
        }
    }
}
