using TMPro;
using UnityEngine;

public class WinConditionUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI DoThisToWinText;
    [SerializeField]
    string doWhatToWin;

    [SerializeField]
    TextMeshProUGUI ProgressDescText;
    [SerializeField]
    string progressDescription;

    [SerializeField]
    TextMeshProUGUI ProgressText;

    [SerializeField]
    TextMeshProUGUI TimeText;
    void Start()
    {
        
    }

    void Update()
    {
        DoThisToWinText.text = doWhatToWin;
        ProgressDescText.text = progressDescription;
        ProgressText.text = $"{WinConditionSystem.CapturedWinAreas}/{WinConditionSystem.RequiredWinAreas}";

        float timeLeftSeconds = WinConditionSystem.TimeLeftSeconds;
        if(timeLeftSeconds >= 0)
        {
            int minutes = Mathf.FloorToInt(timeLeftSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeLeftSeconds % 60f);
            TimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
