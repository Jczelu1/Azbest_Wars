using UnityEngine;
using System;

public class UnitTypeManager : MonoBehaviour
{
    public static UnitTypeManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    [SerializeField]
    public string[] names;
    [SerializeField]
    public GameObject[] prefabs;
    [SerializeField]
    public int[] costs;
    void Start()
    {
        if(prefabs.Length != costs.Length || prefabs.Length != names.Length)
        {
            throw new Exception("Invalid values in UnitTypeManager");
        }
    }

    void Update()
    {
        
    }
}
