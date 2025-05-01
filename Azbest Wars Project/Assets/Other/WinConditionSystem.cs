using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(CaptureAreaSystem))]
public partial class WinConditionSystem : SystemBase
{
    public static int TotalWinAreas = -1;
    public static float TimeLeftSeconds = -1;
    public static float TimeTotalSeconds = -1;
    public static bool EndIfCompleted = false;
    public static bool EndIfNoWinPoints = false;
    public static bool Ended = false;
    public static bool Win = false;
    public static int WinConditionType = 0;
    public static int WinPoints = 0;
    public static int EnemyWinPoints = 0;
    public static int RequiredWinPoints = 1;
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
        if (TutorialSystem.IsTutorial)
        {
            if(TimeLeftSeconds < 0)
            {
                Ended = true;
                Win = false;
            }
            return;
        }
        if (TotalWinAreas == -1)
        {
            TotalWinAreas = 0;
            Entities.WithoutBurst().ForEach((ref CaptureAreaData captureArea, ref TeamData team) =>
            {
                if (!captureArea.WinCondition) return;
                TotalWinAreas++;

            }).Run();
        }
        if(WinConditionType == 0)
        {
            WinPoints = 0;
            EnemyWinPoints = 0;
        }
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
            if (team.Team == TeamManager.Instance.PlayerTeam)
            {
                WinPoints++;
            }
            else if(team.Team == TeamManager.Instance.AITeam)
            {
                EnemyWinPoints++;
            }
            
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

        if (EndIfCompleted && WinPoints >= RequiredWinPoints)
        {
            Debug.Log("win2");
            Ended = true;
            Win = true;
            return;
        }
        if(EndIfNoWinPoints && WinPoints == 0)
        {
            Ended = true;
            Win = false;
            return;
        }
        if(WinConditionType == 1 && EndIfCompleted && EnemyWinPoints >= RequiredWinPoints)
        {
            Ended = true;
            Win = false;
            return;
        }
        if(TimeLeftSeconds <= 0)
        {
            Ended = true;
            if(WinConditionType == 0)
            {
                if (WinPoints == RequiredWinPoints)
                {
                    Debug.Log("win3");
                    Win = true;
                }
            }
            else if(WinConditionType == 1)
            {
                if(WinPoints >= EnemyWinPoints)
                {
                    Debug.Log("win4");
                    Win = true;
                }
            }
            
        }
    }
}
