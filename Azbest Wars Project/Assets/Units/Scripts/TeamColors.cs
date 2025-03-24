using Unity.Rendering;
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
    public static Color baseColor = Color.white;
    public static Color selectedColor = Color.green;
    public static Color attackedColor = Color.red;
    public static Color GetTeamColor(byte team)
    {
        if(team < 0 || team >= colors.Length) return baseColor;
        return colors[team];
    }
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
