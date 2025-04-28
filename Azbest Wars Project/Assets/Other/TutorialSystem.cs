using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//[UpdateInGroup(typeof(TickSystemGroup))]
//[UpdateAfter(typeof(CaptureAreaSystem))]
public partial class TutorialSystem : SystemBase
{
    public static bool IsTutorial = false;
    public static int TutorialState = 0;
    public static int TutorialProgress = 0;
    public static int TotalTutorialProgress = 5;
    public static int delay = 2;

    private string tutorial1 = "Witaj na polu bitwy!";
    private string tutorial2 = "No nie do koñca, to tylko æwiczenia.";
    private string tutorial3 = "Ale to praktycznie to samo.";

    protected override void OnCreate()
    {
        RequireForUpdate<CaptureAreaData>();
    }
    protected override void OnUpdate()
    {
        if (!IsTutorial) return;
        if (SetupSystem.startDelay != -1) return;
        if (delay >= 0)
        {
            delay--;
            return;
        }
        switch (TutorialState)
        {
            case 0:
                InfoBoardUI.Instance.ShowInfo(tutorial1);
                TutorialState++;
                delay = 4;
                break;
            case 1:
                InfoBoardUI.Instance.ShowInfo(tutorial2);
                InfoBoardUI.Instance.ShowInfo(tutorial3);
                TutorialState++;
                delay = 2;
                break;
        }
    }
}
