using TMPro;
using UnityEngine;
using System.Collections;

public class WinConditionUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI DoThisToWinText;
    [TextArea]
    [SerializeField]
    string doWhatToWin;

    [SerializeField]
    TextMeshProUGUI ProgressDescText;

    [TextArea]
    [SerializeField]
    string progressDescription;

    [SerializeField]
    TextMeshProUGUI ProgressText;

    [SerializeField]
    TextMeshProUGUI TimeText;

    [SerializeField]
    GameObject VictoryUI;

    [SerializeField]
    GameObject DefeatUI;


    private bool _endedHandled = false;
    private bool _won = false;
    void Start()
    {
        
    }

    void Update()
    {
        DoThisToWinText.text = doWhatToWin;
        ProgressDescText.text = progressDescription;

        if (WinConditionSystem.WinConditionType == 0)
        {
            ProgressText.text = $"{WinConditionSystem.WinPoints}/{WinConditionSystem.RequiredWinPoints}";
        }
        else if (WinConditionSystem.WinConditionType == 1)
        {
            ProgressText.text = $"{WinConditionSystem.WinPoints}-{WinConditionSystem.EnemyWinPoints}\n/{WinConditionSystem.RequiredWinPoints}";
        }

        float timeLeftSeconds = WinConditionSystem.TimeLeftSeconds;
        if (timeLeftSeconds >= 0)
        {
            int minutes = Mathf.FloorToInt(timeLeftSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeLeftSeconds % 60f);
            TimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (WinConditionSystem.Ended && !_endedHandled)
        {
            _endedHandled = true;
            _won = WinConditionSystem.Win;
            if (_won)
            {
                InfoBoardUI.Instance.ShowInfo("Wygraliœmy!");
            }
            else
            {
                InfoBoardUI.Instance.ShowInfo("Przegraliœmy.");
            }
            TickSystemGroup.SetTickrate(0);
            MusicPlayer.Instance.StopMusic();
            StartCoroutine(ShowEndScreenWithDelay());
        }
        if(WinConditionSystem.Ended && _endedHandled)
        {
            TickSystemGroup.SetTickrate(0);
        }
    }

    

    private IEnumerator ShowEndScreenWithDelay()
    {
        yield return new WaitForSeconds(3f);
        TickSystemGroup.SetTickrate(0);
        if (_won)
        {
            VictoryUI.SetActive(true);
        }
        else
        {
            DefeatUI.SetActive(true);
        }
    }

}
