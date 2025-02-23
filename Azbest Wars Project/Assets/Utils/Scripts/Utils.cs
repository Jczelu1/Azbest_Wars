using TMPro;
using UnityEngine;

public static class Utils
{
    public static TextMeshPro CreateWorldText(string text = "", Transform parent = null, Vector3? localPosition = null, int fontSize = 2, Color? color = null, TextAlignmentOptions textAlignment = TextAlignmentOptions.Center)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition ?? new Vector3(0, 0);
        TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color ?? Color.white;
        return textMesh;
    }
    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y));
        return worldPosition;
    }
}
