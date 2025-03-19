using UnityEngine;

public class TeamColors : MonoBehaviour
{
    public static TeamColors Instance;

    [SerializeField]
    public Color[] teamColors;
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
}
