using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectorUI : MonoBehaviour
{
    public static bool enableOnStart = false;
    [SerializeField]
    GameObject LevelSelector;
    [SerializeField]
    LevelController levelController;
    [SerializeField]
    string[] MapNames;
    [SerializeField]
    string[] LevelDescriptions;
    [SerializeField]
    CustomButtonScript[] levelButtons;

    [SerializeField]
    TextMeshProUGUI descriptionText;

    [SerializeField] Button playButton;

    void Start()
    {
        if (enableOnStart)
        {
            LevelSelector.SetActive(true);
        }
    }
    public void SelectLevel(int level)
    {
        if (level < 0 || level >= MapNames.Length) return;
        descriptionText.gameObject.SetActive(true);
        playButton.gameObject.SetActive(true);
        foreach(var cbs in levelButtons)
        {
            cbs.UnSelect();
        }
        levelButtons[level].Select();
        descriptionText.text = LevelDescriptions[level];
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() => levelController.LoadScene(MapNames[level]));
        enableOnStart = true;
    }
}
