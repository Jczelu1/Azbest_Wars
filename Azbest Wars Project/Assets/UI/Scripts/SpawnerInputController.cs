using System;
using TMPro;
using UnityEngine;

public class SpawnerInputController : MonoBehaviour
{
    public static int Queued;
    public static int UnitType;
    [SerializeField]
    TextMeshProUGUI QueueText;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Queued++;
            SetQueued();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if(Queued > 0)
            {
                Queued--;
                SetQueued();
            }
        }
        if(Queued > 999)
        {
            QueueText.text = "INF";
        }
        else
        {
            QueueText.text = Queued.ToString();
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
        if (Queued > int.MaxValue / 2)
            Queued = 0;
        else
            Queued = int.MaxValue;
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
