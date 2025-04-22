using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TickSystemGroup))]
[UpdateAfter(typeof(CaptureAreaSystem))]
public partial class WinConditionSystem : SystemBase
{
    public static int TotalWinAreas;
    public static int CapturedWinAreas;
    public static float TimeLeftSeconds;
    public static bool EndIfCompleted;
    public static bool VariablesSet;
    protected override void OnCreate()
    {

    }
    protected override void OnUpdate()
    {
        if (!VariablesSet) return;

    }
}
