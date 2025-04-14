using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingDescriptionUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI descriptionText;
    [SerializeField]
    Image image;
    int showingBuilding = -1;
    void Start()
    {
        
    }
    void Update()
    {
        if(SelectSystem.buildingTypeSelected > -1 && SelectSystem.buildingTypeSelected != showingBuilding)
        {
            showingBuilding = SelectSystem.buildingTypeSelected;
            DescriptionData desc = BuildingTypeSystem.buildingTypesDescription[showingBuilding];
            descriptionText.text = desc.Name + '\n' + desc.Description;
            image.sprite = desc.BaseSprite;
        }
    }
}
