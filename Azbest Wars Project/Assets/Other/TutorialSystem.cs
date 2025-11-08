using Unity.Entities;

//[UpdateInGroup(typeof(TickSystemGroup))]
//[UpdateAfter(typeof(CaptureAreaSystem))]
public partial class TutorialSystem : SystemBase
{
    public static bool IsTutorial = false;
    public static int TutorialState = 0;
    public static int TutorialProgress = 0;
    public static int TotalTutorialProgress = 6;
    public static float delay = 2;
    public static bool startTutorial = false;

    private string tutorial1 = "-Welcome to the battlefield!";
    private string tutorial2 = "-Well, not really, it's just a training exercise.";
    private string tutorial3 = "-But it's practically the same thing.";
    private string tutorial4 = "-Look around.";
    private string tutorial5 = "-Okay, now select your units.";
    private string tutorial6 = "-See that zone north of our position? Move your units there.";
    private string tutorial7 = "-Thanks to zones, you gain asbestos, which you can use to produce units.";
    private string tutorial8 = "-You have enough asbestos. Select a camp and produce a unit.";
    private string tutorial9 = "-Units can defend or attack. If they attack, they will move towards the enemy. You can also stop your units.";
    private string tutorial10 ="-There are enemy units near the zone. Select your units, issue an attack order, and defeat them.";
    private string tutorial11 ="-Great, you've defeated the enemy units.";
    private string tutorial12 = "-Enough training. Good luck.";

    protected override void OnCreate()
    {
        RequireForUpdate<CaptureAreaData>();
    }
    protected override void OnUpdate()
    {
        if (!IsTutorial) return;
        if (!SetupSystem.started) return;
        if (!startTutorial) return;
        if (delay >= 0)
        {
            delay -= SystemAPI.Time.DeltaTime;
            return;
        }
        int playerTeam = TeamManager.Instance.PlayerTeam;
        int enemyTeam = TeamManager.Instance.AITeam;
        switch (TutorialState)
        {
            case 0:
                InfoBoardUI.Instance.ShowInfo(tutorial1);
                TutorialState++;
                delay = 3;
                break;
            case 1:
                InfoBoardUI.Instance.ShowInfo(tutorial2);
                TutorialState++;
                delay = 3;
                break;
            case 2:
                InfoBoardUI.Instance.ShowInfo(tutorial3);
                TutorialState++;
                delay = 3;
                break;
            case 3:
                InfoBoardUI.Instance.ShowInfo(tutorial4);
                TutorialUI.Instance.tutorialControls[0].SetActive(true);
                TutorialState++;
                delay = 0;
                break;
            case 4:
                if (TutorialUI.Instance.moved)
                {
                    TutorialState++;
                    delay = 2;
                }
                break;
            case 5:
                TutorialProgress++;
                TutorialState++;
                TutorialUI.Instance.tutorialControls[0].SetActive(false);
                TutorialUI.Instance.tutorialControls[1].SetActive(true);
                break;
            case 6:
                if (TutorialUI.Instance.maped)
                {
                    TutorialState++;
                    delay = 2;
                }
                break;
            case 7:
                TutorialProgress++;
                TutorialState++;
                InfoBoardUI.Instance.ShowInfo(tutorial5);
                TutorialUI.Instance.tutorialControls[1].SetActive(false);
                TutorialUI.Instance.tutorialControls[2].SetActive(true);
                break;
            case 8:
                if (SelectSystem.unitsSelected > 0)
                {
                    TutorialState++;
                    delay = 2;
                }
                break;
            case 9:
                TutorialProgress++;
                TutorialState++;
                InfoBoardUI.Instance.ShowInfo(tutorial6);
                TutorialUI.Instance.tutorialControls[2].SetActive(false);
                TutorialUI.Instance.tutorialControls[3].SetActive(true);
                break;
            case 10:
                bool isAreaCaptured = false;
                Entities.ForEach((in CaptureAreaData captureArea, in TeamData team) =>
                {
                    if (team.Team == playerTeam)
                    {
                        isAreaCaptured = true;
                    }
                }).Run();
                if (isAreaCaptured)
                {
                    TutorialState++;
                    delay = 2;
                }
                break;
            case 11:
                TutorialProgress++;
                TutorialState++;
                InfoBoardUI.Instance.ShowInfo(tutorial7);
                TutorialUI.Instance.tutorialControls[3].SetActive(false);
                TutorialUI.Instance.tutorialControls[4].SetActive(true);
                delay = 2;
                break;
            case 12:
                if (TeamManager.Instance.teamResources[playerTeam] >= 60)
                {
                    InfoBoardUI.Instance.ShowInfo(tutorial8);
                    TutorialState++;
                    TutorialUI.Instance.tutorialControls[4].SetActive(false);
                    TutorialUI.Instance.tutorialControls[5].SetActive(true);
                }
                break;
            case 13:
                if (SelectSystem.spawnerSelected)
                {
                    TutorialState++;
                    TutorialUI.Instance.tutorialControls[5].SetActive(false);
                    TutorialUI.Instance.tutorialControls[6].SetActive(true);
                    delay = 2;
                }
                break;
            case 14:
                if (SelectSystem.spawnerSelected)
                {
                    TutorialUI.Instance.tutorialControls[6].SetActive(true);
                }
                else
                {
                    TutorialUI.Instance.tutorialControls[6].SetActive(false);
                }
                int playerUnits = 0;
                Entities.ForEach((in UnitStateData unitState, in TeamData team) =>
                {
                    if (team.Team == playerTeam)
                    {
                        playerUnits++;
                    }
                }).Run();
                if(playerUnits > 2)
                {
                    TutorialProgress++;
                    TutorialState++;
                    delay = 2;
                }
                break;
            case 15:
                SelectSystem.resetSelect = true;
                InfoBoardUI.Instance.ShowInfo(tutorial9);
                TutorialUI.Instance.tutorialControls[6].SetActive(false);
                TutorialState++;
                delay = 10;
                break;
            case 16:
                InfoBoardUI.Instance.ShowInfo(tutorial10);
                TutorialState++;
                break;
            case 17:
                if (SelectSystem.unitsSelected > 0)
                {
                    TutorialUI.Instance.tutorialControls[7].SetActive(true);
                    TutorialUI.Instance.tutorialControls[2].SetActive(false);
                }
                else
                {
                    TutorialUI.Instance.tutorialControls[7].SetActive(false);
                    TutorialUI.Instance.tutorialControls[2].SetActive(true);
                }
                int enemyUnits = 0;
                Entities.ForEach((in UnitStateData unitState, in TeamData team) =>
                {
                    if (team.Team == enemyTeam)
                    {
                        enemyUnits++;
                    }
                }).Run();
                if (enemyUnits == 0)
                {
                    TutorialProgress++;
                    TutorialState++;
                    delay = 2;
                    TutorialUI.Instance.tutorialControls[7].SetActive(false);
                    TutorialUI.Instance.tutorialControls[7].SetActive(false);
                }
                break;
            case 18:
                InfoBoardUI.Instance.ShowInfo(tutorial11);
                TutorialState++;
                delay = 3;
                break;
            case 19:
                InfoBoardUI.Instance.ShowInfo(tutorial12);
                TutorialState++;
                delay = 5;
                break;
            case 20:
                WinConditionSystem.Ended = true;
                WinConditionSystem.Win = true;
                break;

        }
        TutorialUI.Instance.moved = false;
        TutorialUI.Instance.maped = false;
    }
}
