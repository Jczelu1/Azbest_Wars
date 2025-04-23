using JetBrains.Annotations;
using UnityEngine;

public class WinConditionAuthoring : MonoBehaviour
{
    public float TimeSeconds;
    public bool EndIfCompleted;
    public int RequiredWinAreas;
    void Start()
    {
        WinConditionSystem.TimeLeftSeconds = TimeSeconds;
        WinConditionSystem.TimeTotalSeconds = TimeSeconds;
        WinConditionSystem.EndIfCompleted = EndIfCompleted;
        WinConditionSystem.RequiredWinAreas = RequiredWinAreas;
    }
}
