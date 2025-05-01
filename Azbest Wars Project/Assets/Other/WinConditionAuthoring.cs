using UnityEngine;

public class WinConditionAuthoring : MonoBehaviour
{
    public float TimeSeconds;
    public bool EndIfCompleted;
    public bool EndIfNoWinPoints = false;
    public int RequiredWinPoints;
    public int WinConditionType;
    public bool isTutorial;
    public bool StartYapperEnabled = true;
    [SerializeField] GameObject startYapper;
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
        TutorialSystem.startTutorial = false;
        if(startYapper != null)
        {
            startYapper.SetActive(StartYapperEnabled);
            if (!StartYapperEnabled)
            {
                MusicPlayer.Instance.PlayRandomTheme();
                TutorialSystem.startTutorial = true;
                SetupSystem.pauseOnSetup = false;
            }
        }
        else
        {
            MusicPlayer.Instance.PlayRandomTheme();
            TutorialSystem.startTutorial = true;
            SetupSystem.pauseOnSetup = false;
        }
    }
}
