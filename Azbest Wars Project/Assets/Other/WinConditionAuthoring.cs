using JetBrains.Annotations;
using UnityEngine;

public class WinConditionAuthoring : MonoBehaviour
{
    public float TimeSeconds;
    public bool EndIfCompleted;
    public bool EndIfNoWinPoints = false;
    public int RequiredWinPoints;
    public int WinConditionType;
    public bool isTutorial;
    void Start()
    {
        WinConditionSystem.TimeLeftSeconds = TimeSeconds;
        WinConditionSystem.TimeTotalSeconds = TimeSeconds;
        WinConditionSystem.EndIfCompleted = EndIfCompleted;
        WinConditionSystem.RequiredWinPoints = RequiredWinPoints;
        WinConditionSystem.WinConditionType = WinConditionType;
        WinConditionSystem.EndIfNoWinPoints = EndIfNoWinPoints;
        TutorialSystem.IsTutorial = isTutorial;
        TutorialSystem.TutorialProgress = 0;
        TutorialSystem.TutorialState = 0;
        TutorialSystem.delay = 2;
    }
}
