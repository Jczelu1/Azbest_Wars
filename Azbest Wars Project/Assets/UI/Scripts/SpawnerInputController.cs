using System;
using TMPro;
using UnityEngine;

public class SpawnerInputController : MonoBehaviour
{
    public static int Queued;
    public static int UnitType;
    public static float ProductionProgress;
    [SerializeField]
    TextMeshProUGUI QueueText;
    [SerializeField]
    TextMeshProUGUI UnitTypeText;
    [SerializeField]
    TextMeshProUGUI CostText;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Queued > 999)
        {
            QueueText.text = "INF";
        }
        else
        {
            QueueText.text = Queued.ToString();
        }
        if(UnitType < SpawnerSystem.unitTypes.Length)
        {
            UnitTypeText.text = SpawnerSystem.unitTypes[UnitType].UnitName.ToString();
            CostText.text = SpawnerSystem.unitTypes[UnitType].Cost.ToString();
        }
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
            SpawnerSystem.setSelectedQueue = Queued;
        }
    }
    private void SetUnitType()
    {
        if (SelectSystem.spawnerSelected)
        {
            SpawnerSystem.setSelectedUnitType = UnitType;
        }
    }
}
