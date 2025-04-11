using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButtonScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    Image image;
    [SerializeField]
    TextMeshProUGUI text;

    [SerializeField]
    Sprite NormalSprite;

    [SerializeField]
    Sprite SelectedSprite;

    [SerializeField]
    Color normalTextColor = Color.white;

    [SerializeField]
    Color selectedTextColor = Color.black;

    bool isSelected = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        image.sprite = SelectedSprite;
        text.color = selectedTextColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isSelected)
        {
            image.sprite = NormalSprite;
            text.color = normalTextColor;
        }
    }
    public void Select()
    {
        isSelected = true;
        image.sprite = SelectedSprite;
        text.color = selectedTextColor;
    }
    public void UnSelect()
    {
        isSelected = false;
        image.sprite = NormalSprite;
        text.color = normalTextColor;
    }
}
