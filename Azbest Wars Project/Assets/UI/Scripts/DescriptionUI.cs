using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionController : MonoBehaviour
{
    [SerializeField]
    GameObject DescriptionUI;
    [SerializeField]
    TextMeshProUGUI descriptionText;
    [SerializeField]
    Image image;
    public static bool show = false;
    public static bool close = false;
    public static bool updateDesc;
    public static int showId = -1;
    private int showingId = -1;
    public static bool showBuilding = false;
    void Start()
    {
        
    }
    void Update()
    {
        if (show)
        {
            show = false;
            DescriptionUI.SetActive(true);
        }
        else if (close)
        {
            Close();
        }
        close = false;
        if (updateDesc)
        {
            updateDesc = false;
            showingId = showId;
            showId = -1;
            DescriptionData desc;
            if (showBuilding)
            {
                desc = BuildingTypeSystem.buildingTypesDescription[showingId];
            }
            else
            {
                desc = SpawnerSystem.unitTypesDescription[showingId];
            }
            descriptionText.text = desc.Name + '\n' + desc.Description;
            image.sprite = desc.BaseSprite;
            image.preserveAspect = true;
        }
    }
    public static void showDescription(int ShowId, bool ShowBuilding)
    {
        show = true;
        updateDesc = true;
        showBuilding = ShowBuilding;
        showId = ShowId;
    }
    public static void closeDescription()
    {
        close = true;
    }
    public void Close()
    {
        DescriptionUI.SetActive(false);
    }
}
