using UnityEngine;
using Unity.Collections;
using TMPro;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance;
    public byte PlayerTeam;
    public byte AITeam;

    [SerializeField]
    public Color[] teamColors =
    {
        new Color(1,1,0),
        new Color(0,0,1),
        new Color(1,0,1),
        new Color(0,1,1),
    };
    [SerializeField]
    public Color[] teamColorsLow =
    {
        new Color(148f/255,148f/255,0),
        new Color(0,0,148f/255),
        new Color(148f/255,0,148f/255),
        new Color(0,148f/255,148f/255),
    };
    [SerializeField]
    private int[] startingResources = { 0, 0, 0, 0 };
    public NativeArray<int> teamResources = new NativeArray<int>(4, Allocator.Persistent);
    public Color baseColor = Color.white;
    public Color baseColorLow = new Color(184f / 255, 184f / 255, 184f / 255);
    public Color selectedColor = Color.green;
    public Color attackedColor = Color.red;
    public Color GetTeamColor(byte team)
    {
        if (team < 0 || team >= teamColors.Length) return baseColor;
        return teamColors[team];
    }
    public Color GetTeamColorLow(byte team)
    {
        if (team < 0 || team >= teamColorsLow.Length) return baseColorLow;
        return teamColorsLow[team];
    }

    public TextMeshProUGUI resourceText;

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
    private void Start()
    {
        for(int i = 0; i < 4; i++)
        {
            teamResources[i] = startingResources[i];
        }
    }
    private void Update()
    {
        resourceText.text = $"{teamResources[PlayerTeam]}";
    }
    private void OnDestroy()
    {
        teamResources.Dispose();
    }
}
