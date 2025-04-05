using Unity.Rendering;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance;
    public byte playerTeam;
    public byte AITeam;

    [SerializeField]
    public Color[] teamColors =
    {
        new Color(1,1,0),
        new Color(0,0,1),
        new Color(1,0,1),
        new Color(0,1,1),
    };
    public int[] teamResources = { 0, 0, 0, 0 };
    public Color baseColor = Color.white;
    public Color selectedColor = Color.green;
    public Color attackedColor = Color.red;
    public Color GetTeamColor(byte team)
    {
        if (team < 0 || team >= teamColors.Length) return baseColor;
        return teamColors[team];
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
    }
}
