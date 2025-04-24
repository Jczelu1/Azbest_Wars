using UnityEngine;

public class Rotato : MonoBehaviour
{
    public RectTransform uiElement;
    public float rotationInterval = 0.5f;
    public float rotationAmount = 90f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= rotationInterval)
        {
            timer = 0f;
            RotateUIElement();
        }
    }

    void RotateUIElement()
    {
        if (uiElement != null)
        {
            uiElement.Rotate(0f, 0f, rotationAmount);
        }
    }
}
