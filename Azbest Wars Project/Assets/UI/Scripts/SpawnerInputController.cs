using UnityEngine;

public class SpawnerInputController : MonoBehaviour
{
    public static int Queued;
    public static int UnitType;
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
