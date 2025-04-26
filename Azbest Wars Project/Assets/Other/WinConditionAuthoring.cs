using JetBrains.Annotations;
using UnityEngine;

public class WinConditionAuthoring : MonoBehaviour
{
    public float TimeSeconds;
    public bool EndIfCompleted;
    public bool EndIfNoWinPoints = false;
    public int RequiredWinPoints;
    public int WinConditionType;
    void Start()
    {
        WinConditionSystem.TimeLeftSeconds = TimeSeconds;
        WinConditionSystem.TimeTotalSeconds = TimeSeconds;
        WinConditionSystem.EndIfCompleted = EndIfCompleted;
        WinConditionSystem.RequiredWinPoints = RequiredWinPoints;
        WinConditionSystem.WinConditionType = WinConditionType;
        WinConditionSystem.EndIfNoWinPoints = EndIfNoWinPoints;
    }
}
