using UnityEngine;

public class TeamColors : MonoBehaviour
{
    public static TeamColors Instance;

    [SerializeField]
    public Color[] teamColors;
    public static Color[] colors =
    {
        new Color(1,1,0),
        new Color(0,0,1),
        new Color(1,0,1),
        new Color(0,1,1),
    };
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        colors = teamColors;
    }
}
