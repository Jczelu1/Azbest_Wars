using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnerInputController : MonoBehaviour
{
    public static int Queued;
    public static int UnitType;
    public static float ProductionProgress = 0;
    public static bool UIClosedByPlayer = false;
    private bool UIOpen = false;
    [SerializeField]
    GameObject SpawnerUI;
    [SerializeField]
    TextMeshProUGUI QueueText;
    [SerializeField]
    TextMeshProUGUI UnitTypeText;
    [SerializeField]
    TextMeshProUGUI CostText;
    [SerializeField]
    TextMeshProUGUI TimeText;
    [SerializeField]
    Image Progressbar;

    [SerializeField]
    Image UnitImage;
    [SerializeField]
    Sprite[] UnitSprites;
    void Start()
    {
        //UnitImage.color = TeamManager.Instance.GetTeamColor(TeamManager.Instance.PlayerTeam);
    }

    // Update is called once per frame
    void Update()
    {
        if (!SelectSystem.spawnerSelected)
        {
            SpawnerUI.SetActive(false);
            UIOpen = false;
            UIClosedByPlayer = false;
        }
        if (SelectSystem.spawnerSelected && !UIOpen && !UIClosedByPlayer)
        {
            SpawnerUI.SetActive(true);
            UIOpen = true;
        }
        if (UIOpen)
        {
            if (Queued > 999)
            {
                QueueText.text = "INF";
            }
            else
            {
                QueueText.text = Queued.ToString();
            }
            if (UnitType < SpawnerSystem.unitTypes.Length)
            {
                UnitTypeText.text = SpawnerSystem.unitTypesDescription[UnitType].Name;
                CostText.text = SpawnerSystem.unitTypes[UnitType].Cost.ToString();
                TimeText.text = SpawnerSystem.unitTypes[UnitType].TimeToSpawn.ToString();
            }
            if (UnitType < UnitSprites.Length)
            {
                UnitImage.sprite = UnitSprites[UnitType];
            }
            if (Queued == 0)
            {
                Progressbar.fillAmount = 0;
            }
            else
            {
                Progressbar.fillAmount = 1 - ProductionProgress;
            }
        }
    }
    public void Close()
    {
        SpawnerUI.SetActive(false);
        UIOpen = false;
        UIClosedByPlayer = true;
    }
    public void UnitViewDescription()
    {
        DescriptionController.show = true;
        DescriptionController.showId = UnitType;
        DescriptionController.updateDesc = true;
        DescriptionController.showBuilding = false;
    }
    public void UnitTypeNext()
    {
        if (SpawnerSystem.unitTypes.Length > UnitType+1)
        {
            UnitType++;
            SetUnitType();
        }
    }
    public void UnitTypePrevious()
    {
        if (UnitType > 0)
        {
            UnitType--;
            SetUnitType();
        }
    }
    public void IncrementQueue()
    {
        if(Queued < int.MaxValue)
        {
            Queued++;
            SetQueued();
        }
    }
    public void DecrementQueue()
    {
        if (Queued > 0)
        {
            Queued--;
            SetQueued();
        }
    }
    public void AddFiveQueue()
    {
        if (Queued < int.MaxValue-6)
        {
            Queued+=5;
            SetQueued();
        }
    }
    public void RemoveFiveQueue()
    {
        Queued = Math.Max(0, Queued - 5);
        SetQueued();
    }
    public void InfiniteQueued()
    {
        Queued = int.MaxValue;
        SetQueued();
    }
    public void ZeroQueue()
    {
        Queued = 0;
        SetQueued();
    }

    private void SetQueued()
    {
        if (SelectSystem.spawnerSelected)
        {
            SpawnerSetter.setSelectedQueue = Queued;
        }
    }
    private void SetUnitType()
    {
        if (SelectSystem.spawnerSelected)
        {
            SpawnerSetter.setSelectedUnitType = UnitType;
        }
    }
}
